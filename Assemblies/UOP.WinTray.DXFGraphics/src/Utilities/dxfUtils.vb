
Imports System.Runtime.InteropServices
Imports System.IO
Imports System.Linq
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.dxfGlobals
Imports UOP.DXFGraphics.Fonts.dxfFonts

Namespace UOP.DXFGraphics.Utilities
    Public Class dxfUtils
#Region "Types"
        Friend Structure FILETIME
#Region "Members"
            Public dwHighDateTime As Long
            Public dwLowDateTime As Long
#End Region 'Members
        End Structure
#End Region 'Types
#Region "Constants"
        Public Const REG_SZ = 1
        Public Const KEY_ENUMERATE_SUB_KEYS = &H8
        Public Const READ_CONTROL = &H20000
        Public Const KEY_QUERY_VALUE = &H1
        Public Const STANDARD_RIGHTS_READ = (READ_CONTROL)
        Public Const KEY_NOTIFY = &H10
        Public Const SYNCHRONIZE = &H100000
        Private Const KEY_WOW64_64KEY As Long = &H100&
        Private Const KEY_WOW64_32KEY As Long = &H200&
        Public Const KEY_READ = ((STANDARD_RIGHTS_READ Or KEY_QUERY_VALUE Or KEY_ENUMERATE_SUB_KEYS Or KEY_NOTIFY) And (Not SYNCHRONIZE))
#End Region 'Constants
#Region "Declares"
        ''======== reg read api declares
        'Private Declare Ansi Function RegQueryValueEx Lib "advapi32.dll" Alias "RegQueryValueExA" (hKey As Long, lpValueName As String, lpReserved As Long, lpType As Long, <System.Runtime.InteropServices.MarshalAsAttribute(
        'System.Runtime.InteropServices.UnmanagedType.AsAny)> lpData As Object, lpcbData As Long) As Long
        Private Declare Function RegOpenKeyEx Lib "advapi32.dll" Alias "RegOpenKeyExA" (hKey As Long, lpSubKey As String, ulOptions As Long, samDesired As Long, phkResult As Long) As Long
        Private Declare Function RegEnumKeyEx Lib "advapi32.dll" Alias "RegEnumKeyExA" (hKey As Long, dwIndex As Long, lpName As String, lpcbName As Long, lpReserved As Long, lpClass As String, lpcbClass As Long, lpftLastWriteTime As FILETIME) As Long
        Private Declare Function RegCloseKey Lib "advapi32.dll" (hKey As Long) As Long
#End Region 'Declares
#Region "Methods"
#End Region 'Methods
#Region "Shared Methods"

        Public Shared Function LimitedValue(aValue As Integer, aMin As Integer, aMax As Integer) As Integer
            Dim _rVal As Integer = aMin
            If aMin > aMax Then
                aMin = aMax
                aMax = _rVal
            End If
            _rVal = aValue
            If _rVal < aMin Then _rVal = aMin
            If _rVal > aMax Then _rVal = aMax
            Return _rVal
        End Function
        Public Shared Function LimitedValue(aValue As Double, aMin As Double, aMax As Double) As Double
            Dim _rVal As Double = aMin
            If aMin > aMax Then
                aMin = aMax
                aMax = _rVal
            End If
            _rVal = aValue
            If _rVal < aMin Then _rVal = aMin
            If _rVal > aMax Then _rVal = aMax
            Return _rVal
        End Function
        Public Shared Function PhantomPoints(aEntity As dxfEntity, Optional aCurveDivisions As Integer = 20, Optional aLineDivisions As Integer = 1) As colDXFVectors
            Dim _rVal As New colDXFVectors
            If aEntity Is Nothing Then Return _rVal
            Select Case aEntity.GraphicType
                Case dxxGraphicTypes.Arc
                    Dim aA As dxeArc
                    aA = aEntity
                    aA.ArcStructure.PhantomPoints(aCurveDivisions, True, aCollector:=_rVal)
                Case dxxGraphicTypes.Bezier
                    Dim aBz As dxeBezier
                    aBz = aEntity
                    aBz.BezierStructure.PhantomPoints(aCurveDivisions, True, aCollector:=_rVal)
                Case dxxGraphicTypes.Dimension
                    Dim aDim As dxeDimension
                    aDim = aEntity
                    _rVal = aDim.Entities.PhantomPoints(aCurveDivisions, aLineDivisions)
                Case dxxGraphicTypes.Ellipse
                    Dim aE As dxeEllipse
                    aE = aEntity
                    aE.ArcStructure.PhantomPoints(aCurveDivisions, True, aCollector:=_rVal)
                Case dxxGraphicTypes.Hatch
                    Dim aHt As dxeHatch
                    aHt = aEntity
                    _rVal = aHt.BoundingRectangle.BorderLines.PhantomPoints(aCurveDivisions, aLineDivisions, True)
                Case dxxGraphicTypes.Hole
                    Dim aH As dxeHole
                    aH = aEntity
                    _rVal = aH.Boundary.PhantomPoints(aCurveDivisions, aLineDivisions)
                Case dxxGraphicTypes.Insert
                    Dim aI As dxeInsert
                    aI = aEntity
                    _rVal = aI.BoundingRectangle.BorderLines.PhantomPoints(aCurveDivisions, aLineDivisions, True)
                Case dxxGraphicTypes.Leader
                    Dim aLdr As dxeLeader
                    aLdr = aEntity
                    aLdr.UpdatePath()
                    _rVal = aLdr.LeaderLines.PhantomPoints(aCurveDivisions, aLineDivisions)
                Case dxxGraphicTypes.Line
                    Dim tl As New TLINE(DirectCast(aEntity, dxeLine))

                    tl.PhantomPoints(aLineDivisions, True, aCollector:=_rVal)
                Case dxxGraphicTypes.Point
                    Dim aPt As dxePoint
                    aPt = aEntity
                    _rVal.AddV(aPt.Vector)
                Case dxxGraphicTypes.Polygon
                    Dim aPg As dxePolygon
                    aPg = aEntity
                    Dim aSegs As New colDXFEntities(aPg.PathSegments)
                    _rVal = aSegs.PhantomPoints(aCurveDivisions, aLineDivisions, True)
                    If aPg.Closed Then _rVal.Remove(_rVal.Count)
                    If Not aPg.SuppressAdditionalSegments Then _rVal.Append(aPg.AdditionalSegments.PhantomPoints(aCurveDivisions, aLineDivisions))
                Case dxxGraphicTypes.Polyline
                    Dim aPl As dxePolyline
                    aPl = aEntity
                    aPl.UpdatePath()
                    _rVal = aPl.Segments.PhantomPoints(aCurveDivisions, aLineDivisions, True)
                    If aPl.Closed Then _rVal.Remove(_rVal.Count)
                Case dxxGraphicTypes.Solid
                    Dim aSld As dxeSolid
                    aSld = aEntity
                    _rVal = aSld.Vertices.ConnectingLines(True).PhantomPoints(aLineDivision:=aLineDivisions, bExcludeStartPts:=True)
                Case dxxGraphicTypes.Symbol
                    Dim aSy As dxeSymbol
                    aSy = aEntity
                    _rVal = aSy.BoundingRectangle.BorderLines.PhantomPoints(aLineDivision:=aLineDivisions, bExcludeStartPts:=True)
                Case dxxGraphicTypes.Table
                    Dim aTB As dxeTable
                    aTB = aEntity
                    _rVal = aTB.BoundingRectangle.BorderLines.PhantomPoints(aLineDivision:=aLineDivisions, bExcludeStartPts:=True)
                Case dxxGraphicTypes.Text
                    Dim aTxt As dxeText
                    aTxt = aEntity
                    _rVal = aTxt.BoundingRectangle.BorderLines.PhantomPoints(aLineDivision:=aLineDivisions, bExcludeStartPts:=True)
                Case dxxGraphicTypes.Shape
                    Dim aShp As dxeShape
                    aShp = aEntity
                    _rVal = aShp.Entities.PhantomPoints
            End Select
            Return _rVal
        End Function
        Public Shared Sub ShowImageProperties(aImage As dxfImage, Optional aOwnerForm As System.Windows.Forms.Form = Nothing)
            If aImage Is Nothing AndAlso aImage.Disposed Then Return
            Dim aFrm As New frmDXFProperties
            Try
                aFrm.ShowImageProperties(aImage, aOwnerForm)
            Catch ex As Exception
            Finally
                aFrm.Dispose()
            End Try
            Return
        End Sub
        Public Shared Function ThisOrThat(This As String, That As String) As String
            If Not String.IsNullOrWhiteSpace(This) Then Return This
            If Not String.IsNullOrWhiteSpace(That) Then Return That Else Return String.Empty

        End Function




        Public Shared Function ConvertLetterToInteger(aLetter As String) As Integer
            Dim _rVal As Integer
            '#1the character to convert to a number
            '^converts the passed character to a number
            '~i.e. "A" = 1 or "a" = 1 or "Z" = 26 OR "AA" = 27 or "BA" = 53
            Dim C As String
            Dim asci1 As Integer
            Dim asci2 As Integer
            Dim ltr As String
            ltr = Trim(UCase(aLetter))
            If ltr.Length = 0 Then Return _rVal
            If ltr.Length > 2 Then ltr = ltr.Substring(0, 2)
            C = ltr.Substring(0, 1)
            asci1 = Asc(C)
            If asci1 < 65 Or asci1 > 90 Then asci1 = 65
            asci1 -= 64
            If ltr.Length = 1 Then
                _rVal = asci1
            Else
                C = Right(ltr, 1)
                asci2 = Asc(C)
                If asci2 < 65 Or asci2 > 90 Then asci2 = 65
                asci2 -= 64
                _rVal = asci1 * 26 + asci2
            End If
            Return _rVal
        End Function
        Public Shared Function ConvertIntegerToLetter(aNum As Object) As String

            '#1the number to convert
            '^converts the passed number to it's relative alphabetic character
            '~i.e. 1 = "A",  2 = "B" etc.
            '~numbers greater than 26 are turned into a string of mutilple letters (27 = "AA")
            Dim times As Integer
            Dim pnum As Integer = TVALUES.ToInteger(aNum, True)
            Dim nnum As Integer = pnum
            Dim c1 As String
            'On Error Resume Next

            If pnum > 26 Then
                times = Fix(pnum / 26)
                c1 = Chr((times - 1) + 65)
                nnum = pnum - (times * 26) - 1
                If nnum < 0 Then nnum = 0
            Else
                c1 = Chr((pnum - 1) + 65)
            End If
            If pnum > 26 Then
                Return c1 & Chr(nnum + 65)
            Else
                Return c1
            End If
        End Function
        Public Shared Function IsHost64Bit() As Boolean
            Return Environment.Is64BitOperatingSystem
        End Function
        Public Shared Function Registry_GetSubKeys(aRoot As mzRegistryRoots, aKey As String, ByRef rKeyFound As Boolean, ByRef r64Bit As Boolean) As ArrayList
            Dim _rVal As New ArrayList
            '#1the root registry key to look in
            '#2the registry key to search under
            '#3returns True if the passes root and key exists
            '^used to get the list of keys below a the passed key in the registry
            '~ Enumerate the subkeys under the passed registry key location.  The name
            ' ~of the subkeys is returned in a collection
            Dim hKey As Long ' receives a handle to the newly created or opened registry key
            Dim keyname As String ' receives name of each subkey
            Dim keylen As Long ' length of keyname
            Dim classname As String ' receives class of each subkey
            Dim classlen As Long ' length of classname
            Dim lastwrite As New FILETIME ' receives last-write-to time, but we ignore it here
            Dim Index As Long ' counter variable for index
            Dim retVal As Long ' function's return value
            Try
                r64Bit = dxfUtils.IsHost64Bit()
                ' Open the desired registry key.  Note the access level requested.
                If r64Bit = False Then
                    retVal = RegOpenKeyEx(aRoot, aKey, 0, KEY_ENUMERATE_SUB_KEYS, hKey)
                Else
                    retVal = RegOpenKeyEx(aRoot, aKey, 0, KEY_ENUMERATE_SUB_KEYS Or KEY_WOW64_64KEY, hKey)
                End If
                rKeyFound = (retVal = 0)
                ' Test to make sure the key was opened successfully.
                If rKeyFound Then
                    ' List through each possible subkey.  Note how the strings receiving the information
                    ' must be reinitialized each loop iteration.
                    Index = 0 ' initial index value
                    Do While retVal = 0 ' while we keep having success (retval equals 0 from the above API call)
                        keyname = Space(255)
                        classname = Space(255) ' make room in string buffers
                        keylen = 255
                        classlen = 255 ' identify the allocated space
                        ' Get information about the next subkey, if one exists.
                        retVal = RegEnumKeyEx(hKey, Index, keyname, keylen, 0, classname, classlen, lastwrite)
                        If retVal = 0 Then ' only display info if another subkey was found
                            ' Extract the useful information from the string buffers.
                            keyname = keyname.Substring(0, keylen) ' trim off the excess space
                            classname = classname.Substring(0, classlen)
                            ' Display the returned information.
                            _rVal.Add(keyname)
                        End If
                        Index += 1 ' increment the index counter
                    Loop ' end the loop
                    ' Close the registry key after enumeration is complete.
                    retVal = RegCloseKey(hKey)
                End If
                Return _rVal
            Catch ex As Exception
                Throw New Exception($"dxfUtils.{ Reflection.MethodBase.GetCurrentMethod.Name} - { ex.Message}")
                Return _rVal
            End Try
        End Function
        Public Shared Function Registry_ReadKey(aRoot As mzRegistryRoots, aSubKey As String, aKey As String) As String
            Dim rKeyFound As Boolean = False
            Return Registry_ReadKey(aRoot, aSubKey, aKey, rKeyFound)
        End Function
        Public Shared Function Registry_ReadKey(aRoot As mzRegistryRoots, aSubKey As String, aKey As String, ByRef rKeyFound As Boolean) As String
            Dim _rVal As String = String.Empty
            rKeyFound = False
            '#1the root registry key to look in
            '#2the registry subkey to search for
            '#3the registry key to find
            '^used to get key values from the host system registry
            Dim oReg As Microsoft.Win32.RegistryKey
            Dim keyVal As Object
            Try
                Select Case aRoot
                    Case mzRegistryRoots.HKEY_CLASSES_ROOT
                        oReg = My.Computer.Registry.ClassesRoot.OpenSubKey(aSubKey)
                    Case mzRegistryRoots.HKEY_CURRENT_CONFIG
                        oReg = My.Computer.Registry.CurrentConfig.OpenSubKey(aSubKey)
                    Case mzRegistryRoots.HKEY_CURRENT_USER
                        oReg = My.Computer.Registry.CurrentUser.OpenSubKey(aSubKey)
                    Case mzRegistryRoots.HKEY_LOCAL_MACHINE
                        oReg = My.Computer.Registry.LocalMachine.OpenSubKey(aSubKey)
                    Case mzRegistryRoots.HKEY_PERFORMANCE_DATA
                        oReg = My.Computer.Registry.PerformanceData.OpenSubKey(aSubKey)
                    Case mzRegistryRoots.HKEY_USERS
                        oReg = My.Computer.Registry.Users.OpenSubKey(aSubKey)
                    Case Else
                        Return String.Empty
                End Select
                If oReg Is Nothing Then
                    Return String.Empty
                Else
                    keyVal = oReg.GetValue(aKey)
                    If keyVal Is Nothing Then
                        _rVal = ""
                    Else
                        _rVal = keyVal.ToString
                        rKeyFound = True
                    End If
                    oReg.Close()
                    Return _rVal
                End If
            Catch ex As Exception
                rKeyFound = False
                Return String.Empty
            End Try
            Return _rVal
        End Function
        Friend Shared Function DeviceIsDefined(aDeviceHwnd As IntPtr, ByRef rControl As Control, ByRef rBackgroundOnly As Boolean, ByRef rSize As System.Drawing.Size) As Boolean
            rSize = New System.Drawing.Size(0, 0)
            Dim _rVal As Boolean
            rControl = Nothing
            rBackgroundOnly = False
            If aDeviceHwnd = IntPtr.Zero Then Return False
            Dim aCntrl As Control = Control.FromHandle(aDeviceHwnd)
            If aCntrl IsNot Nothing Then
                _rVal = dxfBitmap.ValidateControl(aCntrl, rControl, rBackgroundOnly)
                If _rVal Then
                    rSize = aCntrl.ClientSize
                End If
            End If
            Return _rVal
        End Function
        Public Shared Function BitmapToBytes(aBitmap As Bitmap, ByRef rBytes() As Byte) As Boolean
            Dim rBytesPerPixel As Integer = 0
            Return BitmapToBytes(aBitmap, rBytes, rBytesPerPixel)
        End Function
        Public Shared Function BitmapToBytes(aBitmap As Bitmap, ByRef rBytes() As Byte, ByRef rBytesPerPixel As Integer) As Boolean
            ReDim rBytes(0)
            rBytesPerPixel = 0
            Dim bWd As Integer
            Dim bHt As Integer
            Dim bmpData As Imaging.BitmapData = Nothing
            Dim g As Graphics
            Dim absStride As Integer
            Dim bytes As Integer
            Dim ptr As IntPtr
            Dim i As Integer
            Dim iStart As Integer
            Dim iEnd As Integer
            Dim iStep As Integer
            If aBitmap Is Nothing Then
                Return False
            Else
                Try
                    g = Graphics.FromImage(aBitmap)
                    bmpData = aBitmap.LockBits(New Rectangle(0, 0, aBitmap.Width, aBitmap.Height), Imaging.ImageLockMode.ReadWrite, aBitmap.PixelFormat)
                    bWd = aBitmap.Width
                    bHt = aBitmap.Height
                    rBytesPerPixel = (APIWrapper.GetDeviceCaps(g.GetHdc, Vanara.PInvoke.Gdi32.DeviceCap.BITSPIXEL) / 8) - 1
                    g.ReleaseHdc()
                    ' Lock the bitmap's bits.
                    absStride = Math.Abs(bmpData.Stride)
                    bytes = absStride * bHt
                    ReDim rBytes(bytes)
                    If bmpData.Stride > 0 Then
                        iStart = bHt - 1
                        iEnd = 0
                        iStep = -1
                    Else
                        iStart = 0
                        iEnd = bHt - 1
                        iStep = 1
                    End If
                    For i = iStart To iEnd Step iStep
                        ptr = New IntPtr(CType(bmpData.Scan0, Integer) + (bmpData.Stride * i))
                        Marshal.Copy(ptr, rBytes, absStride * (bHt - i - 1), absStride)
                    Next i
                    Return True
                Catch ex As Exception
                    ReDim rBytes(0)
                    rBytesPerPixel = 0
                    Return False
                Finally
                    If bmpData IsNot Nothing Then aBitmap.UnlockBits(bmpData)
                End Try
            End If
        End Function
        Public Shared Function CheckProperty(ByRef aObject As Object, aPropertyName As String) As Boolean
            Try
                If aObject Is Nothing Then Return False
                Dim type As Type = aObject.GetType
                Dim rVal1 As Reflection.PropertyInfo = type.GetProperty(aPropertyName)

                If rVal1 IsNot Nothing Then Return True
                Dim rVal2 As Reflection.MethodInfo = type.GetMethod(aPropertyName)
                Return rVal2 IsNot Nothing

            Catch ex As Exception
                Return False
            End Try
        End Function
        Public Shared Function FormatSeconds(aSeconds As Object, Optional bDecimalSeconds As Boolean = False) As String
            'On Error Resume Next
            Dim secs As Double = TVALUES.ToDouble(aSeconds)
            Dim mns As Double = Fix(secs / 60)
            secs -= (60 * mns)
            Dim _rVal As String = Format(mns, "0")
            If Not bDecimalSeconds Then
                _rVal += $":{Format(secs, "00")}"
            Else
                _rVal += $":{Format(secs, "00.00")}"
            End If
            Return _rVal
        End Function
        Public Shared Function Array_Rectangular(aStartPtXY As iVector, aColumnCount As Integer, aRowCount As Integer, aXPitch As Double, aYPitch As Double, Optional aPitchType As dxxPitchTypes = dxxPitchTypes.Rectangular, Optional aRotation As Double? = Nothing, Optional aMaxCount As Long = 0, Optional aPlane As dxfPlane = Nothing, Optional aCollector As colDXFVectors = Nothing) As colDXFVectors
            Dim _rVal As colDXFVectors
            '#1the point to start the array from
            '#2the number of columns for the array
            '#3the number of rows for the array
            '#4the distance between columns in the array
            '#5the distance between rows in the array
            '#6the type of pitch to apply
            '#7a rotation to apply
            '#8a limit to the number of points to add
            '#9 the plane to use for the X and Y Directions
            '^creates a rectangular array of vectors based on the passed information.
            '~the array is built to the right and down from the starting point. Negative pitches reverse the subject direction.
            If aCollector Is Nothing Then _rVal = New colDXFVectors Else _rVal = aCollector
            TVECTORS.Array(New TVECTOR(aStartPtXY), aColumnCount, aRowCount, aXPitch, aYPitch, aPitchType, aRotation, aMaxCount, aPlane, _rVal)
            Return _rVal
        End Function
        Public Shared Function SwapObjects(ByRef ioObject1 As Object, ByRef ioObject2 As Object, Optional aBooleanCondition As Boolean? = Nothing) As Boolean
            Dim _rVal As Boolean
            '#1the first object
            '#2the second object
            '#3a flag when evaluated as a boolean equals False will prevent the swap from being made
            '^swaps the two object references if the third argument is not passed or if it evaluates to True
            '~Returns True if the swap was made
            'On Error Resume Next
            If Not aBooleanCondition.HasValue Then
                _rVal = True
            Else
                _rVal = aBooleanCondition.Value
            End If
            If Not _rVal Then Return _rVal
            Dim aObj As Object = ioObject1
            ioObject1 = ioObject2
            ioObject2 = aObj
            Return _rVal
        End Function
        Public Shared Function RectangleAligmentToMTextAlignment(aRecAligment As dxxRectangularAlignments) As dxxMTextAlignments
            Select Case aRecAligment
                Case dxxRectangularAlignments.TopLeft
                    Return dxxMTextAlignments.TopLeft
                Case dxxRectangularAlignments.TopCenter
                    Return dxxMTextAlignments.TopCenter
                Case dxxRectangularAlignments.TopRight
                    Return dxxMTextAlignments.TopRight
                Case dxxRectangularAlignments.MiddleLeft
                    Return dxxMTextAlignments.MiddleLeft
                Case dxxRectangularAlignments.MiddleCenter
                    Return dxxMTextAlignments.MiddleCenter
                Case dxxRectangularAlignments.MiddleRight
                    Return dxxMTextAlignments.MiddleRight
                Case dxxRectangularAlignments.BottomLeft
                    Return dxxMTextAlignments.BottomLeft
                Case dxxRectangularAlignments.BottomCenter
                    Return dxxMTextAlignments.BottomCenter
                Case dxxRectangularAlignments.BottomRight
                    Return dxxMTextAlignments.BottomRight
                Case Else
                    Return dxxMTextAlignments.TopLeft
            End Select
        End Function
        Friend Shared Function VectorPropertyName(aPropEnum As dxxVectorProperties) As String
            Select Case aPropEnum
                Case dxxVectorProperties.Inverted
                    Return "Inverted"
                Case dxxVectorProperties.EndWidth
                    Return "EndWidth"
                Case dxxVectorProperties.StartWidth
                    Return "StartWidth"
                Case dxxVectorProperties.Flag
                    Return "Flag"
                Case dxxVectorProperties.Mark
                    Return "Marker"
                Case dxxVectorProperties.Radius
                    Return "Radius"
                Case dxxVectorProperties.Tag
                    Return "Tag"
                Case dxxVectorProperties.Value
                    Return "Value"
                Case dxxVectorProperties.Rotation
                    Return "Rotation"
                Case dxxVectorProperties.Color
                    Return "Color"
                Case dxxVectorProperties.LayerName
                    Return "LayerName"
                Case dxxVectorProperties.Linetype
                    Return "Linetype"
                Case dxxVectorProperties.LTScale
                    Return "LTScale"
                Case dxxVectorProperties.Suppressed
                    Return "Suppressed"
                Case dxxVectorProperties.Row
                    Return "Row"
                Case dxxVectorProperties.Col
                    Return "Col"
                Case dxxVectorProperties.X
                    Return "X"
                Case dxxVectorProperties.Y
                    Return "Y"
                Case dxxVectorProperties.Z
                    Return "Z"
                Case Else
                    Return String.Empty
            End Select
        End Function
        Public Shared Function AlignmentName(aAlignment As dxxRectangularAlignments) As String
            '^used to set the alignment by string
            Select Case aAlignment
                Case dxxRectangularAlignments.MiddleCenter
                    Return "MIDDLE CENTER"
                Case dxxRectangularAlignments.MiddleLeft
                    Return "MIDDLE LEFT"
                Case dxxRectangularAlignments.MiddleRight
                    Return "MIDDLE RIGHT"
                Case dxxRectangularAlignments.BottomCenter
                    Return "BOTTOM CENTER"
                Case dxxRectangularAlignments.BottomLeft
                    Return "BOTTOM LEFT"
                Case dxxRectangularAlignments.BottomRight
                    Return "BOTTOM RIGHT"
                Case dxxRectangularAlignments.TopCenter
                    Return "TOP CENTER"
                Case dxxRectangularAlignments.TopLeft
                    Return "TOP LEFT"
                Case dxxRectangularAlignments.TopRight
                    Return "TOP RIGHT"
                Case Else
                    Return String.Empty
            End Select
        End Function
        Public Shared Function AlignmentNameDecode(aAlgn As String) As dxxRectangularAlignments
            '^used to set the alignment by string
            If String.IsNullOrWhiteSpace(aAlgn) Then Return dxxRectangularAlignments.MiddleCenter
            aAlgn = aAlgn.Replace("_", " ").Trim
            aAlgn = aAlgn.Replace(" ", "").Trim.ToUpper
            If aAlgn.StartsWith("DXF") Then
                aAlgn = aAlgn.Substring(3, aAlgn.Length - 3)
            End If
            Select Case aAlgn
                Case "MIDDLE", "CENTER", "MIDDLECENTER"
                    Return dxxRectangularAlignments.MiddleCenter
                Case "MIDDLELEFT", "CENTERLEFT"
                    Return dxxRectangularAlignments.MiddleLeft
                Case "MIDDLERIGHT", "CENTERRIGHT"
                    Return dxxRectangularAlignments.MiddleRight
                Case "BOTTOM", "BOTTOMCENTER"
                    Return dxxRectangularAlignments.BottomCenter
                Case "BOTTOMLEFT", "CENTERLEFT"
                    Return dxxRectangularAlignments.BottomLeft
                Case "BOTTOMRIGHT", "CENTERRIGHT"
                    Return dxxRectangularAlignments.BottomRight
                Case "TOP", "TOPCENTER"
                    Return dxxRectangularAlignments.TopCenter
                Case "TOPLEFT", "CENTERLEFT"
                    Return dxxRectangularAlignments.TopLeft
                Case "TOPRIGHT", "CENTERRIGHT"
                    Return dxxRectangularAlignments.TopRight
                Case Else
                    Return dxxRectangularAlignments.General
            End Select
        End Function
        Friend Shared Function AngleBetweenLines(sp1 As TVECTOR, ep1 As TVECTOR, sp2 As TVECTOR, ep2 As TVECTOR, aNormal As TVECTOR) As Double
            '#1the start of the first line
            '#2the end of the first line
            '#3the start of the second line
            '#4the end of the second line
            '#3the normal (cross product of the two passed vectors)
            '^the angle between the two passedlines in degrees
            '~if the known normal is passed then the counter clockwise angle between the vectors is return (0 to 360)
            '~otherwise the angle is returned (0 to 180) and the normal is returned
            If sp1.DistanceTo(ep1) = 0 Then Return 0
            If sp2.DistanceTo(ep2) = 0 Then Return 0
            Return (ep1 - sp1).AngleTo(ep2 - sp2, aNormal)
        End Function
        Public Shared Function AngleBetweenPoints(aPoint1XY As iVector, aPoint2XY As iVector, Optional bInRadians As Boolean = False, Optional aCS As dxfPlane = Nothing) As Double
            Dim _rVal As Double
            '#1the first point to search
            '#2the second point to search
            '#3flag to indicate if the returned angle should be in degrees or radians
            '^returns the angle between the passed points
            '~equates to the angle of inclination of a line starting at the first point ending at the second.
            '~the returned angle is measured counterclockwise from the positive X axis.
            '~returned in degrees unless the InRadians argument is = True.
            Try
                If aPoint1XY Is Nothing And aPoint2XY Is Nothing Then Throw New Exception("Undefined Point Detected")
                Dim v1 As TVECTOR = New TVECTOR(aPoint1XY)
                Dim v2 As TVECTOR = New TVECTOR(aPoint2XY)
                Dim aDir As TVECTOR
                If v1.Equals(v2, 3) Then Throw New Exception("Passed Points Are Coincident")
                aDir = v1.DirectionTo(v2)
                If aCS IsNot Nothing Then
                    _rVal = aCS.XDirectionV.AngleTo(aDir, aCS.ZDirectionV)
                Else
                    _rVal = TVECTOR.WorldX.AngleTo(aDir, TVECTOR.WorldZ)
                End If
                If bInRadians Then _rVal *= Math.PI / 180
                Return _rVal
            Catch ex As Exception
                Throw New Exception($"dxfUtils.{ Reflection.MethodBase.GetCurrentMethod.Name} - { ex.Message}")
                Return 0
            End Try
        End Function
        Public Shared Function AngleBetweenVectorObjects(aVectorObj As iVector, bVectorObj As iVector) As Double
            Dim rNormal As dxfDirection = Nothing
            Return AngleBetweenVectorObjects(aVectorObj, bVectorObj, rNormal)
        End Function
        Public Shared Function AngleBetweenVectorObjects(aVectorObj As iVector, bVectorObj As iVector, ByRef rNormal As dxfDirection) As Double
            '^the angle between the two passed vector objects
            '~in degrees
            Try
                Dim _rVal As Double
                Dim aN As TVECTOR
                Dim v1 As New TVECTOR(aVectorObj)
                Dim v2 As New TVECTOR(bVectorObj)
                If rNormal IsNot Nothing Then aN = rNormal.Strukture
                _rVal = v1.AngleTo(v2, aN)
                rNormal = New dxfDirection(aN)
                Return _rVal
            Catch ex As Exception
                Throw New Exception($"dxfUtils.{ Reflection.MethodBase.GetCurrentMethod.Name} - { ex.Message}")
                Return 0
            End Try
        End Function
        Public Shared Function AngleBetweenDirections(aDirection As dxfVector, bDirection As dxfVector, ByRef rNormal As dxfDirection) As Double
            '^the angle between the two passed vector objects
            '~in degrees
            Try
                Dim _rVal As Double
                Dim aN As TVECTOR
                Dim v1 As New TVECTOR(aDirection)
                Dim v2 As New TVECTOR(bDirection)
                If rNormal IsNot Nothing Then aN = rNormal.Strukture
                _rVal = v1.AngleTo(v2, aN)
                rNormal = New dxfDirection(aN)
                Return _rVal
            Catch ex As Exception
                Throw New Exception($"dxfUtils.{ Reflection.MethodBase.GetCurrentMethod.Name} - { ex.Message}")
                Return 0
            End Try
        End Function
        Public Shared Function ArcArea(aRadius As Double, aSpannedAngle As Double, Optional bInRadians As Boolean = False) As Double
            Dim _rVal As Double
            '^the area of the arc
            Dim ang As Double
            Dim Span As Double
            Dim tot As Double
            tot = Math.PI * aRadius ^ 2
            Span = aSpannedAngle
            If bInRadians Then Span = Span * 180 / Math.PI
            Span = TVALUES.NormAng(Span, False, False, True)
            If Span = 0 Then
                Return _rVal
            ElseIf Span = 180 Then
                _rVal = tot / 2
            ElseIf Span = 360 Then
                _rVal = tot
            Else
                ang = Span * Math.PI / 180
                _rVal = ((2 * aRadius) ^ 2) / 8
                _rVal *= (ang - Math.Sin(ang))
                If Span > 180 Then _rVal = tot - _rVal
            End If
            Return _rVal
        End Function
        Public Shared Function ArcBetweenPoints(aRadius As Double, aSP As iVector, aEP As iVector, Optional bClockwise As Boolean? = Nothing, Optional aReturnLargerArc As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional bSuppressErrors As Boolean = False) As dxeArc
            '#1the radius of the arc to create
            '#2the first vector
            '#3the second vector
            '#4flag to invert the returned arc
            '^returns a upArc object starting at the first vector ending at the second vector with the requested radius
            '~an error is raised if the requested arc cannot be defined
            Try
                If aSP Is Nothing Then Throw New Exception("Invalid Start Pt Passed")
                If aEP Is Nothing Then Throw New Exception("Invalid End Pt Passed")
                Dim bFlag As Boolean
                Dim aPl As TPLANE = TPLANE.World
                If Not dxfPlane.IsNull(aPlane) Then
                    If Not aPl.ZDirection.Equals(aPlane.ZDirectionV, 4) Then
                        aPl.AlignTo(aPlane.ZDirectionV, dxxAxisDescriptors.Z)
                    End If
                    aPl.Origin = aPlane.OriginV
                End If
                Dim aArc As TSEGMENT = dxfPrimatives.ArcBetweenPointsV(aRadius, New TVECTOR(aSP), New TVECTOR(aEP), aPl, bClockwise, aReturnLargerArc, bSuppressErrors, bFlag, aDisplaySettings)
                If bFlag Then
                    Return New dxeArc(aArc)
                Else
                    Return Nothing
                End If
            Catch ex As Exception
                Throw New Exception($"dxfUtils.{ Reflection.MethodBase.GetCurrentMethod.Name} - { ex.Message}")
                Return Nothing
            End Try
        End Function
        Friend Shared Function ArcBetweenPoints(aRadius As Double, aSP As TVECTOR, aEP As TVECTOR, Optional bClockwise As Boolean? = Nothing, Optional aReturnLargerArc As Boolean = False, Optional bSuppressErrs As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxeArc
            '#1the radius of the arc to create
            '#2the first vector
            '#3the second vector
            '#4flag to invert the returned arc
            '^returns a upArc object starting at the first vector ending at the second vector with the requested radius
            '~an error is raised if the requested arc cannot be defined
            Try
                Dim bFlag As Boolean
                Dim aPl As New TPLANE("World")
                If Not dxfPlane.IsNull(aPlane) Then
                    If Not aPl.ZDirection.Equals(aPlane.ZDirectionV, 4) Then
                        aPl.AlignTo(aPlane.ZDirectionV, dxxAxisDescriptors.Z)
                    End If
                    aPl.Origin = aPlane.OriginV
                End If
                Dim aArc As TSEGMENT = dxfPrimatives.ArcBetweenPointsV(aRadius, aSP, aEP, aPl, bClockwise, aReturnLargerArc, False, bFlag, aDisplaySettings)
                If bFlag Then
                    Return New dxeArc(aArc)
                Else
                    Return Nothing
                End If
            Catch ex As Exception
                If Not bSuppressErrs Then Throw New Exception($"dxfUtils.{ Reflection.MethodBase.GetCurrentMethod.Name} - { ex.Message}")
                Return Nothing
            End Try
        End Function
        Public Shared Function ArcByBulge(aPoint As iVector, bPoint As iVector, aBulge As Double, Optional bSuppressErrs As Boolean = False, Optional aPlane As dxfPlane = Nothing) As dxeArc
            Dim rArcMidPt As dxfVector = Nothing
            Return ArcByBulge(aPoint, bPoint, aBulge, rArcMidPt, bSuppressErrs, aPlane)
        End Function
        Public Shared Function ArcByBulge(aPoint As iVector, bPoint As iVector, aBulge As Double, ByRef rArcMidPt As dxfVector, Optional bSuppressErrs As Boolean = False, Optional aPlane As dxfPlane = Nothing) As dxeArc
            '#1the first vector
            '#2the second vector
            '#3the bulge used to compute the arc
            '^returns a dxeArc passing through all three of the passed vectors
            '~no errors raised if the arc can't be defined.
            '~the first vector is assumed as the arc start vector and the last vector is assummed as the end.
            Dim mp As TVECTOR
            Dim _rValBase As TARC
            rArcMidPt = Nothing

            Try
                _rValBase = TARC.ByBulge(New TVECTOR(aPoint), New TVECTOR(bPoint), aBulge, mp, True, New TPLANE(aPlane))
                If _rValBase.Radius <= 0 Then
                    If Not bSuppressErrs Then Throw New Exception("ArcByBulge - Invalid Input Detected")

                Else

                    rArcMidPt = New dxfVector(mp)
                    Return New dxeArc(_rValBase)
                End If
            Catch ex As Exception

            End Try
            Return Nothing
        End Function

        Public Shared Function ArcByOctant(ByRef aStartPointXY As iVector, aRadius As Double, aStartingOctant As Integer, aOctantSpan As Integer, bClockwise As Boolean, Optional aStartOffset As Double = 0.0, Optional aEndOffset As Double = 0.0) As dxeArc
            '#1the first vector of the arc
            '#2the radius of the arc
            '#3the starting octant angle (0-7)
            '#4the ending octant angle (0-7)
            '#5flag to return an inverted arc
            '^returns a dxeArc defined using the passed octant info
            '~no errors raised if the arc can't be defined.
            '~the first vector is assumed as the arc start vector.
            Dim _rValBase As TARC

            Try
                _rValBase = TARC.ByOctant(New TVECTOR(aStartPointXY), aRadius, aStartingOctant, aOctantSpan, bClockwise, aStartOffset, aEndOffset, True)
                If _rValBase.Radius <= 0 Then
                    Throw New Exception("ArcByOctant - Invalid Input Detected")

                Else
                    Return New dxeArc(_rValBase)
                End If
            Catch ex As Exception
                Throw New Exception($"dxfUtils.{ Reflection.MethodBase.GetCurrentMethod.Name} - { ex.Message}")
            End Try
            Return Nothing
        End Function

        Friend Shared Function ArcDivide(aArc As TARC, aAngle As Double, Optional bNoWidth As Boolean = False) As TSEGMENTS
            Dim _rVal As New TSEGMENTS(0)
            '#1the angle to divide the arc by
            '#2the angle to divided the arc into
            '^returns a subset of arcs that are this arc divided by the passed angle

            Dim eang As Double

            Dim i As Integer

            Dim bArc As TARC
            Dim swd As Double = 0
            Dim ewd As Double = 0


            Dim bWidth As Boolean
            Dim bOneWidth As Boolean
            Dim bBigToSmall As Boolean
            Dim f1 As Double
            Dim stp1 As Double
            If Not bNoWidth Then
                swd = Math.Round(aArc.StartWidth, 6)
                ewd = Math.Round(aArc.EndWidth, 6)
                bWidth = swd > 0 Or ewd > 0
            End If

            aAngle = TVALUES.NormAng(aAngle, False, True)
            If aAngle = 0 Then aAngle = 360
            '.SpannedAngle = SpannedAngle(aArc.ClockWise, aArc.StartAngle, aArc.EndAngle)
            Dim Span As Double = aArc.SpannedAngle
            If aAngle = 360 Or aAngle >= Span Or Span = 0 Then
                bArc = TARC.ArcStructure(aArc.Plane, aArc.Plane.Origin, aArc.Radius, aArc.StartAngle, aArc.EndAngle, aArc.ClockWise, False, 0, False, swd, ewd, aArc.Identifier)
                _rVal.Add(bArc)
                Return _rVal
            End If
            Dim divs As Integer = Fix(Span / aAngle)
            If aArc.ClockWise Then f1 = -1 Else f1 = 1
            Dim sang As Double = aArc.StartAngle
            Dim remd As Double = Math.Round(Span - (divs * aAngle), 2)
            Dim pieces As Integer = divs
            If remd > 0 Then pieces += 1
            If swd > 0 Or ewd > 0 And pieces > 1 Then

                bOneWidth = swd = ewd
                bBigToSmall = swd >= ewd
            End If
            i = 1
            If Not bOneWidth Then
                stp1 = (ewd - swd) * (1 / pieces)
            End If
            Do While i <= divs
                eang = sang + aAngle * f1
                If Not bOneWidth Then
                    ewd = swd + stp1
                End If
                bArc = TARC.ArcStructure(aArc.Plane, aArc.Plane.Origin, aArc.Radius, sang, eang, aArc.ClockWise, False, 0, False, swd, ewd, aArc.Identifier)
                _rVal.Add(bArc)
                i += 1
                sang = eang
                If Not bOneWidth Then swd = ewd
            Loop
            If remd > 0 Then
                If Not bOneWidth Then ewd = swd + stp1
                sang = eang
                eang = sang + remd * f1
                bArc = TARC.ArcStructure(aArc.Plane, aArc.Plane.Origin, aArc.Radius, sang, eang, aArc.ClockWise, False, 0, False, swd, ewd, aArc.Identifier)
                _rVal.Add(bArc)
            End If
            Return _rVal
        End Function
        Friend Shared Function ArcLineMidPointV(aSegment As TSEGMENT, ByRef rLength As Double) As TVECTOR


            If aSegment.IsArc Then
                Dim aArc As TARC = aSegment.ArcStructure
                rLength = dxfMath.SpannedAngle(aArc.ClockWise, aArc.StartAngle, aArc.EndAngle)
                If Not aArc.Elliptical Then
                    If Not aArc.ClockWise Then
                        Return aArc.Plane.AngleVector(TVALUES.To_DBL(aArc.StartAngle + 0.5 * rLength), aArc.Radius, False)
                    Else
                        Return aArc.Plane.AngleVector(aArc.EndAngle + 0.5 * rLength, aArc.Radius, False)
                    End If
                Else
                    If Not aArc.ClockWise Then
                        Return EllipsePoint(aArc.Plane.Origin, 2 * aArc.Radius, 2 * aArc.MinorRadius, aArc.StartAngle + 0.5 * rLength, aArc.Plane)
                    Else
                        Return EllipsePoint(aArc.Plane.Origin, 2 * aArc.Radius, 2 * aArc.MinorRadius, aArc.EndAngle + 0.5 * rLength, aArc.Plane)
                    End If
                End If
            Else
                Dim aLine As TLINE = aSegment.LineStructure
                rLength = aLine.SPT.DistanceTo(aLine.EPT)
                Return aLine.SPT + aLine.Direction * 0.5 * rLength ' .Interpolate(aLine.EPT, 0.5)
            End If

        End Function
        Friend Shared Function ArcMidPoint(aCS As dxfPlane, aRadius As Double, aStartAngle As Double, aEndAngle As Double, bClockwise As Boolean) As TVECTOR


            Dim aPl As New TPLANE(aCS)

            Dim aSpan As Double = dxfMath.SpannedAngle(bClockwise, aStartAngle, aEndAngle)
            If Not bClockwise Then
                Return aPl.AngleVector(aStartAngle + 0.5 * aSpan, aRadius, False)
            Else
                Return aPl.AngleVector(aEndAngle + 0.5 * aSpan, aRadius, False)
            End If

        End Function
        Friend Shared Function ArcMidPointP(aPlane As TPLANE, aRadius As Double, aStartAngle As Double, aEndAngle As Double, bClockwise As Boolean) As TVECTOR
            Dim rSpan As Double = 0.0
            Return ArcMidPointP(aPlane, aRadius, aStartAngle, aEndAngle, bClockwise, rSpan)
        End Function
        Friend Shared Function ArcMidPointP(aPlane As TPLANE, aRadius As Double, aStartAngle As Double, aEndAngle As Double, bClockwise As Boolean, ByRef rSpan As Double) As TVECTOR
            rSpan = dxfMath.SpannedAngle(bClockwise, aStartAngle, aEndAngle)
            If Not bClockwise Then
                Return aPlane.AngleVector(aStartAngle + 0.5 * rSpan, aRadius, False)
            Else
                Return aPlane.AngleVector(aEndAngle + 0.5 * rSpan, aRadius, False)
            End If
        End Function

        Friend Shared Function ArcTangentLine(aArc As TARC, aAngle As Double, aLength As Double) As TLINE
            '#2the arc angle to generate a tangent line at
            '#3the length for the returned line
            '^returns a tangent line to the arc starting at a point on the arc at the passed angle
            '~a negative length will invert the returned lines direction
            Dim aDir As TVECTOR
            Dim bDir As TVECTOR
            Dim aPlane As TPLANE = aArc.Plane
            If aLength = 0 Then aLength = aArc.Radius
            aDir = aPlane.Direction(aAngle, False)
            If Not aArc.ClockWise Then
                bDir = aDir.RotatedAbout(aPlane.ZDirection, 90, False).Normalized
            Else
                bDir = aDir.RotatedAbout(aPlane.ZDirection, -90, False).Normalized
            End If
            Dim _rVal As New TLINE With {
                .SPT = aArc.Plane.Origin + (aDir * aArc.Radius)
            }
            _rVal.EPT = _rVal.SPT + (bDir * aLength)
            Return _rVal
        End Function
        Friend Shared Function ArcTangentLineE(aArc As dxeArc, aAngle As Double, aLength As Double) As dxeLine
            '#1the arc angle to generate a tangent line at
            '#2the length for the returned line
            '^returns a tangent line to the arc starting at a point on the arc at the passed angle
            '~a negative length will invert the returned lines direction
            If aArc Is Nothing Then Return Nothing
            Return New dxeLine With {
                    .LineStructure = ArcTangentLine(aArc.ArcStructure, aAngle, aLength),
                    .DisplayStructure = aArc.DisplayStructure,
                    .PlaneV = aArc.PlaneV
                }
        End Function
        Public Shared Function ArcThreePoint(aStartPtXY As iVector, aArcPtXY As iVector, aEndPtXY As iVector, Optional bSuppressErrs As Boolean = False, Optional aCS As dxfPlane = Nothing) As dxeArc
            Dim _rVal As dxeArc = Nothing
            '#1the first vector
            '#2the second vector
            '#3the third vector
            '^returns a dxeArc passing through all three of the passed vectors
            '~no errors raised if the arc can't be defined.
            '~the first vector is assumed as the arc start vector and the last vector is assummed as the end.
            Try
                If aStartPtXY Is Nothing Or aArcPtXY Is Nothing Or aEndPtXY Is Nothing Then Throw New Exception("Undefined Point Passed")
                Dim sp As New dxfVector(aStartPtXY)

                _rVal = dxfPrimatives.CreateArcThreePoint(New TVECTOR(aStartPtXY), New TVECTOR(aArcPtXY), New TVECTOR(aEndPtXY), False, aCS.Strukture)
                If _rVal IsNot Nothing Then
                    _rVal.CopyDisplayValues(sp.DisplaySettings)
                End If
                Return _rVal
            Catch ex As Exception
                If Not bSuppressErrs Then Throw New Exception($"dxfUtils.{ Reflection.MethodBase.GetCurrentMethod.Name} - { ex.Message}")
            End Try
            Return _rVal
        End Function
        Friend Shared Function Arc_Define(aArc As dxeArc, aCenter As iVector, aRadius As Double, aStartAngle As Double, aEndAngle As Double, Optional cClockwise As Boolean = False, Optional aCS As dxfPlane = Nothing) As Boolean
            Dim _rVal As Boolean
            '^a short hand way to define the properties of the arc
            If aArc Is Nothing Then aArc = New dxeArc
            Dim wuz As Boolean = aArc.SuppressEvents
            Dim rad As Double = aArc.Radius
            Dim cp As New TVECTOR(aArc)
            Dim sa As Double
            Dim ea As Double
            Dim aDir As TVECTOR
            Dim bDir As TVECTOR
            Dim bCS As dxfPlane

            aArc.SuppressEvents = True

            If aRadius <> 0 Then rad = Math.Abs(aRadius)

            If aStartAngle = aEndAngle Then aEndAngle = aStartAngle + 360
            If aCenter IsNot Nothing Then cp = New TVECTOR(aCenter)
            If aCS Is Nothing Then bCS = aArc.Plane Else bCS = aCS.Clone
            bCS.OriginV = cp
            aDir = bCS.XDirectionV
            bDir = aDir
            aDir.RotateAbout(bCS.OriginV, bCS.ZDirectionV, aStartAngle, False)
            bDir.RotateAbout(bCS.OriginV, bCS.ZDirectionV, aEndAngle, False)
            sa = bCS.XDirectionV.AngleTo(aDir, bCS.ZDirectionV)
            ea = bCS.XDirectionV.AngleTo(bDir, bCS.ZDirectionV)
            If ea = sa Then
                sa = 0
                ea = 360
            End If
            If Not aArc.Plane.IsEqual(bCS, True, True) Then _rVal = True
            aArc.Plane = bCS
            If Not aArc.CenterV.Equals(cp, 5) Then _rVal = True
            aArc.CenterV = cp
            If aArc.Radius <> rad Then _rVal = True
            If aArc.ClockWise <> cClockwise Then _rVal = True
            If aArc.StartAngle <> sa Then _rVal = True
            If aArc.EndAngle <> ea Then _rVal = True
            aArc.Radius = rad
            aArc.ClockWise = cClockwise
            aArc.StartAngle = sa
            aArc.EndAngle = ea
            aArc.SuppressEvents = wuz
            Return _rVal
        End Function
        Friend Shared Function Arc_DefineWithPoints(aArc As dxeArc, aCenterPtXY As iVector, aStartPointXY As iVector, aEndPointXY As iVector, Optional aClockwise As Boolean = False, Optional aCS As dxfPlane = Nothing) As Boolean
            Dim _rVal As Boolean
            If aArc Is Nothing Then aArc = New dxeArc
            If aCenterPtXY Is Nothing Then Throw New Exception("Invalid Center Point Passed")
            If aStartPointXY Is Nothing Then Throw New Exception("Invalid Start Point Passed")
            If aEndPointXY Is Nothing Then Throw New Exception("Invalid End Point Passed")
            '#1the center vector of the arc
            '#2the starting vector of the arc
            '#3the ending vector of the arc
            '#4flag to toggle between the two possible arcs that can be defined by the passed vectors
            '^a shorthand way to set the properties of a arc object
            Dim cp As TVECTOR
            Dim sp As TVECTOR
            Dim ep As TVECTOR
            '**UNUSED VAR** Dim swap As Single
            Dim aPl As dxfPlane = Nothing
            Dim wuz As Boolean
            Dim bCS As dxfPlane
            wuz = aArc.SuppressEvents
            aArc.SuppressEvents = True
            If dxfPlane.IsNull(aCS) Then bCS = aArc.Plane Else bCS = New dxfPlane(aCS)
            cp = New TVECTOR(aPl, aCenterPtXY)
            bCS.OriginV = cp
            aPl = bCS
            sp = New TVECTOR(aPl, aStartPointXY)
            ep = New TVECTOR(aPl, aEndPointXY)
            aArc.PlaneV = bCS.Strukture
            If Not aArc.CenterV.Equals(cp, 5) Then _rVal = True
            aArc.CenterV = cp
            If aArc.Radius <> (cp.DistanceTo(sp) + cp.DistanceTo(ep)) / 2 Then _rVal = True
            If aArc.ClockWise <> aClockwise Then _rVal = True
            If aArc.StartAngle <> aArc.XDirectionV.AngleTo(cp.DirectionTo(sp), aArc.ZDirectionV) Then _rVal = True
            If aArc.EndAngle <> aArc.XDirectionV.AngleTo(cp.DirectionTo(ep), aArc.ZDirectionV) Then _rVal = True
            aArc.Radius = (cp.DistanceTo(sp) + cp.DistanceTo(ep)) / 2
            aArc.StartAngle = aArc.PlaneV.XDirection.AngleTo(cp.DirectionTo(sp), aArc.PlaneV.ZDirection)
            aArc.EndAngle = aArc.PlaneV.XDirection.AngleTo(cp.DirectionTo(ep), aArc.PlaneV.ZDirection)
            aArc.ClockWise = aClockwise
            aArc.SuppressEvents = wuz
            Return _rVal
        End Function
        Friend Shared Function ArcsBetweenPoints(aRadius As Double, aSP As TVECTOR, aEP As TVECTOR, aPlane As TPLANE, Optional aClockwise As Boolean? = Nothing, Optional bReturnLargerArcs As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As List(Of dxeArc)
            '#1the radius of the arcs to create
            '#2the first vector
            '#3the second vector
            '#4the arc plane
            '#5flag to return clockwise arcs otherwise counterclockwise arcs are returned
            '#6flag to return the longer of the possible arcs
            '^returns all the posibble arcs bewteen the two passed vectors with the passed radius
            '~an error is raised if the requested arcs cannot be defined
            Dim _rVal As New List(Of dxeArc)

            Try
                Dim arcs As TSEGMENTS = dxfPrimatives.ArcsBetweenPoints(aRadius, aSP, aEP, aPlane, aClockwise, bReturnLargerArcs, aDisplaySettings)
                For i As Integer = 1 To arcs.Count

                    _rVal.Add(New dxeArc(arcs.Item(i)))
                Next
                Return _rVal
            Catch ex As Exception
                Throw New Exception($"dxfUtils.{ Reflection.MethodBase.GetCurrentMethod.Name} - { ex.Message}")
                Return _rVal
            End Try
        End Function
        Public Shared Function ArcsTangentToTwoArcs(arc1 As dxeArc, ByRef Arc2 As dxeArc, aRadius As Double) As List(Of dxeArc)
            Dim _rVal As New List(Of dxeArc)
            '#1the first of two curves
            '#2the second of two curves
            '#3the radius of the tangent arcs to create
            '^used to find the arcs with the passed radius that can be draw between the two passed curves and are tangent to the passed curves
            '~returns a collection of uopArcs.
            '~returns an empty collection if the arc(s) can't be defined.
            '~error raised if the passed arguments are not valid.

            Try
                Dim rad As Double
                If arc1 Is Nothing Or Arc2 Is Nothing Then Throw New Exception("Undefined Curve Detected")
                rad = Math.Round(Math.Abs(aRadius), 5)
                If rad <= 0 Then Throw New Exception("Invalid Radius Detected")
                If Not TPLANES.Compare(arc1.Plane.Strukture, Arc2.Plane.Strukture, bCompareOrigin:=False, bCompareRotation:=False) Then Throw New Exception("Arcs Must Lie On the Same Plane")
                '**UNUSED VAR** Dim iPts As colDXFVectors
                Dim ar1 As dxeArc
                Dim Span As Double
                Dim cp As TVECTOR
                Dim R1 As Double
                Dim R2 As Double
                Dim Factor As Double
                Dim denom As Double
                Dim A As Double
                Dim B As Double
                Dim C As Double
                Dim numer As Double
                Dim aDir As TVECTOR
                Dim ang1 As Double
                Dim l1 As New TLINE(arc1.CenterV, Arc2.CenterV, False)
                cp = l1.SPT.Interpolate(l1.EPT, 0.5)
                R1 = arc1.Radius
                R2 = Arc2.Radius
                Span = Math.Round(l1.SPT.DistanceTo(l1.EPT) - R1 - R2, 5)
                If Span > rad Then
                    ''GoTo NoSolution
                ElseIf Span = rad Then
                    ar1 = New dxeArc With {
                    .SuppressEvents = True,
                    .PlaneV = arc1.PlaneV,
                    .DisplayStructure = arc1.DisplayStructure,
                    .CenterV = cp,
                    .Radius = Span / 2}
                    ar1.SuppressEvents = False
                    _rVal.Add(ar1)
                Else
                    'using the law of cosines
                    C = l1.SPT.DistanceTo(l1.EPT)
                    A = R1 + rad
                    B = R2 + rad
                    numer = B ^ 2 - A ^ 2 - C ^ 2
                    denom = -2 * A * Span
                    If denom <> 0 Then
                        Factor = numer / denom
                        ang1 = dxfMath.ArcCosine(Factor, True) 'in degrees
                        aDir = l1.SPT.DirectionTo(l1.EPT)
                        aDir.RotateAbout(arc1.PlaneV.ZDirection, ang1, False, True)
                        ar1 = New dxeArc With {
                        .SuppressEvents = True,
                        .PlaneV = arc1.PlaneV,
                        .DisplayStructure = arc1.DisplayStructure,
                        .CenterV = arc1.CenterV + (aDir * A),
                        .Radius = aRadius}
                        ar1.SuppressEvents = False
                        _rVal.Add(ar1)
                        aDir.RotateAbout(arc1.PlaneV.ZDirection, -2 * ang1, False, True)
                        ar1 = New dxeArc With {
                        .SuppressEvents = True,
                        .PlaneV = arc1.PlaneV,
                        .DisplayStructure = arc1.DisplayStructure,
                        .CenterV = arc1.CenterV + (aDir * A),
                        .Radius = aRadius}
                        ar1.SuppressEvents = False
                        _rVal.Add(ar1)
                    End If
                End If
            Catch ex As Exception
                Throw New Exception("ArcsTangentToTwoArcs - " & ex.Message)
            End Try
            Return _rVal
        End Function
        Public Shared Function ArcsTangentToTwoLines(aRadius As Double, aLine As dxeLine, bLine As dxeLine, Optional aSelectionPoint As dxfVector = Nothing, Optional bReturnTouchersOnly As Boolean = False, Optional bTrimToLines As Boolean = False) As List(Of dxeArc)

            Dim _rVal As New List(Of dxeArc)
            '#2the first of two lines
            '#3the second of two lines
            '#4a point to tell the function which side of the lines to use
            '#5flag to return only the arcs whose tangent points line on both of the passed lines
            '#6flag to return the arcs trimmed to their tangent points (full circles returned by default)
            '^used to create an arc tangent to the two passed lines
            '~error raised if the passed arguments are not valid.
            Try

                If aRadius <= 0 Then Throw New Exception("Invalid Radius Detected")
                If aLine Is Nothing Or bLine Is Nothing Then Throw New Exception("Undefined Line Detected")
                If aLine.Length <= 0 Or bLine.Length <= 0 Then Throw New Exception("Undefined Line Detected")
                Dim v1 As TVECTOR
                Dim ip As TVECTOR
                Dim dDirs(0 To 1) As TVECTOR
                Dim d1 As Double
                Dim aPl As New TPLANE("")
                Dim cp As TVECTOR
                Dim aArc As New TARC
                Dim bArc As New TARC
                Dim cArc As New TARC
                Dim dArc As New TARC
                Dim l1 As New TLINE(aLine)
                Dim l2 As New TLINE(bLine)
                Dim aDir As TVECTOR = TVECTOR.Zero
                Dim ctrs As TVECTORS
                Dim idx As Integer


                dDirs(0) = l1.SPT.DirectionTo(l1.EPT)
                dDirs(1) = l2.SPT.DirectionTo(l2.EPT)
                If dDirs(0).Equals(dDirs(1), True, 3) Then 'parallel lines
                    If aSelectionPoint Is Nothing Then v1 = aLine.MidPtV Else v1 = aSelectionPoint.Strukture.ProjectedTo(l1)
                    v1 = dxfProjections.ToLine(v1, l2, aDir, d1)
                    'the distance between the line must me equal to the diameter
                    If Math.Round(d1 / 2, 3) <> Math.Round(aRadius, 3) Then Return _rVal
                    cp = v1 + aDir * (0.5 * d1)
                    aPl = aPl.ReDefined(cp, dDirs(1), aDir)
                    aArc = TARC.ArcStructure(aPl, cp, aRadius, 0, 360, False)
                    _rVal.Add(New dxeArc(aArc))
                Else
                    ip = l1.IntersectionPt(l2)
                    aArc = dxfPrimatives.CreateFilletArc(aRadius, l1.EPT, ip, l2.EPT, aPl, bReturnCircle:=Not bTrimToLines)
                    bArc = dxfPrimatives.CreateFilletArc(aRadius, l2.SPT, ip, l1.EPT, aPl, bReturnCircle:=Not bTrimToLines)
                    cArc = dxfPrimatives.CreateFilletArc(aRadius, l2.SPT, ip, l1.SPT, aPl, bReturnCircle:=Not bTrimToLines)
                    dArc = dxfPrimatives.CreateFilletArc(aRadius, l1.SPT, ip, l2.EPT, aPl, bReturnCircle:=Not bTrimToLines)
                    If aSelectionPoint Is Nothing Then
                        If aArc.Radius > 0 Then _rVal.Add(New dxeArc(aArc))
                        If bArc.Radius > 0 Then _rVal.Add(New dxeArc(bArc))
                        If cArc.Radius > 0 Then _rVal.Add(New dxeArc(cArc))
                        If dArc.Radius > 0 Then _rVal.Add(New dxeArc(dArc))
                    Else
                        'return the arc whose center is nearest to the passed point
                        ctrs = New TVECTORS
                        ctrs.Add(aArc.Plane.Origin)
                        ctrs.Add(bArc.Plane.Origin)
                        ctrs.Add(cArc.Plane.Origin)
                        ctrs.Add(dArc.Plane.Origin)
                        ctrs.Nearest(aSelectionPoint.Strukture, TVECTOR.Zero, dxxLineDescripts.Normal, d1, idx)
                        If idx = 0 Then
                            _rVal.Add(New dxeArc(aArc))
                        ElseIf idx = 1 Then
                            _rVal.Add(New dxeArc(bArc))
                        ElseIf idx = 2 Then
                            _rVal.Add(New dxeArc(cArc))
                        ElseIf idx = 3 Then
                            _rVal.Add(New dxeArc(dArc))
                        End If
                    End If
                End If
            Catch ex As Exception
                Throw New Exception("ArcsTangentToTwoLines - " & ex.Message)
            End Try
            Return _rVal
        End Function
        Public Shared Function BreakArc(aArc As dxeArc, aBreaker As Object, bBreakersAreInfinite As Boolean, rWasBroken As Boolean) As colDXFEntities
            '^returns the arc broken into parts at the intersection of its self and the passed segment or segments
            Return dxfBreakTrimExtend.break_Arc(aArc, aBreaker, bBreakersAreInfinite, rWasBroken)
        End Function
        Public Shared Function BreakLine(aLine As dxeLine, aBreaker As Object, Optional bBreakersAreInfinite As Boolean = False, Optional bWasBroken As Boolean = False) As colDXFEntities
            '^returns the line broken into parts at the intersection of its self and the passed segment or segments
            Return dxfBreakTrimExtend.break_Line(aLine, aBreaker, bBreakersAreInfinite, bWasBroken)
        End Function
        Public Shared Function BreakLinescAndArcs(aEnts As IEnumerable(Of dxfEntity), aBreaker As Object, Optional bBreakersAreInfinite As Boolean = False, Optional bBreakSegsWereBroken As Boolean = False, Optional aIntersects As colDXFVectors = Nothing) As colDXFEntities
            Return dxfBreakTrimExtend.break_Segments(aEnts, aBreaker, bBreakersAreInfinite, bBreakSegsWereBroken, aIntersects)
        End Function
        Public Shared Function CADTextToScreenText(txt As String) As String
            '#1the string to convert to screen text
            '^used to replace AutoCAD "%%" character codes found in the passed string to their ascii symbol equivalent
            Return TFONT.CADTextToScreenText(txt)
        End Function
        Friend Shared Sub ClearHandles(aCollection As Collection, Optional aEntToSkip As dxfEntity = Nothing)
            '^resets the handle of all the members and thier sub entities
            If aCollection Is Nothing Then Return
            Dim i As Long
            Dim aMem As dxfEntity
            Dim aA As dxeArc
            Dim aB As dxeBezier
            Dim aD As dxeDimension
            Dim aE As dxeEllipse
            Dim aH As dxeHatch
            Dim aHl As dxeHole
            Dim aI As dxeInsert
            Dim aL As dxeLeader
            Dim aLn As dxeLine
            Dim aP As dxePoint
            Dim aPg As dxePolygon
            Dim aPl As dxePolyline
            Dim aSl As dxeSolid
            Dim aSy As dxeSymbol
            Dim aTB As dxeTable
            Dim aTx As dxeText
            For i = 1 To aCollection.Count
                aMem = aCollection.Item(i)
                If aEntToSkip Is Nothing Or (aEntToSkip IsNot Nothing And aMem IsNot aEntToSkip) Then
                    Select Case aMem.GraphicType
                        Case dxxGraphicTypes.Arc
                            aA = aMem
                            aA.ClearHandles()
                        Case dxxGraphicTypes.Bezier
                            aB = aMem
                            aB.ClearHandles()
                        Case dxxGraphicTypes.Dimension
                            aD = aMem
                            aD.ClearHandles()
                        Case dxxGraphicTypes.Ellipse
                            aE = aMem
                            aE.ClearHandles()
                        Case dxxGraphicTypes.Hatch
                            aH = aMem
                            aH.ClearHandles()
                        Case dxxGraphicTypes.Hole
                            aHl = aMem
                            aHl.ClearHandles()
                        Case dxxGraphicTypes.Insert
                            aI = aMem
                            aI.ClearHandles()
                        Case dxxGraphicTypes.Leader
                            aL = aMem
                            aL.ClearHandles()
                        Case dxxGraphicTypes.Line
                            aLn = aMem
                            aLn.ClearHandles()
                        Case dxxGraphicTypes.Point
                            aP = aMem
                            aP.ClearHandles()
                        Case dxxGraphicTypes.Polygon
                            aPg = aMem
                            aPg.ClearHandles()
                        Case dxxGraphicTypes.Polyline
                            aPl = aMem
                            aPl.ClearHandles()
                        Case dxxGraphicTypes.Solid
                            aSl = aMem
                            aSl.ClearHandles()
                        Case dxxGraphicTypes.Symbol
                            aSy = aMem
                            aSy.ClearHandles()
                        Case dxxGraphicTypes.Table
                            aTB = aMem
                            aTB.ClearHandles()
                        Case dxxGraphicTypes.Text
                            aTx = aMem
                            aTx.ClearHandles()
                    End Select
                End If
            Next i
        End Sub

        Public Shared Function PointsToBeziers(aVectors As colDXFVectors, Optional bContiguous As Boolean = True, Optional aPlane As dxfPlane = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As List(Of dxeBezier)
            Dim _rVal As New List(Of dxeBezier)
            If aVectors Is Nothing Then Return _rVal
            If aVectors.Count < 4 Then Return _rVal
            Dim cnt As Integer = aVectors.Count
            Dim aPl As TPLANE = IIf(dxfPlane.IsNull(aPlane), TPLANE.World, New TPLANE(aPlane))
            Dim aBz As dxeBezier

            For i As Integer = 1 To cnt
                If i + 3 > cnt Then Exit For
                aBz = New dxeBezier(aVectors.ItemVector(i), aVectors.ItemVector(i + 1), aVectors.ItemVector(i + 2), aVectors.ItemVector(i + 3), aPl, aDisplaySettings)

                _rVal.Add(aBz)
                If bContiguous Then i += 2 Else i += 3
            Next i
            Return _rVal
        End Function
        Friend Shared Function ComputeFraction(aValue As Double, aPrecis As Integer, ByRef rNumerator As String, ByRef rDenominator As String, bLeastCommonDenom As Boolean, ByRef rLeastDenom As Double) As Boolean
            '#1the number to get the fraction for (only the decimal part is used)
            '#2the precision for the fraction 1 to 8 (1 = 1/2  8 = 1/256)
            '#3returns tthe numerator of the fraction
            '#4returns the denominator of the fraction
            '#5flag to reduce to the lowest denominator (i.e. 4/8 = 1/2)
            '^computes the numerator and denominator of the passed number based on the input.
            '~returns True if the numerator is greater than zero
            rNumerator = "0"
            rDenominator = "0"
            rLeastDenom = 0
            aPrecis = TVALUES.LimitedValue(aPrecis, 1, 15)
            aValue -= Fix(aValue)
            If aValue = 0 Then Return False
            Dim aDivisor As Double
            Dim bDivisor As Double
            Dim i As Long
            Dim iParts As Integer
            Dim ip As Integer
            Dim aD As Double
            Dim bValue As Double
            Dim cValue As Double
            aDivisor = 2
            For i = 2 To aPrecis
                aDivisor *= 2
            Next i
            bDivisor = 1 / aDivisor
            bValue = aValue / bDivisor
            iParts = Fix(bValue)
            cValue = aValue - iParts * bDivisor
            If cValue > 0.5 * bDivisor Then iParts += 1
            If iParts <> 0 Then
                ip = iParts
                aD = aDivisor
                rLeastDenom = 1
                Do While (ip Mod 2) = 0
                    rLeastDenom *= 2
                    ip /= 2
                    aD /= 2
                Loop
                If bLeastCommonDenom Then
                    aDivisor = aD
                    iParts = ip
                End If
            End If
            rNumerator = iParts.ToString()
            rDenominator = aDivisor.ToString()
            Return rNumerator <> "0"
        End Function

        Friend Shared Function ConvertPlotScale(aScale As dxxPlotStandardScales) As Double
            Dim _rVal As Double
            Dim rVal As Double
            Dim ftfctr As Double
            ftfctr = 1 / 12
            rVal = 1
            Select Case aScale
                Case dxxPlotStandardScales.pltS_ScaledToFit
                    rVal = 1
                Case dxxPlotStandardScales.pltS_1_128_f
                    rVal = (1 / 128) * ftfctr
                Case dxxPlotStandardScales.pltS_1_64_f
                    rVal = (1 / 64) * ftfctr
                Case dxxPlotStandardScales.pltS_1_32_f
                    rVal = (1 / 32) * ftfctr
                Case dxxPlotStandardScales.pltS_1_16_f
                    rVal = (1 / 16) * ftfctr
                Case dxxPlotStandardScales.pltS_3_32_f
                    rVal = (3 / 32) * ftfctr
                Case dxxPlotStandardScales.pltS_1_8_f
                    rVal = (1 / 8) * ftfctr
                Case dxxPlotStandardScales.pltS_3_16_f
                    rVal = (3 / 16) * ftfctr
                Case dxxPlotStandardScales.pltS_1_4_f
                    rVal = (1 / 4) * ftfctr
                Case dxxPlotStandardScales.pltS_3_8_f
                    rVal = (3 / 8) * ftfctr
                Case dxxPlotStandardScales.pltS_1_2_f
                    rVal = (1 / 2) * ftfctr
                Case dxxPlotStandardScales.pltS_3_4_f
                    rVal = (3 / 4) * ftfctr
                Case dxxPlotStandardScales.pltS_1_f
                    rVal = ftfctr
                Case dxxPlotStandardScales.pltS_3_f
                    rVal = 3 * ftfctr
                Case dxxPlotStandardScales.pltS_6_f
                    rVal = 6 * ftfctr
                Case dxxPlotStandardScales.pltS_12_f
                    rVal = 12 * ftfctr
                Case dxxPlotStandardScales.pltS_1to1
                    rVal = 1
                Case dxxPlotStandardScales.pltS_1to2
                    rVal = 1 / 2
                Case dxxPlotStandardScales.pltS_1to4
                    rVal = 1 / 4
                Case dxxPlotStandardScales.pltS_1to8
                    rVal = 1 / 8
                Case dxxPlotStandardScales.pltS_1to10
                    rVal = 1 / 10
                Case dxxPlotStandardScales.pltS_1to16
                    rVal = 1 / 16
                Case dxxPlotStandardScales.pltS_1to20
                    rVal = 1 / 20
                Case dxxPlotStandardScales.pltS_1to30
                    rVal = 1 / 30
                Case dxxPlotStandardScales.pltS_1to40
                    rVal = 1 / 40
                Case dxxPlotStandardScales.pltS_1to50
                    rVal = 1 / 50
                Case dxxPlotStandardScales.pltS_1to100
                    rVal = 1 / 100
                Case dxxPlotStandardScales.pltS_2to1
                    rVal = 2
                Case dxxPlotStandardScales.pltS_4to1
                    rVal = 4
                Case dxxPlotStandardScales.pltS_8to1
                    rVal = 8
                Case dxxPlotStandardScales.pltS_10to1
                    rVal = 10
                Case dxxPlotStandardScales.pltS_100to1
                    rVal = 100
                Case dxxPlotStandardScales.pltS_1000to1
                    rVal = 1000
            End Select
            _rVal = rVal
            Return _rVal
        End Function
        Public Shared Function CopyCollection(aColToCopy As Collection, Optional bReturnEmpty As Boolean = False) As Collection
            Dim _rVal As Collection = Nothing
            '#1the collection to copy from
            '^returns a copy of the passed collection
            If bReturnEmpty Then _rVal = New Collection
            If aColToCopy Is Nothing Then Return _rVal
            'On Error Resume Next
            Dim valu As Object
            Dim i As Long
            Dim obj As Object
            _rVal = New Collection
            For i = 1 To aColToCopy.Count
                If aColToCopy.Item(i) IsNot Nothing Then
                    valu = aColToCopy.Item(i)
                    _rVal.Add(valu)
                Else
                    obj = aColToCopy.Item(i)
                    _rVal.Add(obj)
                End If
            Next i
            Return _rVal
        End Function
        Public Shared Function CreateDirection(Optional aX As Double = 0.0, Optional aY As Double = 0.0, Optional aZ As Double = 0.0, Optional aCS As dxfPlane = Nothing) As dxfDirection
            Dim _rVal As New dxfDirection
            If aCS Is Nothing Then
                _rVal.SetComponents(aX, aY, aZ)
            Else
                Dim v1 As TVECTOR
                v1 = New TPLANE(aCS).Vector(aX, aY, aZ)
                _rVal.SetComponents(v1.X, v1.Y, v1.Z)
            End If
            Return _rVal
        End Function
        Public Shared Function CreateDirectionAngular(Optional aAngle As Double = 0.0, Optional bInRadians As Boolean = False, Optional aPlane As dxfPlane = Nothing) As dxfDirection
            Dim aPl As New TPLANE(aPlane)
            Return New dxfDirection(aPl.Direction(aAngle, bInRadians))
        End Function
        Public Shared Function CreateGUID() As String
            Return System.Guid.NewGuid.ToString
        End Function
        Public Shared Function CreateHolesByString(aHole As dxeHole, Centers As colDXFVectors) As colDXFEntities
            Dim rCenterDescriptors As String = String.Empty
            Return CreateHolesByString(aHole, Centers, rCenterDescriptors)
        End Function
        Public Shared Function CreateHolesByString(aHole As dxeHole, Centers As colDXFVectors, ByRef rCenterDescriptors As String) As colDXFEntities
            rCenterDescriptors = ""
            Dim _rVal As New colDXFEntities
            '#1a hole (slot or hole) to create the members of the collection with
            '#2a collection of points to place the holes on
            '#3a descriptor string that defines a collection of points to place the holes on
            '^used to create a collection of holes by passign a hole and the centers to place the holes on
            If aHole Is Nothing Then Return _rVal
            If Centers Is Nothing And rCenterDescriptors = "" Then Return _rVal
            Dim cPoints As colDXFVectors
            Dim v1 As dxfVector
            Dim i As Integer
            Dim bHole As dxeHole
            bHole = aHole.Clone
            If Centers IsNot Nothing Then
                For i = 1 To Centers.Count
                    v1 = Centers.Item(i)
                    bHole.CenterV = v1.Strukture
                    _rVal.Add(bHole, bAddClone:=True)
                Next i
            End If
            If rCenterDescriptors <> "" Then
                cPoints = New colDXFVectors(rCenterDescriptors)
                For i = 1 To cPoints.Count
                    v1 = cPoints.Item(i)
                    bHole.CenterV = v1.Strukture
                    _rVal.Add(bHole, bAddClone:=True)
                Next i
            End If
            Return _rVal
        End Function

        Public Shared Function CreatePlane(aType As dxxStandardPlanes, Optional aBasePlane As dxfPlane = Nothing, Optional aOrigin As dxfVector = Nothing, Optional aRotation As Double = 0.0) As dxfPlane
            Dim aPln As dxfPlane
            If aBasePlane IsNot Nothing Then aPln = aBasePlane.Clone Else aPln = New dxfPlane
            Return New dxfPlane(aPln.Strukture.StandardPlane(aType, aOrigin, aRotation))
        End Function
        Public Shared Function CreateStackedText(ByRef aNumerator As Object, ByRef aDenominator As Object, Optional aStackStyle As dxxCharacterStackStyles = dxxCharacterStackStyles.Horizontal, Optional aCharAlignment As dxxCharacterAlignments = dxxCharacterAlignments.Undefined, Optional aHeightMultiplier As Double = 0.0) As String
            Dim _rVal As String = String.Empty
            '#1the text to put on top
            Dim aStr As String = IIf(aNumerator Is Nothing, "", aNumerator.ToString())
            Dim bStr As String = IIf(aDenominator Is Nothing, "", aDenominator.ToString())
            If aStackStyle < 1 Then aStackStyle = dxxCharacterStackStyles.Horizontal
            If aStackStyle > 3 Then aStackStyle = dxxCharacterStackStyles.Tolerance
            If aStackStyle = dxxCharacterStackStyles.Horizontal Then
                _rVal = $"\S{ aStr }/{ bStr};"
            ElseIf aStackStyle = dxxCharacterStackStyles.Diagonal Then
                _rVal = $"\S{ aStr}#{ bStr};"
            ElseIf aStackStyle = dxxCharacterStackStyles.Tolerance Then
                _rVal = $"\S{ aStr}^ { bStr};"
            End If
            If aHeightMultiplier > 0 Then
                _rVal = "{\H" + $"{aHeightMultiplier:0.000}x;{ _rVal}" + "}"
            End If
            If aCharAlignment >= dxxCharacterAlignments.Bottom Then
                _rVal = "{\A" + $"{aCharAlignment};{_rVal}" + "}"
            End If
            Return _rVal
        End Function

        Public Shared Function DecodeAlignments(ByRef vAlign As dxxTextJustificationsVertical, ByRef hAlign As dxxTextJustificationsHorizontal) As dxxMTextAlignments
            '^returns DXF mtext alignment based on the passed vertical and horizontal alignment codes
            Return TFONT.DecodeAlignment(vAlign, hAlign)
        End Function
        Public Shared Function DateToJulian(vDate As Date) As String
            Dim _rVal As String = 2415021 + TVALUES.To_INT(DateDiff("d", "01/01/1900", vDate))
            Dim tms As String = Format(vDate, "hh:mm:ss")
            Dim tVals() As String = tms.Split(":")
            Dim dfrac As Double = 3600 * TVALUES.To_DBL(tVals(0))
            dfrac += 60 * TVALUES.To_DBL(tVals(1))
            dfrac += TVALUES.To_DBL(tVals(2))
            dfrac /= 86400
            _rVal += Format(dfrac, "#.00000000")
            Return _rVal
        End Function

        Public Shared Function StripParens(aString As Object, Optional aLeftParen As Char = "(", Optional aRightParen As Char = ")") As String
            If aString Is Nothing Then Return String.Empty
            Dim _rVal As String = aString.ToString().Trim()
            If _rVal.Length = 0 Then Return _rVal

            Do While aString.StartsWith(aLeftParen)
                _rVal = _rVal.Substring(0, _rVal.Length - 1).Trim()

            Loop
            Do While _rVal.StartsWith(aRightParen)
                _rVal = _rVal.Substring(1, _rVal.Length - 1).Trim()

            Loop
            Return _rVal
        End Function
        Public Shared Function LeftOf(ByRef ioString As String, aChar As Char, Optional bTrim As Boolean = True, Optional bFromEnd As Boolean = False, Optional bRemove As Boolean = False) As String
            If ioString Is Nothing Then Return String.Empty
            If bTrim Then ioString.Trim()
            Dim i As Integer
            If Not bFromEnd Then
                i = ioString.IndexOf(aChar)
            Else
                i = ioString.LastIndexOf(aChar)
            End If
            Dim _rVal As String = String.Empty
            If i >= 0 Then
                _rVal = ioString.Substring(0, i)
                If bRemove Then
                    ioString = ioString.Substring(i + 1, ioString.Length - (i + 1))
                    If bTrim Then ioString = ioString.Trim()
                End If

            End If

            If bTrim Then Return _rVal.Trim() Else Return _rVal
        End Function
        Public Shared Function RightOf(ByRef ioString As String, aChar As Char, Optional bTrim As Boolean = True, Optional bFromEnd As Boolean = False, Optional bRemove As Boolean = False) As String
            If ioString Is Nothing Then Return String.Empty
            If bTrim Then ioString.Trim()
            Dim i As Integer
            If Not bFromEnd Then
                i = ioString.IndexOf(aChar)
            Else
                i = ioString.LastIndexOf(aChar)
            End If
            Dim _rVal As String = String.Empty
            If i >= 0 Then
                _rVal = ioString.Substring(i + 1, ioString.Length - (i + 1))
                If bRemove Then
                    ioString = ioString.Substring(0, i)
                    If bTrim Then ioString = ioString.Trim()
                End If

            End If

            If bTrim Then Return _rVal.Trim() Else Return _rVal
        End Function


        Friend Shared Sub DecodeDimPrefixSuffix(aDIMPOST As Object, ByRef rPrefix As String, ByRef rSuffix As String)
            rPrefix = ""
            rSuffix = ""
            Dim aStr As String = IIf(aDIMPOST Is Nothing, "", aDIMPOST.ToString())
            If aStr.Length <= 0 Then Return
            Dim i As Integer = aStr.IndexOf("<>") + 1
            If i = 0 Then
                rPrefix = aStr
            Else
                If i > 1 Then rPrefix = aStr.Substring(0, i - 1)
                rSuffix = Mid(aStr, i + 2, aStr.Length - (i + 1))
            End If
        End Sub
        Friend Shared Function DecodeDimensionType(aGCSeventyValue As Integer, aGCFiftyValue As Double) As dxxEntityTypes
            Dim rUserDefinedText As Boolean = False
            Return DecodeDimensionType(aGCSeventyValue, aGCFiftyValue, rUserDefinedText)
        End Function
        Friend Shared Function DecodeDimensionType(aGCSeventyValue As Integer, aGCFiftyValue As Double, ByRef rUserDefinedText As Boolean) As dxxEntityTypes
            Dim bHorz As Boolean
            Dim aCode As Integer
            rUserDefinedText = False
            aCode = aGCSeventyValue
            If aCode >= 128 Then
                rUserDefinedText = True
                aCode -= 128
            End If
            If aCode >= 64 Then
                aCode -= 64
                bHorz = True
            End If
            If aCode >= 32 Then aCode -= 32
            Select Case aCode
                Case 6
                    If bHorz Then
                        Return dxxEntityTypes.DimOrdinateH
                    Else
                        Return dxxEntityTypes.DimOrdinateV
                    End If
                Case 5
                    Return dxxEntityTypes.DimAngular3P
                Case 4
                    Return dxxEntityTypes.DimRadialR
                Case 3
                    Return dxxEntityTypes.DimRadialD
                Case 2
                    Return dxxEntityTypes.DimAngular
                Case 1
                    Return dxxEntityTypes.DimLinearA
                Case 0
                    If aGCFiftyValue <> 0 Then
                        Return dxxEntityTypes.DimLinearV
                    Else
                        Return dxxEntityTypes.DimLinearH
                    End If
            End Select
            Return dxxEntityTypes.DimLinearH
        End Function
        Friend Shared Function DefineArcByBezierVectors(aCS As dxfPlane, ByRef p0 As TVECTOR, ByRef P1 As TVECTOR, ByRef P2 As TVECTOR, ByRef p3 As TVECTOR, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxeArc
            Dim _rVal As dxeArc = Nothing
            Dim aPl As New TPLANE(aCS)

            Dim v2 As TVECTOR
            Dim v1 As TVECTOR
            Dim ang As Double
            Dim py As Double
            Dim pN As TVECTOR = aPl.ZDirection
            Dim pO As TVECTOR = aPl.Origin
            Dim aDir As TVECTOR
            Dim px As Double
            Dim rad As Double
            Dim cp As TVECTOR
            Dim cwise As Boolean

            Dim d0 As TVECTOR = p0.ProjectedTo(aPl)
            Dim d1 As TVECTOR = P1.ProjectedTo(aPl)
            Dim d2 As TVECTOR = P2.ProjectedTo(aPl)
            Dim d3 As TVECTOR = p3.ProjectedTo(aPl)
            Dim ortho As TVECTOR = TVECTOR.Zero

            Dim dX As Double = d1.DistanceTo(d2) / 2

            If dX <= 0 Then Return _rVal
            py = d3.DistanceTo(d0) / 2
            If py <= 0 Then Return _rVal
            If dX > py Then Return _rVal
            v1 = d0 + d0.DirectionTo(d3) * py
            v2 = d1 + d1.DirectionTo(d2) * dX
            If v1.DistanceTo(v2) <= 0 Then Return _rVal
            aDir = v1.DirectionTo(v2)
            ang = aPl.XDirection.AngleTo(aDir, pN)
            If ang <> 0 Then
                ang = ang * Math.PI / 180
                d0.RotateAbout(v1, pN, -ang, True)
                d1.RotateAbout(v1, pN, -ang, True)
                d2.RotateAbout(v1, pN, -ang, True)
                d3.RotateAbout(v1, pN, -ang, True)
                v1 = d0 + d0.DirectionTo(d3) * py
                'v2 = d1 + d1.DirectionTo(d2) * dX
                'aDir = v1.DirectionTo(v2)
                'ang = aPl.XDirection.AngleTo(aDir, pN)
            End If

            v1 = dxfProjections.ToLine(v1, New TLINE(pO, pO + aPl.XDirection), ortho, dX)

            If dX <> 0 Then
                Dim v3 As TVECTOR = ortho * dX
                d0 += v3
                'd1 += v3
                'd2 += v3
                'd3 += v3
                'v2 += v3
                v1 += v3
            End If
            ang = aPl.XDirection.AngleTo(pO.DirectionTo(d0), pN)
            If Math.Abs(ang) >= 180 Then ang = TVALUES.NormAng(360 - ang)
            d0 = dxfProjections.ToLine(p0, New TLINE(pO, pO + aPl.XDirection), ortho, dX)

            cwise = Not ortho.Equals(aPl.YDirection, False, 3)
            If Math.Tan(ang * Math.PI / 180) <> 0 Then px = py / Math.Tan(ang * Math.PI / 180) Else px = 0
            If Math.Sin(ang * Math.PI / 180) <> 0 Then rad = Math.Abs(py / Math.Sin(ang * Math.PI / 180))
            cp = v1 + aPl.XDirection * -px
            _rVal = New dxeArc With {
                .SuppressEvents = True,
                .PlaneV = aPl,
                .CenterV = cp,
                .Radius = rad,
                .ClockWise = cwise,
                .StartAngle = aPl.XDirection.AngleTo(cp.DirectionTo(p0), pN),
                .EndAngle = aPl.XDirection.AngleTo(cp.DirectionTo(p3), pN)
            }
            If aDisplaySettings IsNot Nothing Then _rVal.DisplayStructure = aDisplaySettings.Strukture
            _rVal.SuppressEvents = False
            Return _rVal
        End Function
        Public Shared Function DefineHolesByString(aHole As dxeHole, ByRef ioCenters As colDXFVectors, Optional CenterDescriptors As String = "") As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1a hole (slot or hole) to create the members of the collection with
            '#2a collection of points to place the holes on
            '#3a descriptor string that defines a collection of points to place the holes on
            '^used to create a collection of holes by passign a hole and the centers to place the holes on
            If ioCenters Is Nothing Then ioCenters = New colDXFVectors
            If String.IsNullOrWhiteSpace(CenterDescriptors) Then

                Return _rVal
            End If
            ioCenters.Clear()

            Dim h1 As dxeHole = aHole.Clone

            ioCenters.AddByString(CenterDescriptors, True)
            For Each ctr As dxfVector In ioCenters
                h1.MoveTo(ctr)
                _rVal.Add(h1.Clone())
            Next
            Return _rVal
        End Function
        Friend Shared Function DimPrefixSuffix(aPrefix As String, aSuffix As String) As String
            If aPrefix Is Nothing Then aPrefix = ""
            If aSuffix Is Nothing Then aSuffix = ""

            Dim _rVal As String = $"{aPrefix}<>{ aSuffix}"
            If _rVal = "<>" Then _rVal = ""
            Return _rVal
        End Function
        Public Shared Sub DisplayErrors(aErrors As List(Of String), Optional aCaption As String = "", Optional aOwner As IWin32Window = Nothing)
            If aErrors Is Nothing Then Return
            If aErrors.Count <= 0 Then Return
            Dim frm As New frmMessages
            frm.ShowMessages(aErrors, aCaption, aOwner)
            'frm.DisplayWarnings(aErrors, aCaption)
            'Unload(frm)
            'frm = Nothing
        End Sub
        Public Shared Function DistanceBetweenPoints(Point1XY As iVector, Point2XY As iVector) As Double
            '#1the first of two points
            '#2the second of two points
            '#3flag to return the X,Y and Z distances as positive regardless of their relative positions in space
            '^returns the distance beween the passed points
            Return DistanceBetweenPoints(Point1XY, Point2XY, rXDistance:=0, rYDistance:=0, rZDistance:=0, bAbsoluteVal:=True)
        End Function
        Public Shared Function DistanceBetweenPoints(Point1XY As iVector, Point2XY As iVector, Optional bAbsoluteVal As Boolean = True) As Double
            '#1the first of two points
            '#2the second of two points
            '#3flag to return the X,Y and Z distances as positive regardless of their relative positions in space
            '^returns the distance beween the passed points
            Return DistanceBetweenPoints(Point1XY, Point2XY, rXDistance:=0, rYDistance:=0, rZDistance:=0, bAbsoluteVal:=bAbsoluteVal)
        End Function
        Public Shared Function DistanceBetweenPoints(Point1XY As iVector, Point2XY As iVector, ByRef rXDistance As Double, ByRef rYDistance As Double, ByRef rZDistance As Double, Optional bAbsoluteVal As Boolean = True) As Double
            Dim _rVal As Double
            Dim v1 As TVECTOR = New TVECTOR(Point1XY)
            Dim v2 As TVECTOR = New TVECTOR(Point2XY)
            '#1the first of two points
            '#2the second of two points
            '#3returns the X distance between the passed points
            '#4returns the Y distance between the passed points
            '#5returns the Z distance between the passed points
            '#6flag to return the X,Y and Z distances as positive regardless of their relative positions in space
            '^returns the distance beween the passed points
            Dim xfactor As Integer = 1
            Dim yfactor As Integer = 1
            Dim ZFactor As Integer = 1
            Dim p1x As Double = v1.X
            Dim p1y As Double = v1.Y
            Dim p1z As Double = v1.Z
            Dim p2x As Double = v2.X
            Dim p2y As Double = v2.Y
            Dim p2z As Double = v2.Z
            If p2x >= p1x Then
                rXDistance = p2x - p1x
            Else
                rXDistance = p1x - p2x
                xfactor = -1
            End If
            If p2y >= p1y Then
                rYDistance = p2y - p1y
            Else
                rYDistance = p1y - p2y
                yfactor = -1
            End If
            If p2z >= p1z Then
                rZDistance = p2z - p1z
            Else
                rZDistance = p1z - p2z
                ZFactor = -1
            End If
            _rVal = Math.Sqrt(rXDistance ^ 2 + rYDistance ^ 2 + rZDistance ^ 2)
            If Not bAbsoluteVal Then
                rXDistance *= xfactor
                rYDistance *= yfactor
                rZDistance *= ZFactor
            End If
            Return _rVal
        End Function
        Public Shared Function DistanceToArc(Point1 As iVector, Arc1 As dxeArc) As Double
            Dim rInterceptPoint As dxfVector = Nothing
            Dim rOrthogLine As dxeLine = Nothing
            Dim rInterceptIsOnArc As Boolean = False
            Return DistanceToArc(Point1, Arc1, rInterceptPoint, rOrthogLine, rInterceptIsOnArc)
        End Function
        Public Shared Function DistanceToArc(Point1 As iVector, Arc1 As dxeArc, ByRef rInterceptPoint As dxfVector, ByRef rOrthogLine As dxeLine, ByRef rInterceptIsOnArc As Boolean) As Double
            '#1the point to find a distance from
            '#2the Arc to find the distance to
            '#3returns the point on the arc where a line through the passed point intercepts the passed Arc
            '#4returns the orthoganal vector from the passed point to the passed Arc
            '#5returns True if the intercept point lines on the arc
            '^returns then distance from the passed point to the passed arc
            'rOrthogLine = Nothing
            'rInterceptPoint = Nothing
            rInterceptIsOnArc = False
            rInterceptPoint = Nothing
            rOrthogLine = Nothing
            Try
                If Arc1 Is Nothing Then Throw New Exception("The Passed Arc Is Undefined")
                Return New TVECTOR(Point1).DistanceTo(Arc1, rInterceptPoint, rOrthogLine, rInterceptIsOnArc)
            Catch ex As Exception
                Throw New Exception($"dxfUtils.DistanceToArc - { ex.Message}")
            End Try
        End Function

        Friend Shared Function EllipsePoint(aCenter As TVECTOR, aMajorDia As Double, aMinorDia As Double, aAngle As Double, aPlane As TPLANE) As TVECTOR
            Dim _rVal As TVECTOR = aCenter.Clone()
            '#1the center of the ellipse
            '#2the major diameter of the ellipse
            '#3the minor diameter of the ellipse
            '#4the angle to calculate (in degrees)
            '#5flag to indicate if the passed angle is relative to the ellipses primary axis or to the XY axis
            '#6the OCS of the ellipse
            '^returns a vector on the ellipse at the requested angle from the major axis of the ellipse.
            '~the angle is measured anti-clockwise from the major axis of the ellipse.
            'On Error Resume Next
            aMajorDia = Math.Abs(aMajorDia)
            aMinorDia = Math.Abs(aMinorDia)
            If aMajorDia = 0 Or aMinorDia = 0 Then Return _rVal
            Dim x1 As Double
            Dim Y1 As Double
            Dim ang As Double
            Dim angrad As Double
            Dim fact1 As Double
            Dim fact2 As Double
            Dim F As Double
            Dim bRad As Double
            Dim sRad As Double
            Dim aPl As TPLANE = aPlane.Clone
            aPl.Origin = aCenter
            bRad = aMajorDia / 2
            sRad = aMinorDia / 2
            TVALUES.SortTwoValues(True, sRad, bRad)
            ang = TVALUES.NormAng(aAngle, ThreeSixtyEqZero:=True)
            Select Case ang
                Case 0
                    x1 = bRad
                    Y1 = 0
                Case 90
                    x1 = 0
                    Y1 = sRad
                Case 180
                    x1 = -bRad
                    Y1 = 0
                Case 360
                    x1 = 0
                    Y1 = -sRad
                Case Else
                    angrad = (ang * Math.PI) / 180
                    F = Math.Tan(angrad)
                    fact1 = bRad ^ 2 * sRad ^ 2
                    fact2 = (sRad ^ 2 + bRad ^ 2 * F ^ 2)
                    x1 = Math.Sqrt(fact1 / fact2)
                    Y1 = x1 * Math.Tan(angrad)
                    If (ang > 90 And ang < 180) Or (ang > 180 And ang < 270) Then
                        x1 = -x1
                        Y1 = -Y1
                    End If
            End Select
            'save the coordinates to a new vector
            _rVal = aPl.Vector(x1, Y1, 0)
            Return _rVal
        End Function

        Friend Shared Function ExtractTrailingIndex(aString As String, Optional bAllowNegatives As Boolean = False) As Integer
            Dim rNoIndex As Boolean = False
            Dim rLeadString As String = String.Empty
            If aString Is Nothing Then Return 0
            Return ExtractTrailingIndex(aString, rNoIndex, bAllowNegatives, rLeadString)
        End Function
        Public Shared Function ExtractTrailingIndex(aString As String, ByRef rNoIndex As Boolean, bAllowNegatives As Boolean, ByRef rLeadString As String) As Integer
            rNoIndex = True
            rLeadString = ""
            If aString Is Nothing Then Return 0
            Dim strVal As String = aString.Trim()
            If strVal.Length <= 0 Then Return 0
            Dim _rVal As Integer

            Dim nums As String = String.Empty
            Dim bNeg As Boolean
            Dim lidx As Integer
            Dim chars As System.Char() = strVal.ToCharArray()
            For i As Integer = chars.Length To 1 Step -1
                Dim cr As System.Char = chars(i - 1)

                Dim aChr As String = chars(i - 1).ToString()

                If Asc(aChr) < 48 Or Asc(aChr) > 57 Then
                    If bAllowNegatives Then
                        bNeg = Asc(aChr) = 45
                        If bNeg Then lidx = i
                    End If
                    Exit For
                Else
                    lidx = i
                End If
                nums = $"{aChr}{nums}"
            Next i
            If nums.Length > 0 Then
                rNoIndex = False
                _rVal = TVALUES.To_INT(nums)
                If bNeg Then _rVal = -_rVal
                If lidx > 1 Then
                    rLeadString = strVal.Substring(0, lidx - 1)
                End If
            End If
            Return _rVal
        End Function
        Friend Shared Function ExtractVertices(aEntities As colDXFEntities, Optional AssumedRadius As Double = 0.0, Optional AssumedLinetype As String = "", Optional bReverseOrder As Boolean = False, Optional bClockWiseArcs As Boolean = False, Optional aGapTag As String = "") As colDXFVectors
            Dim rGapIDs As List(Of Integer) = Nothing
            Return ExtractVertices(aEntities, AssumedRadius, AssumedLinetype, bReverseOrder, bClockWiseArcs, rGapIDs, aGapTag)
        End Function
        Friend Shared Function ExtractVertices(aEntities As colDXFEntities, AssumedRadius As Double, AssumedLinetype As String, bReverseOrder As Boolean, bClockWiseArcs As Boolean, ByRef rGapIDs As List(Of Integer), Optional aGapTag As String = "") As colDXFVectors
            Dim _rVal As New colDXFVectors
            '^returns the vertices that define the segments in the collection
            If aEntities Is Nothing Then Return _rVal
            Dim aEnt As dxfEntity
            Dim bEnt As dxfEntity = Nothing
            Dim si As Integer
            Dim ei As Integer
            Dim stp As Integer
            Dim d1 As Double
            Dim gp As dxfVector
            Dim aA As dxeArc
            AssumedRadius = Math.Abs(AssumedRadius)
            AssumedLinetype = Trim(AssumedLinetype)
            If AssumedLinetype = "" Then AssumedLinetype = dxfLinetypes.Invisible
            If Not bReverseOrder Then
                si = 1
                ei = aEntities.Count
                stp = 1
            Else
                ei = 1
                si = aEntities.Count
                stp = -1
            End If
            rGapIDs = New List(Of Integer)
            For i As Integer = si To ei Step stp
                aEnt = aEntities.Item(i)
                Dim sp As dxfVector = aEnt.DefinitionPoint(dxxEntDefPointTypes.StartPt)
                If aEnt.GraphicType = dxxGraphicTypes.Arc Then
                    aA = aEnt
                    sp.Bulge = aA.Bulge
                End If
                If bEnt Is Nothing Then
                    _rVal.Add(sp, bAddClone:=True)
                Else
                    Dim ep As dxfVector = bEnt.DefinitionPoint(dxxEntDefPointTypes.EndPt)
                    d1 = ep.Strukture.DistanceTo(sp.Strukture)
                    If d1 > 0.01 Then
                        gp = _rVal.Add(ep.X, ep.Y, ep.Z, AssumedRadius, bClockWiseArcs, aLayerName:=bEnt.LayerName, aColor:=bEnt.Color, aLineType:=AssumedLinetype, aTag:=aGapTag)
                        rGapIDs.Add(_rVal.Count)
                        _rVal.Add(aEnt.DefinitionPoint(dxxEntDefPointTypes.StartPt), bAddClone:=True)
                    Else
                        _rVal.Add(aEnt.DefinitionPoint(dxxEntDefPointTypes.StartPt), bAddClone:=True)
                        bEnt.DefinitionPoint(dxxEntDefPointTypes.EndPt).SetStructure(sp.Strukture)
                    End If
                End If
                bEnt = aEnt
            Next i
            If aEntities.Count > 0 Then
                aEnt = aEntities.Item(aEntities.Count)
                Dim ep As dxfVector = aEnt.DefinitionPoint(dxxEntDefPointTypes.EndPt).Clone
                Dim sp As dxfVector = _rVal.FirstVector
                If Not ep.IsEqual(sp, 3) Then
                    ep.Linetype = AssumedLinetype
                    ep.Tag = aGapTag
                    If AssumedRadius > 0 Then
                        ep.VertexRadius = AssumedRadius
                        ep.Inverted = bClockWiseArcs
                    Else
                        ep.Radius = 0
                    End If
                    ep.Color = aEnt.Color
                    ep.LayerName = aEnt.LayerName
                    _rVal.Add(ep)
                    rGapIDs.Add(_rVal.Count)
                End If
            End If
            Return _rVal
        End Function
        Friend Shared Function FactorFromTo(aFromUnits As dxxDeviceUnits, aToUnits As dxxDeviceUnits, Optional aPixelsPerInches As Double = 0.0) As Double
            Dim _rVal As Double
            If aFromUnits = aToUnits Then
                _rVal = 1
            Else
                Dim f1 As Double
                Dim f2 As Double
                If aPixelsPerInches <= 0 Then aPixelsPerInches = dxfGlobals.PixelsPerInch()
                'first convert to pixels
                Select Case aFromUnits
                    Case dxxDeviceUnits.Pixels
                        f1 = 1
                    Case dxxDeviceUnits.Inches
                        f1 = aPixelsPerInches
                    Case dxxDeviceUnits.Centimeters
                        f1 = aPixelsPerInches / 2.54
                    Case dxxDeviceUnits.Millimeters
                        f1 = aPixelsPerInches / 25.4
                    Case Else
                        f1 = 1
                End Select
                Select Case aToUnits
                    Case dxxDeviceUnits.Pixels
                        f2 = 1
                    Case dxxDeviceUnits.Inches
                        f2 = 1 / aPixelsPerInches
                    Case dxxDeviceUnits.Centimeters
                        f2 = 1 / (aPixelsPerInches / 2.54)
                    Case dxxDeviceUnits.Millimeters
                        f2 = 1 / (aPixelsPerInches / 25.4)
                    Case Else
                        f2 = 1
                End Select
                _rVal = f1 * f2
            End If
            Return _rVal
        End Function

        Friend Shared Function [Do]() As String
            Throw New NotImplementedException()
        End Function

        Friend Shared Function FactorToInches(aUnits As dxxDeviceUnits) As Double
            Dim _rVal As Double
            _rVal = FactorToPixels(aUnits) / dxfGlobals.PixelsPerInch()
            Return _rVal
        End Function
        Public Shared Function FactorToPixels(aFromUnits As dxxDeviceUnits) As Double
            Dim _rVal As Double
            Select Case aFromUnits
                Case dxxDeviceUnits.Pixels
                    _rVal = 1
                Case dxxDeviceUnits.Inches
                    _rVal = dxfGlobals.PixelsPerInch()
                Case dxxDeviceUnits.Centimeters
                    _rVal = dxfGlobals.PixelsPerInch() / 2.54
                Case dxxDeviceUnits.Millimeters
                    _rVal = dxfGlobals.PixelsPerInch() / 25.4
                Case Else
                    _rVal = 1
            End Select
            Return _rVal
        End Function



        Friend Shared Function FilletLines(aVertices As colDXFVectors, Optional aGap As Double = 0.0, Optional aDisplayVars As dxfDisplaySettings = Nothing, Optional aCS As dxfPlane = Nothing) As colDXFEntities
            Dim _rVal As New colDXFEntities
            If aVertices Is Nothing Then Return _rVal
            If aVertices.Count < 3 Then Return _rVal
            Dim aLine As TLINE
            Dim bLine As TLINE
            Dim sp1 As TVERTEX
            Dim sp2 As TVERTEX
            Dim ep1 As TVERTEX
            Dim ep2 As TVERTEX
            Dim aFlag As Boolean
            Dim bFlag As Boolean
            Dim ip As TVECTOR
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim v3 As TVECTOR
            Dim aPl As dxePolyline
            Dim aDS As New TDISPLAYVARS("0")
            Dim bPl As New TPLANE("")
            If aCS IsNot Nothing Then bPl = New TPLANE(aCS)
            If aDisplayVars IsNot Nothing Then aDS = aDisplayVars.Strukture
            Dim rInterceptExists As Boolean = False
            Dim rLinesAreParallel As Boolean = False
            Dim rLinesAreCoincident As Boolean = False
            For i As Integer = 1 To aVertices.Count
                ep1 = aVertices.ItemVertex(i)
                If ep1.Radius <> 0 Then
                    sp1 = aVertices.ItemVertex(i - 1)
                    sp2 = aVertices.ItemVertex(i + 1)
                    ep2 = aVertices.ItemVertex(i + 2)
                    If sp1.Radius = 0 And sp2.Radius = 0 Then
                        aLine = New TLINE(sp1, ep1)
                        bLine = New TLINE(sp2, ep2)
                        ip = aLine.IntersectionPt(bLine, rLinesAreParallel, rLinesAreCoincident, aFlag, bFlag, rInterceptExists)
                        If Not aFlag And Not bFlag Then
                            v1 = ep1.Vector
                            v2 = ip
                            v3 = sp2.Vector
                            If aGap > 0 Then
                                v1 += sp1.Vector.DirectionTo(ep1.Vector) * aGap
                                v3 += ep2.Vector.DirectionTo(sp2.Vector) * aGap
                            End If
                            aPl = New dxePolyline With {.Closed = False, .PlaneV = bPl, .DisplayStructure = aDS}
                            aPl.AddV(v1)
                            aPl.AddV(v2)
                            aPl.AddV(v3)
                            _rVal.Add(aPl)
                        End If
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Shared Function FilletPoints(aVertices As colDXFVectors, Optional bClosed As Boolean = False, Optional aRadius As Double = 0.0, Optional bReturnLinearIntersections As Boolean = False) As colDXFVectors
            Dim _rVal As colDXFVectors
            _rVal = New colDXFVectors
            If aVertices Is Nothing Then Return _rVal
            If aVertices.Count < 2 Then Return _rVal
            Dim aFlag As Boolean
            Dim rad As Double = Math.Round(Math.Abs(aRadius), 4)
            For i As Integer = 1 To aVertices.Count
                Dim ep1 As TVERTEX = aVertices.ItemVertex(i)
                If i = aVertices.Count And Not bClosed Then Exit For
                If ep1.Radius <> 0 Then
                    If rad = 0 Or rad = Math.Round(Math.Abs(ep1.Radius), 4) Then
                        Dim sp1 As TVERTEX = aVertices.ItemVertex(i - 1)
                        Dim sp2 As TVERTEX = aVertices.ItemVertex(i + 1)
                        Dim ep2 As TVERTEX = aVertices.ItemVertex(i + 2)
                        If sp1.Radius = 0 And sp2.Radius = 0 Then
                            Dim aLine As New TLINE(sp1.Vector, ep1.Vector)
                            Dim ip As TVECTOR = aLine.IntersectionPt(New TLINE(sp2.Vector, ep2.Vector), rInterceptExists:=aFlag)
                            If aFlag Then
                                _rVal.AddV(ip)
                            Else
                                If bReturnLinearIntersections Then
                                    _rVal.AddV(ep1.Vector)
                                End If
                            End If
                        Else
                            If bReturnLinearIntersections Then
                                _rVal.AddV(ep1.Vector)
                            End If
                        End If
                    End If
                Else
                    If bReturnLinearIntersections Then
                        _rVal.AddV(ep1.Vector)
                    End If
                End If
            Next i
            Return _rVal
        End Function

        Friend Shared Function FilletVertices(aVertices As TVERTICES, aPlane As TPLANE, aVertex As Integer, aRadius As Double, bApplyChamfer As Boolean, aSecondChamfer As Double, ByRef rChanged As Boolean) As TVERTICES
            Dim _rVal As New TVERTICES(aVertices)
            '^fillets the indicated intersection with the passed radius if possble
            'On Error Resume Next
            rChanged = False
            If aVertices.Count < 3 Then Return _rVal
            If aVertex <= 0 Or aVertex > aVertices.Count Then Return _rVal
            aRadius = Math.Round(Math.Abs(aRadius), 6)
            If aRadius = 0 Then Return _rVal
            Dim aSP As TVERTEX
            Dim aEP As TVERTEX
            Dim aMP As TVERTEX
            Dim spid As Integer
            Dim epid As Integer
            Dim idx As Integer
            Dim bFlag1 As Boolean
            Dim bFlag2 As Boolean
            Dim d1 As Double
            Dim aDir As TVECTOR
            Dim bDir As TVECTOR
            Dim vNew As TVERTEX
            idx = aVertex
            aMP = aVertices.Item(idx) 'the primary
            If idx = 1 Then spid = aVertices.Count Else spid = idx - 1
            If idx = aVertices.Count Then epid = 1 Else epid = idx + 1
            aSP = aVertices.Item(spid) 'the one before
            aEP = aVertices.Item(epid) 'the one after
            aSecondChamfer = Math.Round(Math.Abs(aSecondChamfer), 6)
            If aSecondChamfer = 0 Then aSecondChamfer = aRadius
            aDir = aMP.Vector.DirectionTo(aSP.Vector, False, bFlag1, d1)
            If bFlag1 Then Return _rVal
            bDir = aMP.Vector.DirectionTo(aEP.Vector, False, bFlag1, d1)
            If bFlag1 Then Return _rVal
            '=====================================
            If bApplyChamfer Then  'chamfer
                '=====================================
                ' aPlane = TPLANES.FromNormal((aDir * -1).CrossProduct(bDir), aMP.Vector)
                'move the primary
                _rVal.SetVector(aMP.Vector + (aDir * aRadius), aVertex)
                vNew = aMP
                vNew.Radius = 0
                vNew.Vector = aMP.Vector + (bDir * aSecondChamfer)
                _rVal.Insert(vNew, idx)
                rChanged = True
                '=====================================
            Else  'arc
                '=====================================
                Dim aAr As TARC = dxfPrimatives.CreateFilletArc(aRadius, aSP.Vector, aMP.Vector, aEP.Vector, aPlane, bFlag1, bFlag2, d1)
                If aAr.Radius <= 0 Then Return _rVal 'the fillet arc could not be created
                If Not bFlag1 Or Not bFlag2 Then Return _rVal 'the arcs end points don't don't lie on the lines formed by the points
                rChanged = True
                vNew = aMP
                _rVal.SetVector(aMP.Vector + (aDir * d1), aVertex)
                _rVal.SetRadius(aAr.Radius, aVertex)

                _rVal.SetInverted(aAr.ClockWise, aVertex)
                vNew.Vector = aMP.Vector + (bDir * d1)
                _rVal.Insert(vNew, idx)
            End If
            Return _rVal
        End Function

        Friend Shared Function FilletVerticesM(aVertices As TVERTICES, aIndexes As String, aRadiuses As String, bChanged As Boolean, Optional bApplyChamfer As Boolean = False, Optional aSecondChamfer As Double = 0.0) As TVERTICES
            bChanged = False
            Dim _rVal As TVERTICES = aVertices
            If aVertices.Count <= 1 Then Return _rVal
            If String.IsNullOrWhiteSpace(aIndexes) Or String.IsNullOrWhiteSpace(aRadiuses) Then Return _rVal
            '^fillets the indicated intersection with the passed radius if possble
            Try
                aIndexes = aIndexes.Trim
                aRadiuses = aRadiuses.Trim
                If aIndexes = "" Then Return _rVal
                If aRadiuses = "" Then Return _rVal
                Dim ids(0) As Object
                Dim radS(0) As Object
                Dim sVals() As String
                Dim i As Integer
                Dim j As Integer
                Dim cnt1 As Integer
                Dim radCnt As Integer
                Dim idCnt As Integer
                Dim sVal As String = String.Empty
                Dim aID As Integer
                Dim arad As Double
                Dim verts As New TVERTICES
                Dim aFlag As Boolean
                Dim bPlane As New TPLANE("")
                Dim tpl As Tuple(Of Integer, Double)
                Dim tpl2 As Tuple(Of Integer, Double)
                Dim srt As New List(Of Tuple(Of Integer, Double))
                verts = aVertices
                sVals = aIndexes.Split(",")
                cnt1 = sVals.Length - 1
                idCnt = 0
                For i = 0 To cnt1
                    sVal = Trim(sVals(i))
                    If TVALUES.IsNumber(sVal) Then
                        aID = TVALUES.ToInteger(sVal)
                        If aID > 0 And aID <= aVertices.Count Then
                            srt.Add(New Tuple(Of Integer, Double)(aID, 0))
                        End If
                    End If
                Next i
                If srt.Count <= 0 Then Return _rVal
                sVals = aRadiuses.Split(",")
                cnt1 = sVals.Length - 1
                radCnt = 0
                For i = 0 To cnt1
                    If i + 1 > srt.Count Then Exit For
                    sVal = Trim(sVals(i))
                    If TVALUES.IsNumber(sVal) Then
                        arad = TVALUES.To_DBL(sVal, bAbsVal:=True)
                        If arad > 0 Then
                            radCnt += 1
                            tpl = srt.Item(i)
                            tpl = New Tuple(Of Integer, Double)(tpl.Item1, arad)
                            srt.Item(i) = tpl
                        End If
                    End If
                Next i
                If radCnt = 0 Then Return _rVal
                If radCnt < srt.Count Then
                    arad = srt.Item(srt.Count - 1).Item2
                    For i = 1 To srt.Count
                        tpl = srt.Item(i - 1)
                        If tpl.Item2 = 0 Then
                            tpl = New Tuple(Of Integer, Double)(tpl.Item1, arad)
                            srt.Item(i - 1) = tpl
                        End If
                    Next
                End If
                srt.Sort(Function(tupl1 As Tuple(Of Integer, Double), tupl2 As Tuple(Of Integer, Double)) tupl1.Item1.CompareTo(tupl2.Item1))
                'srt.Reverse()
                For i = 1 To srt.Count
                    tpl = srt.Item(i - 1)
                    arad = tpl.Item2
                    aID = tpl.Item1
                    verts = FilletVertices(verts, bPlane, tpl.Item1, tpl.Item2, bApplyChamfer, aSecondChamfer, aFlag)
                    If aFlag Then
                        bChanged = True
                        For j = i + 1 To srt.Count
                            tpl2 = srt.Item(j - 1)
                            tpl2 = New Tuple(Of Integer, Double)(tpl2.Item1 + 1, tpl2.Item2)
                            srt.Item(j - 1) = tpl2
                        Next j
                    End If
                Next
                _rVal = verts
            Catch ex As Exception
            End Try
            Return _rVal
        End Function
        Friend Shared Function FormatTextForOutput(aMText As dxeText) As List(Of String)
            Dim _rVal As New List(Of String)
            '^used internally to added AutoCAD aMText format codes to the DXFOutput
            If aMText Is Nothing Then Return _rVal

            aMText.UpdatePath()
            Dim aStr As String = aMText.TextString
            'now break into 250 char _rVal
            If aStr.Length < 250 Then
                _rVal.Add(aStr)
            Else
                Do Until aStr.Length <= 250
                    Dim ln As String = aStr.Substring(0, 250)
                    _rVal.Add(ln)
                    aStr = Right(aStr, aStr.Length - 250)
                Loop
                If aStr.Length > 0 Then _rVal.Add(aStr)
            End If
            If _rVal.Count = 0 Then _rVal.Add("")

            Return _rVal
        End Function
        Public Shared Sub GetAlignmentCodes(aAlignment As dxxMTextAlignments, ByRef vAlign As dxxTextJustificationsVertical, ByRef hAlign As dxxTextJustificationsHorizontal)
            '^returns DXF horizontal and vertical text alignment codes for the passed text alignment
            TFONT.EncodeAlignment(aAlignment, vAlign, hAlign)
        End Sub
        Public Shared Sub GetColorHSB(aColor As Integer, ByRef rHue As Integer, ByRef rSaturation As Integer, ByRef rBrightness As Integer)
            '^get the HSB values for a windows color
            Dim aHSB As COLOR_HSB = dxfColors.Win32ToHSB(aColor)
            rBrightness = aHSB.B
            rSaturation = aHSB.S
            rHue = aHSB.H
        End Sub
        Public Shared Sub GetColorRGB(aColor As Integer, ByRef rRed As Integer, ByRef rGreen As Integer, ByRef rBlue As Integer)
            '^get the RGB values for a windows color
            Dim aRGB As COLOR_ARGB = dxfColors.Win32ToARGB(aColor)
            rRed = aRGB.R
            rGreen = aRGB.G
            rBlue = aRGB.B
        End Sub
        Public Shared Function GetFontNames(Optional aSuppressShapes As Boolean = False, Optional aSuppressTrueTypes As Boolean = False, Optional aSorted As Boolean = False, Optional aSuppressExtenstions As Boolean = False) As List(Of String)
            '#1flag to not return shape fonts in the list
            '#2flag to not return true type fonts
            '#3flag sort the returned collection alphabetically
            '#4flag to include the fonts file extension in the name
            '^returns the names of the members in the array
            Return dxoFonts.GetFontNames(aSuppressShapes, aSuppressTrueTypes, aSorted, aSuppressExtenstions)
        End Function
        Public Shared Function GetPointOnEllipse(aCenter As iVector, MajorDia As Double, MinorDia As Double, aAngle As Double, Optional aCS As dxfPlane = Nothing) As dxfVector
            '#1the center of the ellipse
            '#2the major diameter of the ellipse
            '#3the minor diameter of the ellipse
            '#4the angle to calculate (in degrees)
            '#5flag to indicate if the passed angle is relative to the ellipses primary axis or to the XY axis
            '#6the OCS of the ellipse
            '^returns a vector on the ellipse at the requested angle from the major axis of the ellipse.
            '~the angle is measured anti-clockwise from the major axis of the ellipse.

            Return New dxfVector(dxfUtils.EllipsePoint(New TVECTOR(aCenter), MajorDia, MinorDia, aAngle, New TPLANE(aCS)))
        End Function
        Public Shared Function GetEntityTypeName(aEntType As dxxEntityTypes) As String
            Select Case aEntType
                Case dxxEntityTypes.Arc
                    Return "ARC"
                Case dxxEntityTypes.Symbol
                    Return "SYMBOL"
                Case dxxEntityTypes.Attdef
                    Return "ATTDEF"
                Case dxxEntityTypes.Attribute
                    Return "ATTRIB"
                Case dxxEntityTypes.Bezier
                    Return "BEZIER"
                Case dxxEntityTypes.Circle
                    Return "CIRCLE"
                Case dxxEntityTypes.DimRadialR, dxxEntityTypes.DimRadialD, dxxEntityTypes.DimOrdinateV, dxxEntityTypes.DimOrdinateH, dxxEntityTypes.DimAngular, dxxEntityTypes.DimAngular3P, dxxEntityTypes.DimLinearA, dxxEntityTypes.DimLinearH, dxxEntityTypes.DimLinearV
                    Return "DIMENSION"
                Case dxxEntityTypes.Ellipse
                    Return "ELLIPSE"
                Case dxxEntityTypes.Hatch
                    Return "HATCH"
                Case dxxEntityTypes.Hatch
                    Return "GRID"
                Case dxxEntityTypes.Hole, dxxEntityTypes.Slot
                    Return "HOLE"
                Case dxxEntityTypes.Insert
                    Return "INSERT"
                Case dxxEntityTypes.Leader, dxxEntityTypes.LeaderText, dxxEntityTypes.LeaderBlock, dxxEntityTypes.LeaderTolerance
                    Return "LEADER"
                Case dxxEntityTypes.Line
                    Return "LINE"
                Case dxxEntityTypes.MText
                    Return "MTEXT"
                Case dxxEntityTypes.Point
                    Return "POINT"
                Case dxxEntityTypes.Polygon
                    Return "POLYGON"
                Case dxxEntityTypes.Polyline
                    Return "POLYLINE"
                Case dxxEntityTypes.Solid
                    Return "SOLID"
                Case dxxEntityTypes.Table
                    Return "TABLE"
                Case dxxEntityTypes.Text
                    Return "TEXT"
                Case dxxEntityTypes.Trace
                    Return "TRACE"
                Case Else
                    Return "UNDEFINED"
            End Select
        End Function
        Private Shared Function GetShellExecuteError(ByRef retVal As Long) As String
            Dim _rVal As String = String.Empty
            '#1the return value of the shellexecute API call
            '^internal function used to translate the return code of the shellexecute statement to a string
            Select Case retVal
                Case 2
                    _rVal = "The specified file could not be found."
                Case 3
                    _rVal = "The specified directory could not be found."
                Case 5
                    _rVal = "Win 95/98 only: Windows denied access to the specified file."
                Case 8
                    _rVal = "Win 95/98 only: Windows has insufficient memory to perform the operation."
                Case 11
                    _rVal = "The specified executable file(.EXE) was somehow invalid."
                Case 26
                    _rVal = "A sharing violation occured."
                Case 27
                    _rVal = "The filename association is either incomplete or invalid."
                Case 28
                    _rVal = "The DDE transaction was not completed because the request timed out."
                Case 29
                    _rVal = "The DDE transaction failed."
                Case 30
                    _rVal = "The DDE action could not run because other DDE actions are in process."
                Case 31
                    _rVal = "There is no program associated with the specified type of file."
                Case 32
                    _rVal = "Win 95/98 only: The specified DLL file was not found."
            End Select
            Return _rVal
        End Function
        Friend Shared Function GetSpecifiedArc(Line1XY As iLine, Line2XY As iLine, SpecPoint As iVector) As dxeArc
            Dim rWorkline1 As dxeLine = Nothing
            Dim rWorkline2 As dxeLine = Nothing
            Return GetSpecifiedArc(Line1XY, Line2XY, SpecPoint, rWorkline1, rWorkline2)
        End Function
        Friend Shared Function GetSpecifiedArc(Line1XY As iLine, Line2XY As iLine, SpecPoint As iVector, ByRef rWorkline1 As dxeLine, ByRef rWorkline2 As dxeLine) As dxeArc
            Dim _rVal As dxeArc = Nothing
            rWorkline1 = Nothing
            rWorkline2 = Nothing
            If Line1XY Is Nothing Or Line2XY Is Nothing Then Return Nothing

            '#1the first of two lines that forms the angle to be specified
            '#2the second of two lines that forms the angle to be specified
            '#3the pick point that indicates which the arc to return
            '^used to draw an Angular dimension
            Dim PicPt As New TVECTOR(SpecPoint)
            Dim line1 As New TLINE(Line1XY)
            Dim line2 As New TLINE(Line2XY)
            Dim iptexists As Boolean = False
            Dim iPt As TVECTOR = line1.IntersectionPt(line2, iptexists)
            rWorkline1 = New dxeLine(line1)
            rWorkline2 = New dxeLine(line2)

            If Not iptexists Then Return Nothing
            Dim rad As Double

            Dim i As Integer
            Dim ang1 As Double
            Dim ang2 As Double
            Dim aArcs As Collection
            Dim aArc As dxeArc

            aArcs = New Collection

            'get the intersection point
            'get the radius
            rad = iPt.DistanceTo(PicPt)
            If rad <= 0 Then Return _rVal
            If line1.SPT.DistanceTo(iPt) > line1.EPT.DistanceTo(iPt) Then line1 = line1.Inverse
            If line2.SPT.DistanceTo(iPt) > line2.EPT.DistanceTo(iPt) Then line2 = line2.Inverse
            Dim angs As New List(Of Double) From {
                line1.AngleOfInclination,
                line2.AngleOfInclination
            }
            angs.Add(TVALUES.NormAng(angs(0) + 180))
            angs.Add(TVALUES.NormAng(angs(1) + 180))

            angs.Sort() 'low to high
            For i = 0 To 3
                ang1 = angs.Item(i)
                If i < 3 Then ang2 = angs(i + 1) Else ang2 = angs(0)
                aArc = New dxeArc
                aArc.MoveTo(iPt)
                aArc.Color = i
                aArc.Radius = rad
                aArc.StartAngle = ang1
                aArc.EndAngle = ang2
                aArcs.Add(aArc)
            Next i
            For i = 1 To 4
                aArc = aArcs.Item(i)
                If aArc.ContainsVector(PicPt, bTreatAsInfinite:=True) Then
                    If Not aArc.EndPtV.Equals(PicPt, 4) Then
                        _rVal = aArc
                        Exit For
                    End If
                End If
            Next i

            If Math.Abs(Math.Round(rWorkline1.AngleOfInclination - line1.AngleOfInclination, 1)) <> 0 And Math.Abs(Math.Round(rWorkline1.AngleOfInclination - line1.AngleOfInclination, 1)) <> 180 Then
                dxfUtils.SwapObjects(rWorkline1, rWorkline2)

            End If
            Return _rVal
        End Function

        Friend Shared Function HoleDescriptor(aHole As dxeHole) As String
            Dim _rVal As String = String.Empty
            '^a string that completely defines the slots current property values
            '~see dxfSlot.DefineByString
            If aHole Is Nothing Then Return _rVal

            Dim ctr As dxfVector
            '**UNUSED VAR** Dim aStr As String = String.Empty
            ctr = aHole.Center
            If 2 * aHole.Radius >= aHole.Length Then
                TLISTS.Add(_rVal, "(0")
            Else
                'Dim dlm As String = gsTemDXFFileName
                TLISTS.Add(_rVal, "(1")
            End If
            TLISTS.Add(_rVal, $"{ ctr.X},{ ctr.Y },{ ctr.Z}", bAllowDuplicates:=True)
            TLISTS.Add(_rVal, $"{aHole.Radius},{ aHole.Length},{ aHole.Rotation},{ aHole.Depth}", bAllowDuplicates:=True)
            TLISTS.Add(_rVal, $"{aHole.DownSet },{aHole.Inset},{TVALUES.To_INT(aHole.IsSquare)}", bAllowDuplicates:=True)
            TLISTS.Add(_rVal, $"{aHole.Tag },{aHole.Flag},{aHole.MinorRadius}", bAllowDuplicates:=True)
            TLISTS.Add(_rVal, Replace(aHole.ZDirection.Components, ",", dxfGlobals.Delim), bAllowDuplicates:=True)
            TLISTS.Add(_rVal, Replace(aHole.XDirection.Components, ",", dxfGlobals.Delim), bAllowDuplicates:=True)
            _rVal += ") "

            Return _rVal
        End Function
        Public Shared Function HolesAsViewedFrom(aHoles As colDXFEntities, aViewVector As String, Optional aHandlesToInclude As String = "", Optional bReturnTheInverse As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1the collection of holes to search through
            '#2the name of the axis to look at the holes from
            '#3a comma delimited string of the handles of the holes to include
            '#4flag to return the holes whose handle dont match the passed handles
            '^returns a clone of the collection as view from the indicated direction axis
            '~the holes are assumed to be defined with respect to the standard xyz coordinate system.
            '~the passed aViewVector argument must be either "X" or "Y"
            If aHoles Is Nothing Then Return _rVal
            aViewVector = Trim(UCase(aViewVector))
            If aViewVector <> "X" And aViewVector <> "Y" Then Return _rVal
            Dim hndls As List(Of String)
            Dim wCol As colDXFEntities
            If aHandlesToInclude <> "" Then
                hndls = TLISTS.ToList(aHandlesToInclude, ",")
                wCol = HolesGetByHandles(aHoles, hndls, bReturnTheInverse)
            Else
                wCol = aHoles
            End If
            Dim aMem As dxfEntity
            Dim aHole As dxeHole
            For i As Integer = 1 To wCol.Count
                aMem = wCol.Item(i)
                If aMem.GraphicType = dxxGraphicTypes.Hole Then
                    aHole = aMem
                    _rVal.Add(aHole.AsViewedFrom(aViewVector))
                End If
            Next i
            Return _rVal
        End Function
        Public Shared Function IntersectionOfLineAndPlane(aLine As iLine, aPlane As dxfPlane, Optional rPointIsOnLine As Boolean = False, Optional rCoplanar As Boolean = False) As dxfVector
            '#1the line to search
            '#2the plane  to search
            '#3returns True if the intersection point is actually on the passed line
            '#4returns true if the line doesn't intersect the plane and the line lines on the plane
            '^returns the point where the passed line intersects the passed plane
            rPointIsOnLine = False
            rCoplanar = False
            Dim bLine As New TLINE(aLine)
            Try
                If TPLANE.IsNull(aPlane) Or bLine.Length <= 0 Then Throw New Exception("The Passed Plane Is Undefined")


                Dim v1 As TVECTOR = dxfIntersections.LinePlane(bLine, New TPLANE(aPlane), rPointIsOnLine, rCoplanar)
                If Not rCoplanar Or (rCoplanar And rPointIsOnLine) Then
                    Return New dxfVector(v1)
                Else
                    Return Nothing
                End If
            Catch ex As Exception
                Throw New Exception($"dxfUtils.{ Reflection.MethodBase.GetCurrentMethod.Name} - { ex.Message}")
                Return Nothing
            End Try
        End Function


        Private Shared Function HolesGetByHandles(ByRef aHoles As colDXFEntities, ByRef aHoleHandles As List(Of String), Optional bReturnTheInverse As Boolean = False) As colDXFEntities
            '^returns the holes listed in the passed collection of handles
            Dim bAddit As Boolean
            Dim aMem As dxeHole
            If aHoleHandles Is Nothing Then aHoleHandles = New List(Of String)
            Dim holes As List(Of dxeHole) = aHoles.FindAll(Function(x) x.GraphicType = dxxGraphicTypes.Hole).Cast(Of dxeHole)
            Dim _rVal As New colDXFEntities
            For Each aMem In holes
                If aHoleHandles.Count <= 0 Then
                    _rVal.Add(aMem)
                Else
                    bAddit = False
                    For j As Integer = 1 To aHoleHandles.Count
                        Dim aStr As String = aHoleHandles.Item(j)
                        If aStr <> "" Then
                            If StrComp(aStr, "*,*", vbTextCompare) = 0 Then
                                bAddit = True
                                Exit For
                            End If
                            If StrComp(aStr, aMem.HoleHandle, vbTextCompare) = 0 Then
                                bAddit = True
                                Exit For
                            End If
                            If Right(aStr, 2) = ",*" Then
                                If StrComp(Left(aStr, aStr.Length - 2), aMem.Tag, vbTextCompare) = 0 Then
                                    bAddit = True
                                    Exit For
                                End If
                            End If
                            If Left(aStr, 2) = "*," Then
                                If StrComp(Right(aStr, aStr.Length - 2), aMem.Flag, vbTextCompare) = 0 Then
                                    bAddit = True
                                    Exit For
                                End If
                            End If
                        End If
                    Next j
                    If Not bReturnTheInverse Then
                        If bAddit Then _rVal.Add(aMem)
                    Else
                        If Not bAddit Then _rVal.Add(aMem)
                    End If
                End If
            Next
            Return _rVal
        End Function
        Public Shared Function Input_Integer(aInitValue As Object, ByRef rCanceled As Boolean, Optional aHeader As String = "", Optional aPrompt As String = "", Optional aInfo As String = "", Optional aAllowZeroInput As Boolean = False, Optional aAllowNegatives As Boolean = False, Optional aMaxLimit As Integer? = Nothing, Optional aMinLimit As Integer? = Nothing, Optional aMaxWhole As Integer = 4, Optional aEqualReturnCancel As Boolean = False) As Integer
            Dim _rVal As Integer
            '#1the initial value to display
            '#2a header string (caption) for the input form
            '#3a string to show as a prompt above the input box
            '#4a string to display under the input box
            '#5flag to allow zero as valid input
            '#6flag to allow negative values
            '#7a max limit to apply
            '#8a minimum limit to apply
            '#9the maximimum number of digits to allow (max is 5)
            '#10flag indicating to return the cancel flag as true is the initial value is equal to the inputed value
            '#11returns True if the user cancels the form
            '^shows a form prompting the user to enter a integer value
            If dxfUtils.RunningInIDE Then MessageBox.Show("TODO - FIX THIS")
            '       'On Error Resume Next
            'Dim iFrm As frmInput
            'iFrm = New frmInput
            '_rVal = iFrm.Input_Integer(aInitValue, aHeader, aPrompt, aInfo, aAllowZeroInput, aAllowNegatives, aMaxLimit, aMinLimit, aMaxWhole, aEqualReturnCancel, rCanceled)
            'Unload(iFrm)
            'iFrm = Nothing
            Return _rVal
        End Function
        Public Shared Function Input_Long(aInitValue As Object, ByRef rCanceled As Boolean, Optional aHeader As String = "", Optional aPrompt As String = "", Optional aInfo As String = "", Optional aAllowZeroInput As Boolean = False, Optional aAllowNegatives As Boolean = False, Optional aMaxLimit As Long? = Nothing, Optional aMinLimit As Long? = Nothing, Optional aMaxWhole As Integer = 6, Optional aEqualReturnCancel As Boolean = False) As Long
            Dim _rVal As Long = 0
            '#1the initial value to display
            '#2a header string (caption) for the input form
            '#3a string to show as a prompt above the input box
            '#4a string to display under the input box
            '#5flag to allow zero as valid input
            '#6flag to allow negative values
            '#7a max limit to apply
            '#8a minimum limit to apply
            '#9the maximimum number of digits to allow (max is 9)
            '#10flag indicating to return the cancel flag as true is the initial value is equal to the inputed value
            '#11returns True if the user cancels the form
            '^shows a form prompting the user to enter a long value
            If dxfUtils.RunningInIDE Then MessageBox.Show("TODO - FIX THIS")
            ''On Error Resume Next
            'Dim iFrm As frmInput
            'iFrm = New frmInput
            '_rVal = iFrm.Input_Long(aInitValue, aHeader, aPrompt, aInfo, aAllowZeroInput, aAllowNegatives, aMaxLimit, aMinLimit, aMaxWhole, aEqualReturnCancel, rCanceled)
            'Unload(iFrm)
            'iFrm = Nothing
            Return _rVal
        End Function
        Public Shared Function ShowInput_SelectFromList(aStrings As List(Of String), rCanceled As Boolean, Optional aInitString As String = "", Optional aHeader As String = "", Optional aPrompt As String = "", Optional aInfo As String = "", Optional aAllowNullInput As Boolean = False, Optional aInvalidValues As Collection = Nothing, Optional aEqualReturnCancel As Boolean = False) As String
            Dim _rVal As String = String.Empty
            '#1a collection of strings to show in the combo box
            '#2the initial string to show as selected in the combo box
            '#3a header string (caption) for the input form
            '#4a string to show as a prompt above the combo box
            '#5a string to display under the combo box
            '#6flag to allow the null string as valid input
            '#7a collection of strings to treat as invalid selections
            '#8flag indicating to return the cancel flag as true is the initial value is the selected value
            '#9returns True if the user cancels the selection
            '^returns the string the user selects from the passed list of strings
            '~displays a form with a combo box containing the unique members of the passed list
            If dxfUtils.RunningInIDE Then MessageBox.Show("TODO - FIX THIS")
            ''On Error Resume Next
            'Dim iFrm As frmInput
            'iFrm = New frmInput
            '_rVal = iFrm.Input_Select(aStringCol, aInitString, aHeader, aPrompt, aInfo, aAllowNullInput, aInvalidValues, aEqualReturnCancel, rCanceled)
            'Unload(iFrm)
            'iFrm = Nothing
            Return _rVal
        End Function
        Public Shared Function Input_String(aInitString As Object, rCanceled As Boolean, Optional aHeader As String = "", Optional aPrompt As String = "", Optional aInfo As String = "", Optional aAllowNullInput As Boolean = False, Optional aInvalidValues As Collection = Nothing, Optional aInvalidChars As String = "", Optional aMaxLen As Integer = 0, Optional aEqualReturnCancel As Boolean = False) As String
            Dim _rVal As String = String.Empty
            '#1the initial value to display
            '#2a header string (caption) for the input form
            '#3a string to show as a prompt above the input box
            '#4a string to display under the input box
            '#5flag to allow the null string as valid input
            '#6a collection of strings to treat as invalid values
            '#7characters to dissallow in the returned string
            '#8a maximum string length to enforce
            '#11flag indicating to return the cancel flag as true is the initial value is equal to the inputed value
            '#12returns True if the user cancels the form
            '^displays a form prompting the user to enter a string value
            If dxfUtils.RunningInIDE Then MessageBox.Show("TODO - FIX THIS")
            'On Error Resume Next
            'Dim iFrm As frmInput
            'iFrm = New frmInput
            '_rVal = iFrm.Input_String(aInitString, aHeader, aPrompt, aInfo, aAllowNullInput, aInvalidValues, aInvalidChars, aMaxLen, aEqualReturnCancel, rCanceled)
            'Unload(iFrm)
            'iFrm = Nothing
            Return _rVal
        End Function
        Friend Shared Function InsertString(aString As String, bString As String, aAfterIndex As Long) As String
            Dim _rVal As String = aString
            Dim tlen As Long
            tlen = aString.Length
            If tlen = 0 Then
                _rVal = bString
                Return _rVal
            End If
            If bString.Length = 0 Then Return _rVal
            If aAfterIndex <= 0 Then
                _rVal = $"{bString}{aString}"
            ElseIf aAfterIndex > tlen Then
                _rVal = $"{aString }{ bString}"
            Else
                _rVal = $"{aString.Substring(aAfterIndex) }{ bString }{Right(aString, tlen - aAfterIndex)}"
            End If
            Return _rVal
        End Function
        Public Shared Function IsEven(Num As Object) As Boolean
            '#1the value to search
            '^returns True if the passed value is numeric and is an even number
            '~an Error is raised if the passed value is not numeric
            Try

                If Not TVALUES.IsNumber(Num) Then Throw New Exception("A Non Numeric Value Was Detected")
                Return TVALUES.To_INT(Num) Mod 2 = 0

            Catch ex As Exception
                Throw New Exception($"dxfUtils.{ Reflection.MethodBase.GetCurrentMethod.Name} - { ex.Message}")
                Return False
            End Try
        End Function
        Public Shared Function IsOdd(Num As Object) As Boolean
            '#1the value to test
            '^returns True if the passed value is numeric and is an odd number
            '~an Error is raised if the passed value is not numeric
            Try
                If Not TVALUES.IsNumber(Num) Then Throw New Exception("A Non Numeric Value Was Detected")
                Return TVALUES.To_INT(Num) Mod 2 <> 0
            Catch ex As Exception
                Throw New Exception($"dxfUtils.{ Reflection.MethodBase.GetCurrentMethod.Name} - { ex.Message}")
            End Try
        End Function
        Public Shared Function LayoutPointsOnLine(aLine As dxeLine, TargetSpace As Double, Optional CenterOnLine As Boolean = True, Optional EndBuffer As Double = 0.0, Optional AtLeastOne As Boolean = False, Optional aMinSpace As Double? = Nothing) As colDXFVectors
            Dim _rVal As New colDXFVectors
            '#1the line to layout the points on
            '#2the requested distance between points
            '#3flag to center the points on then midpoint of the line
            '
            '^spaces points along the passed line using the indicated spacing
            '~centering the points on the line will return points with the exact spacing.
            '~not centering will use the best fit with the first and last points lying on the lines end points
            If aLine Is Nothing Then Return _rVal
            TargetSpace = Math.Abs(TargetSpace)
            If aMinSpace.HasValue Then
                If TargetSpace < Math.Abs(aMinSpace.Value) Then TargetSpace = Math.Abs(aMinSpace.Value)
            End If
            If EndBuffer <= 0 Then EndBuffer = 0
            Dim llen As Double = aLine.Length - 2 * EndBuffer
            Dim ctr As TVECTOR = aLine.MidPtV
            Dim linedir As TVECTOR = aLine.DirectionV
            Dim sp As TVECTOR = ctr + linedir * -llen / 2

            If llen <= 0 Then
                If AtLeastOne Then _rVal.Add(ctr)
                Return _rVal
            End If
            Dim spc As Double = TargetSpace
            Dim spaces As Integer = Int(llen / spc) + 1
            Dim numpt As Integer = spaces + 1
            If Not CenterOnLine Then
                spc = llen / spaces
            End If

            If numpt = 1 Then

                If CenterOnLine Then _rVal.Add(ctr) Else _rVal.Add(sp)
                Return _rVal
            End If


            Dim offset As TVECTOR = linedir * spc
            Dim pts As New TVECTORS(0)
            Dim v1 As TVECTOR = sp.Clone()
            pts.Add(v1.Clone())
            Do While pts.Count < numpt
                v1 += offset
                pts.Add(v1.Clone())

            Loop
            If CenterOnLine Then
                Dim v2 As TVECTOR = sp.MidPt(v1) ' midpoint of the return set
                Dim flag As Boolean = False
                Dim d1 As Double = 0
                Dim dir As TVECTOR = v2.DirectionTo(ctr, False, flag, d1) 'direction from midpt to line center
                If Not flag Then
                    pts.Translate(dir * d1)
                End If

            End If

            Return New colDXFVectors(pts)

        End Function

        Public Shared Function LayoutPointsOnLineByCount(aLine As dxeLine, TargetCount As Integer, Optional StartBuffer As Double = 0.0, Optional EndBuffer As Double = 0.0) As colDXFVectors
            Dim _rVal As colDXFVectors
            '#1the line to layout the holes on
            '#2the target number of points
            '#4a buffer to apply to the starting end of the line
            '#5a buffer to apply to the ending end of the line
            '^spaces points along the line based on the passed count
            Dim llen As Double
            _rVal = New colDXFVectors
            If aLine Is Nothing Then Return _rVal
            llen = aLine.Length
            If TargetCount <= 0 Then Return _rVal
            If EndBuffer <= 0 Then EndBuffer = 0
            If StartBuffer <= 0 Then StartBuffer = 0
            If EndBuffer > 0.45 * llen Then EndBuffer = 0.45 * llen
            If StartBuffer > 0.45 * llen Then StartBuffer = 0.45 * llen
            Dim spac As Double
            Dim v1 As TVECTOR
            Dim lDir As TVECTOR
            Dim v2 As TVECTOR
            Dim v3 As TVECTOR
            Dim mpt As TVECTOR
            Dim d1 As Double
            Dim aSP As TVECTOR
            Dim aEP As TVECTOR
            llen -= StartBuffer - EndBuffer
            If llen > 0 Then
                lDir = aLine.DirectionV
                aSP = aLine.StartPt.Strukture + lDir * StartBuffer
                aEP = aLine.EndPt.Strukture + lDir * -EndBuffer
                mpt = aSP.Interpolate(aEP, 0.5)
                If TargetCount = 1 Then
                    _rVal.AddV(mpt)
                Else
                    spac = llen / (TargetCount - 1)
                    v1 = aSP
                    Do Until _rVal.Count = TargetCount
                        _rVal.AddV(v1)
                        v1 += lDir * spac
                    Loop
                    v1 = _rVal.ItemVector(1)
                    v2 = _rVal.ItemVector(_rVal.Count)
                    v3 = v1.Interpolate(v2, 0.5)
                    d1 = v3.DistanceTo(mpt, 4)
                    If d1 > 0 Then
                        lDir = v3.DirectionTo(mpt)
                        _rVal.Project(lDir, d1)
                    End If
                End If
            End If
            Return _rVal
        End Function

        Public Shared Function LayoutPointsOnArc(aArc As dxeArc, TargetSpace As Double, Optional CenterOnArc As Boolean = True, Optional EndBuffer As Double = 0.0, Optional AtLeastOne As Boolean = False, Optional aMinSpace As Double? = Nothing) As colDXFVectors
            Dim _rVal As New colDXFVectors
            '#1the line to layout the points on
            '#2the requested distance between points
            '#3flag to center the points on then midpoint of the line
            '
            '^spaces points along the passed line using the indicated spacing
            '~centering the points on the line will return points with the exact spacing.
            '~not centering will use the best fit with the first and last points lying on the lines end points
            If aArc Is Nothing Then Return _rVal
            TargetSpace = Math.Abs(TargetSpace)
            If aMinSpace.HasValue Then
                If TargetSpace < Math.Abs(aMinSpace.Value) Then TargetSpace = Math.Abs(aMinSpace.Value)
            End If
            If EndBuffer <= 0 Then EndBuffer = 0
            Dim llen As Double = aArc.Length - 2 * EndBuffer
            Dim ctr As TVECTOR = aArc.MidPtV

            Dim sp As TVECTOR = aArc.StartPtV


            If llen <= 0 Then
                If AtLeastOne Then _rVal.Add(ctr)
                Return _rVal
            End If
            Dim spc As Double
            Dim spaces As Integer = Int(llen / TargetSpace) + 1
            Dim numpt As Integer = spaces + 1
            If Not CenterOnArc Then
                spc = llen / spaces
            Else
                spc = TargetSpace
            End If

            If numpt = 1 Then

                If CenterOnArc Then _rVal.Add(ctr) Else _rVal.Add(sp)
                Return _rVal
            End If

            Dim span As Double = aArc.SpannedAngle
            Dim angstep As Double = span / numpt
            Dim plane As TPLANE = aArc.PlaneV
            plane.AlignedTo(aArc.CenterV.DirectionTo(sp), dxxAxisDescriptors.X)


            Dim pts As New TVECTORS(0)
            Dim v1 As TVECTOR = sp.Clone()
            Dim axis As TVECTOR = plane.ZDirection

            pts.Add(v1.Clone())
            Do While pts.Count < numpt
                v1.RotateAbout(axis, angstep)
                pts.Add(v1.Clone())

            Loop
            If CenterOnArc Then

                Dim p1 As TVECTOR = pts.Item(1)
                Dim p2 As TVECTOR = pts.Last

                Dim dir1 As TVECTOR = plane.Origin.DirectionTo(p1)
                Dim dir2 As TVECTOR = plane.Origin.DirectionTo(p2)
                Dim ang As Double = dir1.AngleTo(dir1, bReturnDegrees:=True)
                p1 = plane.AngleVector(ang / 2, aArc.Radius, False)
                dir1 = plane.Origin.DirectionTo(p1)
                dir2 = plane.Origin.DirectionTo(ctr)
                ang = dir1.AngleTo(dir1, bReturnDegrees:=True)
                If ang <> 0 Then
                    pts.Rotate(aArc.CenterV, axis, ang)
                End If

            End If

            Return New colDXFVectors(pts)

        End Function

        Public Shared Function LayoutRectangles(aBorder As dxfPolyline, aRectangle As dxfRectangle, aPitchType As dxxPitchTypes, aTarget As Integer, Optional aRotation As Double = 0.0, Optional aEnt As dxfEntity = Nothing, Optional aMultiEnt As dxfEntity = Nothing, Optional aBlockName As String = "", Optional aMinusTol As Integer = 0) As colDXFVectors
            Dim _rVal As New colDXFVectors
            'On Error Resume Next
            If aBorder Is Nothing Then Return _rVal
            If aBorder.Vertices.Count < 2 Then Return _rVal
            If aRectangle Is Nothing Then Return _rVal
            If Not aRectangle.IsDefined(True) Then Return _rVal
            If aTarget <= 0 Then Return _rVal
            aMinusTol = Math.Abs(aMinusTol)
            aBorder.UpdatePath(True)
            'initialize
            Dim bRectangle As dxfRectangle = aRectangle.Clone
            If aRotation <> 0 Then bRectangle.Rotate(aRotation, False)
            Dim tRec As New dxfRectangle(bRectangle.Width, bRectangle.Height)
            Dim bSegs As TBOUNDLOOPS = dxfHatches.BoundLoops_Entity(aBorder, tRec.Strukture) 'get the bordering entities
            If bSegs.Count <= 0 Then Return _rVal 'no border no points
            Dim boundRec As TPLANE = aBorder.BoundingRectangle.Strukture
            If boundRec.Height <= 0 Or boundRec.Width = 0 Then Return _rVal 'no bounds no points
            Dim aRect As TPLANE = bRectangle.Strukture
            Dim diag As Double = bRectangle.Diagonal
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim gProps As New TGRID
            Dim i As Long
            Dim aPlane As TPLANE = boundRec
            Dim aspct As Double = boundRec.Width / boundRec.Height 'w/h
            Dim minh As Double = aRect.Width
            Dim minv As Double = aRect.Height
            Dim H As Double
            Dim V As Double
            Dim ang As Double = aPlane.XDirection.AngleTo(aRect.XDirection, aPlane.ZDirection)
            Dim bLines As TLINES
            Dim cLines As TLINES
            Dim d2 As Double
            Dim bndArea As Double = aBorder.Area
            Dim gPts As TVECTORS
            Dim kPts As TVECTORS
            Dim stepFactor As Double
            Dim bBail As Boolean
            Dim d1 As Double
            tRec.Rotate(ang)
            If aPitchType < dxxPitchTypes.Rectangular Or aPitchType > dxxPitchTypes.InvertedTriangular Then aPitchType = dxxPitchTypes.Rectangular
            'compute the minimum pitchs if the rectangle is not aligned with the planes X and Y axes
            If ang <> 0 Then
                Dim Crns As colDXFVectors = tRec.Corners()
                Dim aLines As TLINES = Crns.ToLineSegments(True)
                Dim iLine As New TLINE(aPlane.XDirection * -diag, aPlane.XDirection * diag)
                Dim intP As TVECTORS = iLine.IntersectionPts(aLines, False, False, True)
                minh = intP.GetDimensions().X
                iLine = New TLINE(aPlane.YDirection * -diag, aPlane.YDirection * diag)
                intP = iLine.IntersectionPts(aLines, False, False, True)
                minv = intP.GetDimensions().Y
                If aPitchType = dxxPitchTypes.InvertedTriangular Or aPitchType = dxxPitchTypes.Triangular Then
                    Crns.Move(-minh)
                    cLines = Crns.ToLineSegments(True)
                    Crns.Move(0.5 * minh, minv)
                    bLines = Crns.ToLineSegments(True)
                    d1 = 0
                    intP = bLines.IntersectionPts(aLines, bFirstPointOnly:=True)
                    If intP.Count > 0 Then
                        For i = 1 To Crns.Count
                            v1 = Crns.ItemVector(i)
                            If tRec.ContainsVector(v1, bSuppressPlaneTest:=True, bSuppressEdgeTest:=True) Then
                                iLine.SPT = v1
                                iLine.EPT = v1 + aPlane.YDirection * diag
                                intP = iLine.IntersectionPts(aLines, False, False, True)
                                If intP.Count > 0 Then
                                    d1 = intP.Item(1).Y - v1.Y
                                    Exit For
                                End If
                            End If
                        Next i
                    End If
                    If d1 = 0 Then
                        tRec.Move(-minh)
                        intP = bLines.IntersectionPts(cLines, bFirstPointOnly:=True)
                        If intP.Count > 0 Then
                            For i = 1 To Crns.Count
                                v1 = Crns.ItemVector(i)
                                If tRec.ContainsVector(v1, bSuppressPlaneTest:=True, bSuppressEdgeTest:=True) Then
                                    iLine.SPT = v1
                                    iLine.EPT = v1 + aPlane.YDirection * diag
                                    intP = iLine.IntersectionPts(cLines, False, False, True)
                                    If intP.Count > 0 Then
                                        d1 = intP.Item(1).Y - v1.Y
                                        Exit For
                                    End If
                                End If
                            Next i
                        End If
                    End If
                    If d1 <> 0 Then
                        minv += d1
                    Else
                        v1 = dxfVectors.FindVertexVector(Crns, dxxPointFilters.AtMinY)
                        iLine.SPT = v1
                        iLine.EPT = v1 + aPlane.YDirection * -diag
                        intP = iLine.IntersectionPts(aLines, False, False, True)
                        d1 = -1
                        If intP.Count > 0 Then
                            v2 = intP.GetVector(dxxPointFilters.AtMaxY)
                            d1 = v1.Y - v2.Y
                        End If
                        intP = iLine.IntersectionPts(cLines, False, False, True)
                        d2 = -1
                        If intP.Count > 0 Then
                            v2 = intP.GetVector(dxxPointFilters.AtMaxY)
                            d2 = v1.Y - v2.Y
                            If d1 < 0 Then d1 = d2
                        End If
                        If d2 < 0 Then d2 = d1
                        If d2 < d1 And d2 > 0 Then d1 = d2
                        minv -= d1
                    End If
                End If
            End If
            H = minh
            V = minv
            If aMinusTol > aTarget Then aMinusTol = 0
            'estimate the pitches to achieve the target
            d1 = Math.Sqrt(bndArea / (aTarget * aspct))
            If d1 > minv Then
                V = d1
                H = aspct * V
            End If
            'set the info of the grid (hatch) request
            gProps.PitchType = aPitchType
            gProps.VerticalPitch = V
            gProps.HorizontalPitch = H
            gProps.Plane = aPlane
            gProps.Rotation = 0
            'request the grid (hatch) points
            gProps.BoundaryLoops = bSegs
            gPts = gProps.GridPoints(Nothing, bRectangle)
            'iterate until the we have at least the target number of points
            If gPts.Count < aTarget Then
                Do Until gPts.Count >= aTarget
                    If gPts.Count > 0 Then stepFactor = gPts.Count / aTarget Else stepFactor = 0.05
                    d1 = V * stepFactor
                    If d1 >= minv Then V = d1 Else V = minv
                    d1 = H * stepFactor
                    If d1 >= minh Then H = d1 Else H = minh
                    gProps.VerticalPitch = V
                    gProps.HorizontalPitch = H
                    gPts = gProps.GridPoints(Nothing, bRectangle)
                    If V <= minv And H <= minh Then Exit Do
                Loop
            End If
            'increase the pitch(reduce the point count) until we are within and acceptable margin over the target
            For i = 1 To 3
                bBail = False
                If i = 1 Then
                    d2 = 0.1
                ElseIf i = 2 Then
                    d2 = 0.05
                Else
                    d2 = 0.025
                End If
                Do Until gPts.Count <= aTarget - aMinusTol ' Until Round(gPts.Count / aTarget, 3) <= 1 + d2
                    stepFactor = 1 + d2
                    Dim kProps As TGRID = gProps
                    kPts = gPts
                    d1 = V * stepFactor
                    If d1 >= minv Then V = d1 Else V = minv
                    d1 = H * stepFactor
                    If d1 >= minh Then H = d1 Else H = minh
                    gProps.VerticalPitch = V
                    gProps.HorizontalPitch = H
                    gPts = gProps.GridPoints(Nothing, bRectangle)
                    If gPts.Count < aTarget - aMinusTol Then
                        H = kProps.HorizontalPitch
                        V = kProps.VerticalPitch
                        gProps = kProps
                        gPts = kPts
                        gProps.HorizontalPitch = H
                        gProps.VerticalPitch = V
                        If i = 3 Then bBail = True
                        Exit Do
                    End If
                    If V <= minv And H <= minh Then
                        bBail = True
                        Exit Do
                    End If
                Loop
                If bBail Then Exit For
            Next i
            'set the return
            _rVal.Populate(gPts)
            If Not aEnt Is Nothing Then
                aMultiEnt = aEnt.Clone
                If _rVal.Count > 0 Then
                    aMultiEnt.MoveFromTo(aMultiEnt.Rectangle.Center, _rVal.Item(1))
                    aMultiEnt.Instances.FromVertices(New TVERTICES(_rVal), aEnt.Plane.Strukture, 1, aRotation)
                End If
            End If
            Return _rVal
        End Function



        Public Shared Function LinesTangentToTwoArcs(aArc As dxeArc, bArc As dxeArc) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1the first arc
            '#2the second arc
            '^two lines that are tangent to the passed arcs
            If aArc IsNot Nothing And bArc IsNot Nothing Then
                Dim aLns As TLINES = dxfUtils.LinesTangentToTwoArcs(aArc.ArcStructure, bArc.ArcStructure)
                For i As Integer = 1 To aLns.Count
                    _rVal.Add(New dxeLine(aLns.Item(i)))
                Next i
            End If
            Return _rVal
        End Function
        Friend Shared Function LinesTangentToTwoArcs(aArc As TARC, bArc As TARC) As TLINES
            '#1the first arc
            '#2the second arc
            '^two lines that are tangent to the passed arcs
            Dim aPlane As TPLANE = aArc.Plane
            Dim aLn As New TLINE(aPlane.Origin, bArc.Plane.Origin)
            Dim aDir As TVECTOR = aLn.Direction
            Dim bDir As TVECTOR = aDir.Clone
            Dim bLn As TLINE = aLn.Clone
            Dim R1 As Double = aArc.Radius
            Dim R2 As Double = bArc.Radius
            bDir.RotateAbout(aPlane.ZDirection, 90)
            aLn.SPT += bDir * R1
            aLn.EPT += bDir * R2
            bLn.SPT += bDir * -R1
            bLn.EPT += bDir * -R2
            aLn.SPT += bDir * R1
            aLn.EPT += bDir * R2
            Return New TLINES(aLn, bLn)
        End Function
        Friend Shared Function Lines_Parallel(aLines As IEnumerable(Of dxfEntity), aBaseLine As dxeLine, Optional aPrecis As Integer = 3, Optional bMustBeDirectionEqual As Boolean = False, Optional bReturnClones As Boolean = False) As colDXFEntities
            Dim _rVal As New colDXFEntities
            '#1the collection of lines
            '#2the line to compare to
            '#3the precision for the comparison
            '#4flag to only return lines that are parrallel and have the same direction as the base line
            '#5flag to return clones rather than the actual members
            '^returns the lines from the collection that are parrallel to the passed base line
            If Not bReturnClones Then _rVal.MaintainIndices = False
            If aBaseLine Is Nothing Then Return _rVal
            If aLines Is Nothing Then Return _rVal
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            Dim bLine As dxeLine
            Dim d1 As TVECTOR = aBaseLine.DirectionV
            Dim aEnt As dxfEntity
            Dim aFlag As Boolean
            Dim bKeep As Boolean
            For i As Integer = 1 To aLines.Count
                aEnt = aLines(i - 1)
                If aEnt.GraphicType = dxxGraphicTypes.Line Then
                    bLine = aEnt
                    bKeep = False
                    If d1.Equals(bLine.DirectionV, True, aPrecis, aFlag) Then
                        If Not aFlag Or (aFlag And Not bMustBeDirectionEqual) Then bKeep = True
                    End If
                    If bKeep Then
                        If Not bReturnClones Then _rVal.Add(bLine) Else _rVal.Add(bLine.Clone)
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Shared Function LoopIndices(aMaxVal As Integer, aStartID As Integer?, aEndID As Integer?, ByRef rStartID As Integer, ByRef rEndID As Integer) As Boolean
            Dim stp As Integer = 1
            Return LoopIndices(aMaxVal, aStartID, aEndID, rStartID, rEndID, Nothing, stp)
        End Function

        Public Shared Function LoopIndices(aMaxVal As Integer, aStartID As Integer?, aEndID As Integer?, ByRef rStartID As Integer, ByRef rEndID As Integer, aReverseOrderBool As Boolean?, ByRef rStep As Integer) As Boolean
            rStartID = -1
            rEndID = -1
            rStep = 1
            If aMaxVal <= 0 Then Return False
            Dim aReverseOrder As Boolean
            If aReverseOrderBool.HasValue Then
                aReverseOrder = aReverseOrderBool.Value
            Else
                If aStartID > aEndID And aEndID > 0 Then
                    aReverseOrder = True
                End If
            End If
            If Not aStartID.HasValue Then aStartID = 1
            If Not aEndID.HasValue Then aEndID = aMaxVal

            If aStartID.Value <= 0 Then aStartID = 1
            If aEndID.Value <= 0 Then aEndID = aMaxVal
            rStartID = TVALUES.LimitedValue(aStartID.Value, 1, aMaxVal, 1)
            rEndID = TVALUES.LimitedValue(aEndID.Value, 1, aMaxVal, aMaxVal)
            TVALUES.SortTwoValues(Not aReverseOrder, rStartID, rEndID)
            If aReverseOrder Then rStep = -1
            Return Math.Abs(rEndID - rStartID + 1) >= 1
        End Function
        Public Shared Sub LoadFonts(Optional aSHPFolderSpec As String = "", Optional aSHPFileSpec As String = "", Optional aOverrideExisting As Boolean = False)
            '^causes the application to create its fonts collection based the systems
            '^true types collection and/or the .shp files in the passed file name

            If Not String.IsNullOrWhiteSpace(aSHPFolderSpec) Then
                If Directory.Exists(aSHPFolderSpec.Trim()) Then dxoFonts.LoadShapeFonts(aSHPFolderSpec.Trim(), aOverrideExisting)
            End If
            If Not String.IsNullOrWhiteSpace(aSHPFileSpec) Then
                If File.Exists(aSHPFileSpec.Trim()) Then dxoFonts.LoadShapeFont(aSHPFileSpec.Trim(), aOverrideExisting)
            End If
        End Sub
        Public Shared Function NearestAnsiScale(aScaleFactor As Double, ByRef rScaleFactor As Double, Optional bMetric As Boolean = False) As String

            '#1the scale factor to round to the nearest ansi scale
            '#2returns the rounded scale factor
            '^rounds the passed scale up to the nearest standard ANSI scale and returns the scale string (i.e. '  4')

            aScaleFactor = Math.Abs(aScaleFactor)
            rScaleFactor = aScaleFactor

            If rScaleFactor <= 0 Then Return "  0"
            Dim aNum As Integer
            Dim aDenom As Integer
            Dim roundedScale As Double = 0
            If Not bMetric Then
                rScaleFactor = dxfMath.RoundTo(aScaleFactor, dxxRoundToLimits.Third, True)
                If rScaleFactor > 0 And rScaleFactor <= 1 Then
                    aNum = 1 : aDenom = 1 : roundedScale = 1
                ElseIf rScaleFactor > 1 And rScaleFactor <= 1.333 Then
                    aNum = 3 : aDenom = 4 : roundedScale = 1.333
                ElseIf rScaleFactor > 1.333 And rScaleFactor <= 2 Then
                    aNum = 1 : aDenom = 2 : roundedScale = 2
                ElseIf rScaleFactor > 2 And rScaleFactor <= 2.667 Then
                    aNum = 3 : aDenom = 8 : roundedScale = 2.666
                ElseIf rScaleFactor > 2.667 And rScaleFactor <= 4 Then
                    aNum = 1 : aDenom = 4 : roundedScale = 4
                ElseIf rScaleFactor > 4 And rScaleFactor <= 5.333 Then
                    aNum = 3 : aDenom = 16 : roundedScale = 5.333
                ElseIf rScaleFactor > 5.333 And rScaleFactor <= 8 Then
                    aNum = 1 : aDenom = 8 : roundedScale = 8
                ElseIf rScaleFactor > 8 And rScaleFactor <= 10.667 Then
                    aNum = 3 : aDenom = 32 : roundedScale = 10.666
                ElseIf rScaleFactor > 10.667 And rScaleFactor <= 16 Then
                    aNum = 1 : aDenom = 16 : roundedScale = 16
                ElseIf rScaleFactor > 16 And rScaleFactor <= 21.333 Then
                    aNum = 3 : aDenom = 64 : roundedScale = 21.333
                ElseIf rScaleFactor > 21.333 And rScaleFactor <= 32 Then
                    aNum = 1 : aDenom = 32 : roundedScale = 32
                ElseIf rScaleFactor > 32 And rScaleFactor <= 42.667 Then
                    aNum = 3 : aDenom = 128 : roundedScale = 42.666
                ElseIf rScaleFactor > 42.667 And rScaleFactor <= 64 Then
                    aNum = 1 : aDenom = 64 : roundedScale = 64
                ElseIf rScaleFactor > 64 And rScaleFactor <= 128 Then
                    aNum = 1 : aDenom = 128 : roundedScale = 128
                ElseIf rScaleFactor > 128 Then
                    aNum = 1 : aDenom = 256 : roundedScale = 256
                Else
                    Return "  0"
                End If
            Else

                rScaleFactor = dxfMath.RoundTo(aScaleFactor, dxxRoundToLimits.One, True)
                If rScaleFactor > 0 And rScaleFactor <= 1 Then
                    aNum = 1 : aDenom = 1 : roundedScale = 1
                ElseIf rScaleFactor > 1 And rScaleFactor <= 2 Then
                    aNum = 1 : aDenom = 2 : roundedScale = 2
                ElseIf rScaleFactor > 2 And rScaleFactor <= 3 Then
                    aNum = 1 : aDenom = 3 : roundedScale = 3
                ElseIf rScaleFactor > 3 And rScaleFactor <= 4 Then
                    aNum = 1 : aDenom = 4 : roundedScale = 4
                ElseIf rScaleFactor > 4 And rScaleFactor <= 5 Then
                    aNum = 1 : aDenom = 5 : roundedScale = 5
                ElseIf rScaleFactor > 5 And rScaleFactor <= 10 Then
                    aNum = 1 : aDenom = 10 : roundedScale = 10
                ElseIf rScaleFactor > 10 And rScaleFactor <= 15 Then
                    aNum = 1 : aDenom = 15 : roundedScale = 15
                ElseIf rScaleFactor > 15 And rScaleFactor <= 20 Then
                    aNum = 1 : aDenom = 20 : roundedScale = 20
                ElseIf rScaleFactor > 20 And rScaleFactor <= 25 Then
                    aNum = 1 : aDenom = 25 : roundedScale = 25
                ElseIf rScaleFactor > 25 And rScaleFactor <= 30 Then
                    aNum = 1 : aDenom = 30 : roundedScale = 30
                ElseIf rScaleFactor > 30 And rScaleFactor <= 35 Then
                    aNum = 1 : aDenom = 35 : roundedScale = 35
                ElseIf rScaleFactor > 35 And rScaleFactor <= 40 Then
                    aNum = 1 : aDenom = 40
                ElseIf rScaleFactor > 40 And rScaleFactor <= 45 Then
                    aNum = 1 : aDenom = 45
                ElseIf rScaleFactor > 45 And rScaleFactor <= 50 Then
                    aNum = 1 : aDenom = 50
                ElseIf rScaleFactor > 50 And rScaleFactor <= 55 Then
                    aNum = 1 : aDenom = 55
                ElseIf rScaleFactor > 55 And rScaleFactor <= 60 Then
                    aNum = 1 : aDenom = 60
                ElseIf rScaleFactor > 60 Then
                    aNum = 1 : aDenom = 65
                Else
                    Return "  0"
                End If
            End If
            If roundedScale > 0 Then
                rScaleFactor = roundedScale
            Else
                rScaleFactor = aDenom / aNum
            End If

            Return $"{aNum}:{ aDenom}"


        End Function
        Friend Shared Function NoLineFeeds(aString As Object) As String

            If aString Is Nothing Then Return String.Empty
            Dim aStr As String = aString.ToString()
            'get rid of bad chars
            aStr = Replace(aStr, vbTab, "")
            aStr = Replace(aStr, vbNullChar, "")
            aStr = Replace(aStr, vbBack, "")
            aStr = Replace(aStr, vbFormFeed, "")
            aStr = Replace(aStr, vbVerticalTab, "")
            'make all returns into a single lfs
            aStr = Replace(aStr, vbCrLf, "")
            aStr = Replace(aStr, vbCr, "")
            'aStr = Replace(aStr, vbNewLine, "")
            aStr = Replace(aStr, vbLf, "")
            Return aStr
        End Function
        Public Shared Function NormalizeAngle(aAngle As Double, Optional bInRadians As Boolean = False, Optional bThreeSixtyEqZero As Boolean = False, Optional bReturnPositive As Boolean = True) As Double
            '#1the angle to normalize
            '#2flag indicating if the passed value is in radians
            '#3flag to return 360 as 0
            '^used to convert an angle to a positive counterclockwise value <= 360 or 2 * pi
            'if radians are passed radians are returned
            Return TVALUES.NormAng(aAngle, bInRadians, bThreeSixtyEqZero, bReturnPositive)
        End Function
        Friend Shared Function NumberPassed(aNumber As Object, ByRef rNumber As Double, Optional aDefault As Double? = Nothing) As Boolean
            If Not aDefault.HasValue Then rNumber = 0 Else rNumber = aDefault.Value
            If aNumber Is Nothing Then Return False
            If TypeOf aNumber IsNot Double Then
                rNumber = TVALUES.To_DBL(aNumber)
            Else
                rNumber = aNumber
            End If
            Return True
        End Function

        Public Shared Function VectorDistanceToLine(aVectorObj As iVector, aLineObj As iLine) As Double
            Dim rDirection As dxfDirection = Nothing
            Return VectorDistanceToLine(aVectorObj, aLineObj, rDirection)
        End Function
        Public Shared Function VectorDistanceToLine(aVectorObj As iVector, aLineObj As iLine, ByRef rDirection As dxfDirection) As Double
            Dim _rVal As Double
            rDirection = Nothing
            If aLineObj Is Nothing Then Return 0
            '#1the line to find the distance to
            '#2returns the orthogonal direction
            '^calculates and returns the orthogonal distance from the vector to the given line
            Try
                Dim aDir As TVECTOR = TVECTOR.Zero
                Dim v1 As TVECTOR = dxfProjections.ToLine(New TVECTOR(aVectorObj), New TLINE(aLineObj), aDir, _rVal)
                rDirection = New dxfDirection(aDir)
            Catch ex As Exception
                Throw New Exception($"dxfUtils.{ Reflection.MethodBase.GetCurrentMethod.Name} - { ex.Message}")
            End Try
            Return _rVal
        End Function

        Public Shared Function OpenFileInSystemApp(aCallingForm As Long, aFileName As String, Optional bMaximizeApp As Boolean = True, Optional aEXEPath As String = "", Optional bSuppressErrors As Boolean = False) As String
            Dim _rVal As String = String.Empty
            If dxfUtils.RunningInIDE Then MessageBox.Show("TODO - FIX THIS")
            '       '#1the calling Form hWnd
            '       '#2the filename to open
            '       '#3flag to request that the started application be maximized
            '       '^used to open a file in it's system defined default application
            '       '~uses the "ShellExecute" windows API call.
            '       '~an message is raised if the API call returns a know error.
            '       'On Error Resume Next
            'Dim retVal As Long
            'Dim winstate As Object
            'Dim errStr As String = String.Empty
            'aEXEPath = Trim(aEXEPath)
            'If aEXEPath <> "" Then
            '    If Not File.Exists(aEXEPath) Then
            '        aEXEPath = ""
            '    Else
            '        aEXEPath = """" & aEXEPath & """"
            '    End If
            'End If
            'winstate = 3
            'If Not bMaximizeApp Then winstate = 9
            'If aEXEPath = "" Then
            '    retVal = ShellExecute(aCallingForm, "open", aFileName, vbNullString, Path.GetDirectoryName(aFileName), winstate)
            '    errStr = GetShellExecuteError(retVal)
            'Else
            '    retVal = ShellExecute(aCallingForm, vbNullString, aEXEPath, """" & aFileName & """", Path.GetDirectoryName(aFileName), winstate)
            'End If
            'If errStr <> "" Then
            '    If Not bSuppressErrors Then
            '  MessageBox.Show($"Unabe To Open '{ aFileName }'  ERROR - {errStr}", "Shell Execute Error", MessageBoxButtons.OK, icon:=MessageBoxIcon.Warning)
            '           Else
            '        _rVal = errStr
            '    End If
            'End If
            Return _rVal
        End Function
        Public Shared Function CreateMTextAlignment(aHAlign As dxxHorizontalJustifications, aVAlign As dxxVerticalJustifications) As dxxMTextAlignments
            If aVAlign = dxxVerticalJustifications.Undefined Then aVAlign = dxxVerticalJustifications.Bottom
            If aHAlign = dxxHorizontalJustifications.Undefined Then aHAlign = dxxHorizontalJustifications.Left
            Dim _rVal As dxxMTextAlignments = dxxMTextAlignments.BottomLeft
            Select Case aHAlign
                Case dxxHorizontalJustifications.Left
                    Select Case aVAlign
                        Case dxxVerticalJustifications.Top
                            _rVal = dxxMTextAlignments.TopLeft
                        Case dxxVerticalJustifications.Center
                            _rVal = dxxMTextAlignments.MiddleLeft
                        Case dxxVerticalJustifications.Bottom
                            _rVal = dxxMTextAlignments.BottomLeft
                    End Select
                Case dxxHorizontalJustifications.Right
                    Select Case aVAlign
                        Case dxxVerticalJustifications.Top
                            _rVal = dxxMTextAlignments.TopRight
                        Case dxxVerticalJustifications.Center
                            _rVal = dxxMTextAlignments.MiddleRight
                        Case dxxVerticalJustifications.Bottom
                            _rVal = dxxMTextAlignments.BottomRight
                    End Select
                Case dxxHorizontalJustifications.Center
                    Select Case aVAlign
                        Case dxxVerticalJustifications.Top
                            _rVal = dxxMTextAlignments.TopCenter
                        Case dxxVerticalJustifications.Center
                            _rVal = dxxMTextAlignments.MiddleCenter
                        Case dxxVerticalJustifications.Bottom
                            _rVal = dxxMTextAlignments.BottomCenter
                    End Select
            End Select
            Return _rVal
        End Function
        Friend Shared Sub ParseRectangleAlignment(aAlignment As dxxRectangularAlignments, ByRef rHAlign As dxxHorizontalJustifications, ByRef rVAlign As dxxVerticalJustifications)
            rHAlign = dxxHorizontalJustifications.Center
            rVAlign = dxxVerticalJustifications.Center
            Select Case aAlignment
                Case dxxRectangularAlignments.BottomLeft, dxxRectangularAlignments.MiddleLeft, dxxRectangularAlignments.TopLeft
                    rHAlign = dxxHorizontalJustifications.Left
                Case dxxRectangularAlignments.BottomRight, dxxRectangularAlignments.MiddleRight, dxxRectangularAlignments.TopRight
                    rHAlign = dxxHorizontalJustifications.Right
            End Select
            Select Case aAlignment
                Case dxxRectangularAlignments.BottomLeft, dxxRectangularAlignments.BottomCenter, dxxRectangularAlignments.BottomRight
                    rVAlign = dxxVerticalJustifications.Bottom
                Case dxxRectangularAlignments.TopCenter, dxxRectangularAlignments.TopLeft, dxxRectangularAlignments.TopRight
                    rVAlign = dxxVerticalJustifications.Top
            End Select
            Return
        End Sub
        Public Shared Function ControlToClipBoard(aControl As Control) As Boolean
            '#1a VB PictureBox (or Form) to copy to the system clipboard
            '^used to copy the image displayed in the passed picturebox to the system clipboard.
            '~no errors raised
            If aControl Is Nothing Then Return False
            Try
                If dxfUtils.CheckProperty(aControl, "Image") Then
                    Dim ctrl As Object = aControl
                    Dim img As Image = ctrl.image
                    Dim aBMP As New Bitmap(img)
                    System.Windows.Forms.Clipboard.SetImage(aBMP)
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                Return False
            End Try
            If dxfUtils.RunningInIDE Then MessageBox.Show("TODO - FIX THIS")
        End Function
        Public Shared Function PointToCircleInterceptPoints(aCircle As dxeArc, aPoint As iVector, Optional aAngle As Double = 0.0) As colDXFVectors
            '#1the Circle to find intercepts on
            '#2a Point within the passed circle used to find the intercept points
            '#3the angle to project a line through the passed point to find the intercepts on the circle
            '^returns the points on a circle where a line drawn through the passed point will intercept the passed circle
            '~assumes the Point and Circle as coplanar on the XY plane.
            Dim _rVal As New colDXFVectors
            If aCircle Is Nothing Then Return _rVal
            Dim aPl As TPLANE = aCircle.PlaneV
            Dim v1 As New TVECTOR(aCircle.Plane, aPoint)
            aPl.Origin = v1

            Dim alist As New List(Of dxfEntity)({New dxeLine(v1, aPl.AngleVector(aAngle, 3 * aCircle.Radius, False))})
            Dim blist As New List(Of dxfEntity)({aCircle})

            dxfIntersections.Points(alist, blist, True, True, aCollector:=_rVal)
            Return _rVal
        End Function
        Friend Shared Function PolarLine(ByRef Point As iVector, ByRef Angle As Double, Distance As Double) As dxeLine
            Dim _rVal As dxeLine = Nothing
            '#1the Point to project from
            '#2the direction to project in
            '#3the distance to project
            '^returns a line with the passed point as its start and with its end point lying the passed distance away at the passed angle
            '~an error is raised if the passed point is undefined
            Try
                If Point Is Nothing Then Throw New Exception("Undefined Entity Detected")
                Dim NLine As dxeLine
                Dim EPT As dxfVector
                Dim SPT As New dxfVector(Point)
                NLine = New dxeLine
                EPT = TPLANE.AngleVec(Point, Angle, Distance)
                If EPT Is Nothing Then Return _rVal
                NLine.StartPt = SPT
                NLine.EndPt = EPT
                _rVal = NLine
            Catch ex As Exception
                Throw New Exception($"dxfUtils.{ Reflection.MethodBase.GetCurrentMethod.Name} - { ex.Message}")
            End Try
            Return _rVal
        End Function
        Public Shared Function PolarVector(aPointXY As iVector, aAngle As Double, aDistance As Double, Optional bInRadians As Boolean = False, Optional aCS As dxfPlane = Nothing) As dxfVector
            '#1the Point to project from
            '#2the direction (angle) to project in
            '#3the distance to project
            '#4flag to indicate if the passed angle is in Radians
            '#5the coordinate system to use
            '^returns a vector located the passed distance away at the passed angle from the passed vector
            '~an error is raised if the passed vector is undefined
            '~the vector is rotated about the z axis of the passed OCS
            Return TPLANE.AngleVec(aPointXY, aAngle, aDistance, bInRadians, aCS)
        End Function


        Friend Shared Sub PrintMatrixToDebug(cMatrix As TMATRIX4)
            Dim bStr As String = String.Empty
            System.Diagnostics.Debug.WriteLine("")
            System.Diagnostics.Debug.WriteLine("--------------------------------------------------------------------------------------------------------------------------")
            System.Diagnostics.Debug.WriteLine(cMatrix.Name)
            Dim aStr As String = Format(cMatrix.A.X, "0.000")
            If aStr.Length < 12 Then aStr = New String(" ", 12 - aStr.Length) & aStr
            bStr += aStr & " |"
            aStr = Format(cMatrix.A.Y, "0.000")
            If aStr.Length < 12 Then aStr = New String(" ", 12 - aStr.Length) & aStr
            bStr += aStr & " |"
            aStr = Format(cMatrix.A.Z, "0.000")
            If aStr.Length < 12 Then aStr = New String(" ", 12 - aStr.Length) & aStr
            bStr += aStr & " |"
            aStr = Format(cMatrix.A.s, "0.000")
            If aStr.Length < 12 Then aStr = New String(" ", 12 - aStr.Length) & aStr
            bStr += aStr
            System.Diagnostics.Debug.WriteLine(bStr)
            bStr = ""
            aStr = Format(cMatrix.B.X, "0.000")
            If aStr.Length < 12 Then aStr = New String(" ", 12 - aStr.Length) & aStr
            bStr += aStr & " |"
            aStr = Format(cMatrix.B.Y, "0.000")
            If aStr.Length < 12 Then aStr = New String(" ", 12 - aStr.Length) & aStr
            bStr += aStr & " |"
            aStr = Format(cMatrix.B.Z, "0.000")
            If aStr.Length < 12 Then aStr = New String(" ", 12 - aStr.Length) & aStr
            bStr += aStr & " |"
            aStr = Format(cMatrix.B.s, "0.000")
            If aStr.Length < 12 Then aStr = New String(" ", 12 - aStr.Length) & aStr
            bStr += aStr
            System.Diagnostics.Debug.WriteLine(bStr)
            bStr = ""
            aStr = Format(cMatrix.C.X, "0.000")
            If aStr.Length < 12 Then aStr = New String(" ", 12 - aStr.Length) & aStr
            bStr += aStr & " |"
            aStr = Format(cMatrix.C.Y, "0.000")
            If aStr.Length < 12 Then aStr = New String(" ", 12 - aStr.Length) & aStr
            bStr += aStr & " |"
            aStr = Format(cMatrix.C.Z, "0.000")
            If aStr.Length < 12 Then aStr = New String(" ", 12 - aStr.Length) & aStr
            bStr += aStr & " |"
            aStr = Format(cMatrix.C.s, "0.000")
            If aStr.Length < 12 Then aStr = New String(" ", 12 - aStr.Length) & aStr
            bStr += aStr
            System.Diagnostics.Debug.WriteLine(bStr)
            bStr = ""
            aStr = Format(cMatrix.D.X, "0.000")
            If aStr.Length < 12 Then aStr = New String(" ", 12 - aStr.Length) & aStr
            bStr += aStr & " |"
            aStr = Format(cMatrix.D.Y, "0.000")
            If aStr.Length < 12 Then aStr = New String(" ", 12 - aStr.Length) & aStr
            bStr += aStr & " |"
            aStr = Format(cMatrix.D.Z, "0.000")
            If aStr.Length < 12 Then aStr = New String(" ", 12 - aStr.Length) & aStr
            bStr += aStr & " |"
            aStr = Format(cMatrix.D.s, "0.000")
            If aStr.Length < 12 Then aStr = New String(" ", 12 - aStr.Length) & aStr
            bStr += aStr
            System.Diagnostics.Debug.WriteLine(bStr)
        End Sub
        Public Shared Function ProjectVectorToArc(aVector As iVector, aArc As dxeArc, Optional aDirection As dxfDirection = Nothing) As dxfVector
            Dim rDistance As Double = 0.0
            Dim rPointIsOnArc As Boolean = False
            Return ProjectVectorToArc(aVector, aArc, aDirection, rDistance, rPointIsOnArc)
        End Function
        Public Shared Function ProjectVectorToArc(aVector As iVector, aArc As dxeArc, aDirection As dxfDirection, ByRef rDistance As Double, ByRef rPointIsOnArc As Boolean) As dxfVector
            '#1the vector to project
            '#2the segment to project to (segments have start and end vectors)
            '#3returns then orthogal distance to the segment from the vector
            '#4returns the orthogonal direction to the segment from the vector
            '#5returns true if then returned vector lines on the passed segment
            '^returns the projection of the passed vector projected orthogonally to the passed line
            If aArc Is Nothing Then Return Nothing
            Return New dxfVector With {.Strukture = dxfProjections.ToArc(New TVECTOR(aVector), aArc, aDirection, rDistance, rPointIsOnArc)}
        End Function
        Public Shared Function ProjectVectorToLine(aVector As iVector, aLineXY As iLine) As dxfVector
            Dim rDistance As Double = 0.0
            Dim rOrthoDirection As dxfDirection = Nothing
            Dim rPointIsOnSegment As Boolean = False
            Return ProjectVectorToLine(aVector, aLineXY, rDistance, rOrthoDirection, rPointIsOnSegment)
        End Function
        Public Shared Function ProjectVectorToLine(aVector As iVector, aLineXY As iLine, ByRef rDistance As Double, ByRef rOrthoDirection As dxfDirection, ByRef rPointIsOnSegment As Boolean) As dxfVector
            '#1the vector to project
            '#2the segment to project to (segments have start and end vectors)
            '#3returns then orthogal distance to the segment from the vector
            '#4returns the orthogonal direction to the segment from the vector
            '#5returns true if then returned vector lines on the passed segment
            '^returns the projection of the passed vector projected orthogonally to the passed line
            Dim dirpos As Boolean = False
            Return New dxfVector(dxfProjections.ToLine(aVector, aLineXY, rDistance, rOrthoDirection, rPointIsOnSegment, dirpos))
        End Function
        Public Shared Function ProjectVectorToPlane(aPointXY As iVector, aPlane As dxfPlane, Optional aDirection As dxfDirection = Nothing) As dxfVector
            Dim rDistance As Double = 0.0
            Return ProjectVectorToPlane(aPointXY, aPlane, aDirection, rDistance)
        End Function
        Public Shared Function ProjectVectorToPlane(aPointXY As iVector, aPlane As dxfPlane, aDirection As dxfDirection, ByRef rDistance As Double) As dxfVector
            '#1the vector to project
            '#2then plane to project to
            '#3 the project direction
            '^projects the passed vector to the passed plane
            '~if the passed direction is nothing then and orthoganal projection is performed
            rDistance = 0
            Dim v2 As TVECTOR
            Try
                If TPLANE.IsNull(aPlane) Then Return Nothing
                Dim v1 As New TVECTOR(aPointXY)
                If aDirection Is Nothing Then
                    v2 = dxfProjections.ToPlane(v1, aPlane.Strukture, rDistance)
                Else
                    v2 = dxfProjections.ToPlane(v1, aPlane.Strukture, aDirection.Strukture, rDistance)
                End If
                Return New dxfVector(v2)
            Catch ex As Exception
                Throw New Exception($"dxfUtils.{ Reflection.MethodBase.GetCurrentMethod.Name} - { ex.Message}")
            End Try
            Return Nothing
        End Function

        Public Shared Function RandomDirection(Optional aPlane As dxfPlane = Nothing) As dxfDirection
            Dim _rVal As dxfDirection = Nothing
            Dim P1 As dxfVector
            P1 = RandomPoint(-10, 10, -10, 10, -10, 10)
            If Not dxfPlane.IsNull(aPlane) Then P1.ProjectToPlane(aPlane)
            Do While P1.IsZero
                P1 = RandomPoint(-10, 10, -10, 10, -10, 10)
                If Not dxfPlane.IsNull(aPlane) Then P1.ProjectToPlane(aPlane)
            Loop
            _rVal.SetStructure(P1.Strukture)
            Return _rVal
        End Function
        Public Shared Function RandomInteger(aLower As Integer, aUpper As Integer) As Integer

            If aLower = aUpper Then
                Return aUpper
            End If

            TVALUES.SortTwoValues(True, aLower, aUpper)
            Randomize()
            Return Int(Rnd() * (aUpper - aLower + 1)) + aLower

        End Function

        Public Shared Function RandomPoint(XLower As Double, XUpper As Double, YLower As Double, YUpper As Double, Optional ZLower As Double = 0.0, Optional ZUpper As Double = 0.0, Optional aPrecis As Integer = 0) As dxfVector
            Return New dxfVector(RandomSingle(XLower, XUpper, aPrecis), RandomSingle(YLower, YUpper, aPrecis), RandomSingle(ZLower, ZUpper, aPrecis))
        End Function
        Public Shared Function RandomPoints(aCount As Integer, XLower As Double, XUpper As Double, YLower As Double, YUpper As Double, Optional ZLower As Double = 0.0, Optional ZUpper As Double = 0.0, Optional aPrecis As Integer = 0) As colDXFVectors
            Dim _rVal As New colDXFVectors
            For i As Integer = 1 To aCount
                _rVal.Add(RandomPoint(XLower, XUpper, YLower, YUpper, ZLower, ZUpper, aPrecis))
            Next i
            Return _rVal
        End Function
        Public Shared Function RandomSingle(Lower As Single, Upper As Single, Optional aPrecis As Integer = 0) As Single
            Dim _rVal As Double
            If aPrecis > 0 Then
                If Math.Round(Lower, aPrecis) = Math.Round(Upper, aPrecis) Then
                    _rVal = Upper
                    Return _rVal
                End If
            Else
                If Upper = Lower Then
                    _rVal = Upper
                    Return _rVal
                End If
            End If
            TVALUES.SortTwoValues(True, Lower, Upper)
            Dim whl As Integer
            Dim deci As Double
            Dim upr As Integer
            Dim lwr As Integer
            upr = Fix(Upper)
            If Math.Abs(Upper) < 1 Then upr += 1
            lwr = Fix(Lower)
            If Math.Abs(Lower) < 1 Then lwr -= 1
            Randomize()
            whl = Int(Rnd() * (upr - lwr + 1)) + lwr 'random integer
            deci = Rnd() 'random single 0 to 1
            _rVal = whl + deci
            If aPrecis > 0 Then _rVal = Math.Round(_rVal, aPrecis)
            Do While _rVal < Lower Or _rVal > Upper
                Randomize()
                whl = Int(Rnd() * (upr - lwr + 1)) + lwr
                deci = Rnd()
                _rVal = whl + deci
                If aPrecis > 0 Then _rVal = Math.Round(_rVal, aPrecis)
            Loop
            Return _rVal
        End Function
        Public Shared Function RandomString(ByRef aLineCount As Integer, aMaxChars As Integer, Optional aMinChars As Integer = 0, Optional aUniformLength As Boolean = False, Optional aAnyPrintableChar As Boolean = False, Optional aSuppressNumbers As Boolean = False) As String
            Dim _rVal As String = String.Empty
            If aLineCount <= 0 Or aMaxChars <= 0 Then Return _rVal
            If aMinChars <= 0 Then aMinChars = 1
            Dim tlen As Integer
            Dim i As Integer
            Dim j As Integer
            Dim P As Integer
            Dim cnum As Integer
            For i = 1 To aLineCount
                If _rVal <> "" Then _rVal += vbLf
                If Not aUniformLength Then
                    tlen = RandomInteger(aMinChars, aMaxChars)
                Else
                    tlen = aMaxChars
                End If
                For j = 1 To tlen
                    If aAnyPrintableChar Then
                        cnum = RandomInteger(33, 126)
                    Else
                        If Not aSuppressNumbers Then
                            P = RandomInteger(1, 3)
                        Else
                            P = RandomInteger(1, 2)
                        End If
                        If P = 1 Then
                            cnum = RandomInteger(65, 90)
                        ElseIf P = 2 Then
                            cnum = RandomInteger(97, 122)
                        Else
                            cnum = RandomInteger(48, 57)
                        End If
                    End If
                    _rVal += Chr(cnum)
                Next j
            Next i
            Return _rVal
        End Function
        Friend Shared Function ReadINI_Boolean(aFileSpec As String, ByRef sSection As String, ByRef sKey As String, Optional DefaultIsTrue As Boolean = False) As Boolean
            '#1the path to an INI formatted text file
            '#2the section in the file to extract a value from
            '#3the name of the value to extract a value from
            '#4flag to indicate that the default value is "True" if the value can't be found
            '^used to extract a boolean value from the passed INI formatted file
            '~returns True only if the upper case of the value found = "TRUE".
            '~does not raise an error if the file or key is not found just returns False.
            '~uses the "GetPrivateProfileString" windows API call.
            If aFileSpec = "" Then aFileSpec = gsINIFilePath()
            Dim bKeyFound As Boolean
            Dim sReturnedValue As String = ReadINI_String(aFileSpec, sSection, sKey, bKeyFound, DefaultIsTrue.ToString).Trim().ToUpper()
            If Not bKeyFound Or sReturnedValue = "" Then
                Return DefaultIsTrue
            Else
                Return sReturnedValue = "TRUE" Or sReturnedValue = "1"
            End If
        End Function
        Friend Shared Function ReadINI_Integer(aFileSpec As String, ByRef sSection As String, ByRef sKey As String, ByRef rKeyFound As Boolean, Optional intDefault As Integer = 0) As Integer
            '#1the path to an INI formatted text file
            '#2the section in the file to extract a value from
            '#3the name of the value to extract a value from
            '#4returns false if the passed key is not found
            '#5an optional default value to return if the key is not found
            '^used to extract an integer value from the passed INI formatted file
            '~returns any numeric string value converted to an integer.
            '~does not raise an error if the file or key is not found just returns the default value.
            '~uses the "GetPrivateProfileString" windows API call.
            Dim sReturnedValue As String = ReadINI_String(aFileSpec, sSection, sKey, rKeyFound, intDefault.ToString().Trim())
            If Not rKeyFound Or sReturnedValue = "" Or Not TVALUES.IsNumber(sReturnedValue) Then
                Return intDefault
            Else
                Try
                    Return TVALUES.To_INT(sReturnedValue)
                Catch ex As Exception
                    Return intDefault
                End Try
            End If
        End Function
        Friend Shared Function ReadINI_Long(aFileSpec As String, sSection As String, sKey As String, Optional aDefault As Long = 0) As Long
            Dim rKeyFound As Boolean = False
            Return ReadINI_Long(aFileSpec, sSection, sKey, aDefault, rKeyFound)
        End Function
        Friend Shared Function ReadINI_Long(aFileSpec As String, sSection As String, sKey As String, ByRef rKeyFound As Boolean, Optional aDefault As Long = 0) As Long
            '#1the path to an INI formatted text file
            '#2the section in the file to extract a value from
            '#3the name of the value to extract a value from
            '#4an optional default value to return if the key is not found
            '#5returns false if the passed key is not found
            '^used to extract a long value from the passed INI formatted file
            '~returns any numeric string value converted to a long.
            '~does not raise an error if the file or key is not found just returns the default value.
            '~uses the "GetPrivateProfileString" windows API call.
            Dim sReturnedValue As String = ReadINI_String(aFileSpec, sSection, sKey, rKeyFound, aDefault.ToString())
            If Not rKeyFound Or sReturnedValue = "" Or Not TVALUES.IsNumber(sReturnedValue) Then
                Return aDefault
            Else
                Try
                    Return TVALUES.ToLong(sReturnedValue)
                Catch ex As Exception
                    Return aDefault
                End Try
            End If
        End Function
        Friend Shared Function ReadINI_Number(aFileSpec As String, ByRef sSection As String, ByRef sKey As String, Optional aDefault As Double = 0.0) As Double
            '#1the path to an INI formatted text file
            '#2the section in the file to extract a value from
            '#3the name of the value to extract a value from
            '#4an optional default value to return if the key is not found
            '^used to extract an Single value from the passed INI formatted file
            '~returns any numeric string value converted to an Single.
            '~does not raise an error if the file or key is not found just returns the default value.
            '~uses the "GetPrivateProfileString" windows API call.
            Dim bKeyFound As Boolean
            Dim sReturnedValue As String = ReadINI_String(aFileSpec, sSection, sKey, bKeyFound, aDefault.ToString())
            If Not bKeyFound Or sReturnedValue = "" Or Not TVALUES.IsNumber(sReturnedValue) Then
                Return aDefault
            Else
                Try
                    Return TVALUES.To_DBL(sReturnedValue)
                Catch ex As Exception
                    Return aDefault
                End Try
            End If
        End Function
        Public Shared Function ReadINI_String(aFileSpec As String, sSection As String, sKey As String, Optional sDefault As String = "") As String
            '#1the path to an INI formatted text file
            '#2the section in the file to extract a value from
            '#3the name of the value to extract a value from
            '#4an optional default value to return if the key is not found
            '#5returns false if the passed key is not found
            '^used to extract an string value from the passed INI formatted file
            '~does not raise an error if the file or key is not found just returns the default value.
            '~uses the "GetPrivateProfileString" windows API call.

            Dim found As Boolean
            If String.IsNullOrWhiteSpace(aFileSpec) Then aFileSpec = gsINIFilePath()
            Dim sReturnedValue As String = APIWrapper.GetPrivateProfileString(sSection, sKey, sDefault, 100000, aFileSpec, found)
            If found Then
                Return sReturnedValue.Trim()
            Else
                Return sDefault
            End If

        End Function
        Public Shared Function ReadINI_String(aFileSpec As String, sSection As String, sKey As String, ByRef rKeyFound As Boolean, Optional sDefault As String = "") As String
            '#1the path to an INI formatted text file
            '#2the section in the file to extract a value from
            '#3the name of the value to extract a value from
            '#4an optional default value to return if the key is not found
            '#5returns false if the passed key is not found
            '^used to extract an string value from the passed INI formatted file
            '~does not raise an error if the file or key is not found just returns the default value.
            '~uses the "GetPrivateProfileString" windows API call.

            If String.IsNullOrWhiteSpace(aFileSpec) Then aFileSpec = gsINIFilePath()
            Dim sReturnedValue As String = APIWrapper.GetPrivateProfileString(sSection, sKey, sDefault, 100000, aFileSpec, rKeyFound)
            Return sReturnedValue.Trim()
        End Function
        Public Shared Function ReadImageFile(aFileSpec As String, Optional aCallingForm As Long = 0) As dxfImage
            Dim _rVal As dxfImage = Nothing
            If dxfUtils.RunningInIDE Then MessageBox.Show("TODO - FIX THIS")
            '            '#1the path to the dxf file to read
            '            '#2the calling form
            '            '#3flag to show the file selection box
            '            '^reads the passed DXF file and sets the files properties to those read in
            '            On Error GoT2 Err:
            '     '**UNUSED VAR** Dim aErr As String = String.Empty
            '     Dim aFrm As frmReadWrite
            '     aFrm = New frmReadWrite
            '     Dim bCanc As Boolean
            '     aFrm.ShowRead(aCallingForm, _rVal, aFileSpec, bCanc)
            '     aFrm = Nothing
            '     Return _rVal
            'Err:
            'MessageBox.Show($"Unabe To Read '{ aFileSpec }'  ERROR - {ex.Message}", "Read Image File", MessageBoxButtons.OK, icon:=MessageBoxIcon.Warning)

            Return _rVal
        End Function
        Friend Shared Function Rectangle_Fit(aContainer As dxfRectangle, aContained As dxfRectangle, Optional aBuffer As Double = 1) As dxfRectangle
            '#1the rectangle that needs to be sized and centered to fit around the contained rectangle
            '#2the rectangle that needs to be contained
            '#3a factor to increased the container by
            '^returns the container rectangle sized and centered to fit around the containded rectangle
            'On Error Resume Next
            If aContainer Is Nothing Then Return Nothing
            If aContained Is Nothing Then Return Nothing
            aBuffer = Math.Abs(aBuffer)
            If aBuffer = 0 Then aBuffer = 1
            Dim aRec As TPLANE = aContainer.Strukture
            Dim bRec As TPLANE = aContained.Strukture
            aRec.Origin = bRec.Origin
            Dim aWd As Double = aRec.Width
            Dim aHt As Double = aRec.Height
            Dim bWd As Double = bRec.Width * aBuffer
            Dim bHt As Double = bRec.Height * aBuffer
            Dim f1 As Double
            If aWd < bWd Then
                If aWd <> 0 Then f1 = bWd / aWd Else f1 = 1
                aWd = f1 * aWd
                aHt = f1 * aHt
            End If
            If aHt < bHt Then
                If aHt <> 0 Then f1 = bHt / aHt Else f1 = 1
                aWd = f1 * aWd
                aHt = f1 * aHt
            End If
            aRec.Width = aWd
            aRec.Height = aHt
            Return New dxfRectangle With {.Strukture = aRec}
        End Function
        Friend Shared Function ReplaceCharacter(ByRef aString As String, ByRef bString As String, ByRef aCharIndex As Integer) As String
            Dim _rVal As String = aString
            If String.IsNullOrEmpty(aString) Then Return aString
            Dim tlen As Integer = aString.Length
            If tlen = 0 Then Return _rVal
            If aCharIndex < 0 Or aCharIndex > tlen Then Return _rVal
            _rVal = $"{Left(aString, aCharIndex - 1) }{ bString }{ Right(aString, tlen - aCharIndex)}"
            Return _rVal
        End Function
        Public Shared Function RetrieveHoles(aEntities As colDXFEntities, Optional aHoleType As dxxHoleTypes = dxxHoleTypes.Undefined, Optional aDiameter As Double? = Nothing, Optional aLength As Double? = Nothing, Optional aDepth As Double? = Nothing, Optional aAngle As Double? = Nothing, Optional aPrecis As Integer = 3, Optional bRemove As Boolean = False, Optional bReturnClones As Boolean = False) As colDXFEntities


            If aEntities Is Nothing Then Return New colDXFEntities
            If aEntities.Count <= 0 Then Return New colDXFEntities
            Dim _rVal As colDXFEntities = aEntities.GetByGraphicType(dxxGraphicTypes.Hole, bReturnClones, bRemove)
            If _rVal.Count <= 0 Then Return _rVal

            Dim dia As Double = -1
            Dim lng As Double = -1
            Dim dpt As Double = -1
            Dim bTestAng As Boolean
            Dim aMem As dxeHole
            Dim bKeep As Boolean
            Dim ang As Double = 0
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 10)
            If aDiameter.HasValue Then
                dia = aDiameter.Value
                If dia <= 0 Then dia = -1
                dia = Math.Round(dia, aPrecis)
            End If
            If aLength.HasValue Then
                lng = aLength.Value
                If lng <= 0 Then lng = -1
                lng = Math.Round(lng, aPrecis)
            End If
            If aDepth.HasValue Then
                dpt = aDepth.Value
                If dpt <= 0 Then dpt = -1
                dpt = Math.Round(dpt, aPrecis)
            End If
            If aAngle.HasValue Then

                bTestAng = True
                ang = TVALUES.NormAng(aAngle.Value, ThreeSixtyEqZero:=True, bReturnPosive:=True)
                ang = Math.Round(ang, aPrecis)
            End If

            If lng = -1 And dia = -1 And dpt = -1 And Not bTestAng Then Return _rVal
            For i As Integer = _rVal.Count To 1 Step -1
                aMem = _rVal.Item(i)
                bKeep = aHoleType = dxxHoleTypes.Undefined Or aMem.HoleType = aHoleType
                If bKeep And dia <> -1 Then
                    If Math.Round(aMem.Diameter, aPrecis) <> dia Then bKeep = False
                End If
                If bKeep And lng <> -1 Then
                    If Math.Round(aMem.Length, aPrecis) <> lng Then bKeep = False
                End If
                If bKeep And dpt <> -1 Then
                    If Math.Round(aMem.Depth, aPrecis) <> dpt Then bKeep = False
                End If
                If bKeep And bTestAng Then
                    If Math.Round(aMem.Rotation, aPrecis) <> ang Then bKeep = False
                End If
                If Not bKeep Then _rVal.RemoveV(i, True)
            Next i
            Return _rVal
        End Function
        Public Shared Function RoundNumTo(aNum As Object, aNearest As dxxRoundToLimits, Optional bRoundUp As Boolean = False, Optional bRoundDown As Boolean = False) As Object
            '#1the number to round
            '#2the limit to round to
            '#3flag to indicate that the value should only be rounded up
            '#4flag to indicate that the value should only be rounded down
            '^used to round a numeric value to the indicated limit.
            '~limits are equated to enums for convenience and clarity and are transformed to numeric values.
            '~i.e. Eighth = 1 means round to the nearest 0.125
            '~if Millimeter or Centimeter is passed then the passed number is
            '~assumed to be in inches and is returned in inches rounded to the metric equivalent
            Return dxfMath.RoundTo(aNum, aNearest, bRoundUp, bRoundDown)
        End Function
        Public Shared Function Rounder(aNum As Object, aFraction As Double) As Double
            '#1the number to round
            '#2the limit to round to
            '^used to round a numeric value to the indicated limit.
            Return dxfMath.Rounder(aNum, aFraction)
        End Function
        Public Shared Function SelectColor(aOwner As IWin32Window, ByRef rCanceled As Boolean, Optional InitColor As dxxColors = dxxColors.Undefined, Optional bNoLogicals As Boolean = False, Optional bNoWindows As Boolean = False, Optional aWin64Color As Color = Nothing) As dxfColor
            Return dxfColors.SelectColor(aOwner, InitColor, bNoLogicals, bNoWindows, rCanceled, aWin64Color)
        End Function
        Public Shared Function SelectFont(ByRef rCanceled As Boolean, Optional aInitFont As String = "", Optional aInitStyle As String = "", Optional bNoShapes As Boolean = False, Optional bNoTrueType As Boolean = False, Optional bNoStyles As Boolean = False, Optional aOwnerForm As IWin32Window = Nothing) As String
            Return dxoFonts.SelectFont(aInitFont, aInitStyle, bNoShapes, bNoTrueType, bNoStyles, rCanceled, aOwnerForm)
        End Function

        Private Shared _SuppressIDE As Boolean
        Public Shared Property SuppressIDE As Boolean
            Get
                Return _SuppressIDE
            End Get
            Set(value As Boolean)
                _SuppressIDE = value
            End Set
        End Property

        Friend Shared Function RunningInIDE() As Boolean
            If _SuppressIDE Then Return False
            Dim _rVal As Boolean
            '^returns True if the current instance is being run in the VB design environment
            Debug.Assert(SetTrue(_rVal) Or True)
            Return _rVal
        End Function
        Friend Shared Function SelectLineWeight(aInitLineWeight As dxxLineWeights, ByRef rSelected As dxxLineWeights, Optional aShowLogicals As Boolean = False, Optional aShowDefault As Boolean = False) As Boolean
            Dim _rVal As Boolean
            'On Error Resume Next
            Dim aTable As TTABLE = TTABLES.LineWeights(Not aShowLogicals, Not aShowDefault)
            Dim aName As String
            Dim idx As Integer

            Dim aFlg As Boolean
            rSelected = aInitLineWeight
            Dim aEntry As TTABLEENTRY = aTable.GetByPropertyValue(70, aInitLineWeight, aStringCompare:=False, rIndex:=idx)
            aName = ShowInput_SelectFromList(aTable.Names.ToStringList, aFlg, aEntry.Name, "Line Weights", "Select Lineweight:", aAllowNullInput:=False, aEqualReturnCancel:=True)
            If Not aFlg Then
                aEntry = aTable.Entry(aName)
                If aEntry.Index >= 0 Then

                    rSelected = aEntry.Props.GCValueL(70)
                    _rVal = rSelected <> aInitLineWeight
                End If
            End If
            Return _rVal
        End Function

        Public Shared Function SetExtremeOrdinates(aVectors As colDXFVectors, aOrdinate As Double, Optional aOrdinateToAffect As dxxOrdinateDescriptors = dxxOrdinateDescriptors.X, Optional aCS As dxfPlane = Nothing, Optional bMinimums As Boolean = False) As colDXFVectors
            Dim _rVal As colDXFVectors
            '1the vectos to work on
            '#2the value to use as the maximum for the indicated ordinate
            '#3the ordinate to test and affect
            '#4a system to use
            '#5flag to set the affected members to 0 radius
            '^set any point in the collections ordinate to the passed value if it is currently less than the passed value
            '~i.e. if arg1 = -10 and arg2 = "X" then any point with an X value less than -10 will be moved to X = -10.
            '~returns the points affected
            _rVal = New colDXFVectors With {.MaintainIndices = False}
            If aVectors Is Nothing Then Return _rVal
            Dim aMem As dxfVector
            Dim i As Integer
            Dim v1 As TVECTOR
            Dim aPl As New TPLANE("")
            If aCS IsNot Nothing Then aPl = New TPLANE(aCS)
            For i = 1 To aVectors.Count
                aMem = aVectors.Item(i)
                v1 = aMem.Strukture
                If aCS IsNot Nothing Then v1 = v1.WithRespectTo(aPl)
                Select Case aOrdinateToAffect
                    Case dxxOrdinateDescriptors.Y
                        If (Not bMinimums And v1.Y > aOrdinate) Or (bMinimums And v1.Y < aOrdinate) Then
                            If aCS IsNot Nothing Then v1 = aPl.Vector(v1.X, aOrdinate, v1.Z) Else v1.Y = aOrdinate
                            aMem.Strukture = v1
                            _rVal.Add(aMem)
                        End If
                    Case dxxOrdinateDescriptors.Z
                        If (Not bMinimums And v1.Z > aOrdinate) Or (bMinimums And v1.Z < aOrdinate) Then
                            If aCS IsNot Nothing Then v1 = aPl.Vector(v1.X, v1.Y, aOrdinate) Else v1.Z = aOrdinate
                            aMem.Strukture = v1
                            _rVal.Add(aMem)
                        End If
                    Case Else
                        If (Not bMinimums And v1.X > aOrdinate) Or (bMinimums And v1.X < aOrdinate) Then
                            If aCS IsNot Nothing Then v1 = aPl.Vector(aOrdinate, v1.Y, v1.Z) Else v1.X = aOrdinate
                            aMem.Strukture = v1
                            _rVal.Add(aMem)
                        End If
                End Select
            Next i
            Return _rVal
        End Function
        Private Shared Function SetTrue(ByRef Value As Boolean) As Boolean
            Dim _rVal As Boolean
            '^used by RunningInIDE to determine if the current instance is running in the VB design environment
            Value = True
            Return _rVal
        End Function
        Friend Shared Sub ShowTextFile(aCallingForm As Long, aFileSpec As String, bShowErrors As Boolean)
            Try
                aFileSpec = Trim(aFileSpec)
                If aFileSpec = "" Then Return
                Dim apppath As String = String.Empty
                Dim eStr As String = String.Empty
                If File.Exists(aFileSpec) Then
                    apppath = "c:\Program Files\EditPlus 2\editplus.exe"
                    If Not File.Exists(apppath) Then apppath = ""
                    eStr = OpenFileInSystemApp(aCallingForm, aFileSpec, True, apppath, True)
                Else
                    Throw New Exception("File Not Found")
                End If
                If eStr <> "" Then Throw New Exception(eStr)
            Catch ex As Exception
                If bShowErrors Then MessageBox.Show($"An Error Occured Opening  '{ aFileSpec }' In System Text Editor. ERROR - {ex.Message}", "View Text File", MessageBoxButtons.OK, icon:=MessageBoxIcon.Warning)

            End Try
        End Sub
        Friend Shared Function SortSegments(ByRef aSegments As colDXFEntities, Optional sMaxGap As Double = 0.08) As List(Of colDXFEntities)
            Dim _rVal As New List(Of colDXFEntities)
            If aSegments Is Nothing Then Return _rVal

            Dim aSeg As dxfEntity = Nothing
            Dim d1 As Double
            Dim maxD As Double
            Dim v1 As TVECTOR
            Dim sKeep As colDXFEntities = Nothing
            Dim bFound As Boolean
            sMaxGap = Math.Abs(sMaxGap)
            Dim sSegs As List(Of dxfEntity) = aSegments.ArcsAndLines(False, False, False)
            bFound = False
            Do Until sSegs.Count = 0
                If Not bFound Then
                    sKeep = New colDXFEntities
                    aSeg = sSegs.Item(0)
                    sSegs.RemoveAt(0)
                    sKeep.Add(aSeg)
                End If
                bFound = False
                maxD = Double.MaxValue
                Dim idx As Integer = 0
                v1 = aSeg.DefinitionPoint(dxxEntDefPointTypes.EndPt).Strukture


                For i As Integer = 1 To sSegs.Count
                    Dim bSeg As dxfEntity = sSegs.Item(i - 1)
                    d1 = v1.DistanceTo(bSeg.DefinitionPoint(dxxEntDefPointTypes.StartPt).Strukture)
                    If d1 < maxD Then
                        maxD = d1
                        idx = i
                    End If
                Next i
                If maxD <= sMaxGap Then
                    aSeg = sSegs.Item(idx - 1)
                    sSegs.Remove(aSeg)
                    sKeep.Add(aSeg)
                    bFound = True
                    If sSegs.Count <= 0 Then
                        _rVal.Add(sKeep)
                        Exit Do
                    End If
                Else
                    _rVal.Add(sKeep)
                End If
            Loop
            Return _rVal
        End Function


        Public Shared Function StringContainsCharacters(ByRef ioString As String, aCharacters As String, Optional bCaseSensitive As Boolean = False, Optional bReplace As Boolean = False, Optional sReplaceWith As Char? = Nothing) As Boolean

            '#1the string to search
            '#2the sting of characters to look for
            '#3flag to search in a case sensitve way
            '^returns True if any of the charcters in the second string occur in the first string.
            If ioString Is Nothing Or aCharacters Is Nothing Then Return False
            If aCharacters.Length <= 0 Or ioString.Length <= 0 Then Return False
            Dim _rVal As Boolean = False
            Dim schars As List(Of Char) = ioString.ToCharArray().ToList()
            Dim cchars As List(Of Char) = aCharacters.ToCharArray().ToList()

            If sReplaceWith.HasValue Then
                If cchars.IndexOf(sReplaceWith.Value) >= 0 Then
                    sReplaceWith = Nothing
                End If
            End If
            For Each cchr As Char In cchars
                Dim lookfor As Char = cchr
                Dim idx As Integer = schars.IndexOf(lookfor)
                If idx < 0 And Not bCaseSensitive Then
                    lookfor = Char.ToUpper(lookfor)
                    idx = schars.IndexOf(lookfor)
                End If

                Do While idx >= 0

                    _rVal = True
                    If Not bReplace Then Return True
                    If Not sReplaceWith.HasValue Then
                        schars.RemoveAt(idx)
                    Else
                        schars(idx) = sReplaceWith.Value
                    End If

                    If schars.Count <= 0 Then Exit Do
                    idx = schars.IndexOf(lookfor)
                Loop
            Next
            If bReplace And _rVal Then
                ioString = ""
                For Each schr As Char In schars
                    ioString += schr
                Next
            End If
            Return _rVal
        End Function
        Public Shared Function StringContainsOtherCharacters(ByRef ioString As String, aCharacters As String, Optional bCaseSensitive As Boolean = False, Optional bReplace As Boolean = False, Optional sReplaceWith As Char? = Nothing) As Boolean
            Dim rBadChars As String = String.Empty
            Return StringContainsOtherCharacters(ioString, aCharacters, bCaseSensitive, bReplace, rBadChars, sReplaceWith)
        End Function
        Public Shared Function StringContainsOtherCharacters(ByRef ioString As String, aCharacters As String, bCaseSensitive As Boolean, bReplace As Boolean, ByRef rBadChars As String, Optional sReplaceWith As Char? = Nothing) As Boolean
            '#1the string to search
            '#2the string of characters to look for
            '#3flag to search in a case sensitve way
            '^returns True if any characters other than those in the second string occur in the first string.


            rBadChars = ""
            If ioString Is Nothing Or aCharacters Is Nothing Then Return False
            If aCharacters.Length <= 0 Or ioString.Length <= 0 Then Return False
            Dim _rVal As Boolean = False
            Dim schars As List(Of Char) = ioString.ToCharArray().ToList()
            Dim cchars As List(Of Char) = aCharacters.ToCharArray().ToList()
            Dim cReplaceIDS As New List(Of Integer)

            For Each schr As Char In schars
                Dim lookfor As Char = schr
                Dim idx As Integer = cchars.IndexOf(lookfor)
                If idx < 0 And Not bCaseSensitive Then
                    lookfor = Char.ToUpper(schr)
                    idx = cchars.IndexOf(lookfor)
                End If
                If idx <= 0 Then
                    _rVal = True
                    cReplaceIDS.Add(idx)
                    If rBadChars.IndexOf(schr) < 0 Then
                        rBadChars += schr
                    End If
                End If
            Next
            If Not _rVal Then Return _rVal
            If bReplace Then
                ioString = ""
                For i As Integer = 0 To schars.Count - 1
                    If cReplaceIDS.IndexOf(i) < 0 Then
                        ioString += schars(i)
                    Else
                        If sReplaceWith.HasValue Then
                            ioString += sReplaceWith.Value
                        End If
                    End If
                Next

            End If
            Return _rVal
        End Function
        Public Shared Function TempFileSpec(aExtension As String) As String
            'Create a buffer
            'Get a temporary filename
            Dim aPth As String = Path.GetTempPath
            If aPth = "" Then aPth = "c:\"

            'Remove all the unnecessary chr$(0)'s
            If Not String.IsNullOrWhiteSpace(aExtension) Then
                aExtension = aExtension.Trim()
            Else
                aExtension = ""
            End If
            If aExtension <> "" Then
                If Left(aExtension, 1) <> "." Then aExtension = $".{aExtension}"
            End If
            Dim sTemp As String = dxfUtils.TempFileName(aExtension) ' APIWrapper.GetTempFileName(aPth, "dxfGraphics", 0)
            Dim _rVal As String = Path.Combine(aPth, sTemp)
            'Set the file attributes
            APIWrapper.SetFileAttributes(_rVal, Vanara.PInvoke.FileFlagsAndAttributes.FILE_ATTRIBUTE_TEMPORARY)
            Return _rVal
        End Function

        Public Shared Function TempFileName(Optional aExtension As String = "") As String
            Dim _rVal As String = Path.GetTempFileName()
            If Not String.IsNullOrWhiteSpace(aExtension) Then _rVal = _rVal.Replace(".tmp", aExtension.Trim().ToLower())
            Return _rVal
        End Function

        Friend Shared Function TextAlignToRecAlign(aAlignm As dxxMTextAlignments) As dxxRectanglePts

            Select Case aAlignm
                Case dxxMTextAlignments.BaselineLeft
                    Return dxxRectanglePts.BaselineLeft
                Case dxxMTextAlignments.BaselineMiddle
                    Return dxxRectanglePts.BaselineCenter
                Case dxxMTextAlignments.BaselineRight
                    Return dxxRectanglePts.BaselineRight
                Case dxxMTextAlignments.BottomCenter
                    Return dxxRectanglePts.BottomCenter
                Case dxxMTextAlignments.BottomLeft
                    Return dxxRectanglePts.BottomLeft
                Case dxxMTextAlignments.BottomRight
                    Return dxxRectanglePts.BottomRight
                Case dxxMTextAlignments.Fit
                    Return dxxRectanglePts.BaselineLeft
                Case dxxMTextAlignments.MiddleCenter
                    Return dxxRectanglePts.MiddleCenter
                Case dxxMTextAlignments.MiddleLeft
                    Return dxxRectanglePts.MiddleLeft
                Case dxxMTextAlignments.MiddleRight
                    Return dxxRectanglePts.MiddleRight
                Case dxxMTextAlignments.TopCenter
                    Return dxxRectanglePts.TopCenter
                Case dxxMTextAlignments.TopLeft
                    Return dxxRectanglePts.TopLeft
                Case dxxMTextAlignments.TopRight
                    Return dxxRectanglePts.TopRight
                Case Else
                    Return dxxRectanglePts.BaselineLeft
            End Select

        End Function
        Friend Shared Function FileIsOpen(aFileName As FileInfo) As Boolean
            Dim stream As FileStream = Nothing
            Try
                If Not File.Exists(aFileName.FullName) Then Return False
                stream = File.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None)
                Return False
            Catch ex As Exception
                If TypeOf ex Is IOException AndAlso IsFileLocked(ex) Then
                    ' do something here, either close the file if you have a handle, show a msgbox, retry  or as a last resort terminate the process - which could cause corruption and lose data
                End If
                Return True
            Finally
                If stream IsNot Nothing Then stream.Close()

            End Try



        End Function
        Private Shared Function IsFileLocked(exception As Exception) As Boolean
            Dim errorCode As Integer = Marshal.GetHRForException(exception) And ((1 << 16) - 1)
            Return errorCode = 32 OrElse errorCode = 33
        End Function
        Friend Shared Function TextOutputProps(aText As String, Optional aGC As Integer = 1, Optional bGC As Integer = 3) As TPROPERTIES
            Dim _rVal As New TPROPERTIES
            '^used internally to added AutoCAD aMText format codes to the DXFOutput
            'On Error Resume Next
            Dim i As Integer
            Dim ln As String
            Dim aStr As String = String.Empty
            If Not String.IsNullOrEmpty(aText) Then aStr = aText
            'now break into 250 char chunks
            If aStr.Length < 250 Then
                _rVal.Add(New TPROPERTY(aGC, aStr, "Text String", dxxPropertyTypes.dxf_String))
            Else
                i = 0
                Do Until aStr.Length <= 0
                    i += 1
                    If aStr.Length > 250 Then
                        ln = aStr.Substring(0, 250)
                        aStr = Right(aStr, aStr.Length - 250)
                    Else
                        ln = aStr
                    End If
                    If i = 1 Then
                        _rVal.Add(New TPROPERTY(aGC, ln, "Text String", dxxPropertyTypes.dxf_String))
                    Else
                        _rVal.Add(New TPROPERTY(bGC, ln, "Text String", dxxPropertyTypes.dxf_String))
                    End If
                    If Len(ln) < 250 Then Exit Do
                Loop
            End If
            If _rVal.Count = 0 Then _rVal.Add(New TPROPERTY(aGC, "", "Text String", dxxPropertyTypes.dxf_String))
            Return _rVal
        End Function

        Public Shared Function TrimPolygonWithLine(Polygon As dxePolygon, TrimLineObj As iLine, RefPtXY As iVector, Optional LineIsInfinite As Boolean = True, Optional bUseLineForGaps As Boolean = False, Optional bTrimAddSegs As Boolean = True) As dxePolygon
            Dim rTrimPerformed As Boolean = False
            Dim rKeepSegs As colDXFEntities = Nothing
            Dim rDiscardSegs As colDXFEntities = Nothing
            Return TrimPolygonWithLine(Polygon, TrimLineObj, RefPtXY, LineIsInfinite, rTrimPerformed, rKeepSegs, rDiscardSegs, bUseLineForGaps, bTrimAddSegs)
        End Function
        Public Shared Function TrimPolygonWithLine(Polygon As dxePolygon, TrimLineObj As iLine, RefPtXY As iVector, LineIsInfinite As Boolean, ByRef rTrimPerformed As Boolean, ByRef rKeepSegs As colDXFEntities, ByRef rDiscardSegs As colDXFEntities, Optional bUseLineForGaps As Boolean = False, Optional bTrimAddSegs As Boolean = True) As dxePolygon
            '#1the polygon to trim
            '#2the line to trim the polygon with
            '#3the point that indicates the side of the line to keep
            '#4flag to treat the passed line as infinite
            '#5return flag indicating if any actual trimming was performed
            '^returns the trimmed version of the passed polygon
            Try
                rKeepSegs = New colDXFEntities
                rDiscardSegs = New colDXFEntities
                Return dxfBreakTrimExtend.trim_Polygon_Line(Polygon, TrimLineObj, RefPtXY, LineIsInfinite, rTrimPerformed, rKeepSegs, rDiscardSegs, bUseLineForGaps, bTrimAddSegs)
            Catch ex As Exception
                Throw New Exception($"dxfUtils.{ Reflection.MethodBase.GetCurrentMethod.Name} - { ex.Message}")
            End Try
            Return Nothing
        End Function
        Public Shared Sub TrimPolygonsOrtho(aPgons As colDXFEntities, aTrimType As dxxTrimTypes, TrimCoordinate As Double, Optional rTrimPerformed As Boolean? = False, Optional bDoAddSegs As Boolean = True, Optional bDoSubPGons As Boolean = False)
            dxfBreakTrimExtend.trim_Polygons_Ortho(aPgons, aTrimType, TrimCoordinate, rTrimPerformed, bDoAddSegs, bDoSubPGons)
        End Sub
        Public Shared Sub TrimPolylineOrtho(aPlines As colDXFEntities, aTrimType As dxxTrimTypes, TrimCoordinate As Double, rTrimPerformed As Boolean?)
            dxfBreakTrimExtend.trim_Polygons_Ortho(aPlines, aTrimType, TrimCoordinate, rTrimPerformed)
        End Sub
        Public Shared Function TrimSegmentsWithPolyline(aSegments As colDXFEntities, aPolyline As dxfPolyline, Optional rTrimmedParts As colDXFEntities = Nothing) As colDXFEntities
            Return dxfBreakTrimExtend.trim_SegmentsWithPolyline(aSegments, aPolyline, rTrimmedParts)
        End Function
        Public Shared Function TrimSegmentsWithPolylines(aSegments As colDXFEntities, aPolygons As Object, Optional KeepInteriors As Boolean = True, Optional KeepExteriors As Boolean = False) As colDXFEntities
            Return dxfBreakTrimExtend.trim_SegmentsWithPolylines(aSegments, aPolygons, KeepInteriors, KeepExteriors)
        End Function
        Friend Shared Function UnfilletedVertices(aVertices As colDXFVectors, Optional bClosed As Boolean = False, Optional bRemoveInLineVertices As Boolean = False, Optional aMaxRadius As Double = 0.0) As colDXFVectors
            Dim _rVal As New colDXFVectors
            If aVertices Is Nothing Then Return _rVal

            Dim sp1 As TVERTEX
            Dim sp2 As TVERTEX
            Dim ep1 As TVERTEX
            Dim ep2 As TVERTEX
            Dim aFlag As Boolean
            Dim ip As TVECTOR
            Dim v1 As dxfVector

            Dim rad As Double
            Dim inv As Boolean
            If aMaxRadius <= 0 Then aMaxRadius = -2.6E+26
            aMaxRadius = Math.Round(aMaxRadius, 5)
            For i As Integer = 1 To aVertices.Count
                Dim P1 As dxfVector = aVertices.Item(i)
                ep1 = P1.VertexV
                sp1 = aVertices.ItemVertex(i - 1)
                sp2 = aVertices.ItemVertex(i + 1)
                ep2 = aVertices.ItemVertex(i + 2)
                v1 = Nothing
                rad = 0
                inv = 0
                If ep1.Radius <> 0 Then
                    If Math.Round(ep1.Radius, 5) > aMaxRadius Then
                        v1 = _rVal.AddV(ep1.Vector)
                        rad = ep1.Radius
                        inv = ep1.Inverted
                    Else
                        If sp1.Radius = 0 And sp2.Radius = 0 Then
                            Dim aLine As New TLINE(sp1.Vector, ep1.Vector)
                            Dim bLine As New TLINE(sp2.Vector, ep2.Vector)
                            ip = aLine.IntersectionPt(bLine, aFlag)
                            If aFlag Then
                                v1 = _rVal.AddV(ip)
                            Else
                                v1 = _rVal.AddV(ep1.Vector)
                            End If
                            i += 1
                        Else
                            v1 = _rVal.AddV(ep1.Vector)
                        End If
                    End If
                Else
                    If bRemoveInLineVertices Then
                        If sp1.Radius = 0 Then
                            If Not sp1.Vector.DirectionTo(ep1.Vector).Equals(ep1.Vector.DirectionTo(sp2.Vector), 4) Then
                                v1 = _rVal.AddV(ep1.Vector)
                            End If
                        Else
                            v1 = _rVal.AddV(ep1.Vector)
                        End If
                    Else
                        v1 = _rVal.AddV(ep1.Vector)
                    End If
                End If
                If v1 IsNot Nothing Then
                    v1.Vars = P1.Vars
                    If rad > 0 Then
                        v1.VertexRadius = rad
                        v1.Inverted = inv
                    End If
                End If
            Next i
            Return _rVal
        End Function
        Public Shared Function UniqueChars(aString As Object) As String
            If aString Is Nothing Then Return String.Empty
            Dim _rVal As String = String.Empty
            Dim aStr As String = aString.ToString()
            For i As Integer = 1 To aStr.Length
                Dim aChr As String = aStr.Substring(i - 1, 1)
                If _rVal.IndexOf(aChr) < 0 Then
                    _rVal += aChr
                End If
            Next i
            Return _rVal
        End Function


        Public Shared Function UserHasAutoCAD() As Boolean
            '^flag which tells the client if the host system has AutoCAD installed
            Return dxfAcad.UserHasAutoCAD()
        End Function
        ''' <summary>
        ''' Checks the passed name to see if it is a valid block name.
        ''' </summary>
        ''' <param name="ioBlockName">the name of the block</param>
        ''' <param name="bFixIt">flag to fix the passed string</param>
        ''' <param name="bAllowNull">flag to treat null strings as valid</param>
        ''' <param name="bApplyIndices">flag to append an index to the name to make it unique if the name already exists in the passed block list.</param>
        ''' <param name="aBlocks">a list of blocks to test for the uniqueness of the passed name</param>
        ''' <returns></returns>
        Public Shared Function ValidateBlockName(ByRef ioBlockName As String, Optional bFixIt As Boolean = False, Optional bAllowNull As Boolean = False, Optional bApplyIndices As Boolean = False, Optional aBlocks As colDXFBlocks = Nothing) As Boolean
            Dim rErrorString As String = String.Empty
            Return ValidateBlockName(ioBlockName, rErrorString, bFixIt, bAllowNull, bApplyIndices, aBlocks)
        End Function

        ''' <summary>
        ''' Checks the passed name to see if it is a valid block name.
        ''' </summary>
        ''' <param name="ioBlockName">the name of the block</param>
        ''' <param name="rErrorString">returns error string if a problem is detected</param>
        ''' <param name="bFixIt">flag to fix the passed string</param>
        ''' <param name="bAllowNull">flag to treat null strings as valid</param>
        ''' <param name="bApplyIndices">flag to append an index to the name to make it unique if the name already exists in the passed block list.</param>
        ''' <param name="aBlocks">a list of blocks to test for the uniqueness of the passed name</param>
        ''' <returns></returns>
        Public Shared Function ValidateBlockName(ByRef ioBlockName As String, ByRef rErrorString As String, Optional bFixIt As Boolean = False, Optional bAllowNull As Boolean = False, Optional bApplyIndices As Boolean = False, Optional aBlocks As colDXFBlocks = Nothing) As Boolean
            rErrorString = String.Empty
            Dim bN As String = ioBlockName
            If bFixIt Then bN = bN.Trim()
            If String.IsNullOrWhiteSpace(ioBlockName) Then
                If Not bAllowNull Then
                    rErrorString = "Block Names Must Not Be Null Strings."
                    Return False
                End If
            End If
            If bN.Substring(0, 1) = " " Or bN.Substring(bN.Length - 1, 1) = " " Then
                If Not bFixIt Then
                    rErrorString = $"Block Names Cannot Have Leading Or trailing Spaces Or Contain '{dxfGlobals.BadBlockChars}'"
                    Return False
                Else
                    bN = bN.Trim()
                End If

            End If
                Dim aStar As String
            If bN.StartsWith("*") Then
                aStar = "*"
                bN = bN.Substring(1, bN.Length - 1).Trim()
            Else
                aStar = String.Empty
            End If
            If StringContainsCharacters(bN, dxfGlobals.BadBlockChars, bCaseSensitive:=True, bFixIt) Then
                If Not bFixIt Then
                    rErrorString = $"An Attempt Was Made To Add a Block With An Invalid Name. Block Names Cannot Have Leading Or trailing Spaces Or Contain '{dxfGlobals.BadBlockChars}'"
                    Return False
                End If

            End If
            ioBlockName = $"{aStar}{bN}"
            If bApplyIndices And aBlocks IsNot Nothing Then
                If aBlocks.BlockExists(ioBlockName) Then
                    Dim j As Integer = 1
                    Do While aBlocks.BlockExists($"{ioBlockName}_{j}")
                        j += 1
                    Loop
                    ioBlockName = $"{ioBlockName}_{j}"
                End If
            End If
            Return True
        End Function
        Public Shared Function FileIsInUse(aFileName As String) As Boolean
            Return APIWrapper.FileIsInUse(aFileName)
        End Function
        Friend Shared Function ValidateFileName(aFileSpec As String) As Boolean
            Dim rFileType As dxxFileTypes = dxxFileTypes.DXF
            Dim rExtension As String = String.Empty
            Dim rIsACADFile As Boolean = False
            Dim rValidExtensions As String = String.Empty
            Return ValidateFileName(aFileSpec, rFileType, rExtension, rIsACADFile, rValidExtensions)
        End Function
        Friend Shared Function ValidateFileName(aFileSpec As String, ByRef rFileType As dxxFileTypes, ByRef rExtension As String, ByRef rIsACADFile As Boolean, ByRef rValidExtensions As String) As Boolean
            rExtension = ""
            rIsACADFile = False
            rFileType = dxxFileTypes.DXF
            rValidExtensions = "DXF,DWG,DWF,DXT"
            If String.IsNullOrWhiteSpace(aFileSpec) Then Return False
            aFileSpec = aFileSpec.Trim()
            If Len(aFileSpec) < 4 Then Return False
            Dim aExt As String = Path.GetExtension(aFileSpec)
            Select Case aExt.ToUpper
                Case ".DWG"
                    rFileType = dxxFileTypes.DWG
                    rExtension = "dwg"
                    rIsACADFile = True
                    Return True
                Case ".DXF"
                    rExtension = "dxf"
                    rFileType = dxxFileTypes.DXF
                    rIsACADFile = True
                    Return True
                Case ".DWF"
                    rExtension = "dwf"
                    rFileType = dxxFileTypes.DWF
                    rIsACADFile = True
                    Return True
                Case ".DXT"
                    rFileType = dxxFileTypes.DXT
                    rExtension = "dxt"
                    Return True
            End Select
            Return False
        End Function
        Friend Shared Function ValidateFileType(aFileType As dxxFileTypes, Optional aDefault As dxxFileTypes = dxxFileTypes.Undefined) As dxxFileTypes
            Dim rExtension As String = String.Empty
            Dim rIsACADFile As Boolean = False
            Return ValidateFileType(aFileType, rExtension, rIsACADFile, aDefault)
        End Function
        Friend Shared Function ValidateFileType(aFileType As dxxFileTypes, ByRef rExtension As String, ByRef rIsACADFile As Boolean, Optional aDefault As dxxFileTypes = dxxFileTypes.Undefined) As dxxFileTypes
            Dim _rVal As dxxFileTypes = aFileType
            If aDefault >= 0 And aFileType < 0 Then aFileType = aDefault
            rIsACADFile = False
            Select Case aFileType
                Case dxxFileTypes.DWG
                    rIsACADFile = True
                Case dxxFileTypes.DXF
                    rIsACADFile = True
                Case dxxFileTypes.DWF
                    rIsACADFile = True
                Case dxxFileTypes.DXT
                Case Else
                    _rVal = dxxFileTypes.DXF
                    rIsACADFile = True
            End Select
            rExtension = dxfEnums.PropertyName(_rVal).ToLower
            Return _rVal
        End Function

        ''' <summary>
        ''' used to test the validity of the passed group name
        ''' </summary>
        ''' <param name="ioGroupName">the group name to test</param>
        ''' <param name="bFixIt">flag to fix the ting if it is invalid</param>
        ''' <returns></returns>
        Public Shared Function ValidateGroupName(ByRef ioGroupName As String, Optional bFixIt As Boolean = False) As Boolean
            Dim rErrorString As String = String.Empty
            Return ValidateGroupName(ioGroupName, rErrorString, bFixIt)
        End Function

        ''' <summary>
        ''' used to test the validity of the passed group name
        ''' </summary>
        ''' <param name="ioGroupName">the group name to test</param>
        ''' <param name="rErrorString">returns an error message if the passed value vioates the rules for group names</param>
        ''' <param name="bFixIt">flag to fix the ting if it is invalid</param>
        ''' <returns></returns>
        Public Shared Function ValidateGroupName(ByRef ioGroupName As String, ByRef rErrorString As String, Optional bFixIt As Boolean = False) As Boolean
            rErrorString = ""
            If String.IsNullOrWhiteSpace(ioGroupName) Then
                ioGroupName = ""

            Else
                If bFixIt Then ioGroupName = ioGroupName.Trim()
            End If

            If ioGroupName.Length <= 0 Then
                rErrorString = "Group cannot be a null string"
                Return False
            End If

            If ioGroupName.Contains(" ") Then
                If Not bFixIt Then
                    rErrorString = "Group cannot contain spaces"
                    Return False
                Else
                    ioGroupName = ioGroupName.Replace(" ", "")
                End If

            End If
            If ioGroupName.Length > 30 Then
                If Not bFixIt Then
                    rErrorString = "Group name length exceeds the 30 character limit"
                    Return False
                Else
                    ioGroupName = ioGroupName.Substring(0, 30)
                End If
            End If

            Dim txt As String = String.Empty
            Dim charStr As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890_-$"

            If StringContainsOtherCharacters(ioGroupName, charStr, bCaseSensitive:=True, bReplace:=bFixIt, txt) Then
                If Not bFixIt Then
                    rErrorString = IIf(txt.Length <= 1, $"Group name contains invalid character '{txt}'", $"Group name contains invalid characters '{txt}'")
                    Return False
                End If
            End If
            Return True
        End Function
        Public Shared Function VBColorsToCADColors(aColor As Integer) As dxxColors
            '#1VB color long value to convert to an AutoCAD ACL color
            '^used to convert the passed VB color value to it's equivalent AutoCAD color constant
            '~if the passed value does not equate to a DXF color constant dxxColors.BlackWhite is returned
            Return dxfColors.GetColor(aColor, True, True).ACLValue
        End Function

        Friend Shared Function WriteINIString(ByRef fname As String, ByRef sSection As String, ByRef sKey As String, sValue As String) As Boolean
            '#1the INI file to write to
            '#2the section in the iINI file to add a value to
            '#3the key to add to the file
            '#4the value to add to the file
            '^used to write a value into an INI formatted file
            '~uses the "WritePrivateProfileString" windows API call.
            '~returns False if the API call returns an error code.
            If fname = "" Then fname = gsINIFilePath()
            If fname = "" Then Return False
            If Not File.Exists(fname) Then Return False
            If sValue Is Nothing Then sValue = ""
            Return APIWrapper.WritePrivateProfileString(sSection, sKey, sValue, fname)
        End Function
        Public Shared Function Input_Decimal(aInitValue As Object, ByRef rCanceled As Boolean, aHeader As String, aPrompt As String, Optional aInfo As String = "",
                                             Optional aAllowZeroInput As Boolean = False, Optional aAllowNegatives As Boolean = False,
                                             Optional aMaxLimit As Double? = Nothing, Optional aMinLimit As Double? = Nothing,
                                             Optional aMaxWhole As Integer = 6, Optional aMaxDeci As Integer = 6,
                                             Optional aEqualReturnCancel As Boolean = False) As Double
            '#1the initial value to display
            '#2a header string (caption) for the input form
            '#3a string to show as a prompt above the input box
            '#4a string to display under the input box
            '#5flag to allow zero as valid input
            '#6flag to allow negative values
            '#7a max limit to apply
            '#8a minimum limit to apply
            '#9the maximimum number of digits to allow to the left of the decimal
            '#10the maximimum number of digits to allow to the right of the decimal
            '#11flag indicating to return the cancel flag as true is the initial value is equal to the inputed value
            '#12returns True if the user cancels the form
            '^shows a form prompting the user to enter a decimal value
            Dim iFrm As New frmInput
            Dim _rVal As Double = TVALUES.To_DBL(aInitValue)
            Try
                _rVal = iFrm.Input_Decimal(_rVal, rCanceled, aHeader, aPrompt, aInfo, aAllowZeroInput, aAllowNegatives, aMaxLimit, aMinLimit, aMaxWhole, aMaxDeci, aEqualReturnCancel)
            Catch ex As Exception
            Finally
                iFrm.Dispose()
            End Try
            Return _rVal
        End Function
#End Region 'Methods
    End Class 'dxfUtils
End Namespace
