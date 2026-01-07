Imports System.Windows.Documents
Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics

    Public Structure COLOR_ARGB
            Implements ICloneable
#Region "Members"
            Public A As Byte
            Public B As Byte
            Public G As Byte
            Public R As Byte
#End Region 'Members
#Region "Constructors"
            Public Sub New(Optional aTag As String = "")
                'init --------------------------
                A = 255
                B = 0
                G = 0
                R = 0
                'init --------------------------
            End Sub
            Public Sub New(aColor As COLOR_ARGB)
                'init --------------------------
                A = aColor.A
                B = aColor.B
                G = aColor.G
                R = aColor.R
                'init --------------------------
            End Sub
            Public Sub New(Optional aRed As Byte = 0, Optional aGreen As Byte = 0, Optional aBlue As Byte = 0, Optional aAlpha As Byte = 255)
                'init --------------------------
                R = aRed
                B = aBlue
                G = aGreen
                A = aAlpha
            End Sub
            Public Sub New(aHSBColor As COLOR_HSB)
                'init --------------------------
                A = 255
                B = 0
                G = 0
                R = 0
                'init --------------------------

                Dim H As Integer = aHSBColor.H
                Dim s As Integer = aHSBColor.S
                Dim l As Integer = aHSBColor.B
                Dim nH As Single
                Dim nS As Single
                Dim nL As Single
                Dim nF As Single
                Dim nP As Single
                Dim nQ As Single
                Dim nT As Single
                Dim lH As Integer
                If s > 0 Then
                    nH = H / 60
                    nL = l / 100
                    nS = s / 100
                    lH = Int(nH)
                    nF = nH - lH
                    nP = nL * (1 - nS)
                    nQ = nL * (1 - nS * nF)
                    nT = nL * (1 - nS * (1 - nF))
                    Select Case lH
                        Case 0
                            R = nL * 255
                            G = nT * 255
                            B = nP * 255
                        Case 1
                            R = nQ * 255
                            G = nL * 255
                            B = nP * 255
                        Case 2
                            R = nP * 255
                            G = nL * 255
                            B = nT * 255
                        Case 3
                            R = nP * 255
                            G = nQ * 255
                            B = nL * 255
                        Case 4
                            R = nT * 255
                            G = nP * 255
                            B = nQ * 255
                        Case 5
                            R = nL * 255
                            G = nP * 255
                            B = nQ * 255
                    End Select
                Else
                    R = (l * 255) / 100
                    G = R
                    B = R
                End If

            End Sub

#End Region 'Constructors
#Region "Properties"
            Public ReadOnly Property Greyscale As COLOR_ARGB
                Get
                    Dim lum As Integer = 0.3 * R + 0.59 * G + 0.11 * B
                    If lum > 255 Then lum = 255
                Return New COLOR_ARGB(TVALUES.ToByte(lum), TVALUES.ToByte(lum), TVALUES.ToByte(lum), A)
            End Get
            End Property
#End Region 'Properties
#Region "Methods"
            Public Function ToWin32(Optional bInvert As Boolean = False, Optional bGreyScaled As Boolean = False) As Integer
                If Not bGreyScaled Then
                    If Not bInvert Then
                        Return RGB(R, G, B)
                    Else
                        Return RGB(B, G, R)
                    End If
                Else
                    Return Greyscale.ToWin32(bInvert)
                End If
            End Function
            Public Function ToWin64(Optional bInvert As Boolean = False, Optional bGreyScaled As Boolean = False) As Color
                Return dxfColors.Win32ToWin64(ToWin32(bInvert, bGreyScaled))
            End Function
            Public Function Clone() As COLOR_ARGB
                Return New COLOR_ARGB(Me)
            End Function
            Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
                Return New COLOR_ARGB(Me)
            End Function
#End Region 'Methods
#Region "Operators"
            Public Shared Operator =(aARGB As COLOR_ARGB, bARGB As COLOR_ARGB) As Boolean
            Return aARGB.R = bARGB.R And aARGB.B = bARGB.B And aARGB.G = bARGB.G
        End Operator
        Public Shared Operator <>(aARGB As COLOR_ARGB, bARGB As COLOR_ARGB) As Boolean
            Return aARGB.R <> bARGB.R Or aARGB.B <> bARGB.B Or aARGB.G <> bARGB.G
        End Operator
        Public Shared Narrowing Operator CType(aRGB As COLOR_ARGB) As Color
            Return Color.FromArgb(aRGB.R, aRGB.G, aRGB.B)
        End Operator
        Public Shared Narrowing Operator CType(aARGB As COLOR_ARGB) As COLOR_HSB

            Dim lMin As Integer
                Dim lMax As Integer
                Dim lDelta As Integer
                Dim R As Long
                Dim G As Long
                Dim B As Long
                Dim H As Integer
                Dim s As Integer
                Dim l As Integer
                Dim nTemp As Single
                R = aARGB.R
                G = aARGB.G
                B = aARGB.B
                lMax = IIf(R > G, IIf(R > B, R, B), IIf(G > B, G, B))
                lMin = IIf(R < G, IIf(R < B, R, B), IIf(G < B, G, B))
                lDelta = lMax - lMin
                l = (lMax * 100) / 255
                If lMax > 0 Then
                    s = (lDelta / lMax) * 100
                    If lDelta > 0 Then
                        If lMax = R Then
                            nTemp = (G - B) / lDelta
                        ElseIf lMax = G Then
                            nTemp = 2 + (B - R) / lDelta
                        Else
                            nTemp = 4 + (R - G) / lDelta
                        End If
                        H = nTemp * 60
                        If H < 0 Then H += 360
                    End If
                End If
            Return New COLOR_HSB(TVALUES.ToByte(H), TVALUES.ToByte(s), TVALUES.ToByte(B))
        End Operator
#End Region 'Operators
        End Structure 'COLOR_ARGB
    Public Structure COLOR_HSB
        Implements ICloneable
#Region "Members"
        Public B As Byte
        Public H As Byte
        Public S As Byte
#End Region 'Members
#Region "Constructors"
        Public Sub New(aHue As Byte, aSaturation As Byte, aBrightness As Byte)
            'init ----------------------------
            H = aHue
            S = aSaturation
            B = aBrightness
            'init ----------------------------
        End Sub
        Public Sub New(Optional aTag As String = "")
            'init ----------------------------
            H = 0
            S = 0
            B = 0
            'init ----------------------------
        End Sub
        Public Sub New(aColor As COLOR_HSB)
            'init ----------------------------
            H = aColor.H
            S = aColor.S
            B = aColor.B
            'init ----------------------------
        End Sub
        Public Sub New(aARGB As COLOR_ARGB)
            'init ----------------------------
            H = 0
            S = 0
            B = 0
            'init ----------------------------

            Dim R As Byte = aARGB.R
            Dim G As Byte = aARGB.G
            Dim nTemp As Double

            B = aARGB.B

            Dim lMin As Integer = IIf(R < G, IIf(R < B, R, B), IIf(G < B, G, B))
            Dim lMax As Integer = IIf(R > G, IIf(R > B, R, B), IIf(G > B, G, B))
            Dim lDelta As Integer = lMax - lMin


            Dim l As Integer = (lMax * 100) / 255
            If lMax > 0 Then
                S = (lDelta / lMax) * 100
                If lDelta > 0 Then
                    If lMax = R Then
                        nTemp = (G - B) / lDelta
                    ElseIf lMax = G Then
                        nTemp = 2 + (B - R) / lDelta
                    Else
                        nTemp = 4 + (R - G) / lDelta
                    End If
                    H = nTemp * 60
                    If H < 0 Then H += 360
                End If
            End If

        End Sub
#End Region 'Constructors
#Region "Methods"

        Public Function Clone() As COLOR_HSB
            Return New COLOR_HSB(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New COLOR_HSB(Me)
        End Function
#End Region 'Methods

#Region "Operators"
        Public Shared Widening Operator CType(aHSBColor As COLOR_HSB) As COLOR_ARGB

            Return New COLOR_ARGB(aHSBColor)
        End Operator
#End Region 'Operators


    End Structure 'COLOR_HSB

    Public Structure dxfColor
        Implements ICloneable
#Region "Members"
        Private _Win32Color As Integer
        Private _ACLNumber As Integer
        Private _RGB As COLOR_ARGB
#End Region 'Members
#Region "Constructors"


        Public Sub New(Optional aACLNumber As Integer = -1, Optional aARGB As COLOR_ARGB? = Nothing)
            'init --------------------------------
            _RGB = New COLOR_ARGB("")
            _Win32Color = 0
            _ACLNumber = 0
            'init --------------------------------
            If aARGB.HasValue Then RGB = aARGB.Value Else RGB = New COLOR_ARGB(0, 0, 0) 'black
            If ACLNumber >= dxxColors.ByBlock And ACLNumber <= dxxColors.ByLayer Then  'this is an ACL index color
                ACLNumber = aACLNumber
                RGB = aARGB
            Else
                ACLNumber = -1
            End If
            Win32Color = RGB.ToWin32
        End Sub
        Public Sub New(aACLNumber As Integer, aRed As Byte, aGreen As Byte, aBlue As Byte, Optional aAlpha As Byte = 255)
            'init --------------------------------
            _RGB = New COLOR_ARGB("")
            _Win32Color = 0
            _ACLNumber = 0
            'init --------------------------------
            RGB = New COLOR_ARGB(aRed, aGreen, aBlue, aAlpha) 'black
            If ACLNumber >= dxxColors.ByBlock And ACLNumber <= dxxColors.ByLayer Then  'this is an ACL index color
                ACLNumber = aACLNumber
            Else
                ACLNumber = -1
            End If
            Win32Color = RGB.ToWin32
        End Sub
        Friend Sub New(Optional aTag As String = "")
            'init --------------------------------
            _RGB = New COLOR_ARGB("")
            _Win32Color = 0
            _ACLNumber = 0
            'init --------------------------------
        End Sub

        Friend Sub New(aColor As dxfColor)
            'init --------------------------------
            _RGB = New COLOR_ARGB(aColor.RGB)
            _Win32Color = aColor.Win32Color
            _ACLNumber = aColor.ACLNumber
            'init --------------------------------
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Property ACLNumber As dxxColors
            Get
                Return _ACLNumber
            End Get
            Friend Set(value As dxxColors)
                _ACLNumber = value
            End Set
        End Property
        Public ReadOnly Property ACLValue As Integer
            Get
                '^if the color is an ACL color then ACLNumber is returned otherwise
                '^the windows integer value is returned
                If IsACL Then Return ACLNumber Else Return ToWin32
            End Get
        End Property
        Public Property RGB As COLOR_ARGB
            Get
                Return _RGB
            End Get
            Friend Set(value As COLOR_ARGB)
                _RGB = value
            End Set
        End Property
        Public Property Win32Color As Integer
            Get
                Return _Win32Color
            End Get
            Friend Set(value As Integer)
                _Win32Color = value
            End Set
        End Property
        Public ReadOnly Property ToWin64 As System.Drawing.Color
            Get
                Return dxfColors.ARGBToWin64(RGB)
            End Get
        End Property
        Public ReadOnly Property ToWin32 As Integer
            Get
                Return dxfColors.ARGBToWin32(RGB)
            End Get
        End Property
        Public ReadOnly Property IsACL As Boolean
            Get
                '^returns True if this color is one of the AutoCAD colors (0 to 256)
                Return _ACLNumber >= dxxColors.ByBlock And _ACLNumber <= dxxColors.ByLayer
            End Get
        End Property
        Public ReadOnly Property IsLogical As Boolean
            Get
                '^returns True if the color is ByBlock or ByLayer
                Return _ACLNumber = dxxColors.ByBlock Or _ACLNumber = dxxColors.ByLayer
            End Get
        End Property


#End Region 'Properties
#Region "Methods"
        Public Overrides Function ToString() As String
            If IsACL Then
                Dim sEnums As String = "0,1,2,3,4,5,6,7,8,9,11,30,51,81,131,151,211,250,255,256"
                If TLISTS.Contains(_ACLNumber, sEnums) Then
                    Return _ACLNumber.ToString & " {" & dxfEnums.DisplayName(Me.ACLNumber) & "}"
                Else
                    Return _ACLNumber.ToString
                End If
            Else
                Return _ACLNumber.ToString
            End If
        End Function
        Public Function Clone() As dxfColor
            Return New dxfColor(Me)
        End Function

        Public Function Descriptor(Optional bIncludeHueSatBrit As Boolean = False) As String

            Dim _rVal As String = String.Empty
            _rVal = dxfColors.ACLName(ACLValue, True)
            If _rVal <> "" Then
                If TVALUES.IsNumber(_rVal) Then
                    _rVal = "ACL=" & _rVal
                Else
                    _rVal = "ACL=" & ACLNumber & " ( " & _rVal & " ) "
                End If
            End If
            TLISTS.Add(_rVal, "Windows=" & ToWin32, bAllowDuplicates:=True, aDelimitor:=dxfGlobals.Delim)
            TLISTS.Add(_rVal, "RGB= ( " & _RGB.R & "," & _RGB.G & "," & _RGB.B & " ) ", bAllowDuplicates:=True, aDelimitor:=dxfGlobals.Delim)
            If bIncludeHueSatBrit Then
                Dim aclr As COLOR_HSB
                aclr = CType(_RGB, COLOR_HSB)
                TLISTS.Add(_rVal, "HSB= ( " & aclr.H & "," & aclr.S & "," & aclr.B & " ) ", bAllowDuplicates:=True, aDelimitor:=dxfGlobals.Delim)
            End If
            Return _rVal

        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New dxfColor(Me)
        End Function
#End Region 'Methods
    End Structure 'dxfColor

End Namespace
