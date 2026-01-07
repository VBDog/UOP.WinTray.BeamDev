Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics

    Public Class dxfColors
#Region "Members"
#End Region 'Members
#Region "Constructors"
#End Region 'Constructors
#Region "Methods"
#End Region 'Methods
#Region "Shared Properties"
        Public Shared ReadOnly Property Black As dxfColor
            '^the basic black color from the collection (250)
            Get
                Return dxfColors.Color(dxxColors.Black)
            End Get
        End Property
        Public Shared ReadOnly Property BlackWhite As dxfColor
            '^the black/white color from the collection
            Get
                Return dxfColors.Color(dxxColors.BlackWhite)
            End Get
        End Property
        Public Shared ReadOnly Property Blue As dxfColor
            '^the blue color from the collection
            Get
                Return dxfColors.Color(dxxColors.Blue)
            End Get
        End Property
        Public Shared ReadOnly Property Cyan As dxfColor
            '^the cyan color from the collection
            Get
                Return dxfColors.Color(dxxColors.Cyan)
            End Get
        End Property
        Public Shared ReadOnly Property Green As dxfColor
            '^the green color from the collection
            Get
                Return dxfColors.Color(dxxColors.Green)
            End Get
        End Property
        Public Shared ReadOnly Property Grey As dxfColor
            '^the basic grey color from the collection
            Get
                Return dxfColors.Color(dxxColors.Grey)
            End Get
        End Property
        Public Shared ReadOnly Property LightBlue As dxfColor
            '^the basic light blue color from the collection
            Get
                Return dxfColors.Color(dxxColors.LightBlue)
            End Get
        End Property
        Public Shared ReadOnly Property LightCyan As dxfColor
            '^the basic light cyan color from the collection
            Get
                Return dxfColors.Color(dxxColors.LightCyan)
            End Get
        End Property
        Public Shared ReadOnly Property LightGreen As dxfColor
            '^the basic light green color from the collection
            Get
                Return dxfColors.Color(dxxColors.LightGreen)
            End Get
        End Property
        Public Shared ReadOnly Property LightGrey As dxfColor
            '^the basic light grey color from the collection
            Get
                Return dxfColors.Color(dxxColors.LightGrey)
            End Get
        End Property
        Public Shared ReadOnly Property LightMagenta As dxfColor
            '^the basic light magenta color from the collection
            Get
                Return dxfColors.Color(dxxColors.LightMagenta)
            End Get
        End Property
        Public Shared ReadOnly Property LightRed As dxfColor
            '^the basic light red color from the collection
            Get
                Return dxfColors.Color(dxxColors.LightRed)
            End Get
        End Property
        Public Shared ReadOnly Property LightYellow As dxfColor
            '^the basic light yellow color from the collection
            Get
                Return dxfColors.Color(dxxColors.LightYellow)
            End Get
        End Property
        Public Shared ReadOnly Property Magenta As dxfColor
            '^the magenta color from the collection
            Get
                Return dxfColors.Color(dxxColors.Magenta)
            End Get
        End Property
        Public Shared ReadOnly Property Orange As dxfColor
            '^the basic orange color from the collection (30)
            Get
                Return dxfColors.Color(dxxColors.Orange)
            End Get
        End Property
        Public Shared ReadOnly Property Red As dxfColor
            '^the red color from the collection
            Get
                Return dxfColors.Color(dxxColors.Red)
            End Get
        End Property
        Public Shared ReadOnly Property White As dxfColor
            '^the basic white color from the collection (255)
            Get
                Return dxfColors.Color(dxxColors.White)
            End Get
        End Property
        Public Shared ReadOnly Property Yellow As dxfColor
            '^the yellow color from the collection
            Get
                Return dxfColors.Color(dxxColors.Yellow)
            End Get
        End Property
#End Region 'Shared Properties
#Region "Shared Methods"
        Public Shared Function ACLPallet(Optional aBlocWd As Integer = 20, Optional aBackColor As Color = Nothing) As dxfBitmap
            Return dxfBitmap.CreateACLPallet(aBlocWd, aBackColor)
        End Function
        Public Shared Function GetPenColor(aIndex As Object, Optional bIsWindowsColor As Boolean = False) As Integer
            bIsWindowsColor = False
            Dim _rVal As Integer = 0
            Dim idx As Integer = TVALUES.To_INT(aIndex)
            '#1the requested item number
            '^returns the pen color of the indicated color
            Dim aColor As dxfColor = dxfColors.Color(idx)
            bIsWindowsColor = idx < dxxColors.ByBlock Or idx > dxxColors.ByLayer
            Return aColor.ToWin32
        End Function
        Public Shared Function PenColor(aColor As dxxColors) As Color
            Return dxfColors.Win32ToWin64(dxfColors.GetPenColor(aColor))
        End Function
        Friend Shared Function ColorOutput(aColor As Integer) As TPROPERTIES
            Dim _rVal As New TPROPERTIES("ColorOutput")
            If aColor = dxxColors.Undefined Then aColor = 256
            Dim aclr As dxfColor = Color(aColor)

            _rVal.Add(New TPROPERTY(aCode:=62, aValue:=aclr.ACLValue, aName:="Color", aPropType:=dxxPropertyTypes.Color, aLastVal:=Nothing, aSuppressedVal:=256))
            If Not aclr.IsACL Then
                _rVal.Add(New TPROPERTY(aCode:=420, aValue:=aclr.RGB.ToWin32(True), aName:="Color Long", aPropType:=dxxPropertyTypes.dxf_Long, aLastVal:=Nothing, aSuppressedVal:=256))
            End If
            Return _rVal
        End Function
        Public Shared Function GetSystemColor(aSystemColor As dxxSystemColorIndex) As dxfColor
            Return New dxfColor(-1, dxfColors.GetSystemColorARGB(aSystemColor))
        End Function
        Public Shared Function GetColor(aIndex As Object, Optional bNoLogicals As Boolean = False, Optional bNearestACL As Boolean = False) As dxfColor
            '#1the requested color number
            '^returns the object from the collection at the requested index in the collection.
            '~returns nothing if the passed index is outside the bounds of the current collection
            Try
                Dim idx As Integer
                If TVALUES.IsNumber(aIndex) Then idx = TVALUES.To_INT(aIndex) Else idx = TVALUES.To_INT(dxxColors.Undefined)
                If idx = dxxColors.Undefined Then idx = 7
                If bNoLogicals Then
                    If idx = 0 Or idx = 256 Then idx = 7
                End If
                If idx >= dxxColors.ByBlock And idx <= dxxColors.ByLayer Then 'ACL Color
                    Return dxfColors.Color(idx)
                Else
                    If bNearestACL Then
                        Return dxfColors.NearestACLColor(dxfColors.Win32ToWin64(idx))
                    Else
                        Return New dxfColor(-1, dxfColors.Win32ToARGB(idx))
                    End If
                End If
            Catch ex As Exception
                Return Nothing
            End Try
        End Function
        Public Shared Function IsLogical(aIndex As Integer) As Boolean
            '^returns True if the color is ByBlock or ByLayer or Undefined
            Return aIndex = dxxColors.ByBlock Or aIndex = dxxColors.ByLayer Or aIndex = dxxColors.Undefined
        End Function
        Friend Shared Function InsertColorOutput(aPropCol As TPROPERTIES, aColor As Integer, Optional NoLogical As Boolean = False, Optional AddNegative As Boolean = False) As TPROPERTIES
            Dim _rVal As New TPROPERTIES
            _rVal = aPropCol
            If aColor = dxxColors.Undefined Then aColor = 256
            If NoLogical Then
                If aColor = 0 Or aColor = 256 Then aColor = 7
            End If
            Dim aclr As dxfColor = Color(aColor)
            Dim aMulti As Integer
            If AddNegative Then aMulti = -1 Else aMulti = 1
            _rVal.Add(New TPROPERTY(aCode:=62, aValue:=aMulti * aclr.ACLValue, aName:="Color", aPropType:=dxxPropertyTypes.Color, aLastVal:=Nothing, aSuppressedVal:=256))
            If Not aclr.IsACL Then
                _rVal.Add(New TPROPERTY(aCode:=420, aValue:=aMulti * aclr.RGB.ToWin32(True), aName:="Color Long", aPropType:=dxxPropertyTypes.dxf_Long, aLastVal:=Nothing, aSuppressedVal:=256))
            End If
            Return _rVal
        End Function
        Friend Shared Function Color(aColorIndex As dxxColors) As dxfColor
            Select Case aColorIndex
                Case 0 : Return New dxfColor(0, 0, 0, 0) 'by block
                Case 1 : Return New dxfColor(1, 255, 0, 0) 'RED
                Case 2 : Return New dxfColor(2, 255, 255, 0) 'Yellow
                Case 3 : Return New dxfColor(3, 0, 255, 0) 'Green
                Case 4 : Return New dxfColor(4, 0, 255, 255) 'Cyan
                Case 5 : Return New dxfColor(5, 0, 0, 255) 'BLUE
                Case 6 : Return New dxfColor(6, 255, 0, 255) 'Magenta
                Case 7 : Return New dxfColor(7, 255, 255, 255) 'Black/white
                Case 8 : Return New dxfColor(8, 128, 128, 128) 'Grey
                Case 9 : Return New dxfColor(9, 242, 242, 242)
                Case 10 : Return New dxfColor(10, 255, 0, 0)
                Case 11 : Return New dxfColor(11, 255, 127, 127)
                Case 12 : Return New dxfColor(12, 165, 0, 0)
                Case 13 : Return New dxfColor(13, 165, 82, 82)
                Case 14 : Return New dxfColor(14, 127, 0, 0)
                Case 15 : Return New dxfColor(15, 127, 63, 63)
                Case 16 : Return New dxfColor(16, 76, 0, 0)
                Case 17 : Return New dxfColor(17, 76, 38, 38)
                Case 18 : Return New dxfColor(18, 38, 0, 0)
                Case 19 : Return New dxfColor(19, 38, 19, 19)
                Case 20 : Return New dxfColor(20, 255, 63, 0)
                Case 21 : Return New dxfColor(21, 255, 159, 127)
                Case 22 : Return New dxfColor(22, 165, 41, 0)
                Case 23 : Return New dxfColor(23, 165, 103, 82)
                Case 24 : Return New dxfColor(24, 127, 31, 0)
                Case 25 : Return New dxfColor(25, 127, 79, 63)
                Case 26 : Return New dxfColor(26, 76, 19, 0)
                Case 27 : Return New dxfColor(27, 76, 47, 38)
                Case 28 : Return New dxfColor(28, 38, 9, 0)
                Case 29 : Return New dxfColor(29, 38, 23, 19)
                Case 30 : Return New dxfColor(30, 255, 127, 0)
                Case 31 : Return New dxfColor(31, 255, 191, 127)
                Case 32 : Return New dxfColor(32, 165, 82, 0)
                Case 33 : Return New dxfColor(33, 165, 124, 82)
                Case 34 : Return New dxfColor(34, 127, 63, 0)
                Case 35 : Return New dxfColor(35, 127, 95, 63)
                Case 36 : Return New dxfColor(36, 76, 38, 0)
                Case 37 : Return New dxfColor(37, 76, 57, 38)
                Case 38 : Return New dxfColor(38, 38, 19, 0)
                Case 39 : Return New dxfColor(39, 38, 28, 19)
                Case 40 : Return New dxfColor(40, 255, 191, 0)
                Case 41 : Return New dxfColor(41, 255, 223, 127)
                Case 42 : Return New dxfColor(42, 165, 124, 0)
                Case 43 : Return New dxfColor(43, 165, 145, 82)
                Case 44 : Return New dxfColor(44, 127, 95, 0)
                Case 45 : Return New dxfColor(45, 127, 111, 63)
                Case 46 : Return New dxfColor(46, 76, 57, 0)
                Case 47 : Return New dxfColor(47, 76, 66, 38)
                Case 48 : Return New dxfColor(48, 38, 28, 0)
                Case 49 : Return New dxfColor(49, 38, 33, 19)
                Case 50 : Return New dxfColor(50, 255, 255, 0)
                Case 51 : Return New dxfColor(51, 255, 255, 127)
                Case 52 : Return New dxfColor(52, 165, 165, 0)
                Case 53 : Return New dxfColor(53, 165, 165, 82)
                Case 54 : Return New dxfColor(54, 127, 127, 0)
                Case 55 : Return New dxfColor(55, 127, 127, 63)
                Case 56 : Return New dxfColor(56, 76, 76, 0)
                Case 57 : Return New dxfColor(57, 76, 76, 38)
                Case 58 : Return New dxfColor(58, 38, 38, 0)
                Case 59 : Return New dxfColor(59, 38, 38, 19)
                Case 60 : Return New dxfColor(60, 191, 255, 0)
                Case 61 : Return New dxfColor(61, 223, 255, 127)
                Case 62 : Return New dxfColor(62, 124, 165, 0)
                Case 63 : Return New dxfColor(63, 145, 165, 82)
                Case 64 : Return New dxfColor(64, 95, 127, 0)
                Case 65 : Return New dxfColor(65, 111, 127, 63)
                Case 66 : Return New dxfColor(66, 57, 76, 0)
                Case 67 : Return New dxfColor(67, 66, 76, 38)
                Case 68 : Return New dxfColor(68, 28, 38, 0)
                Case 69 : Return New dxfColor(69, 33, 38, 19)
                Case 70 : Return New dxfColor(70, 127, 255, 0)
                Case 71 : Return New dxfColor(71, 191, 255, 127)
                Case 72 : Return New dxfColor(72, 82, 165, 0)
                Case 73 : Return New dxfColor(73, 124, 165, 82)
                Case 74 : Return New dxfColor(74, 63, 127, 0)
                Case 75 : Return New dxfColor(75, 95, 127, 63)
                Case 76 : Return New dxfColor(76, 38, 76, 0)
                Case 77 : Return New dxfColor(77, 57, 76, 38)
                Case 78 : Return New dxfColor(78, 19, 38, 0)
                Case 79 : Return New dxfColor(79, 28, 38, 19)
                Case 80 : Return New dxfColor(80, 63, 255, 0)
                Case 81 : Return New dxfColor(81, 159, 255, 127)
                Case 82 : Return New dxfColor(82, 41, 165, 0)
                Case 83 : Return New dxfColor(83, 103, 165, 82)
                Case 84 : Return New dxfColor(84, 31, 127, 0)
                Case 85 : Return New dxfColor(85, 79, 127, 63)
                Case 86 : Return New dxfColor(86, 19, 76, 0)
                Case 87 : Return New dxfColor(87, 47, 76, 38)
                Case 88 : Return New dxfColor(88, 9, 38, 0)
                Case 89 : Return New dxfColor(89, 23, 38, 19)
                Case 90 : Return New dxfColor(90, 0, 255, 0)
                Case 91 : Return New dxfColor(91, 127, 255, 127)
                Case 92 : Return New dxfColor(92, 0, 165, 0)
                Case 93 : Return New dxfColor(93, 82, 165, 82)
                Case 94 : Return New dxfColor(94, 0, 127, 0)
                Case 95 : Return New dxfColor(95, 63, 127, 63)
                Case 96 : Return New dxfColor(96, 0, 76, 0)
                Case 97 : Return New dxfColor(97, 38, 76, 38)
                Case 98 : Return New dxfColor(98, 0, 38, 0)
                Case 99 : Return New dxfColor(99, 19, 38, 19)
                Case 100 : Return New dxfColor(100, 0, 255, 63)
                Case 101 : Return New dxfColor(101, 127, 255, 159)
                Case 102 : Return New dxfColor(102, 0, 165, 41)
                Case 103 : Return New dxfColor(103, 82, 165, 103)
                Case 104 : Return New dxfColor(104, 0, 127, 31)
                Case 105 : Return New dxfColor(105, 63, 127, 79)
                Case 106 : Return New dxfColor(106, 0, 76, 19)
                Case 107 : Return New dxfColor(107, 38, 76, 47)
                Case 108 : Return New dxfColor(108, 0, 38, 9)
                Case 109 : Return New dxfColor(109, 19, 38, 23)
                Case 110 : Return New dxfColor(110, 0, 255, 127)
                Case 111 : Return New dxfColor(111, 127, 255, 191)
                Case 112 : Return New dxfColor(112, 0, 165, 82)
                Case 113 : Return New dxfColor(113, 82, 165, 124)
                Case 114 : Return New dxfColor(114, 0, 127, 63)
                Case 115 : Return New dxfColor(115, 63, 127, 95)
                Case 116 : Return New dxfColor(116, 0, 76, 38)
                Case 117 : Return New dxfColor(117, 38, 76, 57)
                Case 118 : Return New dxfColor(118, 0, 38, 19)
                Case 119 : Return New dxfColor(119, 19, 38, 28)
                Case 120 : Return New dxfColor(120, 0, 255, 191)
                Case 121 : Return New dxfColor(121, 127, 255, 223)
                Case 122 : Return New dxfColor(122, 0, 165, 124)
                Case 123 : Return New dxfColor(123, 82, 165, 145)
                Case 124 : Return New dxfColor(124, 0, 127, 95)
                Case 125 : Return New dxfColor(125, 63, 127, 111)
                Case 126 : Return New dxfColor(126, 0, 76, 57)
                Case 127 : Return New dxfColor(127, 38, 76, 66)
                Case 128 : Return New dxfColor(128, 0, 38, 28)
                Case 129 : Return New dxfColor(129, 19, 38, 33)
                Case 130 : Return New dxfColor(130, 0, 255, 255)
                Case 131 : Return New dxfColor(131, 127, 255, 255) 'light cyan
                Case 132 : Return New dxfColor(132, 0, 165, 165)
                Case 133 : Return New dxfColor(133, 82, 165, 165)
                Case 134 : Return New dxfColor(134, 0, 127, 127)
                Case 135 : Return New dxfColor(135, 63, 127, 127)
                Case 136 : Return New dxfColor(136, 0, 76, 76)
                Case 137 : Return New dxfColor(137, 38, 76, 76)
                Case 138 : Return New dxfColor(138, 0, 38, 38)
                Case 139 : Return New dxfColor(139, 19, 38, 38)
                Case 140 : Return New dxfColor(140, 0, 191, 255)
                Case 141 : Return New dxfColor(141, 127, 223, 255)
                Case 142 : Return New dxfColor(142, 0, 124, 165)
                Case 143 : Return New dxfColor(143, 82, 145, 165)
                Case 144 : Return New dxfColor(144, 0, 95, 127)
                Case 145 : Return New dxfColor(145, 63, 111, 127)
                Case 146 : Return New dxfColor(146, 0, 57, 76)
                Case 147 : Return New dxfColor(147, 38, 66, 76)
                Case 148 : Return New dxfColor(148, 0, 28, 38)
                Case 149 : Return New dxfColor(149, 19, 33, 38)
                Case 150 : Return New dxfColor(150, 0, 127, 255)
                Case 151 : Return New dxfColor(151, 127, 191, 255) 'Light Blue
                Case 152 : Return New dxfColor(152, 0, 82, 165)
                Case 153 : Return New dxfColor(153, 82, 124, 165)
                Case 154 : Return New dxfColor(154, 0, 63, 127)
                Case 155 : Return New dxfColor(155, 63, 95, 127)
                Case 156 : Return New dxfColor(156, 0, 38, 76)
                Case 157 : Return New dxfColor(157, 38, 57, 76)
                Case 158 : Return New dxfColor(158, 0, 19, 38)
                Case 159 : Return New dxfColor(159, 19, 28, 38)
                Case 160 : Return New dxfColor(160, 0, 63, 255)
                Case 161 : Return New dxfColor(161, 127, 159, 255)
                Case 162 : Return New dxfColor(162, 0, 41, 165)
                Case 163 : Return New dxfColor(163, 82, 103, 165)
                Case 164 : Return New dxfColor(164, 0, 31, 127)
                Case 165 : Return New dxfColor(165, 63, 79, 127)
                Case 166 : Return New dxfColor(166, 0, 19, 76)
                Case 167 : Return New dxfColor(167, 38, 47, 76)
                Case 168 : Return New dxfColor(168, 0, 9, 38)
                Case 169 : Return New dxfColor(169, 19, 23, 38)
                Case 170 : Return New dxfColor(170, 0, 0, 255)
                Case 171 : Return New dxfColor(171, 127, 127, 255)
                Case 172 : Return New dxfColor(172, 0, 0, 165)
                Case 173 : Return New dxfColor(173, 82, 82, 165)
                Case 174 : Return New dxfColor(174, 0, 0, 127)
                Case 175 : Return New dxfColor(175, 63, 63, 127)
                Case 176 : Return New dxfColor(176, 0, 0, 76)
                Case 177 : Return New dxfColor(177, 38, 38, 76)
                Case 178 : Return New dxfColor(178, 0, 0, 38)
                Case 179 : Return New dxfColor(179, 19, 19, 38)
                Case 180 : Return New dxfColor(180, 63, 0, 255)
                Case 181 : Return New dxfColor(181, 159, 127, 255)
                Case 182 : Return New dxfColor(182, 41, 0, 165)
                Case 183 : Return New dxfColor(183, 103, 82, 165)
                Case 184 : Return New dxfColor(184, 31, 0, 127)
                Case 185 : Return New dxfColor(185, 79, 63, 127)
                Case 186 : Return New dxfColor(186, 19, 0, 76)
                Case 187 : Return New dxfColor(187, 47, 38, 76)
                Case 188 : Return New dxfColor(188, 9, 0, 38)
                Case 189 : Return New dxfColor(189, 23, 19, 38)
                Case 190 : Return New dxfColor(190, 127, 0, 255)
                Case 191 : Return New dxfColor(191, 191, 127, 255)
                Case 192 : Return New dxfColor(192, 82, 0, 165)
                Case 193 : Return New dxfColor(193, 124, 82, 165)
                Case 194 : Return New dxfColor(194, 63, 0, 127)
                Case 195 : Return New dxfColor(195, 95, 63, 127)
                Case 196 : Return New dxfColor(196, 38, 0, 76)
                Case 197 : Return New dxfColor(197, 57, 38, 76)
                Case 198 : Return New dxfColor(198, 19, 0, 38)
                Case 199 : Return New dxfColor(199, 28, 19, 38)
                Case 200 : Return New dxfColor(200, 191, 0, 255)
                Case 201 : Return New dxfColor(201, 223, 127, 255)
                Case 202 : Return New dxfColor(202, 124, 0, 165)
                Case 203 : Return New dxfColor(203, 145, 82, 165)
                Case 204 : Return New dxfColor(204, 95, 0, 127)
                Case 205 : Return New dxfColor(205, 111, 63, 127)
                Case 206 : Return New dxfColor(206, 57, 0, 76)
                Case 207 : Return New dxfColor(207, 66, 38, 76)
                Case 208 : Return New dxfColor(208, 28, 0, 38)
                Case 209 : Return New dxfColor(209, 33, 19, 38)
                Case 210 : Return New dxfColor(210, 255, 0, 255)
                Case 211 : Return New dxfColor(211, 255, 127, 255)
                Case 212 : Return New dxfColor(212, 165, 0, 165)
                Case 213 : Return New dxfColor(213, 165, 82, 165)
                Case 214 : Return New dxfColor(214, 127, 0, 127)
                Case 215 : Return New dxfColor(215, 127, 63, 127)
                Case 216 : Return New dxfColor(216, 76, 0, 76)
                Case 217 : Return New dxfColor(217, 76, 38, 76)
                Case 218 : Return New dxfColor(218, 38, 0, 38)
                Case 219 : Return New dxfColor(219, 38, 19, 38)
                Case 220 : Return New dxfColor(220, 255, 0, 191)
                Case 221 : Return New dxfColor(221, 255, 127, 223)
                Case 222 : Return New dxfColor(222, 165, 0, 124)
                Case 223 : Return New dxfColor(223, 165, 82, 145)
                Case 224 : Return New dxfColor(224, 127, 0, 95)
                Case 225 : Return New dxfColor(225, 127, 63, 111)
                Case 226 : Return New dxfColor(226, 76, 0, 57)
                Case 227 : Return New dxfColor(227, 76, 38, 66)
                Case 228 : Return New dxfColor(228, 38, 0, 28)
                Case 229 : Return New dxfColor(229, 38, 19, 33)
                Case 230 : Return New dxfColor(230, 255, 0, 127)
                Case 231 : Return New dxfColor(231, 255, 127, 191)
                Case 232 : Return New dxfColor(232, 165, 0, 82)
                Case 233 : Return New dxfColor(233, 165, 82, 124)
                Case 234 : Return New dxfColor(234, 127, 0, 63)
                Case 235 : Return New dxfColor(235, 127, 63, 95)
                Case 236 : Return New dxfColor(236, 76, 0, 38)
                Case 237 : Return New dxfColor(237, 76, 38, 57)
                Case 238 : Return New dxfColor(238, 38, 0, 19)
                Case 239 : Return New dxfColor(239, 38, 19, 28)
                Case 240 : Return New dxfColor(240, 255, 0, 63)
                Case 241 : Return New dxfColor(241, 255, 127, 159)
                Case 242 : Return New dxfColor(242, 165, 0, 41)
                Case 243 : Return New dxfColor(243, 165, 82, 103)
                Case 244 : Return New dxfColor(244, 127, 0, 31)
                Case 245 : Return New dxfColor(245, 127, 63, 79)
                Case 246 : Return New dxfColor(246, 76, 0, 19)
                Case 247 : Return New dxfColor(247, 76, 38, 47)
                Case 248 : Return New dxfColor(248, 38, 0, 9)
                Case 249 : Return New dxfColor(249, 38, 19, 23)
                Case 250 : Return New dxfColor(250, 0, 0, 0) 'BLACK
                Case 251 : Return New dxfColor(251, 51, 51, 51)
                Case 252 : Return New dxfColor(252, 102, 102, 102)
                Case 253 : Return New dxfColor(253, 153, 153, 153)
                Case 254 : Return New dxfColor(254, 244, 244, 244)
                Case 255 : Return New dxfColor(255, 255, 255, 255) 'WHITE
                Case 256 : Return New dxfColor(256, 0, 0, 0) 'by layer
                Case Else
                    Return New dxfColor(-1, dxfColors.Win32ToARGB(TVALUES.To_INT(aColorIndex)))
            End Select
        End Function
        Public Shared Function ACLName(aColor As dxxColors, Optional bStringNameOnly As Boolean = False) As String
            Select Case aColor
                Case dxxColors.ByBlock
                    Return dxfLinetypes.ByBlock
                Case dxxColors.Red
                    Return "Red"
                Case dxxColors.Yellow
                    Return "Yellow"
                Case dxxColors.Green
                    Return "Green"
                Case dxxColors.Cyan
                    Return "Cyan"
                Case dxxColors.Blue
                    Return "Blue"
                Case dxxColors.Black
                    Return "Black"
                Case dxxColors.Magenta
                    Return "Magenta"
                Case dxxColors.BlackWhite
                    Return "BlackWhite"
                Case dxxColors.Grey
                    Return "Grey"
                Case dxxColors.Orange
                    Return "Orange"
                Case dxxColors.LightBlue
                    Return "Light Blue"
                Case dxxColors.LightGreen
                    Return "Light Green"
                Case dxxColors.LightCyan
                    Return "Light Cyan"
                Case dxxColors.LightRed
                    Return "Light Red"
                Case dxxColors.LightMagenta
                    Return "Light Magenta"
                Case dxxColors.LightYellow
                    Return "Light Yellow"
                Case dxxColors.LightGrey
                    Return "Light Grey"
                Case dxxColors.ByLayer
                    Return dxfLinetypes.ByLayer
                Case 255
                    Return "White"
                Case dxxColors.Undefined
                    Return "Undefined"
                Case Else
                    If bStringNameOnly Then Return String.Empty
                    Dim aRGB As COLOR_ARGB
                    If aColor > 0 And aColor < 256 Then
                        Return aColor.ToString()
                    Else
                        If Not bStringNameOnly Then
                            aRGB = dxfColors.Win32ToARGB(aColor)
                            Return $"RBG({ aRGB.R },{ aRGB.B },{ aRGB.G})"
                        Else
                            Return String.Empty
                        End If
                    End If
            End Select
        End Function
        Public Shared Function GetColorData(aWin32Color As Integer, Optional aSystemColor As dxxSystemColorIndex = dxxSystemColorIndex.COLOR_BTNFACE) As dxfColor
            If (aSystemColor >= 1 And aSystemColor <= 21) Then
                Return New dxfColor(-1, dxfColors.GetSystemColorARGB(aSystemColor))
            Else
                Return New dxfColor(-1, dxfColors.Win32ToARGB(aWin32Color))
            End If
        End Function
        Public Shared Function CompareColors(A As Color, B As Color, Optional bCompareAlpha As Boolean = False) As Boolean
            If Not bCompareAlpha Then
                Return (A.R = B.R) And (A.B = B.B) And (A.G = B.G)
            Else
                Return (A.R = B.R) And (A.B = B.B) And (A.G = B.G) And (A.A = B.A)
            End If
        End Function
        Public Shared Function Invert(aWin32Color As Integer) As Integer
            Dim aclr As COLOR_ARGB = dxfColors.Win32ToARGB(aWin32Color)
            aclr.R = 255 - aclr.R
            aclr.B = 255 - aclr.B
            aclr.G = 255 - aclr.G
            Return aclr.ToWin32
        End Function
        Public Shared Function ColorIsReal(aWin32Color As Integer) As Boolean
            Dim rIsACL As Boolean = False
            Return ColorIsReal(aWin32Color, rIsACL)
        End Function
        Public Shared Function ColorIsReal(aWin32Color As Integer, ByRef rIsACL As Boolean) As Boolean
            rIsACL = aWin32Color >= 0 And aWin32Color <= 256
            Return aWin32Color <> dxxColors.ByBlock And aWin32Color <> dxxColors.ByLayer
        End Function
        Public Shared Function NearestACLColor(aWin64Color As Color, Optional bExactMatch As Boolean = False) As dxfColor
            Dim rIndex As dxxColors = dxxColors.Undefined
            Return NearestACLColor(aWin64Color, rIndex, bExactMatch)
        End Function
        Public Shared Function NearestACLColor(aWin64Color As Color, ByRef rIndex As dxxColors, Optional bExactMatch As Boolean = False) As dxfColor
            rIndex = dxxColors.Undefined
            Dim aRGB As COLOR_ARGB = dxfColors.Win64ToARGB(aWin64Color)
            Dim i As Integer
            Dim Difs(0 To 255, 0 To 3) As Double
            Dim Avgs(0 To 255) As Double
            Dim aclr As dxfColor
            Dim mindif As Double = 4 * 255
            Dim i1 As Integer
            Dim i2 As Integer
            Dim bRGB As COLOR_ARGB
            Dim idx As Integer = 7
            For i = dxxColors.ByBlock + 1 To dxxColors.ByLayer - 1
                aclr = dxfColors.Color(i)
                bRGB = aclr.RGB
                i1 = bRGB.R
                i2 = aRGB.R
                Difs(i, 1) = Math.Abs(i1 - i2)
                i1 = bRGB.G
                i2 = aRGB.G
                Difs(i, 2) = Math.Abs(i1 - i2)
                i1 = bRGB.B
                i2 = aRGB.B
                Difs(i, 3) = Math.Abs(i1 - i2)
                Avgs(i) = (Difs(i, 1) + Difs(i, 2) + Difs(i, 3)) / 3
                If Avgs(i) < mindif Then
                    mindif = Avgs(i)
                    idx = i
                End If
                If bExactMatch Then
                    If aclr.RGB.R = aRGB.R And aclr.RGB.G = aRGB.G And aclr.RGB.B = aRGB.B Then
                        mindif = 0
                        Exit For
                    End If
                End If
            Next i
            If bExactMatch Then
                If mindif <> 0 Then
                    rIndex = dxxColors.Undefined
                    Return New dxfColor(-1, aRGB)
                End If
            End If
            rIndex = TVALUES.To_INT(idx)
            Return Color(rIndex)
        End Function
        Public Shared Function RandomACIColor(Optional bIncludeLogicals As Boolean = False, Optional aColorToExclude As dxxColors = dxxColors.Undefined) As dxxColors
            Dim _rVal As dxxColors = dxxColors.BlackWhite
            Do
                If Not bIncludeLogicals Then
                    _rVal = dxfUtils.RandomInteger(1, 255)
                Else
                    _rVal = dxfUtils.RandomInteger(0, 256)
                End If
                If _rVal <> aColorToExclude Then Exit Do
            Loop
            Return _rVal
        End Function
        Public Shared Function RandomWin32Color(Optional aColorToExclude As Integer = -1) As Integer
            Dim _rVal As Integer
            Do
                _rVal = RGB(TVALUES.To_INT(Rnd() * 255), TVALUES.To_INT(Rnd() * 255), TVALUES.To_INT(Rnd() * 255))
                If _rVal <> aColorToExclude Then Exit Do
            Loop
            Return _rVal
        End Function
        Public Shared Function RandomARGBColor(Optional aColorToExclude As COLOR_ARGB? = Nothing) As COLOR_ARGB
            Dim _rVal As COLOR_ARGB
            If aColorToExclude Is Nothing Or Not aColorToExclude.HasValue Then
                Return New COLOR_ARGB(TVALUES.ToByte(Rnd() * 255), TVALUES.ToByte(Rnd() * 255), TVALUES.ToByte(Rnd() * 255))
            Else
                Do
                    _rVal = New COLOR_ARGB(TVALUES.ToByte(Rnd() * 255), TVALUES.ToByte(Rnd() * 255), TVALUES.ToByte(Rnd() * 255))
                    If _rVal <> aColorToExclude Then Exit Do
                Loop
                Return _rVal
            End If
        End Function
        Public Shared Function ACLToWin64(aACLIndex As Integer) As Color
            Return dxfColors.GetColor(aACLIndex, True).ToWin64
        End Function
        Public Shared Function Win32ToWin64(aWin32Color As Integer) As Color
            Return ColorTranslator.FromWin32(aWin32Color)
        End Function
        Public Shared Function Win64ToWin32(aWin64Color As Color) As Integer
            Return ColorTranslator.ToWin32(aWin64Color)
        End Function
        Public Shared Function Win64ToHSB(aWin64Color As Color) As COLOR_HSB
            Return Win32ToHSB(ColorTranslator.ToWin32(aWin64Color))
        End Function
        Public Shared Function Win64ToARGB(aWin64Color As Color) As COLOR_ARGB
            Return dxfColors.Win32ToARGB(Win64ToWin32(aWin64Color))
        End Function
        Public Shared Function ARGBToWin64(aARGB As COLOR_ARGB) As Color
            If aARGB.A <= 0 Then aARGB.A = 255
            Dim _rVal As Color = System.Drawing.Color.FromArgb(TVALUES.To_INT(aARGB.A), TVALUES.To_INT(aARGB.R), TVALUES.To_INT(aARGB.G), TVALUES.To_INT(aARGB.B))
            Return _rVal
        End Function
        Public Shared Function ARGBToWin32(aARGB As COLOR_ARGB) As Integer
            Return RGB(TVALUES.To_INT(aARGB.R), TVALUES.To_INT(aARGB.G), TVALUES.To_INT(aARGB.B))
        End Function
        Public Shared Function Win32ToHSB(aWin32Color As Integer) As COLOR_HSB
            Return CType(dxfColors.Win32ToARGB(aWin32Color), COLOR_HSB)
        End Function
        Public Shared Function Win32ToARGB(aWin32Color As Integer) As COLOR_ARGB
            Dim _rVal As New COLOR_ARGB
            Try
                _rVal.R = aWin32Color And &HFF&
                _rVal.G = (aWin32Color And &HFF00&) / &H100&
                _rVal.B = (aWin32Color And &HFF0000) / &H10000
                Return _rVal
            Catch ex As Exception
                Return _rVal
            End Try
        End Function
        Public Shared Function Win32ColorDescriptor(aColor As Integer, Optional bIncludeHueSatBrit As Boolean = False) As String
            Dim _rVal As String = String.Empty
            Dim aACL As dxfColor = dxfColors.GetColor(aColor, False, False)
            If aACL.ACLNumber >= 0 Then
                _rVal = aACL.Descriptor(bIncludeHueSatBrit)
            Else
                Dim aRGB As COLOR_ARGB = dxfColors.Win32ToARGB(aColor)
                TLISTS.Add(_rVal, $"Windows={ aColor}", bAllowDuplicates:=True, aDelimitor:=dxfGlobals.Delim)
                TLISTS.Add(_rVal, $"RGB=({ aRGB.R },{ aRGB.G },{ aRGB.B })", bAllowDuplicates:=True, aDelimitor:=dxfGlobals.Delim)
                If bIncludeHueSatBrit Then
                    Dim aclr As COLOR_HSB = CType(aRGB, COLOR_HSB)
                    TLISTS.Add(_rVal, $"HSB=({ aclr.H },{ aclr.S},{ aclr.B })", bAllowDuplicates:=True, aDelimitor:=dxfGlobals.Delim)
                End If
            End If
            Return _rVal
        End Function
        Public Shared Function GetSystemColorARGB(aSystemColor As dxxSystemColorIndex) As COLOR_ARGB
            Select Case aSystemColor
                Case dxxSystemColorIndex.COLOR_3DDKSHADOW
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.ControlDark)
                Case dxxSystemColorIndex.COLOR_3DFACE
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.Control)
                Case dxxSystemColorIndex.COLOR_3DHIGHLIGHT, dxxSystemColorIndex.COLOR_3DHILIGHT, dxxSystemColorIndex.COLOR_3DLIGHT
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.ButtonHighlight)
                Case dxxSystemColorIndex.COLOR_3DSHADOW
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.ButtonHighlight)
                Case dxxSystemColorIndex.COLOR_ACTIVEBORDER
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.ActiveBorder)
                Case dxxSystemColorIndex.COLOR_ACTIVECAPTION
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.ActiveCaption)
                Case dxxSystemColorIndex.COLOR_APPWORKSPACE
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.AppWorkspace)
                Case dxxSystemColorIndex.COLOR_BACKGROUND
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.Window)
                Case dxxSystemColorIndex.COLOR_BTNFACE
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.ButtonFace)
                Case dxxSystemColorIndex.COLOR_BTNHIGHLIGHT
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.ButtonHighlight)
                Case dxxSystemColorIndex.COLOR_BTNSHADOW
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.ButtonShadow)
                Case dxxSystemColorIndex.COLOR_BTNTEXT
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.ControlText)
                Case dxxSystemColorIndex.COLOR_CAPTIONTEXT, dxxSystemColorIndex.COLOR_GRAYTEXT
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.GrayText)
                Case dxxSystemColorIndex.COLOR_DESKTOP
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.Desktop)
                Case dxxSystemColorIndex.COLOR_GRADIENTACTIVECAPTION
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.GradientActiveCaption)
                Case dxxSystemColorIndex.COLOR_GRADIENTINACTIVECAPTION
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.GradientInactiveCaption)
                Case dxxSystemColorIndex.COLOR_HIGHLIGHT
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.Highlight)
                Case dxxSystemColorIndex.COLOR_HIGHLIGHTTEXT
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.HighlightText)
                Case dxxSystemColorIndex.COLOR_HOTLIGHT
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.HotTrack)
                Case dxxSystemColorIndex.COLOR_INACTIVEBORDER
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.InactiveBorder)
                Case dxxSystemColorIndex.COLOR_INACTIVECAPTION
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.InactiveCaption)
                Case dxxSystemColorIndex.COLOR_INACTIVECAPTIONTEXT
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.InactiveCaptionText)
                Case dxxSystemColorIndex.COLOR_INFOBK
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.Info)
                Case dxxSystemColorIndex.COLOR_INFOTEXT
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.InfoText)
                Case dxxSystemColorIndex.COLOR_MENU
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.Menu)
                Case dxxSystemColorIndex.COLOR_MENUBAR
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.MenuBar)
                Case dxxSystemColorIndex.COLOR_MENUHILIGHT
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.MenuHighlight)
                Case dxxSystemColorIndex.COLOR_MENUTEXT
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.MenuText)
                Case dxxSystemColorIndex.COLOR_SCROLLBAR
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.ScrollBar)
                Case dxxSystemColorIndex.COLOR_WINDOW
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.Window)
                Case dxxSystemColorIndex.COLOR_WINDOWFRAME
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.WindowFrame)
                Case dxxSystemColorIndex.COLOR_WINDOWTEXT
                    Return dxfColors.Win64ToARGB(Drawing.SystemColors.WindowText)
                Case Else
                    Return dxfColors.Win32ToARGB(System.Drawing.Color.White.ToArgb)
            End Select
        End Function
        Public Shared Function SelectColor(aOwner As IWin32Window, ByRef rCanceled As Boolean, Optional InitColor As dxxColors = dxxColors.Undefined, Optional bNoLogical As Boolean = False, Optional bNoWindows As Boolean = False, Optional aWin64Color As Color = Nothing) As dxfColor
            Dim _rVal As dxfColor = Nothing
            Dim aFrm As New frmColorPicker
            Try
                Dim aclr As dxfColor = aFrm.SelectColor(aOwner, rCanceled, InitColor, bNoLogical, bNoWindows, aWin64Color)
                If Not rCanceled Then _rVal = aclr
                Return _rVal
            Catch ex As Exception
                Throw ex
                Return _rVal
            Finally
                aFrm.Dispose()
                aFrm = Nothing
            End Try
        End Function
#End Region 'Shared Methods
    End Class 'dxfColors
End Namespace