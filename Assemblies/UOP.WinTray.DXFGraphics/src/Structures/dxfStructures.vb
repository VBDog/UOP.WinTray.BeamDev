Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices
Imports System.Security.Cryptography
Imports System.Windows.Controls
Imports System.Windows.Media



Imports UOP.DXFGraphics.dxfGlobals
Imports UOP.DXFGraphics.Fonts.dxfFonts

Imports UOP.DXFGraphics.Utilities
Imports Vanara.PInvoke



Namespace UOP.DXFGraphics.Structures

    Friend Structure TDISPLAY
        Implements IDisposable
        Implements ICloneable
#Region "Members"
        Public hsb_BACKCOLOR As COLOR_HSB
        Public pln_DEVICE As TPLANE
        Public pln_VIEW As TPLANE
        Public pln_VIEWLAST As TPLANE
        Public pln_LIMITS As TPLANE
        Public rec_EXTENTS As TPLANE

        Public AutoRedraw As Boolean
        Public ColorMode As dxxColorModes
        Public IsDirty As Boolean
        Public PixelsPerUnit As Double
        Public Units As dxxDeviceUnits
        Public ZoomFactor As Double
        Public ZoomLast As Double
        Private _PaperScale As Double
        Private _BackGroundColor As System.Drawing.Color
        Private _ImageGUID As String
        Private _DPI As Integer
        Private _DeviceHwnd As IntPtr
        Private disposedValue As Boolean
#End Region 'Members
#Region "Constructors"
        Sub New(aImagGUID As String)
            'init -------------------------------------------
            hsb_BACKCOLOR = New COLOR_HSB("")
            pln_DEVICE = New TPLANE("DEVICE")
            pln_VIEW = New TPLANE("VIEW")
            pln_VIEWLAST = New TPLANE("VIEWLAST")
            pln_LIMITS = New TPLANE("LIMITS")
            rec_EXTENTS = New TPLANE("EXTENTS")
            AutoRedraw = False
            ColorMode = dxxColorModes.Full
            IsDirty = False
            PixelsPerUnit = 0
            Units = dxxDeviceUnits.Pixels
            ZoomFactor = 1
            ZoomLast = 1
            _PaperScale = 1
            _BackGroundColor = System.Drawing.Color.White
            _ImageGUID = ""
            _DPI = 96
            _DeviceHwnd = IntPtr.Zero
            disposedValue = False

            'init -------------------------------------------
            ImageGUID = aImagGUID
        End Sub
        Public Sub New(aDisplay As TDISPLAY)
            'init -------------------------------------------
            hsb_BACKCOLOR = New COLOR_HSB(aDisplay.hsb_BACKCOLOR)
            pln_DEVICE = New TPLANE(aDisplay.pln_DEVICE)
            pln_VIEW = New TPLANE(aDisplay.pln_VIEW)
            pln_VIEWLAST = New TPLANE(aDisplay.pln_VIEWLAST)
            pln_LIMITS = New TPLANE(aDisplay.pln_LIMITS)
            rec_EXTENTS = New TPLANE(aDisplay.rec_EXTENTS)
            AutoRedraw = aDisplay.AutoRedraw
            ColorMode = aDisplay.ColorMode
            IsDirty = aDisplay.IsDirty
            PixelsPerUnit = aDisplay.PixelsPerUnit
            Units = aDisplay.Units
            ZoomFactor = aDisplay.ZoomFactor
            ZoomLast = aDisplay.ZoomLast
            _PaperScale = aDisplay.PaperScale
            _BackGroundColor = aDisplay._BackGroundColor
            _ImageGUID = aDisplay._ImageGUID
            _DPI = aDisplay._DPI
            _DeviceHwnd = aDisplay._DeviceHwnd
            disposedValue = False
            _BackGroundColor = aDisplay._BackGroundColor
            'init ------------------------------------------



        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Property DeviceHwnd As IntPtr
            Get
                Return _DeviceHwnd
            End Get
            Set(value As IntPtr)
                _DeviceHwnd = value
            End Set
        End Property
        '^returns the aspect ratio of the current bitmap
        '~aspect is Width / Height
        Public ReadOnly Property AspectRatio As Double
            Get
                Return pln_DEVICE.AspectRatio
            End Get
        End Property
        Public Property BackGroundColor As System.Drawing.Color
            Get
                Return _BackGroundColor
            End Get
            Set(value As System.Drawing.Color)
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
        Public ReadOnly Property ViewTransform As TTRANSFORMATION
            Get
                Dim _rVal As New TTRANSFORMATION(pln_VIEW.Clone) With {
                    .ScaleVector = New TVECTOR(ZoomFactor * DPI, -ZoomFactor * DPI, ZoomFactor * DPI)
                }
                Return _rVal
            End Get
        End Property
        Public ReadOnly Property OutputDeviceIsDefined As Boolean
            Get
                If _DeviceHwnd = 0 Then Return False
                Return DeviceIsDefined()
            End Get
        End Property
        Public Property DPI As Integer
            Get
                Return _DPI
            End Get
            Set(value As Integer)
                _DPI = value
            End Set
        End Property
        Public Property ImageGUID As String
            Get
                Return _ImageGUID
            End Get
            Set(value As String)
                _ImageGUID = value
            End Set
        End Property
        Friend Property Image As dxfImage
            Get
                Return dxfEvents.GetImage(ImageGUID)
            End Get
            Set(value As dxfImage)

                If value IsNot Nothing Then ImageGUID = value.GUID Else ImageGUID = ""
            End Set
        End Property
        Public Property PaperScale As Double
            Get
                Dim _rVal As Double
                If ZoomFactor <= 0 Then ZoomFactor = 1
                If _PaperScale <= 0 Then
                    _rVal = 1 / ZoomFactor
                Else
                    _rVal = _PaperScale
                End If
                Return _rVal
            End Get
            Friend Set(value As Double)
                _PaperScale = value
                If _PaperScale <= 0 Then _PaperScale = 0
            End Set
        End Property
        Public ReadOnly Property Properties As TPROPERTIES
            Get
                Dim _rVal As New TPROPERTIES("DISPLAY", bNonDXF:=True)

                _rVal.Add(New TPROPERTY(1, AutoRedraw, "AutoRedraw", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                _rVal.Add(New TPROPERTY(2, ColorMode, "ColorMode", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))
                _rVal.Add(New TPROPERTY(3, DeviceHwnd.ToString, "DeviceHwnd", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                _rVal.Add(New TPROPERTY(4, IsDirty, "IsDirty", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                _rVal.Add(New TPROPERTY(5, PaperScale, "PaperScale", dxxPropertyTypes.dxf_Single, bNonDXF:=True))
                _rVal.Add(New TPROPERTY(6, PixelsPerUnit, "PixelsPerUnit", dxxPropertyTypes.dxf_Single, bNonDXF:=True))
                _rVal.Add(New TPROPERTY(7, Units, "Units", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))

                _rVal.Add(New TPROPERTY(8, ZoomFactor, "ZoomFactor", dxxPropertyTypes.dxf_Single, bNonDXF:=True))
                _rVal.Add(New TPROPERTY(9, rec_EXTENTS.Origin.Coordinates(0), "ExtentCenter", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                _rVal.Add(New TPROPERTY(10, rec_EXTENTS.Width, "ExtentWidth", dxxPropertyTypes.dxf_Double, bNonDXF:=True))
                _rVal.Add(New TPROPERTY(11, rec_EXTENTS.Height, "ExtentHeight", dxxPropertyTypes.dxf_Double, bNonDXF:=True))
                Return _rVal
            End Get
        End Property
        Public Property Size As System.Drawing.Size
            Get
                Dim _rVal As New System.Drawing.Size
                Dim myImg As dxfImage = Image
                If myImg IsNot Nothing Then
                    _rVal = myImg.GetImage(False).Size
                    If Convert.ToInt32(pln_DEVICE.Width) <> _rVal.Width Then pln_DEVICE.Width = Convert.ToDouble(_rVal.Width)
                    If Convert.ToInt32(pln_DEVICE.Height) <> _rVal.Height Then pln_DEVICE.Height = Convert.ToDouble(_rVal.Height)
                End If
                Return _rVal
            End Get
            Set(value As System.Drawing.Size)
                Resize(Nothing, value.Width, value.Height)
            End Set
        End Property
        Public ReadOnly Property Width As Double
            Get
                Return pln_DEVICE.Width
            End Get
        End Property
        Public ReadOnly Property Height As Double
            Get
                Return pln_DEVICE.Height
            End Get
        End Property
        Public ReadOnly Property WidthI As Integer
            Get
                Return TVALUES.To_INT(pln_DEVICE.Width)
            End Get
        End Property
        Public ReadOnly Property HeightI As Integer
            Get
                Return TVALUES.To_INT(pln_DEVICE.Height)
            End Get
        End Property
        Public ReadOnly Property HasLimits As Boolean
            Get

                If TPLANE.IsNull(pln_LIMITS) Then Return False
                Return pln_LIMITS.Width > 0 Or pln_LIMITS.Height > 0
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function Resize(aImage As dxfImage, aWidth As Integer, aHeight As Integer) As Boolean
            Try
                If aImage Is Nothing Then aImage = Image
                If aImage Is Nothing Then Return False
                Dim BMP As dxfBitmap = aImage.GetBitmap(False)
                If BMP Is Nothing Then Return False
                If aWidth <= 0 Then aWidth = BMP.Width
                If aHeight <= 0 Then aHeight = BMP.Height
                Dim _rVal As Boolean = False
                If aWidth <> BMP.Width Or aHeight <> BMP.Height Then
                    BMP.Resize(aWidth, aHeight)
                    _rVal = True
                End If
                pln_DEVICE.Width = Convert.ToDouble(BMP.Width)
                pln_DEVICE.Height = Convert.ToDouble(BMP.Height)
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return False
            End Try
        End Function
        Public Function Clone() As TDISPLAY
            Return New TDISPLAY(Me)
        End Function
        Public Function ConvertVector(aVector As TVECTOR, aFromDomain As dxxDomains, aToDomain As dxxDomains) As TVECTOR
            If aFromDomain = aToDomain Then Return aVector.Clone()
            '#1the subject display
            '#2the vector to convert
            '#3the domain that the passed vector is currently in
            '#4the domain to convert the ector to
            '^converts the passed vector from its current domain to the requested domain
            If aFromDomain < 0 And aFromDomain > 3 Then aFromDomain = dxxDomains.World
            If aToDomain < 0 And aToDomain > 3 Then aToDomain = dxxDomains.World
            Dim wrld As TVECTOR
            Select Case aFromDomain
                Case dxxDomains.World
                    wrld = aVector
                Case dxxDomains.Viewport
                    wrld = ViewToWorld(aVector)
                Case dxxDomains.Device
                    wrld = DeviceToWorld(aVector)
                Case dxxDomains.UCS
                    wrld = dxfEvents.GetImageUCSV(_ImageGUID).WorldVector(aVector)
                Case Else
                    wrld = aVector.Clone
            End Select
            Select Case aToDomain
                Case dxxDomains.World
                    Return wrld
                Case dxxDomains.Viewport
                    Return WorldToView(wrld)
                Case dxxDomains.Device
                    Return WorldToDevice(wrld)
                Case dxxDomains.UCS
                    Return dxfEvents.GetImageUCSV(_ImageGUID).WorldVector(wrld)
                Case Else
                    Return wrld
            End Select
        End Function
        Public Function SetLimits(aLowerLeft As TVECTOR, aUpperRight As TVECTOR) As Boolean
            Dim aPln As TPLANE = pln_VIEW.Clone
            Dim org As TVECTOR = aLowerLeft.ProjectedTo(aPln).MidPt(aUpperRight.ProjectedTo(aPln))
            Dim v1 As TVECTOR = aLowerLeft.WithRespectTo(aPln)
            Dim v2 As TVECTOR = aUpperRight.WithRespectTo(aPln)
            Dim _rVal As Boolean = aPln.Define(org, aPln.XDirection, aPln.YDirection, aHeight:=Math.Abs(v1.Y) + Math.Abs(v2.Y), aWidth:=Math.Abs(v1.X) + Math.Abs(v2.X))
            aPln.Name = "Limits"
            pln_LIMITS = aPln
            Return _rVal
        End Function
        Public Function DeviceToUCS(aVector As TVECTOR) As TVECTOR

            '#1the vector to transform (in pixels)
            '^returns the passed vector transformed to a point with respect to the current ucs
            Return DeviceToWorld(aVector).WithRespectTo(dxfEvents.GetImageUCSV(ImageGUID))
        End Function
        Public Function DeviceToView(aVector As TVECTOR) As TVECTOR

            '#1the vector to transform (in pixels)
            '^returns the passed vector transformed to a point with respect to the current ucs
            Dim _rVal As New TVECTOR(aVector)
            If ZoomFactor <= 0 Then ZoomFactor = 1
            If PixelsPerUnit <= 0 Then PixelsPerUnit = dxfGlobals.PixelsPerInch()
            Dim fctr As Double = 1 / (ZoomFactor * PixelsPerUnit)
            _rVal.X = (aVector.X - 0.5 * Width) * fctr
            _rVal.Y = -(aVector.Y - 0.5 * Height) * fctr
            Return _rVal
        End Function
        Public Function ViewToWorld(aVector As TVECTOR) As TVECTOR
            Dim _rVal As TVECTOR = pln_VIEW.WorldVector(New TVECTOR(aVector.X, aVector.Y))
            _rVal.Code = aVector.Code
            _rVal.Rotation = aVector.Rotation
            Return _rVal
        End Function
        Public Function DeviceToWorld(aVector As TVECTOR) As TVECTOR
            '#1the vector to transform (in pixels)
            '^returns the passed vector transformed to a point with respect to the world coordinate system
            Return pln_VIEW.WorldVector(DeviceToView(aVector))
        End Function
        Public Function WorldToView(aVector As TVECTOR) As TVECTOR
            '#1the subject display
            '#2the world vector to transform
            '^transforms the passed world vector to one with respect to the current view plane
            Dim _rVal As TVECTOR = aVector.ProjectedTo(pln_VIEW)
            _rVal = _rVal.WithRespectTo(pln_VIEW, aPrecis:=8)
            Return _rVal ' aVector.ProjectedTo(pln_VIEW).WithRespectTo(pln_VIEW,aPrec:= 8)
        End Function
        Public Function WorldToDevice(aVector As TVECTOR) As TVECTOR
            '#1the vector to transform
            '#2returns the vector with respect to the current view plane in current units
            '^returns the passed vector converted from world units to those of the current device (in pixels)
            Dim v1 As TVECTOR = WorldToView(aVector)
            Dim _rVal As TVECTOR = ViewToDevice(v1)
            Return _rVal
        End Function
        Public Function WorldToDevice(aVector As TVECTOR, aViewVector As dxfVector) As TVECTOR
            '#1the vector to transform
            '#2returns the vector with respect to the current view plane in current units
            '^returns the passed vector converted from world units to those of the current device (in pixels)
            Dim v1 As TVECTOR = WorldToView(aVector)
            If aViewVector IsNot Nothing Then aViewVector.Strukture = v1
            Dim _rVal As TVECTOR = ViewToDevice(v1)
            Return _rVal
        End Function
        Public Function ViewToDevice(aViewVector As TVECTOR) As TVECTOR
            '#2the vector in view coordinates to transform
            '^returns the passed vector converted from view units to those of the current device (in pixels)
            Dim _rVal As TVECTOR = aViewVector.Clone
            _rVal.X = (aViewVector.X * DPI * ZoomFactor) + 0.5 * Width
            _rVal.Y = -(aViewVector.Y * DPI * ZoomFactor) + 0.5 * Height
            Return _rVal
        End Function
        Friend Sub DefineBy_VPORT(aVPort As TTABLEENTRY, Optional aImage As dxfImage = Nothing)
            Dim rUCS As dxfPlane = Nothing
            DefineBy_VPORT(aVPort, aImage, rUCS)
        End Sub
        Friend Sub DefineBy_VPORT(aVPort As TTABLEENTRY, aImage As dxfImage, ByRef rUCS As dxfPlane)
            rUCS = New dxfPlane
            If aVPort.Props.Count <= 0 Then Return
            Dim aPlane As New TPLANE("")
            Dim bPlane As New TPLANE("")
            Dim aUCS As TPLANE = aPlane.Clone
            Dim ang As Double = aVPort.Props.GCValueD(51)
            Dim ZF As Double = 1
            Dim v1 As TVECTOR = aVPort.Props.GCValueV(16, TVECTOR.WorldZ)
            aPlane.AlignTo(v1, dxxAxisDescriptors.Z)
            v1 = New TVECTOR(0, 0, aVPort.Props.GCValueD(146))
            aPlane.Origin = aVPort.Props.GCValueV(12, v1)
            If ang <> 0 Then aPlane.Revolve(ang)
            v1 = aVPort.Props.GCValueV(111, TVECTOR.WorldX)
            Dim v2 As TVECTOR = aVPort.Props.GCValueV(112, New TVECTOR(1, 1, 0))
            If aImage Is Nothing Then
                If _ImageGUID <> "" Then aImage = dxfEvents.GetImage(_ImageGUID)
            End If
            aPlane.Units = dxxDeviceUnits.Inches
            bPlane.Height = aVPort.Props.GCValueD(40)
            If bPlane.Height > 0 Then
                Dim aspct As Double = aVPort.Props.GCValueD(41)
                If aspct > 0 Then
                    bPlane.Width = bPlane.Height * aspct
                    ZF = FactorToView(bPlane)
                End If
            End If
            Update(aPlane, rChangeProps:=New TPROPERTIES(""), aZoomFactor:=ZF, aUnits:=dxxDeviceUnits.Inches, aImage:=aImage, bSuppressEvnts:=True, bNoRedraw:=True, bSuppressImageUpdate:=True)
            rec_EXTENTS = aPlane
            rUCS.Strukture = aUCS.ReDefined(aVPort.Props.GCValueV(110, New TVECTOR(0, 0, 0)), v1, v2)
            If aImage IsNot Nothing Then
                aImage.obj_DISPLAY = Me
                aImage.obj_UCS = rUCS.Strukture
            End If
        End Sub
        Public Sub UpdateImage(ByRef rImage As dxfImage, aProps As TPROPERTIES, Optional aRedraw As Boolean = False, Optional bSuppressViewChange As Boolean = False)
            If rImage Is Nothing Then
                rImage = dxfEvents.GetImage(ImageGUID)
            End If
            If rImage IsNot Nothing Then
                rImage.obj_DISPLAY = Me
                If aProps.Count > 0 Then
                    Dim iDisplay As dxoDisplay = rImage.Display
                    For i As Integer = 1 To aProps.Count
                        rImage.RespondToSettingChange(iDisplay, aProps.Item(i))
                    Next
                End If
                If aRedraw Then
                    If Not rImage.Rendering Then
                        If AutoRedraw Then
                            rImage.Render()
                        Else
                            rImage.IsDirty = True
                        End If
                    End If
                End If
                If Not pln_VIEWLAST.IsEqualTo(pln_VIEW, bCompareDirections:=True, bCompareDimensions:=True, bCompareOrigin:=True) Then
                    If Not bSuppressViewChange Then
                        rImage.RaiseViewChangeEvent()
                        pln_VIEWLAST = pln_VIEW.Clone
                    End If
                End If
            End If
        End Sub
        Public Function DrawBitmap(Optional aImage As dxfImage = Nothing, Optional aBitMap As dxfBitmap = Nothing) As Boolean
            '#1the parent image
            '^draw the current or passed bitmap to the to the destination
            Dim bBackImg As Boolean
            Dim aContrl As System.Windows.Forms.Control = Nothing
            If aImage Is Nothing Then aImage = Image
            If aImage Is Nothing Then Return False
            If aBitMap Is Nothing Then aBitMap = aImage.bmp_Display
            If aBitMap Is Nothing Then Return False
            If Not aBitMap.IsDefined Then Return False
            Dim sBitmap As dxfBitmap = aImage.bmp_Screen
            If DeviceIsDefined(aContrl, bBackImg) Then
                Try
                    If aBitMap.DrawToDevice(DeviceHwnd, dxxRectangularAlignments.TopLeft) Then
                        aImage.RaiseOverlayBMPEvent(True, aBitMap.Image)
                        Return True
                    Else
                        Return False
                    End If
                Catch ex As Exception
                    Return False
                End Try
            Else
                Return False
            End If
        End Function


        Public Function DeviceIsDefined() As Boolean
            Dim _rVal As Boolean
            Dim rControl As System.Windows.Forms.Control = Nothing

            Dim rBackgroundOnly As Boolean
            If DeviceHwnd = 0 Then Return False
            Dim aCntrl As System.Windows.Forms.Control = System.Windows.Forms.Control.FromHandle(DeviceHwnd)
            If aCntrl IsNot Nothing Then
                _rVal = dxfBitmap.ValidateControl(aCntrl, rControl, rBackgroundOnly)
                If Not _rVal Then DeviceHwnd = 0
            End If
            Return _rVal
        End Function
        Public Function DeviceIsDefined(ByRef rControl As System.Windows.Forms.Control, ByRef rBackgroundOnly As Boolean) As Boolean
            Dim _rVal As Boolean

            rBackgroundOnly = False
            If DeviceHwnd = 0 Then Return False
            Dim aCntrl As System.Windows.Forms.Control = System.Windows.Forms.Control.FromHandle(DeviceHwnd)
            If aCntrl IsNot Nothing Then
                _rVal = dxfBitmap.ValidateControl(aCntrl, rControl, rBackgroundOnly)
                If Not _rVal Then DeviceHwnd = 0
            End If
            Return _rVal
        End Function
        Public Function ExtentViewRectangle(aImage As dxfImage, Optional aBuffer As Double = 1) As TPLANE
            Dim rNoExtents As Boolean = False
            Return ExtentViewRectangle(aImage, aBuffer, rNoExtents)
        End Function
        Public Function ExtentViewRectangle(aImage As dxfImage, aBuffer As Double, ByRef rNoExtents As Boolean) As TPLANE
            Dim _rVal As TPLANE = rec_EXTENTS.Clone()
            If aImage Is Nothing Then aImage = Image
            If aImage Is Nothing Then Return _rVal
            '#1a buffer to apply
            '#2returns true if the rectangle is dimensionless
            '^returns the view rectangle required to view the current extent rectangle
            aBuffer = TVALUES.LimitedValue(aBuffer, 1, 3, 1)
            rNoExtents = False
            Dim fW As Double
            Dim fH As Double
            Dim aspct As Double
            fW = _rVal.Width * aBuffer
            fH = _rVal.Height * aBuffer
            aspct = _rVal.AspectRatio
            If fW = 0 Or fH = 0 Then
                If fW = 0 And fH = 0 Then
                    rNoExtents = True
                    _rVal.Width = aImage.bmp_Display.Width
                    _rVal.Height = aImage.bmp_Display.Height
                Else
                    If fW = 0 Then fW = fH * aspct Else fH = fW / aspct
                End If
            Else
                If aspct < 1 Then
                    If fW / aspct > fH Then
                        fH = fW / aspct
                    Else
                        fW = fH * aspct
                    End If
                Else
                    If fH * aspct > fW Then
                        fW = fH * aspct
                    Else
                        fH = fW / aspct
                    End If
                End If

            End If
            _rVal.Width = fW
            _rVal.Height = fH
            Return _rVal
        End Function
        Friend Function FactorToView(aPlane As TPLANE) As Double
            '#1the subject display
            '#2the subject plane
            '^returns the zoom factor required to fit the hieght and width of the passed plane
            '^in the current view rectangle
            Dim _rVal As Double = ZoomFactor
            Dim bW As Double = Width
            Dim bH As Double = Height
            Dim fW As Double
            Dim fH As Double
            Dim d1 As Double
            Dim d2 As Double
            Dim aspct As Double
            aPlane.GetDimensions(fW, fH)
            If bW <= 0 And bH <= 0 Then Return _rVal
            If fW <= 0 And fH <= 0 Then Return _rVal
            If PixelsPerUnit <= 0 Then Return _rVal
            bW /= PixelsPerUnit
            bH /= PixelsPerUnit
            aspct = bW / bH
            If fH <= 0 Then
                fH = fW / aspct
            End If
            If fW <= 0 Then
                fW = fH * aspct
            End If
            If fH < fW / aspct Then
                fH = fW / aspct
            End If
            If fW < fH * aspct Then
                fW = fH * aspct
            End If
            d1 = Math.Sqrt(bW ^ 2 + bH ^ 2)
            d2 = Math.Sqrt(fW ^ 2 + fH ^ 2)
            _rVal = d1 / d2
            Return _rVal
        End Function
        Public Function WorldPathsToDevicePaths(aPaths As TPATHS) As TPATHS
            If aPaths.Count <= 0 Then Return New TPATHS(aPaths)
            Dim _rVal As New TPATHS(aPaths, bNoMembers:=True)
            Dim aMem As TPATH
            Dim vWorldPts As TVECTORS
            Dim aPln As TPLANE = pln_DEVICE
            Dim bBnds As TPLANE
            Dim extVecs As New TVECTORS(0)
            Dim memPln As TPLANE
            _rVal.ExtentVectors = WorldsToDevice(aPaths.ExtentVectors)
            For pi As Integer = 1 To aPaths.Count
                aMem = aPaths.Item(pi).Clone
                memPln = aMem.Plane.Clone
                aMem.Plane.Origin = WorldToDevice(memPln.Origin)
                aMem.Plane.CopyDirections(aPln)
                aMem.Plane.Units = aPln.Units
                aMem.Plane.Name = "DEVICE"
                For li As Integer = 1 To aMem.LoopCount
                    vWorldPts = aMem.Looop(li)
                    If aMem.Relative Then vWorldPts = memPln.WorldVectors(vWorldPts)
                    vWorldPts = WorldsToDevice(vWorldPts)
                    bBnds = vWorldPts.BoundingRectangle(aPln)
                    extVecs.Append(bBnds.Corners)
                    If aMem.Relative Then
                        aMem.SetLoop(li, vWorldPts.WithRespectToPlane(aMem.Plane))
                    Else
                        aMem.SetLoop(li, vWorldPts)
                    End If
                Next li
                'aMem.Plane.Origin = WorldToDevice(aMem.Plane.Origin)
                'aMem.Relative = False
                _rVal.Add(aMem)
            Next pi
            If extVecs.Count > 0 Then
                aPln = extVecs.Bounds(aPln)
                _rVal.ExtentVectors = aPln.Corners
            End If
            Return _rVal
        End Function
        Public Function WorldsToDevice(aVectors As TVECTORS) As TVECTORS
            Dim _rVal As New TVECTORS(0)
            '#1the subject display
            '#2the vector to transform
            '^returns t _rVal
            For i As Integer = 1 To aVectors.Count
                _rVal.Add(WorldToDevice(aVectors.Item(i)))
            Next i
            Return _rVal
        End Function
        Public Function SetDevice(aDevice As Object, Optional aBackColor As Integer = -1, Optional bSuppressEvnts As Boolean = False, Optional aImage As dxfImage = Nothing,
                                    Optional aBackGround As dxfBitmap = Nothing, Optional bNoRedraw As Boolean = False) As Boolean
            Dim rDeviceTypeName As String = String.Empty
            Dim rErrString As String = String.Empty
            Return SetDevice(aDevice, aBackColor, bSuppressEvnts, aImage, aBackGround, rDeviceTypeName, bNoRedraw, rErrString)
        End Function
        Public Function SetDevice(aDevice As Object, aBackColor As Integer, bSuppressEvnts As Boolean, aImage As dxfImage,
                                    aBackGround As dxfBitmap, ByRef rDeviceTypeName As String, bNoRedraw As Boolean, ByRef rErrString As String) As Boolean
            'get the parent dxfImage
            rDeviceTypeName = ""
            rErrString = ""
            If aImage Is Nothing Then aImage = Image
            If aImage Is Nothing Then Return False

            Dim aBMP As dxfBitmap = aImage.GetBitmap(False)

            Dim aProps As TPROPERTIES = TPROPERTIES.Null
            Dim devBitmap As dxfBitmap
            Dim bClr As System.Drawing.Color = aBMP.BackgroundColor
            Dim d1 As Double
            Dim d2 As Double
            Dim bNewDevice As Boolean
            Dim oldWD As IntPtr
            Dim ZF As Double
            Dim _rVal As Boolean

            Dim f1 As Single
            Dim Ptr As New IntPtr()
            Dim backgrd As Boolean
            Dim imgProp As String = String.Empty
            If aBackColor <> -1 Then bClr = dxfColors.Win32ToWin64(aBackColor)
            Dim Sz As New SIZE(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height)
            rErrString = ""
            rDeviceTypeName = "SCREEN"
            oldWD = DeviceHwnd
            ZF = ZoomFactor
            If aDevice IsNot Nothing Then
                Try
                    'validate the device type
                    rDeviceTypeName = TypeName(aDevice).ToUpper
                    If rDeviceTypeName = "DXFBITMAP" Then
                        devBitmap = aDevice
                        If Not devBitmap.IsDefined Then
                            rDeviceTypeName = "SCREEN"
                        Else
                            Sz = New SIZE(aBMP.Width, aBMP.Height)
                        End If
                    ElseIf dxfBitmap.ValidateControl(aDevice, backgrd, imgProp) Then ' If rDeviceTypeName = "FORM" Or rDeviceTypeName = "PICTUREBOX" Or rDeviceTypeName = "PANEL" Then
                        'make sure it has an Image property
                        Sz = New SIZE(aDevice.Width, aDevice.Height)
                        'create a bitmap for the display if it isn't defined
                        Dim blank As New dxfBitmap(Sz.cx, Sz.cy)
                        blank.FloodFill(aBMP.BackgroundColor)
                        If Not backgrd Then
                            aDevice.Image = blank.Bitmap
                            ' nImage = aDevice.Image
                        Else
                            aDevice.BackgroundImage = blank.Bitmap
                            '' nImage = aDevice.BackgroundImage
                        End If
                        'save the handle and DC
                        Ptr = aDevice.Handle

                    Else
                        'ignore every control that does not have an image(bitmap) to use
                        rDeviceTypeName = "SCREEN"
                    End If
                Catch ex As Exception
                    rErrString = ex.Message
                    Return False
                End Try
            End If
            Try
                'create a new bitmap
                Dim nBmap As New Bitmap(Sz.cx, Sz.cy)
                Dim bBMP As New dxfBitmap(nBmap, bClr)
                If aBackGround IsNot Nothing Then aImage.Background = aBackGround.Clone
                'get the current bitmap dimensions in pixels
                d1 = bBMP.Plane(dxxDeviceUnits.Pixels, "DEVICE").Diagonal
                If rDeviceTypeName = "SCREEN" Then
                    'create a bit map based on the current display
                    'get the new bitmap dimension
                    Sz = New SIZE(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height)
                    Ptr = 0
                ElseIf rDeviceTypeName = "DXFBITMAP" Then
                    Ptr = 0
                    'get the new bitmap dimensions in pixels
                    Sz = New SIZE(aBMP.Width, aBMP.Height)
                End If
                Try
                    bNewDevice = Ptr <> DeviceHwnd
                    d2 = Math.Round(Math.Sqrt(Sz.cx ^ 2 + Sz.cy ^ 2), 4)
                    _rVal = bNewDevice Or aBMP.BackgroundColor <> bClr Or d1 <> d2
                    'resize the image.display.bitmap to the New device dimensions
                    If d2 <> d1 Then
                        aBMP.Resize(Sz.cx, Sz.cy)
                        If d2 > 0 And d1 > 0 Then f1 = d2 / d1 Else f1 = 1
                        If bNewDevice Then
                            ZF = 1
                        Else
                            ZF = f1 / ZF
                        End If
                    End If
                    'apply the background color
                    bBMP.BackgroundColor = bClr
                    DeviceHwnd = Ptr
                    Update(pln_VIEW, rChangeProps:=aProps, aZoomFactor:=ZF, aImage:=aImage, bSuppressEvnts:=True, bNoRedraw:=True, bSuppressImageUpdate:=True)
                    If Ptr <> oldWD Then
                        Dim ptype As dxxPropertyTypes = dxxPropertyTypes.dxf_Integer
                        aProps.Add(0, Ptr.ToString, "Device.HWD", Ptr.ToString, ptype, aLastVal:=oldWD)
                    End If
                    If aProps.Count > 0 Then
                        _rVal = True
                        If aImage IsNot Nothing Then
                            aImage.obj_DISPLAY = Me
                            If Not bSuppressEvnts Then
                                UpdateImage(aImage, aProps, False)
                            Else
                                UpdateImage(aImage, New TPROPERTIES(""), False)
                            End If
                        End If
                    End If
                    If aImage IsNot Nothing Then aImage.obj_DISPLAY = Me
                    If Not bNoRedraw And _rVal Then
                        aImage.Render()
                    End If
                Catch ex As Exception
                    rErrString = ex.Message
                    Return False
                End Try
                Return _rVal
            Catch ex As Exception
                rErrString = "Error : Invalid Output Device Detected. " & ex.Message
                Return False
            End Try
        End Function
        Public Function SetDisplayWindow(ByRef rImage As dxfImage, aDisplayWidth As Double?, Optional ZCenter As iVector = Nothing, Optional aDisplayHeight As Double? = Nothing, Optional bSetFeatureScales As Boolean = False, Optional bNoRedraw As Boolean = False) As Boolean
            Dim _rVal As Boolean
            '[]Me~Controling the Display~HelpDocs\DisplayControl.htm~file
            '#1the display width to set for the image
            '#2the point to center in the display
            '#3the minimum height to display
            '^used to set the scale width and zoom center of the image
            '~moves the zoom center to the passed center and zooms the image to show the requested width.
            '~the height is dependent on the image's aspect ratio. if  the display height is passed the
            '~displayed width will be increased to show at least the requested height
            'On Error Resume Next
            If rImage Is Nothing Then rImage = dxfEvents.GetImage(_ImageGUID)
            If rImage Is Nothing Then Return _rVal
            Dim vPlane As TPLANE = pln_VIEW
            Dim ZF As Double
            Dim aProps As TPROPERTIES = TPROPERTIES.Null
            If ZCenter IsNot Nothing Then vPlane.Origin = New TVECTOR(ZCenter)
            Dim aPlane As TPLANE = vPlane
            Dim ht As Double = aPlane.Height
            Dim wd As Double = aPlane.Width
            If aDisplayWidth IsNot Nothing Then
                If aDisplayWidth.HasValue Then wd = aDisplayWidth.Value
            End If
            If aDisplayHeight IsNot Nothing Then
                If aDisplayHeight.HasValue Then ht = aDisplayHeight.Value
            End If
            If wd <= 0 Then wd = aPlane.Width
            If ht <= 0 Then ht = aPlane.Height
            aPlane.SetDimensions(wd, ht)
            ZF = FactorToView(aPlane)
            Dim chnged As Boolean = Update(vPlane, rChangeProps:=aProps, aZoomFactor:=ZF, aImage:=rImage, bSuppressEvnts:=True, bSuppressImageUpdate:=True)
            If rImage Is Nothing Then Return _rVal
            rImage.obj_DISPLAY = Me
            If bSetFeatureScales Then
                PaperScale = 1 / ZoomFactor
                rImage.SetFeatureScales(PaperScale, True, True)

            End If

            If _rVal Then
                Dim aDsp As dxoDisplay = rImage.Display
                For i As Integer = 1 To aProps.Count
                    rImage.RespondToSettingChange(aDsp, aProps.Item(i))
                Next
                If Not bNoRedraw Then
                    If AutoRedraw Then rImage.Render() Else rImage.IsDirty = True
                End If
            End If
            Return _rVal
        End Function
        Friend Function Update(ByRef ioViewPlane As TPLANE, ByRef rChangeProps As TPROPERTIES, Optional aZoomFactor As Double? = Nothing, Optional aBackColor As Object = Nothing, Optional aUnits As dxxDeviceUnits? = Nothing, Optional aImage As dxfImage = Nothing, Optional bSuppressEvnts As Boolean = False, Optional bNoRedraw As Boolean = False, Optional bSuppressImageUpdate As Boolean = False) As Boolean
            Dim rChanged As Boolean
            If aImage Is Nothing Then aImage = Image

            Dim newprops As TDISPLAY = TDISPLAY.UpdateStructure(Me, ioViewPlane:=ioViewPlane, rChangeProps:=rChangeProps, aZoomFactor:=aZoomFactor.Value, aBackColor:=aBackColor, aUnits:=aUnits, rChanged:=rChanged, aImage:=aImage, bSuppressEvnts:=bSuppressEvnts, bNoRedraw:=bNoRedraw, bSuppressImageUpdate:=True)
            hsb_BACKCOLOR = newprops.hsb_BACKCOLOR
            AutoRedraw = newprops.AutoRedraw
            ColorMode = newprops.ColorMode
            IsDirty = newprops.IsDirty
            PixelsPerUnit = newprops.PixelsPerUnit
            pln_DEVICE = newprops.pln_DEVICE
            pln_VIEW = newprops.pln_VIEW
            pln_VIEWLAST = newprops.pln_VIEWLAST
            pln_LIMITS = newprops.pln_LIMITS
            rec_EXTENTS = newprops.rec_EXTENTS
            Units = newprops.Units
            ZoomFactor = newprops.ZoomFactor
            ZoomLast = newprops.ZoomLast
            _PaperScale = newprops.PaperScale
            _BackGroundColor = newprops.BackGroundColor
            _ImageGUID = newprops.ImageGUID
            _DPI = newprops.DPI
            _DeviceHwnd = newprops.DeviceHwnd
            disposedValue = newprops.disposedValue
            If Not bSuppressImageUpdate And aImage IsNot Nothing Then
                aImage.obj_DISPLAY = Me
            End If
            Return rChanged
        End Function
#End Region 'Methods
#Region "Shared Methods"
        Public Shared Function UpdateStructure(ByRef ioDisplay As TDISPLAY, ByRef ioViewPlane As TPLANE, ByRef rChangeProps As TPROPERTIES, Optional aZoomFactor As Double? = Nothing, Optional aBackColor As Object = Nothing, Optional aUnits As dxxDeviceUnits? = Nothing, Optional aImage As dxfImage = Nothing, Optional bSuppressEvnts As Boolean = False, Optional bNoRedraw As Boolean = False, Optional bSuppressImageUpdate As Boolean = False) As TDISPLAY
            Dim rChanged As Boolean = False
            Return UpdateStructure(ioDisplay, ioViewPlane, rChangeProps, aZoomFactor, aBackColor, aUnits, rChanged, aImage, bSuppressEvnts, bNoRedraw, bSuppressImageUpdate)
        End Function
        Public Shared Function UpdateStructure(ByRef ioDisplay As TDISPLAY, ByRef ioViewPlane As TPLANE, ByRef rChangeProps As TPROPERTIES, aZoomFactor As Double?, aBackColor As Object, aUnits As dxxDeviceUnits?, ByRef rChanged As Boolean, Optional aImage As dxfImage = Nothing, Optional bSuppressEvnts As Boolean = False, Optional bNoRedraw As Boolean = False, Optional bSuppressImageUpdate As Boolean = False) As TDISPLAY
            If aImage Is Nothing Then aImage = ioDisplay.Image
            rChanged = False
            If aImage Is Nothing Then Return ioDisplay
            Dim rDisplay As TDISPLAY = ioDisplay


            Dim netBMP As dxfBitmap = aImage.GetBitmap(False)

            Dim pixsperunit As Double
            Dim ZF As Double
            Dim aWd As Long
            Dim aHt As Long
            Dim dunits As dxxDeviceUnits
            Dim bNETColor As Boolean
            Dim nClr As System.Drawing.Color = rDisplay.BackGroundColor
            If aBackColor IsNot Nothing Then

                bNETColor = TypeOf (aBackColor) Is System.Drawing.Color
                If Not bNETColor Then
                    nClr = dxfColors.Win32ToWin64(TVALUES.To_INT(aBackColor))
                Else
                    nClr = aBackColor
                End If
            End If
            rChangeProps = New TPROPERTIES("Display Property Changes", True)
            '========= SET THE CURRENT UNITS =======================
            dunits = rDisplay.Units
            If aUnits IsNot Nothing Then
                If aUnits.HasValue Then
                    If aUnits.Value <> dxxDeviceUnits.Undefined Then dunits = aUnits.Value
                End If
            End If
            If rDisplay.Units <> dunits Then
                Dim ptype As dxxPropertyTypes = dxxPropertyTypes.dxf_Integer
                rChangeProps.Add(0, dunits, "UNITS", Nothing, ptype)
            End If
            '========= VERIFY THE DEVICE(BITMAP) DIMENSIONS =======================
            Dim aCtrl As Object = Nothing
            Dim flg As Boolean = False
            If rDisplay.DeviceIsDefined(aCtrl, flg) Then
                aWd = aCtrl.Width
                aHt = aCtrl.Height
            End If
            If aWd <> netBMP.Width Or aHt <> netBMP.Height Then
                netBMP.Resize(TVALUES.To_INT(aWd), TVALUES.To_INT(aHt))
            End If
            Dim dvPlane As TPLANE = netBMP.Plane(dxxDeviceUnits.Pixels, "DEVICE", True)
            'device plane change
            If Not rDisplay.pln_DEVICE.IsEqualTo(dvPlane, False, True, False) Then
                Dim ptype As dxxPropertyTypes = dxxPropertyTypes.dxf_String
                rChangeProps.Add(0, $"{dvPlane.Width} x  { dvPlane.Height}", "DEVICE.DIMENSIONS", Nothing, ptype, aLastVal:=$"{rDisplay.pln_DEVICE.Width} x {rDisplay.pln_DEVICE.Height}")
                rDisplay.pln_DEVICE = dvPlane
            End If
            'save the linear conversion factor
            pixsperunit = dxfUtils.FactorFromTo(dunits, dxxDeviceUnits.Pixels, netBMP.DPI)
            rDisplay.PixelsPerUnit = pixsperunit
            '========= ZOOM FACTOR =======================
            If rDisplay.pln_VIEW.XDirection.Magnitude <> 1 Then
                rDisplay.pln_VIEW.ResetDirections()
            End If
            Dim vwPlane As TPLANE = rDisplay.pln_VIEW
            ZF = rDisplay.ZoomFactor
            If aZoomFactor.HasValue Then ZF = aZoomFactor.Value
            If ZF <= 0 Then ZF = rDisplay.ZoomFactor
            If ZF <> rDisplay.ZoomFactor Then
                rDisplay.ZoomLast = rDisplay.ZoomFactor
                Dim ptype As dxxPropertyTypes = dxxPropertyTypes.dxf_Double
                rChangeProps.Add(0, ZF, "VIEW.ZOOMFACTOR", Nothing, ptype, aLastVal:=rDisplay.ZoomFactor)
                rDisplay.ZoomFactor = ZF
            End If
            '================== VIEW CENTER ======================
            If Not rDisplay.pln_VIEW.Origin.Equals(ioViewPlane.Origin, 4) Then
                Dim ptype As dxxPropertyTypes = dxxPropertyTypes.dxf_String
                rChangeProps.Add(0, ioViewPlane.Origin.Coordinates(0), "VIEW.FOCALPOINT", Nothing, ptype, aLastVal:=rDisplay.pln_VIEW.Origin.Coordinates(0))
                vwPlane.Origin = ioViewPlane.Origin
            End If
            '================== VIEW DIRECTIONS ======================
            If ioViewPlane.DirectionsAreDefined Then
                If Not rDisplay.pln_VIEW.IsEqualTo(ioViewPlane, True, False, False) Then
                    Dim ptype As dxxPropertyTypes = dxxPropertyTypes.dxf_String
                    rChangeProps.Add(0, ioViewPlane.DirectionDescriptor, "VIEW.DIRECTION", Nothing, ptype, aLastVal:=rDisplay.pln_VIEW.DirectionDescriptor)
                    vwPlane.Define(vwPlane.Origin, ioViewPlane.XDirection, ioViewPlane.YDirection)
                End If
            End If
            '================== BACKGROUND COLOR ======================
            If nClr <> rDisplay.BackGroundColor Then
                rChanged = True
                Dim ptype As dxxPropertyTypes = dxxPropertyTypes.dxf_String
                rChangeProps.Add(0, nClr.ToString, "DEVICE.BACKCOLOR", Nothing, ptype, aLastVal:=rDisplay.BackGroundColor.ToString)
                rDisplay.BackGroundColor = nClr
            End If
            'save the changes
            vwPlane.Width = (dvPlane.Width / pixsperunit) / ZF
            vwPlane.Height = (dvPlane.Height / pixsperunit) / ZF
            rDisplay.ZoomFactor = ZF
            rDisplay.pln_DEVICE = dvPlane.Clone
            rDisplay.pln_VIEWLAST = rDisplay.pln_VIEW.Clone
            rDisplay.pln_VIEWLAST.Units = rDisplay.Units
            rDisplay.Units = dunits
            vwPlane.Units = rDisplay.Units
            rDisplay.pln_VIEW = vwPlane.Clone
            rDisplay.pln_DEVICE.Name = "DEVICE"
            rDisplay.pln_VIEW.Name = "VIEW"
            rDisplay.pln_VIEWLAST.Name = "LASTVIEW"
            If rChangeProps.Count > 0 Then
                rChanged = True
                rDisplay.IsDirty = True
            End If
            If aImage IsNot Nothing Then
                rDisplay._ImageGUID = aImage.GUID
            End If
            If Not bSuppressImageUpdate Then
                If Not bSuppressEvnts Then
                    rDisplay.UpdateImage(aImage, rChangeProps, False)
                Else
                    rDisplay.UpdateImage(aImage, New TPROPERTIES(""), False)
                End If
                rDisplay = aImage.obj_DISPLAY
            End If
            If Not bSuppressImageUpdate And rDisplay.IsDirty Then
                If aImage IsNot Nothing Then aImage.Render()
            End If
            Return rDisplay
        End Function
        Public Shared Function NullDisplay(ioDisplay As TDISPLAY) As TDISPLAY

            Dim aCntrl As Object = Nothing
            Dim flg As Boolean = False
            Dim _rVal As New TDISPLAY("") With
            {
            .AutoRedraw = True,
            .Units = dxxDeviceUnits.Inches,
            .ColorMode = dxxColorModes.Full,
            .PixelsPerUnit = dxfUtils.FactorFromTo(dxxDeviceUnits.Inches, dxxDeviceUnits.Pixels, ioDisplay.DPI),
            .BackGroundColor = ioDisplay.BackGroundColor,
            .rec_EXTENTS = New TPLANE("EXTENTS", dxxDeviceUnits.Inches)
            }


            If Not ioDisplay.DeviceIsDefined(aCntrl, flg) Then
                _rVal.DeviceHwnd = IntPtr.Zero
                _rVal.pln_DEVICE = New TPLANE(TVALUES.To_DBL(Screen.PrimaryScreen.Bounds.Width), (Screen.PrimaryScreen.Bounds.Height), "DEVICE", dxxDeviceUnits.Pixels)
            Else
                _rVal.DeviceHwnd = ioDisplay.DeviceHwnd
                _rVal.pln_DEVICE = New TPLANE(TVALUES.To_DBL(aCntrl.Width), TVALUES.To_DBL(aCntrl.height), "DEVICE", dxxDeviceUnits.Pixels)
            End If
            _rVal.pln_DEVICE.Define(New TVECTOR(0.5 * _rVal.pln_DEVICE.Width, 0.5 * _rVal.pln_DEVICE.Height, 0), TVECTOR.WorldX, New TVECTOR(0, -1, 0))
            Dim sf As Double = dxfUtils.FactorFromTo(dxxDeviceUnits.Pixels, _rVal.Units)
            _rVal.pln_VIEW = New TPLANE(aWidth:=_rVal.pln_DEVICE.Width * sf, aHeight:=_rVal.pln_DEVICE.Height * sf, aName:="VIEW", aUnits:=_rVal.Units) With {.Origin = New TVECTOR(0.5 * _rVal.pln_DEVICE.Width * sf, 0.5 * _rVal.pln_DEVICE.Height * sf, 0)}
            _rVal.pln_VIEWLAST = New TPLANE(_rVal.pln_VIEW) With {.Name = "VIEWLAST"}

            Return _rVal
        End Function
        Friend Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    pln_DEVICE = Nothing
                    pln_VIEW = Nothing
                    pln_VIEWLAST = Nothing
                    rec_EXTENTS = Nothing
                    _DeviceHwnd = 0
                End If
                disposedValue = True
            End If
        End Sub
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TDISPLAY(Me)
        End Function
#End Region 'Shared MEthods
    End Structure 'TDISPLAY
    Friend Structure TTABLEENTRY
        Implements ICloneable
#Region "Members"
        Public BinaryData As TPROPERTIES
        Public ExtendedData As TPROPERTYARRAY
        Public Props As TPROPERTIES
        Public Reactors As TPROPERTYARRAY
        Public Values As TVALUES
        Private _Handlez As THANDLES
        Public AutoReset As Boolean
        Public IsCopied As Boolean
        Public IsDirty As Boolean
        Public IsGlobal As Boolean
        Public IsDefault As Boolean

        Public Suppressed As Boolean



#End Region 'Members
#Region "Constructors"



        Public Sub New(Optional aEntryType As dxxReferenceTypes = dxxReferenceTypes.UNDEFINED, Optional aName As String = "", Optional bIsDefault As Boolean = False, Optional aGUID As String = "") ', Optional bSuppressLTDefs As Boolean = False)

            'init -------------------------------------------------
            BinaryData = New TPROPERTIES("BinaryData")
            ExtendedData = New TPROPERTYARRAY("ExtendedData")
            Props = New TPROPERTIES("Props")
            Reactors = New TPROPERTYARRAY("Reactors")
            Values = New TVALUES(0)
            _Handlez = New THANDLES("")
            AutoReset = False
            IsCopied = False
            IsDirty = False
            IsGlobal = False
            IsDefault = False

            Suppressed = False
            _EntryType = dxxReferenceTypes.UNDEFINED

            'init -------------------------------------------------
            IsDefault = bIsDefault
            Index = -1
            EntryType = aEntryType
            ', Optional aColor As dxxColors = dxxColors.Undefined, Optional aHandle As String, Optional bSuppressLinetypeDefs As Boolean
            If aEntryType = dxxReferenceTypes.UNDEFINED Then Return

            If aEntryType < dxxReferenceTypes.DIMSETTINGS Then
                If aGUID = "" Then aGUID = dxfEvents.NextEntryGUID(aEntryType)
                Props = dxpProperties.GetReferenceProps(aEntryType, aName)

            Else
                Dim settype As dxxSettingTypes = aEntryType.SettingType
                If aGUID = "" Then aGUID = dxfEvents.NextSettingGUID(aEntryType)
                Props = New TPROPERTIES(dxpProperties.GetSettingsProperties(settype, aName))
            End If
            _Handlez = New THANDLES(aGUID)
            Props.GUID = aGUID


        End Sub

        Public Sub New(aEntry As TTABLEENTRY, Optional bCloneHandles As Boolean = False, Optional aImageGUID As String = Nothing)
            'init -------------------------------------------------
            BinaryData = New TPROPERTIES(aEntry.BinaryData)
            ExtendedData = New TPROPERTYARRAY(aEntry.ExtendedData)
            Props = New TPROPERTIES(aEntry.Props)
            Reactors = New TPROPERTYARRAY(aEntry.Reactors)
            Values = New TVALUES(aEntry.Values)
            _Handlez = IIf(bCloneHandles, New THANDLES(aEntry._Handlez), New THANDLES(""))
            AutoReset = aEntry.AutoReset
            IsCopied = aEntry.IsCopied
            IsDirty = aEntry.IsDirty
            IsGlobal = aEntry.IsGlobal
            IsDefault = aEntry.IsDefault

            Suppressed = aEntry.Suppressed
            _EntryType = aEntry._EntryType

            'init -------------------------------------------------
            If Not String.IsNullOrWhiteSpace(aImageGUID) Then ImageGUID = aImageGUID

        End Sub
        Public Sub New(aEntry As dxfTableEntry)
            'init -------------------------------------------------
            BinaryData = New TPROPERTIES("BinaryData")
            ExtendedData = New TPROPERTYARRAY("ExtendedData")
            Props = New TPROPERTIES("Props")
            Reactors = New TPROPERTYARRAY("Reactors")
            Values = New TVALUES(0)
            _Handlez = New THANDLES("")
            AutoReset = False
            IsCopied = False
            IsDirty = False
            IsGlobal = False
            IsDefault = False

            Suppressed = False
            _EntryType = dxxReferenceTypes.UNDEFINED

            'init -------------------------------------------------
            If aEntry Is Nothing Then Return
            'init -------------------------------------------------


            BinaryData = New TPROPERTIES(aEntry.BinaryData)
            ExtendedData = New TPROPERTYARRAY(aEntry.ExtendedData)
            Reactors = New TPROPERTYARRAY(aEntry.Reactors)
            _Handlez = aEntry.HStrukture
            AutoReset = aEntry.AutoReset
            IsCopied = aEntry.IsCopied
            IsDirty = aEntry.IsDirty
            IsGlobal = aEntry.IsGlobal
            IsDefault = aEntry.IsDefault

            Suppressed = aEntry.Suppressed
            _EntryType = aEntry.EntryType
            Props = New TPROPERTIES(aEntry.Properties)



            'init -------------------------------------------------

        End Sub

        Public Sub New(aSettings As dxfSettingObject)
            'init -------------------------------------------------
            BinaryData = New TPROPERTIES("BinaryData")
            ExtendedData = New TPROPERTYARRAY("ExtendedData")
            Props = New TPROPERTIES("Props")
            Reactors = New TPROPERTYARRAY("Reactors")
            Values = New TVALUES(0)
            _Handlez = New THANDLES("")
            AutoReset = False
            IsCopied = False
            IsDirty = False
            IsGlobal = False
            IsDefault = False

            Suppressed = False
            _EntryType = dxxReferenceTypes.UNDEFINED

            'init -------------------------------------------------
            If aSettings Is Nothing Then Return
            'init -------------------------------------------------



            AutoReset = aSettings.AutoReset
            IsCopied = aSettings.IsCopied
            IsDirty = aSettings.IsDirty
            IsGlobal = aSettings.IsGlobal
            IsDefault = aSettings.IsDefault

            Suppressed = aSettings.Suppressed
            _EntryType = aSettings.SettingType.ReferenceType()

            Props = New TPROPERTIES(aSettings.Properties)



            'init -------------------------------------------------

        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property AngleFormat As String
            Get
                '^the format string applied to angles
                Dim _rVal As String
                If EntryType <> dxxReferenceTypes.DIMSTYLE Then Return "0.0"
                '^the format string applied to angles
                Dim azin As dxxZeroSuppression
                Dim bNoLead As Boolean
                Dim bNoTrail As Boolean
                Dim prec As Integer
                'On Error Resume Next
                azin = PropValueI(dxxDimStyleProperties.DIMAZIN) 'ZeroSuppressionAngular
                If azin = dxxZeroSuppression.LeadingAndTrailing Or azin = dxxZeroSuppression.Leading Then
                    bNoLead = True
                End If
                If azin = dxxZeroSuppression.LeadingAndTrailing Or azin = dxxZeroSuppression.Trailing Then
                    bNoTrail = True
                End If
                prec = PropValueI(dxxDimStyleProperties.DIMADEC) ' AngularPrecision
                If prec < 0 Then prec = PropValueI(dxxDimStyleProperties.DIMDEC) 'LinearPrecision
                If bNoLead Then
                    If prec <= 0 Then _rVal = "0" Else _rVal = "#"
                Else
                    _rVal = "0"
                End If
                If prec > 0 Then
                    If bNoTrail Then
                        _rVal += $".{ New String("#", prec)}"
                    Else
                        _rVal += $".{ New String("0", prec)}"
                    End If
                End If
                Return _rVal
            End Get
        End Property
        Public Function LinearFormat(Optional bNoLead As Boolean = False, Optional bNoTrail As Boolean = False, Optional aPrecision As Integer = -1) As String
            Dim _rVal As String = "0.0000"
            If EntryType <> dxxReferenceTypes.DIMSTYLE Then Return _rVal
            '^the format string applied to linear numbers
            Dim azin As dxxZeroSuppression
            Dim prec As Integer
            azin = PropValueI(dxxDimStyleProperties.DIMZEROSUPPRESSION) ' ZeroSuppression
            'On Error Resume Next
            If azin = dxxZeroSuppression.Leading Or azin = dxxZeroSuppression.LeadingAndTrailing Then
                bNoLead = True
            End If
            If azin = dxxZeroSuppression.LeadingAndTrailing Or azin = dxxZeroSuppression.Trailing Then
                bNoTrail = True
            End If
            If aPrecision < 0 Then
                prec = PropValueI(dxxDimStyleProperties.DIMDEC) 'LinearPrecision
            Else
                prec = aPrecision
            End If
            If bNoLead Then
                If prec <= 0 Then _rVal = "0" Else _rVal = "#"
            Else
                _rVal = "0"
            End If
            If prec > 0 Then
                If bNoTrail Then
                    _rVal += "." & New String("#", prec)
                Else
                    _rVal += "." & New String("0", prec)
                End If
            End If
            Return _rVal
        End Function

        Private _EntryType As dxxReferenceTypes
        Public Property EntryType As dxxReferenceTypes
            Get
                Return _EntryType
            End Get
            Set(value As dxxReferenceTypes)
                If value = _EntryType Then Return
                _EntryType = value

            End Set
        End Property
        Public ReadOnly Property SettingType As dxxReferenceTypes
            Get
                If Not IsGlobal Then
                    Return EntryType
                Else
                    If EntryType = dxxReferenceTypes.DIMSTYLE Then
                        Return dxxSettingTypes.DIMOVERRIDES
                    Else
                        Return EntryType
                    End If
                End If
            End Get
        End Property
        Public Property Domain As dxxDrawingDomains
            Get
                Return _Handlez.Domain
            End Get
            Set(value As dxxDrawingDomains)
                _Handlez.Domain = value
            End Set
        End Property
        Friend Property Handlez As THANDLES
            Get
                Handlez = _Handlez
            End Get
            Set(value As THANDLES)
                _Handlez = New THANDLES(value, GUID)
            End Set
        End Property
        Public Property Index As Integer
            Get
                Return _Handlez.Index
            End Get
            Set(value As Integer)
                _Handlez.Index = value
            End Set
        End Property
        Public ReadOnly Property Invisible As Boolean
            Get
                Dim _rVal As Boolean = Suppressed
                If Not _rVal Then
                    If EntryType = dxxReferenceTypes.LTYPE Then
                        _rVal = Name.ToUpper = "INVISIBLE"
                    ElseIf EntryType = dxxReferenceTypes.BLOCK_RECORD Then
                        _rVal = Name.ToUpper = "_CLOSEDFILLED"
                    End If
                End If
                Return _rVal
            End Get
        End Property
        Public Property Name As String
            Get
                Select Case EntryType
                    Case dxxReferenceTypes.DIMSTYLE
                        Return PropValueStr(dxxDimStyleProperties.DIMNAME)
                    Case dxxReferenceTypes.STYLE
                        Return PropValueStr(dxxStyleProperties.NAME)
                    Case dxxReferenceTypes.LAYER
                        Return PropValueStr(dxxLayerProperties.Name)
                    Case Else
                        Return Props.GCValueStr(2)
                End Select

            End Get
            Friend Set(value As String)
                Props.SetVal("Name", value)
            End Set
        End Property
        Public ReadOnly Property HandleGroupCode As Integer
            Get
                If EntryType = dxxReferenceTypes.DIMSTYLE Then
                    Return 105
                Else
                    Return 5
                End If
            End Get
        End Property
        Public Property Handle As String
            Get
                Return Props.GCValueStr(HandleGroupCode)
            End Get
            Set(value As String)
                If value <> _Handlez.Handle Then
                    Props.SetValGC(HandleGroupCode, value, aOccurance:=1)
                    _Handlez.Handle = value
                End If
            End Set
        End Property
        Public Property GUID As String
            Get
                Return _Handlez.GUID
            End Get
            Set(value As String)
                _Handlez.GUID = value
                Props.SetVal("*GUID", value)
            End Set
        End Property
        Public Property OwnerGUID As String
            Get
                Return _Handlez.OwnerGUID
            End Get
            Set(value As String)
                _Handlez.OwnerGUID = value
            End Set
        End Property
        Public Property ImageGUID As String
            Get
                Return _Handlez.ImageGUID
            End Get
            Set(value As String)
                _Handlez.ImageGUID = value
            End Set
        End Property
        Public ReadOnly Property Image As dxfImage
            '^ the guid of the entry's parent image
            Get
                If Not String.IsNullOrWhiteSpace(ImageGUID) Then Return dxfEvents.GetImage(ImageGUID) Else Return Nothing
            End Get
        End Property
        Public ReadOnly Property EntryTypeName As String
            Get
                Return dxfEnums.DisplayName(EntryType)
            End Get
        End Property

#End Region 'Properties
#Region "Methods"
        Public Sub Clear()

            BinaryData = New TPROPERTIES("BinaryData")
            ExtendedData = New TPROPERTYARRAY("ExtendedData")
            Props = New TPROPERTIES("Props")
            Reactors = New TPROPERTYARRAY("Reactors")

        End Sub
        Public Function Clone(Optional bCloneHandles As Boolean = False) As TTABLEENTRY
            Return New TTABLEENTRY(Me, bCloneHandles)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TTABLEENTRY(Me)
        End Function

        Public Function FormatAngle(Num As Object, Optional aAngUnits As dxxAngularUnits = dxxAngularUnits.Undefined) As String
            Dim _rVal As String = String.Empty
            '#1the number to format
            '^used to apply the current angle format to the passed number
            'On Error Resume Next
            Dim aVal As Double
            Dim aSt As dxxAngularUnits
            If aAngUnits >= 0 And aAngUnits <= dxxAngularUnits.Radians Then
                aSt = aAngUnits
            Else
                aSt = PropValueI(dxxDimStyleProperties.DIMAUNIT)
            End If
            If TVALUES.IsNumber(Num) Then aVal = TVALUES.To_DBL(Num)
            Select Case aSt
                Case dxxAngularUnits.DegreesDecimal
                    _rVal = Format(aVal, AngleFormat)
                    If _rVal.EndsWith(".") Then _rVal = _rVal.Substring(1, _rVal.Length - 1)
                    _rVal += "%%D"
                Case dxxAngularUnits.DegreesMinSec
                    _rVal = Format(aVal, AngleFormat)
                    If _rVal.EndsWith(".") Then _rVal = _rVal.Substring(1, _rVal.Length - 1)
                    aVal = TVALUES.To_DBL(_rVal)
                    _rVal = $"{Fix(aVal)}%%D"
                    aVal -= Fix(aVal)
                    _rVal += Fix(aVal * 60) & "'"
                    aVal = (aVal * 60) - Fix(aVal * 60)
                    _rVal += $"{Fix(aVal * 60) }''"
                Case dxxAngularUnits.Gradians
                    aVal *= (400 / 360)
                    _rVal = Format(aVal, AngleFormat)
                    If _rVal.EndsWith(".") Then _rVal = _rVal.Substring(1, _rVal.Length - 1)
                    _rVal += "g"
                Case dxxAngularUnits.Radians
                    aVal *= Math.PI / 180
                    _rVal = Format(aVal, AngleFormat)
                    If _rVal.EndsWith(".") Then _rVal = _rVal.Substring(1, _rVal.Length - 1)
                    _rVal += "r"
            End Select
            Return _rVal
        End Function

        ''' <summary>
        ''' applies the dim styles linear format to the passed number
        ''' </summary>
        ''' <param name="Num">the number to format</param>
        ''' <param name="bApplyLinearMultiplier"></param>
        ''' <param name="aType"></param>
        ''' <param name="aPrecision"></param>
        ''' <returns></returns>
        Public Function FormatNumber(Num As Object, Optional bApplyLinearMultiplier As Boolean = True, Optional aType As dxxLinearUnitFormats = dxxLinearUnitFormats.Undefined, Optional aPrecision As Integer = -1) As String
            Dim _rVal As String

            Dim bNoLead As Boolean
            Dim bNoTrail As Boolean
            Dim aVal As Double
            Dim bVal As Double
            Dim cWhole As Long
            Dim aFrac As Double
            Dim aStr As String
            Dim bStr As String
            Dim fStr As String
            Dim aRnd As Double = PropValueD(dxxDimStyleProperties.DIMRND) 'RoundTo
            Dim sRem As Double
            Dim aPrecis As Integer
            Dim aNumer As String = String.Empty
            Dim aDenom As String = String.Empty
            Dim cStackType As dxxDimFractionStyles
            Dim zSupArch As dxxZeroSuppressionsArchitectural
            If Not dxfEnums.Validate(GetType(dxxLinearUnitFormats), aType, dxxLinearUnitFormats.WindowsDesktop.ToString, True) Then
                aType = PropValueI(dxxDimStyleProperties.DIMLUNIT)
            End If
            If TVALUES.IsNumber(Num) Then aVal = TVALUES.To_DBL(Num)
            If bApplyLinearMultiplier Then aVal *= PropValueD(dxxDimStyleProperties.DIMLFAC)
            If aRnd > 0 Then
                bVal = Math.Truncate(aVal / aRnd) * aRnd
                sRem = aVal - bVal
                If sRem >= aRnd Then bVal += aRnd
                aVal = bVal
            End If
            Select Case aType
                Case dxxLinearUnitFormats.Scientific
                    _rVal = Format(aVal, "Scientific")
                Case dxxLinearUnitFormats.Engineering
                    _rVal = Format(aVal, LinearFormat(bNoLead, aPrecision:=aPrecision))
                    If _rVal.StartsWith(".") Then _rVal = _rVal.Substring(1, _rVal.Length - 1)
                    If TVALUES.To_DBL(_rVal) = 0 Then
                        If Not bNoLead Then _rVal = "0.0" Else _rVal = "0"
                    End If
                    _rVal += Chr(34)
                Case dxxLinearUnitFormats.Architectural
                    cStackType = PropValueD(dxxDimStyleProperties.DIMFRAC) 'FractionStackType
                    zSupArch = PropValueI(dxxDimStyleProperties.DIMZEROSUPPRESSION_ARCH) ' ZeroSuppressionArchitectural

                    fStr = ""
                    aVal /= 12
                    cWhole = Fix(aVal)
                    aFrac = aVal - cWhole
                    aStr = cWhole.ToString()
                    If cWhole = 0 Then
                        If zSupArch <> dxxZeroSuppressionsArchitectural.IncludeZeroFeetAndSuppressZeroInches Or zSupArch <> dxxZeroSuppressionsArchitectural.IncludeZeroFeetAndZeroInches Then
                            aStr = ""
                        End If
                    End If
                    If aStr <> "" Then aStr += "\"
                    aVal = aFrac * 12
                    cWhole = Fix(aVal)
                    aFrac = aVal - cWhole
                    bStr = cWhole.ToString()
                    If aFrac > 0 Then
                        aPrecis = PropValueI(dxxDimStyleProperties.DIMDEC) ' LinearPrecision
                        If aPrecis > 0 Then
                            Dim d1 As Double = 0
                            If dxfUtils.ComputeFraction(aFrac, aPrecis, aNumer, aDenom, True, d1) Then
                                If aNumer = aDenom Then
                                    bStr = (cWhole + 1).ToString()
                                Else
                                    If cStackType = dxxDimFractionStyles.NoStack Then
                                        fStr = $" { aNumer}/{ aDenom}"
                                    ElseIf cStackType = dxxDimFractionStyles.Diagonal Then
                                        fStr = dxfUtils.CreateStackedText(aNumer, aDenom, dxxCharacterStackStyles.Diagonal)
                                    Else
                                        fStr = dxfUtils.CreateStackedText(aNumer, aDenom, dxxCharacterStackStyles.Horizontal)
                                    End If
                                End If
                            End If
                        End If
                    End If
                    bStr += fStr
                    If bStr = "0" Then
                        If zSupArch = dxxZeroSuppressionsArchitectural.IncludeZeroFeetAndSuppressZeroInches Or zSupArch = dxxZeroSuppressionsArchitectural.ZeroFeetAndZeroInches Then
                            bStr = ""
                        End If
                    End If
                    If bStr <> "" Then bStr += """"
                    If aStr <> "" Then
                        _rVal = aStr
                        If bStr <> "" Then _rVal += $"-{ bStr}"
                    Else
                        _rVal = bStr
                    End If
                    If _rVal = "" Then _rVal = "0" & Chr(34)
                Case dxxLinearUnitFormats.Fractional
                    cStackType = PropValueI(dxxDimStyleProperties.DIMFRAC) 'FractionStackType
                    zSupArch = PropValueI(dxxDimStyleProperties.DIMZEROSUPPRESSION_ARCH) 'ZeroSuppressionArchitectural

                    bStr = ""
                    fStr = ""
                    cWhole = Fix(aVal)
                    aFrac = aVal - cWhole
                    aStr = cWhole.ToString()
                    If cWhole = 0 Then
                        If zSupArch <> dxxZeroSuppressionsArchitectural.IncludeZeroFeetAndSuppressZeroInches Or zSupArch <> dxxZeroSuppressionsArchitectural.IncludeZeroFeetAndZeroInches Then
                            aStr = ""
                        End If
                    End If
                    aVal = aFrac
                    cWhole = Fix(aVal)
                    aFrac = aVal - cWhole
                    If cWhole > 0 Then bStr = cWhole.ToString()
                    If aFrac > 0 Then
                        aPrecis = PropValueI(dxxDimStyleProperties.DIMDEC) 'LinearPrecision
                        If aPrecis > 0 Then
                            Dim d1 As Double = 0
                            If dxfUtils.ComputeFraction(aFrac, aPrecis, aNumer, aDenom, True, d1) Then
                                If aNumer = aDenom Then
                                    bStr = (cWhole + 1).ToString()
                                Else
                                    If cStackType = dxxDimFractionStyles.NoStack Then
                                        fStr = " " & aNumer & "/" & aDenom
                                    ElseIf cStackType = dxxDimFractionStyles.Diagonal Then
                                        fStr = dxfUtils.CreateStackedText(aNumer, aDenom, dxxCharacterStackStyles.Diagonal)
                                    Else
                                        fStr = dxfUtils.CreateStackedText(aNumer, aDenom, dxxCharacterStackStyles.Horizontal)
                                    End If
                                End If
                            End If
                        End If
                    End If
                    bStr += fStr
                    If bStr = "0" Then
                        If zSupArch = dxxZeroSuppressionsArchitectural.IncludeZeroFeetAndSuppressZeroInches Or zSupArch = dxxZeroSuppressionsArchitectural.ZeroFeetAndZeroInches Then
                            bStr = ""
                        End If
                    End If
                    _rVal = aStr & bStr
                    If _rVal = "" Then _rVal = "0"
                Case Else
                    _rVal = Format(aVal, LinearFormat(bNoLead, bNoTrail, aPrecision))
                    If _rVal.EndsWith(".") Then _rVal = _rVal.Substring(0, _rVal.Length - 1)

                    If _rVal = "" Then
                        If bNoTrail Then _rVal = "0" Else _rVal = "0.0"
                    End If
            End Select
            If _rVal = "" Then _rVal = "0"
            Dim sep As String = Chr(PropValueI(dxxDimStyleProperties.DIMDSEP))
            If sep <> "" Then _rVal = _rVal.Replace(".", sep)
            Return _rVal
        End Function
        Public Function GetLinetpyeStyleData(ByRef rPatternLength As Double, Optional bRegen As Boolean = False) As TVALUES
            Dim _rVal As TVALUES = Values
            'On Error Resume Next
            Dim i As Integer
            Dim idx As Integer
            Dim aProps As TPROPERTIES
            Dim eCnt As Integer
            Dim ltname As String
            Dim conv As Double
            Dim gcprops As List(Of TPROPERTY) = Props.GroupCodeMembers(49)

            ltname = Props.GCValueStr(2).ToUpper
            eCnt = Props.GCValueI(73)
            aProps = Reactors.GetProps("ElementLengths", idx, False)
            If _rVal.Count <> eCnt Or eCnt <> gcprops.Count Then bRegen = True

            If bRegen Then
                _rVal = New TVALUES
                eCnt = gcprops.Count
                If ltname.ToUpper().Contains("ISO") Then conv = 1 / 25.4 Else conv = 1
                Props.SetValGC(73, eCnt, 1)
                If gcprops.Count > 0 Then
                    Dim tot As Double = 0


                    For i = 1 To gcprops.Count
                        Dim sval As Single = TVALUES.To_SNG(gcprops.Item(i - 1).Value) * conv
                        tot += sval
                        _rVal.Add(sval)

                    Next i
                    _rVal.BaseValue = tot

                End If
                If idx >= 0 Then Reactors.ClearMember("ElementLengths")
                Reactors.Add(gcprops, "ElementLengths")

                _rVal.Defined = True
            End If
            Values = _rVal

            rPatternLength = TVALUES.To_DBL(_rVal.BaseValue)
            Return _rVal
        End Function
        Friend Function GetHeaderDimstyleProps(ByRef rDimStyleName As String) As TPROPERTIES
            Dim _rVal As TPROPERTIES = Props.Clone(True)
            _rVal.Name = "HeaderDimstyleProps"
            rDimStyleName = "Standard"
            If EntryType <> dxxSettingTypes.HEADER Then Return _rVal
            Dim aProp As TPROPERTY
            Dim bProp As TPROPERTY
            Dim bKeep As Boolean
            Dim aName As String
            Dim dsProps As TPROPERTIES = dxpProperties.GetReferenceProps(dxxReferenceTypes.DIMSTYLE)
            Dim ptype As dxxHeaderVars
            For i As Integer = 1 To Props.Count
                aProp = Props.Item(i)
                aName = aProp.Name.ToUpper
                ptype = aProp.EnumName
                bKeep = String.Compare(Left(aName, 4), "$Dim", True) = 0
                If bKeep Then
                    If aName = "$DIMSTYLE" Then
                        rDimStyleName = aProp.StringValue
                        bKeep = False
                    End If
                    If aName = "$DIMASSOC" Or aName = "$DIMASO" Or aName = "$DIMSHO" Then bKeep = False
                    If bKeep Then
                        If Left(aName, 7) = "$DIMBLK" Then
                            bKeep = False
                            If aProp.StringValue() = "" Then aProp.Value = "_ClosedFilled"
                            aProp.Name = $"*{ aName}"
                            _rVal.Add(aProp)
                        ElseIf Left(aName, 8) = "$DIMLTEX" Then
                            bKeep = False
                            If aProp.StringValue() = "" Then aProp.Value = dxfLinetypes.ByBlock
                            aProp.Name = $"*{ aName}"
                            _rVal.Add(aProp)
                        ElseIf aName = "$DIMLTYPE" Then
                            bKeep = False
                            If aProp.StringValue() = "" Then aProp.Value = dxfLinetypes.ByBlock
                            aProp.Name = $"*{ aName}"
                            _rVal.Add(aProp)
                        ElseIf aName = "$DIMTXSTY" Then
                            bKeep = False
                            If aProp.StringValue() = "" Then aProp.Value = "Standard"
                            aProp.Name = $"*{ aName}"
                            _rVal.Add(aProp)
                        End If
                    End If
                End If
                If bKeep Then
                    aProp.GroupCode = bProp.GroupCode
                    _rVal.Add(aProp)
                End If
            Next i
            Return _rVal
        End Function
        Friend Sub CopyProperties(aFromEntry As TTABLEENTRY)
            If EntryType <> aFromEntry.EntryType Then Return
            If EntryType = dxxReferenceTypes.LAYER Then
                Props.CopyValuesByGC(aFromEntry.Props, "0,2,100,330,5,390,347", True, bIgnoreHandles:=True)
            ElseIf EntryType = dxxReferenceTypes.DIMSTYLE Then
                Props.CopyValuesByGC(aFromEntry.Props, "0,2,100,330,105", True, bIgnoreHandles:=True)
            ElseIf EntryType = dxxReferenceTypes.APPID Then
                Props.CopyValuesByGC(aFromEntry.Props, "0,2,100,330,5", True, bIgnoreHandles:=True)
            ElseIf EntryType = dxxReferenceTypes.BLOCK_RECORD Then
                Props.CopyValuesByGC(aFromEntry.Props, "0,2,100,330,5", True, bIgnoreHandles:=True)
            ElseIf EntryType = dxxReferenceTypes.UCS Then
                Props.CopyValuesByGC(aFromEntry.Props, "0,2,100,330,5", True, bIgnoreHandles:=True)
            ElseIf EntryType = dxxReferenceTypes.STYLE Then
                Props.CopyValuesByGC(aFromEntry.Props, "0,2,100,330,5,71,42,3,4", True, False, bIgnoreHandles:=True)
            ElseIf EntryType = dxxReferenceTypes.LTYPE Then
                Dim idx1 As String = Props.GroupCodeKey(3)
                Dim idx2 As String = aFromEntry.Props.GroupCodeKey(3)


                If idx1 <> 0 And idx2 <> 0 Then
                    Props.RemoveToKey(idx1)

                    Props.Append(aFromEntry.Props.RemoveToKey(idx2))
                End If
            End If
        End Sub
        Public Sub ReferenceADD(aReference As String, Optional aGC As Integer = 330)
            aReference = Trim(aReference)
            If aReference <> "" Then
                Reactors.AddReactor("{ACAD_REACTORS", aGC, aReference)
            End If
        End Sub
        Public Sub ReferenceREMOVE(aReference As String)
            Props.RemoveByStringValue(aReference)
        End Sub
        Public Function ToReference() As dxfTableEntry
            Select Case EntryType
                Case dxxReferenceTypes.DIMSTYLE
                    Return New dxoDimStyle(Me)
                Case dxxReferenceTypes.LAYER
                    Return New dxoLayer(Me)
                Case dxxReferenceTypes.LTYPE
                    Return New dxoLinetype(Me)
                Case dxxReferenceTypes.STYLE
                    Return New dxoStyle(Me)
                Case Else
                    Return Nothing
            End Select
        End Function
        Public Overrides Function ToString() As String
            Return dxfEnums.DisplayName(_EntryType) + "{" & Name + "}"
        End Function
        Friend Function UpdateFontName(aFontName As String, aFontStyle As String) As Boolean
            Dim rName As String = String.Empty
            Dim rStyle As String = String.Empty
            Dim rOldValue As String = String.Empty
            Dim rNewValue As String = String.Empty
            Return UpdateFontName(aFontName, aFontStyle, rName, rStyle, rOldValue, rNewValue)
        End Function
        Friend Function UpdateFontName(aFontName As String, aFontStyle As String, ByRef rName As String, ByRef rStyle As String) As Boolean
            Dim rOldValue As String = String.Empty
            Dim rNewValue As String = String.Empty
            Return UpdateFontName(aFontName, aFontStyle, rName, rStyle, rOldValue, rNewValue)
        End Function
        Friend Function UpdateFontName(aFontName As String, aFontStyle As String, ByRef rName As String, ByRef rStyleName As String, ByRef rOldValue As String, ByRef rNewValue As String) As Boolean
            If EntryType <> dxxReferenceTypes.STYLE Then Return False
            rName = PropValueStr(dxxStyleProperties.FONTNAME)
            rStyleName = PropValueStr(dxxStyleProperties.FONTSTYLE)
            rOldValue = ""
            rNewValue = ""
            If String.IsNullOrWhiteSpace(aFontName) Then aFontName = rName
            If String.IsNullOrWhiteSpace(aFontName) Then Return "Txt.shx"
            aFontName = aFontName.Trim
            If aFontStyle IsNot Nothing Then aFontStyle = aFontStyle.Trim() Else aFontStyle = rStyleName

            Dim fnt As String = aFontName
            Dim sty As String = aFontStyle
            Dim i As Integer = aFontName.IndexOf(";") + 1
            Dim newstyle As TFONTSTYLE
            If aFontName.Contains(";") Then
                sty = dxfUtils.RightOf(aFontName, ";")
                fnt = dxfUtils.LeftOf(aFontName, ";")
            End If
            Dim ext As String = IO.Path.GetExtension(fnt).ToLower()
            Dim font As dxoFont = dxfGlobals.GetFont(fnt, "", True, bReturnDefault:=False)
            If font Is Nothing And ext = ".ttf" Then
                Dim tryit As String = aFontStyle.Trim
                tryit = $"{fnt.Substring(0, fnt.Length - 4)} {tryit}{ext}"

                font = dxfGlobals.GetFont(tryit, rName, True)

            End If

            If font Is Nothing Then Return False
            newstyle = font.GetStyleStructure(sty, True)
            If fnt.Contains(".") Then
                ext = IO.Path.GetExtension(fnt)
                fnt = IO.Path.GetFileNameWithoutExtension(fnt)

                aFontName = $"{ fnt}{ext}"
            End If
            Dim bIsShape As Boolean
            bIsShape = PropValueB(dxxStyleProperties.SHAPEFLAG)
            rOldValue = $"{rName};{ rStyleName };{ bIsShape}"
            rNewValue = rOldValue
            Dim fInfo As TFONTSTYLEINFO = dxoFonts.GetFontStyleInfo(aFontName, aStyleName:=newstyle.StyleName)
            aFontName = fInfo.FontName
            aFontStyle = fInfo.StyleName
            Dim rChanged As Boolean = PropValueSet(dxxStyleProperties.FONTINDEX, fInfo.FontIndex)
            If String.Compare(aFontName, rName, ignoreCase:=True) <> 0 Or String.Compare(aFontStyle, rStyleName, ignoreCase:=True) <> 0 Or bIsShape <> fInfo.IsShape Then rChanged = True
            If Not rChanged Then Return False
            rName = fInfo.FontName
            rStyleName = fInfo.StyleName
            PropValueSet(dxxStyleProperties.FONTNAME, rName)
            PropValueSet(dxxStyleProperties.FONTSTYLE, rStyleName)
            PropValueSet(dxxStyleProperties.FONTSTYLETYPE, fInfo.TTFStyle)
            PropValueSet(dxxStyleProperties.SHAPEFLAG, TPROPERTY.SwitchValue(fInfo.IsShape))
            Props.SetVal("*FontFileName", newstyle.FileName)
            rNewValue = $"{rName};{ rStyleName };{ fInfo.IsShape}"
            rChanged = String.Compare(rNewValue, rOldValue, True) <> 0
            If rChanged Then
                If Not fInfo.IsShape Then
                    PropValueSet(dxxStyleProperties.VERTICAL, False)
                End If
            End If
            Return True
        End Function
        Public Function GetHeaderVal(aIndex As dxxHeaderVars) As Object
            Dim rExists As Boolean = False
            Dim rHidden As Boolean = False
            Return GetHeaderVal(aIndex, rExists, rHidden)
        End Function

        Public Function GetHeaderVal_Str(aIndex As dxxHeaderVars) As String
            Dim rExists As Boolean = False
            Dim rHidden As Boolean = False
            Dim oval As Object = GetHeaderVal(aIndex, rExists, rHidden)
            If Not rExists Then Return String.Empty
            Return oval.ToString()
        End Function
        Public Function GetHeaderVal_I(aIndex As dxxHeaderVars) As Integer
            Dim rExists As Boolean = False
            Dim rHidden As Boolean = False
            Dim oval As Object = GetHeaderVal(aIndex, rExists, rHidden)
            If Not rExists Then Return 0
            Return TVALUES.To_INT(oval)
        End Function
        Public Function GetHeaderVal_D(aIndex As dxxHeaderVars) As Double
            Dim rExists As Boolean = False
            Dim rHidden As Boolean = False
            Dim oval As Object = GetHeaderVal(aIndex, rExists, rHidden)
            If Not rExists Then Return 0
            Return TVALUES.To_DBL(oval)
        End Function
        Public Function GetHeaderVal_B(aIndex As dxxHeaderVars) As Boolean
            Dim rExists As Boolean = False
            Dim rHidden As Boolean = False
            Dim sKey As String = dxfEnums.PropertyName(aIndex)
            Dim aProp As TPROPERTY = TPROPERTY.Null
            rExists = Props.TryGet(sKey, aProp)
            If Not rExists Then Return False
            Return Props.ValueB(aProp.Index)
        End Function
        Public Function GetHeaderVal(aIndex As dxxHeaderVars, ByRef rExists As Boolean, ByRef rHidden As Boolean) As Object
            rHidden = False
            Dim sKey As String = dxfEnums.PropertyName(aIndex)
            Dim aProp As TPROPERTY = TPROPERTY.Null
            rExists = Props.TryGet(sKey, aProp)
            If Not rExists Then Return String.Empty

            rHidden = aProp.Hidden
            Return aProp.Value
        End Function
        Public Function GetHeaderProp(aIndex As dxxHeaderVars, ByRef rExists As Boolean, ByRef rHidden As Boolean) As TPROPERTY
            rHidden = False
            Dim sKey As String = dxfEnums.PropertyName(aIndex)
            Dim aProp As TPROPERTY = TPROPERTY.Null
            rExists = Props.TryGet(sKey, aProp)
            If rExists Then rHidden = aProp.Hidden
            Return aProp
        End Function
        Public Function PropertyGet(aIndex As [Enum], Optional aOccur As Integer = 0) As TPROPERTY
            Dim rFound As Boolean = False
            Dim rPropName As String = String.Empty
            Return PropertyGet(aIndex, aOccur, rFound, rPropName)
        End Function
        Public Function PropertyGet(aIndex As [Enum], aOccur As Integer, ByRef rFound As Boolean, ByRef rPropName As String, Optional bSuppressNotFoundError As Boolean = False) As TPROPERTY
            rFound = False
            Dim _rVal As TPROPERTY = TPROPERTY.Null
            Try
                rPropName = dxfEnums.PropertyName(aIndex)
                rFound = Props.TryGet(rPropName, _rVal)
                If Not rFound Then
                    If Not bSuppressNotFoundError Then Throw New Exception($"{EntryTypeName } Property Not Found - { rPropName}")
                End If
                Return _rVal
            Catch ex As Exception
                'add an error
                Throw ex
                Return TPROPERTY.Null
            End Try
        End Function
        Public Function PropValueGet(aIndex As [Enum], Optional aOccur As Integer = 0) As Object
            Dim rFound As Boolean = False
            Dim rPropName As String = String.Empty
            Return PropValueGet(aIndex, aOccur, rFound, rPropName)
        End Function
        Public Function PropValueGet(aIndex As [Enum], ByRef rFound As Boolean, Optional aOccur As Integer = 0) As Object
            Dim rPropName As String = String.Empty
            Return PropValueGet(aIndex, aOccur, rFound, rPropName)
        End Function
        Public Function PropValueGet(aIndex As [Enum], aOccur As Integer, ByRef rFound As Boolean, ByRef rPropNname As String) As Object
            rFound = False
            Dim aProp As TPROPERTY
            Try
                'get the property
                aProp = PropertyGet(aIndex, aOccur, rFound, rPropNname)
                'return the value
                If rFound Then Return aProp.Value Else Return String.Empty
            Catch ex As Exception
                Throw ex
                Return String.Empty
            End Try
        End Function
        Friend Function PropValueI(aIndex As [Enum], Optional aOccur As Integer = 0, Optional bAbsVal As Boolean = False) As Integer
            Dim rFound As Boolean = False
            Return PropValueI(aIndex, aOccur, rFound, bAbsVal)
        End Function
        Friend Function PropValueI(aIndex As [Enum], aOccur As Integer, ByRef rFound As Boolean, Optional bAbsVal As Boolean = False) As Integer
            rFound = False
            Try
                Dim _rVal As Integer = TVALUES.To_INT(PropValueGet(aIndex, rFound, aOccur))
                If bAbsVal Then _rVal = Math.Abs(_rVal)
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return 0
            End Try
        End Function
        Friend Function PropValueL(aIndex As [Enum], Optional aOccur As Integer = 0, Optional bAbsVal As Boolean = False) As Long
            Dim rFound As Boolean = False
            Return PropValueL(aIndex, aOccur, rFound, bAbsVal)
        End Function
        Friend Function PropValueL(aIndex As [Enum], aOccur As Integer, ByRef rFound As Boolean, Optional bAbsVal As Boolean = False) As Long
            rFound = False
            Try
                Dim _rVal As Long = TVALUES.To_LNG(PropValueGet(aIndex, rFound, aOccur))
                If bAbsVal Then _rVal = Math.Abs(_rVal)
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return 0
            End Try
        End Function
        Friend Function PropValueStr(aIndex As [Enum], Optional aOccur As Integer = 0) As String
            Dim rFound As Boolean = False
            Return PropValueStr(aIndex, aOccur, rFound)
        End Function
        Friend Function PropValueStr(aIndex As [Enum], aOccur As Integer, ByRef rFound As Boolean) As String
            rFound = False
            Try
                Return TVALUES.To_STR(PropValueGet(aIndex, rFound, aOccur))
            Catch ex As Exception
                Throw ex
                Return String.Empty
            End Try
        End Function
        Friend Function PropValueB(aIndex As [Enum], Optional aOccur As Integer = 0) As Boolean
            Dim rFound As Boolean = False
            Return PropValueB(aIndex, aOccur, rFound)
        End Function
        Friend Function PropValueB(aIndex As [Enum], aOccur As Integer, ByRef rFound As Boolean) As Boolean
            rFound = False
            Dim _rVal As Boolean
            Try
                Dim pname As String = String.Empty
                Dim aProp As TPROPERTY = PropertyGet(aIndex, aOccur, rFound, pname)
                Dim iVal As Integer
                Dim pval As Object = aProp.Value
                If pname.ToLower().IndexOf("bit code") >= 0 Or aProp.PropType = dxxPropertyTypes.BitCode Then
                    iVal = Math.Abs(TVALUES.To_INT(aIndex))
                    If iVal > 100 Then iVal -= 100
                    _rVal = TVALUES.BitCode_FindSubCode(TVALUES.To_INT(pval), iVal)
                Else
                    If String.Compare(pname, "Color", True) = 0 Then
                        _rVal = pval >= 0
                    Else
                        If aProp.PropType = dxxPropertyTypes.Switch Then
                            _rVal = TVALUES.To_INT(pval) = 1
                        Else
                            _rVal = TVALUES.ToBoolean(pval)
                        End If
                    End If
                End If
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return False
            End Try
        End Function
        Friend Function PropValueS(aIndex As [Enum], Optional aOccur As Integer = 0) As Single
            Dim rFound As Boolean = False
            Return PropValueS(aIndex, aOccur, rFound)
        End Function
        Friend Function PropValueS(aIndex As [Enum], aOccur As Integer, ByRef rFound As Boolean) As Single
            rFound = False
            Try
                Dim _rVal As Single = TVALUES.To_SNG(PropValueGet(aIndex, rFound, aOccur))
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return 0
            End Try
        End Function
        Friend Function PropValueD(aIndex As [Enum], Optional aOccur As Integer = 0) As Double
            Dim rFound As Boolean = False
            Return PropValueD(aIndex, aOccur, rFound)
        End Function
        Friend Function PropValueD(aIndex As [Enum], aOccur As Integer, ByRef rFound As Boolean) As Double
            rFound = False
            Try
                Dim _rVal As Double = TVALUES.To_DBL(PropValueGet(aIndex, rFound, aOccur))
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return 0
            End Try
        End Function




        Public Function PropValueSet(aIndex As [Enum], aValue As Object, Optional aOccur As Integer = 0) As Boolean
            Dim rFound As Boolean = False
            Dim rProp As TPROPERTY = TPROPERTY.Null
            Return PropValueSet(aIndex, aValue, aOccur, rFound, rProp)
        End Function
        Public Function PropValueSet(aIndex As [Enum], aValue As Object, aOccur As Integer, ByRef rFound As Boolean, ByRef rProp As TPROPERTY) As Boolean
            Try
                'get the property
                Dim pname As String = String.Empty
                rProp = PropertyGet(aIndex, aOccur, rFound, pname)
                If Not rFound Then
                    Throw New Exception($"{EntryTypeName()} Property Not Found - { pname}")
                End If
                'see if this is a change
                Dim aVal As Object = aValue
                Dim bVal As Boolean

                Dim iVal As Integer
                rProp.EnumName = aIndex  'attach the enum for the name
                'see if this is a change
                If rProp.PropType = dxxPropertyTypes.BitCode Then
                    Dim iSum As Integer = TVALUES.To_INT(rProp.Value) 'the current bit code sum
                    iVal = Math.Abs(TVALUES.To_INT(aIndex))  'the bit code sub value is the integer enum value of the passed index
                    If iVal > 100 Then iVal -= 100
                    bVal = TVALUES.ToBoolean(aValue) 'the request to to include or exclude the enum (index)   form the sum
                    bVal = TVALUES.BitCode_ToggleSubCode(iSum, iVal, bVal)  'add it or remove it from the sum (toggle it)
                    aVal = iSum 'the sum to set back to the property
                End If
                If rProp.PropType = dxxPropertyTypes.Switch Then
                    aVal = TPROPERTY.SwitchValue(aVal)
                    'If TVALUES.IsBoolean(aVal) Then
                    '    bVal = TVALUES.ToBoolean(aVal)
                    '    If bVal Then aVal = 1 Else aVal = 0
                    'Else
                    '    If TVALUES.IsNumber(aVal) Then
                    '        iVal = TVALUES.To_INT(aVal)
                    '        If iVal <> 0 And iVal <> 1 Then iVal = 1
                    '        aVal = iVal
                    '    Else
                    '        bVal = TVALUES.ToBoolean(aVal)
                    '        If bVal Then aVal = 1 Else aVal = 0
                    '    End If
                    'End If
                End If
                If EntryType = dxxReferenceTypes.LAYER Then
                    Select Case pname.ToUpper
                        Case "COLOR"

                            If TypeOf (aVal) Is Boolean Then
                                bVal = aVal
                                iVal = Math.Abs(TVALUES.ToInteger(rProp.Value))
                                If Not bVal Then iVal *= -1
                                aVal = iVal.ToString()
                            End If
                            If Not TVALUES.IsNumber(aVal) Then Return False
                        Case "*TRANSPARENCY"
                            If Not TVALUES.IsNumber(aVal) Then Return False
                            iVal = TVALUES.To_INT(aVal)
                            If iVal < 0 Then iVal = 0
                            If iVal > 90 Then iVal = 90
                            aVal = iVal
                    End Select
                ElseIf EntryType = dxxReferenceTypes.STYLE Then
                    Dim dsidx As dxxStyleProperties = aIndex
                    Dim isshape As Boolean
                    Dim verticl As Boolean

                    Dim xrefdep As Boolean
                    Dim xrefeslv As Boolean
                    Dim isrefed As Boolean
                    Dim codeval As Integer = 0
                    Dim bitcodegc As Integer = 0
                    Select Case dsidx
                        Case dxxStyleProperties.WIDTHFACTOR
                            If Not TVALUES.IsNumber(aVal) Then Return False
                            aVal = TVALUES.ToDouble(aVal, True, rProp.Value)
                            If aVal < 0.1 Then aVal = 0.1
                            If aVal > 10 Then aVal = 10
                        Case dxxStyleProperties.BACKWARDS
                            aVal = TPROPERTY.SwitchValue(aVal)
                            Dim upsideDown As Boolean = PropValueGet(dxxStyleProperties.UPSIDEDOWN) = 1
                            If aVal = 1 Then
                                codeval = 2
                            End If
                            If upsideDown Then codeval += 4
                            Props.SetValGC(71, codeval, 1)

                        Case dxxStyleProperties.UPSIDEDOWN
                            aVal = TPROPERTY.SwitchValue(aVal)
                            Dim backwards As Boolean = PropValueGet(dxxStyleProperties.BACKWARDS) = 1
                            If backwards Then codeval = 2
                            If aVal = 1 Then
                                codeval += 4
                            End If
                            Props.SetValGC(71, codeval, 1)
                        Case dxxStyleProperties.SHAPEFLAG
                            aVal = TPROPERTY.SwitchValue(aVal)
                            bitcodegc = 70
                            isshape = aVal = 1
                            verticl = PropValueGet(dxxStyleProperties.VERTICAL) = 1
                            xrefdep = PropValueGet(dxxStyleProperties.XREFDEPENANT) = 1
                            xrefeslv = PropValueGet(dxxStyleProperties.XREFRESOLVED) = 1
                            isrefed = PropValueGet(dxxStyleProperties.ISREFERENCED) = 1

                        Case dxxStyleProperties.VERTICAL
                            aVal = TPROPERTY.SwitchValue(aVal)
                            bitcodegc = 70
                            isshape = PropValueGet(dxxStyleProperties.SHAPEFLAG) = 1
                            verticl = aVal = 1
                            xrefdep = PropValueGet(dxxStyleProperties.XREFDEPENANT) = 1
                            xrefeslv = PropValueGet(dxxStyleProperties.XREFRESOLVED) = 1
                            isrefed = PropValueGet(dxxStyleProperties.ISREFERENCED) = 1
                        Case dxxStyleProperties.XREFDEPENANT
                            aVal = TPROPERTY.SwitchValue(aVal)
                            bitcodegc = 70
                            isshape = PropValueGet(dxxStyleProperties.VERTICAL) = 1
                            verticl = PropValueGet(dxxStyleProperties.SHAPEFLAG) = 1
                            xrefdep = aVal = 1
                            xrefeslv = PropValueGet(dxxStyleProperties.XREFRESOLVED) = 1
                            isrefed = PropValueGet(dxxStyleProperties.ISREFERENCED) = 1

                        Case dxxStyleProperties.XREFRESOLVED
                            aVal = TPROPERTY.SwitchValue(aVal)
                            bitcodegc = 70
                            isshape = PropValueGet(dxxStyleProperties.VERTICAL) = 1
                            verticl = PropValueGet(dxxStyleProperties.SHAPEFLAG) = 1
                            xrefdep = PropValueGet(dxxStyleProperties.XREFDEPENANT)
                            xrefeslv = aVal = 1
                            isrefed = PropValueGet(dxxStyleProperties.ISREFERENCED) = 1
                        Case dxxStyleProperties.ISREFERENCED
                            aVal = TPROPERTY.SwitchValue(aVal)
                            bitcodegc = 70
                            isshape = PropValueGet(dxxStyleProperties.VERTICAL) = 1
                            verticl = PropValueGet(dxxStyleProperties.SHAPEFLAG) = 1
                            xrefdep = PropValueGet(dxxStyleProperties.XREFDEPENANT)
                            xrefeslv = PropValueGet(dxxStyleProperties.XREFRESOLVED)
                            isrefed = aVal = 1

                    End Select

                    If bitcodegc = 70 Then
                        If isshape Then codeval += 1
                        If verticl Then codeval += 4
                        If xrefdep Then codeval += 16
                        If xrefdep And xrefeslv Then codeval += 32
                        If isrefed Then codeval += 64
                        Props.SetValGC(70, codeval, 1)

                    End If

                ElseIf EntryType = dxxReferenceTypes.DIMSTYLE Then
                    Dim dsidx As dxxDimStyleProperties = aIndex
                    Select Case dsidx
                        Case dxxDimStyleProperties.DIMBLK_NAME
                        Case dxxDimStyleProperties.DIMLTYPE_NAME, dxxDimStyleProperties.DIMLTEX1_NAME, dxxDimStyleProperties.DIMLTEX2_NAME
                            Dim sval As String = aVal.ToString().Trim()
                            If String.IsNullOrWhiteSpace(sval) Then sval = "ByBLock"
                            If String.Compare(sval, dxfLinetypes.ByBlock, True) = 0 Then sval = dxfLinetypes.ByBlock
                            If String.Compare(sval, dxfLinetypes.ByLayer, True) = 0 Then sval = dxfLinetypes.ByLayer
                            aVal = sval
                    End Select
                End If
                bVal = rProp.SetVal(aVal)
                If bVal Then
                    Props.UpdateProperty = rProp
                End If
                Return bVal
            Catch ex As Exception
                'add an error
                Throw ex
                Return False
            End Try
        End Function
        Public Sub UpdateDimPost()
            PropValueSet(dxxDimStyleProperties.DIMPOST, dxfUtils.DimPrefixSuffix(PropValueStr(dxxDimStyleProperties.DIMPREFIX), PropValueStr(dxxDimStyleProperties.DIMSUFFIX)))
            PropValueSet(dxxDimStyleProperties.DIMAPOST, dxfUtils.DimPrefixSuffix(PropValueStr(dxxDimStyleProperties.DIMAPREFIX), PropValueStr(dxxDimStyleProperties.DIMASUFFIX)))
        End Sub
#End Region 'Methods
#Region "Shared Methods"
        Public Shared ReadOnly Property Null
            Get
                Return New TTABLEENTRY(dxxReferenceTypes.UNDEFINED) With {.Index = -1}
            End Get
        End Property

        Public Shared Function ValidateHeaderPropertyChange(aImageGUID As String, aProp As TPROPERTY, aNewValue As Object, ByRef rError As String, Optional aImage As dxfImage = Nothing) As Boolean
            Dim _rVal As Boolean
            rError = String.Empty
            _rVal = True
            '^catchs and halts invalid header variable values
            Try
                Dim aName As String
                Dim bInvalid As Boolean
                Dim bHid As Boolean
                Dim enu As dxxHeaderVars
                Dim bImg As Boolean
                aName = aProp.Name.ToUpper.Trim
                If aName = "" Then Return _rVal
                If aImage Is Nothing Then
                    If aImageGUID <> "" Then aImage = dxfEvents.GetImage(aImageGUID)
                End If
                If aImage IsNot Nothing Then
                    bImg = True
                End If
                bHid = aName.StartsWith("*")
                If Not bHid Then
                    enu = aProp.Index - 1
                Else
                    enu = -aProp.Index
                End If
                Select Case enu
        '========================================================================================
                    Case dxxHeaderVars.LTSCALE, dxxHeaderVars.CELTSCALE
                        '========================================================================================
                        aNewValue = TVALUES.To_DBL(aNewValue)
                        bInvalid = aNewValue <= 0
                        If bInvalid Then rError = "LT Scale Values Must Be Greater Than 0"
        '========================================================================================
                    Case dxxHeaderVars.CELTYPE
                        '========================================================================================
                        aNewValue = aNewValue.ToString().Trim()
                        If aNewValue = "" Then aNewValue = dxfLinetypes.ByLayer
                        bInvalid = Not TLISTS.Contains(aNewValue, "ByLayer,ByBlock,Continuous")
                        If Not bInvalid And bImg Then
                            Dim lt As dxoLinetype = aImage.TableEntry(dxxReferenceTypes.LTYPE, aNewValue.ToString())
                            bInvalid = lt Is Nothing
                            If Not bInvalid Then aNewValue = lt.Name
                        End If
                        If bInvalid Then
                            rError = $"Unknown Linetype({ aNewValue }) Requested"
                        End If
        '========================================================================================
                    Case dxxHeaderVars.LWSCALE
                        '========================================================================================
                        aNewValue = TVALUES.ToDouble(aNewValue, True, aProp.LastValue, aMaxVal:=1, aValueControl:=mzValueControls.PositiveNonZero)
        '========================================================================================
                    Case dxxHeaderVars.DIMSTYLE
                        '========================================================================================
                        aNewValue = aNewValue.ToString().Trim()
                        If aNewValue = "" Then aNewValue = "Standard"
                        bInvalid = aNewValue.ToString().ToUpper() <> "STANDARD"
                        If Not bInvalid And bImg Then
                            Dim entry As dxoDimStyle = Nothing
                            bInvalid = Not aImage.DimStyles.TryGet(aNewValue, entry)
                            If Not bInvalid Then aNewValue = entry.Name
                        End If
                        If bInvalid Then
                            rError = $"Unknown Dim Style({ aNewValue }) Requested"
                        End If
        '========================================================================================
                    Case dxxHeaderVars.PLINEWID
                        '========================================================================================
                        aNewValue = TVALUES.To_DBL(aNewValue)
                        bInvalid = aNewValue < 0
                        If bInvalid Then rError = $"Invalid Polyline Width({ aNewValue }) Requested"
        '========================================================================================
                    Case dxxHeaderVars.TEXTSTYLE
                        '========================================================================================
                        aNewValue = aNewValue.ToString().Trim()
                        If aNewValue = "" Then aNewValue = "Standard"
                        bInvalid = aNewValue.ToString().ToUpper() <> "STANDARD"
                        If bInvalid And bImg Then
                            Dim entry As dxoStyle = Nothing
                            bInvalid = Not aImage.TextStyles.TryGet(aNewValue, entry)
                            If Not bInvalid Then aNewValue = entry.Name

                        End If
                        If bInvalid Then
                            rError = $"Unknown Text Style({ aNewValue }) Requested"
                        End If
        '========================================================================================
                    Case dxxHeaderVars.CLAYER
                        '========================================================================================
                        aNewValue = aNewValue.ToString().Trim()
                        If aNewValue = "" Then aNewValue = "0"
                        bInvalid = aNewValue <> "0"
                        If bInvalid And bImg Then
                            Dim entry As dxoLayer = Nothing
                            bInvalid = Not aImage.Layers.TryGet(aNewValue, entry)
                            If Not bInvalid Then aNewValue = entry.Name
                        End If
                        If bInvalid Then
                            rError = $"Unknown Layer({ aNewValue }) Requested"
                        End If
        '========================================================================================
                    Case dxxHeaderVars.PDMODE
                        '========================================================================================
                        If (aNewValue < 0 Or aNewValue > 4) Then
                            If aNewValue < 32 Or aNewValue > 36 Then
                                If aNewValue < 64 Or aNewValue > 68 Then
                                    If aNewValue < 96 Or aNewValue > 100 Then
                                        bInvalid = True
                                        rError = "Invaid PDMode Value"
                                    End If
                                End If
                            End If
                        End If
        '========================================================================================
                    Case dxxHeaderVars.DIMLTYPE, dxxHeaderVars.DIMLTEX1, dxxHeaderVars.DIMLTEX2
                        '========================================================================================
                        aNewValue = aNewValue.ToString().Trim()
                        If aNewValue = "" Then aNewValue = dxfLinetypes.ByBlock
                        bInvalid = Not TLISTS.Contains(aNewValue, "ByLayer,ByBlock,Continuous")
                        If Not bInvalid And bImg Then
                            Dim entry As dxoLinetype = Nothing
                            bInvalid = Not aImage.Linetypes.TryGet(aNewValue, entry)
                            If Not bInvalid Then aNewValue = entry.Name


                        End If
                        If bInvalid Then
                            rError = $"Unknown/Unloaded Linetype({ aNewValue }) Requested"
                        End If
                End Select
                If Not bInvalid Then
                    _rVal = True
                End If
            Catch ex As Exception
                Throw ex
            End Try
            Return _rVal
        End Function
#End Region 'Shared Methods
#Region "Operators"

#End Region 'Operators
    End Structure 'TTABLEENTRY
    Friend Structure TTABLE
        Implements ICloneable
#Region "Members"
        Public Props As TPROPERTIES
        Public Reactors As TPROPERTYARRAY
        Private _Handlez As THANDLES
        Private _TableType As dxxReferenceTypes
        Private _Members As TDICTIONARY_TTABLEENTRY
#End Region 'Members
#Region "Constructors"
        Public Sub New(aRefType As dxxReferenceTypes, Optional aName As String = "")
            'init --------------------------------------
            _TableType = dxxReferenceTypes.UNDEFINED
            Props = dxpProperties.Get_TableProps(_TableType)
            _Handlez = New THANDLES("")
            Name = ""
            Reactors = New TPROPERTYARRAY("Reactors")
            _Members = New TDICTIONARY_TTABLEENTRY(Name)
            'init --------------------------------------
            If aRefType <> dxxReferenceTypes.UNDEFINED Then
                _TableType = aRefType
                Props = dxpProperties.Get_TableProps(_TableType)
            End If

            If Not String.IsNullOrWhiteSpace(aName) Then Name = aName.Trim() Else Name = aRefType.ToString()
            If Name <> "" Then

                _Members = New TDICTIONARY_TTABLEENTRY(Name)
            End If
        End Sub
        Public Sub New(aRefType As dxxReferenceTypes, bAddDefaults As Boolean, Optional aGUID As String = Nothing)
            'init --------------------------------------
            _TableType = dxxReferenceTypes.UNDEFINED
            Props = dxpProperties.Get_TableProps(_TableType)
            _Handlez = New THANDLES("")
            Name = ""
            Reactors = New TPROPERTYARRAY("Reactors")
            _Members = New TDICTIONARY_TTABLEENTRY(Name)
            'init --------------------------------------
            If aRefType <> dxxReferenceTypes.UNDEFINED Then
                _TableType = aRefType
                Props = dxpProperties.Get_TableProps(_TableType)
                Name = aRefType.ToString()
            End If

            _Members = New TDICTIONARY_TTABLEENTRY(Name)
            Reactors = New TPROPERTYARRAY("Reactors")
            aGUID = dxfEvents.NextTableGUID(aRefType, aGUID)
            _Handlez = New THANDLES(aGUID)
            Props = dxpProperties.Get_TableProps(TableType)

            If bAddDefaults Then
                Select Case aRefType
                    Case dxxReferenceTypes.APPID
                        _Members.Add("ACAD", New TTABLEENTRY(aRefType, "ACAD"))
                    Case dxxReferenceTypes.VPORT
                        _Members.Add("*ACTIVE", New TTABLEENTRY(aRefType, "*Active"))
                    Case dxxReferenceTypes.LAYER
                        _Members.Add("0", New TTABLEENTRY(aRefType, "0"))
                    Case dxxReferenceTypes.STYLE
                        If bAddDefaults Then _Members.Add("STANDARD", New TTABLEENTRY(aRefType, "Standard"))
                    Case dxxReferenceTypes.DIMSTYLE
                        If bAddDefaults Then _Members.Add("STANDARD", New TTABLEENTRY(aRefType, "Standard"))
                    Case dxxReferenceTypes.LTYPE
                        _Members.Add("INVISIBLE", dxfLinetypes.GetCurrentDefinition(dxfLinetypes.Invisible))
                        _Members.Add("BYBLOCK", dxfLinetypes.GetCurrentDefinition(dxfLinetypes.ByBlock))
                        _Members.Add("BYLAYER", dxfLinetypes.GetCurrentDefinition(dxfLinetypes.ByLayer))
                        _Members.Add("CONTINUOUS", dxfLinetypes.GetCurrentDefinition(dxfLinetypes.Continuous))
                    Case Else
                        TableType = dxxReferenceTypes.UNDEFINED
                End Select
            End If
            MemberCount = Count
        End Sub

        Public Sub New(aTable As TTABLE, Optional bDontCloneEntries As Boolean = False, Optional bCloneHandles As Boolean = True, Optional aGUID As String = Nothing, Optional aImageGUID As String = Nothing)
            If String.IsNullOrWhiteSpace(aGUID) Then aGUID = aTable.GUID
            If String.IsNullOrWhiteSpace(aGUID) Then aGUID = dxfEvents.NextTableGUID(aTable.TableType, aGUID)
            If String.IsNullOrWhiteSpace(aImageGUID) Then aGUID = aTable.ImageGUID

            'init --------------------------------------
            _TableType = aTable.TableType
            Props = New TPROPERTIES(aTable.Props)
            _Handlez = New THANDLES(aGUID, aImageGUID)
            Name = aTable.Name
            Reactors = New TPROPERTYARRAY(aTable.Reactors)
            _Members = New TDICTIONARY_TTABLEENTRY(Name)

            'init --------------------------------------
            If bCloneHandles Then _Handlez = New THANDLES(aTable._Handlez)
            If Not bDontCloneEntries Then _Members = New TDICTIONARY_TTABLEENTRY(aTable._Members)


            MemberCount = Count
        End Sub

        Public Sub New(aTable As dxfTable, Optional bDontCloneEntries As Boolean = False, Optional bCloneHandles As Boolean = True, Optional aGUID As String = Nothing, Optional aImageGUID As String = Nothing)

            'init --------------------------------------
            _TableType = dxxReferenceTypes.UNDEFINED
            Props = dxpProperties.Get_TableProps(_TableType)
            _Handlez = New THANDLES("")
            Name = ""
            Reactors = New TPROPERTYARRAY("Reactors")
            _Members = New TDICTIONARY_TTABLEENTRY(Name)
            'init --------------------------------------
            If aTable Is Nothing Then Return
            If String.IsNullOrWhiteSpace(aGUID) Then aGUID = aTable.GUID
            If String.IsNullOrWhiteSpace(aGUID) Then aGUID = dxfEvents.NextTableGUID(aTable.TableType, aGUID)
            If String.IsNullOrWhiteSpace(aImageGUID) Then aGUID = aTable.ImageGUID

            'init --------------------------------------
            _TableType = aTable.TableType
            Props = New TPROPERTIES(aTable.Properties)
            _Handlez = New THANDLES(aTable._Handlez)
            Name = aTable.Name
            Reactors = New TPROPERTYARRAY(aTable.Reactors)
            _Members = New TDICTIONARY_TTABLEENTRY(Name)

            'init --------------------------------------
            If bCloneHandles Then _Handlez = New THANDLES(aTable._Handlez)
            If Not bDontCloneEntries Then
                _Members = New TDICTIONARY_TTABLEENTRY(Name)
                For Each entry As dxfTableEntry In aTable
                    Add(New TTABLEENTRY(entry))
                Next

            End If



            MemberCount = Count
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Property Handle As String
            Get
                Return _Handlez.Handle
            End Get
            Set(value As String)
                If value <> _Handlez.Handle Then
                    _Handlez.Handle = value
                    Props.Handle = value
                End If
            End Set
        End Property
        Public Property GUID As String
            Get
                Return _Handlez.GUID
            End Get
            Set(value As String)
                _Handlez.GUID = value
                Props.GUID = value
            End Set
        End Property
        Public Property Handlez As THANDLES
            Get
                Return _Handlez
            End Get
            Set(value As THANDLES)
                _Handlez = New THANDLES(value, GUID)
            End Set
        End Property
        Public Property MemberCount As Integer
            Get
                Dim aProp As TPROPERTY = TPROPERTY.Null
                If Props.TryGet(70, aProp) Then Return TVALUES.To_INT(aProp.Value)
                If TableType = dxxReferenceTypes.DIMSTYLE Then
                    aProp = Props.GetByGC(71, aOccurance:=1)
                    If aProp.Index > 0 Then Return TVALUES.To_INT(aProp.Value)
                End If
                aProp = Props.GetByGC(70, aOccurance:=1)
                If aProp.Index > 0 Then Return TVALUES.To_INT(aProp.Value) Else Return 0
            End Get
            Set(value As Integer)
                Dim aProp As TPROPERTY = TPROPERTY.Null
                If Props.TryGet(70, aProp) Then
                    Props.SetVal(aProp.Index, value)
                End If
                If TableType = dxxReferenceTypes.DIMSTYLE Then
                    aProp = Props.GetByGC(71, aOccurance:=1)
                    If aProp.Index > 0 Then Props.SetVal(aProp.Index, value)
                End If
                aProp = Props.GetByGC(70, aOccurance:=1)
                If aProp.Index > 0 Then Props.SetVal(aProp.Index, value)
            End Set
        End Property
        Public Property Name As String
            Get
                Return Props.GCValueStr(2)
            End Get
            Set(value As String)
                Props.SetValGC(2, value, 1)
            End Set
        End Property
        Public Property ImageGUID As String
            Get
                Return _Handlez.ImageGUID
            End Get
            Set(value As String)
                If value <> _Handlez.ImageGUID Then
                    _Handlez.ImageGUID = value
                    UpdateGUIDS()
                End If
            End Set
        End Property
        Public ReadOnly Property Count As Integer
            Get

                Return _Members.Count
            End Get
        End Property
        Public Property TableType As dxxReferenceTypes
            Get
                Return _TableType
            End Get
            Set(value As dxxReferenceTypes)
                If value = _TableType And Not Props.IsEmpty Then Return
                _TableType = value
                Props = dxpProperties.Get_TableProps(_TableType)
            End Set
        End Property
        Public ReadOnly Property TableTypeName As String
            Get
                Return TableType.ToString
            End Get
        End Property
        Public ReadOnly Property SubClassName As String
            Get
                Return TTABLE.GetSubClassName(TableType)
            End Get
        End Property
        Public WriteOnly Property UpdateEntry As TTABLEENTRY
            Set(value As TTABLEENTRY)

                If String.IsNullOrWhiteSpace(value.Name) Then Return
                SetEntry(value.Name, value, True)
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function Contains(aName As String) As Boolean
            Return _Members.ContainsKey(aName.ToUpper)
        End Function
        Public Function Contains(aName As String, ByRef rIndex As Integer) As Boolean
            Return _Members.ContainsKey(aName.ToUpper, rIndex)
        End Function
        Public Function Entry(aNameOrHandle As String, Optional bSearchByHandleIfNotFound As Boolean = False) As TTABLEENTRY
            aNameOrHandle = Trim(aNameOrHandle)
            Dim _rVal As New TTABLEENTRY(dxxReferenceTypes.UNDEFINED, "") With {.Index = -1}
            If String.IsNullOrEmpty(aNameOrHandle) Or Count <= 0 Then Return _rVal
            If _Members.TryGetValue(aNameOrHandle.ToUpper(), _rVal) Then
                _rVal.ImageGUID = ImageGUID
                If TableType <> dxxReferenceTypes.UNDEFINED Then _rVal.EntryType = TableType
                _rVal.ImageGUID = ImageGUID
                Return _rVal
            Else
                If bSearchByHandleIfNotFound Then
                    Return GetByHandle(aNameOrHandle)
                Else
                    Return New TTABLEENTRY(dxxReferenceTypes.UNDEFINED, "") With {.Index = -1}
                End If
            End If
        End Function
        Public Sub SetEntry(aNameOrHandle As String, value As TTABLEENTRY, Optional bSearchByHandleIfNotFound As Boolean = False)
            aNameOrHandle = Trim(aNameOrHandle).ToUpper
            If aNameOrHandle = "" Or Count <= 0 Then Return
            'If TableType = dxxReferenceTypes.DIMSTYLE Then
            '    If aNameOrHandle.ToUpper = "STANDARD" Then
            '        Beep()
            '    End If
            'End If
            Dim aMem As TTABLEENTRY = TTABLEENTRY.Null
            If _Members.TryGetValue(aNameOrHandle, aMem) Then
                value.ImageGUID = ImageGUID
                If TableType <> dxxReferenceTypes.UNDEFINED Then
                    value.EntryType = TableType
                End If
                _Members.SetItem(aNameOrHandle, value)
            Else
                If Not bSearchByHandleIfNotFound Then Return
                aMem = GetByHandle(aNameOrHandle)
                If aMem.Index < 0 Then Return
                value.ImageGUID = ImageGUID
                If TableType <> dxxReferenceTypes.UNDEFINED Then value.EntryType = TableType
                _Members.SetItem(aMem.Name.ToUpper, value)
            End If
        End Sub
        Public Function Names(Optional aSkipList As String = "") As TVALUES
            Dim _rVal As New TVALUES(0)
            '^returns a comma delimited string containing the names of the members

            For i As Integer = 1 To _Members.Count
                Dim aMem As TTABLEENTRY = _Members.Item(i)
                If Not TLISTS.Contains(aMem.Name, aSkipList, bReturnTrueForNullList:=True) Then _rVal.Add(aMem.Name)
            Next
            Return _rVal
        End Function
        Public Function Item(aIndex As Integer) As TTABLEENTRY
            'BASE 1
            Dim aMem As New TTABLEENTRY(dxxReferenceTypes.UNDEFINED, "") With {.Index = -1}
            If aIndex < 1 Or aIndex > Count Then Return aMem
            aMem = _Members.Item(aIndex)
            aMem.ImageGUID = ImageGUID
            If TableType <> dxxReferenceTypes.UNDEFINED Then aMem.EntryType = TableType
            aMem.EntryType = TableType
            aMem.Index = aIndex - 1
            aMem.ImageGUID = ImageGUID
            _Members.SetItem(aIndex, aMem)
            Return aMem
        End Function
        Public Sub SetItem(aIndex As Integer, value As TTABLEENTRY)
            If aIndex < 1 Or aIndex > Count Then Return
            Dim mname As String = UCase(value.Name)
            If mname = "" Then Return
            Dim aMem As TTABLEENTRY = _Members.Item(aIndex)
            If TableType <> dxxReferenceTypes.UNDEFINED Then
                If value.EntryType <> TableType Then
                    Throw New Exception($"Entries Of Type { value.EntryType} Cannot be Members Of Table Type { TableType}")
                End If
                value.EntryType = TableType
            End If
            value.ImageGUID = ImageGUID
            value.Name = aMem.Name
            If TableType <> dxxReferenceTypes.UNDEFINED Then value.EntryType = TableType
            _Members.SetItem(aIndex, value)
        End Sub
        Public Function MemberHandle(aName As String) As String
            Dim aEntry As TTABLEENTRY = Entry(aName)
            If aEntry.Index >= 0 Then Return aEntry.Handle Else Return String.Empty
        End Function
        Public Sub SetMemberHandle(aName As String, value As String)
            Dim aEntry As TTABLEENTRY = Entry(aName)
            If aEntry.Index >= 0 Then
                aEntry.Handle = value
                SetEntry(aName, aEntry)
            End If
        End Sub
        Public Function TryGet(aName As String, Optional aDefaultName As String = "", Optional bAddDefaultIfNotFound As Boolean = False) As Boolean
            Dim rEntry As TTABLEENTRY = TTABLEENTRY.Null
            Return TryGet(aName, aDefaultName, bAddDefaultIfNotFound, rEntry)
        End Function
        Public Function TryGet(aName As String, ByRef rEntry As TTABLEENTRY) As Boolean
            Return TryGet(aName, "", False, rEntry)
        End Function
        Public Function TryGet(aName As String, aDefaultName As String, bAddDefaultIfNotFound As Boolean, ByRef rEntry As TTABLEENTRY) As Boolean
            Dim _rVal As Boolean
            rEntry = New TTABLEENTRY With {.Index = -1}
            aName = Trim(aName)
            If Count > 0 And aName <> "" Then _rVal = _Members.TryGetValue(UCase(aName), rEntry) Else Return False
            If Not _rVal Then
                rEntry.EntryType = TableType
                rEntry.Index = -1
                If aDefaultName <> "" Then
                    aName = Trim(aDefaultName)
                    _rVal = _Members.TryGetValue(aName.ToUpper(), rEntry)
                    If Not _rVal Then
                        rEntry.EntryType = TableType
                        rEntry.Index = -1
                    End If
                    If Not _rVal And bAddDefaultIfNotFound Then
                        rEntry = New TTABLEENTRY(TableType, aDefaultName)
                        Add(rEntry)
                        _rVal = True
                        rEntry = Entry(aName)
                    End If
                End If
            End If
            If _rVal Then
                aName = rEntry.Name
            Else
                rEntry = New TTABLEENTRY With {.Index = -1}
            End If
            Return _rVal
        End Function
        Public Overrides Function ToString() As String
            Return $"{TableType} [{ Count} ]"
        End Function
        Friend Function GetByList(aList As String) As TTABLE
            Dim _rVal As TTABLE = Clone(True, True)
            aList = Trim(aList)
            If aList = "" Then Return _rVal
            Dim i As Integer
            Dim lVals() As String
            Dim lVal As String
            Dim aEntry As TTABLEENTRY
            lVals = aList.Split(",")
            For i = 0 To lVals.Length - 1
                lVal = Trim(lVals(i))
                If lVal <> "" Then
                    aEntry = Entry(lVal)
                    If aEntry.Index >= 0 Then _rVal.Add(aEntry)
                End If
            Next i
            Return _rVal
        End Function
        Public Sub UpdateGUIDS()
            Dim aMem As TTABLEENTRY
            For i = 1 To Count
                aMem = _Members.Item(i)
                aMem.ImageGUID = ImageGUID
                _Members.SetItem(i, aMem)
            Next
        End Sub
        Public Function GetNames(Optional bReturnUpperCase As Boolean = False) As List(Of String)
            Dim _rVal As New List(Of String)
            For i As Integer = 1 To Count
                If Not bReturnUpperCase Then _rVal.Add(_Members.Item(i).Name) Else _rVal.Add(_Members.Item(i).Name.ToUpper)
            Next
            Return _rVal
        End Function
        Public Function Clone(Optional bDontCloneEntries As Boolean = False, Optional bCloneHandles As Boolean = True, Optional aGUID As String = Nothing) As TTABLE

            Return New TTABLE(Me, bDontCloneEntries, bCloneHandles, aGUID)

        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TTABLE(Me)
        End Function
        Public Sub ReferenceADD(aEntryName As String, aReference As String, Optional aGC As Integer = 330)
            Dim aMem As TTABLEENTRY = TTABLEENTRY.Null
            If TryGet(aEntryName, "", False, aMem) Then
                aMem.ReferenceADD(aReference, aGC)
                UpdateEntry = aMem
            End If
        End Sub
        Public Function GetByHandle(aHandle As String) As TTABLEENTRY
            Dim rIndex As Integer = -1
            Return GetByHandle(aHandle, rIndex)
        End Function
        Public Function GetByHandle(aHandle As String, ByRef rIndex As Integer) As TTABLEENTRY
            'base 0
            Dim shnd As String = Trim(aHandle.ToString)
            rIndex = -1
            If Count <= 0 Then Return New TTABLEENTRY With {.Index = -1}
            Dim i As Integer
            Dim aEntry As TTABLEENTRY
            For i = 1 To Count
                aEntry = Item(i)
                If String.Compare(aEntry.Handle, shnd, True) = 0 Then
                    rIndex = i - 1
                    Return aEntry
                End If
            Next i
            Return New TTABLEENTRY With {.Index = -1}
        End Function
        Public Sub ReferenceREMOVE(aEntry As String, aReference As String)
            If Contains(aEntry) Then
                Dim aMem As TTABLEENTRY = Entry(aEntry)
                aMem.ReferenceREMOVE(aReference)
                UpdateEntry = aMem
            End If
        End Sub

        Public Sub Clear()
            _Members = New TDICTIONARY_TTABLEENTRY("")
        End Sub

        Public Function Add(aEntry As TTABLEENTRY, Optional aBeforeIndex As Integer = 0, Optional aAfterIndex As Integer = 0) As Boolean
            If aEntry.Props.Count <= 0 Then Return False


            If aEntry.EntryType <> TableType Then
                If dxfUtils.RunningInIDE Then MessageBox.Show($"Invalid Entry '{ aEntry.EntryTypeName }' Add Attempt")
                Return False
            End If
            Dim aName As String
            aName = aEntry.Name
            If String.IsNullOrEmpty(aName) Then Return False
            If aAfterIndex > 0 And aBeforeIndex > 0 Then aAfterIndex = 0
            If aAfterIndex > 0 And aAfterIndex >= Count Then aAfterIndex = 0
            If aAfterIndex > 0 Then aBeforeIndex = aAfterIndex + 1
            If aBeforeIndex > Count Then aBeforeIndex = 0
            Dim idx As Integer = 0
            Dim rAdded As Boolean
            If Not _Members.ContainsKey(aName.ToUpper, idx) Then
                If aBeforeIndex <= 0 Or Count = 0 Then
                    aEntry.Index = _Members.Count - 1
                    If _Members.Add(aName.ToUpper, aEntry) Then rAdded = True
                Else
                    Dim newMems As New TDICTIONARY_TTABLEENTRY(Name)
                    Dim aMem As TTABLEENTRY
                    For i As Integer = 1 To _Members.Count
                        aMem = _Members.Item(i)
                        If i = aBeforeIndex Then
                            aEntry.Index = i - 1
                            newMems.Add(aName.ToUpper, aEntry)
                        End If
                        aMem.Index = newMems.Count - 1
                        If newMems.Add(aMem.Name.ToUpper, aMem) Then rAdded = True
                    Next
                    _Members = newMems
                End If
                Return True
            Else
                'replace existing
                Dim bEntry As TTABLEENTRY = Item(idx)
                aEntry.GUID = bEntry.GUID
                If TableType = dxxReferenceTypes.DIMSTYLE Then
                    aEntry.Props.SetValueGC(105, bEntry.Props.GCValueStr(105))
                Else
                    aEntry.Props.SetValueGC(5, bEntry.Props.GCValueStr(5))
                End If
                aEntry.Index = _Members.Count
                _Members.SetItem(_Members.Count, aEntry)
                Return False
            End If
        End Function
        Public Sub Remove(aIndex As Integer)
            'base 1
            _Members.Remove(aIndex)
        End Sub
        Public Sub Remove(aName As String)
            aName = Trim(aName)
            If aName = "" Or Count <= 0 Then Return
            If Contains(aName) Then _Members.Remove(aName.ToUpper)
        End Sub
        Public Sub UpdateBitCodes()
            Dim aEntry As TTABLEENTRY
            Dim j As Integer
            For j = 1 To Count
                aEntry = _Members.Item(j)
                Select Case TableType
            '=================================================================
                    Case dxxReferenceTypes.VPORT
            '=================================================================
            '=================================================================
                    Case dxxReferenceTypes.DIMSTYLE
            '=================================================================
            '=================================================================
                    Case dxxReferenceTypes.STYLE
            '=================================================================
            '=================================================================
                    Case dxxReferenceTypes.LAYER
            '=================================================================
            'update the bitcodes
            '=================================================================
                    Case dxxReferenceTypes.LTYPE
                        '=================================================================
                End Select
                _Members.SetItem(j, aEntry)
            Next j
        End Sub
        Public Sub ClearEntryReactors(aReactorsName As String, Optional bAddIfNotFound As Boolean = False)
            aReactorsName = Trim(aReactorsName)
            If Count <= 0 Or aReactorsName = "" Then Return
            Dim aMem As TTABLEENTRY
            For i As Integer = 1 To _Members.Count
                aMem = _Members.Item(i)
                aMem.Reactors.ClearMember(aReactorsName, bAddIfNotFound)
                _Members.SetItem(i, aMem)
            Next
        End Sub
        Public Function GetByPropertyValue(aGC As Integer, aValue As Object, Optional aStringCompare As Boolean = False, Optional aSecondaryValue As Object = Nothing, Optional aOccur As Integer = 0) As TTABLEENTRY
            Dim rIndex As Integer = -1
            Return GetByPropertyValue(aGC, aValue, aStringCompare, aOccur, rIndex, aSecondaryValue)
        End Function
        Public Function GetByPropertyValue(aGC As Integer, aValue As Object, aStringCompare As Boolean, ByRef rIndex As Integer, Optional aOccur As Integer = 0, Optional aSecondaryValue As Object = Nothing) As TTABLEENTRY
            'base 0
            Dim _rVal As TTABLEENTRY = TTABLEENTRY.Null
            rIndex = -1
            Dim i As Integer
            Dim aMem As TTABLEENTRY
            Dim aProp As TPROPERTY = TPROPERTY.Null
            Dim bTest2 As Boolean = aSecondaryValue IsNot Nothing

            For i = 1 To Count
                aMem = Item(i)
                If aMem.Props.TryGet(aGC, aProp, aOccur) Then
                    If Not aStringCompare Then
                        If aProp.Value = aValue Then
                            rIndex = i - 1
                            _rVal = aMem
                            Exit For
                        End If
                        If bTest2 Then
                            If aProp.Value = aSecondaryValue Then
                                rIndex = i - 1
                                _rVal = aMem
                                Exit For
                            End If
                        End If
                    Else
                        If String.Compare(aProp.Value, aValue, True) = 0 Then
                            rIndex = i - 1
                            _rVal = aMem
                            Exit For
                        End If
                        If bTest2 Then
                            If String.Compare(aProp.Value, aSecondaryValue, True) = 0 Then
                                rIndex = i - 1
                                _rVal = aMem
                                Exit For
                            End If
                        End If
                    End If
                End If
            Next i
            Return _rVal
        End Function
#End Region 'Methods
#Region "Shared Methods"

        Public Shared ReadOnly Property Null
            Get
                Return New TTABLE(aRefType:=dxxReferenceTypes.UNDEFINED)
            End Get
        End Property

        Public Shared Function GetSubClassName(aTableType As dxxReferenceTypes) As String
            Select Case aTableType
        '======================================
                Case dxxReferenceTypes.APPID
                    '======================================
                    Return "AcDbRegAppTableRecord"
        '======================================
                Case dxxReferenceTypes.VPORT
                    '======================================
                    Return "AcDbViewportTableRecord"
        '======================================
                Case dxxReferenceTypes.BLOCK_RECORD
                    '======================================
                    Return "AcDbBlockTableRecord"
        '======================================
                Case dxxReferenceTypes.LTYPE
                    '======================================
                    Return "AcDbLinetypeTableRecord"
        '======================================
                Case dxxReferenceTypes.LAYER
                    '======================================
                    Return "AcDbLayerTableRecord"
        '======================================
                Case dxxReferenceTypes.STYLE
                    '======================================
                    Return "AcDbTextStyleTableRecord"
        '======================================
                Case dxxReferenceTypes.DIMSTYLE
                    '======================================
                    Return "AcDbDimStyleTableRecord"
        '======================================
                Case dxxReferenceTypes.UCS
                    '======================================
                    Return "AcDbUCSTableRecord"
        '======================================
                Case dxxReferenceTypes.VIEW
                    '======================================
                    Return "AcDbViewTableRecord"
                Case Else
                    Return String.Empty
            End Select
        End Function
        Public Shared Function SortByName(aTable As TTABLE) As TTABLE
            Dim _rVal As TTABLE = aTable.Clone(True, True)
            If aTable.Count <= 0 Then Return _rVal
            Dim i As Integer
            Dim aVals As New TVALUES(0)
            For i = 1 To aTable.Count
                aVals.Add(aTable.Item(i).Name)
            Next i
            aVals.Sort(False, False)
            For i = 1 To aVals.Count
                _rVal.Add(aTable.Entry(aVals.StringVal(i)).Clone(True))
            Next i
            Return _rVal
        End Function
#End Region 'Shared Methods
#Region "Operators"

#End Region 'OPerators
    End Structure 'TTABLE
    Friend Structure TTABLES
        Implements ICloneable
#Region "Members"
        Private _Init As Boolean
        Private _Members() As TTABLE
        Private _Names As String
        Public Tag As String
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aTag As String = "")
            'init -----------------------
            _Init = True
            ReDim _Members(-1)
            _Names = ""
            Tag = ""
            'init -----------------------
            Tag = aTag
        End Sub
        Public Sub New(aTables As TTABLES)
            'init -----------------------
            _Init = True
            ReDim _Members(-1)
            _Names = aTables.Names
            Tag = aTables.Tag
            'init -----------------------
            If aTables._Init Then _Members = aTables._Members.Clone()
        End Sub
#End Region 'Constructors

#Region "Properties"
        Public ReadOnly Property Names As String
            Get
                Return _Names
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
#End Region 'Properties
#Region "Methods"
#End Region 'Methods

        Public Function Clone() As TTABLES
            Return New TTABLES(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TTABLES(Me)
        End Function

        Public Function Item(aIndex As Integer) As TTABLE
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return New TTABLE(dxxReferenceTypes.UNDEFINED)
            End If
            Return _Members(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, value As TTABLE)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1) = value
        End Sub
        Public Sub Add(aTable As TTABLE)
            If aTable.Props.Count <= 0 Then
                aTable.Clear()
                Return
            End If
            Dim tname As String = aTable.Props.GCValueStr(2)
            Dim idx As Integer

            aTable.TableType = dxfEnums.ReferenceTypeByName(tname)
            If aTable.TableType = dxxReferenceTypes.UNDEFINED Then Return
            GetByType(aTable.TableType, idx)
            If idx <= 0 Then
                System.Array.Resize(_Members, Count + 1)
                _Members(_Members.Count - 1) = aTable
            Else
                _Members(idx - 1) = aTable
            End If
            TLISTS.Add(_Names, tname)
        End Sub
        Public Function GetByType(aType As dxxReferenceTypes) As TTABLE
            Dim rIndex As Integer = 0
            Return GetByType(aType, rIndex)
        End Function
        Public Function GetByType(aType As dxxReferenceTypes, ByRef rIndex As Integer) As TTABLE
            rIndex = -0

            For i As Integer = 1 To Count
                If _Members(i - 1).TableType = aType Then
                    rIndex = i
                    Return _Members(i - 1)
                End If
            Next i
            Return New TTABLE
        End Function
#Region "Shared Methods"
        Public Shared Function LineWeights(Optional bSuppressLogicals As Boolean = False, Optional bSuppressDefault As Boolean = False) As TTABLE
            Dim _rVal As TTABLE
            Dim deflw As TTABLE = DefaultLineWeights_Get()

            If deflw.Count <= 0 Then
                deflw = New TTABLE(dxxReferenceTypes.UNDEFINED, "LINEWEIGHTS")

                Dim aLWt As New TTABLEENTRY(dxxReferenceTypes.UNDEFINED, "")
                Dim bLwt As TTABLEENTRY
                aLWt.Props = New TPROPERTIES()
                aLWt.Props.Add(New TPROPERTY(0, "LINEWEIGHT", "Object Type Name", dxxPropertyTypes.dxf_String))
                aLWt.Props.Add(New TPROPERTY(2, "", "Name", dxxPropertyTypes.dxf_String))
                aLWt.Props.Add(New TPROPERTY(70, 0, "Width", dxxPropertyTypes.dxf_Double))
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(1, "Default")
                bLwt.Props.SetVal(2, -3)
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(1, dxfLinetypes.ByBlock)
                bLwt.Props.SetVal(2, -2)
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(1, dxfLinetypes.ByLayer)
                bLwt.Props.SetVal(2, -1)
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(2, 0)
                bLwt.Props.SetVal(1, $"{(bLwt.Props.Value(2) / 100):0.00} mm")
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(2, 5)
                bLwt.Props.SetVal(1, $"{bLwt.Props.Value(2) / 100:0.00} mm")
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(2, 9)
                bLwt.Props.SetVal(1, $"{bLwt.Props.Value(2) / 100:0.00} mm")
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(2, 13)
                bLwt.Props.SetVal(1, $"{bLwt.Props.Value(2) / 100:0.00} mm")
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(2, 15)
                bLwt.Props.SetVal(1, $"{bLwt.Props.Value(2) / 100:0.00}  mm")
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(2, 18)
                bLwt.Props.SetVal(1, $"{bLwt.Props.Value(2) / 100:0.00}  mm")
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(2, 20)
                bLwt.Props.SetVal(1, $"{bLwt.Props.Value(2) / 100:0.00}  mm")
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(2, 25)
                bLwt.Props.SetVal(1, $"{bLwt.Props.Value(2) / 100:0.00}  mm")
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(2, 30)
                bLwt.Props.SetVal(1, $"{bLwt.Props.Value(2) / 100:0.00}  mm")
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(2, 35)
                bLwt.Props.SetVal(1, $"{bLwt.Props.Value(2) / 100:0.00}  mm")
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(2, 40)
                bLwt.Props.SetVal(1, $"{bLwt.Props.Value(2) / 100:0.00}  mm")
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(2, 50)
                bLwt.Props.SetVal(1, $"{bLwt.Props.Value(2) / 100:0.00}  mm")
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(2, 60)
                bLwt.Props.SetVal(1, $"{bLwt.Props.Value(2) / 100:0.00}  mm")
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(2, 70)
                bLwt.Props.SetVal(1, $"{bLwt.Props.Value(2) / 100:0.00}  mm")
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(2, 80)
                bLwt.Props.SetVal(1, $"{bLwt.Props.Value(2) / 100:0.00}  mm")
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(2, 90)
                bLwt.Props.SetVal(1, $"{bLwt.Props.Value(2) / 100:0.00}  mm")
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(2, 100)
                bLwt.Props.SetVal(1, $"{bLwt.Props.Value(2) / 100:0.00}  mm")
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(2, 106)
                bLwt.Props.SetVal(1, $"{bLwt.Props.Value(2) / 100:0.00}  mm")
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(2, 120)
                bLwt.Props.SetVal(1, $"{bLwt.Props.Value(2) / 100:0.00}  mm")
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(2, 140)
                bLwt.Props.SetVal(1, $"{bLwt.Props.Value(2) / 100:0.00}  mm")
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(2, 158)
                bLwt.Props.SetVal(1, $"{bLwt.Props.Value(2) / 100:0.00}  mm")
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(2, 200)
                bLwt.Props.SetVal(1, $"{bLwt.Props.Value(2) / 100:0.00}  mm")
                deflw.Add(bLwt)
                bLwt = New TTABLEENTRY(aLWt)
                bLwt.Props.SetVal(2, 211)
                bLwt.Props.SetVal(1, $"{bLwt.Props.Value(2) / 100:0.00}  mm")
                deflw.Add(bLwt)
                DefaultLineWeights_Set(deflw)
            End If
            _rVal = deflw.Clone
            If bSuppressLogicals Then
                _rVal.Remove(dxfLinetypes.ByLayer)
                _rVal.Remove(dxfLinetypes.ByBlock)
            End If
            If bSuppressDefault Then
                _rVal.Remove("Default")
            End If
            Return _rVal
        End Function

#End Region 'Shared MEthods
    End Structure 'TTABLES
    Friend Structure TSUBCOMMAND
        Implements ICloneable
#Region "Members"
        Public Flag As Boolean
        Private _Value1 As Double
        Private _Value2 As Double
        Private _Value3 As Double
        Private _Value4 As Double
        Private _Value5 As Double
        Private _Value6 As Double
        Public CommandType As dxxShapeCommands
        Private _b1 As Boolean
        Private _b2 As Boolean
        Private _b3 As Boolean
        Private _b4 As Boolean
        Private _b5 As Boolean
        Private _b6 As Boolean
#End Region 'Members
#Region "Constructors"
        Public Sub New(aCommandType As dxxShapeCommands)
            'init --------------------------------------------
            Flag = False
            _Value1 = 0
            _Value2 = 0
            _Value3 = 0
            _Value4 = 0
            _Value5 = 0
            _Value6 = 0
            CommandType = dxxShapeCommands.Undefined
            _b1 = False
            _b2 = False
            _b3 = False
            _b4 = False
            _b5 = False
            _b6 = False
            'init --------------------------------------------
            CommandType = aCommandType
        End Sub
        Public Sub New(aCommand As TSUBCOMMAND)
            'init --------------------------------------------
            Flag = aCommand.Flag
            _Value1 = aCommand._Value1
            _Value2 = aCommand._Value2
            _Value3 = aCommand._Value3
            _Value4 = aCommand._Value4
            _Value5 = aCommand._Value5
            _Value6 = aCommand._Value6
            CommandType = aCommand.CommandType
            _b1 = aCommand._b1
            _b2 = aCommand._b2
            _b3 = aCommand._b3
            _b4 = aCommand._b4
            _b5 = aCommand._b5
            _b6 = aCommand._b6
            'init --------------------------------------------

        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Function Clone() As TSUBCOMMAND
            Return New TSUBCOMMAND(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TSUBCOMMAND(Me)
        End Function

        Public Property Value1 As Double
            Get
                Return _Value1
            End Get
            Set(value As Double)
                _b1 = True
                If _Value1 <> value Then
                    _Value1 = value
                    'System.Diagnostics.Debug.WriteLine("VALUE1=" & value.ToString)
                End If
            End Set
        End Property
        Public Property Value2 As Double
            Get
                Return _Value2
            End Get
            Set(value As Double)
                _b2 = True
                If _Value2 <> value Then
                    _Value2 = value
                    'System.Diagnostics.Debug.WriteLine("VALUE2=" & value.ToString)
                End If
            End Set
        End Property
        Public Property Value3 As Double
            Get
                Return _Value3
            End Get
            Set(value As Double)
                _b3 = True
                If _Value3 <> value Then
                    _Value3 = value
                    'System.Diagnostics.Debug.WriteLine("VALUE3=" & value.ToString)
                End If
            End Set
        End Property
        Public Property Value4 As Double
            Get
                Return _Value4
            End Get
            Set(value As Double)
                _b4 = True
                If _Value4 <> value Then
                    _Value4 = value
                    'System.Diagnostics.Debug.WriteLine("VALUE4=" & value.ToString)
                End If
            End Set
        End Property
        Public Property Value5 As Double
            Get
                Return _Value5
            End Get
            Set(value As Double)
                _b5 = True
                If _Value5 <> value Then
                    _Value5 = value
                    'System.Diagnostics.Debug.WriteLine("VALUE5=" & value.ToString)
                End If
            End Set
        End Property
        Public Property Value6 As Double
            Get
                Return _Value6
            End Get
            Set(value As Double)
                _b6 = True
                If _Value6 <> value Then
                    _Value6 = value
                    'System.Diagnostics.Debug.WriteLine("VALUE6=" & value.ToString)
                End If
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Overrides Function ToString() As String
            Dim _rVal As String = Replace(CommandType.ToString, "", "")
            If _b1 Then _rVal += $" V1:{ Value1:0.0)}"
            If _b2 Then _rVal += $" V2:{ Value2:0.0)}"
            If _b3 Then _rVal += $" V3:{ Value3:0.0)}"
            If _b4 Then _rVal += $" V4:{ Value4:0.0)}"
            If _b5 Then _rVal += $" V5:{ Value5:0.0)}"
            If _b6 Then _rVal += $" V6:{ Value6:0.0)}"
            Return _rVal
        End Function
#End Region 'Methods
    End Structure 'TSUBCOMMAND
    Friend Structure TSUBCOMMANDS
        Implements ICloneable
#Region "Members"
        Private _Init As Boolean
        Private _Members() As TSUBCOMMAND
#End Region 'Members
#Region "Constructors"
        Public Sub New(aCount As Integer)
            'init --------------------------------------
            _Init = True
            ReDim _Members(-1)
            'init --------------------------------------
            For i As Integer = 1 To aCount
                Add(New TSUBCOMMAND(dxxShapeCommands.PenUp))
            Next

        End Sub
        Public Sub New(aCommands As TSUBCOMMANDS)
            'init --------------------------------------
            _Init = True
            ReDim _Members(-1)
            'init --------------------------------------
            If aCommands._Init Then _Members = aCommands._Members.Clone()


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
#End Region 'Methods
        Public Function Item(aIndex As Integer, Optional bSuppressIndexErr As Boolean = False) As TSUBCOMMAND
            If aIndex < 1 Or aIndex > Count Then
                If bSuppressIndexErr Then Return New TSUBCOMMAND(dxxShapeCommands.Undefined)
                Throw New IndexOutOfRangeException()
            End If
            Return _Members(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, value As TSUBCOMMAND)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1) = value
        End Sub
        Public Sub Clear()
            _Init = True
            ReDim _Members(-1)

        End Sub
        Public Sub Add(aMember As TSUBCOMMAND)
            System.Array.Resize(_Members, Count + 1)
            _Members(_Members.Count - 1) = aMember
        End Sub
        Public Function Clone() As TSUBCOMMANDS

            Return New TSUBCOMMANDS(Me)
        End Function


        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TSUBCOMMANDS(Me)
        End Function

        Public Function ToList() As List(Of TSUBCOMMAND)
            If Count <= 0 Then Return New List(Of TSUBCOMMAND)
            Return _Members.ToList()
        End Function
    End Structure 'TSUBCOMMANDS
    Friend Structure TSHAPECOMMAND
        Implements ICloneable
#Region "Members"
        Public ByteIndex As Integer
        Public PathString As String
        Public Type As dxxShapeCommands
        Public Value As Object
        Public SubCommands As TSUBCOMMANDS
#End Region 'Members
#Region "Constructors"
        Public Sub New(aType As dxxShapeCommands)
            'init --------------------------------------------------
            ByteIndex = 0
            PathString = ""
            Type = dxxShapeCommands.XYDisplacement
            Value = Nothing
            SubCommands = New TSUBCOMMANDS(0)
            'init --------------------------------------------------

            Type = aType

        End Sub
        Public Sub New(aCommand As TSHAPECOMMAND)
            'init --------------------------------------------------
            ByteIndex = aCommand.ByteIndex
            PathString = aCommand.PathString
            Type = aCommand.Type
            Value = Force.DeepCloner.DeepClonerExtensions.DeepClone(Of Object)(aCommand.Value)
            SubCommands = New TSUBCOMMANDS(aCommand.SubCommands)
            'init --------------------------------------------------



        End Sub

#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property TypeName As String
            Get
                Select Case Type
                    Case dxxShapeCommands.EndShape ' = 0
                        Return "EndShape"
                    Case dxxShapeCommands.PenDown ' = 1
                        Return "PenDown"
                    Case dxxShapeCommands.PenUp ' = 2
                        Return "PenUp"
                    Case dxxShapeCommands.DivideBy ' = 3
                        Return "DivideBy"
                    Case dxxShapeCommands.MultiplyBy ' = 4
                        Return "MultiplyBy"
                    Case dxxShapeCommands.PushToStack ' = 5
                        Return "PushToStack"
                    Case dxxShapeCommands.PopFromStack ' = 6
                        Return "PopFromStack"
                    Case dxxShapeCommands.SubShape ' = 7
                        Return "SubShape"
                    Case dxxShapeCommands.XYDisplacement ' = 8
                        Return "XYDisplacement"
                    Case dxxShapeCommands.MultiXYDisplacement ' = 9
                        Return "MultiXYDisplacement"
                    Case dxxShapeCommands.OctantArc ' = 10
                        Return "OctantArc"
                    Case dxxShapeCommands.FractionalArc ' = 11
                        Return "FractionalArc"
                    Case dxxShapeCommands.BulgeArc ' = 12
                        Return "BulgeArc"
                    Case dxxShapeCommands.MultiBulgeArc ' = 13
                        Return "MultiBulgeArc"
                    Case dxxShapeCommands.VerticalOnly ' = 14
                        Return "VerticalOnly"
                    Case dxxShapeCommands.VectorLengthAndDirection ' = 15
                        Return "VectorLengthAndDirection"
                    Case Else
                        Return "Invalid Code"
                End Select
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Public Overrides Function ToString() As String
            Return $"{Type}  : {  PathString}"

        End Function
        Public Function Clone() As TSHAPECOMMAND
            Return New TSHAPECOMMAND(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TSHAPECOMMAND(Me)
        End Function
        Public Function DecodeDisplacement(aBasePt As TPOINT, Optional sMultBy As Double = 1) As TPOINT
            Dim rDX As Double = 0.0
            Dim rDY As Double = 0.0
            Return DecodeDisplacement(aBasePt, sMultBy, rDX, rDY)
        End Function
        Public Function DecodeDisplacement(aBasePt As TPOINT, sMultBy As Double, ByRef rDX As Double, ByRef rDY As Double) As TPOINT
            Try
                Dim vLen As Double = SubCommands.Item(1).Value1
                Dim vAng As Double = SubCommands.Item(1).Value2
                If sMultBy > 0 Then vLen *= sMultBy
                Return aBasePt.SnapToGrid(vAng, vLen, rDX, rDY)
            Catch ex As Exception
            End Try
        End Function
#End Region 'Methods
#Region "Shared Methods"
        Public Shared Sub DecodeDisplacementCode(aDispCode As String, ByRef rDisplacement As Double, ByRef rAngle As Double)
            Dim rError As String = String.Empty
            DecodeDisplacementCode(aDispCode, rDisplacement, rAngle, rError)
        End Sub
        Public Shared Sub DecodeDisplacementCode(aDispCode As String, ByRef rDisplacement As Double, ByRef rAngle As Double, ByRef rError As String)
            rDisplacement = 0
            rAngle = 0
            rError = String.Empty
            Try
                If String.IsNullOrWhiteSpace(aDispCode) Then
                    rError = "Null String Passed"
                    Return
                End If
                aDispCode = aDispCode.Trim().ToUpper()

                If aDispCode.Length = 2 Then aDispCode = $"0{ aDispCode}"
                If aDispCode.Length <> 3 Then
                    rError = "Invalid String Passed"
                    Return
                End If
                aDispCode = aDispCode.Substring(1, 2)

                Dim i1 As Double = TVALUES.HexToDouble(aDispCode.Substring(0, 1))
                Dim i2 As Double = TVALUES.HexToDouble(aDispCode.Substring(1, 1))
                If i1 > 15 Or i2 > 15 Then
                    rError = "Invalid Displacement Passed"
                    Return
                End If
                rDisplacement = i1
                rAngle = i2 * 22.5
            Catch ex As Exception
                rError = ex.Message
            End Try
        End Sub
#End Region 'Shared Methods
    End Structure 'TSHAPECOMMAND
    Friend Structure TSHAPECOMMANDS
        Implements ICloneable
#Region "Members"
        Private _Init As Boolean
        Private _Members() As TSHAPECOMMAND
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aCount As Integer = 0)
            'init -----------------
            _Init = True
            ReDim _Members(-1)
            'init -----------------
            For i As Integer = 1 To aCount
                Add(New TSHAPECOMMAND(dxxShapeCommands.PenUp))
            Next
        End Sub

        Public Sub New(aCommands As TSHAPECOMMANDS)

            'init -----------------
            _Init = True
            ReDim _Members(-1)
            'init -----------------
            If aCommands.Count <= 0 Then Return
            _Members = aCommands._Members.Clone()

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
        Public Function Item(aIndex As Integer) As TSHAPECOMMAND
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return New TSHAPECOMMAND(dxxShapeCommands.Undefined)
            End If
            Return _Members(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, value As TSHAPECOMMAND)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1) = value
        End Sub
        Public Function Clone() As TSHAPECOMMANDS

            Return New TSHAPECOMMANDS(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TSHAPECOMMANDS(Me)
        End Function
        Public Sub Add(aCommand As TSHAPECOMMAND)
            Array.Resize(_Members, Count + 1)
            _Members(_Members.Count - 1) = aCommand
        End Sub
        Public Sub Clear()
            _Init = True
            ReDim _Members(-1)

        End Sub
        Public Function ToList() As List(Of TSHAPECOMMAND)
            If Count <= 0 Then Return New List(Of TSHAPECOMMAND)
            Return _Members.ToList()
        End Function
#End Region 'Methods
#Region "Shared Methods"
        Public Shared Function CommandStringsToCommands(aCommands As TVALUES, ByRef rSubShapes As Boolean, ByRef rError As String) As TSHAPECOMMANDS
            Dim ub As Integer
            Dim lb As Integer
            Dim idx As Integer
            Dim cmd As String
            Dim bCode As Boolean
            Dim hexstr As String
            Dim hexval As Integer
            Dim skip As Integer
            Dim iVal As Integer
            Dim txt As String
            Dim aErr As String = String.Empty
            Dim b1 As Boolean
            Dim sErr As String = String.Empty
            Dim j As Integer
            Dim k As Integer
            Dim n As Integer
            Dim nCmd As New TSHAPECOMMAND
            Dim aCmd As New TSHAPECOMMAND
            Dim rCmds As New TSHAPECOMMANDS
            Dim aSub As New TSUBCOMMAND
            Dim s1 As Double
            Dim s2 As Double
            Dim SubStr As String = String.Empty
            Dim ctypeName As String = String.Empty
            Dim sVals As List(Of String) = aCommands.ToStringList()
            'vals_Print(aCommands)
            rCmds.Clear()
            Try
                rSubShapes = False
                lb = 0
                ub = aCommands.Count - 1
                rError = String.Empty
                aErr = "Invalid Byte Code Detected"
                For idx = 0 To sVals.Count - 1

                    nCmd.ByteIndex = idx
                    bCode = False
                    hexstr = "000"
                    hexval = -1
                    cmd = sVals(idx).ToUpper().Replace("+", "")
                    TLISTS.Add(SubStr, cmd, bAllowDuplicates:=True)
                    If cmd.StartsWith("0") Then
                        hexstr = cmd
                        hexval = TVALUES.HexToInteger(hexstr)
                    Else
                        If TVALUES.IsNumber(cmd) Then
                            iVal = TVALUES.To_INT(cmd)
                            If iVal >= 0 And iVal <= 255 Then hexval = iVal
                        End If
                    End If
                    skip = 0
                    bCode = hexval >= 0 And hexval <= 14
                    If bCode Then
                        aCmd = nCmd
                        aCmd.Type = hexval
                        ctypeName = aCmd.TypeName
                        Select Case aCmd.Type
'===================================================================================================
                            Case dxxShapeCommands.DivideBy, dxxShapeCommands.MultiplyBy
                                'multiply or divide
                                skip = 1
                                If idx + skip <= ub Then
                                    txt = sVals(idx + skip)
                                    SubStr += $",{ txt}"
                                    If Not TVALUES.IsNumber(txt) Then
                                        Throw New Exception($"{ctypeName }:ERR:Non-Numeric Detected '{txt }'")
                                    End If
                                    iVal = TVALUES.To_INT(txt)
                                    If iVal < 0 Or iVal > 255 Then
                                        Throw New Exception($"{ctypeName }:ERR:Invalid Byte Value Detected '{ iVal }'")
                                    End If
                                    aCmd.Value = TVALUES.To_DBL(iVal)
                                Else
                                    Throw New Exception($"{ctypeName }:ERR:Array Bounds Exceeded (Skipping)")
                                End If
'===================================================================================================
                            Case dxxShapeCommands.SubShape
                                'subshape
                                rSubShapes = True
                                skip = 1
                                If idx + 1 <= ub Then
                                    txt = sVals(idx + 1)
                                    SubStr += $",{ txt}"
                                    '                    txt = sVals(idx + 2)
                                    '                    subStr = subStr & "," & txt
                                    aCmd.Value = txt
                                Else
                                    Throw New Exception($"{ctypeName }:ERR:Array Bounds Exceeded (Skipping)")
                                End If
'===================================================================================================
                            Case dxxShapeCommands.XYDisplacement
                                'X-Y Displacement
                                skip = 2
                                If idx + 2 <= ub Then
                                    aSub = New TSUBCOMMAND(aCmd.Type)
                                    SubStr += $",{sVals(idx + 1)}"
                                    SubStr += $",{sVals(idx + 2)}"
                                    txt = sVals(idx + 1).Replace("(", "")
                                    If Not TVALUES.IsNumber(txt) Then
                                        Throw New Exception($"{ctypeName }:ERR:Non-Numeric Detected '{txt }'")
                                    End If
                                    aSub.Value1 = TVALUES.To_DBL(txt)  'dX
                                    txt = sVals(idx + 2).Replace(")", "").Trim()
                                    If Not TVALUES.IsNumber(txt) Then
                                        Throw New Exception($"{ctypeName }:ERR:Non-Numeric Detected '{txt }'")
                                    End If
                                    aSub.Value2 = TVALUES.To_DBL(txt) 'dY
                                    aCmd.SubCommands.Add(aSub.Clone)
                                Else
                                    Throw New Exception($"{ctypeName }:ERR:Array Bounds Exceeded (Skipping)")
                                End If
'===================================================================================================
                            Case dxxShapeCommands.MultiXYDisplacement
                                'multiple X-Y Displacement
                                skip = 0
                                j = idx + 1
                                k = j + 1
                                Do While j <= ub And k <= ub
                                    aSub = New TSUBCOMMAND(aCmd.Type)
                                    SubStr += $",{sVals(j)}"
                                    SubStr += $",{sVals(k)}"
                                    txt = sVals(j).Replace("(", "")
                                    If Not TVALUES.IsNumber(txt) Then
                                        Throw New Exception($"{ctypeName }:ERR:Non-Numeric Detected '{txt }'")
                                    End If
                                    aSub.Value1 = TVALUES.To_DBL(txt)
                                    txt = sVals(k).Replace(")", "").Trim()
                                    If Not TVALUES.IsNumber(txt) Then
                                        Throw New Exception($"{ctypeName }:ERR:Non-Numeric Detected '{txt }'")
                                    End If
                                    aSub.Value2 = TVALUES.To_DBL(txt)
                                    skip += 2
                                    If aSub.Value1 = 0 And aSub.Value2 = 0 Then
                                        Exit Do
                                    Else
                                        aCmd.SubCommands.Add(aSub.Clone)
                                    End If
                                    j = k + 1
                                    k = j + 1
                                Loop
'===================================================================================================
                            Case dxxShapeCommands.OctantArc
                                'Octant Arc
                                skip = 2
                                If idx + 2 <= ub Then
                                    aSub = New TSUBCOMMAND(aCmd.Type)
                                    SubStr += $",{sVals(idx + 1)}"
                                    SubStr += $",{sVals(idx + 2)}"
                                    txt = sVals(idx + 1).Replace("(", "")
                                    If Not TVALUES.IsNumber(txt) Then
                                        Throw New Exception($"{ctypeName }:ERR:Non-Numeric Detected '{txt }'")
                                    End If
                                    iVal = TVALUES.To_INT(txt)
                                    If iVal < 0 Then
                                        Throw New Exception($"{ctypeName }:ERR:Invalid Negative Number Detected '{ iVal }'")
                                    End If
                                    aSub.Value1 = TVALUES.To_DBL(iVal) 'radius
                                    txt = Trim(Replace(sVals(idx + 2), ")", ""))
                                    TSHAPECOMMANDS.DecodeOSCString(txt, s1, s2, b1, sErr)
                                    If sErr <> "" Then
                                        rError = sErr
                                        Throw New Exception($"{ctypeName }:ERR:Invalid OCS String Detected '" & sErr & "'")
                                    End If
                                    aSub.Flag = b1
                                    aSub.Value2 = s1
                                    aSub.Value3 = s2
                                    aCmd.SubCommands.Add(aSub.Clone)
                                Else
                                    Throw New Exception($"{ctypeName }:ERR:Array Bounds Exceeded (Skipping)")
                                End If
'===================================================================================================
                            Case dxxShapeCommands.FractionalArc
                                'Fraction Arc
                                skip = 5
                                If idx + 5 <= ub Then
                                    aSub = New TSUBCOMMAND(aCmd.Type)
                                    SubStr += $",{sVals(idx + 1)}"
                                    SubStr += $",{sVals(idx + 2)}"
                                    SubStr += $",{sVals(idx + 3)}"
                                    SubStr += $",{sVals(idx + 4)}"
                                    SubStr += $",{sVals(idx + 5)}"
                                    txt = Replace(sVals(idx + 1), "(", "")
                                    If Not TVALUES.IsNumber(txt) Then
                                        Throw New Exception($"{ctypeName }:ERR:Non-Numeric Detected '{txt }'")
                                    End If
                                    iVal = TVALUES.To_INT(txt)
                                    If iVal < 0 Then
                                        Throw New Exception($"{ctypeName }:ERR:Invalid Negative Number Detected '{ iVal }'")
                                    End If
                                    aSub.Value1 = TVALUES.To_DBL(iVal) 'start offset
                                    txt = sVals(idx + 2)
                                    If Not TVALUES.IsNumber(txt) Then
                                        Throw New Exception($"{ctypeName }:ERR:Non-Numeric Detected '{txt }'")
                                    End If
                                    iVal = TVALUES.To_INT(txt)
                                    If iVal < 0 Then
                                        Throw New Exception($"{ctypeName }:ERR:Invalid Negative Number Detected '{ iVal }'")
                                    End If
                                    aSub.Value2 = TVALUES.To_DBL(iVal) 'end offset
                                    txt = sVals(idx + 3)
                                    If Not TVALUES.IsNumber(txt) Then
                                        Throw New Exception($"{ctypeName }:ERR:Non-Numeric Detected '{txt }'")
                                    End If
                                    iVal = TVALUES.To_INT(txt)
                                    If iVal < 0 Then
                                        Throw New Exception($"{ctypeName }:ERR:Invalid Negative Number Detected '{ iVal }'")
                                    End If
                                    aSub.Value3 = TVALUES.To_DBL(iVal) 'high radius
                                    txt = sVals(idx + 4)
                                    If Not TVALUES.IsNumber(txt) Then
                                        Throw New Exception($"{ctypeName }:ERR:Non-Numeric Detected '{txt }'")
                                    End If
                                    iVal = TVALUES.To_INT(txt)
                                    If iVal < 0 Then
                                        Throw New Exception($"{ctypeName }:ERR:Invalid Negative Number Detected '{ iVal }'")
                                    End If
                                    aSub.Value4 = iVal 'radius
                                    txt = Trim(Replace(sVals(idx + 5), ")", ""))
                                    If Not TVALUES.IsNumber(txt) Then
                                        Throw New Exception($"{ctypeName }:ERR:Non-Numeric Detected '{txt }'")
                                    End If
                                    txt = sVals(idx + 5)
                                    'octants
                                    TSHAPECOMMANDS.DecodeOSCString(txt, s1, s2, b1, sErr)
                                    If sErr <> "" Then
                                        rError = sErr
                                        Throw New Exception($"{ctypeName }:ERR:'" & rError & "'")
                                    End If
                                    aSub.Flag = b1
                                    aSub.Value5 = s1
                                    aSub.Value6 = s2
                                    aSub.CommandType = aCmd.Type
                                    aCmd.SubCommands.Add(aSub.Clone)
                                Else
                                    Throw New Exception($"{ctypeName }:ERR:Array Bounds Exceeded (Skipping)")
                                End If
'===================================================================================================
                            Case dxxShapeCommands.BulgeArc
                                'Bulge Arc
                                skip = 3
                                If idx + 3 <= ub Then
                                    aSub = New TSUBCOMMAND(aCmd.Type)
                                    SubStr += $",{sVals(idx + 1)}"
                                    SubStr += $",{sVals(idx + 2)}"
                                    SubStr += $",{sVals(idx + 3)}"
                                    txt = Replace(sVals(idx + 1), "(", "")
                                    If Not TVALUES.IsNumber(txt) Then
                                        Throw New Exception($"{ctypeName }:ERR:Non-Numeric Detected '{txt }'")
                                    End If
                                    aSub.Value1 = TVALUES.To_DBL(txt)
                                    txt = Trim(Replace(sVals(idx + 2), ")", ""))
                                    If Not TVALUES.IsNumber(txt) Then
                                        Throw New Exception($"{ctypeName }:ERR:Non-Numeric Detected '{txt }'")
                                    End If
                                    aSub.Value2 = TVALUES.To_DBL(txt)
                                    txt = Trim(Replace(sVals(idx + 3), ")", ""))
                                    If Not TVALUES.IsNumber(txt) Then
                                        Throw New Exception($"{ctypeName }:ERR:Non-Numeric Detected '{txt }'")
                                    End If
                                    aSub.Value3 = TVALUES.To_DBL(txt)
                                    aSub.CommandType = aCmd.Type
                                    aCmd.SubCommands.Add(aSub.Clone)
                                Else
                                    Throw New Exception($"{ctypeName }:ERR:Array Bounds Exceeded (Skipping)")
                                End If
'===================================================================================================
                            Case dxxShapeCommands.MultiBulgeArc
                                'multi -Bulge Arc
                                j = idx + 1
                                k = j + 1
                                n = j + 2
                                Do While j <= ub And k <= ub And n <= ub
                                    aSub = New TSUBCOMMAND(aCmd.Type)
                                    SubStr += $",{sVals(j)}"
                                    SubStr += $"{sVals(k)}"
                                    SubStr += $",{ sVals(n)}"
                                    txt = sVals(j).Replace("(", "")
                                    If Not TVALUES.IsNumber(txt) Then
                                        Throw New Exception($"{ctypeName }:ERR:Non-Numeric Detected '{txt }'")
                                    End If
                                    aSub.Value1 = TVALUES.To_DBL(txt)
                                    txt = Trim(Replace(sVals(k), ")", ""))
                                    If Not TVALUES.IsNumber(txt) Then
                                        Throw New Exception($"{ctypeName }:ERR:Non-Numeric Detected '{txt }'")
                                    End If
                                    aSub.Value2 = TVALUES.To_DBL(txt)
                                    txt = Trim(Replace(sVals(n), ")", ""))
                                    If Not TVALUES.IsNumber(txt) Then
                                        Throw New Exception($"{ctypeName }:ERR:Non-Numeric Detected '{txt }'")
                                    End If
                                    aSub.Value3 = TVALUES.To_DBL(txt)
                                    If aSub.Value1 <> 0 Or aSub.Value2 <> 0 Then
                                        skip += 3
                                        SubStr += $",{sVals(j)}"
                                        SubStr += $",{sVals(k)}"
                                        SubStr += $",{sVals(n)}"
                                        aCmd.SubCommands.Add(aSub.Clone)
                                    Else
                                        SubStr += $",{sVals(j)}"
                                        SubStr += $",{sVals(k)}"
                                        skip += 2
                                        Exit Do
                                    End If
                                    j = n + 1
                                    k = j + 1
                                    n = j + 2
                                Loop
                        End Select
                        aCmd.PathString = SubStr
                        SubStr = ""
                        rCmds.Add(aCmd.Clone)
                    Else
                        aCmd = nCmd
                        '===================================================================================================
                        aCmd.Type = dxxShapeCommands.VectorLengthAndDirection
                        ctypeName = aCmd.TypeName
                        TSHAPECOMMAND.DecodeDisplacementCode(cmd, s1, s2, sErr)
                        If sErr <> "" Then
                            rError = sErr
                            Throw New Exception($"{ctypeName }:ERR:Invalid Displacement Detected '" & sErr & "'")
                        End If
                        aSub = New TSUBCOMMAND(aCmd.Type) With {
                            .Value1 = s1,
                            .Value2 = s2
                        }
                        aCmd.SubCommands.Add(aSub.Clone)
                        aCmd.PathString = SubStr
                        SubStr = ""
                        rCmds.Add(aCmd.Clone)
                    End If
                    If skip > 0 Then idx += skip
                    If idx + 1 > ub Then Exit For
                    If rError <> "" Then Return New TSHAPECOMMANDS
                    'Application.DoEvents()
                Next idx
                Return rCmds
            Catch ex As Exception
                rError = ex.Message
                System.Diagnostics.Debug.WriteLine("shp_CommandStringsToCommands ERROR: " & rError)
                Return rCmds
            End Try
        End Function
        Public Shared Sub DecodeOSCString(aOSC As String, ByRef rStartAngle As Double, ByRef rAngleSpan As Double, ByRef rClockWise As Boolean, ByRef rError As String)
            Try
                Dim aByte As Byte
                Dim T1 As String
                Dim t2 As String
                Dim i1 As Integer
                Dim i2 As Integer
                rError = String.Empty
                rStartAngle = 0
                rAngleSpan = 0
                rClockWise = False
                aOSC = Trim(UCase(aOSC))
                aOSC = Replace(aOSC, ")", "")
                If aOSC = "" Then
                    rError = "Null String Passed"
                    Return
                End If
                If aOSC.Length <> 3 And aOSC.Length <> 4 Then
                    rError = "Invalid String Passed"
                    Return
                End If
                If aOSC.Length = 3 Then
                    If Left(aOSC, 1) <> "0" Then
                        rError = "Invalid String Passed"
                        Return
                    End If
                Else
                    rClockWise = aOSC.StartsWith("-")
                    aByte = 128
                    If Left(aOSC, 1) <> "-" And Mid(aOSC, 2, 1) <> "0" Then
                        rError = "Invalid String Passed"
                        Return
                    End If
                End If
                aOSC = Right(aOSC, 2)
                If Not TVALUES.IsNumber(aOSC) Then
                    rError = "Invalid Octants Passed"
                    Return
                End If
                If Not TVALUES.IsNumber(aOSC) Then
                    rError = "Invalid Octants Passed"
                    Return
                End If
                T1 = aOSC.Substring(0, 1)
                t2 = aOSC.Substring(aOSC.Length - 1, 1)
                i1 = TVALUES.To_INT(T1)
                i2 = TVALUES.To_INT(t2)
                If i1 > 7 Or i2 > 7 Then
                    rError = "Invalid Octants Passed"
                    Return
                End If
                rStartAngle = i1 * 45
                If i2 = 0 Then i2 = 8
                rAngleSpan = i2 * 45
                Return
            Catch ex As Exception
                rError = ex.Message
            End Try
        End Sub
#End Region 'Shared MEthods
    End Structure 'TSHAPECOMMANDS
    Friend Structure TSHAPE
        Implements ICloneable
#Region "Members"
        Public Ascent As Byte
        Public ByteCount As Integer
        Public Descent As Byte
        Public FileName As String
        Public GroupName As String
        Public HasSubShapes As Boolean
        Public Index As Integer
        Public Name As String
        Public Key As String
        Public PathString As String
        Public PenDown As Boolean
        Public ShapeNumber As Integer
        Public ShapeType As String
        Public Commands As TSHAPECOMMANDS
        Public PointStack As TPOINTS
        Public Path As TPOINTS
        Public PathBytes() As Byte
        Public PathCommands As TVALUES
#End Region 'Members
#Region "Constructors"
        Public Sub New(aShape As TSHAPE)
            'init ----------------------------------------
            Ascent = 0
            ByteCount = 0
            Descent = 0
            FileName = ""
            GroupName = ""
            HasSubShapes = False
            Index = -1
            Name = ""
            Key = ""
            PathString = ""
            PenDown = False
            ShapeNumber = 0
            ShapeType = ""
            Commands = New TSHAPECOMMANDS(0)
            PointStack = New TPOINTS(0)
            Path = New TPOINTS(0)
            ReDim PathBytes(-1)
            PathCommands = New TVALUES(0)
            'init ----------------------------------------

            Ascent = aShape.Ascent
            ByteCount = aShape.ByteCount
            Descent = aShape.Descent
            FileName = aShape.FileName
            GroupName = aShape.GroupName
            HasSubShapes = aShape.HasSubShapes
            Name = aShape.Name
            Key = aShape.Key
            PathString = aShape.PathString
            PenDown = aShape.PenDown
            ShapeNumber = aShape.ShapeNumber
            ShapeType = aShape.ShapeType
            Commands = New TSHAPECOMMANDS(aShape.Commands)
            PointStack = New TPOINTS(aShape.PointStack)
            Path = New TPOINTS(aShape.Path)
            If aShape.PathBytes IsNot Nothing Then PathBytes = aShape.PathBytes.Clone()
            PathCommands = aShape.PathCommands.Clone()
        End Sub

        Public Sub New(aShape As dxoShape)
            'init ----------------------------------------
            Ascent = 0
            ByteCount = 0
            Descent = 0
            FileName = ""
            GroupName = ""
            HasSubShapes = False
            Index = -1
            Name = ""
            Key = ""
            PathString = ""
            PenDown = False
            ShapeNumber = 0
            ShapeType = ""
            Commands = New TSHAPECOMMANDS(0)
            PointStack = New TPOINTS(0)
            Path = New TPOINTS(0)
            ReDim PathBytes(-1)
            PathCommands = New TVALUES(0)
            'init ----------------------------------------
            If aShape Is Nothing Then Return
            Ascent = aShape.Ascent
            ByteCount = aShape.ByteCount
            Descent = aShape.Descent
            FileName = aShape.FileName
            GroupName = aShape.GroupName
            HasSubShapes = aShape.HasSubShapes
            Name = aShape.Name
            Key = aShape.Key
            PathString = aShape.PathString
            PenDown = aShape.PenDown
            ShapeNumber = aShape.ShapeNumber
            ShapeType = aShape.ShapeType
            Commands = New TSHAPECOMMANDS(aShape.Commands)
            PointStack = New TPOINTS(aShape.PointStack)
            Path = New TPOINTS(aShape.Path)
            If aShape.PathBytes IsNot Nothing Then PathBytes = aShape.PathBytes.Clone()
            PathCommands = aShape.PathCommands.Clone()
        End Sub
        Public Sub New(Optional aName As String = "")
            'init ----------------------------------------
            Ascent = 0
            ByteCount = 0
            Descent = 0
            FileName = ""
            GroupName = ""
            HasSubShapes = False
            Index = -1
            Name = ""
            Key = ""
            PathString = ""
            PenDown = False
            ShapeNumber = 0
            ShapeType = ""
            Commands = New TSHAPECOMMANDS(0)
            PointStack = New TPOINTS(0)
            Path = New TPOINTS(0)
            ReDim PathBytes(-1)
            PathCommands = New TVALUES(0)
            'init ----------------------------------------
            Name = aName
        End Sub

#End Region 'Constructors
#Region "Methods"
        Public Sub Clear()

            Commands = New TSHAPECOMMANDS(0)
            Path = New TPOINTS(0)
            ReDim PathBytes(-1)
            PathCommands = New TVALUES(0)
            PointStack = New TPOINTS(0)

        End Sub

        Public Function Clone() As TSHAPE
            Return New TSHAPE(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TSHAPE(Me)
        End Function

        Public Function GetPath(aPlane As TPLANE, aDisplay As TDISPLAYVARS, bFilled As Boolean, aColor As Integer, aParentPlane As TPLANE, bRelative As Boolean) As TPATH



            If bRelative Then
                If TPLANE.IsNull(aParentPlane) Then bRelative = False
            End If
            If TPLANE.IsNull(aPlane) Then aPlane = TPLANE.World
            Dim aLoop As New TVECTORS(0)
            For i As Integer = 1 To Path.Count
                Dim P1 As TPOINT = Path.Item(i)
                Dim v1 As New TVECTOR(aPlane, P1.X, P1.Y, 0)
                If bRelative Then
                    v1 = v1.WithRespectTo(aParentPlane)
                End If
                aLoop.Add(v1, P1.Code)
            Next i
            Dim _rVal As TPATH
            If Not bRelative Then
                _rVal = New TPATH(dxxDrawingDomains.Model, aDisplay, New dxfPlane(aPlane), aLoop) With {.Linetype = dxfLinetypes.Continuous, .Filled = bFilled, .Color = aColor, .Relative = bRelative, .LayerName = aDisplay.LayerName}
            Else
                _rVal = New TPATH(dxxDrawingDomains.Model, aDisplay, New dxfPlane(aParentPlane), aLoop) With {.Linetype = dxfLinetypes.Continuous, .Filled = bFilled, .Color = aColor, .Relative = bRelative, .LayerName = aDisplay.LayerName}
            End If
            If bRelative Then _rVal.Plane = aParentPlane
            '_rVal.Print(4)
            'Path.Print()
            Return _rVal
        End Function
        Public Function GetPathString(ByRef rFileText As List(Of String)) As String

            rFileText = New List(Of String)
            If PathString = "" Then UpdatePathCommands()
            Dim _rVal As String = PathString
            Try
                Dim cmds As String
                Dim i As Integer
                Dim stxt As String
                cmds = PathString
                If cmds.Length <= 75 Then
                    rFileText.Add(cmds)
                Else
                    Do Until cmds = ""
                        i = InStrRev(cmds, ",", 76, True)
                        If i > 0 Then
                            stxt = dxfUtils.LeftOf(cmds, ",", bFromEnd:=True)
                            cmds = dxfUtils.RightOf(cmds, ",", bFromEnd:=True)
                            rFileText.Add(stxt)
                            If cmds.Length <= 85 Then
                                rFileText.Add(cmds)
                                Exit Do
                            End If
                        Else
                            If cmds.Length > 0 Then
                                rFileText.Add(cmds)
                                Exit Do
                            End If
                        End If
                    Loop
                End If
            Catch ex As Exception
                Throw ex
            End Try
            Return _rVal
        End Function
        Public Overrides Function ToString() As String
            Return $"{Index }-{ ShapeNumber }[{ Name }]={ PathString}"
        End Function
        Public Sub UpdateCommands(Optional bRegen As Boolean = False)
            Dim rSubShapes As Boolean = False
            UpdateCommands(bRegen, rSubShapes)
        End Sub
        Public Sub UpdateCommands(bRegen As Boolean, ByRef rSubShapes As Boolean)
            If ByteCount = 0 Or PathCommands.Count = 0 Then
                If PathString <> "" Then
                    PathCommands = TVALUES.FromList(PathString)
                    ByteCount = PathCommands.Count
                    bRegen = True
                End If
            End If
            If Commands.Count <= 0 Or bRegen Then
                If ByteCount > 0 Then
                    Dim serr As String = String.Empty
                    Commands = TSHAPECOMMANDS.CommandStringsToCommands(PathCommands, rSubShapes, serr)
                End If
            Else
                rSubShapes = HasSubShapes
            End If
        End Sub
        Public Function UpdatePathCommands() As String
            Dim _rVal As String
            Dim cnt As Integer
            Dim pCmds As New TVALUES
            Dim bSubShapes As Boolean
            Try
                _rVal = dxfBytes.ToPathCommands(PathBytes, pCmds, cnt, True, -1, bSubShapes)
                PathString = _rVal
                HasSubShapes = bSubShapes
                PathCommands = pCmds
                ByteCount = PathCommands.Count
            Catch ex As Exception
                Throw ex
            End Try
            Return _rVal
        End Function
#End Region 'Methods

#Region "Shared Methods"
        Public Shared ReadOnly Property Null
            Get
                Return New TSHAPE("")
            End Get
        End Property
#End Region 'Shared Methods
    End Structure 'TSHAPE
    Friend Structure TSHAPEARRAY
        Implements ICloneable
#Region "Members"
        Public ImageGUID As String
        Public Name As String
        Private _Init As Boolean
        Private _Members() As TSHAPES
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aImageGUID As String = "")
            'init -----------------------
            Name = ""
            _Init = True
            ReDim _Members(-1)
            ImageGUID = ""
            'init -----------------------
            If Not String.IsNullOrEmpty(aImageGUID) Then ImageGUID = aImageGUID.Trim

        End Sub

        Public Sub New(aShapes As TSHAPEARRAY)
            'init -----------------------
            Name = aShapes.Name
            _Init = True
            ReDim _Members(-1)
            ImageGUID = aShapes.ImageGUID
            'init -----------------------
            If aShapes._Init Then _Members = aShapes._Members.Clone()

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
        Public Function Item(aIndex As Integer) As TSHAPES
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return TSHAPES.Null
            End If
            Return _Members(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, value As TSHAPES)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1) = value
        End Sub
        Public Sub Clear()
            _Init = True
            ReDim _Members(-1)

        End Sub
        Public Sub Add(aShapes As TSHAPES, Optional aImage As dxfImage = Nothing)
            Dim rIndex As Integer = 0
            Add(aShapes, aImage, rIndex)
        End Sub
        Public Sub Add(aShapes As TSHAPES, aImage As dxfImage, ByRef rIndex As Integer)
            rIndex = -1
            System.Array.Resize(_Members, Count + 1)
            _Members(_Members.Count - 1) = aShapes
            rIndex = _Members.Count - 1
            If aImage IsNot Nothing Then aImage.obj_SHAPES = Me
        End Sub
        Public Function GetByFileName(aFileName As String, ByRef rIndex As Integer, Optional bLoadIfNotFound As Boolean = False, Optional aImage As dxfImage = Nothing) As TSHAPES
            Dim _rVal As TSHAPES = TSHAPES.Null
            If String.IsNullOrWhiteSpace(aFileName) Then Return _rVal
            rIndex = -1
            Dim i As Integer
            Dim fname As String
            Dim shps As TSHAPES
            fname = aFileName.Trim
            If fname = "" Then Return _rVal
            For i = 1 To Count
                shps = Item(i)
                If String.Compare(shps.FileName, fname, True) = 0 Then
                    rIndex = i - 1
                    _rVal = shps
                    Exit For
                End If
            Next i
            If bLoadIfNotFound And rIndex < 0 Then
                Dim aShps As New TSHAPES
                aShps = TSHAPES.FindSHXFile(aFileName, bLoadIfFound:=True)
                If aShps.FileName <> "" Then Add(aShps, aImage, rIndex)
            End If
            Return _rVal
        End Function
        Public Function GetColByName(aGroupName As String, ByRef rIndex As Integer, Optional bLoadIfNotFound As Boolean = False, Optional aImage As dxfImage = Nothing) As TSHAPES
            Dim _rVal As TSHAPES = TSHAPES.Null
            rIndex = -1
            If String.IsNullOrWhiteSpace(aGroupName) Then Return _rVal
            Dim i As Integer
            Dim gname As String
            Dim shps As TSHAPES
            gname = aGroupName.Trim()
            i = InStrRev(gname, ".")
            If i > 0 Then
                gname = Trim(Left(gname, gname.Length - i))
                If gname = "" Then Return _rVal
            End If
            For i = 0 To Count - 1
                shps = Item(i)
                If String.Compare(shps.Name, gname, True) = 0 Then
                    rIndex = i
                    _rVal = shps
                    Exit For
                End If
            Next i
            If bLoadIfNotFound And rIndex < 0 Then
                Dim aShps As TSHAPES = TSHAPES.FindSHXFile(aGroupName, bLoadIfFound:=True)
                If aShps.FileName <> "" Then Add(aShps, aImage, rIndex)
            End If
            Return _rVal
        End Function


        Public Function GetShapes(aShapesName As String, ByRef rIndex As Integer, Optional bLoadIfNotFound As Boolean = False, Optional aImage As dxfImage = Nothing) As TSHAPES
            Dim _rVal As New TSHAPES
            Dim sName As String
            Dim i As Integer
            sName = Trim(aShapesName)
            rIndex = -1
            If sName = "" Then Return _rVal
            If sName.Contains("\") Then
                _rVal = GetByFileName(sName, rIndex, bLoadIfNotFound, aImage)
            End If
            If rIndex < 0 Then
                i = InStrRev(sName, ".")
                If i > 0 Then
                    sName = Trim(Left(sName, sName.Length - i))
                    If sName = "" Then Return _rVal
                End If
                _rVal = GetColByName(sName, rIndex, bLoadIfNotFound, aImage)
            End If
            Return _rVal
        End Function
        Public Function Clone() As TSHAPEARRAY

            Return New TSHAPEARRAY(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TSHAPEARRAY(Me)
        End Function
#End Region 'Methods
    End Structure 'TSHAPEARRAY
    Friend Structure TSHAPES
        Implements ICloneable
#Region "Members"
        Public Ascent As Byte
        Public Comment As String
        Public Descent As Byte
        Public Embedding As Byte
        Public Encoding As Byte
        Public FileName As String
        Public Header As String
        Public IsFont As Boolean
        Friend _Loaded As Boolean
        Public Mode As Byte
        Public Name As String
        Public ErrorString As String
        Private _Members() As TSHAPE
        Private _Init As Boolean

#End Region 'Members
#Region "Constructors"


        Public Sub New(aName As String)
            'init ------------------------------------
            Ascent = 0
            Comment = ""
            Descent = 0
            Embedding = 0
            Encoding = 0
            FileName = ""
            Header = ""
            IsFont = False
            _Loaded = False
            Mode = 0
            Name = ""
            ErrorString = 0
            _Init = True
            ReDim _Members(-1)

            'init ------------------------------------
            If Not String.IsNullOrWhiteSpace(aName) Then Name = aName
        End Sub

        Public Sub New(aShapes As TSHAPES)
            'init ------------------------------------
            Ascent = aShapes.Ascent
            Comment = aShapes.Comment
            Descent = aShapes.Descent
            Embedding = aShapes.Embedding
            Encoding = aShapes.Encoding
            FileName = aShapes.FileName
            Header = aShapes.Header
            IsFont = aShapes.IsFont
            _Loaded = aShapes._Loaded
            Mode = aShapes.Mode
            Name = aShapes.Name
            ErrorString = aShapes.ErrorString
            _Init = True
            ReDim _Members(-1)
            'init ------------------------------------

        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Property Loaded As Boolean
            Get
                Return _Loaded
            End Get
            Set(value As Boolean)
                _Loaded = value
            End Set
        End Property
        Public ReadOnly Property Count As Integer
            Get
                If Not _Init Then
                    _Init = True
                    ReDim _Members(-1)
                End If
                Return _Members.Count
                'Return _Count
            End Get
        End Property
        Public ReadOnly Property IsEmpty As Boolean
            Get
                If Not _Init Then Return True Else Return Count <= 0
            End Get
        End Property
#End Region
#Region "Methods"
        Public Function Item(aIndex As Integer) As TSHAPE
            If IsEmpty Then Return TSHAPE.Null
            If aIndex < 1 Or aIndex > Count Then Return TSHAPE.Null
            _Members(aIndex - 1).Index = aIndex
            _Members(aIndex - 1).GroupName = Name
            Return _Members(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, value As TSHAPE)
            If IsEmpty Then Return
            If aIndex < 1 Or aIndex > Count Then Return
            value.Index = aIndex
            _Members(aIndex - 1) = value

        End Sub
        Public Sub Clear()
            _Init = True
            ReDim _Members(-1)
        End Sub
        Public Function Clone() As TSHAPES
            Return New TSHAPES(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TSHAPES(Me)
        End Function
        Public Function TryGet(aKey As String, ByRef rShape As TSHAPE) As Boolean
            rShape = TSHAPE.Null

            If IsEmpty Or String.IsNullOrWhiteSpace(aKey) Then Return False
            Dim mems = _Members.Where(Function(x) String.Compare(x.Key, aKey, True) = 0)
            If mems.Count <= 0 Then
                mems = _Members.Where(Function(x) String.Compare(x.Name, aKey, True) = 0)
            End If
            If mems.Count > 0 Then
                Dim idx = Array.IndexOf(Of TSHAPE)(_Members, mems(0))
                _Members(idx).Index = idx + 1
                rShape = _Members(idx)
                Return True
            Else
                Return False
            End If

        End Function

        Public Function Add(aNewMem As TSHAPE) As Boolean
            If Not _Init Then
                _Init = True
                ReDim _Members(-1)
            End If
            Dim sKey As String = aNewMem.Name
            Dim defkey As String = $"SHAPE { Count + 1}"
            If String.IsNullOrWhiteSpace(sKey) Then sKey = defkey
            aNewMem.Key = sKey

            Array.Resize(_Members, _Members.Count + 1)
            aNewMem.Index = _Members.Count
            _Members(_Members.Count - 1) = aNewMem
            Return True

        End Function
        Public Function UpdateMember(aIndex As Integer, aShape As TSHAPE) As Boolean
            If IsEmpty Then Return False
            If aIndex < 1 Or aIndex > Count Then Return False
            Try
                aShape.Index = aIndex
                _Members(aIndex - 1) = aShape
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function
        Public Overrides Function ToString() As String
            Return $"TSHAPES [{ Count }]"
        End Function
        Public Function Member(aKeyOrName As String) As TSHAPE

            If IsEmpty Or String.IsNullOrWhiteSpace(aKeyOrName) Then Return TSHAPE.Null
            Dim sKey As String = aKeyOrName
            Dim _rVal As TSHAPE = TSHAPE.Null
            TryGet(aKeyOrName, _rVal)

            Return _rVal
        End Function
        Public Function Contains(aName As String) As Boolean
            If IsEmpty Or String.IsNullOrWhiteSpace(aName) Then Return False
            Dim mem As TSHAPE = TSHAPE.Null
            Return TryGet(aName, mem)
        End Function

        Friend Function TryGet(aShapeNameorNumber As Object, ByRef rShape As TSHAPE) As Boolean
            If aShapeNameorNumber Is Nothing Or IsEmpty Then Return False
            Dim sVal As String = aShapeNameorNumber.ToString().Trim()

            Dim _rVal As Boolean = False
            If TypeOf aShapeNameorNumber Is String Then

                If String.IsNullOrWhiteSpace(sVal) Then Return False
                _rVal = Contains(sVal)
                If _rVal Then
                    rShape = Member(sVal)
                    Return _rVal
                End If
            End If
            If Not _rVal Then
                If TVALUES.IsNumber(sVal) Then
                    Dim idx As Integer = TVALUES.To_INT(sVal)
                    _rVal = idx > 0 And idx <= Count
                    If _rVal Then
                        rShape = Item(idx)
                    End If
                End If
            End If
            Return _rVal
        End Function

        Public Function GetByShapeNumber(aShapeNum As Integer, ByRef rIndex As Integer) As TSHAPE
            If IsEmpty Then Return TSHAPE.Null
            rIndex = 0

            For i As Integer = 1 To Count
                Dim aMem As TSHAPE = Item(i)
                If aMem.ShapeNumber = aShapeNum Then
                    rIndex = i
                    Return aMem
                End If
            Next i
            Return TSHAPE.Null
        End Function
        Public Sub Write(aFileSpec As String, aBinary As Boolean, aWriteFontsAsShapes As Boolean, ByRef rErrorString As String, Optional bSuppressErrors As Boolean = False)
            rErrorString = ""
            Try
                If String.IsNullOrWhiteSpace(aFileSpec) Then Throw New Exception("No File Name Passed")
                If IsEmpty Then Return

                aFileSpec = aFileSpec.Trim().ToLower()
                If Not aBinary Then
                    If aFileSpec.LastIndexOf(".") < 0 Then
                        aFileSpec += ".shp"
                    End If
                    If IsFont Then
                        If Not aWriteFontsAsShapes Then
                            TSHAPES.WriteSHP_CHAR(Me, aFileSpec, False, rErrorString, True)
                            If rErrorString <> "" Then Throw New Exception(rErrorString)
                        Else
                            TSHAPES.WriteSHP_SHAPES(Me, aFileSpec, False, rErrorString, bSuppressErrors, True)
                            If rErrorString <> "" Then Throw New Exception(rErrorString)
                        End If
                    Else
                        TSHAPES.WriteSHP_SHAPES(Me, aFileSpec, False, rErrorString, bSuppressErrors)
                        If rErrorString <> "" Then Throw New Exception(rErrorString)
                    End If
                Else
                    Throw New Exception("Cant Wite Binary Yet")
                End If
            Catch ex As Exception
                rErrorString = ex.Message
                If Not bSuppressErrors Then Throw New Exception("Write - " & rErrorString)
            End Try
        End Sub
#End Region 'Methods
#Region "Shared Methods"

        Public Shared ReadOnly Property Null As TSHAPES
            Get
                Return New TSHAPES("")
            End Get
        End Property

        Friend Shared Sub WriteSHP_SHAPES(aShapes As TSHAPES, aFileSpec As String, bWriteByteString As Boolean, ByRef rErrorString As String, Optional bSuppressErrors As Boolean = False, Optional bNoInfo As Boolean = False)
            rErrorString = ""
            Dim fout As IO.StreamWriter = Nothing
            Try

                Dim txt As String

                Dim pthStrings As List(Of String) = Nothing

                If IO.File.Exists(aFileSpec) Then IO.File.Delete(aFileSpec)
                fout = IO.File.CreateText(aFileSpec)
                txt = aShapes.Header
                If String.IsNullOrWhiteSpace(txt) Or aShapes.IsFont Then txt = "AutoCAD-86 shapes 1.1"
                If aShapes.IsFont Then txt += "(" & aShapes.Name & " As Individual Shapes" & ")"
                fout.WriteLine(";")
                fout.WriteLine($";{txt}")
                fout.WriteLine(";")
                If Not bNoInfo Then
                    If aShapes.Ascent <> 0 Or aShapes.Descent <> 0 Or aShapes.Mode <> 0 Then
                        fout.WriteLine("")
                        txt = $"*0,4,{ aShapes.Name }"
                        If Not String.IsNullOrWhiteSpace(aShapes.Comment) Then txt = $"{txt} { aShapes.Comment.Trim()}"
                        fout.WriteLine(txt)
                        fout.WriteLine($"{aShapes.Ascent },{ aShapes.Descent },{ aShapes.Mode },0")
                        fout.WriteLine("")
                    End If
                End If
                For i As Integer = 1 To aShapes.Count
                    Dim aMem As TSHAPE = aShapes.Item(i)
                    Dim bKeep As Boolean = Not aMem.HasSubShapes And aMem.ShapeNumber <= 258

                    If bKeep Then
                        aMem.GetPathString(pthStrings)
                        txt = txt = $"*{ aMem.ShapeNumber },{ aMem.ByteCount },"
                        If aShapes.IsFont Then
                            txt = $"{txt} CHAR{aMem.ShapeNumber}"
                        Else
                            txt = $"{txt} {aMem.Name.ToUpper}"
                        End If
                        fout.WriteLine(txt)
                        For j As Integer = 0 To pthStrings.Count - 1
                            txt = pthStrings.Item(j)
                            fout.WriteLine(txt)
                            'Application.DoEvents()
                        Next j
                        If bWriteByteString Then fout.WriteLine(";" & dxfBytes.CommaString(aMem.PathBytes))
                        fout.WriteLine("")
                    End If
                    'Application.DoEvents()
                Next i
                fout.Close()
                fout = Nothing
            Catch ex As Exception
                rErrorString = ex.Message

                If Not bSuppressErrors Then Throw New Exception("WriteSHP_SHAPES - " & rErrorString)
            Finally
                If fout IsNot Nothing Then fout.Close()
            End Try
        End Sub

        Friend Shared Sub WriteSHP_CHAR(aShapes As TSHAPES, aFileSpec As String, aIncludeByteString As Boolean, ByRef rErrorString As String, Optional bSuppressErrors As Boolean = False)
            rErrorString = ""
            Dim fout As IO.StreamWriter = Nothing
            Try

                Dim txt As String = aShapes.Header

                Dim pthStrings As List(Of String) = Nothing

                If IO.File.Exists(aFileSpec) Then IO.File.Delete(aFileSpec)
                fout = IO.File.CreateText(aFileSpec)
                If String.IsNullOrWhiteSpace(txt) = "" Then txt = "AutoCAD-86 unifont 1.1"

                fout.WriteLine(";")
                fout.WriteLine("; " & txt)
                fout.WriteLine("; Created By dxfGraphics On " & Format(Now, "mm/dd/yyyy"))
                fout.WriteLine(";")
                fout.WriteLine("")
                fout.WriteLine("")
                txt = $"*UNIFONT,6,{ aShapes.Name }"
                If Not String.IsNullOrWhiteSpace(aShapes.Comment) Then txt = $"{txt} { aShapes.Comment.Trim()}"
                fout.WriteLine(txt)
                fout.WriteLine($"{aShapes.Ascent }, { aShapes.Descent }, { aShapes.Mode }, { aShapes.Encoding }, { aShapes.Embedding }, 0")
                fout.WriteLine("")
                For i As Integer = 1 To aShapes.Count
                    Dim aMem As TSHAPE = aShapes.Item(i)
                    aMem.GetPathString(pthStrings)
                    txt = $"*{ aMem.ShapeNumber }, {aMem.ByteCount}, {aMem.Name.ToUpper()}"
                    fout.WriteLine(txt)
                    For j As Integer = 0 To pthStrings.Count - 1
                        txt = pthStrings.Item(j)
                        fout.WriteLine(txt)
                        'Application.DoEvents()
                    Next j
                    If aIncludeByteString Then fout.WriteLine($";{dxfBytes.CommaString(aMem.PathBytes)}")
                    fout.WriteLine("")
                    'Application.DoEvents()
                Next i
                fout.Close()
                fout = Nothing
            Catch ex As Exception
                rErrorString = ex.Message

                If Not bSuppressErrors Then Throw New Exception($"WriteSHP_CHAR - { rErrorString}")
            Finally
                If fout IsNot Nothing Then fout.Close()
            End Try
        End Sub

        Friend Shared Sub DecodeSHX_SHAPES(ByRef aShapes As TSHAPES, ByRef iogBytes() As Byte, Optional bJustHeader As Boolean = False)
            '#1the subject shapes array
            '#2the file bytes to use to define the shapes
            Try
                If aShapes.FileName <> "" Then
                    aShapes.Name = IO.Path.GetFileNameWithoutExtension(aShapes.FileName)
                End If
                'two formats
                If iogBytes(0) = 0 And iogBytes(1) = 0 Then
                    TSHAPES.DecodeSHX_SHAPES_2(aShapes, iogBytes, bJustHeader)
                Else
                    TSHAPES.DecodeSHX_SHAPES_1(aShapes, iogBytes, bJustHeader)
                End If
            Catch ex As Exception
                Throw ex
            End Try
        End Sub

        Friend Shared Sub DecodeSHX_SHAPES_1(ByRef aShapes As TSHAPES, ByRef iogBytes() As Byte, Optional bJustHeader As Boolean = False)
            '#1the subject shapes array
            '#2the file bytes to use to define the shapes
            Try
                aShapes.Clear()

                Dim sBytes(0) As Byte
                Dim nBytes(0) As Byte
                Dim cShape As TSHAPE

                sBytes = dxfBytes.GetFirst(iogBytes, 6)
                Dim cnt As Integer = TVALUES.To_INT(sBytes(4))
                'four byte chunks contain shape numbers and byte counts
                For i As Integer = 0 To cnt - 1
                    sBytes = dxfBytes.GetFirst(iogBytes, 4)
                    cShape = New TSHAPE("") With
                {
                .ShapeNumber = sBytes(1) * 256 + sBytes(0),
                .ByteCount = sBytes(3) * 256 + sBytes(2)
                }
                    cShape.Name = cShape.ShapeNumber.ToString
                    aShapes.Add(cShape)

                Next i
                For i As Integer = 0 To cnt - 1
                    sBytes = dxfBytes.GetFirst(iogBytes, cShape.ByteCount)
                    Dim j As Integer = dxfBytes.GetIndex(sBytes, 0)
                    nBytes = dxfBytes.GetFirst(sBytes, j + 1)
                    cShape = New TSHAPE("") With {.PathBytes = sBytes}

                    cShape.Name = dxfBytes.ToAscii(nBytes, bRegularCharsOnly:=True)
                    cShape.UpdatePathCommands()
                    aShapes.Add(cShape)

                Next i
            Catch ex As Exception
                Throw ex
            End Try
        End Sub
        Friend Shared Sub DecodeSHX_SHAPES_2(ByRef aShapes As TSHAPES, ByRef iogBytes() As Byte, Optional bJustHeader As Boolean = False)
            '#1the subject shapes array
            '#2the file bytes to use to define the shapes
            Try
                Dim i As Integer
                Dim j As Integer
                Dim k As Integer
                Dim cnt As Integer
                Dim CD1 As Byte
                Dim sBytes(0) As Byte
                Dim pBytes(0) As Byte
                Dim cShape As TSHAPE
                aShapes.Clear()
                'the first 10 are information like count and string lengths
                sBytes = dxfBytes.GetFirst(iogBytes, 10)
                cnt = sBytes(5) * 255 + sBytes(4) - 1
                If cnt <= 0 Then Return
                CD1 = sBytes(8)
                'four bytes for each member contain shape numbers and byte counts
                sBytes = dxfBytes.GetFirst(iogBytes, 4 * cnt)
                j = 0
                For i = 0 To 4 * cnt - 1 Step 4
                    cShape = New TSHAPE("") With {.ShapeNumber = sBytes(i + 1) * 256 + sBytes(i),
            .ByteCount = sBytes(i + 3) * 256 + sBytes(i + 2),
            .Name = cShape.ShapeNumber.ToString}
                    j += 1
                    aShapes.Add(cShape)
                    'Application.DoEvents()
                Next i
                'the name follows
                sBytes = dxfBytes.GetFirst(iogBytes, CD1)
                aShapes.Name = dxfBytes.ToAscii(sBytes, TVALUES.To_INT(CD1) - 4, True)
                aShapes.Comment = ""
                i = aShapes.Name.IndexOf(" ") + 1
                If aShapes.Name.Contains(" ") Then
                    aShapes.Comment = aShapes.Name
                    aShapes.Name = dxfUtils.LeftOf(aShapes.Name, " ")
                    aShapes.Comment = dxfUtils.RightOf(aShapes.Comment, " ")
                End If
                aShapes.Ascent = sBytes(CD1 - 4)
                aShapes.Descent = sBytes(CD1 - 3)
                aShapes.Mode = sBytes(CD1 - 2)
                aShapes.Encoding = sBytes(CD1 - 1)
                If bJustHeader Then Return
                'now get the path bytes
                j = 0
                For i = 0 To 4 * cnt - 1 Step 4
                    cShape = New TSHAPE("")
                    sBytes = dxfBytes.GetFirst(iogBytes, cShape.ByteCount)
                    pBytes = dxfBytes.RemoveToByte(sBytes, 0, aReturnLead:=True, aOccur:=0, rRemoved:=k)
                    cShape.Name = dxfBytes.ToAscii(pBytes, bRegularCharsOnly:=True)
                    cShape.ByteCount = sBytes.Length
                    cShape.PathBytes = sBytes
                    cShape.UpdateCommands()
                    aShapes.Add(cShape)
                    j += 1
                    'Application.DoEvents()
                Next i
            Catch ex As Exception
                Throw ex
            End Try
        End Sub
        Friend Shared Function ReadSHX(aFileName As String, bSuppressErrors As Boolean, ByRef rErrorString As String, Optional bJustHeaders As Boolean = False) As TSHAPES
            Try
                Dim aBytes(0) As Byte
                Dim s As IO.Stream = IO.File.Open(aFileName, IO.FileMode.Open)
                Dim aReader As New IO.BinaryReader(s)
                aBytes = aReader.ReadBytes(s.Length)
                aReader.Close()
                aReader.Dispose()
                s.Dispose()
                Return TSHAPES.ReadSHX(aBytes, aFileName, bSuppressErrors, rErrorString, bJustHeaders)
            Catch ex As Exception
                If Not bSuppressErrors Then Throw ex
                Return Nothing
            End Try
        End Function
        Friend Shared Function ReadSHX(Bytes() As Byte, aFileName As String, bSuppressErrors As Boolean, ByRef rErrorString As String, Optional bJustHeaders As Boolean = False) As TSHAPES
            Dim rShps As TSHAPES = TSHAPES.Null
            Dim fBytes(0) As Byte
            Dim idx As Long
            Try
                rErrorString = ""
                'read til code 26 is found this is where bytes start defining shapes
                'before this code is the file header
                'find the first linefeed
                'the 26 byte indicates the end of the header section
                idx = dxfBytes.GetIndex(Bytes, 26, 1)
                If idx < 0 Then Throw New Exception("Invalid File Format Detected")
                fBytes = dxfBytes.GetFirst(Bytes, idx + 1)
                rShps.Header = dxfBytes.ToAscii(fBytes, bRegularCharsOnly:=True).Substring(0, idx)
                'the word autocad indicates the file is an autocad shape
                If Not rShps.Header.Contains("AutoCAD", StringComparer.OrdinalIgnoreCase) Then
                    rErrorString = "Invalid File Header Detected '" & rShps.Header & "'"
                    If Not bSuppressErrors Then Throw New Exception(rErrorString)
                End If
                rShps.IsFont = rShps.Header.Contains("font", StringComparer.OrdinalIgnoreCase)
                If rShps.IsFont And rErrorString = "" Then
                    If rShps.Header.Contains("unifont", StringComparer.OrdinalIgnoreCase) Then
                        TSHAPES.DecodeSHX_CHARS(rShps, Bytes, bJustHeaders)
                    ElseIf rShps.Header.Contains("Bigfont", StringComparer.OrdinalIgnoreCase) Then
                        rErrorString = $"{rShps.Header } Cannot Currently Be Decode. Only Unifonts Can Be Decoded"
                        'DecodeSHX_CHARS_BIG rShps, Bytes
                    Else
                        rErrorString = $"{rShps.Header} Cannot Currently Be Decode. Only Unifonts Can Be Decoded"
                    End If
                Else
                    'the file contains generic shapes
                    TSHAPES.DecodeSHX_SHAPES(rShps, Bytes, bJustHeaders)
                End If
                If rErrorString <> "" Then
                    If Not bSuppressErrors Then Throw New Exception(rErrorString)
                End If
                rShps.FileName = aFileName
                Return rShps
            Catch ex As Exception
                rErrorString = ex.Message
                If Not bSuppressErrors Then Throw ex
                Return Nothing
            Finally
            End Try
        End Function
        Friend Shared Function ReadSHP(aFileName As String, bSuppressErrors As Boolean, ByRef rErrorString As String, Optional bHeaderOnly As Boolean = False) As TSHAPES
            Try
                Return TSHAPES.ReadSHP(IO.File.OpenText(aFileName), aFileName, bSuppressErrors, rErrorString, bHeaderOnly)
            Catch ex As Exception
                If Not bSuppressErrors Then Throw ex
                Return Nothing
            End Try
        End Function
        Friend Shared Function ReadSHP(aReader As IO.StreamReader, aFileName As String, bSuppressErrors As Boolean, ByRef rErrorString As String, Optional bHeaderOnly As Boolean = False) As TSHAPES
            Dim txt As String
            Dim cmds As String
            Dim i As Integer
            Dim j As Integer
            Dim ub As Integer
            Dim fText As New List(Of String)
            Dim sVals() As String
            Dim cnt As Long
            Dim cShape As TSHAPE = Nothing
            Dim bKeep As Boolean
            Dim snum As Integer
            Dim bFont As Boolean
            Dim blen As Integer
            Dim fld As String = String.Empty
            Dim snumtxt As String = String.Empty
            Dim pBytes(0) As Byte
            Dim sErr As String = String.Empty
            Dim bSubShapes As Boolean
            Try
                If aReader Is Nothing Then Throw New Exception("Null Reader Passed")
                Dim rShps As New TSHAPES
                cShape = New TSHAPE("")
                rShps.FileName = aFileName.Trim()
                If rShps.FileName <> "" Then
                    rShps.Name = IO.Path.GetFileNameWithoutExtension(rShps.FileName)
                End If

                Do While Not aReader.EndOfStream
                    txt = aReader.ReadLine.Trim()
                    If String.IsNullOrWhiteSpace(txt) Then Continue Do

                    i = txt.IndexOf(";") + 1
                    If i > 0 Then
                        If rShps.Header = "" Then
                            If txt.Contains("AutoCAD", StringComparer.OrdinalIgnoreCase) Then
                                If txt.Contains("shapes", StringComparer.OrdinalIgnoreCase) Then
                                    rShps.Header = "AutoCAD-86 shapes 1.1"
                                ElseIf txt.Contains("unifont", StringComparer.OrdinalIgnoreCase) Then
                                    rShps.Header = "AutoCAD-86 unifont 1.0"
                                    rShps.IsFont = True
                                    bFont = True
                                End If
                            End If
                        End If
                        If i = 1 Then
                            txt = ""
                        Else
                            txt = dxfUtils.LeftOf(txt, ";")
                        End If
                    End If

                    txt = txt.Trim()
                    If txt <> "" Then
                        fText.Add(txt)
                        If txt.StartsWith("*") Then
                            cnt += 1
                        End If
                    End If
                Loop
                aReader.Close()
                If cnt <= 0 Then Throw New Exception("Invalid Shape File Detected")
                'loop on the file lines
                cnt = -1
                For i = 1 To fText.Count
                    txt = ""
                    cmds = ""
                    If fText.Item(i - 1).StartsWith("*") Then
                        txt = fText.Item(i - 1)
                        cnt += 1
                        For j = i + 1 To fText.Count
                            If Not fText.Item(j - 1).StartsWith("*") Then
                                cmds += fText.Item(j - 1).ToUpper()
                                i = j
                            Else
                                i = j - 1
                                Exit For
                            End If
                            'Application.DoEvents()
                        Next j
                    End If

                    If txt <> "" And cmds.Contains(",") And cnt = 0 Then
                        sVals = cmds.Split(",")
                        ub = sVals.Length - 1
                        rShps.Ascent = TVALUES.ToByte(sVals(0))
                        If ub >= 1 Then rShps.Descent = TVALUES.ToByte(sVals(1))
                        If ub >= 2 Then rShps.Mode = TVALUES.ToByte(sVals(2))
                        If ub >= 3 Then rShps.Encoding = TVALUES.ToByte(sVals(3))
                        If ub >= 4 Then rShps.Embedding = TVALUES.ToByte(sVals(4))
                        rShps.Header = "AutoCAD-86 shapes 1.1"
                        If bHeaderOnly Then
                            rShps = rShps
                            Return rShps
                        End If
                    End If
                    bKeep = True
                    If Not txt.Contains(",") Then bKeep = False
                    If Not cmds.EndsWith(",0") Then bKeep = False
                    If Not bKeep Then Continue For
                    ReDim sVals(0)
                    snum = 0
                    blen = 0
                    snumtxt = ""
                    sVals = txt.Split(",")
                    ub = sVals.Length - 1
                    snumtxt = sVals(0)
                    snumtxt = snumtxt.Substring(1, snumtxt.Length - 1).Trim()
                    snum = -1
                    If snumtxt = "" Then
                        bKeep = False
                        Continue For
                    End If
                    If Not bKeep Then Continue For

                    If fld.StartsWith("0") Then
                        snum = TVALUES.HexToInteger(fld)
                    Else
                        If TVALUES.IsNumber(snumtxt) Then
                            snum = TVALUES.To_INT(snumtxt)
                        End If
                    End If
                    If snum <= 0 Then
                        sVals = cmds.Split(",")
                        If String.Compare(snumtxt, "UNIFONT", True) = 0 Then
                            rShps.Header = "AutoCAD-86 unifont 1.0"
                            rShps.IsFont = True
                            bFont = True
                        Else
                            rShps.Header = "AutoCAD-86 shapes 1.1"
                        End If
                        ub = sVals.Length - 1
                        rShps.Ascent = TVALUES.ToByte(sVals(0))
                        If ub >= 1 Then rShps.Descent = TVALUES.ToByte(sVals(1))
                        If ub >= 2 Then rShps.Mode = TVALUES.ToByte(sVals(2))
                        If ub >= 3 Then rShps.Encoding = TVALUES.ToByte(sVals(3))
                        If ub >= 4 Then rShps.Embedding = TVALUES.ToByte(sVals(4))
                        If bHeaderOnly Then
                            rShps = rShps
                            Return rShps
                        End If
                    Else
                        If snum < 1 Or (Not bFont And snum > 258) Or (bFont And snum > 32768) Then
                            bKeep = False
                        End If
                        If bKeep Then
                            fld = sVals(1).Trim()
                            If fld = "" Or fld = "0" Then bKeep = False
                        End If
                        If bKeep Then
                            If TVALUES.IsNumber(fld) Then blen = TVALUES.To_INT(fld)
                            If blen <= 0 Then bKeep = False
                        End If
                        If bKeep Then
                            'good two lines so add the shape
                            cShape = New TSHAPE("") With {
                                        .ShapeNumber = snum,
                                        .ByteCount = blen,
                                        .Ascent = rShps.Ascent,
                                        .Descent = rShps.Descent,
                                        .FileName = rShps.FileName,
                                        .GroupName = rShps.Name
                                        }
                            If ub >= 2 Then cShape.Name = sVals(2).Trim
                            If cShape.Name = "" Then
                                If bFont Then
                                    Select Case cShape.ShapeNumber
                                        Case 10
                                            cShape.Name = "lf"
                                        Case 32
                                            cShape.Name = "spc"
                                        Case 186
                                            cShape.Name = "Degree_Sign"
                                        Case 177
                                            cShape.Name = "Plus_Or_Minus_Sign"
                                        Case 248
                                            cShape.Name = "Diameter_Symbol"
                                        Case Else
                                            cShape.Name = ChrW(cShape.ShapeNumber)
                                    End Select
                                Else
                                    cShape.Name = snumtxt
                                End If
                            Else
                                cShape.Name = snumtxt
                            End If
                            cmds = cmds.Replace("+", "")
                            cShape.PathCommands = TVALUES.FromList(cmds)
                            cShape.ByteCount = cShape.PathCommands.Count
                            cShape.PathString = cmds
                            cShape.Commands = TSHAPECOMMANDS.CommandStringsToCommands(cShape.PathCommands, bSubShapes, sErr)
                            If sErr <> "" Then
                                System.Diagnostics.Debug.WriteLine(sErr)
                                bKeep = False
                            End If
                            cShape.HasSubShapes = bSubShapes
                            cShape.PathBytes = pBytes
                            If bKeep Then
                                rShps.Add(cShape)
                            End If
                        End If
                    End If

                    'Application.DoEvents()
                Next i
                If rShps.Header = "" Then
                    rErrorString = "Invalid Shape File Detected. Comments Must contain AutoCAD and unifont or shapes "
                End If
                bFont = rShps.Header.ToLower().IndexOf("font") >= 0
                If rShps.Count <= 0 Then
                    rErrorString = "No Shapes Read From File"
                End If
                If rErrorString <> "" Then
                    If Not bSuppressErrors Then Throw New Exception(rErrorString)
                End If
                Return rShps
            Catch ex As Exception
                rErrorString = ex.Message
                If Not bSuppressErrors Then Throw New Exception(rErrorString)
                Return Nothing
            Finally
                If aReader IsNot Nothing Then
                    aReader.Close()
                    aReader.Dispose()
                End If
            End Try
        End Function
        Public Shared Function ArcByOctant(aCmd As TSHAPECOMMAND, aSubCmd As TSUBCOMMAND, aSP As TPOINT, Optional aMultiplier As Double = 0.0, Optional bPenDown As Boolean = False) As TARC
            If aMultiplier <= 0 Then aMultiplier = 1
            '#1the first vector of the arc
            '#2the radius of the arc
            '#3the starting octant angle (0-7)
            '#4the ending octant angle (0-7)
            '#5flag to return an inverted arc
            '^returns a dxeArc defined using the passed octant info
            '~no errors raised if the arc can't be defined.
            '~the first vector is assumed as the arc start vector.
            Dim _rVal As TARC
            Try
                Dim aPlane As New TPLANE("")
                Dim aDir As TVECTOR
                Dim sa As Double
                Dim ea As Double
                Dim rad As Double
                Dim so As Integer
                Dim octspn As Integer
                Dim cw As Boolean
                Dim aspn As Double
                Dim dif As Double
                Dim ang1 As Double
                Dim ang2 As Double
                cw = aSubCmd.Flag
                aSubCmd.Value1 = Math.Round(aSubCmd.Value1, 0)
                aSubCmd.Value2 = Math.Round(aSubCmd.Value2, 0)
                aSubCmd.Value3 = Math.Round(aSubCmd.Value3, 0)
                aSubCmd.Value4 = Math.Round(aSubCmd.Value4, 0)
                aSubCmd.Value5 = Math.Round(aSubCmd.Value5, 0)
                aSubCmd.Value6 = Math.Round(aSubCmd.Value6, 0)
                If aCmd.Type = dxxShapeCommands.FractionalArc Then
                    ang1 = aSubCmd.Value5
                    ang2 = aSubCmd.Value6
                    rad = aSubCmd.Value3 * 256
                    rad = (rad + aSubCmd.Value4) * aMultiplier
                Else
                    ang1 = aSubCmd.Value2
                    ang2 = aSubCmd.Value3
                    rad = aSubCmd.Value1 * aMultiplier
                End If
                If ang1 Mod (360 = 0) Then so = 0 Else so = TVALUES.To_INT(ang1 / 45)
                If ang2 Mod (360 = 0) Then octspn = 8 Else octspn = TVALUES.To_INT(ang2 / 45)
                Do While so > 7
                    so -= 7
                Loop
                Do While octspn > 8
                    octspn -= 8
                Loop
                sa = (so * 45)
                aspn = octspn * 45
                If Not cw Then
                    ea = sa + aspn
                Else
                    ea = sa - aspn
                End If
                If aCmd.Type = dxxShapeCommands.FractionalArc And (aSubCmd.Value1 <> 0 Or aSubCmd.Value2 <> 0) Then
                    If aSubCmd.Value1 <> 0 Then
                        dif = Math.Round((45 * aSubCmd.Value1) / 255, 0)
                        If cw Then dif = -dif
                        sa += dif
                    End If
                    If aSubCmd.Value2 <> 0 Then
                        dif = Math.Round((aSubCmd.Value2 * 45) / 255, 0)
                        dif = 45 - dif
                        If Not cw Then dif = -dif
                        ea += dif
                    End If
                    aspn = dxfMath.SpannedAngle(cw, sa, ea)
                End If
                aDir = aPlane.XDirection.RotatedAbout(aPlane.ZDirection, sa, False).Normalized
                aPlane.Origin = New TVECTOR(aSP) + (aDir * -rad)
                _rVal = New TARC With {
                        .Plane = aPlane,
                        .ClockWise = cw,
                        .Radius = rad,
                        .StartAngle = sa,
                        .EndAngle = ea
                }
            Catch ex As Exception
                Throw New Exception($"shp_ArcByOctant - {ex.Message}")
            End Try
            Return _rVal
        End Function
        Public Shared Function ArcFromCommand(aCommand As TSHAPECOMMAND, aStartPt As TPOINT, aIndex As Integer, aMultiplier As Double, ByRef rPathPoints As TPOINTS, ByRef rEndPt As TPOINT, Optional bPenDown As Boolean = False) As TARC
            Dim rAddLine As Boolean = False
            Return ArcFromCommand(aCommand, aStartPt, aIndex, aMultiplier, rPathPoints, rEndPt, bPenDown, rAddLine)
        End Function
        Public Shared Function ArcFromCommand(aCommand As TSHAPECOMMAND, aStartPt As TPOINT, aIndex As Integer, aMultiplier As Double, ByRef rPathPoints As TPOINTS, ByRef rEndPt As TPOINT, bPenDown As Boolean, ByRef rAddLine As Boolean) As TARC
            Dim _rVal As New TARC
            Dim sCmd As New TSUBCOMMAND
            Dim aArc As New TARC
            Dim dX As Double
            Dim dY As Double
            Dim blg As Double
            Dim wplane As TPLANE = TPLANE.World
            rAddLine = False
            rEndPt = aStartPt
            rPathPoints = New TPOINTS(0)
            If aMultiplier <= 0 Then aMultiplier = 1
            If aIndex = 0 Then aIndex = 1
            If aIndex < 1 Or aIndex > aCommand.SubCommands.Count Then Return _rVal
            sCmd = aCommand.SubCommands.Item(aIndex)
            Select Case aCommand.Type
                Case dxxShapeCommands.FractionalArc
                    aArc = TSHAPES.ArcByOctant(aCommand, sCmd, aStartPt, aMultiplier, bPenDown)
                Case dxxShapeCommands.OctantArc
                    aArc = TSHAPES.ArcByOctant(aCommand, sCmd, aStartPt, aMultiplier, bPenDown)
                Case dxxShapeCommands.BulgeArc, dxxShapeCommands.MultiBulgeArc
                    dX = sCmd.Value1 * aMultiplier
                    dY = sCmd.Value2 * aMultiplier
                    blg = sCmd.Value3
                    rEndPt.X = aStartPt.X + dX
                    rEndPt.Y = aStartPt.Y + dY
                    If blg = 0 Or (dX = 0 And dY = 0) Then
                        rAddLine = True
                    Else
                        aArc = TARC.ByBulge(aStartPt, rEndPt, blg, True, wplane)
                    End If
            End Select
            If aArc.Radius > 0 Then
                rPathPoints = TBEZIER.ArcPathSimple(aArc, bPenDown)
                If aCommand.Type <> dxxShapeCommands.BulgeArc And aCommand.Type <> dxxShapeCommands.MultiBulgeArc Then
                    rEndPt = New TPOINT(aArc.Plane.PolarVector(aArc.Radius, aArc.EndAngle, False))
                End If
            End If
            _rVal = aArc
            Return _rVal
        End Function
        Public Shared Function BytesToInteger(aByte As Byte, bByte As Byte, ByRef rHexString As String) As Integer
            Dim _rVal As Integer
            Dim txt1 As String = String.Empty
            Dim txt2 As String = String.Empty
            If bByte > 0 Then
                txt1 = Hex(bByte)
                If txt1.Length < 2 Then
                    Do While txt1.Length < 2
                        txt1 = $"0{txt1}"
                    Loop
                Else
                    txt1 = $"0{ txt1}"
                End If
                txt2 = Hex(aByte)
                Do While txt2.Length < 2
                    txt2 = $"0{txt2}"
                Loop
            Else
                txt1 = aByte
            End If
            rHexString = $"{txt1}{txt2}"
            If txt2 <> "" Then
                _rVal = TVALUES.HexToInteger(rHexString)
            Else
                _rVal = TVALUES.To_INT(aByte)
            End If
            Return _rVal
        End Function
        Public Shared Sub DecodeSHX_CHARS(ByRef ioShapes As TSHAPES, ByRef iogBytes() As Byte, Optional bJustHeaders As Boolean = False)
            '#1the subject shapes array
            '#2the file bytes to use to define the shapes
            ioShapes.Clear()
            Dim i As Integer
            Dim j As Integer
            Dim txt1 As String = String.Empty
            Dim remains As Integer
            Dim sBytes(0) As Byte
            Dim aBytes() As Byte = iogBytes
            Dim pBytes(0) As Byte
            Dim nShape As New TSHAPE("")
            Dim cShape As New TSHAPE("")
            Dim lShape As New TSHAPE("")
            Dim bKeep As Boolean
            Dim bMultiCount As Boolean
            Try
                If ioShapes.FileName <> "" Then
                    ioShapes.Name = IO.Path.GetFileName(ioShapes.FileName)

                    If ioShapes.Name.Contains(".") Then ioShapes.Name = dxfUtils.LeftOf(ioShapes.Name, ".", bFromEnd:=True)
                End If

                'strip the first 6
                sBytes = dxfBytes.GetFirst(aBytes, 6)
                'get the length of the font name
                i = sBytes(4)
                sBytes = dxfBytes.GetFirst(aBytes, i - 6)
                ioShapes.Name = dxfBytes.ToAscii(sBytes, bRegularCharsOnly:=True).Trim()
                ioShapes.Comment = ""

                If ioShapes.Name.Contains(" ") Then
                    ioShapes.Comment = ioShapes.Name
                    ioShapes.Name = dxfUtils.LeftOf(ioShapes.Name, " ")
                    ioShapes.Comment = dxfUtils.RightOf(ioShapes.Comment, " ")
                End If
                'strip the first 6
                sBytes = dxfBytes.GetFirst(aBytes, 6)
                ioShapes.Ascent = sBytes(0)
                ioShapes.Descent = sBytes(1)
                ioShapes.Mode = sBytes(2)
                ioShapes.Encoding = sBytes(3)
                ioShapes.Embedding = sBytes(4)
                'these values must be valid!
                If ioShapes.Ascent <= 0 Or ioShapes.Descent <= 0 Then Throw New Exception("Invalid Shapes Detected. Ascent or Descent Not Defined")
                If bJustHeaders Then Return
                remains = aBytes.Length
                Do While remains > 0
                    bKeep = True
                    cShape = nShape
                    cShape.GroupName = ioShapes.Name
                    cShape.FileName = ioShapes.FileName
                    sBytes = dxfBytes.GetFirst(aBytes, 2, False, remains)
                    cShape.ShapeNumber = TSHAPES.BytesToInteger(sBytes(0), sBytes(1), txt1)
                    If cShape.ShapeNumber <= 255 Then
                        If cShape.ShapeNumber = 10 Then
                            cShape.Name = "lf"
                        ElseIf cShape.ShapeNumber = 32 Then
                            cShape.Name = "spc"
                        ElseIf cShape.ShapeNumber = 186 Then
                            cShape.Name = "Degree_Sign"
                        ElseIf cShape.ShapeNumber = 177 Then
                            cShape.Name = "Plus_Or_Minus_Sign"
                        ElseIf cShape.ShapeNumber = 248 Then
                            cShape.Name = "Diameter_Symbol"
                        Else
                            cShape.Name = ChrW(cShape.ShapeNumber)
                        End If
                    Else
                        cShape.Name = "0" & txt1
                    End If
                    'get the nuber of bytes for the chrrent shape
                    sBytes = dxfBytes.GetFirst(aBytes, 2, bDontRemove:=False, rRemains:=remains)
                    cShape.ByteCount = sBytes(0)
                    bMultiCount = False
                    Do While sBytes(1) <> 0
                        bMultiCount = True
                        sBytes = dxfBytes.GetFirst(aBytes, 2, False, remains)
                        cShape.ByteCount += sBytes(0)
                    Loop
                    'Debug.Print "Bytes - " & cShape.ByteCount
                    ReDim pBytes(0)
                    If cShape.ByteCount <> 0 Then
                        If bMultiCount Then
                            dxfBytes.RemoveLeadBytes(aBytes, 0, j)
                            i = dxfBytes.FindEndOfPathCommands(aBytes)
                            pBytes = dxfBytes.GetFirst(aBytes, i + 1, False, remains)
                        Else
                            'get the shape path bytes
                            pBytes = dxfBytes.GetFirst(aBytes, cShape.ByteCount, False, remains)
                            'remove any leading zeros
                            If pBytes(0) = 0 Then
                                dxfBytes.RemoveLeadBytes(pBytes, 0, j)
                                cShape.ByteCount -= j
                            End If
                            If ioShapes.IsFont Then
                                If pBytes(0) > 14 Then
                                    sBytes = dxfBytes.RemoveToLeadBytes(pBytes, "1,2,3,4,5,6,7,8,9,10,11,12,13,14", j)
                                    cShape.ByteCount -= j
                                End If
                            End If
                        End If
                    End If
                    cShape.ByteCount = pBytes.Length
                    cShape.PathBytes = pBytes
                    cShape.Descent = ioShapes.Descent
                    cShape.Ascent = ioShapes.Ascent
                    If bKeep Then
                        cShape.UpdatePathCommands()
                        ioShapes.Add(cShape)
                        'Debug.Print shp_UnicodeName(cShape) & " = " & shp_PathString(cShape)
                    End If
                    lShape = cShape
                Loop
                Return
            Catch ex As Exception
                Throw ex
                Return
            End Try
        End Sub
        Public Shared Function ComputePath(ByRef ioShapes As TSHAPES, ByRef ioShape As TSHAPE, ByRef rExtents As TPOINTS, ByRef rLimits As TLIMITS, Optional aScaleFactor As Double = 0.0, Optional bRegenCommands As Boolean = False, Optional bIncludeMoveToPoint As Boolean = False, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0) As TSEGMENTS
            '#1 the shape array the shape is a member of
            '#2 the shape
            '#3 returns the extent points for the shape which are the path points
            '#4 returns the limits of the path points (left,right,top bottom)
            '#5 a scale factor to apply
            If aScaleFactor <= 0 Then aScaleFactor = 1
            rLimits = New TLIMITS(New TPOINT(aXOffset, aYOffset))
            rExtents = New TPOINTS(0)
            '^computes the un-transform path of the passed shape based character
            Dim P1 As New TPOINT(aXOffset, aYOffset)
            Dim P2 As New TPOINT(aXOffset, aYOffset)
            Dim v1 As TVECTOR
            Dim arcPts As TPOINTS = Nothing
            Dim aArc As TARC
            Dim aCmd As TSHAPECOMMAND
            Dim sCmd As TSUBCOMMAND
            Dim bShape As TSHAPE
            Dim bExtents As TPOINTS = Nothing
            Dim bLims As TLIMITS = Nothing
            Dim aPth As New TPOINTS(0)
            Dim rSegs As New TSEGMENTS(0)
            Dim bSkipping As Boolean = False
            Dim sMultBy As Double = 1
            Dim bAddLine As Boolean
            Try
                ioShape.UpdateCommands(bRegenCommands)
                ioShape.PointStack = New TPOINTS(0)
                ioShape.PenDown = False
                If bIncludeMoveToPoint Then aPth.Add(P1, dxxVertexStyles.MOVETO)
                For i As Integer = 1 To ioShape.Commands.Count
                    'If aPth.Count = 22 Then
                    '    Beep()
                    'End If
                    aCmd = ioShape.Commands.Item(i)
                    sCmd = aCmd.SubCommands.Item(1, True)
                    Select Case aCmd.Type
                            '------------------------------------------------------------------
                        Case dxxShapeCommands.EndShape '= 0
                            '------------------------------------------------------------------
                            Exit For
                            '------------------------------------------------------------------
                        Case dxxShapeCommands.PenDown '= 1
                            '------------------------------------------------------------------
                            ioShape.PenDown = True
                                '------------------------------------------------------------------
                        Case dxxShapeCommands.PenUp '= 2
                            '------------------------------------------------------------------
                            ioShape.PenDown = False
                            '------------------------------------------------------------------
                        Case dxxShapeCommands.DivideBy '= 3
                            '------------------------------------------------------------------
                            If aCmd.Value <> 0 Then sMultBy /= TVALUES.To_DBL(aCmd.Value)
                            '------------------------------------------------------------------
                        Case dxxShapeCommands.MultiplyBy '= 4
                            '------------------------------------------------------------------
                            If aCmd.Value <> 0 Then sMultBy *= TVALUES.To_DBL(aCmd.Value)
                            '------------------------------------------------------------------
                        Case dxxShapeCommands.PushToStack '= 5
                            '------------------------------------------------------------------
                            ioShape.PointStack.Add(P1)
                            '------------------------------------------------------------------
                        Case dxxShapeCommands.PopFromStack '= 6
                            '------------------------------------------------------------------
                            P1 = ioShape.PointStack.LastMember(True)
                            '------------------------------------------------------------------
                        Case dxxShapeCommands.SubShape '= 7
                            '------------------------------------------------------------------
                            'get the sub shape ???
                            Dim shpid As Integer = 0
                            bShape = ioShapes.GetByShapeNumber(TVALUES.ToInteger(aCmd.Value), shpid)
                            If shpid <= 0 Then bShape = ioShapes.GetByShapeNumber(TVALUES.HexToInteger(aCmd.Value), shpid)
                            If shpid > 0 Then
                                rSegs.Append(TSHAPES.ComputePath(ioShapes, bShape, bExtents, bLims, 1, bRegenCommands, aXOffset:=P1.X, aYOffset:=P1.Y))
                                If bShape.Path.Count > 0 Then
                                    aPth.Append(bShape.Path)
                                    rExtents.Append(bExtents)
                                    rLimits.Update(bLims)
                                    P1 = New TPOINT(bLims.Right, 0)
                                End If
                            End If
                            '------------------------------------------------------------------
                        Case dxxShapeCommands.XYDisplacement '= 8
                            '------------------------------------------------------------------
                            P2.X = P1.X + (sCmd.Value1 * sMultBy)
                            P2.Y = P1.Y + (sCmd.Value2 * sMultBy)
                            rLimits.Update(P2)
                            aPth.AddLine(P1, P2, True, 3, bJustMoveTo:=Not ioShape.PenDown)


                            If ioShape.PenDown Then
                                rSegs.Add(P1, P2)
                                rExtents.AddLine(P1, P2, True, 3)
                            End If
                            P1 = P2.Clone
                            '------------------------------------------------------------------
                        Case dxxShapeCommands.MultiXYDisplacement '=9
                            '------------------------------------------------------------------
                            For icmd As Integer = 1 To aCmd.SubCommands.Count
                                sCmd = aCmd.SubCommands.Item(icmd)
                                P2.X = P1.X + (sCmd.Value1 * sMultBy)
                                P2.Y = P1.Y + (sCmd.Value2 * sMultBy)
                                rLimits.Update(P2)
                                aPth.AddLine(P1, P2, True, 3, bJustMoveTo:=Not ioShape.PenDown)

                                If ioShape.PenDown Then
                                    rSegs.Add(P1, P2)
                                    rExtents.AddLine(P1, P2, True, 3)
                                End If
                                P1 = P2.Clone
                                'Application.DoEvents()
                            Next icmd
                            '------------------------------------------------------------------
                        Case dxxShapeCommands.OctantArc '=10
                            '------------------------------------------------------------------
                            aArc = TSHAPES.ArcFromCommand(aCmd, P1, 1, sMultBy, arcPts, P2, ioShape.PenDown)
                            aPth.Append(arcPts)
                            If ioShape.PenDown Then

                                rSegs.Add(aArc)
                                arcPts = aArc.PhantomPts(20)
                                rExtents.Append(arcPts)
                                rLimits.Update(arcPts)
                            End If
                            P1 = P2.Clone
                            '------------------------------------------------------------------
                        Case dxxShapeCommands.FractionalArc '= 11
                            '------------------------------------------------------------------
                            aArc = TSHAPES.ArcFromCommand(aCmd, P1, 1, sMultBy, arcPts, P2, ioShape.PenDown)
                            aPth.Append(arcPts)
                            If ioShape.PenDown Then

                                arcPts = aArc.PhantomPts(20)
                                rExtents.Append(arcPts)
                                rLimits.Update(arcPts)
                                rSegs.Add(aArc)
                            End If
                            P1 = P2.Clone
                            '------------------------------------------------------------------
                        Case dxxShapeCommands.BulgeArc '= 12
                            '------------------------------------------------------------------
                            aArc = TSHAPES.ArcFromCommand(aCmd, P1, 1, sMultBy, arcPts, P2, ioShape.PenDown, bAddLine)
                            If Not bAddLine Then
                                aPth.Append(arcPts)
                            Else
                                aPth.AddLine(P1, P2, True, bJustMoveTo:=Not ioShape.PenDown)
                            End If

                            If ioShape.PenDown Then
                                If Not bAddLine Then

                                    arcPts = aArc.PhantomPts(20)
                                    rExtents.Append(arcPts)
                                    rLimits.Update(arcPts)
                                    rSegs.Add(aArc)
                                Else

                                    rExtents.AddLine(P1, P2, True)
                                    rSegs.Add(P1, P2)
                                End If
                            End If
                            P1 = P2.Clone
                            '------------------------------------------------------------------
                        Case dxxShapeCommands.MultiBulgeArc '= 13
                            '------------------------------------------------------------------
                            For icmd As Integer = 1 To aCmd.SubCommands.Count
                                sCmd = aCmd.SubCommands.Item(icmd)
                                aArc = TSHAPES.ArcFromCommand(aCmd, P1, icmd, sMultBy, arcPts, P2, ioShape.PenDown)
                                If Not bAddLine Then
                                    aPth.Append(arcPts)
                                Else
                                    aPth.AddLine(P1, P2, True, bJustMoveTo:=Not ioShape.PenDown)
                                End If

                                If ioShape.PenDown Then
                                    If Not bAddLine Then
                                        arcPts = aArc.PhantomPts(20)
                                        rExtents.Append(arcPts)
                                        rLimits.Update(arcPts)
                                        rSegs.Add(aArc)
                                    Else
                                        rExtents.AddLine(P1, P2, True)
                                        rSegs.Add(P1, P2)
                                    End If
                                Else
                                    rLimits.Update(P2)
                                End If
                                P1 = P2.Clone
                            Next icmd
                            '------------------------------------------------------------------
                        Case dxxShapeCommands.VerticalOnly '= 14
                            '------------------------------------------------------------------
                            bSkipping = True
                                '------------------------------------------------------------------
                        Case dxxShapeCommands.VectorLengthAndDirection ' = 15
                            '------------------------------------------------------------------
                            P2 = aCmd.DecodeDisplacement(P1, sMultBy)
                            rLimits.Update(P2)
                            aPth.AddLine(P1, P2, True, 3, bJustMoveTo:=Not ioShape.PenDown)

                            If ioShape.PenDown Then

                                rExtents.AddLine(P1, P2, True, 3)
                                rSegs.Add(P1, P2)
                            End If
                            P1 = P2.Clone
                    End Select
                    If bSkipping Then
                        i += 1
                        If i + 1 > ioShape.Commands.Count - 1 Then Exit For
                    End If
                    bSkipping = False
                    'Application.DoEvents()
                Next i
                'p2 = New TPOINT(p1.X + 1, p1.Y)
                'pnts_AddLine aPth, p1, p2, True
                'pnts_AddLine rExtents, p1, p2, True
                'alss_AddLineP rSegs, p1, p2
                If aScaleFactor <> 1 Then
                    P1 = New TPOINT(0, 0)
                    rLimits.Bottom *= aScaleFactor
                    rLimits.Top *= aScaleFactor
                    rLimits.Left *= aScaleFactor
                    rLimits.Right *= aScaleFactor
                    v1 = TVECTOR.Zero
                    If rExtents.Count < 1 Then rExtents.Add(P1)
                    rExtents.Scale(aScaleFactor, Nothing)
                    aPth.Scale(aScaleFactor, Nothing)
                    TTRANSFORM.Apply(TTRANSFORM.CreateScale(v1, aScaleFactor), rSegs)

                End If
                ioShape.Path = aPth
                ioShape.PointStack = New TPOINTS(0)
                'aPth.Print()
                Return rSegs
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine($"{Reflection.MethodBase.GetCurrentMethod.Name} - { ex.Message}")
                Return rSegs
            End Try
        End Function

        Friend Shared Function FindSHXFile(aName As String, Optional bSuppressFonts As Boolean = False, Optional bLoadIfFound As Boolean = False) As TSHAPES

            Dim fldr As String
            Dim aShapes As TSHAPES = TSHAPES.Null
            Dim aErr As String = String.Empty
            Dim fname As String = String.Empty
            Dim fldrs As New TVALUES
            If String.IsNullOrWhiteSpace(aName) Then Return aShapes
            aName = aName.Trim()

            If aName.Contains("\") Then
                If IO.File.Exists(aName) Then
                    aShapes = TSHAPES.ReadFromFile(fname, aErr, bSuppressErrors:=True, bJustHeaders:=True)
                    If aErr = "" Then
                        Return aShapes
                    Else
                        aShapes.FileName = ""
                        aName = IO.Path.GetFileName(aName)
                    End If
                Else
                    aName = IO.Path.GetFileName(aName)
                End If
            End If
            Dim i As Integer = aName.LastIndexOf(".")
            If i > 0 Then aName = IO.Path.GetFileNameWithoutExtension(aName)
            aName = aName.Trim()
            If aName = "" Then Return aShapes
            aName += ".shx"
            fldr = Application.ExecutablePath
            fname = IO.Path.Combine(fldr, aName)
            If IO.File.Exists(fname) Then
                aShapes = TSHAPES.ReadFromFile(fname, aErr, True, bJustHeaders:=Not bLoadIfFound)
                If aErr = "" Then
                    Return aShapes
                Else
                    aShapes.FileName = ""
                End If
            End If
            fldr = Application.ExecutablePath & "\Fonts"
            If IO.Directory.Exists(fldr) Then
                fname = IO.Path.Combine(fldr, aName)
                If IO.File.Exists(fname) Then
                    aShapes = TSHAPES.ReadFromFile(fname, aErr, True, bJustHeaders:=Not bLoadIfFound)
                    If aErr = "" Then
                        Return aShapes
                    Else
                        aShapes.FileName = ""
                    End If
                End If
            End If
            fldrs = goACAD.SearchPaths(Not bSuppressFonts)
            For i = 1 To fldrs.Count
                fldr = fldrs.Item(i)
                fname = fldr & "\" & aName
                If IO.File.Exists(fname) Then
                    aShapes = TSHAPES.ReadFromFile(fname, aErr, True, bJustHeaders:=Not bLoadIfFound)
                    If aErr = "" Then
                        Exit For
                    Else
                        aShapes.FileName = ""
                    End If
                End If
                'Application.DoEvents()
            Next i
            Return aShapes
        End Function

        Public Shared Function ReadFromFile(aFileName As String, ByRef rError As String, Optional bSuppressErrors As Boolean = False, Optional bJustFonts As Boolean = False, Optional bJustHeaders As Boolean = False) As TSHAPES
            Dim rShapes As TSHAPES = TSHAPES.Null
            rError = String.Empty
            Try
                If String.IsNullOrWhiteSpace(aFileName) Then
                    rError = "No File Name Passed"
                    Return rShapes
                End If
                aFileName = aFileName.Trim
                Dim ext As String = IO.Path.GetExtension(aFileName)
                If IO.File.Exists(aFileName) Then
                    If String.Compare(ext, ".SHX", True) = 0 Then
                        rShapes = TSHAPES.ReadSHX(aFileName, True, rError, bJustHeaders)
                    ElseIf String.Compare(ext, ".SHP", True) = 0 Then
                        rShapes = TSHAPES.ReadSHP(aFileName, True, rError, bJustHeaders)
                    Else
                        rError = "Only Files With SHP or SHX Extensions Can Be Read"
                    End If
                Else
                    rError = "File Not Found"
                End If
                If rShapes.Ascent > 0 Then rShapes.IsFont = True
                If rError <> "" Then
                    If Not bSuppressErrors Then Throw New Exception(rError)
                End If
            Catch ex As Exception
                rError = ex.Message
                If Not bSuppressErrors Then Throw New Exception("TSHAPES.ReadFromFile : " & rError)
            End Try
            rShapes.ErrorString = rError
            Return rShapes
        End Function
#End Region 'Shared Methods
    End Structure 'TSHAPES

    Friend Structure TDISPLAYVARS
#Region "Members"
        Public Color As dxxColors
        Public DimStyle As String
        Public IsDirty As Boolean
        Public LayerName As String '"0"
        Public Linetype As String ' dxfLinetypes.ByLayer
        Public LineWeight As dxxLineWeights
        Public LTScale As Double
        Public Suppressed As Boolean
        Public TextStyle As String
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aLayer As String = "0", Optional aLineType As String = "ByLayer", Optional aColor As dxxColors = dxxColors.ByLayer, Optional aLineweight As dxxLineWeights = dxxLineWeights.ByLayer, Optional aLTScale As Double = 1, Optional aDimStyleName As String = "", Optional aTextStyleName As String = "")
            'init -----------------------------------------------------
            Color = dxxColors.Undefined
            DimStyle = String.Empty
            IsDirty = False
            LayerName = String.Empty
            Linetype = String.Empty
            LineWeight = dxxLineWeights.Undefined
            Suppressed = False
            LTScale = 1
            TextStyle = String.Empty
            'init -----------------------------------------------------
            If aLayer IsNot Nothing Then LayerName = aLayer.Trim
            If aLineType IsNot Nothing Then Linetype = aLineType.Trim
            If aColor <> dxxColors.Undefined Then Color = aColor
            If aLineweight <> dxxLineWeights.Undefined Then LineWeight = aLineweight
            LTScale = Math.Abs(aLTScale)
            If LTScale = 0 Then LTScale = 1
            If aDimStyleName IsNot Nothing Then DimStyle = aDimStyleName.Trim
            If aTextStyleName IsNot Nothing Then TextStyle = aTextStyleName.Trim
        End Sub
        Friend Sub New(aDisplay As TDISPLAYVARS)
            'init -----------------------------------------------------
            Color = aDisplay.Color
            DimStyle = aDisplay.DimStyle
            IsDirty = aDisplay.IsDirty
            LayerName = aDisplay.LayerName
            Linetype = aDisplay.Linetype
            LineWeight = aDisplay.LineWeight
            Suppressed = aDisplay.Suppressed
            LTScale = aDisplay.LTScale
            TextStyle = aDisplay.TextStyle
            'init -----------------------------------------------------

        End Sub

        Friend Sub New(aDisplay As dxfDisplaySettings)
            'init -----------------------------------------------------
            Color = dxxColors.Undefined
            DimStyle = ""
            IsDirty = False
            LayerName = ""
            Linetype = ""
            LineWeight = dxxLineWeights.Undefined
            Suppressed = False
            LTScale = 1
            TextStyle = ""
            'init -----------------------------------------------------
            If aDisplay Is Nothing Then Return
            'init -----------------------------------------------------
            Color = aDisplay.Color
            DimStyle = aDisplay.DimStyleName
            IsDirty = aDisplay.IsDirty
            LayerName = aDisplay.LayerName
            Linetype = aDisplay.Linetype
            LineWeight = aDisplay.LineWeight
            Suppressed = aDisplay.Suppressed
            LTScale = aDisplay.LTScale
            TextStyle = aDisplay.TextStyleName
            'init -----------------------------------------------------

        End Sub

        Friend Sub New(aEntity As dxfEntity)
            'init -----------------------------------------------------
            Color = dxxColors.Undefined
            DimStyle = ""
            IsDirty = False
            LayerName = ""
            Linetype = ""
            LineWeight = dxxLineWeights.Undefined
            Suppressed = False
            LTScale = 1
            TextStyle = ""
            'init -----------------------------------------------------
            If aEntity Is Nothing Then Return
            Dim aDisplay As TDISPLAYVARS = aEntity.DisplayStructure
            'init -----------------------------------------------------
            Color = aDisplay.Color
            DimStyle = aDisplay.DimStyle
            IsDirty = aDisplay.IsDirty
            LayerName = aDisplay.LayerName
            Linetype = aDisplay.Linetype
            LineWeight = aDisplay.LineWeight
            Suppressed = aDisplay.Suppressed
            LTScale = aDisplay.LTScale
            TextStyle = aDisplay.TextStyle
            'init -----------------------------------------------------

        End Sub
#End Region 'Constructors
#Region "Methods"
        Public Function Clone() As TDISPLAYVARS
            Return New TDISPLAYVARS(LayerName, Linetype, Color, LineWeight, LTScale, DimStyle, TextStyle) With {.Suppressed = Suppressed, .IsDirty = IsDirty}
        End Function

        Public Overrides Function ToString() As String
            Dim _rVal As String = $"{LayerName},{IIf(Color <> dxxColors.Undefined, Color, "") },{Linetype}"
            If _rVal = ",," Then Return $"TDISPLAYVARS" Else Return _rVal
        End Function
#End Region 'Methods
#Region "Shared Methods"

        Public Shared ReadOnly Property Null
            Get
                Return New TDISPLAYVARS("", "", dxxColors.Undefined, dxxLineWeights.Undefined, aLTScale:=1)
            End Get
        End Property

        Public Shared Function StringToEnum(ByRef aPropertyName As String) As dxxDisplayProperties
            '-------------------------------------------------------------------------
            If aPropertyName.Contains("COLOR", StringComparer.OrdinalIgnoreCase) Then
                '-------------------------------------------------------------------------
                aPropertyName = "COLOR"
                Return dxxDisplayProperties.Color
                '-------------------------------------------------------------------------
            ElseIf aPropertyName.Contains("WEIGHT", StringComparer.OrdinalIgnoreCase) Then
                '-------------------------------------------------------------------------
                aPropertyName = "LINEWEIGHT"
                Return dxxDisplayProperties.LineWeight
                '-------------------------------------------------------------------------
            ElseIf aPropertyName.Contains("LTSCALE", StringComparer.OrdinalIgnoreCase) Then
                '-------------------------------------------------------------------------
                aPropertyName = "LTSCALE"
                Return dxxDisplayProperties.LTScale
                '-------------------------------------------------------------------------
            ElseIf aPropertyName.Contains("LAYER", StringComparer.OrdinalIgnoreCase) Then
                '-------------------------------------------------------------------------
                aPropertyName = "LAYERNAME"
                Return dxxDisplayProperties.LayerName
                '-------------------------------------------------------------------------
            ElseIf aPropertyName.Contains("DIMSTYLE", StringComparer.OrdinalIgnoreCase) Then
                '-------------------------------------------------------------------------
                aPropertyName = "DIMSTYLE"
                Return dxxDisplayProperties.DimStyle
                '-------------------------------------------------------------------------
            ElseIf aPropertyName.Contains("TEXTSTYLE", StringComparer.OrdinalIgnoreCase) Or aPropertyName.Contains("STYLE", StringComparer.OrdinalIgnoreCase) Then
                '-------------------------------------------------------------------------
                aPropertyName = "TEXTSTYLE"
                Return dxxDisplayProperties.TextStyle
                '-------------------------------------------------------------------------
            ElseIf aPropertyName.Contains("LINETYPE", StringComparer.OrdinalIgnoreCase) Then
                '-------------------------------------------------------------------------
                aPropertyName = "LINETYPE"
                Return dxxDisplayProperties.Linetype
                '-------------------------------------------------------------------------
            ElseIf aPropertyName.Contains("SUPPRESS", StringComparer.OrdinalIgnoreCase) Then
                '-------------------------------------------------------------------------
                aPropertyName = "SUPPRESSED"
                Return dxxDisplayProperties.Suppressed
            Else
                Return dxxDisplayProperties.Undefined
            End If
        End Function

        Friend Shared Function ParseColor(aBaseColor As dxxColors, aByBlockColor As dxxColors, aByLayerColor As dxxColors) As dxxColors
            Dim _rVal As dxxColors = aBaseColor
            If _rVal = dxxColors.ByBlock Then _rVal = aByBlockColor
            If _rVal = dxxColors.ByLayer Then _rVal = aByLayerColor
            Return _rVal
        End Function

        Friend Shared Function ParseLineWeight(aBaseWeight As dxxLineWeights, aByBlockWeight As dxxLineWeights, aByLayerWeight As dxxLineWeights) As dxxLineWeights
            Dim _rVal As dxxLineWeights = aBaseWeight
            If _rVal = dxxLineWeights.ByBlock Then _rVal = aByBlockWeight
            If _rVal = dxxLineWeights.ByLayer Then _rVal = aByLayerWeight
            Return _rVal
        End Function

        Friend Shared Function ParseLineType(aBaseLType As String, aByBlockLType As String, aByLayerLType As String) As String

            Dim _rVal As String = aBaseLType.Trim()
            If String.Compare(_rVal, dxfLinetypes.ByBlock, True) = 0 Then _rVal = aByBlockLType.Trim()
            If String.Compare(_rVal, dxfLinetypes.ByLayer, True) = 0 Then _rVal = aByLayerLType.Trim()
            Return _rVal
        End Function

        Friend Shared Function FromEntityProperties(aEntityProps As TPROPERTIES) As TDISPLAYVARS
            Dim _rVal As New TDISPLAYVARS()
            If aEntityProps.Count <= 0 Then Return _rVal
            Dim aProp As TPROPERTY = TPROPERTY.Null
            Dim sClass As String = aEntityProps.GCValueStr(100, aOccurance:=1).ToUpper
            If aEntityProps.TryGet(62, aProp) Then _rVal.Color = TVALUES.To_INT(aProp.Value, dxxColors.ByLayer)
            If aEntityProps.TryGet(8, aProp) Then _rVal.LayerName = TVALUES.To_STR(aProp.Value, "0")
            If aEntityProps.TryGet(60, aProp) Then _rVal.Suppressed = TVALUES.ToBoolean(aProp.Value, False, True)
            If aEntityProps.TryGet(370, aProp) Then _rVal.LayerName = TVALUES.To_INT(aProp.Value, dxxLineWeights.ByLayer)
            If aEntityProps.TryGet(48, aProp) Then _rVal.LTScale = TVALUES.To_INT(aProp.Value, 1)
            If _rVal.LTScale <= 0 Then _rVal.LTScale = 1
            If aEntityProps.TryGet(66, aProp) Then _rVal.Linetype = TVALUES.To_STR(aProp.Value, dxfLinetypes.ByLayer)
            If sClass = "ACDBDIMENSION" Or sClass = "ACDBLEADER" Then
                If aEntityProps.TryGet(3, aProp) Then _rVal.Linetype = TVALUES.To_STR(aProp.Value, "Standard")
            End If
            If sClass = "ACDBDIMENSION" Or sClass = "ACDBLEADER" Then
                If aEntityProps.TryGet(3, aProp) Then _rVal.DimStyle = TVALUES.To_STR(aProp.Value, "Standard")
            Else
                _rVal.TextStyle = "Standard"
            End If
            If sClass = "ACDBMTEXT" Or sClass = "ACDBTEXT" Then
                If aEntityProps.TryGet(7, aProp) Then _rVal.TextStyle = TVALUES.To_STR(aProp.Value, "Standard")
            Else
                _rVal.TextStyle = "Standard"
            End If
            Return _rVal
        End Function

        Friend Shared Function FromEntityProperties(aEntityProps As dxoProperties) As TDISPLAYVARS
            Dim _rVal As New TDISPLAYVARS()
            If aEntityProps Is Nothing Then Return _rVal
            If aEntityProps.Count <= 0 Then Return _rVal
            Dim aProp As dxoProperty = Nothing
            Dim sClass As String = aEntityProps.ValueS(100, aOccur:=1).ToUpper
            If aEntityProps.TryGet(62, aProp) Then _rVal.Color = TVALUES.To_INT(aProp.Value, dxxColors.ByLayer)
            If aEntityProps.TryGet(8, aProp) Then _rVal.LayerName = TVALUES.To_STR(aProp.Value, "0")
            If aEntityProps.TryGet(60, aProp) Then _rVal.Suppressed = TVALUES.ToBoolean(aProp.Value, False, True)
            If aEntityProps.TryGet(370, aProp) Then _rVal.LayerName = TVALUES.To_INT(aProp.Value, dxxLineWeights.ByLayer)
            If aEntityProps.TryGet(48, aProp) Then _rVal.LTScale = TVALUES.To_INT(aProp.Value, 1)
            If _rVal.LTScale <= 0 Then _rVal.LTScale = 1
            If aEntityProps.TryGet(66, aProp) Then _rVal.Linetype = TVALUES.To_STR(aProp.Value, dxfLinetypes.ByLayer)
            If sClass = "ACDBDIMENSION" Or sClass = "ACDBLEADER" Then
                If aEntityProps.TryGet(3, aProp) Then _rVal.Linetype = TVALUES.To_STR(aProp.Value, "Standard")
            End If
            If sClass = "ACDBDIMENSION" Or sClass = "ACDBLEADER" Then
                If aEntityProps.TryGet(3, aProp) Then _rVal.DimStyle = TVALUES.To_STR(aProp.Value, "Standard")
            Else
                _rVal.TextStyle = "Standard"
            End If
            If sClass = "ACDBMTEXT" Or sClass = "ACDBTEXT" Then
                If aEntityProps.TryGet(7, aProp) Then _rVal.TextStyle = TVALUES.To_STR(aProp.Value, "Standard")
            Else
                _rVal.TextStyle = "Standard"
            End If
            Return _rVal
        End Function

#End Region 'Shared Methods
#Region "Operators"
        Public Shared Operator =(A As TDISPLAYVARS, B As TDISPLAYVARS) As Boolean
            If A.Color <> B.Color Then Return False
            If A.LTScale <> B.LTScale Then Return False
            If A.Suppressed <> B.Suppressed Then Return False
            If A.LineWeight <> B.LineWeight Then Return False
            If String.Compare(A.LayerName, B.LayerName, True) <> 0 Then Return False
            If String.Compare(A.Linetype, B.Linetype, True) <> 0 Then Return False
            Return True
        End Operator
        Public Shared Operator <>(A As TDISPLAYVARS, B As TDISPLAYVARS) As Boolean
            If A.Color <> B.Color Then Return True
            If A.LTScale <> B.LTScale Then Return True
            If A.Suppressed <> B.Suppressed Then Return True
            If A.LineWeight <> B.LineWeight Then Return True
            If String.Compare(A.LayerName, B.LayerName, True) <> 0 Then Return True
            If String.Compare(A.Linetype, B.Linetype, True) <> 0 Then Return True
            Return False
        End Operator
#End Region 'Operators
    End Structure 'TDISPLAYVARS
    Friend Structure TOBJECT
        Implements ICloneable
#Region "Members"
        Public Properties As TPROPERTIES
        Public Reactors As TPROPERTYARRAY
        Public Attributes As TPROPERTIES
        Public BinaryData As TPROPERTIES
        Public DisplayVars As TDISPLAYVARS
        Public ExtendedData As TPROPERTYARRAY
        Public Block As String
        Public ImageGUID As String
        Public Name As String
        Public Section As String
        Public Suppressed As Boolean
        Public Table As String
#End Region 'Members
#Region "Constructors"
        Public Sub New(aAddBlockProps As Boolean, Optional aBlockName As String = "")
            'init ------------------------------------------
            Properties = New TPROPERTIES("Properties")
            Reactors = New TPROPERTYARRAY("Reactors")
            Attributes = New TPROPERTIES("Attributes")
            BinaryData = New TPROPERTIES("BinaryData")
            ExtendedData = New TPROPERTYARRAY("ExtendedData")
            DisplayVars = New TDISPLAYVARS("")
            Block = ""
            ImageGUID = ""
            Name = ""
            Section = ""
            Suppressed = False
            Table = ""
            'init ------------------------------------------

            If aAddBlockProps Then
                Properties = dxpProperties.Get_BlockProps(aBlockName)
            End If
            Name = ""

        End Sub
        Public Sub New(aName As String)
            'init ------------------------------------------
            Properties = New TPROPERTIES("Properties")
            Reactors = New TPROPERTYARRAY("Reactors")
            Attributes = New TPROPERTIES("Attributes")
            BinaryData = New TPROPERTIES("BinaryData")
            ExtendedData = New TPROPERTYARRAY("ExtendedData")
            DisplayVars = New TDISPLAYVARS("")
            Block = ""
            ImageGUID = ""
            Name = ""
            Section = ""
            Suppressed = False
            Table = ""
            'init ------------------------------------------

            If Not String.IsNullOrWhiteSpace(aName) Then Name = aName

        End Sub
        Public Sub New(aObject As TOBJECT)
            'init ------------------------------------------
            Properties = New TPROPERTIES(aObject.Properties)
            Reactors = New TPROPERTYARRAY(aObject.Reactors)
            Attributes = New TPROPERTIES(aObject.Attributes)
            BinaryData = New TPROPERTIES(aObject.BinaryData)
            ExtendedData = New TPROPERTYARRAY(aObject.ExtendedData)
            DisplayVars = New TDISPLAYVARS(aObject.DisplayVars)
            Block = aObject.Block
            ImageGUID = aObject.ImageGUID
            Name = aObject.Name
            Section = aObject.Section
            Suppressed = aObject.Suppressed
            Table = aObject.Table
            'init ------------------------------------------


        End Sub

        Public Sub New(aName As String, aProperties As TPROPERTIES)
            'init ------------------------------------------
            Properties = aProperties
            Reactors = New TPROPERTYARRAY("Reactors")
            Attributes = New TPROPERTIES("Attributes")
            BinaryData = New TPROPERTIES("BinaryData")
            ExtendedData = New TPROPERTYARRAY("ExtendedData")
            DisplayVars = New TDISPLAYVARS("")
            Block = ""
            ImageGUID = ""
            Name = ""
            Section = ""
            Suppressed = False
            Table = ""
            'init ------------------------------------------

            If Not String.IsNullOrWhiteSpace(aName) Then Name = aName

        End Sub
        Public Sub New(aName As String, aProperties As dxoProperties)
            'init ------------------------------------------
            Properties = New TPROPERTIES("Properties")
            Reactors = New TPROPERTYARRAY("Reactors")
            Attributes = New TPROPERTIES("Attributes")
            BinaryData = New TPROPERTIES("BinaryData")
            ExtendedData = New TPROPERTYARRAY("ExtendedData")
            DisplayVars = New TDISPLAYVARS("")
            Block = ""
            ImageGUID = ""
            Name = ""
            Section = ""
            Suppressed = False
            Table = ""
            'init ------------------------------------------

            If Not String.IsNullOrWhiteSpace(aName) Then Name = aName
            If aProperties Is Nothing Then Return
            Properties = New TPROPERTIES(aProperties)

        End Sub

#End Region 'Constructors
#Region "Methods"


        Public Overrides Function ToString() As String
            Return $"TOBJECT [{Name} ]"
        End Function
        Public Sub ExtractExtendedData()

            Dim rProps As TPROPERTIES = New TPROPERTIES(Properties, True)

            For i As Integer = 1 To Properties.Count
                Dim aProp As TPROPERTY = Properties.Item(i)
                If aProp.GroupCode = 1001 Then
                    Dim aProps As New TPROPERTIES(aProp.Name)

                    i += 1
                    For j As Integer = i To Properties.Count
                        Dim bProp As TPROPERTY = Properties.Item(j)
                        If bProp.GroupCode <> 1001 Then
                            aProps.Add(bProp)
                            i = j
                        Else
                            Exit For
                        End If
                    Next j
                    ExtendedData.Add(aProps, aProps.Name, True)
                Else
                    rProps.Add(aProp)
                End If
            Next i
            Properties = rProps
        End Sub

        Public Function Clone() As TOBJECT
            Return New TOBJECT(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TOBJECT(Me)
        End Function
        Public Sub ExtractReactors_Entity()
            Reactors = Properties.ExtractReactorGroups
            ExtendedData = New TPROPERTYARRAY("ExtendedData")
            ExtractExtendedData()
        End Sub
        Public Sub ExtractReactors_Object()
            Reactors = Properties.ExtractReactorGroups
            Select Case Properties.GCValueStr(0).ToUpper
                Case "DICTIONARY", "ACDBDICTIONARYWDFLT"
                    Reactors.Add(Properties.GetByGroupCode(3, aFollowerCount:=1, bRemove:=True, aName:="Members", aNameList:="Member Name,Member Handle"), "", True)
                Case "GROUP"
                    Reactors.Add(Properties.GetByGroupCode(340, bRemove:=True, aName:="Members", aNameList:="Member Handle"), "", True)
                Case "MLINESTYLE"
                    Reactors.Add(Properties.GetByGroupCode(49, aFollowerCount:=2, bRemove:=True, aName:="Elements", aNameList:="Element Offset,Element Color,Element Linetype"), "", True)
                Case "SORTENTSTABLE"
                    Reactors.Add(Properties.GetByGroupCode(331, aFollowerCount:=1, bRemove:=True, aName:="Members", aNameList:="Member Handle,Member Sort Handle"), "", True)
                Case "TABLESTYLE"
                    '        propa_Add Reactors, props_ExtractAfter(Properties, 7, "Cell Settings"), "", True
            End Select
            ExtractExtendedData()
        End Sub
        Public Sub ExtractReactors_TableEntry(aEntryType As dxxReferenceTypes)
            Reactors = Properties.ExtractReactorGroups
            ExtendedData = New TPROPERTYARRAY("ExtendedData")
            If aEntryType = dxxReferenceTypes.BLOCK_RECORD Then
                BinaryData = Properties.GetByGroupCode(310, bRemove:=True, aName:="Binary Data")
            Else
                BinaryData = New TPROPERTIES()
            End If
            ExtractExtendedData()
        End Sub
#End Region 'Methods
    End Structure 'TOBJECT
    Friend Structure TOBJECTS
        Implements ICloneable
#Region "Members"

        Public Properties As TPROPERTIES
        Public Reactors As TPROPERTYARRAY
        Public Name As String
        Public Names As String
        Private _Init As Boolean
        Private _Members() As TOBJECT

#End Region 'Members
#Region "Constructors"
        Public Sub New(aName As String)
            'init ----------------------------------------------
            Properties = TPROPERTIES.Null
            Reactors = New TPROPERTYARRAY("")
            Name = ""
            Names = ""
            _Init = True
            ReDim _Members(-1)
            'init ----------------------------------------------
            Name = aName

        End Sub

        Public Sub New(aObjects As TOBJECTS)
            'init ----------------------------------------------
            Properties = New TPROPERTIES(aObjects.Properties)
            Reactors = New TPROPERTYARRAY(aObjects.Reactors)
            Name = aObjects.Names
            Names = aObjects.Names
            _Init = True
            ReDim _Members(-1)
            'init ----------------------------------------------
            If aObjects._Init Then _Members = aObjects._Members.Clone()

        End Sub

        Public Sub New(aName As String, aProperties As TPROPERTIES)
            'init ----------------------------------------------
            Properties = New TPROPERTIES(aProperties)
            Reactors = New TPROPERTYARRAY("")
            Name = ""
            Names = ""
            _Init = True
            ReDim _Members(-1)
            'init ----------------------------------------------
            Name = aName
        End Sub

        Public Sub New(aName As String, aProperties As dxoProperties)
            'init ----------------------------------------------
            Properties = TPROPERTIES.Null
            Reactors = New TPROPERTYARRAY("")
            Name = ""
            Names = ""
            _Init = True
            ReDim _Members(-1)
            'init ----------------------------------------------
            Name = aName
            If aProperties Is Nothing Then Return
            Reactors = aProperties.ExtractReactorGroups().Structure_Get
            Properties = New TPROPERTIES(aProperties)

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
#End Region '"Properties"
#Region "Methods"
        Public Overrides Function ToString() As String
            Return $"TOBJECTS [{Name}] ({Count})"
        End Function
        Public Function Item(aIndex As Integer) As TOBJECT
            If aIndex < 1 Or aIndex > Count Then Return New TOBJECT("")
            Return _Members(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, value As TOBJECT)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1) = value
        End Sub
        Public Function MemberProperties(aIndex As Integer) As TPROPERTIES
            If aIndex < 1 Or aIndex > Count Then Return New TPROPERTIES("")
            Return _Members(aIndex - 1).Properties
        End Function
        Public Sub SetMemberProperties(aIndex As Integer, value As TPROPERTIES)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1).Properties = value
        End Sub
        Public Sub Clear(Optional bClearReactors As Boolean = True)
            _Init = True
            ReDim _Members(-1)
            If bClearReactors Then Reactors = New TPROPERTYARRAY("")
        End Sub
        Public Sub ClearProps(aIndex As Integer)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1).Properties.Clear()
        End Sub
        Public Sub Add(aObj As TOBJECT)
            If Count >= Integer.MaxValue Then Return
            System.Array.Resize(_Members, Count + 1)
            _Members(_Members.Count - 1) = aObj
        End Sub
        Public Sub ExtractReactors_Table(aTableType As dxxReferenceTypes)
            Reactors = Properties.ExtractReactorGroups
        End Sub
        Public Sub ExtractReactors(aTableType As dxxReferenceTypes)
            Reactors = Properties.ExtractReactorGroups
        End Sub
        Public Function GetByPropertyStringValue(rValue As String, aGC As Integer, ByRef rIndex As Integer) As TOBJECT
            Dim aMem As TOBJECT
            Dim j As Integer
            rIndex = 0
            For i As Integer = 1 To Count
                aMem = Item(i)
                aMem.Properties.GetByStringValue(rValue, 1, j, aGC)
                If j > 0 Then
                    rIndex = i
                    Return aMem
                End If
            Next i
            Return New TOBJECT(False)
        End Function

        Public Function Clone() As TOBJECTS
            Return New TOBJECTS(Me)

        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TOBJECTS(Me)
        End Function

        Public Function ToList() As List(Of TOBJECT)
            If Count <= 0 Then Return New List(Of TOBJECT)
            Return _Members.ToList()
        End Function
#End Region 'Methods
    End Structure 'TOBJECTS
    Friend Structure TACADOPTION
#Region "Members"
        Public Index As Integer
        Public IsLite As Boolean
        Public Name As String
        Public Path As String
        Public Release As Double
        Public SearchPath As String
        Public SubKey As String
        Public Version As dxxACADVersions
#End Region 'Members
#Region "Constructors"

        Public Sub New(Optional aName As String = "")
            'init ---------------------------------
            Index = 0
            IsLite = False
            Name = ""
            Path = ""
            Release = 0
            SearchPath = ""
            SubKey = ""
            Version = dxxACADVersions.UnknownVersion
            'init ---------------------------------

            If Not String.IsNullOrWhiteSpace(aName) Then Name = aName
        End Sub

        Public Sub New(aOption As TACADOPTION)
            'init ---------------------------------
            Index = aOption.Index
            IsLite = aOption.IsLite
            Name = aOption.Name
            Path = aOption.Path
            Release = aOption.Release
            SearchPath = aOption.SearchPath
            SubKey = aOption.SubKey
            Version = aOption.Version
            'init ---------------------------------
        End Sub

#End Region 'Constructors
    End Structure 'TACADOPTION
    Friend Structure TACADOPTIONS
#Region "Members"
        Private _init As Boolean
        Public Members() As TACADOPTION


#End Region 'Members
#Region "Constructors"

        Public Sub New(Optional aCount As Integer = 0)
            'init -------------------------------------------
            _init = True
            ReDim Members(-1)
            'init -------------------------------------------
            For i As Integer = 1 To aCount
                System.Array.Resize(Members, i)
                Members(i - 1) = New TACADOPTION("")
            Next
        End Sub

        Public Sub New(aOptions As TACADOPTIONS)
            'init -------------------------------------------
            _init = True
            ReDim Members(-1)
            'init -------------------------------------------
            If aOptions.Members IsNot Nothing Then Members = aOptions.Members.Clone()

        End Sub
#End Region 'Constructors
        Public ReadOnly Property Count
            Get
                If Not _init Then
                    _init = True
                    ReDim Members(-1)
                End If
                Return Members.Count
            End Get
        End Property

        Public Sub Add(aOption As TACADOPTION)
            If Not _init Then
                _init = True
                ReDim Members(-1)
            End If
            System.Array.Resize(Members, Members.Count + 1)
            Members(Members.Count - 1) = aOption
        End Sub

    End Structure 'TACADOPTIONS
    Friend Structure TACAD
#Region "Members"
        Public AcadPath As String
        Public AutoCADName As String
        Public ConverterCode As String
        Public CurrentFileName As String
        Public CurrentOption As TACADOPTION
        Public FontPath As String
        Public HasLiteAndFullVersion As Boolean
        Public OpCanceled As Boolean
        Public Options As TACADOPTIONS
        Public Status As String
        Public Versions As TACADVERSIONS
#End Region 'Members
#Region "Constructors"
        Public Sub New(aCADPath As String)

            'init --------------------------------------------------
            AutoCADName = ""
            ConverterCode = ""
            CurrentFileName = ""
            CurrentOption = New TACADOPTION("")
            FontPath = ""
            HasLiteAndFullVersion = False
            OpCanceled = False
            Options = New TACADOPTIONS(0)
            Status = ""
            Versions = New TACADVERSIONS(0)
            'init --------------------------------------------------

            Dim v As dxxACADVersions
            Dim enumVals As Dictionary(Of String, Integer) = dxfEnums.EnumValues(GetType(dxxACADVersions))
            For i As Integer = 1 To enumVals.Count
                v = enumVals.ElementAt(i - 1).Value
                Versions.Count += 1
                System.Array.Resize(Versions.Members, Versions.Count)
                Versions.Members(Versions.Count - 1) = New TACADVERSION(v)
            Next i
        End Sub
#End Region 'Constructors
    End Structure 'TACAD
    Friend Structure TACADVERSION
#Region "Members"
        Public HeaderName As String
        Public Name As String
        Public UnWritable As Boolean
        Public Version As dxxACADVersions
#End Region 'Members
#Region "Constructors"
        Public Sub New(aVersion As dxxACADVersions)
            '.'init ---------------------------------
            HeaderName = ""
            Name = ""
            UnWritable = False
            Version = dxxACADVersions.UnknownVersion
            '.'init ---------------------------------

            Version = aVersion
            HeaderName = dxfEnums.CodeValue(aVersion)
            Name = dxfEnums.Description(aVersion)
        End Sub
#End Region 'Constructors
    End Structure 'TACADVERSION
    Friend Structure TACADVERSIONS
#Region "Members"
        Public Count As Integer
        Public Members() As TACADVERSION
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aCount As Integer = 0)
            Count = Count
            ReDim Members(-1)
            For i As Integer = 1 To aCount
                System.Array.Resize(Members, i)
                Members(i - 1) = New TACADVERSION(dxxACADVersions.UnknownVersion)
            Next
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Public Function GetVersion(aDXFVersion As dxxACADVersions, ByRef rIndex As Integer) As TACADVERSION
            '#1the version to look for
            '#2returns the array index of the version if it is found (-1 = not found)
            '^returns the requested autoCAD version
            rIndex = -1
            For i As Integer = 1 To Count
                If Members(i - 1).Version = aDXFVersion Then
                    rIndex = i - 1
                    Return Members(i - 1)
                    Exit For
                End If
            Next i
            Return New TACADVERSION(dxxACADVersions.UnknownVersion)
        End Function
        Public Function GetVersionByName(aVersionName As String, Optional bHeaderName As Boolean = False) As TACADVERSION
            Dim rIndex As Integer = 0
            Return GetVersionByName(aVersionName, bHeaderName, rIndex)
        End Function
        Public Function GetVersionByName(aVersionName As String, bHeaderName As Boolean, ByRef rIndex As Integer) As TACADVERSION
            rIndex = -1
            If String.IsNullOrWhiteSpace(aVersionName) Then Return New TACADVERSION(dxxACADVersions.UnknownVersion) Else aVersionName = aVersionName.Trim()
            For i As Integer = 1 To Count
                If Not bHeaderName Then
                    If String.Compare(Members(i - 1).Name, aVersionName, True) = 0 Then
                        rIndex = i - 1
                        Return Members(i - 1)
                    End If
                Else
                    If String.Compare(Members(i - 1).HeaderName, aVersionName, True) = 0 Then
                        rIndex = i - 1
                        Return Members(i - 1)
                        Exit For
                    End If
                End If
            Next i
            Return New TACADVERSION(dxxACADVersions.UnknownVersion)
        End Function


#End Region 'Methods
    End Structure 'TACADVERSIONS
    Friend Structure TTAGFLAGVALUE
        Implements ICloneable
        Public Tag As String
        Public Flag As String
        Public Value As Double
        Public Row As Integer
        Public Col As Integer
        Public Factor As Double
#Region "Constructors"
        Public Sub New(aTag As String, Optional aFlag As String = "", Optional aValue As Double = 0, Optional aRow As Integer = 0, Optional aCol As Integer = 0, Optional aFactor As Double = 1)
            'init ----------------
            Tag = aTag
            Flag = aFlag
            Value = aValue
            Row = aRow
            Col = aCol
            Factor = aFactor
            'init ----------------
        End Sub
        Public Sub New(aTagVal As TTAGFLAGVALUE)
            'init ----------------
            Tag = aTagVal.Tag
            Flag = aTagVal.Flag
            Value = aTagVal.Value
            Row = aTagVal.Row
            Col = aTagVal.Col
            Factor = aTagVal.Factor
            'init ----------------
        End Sub
#End Region 'Constructors
#Region "Methods"
        Public Overrides Function ToString() As String
            Return "TTAGFLAGVALUE"
        End Function
        Public Function Clone() As TTAGFLAGVALUE
            Return New TTAGFLAGVALUE(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TTAGFLAGVALUE(Me)
        End Function
        Public Function TagAndFlag(Optional aDelimitor As String = ":") As String
            If String.IsNullOrWhiteSpace(Tag) And String.IsNullOrWhiteSpace(Flag) Then Return String.Empty
            Return $"{Tag}{aDelimitor}{Flag}"
        End Function
#End Region 'Methods
    End Structure
    Friend Structure THANDLES
        Implements ICloneable
#Region "Members"
        Public ReactorGUID As String
        Public BlockGUID As String
        Public CollectionGUID As String
        Public BlockCollectionGUID As String
        Public GUID As String
        Public Handle As String
        Public ImageGUID As String
        Public Index As Integer
        Public OwnerGUID As String
        Public SourceGUID As String
        Public Domain As dxxDrawingDomains
        Public Identifier As String
        Public ObjectType As dxxFileObjectTypes
        Public OwnerType As dxxFileObjectTypes
        Public Name As String
        Public Suppressed As Boolean
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aGUID As String = "", Optional aImageGUID As String = "")
            'init -------------------------------
            ReactorGUID = ""
            BlockGUID = ""
            CollectionGUID = ""
            BlockCollectionGUID = ""
            GUID = ""
            ImageGUID = ""
            Index = 0
            Handle = ""
            OwnerGUID = ""
            SourceGUID = ""
            Domain = dxxDrawingDomains.Model
            Identifier = ""
            Name = ""
            OwnerType = dxxFileObjectTypes.Undefined
            ObjectType = dxxFileObjectTypes.Undefined
            'init -------------------------------
            GUID = aGUID
            ImageGUID = aImageGUID
        End Sub

        Public Sub New(aHandles As THANDLES, Optional aGUID As String = "")
            'init -------------------------------
            GUID = IIf(String.IsNullOrWhiteSpace(aGUID), aHandles.GUID, aGUID)
            ImageGUID = aHandles.ImageGUID
            ReactorGUID = aHandles.ReactorGUID
            BlockGUID = aHandles.BlockGUID
            CollectionGUID = aHandles.CollectionGUID
            BlockCollectionGUID = aHandles.BlockCollectionGUID
            Handle = aHandles.Handle
            Index = aHandles.Index
            OwnerGUID = aHandles.OwnerGUID
            SourceGUID = aHandles.SourceGUID
            Domain = aHandles.Domain
            Identifier = aHandles.Identifier
            Name = aHandles.Name
            OwnerType = aHandles.OwnerType
            ObjectType = aHandles.ObjectType
            'init -------------------------------

        End Sub
        Public Sub New(aHandles As dxfHandleOwner)
            'init -------------------------------
            ReactorGUID = ""
            BlockGUID = ""
            CollectionGUID = ""
            BlockCollectionGUID = ""
            GUID = ""
            ImageGUID = ""
            Index = 0
            Handle = ""
            OwnerGUID = ""
            SourceGUID = ""
            Domain = dxxDrawingDomains.Model
            Identifier = ""
            Name = ""
            OwnerType = dxxFileObjectTypes.Undefined
            ObjectType = dxxFileObjectTypes.Undefined
            'init -------------------------------
            If aHandles Is Nothing Then Return
            'init -------------------------------
            GUID = aHandles.GUID
            ImageGUID = aHandles.ImageGUID
            ReactorGUID = aHandles.ReactorGUID
            BlockGUID = aHandles.BlockGUID
            CollectionGUID = aHandles.CollectionGUID
            BlockCollectionGUID = aHandles.BlockCollectionGUID
            Handle = aHandles.Handle
            Index = aHandles.Index
            OwnerGUID = aHandles.OwnerGUID
            SourceGUID = aHandles.SourceGUID
            Domain = aHandles.Domain
            Identifier = aHandles.Identifier
            Name = aHandles.Name
            OwnerType = aHandles.OwnerType
            ObjectType = aHandles.ObjectType
            'init -------------------------------

        End Sub

#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Public Function Clone(Optional aGUID As String = "") As THANDLES
            Return New THANDLES(Me, aGUID)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New THANDLES(Me)
        End Function

        Public Overrides Function ToString() As String
            Return $"{GUID} [{ Handle}]"
        End Function
#End Region 'Methods
    End Structure 'THANDLES
    Friend Structure TBOUNDLOOP
        Implements ICloneable
#Region "Members"
        Public Derived As Boolean
        Public External As Boolean
        Public IsTextBox As Boolean
        Public LoopType As dxxLoopTypes
        Public OuterMost As Boolean
        Public Segments As TSEGMENTS
#End Region 'Members
#Region "Constructors"
        Public Sub New(aLoopType As dxxLoopTypes, Optional aDerived As Boolean = False, Optional aExternal As Boolean = False, Optional aIsTextBox As Boolean = False, Optional aOutermost As Boolean = False)
            'init ---------------------------------
            Derived = aDerived
            External = aExternal
            IsTextBox = aIsTextBox
            LoopType = aLoopType
            OuterMost = aOutermost
            Segments = New TSEGMENTS(0)
            'init ---------------------------------
        End Sub

        Public Sub New(aLoop As TBOUNDLOOP)
            'init ---------------------------------
            Derived = aLoop.Derived
            External = aLoop.External
            IsTextBox = aLoop.IsTextBox
            LoopType = aLoop.LoopType
            OuterMost = aLoop.OuterMost
            Segments = New TSEGMENTS(aLoop.Segments)
            'init ---------------------------------

        End Sub
#End Region 'Constructors
#Region "Methods"
        Public Function Clone() As TBOUNDLOOP

            Return New TBOUNDLOOP(Me)

        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TBOUNDLOOP(Me)
        End Function

#End Region 'Methods
    End Structure 'TBOUNDLOOP
    Friend Structure TBOUNDLOOPS
        Implements ICloneable
#Region "_Members"

        Public ExtentPts As TVECTORS
        Private _Init As Boolean
        Private _Members() As TBOUNDLOOP
#End Region '_Members


#Region "Constructors"
        Public Sub New(Optional aCount As Integer = 0)
            ExtentPts = New TVECTORS
            ReDim _Members(-1)
            _Init = True

            Do While Count < aCount
                Add(New TBOUNDLOOP(dxxLoopTypes.Undefined))
            Loop
        End Sub
        Public Sub New(aLoops As TBOUNDLOOPS)
            ExtentPts = New TVECTORS(aLoops.ExtentPts)
            ReDim _Members(-1)
            _Init = True
            If aLoops._Init Then _Members = aLoops._Members.Clone()

        End Sub

#End Region 'Constructors

        Public ReadOnly Property Count As Integer
            Get
                If Not _Init Then Clear()
                Return _Members.Count
            End Get
        End Property
#Region "Methods"


        Public Function Item(aIndex As Integer) As TBOUNDLOOP
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return New TBOUNDLOOP(dxxLoopTypes.Undefined)
            End If
            Return _Members(aIndex - 1)
        End Function

        Public Sub SetItem(aIndex As Integer, aLoop As TBOUNDLOOP)
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()

            End If
            _Members(aIndex - 1) = aLoop
        End Sub

        Public Sub Add(aLoop As TBOUNDLOOP)
            If Count >= Integer.MaxValue Then Return

            System.Array.Resize(_Members, Count + 1)
            _Members(Count - 1) = aLoop

        End Sub
        Public Sub Append(bLoops As TBOUNDLOOPS)


            ExtentPts.Append(bLoops.ExtentPts)
            For i As Integer = 1 To bLoops.Count
                Add(bLoops._Members(i - 1))
            Next i

        End Sub
        Public Overrides Function ToString() As String
            Return $"TBOUNDARYLOOPS[{Count}]"
        End Function


        Public Sub Clear()
            _Init = True
            ReDim _Members(0)
            ExtentPts = TVECTORS.Zero
        End Sub


        Public Function Clone() As TBOUNDLOOPS

            Return New TBOUNDLOOPS(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TBOUNDLOOPS(Me)
        End Function

        Public Sub AddLineSegments(aEndPoints As TVECTORS, aPlane As TPLANE, Optional bSuppressProjection As Boolean = False, Optional bOneLoopPerLine As Boolean = False, Optional bTextBox As Boolean = False)

            Dim cnt As Integer
            ExtentPts = New TVECTORS(0)
            Dim aLoop As New TBOUNDLOOP(dxxLoopTypes.Line, aDerived:=True, aExternal:=True)

            If Not bOneLoopPerLine Then aLoop.LoopType = dxxLoopTypes.Polyline Else aLoop.LoopType = dxxLoopTypes.Line
            For i As Integer = 1 To aEndPoints.Count - 1
                Dim v1 As TVECTOR = aEndPoints.Item(i)
                Dim v2 As TVECTOR = aEndPoints.Item(i + 1)

                If Not bSuppressProjection Then
                    v1.ProjectTo(aPlane)
                    v2.ProjectTo(aPlane)
                End If
                If v1.DistanceTo(v2, 4) > 0 Then
                    cnt += 1
                    If cnt = 1 Then ExtentPts.Add(v1)
                    ExtentPts.Add(v2)
                    aLoop.Segments.Add(v1, v2)
                    If bOneLoopPerLine Then
                        Add(aLoop)
                        aLoop.Segments = New TSEGMENTS(0)
                    End If
                End If
            Next i
            If Not bOneLoopPerLine Then
                aLoop.IsTextBox = bTextBox
                Add(aLoop)
            End If
        End Sub

        Public Function ToPath(aPlane As TPLANE) As TVECTORS
            Dim _rVal As New TVECTORS

            Dim aLoop As TBOUNDLOOP
            Dim aSeg As TSEGMENT
            Dim sVecs As TVECTORS
            For i As Integer = 1 To Count
                aLoop = _Members(i - 1)
                Select Case aLoop.LoopType
                    Case dxxLoopTypes.CircularArc
                        _rVal.Append(aLoop.Segments.Arc(1).PathPoints)
                    Case dxxLoopTypes.Line
                        _rVal.Add(aLoop.Segments.SPT(1), TVALUES.ToByte(dxxVertexStyles.MOVETO))
                        _rVal.Add(aLoop.Segments.EPT(1), TVALUES.ToByte(dxxVertexStyles.LINETO))
                    Case dxxLoopTypes.EllipticalArc
                        _rVal.Append(aLoop.Segments.Arc(1).PathPoints)
                    Case dxxLoopTypes.Polyline
                        For j As Integer = 1 To aLoop.Segments.Count
                            aSeg = aLoop.Segments.Item(j)
                            If aSeg.IsArc Then
                                sVecs = TBEZIER.ArcPathSimple(aSeg.ArcStructure, False)
                                If j = 1 Then
                                    _rVal.Append(sVecs)
                                Else
                                    _rVal.Append(sVecs, 1)
                                End If
                            Else
                                If j = 1 Then _rVal.Add(aSeg.LineStructure.SPT, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                                _rVal.Add(aSeg.LineStructure.EPT, TVALUES.ToByte(dxxVertexStyles.LINETO))
                            End If
                        Next j
                End Select
            Next i
            Return _rVal
        End Function

        Public Sub AddLoopAEL(aLoopIndex As Integer, aSegment As TSEGMENT, Optional aLoopType As dxxLoopTypes = dxxLoopTypes.Undefined)
            If Count >= Integer.MaxValue Or aLoopIndex <= 0 Or aLoopIndex >= Integer.MaxValue Then Return
            Dim idx As Integer
            Dim aExtPoints As TVECTORS
            idx = aLoopIndex - 1
            If aLoopIndex > Count Then
                Do Until Count = aLoopIndex
                    Add(New TBOUNDLOOP(dxxLoopTypes.Undefined))
                Loop
            Else
                If aLoopType = dxxLoopTypes.Undefined Then
                    aLoopType = _Members(idx).LoopType
                End If
            End If
            _Members(idx).Segments.Add(aSegment)
            aExtPoints = aSegment.ExtentPts
            If aSegment.IsArc Then
                If aSegment.ArcStructure.Elliptical Then
                    If aLoopType = dxxLoopTypes.Undefined Then aLoopType = dxxLoopTypes.EllipticalArc
                Else
                    If aLoopType = dxxLoopTypes.Undefined Then aLoopType = dxxLoopTypes.CircularArc
                End If
            Else
                If aLoopType = dxxLoopTypes.Undefined Then aLoopType = dxxLoopTypes.Line
            End If
            If aLoopType = dxxLoopTypes.Polyline Then
                If ExtentPts.Count > 0 Then
                    If ExtentPts.Last.Equals(aExtPoints.Item(1), 3) Then
                        ExtentPts.Remove(ExtentPts.Count)
                    End If
                End If
            End If
            ExtentPts.Append(aExtPoints)
            _Members(idx).LoopType = aLoopType
            _Members(idx).Derived = True
            _Members(idx).External = True
        End Sub

#End Region 'Methods
    End Structure 'TBOUNDLOOPS
    Friend Structure TVIEWPORT
        Implements ICloneable
#Region "Members"
        Public CurrentView As TPLANE
        Public ExtentRectangle As TPLANE
        Public LastView As TPLANE
        Public Name As String
        Public TableHandle As String
        Public Twist As Double
        Public UCS As TPLANE
        Public Units As dxxDeviceUnits
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aName As String = "")
            'init ------------------------------------
            CurrentView = TPLANE.World
            ExtentRectangle = TPLANE.World
            LastView = TPLANE.World
            Name = ""
            TableHandle = ""
            Twist = 0
            UCS = TPLANE.World
            Units = dxxDeviceUnits.Undefined
            'init ------------------------------------
            If Not String.IsNullOrWhiteSpace(aName) Then Name = aName

        End Sub

        Public Sub New(aViewPort As TVIEWPORT)
            'init ------------------------------------
            CurrentView = New TPLANE(aViewPort.CurrentView)
            ExtentRectangle = New TPLANE(aViewPort.ExtentRectangle)
            LastView = New TPLANE(aViewPort.LastView)
            Name = aViewPort.Name
            TableHandle = aViewPort.TableHandle
            Twist = aViewPort.Twist
            UCS = New TPLANE(aViewPort.UCS)
            Units = aViewPort.Units
            'init ------------------------------------

        End Sub


#End Region 'Constructors

        Public Function Clone() As TVIEWPORT
            Return New TVIEWPORT(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TVIEWPORT(Me)
        End Function

    End Structure 'TVIEWPORT
    Friend Structure THATCHLINE
        Implements ICloneable

#Region "Members"
        Public Angle As Double
        Public DashCount As Integer
        Public Dashes() As Double
        Public DeltaX As Double
        Public DeltaY As Double
        Public OriginX As Double
        Public OriginY As Double
        Private _Init As Boolean
#End Region 'Members
        Public Sub New(Optional aAngle As Double = 0)
            'init ---------------------------------
            Angle = aAngle
            DashCount = 0
            _Init = True
            ReDim Dashes(-1)
            DeltaX = 0
            DeltaY = 0
            OriginX = 0
            OriginY = 0
            'init ---------------------------------
        End Sub
        Public Sub New(aHatchLine As THATCHLINE)
            'init ---------------------------------
            Angle = aHatchLine.Angle
            DashCount = aHatchLine.DashCount
            _Init = True
            ReDim Dashes(-1)
            DeltaX = aHatchLine.DeltaX
            DeltaY = aHatchLine.DeltaY
            OriginX = aHatchLine.OriginX
            OriginY = aHatchLine.OriginY
            'init ---------------------------------

            If aHatchLine._Init Then Dashes = aHatchLine.Dashes.Clone()
        End Sub

#Region "Constructors"

        Public Function Clone() As THATCHLINE
            Return New THATCHLINE(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New THATCHLINE(Me)
        End Function
#End Region 'Constructors
    End Structure 'THATCHLINE
    Friend Structure THATCHPATTERN
        Implements ICloneable
#Region "Members"
        Public Description As String
        Public HatchLineCnt As Integer
        Public HatchLines() As THATCHLINE
        Public Name As String
        Private _Init As Boolean

#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aName As String = "", Optional aDescription As String = "")
            'init -------------------------
            Description = aDescription
            HatchLineCnt = 0
            ReDim HatchLines(-1)
            Name = aName
            _Init = True
            'init -------------------------

        End Sub

        Public Sub New(aPat As THATCHPATTERN)
            'init -------------------------
            Description = aPat.Description
            HatchLineCnt = aPat.HatchLineCnt
            ReDim HatchLines(-1)
            Name = aPat.Name
            _Init = True
            'init -------------------------
            If aPat._Init Then HatchLines = aPat.HatchLines.Clone()
        End Sub

#End Region 'Constructors

        Public Function Clone() As THATCHPATTERN
            Return New THATCHPATTERN(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New THATCHPATTERN(Me)
        End Function
    End Structure 'THATCHPATTERN
    Friend Structure THATCHPATTERNS
        Implements ICloneable
#Region "Members"

        Private _Init As Boolean
        Public Members() As THATCHPATTERN
        Public Name As String
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aName As String = "")
            'init --------------------------
            _Init = True
            ReDim Members(-1)
            Name = ""
            'init --------------------------
            If Not String.IsNullOrWhiteSpace(aName) Then Name = aName.Trim()
        End Sub

        Public Sub New(aPatterns As THATCHPATTERNS)
            'init --------------------------
            _Init = True
            ReDim Members(-1)
            Name = aPatterns.Name
            'init --------------------------
            If aPatterns._Init Then Members = aPatterns.Members.Clone()
        End Sub

#End Region 'Constructors

        Public ReadOnly Property Count As Integer
            Get
                If Not _Init Then
                    _Init = True
                End If
                Return Members.Count
            End Get

        End Property
        Public Sub Add(aPattern As THATCHPATTERN)
            System.Array.Resize(Members, Count + 1)
            Members(Members.Count - 1) = aPattern
        End Sub
        Public Function Clone() As THATCHPATTERNS
            Return New THATCHPATTERNS(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New THATCHPATTERNS(Me)
        End Function

        Public Function ToList() As List(Of THATCHPATTERN)
            If Count <= 0 Then Return New List(Of THATCHPATTERN)
            Return Members.ToList()
        End Function
    End Structure 'THATCHPATTERNS
    Friend Structure TGRID
        Implements ICloneable
#Region "Members"
        Public BoundaryLoops As TBOUNDLOOPS
        Public Columns As Integer
        Public HATCHPAT As THATCHPATTERN
        Public HorizontalPitch As Double
        Public LINEORIGIN As TVECTOR
        Public Method As dxxHatchMethods
        Public OffsetX As Double
        Public OffsetY As Double
        Public PitchType As dxxPitchTypes
        Public Plane As TPLANE
        Public Rotation As Double
        Public Rows As Integer
        Public ScaleFactor As Double
        Public HatchStyle As dxxHatchStyle
        Public VerticalPitch As Double
#End Region 'Members

#Region "Constructors"

        Public Sub New(Optional aMethod As dxxHatchMethods = dxxHatchMethods.Normal)
            'init --------------------------------------------------
            BoundaryLoops = New TBOUNDLOOPS(0)
            Columns = 0
            HATCHPAT = New THATCHPATTERN("", "")
            HorizontalPitch = 0
            LINEORIGIN = TVECTOR.Zero
            Method = aMethod
            OffsetX = 0
            OffsetY = 0
            PitchType = dxxPitchTypes.Undefined
            Plane = TPLANE.World
            Rotation = 0
            Rows = 0
            ScaleFactor = 1
            HatchStyle = dxxHatchStyle.dxfHatchUserDefined
            VerticalPitch = 0
            'init --------------------------------------------------
        End Sub
        Public Sub New(aGrid As TGRID)
            'init --------------------------------------------------
            BoundaryLoops = New TBOUNDLOOPS(aGrid.BoundaryLoops)
            Columns = aGrid.Columns
            HATCHPAT = New THATCHPATTERN(aGrid.HATCHPAT)
            HorizontalPitch = aGrid.HorizontalPitch
            LINEORIGIN = New TVECTOR(aGrid.LINEORIGIN)
            Method = aGrid.Method
            OffsetX = aGrid.OffsetX
            OffsetY = aGrid.OffsetY
            PitchType = aGrid.PitchType
            Plane = New TPLANE(aGrid.Plane)
            Rotation = aGrid.Rotation
            Rows = aGrid.Rows
            ScaleFactor = aGrid.ScaleFactor
            HatchStyle = aGrid.HatchStyle
            VerticalPitch = aGrid.VerticalPitch
            'init --------------------------------------------------
        End Sub
#End Region 'Constructors
        Public Function Clone() As TGRID
            Return New TGRID(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TGRID(Me)
        End Function
        Public Function GridPoints(aOrigin As dxfVector, Optional aRectangle As dxfRectangle = Nothing) As TVECTORS
            Dim _rVal As New TVECTORS
            If HorizontalPitch <= 0 Or VerticalPitch <= 0 Then Return _rVal


            Dim aRec As TPLANE

            HATCHPAT = dxfHatches.GridPointsPattern(PitchType, HorizontalPitch, VerticalPitch)
            Dim aPth As New TPATH(dxxDrawingDomains.Model)
            aPth = dxfHatches.Paths(aPth, Me, True, aOrigin)
            Dim gPts As TVECTORS = aPth.Looop(1).GetByCode(dxxVertexStyles.PIXEL)
            If aRectangle IsNot Nothing Then aRec = aRectangle.Strukture Else aRec = TPLANE.World
            If (aRec.Width > 0 Or aRec.Height > 0) And gPts.Count > 0 Then
                aRec.Origin = gPts.Item(1)

                Dim diag As Double = (Math.Sqrt(aRec.Height ^ 2 + aRec.Width ^ 2) / 2) + 0.01

                Dim vlst As TVECTOR
                Dim dsp As TVECTOR
                Dim aLoops As TBOUNDLOOPS = BoundaryLoops
                Dim bSegs As TSEGMENTS = aRec.PlanarBounds

                Dim bKeep As Boolean

                For i As Integer = 1 To gPts.Count
                    Dim v1 As TVECTOR = gPts.Item(i)
                    bKeep = True
                    If i > 1 Then
                        dsp = v1 - vlst
                    End If
                    bSegs.Translate(dsp)
                    For j As Integer = 1 To aLoops.Count
                        Dim iPts As TVECTORS = dxfIntersections.LAES_LAES(aLoops.Item(j).Segments, bSegs, False)
                        If iPts.Count > 0 Then
                            bKeep = False
                            Exit For
                        End If
                    Next j
                    vlst = v1
                    If bKeep Then _rVal.Add(v1)
                Next i
            Else
                _rVal = gPts
            End If
Done:
            Return _rVal
        End Function


    End Structure 'TGRID
    Friend Structure TBUFFER
#Region "Members"
        Public AcadVersion As dxxACADVersions
        Public BlockNames As String
        Public ErrorString As String
        Public FileName As String
        Public ImageGUID As String
        Public Index As Integer
        Public Loaded As Boolean
        Public TempFilePath As String
        Public Blocks() As TOBJECTS
        Public CLASSES As TOBJECTS
        Public Entities As TOBJECTS
        Public Header As TPROPERTIES
        Public Objects As TOBJECTS
        Public Table_APPID As TOBJECTS
        Public Table_BLOCK_RECORD As TOBJECTS
        Public Table_DIMSTYLE As TOBJECTS
        Public Table_LAYER As TOBJECTS
        Public Table_LTYPE As TOBJECTS
        Public Table_STYLE As TOBJECTS
        Public Table_UCS As TOBJECTS
        Public Table_VIEW As TOBJECTS
        Public Table_VPORT As TOBJECTS
        Public Tables As TTABLES
        Public Thumbnail As TPROPERTIES
#End Region 'Members
#Region "Constructors"
        Public Sub New(aFileSpec As String)
            'init ----------------------------------------------------
            AcadVersion = dxxACADVersions.UnknownVersion
            BlockNames = ""
            ErrorString = ""
            FileName = ""
            ImageGUID = ""
            Index = 0
            Loaded = False
            TempFilePath = ""

            ReDim Blocks(-1)
            CLASSES = New TOBJECTS("Classes")
            Entities = New TOBJECTS("ENTITIES")
            Header = New TPROPERTIES("Header")
            Objects = New TOBJECTS("OBJECTS")
            Table_APPID = New TOBJECTS("APPID")
            Table_BLOCK_RECORD = New TOBJECTS("BLOCK RECORD")
            Table_DIMSTYLE = New TOBJECTS("DIMSTYLE")
            Table_LAYER = New TOBJECTS("LAYER")
            Table_LTYPE = New TOBJECTS("LTYPE")
            Table_STYLE = New TOBJECTS("STYLE")
            Table_VIEW = New TOBJECTS("VIEW")
            Table_VPORT = New TOBJECTS("VPORT")
            Table_UCS = New TOBJECTS("UCS")
            Tables = New TTABLES("")
            Thumbnail = New TPROPERTIES("Thumbnail")
            'init ----------------------------------------------------
            If Not String.IsNullOrWhiteSpace(aFileSpec) Then FileName = aFileSpec.Trim()



        End Sub
#End Region 'Constructors
#Region "Methods"

        Public Sub SetTableobjects(aTableType As dxxTableTypes, aObjs As TOBJECTS)
            Select Case aTableType
                Case dxxTableTypes.AppID
                    Table_APPID = aObjs
                Case dxxTableTypes.BlockRecord
                    Table_BLOCK_RECORD = aObjs
                Case dxxTableTypes.DimStyle
                    Table_DIMSTYLE = aObjs
                Case dxxTableTypes.Layer
                    Table_LAYER = aObjs
                Case dxxTableTypes.LType
                    Table_LTYPE = aObjs
                Case dxxTableTypes.Style
                    Table_STYLE = aObjs
                Case dxxTableTypes.UCS
                    Table_UCS = aObjs
                Case dxxTableTypes.View
                    Table_VIEW = aObjs
                Case dxxTableTypes.VPort
                    Table_VPORT = aObjs
                Case Else

            End Select
        End Sub
        Public Function GetObjectByHandle(aObjectTypeName As String, aHandle As String) As TOBJECT
            Dim rIndex As Integer = 0
            Return GetObjectByHandle(aObjectTypeName, aHandle, rIndex)
        End Function
        Public Function GetObjectByHandle(aObjectTypeName As String, aHandle As String, ByRef rIndex As Integer) As TOBJECT
            Dim _rVal As New TOBJECT(False)
            rIndex = -1

            aObjectTypeName = aObjectTypeName.Trim().ToUpper()
            aHandle = aHandle.Trim().ToUpper()
            If aHandle = "" Then Return _rVal
            For i As Integer = 1 To Objects.Count
                Dim aObj As TOBJECT = Objects.Item(i)
                If aObjectTypeName <> "" Then
                    Dim tname As String = aObj.Properties.GCValueStr(0).ToUpper()
                    If tname = aObjectTypeName Then
                        Dim hndl As String = aObj.Properties.GCValueStr(5).ToUpper()
                        If hndl = aHandle Then
                            _rVal = aObj
                            rIndex = i
                            Exit For
                        End If
                    End If
                Else
                    Dim hndl As String = aObj.Properties.GCValueStr(5).ToUpper()
                    If hndl = aHandle Then
                        _rVal = aObj
                        rIndex = i
                        Exit For
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Function GetTable(aTableType As dxxReferenceTypes, Optional aMemberList As String = "") As TTABLE
            Dim iGUID As String = String.Empty
            Return GetTable(aTableType, aMemberList, iGUID)
        End Function
        Public Function GetTable(aTableType As dxxReferenceTypes, aMemberList As String, ByRef iGUID As String) As TTABLE
            Dim _rVal As New TTABLE(aTableType)
            Dim aObj As TOBJECT
            Dim aEntry As TTABLEENTRY
            Dim bufObjs As TOBJECTS
            Dim SubProps As TPROPERTIES
            Dim defProps As TPROPERTIES
            Dim idx As Integer
            Dim oname As String
            Dim bKeep As Boolean

            If _rVal.TableType = dxxReferenceTypes.UNDEFINED Then Return _rVal
            Select Case aTableType
                Case dxxReferenceTypes.APPID
                    bufObjs = Table_APPID
                Case dxxReferenceTypes.BLOCK_RECORD
                    bufObjs = Table_BLOCK_RECORD
                Case dxxReferenceTypes.DIMSTYLE
                    bufObjs = Table_DIMSTYLE
                Case dxxReferenceTypes.LAYER
                    bufObjs = Table_LAYER
                Case dxxReferenceTypes.LTYPE
                    bufObjs = Table_LTYPE
                Case dxxReferenceTypes.STYLE
                    bufObjs = Table_STYLE
                Case dxxReferenceTypes.UCS
                    bufObjs = Table_UCS
                Case dxxReferenceTypes.VIEW
                    bufObjs = Table_VIEW
                Case dxxReferenceTypes.VPORT
                    bufObjs = Table_VPORT
                Case Else
                    bufObjs = New TOBJECTS("")
            End Select
            _rVal.Props.CopyValuesByGC(bufObjs.Properties, "0,2,100", True)
            defProps = New TTABLEENTRY(_rVal.TableType).Props
            _rVal.Reactors = bufObjs.Reactors
            _rVal.ImageGUID = iGUID
            For j As Integer = 1 To bufObjs.Count
                aObj = bufObjs.Item(j)
                bKeep = aObj.Properties.Count > 0
                If bKeep Then
                    oname = aObj.Properties.GCValueStr(2)
                    bKeep = TLISTS.Contains(oname, aMemberList, bReturnTrueForNullList:=True)
                    If bKeep Then
                        aEntry = New TTABLEENTRY(_rVal.TableType, aObj.Properties.GCValueStr(2))  ''idx
                        If aEntry.EntryType = dxxReferenceTypes.LTYPE Then
                            aEntry.Props = defProps
                            SubProps = aObj.Properties.GetAfter(40, False, bRemove:=True)
                            aEntry.Props.CopyValuesByGC(aObj.Properties, "0,100", False, True)
                            idx = aEntry.Props.GroupCodeIndex(40)
                            dxfLinetypes.AssignPropertyNames(SubProps)
                            aEntry.Props.Append(SubProps)
                        Else
                            aEntry.Props = defProps
                            aEntry.Props.CopyValuesByGC(aObj.Properties, "0,100", False, True)
                        End If
                        If aEntry.EntryType = dxxReferenceTypes.STYLE Then
                            If Not aEntry.PropValueB(dxxStyleProperties.SHAPESTYLEFLAG) Then
                                aEntry.UpdateFontName(aEntry.PropValueStr(dxxStyleProperties.FONTNAME), aEntry.PropValueStr(dxxStyleProperties.FONTSTYLE))
                            End If
                        ElseIf aEntry.EntryType = dxxReferenceTypes.DIMSTYLE Then
                            'update prefix and suffix
                            aEntry.PropValueSet(dxxDimStyleProperties.DIMPOST, aEntry.PropValueStr(dxxDimStyleProperties.DIMPOST))
                            aEntry.PropValueSet(dxxDimStyleProperties.DIMAPOST, aEntry.PropValueStr(dxxDimStyleProperties.DIMAPOST))
                        End If
                        aEntry.Reactors = aObj.Reactors
                        aEntry.ExtendedData.Append(aObj.ExtendedData)
                        aEntry.BinaryData = aObj.BinaryData
                        aEntry.ImageGUID = iGUID
                        _rVal.Add(aEntry)
                    End If
                End If
            Next j
            Tables.Add(_rVal)
            Return _rVal
        End Function
        Public Sub UpdateTables(Optional aNames As String = "")
            If TLISTS.Contains("APPID", aNames, bReturnTrueForNullList:=True) Then
                GetTable(dxxReferenceTypes.APPID)
            End If
            If TLISTS.Contains("BLOCKRECORD", aNames, bReturnTrueForNullList:=True) Then
                GetTable(dxxReferenceTypes.BLOCK_RECORD)
            End If
            If TLISTS.Contains("DIMSTYLE", aNames, bReturnTrueForNullList:=True) Then
                GetTable(dxxReferenceTypes.DIMSTYLE)
            End If
            If TLISTS.Contains("LAYER", aNames, bReturnTrueForNullList:=True) Then
                GetTable(dxxReferenceTypes.LAYER)
            End If
            If TLISTS.Contains("LTYPE", aNames, bReturnTrueForNullList:=True) Then
                GetTable(dxxReferenceTypes.LTYPE)
            End If
            If TLISTS.Contains("STYLE", aNames, bReturnTrueForNullList:=True) Then
                GetTable(dxxReferenceTypes.STYLE)
            End If
            If TLISTS.Contains("UCS", aNames, bReturnTrueForNullList:=True) Then
                GetTable(dxxReferenceTypes.UCS)
            End If
            If TLISTS.Contains("VIEW", aNames, bReturnTrueForNullList:=True) Then
                GetTable(dxxReferenceTypes.VIEW)
            End If
            If TLISTS.Contains("VPORT", aNames, bReturnTrueForNullList:=True) Then
                GetTable(dxxReferenceTypes.VPORT)
            End If
        End Sub
#End Region 'Methods
    End Structure 'TBUFFER
    Friend Structure TBUFFERS
#Region "Members"
        Public Name As String
        Private _Init As Boolean
        Private _Members() As TBUFFER
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aName As String = "")
            'init -------------------------------
            Name = ""
            _Init = True
            ReDim _Members(-1)
            'init -------------------------------
            If Not String.IsNullOrWhiteSpace(aName) Then Name = aName.Trim()

        End Sub
#End Region 'Constructors
        Public ReadOnly Property Count As Integer
            Get
                If Not _Init Then
                    _Init = True
                    ReDim _Members(-1)
                End If
                Return _Members.Count
            End Get
        End Property
#Region "Methods"
        Public Function Item(aIndex As Integer) As TBUFFER
            If aIndex < 1 Or aIndex > Count Then Return New TBUFFER("")
            Return _Members(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, value As TBUFFER)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1) = value
        End Sub
        Public Sub Clear()
            _Init = True
            ReDim _Members(-1)
        End Sub
        Public Sub Add(aBuf As TBUFFER)
            If Count >= Integer.MaxValue Then Return
            System.Array.Resize(_Members, Count + 1)
            _Members(_Members.Count - 1) = aBuf
        End Sub
        Friend Sub Remove(aIndex As Integer)
            If aIndex < 1 Or aIndex > Count Or Count = 0 Then Return
            If aIndex = Count Then
                System.Array.Resize(_Members, Count - 1)
                Return
            End If
            If Count = 1 And aIndex = 1 Then
                Clear()
                Return
            End If
            Dim j As Integer = 0
            Dim newMems(0 To Count - 1) As TBUFFER
            For i As Integer = 1 To Count
                If i <> aIndex Then
                    newMems(j) = Item(i)
                    j += 1
                End If
            Next i
            _Members = newMems
        End Sub
        Friend Function GetByFileName(aFileName As String, ByRef rIndex As Integer) As TBUFFER
            Dim _rVal As New TBUFFER
            rIndex = -1
            If String.IsNullOrWhiteSpace(aFileName) Then Return _rVal
            For i As Integer = 1 To Count
                If String.Compare(_Members(i - 1).FileName, aFileName, ignoreCase:=True) = 0 Or
                    String.Compare(IO.Path.GetFileName(_Members(i - 1).FileName), aFileName, ignoreCase:=True) = 0 Or
                    String.Compare(IO.Path.GetFileNameWithoutExtension(_Members(i - 1).FileName), aFileName, ignoreCase:=True) = 0 Then
                    rIndex = i
                    Return _Members(i - 1)
                End If
            Next i
            Return Nothing
        End Function
#End Region 'Methods
    End Structure 'TBUFFERS
    Friend Structure TBLOCK
        Implements ICloneable
#Region "Members"


        Public EndBlockHandle As String
        Public Flag As String
        Public IsDefault As Boolean
        Public IsAnonomous As Boolean
        Public IsArrowHead As Boolean
        Public IsExDependant As Boolean
        Public IsExref As Boolean
        Public IsExrefed As Boolean
        Public IsOverlay As Boolean
        Public IsResolved As Boolean
        Public XRefPath As String
        Public PathsDefined As Boolean
        Public ReadFrom As String
        Public Suppressed As Boolean
        Public SuppressInstances As Boolean
        Public Tag As String
        Public RelativePaths As TPATHS
        Public SubBlockNames As TLIST
        Public VIEWPORT As TTABLEENTRY
        Private _Handlez As THANDLES
        Private _Plane As TPLANE
        Public Props As TPROPERTIES
        Public BLKRECORD As TTABLEENTRY

#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aName As String = "")
            'init ------------------------------------------------
            VIEWPORT = New TTABLEENTRY(dxxReferenceTypes.VPORT)
            _Handlez = New THANDLES(String.Empty)
            _Plane = TPLANE.World
            Props = dxpProperties.Get_BlockProps(String.Empty)
            EndBlockHandle = String.Empty
            Flag = String.Empty
            IsDefault = False
            IsAnonomous = False
            IsArrowHead = False
            IsExDependant = False
            IsExref = False
            IsExrefed = False
            IsOverlay = False
            IsResolved = False
            XRefPath = String.Empty
            PathsDefined = False
            ReadFrom = String.Empty
            Suppressed = False
            SuppressInstances = False
            Tag = String.Empty
            RelativePaths = New TPATHS(dxxDrawingDomains.Model)
            SubBlockNames = New TLIST(",")

            'init ------------------------------------------------
            If Not String.IsNullOrWhiteSpace(aName) Then
                Name = aName.Trim()
                Props.Name = Name
                Props.GCValueSet(2, Name)
                Props.GCValueSet(3, Name)

            End If
            Props.GCValueSet(8, "0") 'LayerName = "0"
        End Sub


        Public Sub New(aBlock As TBLOCK)
            'init ------------------------------------------------
            VIEWPORT = New TTABLEENTRY(aBlock.VIEWPORT)
            _Handlez = New THANDLES(aBlock._Handlez)
            _Plane = New TPLANE(aBlock.Plane)
            Props = New TPROPERTIES(aBlock.Props)

            EndBlockHandle = aBlock.EndBlockHandle
            Flag = aBlock.Flag
            IsDefault = aBlock.IsDefault
            IsAnonomous = aBlock.IsAnonomous
            IsArrowHead = aBlock.IsArrowHead
            IsExDependant = aBlock.IsExDependant
            IsExref = aBlock.IsExref
            IsExrefed = aBlock.IsExrefed
            IsOverlay = aBlock.IsOverlay
            IsResolved = aBlock.IsResolved
            XRefPath = aBlock.XRefPath
            PathsDefined = aBlock.PathsDefined
            ReadFrom = aBlock.ReadFrom
            Suppressed = aBlock.Suppressed
            SuppressInstances = aBlock.SuppressInstances
            Tag = aBlock.Tag
            RelativePaths = New TPATHS(aBlock.RelativePaths)
            SubBlockNames = New TLIST(aBlock.SubBlockNames)

            'init ------------------------------------------------

        End Sub

        Public Property Name As String
            Get
                Return Props.GCValueStr(2)
            End Get
            Set(value As String)
                Props.GCValueSet(2, value)
                Props.GCValueSet(3, value)
            End Set
        End Property

        Public Property LayerName As String
            Get
                Return Props.GCValueStr(8)
            End Get
            Set(value As String)
                Props.GCValueSet(8, value)
            End Set
        End Property

        Public Property Description As String
            Get
                Return Props.GCValueStr(4)
            End Get
            Set(value As String)
                Props.GCValueSet(4, value)
            End Set
        End Property

#End Region 'Constructors
#Region "Properties"
        Public Property Handlez As THANDLES
            Get
                Return _Handlez
            End Get
            Set(value As THANDLES)
                _Handlez = value
            End Set
        End Property
        Public Property Plane As TPLANE
            Get
                Return _Plane
            End Get
            Set(value As TPLANE)
                _Plane = New TPLANE(value)
            End Set
        End Property
#End Region 'Properties

        Public Function Clone() As TBLOCK
            Return New TBLOCK(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TBLOCK(Me)
        End Function

    End Structure 'TBLOCK
    Friend Structure TCOMPONENTS
        Implements ICloneable
#Region "Members"
        Public BoundaryLoops As TBOUNDLOOPS
        Public DrawnPaths As TPATHS
        Public Segments As TSEGMENTS
        Public SubStrings As TSTRINGS
        Public Paths As TPATHS
        Public GraphicType As dxxGraphicTypes
#End Region 'Members
#Region "Constructors"
        Public Sub New(aGraphicType As dxxGraphicTypes, Optional aOwnerGUID As String = "")
            'init ----------------------------------------------------------
            GraphicType = aGraphicType
            DrawnPaths = New TPATHS(dxxDrawingDomains.Model)
            Segments = New TSEGMENTS(0)
            SubStrings = New TSTRINGS()
            BoundaryLoops = New TBOUNDLOOPS(0)
            Paths = New TPATHS(dxxDrawingDomains.Model, aOwnerGUID)
            'init ----------------------------------------------------------
        End Sub
        Public Sub New(aComponents As TCOMPONENTS, Optional aOwnerGUID As String = "")
            'init ----------------------------------------------------------
            GraphicType = aComponents.GraphicType
            DrawnPaths = New TPATHS(aComponents.Paths)
            Segments = New TSEGMENTS(aComponents.Segments)
            SubStrings = New TSTRINGS(aComponents.SubStrings)
            BoundaryLoops = New TBOUNDLOOPS(aComponents.BoundaryLoops)
            Paths = New TPATHS(aComponents.Paths)
            'init ----------------------------------------------------------
            If Not String.IsNullOrWhiteSpace(aOwnerGUID) Then Paths.EntityGUID = aOwnerGUID

        End Sub

#End Region 'Constructors
#Region "Methods"
        Public Overrides Function ToString() As String
            Return $"TCOMPONENTS[{ dxfEnums.Description(GraphicType) }]"
        End Function
        Public Sub Clear()
            BoundaryLoops = New TBOUNDLOOPS(0)
            Paths = New TPATHS(dxxDrawingDomains.Model)
            Segments = New TSEGMENTS(0)
            DrawnPaths = New TPATHS(dxxDrawingDomains.Model)

        End Sub
        Public Function Clone() As TCOMPONENTS

            Return New TCOMPONENTS(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TCOMPONENTS(Me)
        End Function
#End Region 'Methods
    End Structure 'TCOMPONENTS


    Friend Structure TDIMLINESPRESSION
#Region "Members"
#End Region 'Members
        Public SuppressExtLine1 As Boolean
        Public SuppressExtLine2 As Boolean
        Public SuppressDimLine1 As Boolean
        Public SuppressDimLine2 As Boolean
#Region "Constructors"
        Public Sub New(Optional aSupDimLn1 As Boolean? = Nothing, Optional aSupDimLn2 As Boolean? = Nothing, Optional aSupExtLn1 As Boolean? = Nothing, Optional aSupExtLn2 As Boolean? = Nothing)
            'init ----------------------------------
            SuppressDimLine1 = False
            SuppressDimLine2 = False
            SuppressExtLine1 = False
            SuppressExtLine2 = False
            'init ----------------------------------

            If aSupDimLn1.HasValue Then SuppressDimLine1 = aSupDimLn1.Value

            If aSupDimLn2.HasValue Then SuppressDimLine2 = aSupDimLn2.Value

            If aSupExtLn1.HasValue Then SuppressExtLine1 = aSupExtLn1.Value
            If aSupExtLn2.HasValue Then SuppressExtLine2 = aSupExtLn2.Value
        End Sub

        Public Sub New(aSup As TDIMLINESPRESSION)
            'init ----------------------------------
            SuppressDimLine1 = aSup.SuppressDimLine1
            SuppressDimLine2 = aSup.SuppressDimLine2
            SuppressExtLine1 = aSup.SuppressExtLine1
            SuppressExtLine2 = aSup.SuppressExtLine2
            'init ----------------------------------

        End Sub

#End Region 'Constructors
    End Structure
    Friend Structure TDIMINPUT
#Region "Members"
        Public AbsolutePlacement As Boolean
        Public ArrowBlock1 As String
        Public ArrowBlock2 As String
        Public ArrowBlockL As String
        Public ArrowHead1 As dxxArrowHeadTypes
        Public ArrowHead2 As dxxArrowHeadTypes
        Public ArrowHeadL As dxxArrowHeadTypes
        Public ArrowSize As Double
        Public aSingle As Double
        Public BubbleType As Long
        Public CenterMarkSize As Double?
        Public LinearPrecision As Integer?
        Public AngularPrecision As Integer?
        Public Color As dxxColors?
        Public DimLineColor As dxxColors?
        Public DimOffset As Double
        Public DimScale As Double?
        Public DimStyleName As String
        Public DimType As Integer
        Public ExtLineColor As dxxColors?
        Public factor2 As Double
        Public Flag1 As Boolean
        Public Flag2 As Boolean
        Public Flag3 As Boolean
        Public Flag4 As Boolean
        Public LayerName As String
        Public Linetype As String
        Public OverideText As String
        Public PlacementAngle As Double
        Public PlacementDistance As Double
        Public Plane As TPLANE
        Public PointerType As Long
        Public PopArrows As Boolean
        Public PopText As Boolean
        Public Prefix As String
        Public RetainImage As Boolean
        Public SaveAndDraw As Boolean
        Public SpacingFactor As Double
        Public Suffix As String
        Public SymbolType As Long
        Public TextColor As dxxColors?
        Public TextOffset As Double
        Public TextStyleName As String
        Public TickOffset As Double
        Public TickPoint As Integer
        Public WeldType As Long
        Public XYPoint1 As TVECTOR
        Public XYPoint2 As TVECTOR
        Public XYPoint3 As TVECTOR
        Public XYPoint4 As TVECTOR
        Public XYPoint5 As TVECTOR
        Public DisplayVars As TDISPLAYVARS
#End Region 'Members
#Region "Constructors"
        Public Sub New(aDimStyleName As String, aTextStyleName As String, Optional aDimensionType As dxxDimTypes = dxxDimTypes.Undefined)

            'init ---------------------------------------------------------
            AbsolutePlacement = False
            ArrowBlock1 = String.Empty
            ArrowBlock2 = String.Empty
            ArrowBlockL = String.Empty
            ArrowHead1 = dxxArrowHeadTypes.ClosedFilled
            ArrowHead2 = dxxArrowHeadTypes.ClosedFilled
            ArrowHeadL = dxxArrowHeadTypes.ClosedFilled
            ArrowSize = 0
            aSingle = 0
            BubbleType = 0
            CenterMarkSize = Nothing
            LinearPrecision = Nothing
            AngularPrecision = Nothing
            Color = Nothing
            DimLineColor = Nothing
            DimOffset = 0
            DimScale = Nothing
            DimStyleName = String.Empty
            DimType = dxxDimTypes.Undefined
            ExtLineColor = Nothing
            factor2 = 0
            Flag1 = False
            Flag2 = False
            Flag3 = False
            Flag4 = False
            LayerName = String.Empty
            Linetype = String.Empty
            OverideText = String.Empty
            PlacementAngle = 0
            PlacementDistance = 0
            Plane = TPLANE.World
            PointerType = 0
            PopArrows = False
            PopText = False
            Prefix = String.Empty
            RetainImage = False
            SaveAndDraw = False
            SpacingFactor = 0
            Suffix = String.Empty
            SymbolType = 0
            TextColor = Nothing
            TextOffset = 0
            TextStyleName = String.Empty
            TickOffset = 0
            TickPoint = 0
            WeldType = 0
            XYPoint1 = TVECTOR.Zero
            XYPoint2 = TVECTOR.Zero
            XYPoint3 = TVECTOR.Zero
            XYPoint4 = TVECTOR.Zero
            XYPoint5 = TVECTOR.Zero
            DisplayVars = TDISPLAYVARS.Null
            'init ---------------------------------------------------------

            If Not String.IsNullOrWhiteSpace(aDimStyleName) Then DimStyleName = aDimStyleName.Trim Else DimStyleName = ""
            If Not String.IsNullOrWhiteSpace(aTextStyleName) Then TextStyleName = aTextStyleName.Trim Else TextStyleName = ""
            ArrowSize = -1

            ArrowHead1 = dxxArrowHeadTypes.ByStyle
            ArrowHead2 = dxxArrowHeadTypes.ByStyle
            ArrowHeadL = dxxArrowHeadTypes.ByStyle
            Dim eType As dxxEntityTypes = dxxEntityTypes.Undefined
            DimType = aDimensionType
        End Sub

        Public Sub New(aTag As String)

            'init ---------------------------------------------------------
            AbsolutePlacement = False
            ArrowBlock1 = String.Empty
            ArrowBlock2 = String.Empty
            ArrowBlockL = String.Empty
            ArrowHead1 = dxxArrowHeadTypes.ClosedFilled
            ArrowHead2 = dxxArrowHeadTypes.ClosedFilled
            ArrowHeadL = dxxArrowHeadTypes.ClosedFilled
            ArrowSize = 0
            aSingle = 0
            BubbleType = 0
            CenterMarkSize = Nothing
            LinearPrecision = Nothing
            AngularPrecision = Nothing
            Color = Nothing
            DimLineColor = Nothing
            DimOffset = 0
            DimScale = Nothing
            DimStyleName = String.Empty
            DimType = dxxDimTypes.Undefined
            ExtLineColor = Nothing
            factor2 = 0
            Flag1 = False
            Flag2 = False
            Flag3 = False
            Flag4 = False
            LayerName = String.Empty
            Linetype = String.Empty
            OverideText = String.Empty
            PlacementAngle = 0
            PlacementDistance = 0
            Plane = TPLANE.World
            PointerType = 0
            PopArrows = False
            PopText = False
            Prefix = String.Empty
            RetainImage = False
            SaveAndDraw = False
            SpacingFactor = 0
            Suffix = String.Empty
            SymbolType = 0
            TextColor = 0
            TextOffset = 0
            TextStyleName = String.Empty
            TickOffset = 0
            TickPoint = 0
            WeldType = 0
            XYPoint1 = TVECTOR.Zero
            XYPoint2 = TVECTOR.Zero
            XYPoint3 = TVECTOR.Zero
            XYPoint4 = TVECTOR.Zero
            XYPoint5 = TVECTOR.Zero
            DisplayVars = TDISPLAYVARS.Null
            'init ---------------------------------------------------------


        End Sub

        Public Sub New(aInput As TDIMINPUT)
            'init ---------------------------------------------------------
            AbsolutePlacement = aInput.AbsolutePlacement
            ArrowBlock1 = aInput.ArrowBlock1
            ArrowBlock2 = aInput.ArrowBlock2
            ArrowBlockL = aInput.ArrowBlockL
            ArrowHead1 = aInput.ArrowHead1
            ArrowHead2 = aInput.ArrowHead2
            ArrowHeadL = aInput.ArrowHeadL
            ArrowSize = aInput.ArrowSize
            aSingle = aInput.aSingle
            BubbleType = aInput.BubbleType
            CenterMarkSize = aInput.CenterMarkSize
            LinearPrecision = aInput.LinearPrecision
            AngularPrecision = aInput.AngularPrecision
            Color = aInput.Color
            DimLineColor = aInput.DimLineColor
            DimOffset = aInput.DimOffset
            DimScale = aInput.DimScale
            DimStyleName = aInput.DimStyleName
            DimType = aInput.DimType
            ExtLineColor = aInput.ExtLineColor
            factor2 = aInput.factor2
            Flag1 = aInput.Flag1
            Flag2 = aInput.Flag2
            Flag3 = aInput.Flag3
            Flag4 = aInput.Flag4
            LayerName = aInput.LayerName
            Linetype = aInput.Linetype
            OverideText = aInput.OverideText
            PlacementAngle = aInput.PlacementAngle
            PlacementDistance = aInput.PlacementDistance
            Plane = New TPLANE(aInput.Plane)
            PointerType = aInput.PointerType
            PopArrows = aInput.PopArrows
            PopText = aInput.PopText
            Prefix = aInput.Prefix
            RetainImage = aInput.RetainImage
            SaveAndDraw = aInput.SaveAndDraw
            SpacingFactor = aInput.SpacingFactor
            Suffix = aInput.Suffix
            SymbolType = aInput.SymbolType
            TextColor = aInput.TextColor
            TextOffset = aInput.TextOffset
            TextStyleName = aInput.TextStyleName
            TickOffset = aInput.TickOffset
            TickPoint = aInput.TickPoint
            WeldType = aInput.WeldType
            XYPoint1 = New TVECTOR(aInput.XYPoint1)
            XYPoint2 = New TVECTOR(aInput.XYPoint2)
            XYPoint3 = New TVECTOR(aInput.XYPoint3)
            XYPoint4 = New TVECTOR(aInput.XYPoint4)
            XYPoint5 = New TVECTOR(aInput.XYPoint5)
            DisplayVars = New TDISPLAYVARS(aInput.DisplayVars)
            'init ---------------------------------------------------------
        End Sub
#End Region 'Constructors

#Region "Shared Methods"
        Public Shared ReadOnly Property Null As TDIMINPUT
            Get

                Return New TDIMINPUT("")
            End Get
        End Property
#End Region 'Shared Methods
    End Structure 'TDIMINPUT
    Friend Structure TBITMAPDATA
#Region "Members"
        ' Provide public access to the picture's byte data.
        Public ImageBytes() As Byte
        Public RowSizeBytes As Integer
        Public Const PixelDataSize As Integer = 24
        ' A reference to the Bitmap.
        Private m_Bitmap As Bitmap
        ' Bitmap data.
        Private m_BitmapData As BitmapData
#End Region 'Members
#Region "Properties"
        Public ReadOnly Property Data As BitmapData
            Get
                Return m_BitmapData
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        ' Save a reference to the bitmap.
        Public Sub New(bm As Bitmap)
            m_Bitmap = bm
        End Sub
        ' Lock the bitmap's data.
        Public Sub LockBitmap()
            ' Lock the bitmap data.
            Dim bounds As New Rectangle(0, 0, m_Bitmap.Width, m_Bitmap.Height)
            m_BitmapData = m_Bitmap.LockBits(bounds, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            RowSizeBytes = m_BitmapData.Stride
            ' Allocate room for the data.
            Dim total_size As Integer = Math.Abs(m_BitmapData.Stride) * m_BitmapData.Height
            ReDim ImageBytes(total_size)
            ' Copy the data into the ImageBytes array.
            Marshal.Copy(m_BitmapData.Scan0, ImageBytes, 0, total_size)
        End Sub
        ' Copy the data back into the Bitmap
        ' and release resources.
        Public Sub UnlockBitmap()
            ' Copy the data back into the bitmap.
            Dim total_size As Integer = m_BitmapData.Stride * m_BitmapData.Height
            Marshal.Copy(ImageBytes, 0, m_BitmapData.Scan0, total_size)
            ' Unlock the bitmap.
            m_Bitmap.UnlockBits(m_BitmapData)
            ' Release resources.
            ImageBytes = Nothing
            m_BitmapData = Nothing
        End Sub
#End Region 'Methods
#Region "Share Methods"
        Public Shared Function ConvertToRGB32(original As Bitmap) As Bitmap
            If original Is Nothing Then Return Nothing
            If original.PixelFormat = System.Drawing.Imaging.PixelFormat.Format32bppArgb Then Return original
            Dim newImage As New Bitmap(original.Width, original.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb)
            newImage.SetResolution(original.HorizontalResolution, original.VerticalResolution)
            Dim g As Graphics = Graphics.FromImage(newImage)
            g.DrawImageUnscaled(original, 0, 0)
            g.Dispose()
            Return newImage
        End Function
        Public Shared Function ConvertToRGB24(original As Bitmap) As Bitmap
            If original Is Nothing Then Return Nothing
            If original.PixelFormat = System.Drawing.Imaging.PixelFormat.Format24bppRgb Then Return original
            Dim newImage As New Bitmap(original.Width, original.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb)
            newImage.SetResolution(original.HorizontalResolution, original.VerticalResolution)
            Dim g As Graphics = Graphics.FromImage(newImage)
            g.DrawImageUnscaled(original, 0, 0)
            g.Dispose()
            Return newImage
        End Function
#End Region 'Share Methods
    End Structure 'TBITMAPDATA
    Friend Structure TLIMITS
        Implements ICloneable
#Region "Members"
        Public base As Double
        Public Bottom As Double
        Public Left As Double
        Public Right As Double
        Public Top As Double
        Public MaxZ As Double
        Public MinZ As Double

#End Region 'Members
#Region "Constructors"
        Public Sub New(bMaxed As Boolean)
            'init =========================================
            base = 0
            Bottom = 0
            Left = 0
            Right = 0
            Top = 0
            MaxZ = 0
            MinZ = 0
            'init =========================================
            If bMaxed Then
                Left = Double.MaxValue
                Right = -Double.MaxValue
                Top = -Double.MaxValue
                Bottom = Double.MaxValue
                MaxZ = -Double.MaxValue
                MinZ = Double.MaxValue

            End If
        End Sub
        Public Sub New(aLimits As TLIMITS)
            'init =========================================
            base = aLimits.base
            Bottom = aLimits.Bottom
            Left = aLimits.Left
            Right = aLimits.Right
            Top = aLimits.Top
            MaxZ = aLimits.MaxZ
            MinZ = aLimits.MinZ
            'init =========================================

        End Sub

        Public Sub New(aPoint As TPOINT)
            'init =========================================
            base = 0
            Bottom = 0
            Left = 0
            Right = 0
            Top = 0
            MaxZ = 0
            MinZ = 0
            'init =========================================
            Bottom = aPoint.Y
            Top = aPoint.Y
            Left = aPoint.X
            Right = aPoint.X
        End Sub
        Public Sub New(aVector As TVECTOR)
            'init =========================================
            base = 0
            Bottom = 0
            Left = 0
            Right = 0
            Top = 0
            MaxZ = 0
            MinZ = 0
            'init =========================================


            Bottom = aVector.Y
            Top = aVector.Y
            Left = aVector.X
            Right = aVector.X

        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Width As Double
            Get
                Return Math.Abs(Right - Left)
            End Get
        End Property
        Public ReadOnly Property Height As Double
            Get
                Return Math.Abs(Top - Bottom)
            End Get
        End Property
#End Region 'Properties
#Region "Methods"

        Public Function CornerPts() As TPOINTS
            Dim _rVal As New TPOINTS(0)
            _rVal.Add(Left, Bottom, TVALUES.ToByte(dxxVertexStyles.MOVETO))
            _rVal.Add(Right, Bottom, TVALUES.ToByte(dxxVertexStyles.MOVETO))
            _rVal.Add(Right, Top, TVALUES.ToByte(dxxVertexStyles.MOVETO))
            _rVal.Add(Left, Top, TVALUES.ToByte(dxxVertexStyles.MOVETO))

            Return _rVal

        End Function

        Public Overrides Function ToString() As String
            Return $"L:{ Left:0,0.0##} R:{ Right:0,0.0##} T:{ Top:0,0.0##} B:{ Bottom:0,0.0##}"
        End Function
        Public Sub Update(aLimits As TLIMITS, Optional bMinBase As Boolean = False)
            If aLimits.Left < Left Then Left = aLimits.Left
            If aLimits.Bottom < Bottom Then Bottom = aLimits.Bottom
            If aLimits.Right > Right Then Right = aLimits.Right
            If aLimits.Top > Top Then Top = aLimits.Top
            If bMinBase Then
                If aLimits.base < base Then base = aLimits.base
            End If
        End Sub

        Public Sub Update(Optional aLeft As Double? = Nothing, Optional aRight As Double? = Nothing, Optional aBottom As Double? = Nothing, Optional aTop As Double? = Nothing)
            If aLeft.HasValue Then Left = aLeft.Value
            If aRight.HasValue Then Right = aRight.Value
            If aBottom.HasValue Then Bottom = aBottom.Value
            If aTop.HasValue Then Top = aTop.Value
            Rectify()
        End Sub

        Public Function Clone() As TLIMITS
            Return New TLIMITS(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TLIMITS(Me)
        End Function
        Public Sub Rectify()
            TVALUES.SortTwoValues(True, Left, Right)
            TVALUES.SortTwoValues(True, Bottom, Top)
        End Sub

        Public Sub Update(aPoint As TPOINT)
            If aPoint.X < Left Then Left = aPoint.X
            If aPoint.Y < Bottom Then Bottom = aPoint.Y
            If aPoint.X > Right Then Right = aPoint.X
            If aPoint.Y > Top Then Top = aPoint.Y
        End Sub

        Public Sub Update(aPoint As dxoPoint)
            If aPoint Is Nothing Then Return
            If aPoint.X < Left Then Left = aPoint.X
            If aPoint.Y < Bottom Then Bottom = aPoint.Y
            If aPoint.X > Right Then Right = aPoint.X
            If aPoint.Y > Top Then Top = aPoint.Y
        End Sub

        Public Sub Update(aVector As TVECTOR, Optional aCS As dxfPlane = Nothing)
            Dim v1 As TVECTOR
            v1 = aVector
            If aCS IsNot Nothing Then v1 = v1.WithRespectTo(aCS.Strukture)
            If v1.Y < Bottom Then Bottom = v1.Y
            If v1.Y > Top Then Top = v1.Y
            If v1.X < Left Then Left = v1.X
            If v1.X > Right Then Right = v1.X
            If v1.Z < MinZ Then MinZ = v1.Z
            If v1.Z > MaxZ Then MaxZ = v1.Z
        End Sub
        Public Sub Update(aPoints As TPOINTS)
            Dim i As Integer
            For i = 1 To aPoints.Count
                Update(aPoints.Item(i))
            Next i
        End Sub
        Public Function Contains(aLimits As TLIMITS) As Boolean
            Dim rFitsLeftToRight As Boolean = False
            Dim rFitsTopToBottom As Boolean = False
            Return Contains(aLimits, rFitsLeftToRight, rFitsTopToBottom)
        End Function
        Public Function Contains(aLimits As TLIMITS, ByRef rFitsLeftToRight As Boolean, ByRef rFitsTopToBottom As Boolean) As Boolean
            rFitsLeftToRight = False
            rFitsTopToBottom = False
            rFitsLeftToRight = aLimits.Left >= Left And aLimits.Right <= Right
            rFitsTopToBottom = aLimits.Bottom >= Bottom And aLimits.Top <= Top
            Return rFitsLeftToRight And rFitsTopToBottom
        End Function
        Public Sub Scale(Optional aXScale As Double? = 1, Optional aYScale As Double? = 1)
            If aXScale.HasValue Then
                Left *= aXScale.Value
                Right *= aXScale.Value
            End If
            If aYScale.HasValue Then
                Top *= aYScale.Value
                Bottom *= aYScale.Value
            End If
        End Sub

        Public Sub Translate(aDX As Double, aDY As Double)
            Left += aDX
            Right += aDX
            Top += aDY
            Bottom += aDY
        End Sub
#End Region 'Methods
    End Structure 'TLIMITS

    Friend Structure TPATH
        Implements ICloneable
#Region "Members"
        Public Color As dxxColors
        Public Domain As dxxDrawingDomains
        Public Filled As Boolean
        Public GraphicType As dxxGraphicTypes
        Public LayerName As String
        Public Linetype As String
        Public LineWeight As dxxLineWeights
        Public LTScale As Double
        Public Identifier As String
        Public Plane As TPLANE
        Public Suppressed As Boolean
        Private _Loops() As TVECTORS
        Private _Init As Boolean
        Private _Relative As Boolean
        Public Tag As String
#End Region 'Members
#Region "Constructors"

        Public Sub New(Optional aTag As String = "")
            'init ----------------------------------
            Color = dxxColors.ByBlock
            Domain = dxxDrawingDomains.Model
            Filled = False
            GraphicType = dxxGraphicTypes.Undefined
            LayerName = "0"
            Linetype = dxfLinetypes.Continuous
            LineWeight = dxxLineWeights.ByDefault
            LTScale = 1
            Plane = TPLANE.World
            Suppressed = False
            ReDim _Loops(-1)
            _Init = True
            _Relative = False
            Tag = ""
            Identifier = ""
            'init ----------------------------------
            Tag = aTag
        End Sub

        Public Sub New(aPath As TPATH)
            'init ----------------------------------
            Color = aPath.Color
            Domain = aPath.Domain
            Filled = aPath.Filled
            GraphicType = aPath.GraphicType
            LayerName = aPath.LayerName
            Linetype = aPath.Linetype
            LineWeight = aPath.LineWeight
            LTScale = aPath.LTScale
            Plane = New TPLANE(aPath.Plane)
            Suppressed = aPath.Suppressed
            ReDim _Loops(-1)
            _Init = True
            _Relative = aPath._Relative
            Tag = aPath.Tag
            Identifier = ""
            'init ----------------------------------
            If aPath._Init Then _Loops = aPath._Loops.Clone()
        End Sub

        Public Sub New(aDomain As dxxDrawingDomains, Optional aDisplayVars As TDISPLAYVARS? = Nothing, Optional aPlane As dxfPlane = Nothing, Optional aLoop As TVECTORS? = Nothing)
            'init ----------------------------------
            Color = dxxColors.ByBlock
            Domain = dxxDrawingDomains.Model
            Filled = False
            GraphicType = dxxGraphicTypes.Undefined
            LayerName = "0"
            Linetype = dxfLinetypes.Continuous
            LineWeight = dxxLineWeights.ByDefault
            LTScale = 1
            Plane = TPLANE.World
            Suppressed = False
            ReDim _Loops(-1)
            _Init = True
            _Relative = False
            Tag = ""
            Identifier = ""
            'init ----------------------------------
            Domain = aDomain
            If aDisplayVars.HasValue Then DisplayVars = aDisplayVars.Value
            If aLoop.HasValue Then
                If aLoop.Value.Count > 0 Then AddLoop(aLoop.Value)
            End If
        End Sub

        Public Sub New(aPoints As TPOINTS, aBasePlane As TPLANE, aDisplay As TDISPLAYVARS, aDomain As dxxDrawingDomains, bFilled As Boolean, Optional aShearAngle As Double = 0, Optional aRelativeToPlane As dxfPlane = Nothing, Optional aIdentifier As String = "")
            'init ----------------------------------
            Color = aDisplay.Color
            Domain = aDomain
            Filled = bFilled
            GraphicType = dxxGraphicTypes.Undefined
            LayerName = aDisplay.LayerName
            Linetype = aDisplay.Linetype
            LineWeight = aDisplay.LineWeight
            LTScale = 1
            Plane = IIf(aRelativeToPlane Is Nothing, aBasePlane, New TPLANE(aRelativeToPlane))
            Suppressed = False
            ReDim _Loops(-1)
            _Init = True
            _Relative = aRelativeToPlane IsNot Nothing
            Tag = ""
            Identifier = aIdentifier
            'init ----------------------------------
            If aPoints.Count <= 0 Then Return
            AddLoop(aPoints.ToPlaneVectors(aBasePlane, aShearAngle, aRelativeToPlane))



        End Sub

        Public Sub New(aShape As TSHAPE, aBasePlane As TPLANE, aDisplay As TDISPLAYVARS, aDomain As dxxDrawingDomains, bFilled As Boolean, Optional aShearAngle As Double = 0, Optional aRelativeToPlane As dxfPlane = Nothing, Optional aIdentifier As String = "")
            'init ----------------------------------
            Color = aDisplay.Color
            Domain = aDomain
            Filled = bFilled
            GraphicType = dxxGraphicTypes.Undefined
            LayerName = aDisplay.LayerName
            Linetype = aDisplay.Linetype
            LineWeight = aDisplay.LineWeight
            LTScale = 1
            Plane = IIf(aRelativeToPlane Is Nothing, aBasePlane, New TPLANE(aRelativeToPlane))
            Suppressed = False
            ReDim _Loops(-1)
            _Init = True
            _Relative = aRelativeToPlane IsNot Nothing
            Tag = ""
            Identifier = aIdentifier
            'init ----------------------------------
            If aShape.Path.Count <= 0 Then Return
            AddLoop(aShape.Path.ToPlaneVectors(aBasePlane, aShearAngle, aRelativeToPlane))



        End Sub
        Public Sub New(aShape As dxoShape, aBasePlane As TPLANE, aDisplay As TDISPLAYVARS, aDomain As dxxDrawingDomains, bFilled As Boolean, Optional aShearAngle As Double = 0, Optional aRelativeToPlane As dxfPlane = Nothing, Optional aIdentifier As String = "")
            'init ----------------------------------
            Color = aDisplay.Color
            Domain = aDomain
            Filled = bFilled
            GraphicType = dxxGraphicTypes.Undefined
            LayerName = aDisplay.LayerName
            Linetype = aDisplay.Linetype
            LineWeight = aDisplay.LineWeight
            LTScale = 1
            Plane = IIf(dxfPlane.IsNull(aRelativeToPlane), aBasePlane, New TPLANE(aRelativeToPlane))
            Suppressed = False
            ReDim _Loops(-1)
            _Init = True
            _Relative = aRelativeToPlane IsNot Nothing
            Tag = ""
            Identifier = aIdentifier
            'init ----------------------------------
            If aShape Is Nothing Then Return
            If aShape.Path.Count <= 0 Then Return
            AddLoop(aShape.Path.ToPlaneVectors(aBasePlane, aShearAngle, aRelativeToPlane))



        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Property Relative As Boolean
            Get
                Return _Relative
            End Get
            Set(value As Boolean)
                _Relative = value
            End Set
        End Property
        Public ReadOnly Property LoopCount As Integer
            Get
                If Not _Init Then
                    _Init = True
                    ReDim _Loops(-1)
                End If
                Return _Loops.Count
            End Get
        End Property
        Public Property DisplayVars As TDISPLAYVARS
            Get
                Return New TDISPLAYVARS With {
                    .Suppressed = Suppressed,
                    .Linetype = Linetype,
                    .LineWeight = LineWeight,
                    .LayerName = LayerName,
                    .LTScale = LTScale,
                    .Color = Color}
            End Get
            Set(value As TDISPLAYVARS)

                Suppressed = value.Suppressed
                Linetype = value.Linetype
                LineWeight = value.LineWeight
                LayerName = value.LayerName
                LTScale = value.LTScale
                Color = value.Color

            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function IsEqualTo(B As TPATH, Optional bCompareLoops As Boolean = False, Optional bIgnoreDisplayProperties As Boolean = False) As Boolean

            If Not bIgnoreDisplayProperties Then
                If Color <> B.Color Then Return False
                If LayerName <> B.LayerName Then Return False
                If Linetype <> B.Linetype Then Return False
                If LineWeight <> B.LineWeight Then Return False

            End If

            If Filled <> B.Filled Then Return False
            If Suppressed <> B.Suppressed Then Return False
            If Relative <> B.Relative Then Return False
            If Relative And B.Relative Then
                If Not Plane.IsEqualTo(B.Plane, bCompareDirections:=True, bCompareDimensions:=False, bCompareOrigin:=bCompareLoops) Then Return False
            End If
            If bCompareLoops Then
                If LoopCount <> B.LoopCount Then Return False
                Dim myLoop As TVECTORS
                Dim bLoop As TVECTORS
                For i As Integer = 1 To LoopCount
                    myLoop = Looop(i)
                    bLoop = B.Looop(i)
                    If myLoop.Count <> bLoop.Count Then Return False
                    For j As Integer = 1 To myLoop.Count
                        If myLoop.Item(j) <> bLoop.Item(i) Then Return False
                    Next
                Next
            End If
            Return True
        End Function
        Public Function ToGraphicPath(aPixelSize As Integer, aDeviceSize As System.Drawing.Size) As System.Drawing.Drawing2D.GraphicsPath
            Dim _rVal As New System.Drawing.Drawing2D.GraphicsPath
            Dim li As Integer
            Dim lvs As TVECTORS
            For li = 1 To LoopCount
                lvs = _Loops(li - 1)
                If lvs.Count > 0 Then
                    _rVal.AddPath(lvs.ToGraphicPath(aPixelSize, aDeviceSize), False)
                End If
            Next li
            Return _rVal
        End Function
        Public Function AddLoop(aLoop As TVECTORS) As Boolean
            Dim _rVal As Boolean = True
            If Not _Init Then
                ReDim _Loops(0)
                _Loops(0) = aLoop
            Else
                If LoopCount < Integer.MaxValue Then
                    System.Array.Resize(_Loops, LoopCount + 1)
                    _Loops(LoopCount - 1) = aLoop
                Else
                    _rVal = False
                    System.Diagnostics.Debug.WriteLine("loop overflow")
                End If
            End If
            Return _rVal
        End Function
        Public Function Looop(aIndex As Integer) As TVECTORS
            'base 1
            If aIndex < 1 Or aIndex > LoopCount Then Return TVECTORS.Zero
            Return _Loops(aIndex - 1)
        End Function
        Public Sub AppendToLooop(aIndex As Integer, aPathVectors As TVECTORS)
            'base 1
            If aIndex < 1 Or aIndex > LoopCount Or aPathVectors.Count <= 0 Then Return
            _Loops(aIndex - 1).Append(aPathVectors)
        End Sub


        Public Sub SetLoop(aIndex As Integer, value As TVECTORS)
            If aIndex < 1 Or aIndex > LoopCount Then Return
            _Loops(aIndex - 1) = value
        End Sub
        Public Sub Clear()
            _Init = True
            ReDim _Loops(-1)

        End Sub
        Friend Sub Print(Optional aIndent As Integer = 0)
            Dim i As Integer
            Dim vs As TVECTORS
            Dim p1 As TPOINT
            For i = 1 To LoopCount
                vs = _Loops(i - 1)
                System.Diagnostics.Debug.WriteLine(New String(" ", aIndent) & "LOOP - " & i.ToString)
                For j As Integer = 1 To vs.Count
                    p1 = New TPOINT(vs.Item(j))
                    System.Diagnostics.Debug.WriteLine(New String(" ", aIndent + 4) & p1.ToString)
                Next j
            Next i
        End Sub
        Public Sub ConvertToWorld()
            If Not _Relative Then Return
            For li As Integer = 1 To LoopCount
                For vi As Integer = 1 To _Loops(li - 1).Count
                    _Loops(li - 1).SetItem(vi, Plane.WorldVector(_Loops(li - 1).Item(vi)))
                Next vi
            Next li
            _Relative = False
        End Sub
        Public Function ConvertedToWorld() As TPATH
            Dim _rVal As TPATH = Clone()
            If _Relative Then
                _rVal.ConvertToWorld()
            End If
            Return _rVal
        End Function
        Public Sub AddLine(aSP As TVECTOR, aEP As TVECTOR)
            AddLoop(New TVECTORS)
            _Loops(LoopCount - 1).Add(aSP, TVALUES.ToByte(dxxVertexStyles.MOVETO))
            _Loops(LoopCount - 1).Add(aEP, TVALUES.ToByte(dxxVertexStyles.LINETO))
        End Sub
        Public Function Clone() As TPATH

            Return New TPATH(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TPATH(Me)
        End Function
        Public Function WithRespectToPlane(aNewPlane As TPLANE, aXScale As Double, aYScale As Double, aZScale As Double, aRotation As Double) As TPATH
            Dim _rVal As TPATH
            Dim bPlane As New TPLANE("")
            Dim i As Integer
            bPlane = aNewPlane
            _rVal = Clone()
            If aRotation <> 0 Then bPlane.Revolve(aRotation, False)
            _rVal.Plane = bPlane
            _rVal.Plane.Origin = bPlane.WorldVector(Plane.Origin)
            For i = 1 To _rVal.LoopCount
                _rVal.SetLoop(i, Looop(i).WithRespectToPlane(bPlane, aXScale, aYScale, aZScale, 0))
            Next i
            Return _rVal
        End Function
        Public Function WithRespectToPlane(aPlane As TPLANE) As TPATH
            Dim _rVal As TPATH = Clone()
            _rVal.Plane = aPlane
            _rVal.Plane.Origin = Plane.Origin.WithRespectTo(aPlane, 12)
            Dim i As Integer
            For i = 1 To LoopCount
                _rVal.SetLoop(i, Looop(i).WithRespectToPlane(aPlane, 12))
            Next i
            Return _rVal
        End Function
        Public Overrides Function ToString() As String
            Dim _rVal As String = "TPATH"
            _rVal += " (loops = " & LoopCount.ToString & ")"
            If Identifier IsNot Nothing Then
                If Identifier <> "" Then _rVal += " [" & Identifier & "]"
            End If
            Return _rVal
        End Function
        Public Function ToRelative(aScaleVector As dxfVector) As TPATH
            Dim _rVal As TPATH = Clone()
            Dim li As Integer
            Dim vi As Integer
            Dim v1 As TVECTOR
            Dim sclr As TVECTOR
            Dim bScl As Boolean = aScaleVector IsNot Nothing
            Dim aLoop As TVECTORS
            If bScl Then sclr = New TVECTOR(aScaleVector)

            For li = 1 To LoopCount
                aLoop = Looop(li)
                For vi = 1 To aLoop.Count
                    v1 = aLoop.Item(vi)
                    If Not Relative Then v1 = v1.WithRespectTo(Plane)
                    If bScl Then v1 *= sclr
                    aLoop.SetItem(vi, v1)
                Next vi
                SetLoop(li, aLoop)
            Next li
            _rVal._Relative = True
            Return _rVal
        End Function
        Public Sub Translate(aTranslation As TVECTOR)
            If TVECTOR.IsNull(aTranslation) Then Return
            Plane.Origin += aTranslation
            If Not Relative Then
                Dim aLoop As TVECTORS
                For li As Integer = 1 To LoopCount
                    aLoop = Looop(li)
                    aLoop.Translate(aTranslation)
                    SetLoop(li, aLoop)
                Next
            End If
        End Sub
#End Region 'Methods
#Region "Shared Methods"
        Public Shared Function ToEntities(aPath As TPATH, Optional aCollector As colDXFEntities = Nothing) As colDXFEntities
            Dim _rVal As colDXFEntities
            If aCollector IsNot Nothing Then _rVal = aCollector Else _rVal = New colDXFEntities
            'On Error Resume Next
            If aPath.LoopCount <= 0 Then Return _rVal
            Dim ptype As Byte
            Dim vWorld As TVECTOR
            Dim vWorldLast As TVECTOR
            Dim dsp As TDISPLAYVARS = aPath.DisplayVars
            Dim aLn As dxeLine
            Dim idx As Long
            Dim aBezID As Integer = 3
            Dim pBezID As Integer
            Dim li As Integer
            Dim aBz As dxeBezier
            Dim vWorldPts As TVECTORS
            For li = 1 To aPath.LoopCount
                vWorldPts = aPath.Looop(li)
                If vWorldPts.Count > 0 Then
                    vWorldLast = vWorldPts.Item(1)
                    For idx = 1 To vWorldPts.Count
                        'render the path in the image
                        vWorld = vWorldPts.Item(idx)
                        If aPath.Relative Then vWorld = aPath.Plane.WorldVector(vWorld)
                        ptype = vWorld.Code
                        If (ptype And dxfGlobals.PT_BEZIERTO) = 0 Then
                            aBezID = 0
                        End If
                        Select Case ptype And Not dxfGlobals.PT_CLOSEFIGURE
                         '+++++++++++++++++++++++++++++++++++++++++++
                            Case dxfGlobals.PT_PIXELTO ' pixel vector
                                '+++++++++++++++++++++++++++++++++++++++++++
                                aBezID = 0
                         '+++++++++++++++++++++++++++++++++++++++++++++++++++
                            Case dxfGlobals.PT_LINETO ' Straight line segment
                                '+++++++++++++++++++++++++++++++++++++++++++++++++++
                                If vWorldLast.DistanceTo(vWorld) > 0 Then
                                    aLn = _rVal.AddLineV(vWorldLast, vWorld, aMinLength:=0.001)
                                    aLn.DisplayStructure = dsp
                                End If
                                aBezID = 0
                         '+++++++++++++++++++++++++++++++++++++++++++
                            Case dxfGlobals.PT_BEZIERTO ' Curve segment
                                '+++++++++++++++++++++++++++++++++++++++++++
                                aBezID += 1
                                If (aBezID = pBezID) Then
                                    If idx >= 4 Then
                                        aBz = New dxeBezier With {.PlaneV = aPath.Plane}
                                        aBz.StartPtV = vWorldPts.Item(idx - 3)
                                        aBz.ControlPt1V = vWorldPts.Item(idx - 2)
                                        aBz.ControlPt2V = vWorldPts.Item(idx - 1)
                                        aBz.EndPtV = vWorldPts.Item(idx)
                                        aBz.DisplayStructure = dsp
                                        _rVal.Add(aBz)
                                    End If
                                End If
                         '+++++++++++++++++++++++++++++++++++++++++++
                            Case dxfGlobals.PT_MOVETO ' Move current drawing vector
                                '+++++++++++++++++++++++++++++++++++++++++++
                                aBezID = 0
                        End Select
                        '+++++++++++++++++++++++++++++++++++++++++++
                        If ptype And dxfGlobals.PT_CLOSEFIGURE Then
                            '+++++++++++++++++++++++++++++++++++++++++++
                        End If
                        'Application.DoEvents()
                        vWorldLast = vWorld
                    Next idx
                End If
                'Application.DoEvents()
            Next li
            Return _rVal
        End Function
        Public Shared Function ToArcLines(aPath As TPATH, Optional aBezierDivisions As Integer = 100) As TSEGMENTS
            Dim _rVal As New TSEGMENTS(0)

            Dim ptype As Byte
            Dim vWorld As TVECTOR
            Dim vWorldLast As TVECTOR

            Dim idx As Long
            Dim aBezID As Long
            Dim pBezID As Integer
            Dim li As Integer
            Dim i As Integer
            Dim vWorldPts As TVECTORS
            pBezID = 3
            If aPath.LoopCount > 0 Then
                For li = 1 To aPath.LoopCount
                    vWorldPts = aPath.Looop(li)
                    If vWorldPts.Count > 0 Then
                        vWorldLast = vWorldPts.Item(1)
                        For idx = 1 To vWorldPts.Count
                            'render the path in the image
                            vWorld = vWorldPts.Item(idx)
                            ptype = vWorld.Code
                            If (ptype And dxfGlobals.PT_BEZIERTO) = 0 Then
                                aBezID = 0
                            End If
                            Select Case ptype And Not dxfGlobals.PT_CLOSEFIGURE
                         '+++++++++++++++++++++++++++++++++++++++++++
                                Case dxfGlobals.PT_PIXELTO ' pixel vector
                                    '+++++++++++++++++++++++++++++++++++++++++++
                                    aBezID = 0
                         '+++++++++++++++++++++++++++++++++++++++++++++++++++
                                Case dxfGlobals.PT_LINETO ' Straight line segment
                                    '+++++++++++++++++++++++++++++++++++++++++++++++++++
                                    If vWorldLast.DistanceTo(vWorld) > 0 Then
                                        _rVal.Add(vWorldLast, vWorld)
                                    End If
                                    aBezID = 0
                         '+++++++++++++++++++++++++++++++++++++++++++
                                Case dxfGlobals.PT_BEZIERTO ' Curve segment
                                    '+++++++++++++++++++++++++++++++++++++++++++
                                    aBezID += 1
                                    If (aBezID = pBezID) Then
                                        Dim aB As New TBEZIER(vWorldPts.Item(idx - 3), vWorldPts.Item(idx - 2), vWorldPts.Item(idx - 1), vWorldPts.Item(idx), aPath.Plane)
                                        If idx >= 4 Then
                                            Dim aLns As TLINES = aB.PhantomLines(aBezierDivisions, True)
                                            For i = 1 To aLns.Count
                                                _rVal.Add(aLns.Item(i))
                                            Next i
                                        End If
                                    End If
                         '+++++++++++++++++++++++++++++++++++++++++++
                                Case dxfGlobals.PT_MOVETO ' Move current drawing vector
                                    '+++++++++++++++++++++++++++++++++++++++++++
                                    aBezID = 0
                            End Select
                            '+++++++++++++++++++++++++++++++++++++++++++
                            If ptype And dxfGlobals.PT_CLOSEFIGURE Then
                                '+++++++++++++++++++++++++++++++++++++++++++
                            End If
                            'Application.DoEvents()
                            vWorldLast = vWorld
                        Next idx
                    End If
                    'Application.DoEvents()
                Next li
            End If
            Return _rVal
        End Function
        Public Shared Function CIRCLE(aPlane As TPLANE, Optional bJustMoveTo As Boolean = False) As TPATH
            Dim _rVal As New TPATH(dxxDrawingDomains.Model, aPlane:=New dxfPlane(aPlane))
            If aPlane.Height > 0 Then
                Dim aArc As New TARC With {.Plane = aPlane, .Radius = aPlane.Height}
                _rVal.AddLoop(TBEZIER.ArcPath(aArc, True))
            End If
            Return _rVal
        End Function
        Public Shared Function POINT(aMode As dxxPointModes, aSize As Double, aPlane As TPLANE, aExtentPts As TVECTORS, aDisplaySettings As TDISPLAYVARS) As TPATH
            Dim _rVal As TPATH
            Dim diag As Double
            If aSize = 0 Then aSize = 5
            Dim bAddSqr As Boolean
            Dim bAddCircle As Boolean
            Dim bAddDot As Boolean
            Dim bAddCross As Boolean
            Dim bAddX As Boolean
            Dim bAddTick As Boolean
            Dim wrld As New TVECTORS
            aExtentPts = New TVECTORS
            If aMode >= 96 Then
                bAddSqr = True
                bAddCircle = True
                aMode -= 96
            End If
            If aMode >= 64 Then
                bAddSqr = True
                aMode -= 64
            End If
            If aMode >= 32 Then
                bAddCircle = True
                aMode -= 32
            End If
            If aMode = dxxPointModes.Dot Then
                bAddDot = True
            ElseIf aMode = dxxPointModes.Cross Then
                bAddCross = True
            ElseIf aMode = dxxPointModes.X Then
                bAddX = True
            ElseIf aMode = dxxPointModes.Tick Then
                bAddTick = True
            End If
            Dim aAr As New TARC
            Dim v1 As TVECTOR
            Dim P1 As TVECTOR
            Dim v2 As TVECTOR
            Dim bzPts As TVECTORS
            Dim j As Integer
            diag = Math.Sqrt(2 * aSize ^ 2) / 2
            P1 = aPlane.Origin
            wrld.Add(P1, TVALUES.ToByte(dxxVertexStyles.MOVETO))
            If bAddDot Then wrld.Add(P1, TVALUES.ToByte(dxxVertexStyles.PIXEL))
            aExtentPts.Add(P1)
            If bAddCircle Then
                aAr.Radius = aSize / 2
                aAr.Plane = aPlane
                aAr.StartAngle = 0
                aAr.EndAngle = 360
                'aAr.SpannedAngle = 360
                bzPts = TBEZIER.ArcPath(aAr, True)
                For j = 1 To bzPts.Count
                    v1 = bzPts.Item(j)
                    If j = 1 Then wrld.Add(v1, TVALUES.ToByte(dxxVertexStyles.MOVETO)) Else wrld.Add(v1, TVALUES.ToByte(dxxVertexStyles.BEZIERTO))
                Next j
                aExtentPts.Add(New TVECTOR(aPlane, aSize / 2, 0))
                aExtentPts.Add(New TVECTOR(aPlane, 0, aSize / 2))
                aExtentPts.Add(New TVECTOR(aPlane, -aSize / 2, 0))
                aExtentPts.Add(New TVECTOR(aPlane, 0, -aSize / 2))
            End If
            If bAddTick Then
                v1 = P1
                v2 = New TVECTOR(aPlane, 0, 0.5 * aSize)
                wrld.Add(v1, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                wrld.Add(v2, TVALUES.ToByte(dxxVertexStyles.LINETO))
                aExtentPts.Add(v2)
            End If
            If bAddCross Then
                v1 = New TVECTOR(aPlane, 0, -diag)
                v2 = New TVECTOR(aPlane, 0, diag)
                wrld.Add(v1, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                wrld.Add(v2, TVALUES.ToByte(dxxVertexStyles.LINETO))
                aExtentPts.Add(v1)
                aExtentPts.Add(v2)
                v1 = New TVECTOR(aPlane, -diag)
                v2 = New TVECTOR(aPlane, diag)
                wrld.Add(v1, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                wrld.Add(v2, TVALUES.ToByte(dxxVertexStyles.LINETO))
                aExtentPts.Add(v1)
                aExtentPts.Add(v2)
            End If
            If bAddX Then
                v1 = aPlane.AngleVector(225, diag, False)
                v2 = aPlane.AngleVector(45, diag, False)
                wrld.Add(v1, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                wrld.Add(v2, TVALUES.ToByte(dxxVertexStyles.LINETO))
                aExtentPts.Add(v1)
                aExtentPts.Add(v2)
                v1 = aPlane.AngleVector(315, diag, False)
                v2 = aPlane.AngleVector(135, diag, False)
                wrld.Add(v1, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                wrld.Add(v2, TVALUES.ToByte(dxxVertexStyles.LINETO))
                aExtentPts.Add(v1)
                aExtentPts.Add(v2)
            End If
            If bAddSqr Then
                v1 = aPlane.AngleVector(225, diag, False)
                v2 = aPlane.AngleVector(315, diag, False)
                wrld.Add(v1, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                wrld.Add(v2, TVALUES.ToByte(dxxVertexStyles.LINETO))
                aExtentPts.Add(v2)
                v2 = aPlane.AngleVector(45, diag, False)
                wrld.Add(v2, TVALUES.ToByte(dxxVertexStyles.LINETO))
                aExtentPts.Add(v2)
                v2 = aPlane.AngleVector(135, diag, False)
                wrld.Add(v2, TVALUES.ToByte(dxxVertexStyles.LINETO))
                aExtentPts.Add(v2)
                v2 = aPlane.AngleVector(225, diag, False)
                wrld.Add(v2, TVALUES.ToByte(dxxVertexStyles.LINETO))
                aExtentPts.Add(v2)
            End If
            _rVal = New TPATH(dxxDrawingDomains.Model, aDisplaySettings, New dxfPlane(aPlane), wrld)
            Return _rVal
        End Function
        Public Shared Function PILL(aPlane As TPLANE) As TPATH
            Dim _rVal As New TPATH(dxxDrawingDomains.Model, aPlane:=New dxfPlane(aPlane))
            If aPlane.Width <= 0 Or aPlane.Height <= 0 Then Return _rVal
            Dim aArc As New TARC
            Dim Crns As TVECTORS
            Dim aAr1 As TVECTORS
            Dim aAr2 As TVECTORS
            Dim rad As Double
            Dim d1 As Double
            rad = 0.5 * aPlane.Height
            d1 = 0.5 * aPlane.Width - rad
            Crns = New TVECTORS
            aArc.Plane = aPlane
            aArc.Radius = aPlane.Height
            aArc.Plane.Origin = New TVECTOR(aPlane, -d1)
            aArc.StartAngle = 90
            aArc.EndAngle = 270
            aAr1 = TBEZIER.ArcPath(aArc, False)
            aArc.Plane.Origin = New TVECTOR(aPlane, d1)
            aArc.StartAngle = 270
            aArc.EndAngle = 90
            aAr2 = TBEZIER.ArcPath(aArc, False)
            Crns = aAr1
            aAr2.SetCode(1, aCode:=TVALUES.ToByte(dxxVertexStyles.LINETO))
            Crns.Append(aAr2)
            Crns.Add(aAr1.Item(1), aCode:=TVALUES.ToByte(dxxVertexStyles.LINETO))
            _rVal.AddLoop(Crns)
            Return _rVal
        End Function
        Public Shared Function RECTANGLE(aRect As TPLANE, bIncludeBaseline As Boolean, Optional aDomain As dxxDrawingDomains = dxxDrawingDomains.Model) As TPATH
            Dim _rVal As New TPATH(aDomain, aPlane:=New dxfPlane(aRect))
            If aRect.Width > 0 Or aRect.Height > 0 Then
                Dim Crns As TVECTORS = aRect.Corners(True, True)
                If bIncludeBaseline And aRect.Descent > 0 Then
                    Crns.Add(aRect.Point(dxxRectanglePts.BaselineLeft, True, aCode:=dxxVertexStyles.MOVETO))
                    Crns.Add(aRect.Point(dxxRectanglePts.BaselineRight, True, aCode:=dxxVertexStyles.LINETO))
                End If
                _rVal.AddLoop(Crns)
            End If
            Return _rVal
        End Function
        Public Shared Function TRIANGLE(aRect As TPLANE, Optional bCenterCenter As Boolean = False) As TPATH
            Dim _rVal As New TPATH(dxxDrawingDomains.Model, aPlane:=New dxfPlane(aRect))
            If aRect.Width > 0 Or aRect.Height > 0 Then
                Dim Crns As New TVECTORS
                If Not bCenterCenter Then
                    Crns.Add(aRect.Origin, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                    Crns.Add(aRect.Vector(-aRect.Width / 2, -aRect.Height), TVALUES.ToByte(dxxVertexStyles.LINETO))
                    Crns.Add(aRect.Vector(aRect.Width / 2, -aRect.Height), TVALUES.ToByte(dxxVertexStyles.LINETO))
                    Crns.Add(aRect.Origin, TVALUES.ToByte(dxxVertexStyles.LINETO))
                    Crns.Add(aRect.Vector(-aRect.Width / 2, -aRect.Height), TVALUES.ToByte(dxxVertexStyles.LINETO))
                Else
                    Crns.Add(aRect.Vector(0, aRect.Height / 2), TVALUES.ToByte(dxxVertexStyles.MOVETO))
                    Crns.Add(aRect.Vector(-aRect.Width / 2, -aRect.Height / 2), TVALUES.ToByte(dxxVertexStyles.LINETO))
                    Crns.Add(aRect.Vector(aRect.Width / 2, -aRect.Height / 2), TVALUES.ToByte(dxxVertexStyles.LINETO))
                    Crns.Add(aRect.Vector(0, aRect.Height / 2), TVALUES.ToByte(dxxVertexStyles.LINETO))
                    Crns.Add(aRect.Vector(-aRect.Width / 2, -aRect.Height / 2), TVALUES.ToByte(dxxVertexStyles.LINETO))
                End If
                _rVal.AddLoop(Crns)
            End If
            Return _rVal
        End Function
        Public Shared Function UCS(aUCS As TPLANE, Optional aOrigin As dxfVector = Nothing, Optional aColor As dxxColors = dxxColors.BlackWhite, Optional aLength As Double = 1, Optional bSuppressX As Boolean = False, Optional bSuppressY As Boolean = False, Optional bSuppressZ As Boolean = False, Optional bInvertY As Boolean = False) As TPATH
            If TPLANE.IsNull(aUCS) Then Return New TPATH(dxxDrawingDomains.Screen)
            Dim aPlane As New TPLANE(aUCS)
            If aOrigin IsNot Nothing Then aPlane.Origin = New TVECTOR(aOrigin)
            Dim _rVal As New TPATH(dxxDrawingDomains.Screen, aPlane:=New dxfPlane(aPlane)) With {.Color = aColor, .Linetype = dxfLinetypes.Continuous}
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim v3 As TVECTOR
            Dim yDir As TVECTOR
            Dim wvs As New TVECTORS
            aLength = Math.Abs(aLength)
            If aLength = 0 Then aLength = 0.2 * aPlane.Width
            If aLength = 0 Then aLength = 0.2 * aPlane.Height
            If aLength = 0 Then aLength = 1
            Dim aAxisLength As Double = aLength
            Dim ht As Double = 0.2 * aAxisLength
            Dim wd As Double = 0.8 * ht
            Dim org As TVECTOR = aPlane.Origin
            If Not bInvertY Then
                yDir = aPlane.YDirection
            Else
                yDir = aPlane.YDirection * -1
            End If
            Dim xDir As TVECTOR = aPlane.XDirection
            Dim zDir As TVECTOR = aPlane.ZDirection
            'the axis lines
            If Not bSuppressX Then
                Dim epX As TVECTOR = org + (xDir * aLength)
                wvs.Add(org, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                wvs.Add(epX, TVALUES.ToByte(dxxVertexStyles.LINETO))
                'x pointer
                wvs.Add(epX + (xDir * (-0.25 * aLength)) + (yDir * (0.125 * aLength)), TVALUES.ToByte(dxxVertexStyles.MOVETO))
                wvs.Add(epX, TVALUES.ToByte(dxxVertexStyles.LINETO))
                wvs.Add(epX + (xDir * (-0.25 * aLength)) + (yDir * (-0.125 * aLength)), TVALUES.ToByte(dxxVertexStyles.LINETO))
                v1 = epX + (xDir * (0.75 * wd))
                '.x text
                v2 = v1 + (yDir * (-0.5 * ht)) + (xDir * (-0.5 * wd))
                v3 = v1 + (yDir * (0.5 * ht)) + (xDir * (0.5 * wd))
                wvs.Add(v2, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                wvs.Add(v3, TVALUES.ToByte(dxxVertexStyles.LINETO))
                wvs.Add(v2 + (xDir * wd), TVALUES.ToByte(dxxVertexStyles.MOVETO))
                wvs.Add(v3 + (xDir * -wd), TVALUES.ToByte(dxxVertexStyles.LINETO))
            End If
            If Not bSuppressY Then
                Dim epY As TVECTOR = org + yDir * aLength
                wvs.Add(org, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                wvs.Add(epY, TVALUES.ToByte(dxxVertexStyles.LINETO))
                'y pointer
                wvs.Add(epY + (yDir * (-0.25 * aLength)) + (xDir * (0.125 * aLength)), TVALUES.ToByte(dxxVertexStyles.MOVETO))
                wvs.Add(epY, TVALUES.ToByte(dxxVertexStyles.LINETO))
                wvs.Add(epY + (yDir * (-0.25 * aLength)) + (xDir * (-0.125 * aLength)), TVALUES.ToByte(dxxVertexStyles.LINETO))
                'y text
                v2 = epY + (yDir * (0.85 * ht))
                v1 = v2 + (yDir * (-0.5 * ht))
                v3 = v2 + (xDir * (-0.5 * wd)) + (yDir * (0.5 * ht))
                wvs.Add(v1, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                wvs.Add(v2, TVALUES.ToByte(dxxVertexStyles.LINETO))
                wvs.Add(v3, TVALUES.ToByte(dxxVertexStyles.LINETO))
                wvs.Add(v2, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                wvs.Add(v3 + (xDir * wd), TVALUES.ToByte(dxxVertexStyles.LINETO))
            End If
            If Not bSuppressZ Then
                Dim epZ As TVECTOR = org + zDir * aLength
                wvs.Add(org, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                wvs.Add(epZ, TVALUES.ToByte(dxxVertexStyles.LINETO))
                'Z pointer
                v2 = epZ.Clone
                v1 = epZ + (zDir * -0.25 * aAxisLength)
                v3 = v1.Clone
                v1 += xDir * (0.125 * aAxisLength)
                v3 += xDir * (-0.125 * aAxisLength)
                wvs.Add(v1, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                wvs.Add(v2, TVALUES.ToByte(dxxVertexStyles.LINETO))
                wvs.Add(v3, TVALUES.ToByte(dxxVertexStyles.LINETO))
                'letter Z
                v1 = epZ + zDir * (0.35 * ht)
                v2 = v1 + xDir * (0.5 * wd)
                v3 = v1 + xDir * (-0.5 * wd)
                v1 = v3.Clone
                wvs.Add(v2, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                wvs.Add(v3, TVALUES.ToByte(dxxVertexStyles.LINETO))
                v2 += zDir * ht
                v3 += zDir * ht
                wvs.Add(v2, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                wvs.Add(v3, TVALUES.ToByte(dxxVertexStyles.LINETO))
                wvs.Add(v2, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                wvs.Add(v1, TVALUES.ToByte(dxxVertexStyles.LINETO))
            End If
            _rVal.AddLoop(wvs)
            Return _rVal
        End Function
#End Region 'Shared Methods
    End Structure 'TPATH
    Friend Structure TPATHS
        Implements ICloneable
#Region "Members"
        Public Color As dxxColors
        Public Domain As dxxDrawingDomains
        Public EntityGUID As String
        Public ExtentVectors As TVECTORS
        Public Identifier As String
        Public Linetype As String
        Public LineWeight As dxxLineWeights
        Public GraphicType As dxxGraphicTypes
        Public LTScale As Double
        Public PenWidth As Integer
        Private _PixelSize As Integer
        Public Suppressed As Boolean
        Private _LayerName As String
        Private _Members() As TPATH
        Private _Init As Boolean
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aDomain As dxxDrawingDomains = dxxDrawingDomains.Model, Optional aEntityGUID As String = "")
            'init ---------------------------
            Color = dxxColors.ByBlock
            Domain = dxxDrawingDomains.Model
            EntityGUID = String.Empty
            ExtentVectors = TVECTORS.Zero
            Identifier = String.Empty
            Linetype = String.Empty
            LineWeight = dxxLineWeights.ByDefault
            LTScale = 1
            PenWidth = 0
            _PixelSize = 1
            Suppressed = False
            _LayerName = String.Empty
            GraphicType = dxxGraphicTypes.Undefined
            _Init = True
            ReDim _Members(-1)
            'init ---------------------------
            Domain = aDomain
            'Bounds As new TPLANE("")
            ReDim _Members(-1)
            _Init = True
            EntityGUID = aEntityGUID.Trim()
        End Sub

        Public Sub New(aPaths As TPATHS, Optional bNoMembers As Boolean = False)
            'init ---------------------------
            Color = aPaths.Color
            Domain = aPaths.Domain
            EntityGUID = aPaths.EntityGUID
            ExtentVectors = New TVECTORS(aPaths.ExtentVectors)
            Identifier = aPaths.Identifier
            Linetype = aPaths.Linetypes
            LineWeight = aPaths.LineWeight
            LTScale = aPaths.LTScale
            PenWidth = aPaths.PenWidth
            _PixelSize = aPaths.PixelSize
            Suppressed = aPaths.Suppressed
            _LayerName = aPaths.LayerName
            GraphicType = aPaths.GraphicType
            _Init = True
            ReDim _Members(-1)
            'init ---------------------------
            If aPaths._Init And Not bNoMembers And aPaths.Count > 0 Then
                _Members = Force.DeepCloner.DeepClonerExtensions.DeepClone(Of TPATHS)(aPaths)._Members
                '_Members = aPaths._Members.Clone()
            End If
        End Sub
        Public Sub New(aDomain As dxxDrawingDomains, aPaths As List(Of TPATH), Optional aEntityGUID As String = "")
            'init ---------------------------
            Color = dxxColors.ByBlock
            Domain = dxxDrawingDomains.Model
            EntityGUID = String.Empty
            ExtentVectors = TVECTORS.Zero
            Identifier = String.Empty
            Linetype = String.Empty
            LineWeight = dxxLineWeights.ByDefault
            LTScale = 1
            PenWidth = 0
            _PixelSize = 1
            Suppressed = False
            _LayerName = String.Empty
            _Init = True
            GraphicType = dxxGraphicTypes.Undefined
            ReDim _Members(-1)
            'init ---------------------------
            If Not String.IsNullOrWhiteSpace(aEntityGUID) Then EntityGUID = aEntityGUID.Trim()
            If aPaths Is Nothing Then Return
            For Each mem As TPATH In aPaths
                Add(mem)
            Next
        End Sub
#End Region 'Constructors
#Region "Properties"




        Public ReadOnly Property IsEmpty As Boolean
            Get
                If Not _Init Then Return True Else Return Count <= 0
            End Get
        End Property

        Public Property PixelSize As Integer
            Get
                Return _PixelSize
            End Get
            Set(value As Integer)
                _PixelSize = value
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
#End Region'Properties
#Region "Methods"
        Public Function Bounds(aPlane As TPLANE) As TPLANE

            If TPLANE.IsNull(aPlane) Then Return aPlane
            If ExtentVectors.Count <= 0 Then Return aPlane
            Return ExtentVectors.Bounds(aPlane)
        End Function
        Public Function LayerName(Optional aIndex As Integer = 0) As String

            Dim _rVal As String = _LayerName
            If Not IsEmpty Then
                If aIndex > 0 And aIndex <= Count Then
                    _rVal = Item(aIndex).LayerName
                    If _rVal = "" Then _rVal = _LayerName
                End If
            End If

            If _rVal = "" Then _rVal = "0"
            Return _rVal
        End Function
        Public Sub SetLayerName(value As String, Optional aIndex As Integer = 0)
            If IsEmpty Then Return
            If aIndex > 0 And aIndex <= Count Then
                _Members(aIndex - 1).LayerName = value
            Else
                _LayerName = Trim(value)
            End If
        End Sub
        Public Function LayerNames(Optional aLayerToInclude As String = "") As String
            Dim _rVal As String = String.Empty
            If Not String.IsNullOrWhiteSpace(aLayerToInclude) Then TLISTS.Add(_rVal, aLayerToInclude)
            If IsEmpty Then Return _rVal


            For i As Integer = 1 To Count
                Dim aPth As TPATH = _Members(i - 1)
                If Not aPth.Suppressed Then
                    Dim lname As String = aPth.LayerName
                    If lname = "" Then lname = "0"
                    TLISTS.Add(_rVal, lname)
                End If
            Next i
            Return _rVal
        End Function
        Public Function Linetypes(Optional aLTToInclude As String = "") As String
            Dim _rVal As String = String.Empty
            If Not String.IsNullOrWhiteSpace(aLTToInclude) Then TLISTS.Add(_rVal, aLTToInclude)
            If IsEmpty Then Return _rVal

            For i As Integer = 1 To Count
                Dim aPth As TPATH = _Members(i - 1)
                If Not aPth.Suppressed Then
                    Dim lname As String = aPth.Linetype
                    If lname = "" Then lname = dxfLinetypes.ByLayer
                    TLISTS.Add(_rVal, lname)
                End If
            Next i
            Return _rVal
        End Function
        Public Function Item(aIndex As Integer) As TPATH
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return New TPATH("")
            End If
            Return _Members(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, value As TPATH)
            If IsEmpty Then Return
            If aIndex < 1 Or aIndex > Count Then Return

            _Members(aIndex - 1) = value
        End Sub
        Public Overrides Function ToString() As String
            Dim _rVal As String = "TPATHS"
            _rVal += $"[{ Count }]"
            If Identifier IsNot Nothing Then
                If Identifier <> "" Then _rVal += $" [{ Identifier }]"
            End If
            Return _rVal
        End Function
        Public Function Clone(Optional bNoMembers As Boolean = False) As TPATHS
            Return New TPATHS(Me, bNoMembers)

        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TPATHS(Me)
        End Function

        Friend Function UpdateColors(aOldValue As dxxColors, aNewValue As dxxColors) As Boolean
            If Count <= 0 Then Return False
            Dim _rVal As Boolean = False

            For i As Integer = 1 To Count

                If _Members(i - 1).Color = aOldValue Then
                    _rVal = True
                    _Members(i - 1).Color = aNewValue
                End If
            Next
            Return _rVal
        End Function
        Public Function UpdateLayers(aOldValue As String, aNewValue As String) As Boolean
            Dim _rVal As Boolean = False
            If String.Compare(aOldValue, aNewValue, True) = 0 Then Return _rVal
            For i As Integer = 1 To Count

                If TLISTS.Contains(_Members(i - 1).LayerName, aOldValue) Then
                    _rVal = True
                    _Members(i - 1).LayerName = aNewValue
                End If
            Next


            Return _rVal
        End Function
        Public Sub TransferToPlane(aFromPlane As TPLANE, aNewPlane As TPLANE, aXScale As Double, aYScale As Double, aZScale As Double, aRotation As Double, Optional bKeepOrigin As Boolean = False, Optional bReturnRelativePaths As Boolean = False)
            If TPLANE.IsNull(aNewPlane) Or TPLANE.IsNull(aFromPlane) Then Return
            Dim bPlane As TPLANE = aNewPlane
            Dim aPth As TPATH
            Dim v1 As TVECTOR
            Dim vPath As TVECTORS
            Dim sclr As New TVECTOR(aXScale, aYScale, aZScale)
            If bKeepOrigin Then bPlane.Origin = aFromPlane.Origin
            If aRotation <> 0 Then bPlane.Revolve(aRotation, False)
            If TPLANES.Compare(aFromPlane, bPlane, 3, True, True) Then
                If sclr.IsUnit(4) Then Return
            End If
            For vi As Integer = 1 To ExtentVectors.Count
                v1 = ExtentVectors.Item(vi).WithRespectTo(aFromPlane)
                v1 = bPlane.Vector(v1.X * sclr.X, v1.Y * sclr.Y, v1.Z * sclr.Z, aVertexType:=TVALUES.To_INT(v1.Code))
                ExtentVectors.SetItem(vi, v1)
            Next vi
            For ip As Integer = 1 To Count
                aPth = Item(ip)
                'get the path relative to it's plane and scaleed up
                '        aPth = pth_ToRelative(aPth, sclr)
                For li As Integer = 1 To aPth.LoopCount
                    vPath = aPth.Looop(li)
                    For vi As Integer = 1 To vPath.Count
                        v1 = vPath.Item(vi)
                        If aPth.Relative Then v1 = aPth.Plane.WorldVector(v1)
                        aPth.Relative = False
                        v1 = v1.WithRespectTo(aFromPlane)
                        v1 = bPlane.Vector(v1.X * aXScale, v1.Y * aYScale, v1.Z * aZScale, aVertexType:=TVALUES.To_INT(v1.Code))
                        If bReturnRelativePaths Then
                            v1 = v1.WithRespectTo(bPlane)
                        End If
                        vPath.SetItem(vi, v1)
                    Next vi
                    aPth.SetLoop(li, vPath)
                Next li
                If bReturnRelativePaths Then
                    aPth.Plane = bPlane
                    aPth.Relative = True
                End If
                SetItem(ip, aPth)
            Next ip
        End Sub
        Public Function BoundingRectangle(aPlane As TPLANE) As TPLANE
            If TPLANE.IsNull(aPlane) Then Return Nothing
            'get all my path vectors
            Dim aMem As TPATH
            Dim li As Integer
            Dim mVecs As New TVECTORS(0)
            Dim aVecs As TVECTORS
            For i As Integer = 1 To Count
                aMem = Item(i)
                For li = 1 To aMem.LoopCount
                    aVecs = aMem.Looop(li)
                    If aMem.Relative Then aVecs = aMem.Plane.WorldVectors(aVecs)
                    mVecs.Append(aVecs)
                Next li
            Next i
            Return mVecs.BoundingRectangle(aPlane)
        End Function
        Public Sub Translate(aTranslation As TVECTOR)
            If TVECTOR.IsNull(aTranslation) Then Return

            Dim aPth As TPATH
            For i As Integer = 1 To Count
                aPth = _Members(i - 1)
                aPth.Translate(aTranslation)
                _Members(i - 1) = aPth
            Next i
        End Sub
        Public Sub Project(aDistance As Double, aDirection As TVECTOR)
            If aDistance = 0 Then Return
            If TVECTOR.IsNull(aDirection) Then Return
            Dim aPth As TPATH
            Dim aDir As TVECTOR = aDirection.Normalized()
            Dim dsp As TVECTOR = aDir * aDistance
            'Bounds.Origin.Project(aDir, aDistance, True)
            For i As Integer = 1 To Count
                aPth = _Members(i - 1)
                aPth.Translate(dsp)
                _Members(i - 1) = aPth
            Next i
        End Sub
        Public Function Add(aPath As TPATH) As Integer

            If aPath.LoopCount <= 0 Then
                Return Count
            End If
            If Count >= Integer.MaxValue Then Return Count
            System.Array.Resize(_Members, Count + 1)
            _Members(_Members.Count - 1) = aPath
            'Application.DoEvents()
            Return Count
        End Function
        Public Sub Remove(aIndex As Integer)
            Dim newmems() As TPATH
            Dim cnt As Integer
            If aIndex < 1 Or aIndex > cnt Then Return
            Dim idx As Integer = 0
            ReDim newmems(0 To cnt - 1)
            For i As Integer = 1 To cnt
                If i <> aIndex Then
                    newmems(idx) = Item(i)
                    idx = i + 1
                End If
            Next i
            _Members = newmems
        End Sub
        Public Function Append(aPaths As TPATHS, Optional bAddClones As Boolean = False) As Integer
            Try
                If aPaths.Count > 0 Then
                    If bAddClones Then
                        _Members = _Members.Concat(aPaths._Members.Clone).ToArray
                    Else
                        _Members = _Members.Concat(aPaths._Members).ToArray
                    End If
                End If
                Return Count
            Catch ex As Exception
                Return 0
            End Try
        End Function
        Public Sub Clear()
            ReDim _Members(-1)
        End Sub
        Friend Sub Print(Optional aTag As String = "")
            Dim i As Integer
            Dim aPth As TPATH
            If Count <= 0 Then Return
            For i = 1 To Count
                System.Diagnostics.Debug.WriteLine("")
                System.Diagnostics.Debug.WriteLine(Trim(aTag & " PATH - " & i.ToString))
                For j As Integer = 1 To Count
                    aPth = _Members(j - 1)
                    aPth.Print(4)
                Next j
                System.Diagnostics.Debug.WriteLine("END PATH - " & i.ToString)
            Next i
            ''Beep()
        End Sub
        Public Function UpdateLinetypes(aOldValue As String, aNewValue As String) As Boolean
            Dim aMem As TPATH
            Dim _rVal As Boolean = False
            If String.Compare(aOldValue, aNewValue, True) = 0 Then Return _rVal
            For i As Integer = 1 To Count
                aMem = Item(i)
                If TLISTS.Contains(aMem.Linetype, aOldValue) Then
                    _rVal = True
                    aMem.Linetype = aNewValue
                    SetItem(i, aMem)
                End If
            Next
            Return _rVal
        End Function
        Friend Sub Combine() 'As TPATHS
            If Count <= 1 Then Return
            '^returns the paths merged based on fill and color status
            Dim rPaths As New TPATHS(Me, bNoMembers:=True)
            'mark the paths with uniqe strings
            For i As Integer = 1 To Count
                rPaths.AddOrJoin(Item(i))
            Next
            _Members = rPaths._Members.Clone
        End Sub
        Friend Sub AddOrJoin(aPath As TPATH, Optional bIgnoreDisplayProperties As Boolean = False)
            Dim bAddit As Boolean = True


            If Not aPath.Relative Then
                For i As Integer = 1 To Count
                    Dim aMem As TPATH = Item(i)
                    If aMem.IsEqualTo(aPath, bCompareLoops:=False, bIgnoreDisplayProperties:=bIgnoreDisplayProperties) Then
                        bAddit = False
                        For j As Integer = 1 To aPath.LoopCount
                            aMem.AddLoop(aPath.Looop(j).Clone)
                        Next j
                        SetItem(i, aMem)
                        Exit For
                    End If
                Next i
            End If
            If bAddit Then Add(aPath.Clone)
        End Sub
        Friend Sub ProjectToPlane(aPlane As TPLANE, Optional aDirection As dxfDirection = Nothing)
            If TPLANE.IsNull(aPlane) Then Return
            Dim i As Integer
            Dim li As Integer
            Dim vi As Integer
            Dim aPth As TPATH
            Dim aDir As TVECTOR
            Dim v1 As TVECTOR
            Dim aLoop As TVECTORS
            If aDirection Is Nothing Then aDir = aPlane.ZDirection Else aDir = aDirection.Strukture
            For i = 1 To Count
                aPth = _Members(i - 1)
                For li = 1 To aPth.LoopCount
                    aLoop = aPth.Looop(li)
                    For vi = 1 To aLoop.Count
                        v1 = dxfProjections.ToPlane(aLoop.Item(vi), aPlane, aDir)
                        aLoop.SetItem(vi, v1)
                    Next vi
                    aPth.SetLoop(li, aLoop)
                    aPth.Plane.Origin = dxfProjections.ToPlane(aPth.Plane.Origin, aPlane, aDir)
                    aPth.Plane = aPth.Plane.AlignedTo(aDir, dxxAxisDescriptors.Z)
                Next li
                _Members(i - 1) = aPth
            Next i
            For i = 1 To ExtentVectors.Count
                v1 = dxfProjections.ToPlane(ExtentVectors.Item(i), aPlane, aDir)
                ExtentVectors.SetItem(i, v1)
            Next i
            'Bounds.Origin.ProjectTo(aPlane, aDir)
            'Bounds = Bounds.AlignedTo(aDir, dxxAxisDescriptors.Z)
            'Bounds = ExtentVectors.Bounds(aPlane)
        End Sub
        Public Function ProjectedToPlane(aPlane As TPLANE, Optional aDirection As dxfDirection = Nothing) As TPATHS
            Dim _rVal As New TPATHS(Me)
            _rVal.ProjectToPlane(aPlane, aDirection)
            Return _rVal
        End Function
        Public Sub Scale(aRefPt As TVECTOR, aXScale As Double, aYScale As Double, aZScale As Double)
            Dim i As Integer
            Dim aPth As TPATH
            Dim j As Integer
            Dim li As Integer
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim aLoop As TVECTORS
            For i = 1 To Count
                aPth = _Members(i - 1)
                For li = 1 To aPth.LoopCount
                    aLoop = aPth.Looop(li)
                    If aPth.Relative Then
                        'aPth = pth_ToWorld(aPth)
                        v1 = aRefPt.WithRespectTo(aPth.Plane)
                        For j = 1 To aLoop.Count
                            v2 = aLoop.Item(j).Scaled(aXScale, v1, aYScale, aZScale)
                            aLoop.SetItem(j, v2)
                        Next j
                        '                vec_ReScale aPth.Plane.Origin, aXScale, aRefPt, aYScale, aZScale
                    Else
                        For j = 1 To aLoop.Count
                            v2 = aLoop.Item(j).Scaled(aXScale, aRefPt, aYScale, aZScale)
                            aLoop.SetItem(j, v2)
                        Next j
                    End If
                    aPth.SetLoop(li, aLoop)
                Next li
                _Members(i - 1) = aPth
            Next i
        End Sub
        Public Function SetDisplayVariable(aVariableName As String, aNewValue As Object, Optional aSearchValue As Object = Nothing) As Boolean
            Dim _rVal As Boolean
            '#1the name of the display variable to affect (LayerName, Color, Linetype etc.)
            '#2the new value for the  display variable
            '#3a variable value to match
            '#4flag to set any undefined variable to the new value
            '^sets the members indicated display variable to the new value
            '~if a seach value is passed then only members with a current value equal to the search value will be affected.
            '~returns the affected members.
            _rVal = False
            If Count <= 0 Then Return _rVal
            If String.IsNullOrWhiteSpace(aVariableName) Then Return _rVal
            aVariableName = aVariableName.Trim().ToUpper()
            Dim bTestIt As Boolean
            Dim lTest As Integer
            Dim sTest As String = String.Empty
            Dim sngTest As Double
            Dim bTest As Boolean
            Dim lValue As Integer
            Dim sValue As String = String.Empty
            Dim sngValue As Double
            Dim bValue As Boolean
            '-------------------------------------------------------------------------
            If aVariableName.IndexOf("COLOR") >= 0 Then
                '-------------------------------------------------------------------------
                aVariableName = "COLOR"
                If Not TVALUES.IsNumber(aNewValue) Then Return _rVal
                lValue = TVALUES.To_INT(aNewValue)
                If lValue = -1 Then Return _rVal
                If aSearchValue IsNot Nothing Then
                    If TVALUES.IsNumber(aSearchValue) Then
                        bTestIt = True
                        lTest = TVALUES.To_INT(aSearchValue)
                    End If
                End If
                '-------------------------------------------------------------------------
            ElseIf aVariableName.IndexOf("WEIGHT") >= 0 Then
                '-------------------------------------------------------------------------
                aVariableName = "LINEWEIGHT"
                If Not TVALUES.IsNumber(aNewValue) Then Return _rVal
                lValue = TVALUES.To_INT(aNewValue)
                If lValue < dxxLineWeights.ByLayer Or lValue > dxxLineWeights.LW_211 Then Return _rVal
                If aSearchValue IsNot Nothing Then
                    If TVALUES.IsNumber(aSearchValue) Then
                        bTestIt = True
                        lTest = TVALUES.To_INT(aSearchValue)
                    End If
                End If
                '-------------------------------------------------------------------------
            ElseIf aVariableName.IndexOf("LTSCALE") >= 0 Then
                '-------------------------------------------------------------------------
                aVariableName = "LTSCALE"
                If Not TVALUES.IsNumber(aNewValue) Then Return _rVal
                sngValue = TVALUES.To_DBL(aNewValue)
                If sngValue < 0 Then Return _rVal
                If aSearchValue IsNot Nothing Then
                    If TVALUES.IsNumber(aSearchValue) Then
                        bTestIt = True
                        sngTest = TVALUES.To_DBL(aSearchValue)
                    End If
                End If
                '-------------------------------------------------------------------------
            ElseIf aVariableName.IndexOf("LAYER") >= 0 Then
                '-------------------------------------------------------------------------
                aVariableName = "LAYERNAME"
                sValue = aNewValue.ToString.Trim()
                If sValue = "" Then sValue = "0"
                If aSearchValue IsNot Nothing Then
                    sTest = aSearchValue.ToString().Trim()
                    bTestIt = sTest <> ""
                End If
                '-------------------------------------------------------------------------
            ElseIf aVariableName.IndexOf("LINETYPE") >= 0 Then
                '-------------------------------------------------------------------------
                aVariableName = "LINETYPE"
                sValue = aNewValue.ToString().Trim()
                If aSearchValue IsNot Nothing Then
                    sTest = aSearchValue.ToString().Trim()
                    bTestIt = sTest <> ""
                End If
                '-------------------------------------------------------------------------
            ElseIf aVariableName = "SUPPRESSED" Then
                '-------------------------------------------------------------------------
                bValue = TVALUES.ToBoolean(aNewValue)
                If aSearchValue IsNot Nothing Then
                    bTestIt = True
                    bTest = TVALUES.ToBoolean(aSearchValue)
                End If
            Else
                Return _rVal
            End If
            _rVal = False
            Dim i As Integer
            Dim pChng As Boolean
            Dim aPth As TPATH
            For i = 1 To Count
                aPth = _Members(i - 1)
                pChng = False
                Select Case aVariableName
                '=================================================================
                    Case "COLOR"
                        '=================================================================
                        If Not bTestIt Then
                            pChng = aPth.Color <> lValue
                            aPth.Color = lValue
                        Else
                            If aPth.Color = lTest Then
                                pChng = aPth.Color <> lValue
                                aPth.Color = lValue
                            End If
                        End If
                '=================================================================
                    Case "LINWEIGHT"
                        '=================================================================
                        If Not bTestIt Then
                            pChng = aPth.LineWeight <> lValue
                            aPth.LineWeight = lValue
                        Else
                            If aPth.LineWeight = lTest Then
                                pChng = aPth.LineWeight <> lValue
                                aPth.LineWeight = lValue
                            End If
                        End If
                '=================================================================
                    Case "LAYERNAME"
                        '=================================================================
                        If Not bTestIt Then
                            pChng = String.Compare(aPth.LayerName, sValue, True) <> 0
                            aPth.LayerName = sValue
                        Else
                            If String.Compare(aPth.LayerName, sTest, True) = 0 Then
                                pChng = String.Compare(aPth.LayerName, sValue, True) <> 0
                                aPth.LayerName = sValue
                            End If
                        End If
                '=================================================================
                    Case "LINETYPE"
                        '=================================================================
                        If Not bTestIt Then
                            pChng = String.Compare(aPth.Linetype, sValue, True) <> 0
                            aPth.Linetype = sValue
                        Else
                            If String.Compare(aPth.Linetype, sTest, True) = 0 Then
                                pChng = String.Compare(aPth.Linetype, sValue, True) <> 0
                                aPth.Linetype = sValue
                            End If
                        End If
                '=================================================================
                    Case "LTSCALE"
                        '=================================================================
                        If Not bTestIt Then
                            pChng = aPth.LTScale <> sngValue
                            aPth.LTScale = sngValue
                        Else
                            If aPth.LTScale = sngTest Then
                                pChng = aPth.LTScale <> sngValue
                                aPth.LTScale = sngValue
                            End If
                        End If
                '=================================================================
                    Case "SUPPRESSED"
                        '=================================================================
                        If Not bTestIt Then
                            pChng = aPth.Suppressed <> bValue
                            aPth.Suppressed = bValue
                        Else
                            If aPth.Suppressed = bTest Then
                                pChng = aPth.Suppressed <> bValue
                                aPth.Suppressed = bValue
                            End If
                        End If
                End Select
                If pChng Then
                    _rVal = True
                    '.Pen.Defined = False
                End If
                _Members(i - 1) = aPth
            Next i
            Return _rVal
        End Function
#End Region 'Methods
#Region "Shared Methods"
        Public Shared Function NullEnt(aEnt As dxfEntity) As TPATHS
            If aEnt Is Nothing Then Return Nothing
            Dim _rVal As New TPATHS(aEnt.Domain, aEnt.GUID)
            Dim lname As String = String.Empty
            Dim aColor As dxxColors
            _rVal.Identifier = aEnt.Identifier
            aEnt.LCLGet(lname, _rVal.Color, _rVal.Linetype, _rVal.LineWeight, _rVal.LTScale)
            _rVal.SetLayerName(lname, 0)
            If aColor = dxxColors.ByBlock Then aColor = _rVal.Color
            If _rVal.LineWeight < dxxLineWeights.ByLayer Then _rVal.LineWeight = dxxLineWeights.ByLayer
            If String.Compare(_rVal.Linetype, dxfLinetypes.ByBlock, True) = 0 Then _rVal.Linetype = dxfLinetypes.ByLayer
            If _rVal.Linetype = "" Then _rVal.Linetype = dxfLinetypes.ByLayer
            If _rVal.LTScale <= 0 Then _rVal.LTScale = 1
            If aEnt.GraphicType = dxxGraphicTypes.Point Or aEnt.GraphicType = dxxGraphicTypes.Text Or aEnt.GraphicType = dxxGraphicTypes.Solid Then
                _rVal.Linetype = dxfLinetypes.Continuous
                _rVal.LTScale = 1
                _rVal.LineWeight = dxxLineWeights.LW_000
            End If
            _rVal.Identifier = aEnt.Identifier
            Return _rVal
        End Function
        Public Shared Sub BLOCK(aBlock As dxfBlock, Optional bRegen As Boolean = False, Optional aImage As dxfImage = Nothing)
            Dim rAttribs As colDXFEntities = Nothing
            BLOCK(aBlock, bRegen, aImage, rAttribs)
        End Sub
        Public Shared Sub BLOCK(aBlock As dxfBlock, bRegen As Boolean, aImage As dxfImage, ByRef rAttribs As colDXFEntities)
            If aBlock Is Nothing Then Return
            'On Error Resume Next
            Dim aEnts As colDXFEntities
            Dim aEnt As dxfEntity
            Dim aPaths As TPATHS
            Dim aPl As New TPLANE("") With {.Origin = aBlock.InsertionPtV}
            Dim entPaths As TPATHS
            Dim j As Integer
            Dim ePth As TPATH
            Dim li As Integer
            Dim vi As Integer
            Dim bGUID As String
            Dim aTxt As dxeText
            Dim pVecs As TVECTORS
            If rAttribs IsNot Nothing Then rAttribs.Clear()
            aPaths = New TPATHS(aBlock.Domain, aBlock.GUID)
            If Not aBlock.GetImage(aImage) Then Return

            bGUID = aBlock.GUID
            aBlock.RelativePaths = aPaths
            aEnts = aBlock.Entities
            For i As Integer = 1 To aEnts.Count
                aEnt = aEnts.Item(i)
                If Not aEnt.Suppressed Then
                    aEnt.BlockGUID = ""
                    aEnt.UpdatePath(bRegen, aImage)
                    entPaths = aEnt.Paths
                    If rAttribs IsNot Nothing Then
                        If aEnt.EntityType = dxxEntityTypes.Attdef Then
                            aTxt = aEnt.Clone
                            rAttribs.Add(aTxt)
                        End If
                    End If
                    For j = 1 To entPaths.Count
                        ePth = entPaths.Item(j)
                        For li = 1 To ePth.LoopCount
                            pVecs = ePth.Looop(li)
                            For vi = 1 To pVecs.Count
                                If ePth.Relative Then pVecs.SetItem(vi, ePth.Plane.WorldVector(pVecs.Item(vi)))
                                pVecs.SetItem(vi, pVecs.Item(vi).WithRespectTo(aPl))
                            Next vi
                            ePth.SetLoop(li, pVecs)
                        Next li
                        ePth.Relative = True
                        ePth.Plane = aPl
                        entPaths.SetItem(j, ePth)
                        aPaths.Add(ePth)
                    Next j
                    For j = 1 To entPaths.ExtentVectors.Count
                        aPaths.ExtentVectors.Add(entPaths.ExtentVectors.Item(j).WithRespectTo(aPl))
                    Next j
                    aEnt.BlockGUID = bGUID
                End If
            Next i
            aBlock.RelativePaths = aPaths
        End Sub
#End Region 'Shared Methods
#Region "Operators"
        Public Shared Widening Operator CType(aPaths As TPATHS) As TVECTORS
            Dim _rVal As New TVECTORS(0)
            Dim aPath As TPATH
            Dim pPts As TVECTORS
            Dim v1 As TVECTOR
            Dim zDir As TVECTOR
            For i As Integer = 1 To aPaths.Count
                aPath = aPaths.Item(i)
                If aPath.Relative Then
                    'aPath = aPath.ConvertedToWorld()
                    zDir = aPath.Plane.ZDirection
                End If
                For j As Integer = 1 To aPath.LoopCount
                    pPts = aPath.Looop(j)
                    For k As Integer = 1 To pPts.Count
                        v1 = pPts.Item(k).Clone
                        If aPath.Relative Then v1 = aPath.Plane.Origin + aPath.Plane.XDirection * v1.X + aPath.Plane.YDirection * v1.Y + zDir * v1.Z
                        _rVal.Add(v1)
                        '_rVal.Append(aPath.Looop(j))
                    Next k
                Next j
            Next i
            Return _rVal
        End Operator
#End Region 'Operators
    End Structure 'TPATHS


    Friend Structure TARC
        Implements ICloneable
#Region "Members"
        Public ClockWise As Boolean
        Public EllipseSegments As TLINES
        Public Elliptical As Boolean
        Public EndAngle As Double
        Public EndWidth As Double
        Public Identifier As String
        Public MinorRadius As Double
        Public Plane As TPLANE
        Public Radius As Double
        Public StartAngle As Double
        Public StartWidth As Double
        Public Tag As String
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aTag As String = "")

            'init ---------------------------
            ClockWise = False
            EllipseSegments = New TLINES(0)
            Elliptical = False
            StartAngle = 0
            EndAngle = 360
            EndWidth = 0
            Identifier = ""
            MinorRadius = 0
            Plane = TPLANE.World
            Radius = 0
            StartWidth = 0
            Tag = aTag
            'init ---------------------------


            'SpannedAngle = 360
            'StartPt = aPlane.AngleVector( 0, Radius, False)
            'EndPt = StartPt.Clone
            'Length = 2 * Math.PI * Radius
        End Sub

        Public Sub New(aRadius As Double)

            'init ---------------------------
            ClockWise = False
            EllipseSegments = New TLINES(0)
            Elliptical = False
            StartAngle = 0
            EndAngle = 360
            EndWidth = 0
            Identifier = ""
            MinorRadius = 0
            Plane = TPLANE.World
            Radius = Math.Abs(aRadius)
            StartWidth = 0
            Tag = ""
            'init ---------------------------


            'SpannedAngle = 360
            'StartPt = aPlane.AngleVector( 0, Radius, False)
            'EndPt = StartPt.Clone
            'Length = 2 * Math.PI * Radius
        End Sub

        Public Sub New(aCenter As iVector, aRadius As Double)

            'init ---------------------------
            ClockWise = False
            EllipseSegments = New TLINES(0)
            Elliptical = False
            StartAngle = 0
            EndAngle = 360
            EndWidth = 0
            Identifier = ""
            MinorRadius = 0
            Plane = TPLANE.World
            Radius = Math.Abs(aRadius)
            StartWidth = 0
            Tag = ""
            'init ---------------------------
            Plane.Origin = New TVECTOR(aCenter)

            'SpannedAngle = 360
            'StartPt = aPlane.AngleVector( 0, Radius, False)
            'EndPt = StartPt.Clone
            'Length = 2 * Math.PI * Radius
        End Sub

        Public Sub New(aArc As TARC)

            'init ---------------------------
            ClockWise = aArc.ClockWise
            EllipseSegments = New TLINES(aArc.EllipseSegments)
            Elliptical = aArc.Elliptical
            StartAngle = aArc.StartAngle
            EndAngle = aArc.EndAngle
            EndWidth = aArc.EndWidth
            Identifier = aArc.Identifier
            MinorRadius = aArc.MinorRadius
            Plane = New TPLANE(aArc.Plane)
            Radius = Math.Abs(aArc.Radius)
            StartWidth = aArc.StartWidth
            Tag = aArc.Tag
            'init ---------------------------


        End Sub

        Public Sub New(aPlane As TPLANE, aCenter As TVECTOR, aRadius As Double, Optional aStartAngle As Double = 0, Optional aEndAngle As Double = 360)

            'init ---------------------------
            ClockWise = False
            EllipseSegments = New TLINES(0)
            Elliptical = False
            StartAngle = TVALUES.NormAng(aStartAngle, False, True)
            EndAngle = TVALUES.NormAng(aEndAngle, False, False)
            StartWidth = 0
            EndWidth = 0
            Identifier = ""
            MinorRadius = 0
            Plane = TPLANE.World
            Radius = Math.Abs(aRadius)
            Tag = ""
            'init ---------------------------

            If aPlane.DirectionsAreDefined Then Plane = aPlane
            Plane.Origin = aCenter
            'SpannedAngle = 360
            'StartPt = aPlane.AngleVector( 0, Radius, False)
            'EndPt = StartPt.Clone
            'Length = 2 * Math.PI * Radius
        End Sub
        Public Sub New(aPlane As TPLANE, aCenter As TVECTOR, aSP As TVECTOR, aEP As TVECTOR, Optional aClockwise As Boolean = False)
            'init ---------------------------
            ClockWise = False
            EllipseSegments = New TLINES(0)
            Elliptical = False
            StartAngle = 0
            EndAngle = 360
            EndWidth = 0
            Identifier = ""
            MinorRadius = 0
            Plane = TPLANE.World
            Radius = 0
            StartWidth = 0
            Tag = ""
            'init ---------------------------

            Dim Arc3P As TARC = TARC.DefineWithPoints(aPlane, aCenter, aSP, aEP, aClockwise, False)
            ClockWise = Arc3P.ClockWise
            Plane = Arc3P.Plane
            Radius = Arc3P.Radius
            StartAngle = Arc3P.StartAngle
            EndAngle = Arc3P.EndAngle
        End Sub
#End Region 'Constructors
#Region "Properties"

        Public ReadOnly Property MidPoint As TVECTOR
            Get
                If Not Elliptical Then
                    If Not ClockWise Then
                        Return Plane.AngleVector(StartAngle + 0.5 * SpannedAngle, Radius, False)
                    Else
                        Return Plane.AngleVector(EndAngle + 0.5 * SpannedAngle, Radius, False)
                    End If
                Else
                    If Not ClockWise Then
                        Return dxfUtils.EllipsePoint(Plane.Origin, 2 * Radius, 2 * MinorRadius, StartAngle + 0.5 * SpannedAngle, Plane)
                    Else
                        Return dxfUtils.EllipsePoint(Plane.Origin, 2 * Radius, 2 * MinorRadius, EndAngle + 0.5 * SpannedAngle, Plane)
                    End If
                End If
            End Get
        End Property

        Public ReadOnly Property Bulge As Double
            Get
                Dim _rVal As Double
                If Elliptical Then Return 0
                _rVal = Math.Tan((SpannedAngle * Math.PI / 180) / 4)
                If ClockWise Then _rVal = -_rVal
                Return _rVal
            End Get
        End Property
        Public ReadOnly Property StartPt As TVECTOR
            Get
                If Elliptical Then
                    Return dxfUtils.EllipsePoint(Plane.Origin, 2 * Radius, 2 * MinorRadius, StartAngle, Plane)
                Else
                    Return Plane.AngleVector(StartAngle, Radius, False)
                End If
            End Get
        End Property
        Public ReadOnly Property EndPt As TVECTOR
            Get
                If Elliptical Then
                    Return dxfUtils.EllipsePoint(Plane.Origin, 2 * Radius, 2 * MinorRadius, EndAngle, Plane)
                Else
                    Return Plane.AngleVector(EndAngle, Radius, False)
                End If
            End Get
        End Property
        Public ReadOnly Property Length As Double
            Get
                If Elliptical Then
                    If SpannedAngle < 569.99 Then
                        Return PhantomPoints(100).LengthSummation(False)
                    Else
                        Return 2 * Math.PI * Math.Sqrt(0.5 * (MinorRadius ^ 2 + Radius ^ 2))
                    End If
                Else
                    Return ((SpannedAngle * Math.PI) / 180) * Radius
                End If
            End Get
        End Property
        Public ReadOnly Property SpannedAngle As Double
            Get
                Return dxfMath.SpannedAngle(ClockWise, StartAngle, EndAngle)
            End Get
        End Property
        Public ReadOnly Property Inverse As TARC
            Get
                Dim _rVal As New TARC(Plane, Plane.Origin, aSP:=EndPt, aEP:=StartPt, Not ClockWise) With {
                    .Elliptical = Elliptical,
                    .StartWidth = EndWidth,
                    .EndWidth = StartWidth,
                    .Identifier = Identifier,
                    .MinorRadius = MinorRadius
                }
                Return _rVal
            End Get
        End Property
        Public Property Center As TVECTOR
            Get
                Return Plane.Origin
            End Get
            Set(value As TVECTOR)
                Plane.Origin = value
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function PathPoints(Optional bFullPath As Boolean = False) As TVECTORS
            If Elliptical Then
                Return TBEZIER.EllipsePath(Me, bFullPath)
            Else
                Return TBEZIER.ArcPath(Me, bFullPath)
            End If
        End Function
        Public Function QuadrantPoints(Optional aRotation As Double = 0.0) As TVERTICES
            '^returns vectors on the arc at 0,90,180 and 270
            Dim _rVal As New TVERTICES(0)
            '^returns vectors on the arc at 0,90,180 and 270
            Dim pln As TPLANE = Plane.Clone()
            If aRotation <> 0 Then
                pln.Revolve(aRotation)
            End If

            If Elliptical Then
                _rVal.Add(pln.Vertex(aVectorX:=MinorRadius, aVectorY:=0, aTag:="EAST"))
                _rVal.Add(pln.Vertex(aVectorX:=0, aVectorY:=Radius, aTag:="NORTH"))
                _rVal.Add(pln.Vertex(aVectorX:=-MinorRadius, aVectorY:=0, aTag:="WEST"))
                _rVal.Add(pln.Vertex(aVectorX:=0, aVectorY:=-Radius, aTag:="SOUTH"))
            Else
                _rVal.Add(pln.Vertex(aVectorX:=Radius, aVectorY:=0, aTag:="EAST"))
                _rVal.Add(pln.Vertex(aVectorX:=0, aVectorY:=Radius, aTag:="NORTH"))
                _rVal.Add(pln.Vertex(aVectorX:=-Radius, aVectorY:=0, aTag:="WEST"))
                _rVal.Add(pln.Vertex(aVectorX:=0, aVectorY:=-Radius, aTag:="SOUTH"))
            End If
            Return _rVal
        End Function
        Public Function RectangleIntersections(Optional aLeftLim As Double? = Nothing, Optional aRightLim As Double? = Nothing, Optional aTopLim As Double? = Nothing, Optional aBottomLim As Double? = Nothing,
                                                Optional bReturnInteriorCorners As Boolean = False, Optional bReturnQuadrantPt As Boolean = False) As TVERTICES
            Dim rInsinside As TVERTICES = Nothing
            Dim rTrimRectangle As dxfRectangle = Nothing
            Return RectangleIntersections(aLeftLim, aRightLim, aTopLim, aBottomLim, bReturnInteriorCorners, bReturnQuadrantPt, rInsinside, rTrimRectangle)
        End Function
        Public Function RectangleIntersections(aLeftLim As Double?, aRightLim As Double?, aTopLim As Double?, aBottomLim As Double?,
                                                bReturnInteriorCorners As Boolean, bReturnQuadrantPt As Boolean, ByRef rInsinside As TVERTICES, ByRef rTrimRectangle As dxfRectangle) As TVERTICES
            Dim _rVal As New TVERTICES(0)
            rInsinside = New TVERTICES(0)
            Try
                Dim rad As Double = Math.Round(Math.Abs(Radius), 8)
                If rad = 0 Then Return _rVal
                Dim left As Double = -rad - 10
                Dim right As Double = rad + 10
                Dim top As Double = rad + 10
                Dim bot As Double = -rad - 10
                Dim inside As Integer = 0
                If aLeftLim.HasValue Then left = aLeftLim.Value
                If aRightLim.HasValue Then right = aRightLim.Value
                If aTopLim.HasValue Then top = aTopLim.Value
                If aBottomLim.HasValue Then bot = aBottomLim.Value

                TVALUES.SortTwoValues(True, left, right)
                TVALUES.SortTwoValues(True, bot, top)
                If left <= -rad Then left = -rad - 10
                If right >= rad Then right = rad + 10
                If top >= rad Then top = rad + 10
                If bot <= -rad Then bot = -rad - 10
                Dim rectangle As New TPLANE(Math.Abs(right - left), Math.Abs(top - bot))

                rectangle.Origin = New TVECTOR(left + rectangle.Width / 2, bot + rectangle.Height / 2)
                Dim arc As New TARC(New TPLANE(""), New TVECTOR(0, 0, 0), rad)
                Dim edges As TLINES = rectangle.Edges
                Dim ul As New TVERTEX(left, top, aTag:="UL")
                Dim ll As New TVERTEX(left, bot, aTag:="LL")
                Dim lr As New TVERTEX(right, bot, aTag:="LR")
                Dim ur As New TVERTEX(right, top, aTag:="UR")
                inside = 0
                If ul.Magnitude < rad Then inside += 1
                If ll.Magnitude < rad Then inside += 1
                If lr.Magnitude < rad Then inside += 1
                If ur.Magnitude < rad Then inside += 1
                'all corners lie in the circle so we can't do this
                If inside = 4 Then Return _rVal
                Dim ipts As TVECTORS = rectangle.Edges.IntersectionPts(arc, aLines_AreInfinite:=False, bArc_IsInfinite:=True, bMustBeOnBoth:=True)
                Dim sp As TVECTOR
                Dim vrt As TVERTEX
                For i As Integer = 1 To ipts.Count
                    sp = ipts.Item(i)
                    vrt = New TVERTEX(sp.X, sp.Y, aVertexRadius:=rad, aTag:="IP")
                    _rVal.Add(vrt)
                Next
                rTrimRectangle = rectangle.ToRectangle()

                If bReturnQuadrantPt Then
                    vrt = New TVERTEX(0, rad, aVertexRadius:=rad, aTag:="Q1")
                    If rectangle.Contains(vrt) Then _rVal.Add(vrt)
                    vrt = New TVERTEX(-rad, 0, aVertexRadius:=rad, aTag:="Q2")
                    If rectangle.Contains(vrt) Then _rVal.Add(vrt)
                    vrt = New TVERTEX(0, -rad, aVertexRadius:=rad, aTag:="Q3")
                    If rectangle.Contains(vrt) Then _rVal.Add(vrt)
                    vrt = New TVERTEX(rad, 0, aVertexRadius:=rad, aTag:="Q4")
                    If rectangle.Contains(vrt) Then _rVal.Add(vrt)
                End If
                _rVal.CounterClockwise(Plane, 0)
                If bReturnInteriorCorners Then
                    For i As Integer = 1 To 4
                        Select Case i
                            Case 1
                                vrt = ul
                            Case 2
                                vrt = ll
                            Case 3
                                vrt = lr
                            Case 4
                                vrt = ur
                            Case Else
                                vrt = TVERTEX.Zero
                                Exit For
                        End Select
                        If vrt.Magnitude < rad Then
                            _rVal.Add(vrt)
                            rInsinside.Add(vrt)
                        End If
                    Next
                End If
                'transfer to the arcs plane if needed
                If Not Plane.IsWorld Or Not TVECTOR.IsNull(Center) Then
                    For i As Integer = 1 To _rVal.Count
                        vrt = _rVal.Item(i)
                        vrt.Vector = Plane.Vector(vrt.X, vrt.Y)
                        _rVal.SetItem(i, vrt)
                    Next
                End If
                Return _rVal
            Catch ex As Exception
                Throw ex
            End Try
        End Function
        Public Overrides Function ToString() As String
            Dim _rVal As String
            Dim angles As Boolean = SpannedAngle < 359.9999
            If Not Elliptical Then
                If angles Then
                    _rVal = "TARC [" & Center.ToString.Replace("TVECTOR", "CENTER:") & " RAD:" & Radius.ToString("0.0###")
                Else
                    angles = False
                    _rVal = "TCIRCLE [" & Center.ToString.Replace("TVECTOR", "CENTER:") & " RAD:" & Radius.ToString("0.0###")
                End If
            Else
                _rVal = "TELLIPSE [" & Center.ToString.Replace("TVECTOR", "CENTER:") & " MAJRAD:" & Radius.ToString("0.0###") & " MINRAD:" & MinorRadius.ToString("0.0###")
            End If
            If angles Then _rVal += " SA:" & StartAngle.ToString("0.0#") & " EA:" + EndAngle.ToString("0.0#")
            If Plane.XDirection.X <> 1 Then
                _rVal += Plane.XDirection.ToString.Replace("TVECTOR", " XDIR:")
            End If
            If Plane.ZDirection.Z <> 1 Then
                _rVal += Plane.ZDirection.ToString.Replace("TVECTOR", " NORM:")
            End If
            If ClockWise Then
                _rVal += " CW"
            Else
                _rVal += " CCW"
            End If
            Return _rVal & "]"
        End Function
        Public Sub Translate(aTranslation As TVECTOR)
            Plane.Origin += aTranslation
        End Sub

        Friend Function IntersectionPts(bArc As TARC, Optional bSuppressPlaneCheck As Boolean = False, Optional aCollector As colDXFVectors = Nothing) As TVECTORS
            Dim _rVal As New TVECTORS(0)
            '^finds the intersection of the two entities assuming they are circular (arcs or circles) and they lie on the same plane
            Dim bCoplanar As Boolean
            'check for co-planars
            If bSuppressPlaneCheck Then
                bCoplanar = True
            Else
                bCoplanar = Plane.IsCoplanar(bArc.Plane)
            End If
            If Not bCoplanar Then
                'if the arcs are not coplanar then get the intersections of the
                'convert both to line segments and get their intersections
                Return PhantomLines(1000).IntersectionPts(bArc.PhantomLines(1000), False, False, True, aCollector:=aCollector)
            Else
                If Not Elliptical And Not bArc.Elliptical Then
                    Return dxfIntersections.PlanarArcArc(Me, bArc, True, True, aCollector)
                ElseIf Elliptical And Not bArc.Elliptical Then
                    Return dxfIntersections.PlanarArcArc(Me, bArc, True, True, aCollector)
                ElseIf Not Elliptical And bArc.Elliptical Then
                    Return dxfIntersections.PlanarArcArc(Me, bArc, True, True, aCollector)
                Else  'two elipses
                    Return dxfIntersections.PlanarArcArc(Me, bArc, True, True, aCollector)
                End If
            End If
            Return _rVal
        End Function
#End Region 'Methods
        Public Function PhantomLines(Optional aCurveDivisions As Integer = 100, Optional bFullPath As Boolean = False, Optional aCollector As colDXFEntities = Nothing) As TLINES
            Dim _rVal As New TLINES(0)
            '^a collection of phantom vertices along the arc segment
            '~the arc is parsed into small arc segments
            'On Error Resume Next
            aCurveDivisions = TVALUES.LimitedValue(aCurveDivisions, 2, 10000)
            Dim angchange As Double
            Dim i As Long
            Dim j As Integer
            Dim sp As TVECTOR
            Dim ep As TVECTOR
            If Not ClockWise Then j = 1 Else j = -1
            If Not Elliptical Then
                angchange = SpannedAngle / aCurveDivisions
                Plane.Origin = Plane.Origin
                'StartPt = Plane.AngleVector( StartAngle, Radius, False)
                'EndPt = Plane.AngleVector( EndAngle, Radius, False)
                sp = StartPt
                ep = sp.Clone
                For i = 1 To aCurveDivisions - 1
                    ep.RotateAbout(Plane.Origin, Plane.ZDirection, angchange * j, False)
                    _rVal.Add(sp, ep)
                    If aCollector IsNot Nothing Then
                        aCollector.AddLineV(sp, ep)
                    End If
                Next i
                _rVal.Add(sp, EndPt)
                If aCollector IsNot Nothing Then
                    aCollector.AddLineV(sp, EndPt)
                End If
            Else
                If MinorRadius = 0 Or Radius = 0 Or (SpannedAngle = 0 And Not bFullPath) Then Return _rVal
                Dim angstep As Double
                Dim aAng As Double
                Dim f1 As Double
                Dim majdia As Double
                Dim mindia As Double
                Dim espan As Double = 360
                Dim sa As Double = 0
                'Dim ea As Double = 360
                If Not bFullPath Then
                    espan = SpannedAngle
                    sa = StartAngle
                    'ea = EndAngle
                End If
                majdia = Radius * 2
                mindia = MinorRadius * 2
                If aCurveDivisions < 4 Then aCurveDivisions = 4
                If aCurveDivisions > 10000 Then aCurveDivisions = 10000
                angstep = espan / aCurveDivisions
                f1 = 1
                If Not ClockWise Then aAng = sa Else aAng = sa
                Do Until i >= aCurveDivisions + 1
                    ep = dxfUtils.EllipsePoint(Plane.Origin, majdia, mindia, aAng, Plane)
                    i += 1
                    aAng += f1 * angstep
                    If i > 1 Then
                        _rVal.Add(sp, ep)
                        If aCollector IsNot Nothing Then aCollector.AddLineV(sp, ep)
                    End If
                    sp = ep
                Loop
                Return _rVal
            End If
            Return _rVal
        End Function
        Public Function PhantomPts(Optional aCurveDivisions As Integer = 20, Optional bIncludeEndPt As Boolean = True) As TPOINTS
            Dim _rVal As New TPOINTS(0)
            '^a collection of phantom vertices along the arc segment
            '~the arc is parsed into small line segments

            If Not Elliptical Then
                aCurveDivisions = TVALUES.LimitedValue(aCurveDivisions, 2, 1000)
            Else
                aCurveDivisions = TVALUES.LimitedValue(aCurveDivisions, 4, 1000)
            End If

            Dim angchange As Double
            Dim i As Long
            Dim j As Integer
            Dim v1 As TVECTOR
            'Dim rad As Double
            Dim remain As Double
            Dim Span As Double
            Dim sp As TVECTOR
            Dim ep As TVECTOR
            Dim Segs As Integer
            Dim spz As TVECTOR
            Dim aZDir As TVECTOR
            ' Dim ang As Double
            Span = SpannedAngle
            If Span <= 0.001 Then Return _rVal
            If Not Elliptical Then

                spz = Plane.Origin
                If Not ClockWise Then j = 1 Else j = -1
                aZDir = Plane.ZDirection
                sp = Plane.AngleVector(StartAngle, Radius, False)
                ep = Plane.AngleVector(EndAngle, Radius, False)

                Segs = aCurveDivisions
                angchange = Span / Segs
                _rVal.Add(sp)

                v1 = sp
                'ang = angchange
                remain = Span
                Do While remain > angchange
                    v1.RotateAbout(spz, aZDir, angchange * j, False)
                    _rVal.Add(v1)
                    remain -= angchange
                Loop
                If bIncludeEndPt Then _rVal.Add(ep)
            Else
                If MinorRadius = 0 Or Radius = 0 Then Return _rVal
                Dim angstep As Double = SpannedAngle / aCurveDivisions
                Dim aAng As Double
                Dim f1 As Double = 1
                Dim majdia As Double = Radius * 2
                Dim mindia As Double = MinorRadius * 2


                If Not ClockWise Then aAng = StartAngle Else aAng = EndAngle

                Do Until i >= aCurveDivisions + 1
                    v1 = dxfUtils.EllipsePoint(Plane.Origin, majdia, mindia, aAng, Plane)
                    _rVal.Add(v1)
                    i += 1
                    aAng += f1 * angstep
                Loop
            End If
            Return _rVal
        End Function
        Public Function PhantomPoints(Optional aCurveDivisions As Integer = 20, Optional bIncludeEndPt As Boolean = True, Optional bAsLines As Boolean = True, Optional bDivisIsAngle As Boolean = False, Optional aCollector As colDXFVectors = Nothing) As TVECTORS
            Dim _rVal As New TVECTORS(0)
            '^a collection of phantom vertices along the arc segment
            '~the arc is parsed into small line segments
            If Not TVALUES.IsNumber(aCurveDivisions) Then aCurveDivisions = 20
            'On Error Resume Next
            Dim v1 As TVECTOR
            Dim sp As TVECTOR
            Dim ep As TVECTOR
            Dim spz As TVECTOR
            Dim aZDir As TVECTOR
            Dim angchange As Double
            Dim i As Long
            Dim j As Integer
            ' Dim rad As Double
            Dim remain As Double
            Dim Span As Double = SpannedAngle
            Dim Segs As Integer
            ' Dim ang As Double
            If Span <= 0.001 Then Return _rVal
            If Not Elliptical Then
                If Not bDivisIsAngle Then
                    aCurveDivisions = TVALUES.To_INT(aCurveDivisions)
                    If aCurveDivisions < 2 Then aCurveDivisions = 2
                    If aCurveDivisions > 10000 Then aCurveDivisions = 10000
                Else
                    aCurveDivisions = TVALUES.NormAng(TVALUES.To_DBL(aCurveDivisions), False, True, True)
                    If aCurveDivisions = 0 Or aCurveDivisions > Span Then aCurveDivisions = Span
                End If
                If Not bAsLines Then
                    'rad = Radius
                End If
                spz = Plane.Origin
                If Not ClockWise Then j = 1 Else j = -1
                aZDir = Plane.ZDirection
                sp = Plane.AngleVector(StartAngle, Radius, False)
                ep = Plane.AngleVector(EndAngle, Radius, False)
                If bDivisIsAngle Then
                    angchange = aCurveDivisions
                Else
                    Segs = aCurveDivisions
                    angchange = Span / Segs
                End If
                _rVal.Add(sp)
                If aCollector IsNot Nothing Then aCollector.AddV(sp)
                If bDivisIsAngle Then
                    v1 = sp
                    'ang = angchange
                    remain = Span
                    Do While remain > angchange
                        v1.RotateAbout(spz, aZDir, angchange * j, False)
                        _rVal.Add(v1)
                        If aCollector IsNot Nothing Then aCollector.AddV(v1)
                        remain -= angchange
                    Loop
                Else
                    v1 = sp.Clone
                    For i = 1 To Segs - 1
                        v1.RotateAbout(spz, aZDir, angchange * j, False)
                        _rVal.Add(v1.Clone)
                        If aCollector IsNot Nothing Then aCollector.AddV(v1)
                    Next i
                End If
                If bIncludeEndPt And Math.Round(Span, 4) <> 360 Then
                    _rVal.Add(ep)
                    If aCollector IsNot Nothing Then aCollector.AddV(ep)
                End If
            Else
                If MinorRadius = 0 Or Radius = 0 Then Return _rVal
                Dim angstep As Double
                Dim aAng As Double
                Dim f1 As Double
                Dim majdia As Double
                Dim mindia As Double
                majdia = Radius * 2
                mindia = MinorRadius * 2
                If aCurveDivisions < 4 Then aCurveDivisions = 4
                If aCurveDivisions > 1000 Then aCurveDivisions = 1000
                angstep = SpannedAngle / aCurveDivisions
                f1 = 1
                If Not ClockWise Then aAng = StartAngle Else aAng = EndAngle
                Do Until i >= aCurveDivisions + 1
                    v1 = dxfUtils.EllipsePoint(Plane.Origin, majdia, mindia, aAng, Plane)
                    _rVal.Add(v1)
                    If aCollector IsNot Nothing Then aCollector.AddV(v1)
                    i += 1
                    aAng += f1 * angstep
                Loop
            End If
            Return _rVal
        End Function
        Public Function ContainsVector(aVector As TVECTOR, Optional aFudgeFactor As Double = 0.001, Optional bTreatAsInfinite As Boolean = False, Optional bSuppressPlaneCheck As Boolean = True) As Boolean
            Dim rIsStartPt As Boolean = False
            Dim rIsEndPt As Boolean = False
            Dim rWithin As Boolean = False
            Return ContainsVector(aVector, aFudgeFactor, rIsStartPt, rIsEndPt, bTreatAsInfinite, bSuppressPlaneCheck, rWithin)
        End Function
        Public Function ContainsVector(aVector As TVECTOR, aFudgeFactor As Double, ByRef rIsStartPt As Boolean, ByRef rIsEndPt As Boolean, Optional bTreatAsInfinite As Boolean = False, Optional bSuppressPlaneCheck As Boolean = True) As Boolean
            Dim rWithin As Boolean = False
            Return ContainsVector(aVector, aFudgeFactor, rIsStartPt, rIsEndPt, bTreatAsInfinite, bSuppressPlaneCheck, rWithin)
        End Function
        Public Function ContainsVector(aVector As TVECTOR, aFudgeFactor As Double, ByRef rIsStartPt As Boolean, ByRef rIsEndPt As Boolean, bTreatAsInfinite As Boolean, bSuppressPlaneCheck As Boolean, ByRef rWithin As Boolean) As Boolean
            Return TARC.ArcContainsVector(Me, aVector, aFudgeFactor, rIsStartPt, rIsEndPt, bTreatAsInfinite, bSuppressPlaneCheck, rWithin)
        End Function
#Region "Shared Methods"
        Friend Shared Function BulgeToRadius(aSP As TVECTOR, aEP As TVECTOR, aBulge As Double, Optional aCS As dxfPlane = Nothing) As Double
            Dim _rVal As Double
            '#1the first vector
            '#2the second vector
            '#3the bulge used to compute the arc
            '^returns the radius of the dxeArc defined by the passed bulge
            '~no errors raised if the arc can't be defined.
            '~the first vector is assumed as the arc start vector and the last vector is assummed as the end.
            If aBulge = 0 Then Return _rVal
            Dim crdLen As Double
            Dim arcHt As Double
            Dim mp As TVECTOR
            ' Dim aSign As Integer
            '**UNUSED VAR** Dim eStr As String
            Dim bCS As dxfPlane
            Dim aDir As TVECTOR
            crdLen = dxfProjections.DistanceTo(aSP, aEP)
            If crdLen <= 0 Then Return _rVal
            'If aBulge <= 0 Then aSign = -1 Else aSign = 1
            arcHt = (Math.Abs(aBulge) * crdLen) / 2
            bCS = New dxfPlane
            If aCS IsNot Nothing Then bCS.Strukture = New TPLANE(aCS)
            aDir = aSP.DirectionTo(aEP)
            bCS.OriginV = aSP
            bCS.AlignXToV(aDir)
            mp = aSP + bCS.XDirectionV * (0.5 * crdLen)
            aDir.RotateAbout(New TVECTOR, bCS.ZDirectionV, 90, False)
            mp += aDir * arcHt
            Dim errstr As String = String.Empty
            Dim aArc As TARC = dxfPrimatives.ArcThreePointV(aSP, mp, aEP, True, bCS.Strukture, errstr)
            If String.IsNullOrWhiteSpace(errstr) Then _rVal = aArc.Radius
            Return _rVal
        End Function
        Friend Shared Function ArcStructure(aPlane As TPLANE, aCenter As TVECTOR, aRadius As Double, aStartAngle As Double, aEndAngle As Double, bClockwise As Boolean, Optional bElliptical As Boolean = False, Optional aMinorRadius As Double = 0.0, Optional bFullCircle As Boolean = False, Optional aStartWidth As Double = 0.0, Optional aEndWidth As Double = 0.0, Optional aIdentifier As String = "") As TARC
            Dim _rVal As New TARC
            If Math.Abs(aMinorRadius) = Math.Abs(aRadius) Or aMinorRadius = 0 Then bElliptical = False
            _rVal.ClockWise = bClockwise
            _rVal.Elliptical = bElliptical
            _rVal.MinorRadius = Math.Abs(aMinorRadius)
            _rVal.Radius = Math.Abs(aRadius)
            _rVal.StartWidth = Math.Abs(aStartWidth)
            _rVal.EndWidth = Math.Abs(aEndWidth)
            _rVal.Plane = aPlane.Clone
            _rVal.Plane.Origin = aCenter.Clone
            _rVal.Identifier = aIdentifier
            If Not bFullCircle Then
                _rVal.StartAngle = TVALUES.NormAng(aStartAngle, False, True, True)
                _rVal.EndAngle = TVALUES.NormAng(aEndAngle, False, False, True)

            Else
                _rVal.StartAngle = 0
                _rVal.EndAngle = 360

            End If

            Return _rVal
        End Function
        Public Shared Function ArcContainsVector(aArc As TARC, aVector As TVECTOR, Optional aFudgeFactor As Double = 0.001, Optional bTreatAsInfinite As Boolean = False, Optional bSuppressPlaneCheck As Boolean = True) As Boolean
            Dim rIsStartPt As Boolean = False
            Dim rIsEndPt As Boolean = False
            Dim rWithin As Boolean = False
            Return ArcContainsVector(aArc, aVector, aFudgeFactor, rIsStartPt, rIsEndPt, bTreatAsInfinite, bSuppressPlaneCheck, rWithin)
        End Function
        Public Shared Function ArcContainsVector(aArc As TARC, aVector As TVECTOR, aFudgeFactor As Double, ByRef rIsStartPt As Boolean, ByRef rIsEndPt As Boolean, Optional bTreatAsInfinite As Boolean = False, Optional bSuppressPlaneCheck As Boolean = True) As Boolean
            Dim rWithin As Boolean = False
            Return ArcContainsVector(aArc, aVector, aFudgeFactor, rIsStartPt, rIsEndPt, bTreatAsInfinite, bSuppressPlaneCheck, rWithin)
        End Function
        Public Shared Function ArcContainsVector(aArc As TARC, aVector As TVECTOR, aFudgeFactor As Double, ByRef rIsStartPt As Boolean, ByRef rIsEndPt As Boolean, bTreatAsInfinite As Boolean, bSuppressPlaneCheck As Boolean, ByRef rWithin As Boolean) As Boolean
            rIsStartPt = False
            rIsEndPt = False
            rWithin = False
            Dim _rVal As Boolean
            Dim f1 As Double = Math.Abs(aFudgeFactor)
            If f1 > 0.1 Then f1 = 0.1
            If f1 < 0.000001 Then f1 = 0.000001
            '^returns True if the passed vector lies on this entity
            If Not aArc.Elliptical Then
                Dim d1 As Double
                Dim bD As TVECTOR
                Dim ang As Double
                Dim rad As Double
                'not in my plane
                If Not bSuppressPlaneCheck Then
                    If Not aArc.Plane.ContainsVector(aVector, f1) Then
                        Return _rVal
                    End If
                End If
                rad = Math.Round(aArc.Radius, 4)
                d1 = aVector.DistanceTo(aArc.Plane.Origin, 4)
                rWithin = d1 < rad - f1
                If rWithin Or d1 > rad + f1 Then Return _rVal
                d1 = aVector.DistanceTo(aArc.StartPt)
                If d1 <= f1 Then
                    _rVal = True
                    rIsStartPt = True
                    Return _rVal
                End If
                d1 = aVector.DistanceTo(aArc.EndPt)
                If d1 <= f1 Then
                    _rVal = True
                    rIsEndPt = True
                    Return _rVal
                End If
                If bTreatAsInfinite Then
                    _rVal = True
                Else
                    If aArc.SpannedAngle >= 359.99 Then
                        _rVal = True
                    Else
                        bD = (aVector - aArc.Plane.Origin).Normalized()
                        ang = aArc.Plane.XDirection.AngleTo(bD, aArc.Plane.ZDirection)
                        If aArc.StartAngle = ang Then rIsStartPt = True
                        If aArc.EndAngle = ang Then rIsEndPt = True
                        If Not aArc.ClockWise Then
                            If aArc.StartAngle < aArc.EndAngle Then
                                _rVal = ang >= aArc.StartAngle - f1 And ang <= aArc.EndAngle + f1
                            Else
                                _rVal = (ang >= aArc.StartAngle - f1 And ang <= 360) Or (ang <= aArc.EndAngle + f1 And ang >= 0)
                            End If
                        Else
                            If aArc.StartAngle > aArc.EndAngle Then
                                _rVal = ang >= aArc.EndAngle - f1 And ang <= aArc.StartAngle + f1
                            Else
                                _rVal = (ang >= aArc.EndAngle - f1 And ang <= 360) Or (ang <= aArc.StartAngle + f1 And ang >= 0)
                            End If
                        End If
                    End If
                End If
                Return _rVal
            Else
                Dim d1 As Double
                Dim aD As TVECTOR
                Dim bD As TVECTOR
                Dim sp As TVECTOR
                Dim ep As TVECTOR
                Dim cp As TVECTOR
                Dim v1 As TVECTOR
                Dim aFlg As Boolean
                If Not bSuppressPlaneCheck Then
                    If Not aVector.LiesOn(aArc.Plane, f1) Then
                        Return _rVal
                    End If
                End If
                sp = aArc.StartPt
                d1 = aVector.DistanceTo(sp)
                If d1 <= 0.0001 Then
                    _rVal = True
                    rIsStartPt = True
                    Return _rVal
                End If
                ep = aArc.EndPt
                d1 = aVector.DistanceTo(ep)
                If d1 <= 0.0001 Then
                    _rVal = True
                    rIsEndPt = True
                    Return _rVal
                End If
                'get distance and direction
                cp = aArc.Plane.Origin
                aD = cp.DirectionTo(aVector, False, aFlg, d1)
                If aFlg Then
                    rWithin = True
                    Return _rVal
                End If
                'gross not within radius
                If d1 >= aArc.Radius + 0.0001 Then Return _rVal
                Dim ang1 As Double
                ang1 = aArc.Plane.XDirection.AngleTo(aD, aArc.Plane.ZDirection)
                v1 = dxfUtils.EllipsePoint(cp, 2 * aArc.Radius, 2 * aArc.MinorRadius, ang1, aArc.Plane)
                bD = v1.DirectionTo(aVector, False, aFlg, d1)
                If aFlg Then
                    'the vector is the bound point
                    _rVal = True
                    If Not bTreatAsInfinite Then
                        'se if it is with the spanned angle
                        If Not aArc.ClockWise Then bD = cp.DirectionTo(sp) Else bD = cp.DirectionTo(ep)
                        ang1 = aD.AngleTo(bD, aArc.Plane.ZDirection)
                        If ang1 > aArc.SpannedAngle Then _rVal = False
                    End If
                Else
                    rWithin = Not aD.Equals(bD)
                End If
                Return _rVal
            End If
        End Function
        Public Shared Function DefineWithPoints(aPlane As TPLANE, aCenter As TVECTOR, aSP As TVECTOR, aEP As TVECTOR, Optional aClockwise As Boolean = False, Optional bSuppressProjection As Boolean = False) As TARC

            '#1the plane for the arc
            '#2the center vector of the arc
            '#3the starting vector of the arc
            '#4the ending vector of the arc
            '#5flag to toggle between the two possible arcs that can be defined by the passed vectors
            '^a shorthand way to set the properties of a arc object
            Dim aPl As New TPLANE(aPlane)
            Dim d1 As TVECTOR
            Dim d2 As TVECTOR
            Dim sp As New TVECTOR(aSP)
            Dim ep As New TVECTOR(aEP)
            Dim cp As New TVECTOR(aCenter)
            Dim rad1 As Double
            Dim rad2 As Double
            Dim rad As Double
            Dim bFlag1 As Boolean
            Dim bFlag2 As Boolean
            Dim sa As Double
            Dim ea As Double

            aPl.Origin = cp

            If Not bSuppressProjection Then
                sp = dxfProjections.ToPlane(sp, aPl)
                ep = dxfProjections.ToPlane(ep, aPl)
            End If

            d1 = cp.DirectionTo(sp, False, bFlag1, rad1)
            d2 = cp.DirectionTo(ep, False, bFlag2, rad2)
            rad = rad1
            If bFlag1 And bFlag2 Then
                rad = 1
                d1 = TVECTOR.WorldX
                d2 = d1
            Else
                If bFlag1 Then
                    rad = rad2
                    d2 = d1
                ElseIf bFlag2 Then
                    rad = rad1
                    d1 = d2
                End If
            End If
            sp = aPl.Origin + (d1 * rad)
            ep = aPl.Origin + (d2 * rad)
            sa = sp.PlanarAngle(aPl)
            ea = ep.PlanarAngle(aPl)
            If Not aClockwise Then
                If ea < sa Then
                    ea = TVALUES.NormAng(ea + 360, False, False, True)
                End If
            Else
                If ea < sa Then
                    ea = TVALUES.NormAng(ea - 360, False, True, True)
                End If
            End If
            If sa = ea Then
                sa = 0
                ea = 360
            End If
            Return TARC.ArcStructure(aPl, cp, rad, sa, ea, aClockwise, False, 0)

        End Function
        Friend Shared Function ByOctant(aSP As TVECTOR, aRadius As Double, aStartingOctant As Integer, aOctantSpan As Integer, ByRef rClockWise As Boolean, Optional aStartOffset As Double? = Nothing, Optional aEndOffset As Double? = Nothing, Optional bSuppressError As Boolean = False) As TARC
            Dim rStartAngle As Double = 0.0
            Dim rEndAngle As Double = 0.0
            Return ByOctant(aSP, aRadius, aStartingOctant, aOctantSpan, rClockWise, aStartOffset, aEndOffset, rStartAngle, rEndAngle, bSuppressError)
        End Function
        Friend Shared Function ByOctant(aSP As TVECTOR, aRadius As Double, aStartingOctant As Integer, aOctantSpan As Integer, ByRef rClockWise As Boolean, aStartOffset As Double?, aEndOffset As Double?, ByRef rStartAngle As Double, ByRef rEndAngle As Double, Optional bSuppressError As Boolean = False) As TARC

            Dim _rVal As New TARC

            If aRadius <= 0 Then rClockWise = True Else rClockWise = False
            rStartAngle = 0
            rEndAngle = 0
            If aRadius = 0 Then Return _rVal
            '#1the first vector of the arc
            '#2the radius of the arc
            '#3the starting octant angle (0-7)
            '#4the ending octant angle (0-7)
            '#5flag to return an inverted arc
            '^returns a dxeArc defined using the passed octant info
            '~no errors raised if the arc can't be defined.
            '~the first vector is assumed as the arc start vector.
            rStartAngle = 0
            rEndAngle = 0
            aRadius = Math.Abs(aRadius)
            Dim aPlane As TPLANE = TPLANE.World


            Dim aDir As TVECTOR
            Dim sErr As String = String.Empty
            Try
                rStartAngle = aStartingOctant * 45
                If aStartOffset IsNot Nothing Then
                    If aStartOffset.HasValue Then
                        rStartAngle = 45 * aStartOffset.Value / 256 + rStartAngle
                    End If

                End If
                If aEndOffset IsNot Nothing Then
                    If aEndOffset.HasValue Then
                        Dim loct As Integer = aStartingOctant + aOctantSpan
                        rEndAngle = (loct * 45)
                        Dim dif As Double = 45 * aEndOffset.Value / 256
                        rEndAngle = dif + rEndAngle
                    End If
                Else
                    If aOctantSpan <= 0 Then aOctantSpan = 8
                    If aOctantSpan > 7 Then aOctantSpan = 8
                    rEndAngle = rStartAngle + aOctantSpan * 45
                End If
                Dim aspn As Double = dxfMath.SpannedAngle(rClockWise, rStartAngle, rEndAngle)
                If aspn = 0 Then aspn = 360

                aDir = aPlane.XDirection.RotatedAbout(aPlane.ZDirection, rStartAngle, False).Normalized
                aPlane.Origin = aSP + aDir * -aRadius
                _rVal = New TARC(aPlane, aPlane.Origin, aRadius, rStartAngle, rEndAngle) With {.ClockWise = rClockWise}
            Catch ex As Exception
                sErr = ex.Message
            Finally
                If Not bSuppressError And sErr <> "" Then
                    Throw New Exception($"ArcByOctant - {sErr}")
                End If
            End Try

            Return _rVal

        End Function

        Friend Shared Function ByBulge(aSP As TPOINT, aEP As TPOINT, aBulge As Double, bSuppressErrs As Boolean, aPlane As TPLANE) As TARC
            Dim _rVal As New TARC
            '#1the first vector
            '#2the second vector
            '#3the bulge used to compute the arc
            '#4returns the midpoint of the chord that is used to define the bulge arc
            '^returns a dxeArc defined by the two points and passed bulge.
            '~the first vector is assumed as the arc start vector and the last vector is assummed as the end vector.
            Dim sErr As String = String.Empty
            Try
                If aBulge = 0 Then
                    sErr = "Zero Bulge Detected"
                    Return _rVal
                End If
                Dim aDir As TVECTOR
                Dim aPl As New TPLANE(aPlane)
                Dim mp As New TPOINT
                Dim v1 As TVECTOR
                Dim v2 As TVECTOR
                Dim v3 As TVECTOR
                Dim crdLen As Double
                Dim arcHt As Double
                Dim aSign As Integer
                Dim bFlag As Boolean
                aBulge /= 127
                v1 = TPOINT.Direction(aSP, aEP, False, bFlag, crdLen)
                If bFlag Then
                    sErr = "Undefined Point Detected"
                    Return _rVal
                End If
                v2 = TVECTOR.WorldZ.CrossProduct(v1)
                aPl.Define(New TVECTOR(aSP), v1, v2)
                mp = TPOINT.Interpolate(aSP, aEP, 0.5)
                If aBulge <= 0 Then aSign = -1 Else aSign = 1
                arcHt = (Math.Abs(aBulge) * crdLen) / 2
                If aBulge < 0 Then
                    aDir = aPl.YDirection
                Else
                    aDir = aPl.YDirection * -1
                End If
                mp.Project(aDir, arcHt, True)
                v2 = New TVECTOR(mp)
                If aBulge < 0 Then
                    v1 = New TVECTOR(aEP)
                    v3 = New TVECTOR(aSP)
                Else
                    v1 = New TVECTOR(aSP)
                    v3 = New TVECTOR(aEP)
                End If
                _rVal = dxfPrimatives.ArcThreePointV(v1, v2, v3, False, aPl, True)
            Catch ex As Exception
                sErr = ex.Message

            Finally
                If Not bSuppressErrs And sErr <> "" Then
                    Throw New Exception($"ArcByBulgeP - {sErr}")
                End If
            End Try
            Return _rVal
        End Function
        Friend Shared Function ByBulge(aSP As TVECTOR, aEP As TVECTOR, aBulge As Double, rMP As TVECTOR, bSuppressErrs As Boolean, aPlane As TPLANE, Optional bSuppressProjection As Boolean = False) As TARC
            Dim _rVal As New TARC
            '#1the first vector
            '#2the second vector
            '#3the bulge used to compute the arc
            '#4returns the midpoint of the chord that is used to define the bulge arc
            '^returns a dxeArc defined by the two points and passed bulge.
            '~the first vector is assumed as the arc start vector and the last vector is assummed as the end vector.
            Dim eStr As String = String.Empty
            Try


                Dim blg As Double = aBulge

                If blg = 0 Then
                    eStr = "Zero Bulge Detected"
                    Return _rVal
                End If
                Dim sp As TVECTOR = aSP
                Dim ep As TVECTOR = aEP
                Dim crdLen As Double
                Dim arcHt As Double
                Dim mp As TVECTOR
                Dim aSign As Integer
                Dim aDir As TVECTOR
                Dim xDir As TVECTOR
                Dim bFlag As Boolean
                Dim bPlane As TPLANE = aPlane

                If Not bSuppressProjection Then
                    sp = aSP.ProjectedTo(bPlane)
                    ep = aEP.ProjectedTo(bPlane)

                End If
                xDir = sp.DirectionTo(ep, bReturnInverse:=False, rDirectionIsNull:=bFlag, rDistance:=crdLen)
                If bFlag Then
                    eStr = "Undefined Point Detected"
                    Return _rVal
                End If
                bPlane.Origin = sp
                bPlane.AlignTo(xDir, dxxAxisDescriptors.X)
                mp = sp + xDir * (0.5 * crdLen)
                If blg <= 0 Then aSign = -1 Else aSign = 1
                arcHt = (Math.Abs(blg) * crdLen) / 2
                aDir = xDir.RotatedAbout(bPlane.ZDirection, aSign * 90).Normalized
                mp += aDir * arcHt
                _rVal = dxfPrimatives.ArcThreePointV(sp, mp, ep, False, bPlane, True)
                _rVal.ClockWise = (aSign = -1)
                rMP = mp
            Catch ex As Exception
                eStr = ex.Message

            Finally
                If Not bSuppressErrs And eStr <> "" Then
                    Throw New Exception($"ArcByBulgeV - {eStr}")
                End If

            End Try
            Return _rVal
        End Function

        Public Function Clone() As TARC
            Return New TARC(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TARC(Me)
        End Function


#End Region 'Shared Methods
    End Structure 'TARC
    Friend Structure TSEGMENT
        Implements ICloneable
#Region "Members"
        Public ArcStructure As TARC
        Public DisplayStructure As TDISPLAYVARS
        Public LineStructure As TLINE
        Public IsArc As Boolean
        Public Marker As Boolean
        Public INFINITE As Boolean
        Public INFINITE_RIGHT As Boolean
        Public Row As Integer
        Public Col As Integer
        Public StartEdgeAngle As Double
        Public EndEdgeAngle As Double
        Public Flag As String
        Public Identifier As String
        Public ImageGUID As String
        Public Mark As String
        Public OwnerGUID As String
        Public Tag As String
        Public Value As Double
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aTag As String = "")

            'init ---------------------------
            ArcStructure = New TARC("")
            DisplayStructure = New TDISPLAYVARS("0")
            LineStructure = TLINE.Null

            IsArc = False
            Marker = False
            INFINITE = False
            INFINITE_RIGHT = False
            Row = 0
            Col = 0
            StartEdgeAngle = 0
            EndEdgeAngle = 0
            Flag = ""
            Identifier = ""
            ImageGUID = ""
            Mark = ""
            OwnerGUID = ""
            Tag = aTag
            Value = 0
            'init ---------------------------
        End Sub

        Public Sub New(aSegment As iPolylineSegment)

            'init ---------------------------
            ArcStructure = New TARC("")
            DisplayStructure = New TDISPLAYVARS("0")
            LineStructure = TLINE.Null

            IsArc = False
            Marker = False
            INFINITE = False
            INFINITE_RIGHT = False
            StartEdgeAngle = 0
            EndEdgeAngle = 0
            Row = 0
            Col = 0
            Flag = String.Empty
            Identifier = String.Empty
            ImageGUID = String.Empty
            Mark = String.Empty
            OwnerGUID = String.Empty
            Tag = String.Empty
            Value = 0
            'init ---------------------------

            If aSegment Is Nothing Then Return

            Dim ent As dxfEntity = Nothing
            If aSegment.SegmentType = dxxSegmentTypes.Arc Then
                Dim arc As dxeArc = DirectCast(aSegment, dxeArc)
                ent = arc
                ArcStructure = arc.ArcStructure
                IsArc = True
            ElseIf aSegment.SegmentType = dxxSegmentTypes.Line Then
                Dim line As dxeLine = DirectCast(aSegment, dxeLine)
                ent = line
                LineStructure = New TLINE(line)
            Else
                Return
            End If

            DisplayStructure = ent.DisplayStructure
            Row = ent.Row
            Col = ent.Col
            Flag = ent.Flag
            Identifier = ent.Identifier
            ImageGUID = ent.ImageGUID
            OwnerGUID = ent.OwnerGUID
            Tag = ent.Tag
            Value = ent.Value

        End Sub


        Public Sub New(aLine As TLINE, Optional bInfinite As Boolean = False, Optional binfinite_Right As Boolean = False)

            'init ---------------------------
            ArcStructure = New TARC("")
            DisplayStructure = New TDISPLAYVARS("0")
            LineStructure = TLINE.Null

            IsArc = False
            Marker = False
            INFINITE = False
            INFINITE_RIGHT = False
            Row = 0
            Col = 0
            StartEdgeAngle = 0
            EndEdgeAngle = 0
            Flag = ""
            Identifier = ""
            ImageGUID = ""
            Mark = ""
            OwnerGUID = ""
            Tag = ""
            Value = 0
            'init ---------------------------


            If bInfinite Then
                INFINITE = True
            Else
                INFINITE_RIGHT = binfinite_Right
            End If
        End Sub
        Public Sub New(aArc As TARC, Optional bInfinite As Boolean = False)
            'init ---------------------------
            ArcStructure = aArc
            DisplayStructure = New TDISPLAYVARS("0")
            LineStructure = TLINE.Null

            IsArc = True
            Marker = False
            INFINITE = False
            INFINITE_RIGHT = False
            Row = 0
            Col = 0
            StartEdgeAngle = 0
            EndEdgeAngle = 0
            Flag = ""
            Identifier = ""
            ImageGUID = ""
            Mark = ""
            OwnerGUID = ""
            Tag = ""
            Value = 0
            'init ---------------------------
            If bInfinite Then
                ArcStructure.StartAngle = 0
                ArcStructure.EndAngle = 360
            End If
        End Sub
        Public Sub New(aArc As TARC, aDisplayStructure As TDISPLAYVARS, Optional bInfinite As Boolean = False)
            'init ---------------------------
            ArcStructure = aArc
            DisplayStructure = aDisplayStructure
            LineStructure = TLINE.Null

            IsArc = True
            Marker = False
            INFINITE = False
            INFINITE_RIGHT = False
            Row = 0
            Col = 0
            StartEdgeAngle = 0
            EndEdgeAngle = 0
            Flag = ""
            Identifier = ""
            ImageGUID = ""
            Mark = ""
            OwnerGUID = ""
            Tag = ""
            Value = 0
            'init ---------------------------
            If bInfinite Then
                ArcStructure.StartAngle = 0
                ArcStructure.EndAngle = 360
            End If
        End Sub
        Public Sub New(aSP As TVECTOR, aEP As TVECTOR, Optional bInfinite As Boolean = False)

            'init ---------------------------
            ArcStructure = New TARC("")
            DisplayStructure = New TDISPLAYVARS("0")
            LineStructure = TLINE.Null

            IsArc = False
            Marker = False
            INFINITE = False
            INFINITE_RIGHT = False
            Row = 0
            Col = 0
            StartEdgeAngle = 0
            EndEdgeAngle = 0
            Flag = ""
            Identifier = ""
            ImageGUID = ""
            Mark = ""
            OwnerGUID = ""
            Tag = ""
            Value = 0
            'init ---------------------------

            LineStructure = New TLINE(New TVECTOR(aSP), New TVECTOR(aEP))



        End Sub
        Public Sub New(aSP As TPOINT, aEP As TPOINT, Optional bInfinite As Boolean = False)
            'init ---------------------------
            ArcStructure = New TARC("")
            DisplayStructure = New TDISPLAYVARS("0")
            LineStructure = TLINE.Null

            IsArc = False
            Marker = False
            INFINITE = False
            INFINITE_RIGHT = False
            Row = 0
            Col = 0
            StartEdgeAngle = 0
            EndEdgeAngle = 0
            Flag = ""
            Identifier = ""
            ImageGUID = ""
            Mark = ""
            OwnerGUID = ""
            Tag = ""
            Value = 0
            'init ---------------------------

            LineStructure = New TLINE(CType(aSP, TVECTOR), CType(aEP, TVECTOR))

            INFINITE = bInfinite
            INFINITE_RIGHT = False

        End Sub

        Public Sub New(aSegment As TSEGMENT)
            'init ---------------------------
            ArcStructure = New TARC(aSegment.ArcStructure)
            DisplayStructure = New TDISPLAYVARS(aSegment.DisplayStructure)
            LineStructure = New TLINE(aSegment.LineStructure)

            IsArc = aSegment.IsArc
            Marker = aSegment.Marker
            INFINITE = aSegment.INFINITE
            INFINITE_RIGHT = aSegment.INFINITE_RIGHT
            Row = aSegment.Row
            Col = aSegment.Col
            StartEdgeAngle = aSegment.StartEdgeAngle
            EndEdgeAngle = aSegment.EndEdgeAngle
            Flag = aSegment.Flag
            Identifier = aSegment.Identifier
            ImageGUID = aSegment.ImageGUID
            Mark = aSegment.Mark
            OwnerGUID = aSegment.OwnerGUID
            Tag = aSegment.Tag
            Value = aSegment.Value
            'init ---------------------------


        End Sub

#End Region 'Constructors
#Region "Properties"
        Public Property TagFlagValue As TTAGFLAGVALUE
            Get
                Return New TTAGFLAGVALUE(Tag, Flag, Value, Row, Col)
            End Get
            Set(value As TTAGFLAGVALUE)
                Tag = value.Tag
                Flag = value.Flag
                Me.Value = value.Value
                Row = value.Row
                Col = value.Col
            End Set
        End Property
        Public ReadOnly Property ExtentPts As TVECTORS
            Get
                Dim _rVal As New TVECTORS(0)
                Dim aspn As Double
                Dim eCnt As Integer
                If IsArc Then
                    eCnt = 5
                    aspn = ArcStructure.SpannedAngle
                    If aspn > 90 And aspn <= 180 Then
                        eCnt = 9
                    ElseIf aspn > 180 And aspn <= 270 Then
                        eCnt = 13
                    ElseIf aspn > 270 Then
                        eCnt = 16
                    End If
                    _rVal = ArcStructure.PhantomPoints(eCnt)
                Else
                    _rVal.Add(LineStructure.SPT)
                    _rVal.Add(LineStructure.EPT)
                End If
                Return _rVal
            End Get
        End Property

        Public Property LayerName As String
            Get
                Return DisplayStructure.LayerName
            End Get
            Set(value As String)
                DisplayStructure.LayerName = value.Trim()
            End Set
        End Property
        Public Property Linetype As String
            Get
                Return DisplayStructure.Linetype
            End Get
            Set(value As String)
                DisplayStructure.Linetype = value.Trim()
            End Set
        End Property
        Public Property Color As dxxColors
            Get
                Return DisplayStructure.Color
            End Get
            Set(value As dxxColors)
                DisplayStructure.Color = value
            End Set
        End Property

        Public Property LineWeight As dxxLineWeights
            Get
                Return DisplayStructure.LineWeight
            End Get
            Set(value As dxxLineWeights)
                DisplayStructure.LineWeight = value
            End Set
        End Property

        Public Property LTScale As Double
            Get
                Return DisplayStructure.LTScale
            End Get
            Set(value As Double)
                DisplayStructure.LTScale = value
            End Set
        End Property

        Public Property Suppressed As Boolean
            Get
                Return DisplayStructure.Suppressed
            End Get
            Set(value As Boolean)
                DisplayStructure.Suppressed = value
            End Set
        End Property

        Public ReadOnly Property StartPt As TVECTOR
            Get
                Dim _rVal As TVECTOR = IIf(IsArc, ArcStructure.StartPt, LineStructure.SPT)
                Return _rVal
            End Get

        End Property
        Public ReadOnly Property EndPt As TVECTOR
            Get
                Dim _rVal As TVECTOR = IIf(IsArc, ArcStructure.EndPt, LineStructure.EPT)
                Return _rVal
            End Get

        End Property
        Public ReadOnly Property Radius As Double
            Get
                Dim _rVal As Double = IIf(IsArc, ArcStructure.Radius, 0)
                Return _rVal
            End Get

        End Property
        Public ReadOnly Property ClockWise As Boolean
            Get
                Dim _rVal As Boolean = IIf(IsArc, ArcStructure.ClockWise, 0)
                Return _rVal
            End Get

        End Property
        Public ReadOnly Property Bulge As Double
            '^the bulge value for the arc
            '~the bulge is = (2* arc.height/arc.chord)
            '~this value is used in autocad to define a polyline segment
            Get
                If Not IsArc Then Return 0
                If ArcStructure.Elliptical Then Return 0
                Return ArcStructure.Bulge
            End Get
        End Property

        Public Property StartWidth As Double
            Get
                If IsArc Then Return ArcStructure.StartWidth Else Return LineStructure.StartWidth
            End Get
            Set(value As Double)
                If IsArc Then ArcStructure.StartWidth = value Else LineStructure.StartWidth = value
            End Set
        End Property
        Public Property EndWidth As Double
            Get
                If IsArc Then Return ArcStructure.EndWidth Else Return LineStructure.EndWidth
            End Get
            Set(value As Double)
                If IsArc Then ArcStructure.EndWidth = value Else LineStructure.EndWidth = value
            End Set
        End Property
#End Region 'Properties
#Region "Methods"

        Public Function Clone() As TSEGMENT

            Return New TSEGMENT(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TSEGMENT(Me)
        End Function

        Public Function ArcBounds(bUpdateArcPts As Boolean, ByRef rNoWidth As Boolean) As TSEGMENTS
            Dim _rVal As New TSEGMENTS(0)
            Dim aAr As TARC = ArcStructure
            If aAr.Radius <= 0 Then Return _rVal
            rNoWidth = True
            Dim sZero As Double = 0.000001
            Dim sArc As TSEGMENT
            Dim aSegs As TSEGMENTS
            Dim sp As TVECTOR = aAr.StartPt
            Dim ep As TVECTOR = aAr.EndPt
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim mp As TVECTOR
            Dim d1 As TVECTOR
            Dim d2 As TVECTOR
            Dim d3 As TVECTOR
            Dim aSeg As TSEGMENT
            Dim aPl As TPLANE = aAr.Plane
            Dim sLine As New TLINE("")
            Dim eLn As New TLINE("")
            Dim ips As TVECTORS
            Dim ip As TVECTOR
            Dim i As Integer
            Dim ang As Double
            Dim angspan As Double
            Dim swd As Double = Math.Round(aAr.StartWidth, 6)
            Dim ewd As Double = Math.Round(aAr.EndWidth, 6)
            Dim rad As Double = aAr.Radius
            Dim f1 As Double
            Dim eid As Integer
            Dim sea As Double = StartEdgeAngle
            Dim eea As Double = EndEdgeAngle
            Dim sang As Double
            Dim eang As Double
            Dim bClockWs As Boolean

            If swd <= sZero Then swd = 0
            If ewd <= sZero Then ewd = 0
            If (swd = 0 And ewd = 0) Then
                _rVal.Add(ArcStructure, OwnerGUID, "EDGE1")
            Else
                aSegs = dxfUtils.ArcDivide(ArcStructure, 90)
                If aSegs.Count <= 0 Then Return _rVal
                d1 = aPl.Origin.DirectionTo(sp)
                d2 = aPl.Origin.DirectionTo(ep)
                rNoWidth = False
                sArc = aSegs.Item(1)
                If swd > 0 Then
                    sLine = New TLINE(sp, sp + d1 * swd)
                    If sea <> 0 Then sLine.Rotate(sp, aPl.ZDirection, sea)
                Else
                    sea = 0
                End If
                If ewd > 0 Then
                    eLn = New TLINE(ep, ep + d2 * ewd)
                    If eea <> 0 Then eLn.Rotate(ep, aPl.ZDirection, eea)
                Else
                    eea = 0
                End If
                If swd > 0 Then
                    _rVal.Add(sLine.SPT, sLine.EPT, aIdentifier:="STARTWIDTH")
                End If
                For i = 1 To aSegs.Count
                    aSeg = aSegs.Item(i)
                    aPl = aSeg.ArcStructure.Plane
                    swd = aSeg.ArcStructure.StartWidth
                    ewd = aSeg.ArcStructure.EndWidth
                    sang = aSeg.ArcStructure.StartAngle
                    eang = aSeg.ArcStructure.EndAngle
                    bClockWs = aSeg.ArcStructure.ClockWise
                    angspan = aSeg.ArcStructure.SpannedAngle
                    v1 = aSeg.ArcStructure.StartPt
                    v2 = aSeg.ArcStructure.EndPt
                    If swd = ewd Then
                        d1 = aPl.Origin.DirectionTo(v1)
                        d2 = aPl.Origin.DirectionTo(v2)
                        v1 = aPl.Origin + d1 * (rad + swd / 2)
                        v2 = aPl.Origin + d2 * (rad + ewd / 2)
                        aAr = dxfPrimatives.ArcBetweenPointsV(rad + swd / 2, v1, v2, aPl, bClockWs, False).ArcStructure
                    Else
                        If swd >= ewd Then
                            f1 = swd - 0.5 * Math.Abs(swd - ewd)
                        Else
                            f1 = ewd - 0.5 * Math.Abs(swd - ewd)
                        End If
                        If Not bClockWs Then
                            ang = sang + angspan / 2
                        Else
                            ang = eang + angspan / 2
                        End If
                        d1 = aPl.Origin.DirectionTo(v1)
                        d2 = aPl.Origin.DirectionTo(v2)
                        v1 = aPl.Origin + d1 * (rad + swd / 2)
                        v2 = aPl.Origin + d2 * (rad + ewd / 2)
                        mp = aPl.AngleVector(ang, rad + (f1 / 2), False)
                        aAr = dxfPrimatives.ArcThreePointV(v1, mp, v2, False, aPl, True)
                    End If
                    If sea <> 0 Or eea <> 0 Then
                        If i = 1 Then
                            If sea <> 0 Then
                                ips = sLine.IntersectionPts(aAr, True, True, True)
                                If ips.Count > 0 Then
                                    ip = ips.Nearest(sLine.SPT, d3, dxxLineDescripts.Normal)
                                    aAr.StartAngle = aAr.Plane.XDirection.AngleTo(aAr.Plane.Origin.DirectionTo(ip), aPl.ZDirection)
                                    'aAr.StartPt = ip
                                    'aAr.SpannedAngle = dxfMath.SpannedAngle(aAr.ClockWise, aAr.StartAngle, aAr.EndAngle)
                                    _rVal.SetEPT(1, ip)
                                End If
                            End If
                        End If
                        If i = aSegs.Count Then
                            If eea <> 0 Then
                                ips = eLn.IntersectionPts(aAr, True, True, True)
                                If ips.Count > 0 Then
                                    ip = ips.Nearest(eLn.SPT, d3, dxxLineDescripts.Normal)
                                    aAr.EndAngle = aAr.Plane.XDirection.AngleTo(aAr.Plane.Origin.DirectionTo(ip), aPl.ZDirection)
                                    'aAr.EndPt = ip
                                    'aAr.SpannedAngle = dxfMath.SpannedAngle(aAr.ClockWise, aAr.StartAngle, aAr.EndAngle)
                                    eLn.SPT = ip
                                End If
                            End If
                        End If
                    End If
                    _rVal.Add(aAr, aIdentifier:="EDGE1")
                Next i
                If ewd > 0 Then
                    _rVal.Add(eLn, aIdentifier:="ENDWIDTH")
                    eid = _rVal.Count
                End If
                For i = aSegs.Count To 1 Step -1
                    aSeg = aSegs.Item(i)
                    'als_UpdateArcPoints(aSeg)
                    aPl = aSeg.ArcStructure.Plane
                    swd = aSeg.ArcStructure.StartWidth
                    ewd = aSeg.ArcStructure.EndWidth
                    sang = aSeg.ArcStructure.StartAngle
                    eang = aSeg.ArcStructure.EndAngle
                    bClockWs = aSeg.ArcStructure.ClockWise
                    angspan = aSeg.ArcStructure.SpannedAngle
                    v1 = aSeg.ArcStructure.StartPt
                    v2 = aSeg.ArcStructure.EndPt
                    If swd = ewd Then
                        d1 = aPl.Origin.DirectionTo(v2)
                        d2 = aPl.Origin.DirectionTo(v1)
                        v1 = aPl.Origin + (d1 * (rad - swd / 2))
                        v2 = aPl.Origin + (d2 * (rad - swd / 2))
                        aAr = dxfPrimatives.ArcBetweenPointsV(rad - swd / 2, v1, v2, aPl, Not bClockWs, bSuppressErrs:=True).ArcStructure
                    Else
                        If swd >= ewd Then
                            f1 = swd - 0.5 * Math.Abs(swd - ewd)
                        Else
                            f1 = ewd - 0.5 * Math.Abs(swd - ewd)
                        End If
                        If Not bClockWs Then
                            ang = sang + angspan / 2
                        Else
                            ang = eang + angspan / 2
                        End If
                        d1 = aPl.Origin.DirectionTo(v1)
                        d2 = aPl.Origin.DirectionTo(v2)
                        v1 = aPl.Origin + (d2 * (rad - ewd / 2))
                        v2 = aPl.Origin + (d1 * (rad - swd / 2))
                        mp = aPl.AngleVector(ang, rad - (f1 / 2), False)
                        aAr = dxfPrimatives.ArcThreePointV(v1, mp, v2, False, aPl, True)
                    End If
                    If sea <> 0 Or eea <> 0 Then
                        If i = 1 Then
                            If eea <> 0 Then
                                ips = eLn.IntersectionPts(aAr, True, True, True)
                                If ips.Count > 0 Then
                                    ip = ips.Nearest(eLn.EPT, d3, dxxLineDescripts.Normal)
                                    aAr.StartAngle = aAr.Plane.XDirection.AngleTo(aAr.Plane.Origin.DirectionTo(ip), aPl.ZDirection)
                                    'aAr.StartPt = ip
                                    'aAr.SpannedAngle = dxfMath.SpannedAngle(aAr.ClockWise, aAr.StartAngle, aAr.EndAngle)
                                    _rVal.SetEPT(eid, ip)
                                End If
                            End If
                        End If
                        If i = aSegs.Count Then
                            If sea <> 0 Then
                                ips = sLine.IntersectionPts(aAr, True, True, True)
                                If ips.Count > 0 Then
                                    ip = ips.Nearest(sLine.SPT, d3, dxxLineDescripts.Normal)
                                    aAr.EndAngle = aAr.Plane.XDirection.AngleTo(aAr.Plane.Origin.DirectionTo(ip), aPl.ZDirection)
                                    'aAr.EndPt = ip
                                    'aAr.SpannedAngle = dxfMath.SpannedAngle(aAr.ClockWise, aAr.StartAngle, aAr.EndAngle)
                                    _rVal.SetSPT(1, ip)
                                End If
                            End If
                        End If
                    End If
                    _rVal.Add(aAr, OwnerGUID, "EDGE2")
                Next i
            End If
            Return _rVal
        End Function
        Public Function ContainsVector(aVector As TVECTOR, Optional aFudgeFactor As Double = 0.001, Optional bTreatAsInfinite As Boolean = False, Optional bSuppressPlaneCheck As Boolean = True) As Boolean
            Dim rIsStartPt As Boolean = False
            Dim rIsEndPt As Boolean = False
            Dim rWithin As Boolean = False
            Return ContainsVector(aVector, aFudgeFactor, rIsStartPt, rIsEndPt, bTreatAsInfinite, bSuppressPlaneCheck, rWithin)
        End Function
        Public Function ContainsVector(aVector As TVECTOR, aFudgeFactor As Double, ByRef rIsStartPt As Boolean, ByRef rIsEndPt As Boolean, Optional bTreatAsInfinite As Boolean = False, Optional bSuppressPlaneCheck As Boolean = True) As Boolean
            Dim rWithin As Boolean = False
            Return ContainsVector(aVector, aFudgeFactor, rIsStartPt, rIsEndPt, bTreatAsInfinite, bSuppressPlaneCheck, rWithin)
        End Function
        Public Function ContainsVector(aVector As TVECTOR, aFudgeFactor As Double, ByRef rIsStartPt As Boolean, ByRef rIsEndPt As Boolean, bTreatAsInfinite As Boolean, bSuppressPlaneCheck As Boolean, ByRef rWithin As Boolean) As Boolean
            If IsArc Then
                Return TARC.ArcContainsVector(ArcStructure, aVector, aFudgeFactor, rIsStartPt, rIsEndPt, bTreatAsInfinite, bSuppressPlaneCheck, rWithin)
            Else
                Return TLINE.LineContainsVector(LineStructure, aVector, aFudgeFactor, rIsStartPt, rIsEndPt, bTreatAsInfinite, bSuppressPlaneCheck, rWithin)
            End If
        End Function
        Public Sub SegmentPts(ByRef rStartPt As TVECTOR, ByRef rEndPt As TVECTOR)

            rEndPt = EndPt
            rStartPt = StartPt

        End Sub

        Public Sub SegmentVerts(ByRef rStartPt As TVERTEX, ByRef rEndPt As TVERTEX)

            rEndPt = New TVERTEX(EndPt)
            rStartPt = New TVERTEX(StartPt)
            If IsArc Then
                rStartPt.Vars.Radius = Radius
                rStartPt.Vars.Inverted = ArcStructure.ClockWise
                rStartPt.Vars.Bulge = Bulge
            End If

        End Sub

        Public Sub Translate(aTranslation As TVECTOR)
            If IsArc Then
                ArcStructure.Plane.Origin += aTranslation
            Else
                LineStructure.SPT += aTranslation
                LineStructure.EPT += aTranslation
            End If
        End Sub
        Public Function IntersectionPt(aLine As TSEGMENT, ByRef rInterceptExists As Boolean) As TVECTOR
            Dim rLinesAreParallel As Boolean = False
            Dim rLinesAreCoincident As Boolean = False
            Dim rIsOnFirstLine As Boolean = False
            Dim rIsOnSecondLine As Boolean = False
            Return IntersectionPt(aLine, rLinesAreParallel, rLinesAreCoincident, rIsOnFirstLine, rIsOnSecondLine, rInterceptExists)
        End Function
        Public Function IntersectionPt(aLine As TSEGMENT, ByRef rLinesAreParallel As Boolean, ByRef rLinesAreCoincident As Boolean, ByRef rIsOnFirstLine As Boolean, ByRef rIsOnSecondLine As Boolean, ByRef rInterceptExists As Boolean) As TVECTOR
            '#1the first of two lines
            '#2the second of two lines
            '#3returns true if the two lines are cooincident
            '^returns the vector where the two passed line segments intersect (if it exists)
            If Not IsArc Then
                If Not aLine.IsArc Then
                    Return LineStructure.IntersectionPt(aLine.LineStructure, rLinesAreParallel, rLinesAreCoincident, rIsOnFirstLine, rIsOnSecondLine, rInterceptExists)
                Else
                End If
            Else
            End If
            '^returns the vector where the two passed line segments intersect (if it exists)
            Return LineStructure.IntersectionPt(aLine.LineStructure, rLinesAreParallel, rLinesAreCoincident, rIsOnFirstLine, rIsOnSecondLine, rInterceptExists)
        End Function
        Public Function ToSubEnt(Optional aGroupName As String = "") As TENTITY
            Dim _rVal As TENTITY
            If IsArc Then
                If ArcStructure.Elliptical Then
                    _rVal = New TENTITY(dxxGraphicTypes.Ellipse)
                Else
                    _rVal = New TENTITY(dxxGraphicTypes.Arc)
                End If
            Else
                _rVal = New TENTITY(dxxGraphicTypes.Line)
            End If
            _rVal.Props.CopyDisplayProperties(DisplayStructure)
            _rVal.Props.SetVal("*GroupName", aGroupName)
            _rVal.TagFlagValue = TagFlagValue
            _rVal.Props.SetVal("*Identifier", Identifier)
            _rVal.ImageGUID = ImageGUID
            If IsArc Then
                _rVal.DefPts.Plane = ArcStructure.Plane
                _rVal.DefPts.DefPt1 = _rVal.DefPts.Plane.Origin
                If ArcStructure.Elliptical Then
                    _rVal.Props.SetVal("*MajorRadius", Math.Abs(ArcStructure.Radius))
                    _rVal.Props.SetVal("*MinorRadius", Math.Abs(ArcStructure.MinorRadius))
                    _rVal.Props.SetVal("*StartAngle", ArcStructure.StartAngle)
                    _rVal.Props.SetVal("*EndAngle", ArcStructure.EndAngle)
                Else
                    _rVal.Props.SetVal("*Radius", Math.Abs(ArcStructure.Radius))
                    _rVal.Props.SetVal("Start Angle", ArcStructure.StartAngle)
                    _rVal.Props.SetVal("End Angle", ArcStructure.EndAngle)
                    _rVal.Props.SetVal("*StartWidth", ArcStructure.StartWidth)
                    _rVal.Props.SetVal("*EndWidth", ArcStructure.EndWidth)
                    _rVal.Props.SetVal("End Angle", ArcStructure.EndAngle)
                End If
            Else
                _rVal.DefPts.DefPt1 = LineStructure.SPT
                _rVal.DefPts.DefPt2 = LineStructure.EPT
                _rVal.Props.SetVal("*StartWidth", LineStructure.StartWidth)
                _rVal.Props.SetVal("*EndWidth", LineStructure.EndWidth)
                _rVal.DefPts.Plane.Origin = _rVal.DefPts.DefPt1
                _rVal.DefPts.Plane = _rVal.DefPts.Plane.AlignedTo(_rVal.DefPts.DefPt1.DirectionTo(_rVal.DefPts.DefPt2), dxxAxisDescriptors.X)
            End If
            Return _rVal
        End Function
        Public Function ToSubEntity(Optional aGroupName As String = "") As dxfEntity
            Dim _rVal As dxfEntity
            If IsArc Then
                If ArcStructure.Elliptical Then
                    _rVal = New dxeEllipse()
                Else
                    _rVal = New dxeArc()
                End If
            Else
                _rVal = New dxeLine
            End If
            _rVal.Properties.CopyDisplayProperties(DisplayStructure)
            _rVal.SetPropVal("*GroupName", aGroupName)
            _rVal.TagFlagValue = TagFlagValue
            _rVal.SetPropVal("*Identifier", Identifier)
            _rVal.ImageGUID = ImageGUID
            If IsArc Then
                _rVal.DefPts.Plane = ArcStructure.Plane
                _rVal.DefPts.VectorSet(1, _rVal.DefPts.Plane.Origin)

                If ArcStructure.Elliptical Then
                    _rVal.SetPropVal("*MajorRadius", Math.Abs(ArcStructure.Radius))
                    _rVal.SetPropVal("*MinorRadius", Math.Abs(ArcStructure.MinorRadius))
                    _rVal.SetPropVal("*StartAngle", ArcStructure.StartAngle)
                    _rVal.SetPropVal("*EndAngle", ArcStructure.EndAngle)
                Else
                    _rVal.SetPropVal("*Radius", Math.Abs(ArcStructure.Radius))
                    _rVal.SetPropVal("Start Angle", ArcStructure.StartAngle)
                    _rVal.SetPropVal("End Angle", ArcStructure.EndAngle)
                    _rVal.SetPropVal("*StartWidth", ArcStructure.StartWidth)
                    _rVal.SetPropVal("*EndWidth", ArcStructure.EndWidth)
                    _rVal.SetPropVal("End Angle", ArcStructure.EndAngle)
                End If
            Else
                _rVal.DefPts.VectorSet(1, LineStructure.SPT)
                _rVal.DefPts.VectorSet(2, LineStructure.EPT)
                _rVal.SetPropVal("*StartWidth", LineStructure.StartWidth)
                _rVal.SetPropVal("*EndWidth", LineStructure.EndWidth)
                _rVal.DefPts.Plane = _rVal.DefPts.Plane.AlignedTo(New TVECTOR(_rVal.DefPts.Vector1.DirectionTo(_rVal.DefPts.Vector2)), dxxAxisDescriptors.X)
            End If
            Return _rVal
        End Function


        Public Function GetPath(aPlane As TPLANE, ByRef rSegments As TSEGMENTS, ByRef rExtentVectors As TVECTORS, Optional bUpdateArcPts As Boolean = False) As TPATH
            Dim rNoWidth As Boolean = False
            Return GetPath(aPlane, rSegments, rExtentVectors, bUpdateArcPts, rNoWidth)
        End Function
        Public Function GetPath(aPlane As TPLANE, ByRef rSegments As TSEGMENTS, ByRef rExtentVectors As TVECTORS, bUpdateArcPts As Boolean, ByRef rNoWidth As Boolean) As TPATH
            Dim _rVal As New TPATH(dxxDrawingDomains.Model, DisplayStructure, New dxfPlane(aPlane)) With {
                .Plane = aPlane,
                .Color = DisplayStructure.Color,
                .Linetype = DisplayStructure.Linetype,
                .LayerName = DisplayStructure.LayerName,
                .LTScale = DisplayStructure.LTScale,
                .LineWeight = DisplayStructure.LineWeight
            }
            rExtentVectors = New TVECTORS(0)


            Dim pVecs As TVECTORS
            Dim eVecs As TVECTORS
            Dim nVecs As New TVECTORS(0)
            Dim bNoStart As Boolean
            Dim aLoop As TVECTORS
            Dim pCnt As Integer
            Dim span As Double
            rSegments = SegmentBounds(aPlane, bUpdateArcPts, rNoWidth)
            If rSegments.Count <= 0 Then Return _rVal
            _rVal.Filled = Not rNoWidth

            For i As Integer = 1 To rSegments.Count
                Dim aSeg As TSEGMENT = rSegments.Item(i)
                pVecs = New TVECTORS(0)
                eVecs = New TVECTORS(0)
                bNoStart = _rVal.Looop(1).Count > 0
                If aSeg.IsArc Then
                    span = aSeg.ArcStructure.SpannedAngle
                    pVecs = TBEZIER.ArcPath(aSeg.ArcStructure, bFullCircle:=False, bNoStart:=bNoStart)
                    If span >= 90 And span < 180 Then
                        pCnt = 4
                    ElseIf span >= 180 And span < 270 Then
                        pCnt = 6
                    ElseIf span >= 270 Then
                        pCnt = 8
                    Else
                        pCnt = 3
                    End If
                    eVecs = aSeg.ArcStructure.PhantomPoints(pCnt)
                Else
                    If Not bNoStart Then eVecs.Add(aSeg.LineStructure.SPT)
                    eVecs.Add(aSeg.LineStructure.EPT)
                    If Not bNoStart Then pVecs.Add(aSeg.LineStructure.SPT, TVALUES.ToByte(dxxVertexStyles.MOVETO))
                    pVecs.Add(aSeg.LineStructure.EPT, TVALUES.ToByte(dxxVertexStyles.LINETO))
                End If
                ' pVecs.Print("CIRCLE PATH")
                'eVecs.Print("EXTENTPT")
                If _rVal.LoopCount = 0 Then
                    _rVal.AddLoop(pVecs)
                End If
                aLoop = _rVal.Looop(1)
                'If aLoop.Count > 0 Then j = 1
                'aLoop.Append(pVecs, 1) ', j
                rExtentVectors.Append(eVecs)
                _rVal.SetLoop(1, aLoop)
            Next i
            '_rVal.Loops(0).Print()
            Return _rVal
        End Function
        Public Function LineBounds(aPlane As TPLANE, ByRef rNoWidth As Boolean) As TSEGMENTS
            Dim _rVal As New TSEGMENTS(0)

            Dim sp As TVECTOR = LineStructure.SPT
            Dim ep As TVECTOR = LineStructure.EPT
            Dim aDir As TVECTOR = sp.DirectionTo(ep)
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim v3 As TVECTOR
            Dim v4 As TVECTOR
            Dim l1 As TLINE
            Dim l2 As TLINE
            Dim l3 As New TLINE
            Dim swd As Double = Math.Round(Math.Abs(LineStructure.StartWidth), 6)
            Dim ewd As Double = Math.Round(Math.Abs(LineStructure.EndWidth), 6)
            Dim zDir As TVECTOR = aPlane.ZDirection
            Dim sZero As Double = 0.000001
            Dim sea As Double = StartEdgeAngle
            Dim eea As Double = EndEdgeAngle

            rNoWidth = (swd < sZero And ewd <= sZero)
            If dxfProjections.DistanceTo(sp, ep) <= sZero Or rNoWidth Then
                _rVal.Add(sp, ep, aIdentifier:="EDGE1")
            Else
                aDir.RotateAbout(zDir, -90, False)
                v1 = sp + aDir * (0.5 * swd)
                v2 = ep + aDir * (0.5 * ewd)
                v3 = ep + aDir * (-0.5 * ewd)
                v4 = sp + aDir * (-0.5 * swd)
                l1 = New TLINE(v1, v2)
                l2 = New TLINE(v3, v4)
                If swd > sZero Then
                    l3.SPT = v1
                    l3.EPT = v4
                    If sea <> 0 Then
                        l3.SPT.RotateAbout(sp, zDir, sea, False, True)
                        l3.EPT.RotateAbout(sp, zDir, sea, False, True)
                        v1 = l1.IntersectionPt(l3)
                        v4 = l2.IntersectionPt(l3)
                    End If
                End If
                If ewd > sZero Then
                    l3.SPT = v2
                    l3.EPT = v3
                    If eea <> 0 Then
                        l3.SPT.RotateAbout(ep, zDir, eea, False, True)
                        l3.EPT.RotateAbout(ep, zDir, eea, False, True)
                        v2 = l1.IntersectionPt(l3)
                        v3 = l2.IntersectionPt(l3)
                    End If
                End If
                If swd > sZero Then
                    _rVal.Add(v4, v1, aIdentifier:="STARTWIDTH")
                End If
                _rVal.Add(v1, v2, aIdentifier:="EDGE1")
                If ewd > sZero Then
                    _rVal.Add(v2, v3, aIdentifier:="ENDWIDTH")
                End If
                _rVal.Add(v3, v4, aIdentifier:="EDGE2")
            End If
            Return _rVal
        End Function
        Public Function SegmentBounds(aPlane As TPLANE, Optional bUpdateArcPts As Boolean = False) As TSEGMENTS
            Dim rNoWidth As Boolean = False
            Return SegmentBounds(aPlane, bUpdateArcPts, rNoWidth)
        End Function
        Public Function SegmentBounds(aPlane As TPLANE, bUpdateArcPts As Boolean, ByRef rNoWidth As Boolean) As TSEGMENTS
            If IsArc Then
                Return ArcBounds(bUpdateArcPts, rNoWidth)
            Else
                Return LineBounds(aPlane, rNoWidth)
            End If
        End Function
        Public Overrides Function ToString() As String
            If IsArc Then
                Return $"ARC SEGMENT -{ ArcStructure.ToString}"
            Else
                Return $"LINE SEGMENT-{ LineStructure.ToString}"
            End If
        End Function
#End Region'Methods
#Region "Operators"
        Public Shared Widening Operator CType(aSegment As TSEGMENT) As dxfEntity

            If aSegment.IsArc Then
                If aSegment.ArcStructure.Elliptical Then
                    Return New dxeEllipse(aSegment)
                Else
                    Return New dxeArc(aSegment)
                End If
            Else
                Return New dxeLine(aSegment)
            End If
        End Operator
#End Region 'Operators
    End Structure 'TSEGMENT
    Friend Structure TSEGMENTS
        Implements ICloneable
#Region "Members"
        Public Marker As Boolean
        Private _Members() As TSEGMENT
        Private _Init As Boolean

#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aCount As Integer = 0)
            'init --------------------------------------------------
            _Init = True
            ReDim _Members(-1)
            'init --------------------------------------------------
            If aCount <= 0 Then Return
            For i As Integer = 1 To aCount
                Add(New TSEGMENT(""))
            Next i
        End Sub
        Friend Sub New(aSegments As TSEGMENTS)
            'init --------------------------------------------------
            _Init = True
            ReDim _Members(-1)
            'init --------------------------------------------------

            For i As Integer = 1 To aSegments.Count
                Add(New TSEGMENT(aSegments.Item(i)))
            Next i
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
        Public Function Clone() As TSEGMENTS

            Return New TSEGMENTS(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TSEGMENTS(Me)
        End Function

        Public Function DisplayStructure(aIndex As Integer) As TDISPLAYVARS
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return TDISPLAYVARS.Null
            End If
            Return _Members(aIndex - 1).DisplayStructure
        End Function
        Public Sub SetDisplayStructure(aIndex As Integer, value As TDISPLAYVARS)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1).DisplayStructure = value
        End Sub
        Public Function SPT(aIndex As Integer) As TVECTOR
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return TVECTOR.Zero
            End If
            Return _Members(aIndex - 1).LineStructure.SPT
        End Function
        Public Sub SetSPT(aIndex As Integer, value As TVECTOR)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1).LineStructure.SPT = value
        End Sub
        Public Function EPT(aIndex As Integer) As TVECTOR
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return TVECTOR.Zero
            End If
            Return _Members(aIndex - 1).LineStructure.EPT
        End Function
        Public Sub SetEPT(aIndex As Integer, value As TVECTOR)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1).LineStructure.EPT = value
        End Sub
        Public Function Center(aIndex As Integer) As TVECTOR
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return TVECTOR.Zero
            End If
            Return _Members(aIndex - 1).ArcStructure.Center
        End Function
        Public Sub SetCenter(aIndex As Integer, value As TVECTOR)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1).ArcStructure.Center = value
        End Sub
        Public Function Item(aIndex As Integer, Optional bSuppressIndexErr As Boolean = False) As TSEGMENT
            If aIndex < 1 Or aIndex > Count Then
                If bSuppressIndexErr Then Return New TSEGMENT("")
                Throw New IndexOutOfRangeException()
            End If
            Return _Members(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, aMember As TSEGMENT)
            'Base 1
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1) = aMember
        End Sub
        Public Function Line(aIndex As Integer) As TLINE
            'Base 1
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return TLINE.Null
            End If
            Return _Members(aIndex - 1).LineStructure
        End Function
        Public Sub SetLine(aIndex As Integer, value As TLINE)
            'Base 1
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1).IsArc = False
            _Members(aIndex - 1).LineStructure = value
        End Sub
        Public Function Arc(aIndex As Integer) As TARC
            'Base 1
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return New TARC(0)
            End If
            Return _Members(aIndex - 1).ArcStructure
        End Function
        Public Sub SetArc(aIndex As Integer, value As TARC)
            'Base 1
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1).IsArc = True
            _Members(aIndex - 1).ArcStructure = value
        End Sub
        Public Function ArcCount(Optional bCountEllipses As Boolean = False) As Integer
            Dim _rVal As Integer
            Dim i As Integer
            For i = 1 To Count
                If _Members(i - 1).IsArc Then
                    If (bCountEllipses And Not _Members(i - 1).ArcStructure.Elliptical) Or (Not bCountEllipses) Then _rVal += 1
                End If
            Next i
            Return _rVal
        End Function
        Public Function Bounds(aPlane As TPLANE, Optional bSuppressProjection As Boolean = False, Optional aCurveDivisions As Integer = 20) As TPLANE
            Dim aSeg As TSEGMENT
            Dim vPts As New TVECTORS(0)
            For i As Integer = 1 To Count
                aSeg = _Members(i - 1)
                If Not aSeg.IsArc Then
                    vPts.Add(aSeg.LineStructure.SPT)
                    vPts.Add(aSeg.LineStructure.EPT)
                Else
                    vPts.Append(aSeg.ArcStructure.PhantomPoints(aCurveDivisions))
                End If
            Next i
            Return vPts.Bounds(aPlane)
        End Function

        Public Sub Clear()
            _Init = True
            ReDim _Members(-1)
        End Sub

        Public Sub Add(aMember As TSEGMENT, Optional aOwnerGUID As String = Nothing, Optional aIdentifier As String = Nothing, Optional bAddtoStart As Boolean = False)

            If Not _Init Then
                Clear()
            End If
            System.Array.Resize(_Members, Count + 1)
            Dim idx As Integer
            If Not bAddtoStart Then
                idx = Count - 1
                _Members(idx) = aMember.Clone
            Else
                idx = 0
                Dim j As Integer = Count
                For i As Integer = j - 1 To 1 Step -1
                    _Members(j - 1) = _Members(i - 1)
                    j -= 1
                Next i
                _Members(0) = aMember
            End If
            aMember = _Members(idx)
            If aOwnerGUID IsNot Nothing Then
                aMember.OwnerGUID = aOwnerGUID.Trim()
                _Members(idx) = aMember
            End If
            If aIdentifier IsNot Nothing Then
                aMember.Identifier = aIdentifier.Trim()
                _Members(idx) = aMember
            End If
        End Sub
        Public Sub Add(aArc As TARC, Optional aOwnerGUID As String = Nothing, Optional aIdentifier As String = Nothing, Optional bAddtoStart As Boolean = False)

            Add(New TSEGMENT(aArc), aOwnerGUID, aIdentifier, bAddtoStart)

        End Sub
        Public Sub Add(aLine As TLINE, Optional aOwnerGUID As String = Nothing, Optional aIdentifier As String = Nothing, Optional bAddtoStart As Boolean = False)

            Add(New TSEGMENT(aLine), aOwnerGUID, aIdentifier, bAddtoStart)

        End Sub
        Public Sub Add(aSP As TVECTOR, aEP As TVECTOR, Optional bInfinite As Boolean = False, Optional aOwnerGUID As String = Nothing, Optional aIdentifier As String = Nothing)
            Add(New TSEGMENT(aSP, aEP, bInfinite), aOwnerGUID, aIdentifier)
        End Sub
        Public Sub Add(aSP As TPOINT, aEP As TPOINT, Optional bInfinite As Boolean = False, Optional aOwnerGUID As String = Nothing, Optional aIdentifier As String = Nothing)
            Add(New TSEGMENT(aSP, aEP, bInfinite), aOwnerGUID, aIdentifier)
        End Sub
        Public Sub Append(bArcLines As TSEGMENTS, Optional aIdentifier As String = Nothing)
            Try
                Dim cnt As Integer = Count
                If cnt <= 0 Then
                    _Members = bArcLines._Members.Clone
                Else
                    _Members = _Members.Concat(bArcLines._Members).ToArray
                End If
                If aIdentifier IsNot Nothing Then
                    For i As Integer = cnt - 1 To Count - 1
                        _Members(i - 1).Identifier = aIdentifier.ToString
                    Next i
                End If
            Catch ex As Exception
            End Try
            Return
        End Sub
        Public Function ContainsVector(aVector As TVECTOR, aFudgeFactor As Double, aSuppressPlaneCheck As Boolean, ByRef rIsEndPoint As Boolean, Optional aRunTilTrue As Boolean = False) As Boolean
            Dim _rVal As Boolean = False
            rIsEndPoint = False
            Dim aArcL As TSEGMENT
            Dim aFlag As Boolean
            Dim bFlag As Boolean
            For i As Integer = 1 To Count
                aArcL = _Members(i - 1)
                If aArcL.ContainsVector(aVector, aFudgeFactor, aFlag, bFlag, False, aSuppressPlaneCheck) Then
                    _rVal = True
                    If aFlag Or bFlag Then rIsEndPoint = True
                    If aRunTilTrue Then Exit For
                End If
            Next i
            Return _rVal
        End Function
        Public Function EncloseVector(aVector As TVECTOR, aBoundingRectangle As TPLANE, Optional aFudgeFactor As Double = 0.001, Optional aOnBoundIsIn As Boolean = True) As Boolean
            Dim rIsonBound As Boolean = False
            Dim rIsEndPoint As Boolean = False
            Return EncloseVector(aVector, aBoundingRectangle, aFudgeFactor, rIsonBound, rIsEndPoint, aOnBoundIsIn)
        End Function
        Public Function EncloseVector(aVector As TVECTOR, aBoundingRectangle As TPLANE, aFudgeFactor As Double, ByRef rIsonBound As Boolean, ByRef rIsEndPoint As Boolean, Optional aOnBoundIsIn As Boolean = True) As Boolean
            Dim _rVal As Boolean
            '#1the bounding segments to test
            '#2the vector to test
            '#3the plane to use
            '#4a fudge factor to apply
            '#5returns true if the passed vector lies on a bounding segment
            '#6returns true if the passed vector is the end or start vector of the a member of the collection
            '#7flag to treat a vector on the boundary as within the boundary
            '^returns true if the passed vector is enclosed by the passed collection of entities
            '~all entities are assumed to lie on the working plane
            rIsonBound = False
            rIsEndPoint = False
            Try
                If Count <= 0 Then Return _rVal
                Dim aPlane As TPLANE = aBoundingRectangle
                Dim d1 As Double
                Dim f1 As Double
                Dim i As Long
                Dim aSeg As TSEGMENT
                Dim aFlag As Boolean
                Dim bFlag As Boolean
                Dim diag As Double
                Dim iPts As TVECTORS
                If aPlane.Width = 0 Or aPlane.Height = 0 Then aPlane = Bounds(aPlane)
                If aPlane.Width <> 0 Or aPlane.Height <> 0 Then diag = Math.Sqrt(aPlane.Width ^ 2 + aPlane.Height ^ 2)
                f1 = Math.Abs(aFudgeFactor)
                If f1 > 0.1 Then f1 = 0.1
                If f1 < 0.000001 Then f1 = 0.000001
                For i = 0 To Count - 1
                    aSeg = _Members(i)
                    If aSeg.ContainsVector(aVector, f1, aFlag, bFlag, False, True) Then
                        rIsonBound = True
                        If aFlag Or bFlag Then rIsEndPoint = True
                    End If
                Next i
                'see if its on my plane
                If Not aVector.LiesOn(aPlane, f1) Then Return _rVal
                'chech the bounds
                If rIsonBound Then
                    If aOnBoundIsIn Then _rVal = True
                Else
                    'gross check
                    d1 = aPlane.Origin.DistanceTo(aVector)
                    If d1 > (diag / 2) + (3 * f1) Then Return _rVal
                    aPlane.Origin = aVector
                    Dim testLine As New TSEGMENT(aPlane.Line(3 * diag, 0, bFromEndPt:=True))
                    iPts = dxfIntersections.LAE_LAES(testLine, Me, False, False)
                    If iPts.Count > 0 Then
                        _rVal = (iPts.Count Mod 2 <> 0)
                    End If
                End If
            Catch ex As Exception
            End Try
            Return _rVal
        End Function
        Public Function GetByMark(aMark As String, Optional bIgnorCase As Boolean = True) As TSEGMENTS
            Dim rReturn As New TSEGMENTS(0)
            Dim rTheOthers As New TSEGMENTS(0)
            If aMark Is Nothing Then aMark = ""
            For i As Integer = 1 To Count
                If String.Compare(_Members(i - 1).Mark, aMark, ignoreCase:=bIgnorCase) = 0 Then
                    rReturn.Add(_Members(i - 1))
                Else
                    rTheOthers.Add(_Members(i - 1))
                End If
            Next i
            Return rReturn
        End Function
        Public Sub Translate(aTranslation As TVECTOR)
            For i As Integer = 1 To Count
                _Members(i - 1).Translate(aTranslation)
            Next
        End Sub
        Public Function GetByIdentifier(aIdentifier As String, Optional bIgnoreCase As Boolean = True) As TSEGMENTS
            Dim rReturn As New TSEGMENTS(0)
            Dim rTheOthers As New TSEGMENTS(0)
            If aIdentifier Is Nothing Then aIdentifier = ""
            For i As Integer = 1 To Count
                If String.Compare(_Members(i - 1).Identifier, aIdentifier, ignoreCase:=bIgnoreCase) = 0 Then
                    rReturn.Add(_Members(i - 1))
                Else
                    rTheOthers.Add(_Members(i - 1))
                End If
            Next i
            Return rReturn
        End Function

        Public Function PolylineVertices(aOCS As TPLANE, bClosed As Boolean, aGlobalWidth As Double) As TPROPERTIES
            Dim rVCount As Integer = 0
            Dim rUniWidth As Double = 0.0
            Return PolylineVertices(aOCS, bClosed, aGlobalWidth, rVCount, rUniWidth)
        End Function
        Public Function PolylineVertices(aOCS As TPLANE, bClosed As Boolean, aGlobalWidth As Double, ByRef rVCount As Integer, ByRef rUniWidth As Double) As TPROPERTIES
            Dim _rVal As New TPROPERTIES()
            rVCount = 0
            Dim aSeg As TSEGMENT
            Dim v1 As TVECTOR
            Dim sw As Double
            Dim ew As Double
            Dim blg As Double
            Dim v2 As TVECTOR
            Dim vprime As TVECTOR
            Dim uwd As Double
            Dim buniwd As Boolean
            rUniWidth = -1
            uwd = -1
            buniwd = True
            If aGlobalWidth < 0 Then aGlobalWidth = 0
            For i As Integer = 1 To Count
                aSeg = _Members(i - 1)
                blg = 0
                sw = 0
                ew = 0
                rVCount += 1
                If Not aSeg.IsArc Then
                    v1 = aSeg.LineStructure.SPT
                    v2 = aSeg.LineStructure.EPT
                    If uwd < 0 Then uwd = Math.Round(aSeg.LineStructure.StartWidth, 6)
                    If Math.Round(aSeg.LineStructure.StartWidth, 6) <> uwd Then buniwd = False
                    If Math.Round(aSeg.LineStructure.EndWidth, 6) <> uwd Then buniwd = False
                    If aGlobalWidth <= 0 Then
                        sw = aSeg.LineStructure.StartWidth
                        ew = aSeg.LineStructure.EndWidth
                        If sw < 0 Then sw = aGlobalWidth
                        If ew < 0 Then ew = aGlobalWidth
                    End If
                Else
                    'als_UpdateArcPoints(aSeg, True)
                    v1 = aSeg.ArcStructure.StartPt
                    v2 = aSeg.ArcStructure.EndPt
                    If uwd < 0 Then uwd = Math.Round(aSeg.ArcStructure.StartWidth, 6)
                    If Math.Round(aSeg.ArcStructure.StartWidth, 6) <> uwd Then buniwd = False
                    If Math.Round(aSeg.ArcStructure.EndWidth, 6) <> uwd Then buniwd = False
                    If aGlobalWidth = 0 Then
                        sw = aSeg.ArcStructure.StartWidth
                        ew = aSeg.ArcStructure.EndWidth
                        If sw < 0 Then sw = aGlobalWidth
                        If ew < 0 Then ew = aGlobalWidth
                    End If
                    blg = aSeg.ArcStructure.Bulge
                End If
                If i = 1 Then vprime = v1
                v1 = v1.WithRespectTo(aOCS)
                _rVal.AddVector(10, v1, $"Vertex { rVCount}", True)
                _rVal.Add(New TPROPERTY(40, sw, "Start Width", dxxPropertyTypes.dxf_Double))
                _rVal.Add(New TPROPERTY(41, ew, "End Width", dxxPropertyTypes.dxf_Double))
                If blg <> 0 Then
                    _rVal.Add(New TPROPERTY(42, blg, "Bulge", dxxPropertyTypes.dxf_Double))
                End If
                If i = Count Then
                    If Not bClosed Then
                        If Not v2.Equals(vprime, False, 5) Then
                            rVCount += 1
                            v1 = v2.WithRespectTo(aOCS)
                            _rVal.AddVector(10, v1, "Vertex " & rVCount, True)
                            _rVal.Add(New TPROPERTY(40, sw, "Start Width", dxxPropertyTypes.dxf_Double))
                            _rVal.Add(New TPROPERTY(41, ew, "End Width", dxxPropertyTypes.dxf_Double))
                            If blg > 0 Then
                                _rVal.Add(New TPROPERTY(42, blg, "Bulge", dxxPropertyTypes.dxf_Double))
                            End If
                        Else
                            bClosed = True
                        End If
                    Else
                        If Not v2.Equals(vprime, False, 5) Then
                            bClosed = False
                            rVCount += 1
                            v1 = v2.WithRespectTo(aOCS)
                            _rVal.AddVector(10, v1, "Vertex " & rVCount, True)
                            If sw > 0 Then
                                _rVal.Add(New TPROPERTY(40, sw, "Start Width", dxxPropertyTypes.dxf_Double))
                            End If
                            If ew > 0 Then
                                _rVal.Add(New TPROPERTY(41, ew, "End Width", dxxPropertyTypes.dxf_Double))
                            End If
                            If blg > 0 Then
                                _rVal.Add(New TPROPERTY(42, blg, "Bulge", dxxPropertyTypes.dxf_Double))
                            End If
                        End If
                    End If
                End If
            Next i
            If buniwd Then rUniWidth = uwd
            Return _rVal
        End Function
        Public Function Remove(aIndex As Integer) As TSEGMENT
            Dim _rVal As New TSEGMENT
            If aIndex < 1 Or aIndex > Count Then Return _rVal
            Dim rALS As New TSEGMENTS(0)
            For i As Integer = 1 To Count
                If i = aIndex Then
                    _rVal = _Members(i - 1)
                Else
                    rALS.Add(_Members(i - 1))
                End If
            Next i
            _Members = rALS._Members
            Return _rVal
        End Function

        Public Function ToList() As List(Of TSEGMENT)
            If Count <= 0 Then Return New List(Of TSEGMENT)
            Return _Members.ToList()
        End Function


        Public Overrides Function ToString() As String
            Return $"TSEGMENTS[{ Count }]"
        End Function
#End Region'Methods

    End Structure 'TSEGMENTS
    Friend Structure TLINE
        Implements ICloneable
#Region "Members"
        Public EndWidth As Double
        Public EPT As TVECTOR
        Public SPT As TVECTOR
        Public StartWidth As Double
        Public Tag As String
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aTag As String = "")
            'init ------------------------
            StartWidth = 0
            EndWidth = 0
            SPT = TVECTOR.Zero
            EPT = TVECTOR.Zero
            Tag = aTag
            'init ------------------------

        End Sub
        Friend Sub New(aLine As TLINE)
            'init ------------------------
            StartWidth = aLine.StartWidth
            EndWidth = aLine.EndWidth
            SPT = New TVECTOR(aLine.SPT)
            EPT = New TVECTOR(aLine.EPT)
            Tag = aLine.Tag
            'init ------------------------

        End Sub



        Public Sub New(aSP As TVECTOR, aEP As TVECTOR, Optional bInverted As Boolean = False)
            'init ------------------------
            StartWidth = 0
            EndWidth = 0
            SPT = TVECTOR.Zero
            EPT = TVECTOR.Zero
            Tag = ""
            'init ------------------------
            If Not bInverted Then
                SPT = New TVECTOR(aSP)
                EPT = New TVECTOR(aEP)
            Else
                EPT = New TVECTOR(aSP)
                SPT = New TVECTOR(aEP)
            End If
        End Sub
        Public Sub New(aSP As dxfVector, aEP As dxfVector, Optional bInverted As Boolean = False)
            'init ------------------------
            StartWidth = 0
            EndWidth = 0
            SPT = TVECTOR.Zero
            EPT = TVECTOR.Zero
            Tag = ""
            'init ------------------------
            If Not bInverted Then
                SPT = New TVECTOR(aSP)
                EPT = New TVECTOR(aEP)
            Else
                EPT = New TVECTOR(aSP)
                SPT = New TVECTOR(aEP)
            End If
        End Sub
        Public Sub New(aSP As TVERTEX, aEP As TVERTEX, Optional bInverted As Boolean = False)
            'init ------------------------
            StartWidth = 0
            EndWidth = 0
            SPT = TVECTOR.Zero
            EPT = TVECTOR.Zero
            Tag = ""
            'init ------------------------
            If Not bInverted Then
                SPT = New TVECTOR(aSP.Vector)
                EPT = New TVECTOR(aEP.Vector)
            Else
                EPT = New TVECTOR(aSP.Vector)
                SPT = New TVECTOR(aEP.Vector)
            End If
        End Sub
        Public Sub New(aOrigin As TVECTOR, aDirection As TVECTOR, aDistance As Double, Optional binverted As Boolean = False)
            'init ------------------------
            StartWidth = 0
            EndWidth = 0
            SPT = TVECTOR.Zero
            EPT = TVECTOR.Zero
            Tag = ""
            'init ------------------------
            If TVECTOR.IsNull(aDirection) Then aDirection = TVECTOR.WorldX
            aDirection.Normalize()
            If Not binverted Then
                SPT = New TVECTOR(aOrigin)
                EPT = aOrigin + aDirection * aDistance
            Else
                EPT = New TVECTOR(aOrigin)
                SPT = aOrigin + aDirection * aDistance
            End If
        End Sub

        Public Sub New(aLine As iLine, Optional aProjectionPlane As dxfPlane = Nothing)
            'init ------------------------
            StartWidth = 0
            EndWidth = 0
            SPT = TVECTOR.Zero
            EPT = TVECTOR.Zero
            Tag = ""
            'init ------------------------
            If aLine Is Nothing Then Return

            SPT = New TVECTOR(aLine.StartPt)
            EPT = New TVECTOR(aLine.EndPt)
            If TypeOf aLine Is dxeLine Then
                Dim dxflin As dxeLine = DirectCast(aLine, dxeLine)
                StartWidth = dxflin.StartWidth
                EndWidth = dxflin.EndWidth
                Tag = dxflin.Tag
            End If

            If Not dxfPlane.IsNull(aProjectionPlane) Then
                SPT = dxfProjections.ToPlane(SPT, aProjectionPlane)
                EPT = dxfProjections.ToPlane(EPT, aProjectionPlane)
            End If

        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Length As Double
            Get
                Return dxfProjections.DistanceTo(SPT, EPT)
            End Get
        End Property
        Public ReadOnly Property MPT As TVECTOR
            Get
                Return SPT.MidPt(EPT)
            End Get
        End Property
        Public ReadOnly Property EndPts As TVECTORS
            Get
                Return New TVECTORS(SPT, EPT)
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Public ReadOnly Property Plane As TPLANE
            Get
                Return TLINE.ToPlane(Me)
            End Get
        End Property
        Public Function Clone() As TLINE
            Return New TLINE(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TLINE(Me)
        End Function

        Public Function ContainsVector(aVector As TVECTOR, Optional aFudgeFactor As Double = 0.001,
                                                  Optional bTreatAsInfinite As Boolean = False, Optional bSuppressPlaneCheck As Boolean = True
                                                  ) As Boolean
            Dim rIsStartPt As Boolean = False
            Dim rIsEndPt As Boolean = False
            Dim rWithin As Boolean = False
            Return ContainsVector(aVector, aFudgeFactor, rIsStartPt, rIsEndPt, bTreatAsInfinite, bSuppressPlaneCheck, rWithin)
        End Function
        Public Function ContainsVector(aVector As TVECTOR, aFudgeFactor As Double, ByRef rIsStartPt As Boolean, ByRef rIsEndPt As Boolean,
                                                  Optional bTreatAsInfinite As Boolean = False, Optional bSuppressPlaneCheck As Boolean = True
                                                  ) As Boolean
            Dim rWithin As Boolean = False
            Return ContainsVector(aVector, aFudgeFactor, rIsStartPt, rIsEndPt, bTreatAsInfinite, bSuppressPlaneCheck, rWithin)
        End Function
        Public Function ContainsVector(aVector As TVECTOR, aFudgeFactor As Double, ByRef rIsStartPt As Boolean, ByRef rIsEndPt As Boolean, bTreatAsInfinite As Boolean, bSuppressPlaneCheck As Boolean, ByRef rWithin As Boolean) As Boolean
            Return TLINE.LineContainsVector(Me, aVector, aFudgeFactor, rIsStartPt, rIsEndPt, bTreatAsInfinite, bSuppressPlaneCheck, rWithin)
        End Function
        Public Sub Invert()
            Dim v1 As New TVECTOR(SPT)
            SPT = New TVECTOR(EPT)
            EPT = v1
        End Sub
        Public Function Inverse() As TLINE
            Dim _rVal As New TLINE(Me)
            _rVal.Invert()
            Return _rVal
        End Function

        Public Function AngleOfInclination(Optional aPlane As dxfPlane = Nothing) As Double
            '#1the coordinate system to use to get the X Axis
            '^the angle from the X Axis to this line
            '~if nothing is passed the world coordinate system is used
            Dim bPl As New dxfPlane(aPlane)

            Return bPl.XDirection.AngleTo(New dxfDirection(Direction))
        End Function
        Public Function Direction(Optional bReturnInverse As Boolean = False) As TVECTOR
            Dim rDirectionIsNull As Boolean = False
            Dim rDistance As Double = 0.0
            Return Direction(bReturnInverse, rDirectionIsNull, rDistance)
        End Function
        Public Function Direction(bReturnInverse As Boolean, ByRef rDirectionIsNull As Boolean, ByRef rDistance As Double) As TVECTOR
            Return SPT.DirectionTo(EPT, bReturnInverse, rDirectionIsNull, rDistance)
        End Function
        Public Overrides Function ToString() As String
            Return "TLINE[" & SPT.ToString.Replace("TVECTOR", "SP:") & EPT.ToString.Replace("TVECTOR", " EP:") & "]"
        End Function
        Public Function TValue(aLineVector As TVECTOR, Optional bPointIsOnLine As Boolean = False) As Double
            Dim d1 As Double
            Dim d2 As Double
            Dim aFlg As Boolean
            Try
                Dim aDir As TVECTOR = SPT.DirectionTo(EPT, False, aFlg, d1)
                If aFlg Then Return 0
                Dim bDir As TVECTOR = SPT.DirectionTo(aLineVector, False, aFlg, d2)
                If aFlg Then Return 0
                If aDir.Equals(bDir, True, 4, aFlg) Or bPointIsOnLine Then
                    If aFlg Then
                        Return -d2 / d1
                    Else
                        Return d2 / d1
                    End If
                Else
                    Return 0
                End If
            Catch ex As Exception
                Return 0
            End Try
        End Function
        Public Function LiesOn(aPlane As TPLANE, Optional aFudgeFactor As Double = 0.001, Optional bUseWorldOrigin As Boolean = False) As Boolean
            '#1the line to test
            '#2the plane
            '#3a tolerance for the test
            '#4flag to use a plane equal to the passed plane but center on 0,0,0 for the test
            '^returns True if both end points of the line lie on the plane
            If SPT.LiesOn(aPlane, aFudgeFactor, bUseWorldOrigin) Then
                If EPT.LiesOn(aPlane, aFudgeFactor, bUseWorldOrigin) Then
                    Return True
                Else
                    Return False
                End If
            Else
                Return False
            End If
        End Function
        Public Function Paramatize(ByRef rSP As TVECTOR, ByRef rDir As TVECTOR) As Boolean
            rSP = New TVECTOR(SPT)
            rDir = EPT - SPT
            Return Not TVECTOR.IsNull(rDir, 8)
        End Function
        Public Function PhantomPoints(aDivision As Integer, Optional bIncludeEndPt As Boolean = True, Optional aCollector As colDXFVectors = Nothing) As TVECTORS
            Dim _rVal As New TVECTORS(0)
            '^returns the phantom vertices for the line segment.
            '~contains the start and end vectors of the segment
            'On Error Resume Next
            If aDivision < 1 Then aDivision = 1
            If aDivision > 10000 Then aDivision = 10000
            Dim aStep As Double
            Dim aLg As Double
            Dim aDir As TVECTOR
            Dim v1 As TVECTOR
            Dim sp As TVECTOR
            Dim ep As TVECTOR
            sp = SPT.Clone
            ep = EPT.Clone
            _rVal.Add(sp.Clone)
            If aCollector IsNot Nothing Then aCollector.AddV(sp)
            aLg = dxfProjections.DistanceTo(sp, ep)
            If aLg > 0 Then
                aStep = aLg / aDivision
                aDir = (ep - sp).Normalized()
                v1 = aDir * aStep
                For i As Integer = 1 To aDivision - 1
                    sp += v1
                    _rVal.Add(sp.Clone)
                    If aCollector IsNot Nothing Then aCollector.AddV(sp)
                Next i
                If bIncludeEndPt Then
                    _rVal.Add(ep)
                    If aCollector IsNot Nothing Then aCollector.AddV(ep)
                End If
            End If
            Return _rVal
        End Function
        Public Sub Project(aDirection As TVECTOR, aDistance As Double, Optional bSuppressNormalize As Boolean = True, Optional bInvertDirection As Boolean = False)

            If aDistance = 0 Then Return
            If Not bSuppressNormalize Then aDirection.Normalize()
            If bInvertDirection Then aDirection *= -1
            Translate(aDirection * aDistance)
        End Sub
        Public Sub Translate(aTranslation As TVECTOR)
            If TVECTOR.IsNull(aTranslation) Then Return
            SPT += aTranslation
            EPT += aTranslation
        End Sub
        Public Sub Rotate(aOrigin As TVECTOR, aNormal As TVECTOR, aAngle As Double, Optional bInRadians As Boolean = False)
            SPT.RotateAbout(aOrigin, aNormal, aAngle, bInRadians, True)
            EPT.RotateAbout(aOrigin, aNormal, aAngle, bInRadians, True)
        End Sub
        Public Function WithRespectTo(aPlane As TPLANE, Optional aPrecis As Integer = 8, Optional aScaleFactor As Double = 0.0, Optional bSuppressZ As Boolean = False) As TLINE
            Dim _rVal As TLINE = Clone()
            If TPLANE.IsNull(aPlane) Then Return _rVal
            _rVal.SPT = SPT.WithRespectTo(aPlane, aPrecis, aScaleFactor, bSuppressZ)
            _rVal.EPT = EPT.WithRespectTo(aPlane, aPrecis, aScaleFactor, bSuppressZ)
            '#1the structure of the plane
            '#2the number of decimals to round the returned vertices coordinates to
            '#3a scale factor to apply 0 means no scaling)
            '^returns the line with it's endpoints defined with respect to the center and origin of the passed plane
            Return _rVal
        End Function
        Public Function WithRespectTo(aCharBox As TCHARBOX, Optional aPrecis As Integer = 8, Optional aScaleFactor As Double = 0.0, Optional bSuppressZ As Boolean = False) As TLINE
            Return New TLINE(Me) With {.SPT = SPT.WithRespectTo(aCharBox, aPrecis, aScaleFactor, bSuppressZ), .EPT = EPT.WithRespectTo(aCharBox, aPrecis, aScaleFactor, bSuppressZ)}


            '#1the structure of the plane
            '#2the number of decimals to round the returned vertices coordinates to
            '#3a scale factor to apply 0 means no scaling)
            '^returns the line with it's endpoints defined with respect to the center and origin of the passed plane

        End Function

        Public Function WithRespectTo(aCharBox As dxoCharBox, Optional aPrecis As Integer = 8, Optional aScaleFactor As Double = 0.0, Optional bSuppressZ As Boolean = False) As TLINE
            Dim _rVal As TLINE = Clone()
            If aCharBox Is Nothing Then Return _rVal
            _rVal.SPT = SPT.WithRespectTo(aCharBox, aPrecis, aScaleFactor, bSuppressZ)
            _rVal.EPT = EPT.WithRespectTo(aCharBox, aPrecis, aScaleFactor, bSuppressZ)
            '#1the structure of the plane
            '#2the number of decimals to round the returned vertices coordinates to
            '#3a scale factor to apply 0 means no scaling)
            '^returns the line with it's endpoints defined with respect to the center and origin of the passed plane
            Return _rVal
        End Function
        Friend Function IntersectionPts(aArc As TARC, Optional bSuppressPlaneCheck As Boolean = False, Optional aLineIsInfinite As Boolean = True, Optional aArcIsInfinite As Boolean = True, Optional aCurveDivisions As Integer = 1000, Optional aCollector As colDXFVectors = Nothing) As TVECTORS
            Dim _rVal As New TVECTORS(0)
            If SPT.DistanceTo(EPT) <= 0.00001 Then Return _rVal
            Dim bPlanar As Boolean
            If aArc.SpannedAngle >= 359.99 Then aArcIsInfinite = True
            If Not bSuppressPlaneCheck Then
                bPlanar = LiesOn(aArc.Plane)
            Else
                bPlanar = True
            End If
            Dim ips As TVECTORS
            Dim aPl As TPLANE = aArc.Plane
            Dim v1 As TVECTOR
            Dim bKeep As Boolean
            Dim bLine As New TSEGMENT(Me, aLineIsInfinite)
            Dim bArc As New TSEGMENT(aArc, aArcIsInfinite)
            Dim aDir As TVECTOR
            If aArc.Elliptical Then
                If aArc.MinorRadius <= 0 Or aArc.Radius <= 0 Then Return _rVal
                If aCurveDivisions < 10 Then aCurveDivisions = 10
                If aCurveDivisions > 10000 Then aCurveDivisions = 10000
                Dim bL As TLINE
                Dim aFlag As Boolean
                Dim bFlag As Boolean
                Dim cFlag As Boolean
                Dim ang As Double
                Dim xDir As TVECTOR = aPl.XDirection
                Dim zDir As TVECTOR = aPl.ZDirection
                Dim cp As TVECTOR = aPl.Origin
                Dim majD As Double = 2 * aArc.Radius
                Dim minD As Double = 2 * aArc.MinorRadius
                Dim bCreateLines As Boolean
                Dim esegs As TLINES = aArc.EllipseSegments
                bCreateLines = esegs.Count = 0 Or esegs.Count <> aCurveDivisions
                If bCreateLines Then
                    esegs = aArc.PhantomLines(aCurveDivisions, True)
                    aArc.EllipseSegments = esegs
                End If
                Dim rLinesAreCoincident As Boolean = False
                Dim rInterceptExists As Boolean = False
                For i As Integer = 1 To esegs.Count
                    bL = esegs.Item(i)
                    bKeep = False
                    v1 = IntersectionPt(bL, aFlag, rLinesAreCoincident, bFlag, cFlag, rInterceptExists)
                    If Not aFlag Then
                        bKeep = cFlag 'the intersection lies on the complete ellipse
                        'bKeep = bFlag And cFlag    'the intersection lies on both lines
                    End If
                    If bKeep Then
                        'v2 = bL.SPT.interpolate( bL.EPT, 0.5)
                        aDir = cp.DirectionTo(v1)
                        ang = xDir.AngleTo(aDir, zDir)
                        v1 = dxfUtils.EllipsePoint(cp, majD, minD, ang, aPl)
                        _rVal.Add(v1)
                        If aCollector IsNot Nothing Then aCollector.AddV(v1)
                    End If
                Next i
                Return _rVal
            Else
                If bPlanar Then
                    _rVal = dxfIntersections.PlanarLineArc(Me, aArc, aLineIsInfinite, aArcIsInfinite, aCollector)
                    Return _rVal
                End If
                ips = dxfIntersections.LineSphere(Me, aArc.Plane.Origin, aArc.Radius)
                If ips.Count <= 0 Then Return _rVal
                aPl = aArc.Plane
                For i As Integer = 1 To ips.Count
                    v1 = ips.Item(i)
                    bKeep = v1.LiesOn(aArc.Plane)
                    If bKeep Then
                        aDir = aPl.Origin.DirectionTo(v1)
                        v1 = aPl.Origin + aDir * aArc.Radius
                    End If
                    If bKeep And Not aLineIsInfinite Then
                        If Not ContainsVector(v1, 0.001) Then bKeep = False
                    End If
                    If bKeep And Not aArcIsInfinite Then
                        If Not bArc.ContainsVector(v1, aFudgeFactor:=0.001) Then bKeep = False
                    End If
                    If bKeep Then
                        _rVal.Add(v1)
                        If aCollector IsNot Nothing Then aCollector.AddV(v1)
                    End If
                Next i
            End If
            Return _rVal
        End Function
        Friend Function IntersectionPts(bLines As TLINES, Optional aLine_IsInfinite As Boolean = False, Optional bLines_AreInfinite As Boolean = False, Optional bMustBeOnBoth As Boolean = True, Optional bJustOne As Boolean = False, Optional aCollector As colDXFVectors = Nothing) As TVECTORS
            Dim _rVal As New TVECTORS(0)
            'On Error Resume Next
            Dim iL As TLINE = Me
            Dim jL As TLINE
            Dim v1 As TVECTOR
            Dim bOn1 As Boolean
            Dim bOn2 As Boolean
            Dim bKeep As Boolean
            Dim bparel As Boolean = False
            Dim coinc As Boolean = False
            For j As Integer = 1 To bLines.Count
                jL = bLines.Item(j)
                v1 = iL.IntersectionPt(jL, bparel, coinc, bOn1, bOn2, bKeep)
                If bKeep Then  'keep is false if the intersection is not found
                    If bMustBeOnBoth Then
                        If Not aLine_IsInfinite Then
                            If Not bOn1 Then bKeep = False
                        End If
                        If Not bLines_AreInfinite Then
                            If Not bOn2 Then bKeep = False
                        End If
                    End If
                    If bKeep Then
                        _rVal.Add(v1)
                        If aCollector IsNot Nothing Then aCollector.AddV(v1)
                        If bJustOne Then Return _rVal
                    End If
                End If
            Next j
            Return _rVal
        End Function
        Friend Function IntersectionPt(bLine As TLINE) As TVECTOR
            Dim rLinesAreParallel As Boolean = False
            Dim rLinesAreCoincident As Boolean = False
            Dim rIsOnFirstLine As Boolean = False
            Dim rIsOnSecondLine As Boolean = False
            Dim rInterceptExists As Boolean = False
            Return IntersectionPt(bLine, rLinesAreParallel, rLinesAreCoincident, rIsOnFirstLine, rIsOnSecondLine, rInterceptExists)
        End Function
        Friend Function IntersectionPt(bLine As TLINE, ByRef rInterceptExists As Boolean) As TVECTOR
            Dim rLinesAreParallel As Boolean = False
            Dim rLinesAreCoincident As Boolean = False
            Dim rIsOnFirstLine As Boolean = False
            Dim rIsOnSecondLine As Boolean = False
            Return IntersectionPt(bLine, rLinesAreParallel, rLinesAreCoincident, rIsOnFirstLine, rIsOnSecondLine, rInterceptExists)
        End Function

        Friend Function IntersectionPt(bLine As TLINE, ByRef rInterceptExists As Boolean, ByRef rLinesAreParallel As Boolean) As TVECTOR

            Dim rLinesAreCoincident As Boolean = False
            Dim rIsOnFirstLine As Boolean = False
            Dim rIsOnSecondLine As Boolean = False
            Return IntersectionPt(bLine, rLinesAreParallel, rLinesAreCoincident, rIsOnFirstLine, rIsOnSecondLine, rInterceptExists)
        End Function
        Friend Function IntersectionPt(bLine As TLINE, ByRef rLinesAreParallel As Boolean, ByRef rLinesAreCoincident As Boolean, ByRef rIsOnFirstLine As Boolean, ByRef rIsOnSecondLine As Boolean, ByRef rInterceptExists As Boolean) As TVECTOR
            Dim _rVal As TVECTOR = TVECTOR.Zero
            '#1the second of two lines
            '#2returns true if the two lines are parallel
            '#3returns true if the two lines are cooincident
            '#4returns true if the intersection point lies on this line
            '#5returns true if the intersection point lies on the passed line
            '^returns the vector where the two passed line segments intersect (if it exists)
            rInterceptExists = False
            rLinesAreParallel = False
            rLinesAreCoincident = False
            rIsOnFirstLine = False
            rIsOnSecondLine = False
            If Math.Round(Length, 8) <= 0 Then Return _rVal
            If Math.Round(bLine.Length, 8) <= 0 Then Return _rVal
            Dim aDir As TVECTOR = Direction()
            Dim bDir As TVECTOR = bLine.Direction
            Dim f1 As Double
            Dim bInvsPar As Boolean
            Dim bParel As Boolean
            Dim v1 As TVECTOR
            'see if they are parallel
            bParel = aDir.Equals(bDir, True, 6, bInvsPar)
            rLinesAreParallel = bParel Or bInvsPar
            If rLinesAreParallel Then
                'get the distance between them
                v1 = bLine.SPT
                v1 = dxfProjections.ToLine(v1, Me, f1)
                rLinesAreCoincident = f1 <= 0.00001
            Else
                Dim parallel As Boolean = False
                Dim sL As TLINE = TLINE.ShortestConnector(Me, bLine, parallel)
                If Not parallel Then
                    f1 = sL.Length
                    rInterceptExists = Math.Round(f1, 4) = 0
                    _rVal = sL.SPT
                    If rInterceptExists Then
                        f1 = TValue(_rVal, True)
                        rIsOnFirstLine = f1 >= 0 And Math.Round(f1, 10) <= 1
                        f1 = bLine.TValue(_rVal, True)
                        rIsOnSecondLine = f1 >= 0 And Math.Round(f1, 10) <= 1
                    End If
                End If
            End If
            Return _rVal
        End Function
        Friend Function IntersectionPts(aLoops As TBOUNDLOOPS, Optional aLineIsInfinite As Boolean = True, Optional aCollector As colDXFVectors = Nothing) As TVECTORS
            Dim _rVal As New TVECTORS
            'On Error Resume Next
            If aLoops.Count <= 0 Then Return _rVal
            Dim aLns As New TSEGMENTS(0)
            aLns.Add(New TSEGMENT(Me, aLineIsInfinite))
            For i As Integer = 1 To aLoops.Count
                _rVal.Append(dxfIntersections.LAES_LAES(aLns, aLoops.Item(i).Segments, aLineIsInfinite, False, aCollector:=aCollector))
            Next i
            Return _rVal
        End Function
        Friend Function Intersects(bLine As TLINE) As Boolean
            Dim rLinesAreParallel As Boolean = False
            Dim rLinesAreCoincident As Boolean = False
            Dim rIsOnFirstLine As Boolean = False
            Dim rIsOnSecondLine As Boolean = False
            Return Intersects(bLine, rLinesAreParallel, rLinesAreCoincident, rIsOnFirstLine, rIsOnSecondLine)
        End Function
        Friend Function Intersects(bLine As TLINE, ByRef rLinesAreParallel As Boolean, ByRef rLinesAreCoincident As Boolean, ByRef rIsOnFirstLine As Boolean, ByRef rIsOnSecondLine As Boolean) As Boolean
            Dim _rVal As Boolean
            Dim ip As TVECTOR = IntersectionPt(bLine, rLinesAreParallel, rLinesAreCoincident, rIsOnFirstLine, rIsOnSecondLine, _rVal)
            Return _rVal
            '#1the second of two lines
            '#2returns true if the two lines are parallel
            '#3returns true if the two lines are cooincident
            '#4returns true if the intersection point lies on this line
            '#5returns true if the intersection point lies on the passed line
            '^returns True if the this line and the passed line intersect
        End Function
#End Region 'Methods
#Region "Shared Methods"

        Public Shared ReadOnly Property Null As TLINE
            Get
                Return New TLINE("")
            End Get
        End Property

        Public Function IsNull(aLine As TLINE, Optional aPrecis As Integer = 8)
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            Return Math.Round(aLine.Length, aPrecis) = 0
        End Function
        Public Shared Function PolarLine(anXDirection As TVECTOR, aNormal As TVECTOR, aLength As Double, aAngle As Double, bInRadians As Boolean, aElevation As Double, aMidPt As TVECTOR, Optional bFromEndPt As Boolean = False, Optional bSuppressNormalize As Boolean = False) As TLINE
            Dim _rVal As New TLINE
            '#1the X Direction of the system
            '#2the normal or z direction of the system
            '#3the length of the line
            '#4the angle for the line
            '#5flag indicating that the angle is degrees or radians
            '#6the Z coordinate for the new line
            '#7the mid point of the line
            '#8flag to begin then line at the midpoint and project the end the desired angle
            '^used to create a new polar line with respect to the passed origin
            '~the returned lines mid point is at the passed origin and is oriented at the passed angle with respec to the passed x direction
            Dim xDir As TVECTOR
            Dim zDir As TVECTOR
            Dim aFlag As Boolean
            _rVal.SPT = aMidPt
            _rVal.EPT = aMidPt
            If aLength <> 0 Then
                If Not bSuppressNormalize Then xDir = anXDirection.Normalized(aFlag) Else xDir = anXDirection
                If Not aFlag Then
                    'the x is not null
                    If Not bSuppressNormalize Then zDir = aNormal.Normalized(aFlag) Else zDir = aNormal
                    If Not aFlag Then
                        'the normal is not null
                        If aAngle <> 0 Then xDir.RotateAbout(New TVECTOR, zDir, aAngle, bInRadians)
                        If Not bFromEndPt Then
                            _rVal.SPT = aMidPt + (xDir * (-0.5 * aLength))
                            _rVal.EPT = aMidPt + (xDir * (0.5 * aLength))
                        Else
                            _rVal.SPT = aMidPt
                            _rVal.EPT = aMidPt + (xDir * aLength)
                        End If
                    End If
                End If
            End If
            If aElevation <> 0 And Not aFlag Then
                _rVal.SPT += (zDir * aElevation)
                _rVal.EPT += (zDir * aElevation)
            End If
            Return _rVal
        End Function
        Public Shared Function ShortestConnector(aLine As TLINE, bLine As TLINE) As TLINE
            Dim rLinesAreParallel As Boolean = False
            Dim rAIsNull As Boolean = False
            Dim rBIsNull As Boolean = False
            Return ShortestConnector(aLine, bLine, rLinesAreParallel, rAIsNull, rBIsNull)
        End Function
        Public Shared Function ShortestConnector(aLine As TLINE, bLine As TLINE, ByRef rLinesAreParallel As Boolean) As TLINE
            Dim rAIsNull As Boolean = False
            Dim rBIsNull As Boolean = False
            Return ShortestConnector(aLine, bLine, rLinesAreParallel, rAIsNull, rBIsNull)
        End Function
        Public Shared Function ShortestConnector(aLine As TLINE, bLine As TLINE, ByRef rLinesAreParallel As Boolean, ByRef rAIsNull As Boolean, ByRef rBIsNull As Boolean) As TLINE
            rLinesAreParallel = True
            Dim _rVal As TLINE = aLine.Clone
            Dim P1 As New TVECTOR(0)
            Dim P21 As New TVECTOR(0)
            Dim p3 As New TVECTOR(0)
            Dim P43 As New TVECTOR(0)
            rAIsNull = Not aLine.Paramatize(P1, P21)
            rBIsNull = Not bLine.Paramatize(p3, P43)
            rLinesAreParallel = rAIsNull Or rBIsNull
            If rLinesAreParallel Then Return _rVal
            Dim P2 As TVECTOR = aLine.EPT
            Dim p4 As TVECTOR = bLine.EPT
            Dim P13 As TVECTOR = P1 - p3
            Dim d1343 As Double = P13.MultiSum(P43)
            Dim d4321 As Double = P43.MultiSum(P21)
            Dim d1321 As Double = P13.MultiSum(P21)
            Dim d4343 As Double = P43.MultiSum(P43)
            Dim d2121 As Double = P21.MultiSum(P21)
            Dim denom As Double = d2121 * d4343 - d4321 * d4321
            rLinesAreParallel = denom <= 0.0000001
            If rLinesAreParallel Then Return _rVal
            Dim numer As Double = d1343 * d4321 - d1321 * d4343
            Dim mua As Double = numer / denom
            Dim mub As Double = (d1343 + d4321 * mua) / d4343
            _rVal.SPT.X = P1.X + mua * P21.X
            _rVal.SPT.Y = P1.Y + mua * P21.Y
            _rVal.SPT.Z = P1.Z + mua * P21.Z
            _rVal.EPT.X = p3.X + mub * P43.X
            _rVal.EPT.Y = p3.Y + mub * P43.Y
            _rVal.EPT.Z = p3.Z + mub * P43.Z
            Return _rVal
        End Function

        Public Shared Function LineContainsVector(aLine As TLINE, aVector As TVECTOR, Optional aFudgeFactor As Double = 0.001,
                                                  Optional bTreatAsInfinite As Boolean = False, Optional bSuppressPlaneCheck As Boolean = True
                                                  ) As Boolean
            Dim rIsStartPt As Boolean = False
            Dim rIsEndPt As Boolean = False
            Dim rWithin As Boolean = False
            Return LineContainsVector(aLine, aVector, aFudgeFactor, rIsStartPt, rIsEndPt, bTreatAsInfinite, bSuppressPlaneCheck, rWithin)
        End Function
        Public Shared Function LineContainsVector(aLine As TLINE, aVector As TVECTOR, aFudgeFactor As Double,
                                                   ByRef rIsStartPt As Boolean, ByRef rIsEndPt As Boolean,
                                                   bTreatAsInfinite As Boolean, bSuppressPlaneCheck As Boolean,
                                                   ByRef rWithin As Boolean) As Boolean
            rIsStartPt = False
            rIsEndPt = False
            rWithin = False
            'create an arbitrary plane with the x direction eqaul to the lines direction
            Dim plane As TPLANE = aLine.Plane
            Dim lnlen As Double = aLine.Length
            Dim f1 As Double = TVALUES.LimitedValue(Math.Abs(aFudgeFactor), 0.000001, 0.1)
            'get the vectors coordiantes with respect to the plane
            Dim v1 As TVECTOR = aVector.WithRespectTo(plane, 12, bSuppressZ:=bSuppressPlaneCheck)
            'if Y is not zero then the vector is not on the line
            Dim _rVal As Boolean = Math.Abs(v1.Y) <= f1
            'if Z is not zero the vector is on a line parallel to the line but on a parrallel plane
            If Not bSuppressPlaneCheck And _rVal Then
                _rVal = Math.Abs(v1.Z) <= f1
            End If
            If _rVal Then
                'the vector lies between the lines start and end pt if  X is greater than or equal to zero and less than or equal to the lines length
                rWithin = v1.X >= -f1 And v1.X <= lnlen + f1
                'the vector is the lines start point if X is zero
                rIsStartPt = Math.Abs(v1.X) <= f1
                'the vector is the lines end point if X is eqaul to the length of the line
                rIsEndPt = v1.X > 0 And v1.X >= lnlen - f1 And v1.X <= lnlen + f1
                If Not bTreatAsInfinite Then
                    _rVal = rWithin
                End If
            End If
            Return _rVal
        End Function
        Public Shared Function ToPlane(aLine As TLINE) As TPLANE
            Return New TPLANE(aLine.SPT, aLine.Direction, New TVECTOR(0, 0, 0))
        End Function

        Friend Shared Function Compare(A As TLINE, B As TLINE, Optional aPrecis As Integer = 3, Optional bBothDirections As Boolean = True) As Boolean
            Dim _rVal As Boolean
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            If A.SPT = B.SPT Then
                _rVal = A.EPT.Equals(B.EPT, aPrecis)
            End If
            If bBothDirections And Not _rVal Then
                If A.SPT.Equals(B.EPT, aPrecis) Then
                    _rVal = A.EPT.Equals(B.SPT, aPrecis)
                End If
            End If
            Return _rVal
        End Function

        Public Shared Function ByProjection(aOriginOrMidPt As TVECTOR, aDirection As TVECTOR, aDistance As Double, Optional bUseMidPt As Boolean = False, Optional bInvertDirection As Boolean = False) As TLINE
            Dim v1 As New TVECTOR(aOriginOrMidPt)
            Dim dir As New TVECTOR(aDirection)
            dir.Normalize()
            If bInvertDirection Then dir *= -1
            Dim d1 As Double = aDistance
            Dim v2 As New TVECTOR(aOriginOrMidPt)
            If Not bUseMidPt Then
                v2 += dir * aDistance
            Else
                v2 += dir * aDistance / 2
                v1 += dir * -aDistance / 2

            End If

            Return New TLINE(v1, v2)

            '#1the start pt for the new line
            '#2the direction to project the line in
            '#3the distance to project (length)

            '^returns a line with its start point at the Origin and its end point projected the passed distance in the passed direction

        End Function
#End Region 'Shared Methods
#Region "Operators"
        Public Shared Operator +(A As TLINE, B As TVECTOR) As TLINE
            Dim _rVal As New TLINE(A)
            _rVal.SPT += B
            _rVal.EPT += B
            Return _rVal
        End Operator

        Public Shared Operator -(A As TLINE, B As TVECTOR) As TLINE
            Dim _rVal As New TLINE(A)
            _rVal.SPT -= B
            _rVal.EPT -= B
            Return _rVal
        End Operator

#End Region 'Operators
    End Structure 'TLINE
    Friend Structure TLINES
        Implements ICloneable

#Region "Members"
        Private _Members() As TLINE
        Private _Init As Boolean
#End Region 'Members
#Region "Constructors"
        Public Sub New(aCount As Integer)
            If aCount < 0 Then aCount = 0
            _Init = True
            ReDim _Members(0 To aCount - 1)
        End Sub
        Public Sub New(aLine As TLINE)
            'init -----------------------------
            _Init = True
            ReDim _Members(-1)
            'init -----------------------------
            Add(aLine)

        End Sub
        Public Sub New(aLine As TLINE, bLine As TLINE)
            'init -----------------------------
            _Init = True
            ReDim _Members(-1)
            'init -----------------------------
            Add(aLine)
            Add(bLine)
        End Sub

        Public Sub New(aLines As TLINES)
            'init -----------------------------
            _Init = True
            ReDim _Members(-1)
            'init -----------------------------
            If aLines._Init Then _Members = aLines._Members.Clone()
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
        Public Function Item(aIndex As Integer) As TLINE
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return TLINE.Null
            End If
            Return _Members(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, value As TLINE)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1) = value
        End Sub
        Public Function EndPt(aIndex As Integer) As TVECTOR
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return TVECTOR.Zero
            End If
            Return _Members(aIndex - 1).EPT
        End Function
        Public Sub SetEndPt(aIndex As Integer, value As TVECTOR)
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return
            End If
            _Members(aIndex - 1).EPT = value
        End Sub
        Public Function StartPt(aIndex As Integer) As TVECTOR
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return TVECTOR.Zero
            End If
            Return _Members(aIndex - 1).SPT
        End Function
        Public Sub SetStartPt(aIndex As Integer, value As TVECTOR)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1).SPT = value
        End Sub
        Public Function ArcLines(Optional aInfinite As Boolean = False) As TSEGMENTS
            Dim _rVal As New TSEGMENTS(0)
            For i As Integer = 1 To Count
                _rVal.Add(Item(i), bAddtoStart:=aInfinite)
            Next i
            Return _rVal
        End Function
        Public Overrides Function ToString() As String
            Return $"TLINES [{ Count }]"
        End Function
        Public Sub Clear()
            _Init = True
            ReDim _Members(-1)
        End Sub
        Public Sub Add(aLine As TLINE)

            System.Array.Resize(_Members, Count + 1)
            _Members(Count - 1) = aLine.Clone
        End Sub
        Public Sub Add(aSPT As TVECTOR, aEPT As TVECTOR)
            System.Array.Resize(_Members, Count + 1)
            _Members(_Members.Count - 1) = New TLINE(aSPT, aEPT)
        End Sub
        Friend Function IntersectionPts(bLines As TLINES, Optional aLines_AreInfinite As Boolean = False, Optional bLines_AreInfinite As Boolean = False, Optional bMustBeOnBoth As Boolean = True, Optional bFirstPointOnly As Boolean = False, Optional aCollector As colDXFVectors = Nothing) As TVECTORS
            Dim _rVal As New TVECTORS(0)
            'On Error Resume Next
            Dim i As Integer
            Dim j As Integer
            Dim iL As TLINE
            Dim jL As TLINE
            Dim v1 As TVECTOR
            Dim bOn1 As Boolean
            Dim bOn2 As Boolean
            Dim bKeep As Boolean
            Dim bparel As Boolean = False
            Dim coinc As Boolean = False
            Dim exst As Boolean = False
            For i = 1 To Count
                iL = Item(i)
                For j = 1 To bLines.Count
                    jL = bLines.Item(j)
                    v1 = iL.IntersectionPt(jL, bparel, coinc, bOn1, bOn2, exst)
                    If bOn1 Or bOn2 Then
                        bKeep = True
                        If bMustBeOnBoth Then
                            If Not aLines_AreInfinite Then
                                If Not bOn1 Then bKeep = False
                            End If
                            If Not bLines_AreInfinite Then
                                If Not bOn2 Then bKeep = False
                            End If
                        End If
                        If bKeep Then
                            _rVal.Add(v1)
                            If aCollector IsNot Nothing Then aCollector.AddV(v1)
                            If bFirstPointOnly Then Return _rVal
                        End If
                    End If
                Next j
            Next i
            Return _rVal
        End Function
        Friend Function IntersectionPts(aArc As TARC, Optional aLines_AreInfinite As Boolean = False, Optional bArc_IsInfinite As Boolean = False, Optional bMustBeOnBoth As Boolean = True, Optional bSuppressPlaneCheck As Boolean = True, Optional aCurveDivisions As Integer = 1000, Optional aCollector As colDXFVectors = Nothing) As TVECTORS
            Dim _rVal As New TVECTORS(0)
            Try
                Dim iL As TLINE
                Dim v1 As TVECTOR
                Dim bOn1 As Boolean
                Dim bOn2 As Boolean
                Dim bKeep As Boolean
                Dim ips As TVECTORS
                For i As Integer = 1 To Count
                    iL = Item(i)
                    ips = iL.IntersectionPts(aArc, bSuppressPlaneCheck, aLineIsInfinite:=True, aArcIsInfinite:=True, aCurveDivisions:=aCurveDivisions)
                    For j As Integer = 1 To ips.Count
                        v1 = ips.Item(j)
                        bOn1 = iL.ContainsVector(v1)
                        bOn2 = aArc.ContainsVector(v1)
                        bKeep = False
                        If bOn1 Or bOn2 Then
                            bKeep = True
                            If bMustBeOnBoth Then
                                If Not aLines_AreInfinite And Not bOn1 Then bKeep = False
                                If Not bArc_IsInfinite And Not bOn2 Then bKeep = False
                            End If
                            If bKeep Then
                                _rVal.Add(v1)
                                If aCollector IsNot Nothing Then aCollector.AddV(v1)
                            End If
                        End If
                    Next j
                Next i
                Return _rVal
            Catch
                Return _rVal
            End Try
        End Function
        Public Sub Translate(aTranslation As TVECTOR)
            If TVECTOR.IsNull(aTranslation) Then Return

            For i = 0 To Count - 1
                _Members(i).Translate(aTranslation)
            Next
        End Sub

        Public Function Clone() As TLINES
            Return New TLINES(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TLINES(Me)
        End Function

        Public Function ToList() As List(Of TLINE)
            If Count <= 0 Then Return New List(Of TLINE)
            Return _Members.ToList()
        End Function
#End Region 'Methods
#Region "Shared Methods"
#End Region 'Shared Methods
    End Structure 'TLINES

    Friend Structure TARCS
        Implements ICloneable
#Region "Members"
        Private _Members() As TARC
        Private _Init As Boolean
#End Region 'Members
#Region "Constructors"
        Public Sub New(aCount As Integer)
            'init ---------------------------------
            _Init = True
            ReDim _Members(0 To aCount - 1)
            'init ---------------------------------

        End Sub
        Public Sub New(aArc As TARC)
            'init ---------------------------------
            _Init = True
            ReDim _Members(0 To -1)
            'init ---------------------------------

            Add(aArc)

        End Sub
        Public Sub New(aArcs As TARCS)
            'init -----------------------------
            _Init = True
            ReDim _Members(-1)
            'init -----------------------------
            _Members = aArcs._Members.Clone()
        End Sub

#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property ArcLines As TSEGMENTS
            Get
                Dim _rVal As New TSEGMENTS(0)
                For i As Integer = 1 To Count
                    _rVal.Add(Item(i))
                Next i
                Return _rVal
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
#End Region 'Properties
#Region "Methods"
        Public Function Item(aIndex As Integer) As TARC
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return New TARC(0)
            End If
            Return _Members(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, value As TARC)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1) = value
        End Sub
        Public Overrides Function ToString() As String
            Return $"TARCS [{Count }]"
        End Function
        Public Function EndPt(aIndex As Integer) As TVECTOR
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return TVECTOR.Zero
            End If
            Return _Members(aIndex - 1).EndPt
        End Function
        Public Function StartPt(aIndex As Integer) As TVECTOR
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return TVECTOR.Zero
            End If
            Return _Members(aIndex - 1).StartPt
        End Function
        Public Sub Clear()
            _Init = True
            ReDim _Members(-1)
        End Sub
        Public Sub Add(aArc As TARC)
            System.Array.Resize(_Members, Count + 1)
            _Members(_Members.Count - 1) = aArc.Clone
        End Sub
        Public Sub Add(aPlane As TPLANE, aCenter As TVECTOR, aSP As TVECTOR, aEP As TVECTOR, Optional aClockwise As Boolean = False, Optional bSuppressProjection As Boolean = False)
            Dim aArc As TARC = TARC.DefineWithPoints(aPlane, aCenter, aSP, aEP, aClockwise, bSuppressProjection)
            System.Array.Resize(_Members, Count + 1)
            _Members(_Members.Count - 1) = aArc
        End Sub
        Friend Function IntersectionPts(bLines As TLINES, Optional aLines_AreInfinite As Boolean = False, Optional bMembers_AreInfinite As Boolean = False) As TVECTORS
            Dim _rVal As New TVECTORS(0)
            'On Error Resume Next
            Dim jL As TLINE
            Dim iA As TARC
            For i As Integer = 1 To Count
                iA = Item(i)
                For j As Integer = 1 To bLines.Count
                    jL = bLines.Item(j)
                    _rVal.Append(jL.IntersectionPts(iA, False, aLineIsInfinite:=aLines_AreInfinite, aArcIsInfinite:=bMembers_AreInfinite))
                Next j
            Next i
            Return _rVal
        End Function
        Friend Function IntersectionPts(aArc As TARC, Optional aArc_IsInfinite As Boolean = False, Optional bMembers_AreInfinite As Boolean = False, Optional bSuppressPlaneCheck As Boolean = True) As TVECTORS
            Dim _rVal As New TVECTORS(0)
            'On Error Resume Next
            Dim iAr As TARC
            For i As Integer = 1 To Count
                iAr = Item(i)
                _rVal.Append(iAr.IntersectionPts(aArc, bSuppressPlaneCheck))
            Next i
            Return _rVal
        End Function
        Public Sub Translate(aTranslation As TVECTOR)
            If TVECTOR.IsNull(aTranslation) Then Return

            For i = 1 To Count
                _Members(i - 1).Translate(aTranslation)
            Next
        End Sub
        Friend Function Clone()
            Return New TARCS(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TARCS(Me)
        End Function

        Public Function ToList() As List(Of TARC)
            If Count <= 0 Then Return New List(Of TARC)
            Return _Members.ToList()
        End Function
#End Region 'Methods
#Region "Shared Methods"
#End Region 'Shared Methods
    End Structure 'TARCS
End Namespace
