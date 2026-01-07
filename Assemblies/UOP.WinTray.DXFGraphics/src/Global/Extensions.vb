Imports System.Runtime.CompilerServices

Namespace UOP.DXFGraphics

    Public Module Extensions
        <Extension()>
        Function IsLogical(ByVal aColor As dxxColors) As Boolean

            Return aColor = dxxColors.Undefined Or aColor = dxxColors.ByBlock Or aColor = dxxColors.ByLayer

        End Function


        <Extension()>
        Function IsLogical(ByVal aLineWt As dxxLineWeights) As Boolean

            Return aLineWt = dxxLineWeights.Undefined Or aLineWt = dxxLineWeights.ByBlock Or aLineWt = dxxLineWeights.ByLayer

        End Function

        <Extension()>
        Function EntityType(ByVal aDimType As dxxDimTypes) As dxxEntityTypes

            Select Case aDimType
                Case dxxDimTypes.Angular3P
                    Return dxxEntityTypes.DimAngular3P
                Case dxxDimTypes.Angular
                    Return dxxEntityTypes.DimAngular
                Case dxxDimTypes.LinearAligned
                    Return dxxEntityTypes.DimLinearA
                Case dxxDimTypes.LinearHorizontal
                    Return dxxEntityTypes.DimLinearH
                Case dxxDimTypes.LinearVertical
                    Return dxxEntityTypes.DimLinearV
                Case dxxDimTypes.OrdHorizontal
                    Return dxxEntityTypes.DimOrdinateH
                Case dxxDimTypes.OrdVertical
                    Return dxxEntityTypes.DimOrdinateV
                Case dxxDimTypes.Diametric
                    Return dxxEntityTypes.DimRadialD
                Case dxxDimTypes.Radial
                    Return dxxEntityTypes.DimRadialR
                Case Else
                    Return dxxEntityTypes.Undefined
            End Select

        End Function

        <Extension()>
        Function DimensionType(ByVal aEntityType As dxxEntityTypes) As dxxDimTypes

            Select Case aEntityType
                Case dxxEntityTypes.DimAngular3P
                    Return dxxDimTypes.Angular3P
                Case dxxEntityTypes.DimAngular
                    Return dxxDimTypes.Angular
                Case dxxEntityTypes.DimLinearA
                    Return dxxDimTypes.LinearAligned
                Case dxxEntityTypes.DimLinearH
                    Return dxxDimTypes.LinearHorizontal
                Case dxxEntityTypes.DimLinearV
                    Return dxxDimTypes.LinearVertical
                Case dxxEntityTypes.DimOrdinateH
                    Return dxxDimTypes.OrdHorizontal
                Case dxxEntityTypes.DimOrdinateV
                    Return dxxDimTypes.OrdVertical
                Case dxxEntityTypes.DimRadialD
                    Return dxxDimTypes.Diametric
                Case dxxEntityTypes.DimRadialR
                    Return dxxDimTypes.Radial
                Case Else
                    Return dxxDimTypes.Undefined
            End Select

        End Function


        <Extension()>
        Function GraphicType(ByVal aEntityType As dxxEntityTypes) As dxxGraphicTypes

            Select Case aEntityType
                Case dxxEntityTypes.DimAngular3P, dxxEntityTypes.DimAngular, dxxEntityTypes.DimLinearA, dxxEntityTypes.DimLinearH, dxxEntityTypes.DimLinearV, dxxEntityTypes.DimOrdinateH, dxxEntityTypes.DimOrdinateV, dxxEntityTypes.DimRadialD, dxxEntityTypes.DimRadialR
                    Return dxxGraphicTypes.Dimension
                Case dxxEntityTypes.Line
                    Return dxxGraphicTypes.Line
                Case dxxEntityTypes.Arc, dxxEntityTypes.Circle
                    Return dxxGraphicTypes.Arc
                Case dxxEntityTypes.Attdef, dxxEntityTypes.Attribute, dxxEntityTypes.Text
                    Return dxxGraphicTypes.Text
                Case dxxEntityTypes.MText
                    Return dxxGraphicTypes.MText
                Case dxxEntityTypes.Solid, dxxEntityTypes.Trace
                    Return dxxGraphicTypes.Solid
                Case dxxEntityTypes.Symbol
                    Return dxxGraphicTypes.Symbol
                Case dxxEntityTypes.Slot, dxxEntityTypes.Hole
                    Return dxxGraphicTypes.Hole
                Case dxxEntityTypes.Bezier
                    Return dxxGraphicTypes.Bezier
                Case dxxEntityTypes.Character
                    Return dxxGraphicTypes.Text
                Case dxxEntityTypes.Ellipse
                    Return dxxGraphicTypes.Ellipse
                Case dxxEntityTypes.EndBlock
                    Return dxxGraphicTypes.Undefined
                Case dxxEntityTypes.SequenceEnd
                    Return dxxGraphicTypes.SequenceEnd
                Case dxxEntityTypes.Point
                    Return dxxGraphicTypes.Point
                Case dxxEntityTypes.Polygon
                    Return dxxGraphicTypes.Polygon
                Case dxxEntityTypes.Polyline
                    Return dxxGraphicTypes.Polyline
                Case dxxEntityTypes.Shape
                    Return dxxGraphicTypes.Shape
                Case dxxEntityTypes.Table
                    Return dxxGraphicTypes.Table
                Case Else
                    Return dxxGraphicTypes.Undefined
            End Select

        End Function

        <Extension()>
        Function DimensionFamily(ByVal aDimType As dxxDimTypes) As dxxDimensionTypes

            Select Case aDimType
                Case dxxDimTypes.Angular3P, dxxDimTypes.Angular
                    Return dxxDimensionTypes.Angular
                Case dxxDimTypes.LinearAligned, dxxDimTypes.LinearHorizontal, dxxDimTypes.LinearVertical
                    Return dxxDimensionTypes.Linear
                Case dxxDimTypes.OrdHorizontal, dxxDimTypes.OrdVertical
                    Return dxxDimensionTypes.Ordinate
                Case dxxDimTypes.Diametric, dxxDimTypes.Radial
                    Return dxxDimensionTypes.Radial
                Case Else
                    Return dxxDimensionTypes.Undefined
            End Select

        End Function

        <Extension()>
        Function EntityType(ByVal aGraphicType As dxxGraphicTypes) As dxxEntityTypes
            Select Case aGraphicType
                Case dxxGraphicTypes.Arc
                    Return dxxEntityTypes.Circle
                Case dxxGraphicTypes.Bezier
                    Return dxxEntityTypes.Bezier
                Case dxxGraphicTypes.Dimension
                    Return dxxEntityTypes.DimLinearH
                Case dxxGraphicTypes.Ellipse
                    Return dxxEntityTypes.Ellipse
                Case dxxGraphicTypes.Hatch
                    Return dxxEntityTypes.Hatch
                Case dxxGraphicTypes.Hole
                    Return dxxEntityTypes.Hole
                Case dxxGraphicTypes.Insert
                    Return dxxEntityTypes.Insert
                Case dxxGraphicTypes.Leader
                    Return dxxEntityTypes.Leader
                Case dxxGraphicTypes.Line
                    Return dxxEntityTypes.Line
                Case dxxGraphicTypes.Point
                    Return dxxEntityTypes.Point
                Case dxxGraphicTypes.Polygon
                    Return dxxEntityTypes.Polygon
                Case dxxGraphicTypes.Polyline
                    Return dxxEntityTypes.Polyline
                Case dxxGraphicTypes.Solid
                    Return dxxEntityTypes.Solid
                Case dxxGraphicTypes.Symbol
                    Return dxxEntityTypes.Symbol
                Case dxxGraphicTypes.Table
                    Return dxxEntityTypes.Table
                Case dxxGraphicTypes.MText
                    Return dxxEntityTypes.MText
                Case dxxGraphicTypes.Text
                    Return dxxEntityTypes.Text
                Case Else
                    Return dxxEntityTypes.Undefined
            End Select
        End Function

        <Extension()>
        Function SettingType(ByVal aRefType As dxxReferenceTypes) As dxxSettingTypes

            Select Case aRefType
                Case dxxReferenceTypes.DIMSETTINGS
                    Return dxxSettingTypes.DIMSETTINGS
                Case dxxReferenceTypes.TABLESETTINGS
                    Return dxxSettingTypes.TABLESETTINGS
                Case dxxReferenceTypes.SYMBOLSETTINGS
                    Return dxxSettingTypes.SYMBOLSETTINGS
                Case dxxReferenceTypes.HEADER
                    Return dxxSettingTypes.HEADER
                Case dxxReferenceTypes.DIMOVERRIDES
                    Return dxxSettingTypes.DIMOVERRIDES

                Case dxxReferenceTypes.TEXTSETTINGS
                    Return dxxSettingTypes.TEXTSETTINGS
                Case dxxReferenceTypes.LINETYPESETTINGS
                    Return dxxSettingTypes.LINETYPESETTINGS
                Case dxxReferenceTypes.SCREENSETTINGS
                    Return dxxSettingTypes.SCREENSETTINGS
                Case dxxReferenceTypes.DISPLAYSETTINGS
                    Return dxxSettingTypes.DISPLAYSETTINGS


                Case Else
                    Return dxxSettingTypes.UNDEFINED
            End Select
        End Function
        <Extension()>
        Function GroupCode(ByVal aHeaderVar As dxxHeaderVars) As Integer
            Return dxfEnums.GroupCode(aHeaderVar)
        End Function
        <Extension()>
        Function GroupCode(ByVal aDimStyleProperty As dxxDimStyleProperties) As Integer

            Select Case aDimStyleProperty
                Case dxxDimStyleProperties.DIMPOST ' = 7
                    Return 3
                Case dxxDimStyleProperties.DIMAPOST ' = 8
                    Return 4
                Case dxxDimStyleProperties.DIMSCALE ' = 9
                    Return 40
                Case dxxDimStyleProperties.DIMASZ ' = 10
                    Return 41
                Case dxxDimStyleProperties.DIMEXO ' = 11
                    Return 42
                Case dxxDimStyleProperties.DIMDLI ' = 12
                    Return 43
                Case dxxDimStyleProperties.DIMEXE ' = 13
                    Return 44
                Case dxxDimStyleProperties.DIMRND ' = 14
                    Return 45
                Case dxxDimStyleProperties.DIMDLE ' = 15
                    Return 46
                Case dxxDimStyleProperties.DIMTP ' = 16
                    Return 47
                Case dxxDimStyleProperties.DIMTM ' = 17
                    Return 48
                Case dxxDimStyleProperties.DIMFXL ' = 18
                    Return 49
                Case dxxDimStyleProperties.DIMJOGANG ' = 19
                    Return 50
                Case dxxDimStyleProperties.DIMTFILL ' = 20
                    Return 69
                Case dxxDimStyleProperties.DIMTFILLCLR ' = 21
                    Return 70
                Case dxxDimStyleProperties.DIMTOL ' = 22
                    Return 71
                Case dxxDimStyleProperties.DIMLIM ' = 23
                    Return 72
                Case dxxDimStyleProperties.DIMTIH ' = 24
                    Return 73
                Case dxxDimStyleProperties.DIMTOH ' = 25
                    Return 74
                Case dxxDimStyleProperties.DIMSE1 ' = 26
                    Return 75
                Case dxxDimStyleProperties.DIMSE2 ' = 27
                    Return 76
                Case dxxDimStyleProperties.DIMTAD ' = 28
                    Return 77
                Case dxxDimStyleProperties.DIMZIN ' = 29
                    Return 78
                Case dxxDimStyleProperties.DIMAZIN ' = 30
                    Return 79
                Case dxxDimStyleProperties.DIMARCSYM ' = 31
                    Return 90
                Case dxxDimStyleProperties.DIMTXT ' = 32
                    Return 140
                Case dxxDimStyleProperties.DIMCEN ' = 33
                    Return 141
                Case dxxDimStyleProperties.DIMTSZ ' = 34
                    Return 142
                Case dxxDimStyleProperties.DIMALTF ' = 35
                    Return 143
                Case dxxDimStyleProperties.DIMLFAC ' = 36
                    Return 144
                Case dxxDimStyleProperties.DIMTVP ' = 37
                    Return 145
                Case dxxDimStyleProperties.DIMTFAC ' = 38
                    Return 146

                Case dxxDimStyleProperties.DIMGAP ' = 39
                    Return 147
                Case dxxDimStyleProperties.DIMALTRND ' = 40
                    Return 148
                Case dxxDimStyleProperties.DIMALT ' = 41
                    Return 170
                Case dxxDimStyleProperties.DIMALTD ' = 42
                    Return 171
                Case dxxDimStyleProperties.DIMTOFL ' = 43
                    Return 172
                Case dxxDimStyleProperties.DIMSAH ' = 44
                    Return 173
                Case dxxDimStyleProperties.DIMTIX ' = 45
                    Return 174
                Case dxxDimStyleProperties.DIMSOXD ' = 46
                    Return 175
                Case dxxDimStyleProperties.DIMCLRD ' = 47
                    Return 176
                Case dxxDimStyleProperties.DIMCLRE ' = 48
                    Return 177
                Case dxxDimStyleProperties.DIMCLRT ' = 49
                    Return 178
                Case dxxDimStyleProperties.DIMADEC ' = 50
                    Return 179

                Case dxxDimStyleProperties.DIMUNIT ' = 51  (obsolete)
                    Return 270
                Case dxxDimStyleProperties.DIMDEC ' = 52
                    Return 271
                Case dxxDimStyleProperties.DIMTDEC ' = 53
                    Return 272
                Case dxxDimStyleProperties.DIMALTU ' = 54
                    Return 273
                Case dxxDimStyleProperties.DIMALTTD ' = 55
                    Return 274
                Case dxxDimStyleProperties.DIMAUNIT ' = 56
                    Return 275
                Case dxxDimStyleProperties.DIMFRAC ' = 57
                    Return 276
                Case dxxDimStyleProperties.DIMLUNIT ' = 58
                    Return 277
                Case dxxDimStyleProperties.DIMDSEP ' = 59
                    Return 278
                Case dxxDimStyleProperties.DIMTMOVE ' = 60
                    Return 279
                Case dxxDimStyleProperties.DIMJUST ' = 61
                    Return 280
                Case dxxDimStyleProperties.DIMSD1 ' = 62
                    Return 281
                Case dxxDimStyleProperties.DIMSD2 ' = 63
                    Return 282
                Case dxxDimStyleProperties.DIMTOLJ ' = 64
                    Return 283
                Case dxxDimStyleProperties.DIMTZIN ' = 65
                    Return 284
                Case dxxDimStyleProperties.DIMALTZ ' = 66
                    Return 285
                Case dxxDimStyleProperties.DIMALTTZ ' = 67
                    Return 286
                Case dxxDimStyleProperties.DIMFIT ' = 68
                    Return 287
                Case dxxDimStyleProperties.DIMUPT ' = 69
                    Return 288
                Case dxxDimStyleProperties.DIMATFIT ' = 70
                    Return 289
                Case dxxDimStyleProperties.DIMFXLON ' = 71
                    Return 290
                Case dxxDimStyleProperties.DIMTXSTY ' = 72
                    Return 340
                Case dxxDimStyleProperties.DIMLDRBLK ' = 73
                    Return 341
                Case dxxDimStyleProperties.DIMBLK ' = 74
                    Return 342
                Case dxxDimStyleProperties.DIMBLK1 ' = 75
                    Return 343
                Case dxxDimStyleProperties.DIMBLK2 ' = 76
                    Return 344
                Case dxxDimStyleProperties.DIMLTYPE ' = 77
                    Return 345
                Case dxxDimStyleProperties.DIMLTEX1 ' = 78
                    Return 346
                Case dxxDimStyleProperties.DIMLTEX2 ' = 79
                    Return 347
                Case dxxDimStyleProperties.DIMLWD ' = 80
                    Return 371

                Case dxxDimStyleProperties.DIMLWE ' = 81
                    Return 372
                Case dxxDimStyleProperties.DIMTXTDIRECTION  ' = 82
                    Return 370
                    'hidden properties follow
                Case dxxDimStyleProperties.DIMZEROSUPPRESSION
                    Return 1
                Case dxxDimStyleProperties.DIMTOLZEROSUPPRESSION ' = 90
                    Return 2
                Case dxxDimStyleProperties.DIMZEROSUPPRESSION_ARCH ' = 91
                    Return 3
                Case dxxDimStyleProperties.DIMPREFIX ' = 92
                    Return 4
                Case dxxDimStyleProperties.DIMSUFFIX ' = 93
                    Return 5
                Case dxxDimStyleProperties.DIMAPREFIX ' = 94
                    Return 6
                Case dxxDimStyleProperties.DIMASUFFIX ' = 95
                    Return 7
                Case dxxDimStyleProperties.DIMTANGLE ' = 96
                    Return 8
                Case dxxDimStyleProperties.DIMTXSTY_NAME ' = 82
                    Return 9
                Case dxxDimStyleProperties.DIMLDRBLK_NAME ' = 83
                    Return 10
                Case dxxDimStyleProperties.DIMBLK_NAME ' = 84
                    Return 11
                Case dxxDimStyleProperties.DIMBLK1_NAME ' = 85
                    Return 12
                Case dxxDimStyleProperties.DIMBLK2_NAME ' = 86
                    Return 13
                Case dxxDimStyleProperties.DIMLTYPE_NAME ' = 87
                    Return 14
                Case dxxDimStyleProperties.DIMLTEX1_NAME ' = 88
                    Return 15
                Case dxxDimStyleProperties.DIMLTEX2_NAME ' = 89
                    Return 16
                Case Else
                    Return -1

            End Select

        End Function

        <Extension()>
        Function PropertyType(ByVal aHeaderVar As dxxHeaderVars) As dxxPropertyTypes
            Select Case aHeaderVar
                Case dxxHeaderVars.ACADVER ' = 1
                    Return dxxPropertyTypes.dxf_String
                Case dxxHeaderVars.ACADMAINTVER ' = 2
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.DWGCODEPAGE ' = 3
                    Return dxxPropertyTypes.dxf_String
                Case dxxHeaderVars.LASTSAVEDBY, dxxHeaderVars.HYPERLINKBASE, dxxHeaderVars.STYLESHEET, dxxHeaderVars.PROJECTNAME
                    Return dxxPropertyTypes.dxf_String
                Case dxxHeaderVars.INSBASE, dxxHeaderVars.PINSBASE, dxxHeaderVars.EXTMIN, dxxHeaderVars.PEXTMIN, dxxHeaderVars.EXTMAX, dxxHeaderVars.PEXTMAX
                    Return dxxPropertyTypes.dxf_Double

                Case dxxHeaderVars.LIMMIN, dxxHeaderVars.PLIMMIN, dxxHeaderVars.LIMMAX, dxxHeaderVars.PLIMMAX
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.ORTHOMODE ' = 10
                    Return dxxPropertyTypes.Switch
                Case dxxHeaderVars.REGENMODE ' = 11
                    Return dxxPropertyTypes.Switch
                Case dxxHeaderVars.FILLMODE ' = 12
                    Return dxxPropertyTypes.Switch
                Case dxxHeaderVars.QTEXTMODE ' = 13
                    Return dxxPropertyTypes.Switch
                Case dxxHeaderVars.MIRRTEXT ' = 14
                    Return dxxPropertyTypes.Switch
                Case dxxHeaderVars.LTSCALE ' = 15
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.ATTMODE ' = 16
                    Return dxxPropertyTypes.Switch
                Case dxxHeaderVars.TEXTSIZE ' = 17
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.TRACEWID ' = 18
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.TEXTSTYLE ' = 19
                    Return dxxPropertyTypes.dxf_String
                Case dxxHeaderVars.CLAYER ' = 20
                    Return dxxPropertyTypes.dxf_String
                Case dxxHeaderVars.CELTYPE ' = 21
                    Return dxxPropertyTypes.dxf_String
                Case dxxHeaderVars.CECOLOR ' = 22
                    Return dxxPropertyTypes.Color
                Case dxxHeaderVars.CELTSCALE ' = 23
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.DISPSILH ' = 24
                    Return dxxPropertyTypes.Switch
                Case dxxHeaderVars.DIMASSOC
                    Return dxxPropertyTypes.dxf_Integer

                Case dxxHeaderVars.DIMASO, dxxHeaderVars.DIMSHO
                    Return dxxPropertyTypes.Switch

                Case dxxHeaderVars.DIMSTYLE ' = 61
                    Return dxxPropertyTypes.dxf_String
                Case dxxHeaderVars.LUNITS ' = 101
                    Return dxxPropertyTypes.dxf_Integer

                Case dxxHeaderVars.LUPREC ' = 102
                    Return dxxPropertyTypes.dxf_Integer

                Case dxxHeaderVars.SKETCHINC ' = 103
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.FILLETRAD ' = 104
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.AUNITS ' = 105
                    Return dxxPropertyTypes.dxf_Integer

                Case dxxHeaderVars.AUPREC ' = 106
                    Return dxxPropertyTypes.dxf_Integer

                Case dxxHeaderVars.MENU ' = 107
                    Return dxxPropertyTypes.dxf_String
                Case dxxHeaderVars.ELEVATION, dxxHeaderVars.PELEVATION, dxxHeaderVars.THICKNESS ' = 108-110
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.LIMCHECK, dxxHeaderVars.PLIMCHECK
                    Return dxxPropertyTypes.Switch
                Case dxxHeaderVars.CHAMFERA, dxxHeaderVars.CHAMFERB
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.CHAMFERC
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.CHAMFERD
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.SKPOLY ' = 116
                    Return dxxPropertyTypes.Switch
                Case dxxHeaderVars.TDCREATE, dxxHeaderVars.TDUCREATE, dxxHeaderVars.TDUPDATE, dxxHeaderVars.TDUUPDATE, dxxHeaderVars.TDINDWG, dxxHeaderVars.TDUSRTIMER ' = 117-122
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.USRTIMER ' = 123
                    Return dxxPropertyTypes.Switch
                Case dxxHeaderVars.ANGBASE ' = 124
                    Return dxxPropertyTypes.Angle
                Case dxxHeaderVars.ANGDIR ' = 125
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.PDMODE ' = 126
                    Return dxxPropertyTypes.dxf_Integer

                Case dxxHeaderVars.PDSIZE ' = 127
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.PLINEWID ' = 128
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.SPLFRAME ' = 128
                    Return dxxPropertyTypes.Switch
                Case dxxHeaderVars.SPLINETYPE ' = 130
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.SPLINESEGS ' = 131
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.HANDSEED ' = 133
                    Return dxxPropertyTypes.dxf_String
                Case dxxHeaderVars.SURFTAB1, dxxHeaderVars.SURFTAB2 ' = 134-135
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.SURFTYPE ' = 136
                    Return dxxPropertyTypes.dxf_Integer

                Case dxxHeaderVars.SURFU, dxxHeaderVars.SURFV ' = 137,138
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.UCSBASE, dxxHeaderVars.UCSNAME, dxxHeaderVars.PUCSBASE, dxxHeaderVars.PUCSNAME
                    Return dxxPropertyTypes.dxf_String
                Case dxxHeaderVars.UCSORG, dxxHeaderVars.PUCSORG
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.UCSXDIR, dxxHeaderVars.PUCSXDIR, dxxHeaderVars.UCSYDIR, dxxHeaderVars.PUCSYDIR
                    Return dxxPropertyTypes.dxf_Double

                Case dxxHeaderVars.UCSORTHOREF, dxxHeaderVars.PUCSORTHOREF ' = 144
                    Return dxxPropertyTypes.dxf_String
                Case dxxHeaderVars.UCSORTHOVIEW, dxxHeaderVars.PUCSORTHOVIEW
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.UCSORGTOP, dxxHeaderVars.UCSORGBOTTOM, dxxHeaderVars.UCSORGLEFT, dxxHeaderVars.UCSORGRIGHT, dxxHeaderVars.UCSORGFRONT, dxxHeaderVars.UCSORGBACK
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.PUCSORGTOP, dxxHeaderVars.PUCSORGBOTTOM, dxxHeaderVars.PUCSORGLEFT, dxxHeaderVars.PUCSORGRIGHT, dxxHeaderVars.PUCSORGFRONT, dxxHeaderVars.PUCSORGBACK
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.USERI1, dxxHeaderVars.USERI2, dxxHeaderVars.USERI3, dxxHeaderVars.USERI4, dxxHeaderVars.USERI5
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.USERR1, dxxHeaderVars.USERR2, dxxHeaderVars.USERR3, dxxHeaderVars.USERR4, dxxHeaderVars.USERR5
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.WORLDVIEW ' = 175
                    Return dxxPropertyTypes.Switch
                Case dxxHeaderVars.SHADEDGE ' = 176
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.SHADEDIF ' = 177
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.TILEMODE ' = 178
                    Return dxxPropertyTypes.Switch
                Case dxxHeaderVars.MAXACTVP ' = 179
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.UNITMODE, dxxHeaderVars.PLINEGEN, dxxHeaderVars.MEASUREMENT, dxxHeaderVars.LWDISPLAY
                    Return dxxPropertyTypes.Switch
                Case dxxHeaderVars.PSLTSCALE, dxxHeaderVars.VISRETAIN, dxxHeaderVars.PROXYGRAPHICS
                    Return dxxPropertyTypes.Switch
                Case dxxHeaderVars.TREEDEPTH ' = 190
                    Return dxxPropertyTypes.dxf_Integer

                Case dxxHeaderVars.CMLSTYLE
                    Return dxxPropertyTypes.dxf_String
                Case dxxHeaderVars.CMLJUST ' = 192
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.CMLSCALE ' = 193
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.CELWEIGHT ' = 196
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.ENDCAPS ' = 197
                    Return dxxPropertyTypes.dxf_Integer

                Case dxxHeaderVars.JOINSTYLE ' = 198
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.INSUNITS ' = 200
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.XEDIT, dxxHeaderVars.PSTYLEMODE, dxxHeaderVars.EXTNAMES, dxxHeaderVars.HIDETEXT
                    Return dxxPropertyTypes.Switch
                Case dxxHeaderVars.CEPSNID ' = 132
                    Return dxxPropertyTypes.Pointer
                Case dxxHeaderVars.CEPSNTYPE ' = 204
                    Return dxxPropertyTypes.dxf_Integer

                Case dxxHeaderVars.FINGERPRINTGUID
                    Return dxxPropertyTypes.dxf_String
                Case dxxHeaderVars.VERSIONGUID
                    Return dxxPropertyTypes.dxf_String
                Case dxxHeaderVars.PSVPSCALE, dxxHeaderVars.NORTHDIRECTION
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.OLESTARTUP, dxxHeaderVars.INTERSECTIONDISPLAY, dxxHeaderVars.CAMERADISPLAY
                    Return dxxPropertyTypes.Switch
                Case dxxHeaderVars.SORTENTS ' = 211
                    Return dxxPropertyTypes.dxf_Integer

                Case dxxHeaderVars.INDEXCTL ' = 212
                    Return dxxPropertyTypes.dxf_Integer

                Case dxxHeaderVars.XCLIPFRAME ' = 214
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.HALOGAP ' = 215
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.OBSCOLOR, dxxHeaderVars.INTERSECTIONCOLOR
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.OBSLTYPE ' = 217
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.INTERSECTIONDISPLAY
                    Return dxxPropertyTypes.Switch
                Case dxxHeaderVars.SOLIDHIST
                    Return dxxPropertyTypes.Switch
                Case dxxHeaderVars.DIMASSOC ' = 220
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.LENSLENGTH ' = 223
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.CAMERAHEIGHT ' = 224
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.STEPSPERSEC ' = 225
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.STEPSIZE ' = 226
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.THREEDDWFPREC ' = 227
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.PSOLWIDTH ' = 228
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.PSOLHEIGHT ' = 229
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.LOFTANG1, dxxHeaderVars.LOFTANG2
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.LOFTMAG1, dxxHeaderVars.LOFTMAG2
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.LOFTPARAM ' = 234
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.LOFTNORMALS ' = 235
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.LATITUDE
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.LONGITUDE
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.TIMEZONE ' = 239
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.LIGHTGLYPHDISPLAY
                    Return dxxPropertyTypes.Switch

                Case dxxHeaderVars.TILEMODELIGHTSYNCH
                    Return dxxPropertyTypes.Switch
                Case dxxHeaderVars.CMATERIAL ' = 242
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.SHOWHIST
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.DWFFRAME ' = 245
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.DGNFRAME ' = 246
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.REALWORLDSCALE ' = 247
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.INTERFERECOLOR ' = 248
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.INTERFEREOBJVS ' = 249
                    Return dxxPropertyTypes.Pointer
                Case dxxHeaderVars.INTERFEREVPVS ' = 250
                    Return dxxPropertyTypes.Pointer
                Case dxxHeaderVars.CSHADOW ' = 251
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.SHADOWPLANELOCATION ' = 252
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.UCSMODE '= -1
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.UCSSIZE ' = (-1 * 2)
                    Return dxxPropertyTypes.dxf_Double
                Case dxxHeaderVars.UCSCOLOR ' = (-1 * 3)
                    Return dxxPropertyTypes.Color
                Case dxxHeaderVars.LWDEFAULT ' = (-1 * 4)
                    Return dxxPropertyTypes.dxf_Integer
                Case dxxHeaderVars.LWSCALE ' = (-1 * 5)
                    Return dxxPropertyTypes.dxf_Double
                Case Else
                    Return dxxPropertyTypes.dxf_String
            End Select
        End Function

        <Extension()>
        Function PropertyName(ByVal aDimStyleProperty As dxxDimStyleProperties) As String
            Return dxfEnums.PropertyName(aDimStyleProperty)
        End Function

        <Extension()>
        Function PropertyName(ByVal aHeaderVar As dxxHeaderVars) As String
            Return dxfEnums.PropertyName(aHeaderVar)
        End Function
        <Extension()>
        Function PropertyType(ByVal aDimStyleProperty As dxxDimStyleProperties) As dxxPropertyTypes
            Select Case aDimStyleProperty
                Case dxxDimStyleProperties.DIMPOST, dxxDimStyleProperties.DIMAPOST, dxxDimStyleProperties.DIMASUFFIX
                    Return dxxPropertyTypes.dxf_String

                Case dxxDimStyleProperties.DIMSCALE, dxxDimStyleProperties.DIMASZ, dxxDimStyleProperties.DIMEXO, dxxDimStyleProperties.DIMDLI, dxxDimStyleProperties.DIMEXE, dxxDimStyleProperties.DIMRND
                    Return dxxPropertyTypes.dxf_Single

                Case dxxDimStyleProperties.DIMDLE, dxxDimStyleProperties.DIMTP, dxxDimStyleProperties.DIMTM, dxxDimStyleProperties.DIMFXL, dxxDimStyleProperties.DIMTFILL, dxxDimStyleProperties.DIMTP
                    Return dxxPropertyTypes.dxf_Single

                Case dxxDimStyleProperties.DIMJOGANG
                    Return dxxPropertyTypes.dxf_Double


                Case dxxDimStyleProperties.DIMTOL, dxxDimStyleProperties.DIMLIM, dxxDimStyleProperties.DIMTIH, dxxDimStyleProperties.DIMTOH, dxxDimStyleProperties.DIMSE1, dxxDimStyleProperties.DIMSE2, dxxDimStyleProperties.DIMLIM
                    Return dxxPropertyTypes.Switch

                Case dxxDimStyleProperties.DIMTAD, dxxDimStyleProperties.DIMZIN, dxxDimStyleProperties.DIMAZIN, dxxDimStyleProperties.DIMARCSYM
                    Return dxxPropertyTypes.dxf_Integer

                Case dxxDimStyleProperties.DIMTXT, dxxDimStyleProperties.DIMCEN, dxxDimStyleProperties.DIMTSZ, dxxDimStyleProperties.DIMALTF, dxxDimStyleProperties.DIMLFAC, dxxDimStyleProperties.DIMTVP, dxxDimStyleProperties.DIMTFAC
                    Return dxxPropertyTypes.dxf_Single

                Case dxxDimStyleProperties.DIMGAP, dxxDimStyleProperties.DIMALTRND, dxxDimStyleProperties.DIMALT, dxxDimStyleProperties.DIMALTD
                    Return dxxPropertyTypes.dxf_Single

                Case dxxDimStyleProperties.DIMTOFL, dxxDimStyleProperties.DIMSAH, dxxDimStyleProperties.DIMTIX, dxxDimStyleProperties.DIMSOXD, dxxDimStyleProperties.DIMSD1, dxxDimStyleProperties.DIMSD2, dxxDimStyleProperties.DIMUPT, dxxDimStyleProperties.DIMFXLON
                    Return dxxPropertyTypes.Switch

                Case dxxDimStyleProperties.DIMCLRD, dxxDimStyleProperties.DIMCLRE, dxxDimStyleProperties.DIMCLRT, dxxDimStyleProperties.DIMTFILLCLR
                    Return dxxPropertyTypes.Color
                Case dxxDimStyleProperties.DIMADEC, dxxDimStyleProperties.DIMDEC, dxxDimStyleProperties.DIMTDEC, dxxDimStyleProperties.DIMALTU, dxxDimStyleProperties.DIMALTTD
                    Return dxxPropertyTypes.dxf_Integer

                Case dxxDimStyleProperties.DIMAUNIT, dxxDimStyleProperties.DIMFRAC, dxxDimStyleProperties.DIMUNIT, dxxDimStyleProperties.DIMLUNIT, dxxDimStyleProperties.DIMDSEP, dxxDimStyleProperties.DIMTMOVE, dxxDimStyleProperties.DIMJUST
                    Return dxxPropertyTypes.dxf_Integer

                Case dxxDimStyleProperties.DIMTOLJ, dxxDimStyleProperties.DIMTZIN, dxxDimStyleProperties.DIMALTZ, dxxDimStyleProperties.DIMALTTZ, dxxDimStyleProperties.DIMFIT, dxxDimStyleProperties.DIMATFIT, dxxDimStyleProperties.DIMTZIN
                    Return dxxPropertyTypes.dxf_Integer

                Case dxxDimStyleProperties.DIMTXSTY, dxxDimStyleProperties.DIMLDRBLK, dxxDimStyleProperties.DIMBLK, dxxDimStyleProperties.DIMBLK1, dxxDimStyleProperties.DIMBLK2, dxxDimStyleProperties.DIMLTYPE, dxxDimStyleProperties.DIMLTEX1, dxxDimStyleProperties.DIMLTEX2
                    Return dxxPropertyTypes.Pointer

                Case dxxDimStyleProperties.DIMLWD, dxxDimStyleProperties.DIMLWE, dxxDimStyleProperties.DIMZEROSUPPRESSION, dxxDimStyleProperties.DIMTOLZEROSUPPRESSION, dxxDimStyleProperties.DIMZEROSUPPRESSION_ARCH
                    Return dxxPropertyTypes.dxf_Integer

                Case dxxDimStyleProperties.DIMPREFIX, dxxDimStyleProperties.DIMSUFFIX, dxxDimStyleProperties.DIMAPREFIX, dxxDimStyleProperties.DIMSUFFIX
                    Return dxxPropertyTypes.dxf_String
                Case dxxDimStyleProperties.DIMTANGLE
                    Return dxxPropertyTypes.Angle
                Case dxxDimStyleProperties.DIMTXSTY_NAME, dxxDimStyleProperties.DIMLDRBLK_NAME, dxxDimStyleProperties.DIMBLK_NAME, dxxDimStyleProperties.DIMBLK1_NAME, dxxDimStyleProperties.DIMBLK2_NAME, dxxDimStyleProperties.DIMLTYPE_NAME, dxxDimStyleProperties.DIMLTEX1_NAME, dxxDimStyleProperties.DIMLTEX2_NAME
                    Return dxxPropertyTypes.dxf_String
                Case Else
                    Return dxxPropertyTypes.dxf_String
            End Select

        End Function


        <Extension()>
        Function ReferenceType(ByVal aSettingType As dxxSettingTypes) As dxxReferenceTypes

            Select Case aSettingType
                Case dxxSettingTypes.DIMSETTINGS
                    Return dxxReferenceTypes.DIMSETTINGS
                Case dxxSettingTypes.TABLESETTINGS
                    Return dxxReferenceTypes.TABLESETTINGS
                Case dxxSettingTypes.SYMBOLSETTINGS
                    Return dxxReferenceTypes.SYMBOLSETTINGS
                Case dxxSettingTypes.HEADER
                    Return dxxReferenceTypes.HEADER
                Case dxxSettingTypes.DIMOVERRIDES
                    Return dxxReferenceTypes.DIMOVERRIDES

                Case dxxSettingTypes.TEXTSETTINGS
                    Return dxxReferenceTypes.TEXTSETTINGS
                Case dxxSettingTypes.LINETYPESETTINGS
                    Return dxxReferenceTypes.LINETYPESETTINGS
                Case dxxSettingTypes.SCREENSETTINGS
                    Return dxxReferenceTypes.SCREENSETTINGS
                Case dxxSettingTypes.DISPLAYSETTINGS
                    Return dxxReferenceTypes.DISPLAYSETTINGS


                Case Else
                    Return dxxReferenceTypes.UNDEFINED
            End Select
        End Function


        <Extension()>
        Function DefaultMemberName(ByVal aRefType As dxxReferenceTypes) As String
            Select Case aRefType
                Case dxxReferenceTypes.APPID
                    Return String.Empty
                Case dxxReferenceTypes.VPORT
                    Return "*Active"
                Case dxxReferenceTypes.BLOCK_RECORD
                    Return String.Empty
                Case dxxReferenceTypes.LTYPE
                    Return dxfLinetypes.Continuous
                Case dxxReferenceTypes.LAYER
                    Return "0"

                Case dxxReferenceTypes.STYLE
                    Return "Standard"
                Case dxxReferenceTypes.DIMSTYLE
                    Return "Standard"
                Case dxxReferenceTypes.UCS
                    Return "World"
                Case dxxReferenceTypes.VIEW
                    Return "Top"


                Case Else
                    Return String.Empty
            End Select
        End Function
    End Module


End Namespace