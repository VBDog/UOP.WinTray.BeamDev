Imports System.Runtime.InteropServices
Imports Vanara.PInvoke
Imports UOP.DXFGraphics.Fonts.dxfFonts
Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class APIWrapper
        'List(Of ValueTuple(Of Gdi32.ENUMLOGFONTEXDV, Gdi32.ENUMTEXTMETRIC, Gdi32.FontType))
        Private Const ERROR_SHARING_VIOLATION As Integer = 32
        Friend Structure FontData
            'Public LogicalFont As Gdi32.ENUMLOGFONTEX
            Public TextMetrics As Gdi32.NEWTEXTMETRICEX
            Public TTFStyle As dxxTextStyleFontSettings

            Public Sub New(aTextMetrics As Gdi32.NEWTEXTMETRICEX, aTTFStyle As dxxTextStyleFontSettings)
                'If Not String.IsNullOrWhiteSpace(aStyleName) Then StyleName = aStyleName.Trim Else StyleName = ""
                TTFStyle = aTTFStyle
                TextMetrics = aTextMetrics
            End Sub


            Public Overrides Function ToString() As String
                Return StyleName
            End Function
            Public ReadOnly Property StyleName As String
                Get
                    Return dxfEnums.Description(TTFStyle)
                End Get
            End Property

        End Structure



#Region "Members"
#End Region 'Members
#Region "Declares"
        'Private Declare Ansi Function EnumFontFamiliesEx Lib "GDI32.dll" Alias "EnumFontFamiliesExA" (hdc As HDC,
        '<[In]()> ByRef lpLogFont As IntPtr,
        '                                                                                             lpEnumFontProc As EnumFontFamExProcDelegate,lParam As IntPtr,dwFlags As UInteger) As Integer
        <DllImport("gdi32.dll",
        EntryPoint:="EnumFontFamiliesExA")>
        Public Shared Function EnumFontFamiliesEx(
        hDC As HDC,
            <[In]()> ByRef lpLogFont As IntPtr,
            lpEnumFontProc As EnumFontFamExProcDelegate,
            lParam As IntPtr,
            dwFlags As UInteger) As Integer
        End Function
        Public Delegate Function EnumFontFamExProcDelegate(
        ByRef lpELFE As Gdi32.ENUMLOGFONTEX,
        ByRef lpNTME As Gdi32.NEWTEXTMETRIC,
        lFontType As Gdi32.FontType,
        lParam As Integer) As Integer
#End Region 'Declares
#Region "SharedMethods"
        'Public Shared Function ShowWindow(aHwnd As Long, nCmd As ShowWindowCommand) As Boolean
        '    Try
        '        Return User32.ShowWindow(New HWND(CType(aHwnd, IntPtr)), nCmd)
        '    Catch ex As Exception
        '        Return False
        '    End Try
        'End Function
        'Public Shared Function SetForegroundWindow(aHwnd As Long) As Boolean
        '    Try
        '        Return User32.SetForegroundWindow(New HWND(CType(aHwnd, IntPtr)))
        '    Catch ex As Exception
        '        Return False
        '    End Try
        'End Function
        'Public Shared Sub Sleep(aMillisecs As Integer)
        '    Try
        '        Kernel32.Sleep(CType(aMillisecs, UInteger))
        '    Catch ex As Exception
        '        Return
        '    End Try
        'End Sub
        Friend Shared Function GetPath(aDC As IntPtr, aXOffset As Integer, aYOffset As Integer, aXScale As Double, aYScale As Double, ByRef rLimits As TLIMITS) As TPOINTS
            rLimits = New TLIMITS(bMaxed:=True)
            Dim _rVal As New TPOINTS(0)
            Try
                Dim rPts(0) As Vanara.PInvoke.POINT
                Dim rCodes(0) As Gdi32.VertexType
                Dim code As Byte
                Dim Pt As TPOINT
                'to get the number of points
                Dim cnt As Integer = Gdi32.GetPath(New HDC(aDC), rPts, rCodes, 0)
                If cnt <= 0 Then
                    Return _rVal
                End If
                System.Array.Resize(rPts, cnt)
                System.Array.Resize(rCodes, cnt)
                'again to get the actual points
                cnt = Gdi32.GetPath(New HDC(aDC), rPts, rCodes, cnt)
                If rPts.Count <= 0 Then
                    rLimits = New TLIMITS()
                End If
                Dim p1 As Object
                For i As Integer = 1 To rPts.Length
                    p1 = rPts(i - 1)
                    Pt = New TPOINT(p1.X * aXScale + aXOffset, p1.Y * aYScale + aYOffset)
                    If i <= rCodes.Length Then
                        code = TVALUES.ToByte(rCodes(i - 1))
                    Else
                        code = dxxVertexStyles.MOVETO
                    End If
                    rLimits.Update(Pt)
                    Pt.Code = code
                    _rVal.Add(Pt)
                Next i
                Return _rVal
            Catch ex As Exception
                Return _rVal
            End Try
        End Function
        Public Shared Function BeginPath(aDC As IntPtr) As Boolean
            Try
                Return Gdi32.BeginPath(New HDC(aDC))
            Catch ex As Exception
                Return False
            End Try
        End Function
        Public Shared Function EndPath(aDC As IntPtr) As Boolean
            Try
                Return Gdi32.EndPath(New HDC(aDC))
            Catch ex As Exception
                Return False
            End Try
        End Function
        Public Shared Function TextOut(aDC As IntPtr, X As Integer, Y As Integer, lpString As String, nCount As Integer) As Long
            Try
                Return Gdi32.TextOut(New HDC(aDC), X, Y, lpString, nCount)
            Catch ex As Exception
                Return False
            End Try
        End Function
        Public Shared Function CreateFontIndirect(lpLogfont As LOGFONT) As Gdi32.SafeHFONT
            Try
                Dim lgfnt As Vanara.PInvoke.LOGFONT = LogFontToVanara(lpLogfont)
                Return Gdi32.CreateFontIndirect(lgfnt)
            Catch ex As Exception
                Return Nothing
            End Try
        End Function

        Public Shared Function LogFontToVanara(aLogFont As LOGFONT) As Vanara.PInvoke.LOGFONT
            Dim p As Byte = 0
            Dim f As Byte = 0
            Dim j As Byte
            Dim bitvals As TVALUES = TVALUES.BitCode_Decompose(aLogFont.lfPitchAndFamily)
            For i As Integer = 1 To bitvals.Count
                j = TVALUES.ToByte(bitvals.Item(i))
                If j < 8 Then p += j Else f += j
            Next
            Dim _rVal As New Vanara.PInvoke.LOGFONT With
            {
              .lfHeight = aLogFont.lfHeight,
              .lfWidth = aLogFont.lfWidth,
              .lfEscapement = aLogFont.lfEscapement,
              .lfOrientation = aLogFont.lfOrientation,
              .lfItalic = aLogFont.lfItalic,
              .lfUnderline = aLogFont.lfUnderline,
              .lfStrikeOut = aLogFont.lfStrikeOut,
              .lfCharSet = aLogFont.lfCharSet,
              .lfOutPrecision = aLogFont.lfOutPrecision,
              .lfClipPrecision = aLogFont.lfClipPrecision,
              .lfQuality = aLogFont.lfQuality,
              .lfFaceName = aLogFont.lfFaceName,
              .lfWeight = aLogFont.lfWeight,
              .Pitch = p,
              .FontFamily = f
            }
            Return _rVal
        End Function

        Private Shared _FontInfoArray As List(Of TFONTINFO)

        Friend Shared Function FontInfoArray() As List(Of TFONTINFO)
            If _FontInfoArray IsNot Nothing Then Return _FontInfoArray

            Try
                _FontInfoArray = New List(Of TFONTINFO)
                Dim factory As New SharpDX.DirectWrite.Factory()
                Dim sort As New List(Of Tuple(Of String, TFONTINFO))
                Dim fontCollection As SharpDX.DirectWrite.FontCollection = factory.GetSystemFontCollection(False)
                Dim familCount As Integer = fontCollection.FontFamilyCount
                For i As Integer = 1 To familCount
                    Dim fontFamily As SharpDX.DirectWrite.FontFamily = fontCollection.GetFontFamily(i - 1)
                    Dim familyNames As SharpDX.DirectWrite.LocalizedStrings = fontFamily.FamilyNames
                    Dim fontfacelist As New List(Of String)
                    Dim fontface As New List(Of String)
                    For j As Integer = 1 To fontFamily.FontCount
                        Dim font As SharpDX.DirectWrite.Font = fontFamily.GetFont(j - 1)
                        fontface.Add($"{font.Weight},{ font.Style}")
                        ' Debug.WriteLine($"{font.Weight},{ font.Style}")
                    Next
                    Dim index As Integer = 0
                    Dim locale As String = System.Globalization.CultureInfo.CurrentCulture.Name
                    Dim found As Boolean = TVALUES.ToBoolean(familyNames.FindLocaleName(locale, index))

                    If Not found Then

                        If String.Compare(locale, "en-us", True) <> 0 Then
                            found = familyNames.FindLocaleName("en-us", index).Equals(True)
                        End If
                    End If
                    If Not found Then
                        index = 0
                    End If

                    Dim name As String = familyNames.GetString(index)
                    'Debug.WriteLine($"FONTNAME = {name}")
                    For j As Integer = 1 To fontFamily.FontCount
                        Dim font As SharpDX.DirectWrite.Font = fontFamily.GetFont(j - 1)
                        Dim finfo As New TFONTINFO(name, font.Style.ToString(), font.Weight, font.Metrics, CType(font.IsSymbolFont, Boolean))
                        sort.Add(New Tuple(Of String, TFONTINFO)(name, finfo))
                        '_rVal.Add(finfo)
                        'fontface.Add($"{name} - {font.Weight},{ font.Style}")
                        'Debug.WriteLine($"     {font.Weight},{ font.Style}")
                    Next
                Next
                sort = sort.OrderBy(Function(c) c.Item1).ToList()
                For Each srt As Tuple(Of String, TFONTINFO) In sort
                    _FontInfoArray.Add(srt.Item2)
                Next
            Catch
                _FontInfoArray = Nothing
            End Try



            Return _FontInfoArray

        End Function


        Friend Shared Function GetFontStyeList(aFaceName As String) As List(Of FontData)

            Throw New NotImplementedException
            'this function should work but throws errors!  replaced with the SharpDX FontInfoList 


            'Dim _rVal = New List(Of FontData)
            'If String.IsNullOrWhiteSpace(aFaceName) Then Return _rVal
            'Try
            '    Dim deskhwnd As HWND = User32.GetDesktopWindow()
            '    Dim desckdc As Gdi32.SafeHDC = User32.GetDC(deskhwnd)
            '    Dim finfo As FontData
            '    Dim vRet As List(Of (Gdi32.ENUMLOGFONTEXDV, Gdi32.ENUMTEXTMETRIC, Gdi32.FontType)) = Gdi32.EnumFontFamiliesEx(desckdc, CharacterSet.ANSI_CHARSET, aFaceName.Trim())
            '    User32.ReleaseDC(deskhwnd, desckdc)
            '    For i As Integer = 0 To vRet.Count - 1
            '        finfo = New FontData(vRet.Item(i).Item2.etmNewTextMetricEx, dxxTextStyleFontSettings.Regular)
            '        '.LogicalFont = vRet.Item(i).Item1.elfEnumLogfontEx,
            '        Dim sname As String = vRet.Item(i).Item1.elfEnumLogfontEx.elfStyle
            '        If Not String.IsNullOrWhiteSpace(sname) Then
            '            sname = sname.ToUpper()
            '            If sname.Contains("BOLD") And sname.Contains("ITALIC") Then
            '                finfo.TTFStyle = dxxTextStyleFontSettings.BoldItalic
            '            ElseIf sname.Contains("BOLD") Then
            '                finfo.TTFStyle = dxxTextStyleFontSettings.Bold
            '            ElseIf sname.Contains("ITALIC") Then
            '                finfo.TTFStyle = dxxTextStyleFontSettings.Italic
            '            Else
            '                finfo.TTFStyle = dxxTextStyleFontSettings.Regular
            '            End If
            '        End If



            '        _rVal.Add(finfo)
            '    Next
            '    Return _rVal
            'Catch ex As Exception
            '    Return _rVal
            'End Try
            'Return _rVal
        End Function

        Public Shared Function SetTextColor(aHdc As IntPtr, aColor As Integer) As Long
            Try
                Dim aC As Color = dxfColors.Win32ToWin64(aColor)
                Dim cRef As New COLORREF(aC)
                Dim CWas As COLORREF = Gdi32.SetTextColor(New HDC(aHdc), cRef)
                Return TVALUES.To_LNG(RGB(CWas.R, CWas.G, CWas.B))
            Catch ex As Exception
                Return 0
            End Try
        End Function
        Public Shared Function MoveToEx(aDC As IntPtr, aX As Integer, aY As Integer) As Boolean
            Dim rLastPoint As System.Drawing.Point = Nothing
            Return MoveToEx(aDC, aX, aY, rLastPoint)
        End Function
        Public Shared Function MoveToEx(aDC As IntPtr, aX As Integer, aY As Integer, ByRef rLastPoint As System.Drawing.Point) As Boolean
            Try
                Return Gdi32.MoveToEx(New HDC(aDC), aX, aY, rLastPoint)
            Catch ex As Exception
                Return False
            End Try
        End Function
        Public Shared Function InvalidateRect(hWnd As IntPtr, lpRect As Vanara.PInvoke.RECT, Optional bErase As Boolean = False) As Boolean
            Try
                Return User32.InvalidateRect(New HWND(hWnd), lpRect, bErase)
            Catch ex As Exception
                Return False
            End Try
        End Function
        Public Shared Function GetClientRect(hWnd As IntPtr, ByRef lpRect As Vanara.PInvoke.RECT) As Boolean
            Try
                Return User32.GetClientRect(New HWND(hWnd), lpRect)
            Catch ex As Exception
                Return False
            End Try
        End Function
        Public Shared Function WritePrivateProfileString(lpSection As String, lpKeyName As String, lpString As String,
                                                         lpFileName As String) As Boolean
            Try
                Return Kernel32.WritePrivateProfileString(lpSection, lpKeyName, lpString, lpFileName)
            Catch ex As Exception
                Return False
            End Try
        End Function
        Public Shared Function GetPrivateProfileString(lpSection As String, lpKeyName As String, lpDefault As String,
                                                        nSize As Integer, lpFileName As String) As String

            Dim SB As New System.Text.StringBuilder
            Try
                Dim rVal As UInteger = Kernel32.GetPrivateProfileString(lpSection, lpKeyName, lpDefault, SB, nSize, lpFileName)

                Return SB.ToString
            Catch ex As Exception
                Return String.Empty
            End Try
        End Function
        Public Shared Function GetPrivateProfileString(lpSection As String, lpKeyName As String, lpDefault As String,
                                                        nSize As Integer, lpFileName As String, ByRef rKeyFound As Boolean) As String

            Dim SB As New System.Text.StringBuilder
            Try
                Dim rVal As UInteger = Kernel32.GetPrivateProfileString(lpSection, lpKeyName, lpDefault, SB, nSize, lpFileName)
                rKeyFound = rVal > 0
                Return SB.ToString
            Catch ex As Exception
                Return String.Empty
            End Try
        End Function
        Public Shared Function FileIsInUse(aFileName As String) As Boolean
            Dim inUse As Boolean = False
            Dim fileHandle As Kernel32.SafeHFILE = Kernel32.CreateFile(aFileName, Kernel32.FileAccess.GENERIC_WRITE, IO.FileShare.Write, CType(Nothing, SECURITY_ATTRIBUTES), IO.FileMode.OpenOrCreate, FileFlagsAndAttributes.FILE_ATTRIBUTE_NORMAL, IntPtr.Zero)
            If fileHandle.IsInvalid Then
                inUse = Marshal.GetLastWin32Error() = ERROR_SHARING_VIOLATION
            End If
            fileHandle.Close()
            Return inUse
        End Function
        Public Shared Function SetFileAttributes(lpFileName As String, dwFileAttributes As FileFlagsAndAttributes) As Boolean
            Try
                Return Kernel32.SetFileAttributes(lpFileName, dwFileAttributes)
            Catch ex As Exception
                Return False
            End Try
        End Function

        Public Shared Function SetMapMode(aDC As IntPtr, aMode As Gdi32.MapMode) As Boolean
            Try
                If Gdi32.SetMapMode(New HDC(aDC), aMode) <> 0 Then
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                Return False
            End Try
        End Function
        Public Shared Function SetTextAlign(aDC As IntPtr, aAlign As Gdi32.TextAlign) As Gdi32.TextAlign
            Try
                Return Gdi32.SetTextAlign(New HDC(aDC), aAlign)
            Catch ex As Exception
                Return Gdi32.TextAlign.TA_NOUPDATECP
            End Try
        End Function

        Public Shared Function GetDeviceCaps(aDC As IntPtr, aFunction As Gdi32.DeviceCap) As Integer
            Try
                Return Gdi32.GetDeviceCaps(New HDC(aDC), aFunction)
            Catch ex As Exception
                Return 0
            End Try
        End Function
        'Public Shared Function GetOutlineTextMetrics(aDC As Long, cbData As Integer, lpotm As IntPtr) As Integer
        '    Try
        '        Dim uRet As UInteger = Gdi32.GetOutlineTextMetrics(New HDC(CType(aDC, IntPtr)), CType(cbData, UInteger), lpotm)
        '        Return CType(uRet, Integer)
        '    Catch ex As Exception
        '        Return 0
        '    End Try
        'End Function
        'Public Shared Sub RtlMoveMemory(aDest As IntPtr, aSource As IntPtr, aSize As Integer)
        '    Try
        '        Kernel32.RtlMoveMemory(aDest, aSource, New SizeT(CType(aSize, UInteger)))
        '    Catch ex As Exception
        '    End Try
        'End Sub
        'Public Shared Function GetTempFileName(lpszPath As String, lpPrefixString As String, wUnique As UInteger) As String
        '    Dim rVal As UInteger
        '    Dim SB As New System.Text.StringBuilder
        '    Try
        '        rVal = Kernel32.GetTempFileName(lpszPath, lpPrefixString, wUnique, SB)
        '        If rVal <> 0 Then
        '            Return SB.ToString
        '        Else
        '            Return String.Empty
        '        End If
        '    Catch ex As Exception
        '        Return String.Empty
        '    End Try
        'End Function
        'Public Shared Function SetMapMode(aDC As Long, aMode As Gdi32.MapMode) As Boolean
        '    Try
        '        If Gdi32.SetMapMode(New HDC(CType(aDC, IntPtr)), aMode) <> 0 Then
        '            Return True
        '        Else
        '            Return False
        '        End If
        '    Catch ex As Exception
        '        Return False
        '    End Try
        'End Function

        'Public Shared Function DeleteDC(aDC As HDC) As Boolean
        '    Dim _rVal As Boolean
        '    Try
        '        _rVal = Gdi32.DeleteDC(aDC)
        '    Catch ex As Exception
        '        Return False
        '        Throw ex
        '    End Try
        '    Return _rVal
        'End Function
        'Public Shared Function CreateCompatibleDC(aHwnd As HWND) As Gdi32.SafeHDC
        '    Dim aPtr As HDC
        '    Dim rPtr As Gdi32.SafeHDC = Nothing
        '    Dim released As Boolean
        '    Try
        '        aPtr = User32.GetDC(aHwnd)
        '        rPtr = Gdi32.CreateCompatibleDC(aPtr)
        '    Catch ex As Exception
        '        rPtr = Nothing
        '    Finally
        '        released = User32.ReleaseDC(aHwnd, aPtr)
        '    End Try
        '    Return rPtr
        'End Function
        'Public Shared Function SetBkColor(aDC As HDC, crColor As Integer) As COLORREF
        '    Dim aPtr As IntPtr = aDC
        '    Try
        '        Return Gdi32.SetBkColor(aDC, crColor)
        '    Catch ex As Exception
        '        Return 0
        '    End Try
        'End Function
        Public Shared Function SelectObject(aDC As IntPtr, hObject As Vanara.PInvoke.HGDIOBJ) As Vanara.PInvoke.HGDIOBJ
            '^Selects an object into the specified device context (DC). The new object replaces the previous object
            Try
                Return Gdi32.SelectObject(New HDC(aDC), hObject)
            Catch ex As Exception
                Return Nothing
            End Try
        End Function

        'Public Shared Function ReleaseDC(Hndl As HWND, aDC As HDC) As Boolean
        '    Dim _rVal As Boolean = False
        '    Try
        '        _rVal = User32.ReleaseDC(Hndl, aDC)
        '    Catch ex As Exception
        '        Return False
        '    End Try
        '    Return _rVal
        'End Function
        'Public Shared Function GetDesktopWindowPtr() As HWND
        '    Dim rSuccess As Boolean = False
        '    Return GetDesktopWindowPtr(rSuccess)
        'End Function
        'Public Shared Function GetDesktopWindowPtr(ByRef rSuccess As Boolean) As HWND
        '    Dim _rVal As HWND = Nothing
        '    Try
        '        _rVal = User32.GetDesktopWindow()
        '        rSuccess = Not _rVal.Equals(0)
        '        Return _rVal
        '    Catch ex As Exception
        '        rSuccess = False
        '        Return _rVal
        '    End Try
        'End Function
#End Region 'SharedMethods
    End Class
End Namespace
