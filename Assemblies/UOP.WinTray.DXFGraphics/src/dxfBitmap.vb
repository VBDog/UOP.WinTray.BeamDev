Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxfBitmap
        Implements IDisposable
        Private _Disposed As Boolean
        'Public PixelMap As TPIXELMAP
        Public FileName As String
        Private _Orientation As dxxPaperOrientations
        Private _BackColor As Color
        Private _RaiseErrors As Boolean
        Private _ImageGUID As String
        Private _ImageScale As Double
        'Public ImageX As Double
        'Public ImageY As Double
        Private _Name As String
        Public Tag As String
        'Public X As Integer
        'Public Y As Integer
        Private _Bitmap As System.Drawing.Bitmap
        Private _DPI As Integer
        Private _TransformationMatrix As TMATRIX4
        Private _Transformation As TTRANSFORMATION
#Region "Constructors"
        Public Sub New(Optional aBackColor As Color? = Nothing)

            If aBackColor.HasValue Then _BackColor = aBackColor
            Try
                Bitmap = New System.Drawing.Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb)
            Catch ex As Exception
                Throw ex
            End Try
        End Sub
        Public Sub New(aWidth As Integer, aHeight As Integer, Optional aBackColor As Color? = Nothing, Optional aName As String = "")
            If aBackColor.HasValue Then _BackColor = aBackColor

            If aWidth <= 0 Then aWidth = Screen.PrimaryScreen.Bounds.Width
            If aHeight <= 0 Then aHeight = Screen.PrimaryScreen.Bounds.Height
            If aWidth <= 0 Then aWidth = Screen.PrimaryScreen.Bounds.Width
            If aHeight <= 0 Then aHeight = Screen.PrimaryScreen.Bounds.Height
            Bitmap = New System.Drawing.Bitmap(aWidth, aHeight, System.Drawing.Imaging.PixelFormat.Format32bppRgb)

            FloodFill(_BackColor)
            Name = aName
        End Sub
        Public Sub New(aBitmap As System.Drawing.Bitmap, Optional aBackgroundColor As Color? = Nothing)
            If aBitmap IsNot Nothing Then
                _BackColor = aBitmap.GetPixel(0, 0)
                Bitmap = TBITMAPDATA.ConvertToRGB32(aBitmap)
            Else
                _BackColor = Color.White
                Bitmap = New System.Drawing.Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb)
            End If
            If aBackgroundColor.HasValue Then _BackColor = aBackgroundColor.Value
        End Sub
        Public Sub New(aBitmap As dxfBitmap)
            If aBitmap IsNot Nothing Then
                If aBitmap.IsDefined Then _Bitmap = aBitmap.Bitmap.Clone Else _Bitmap = New System.Drawing.Bitmap(aBitmap.Width, aBitmap.Height)
                _BackColor = aBitmap.BackgroundColor
                _Orientation = aBitmap.Orientation
                _DPI = aBitmap.DPI
            Else
                _BackColor = System.Drawing.Color.White
            End If
        End Sub
#End Region 'Constructors
#Region "Properties"
        Friend Property Transformation As TTRANSFORMATION
            Get
                Return _Transformation
            End Get
            Set(value As TTRANSFORMATION)
                _Transformation = value
            End Set
        End Property
        Friend Property ImageScale As Double
            Get
                Return _ImageScale
            End Get
            Set(value As Double)
                _ImageScale = value
            End Set
        End Property
        Public Property Name As String
            Get
                Return _Name
            End Get
            Set(value As String)
                _Name = value
            End Set
        End Property
        Public ReadOnly Property AspectRatio As Double
            Get
                If Height > 0 Then
                    Return Width / Height
                Else
                    Return 1
                End If
            End Get
        End Property
        Public ReadOnly Property IsDefined As Boolean
            Get
                Return _Bitmap IsNot Nothing And Not _Disposed
            End Get
        End Property
        Public Property RaiseErrors As Boolean
            Get
                Return _RaiseErrors
            End Get
            Set(value As Boolean)
                _RaiseErrors = value
            End Set
        End Property
        Friend Property TransformationMatrix As TMATRIX4
            Get
                Return _TransformationMatrix
            End Get
            Set(value As TMATRIX4)
                _TransformationMatrix = value
            End Set
        End Property
        Public Property Orientation As dxxPaperOrientations
            Get
                Return _Orientation
            End Get
            Set(value As dxxPaperOrientations)
                _Orientation = value
            End Set
        End Property
        Public ReadOnly Property BackColor As Integer
            Get
                Return dxfColors.Win64ToWin32(_BackColor)
            End Get
        End Property
        Public Property BackgroundColor As Color
            Get
                Return _BackColor
            End Get
            Set(value As Color)
                _BackColor = value
            End Set
        End Property
        Public Property IMageGUID As String
            Get
                Return _ImageGUID
            End Get
            Friend Set(value As String)
                _ImageGUID = value
            End Set
        End Property
        Public ReadOnly Property DPI As Integer
            Get
                If IsDefined Then
                    Return _DPI
                Else
                    Return 96
                End If
            End Get
        End Property
        Public ReadOnly Property Disposed As Boolean
            Get
                Return _Disposed
            End Get
        End Property
        Public Property Width As Integer
            Get
                If _Bitmap IsNot Nothing Then
                    Return _Bitmap.Width
                Else
                    Return 0
                End If
            End Get
            Friend Set(value As Integer)
                Resize(value, Height)
            End Set
        End Property
        Public ReadOnly Property Diagonal As Double
            Get
                Return Math.Round(Math.Sqrt(Width ^ 2 + Height ^ 2), 4)
            End Get
        End Property
        Public Property Height As Integer
            Get
                If _Bitmap IsNot Nothing Then
                    Return _Bitmap.Height
                Else
                    Return 0
                End If
            End Get
            Friend Set(value As Integer)
                Resize(Width, value)
            End Set
        End Property
        Public ReadOnly Property Size As System.Drawing.Size
            Get
                If Not Disposed Then Return _Bitmap.Size Else Return New Size(0, 0)
            End Get
        End Property
        Public ReadOnly Property Image As Image
            Get
                If Not IsDefined Then Return Nothing
                Return _Bitmap.Clone
            End Get
        End Property
        Public Property Bitmap As System.Drawing.Bitmap
            Get
                Return _Bitmap
            End Get
            Set(value As System.Drawing.Bitmap)
                _Bitmap = value
                If Not value Is Nothing Then
                    Dim g As Graphics = Graphics.FromImage(value)
                    _DPI = g.DpiX
                Else
                    _DPI = 96
                End If
            End Set
        End Property
        Public ReadOnly Property hBMP As IntPtr
            Get
                If IsDefined Then
                    Return _Bitmap.GetHbitmap
                Else
                    Return 0
                End If
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function DrawToBitmap(aBitmap As dxfBitmap, Optional aAlignment As dxxRectangularAlignments = dxxRectangularAlignments.TopLeft) As Boolean
            Dim rError As String = string.Empty
            Return DrawToBitmap(aBitmap, aAlignment, rError)
        End Function
        Public Function DrawToBitmap(aBitmap As dxfBitmap, aAlignment As dxxRectangularAlignments, ByRef rError As String) As Boolean
            rError = string.Empty
            If Not IsDefined Then Return False
            If aBitmap Is Nothing Then Return False
            If Not aBitmap.IsDefined Then Return False
            '#1the destination bitmap.
            '#2an alignment to apply
            '#3returns an error string
            '^draw the current or passed bitmap to the to the destination
            Try
                dxfBitmap.DrawToBitmap(Bitmap, aBitmap, aAlignment, rError)
                If rError <> "" Then Throw New Exception(rError)
                Return True
            Catch ex As Exception
                rError = ex.Message
                Return False
            End Try
        End Function
        Friend Function Plane(Optional aUnits As dxxDeviceUnits = dxxDeviceUnits.Pixels, Optional aName As String = "", Optional bSetOriginToCenter As Boolean = False) As TPLANE
            If String.IsNullOrEmpty(aName) Then aName = "" Else aName = aName.Trim
            If aName = "" Then aName = Name
            Dim _rVal As New TPLANE(aName, aUnits) With {.Width = Width, .Height = Height}
            '^returns a plane that describes the bitmap
            Dim aSF As Double
            Dim aWd As Integer = _rVal.Width
            Dim aHt As Integer = _rVal.Height
            If aUnits <> dxxDeviceUnits.Pixels And aUnits > 0 Then
                aSF = dxfUtils.FactorFromTo(dxxDeviceUnits.Pixels, aUnits, DPI)
            Else
                aUnits = dxxDeviceUnits.Pixels
                aSF = 1
            End If
            _rVal.Define(New TVECTOR(0, 0), TVECTOR.WorldX, New TVECTOR(0, -1, 0), aHeight:=aHt * aSF, aWidth:=aWd * aSF)
            _rVal.Units = aUnits
            _rVal.Name = aName
            If bSetOriginToCenter Then _rVal.Origin = New TVECTOR(0.5 * _rVal.Width, 0.5 * _rVal.Height)
            Return _rVal
        End Function
        Public Function DrawToDevice(aHwd As IntPtr, Optional aAlignment As dxxRectangularAlignments = dxxRectangularAlignments.TopLeft) As Boolean
            '#2the destination hWnd.
            '#4the alignment to apply
            '^draw the current or passed bitmap to the to the destination
            If Not IsDefined Then Return False
            Dim _rVal As Boolean
            Dim sErr As String = String.Empty
            'Dim Contrl As Object = Nothing
            'Dim bBackImg As Boolean
            Try
                _rVal = dxfBitmap.DrawToDevice(_Bitmap, aHwd, aAlignment, sErr)
                If sErr <> "" Then Throw New Exception(sErr) Else Return _rVal
            Catch ex As Exception
                If RaiseErrors Then Throw ex
                _rVal = False
            End Try
            Return _rVal
        End Function
        Public Function CopyToClipBoard() As Boolean
            '^sends a copy of the current bitmap to the system clipboard
            If Not Disposed Then
                Try
                    System.Windows.Forms.Clipboard.SetImage(_Bitmap)
                    Return True
                Catch ex As Exception
                    Return False
                    If _RaiseErrors Then Throw ex
                End Try
            Else
                Return False
            End If
        End Function
        Public Function CopyFromControl(aHandle As IntPtr) As Boolean
            Dim rError As String = string.Empty
            Return CopyFromControl(aHandle, rError)
        End Function
        Public Function CopyFromControl(aHandle As IntPtr, ByRef rError As String) As Boolean
            rError = string.Empty
            '#1the handle to the device (control) to copy the bitmap from
            '^copies the bitmap at its current dimensions form the target device
            '~similiar to create but allows the user to indicate a specific bitmap on the device
            Try
                Dim devBMAP As Bitmap = dxfBitmap.CopyControlBitmap(aHandle, rError)
                If rError = string.Empty Then
                    Bitmap = devBMAP
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                If RaiseErrors Then Throw ex
                Return False
            End Try
        End Function
        Public Function ConvertToColorMode(aColorMode As dxxColorModes, Optional aBlack As Color? = Nothing, Optional aWhite As Color? = Nothing) As Boolean
            If Not IsDefined Then Return False
            If aColorMode <> dxxColorModes.GreyScales And aColorMode <> dxxColorModes.BlackWhite Then Return False
            If aColorMode = dxxColorModes.BlackWhite Then
                If aBlack Is Nothing Then aBlack = Color.Black
                If aWhite Is Nothing Then aWhite = Color.White
                If Not aBlack.HasValue Then aBlack = Color.Black
                If Not aWhite.HasValue Then aWhite = Color.White

                If aBlack = aWhite Then Return False
            End If
            '#1color mode to convert to (greyscales or black and white)
            '^converts all the pixels int the bitmap to a shade of grey or black and wigth bassed on it's current hue
            Try
                Dim BMAP As Bitmap = Bitmap.Clone
                If dxfBitmap.ConvertToColorMode(BMAP, aColorMode, aBlack, aWhite) Then
                    Bitmap = BMAP
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                If RaiseErrors Then Throw ex
                Return False
            End Try
        End Function
        Public Function GetBytes(ByRef ioBytes() As Byte) As Boolean
            System.Array.Resize(ioBytes, 1)
            If Not IsDefined Then Return False
            Dim BMAP As Bitmap = Bitmap
            If BMAP Is Nothing Then Return False
            Try
                Dim bm_bytes As New TBITMAPDATA(BMAP)
                bm_bytes.LockBitmap()
                ioBytes = bm_bytes.ImageBytes.Clone
                bm_bytes.UnlockBitmap()
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function
        Public Function StretchToControl(aHwnd As IntPtr, aScaleFactor As Double, Optional aYScale As Double = 0.0,
                                      Optional aAlignment As dxxRectangularAlignments = dxxRectangularAlignments.TopLeft,
                                      Optional aXOffset As Integer = 0, Optional aYOffset As Integer = 0,
                                      Optional aColorMode As dxxColorModes = dxxColorModes.Full) As Boolean
            Dim rErrorString As String = String.Empty
            Return StretchToControl(aHwnd, aScaleFactor, aYScale, aAlignment, aXOffset, aYOffset, aColorMode, rErrorString)
        End Function
        Public Function StretchToControl(aHwnd As IntPtr, aScaleFactor As Double, aYScale As Double,
                                       aAlignment As dxxRectangularAlignments,
                                       aXOffset As Integer, aYOffset As Integer,
                                      aColorMode As dxxColorModes, ByRef rErrorString As String) As Boolean
            rErrorString = ""
            If Not IsDefined Then Return False
            If aHwnd.Equals(0) Then Return False
            '#1the subject bitmap
            '#2the device context to draw the bitmap to
            '#3the scale factor to apply
            '#4a Y scale to apply (<=0 means uniform scaling)
            '#5the aligment to apply
            '#6a X offset to apply
            '#7a Y offset to apply
            '#8the color mode to apply
            '#9returns any errors
            '^stretches the bitmap to the indicated device context using the indicated scales
            Try
                dxfBitmap.StretchToControl(Bitmap, aHwnd, aScaleFactor, aYScale, aAlignment, aXOffset, aYOffset, aColorMode, rErrorString)
                Return True
            Catch ex As Exception
                If RaiseErrors Then Throw ex
                Return False
            End Try
        End Function
        Public Function StretchToControl(aHwnd As IntPtr, Optional aColorMode As dxxColorModes = dxxColorModes.Full) As Boolean
            Dim rErrorString As String = String.Empty
            Return StretchToControl(aHwnd, aColorMode, rErrorString)
        End Function
        Public Function StretchToControl(aHwnd As IntPtr, aColorMode As dxxColorModes, ByRef rErrorString As String) As Boolean
            rErrorString = ""
            If Not IsDefined Then Return False
            If aHwnd.Equals(0) Then Return False
            '#1the subject bitmap
            '#2the device context to draw the bitmap to
            '#3the color mode to apply
            '#4returns any errors
            '^stretches the bitmap to the indicated device context to fill the device client area
            Try
                dxfBitmap.StretchToControl(Bitmap, aHwnd, aColorMode, rErrorString)
                Return True
            Catch ex As Exception
                If RaiseErrors Then Throw ex
                Return False
            End Try
        End Function
        Public Function TransferPixels(aToBitmap As dxfBitmap, Optional aAlignment As dxxRectangularAlignments = dxxRectangularAlignments.TopLeft, Optional aColorToKeep As Color? = Nothing, Optional aColorToSkip As Color? = Nothing, Optional bSwapXY As Boolean = False) As Boolean
            If aToBitmap Is Nothing Then Return False
            If Not IsDefined Then Return False
            If Not aToBitmap.IsDefined Then Return False
            Dim _rVal As Boolean
            '#1the destination bitmap
            '#2the alignment for the source bitmap with respect to the destination
            '#3a color to preserve on the destination
            '#4a color NOT to transfer from the source
            '^copies the pixels from the source bitmap to the destination bitmap
            Dim xAlign As dxxHorizontalJustifications
            Dim yAlign As dxxVerticalJustifications
            Dim inX As Integer
            Dim inY As Integer
            Dim aR As Integer
            Dim ac As Integer
            Dim br As Integer
            Dim bC As Integer
            Dim aWd As Integer
            Dim aHt As Integer
            Dim bWd As Integer
            Dim bHt As Integer
            Dim bClr As Color
            Dim aClr As Color
            Dim bDoIt As Boolean
            Dim aBMP As Bitmap = Bitmap
            Dim bBMP As Bitmap = aToBitmap.Bitmap
            aWd = aBMP.Width
            aHt = aBMP.Height
            bWd = aToBitmap.Width
            bHt = aToBitmap.Height
            Try
                If aWd <= 0 Or aHt <= 0 Then Return False
                If bWd <= 0 Or bHt <= 0 Then Return False
                If aAlignment < 1 Or aAlignment > 9 Then aAlignment = dxxRectangularAlignments.TopLeft
                dxfUtils.ParseRectangleAlignment(aAlignment, xAlign, yAlign)
                inX = 0
                inY = 0
                Select Case xAlign
                    Case dxxHorizontalJustifications.Center
                        inX = Math.Round(0.5 * aWd - 0.5 * bWd, 0)
                    Case dxxHorizontalJustifications.Right
                        inX = aWd - bWd
                End Select
                Select Case yAlign
                    Case dxxVerticalJustifications.Center
                        inY = 0.5 * aHt - 0.5 * bHt
                    Case dxxVerticalJustifications.Bottom
                        inY = aHt - bHt
                End Select
                _rVal = True
                br = -1
                For aR = inY To inY + bHt
                    If aR > aHt - 1 Then Exit For
                    br += 1
                    If br > bHt - 1 Then Exit For
                    bC = -1
                    For ac = inX To inX + bWd
                        bC += 1
                        If ac > aWd - 1 Then Exit For
                        If bC > bWd - 1 Then Exit For
                        aClr = aBMP.GetPixel(ac, aR)
                        bClr = bBMP.GetPixel(bC, br)
                        bDoIt = True
                        If bDoIt And aColorToKeep.HasValue Then
                            bDoIt = aClr <> aColorToKeep.Value
                        End If
                        If bDoIt And aColorToSkip.HasValue Then
                            bDoIt = bClr <> aColorToSkip.Value
                        End If
                        If bDoIt Then
                            If Not bSwapXY Then
                                bBMP.SetPixel(ac, aR, bClr)
                            Else
                                bBMP.SetPixel(aR, ac, bClr)
                            End If
                        End If
                    Next ac
                Next aR
            Catch ex As Exception
                Throw New Exception("TransferPixels - " & ex.Message)
            End Try
            Return _rVal
        End Function
        Public Function Clone(Optional aColorMode As dxxColorModes = dxxColorModes.Full) As dxfBitmap
            Dim _rVal As dxfBitmap
            Dim BMAP As Bitmap = Nothing
            If IsDefined Then BMAP = _Bitmap.Clone
            _rVal = New dxfBitmap(BMAP, _BackColor) With {.Orientation = _Orientation, .Name = _Name, .ImageScale = _ImageScale, .FileName = FileName, .RaiseErrors = _RaiseErrors}
            If aColorMode = dxxColorModes.BlackWhite Or aColorMode = dxxColorModes.GreyScales Then _rVal.ConvertToColorMode(aColorMode)
            Return _rVal
        End Function
        Friend Function Render(aPaths As TPATHS, aPathIndex As Integer, aPixelSize As Integer, ByRef ioPens As TPENS, Optional aImage As dxfImage = Nothing) As System.Drawing.Drawing2D.GraphicsPath
            If aPaths.Count <= 0 Then Return Nothing
            If aPathIndex <= 0 Or aPathIndex > aPaths.Count Then Return Nothing
            Dim aPath As TPATH = aPaths.Item(aPathIndex)
            Dim p As TPEN
            Dim gdiPen As Pen = Nothing
            Dim gdiBrush As Brush = Nothing
            Dim aTransformMatrix As TMATRIX4 = TransformationMatrix
            Dim aTransform As TTRANSFORMATION = Transformation
            If aPixelSize <= 0 Then aPixelSize = aPaths.PixelSize
            If aPixelSize <= 1 Then aPixelSize = 1
            Dim g As System.Drawing.Graphics = Graphics.FromImage(Bitmap)
            If aImage Is Nothing Then aImage = dxfEvents.GetImage(IMageGUID)
            If ioPens.Count <= 0 Then ioPens = New TPENS(aImage)
            'convert the TPATH to a GraphicsPath
            Dim _rVal As System.Drawing.Drawing2D.GraphicsPath = aPath.ToGraphicPath(aPixelSize, Size)
            If Not aPath.Filled Then
                Try
                    p = ioPens.GetPathPen(aImage, aPaths, aPathIndex)
                    gdiPen = p.GPen
                    g.DrawPath(gdiPen, _rVal)
                Catch ex As Exception
                    _rVal = Nothing
                    MessageBox.Show($"{Reflection.MethodBase.GetCurrentMethod.Name }  ERROR - {ex.Message}", "Error", MessageBoxButtons.OK, icon:=MessageBoxIcon.Warning)
                Finally
                    If gdiPen IsNot Nothing Then gdiPen.Dispose()
                End Try
            Else
                Try
                    p = ioPens.GetPathPen(aImage, aPaths, Math.PI)
                    gdiBrush = p.GBrush
                    g.FillPath(gdiBrush, _rVal)
                Catch ex As Exception
                    _rVal = Nothing
                    MessageBox.Show($"{Reflection.MethodBase.GetCurrentMethod.Name }  ERROR - {ex.Message}", "Error", MessageBoxButtons.OK, icon:=MessageBoxIcon.Warning)
                Finally
                    If gdiBrush IsNot Nothing Then gdiBrush.Dispose()
                End Try
            End If
            g.Dispose()
            Return _rVal
        End Function
        Friend Function Render(aPath As TVECTORS, bFilled As Boolean, aPen As TPEN?, Optional aPixelSize As Integer = 1) As System.Drawing.Drawing2D.GraphicsPath
            If aPath.Count <= 0 Then Return Nothing
            Dim p As TPEN
            Dim gdiPen As Pen = Nothing
            Dim gdiBrush As Brush = Nothing
            Dim aTransformMatrix As TMATRIX4 = TransformationMatrix
            Dim aTransform As TTRANSFORMATION = Transformation
            If aPixelSize <= 1 Then aPixelSize = 1
            Dim g As System.Drawing.Graphics = Graphics.FromImage(Bitmap)
            'convert the TPATH to a GraphicsPath
            Dim _rVal As System.Drawing.Drawing2D.GraphicsPath = aPath.ToGraphicPath(aPixelSize, Size)
            If Not bFilled Then
                Try
                    If aPen.HasValue Then
                        p = aPen.Value
                    Else
                        p = New TPEN(Color.Black, 1)
                    End If
                    gdiPen = p.GPen
                    g.DrawPath(gdiPen, _rVal)
                Catch ex As Exception
                    _rVal = Nothing
                    MessageBox.Show($"{Reflection.MethodBase.GetCurrentMethod.Name }  ERROR - {ex.Message}", "Error", MessageBoxButtons.OK, icon:=MessageBoxIcon.Warning)
                Finally
                    If gdiPen IsNot Nothing Then gdiPen.Dispose()
                End Try
            Else
                Try
                    If aPen.HasValue Then
                        p = aPen.Value
                    Else
                        p = New TPEN(Color.Black, 1)
                    End If
                    gdiBrush = p.GBrush
                    g.FillPath(gdiBrush, _rVal)
                Catch ex As Exception
                    _rVal = Nothing
                    MessageBox.Show($"{Reflection.MethodBase.GetCurrentMethod.Name }  ERROR - {ex.Message}", "Error", MessageBoxButtons.OK, icon:=MessageBoxIcon.Warning)
                Finally
                    If gdiBrush IsNot Nothing Then gdiBrush.Dispose()
                End Try
            End If
            g.Dispose()
            Return _rVal
        End Function
        Friend Function Render(aPath As System.Drawing.Drawing2D.GraphicsPath, bFilled As Boolean, aPen As TPEN) As System.Drawing.Drawing2D.GraphicsPath
            If aPath Is Nothing Then Return Nothing

            Dim gdiPen As Pen = aPen.GPen

            Dim g As System.Drawing.Graphics = Graphics.FromImage(Bitmap)
            g.SmoothingMode = Drawing2D.SmoothingMode.HighSpeed
            g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
            Dim _rVal As System.Drawing.Drawing2D.GraphicsPath = aPath
            If Not bFilled Then
                Try

                    g.DrawPath(gdiPen, _rVal)
                Catch ex As Exception
                    _rVal = Nothing
                Finally

                    gdiPen?.Dispose()
                End Try
            Else
                Dim gdiBrush As Brush = aPen.GBrush
                Try


                    g.FillPath(gdiBrush, _rVal)
                Catch ex As Exception
                    _rVal = Nothing
                    MessageBox.Show($"{Reflection.MethodBase.GetCurrentMethod.Name }  ERROR - {ex.Message}", "Error", MessageBoxButtons.OK, icon:=MessageBoxIcon.Warning)
                Finally
                    gdiBrush?.Dispose()
                End Try
            End If
            g?.Dispose()
            Return _rVal
        End Function
        Public Function Flip(bVertically As Boolean) As Boolean
            Dim rErrorString As String = String.Empty
            Return Flip(bVertically, rErrorString)
        End Function
        Public Function Flip(bVertically As Boolean, ByRef rErrorString As String) As Boolean
            '#1the subject bitmap
            '#2flag to flip the bitmap around its X axis
            '~inverts the bitmap around it's X or Y axis. Y axis is default
            rErrorString = ""
            If Not IsDefined Then Return False
            Dim _rVal As Boolean = False
            Try
                If dxfBitmap.Flip(Bitmap, False, rErrorString:=rErrorString) Then
                    Return True
                Else
                    If rErrorString <> "" Then
                        Throw New Exception(rErrorString)
                    Else
                        Return False
                    End If
                End If
            Catch ex As Exception
                rErrorString = ex.Message
                If RaiseErrors Then Throw ex
                Return False
            End Try
        End Function
        Public Function AddBorder(aBorderColor As Color, Optional aBorderWidth As Integer = 1, Optional aSuppressTop As Boolean = False, Optional aSuppressBot As Boolean = False, Optional aSuppressLeft As Boolean = False, Optional aSuppressRight As Boolean = False) As Boolean
            If Not IsDefined Then Return False
            Dim _rVal As Boolean = False
            '#1the subject bitmap
            '#2the color to use for the border
            '#3the width for the border
            '#4flag to suppress the top border
            '#5flag to suppress the bottom border
            '#6flag to suppress the left border
            '#7flag to suppress the right border
            '^creates a border of the passed width and color around the bitmap
            Try
                If aSuppressTop And aSuppressBot And aSuppressRight And aSuppressLeft Then Return _rVal
                Dim Rect As System.Drawing.Rectangle
                Dim aWd As Integer = _Bitmap.Width
                Dim aHt As Integer = _Bitmap.Height
                If aHt <= 0 Or aWd <= 0 Then Return False
                If aBorderWidth < 0 Then aBorderWidth = 1
                If aBorderWidth > aWd / 2 Then aBorderWidth = aWd / 2
                If aBorderWidth > aHt / 2 Then aBorderWidth = aHt / 2
                _rVal = True
                Dim g As Graphics = Graphics.FromImage(_Bitmap)
                Dim b As Brush = New SolidBrush(aBorderColor)
                If Not aSuppressTop Then
                    Rect.X = 0
                    Rect.Y = 0
                    Rect.Height = aBorderWidth
                    Rect.Width = aWd
                    g.FillRectangle(b, Rect)
                End If
                If Not aSuppressBot Then
                    Rect.X = 0
                    Rect.Y = aHt - aBorderWidth
                    Rect.Height = aBorderWidth
                    Rect.Width = aWd
                    g.FillRectangle(b, Rect)
                End If
                If Not aSuppressRight Then
                    Rect.X = aWd - aBorderWidth
                    Rect.Y = 0
                    Rect.Height = aHt
                    Rect.Width = aBorderWidth
                    g.FillRectangle(b, Rect)
                End If
                If Not aSuppressLeft Then
                    Rect.X = 0
                    Rect.Y = 0
                    Rect.Height = aHt
                    Rect.Width = aBorderWidth
                    g.FillRectangle(b, Rect)
                End If
                b.Dispose()
                g.Dispose()
            Catch ex As Exception
                If RaiseErrors Then Throw ex
            End Try
            Return _rVal
        End Function
        Public Function SetBackColor(aColor As Color, Optional bFloodFill As Boolean = False) As Boolean
            If Not IsDefined Then Return False
            '#1the new background color
            '#2flag to fill all pixels with the new color otherwise only  pixels which are of the current background color are replaced with the new color
            If Not bFloodFill Then
                Return ReplaceColor(BackgroundColor, aColor)
            Else
                Return FloodFill(aColor)
            End If
            BackgroundColor = aColor
        End Function
        Public Function FloodFill(aColor As Color) As Boolean
            If Not IsDefined Then Return False
            Try
                dxfBitmap.FloodFill(Bitmap, aColor)
                Return True
            Catch ex As Exception
                If RaiseErrors Then Throw ex
                Return False
            End Try
        End Function
        Public Function Scale(aXScaleFactor As Single, Optional aYScaleFactor As Object = Nothing) As Boolean
            If Not IsDefined Then Return False
            Dim _rVal As Boolean = False
            Dim xScl As Single = Math.Abs(aXScaleFactor)
            Dim yScl As Single = TVALUES.To_SNG(aYScaleFactor, True, aDefault:=xScl)
            If xScl = 0 Then Return False
            If yScl = 0 Then yScl = xScl
            Try
                Dim BMAP As Bitmap = Bitmap.Clone
                Dim aSZ As Size = BMAP.Size
                Dim aRec As New Rectangle(New Point(0, 0), aSZ)
                Dim bSz As New Size(aSZ.Width * xScl, aSZ.Height * yScl)
                Dim bRec As New Rectangle(New Point(0, 0), bSz)
                BMAP = New Bitmap(BMAP, bSz.Width, bSz.Height)
                'Public Sub DrawImage(image As Image, destRect As Rectangle, srcRect As Rectangle, srcUnit As GraphicsUnit)
                'Dim g1 As Graphics = Graphics.FromImage(BMAP)
                'g1.DrawImage(Bitmap, bRec, aRec, GraphicsUnit.Pixel)
                'g1.Dispose()
                Bitmap = BMAP
                _rVal = True
            Catch ex As Exception
                If RaiseErrors Then Throw ex
            End Try
            Return _rVal
        End Function
        Public Function ReplaceColor(aSearchColor As Color, aReplaceColor As Color) As Boolean
            If Not IsDefined Then Return False
            Dim _rVal As Boolean = False
            Dim sErr As String = String.Empty
            Try
                If dxfBitmap.ReplaceColor(Bitmap, aSearchColor, aReplaceColor, sErr) Then
                    Return True
                Else
                    If sErr <> "" Then
                        Throw New Exception(sErr)
                    Else
                        Return False
                    End If
                End If
            Catch ex As Exception
                If RaiseErrors Then Throw ex
                Return False
            End Try
        End Function
        Public Sub Resize(aWidth As Integer, aHeight As Integer)
            Try
                If aWidth <= 0 Then aWidth = Width
                If aWidth <= 0 Then aWidth = Screen.PrimaryScreen.Bounds.Width
                If aHeight <= 0 Then aHeight = Height
                If aHeight <= 0 Then aHeight = Screen.PrimaryScreen.Bounds.Height
                If Disposed Then
                    Bitmap = New System.Drawing.Bitmap(aWidth, aHeight)
                    FloodFill(BackgroundColor)
                Else
                    Bitmap = New System.Drawing.Bitmap(Bitmap, aWidth, aHeight)
                End If
            Catch ex As Exception
                If RaiseErrors Then Throw ex
            End Try
        End Sub
        Friend Function Properties() As TPROPERTIES
            Dim _rVal As New TPROPERTIES
            _rVal = New TPROPERTIES("", bNonDXF:=True)
            _rVal.Add(New TPROPERTY(1, hBMP, "hBMP", dxxPropertyTypes.dxf_Long, bNonDXF:=True))
            _rVal.Add(New TPROPERTY(2, FileName, "FileName", dxxPropertyTypes.dxf_String, bNonDXF:=True))
            _rVal.Add(New TPROPERTY(3, Name, "Name", dxxPropertyTypes.dxf_String, bNonDXF:=True))
            _rVal.Add(New TPROPERTY(4, Orientation, "Orientation", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))
            _rVal.Add(New TPROPERTY(5, Tag, "Tag", dxxPropertyTypes.dxf_String, bNonDXF:=True))
            _rVal.Add(New TPROPERTY(6, DPI, "PixelsPerInch", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))
            _rVal.Add(New TPROPERTY(7, Width, "Width", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))
            _rVal.Add(New TPROPERTY(8, Height, "Height", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))
            _rVal.Add(New TPROPERTY(9, BackgroundColor.ToString, "BackColor", dxxPropertyTypes.dxf_String, bNonDXF:=True))
            Return _rVal
        End Function
#End Region 'Methods
#Region "Shared Methods"
        Friend Shared Function ValidateControl(ByRef aControl As System.Windows.Forms.Control, ByRef rControlObj As Object, ByRef rHasBackgoungImage As Boolean) As Boolean
            rHasBackgoungImage = False
            Dim rIPropName As String = String.Empty
            If ValidateControl(aControl, rHasBackgoungImage, rIPropName) Then
                rControlObj = aControl
                Return True
            Else
                rControlObj = Nothing
                Return False
            End If
        End Function
        Friend Shared Function ValidateControl(ByRef aControl As System.Windows.Forms.Control) As Boolean
            Dim rHasBackgoungImage As Boolean = False
            Dim rIPropName As String = String.Empty
            Return ValidateControl(aControl, rHasBackgoungImage, rIPropName)
        End Function
        Friend Shared Function ValidateControl(ByRef aControl As System.Windows.Forms.Control, ByRef rHasBackgoungImage As Boolean, ByRef rIPropName As String) As Boolean
            rHasBackgoungImage = False
            rIPropName = ""
            If aControl Is Nothing Then Return False
            Dim ocontrol As Object = aControl
            If dxfUtils.CheckProperty(ocontrol, "Image") Then
                Return True
                rIPropName = "Image"
            ElseIf dxfUtils.CheckProperty(ocontrol, "BackgroundImage") Then
                rIPropName = "BackgroundImage"
                rHasBackgoungImage = True
                Return True
            Else
                Return False
            End If
        End Function

        Friend Shared Function ValidateControl(aHandle As IntPtr, ByRef rControl As System.Windows.Forms.Control, ByRef rControlObj As Object, ByRef rHasBackgoungImage As Boolean) As Boolean
            rControl = Nothing
            rControlObj = Nothing
            rHasBackgoungImage = False
            Try
                If aHandle = 0 Then Return False
                rControl = Windows.Forms.Control.FromHandle(aHandle)
                Return ValidateControl(rControl, rControlObj, rHasBackgoungImage)
            Catch ex As Exception
                Return False
            End Try
        End Function
        Public Shared Function StretchToControl(aBitmap As Bitmap, aHwnd As IntPtr, aScaleFactor As Double, Optional aYScale As Double = 0.0,
                                      Optional aAlignment As dxxRectangularAlignments = dxxRectangularAlignments.TopLeft,
                                      Optional aXOffset As Integer = 0, Optional aYOffset As Integer = 0,
                                      Optional aColorMode As dxxColorModes = dxxColorModes.Full) As Boolean
            Dim rErrorString As String = String.Empty
            Return StretchToControl(aBitmap, aScaleFactor, aYScale, aAlignment, aXOffset, aYOffset, aColorMode, rErrorString)
        End Function
        Public Shared Function StretchToControl(aBitmap As Bitmap, aHwnd As IntPtr, aScaleFactor As Double, aYScale As Double,
                                       aAlignment As dxxRectangularAlignments,
                                       aXOffset As Integer, aYOffset As Integer,
                                       aColorMode As dxxColorModes, ByRef rErrorString As String) As Boolean
            If aBitmap Is Nothing Then Return False
            If aHwnd.Equals(0) Then Return False
            Dim _rVal As Boolean
            '#1the subject bitmap
            '#2the device context to draw the bitmap to
            '#3the scale factor to apply
            '#4a Y scale to apply (<=0 means uniform scaling)
            '#5the aligment to apply
            '#6a X offset to apply
            '#7a Y offset to apply
            '#8the raster operation to apply
            '#9the color mode to apply
            '#10returns any errors
            '^stretches the bitmap to the indicated device context using the indicated scales
            rErrorString = ""
            Dim xScl As Double = aScaleFactor
            Dim yScl As Double = aYScale
            If xScl <= 0 Then xScl = 1
            If yScl <= 0 Then yScl = xScl
            If xScl = 1 And yScl = 1 And aXOffset = 0 And aYOffset = 0 Then
                'just draw it
                Return DrawToDevice(aBitmap, aHwnd, aColorMode, rErrorString)
            End If
            Dim aImage As Bitmap = aBitmap
            If aColorMode = dxxColorModes.BlackWhite Or aColorMode = dxxColorModes.GreyScales Then
                aImage = aBitmap.Clone
                ConvertToColorMode(aImage, aColorMode)
            End If
            Dim Contrl As Object = Nothing
            Dim bBackImg As Boolean
            Dim Sz As Size
            'get info about the output device
            If Not dxfUtils.DeviceIsDefined(aHwnd, Contrl, bBackImg, Sz) Then Return False
            Dim devBMAP As Bitmap
            If Not bBackImg Then devBMAP = Contrl.Image Else devBMAP = Contrl.BackgroundImage
            If devBMAP Is Nothing Then
                devBMAP = New Bitmap(Sz.Width, Sz.Height)
            End If
            Dim aWd As Integer = aImage.Width * xScl
            Dim aHt As Integer = aImage.Height * yScl
            If aWd <= 0 Or aHt <= 0 Then Return False
            If devBMAP.Width <= 0 Or devBMAP.Height <= 0 Then Return _rVal
            Dim p1 As Point = AlignmentPt(devBMAP, aAlignment, New Rectangle(New Point(0, 0), New Size(aWd, aHt)))
            p1.X += aXOffset
            p1.Y += aYOffset
            Dim g As Graphics = Nothing
            Try
                g = Graphics.FromImage(devBMAP)
                Dim destRect As New Rectangle(p1, New Size(aWd, aHt))
                Dim srcRect = New Rectangle(New Point(0, 0), aImage.Size)
                g.DrawImage(aImage, destRect, srcRect, GraphicsUnit.Pixel)
                If Not bBackImg Then Contrl.Image = devBMAP Else Contrl.BackgroundImage = devBMAP
            Catch ex As Exception
                rErrorString = ex.Message
                Return False
            Finally
                g.Dispose()
            End Try
            Return _rVal
        End Function
        Public Shared Function StretchToControl(aBitmap As Bitmap, aHwnd As IntPtr, Optional aColorMode As dxxColorModes = dxxColorModes.Full) As Boolean
            Dim rErrorString As String = String.Empty
            Return StretchToControl(aBitmap, aHwnd, aColorMode, rErrorString)
        End Function
        Public Shared Function StretchToControl(aBitmap As Bitmap, aHwnd As IntPtr, aColorMode As dxxColorModes, ByRef rErrorString As String) As Boolean
            rErrorString = ""
            If aBitmap Is Nothing Then Return False
            If aHwnd = IntPtr.Zero Then Return False
            Dim _rVal As Boolean
            Dim g As Graphics = Nothing
            '#1the subject bitmap
            '#2the device context to draw the bitmap to
            '#3the color mode to apply
            '#4returns any errors
            '^stretches the bitmap to the indicated device context to fill the device client area
            Try
                rErrorString = ""
                Dim bSz As Size = aBitmap.Size
                If bSz.Width <= 0 Or bSz.Height <= 0 Then Return False
                Dim aImage As Bitmap = aBitmap
                If aColorMode = dxxColorModes.BlackWhite Or aColorMode = dxxColorModes.GreyScales Then
                    aImage = aBitmap.Clone
                    ConvertToColorMode(aImage, aColorMode)
                End If
                Dim Contrl As Object = Nothing
                Dim bBackImg As Boolean
                Dim dSz As Size
                'get info about the output device
                If Not dxfUtils.DeviceIsDefined(aHwnd, Contrl, bBackImg, dSz) Then Return False
                Dim devBMAP As Bitmap
                If Not bBackImg Then devBMAP = Contrl.Image Else devBMAP = Contrl.BackgroundImage
                If devBMAP Is Nothing Then
                    devBMAP = New Bitmap(dSz.Width, dSz.Height)
                End If
                If devBMAP.Width <= 0 Or devBMAP.Height <= 0 Then Return _rVal
                Dim xScl As Double = dSz.Width / bSz.Width
                Dim yScl As Double = dSz.Height / bSz.Height
                Dim p1 As New Point(0, 0)
                g = Graphics.FromImage(devBMAP)
                Dim destRect As New Rectangle(p1, dSz)
                Dim srcRect = New Rectangle(New Point(0, 0), aImage.Size)
                g.DrawImage(aImage, destRect, srcRect, GraphicsUnit.Pixel)
                If Not bBackImg Then Contrl.Image = devBMAP Else Contrl.BackgroundImage = devBMAP
                g.Dispose()
                Contrl = Nothing
            Catch ex As Exception
                rErrorString = ex.Message
                Return False
            Finally
                If g IsNot Nothing Then g.Dispose()
            End Try
            Return _rVal
        End Function
        Public Shared Function ConvertToColorMode(aBitmap As Bitmap, aColorMode As dxxColorModes, Optional aBlack As Color? = Nothing, Optional aWhite As Color? = Nothing) As Boolean
            If aBitmap Is Nothing Then Return False
            If aColorMode <> dxxColorModes.GreyScales And aColorMode <> dxxColorModes.BlackWhite Then Return False
            If aColorMode = dxxColorModes.BlackWhite Then
                If aBlack Is Nothing Then aBlack = Color.Black
                If aWhite Is Nothing Then aWhite = Color.White
                If Not aBlack.HasValue Then aBlack = Color.Black
                If Not aWhite.HasValue Then aWhite = Color.White
                If aBlack = aWhite Then Return False
            End If
            '#1color mode to convert to (greyscales or black and white)
            '^converts all the pixels int the bitmap to a shade of grey or black and wigth bassed on it's current hue
            Dim bmBytes As TBITMAPDATA
            Dim pClr As Color
            Dim i As Integer
            Dim total_Bytes As Integer
            Dim bb, bg, br, ba As Integer
            Dim st As Integer = 0
            Dim pixlen As Integer
            Dim bClr As COLOR_ARGB
            Dim aClr As COLOR_ARGB
            Dim aColor As Color
            Dim bDoiT As Boolean
            Dim _rVal As Boolean = False
            Try
                'make sure it's a 32 bit bitmap
                aBitmap = TBITMAPDATA.ConvertToRGB32(aBitmap)
                'get the bitmap data
                bmBytes = New TBITMAPDATA(aBitmap)
                'lock the data for processing
                bmBytes.LockBitmap()
                'bStep = TValues.To_INT(bmBytes.RowSizeBytes / BMAP.Width) + 1
                total_Bytes = bmBytes.ImageBytes.Length
                pixlen = bmBytes.Data.Stride / aBitmap.Width
                'loop on rows
                For i = 0 To aBitmap.Height - 1 ' total_Bytes - 1 Step bStep
                    'set the row
                    st = (bmBytes.Data.Stride) * i
                    'loop on columns
                    For j As Integer = 0 To aBitmap.Width - 1
                        If st + pixlen > total_Bytes - 1 Then
                            Exit For
                        End If
                        bb = bmBytes.ImageBytes(st)
                        bg = bmBytes.ImageBytes(st + 1)
                        br = bmBytes.ImageBytes(st + 2)
                        If pixlen = 4 Then
                            ba = bmBytes.ImageBytes(st + 3)
                            pClr = Color.FromArgb(ba, br, bg, bb)
                        Else
                            pClr = Color.FromArgb(br, bg, bb)
                        End If
                        If aColorMode = dxxColorModes.GreyScales Then
                            aClr = dxfColors.Win64ToARGB(pClr)
                            bClr = aClr.Greyscale
                            aColor = CType(bClr, Color)
                            bDoiT = True
                        Else
                            If Not dxfColors.CompareColors(pClr, aWhite) Then aColor = aBlack Else aColor = aWhite
                            bDoiT = Not dxfColors.CompareColors(pClr, aBlack) And Not dxfColors.CompareColors(pClr, aWhite)
                        End If
                        If bDoiT Then
                            bmBytes.ImageBytes(st) = aColor.B
                            bmBytes.ImageBytes(st + 1) = aColor.G
                            bmBytes.ImageBytes(st + 2) = aColor.R
                            If pixlen = 4 Then
                                bmBytes.ImageBytes(st + 3) = aColor.A
                            End If
                            _rVal = True
                        End If
                        st += pixlen
                    Next j
                Next i
                bmBytes.UnlockBitmap()
                Return _rVal
            Catch ex As Exception
                Return False
            End Try
        End Function
        Public Shared Function CopyControlBitmap(aHandle As IntPtr) As Bitmap
            Dim rError As String = string.Empty
            Return CopyControlBitmap(aHandle, rError)
        End Function
        Public Shared Function CopyControlBitmap(aHandle As IntPtr, ByRef rError As String) As Bitmap
            rError = string.Empty
            '#1the handle to the device (control) to copy the bitmap from
            '^copies the bitmap at its current dimensions form the target device
            '~similiar to create but allows the user to indicate a specific bitmap on the device
            Try
                If aHandle.Equals(0) Then Throw New Exception("The Passed Handle Is Invalid")
                Dim sErr As String = String.Empty
                Dim Contrl As Object = Nothing
                Dim bBackImg As Boolean
                Dim sz As System.Drawing.Size
                If Not dxfUtils.DeviceIsDefined(aHandle, Contrl, bBackImg, sz) Then Throw New Exception("The Passed Handle Is Invalid")
                Dim devBMAP As Bitmap
                If Not bBackImg Then devBMAP = Contrl.Image Else devBMAP = Contrl.BackgroundImage
                Contrl = Nothing
                If devBMAP Is Nothing Then Throw New Exception("The Passed Control Has No Image Defined")
                Return devBMAP
            Catch ex As Exception
                rError = ex.Message
                Throw ex
            End Try
        End Function
        Public Shared Function DrawToBitmap(aBitmap As Bitmap, aToBitmap As dxfBitmap, aAlignment As dxxRectangularAlignments, ByRef rError As String) As Boolean
            rError = string.Empty
            If aBitmap Is Nothing Or aToBitmap Is Nothing Then Return False
            If Not aToBitmap.IsDefined Then Return False
            '#2the destination bitmap.
            '#4the alignment to apply
            '^draw the current or passed bitmap to the to the destination
            Try
                Dim b1 As Bitmap = aToBitmap.Bitmap
                Dim g As Graphics = Graphics.FromImage(b1)
                g.DrawImageUnscaled(aBitmap, dxfBitmap.AlignmentPt(b1, aAlignment, New Rectangle(New Point(0, 0), b1.Size)))
                Return True
            Catch ex As Exception
                rError = ex.Message
                Return False
            End Try
        End Function
        Public Shared Function DrawToDevice(aBitmap As Bitmap, aHwd As IntPtr, aAlignment As dxxRectangularAlignments, ByRef rError As String) As Boolean
            rError = string.Empty
            Dim _rVal As Boolean
            '#2the destination hWnd.
            '#4the alignment to apply
            '^draw the current or passed bitmap to the to the destination
            If aBitmap Is Nothing Then Return False
            If aHwd.Equals(0) Then
                Return False
            End If
            Dim Contrl As Object = Nothing
            Dim bBackImg As Boolean
            Dim aSz As Size = Nothing
            Try
                If dxfUtils.DeviceIsDefined(aHwd, Contrl, bBackImg, aSz) Then
                    Dim devBMAP As Bitmap
                    If Not bBackImg Then devBMAP = Contrl.Image Else devBMAP = Contrl.BackgroundImage
                    If devBMAP Is Nothing Then devBMAP = New Bitmap(aSz.Width, aSz.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                    Dim g As Graphics = Graphics.FromImage(devBMAP)
                    g.DrawImageUnscaled(aBitmap, dxfBitmap.AlignmentPt(devBMAP, aAlignment, New Rectangle(New Point(0, 0), aBitmap.Size)))
                    g.Dispose()
                    g = Nothing
                    Contrl.Refresh()
                    If Not bBackImg Then Contrl.Image = devBMAP Else Contrl.BackgroundImage = devBMAP
                    Contrl = Nothing
                    _rVal = True
                End If
            Catch ex As Exception
                rError = ex.Message
                _rVal = False
            End Try
            Return _rVal
        End Function
        Public Shared Function FloodFill(aBitmap As Bitmap, aColor As Color) As Boolean
            If aBitmap Is Nothing Then Return False
            Dim g As Graphics = Graphics.FromImage(aBitmap)
            Dim aRec As New Rectangle(New Point(0, 0), aBitmap.Size)
            Dim b As Brush = New SolidBrush(aColor)
            Try
                'g.Clear(aColor)
                g.FillRectangle(b, aRec)
                b.Dispose()
                g.Dispose()
                Return True
            Catch ex As Exception
                Return False
            Finally
                If g IsNot Nothing Then g.Dispose()
                If b IsNot Nothing Then b.Dispose()
            End Try
        End Function
        Public Shared Function Flip(aBitmap As Bitmap, bVertically As Boolean, ByRef rErrorString As String) As Boolean
            '#1the subject bitmap
            '#2flag to flip the bitmap around its X axis
            '~inverts the bitmap around it's X or Y axis. Y axis is default
            rErrorString = ""
            If aBitmap Is Nothing Then Return False
            Dim _rVal As Boolean
            Dim aWd As Integer = aBitmap.Width
            Dim aHt As Integer = aBitmap.Height
            Dim iR As Integer
            Dim iC As Integer
            Dim jR As Integer
            Dim jC As Integer
            Dim aClr As Color
            Dim bClr As Color
            Try
                If Not bVertically Then
                    For iR = 0 To aHt - 1
                        jR = iR
                        jC = aWd - 1
                        For iC = 0 To aWd - 1
                            aClr = aBitmap.GetPixel(iC, iR)
                            bClr = aBitmap.GetPixel(iC, iR)
                            If aClr <> bClr Then _rVal = True
                            aBitmap.SetPixel(jC, jR, aClr)
                            jC -= 1
                        Next iC
                    Next iR
                Else
                    jR = aHt - 1
                    For iR = 0 To aHt - 1
                        For iC = 0 To aWd - 1
                            jC = iC
                            aClr = aBitmap.GetPixel(iC, iR)
                            bClr = aBitmap.GetPixel(iC, iR)
                            If aClr <> bClr Then _rVal = True
                            aBitmap.SetPixel(jC, jR, aClr)
                        Next iC
                        jR -= 1
                    Next iR
                End If
                Return True
            Catch ex As Exception
                rErrorString = ex.Message
                Return False
            End Try
            Return _rVal
        End Function
        Public Shared Function ReplaceColor(aBitmap As Bitmap, aSearchColor As Color, aColor As Color) As Boolean
            Dim rErrorString As String = String.Empty
            Return ReplaceColor(aBitmap, aSearchColor, aColor, rErrorString)
        End Function
        Public Shared Function ReplaceColor(aBitmap As Bitmap, aSearchColor As Color, aColor As Color, ByRef rErrorString As String) As Boolean
            rErrorString = ""
            If aBitmap Is Nothing Then Return False
            Dim _rVal As Boolean = False
            '#2the color to search for
            '#3the color to change to
            '^replaces any pixel of the search color with the rplacement color
            Dim bmBytes As TBITMAPDATA
            Dim pClr As Color
            Dim i As Integer
            Dim total_Bytes As Integer
            Dim bb, bg, br, ba As Integer
            Dim st As Integer = 0
            Dim pixlen As Integer
            Try
                'make sure it's a 32 bit bitmap
                aBitmap = TBITMAPDATA.ConvertToRGB32(aBitmap)
                'get the bitmap data
                bmBytes = New TBITMAPDATA(aBitmap)
                'lock the data for processing
                bmBytes.LockBitmap()
                'bStep = TValues.To_INT(bmBytes.RowSizeBytes / aBitmap.Width) + 1
                total_Bytes = bmBytes.ImageBytes.Length
                pixlen = bmBytes.Data.Stride / aBitmap.Width
                'loop on rows
                For i = 0 To aBitmap.Height - 1 ' total_Bytes - 1 Step bStep
                    'set the row
                    st = (bmBytes.Data.Stride) * i
                    'loop on columns
                    For j As Integer = 0 To aBitmap.Width - 1
                        If st + pixlen > total_Bytes - 1 Then
                            Exit For
                        End If
                        bb = bmBytes.ImageBytes(st)
                        bg = bmBytes.ImageBytes(st + 1)
                        br = bmBytes.ImageBytes(st + 2)
                        If pixlen = 4 Then
                            ba = bmBytes.ImageBytes(st + 3)
                            pClr = Color.FromArgb(ba, br, bg, bb)
                        Else
                            pClr = Color.FromArgb(br, bg, bb)
                        End If
                        If dxfColors.CompareColors(pClr, aColor) Or aSearchColor.Name = "0" Then
                            bmBytes.ImageBytes(st) = aColor.B
                            bmBytes.ImageBytes(st + 1) = aColor.G
                            bmBytes.ImageBytes(st + 2) = aColor.R
                            If pixlen = 4 Then
                                bmBytes.ImageBytes(st + 3) = aColor.A
                            End If
                            _rVal = True
                        End If
                        st += pixlen
                    Next j
                Next i
                bmBytes.UnlockBitmap()
                Return True
            Catch ex As Exception
                rErrorString = ex.Message
                Return False
            End Try
        End Function
        Public Shared Function AlignmentPt(aBitmap As Bitmap, aAlignment As dxxRectangularAlignments, Optional aRecToAlign As Rectangle? = Nothing) As Point
            Dim p1 As New Point(0, 0)
            If aBitmap Is Nothing Then Return p1
            Dim bsz As Size = aBitmap.Size
            Dim sz As New Size(0, 0)
            If aRecToAlign.HasValue Then sz = aRecToAlign.Value.Size
            Select Case aAlignment
                Case dxxRectangularAlignments.TopLeft ' = 1
                Case dxxRectangularAlignments.TopCenter ' = 2
                    p1.X = 0.5 * bsz.Width - 0.5 * sz.Width
                Case dxxRectangularAlignments.TopRight ' = 3
                    p1.X = bsz.Width - sz.Width
                Case dxxRectangularAlignments.MiddleLeft ' = 4
                    p1.Y = -(-0.5 * bsz.Height + 0.5 * sz.Height)
                Case dxxRectangularAlignments.MiddleCenter ' = 5
                    p1.X = 0.5 * bsz.Width - 0.5 * sz.Width
                    p1.Y = -(-0.5 * bsz.Height + 0.5 * sz.Height)
                Case dxxRectangularAlignments.MiddleRight ' = 6
                    p1.X = bsz.Width - sz.Width
                    p1.Y = -(-0.5 * bsz.Height + 0.5 * sz.Height)
                Case dxxRectangularAlignments.BottomLeft ' = 7
                    p1.Y = -(-bsz.Height + sz.Height)
                Case dxxRectangularAlignments.BottomCenter ' = 8
                    p1.X = 0.5 * bsz.Width - 0.5 * sz.Width
                    p1.Y = -(-bsz.Height + sz.Height)
                Case dxxRectangularAlignments.BottomRight ' = 9
                    p1.X = bsz.Width - sz.Width
                    p1.Y = -(-bsz.Height + sz.Height)
            End Select
            Return p1
        End Function
        Public Shared Function CreateACLPallet(Optional aBlockWd As Integer = 20, Optional aBackColor As Color? = Nothing) As dxfBitmap
            If aBackColor Is Nothing Then aBackColor = Color.White
            If Not aBackColor.HasValue Then aBackColor = Color.White
            Dim i As Long
            Dim base As Long
            Dim aBMP As dxfBitmap = Nothing
            Dim Rect As Rectangle
            If aBlockWd <= 0 Then aBlockWd = 10
            If aBlockWd > 400 Then aBlockWd = 400
            Dim aclr As dxfColor
            Dim aStep As Integer
            'On Error Resume Next
            Dim b As Brush
            Dim g As Graphics
            Dim BMP As Bitmap
            Dim p1 As New TVECTOR(1, 1)
            Dim sz As New System.Drawing.Size(aBlockWd, aBlockWd)
            aBMP = New dxfBitmap(aBlockWd * 24 + 24, aBlockWd * 10 + 10 + 1 + (aBlockWd + 1), aBackColor)
            BMP = aBMP.Bitmap
            g = Graphics.FromImage(BMP)
            aclr = dxfColors.Color(dxxColors.Red)
            Rect = New Rectangle(CType(p1, Point), sz)
            b = New SolidBrush(aclr.ToWin64)
            g.FillRectangle(b, Rect)
            b.Dispose()
            p1.X = p1.X + aBlockWd + 1
            aclr = dxfColors.Color(dxxColors.Yellow)
            Rect = New Rectangle(CType(p1, Point), sz)
            b = New SolidBrush(aclr.ToWin64)
            g.FillRectangle(b, Rect)
            b.Dispose()
            p1.X = p1.X + aBlockWd + 1
            aclr = dxfColors.Color(dxxColors.Green)
            Rect = New Rectangle(CType(p1, Point), sz)
            b = New SolidBrush(aclr.ToWin64)
            g.FillRectangle(b, Rect)
            b.Dispose()
            p1.X = p1.X + aBlockWd + 1
            aclr = dxfColors.Color(dxxColors.Cyan)
            Rect = New Rectangle(CType(p1, Point), sz)
            b = New SolidBrush(aclr.ToWin64)
            g.FillRectangle(b, Rect)
            b.Dispose()
            p1.X = p1.X + aBlockWd + 1
            aclr = dxfColors.Color(dxxColors.Blue)
            Rect = New Rectangle(CType(p1, Point), sz)
            b = New SolidBrush(aclr.ToWin64)
            g.FillRectangle(b, Rect)
            b.Dispose()
            p1.X = p1.X + aBlockWd + 1
            aclr = dxfColors.Color(dxxColors.Magenta)
            Rect = New Rectangle(CType(p1, Point), sz)
            b = New SolidBrush(aclr.ToWin64)
            g.FillRectangle(b, Rect)
            b.Dispose()
            p1.X = p1.X + aBlockWd + 1
            aclr = dxfColors.Color(dxxColors.BlackWhite)
            Rect = New Rectangle(CType(p1, Point), sz)
            b = New SolidBrush(aclr.ToWin64)
            g.FillRectangle(b, Rect)
            b.Dispose()
            p1.X = p1.X + aBlockWd + 1
            aclr = dxfColors.Color(8)
            Rect = New Rectangle(CType(p1, Point), sz)
            b = New SolidBrush(aclr.ToWin64)
            g.FillRectangle(b, Rect)
            b.Dispose()
            p1.X = p1.X + aBlockWd + 1
            aclr = dxfColors.Color(9)
            Rect = New Rectangle(CType(p1, Point), sz)
            b = New SolidBrush(aclr.ToWin64)
            g.FillRectangle(b, Rect)
            b.Dispose()
            p1.X = p1.X + aBlockWd + 1
            'greys
            p1.X = p1.X + aBlockWd
            aclr = dxfColors.Color(250)
            Rect = New Rectangle(CType(p1, Point), sz)
            b = New SolidBrush(aclr.ToWin64)
            g.FillRectangle(b, Rect)
            b.Dispose()
            p1.X = p1.X + aBlockWd + 1
            aclr = dxfColors.Color(251)
            Rect = New Rectangle(CType(p1, Point), sz)
            b = New SolidBrush(aclr.ToWin64)
            g.FillRectangle(b, Rect)
            b.Dispose()
            p1.X = p1.X + aBlockWd + 1
            aclr = dxfColors.Color(252)
            Rect = New Rectangle(CType(p1, Point), sz)
            b = New SolidBrush(aclr.ToWin64)
            g.FillRectangle(b, Rect)
            b.Dispose()
            p1.X = p1.X + aBlockWd + 1
            aclr = dxfColors.Color(253)
            Rect = New Rectangle(CType(p1, Point), sz)
            b = New SolidBrush(aclr.ToWin64)
            g.FillRectangle(b, Rect)
            b.Dispose()
            p1.X = p1.X + aBlockWd + 1
            aclr = dxfColors.Color(254)
            Rect = New Rectangle(CType(p1, Point), sz)
            b = New SolidBrush(aclr.ToWin64)
            g.FillRectangle(b, Rect)
            b.Dispose()
            p1.X = p1.X + aBlockWd + 1
            aclr = dxfColors.Color(255)
            Rect = New Rectangle(CType(p1, Point), sz)
            b = New SolidBrush(aclr.ToWin64)
            g.FillRectangle(b, Rect)
            b.Dispose()
            p1.X = p1.X + aBlockWd + 1
            base = 18
            p1.X = 1
            p1.Y = aBlockWd + 2
            aStep = 0
            Do While base >= 10
                aStep = 0
                For i = 0 To 23
                    aclr = dxfColors.Color(base + aStep)
                    Rect = New Rectangle(CType(p1, Point), sz)
                    b = New SolidBrush(aclr.ToWin64)
                    g.FillRectangle(b, Rect)
                    b.Dispose()
                    p1.X = p1.X + aBlockWd + 1
                    aStep += 10
                Next i
                p1.Y = p1.Y + aBlockWd + 1
                p1.X = 1
                base -= 2
            Loop
            base = 11
            Do While base <= 19
                aStep = 0
                For i = 0 To 23
                    aclr = dxfColors.Color(base + aStep)
                    Rect = New Rectangle(CType(p1, Point), sz)
                    b = New SolidBrush(aclr.ToWin64)
                    g.FillRectangle(b, Rect)
                    b.Dispose()
                    p1.X = p1.X + aBlockWd + 1
                    aStep += 10
                Next i
                p1.Y = p1.Y + aBlockWd + 1
                p1.X = 1
                base += 2
            Loop
            g.Dispose()
            aBMP.Bitmap = BMP
            Return aBMP
        End Function
#End Region 'Shared Methods
#Region "IDisposable Implementation"
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not _Disposed Then
                If disposing Then
                    ' dispose managed state (managed objects)
                    If _Bitmap IsNot Nothing Then _Bitmap.Dispose()
                End If
                _Bitmap = Nothing
                ' free unmanaged resources (unmanaged objects) and override finalizer
                ' set large fields to null
                _Disposed = True
            End If
        End Sub
        Protected Overrides Sub Finalize()
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=False)
            MyBase.Finalize()
        End Sub
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region 'IDisposable Implementation
    End Class
End Namespace
