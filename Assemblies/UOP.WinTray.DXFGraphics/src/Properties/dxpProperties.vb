Imports SharpDX.Direct2D1.Effects
Imports UOP.DXFGraphics.dxfGlobals
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxpProperties
#Region "Members"
        Private _NewProp As TPROPERTY
#End Region 'Members

#Region "Public Properties"

        Public Shared ReadOnly Property LineWeightNames As dxoProperties
            Get
                Return New dxoProperties(Props_LineWeightNames)
            End Get
        End Property

        Public Shared ReadOnly Property LineWeightValues As dxoProperties
            Get
                Return New dxoProperties(Props_LineWeightValues)
            End Get
        End Property
        Public Shared ReadOnly Property Block As dxoProperties
            Get
                Return New dxoProperties(Props_Block)
            End Get
        End Property

        Public Shared ReadOnly Property CommonEntity As dxoProperties
            Get
                Return New dxoProperties(Props_Block)
            End Get
        End Property

        Public Shared ReadOnly Property ThreeDFace As dxoProperties
            Get
                Return New dxoProperties(Props_3DFACE)
            End Get
        End Property
        Public Shared ReadOnly Property ThreeDSolid As dxoProperties
            Get
                Return New dxoProperties(Props_3DSOLID)
            End Get
        End Property
        Public Shared ReadOnly Property ProxyEntity As dxoProperties
            Get
                Return New dxoProperties(Props_ACAD_PROXY_ENTITY)
            End Get
        End Property
        Public Shared ReadOnly Property Arc As dxoProperties
            Get
                Return New dxoProperties(Props_ARC)
            End Get
        End Property
        Public Shared ReadOnly Property Attdef As dxoProperties
            Get
                Return New dxoProperties(Props_ATTDEF)
            End Get
        End Property
        Public Shared ReadOnly Property Attrib As dxoProperties
            Get
                Return New dxoProperties(Props_ATTRIB)
            End Get
        End Property
        Public Shared ReadOnly Property Bezier As dxoProperties
            Get
                Return New dxoProperties(Props_BEZIER)
            End Get
        End Property
        Public Shared ReadOnly Property Body As dxoProperties
            Get
                Return New dxoProperties(Props_BODY)
            End Get
        End Property
        Public Shared ReadOnly Property Dimension As dxoProperties
            Get
                Return New dxoProperties(Props_DIMENSION)
            End Get
        End Property
        Public Shared ReadOnly Property Ellipse As dxoProperties
            Get
                Return New dxoProperties(Props_ELLIPSE)
            End Get
        End Property
        Public Shared ReadOnly Property EndBlk As dxoProperties
            Get
                Return New dxoProperties(Props_ENDBLK)
            End Get
        End Property
        Public Shared ReadOnly Property Hatch As dxoProperties
            Get
                Return New dxoProperties(Props_HATCH)
            End Get
        End Property

        Public Shared ReadOnly Property Hole As dxoProperties
            Get
                Return New dxoProperties(Props_HOLE)
            End Get
        End Property
        Public Shared ReadOnly Property Insert As dxoProperties
            Get
                Return New dxoProperties(Props_INSERT)
            End Get
        End Property
        Public Shared ReadOnly Property Leader As dxoProperties
            Get
                Return New dxoProperties(Props_LEADER)
            End Get
        End Property

        Public Shared ReadOnly Property Line As dxoProperties
            Get
                Return New dxoProperties(Props_LINE)
            End Get
        End Property
        Public Shared ReadOnly Property Point As dxoProperties
            Get
                Return New dxoProperties(Props_POINT)
            End Get
        End Property
        Public Shared ReadOnly Property LWPolyline As dxoProperties
            Get
                Return New dxoProperties(Props_LWPOLYLINE)
            End Get
        End Property
        Public Shared ReadOnly Property Polygon As dxoProperties
            Get
                Return New dxoProperties(Props_POLYGON)
            End Get
        End Property
        Public Shared ReadOnly Property Region As dxoProperties
            Get
                Return New dxoProperties(Props_REGION)
            End Get
        End Property
        Public Shared ReadOnly Property SeqEnd As dxoProperties
            Get
                Return New dxoProperties(Props_SEQEND)
            End Get
        End Property
        Public Shared ReadOnly Property Shape As dxoProperties
            Get
                Return New dxoProperties(Props_SHAPE)
            End Get
        End Property
        Public Shared ReadOnly Property Solid As dxoProperties
            Get
                Return New dxoProperties(Props_SOLID)
            End Get
        End Property
        Public Shared ReadOnly Property Symbol As dxoProperties
            Get
                Return New dxoProperties(Props_SYMBOL)
            End Get
        End Property
        Public Shared ReadOnly Property MText As dxoProperties
            Get
                Return New dxoProperties(Props_MTEXT)
            End Get
        End Property
        Public Shared ReadOnly Property Table As dxoProperties
            Get
                Return New dxoProperties(Props_TABLE)
            End Get
        End Property
        Public Shared ReadOnly Property Viewport As dxoProperties
            Get
                Return New dxoProperties(Props_VIEWPORT)
            End Get
        End Property
#End Region 'Public Properties

#Region "Internal Properties"
        Private Shared _LineWeightNames As TPROPERTIES

        Friend Shared ReadOnly Property Props_LineWeightNames As TPROPERTIES
            Get

                If _LineWeightNames.Count = 0 Then ' If _LineWeightNames.IsEmpty Then
                    _LineWeightNames = New TPROPERTIES("LINEWEIGHT_NAMES", True)
                    Dim enumVals As Dictionary(Of String, Integer) = dxfEnums.EnumValues(GetType(dxxLineWeights))
                    Dim iLwt As dxxLineWeights
                    For j As Integer = 1 To 2
                        For i As Integer = 1 To enumVals.Count
                            Try
                                iLwt = enumVals.ElementAt(i - 1).Value
                                If iLwt < 0 Then
                                    If j = 1 Then _LineWeightNames.Add(New TPROPERTY(0, dxfEnums.Description(iLwt), iLwt.ToString, dxxPropertyTypes.dxf_String, bNonDXF:=True))
                                Else
                                    If j <> 1 Then _LineWeightNames.Add(New TPROPERTY(0, dxfEnums.Description(iLwt), iLwt.ToString, dxxPropertyTypes.dxf_String, bNonDXF:=True))
                                End If
                            Catch ex As Exception
                                If dxfUtils.RunningInIDE Then
                                    Beep()
                                End If
                            End Try
                        Next i
                    Next j
                End If
                Return New TPROPERTIES(_LineWeightNames)
            End Get

        End Property


        Private Shared _LineWeightValues As TPROPERTIES
        Friend Shared ReadOnly Property Props_LineWeightValues As TPROPERTIES
            Get

                If _LineWeightValues.Count = 0 Then ' If _LineWeightValues.IsEmpty Then
                    _LineWeightValues = New TPROPERTIES("LINEWEIGHT_VALUES", True)
                    Dim eVals As Dictionary(Of String, Integer) = dxfEnums.EnumValues(GetType(dxxLineWeights))
                    Dim iVal As dxxLineWeights
                    For Each sStr In eVals.Keys
                        iVal = eVals.Item(sStr)
                        _LineWeightValues.Add(New TPROPERTY(0, iVal, sStr, dxxPropertyTypes.dxf_Integer, bNonDXF:=True))
                    Next
                End If
                Return New TPROPERTIES(_LineWeightValues)
            End Get

        End Property


        Private Shared _ExtendedData As TPROPERTIES
        Friend Shared ReadOnly Property ExtendedData As TPROPERTIES
            Get

                If _ExtendedData.Count = 0 Then ' If _LineWeightValues.IsEmpty Then
                    _ExtendedData = TPROPERTIES.FromString("1000=AnnotativeData,1002={,1070=1,1070=1,1002=}", "AcadAnnotative")


                End If
                Return New TPROPERTIES(_LineWeightValues)
            End Get

        End Property

        Private Shared _AnnotativeData As TPROPERTIES
        Friend Shared ReadOnly Property AnnotativeData As TPROPERTIES
            Get

                If _AnnotativeData.Count = 0 Then ' If _LineWeightValues.IsEmpty Then
                    _AnnotativeData = TPROPERTIES.FromString("1000=AnnotativeData,1002={,1070=1,1070=1,1002=}", "AcadAnnotative")


                End If
                Return _AnnotativeData
            End Get

        End Property

        Private Shared _DimStyleExtendedData As TPROPERTYARRAY
        Friend Shared ReadOnly Property DimStyleExtendedData As TPROPERTYARRAY
            Get

                If _DimStyleExtendedData.Count = 0 Then ' If _LineWeightValues.IsEmpty Then
                    _DimStyleExtendedData = New TPROPERTYARRAY("Extended Data")
                    _DimStyleExtendedData.Add(dxpProperties.AnnotativeData, "AcadAnnotative", True)
                    _DimStyleExtendedData.Add(TPROPERTIES.FromString("1070=388,1040=1.5", "ACAD_DSTYLE_DIMJAG"), "ACAD_DSTYLE_DIMJAG", True)
                    _DimStyleExtendedData.Add(TPROPERTIES.FromString("1070=392,1070=0", "ACAD_DSTYLE_DIMTALN"), "ACAD_DSTYLE_DIMTALN", True)



                End If
                Return _DimStyleExtendedData
            End Get

        End Property

        Private Shared _BlockProperties As TPROPERTIES

        Friend Shared ReadOnly Property Props_Block As TPROPERTIES
            Get
                If _BlockProperties.Count = 0 Then ' If _BlockProperties.IsEmpty Then
                    _BlockProperties = New TPROPERTIES("BLOCK")
                    _BlockProperties.Add(New TPROPERTY(0, "BLOCK", "Type Name", dxxPropertyTypes.dxf_String))
                    _BlockProperties.Add(New TPROPERTY(5, "", "Handle", dxxPropertyTypes.Handle))
                    _BlockProperties.Add(New TPROPERTY(330, "", "Block Record Handle", dxxPropertyTypes.Pointer))
                    _BlockProperties.Add(New TPROPERTY(100, "AcDbEntity", "Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _BlockProperties.Add(New TPROPERTY(67, 0, "Paper Space Flag", 0, dxxPropertyTypes.Switch))
                    _BlockProperties.Add(New TPROPERTY(8, "0", "Layer Name", dxxPropertyTypes.dxf_String))
                    _BlockProperties.Add(New TPROPERTY(100, "AcDbBlockBegin", "Block Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _BlockProperties.Add(New TPROPERTY(2, "", "Name", dxxPropertyTypes.dxf_String))
                    _BlockProperties.Add(New TPROPERTY(70, 0, "Block Type Flag", dxxPropertyTypes.dxf_Long))
                    _BlockProperties.AddVector(10, TVECTOR.Zero, "Base Point")
                    _BlockProperties.Add(New TPROPERTY(3, "", "Name_2", dxxPropertyTypes.dxf_String))
                    _BlockProperties.Add(New TPROPERTY(1, "", "X-Ref Path", dxxPropertyTypes.dxf_String))
                    _BlockProperties.Add(New TPROPERTY(4, "", "Description", dxxPropertyTypes.dxf_String))
                    _BlockProperties.Add(New TPROPERTY(-1, "", "*GUID", dxxPropertyTypes.dxf_String) With {.DoNotCopy = True})
                End If
                Return New TPROPERTIES(_BlockProperties)
            End Get
        End Property

        Private Shared _CommonHidden As Integer
        Private Shared _CommonEntProps As TPROPERTIES
        Friend Shared ReadOnly Property Props_CommonEnt As TPROPERTIES
            Get
                If _CommonEntProps.Count = 0 Then ' If _CommonEntProps.IsEmpty Then

                    _CommonEntProps = New TPROPERTIES("COMMON")
                    _CommonEntProps.Add(New TPROPERTY(0, "", "Entity Type", dxxPropertyTypes.dxf_String) With {.DoNotCopy = True})
                    _CommonEntProps.Add(New TPROPERTY(5, "0", "Handle", dxxPropertyTypes.Handle))
                    _CommonEntProps.Add(New TPROPERTY(330, "0", "Owner Handle", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _CommonEntProps.Add(New TPROPERTY(100, "AcDbEntity", "Entity Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _CommonEntProps.Add(New TPROPERTY(67, 0, "IsPaperSpace", 0, dxxPropertyTypes.Switch))
                    _CommonEntProps.Add(New TPROPERTY(410, "", "Layout Tab Name", dxxPropertyTypes.dxf_String, bSetSuppressedValue:=True))
                    _CommonEntProps.Add(New TPROPERTY(8, "0", "LayerName", dxxPropertyTypes.dxf_String))
                    _CommonEntProps.Add(New TPROPERTY(6, dxfLinetypes.ByLayer, "LineType", dxxPropertyTypes.dxf_String, bSetSuppressedValue:=True))
                    _CommonEntProps.Add(New TPROPERTY(347, "0", "Material Handle", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _CommonEntProps.Add(New TPROPERTY(62, dxxColors.ByLayer, "Color", dxxPropertyTypes.Color, bSetSuppressedValue:=True))
                    _CommonEntProps.Add(New TPROPERTY(48, 1.0!, "LT Scale", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, bSetSuppressedValue:=True))
                    _CommonEntProps.Add(New TPROPERTY(60, 0, dxfLinetypes.Invisible, dxxPropertyTypes.Switch, bSetSuppressedValue:=True))
                    _CommonEntProps.Add(New TPROPERTY(420, 0, "Color Long Value", dxxPropertyTypes.dxf_Integer, bSetSuppressedValue:=True))
                    _CommonEntProps.Add(New TPROPERTY(430, "", "Color Name", dxxPropertyTypes.dxf_String, bSetSuppressedValue:=True))
                    _CommonEntProps.Add(New TPROPERTY(440, -1, "Transparency Value", dxxPropertyTypes.dxf_Integer, bSetSuppressedValue:=True))
                    _CommonEntProps.Add(New TPROPERTY(390, "0", "Plot Style Object Handle", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _CommonEntProps.Add(New TPROPERTY(284, dxxShadowModes.CastReceives, "Shadow Mode", dxxPropertyTypes.dxf_Integer, bSetSuppressedValue:=True))

                    'hidden properties follow
                    _CommonEntProps.Add(New TPROPERTY(1, "", "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True})
                    _CommonEntProps.Add(New TPROPERTY(2, dxxGraphicTypes.Undefined, "*GraphicType", dxxPropertyTypes.dxf_Integer, bNonDXF:=True) With {.DoNotCopy = True})
                    _CommonEntProps.Add(New TPROPERTY(3, dxxEntityTypes.Undefined, "*EntityType", dxxPropertyTypes.dxf_Integer, bNonDXF:=True) With {.DoNotCopy = True})
                    _CommonEntProps.Add(New TPROPERTY(4, True, "*SaveToFile", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _CommonEntProps.Add(New TPROPERTY(5, "", "*Identifier", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True})
                    _CommonEntProps.Add(New TPROPERTY(6, "", "*GroupName", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True})
                    _CommonEntProps.Add(New TPROPERTY(7, "", "*Name", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    _CommonEntProps.Add(New TPROPERTY(8, "", "*URL", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    _CommonEntProps.Add(New TPROPERTY(9, dxxDrawingDomains.Model, "*Domain", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))
                    _CommonEntProps.Add(New TPROPERTY(10, "", "*SourceGUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True})
                    _CommonEntProps.Add(New TPROPERTY(11, False, "*Boundless", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _CommonEntProps.Add(New TPROPERTY(12, "0", "*ReactorHandle", dxxPropertyTypes.Pointer, bNonDXF:=True))
                    _CommonHidden = _CommonEntProps.HiddenMemberCount
                End If
                Return New TPROPERTIES(_CommonEntProps)
            End Get
        End Property

        Private Shared _3DFACE As TPROPERTIES
        Friend Shared ReadOnly Property Props_3DFACE As TPROPERTIES
            Get
                If _3DFACE.Count = 0 Then
                    _3DFACE = Get_CommonProps("3DFACE")
                    _3DFACE.Add(New TPROPERTY(100, "AcDbFace", "Entity Sub Class Marker_1", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _3DFACE.AddVector(10, TVECTOR.Zero, "Vertex1")
                    _3DFACE.AddVector(11, TVECTOR.Zero, "Vertex2")
                    _3DFACE.AddVector(12, TVECTOR.Zero, "Vertex3")
                    _3DFACE.AddVector(13, TVECTOR.Zero, "Vertex4")
                    _3DFACE.Add(New TPROPERTY(70, 0, "Invisble Edge Flag", dxxPropertyTypes.Switch, bSetSuppressedValue:=True))
                End If
                Return New TPROPERTIES(_3DFACE)
            End Get

        End Property
        Private Shared _3DSOLID As TPROPERTIES
        Private Shared ReadOnly Property Props_3DSOLID As TPROPERTIES
            Get


                If _3DSOLID.Count = 0 Then
                    _3DSOLID = Get_CommonProps("3DSOLID")
                    _3DSOLID.Add(New TPROPERTY(100, "AcDbModelerGeometry", "Entity Sub Class Marker_1", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _3DSOLID.Add(New TPROPERTY(70, 1, "Modeler format version number", dxxPropertyTypes.dxf_Integer))
                    _3DSOLID.Add(New TPROPERTY(1, "", "Proprietary Data", dxxPropertyTypes.dxf_String))
                    _3DSOLID.Add(New TPROPERTY(3, "", "Proprietary Data(additional) Then", "", dxxPropertyTypes.dxf_String, bSetSuppressedValue:=True))
                    _3DSOLID.Add(New TPROPERTY(100, "AcDb3dSolid", "Entity Sub Class Marker_2", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _3DSOLID.Add(New TPROPERTY(350, "0", "Owner Handle", "0", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                End If
                Return New TPROPERTIES(_3DSOLID)
            End Get
        End Property

        Private Shared _ACAD_PROXY_ENTITY As TPROPERTIES
        Friend Shared ReadOnly Property Props_ACAD_PROXY_ENTITY As TPROPERTIES
            Get
                If _ACAD_PROXY_ENTITY.Count = 0 Then
                    _ACAD_PROXY_ENTITY = Get_CommonProps("ACAD_PROXY_ENTITY")
                    _ACAD_PROXY_ENTITY.Add(New TPROPERTY(100, "AcDbProxyEntity", "Entity Sub Class Marker_1", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _ACAD_PROXY_ENTITY.Add(New TPROPERTY(90, 498, "Proxy Entity Class ID", dxxPropertyTypes.Undefined))
                    _ACAD_PROXY_ENTITY.Add(New TPROPERTY(91, 0, "Entity Class ID", dxxPropertyTypes.Undefined))
                    _ACAD_PROXY_ENTITY.Add(New TPROPERTY(92, 0, "Size Of graphics data In bytes", dxxPropertyTypes.Undefined))
                    _ACAD_PROXY_ENTITY.Add(New TPROPERTY(310, "", "Binary Graphic Data", "", dxxPropertyTypes.Undefined))
                    _ACAD_PROXY_ENTITY.Add(New TPROPERTY(93, 0, "Size Of entity data In bits", dxxPropertyTypes.Undefined))
                    _ACAD_PROXY_ENTITY.Add(New TPROPERTY(310, "", "Binary Entity Data", "", dxxPropertyTypes.Undefined))
                    _ACAD_PROXY_ENTITY.Add(New TPROPERTY(330, "0", "Object ID1", "0", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _ACAD_PROXY_ENTITY.Add(New TPROPERTY(340, "0", "Object ID2", "0", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _ACAD_PROXY_ENTITY.Add(New TPROPERTY(350, "0", "Object ID3", "0", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _ACAD_PROXY_ENTITY.Add(New TPROPERTY(360, "0", "Object ID4", "0", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _ACAD_PROXY_ENTITY.Add(New TPROPERTY(94, 0, "indicates End Of Object ID section", dxxPropertyTypes.Undefined))
                    _ACAD_PROXY_ENTITY.Add(New TPROPERTY(95, 0, "Object drawing format When it becomes a proxy", dxxPropertyTypes.Undefined))
                    _ACAD_PROXY_ENTITY.Add(New TPROPERTY(70, 0, "Original custom Object data format", dxxPropertyTypes.Undefined))
                End If
                Return New TPROPERTIES(_ACAD_PROXY_ENTITY)
            End Get
        End Property
        Private Shared _ARC As TPROPERTIES
        Friend Shared ReadOnly Property Props_ARC As TPROPERTIES
            Get
                If _ARC.Count = 0 Then
                    Dim hidx As Integer = 0
                    _ARC = Get_CommonProps("ARC", hidx)
                    _ARC.Add(New TPROPERTY(100, "AcDbCircle", "Entity Sub Class Marker_1", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _ARC.Add(New TPROPERTY(39, 0, "Thickness", 0, dxxPropertyTypes.dxf_Double, bScalable:=True))
                    _ARC.AddVector(10, TVECTOR.Zero, "Center")
                    _ARC.Add(New TPROPERTY(40, 1, "Radius", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, bScalable:=True))
                    _ARC.AddVector(210, TVECTOR.WorldZ, "Extrusion Direction", bIsDirection:=True)
                    _ARC.Add(New TPROPERTY(100, "AcDbArc", "Entity Sub Class Marker_2", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _ARC.Add(New TPROPERTY(50, 0, "Start Angle", dxxPropertyTypes.Angle))
                    _ARC.Add(New TPROPERTY(51, 360, "End Angle", dxxPropertyTypes.Angle))
                    _ARC.Add(New TPROPERTY(hidx + 1, 0, "*Clockwise", dxxPropertyTypes.Switch, bNonDXF:=True))
                    _ARC.Add(New TPROPERTY(hidx + 2, 0!, "*StartWidth", dxxPropertyTypes.dxf_Single, bScalable:=True, bNonDXF:=True))
                    _ARC.Add(New TPROPERTY(hidx + 3, 0!, "*EndWidth", dxxPropertyTypes.dxf_Single, bScalable:=True, bNonDXF:=True))
                    _ARC.ReIndex(True)
                End If
                Return New TPROPERTIES(_ARC)
            End Get
        End Property

        Private Shared _ATTDEF As TPROPERTIES
        Friend Shared ReadOnly Property Props_ATTDEF As TPROPERTIES
            Get
                If _ATTDEF.Count = 0 Then ' If _ATTDEF.IsEmpty Then
                    _ATTDEF = Get_TextProperties("ATTDEF")

                    _ATTDEF.SetSuppressionByGC("3,2,70,280", False)
                    _ATTDEF.SetValueGC(100, "AcDbAttributeDefinition", 1)
                    _ATTDEF.SetValueGC(100, "AcDbAttributeDefinition", 2)
                    _ATTDEF.SetVal("*EntityType", dxxEntityTypes.Attdef)

                    _ATTDEF.ReIndex(True)
                End If
                Return New TPROPERTIES(_ATTDEF)
            End Get

        End Property
        Private Shared _ATTRIB As TPROPERTIES

        Friend Shared ReadOnly Property Props_ATTRIB As TPROPERTIES
            Get
                If _ATTRIB.Count = 0 Then
                    _ATTRIB = Get_TextProperties("ATTRIB")

                    _ATTRIB.SetSuppressionByGC("3,2,70,280", False)
                    _ATTRIB.SetValueGC(100, "AcDbAttribute", 1)
                    _ATTRIB.SetValueGC(100, "AcDbAttribute", 2)
                    _ATTRIB.SetVal("*EntityType", dxxEntityTypes.Attribute)

                    _ATTRIB.ReIndex(True)
                End If
                Return New TPROPERTIES(_ATTRIB)
            End Get
        End Property
        Private Shared _DTEXT As TPROPERTIES
        Friend Shared ReadOnly Property Props_DTEXT As TPROPERTIES
            Get
                If _DTEXT.Count = 0 Then
                    Dim hidx As Integer = 0
                    _DTEXT = Get_CommonProps("TEXT", hidx)
                    _DTEXT.Add(New TPROPERTY(100, "AcDbText", "Entity Sub Class Marker_1", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _DTEXT.Add(New TPROPERTY(39, "Thickness", 0, dxxPropertyTypes.dxf_Double, bScalable:=True, bSetSuppressedValue:=True))
                    _DTEXT.AddVector(10, TVECTOR.Zero, "First alignment point(In OCS)")
                    _DTEXT.Add(New TPROPERTY(40, 0, "Text Height", dxxPropertyTypes.dxf_Double, bScalable:=True, aValControl:=mzValueControls.Positive))
                    _DTEXT.Add(New TPROPERTY(1, "", "Text String", dxxPropertyTypes.dxf_String))
                    _DTEXT.Add(New TPROPERTY(50, 0, "Rotation", dxxPropertyTypes.Angle, bSetSuppressedValue:=True))
                    _DTEXT.Add(New TPROPERTY(41, 1, "Width Factor", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, bSetSuppressedValue:=True) With {.Max = 100, .Min = 0.01})

                    _DTEXT.Add(New TPROPERTY(51, 0, "Oblique Angle", 0, dxxPropertyTypes.Angle, bSetSuppressedValue:=True))
                    _DTEXT.Add(New TPROPERTY(7, "Standard", "Text Style Name", dxxPropertyTypes.dxf_String, bSetSuppressedValue:=True))
                    _DTEXT.Add(New TPROPERTY(71, 0, "Text generation flags", dxxPropertyTypes.BitCode, aDecodeString:="2=Backwards,4=Upsidedown,6=Backwards & Upsidedown", bSetSuppressedValue:=True))

                    _DTEXT.Add(New TPROPERTY(72, dxxTextJustificationsHorizontal.Left, "Horizontal text justification type", dxxPropertyTypes.dxf_Integer, bSetSuppressedValue:=True))
                    _DTEXT.AddVector(11, TVECTOR.Zero, "Second alignment point(In OCS)")
                    _DTEXT.AddVector(210, TVECTOR.WorldZ, "Extrusion Direction", bIsDirection:=True)
                    _DTEXT.Add(New TPROPERTY(100, "AcDbText", "Entity Sub Class Marker_2", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _DTEXT.Add(New TPROPERTY(3, "", "Attribute Prompt", dxxPropertyTypes.dxf_String, bSetSuppressedValue:=True))
                    _DTEXT.Add(New TPROPERTY(2, "", "Attribute Tag", dxxPropertyTypes.dxf_String, bSetSuppressedValue:=True))
                    _DTEXT.Add(New TPROPERTY(70, 0, "Attribute Flags", dxxPropertyTypes.dxf_Integer, bSetSuppressedValue:=True))
                    _DTEXT.Add(New TPROPERTY(73, 0, "Field length(Not currently used)", dxxPropertyTypes.dxf_Double, bSetSuppressedValue:=True))
                    _DTEXT.Add(New TPROPERTY(74, dxxTextJustificationsVertical.Baseline, "Vertical text justification type", dxxPropertyTypes.dxf_Integer, bSetSuppressedValue:=True))
                    _DTEXT.Add(New TPROPERTY(280, True, "Lock position flag", dxxPropertyTypes.Switch, bSetSuppressedValue:=True))
                    _DTEXT.Add(New TPROPERTY(hidx + 1, dxxTextDrawingDirections.ByStyle, "*DrawingDirection", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))
                    _DTEXT.Add(New TPROPERTY(hidx + 2, 1.0!, "*Line Spacing Factor", dxxPropertyTypes.dxf_Double, bNonDXF:=True))
                    _DTEXT.Add(New TPROPERTY(hidx + 3, dxxLineSpacingStyles.AtLeast, "*Line Spacing Style", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))
                    _DTEXT.Add(New TPROPERTY(hidx + 4, "0", "*InsertHandle", dxxPropertyTypes.Pointer, bNonDXF:=True))
                    _DTEXT.Add(New TPROPERTY(hidx + 5, False, "*MultiAttribute", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _DTEXT.Add(New TPROPERTY(hidx + 6, 1, "*SourceCount", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))
                    _DTEXT.Add(New TPROPERTY(hidx + 7, "", "*SourceString", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    _DTEXT.Add(New TPROPERTY(hidx + 8, False, "*Constant", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _DTEXT.Add(New TPROPERTY(hidx + 9, False, "*Invisible", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _DTEXT.Add(New TPROPERTY(hidx + 10, False, "*Preset", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _DTEXT.Add(New TPROPERTY(hidx + 11, False, "*Verify", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _DTEXT.Add(New TPROPERTY(hidx + 12, 1, "*FitFactor", dxxPropertyTypes.dxf_Double, bNonDXF:=True))
                    _DTEXT.Add(New TPROPERTY(hidx + 13, 1, "*LineNo", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))
                    _DTEXT.Add(New TPROPERTY(hidx + 14, False, "*Backwards", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _DTEXT.Add(New TPROPERTY(hidx + 15, False, "*UpsideDown", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _DTEXT.Add(New TPROPERTY(hidx + 16, False, "*Vertical", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _DTEXT.Add(New TPROPERTY(hidx + 17, False, "*IsDimensionText", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _DTEXT.Add(New TPROPERTY(hidx + 18, 0, "*DimensionTextAngle", dxxPropertyTypes.Angle, bNonDXF:=True))
                    _DTEXT.ReIndex(True)
                End If

                Return New TPROPERTIES(_DTEXT)
            End Get
        End Property


        Private Shared _BEZIER As TPROPERTIES
        Friend Shared ReadOnly Property Props_BEZIER As TPROPERTIES
            Get
                If _BEZIER.Count = 0 Then
                    _BEZIER = Get_CommonProps("SPLINE")
                    _BEZIER.Add(New TPROPERTY(100, "AcDbSpline", "Entity Sub Class Marker_1", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _BEZIER.AddVector(210, TVECTOR.WorldZ, "Extrusion Direction", bIsDirection:=True)
                    _BEZIER.Add(New TPROPERTY(70, 8, "Spline Flag", dxxPropertyTypes.dxf_Integer)) ' 8 = planar
                    _BEZIER.Add(New TPROPERTY(71, 3, "Degree Of splice curve", dxxPropertyTypes.dxf_Integer)) '3 for bezier
                    _BEZIER.Add(New TPROPERTY(72, 8, "Knot count", dxxPropertyTypes.dxf_Integer))
                    _BEZIER.Add(New TPROPERTY(73, 4, "Control Pt Count", dxxPropertyTypes.dxf_Integer))
                    _BEZIER.Add(New TPROPERTY(74, 0, "Fit Pt Count", dxxPropertyTypes.dxf_Integer))
                    _BEZIER.Add(New TPROPERTY(42, 0.0000001, "Knot Tolerance", dxxPropertyTypes.dxf_Double))
                    _BEZIER.Add(New TPROPERTY(43, 0.0000001, "Control Pt Tolerance", dxxPropertyTypes.dxf_Double))
                    _BEZIER.Add(New TPROPERTY(44, 0.00000000001, "Fit Pt Tolerance", dxxPropertyTypes.dxf_Double))
                    _BEZIER.AddVector(12, TVECTOR.Zero, "Start Tangent Pt", bSuppressed:=True)
                    _BEZIER.AddVector(13, TVECTOR.Zero, "End Tangent Pt", bSuppressed:=True)
                    _BEZIER.Add(New TPROPERTY(40, 0, "Knot Value", dxxPropertyTypes.dxf_Double))
                    _BEZIER.Add(New TPROPERTY(40, 0, "Knot Value", dxxPropertyTypes.dxf_Double))
                    _BEZIER.Add(New TPROPERTY(40, 0, "Knot Value", dxxPropertyTypes.dxf_Double))
                    _BEZIER.Add(New TPROPERTY(40, 0, "Knot Value", dxxPropertyTypes.dxf_Double))
                    _BEZIER.Add(New TPROPERTY(40, 4, "Knot Value", dxxPropertyTypes.dxf_Double))
                    _BEZIER.Add(New TPROPERTY(40, 4, "Knot Value", dxxPropertyTypes.dxf_Double))
                    _BEZIER.Add(New TPROPERTY(40, 4, "Knot Value", dxxPropertyTypes.dxf_Double))
                    _BEZIER.Add(New TPROPERTY(40, 4, "Knot Value", dxxPropertyTypes.dxf_Double))
                    _BEZIER.AddVector(10, TVECTOR.Zero, "Control Pt1")
                    _BEZIER.AddVector(10, TVECTOR.Zero, "Control Pt2")
                    _BEZIER.AddVector(10, TVECTOR.Zero, "Control Pt3")
                    _BEZIER.AddVector(10, TVECTOR.Zero, "Control Pt4")
                    _BEZIER.ReIndex(True)
                End If
                Return New TPROPERTIES(_BEZIER)
            End Get
        End Property

        Private Shared _BODY As TPROPERTIES

        Private Shared ReadOnly Property Props_BODY As TPROPERTIES
            Get
                If _BODY.Count = 0 Then ' If _BODY.IsEmpty Then
                    _BODY = Get_CommonProps("BODY")
                    _BODY.Add(New TPROPERTY(100, "AcDbModelerGeometry", "Entity Sub Class Marker_1", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _BODY.Add(New TPROPERTY(70, 1, "Modeler format version number", dxxPropertyTypes.Undefined))
                    _BODY.Add(New TPROPERTY(1, "", "Proprietary Data", dxxPropertyTypes.dxf_String))
                    _BODY.Add(New TPROPERTY(3, "", "Proprietary Data(additional)", dxxPropertyTypes.dxf_String, bSetSuppressedValue:=True))
                    _BODY.ReIndex(True)
                End If
                Return New TPROPERTIES(_BODY)
            End Get
        End Property
        Private Shared _DIMENSION As TPROPERTIES
        Friend Shared ReadOnly Property Props_DIMENSION As TPROPERTIES
            Get

                If _DIMENSION.Count = 0 Then ' If _DIMENSION.IsEmpty Then
                    Dim hidx As Integer = 0
                    _DIMENSION = Get_CommonProps("DIMENSION", hidx)
                    _DIMENSION.Add(New TPROPERTY(100, "AcDbDimension", "Entity Sub Class Marker_1", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _DIMENSION.Add(New TPROPERTY(2, "", "Block Name", dxxPropertyTypes.dxf_String))
                    _DIMENSION.AddVector(10, TVECTOR.Zero, "Definition point(In WCS)")
                    _DIMENSION.AddVector(11, TVECTOR.Zero, "Middle point Of dimension text(In OCS)")
                    _DIMENSION.Add(New TPROPERTY(70, 0, "Dimension type Flag", dxxPropertyTypes.dxf_Integer))
                    _DIMENSION.Add(New TPROPERTY(71, 5, "Attachment point", dxxPropertyTypes.dxf_Integer))
                    _DIMENSION.Add(New TPROPERTY(72, dxxLineSpacingStyles.AtLeast, "Text Line Spacing style", dxxPropertyTypes.dxf_Integer, bSetSuppressedValue:=True))
                    _DIMENSION.Add(New TPROPERTY(41, 1, "Text Line Spacing factor", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, bSetSuppressedValue:=True))
                    _DIMENSION.Add(New TPROPERTY(42, 0, "Actual Measurement", dxxPropertyTypes.dxf_Double))
                    _DIMENSION.Add(New TPROPERTY(73, 0, "Flag 1", dxxPropertyTypes.Switch))
                    _DIMENSION.Add(New TPROPERTY(74, 0, "Flag 2", dxxPropertyTypes.Switch))
                    _DIMENSION.Add(New TPROPERTY(75, 0, "Flag 3", dxxPropertyTypes.Switch))
                    _DIMENSION.Add(New TPROPERTY(1, "", "Override Text", dxxPropertyTypes.dxf_String, bSetSuppressedValue:=True))
                    _DIMENSION.Add(New TPROPERTY(53, 0, "Text Rotation", dxxPropertyTypes.Angle, bSetSuppressedValue:=True))
                    _DIMENSION.Add(New TPROPERTY(51, 0, "Horizontal Direction Angle(OCS)", 0, dxxPropertyTypes.Angle))
                    _DIMENSION.AddVector(210, TVECTOR.WorldZ, "Extrusion Direction", bIsDirection:=True)
                    _DIMENSION.Add(New TPROPERTY(3, "Standard", "DimStyle Name", dxxPropertyTypes.dxf_String) With {.Key = "DimStyle"})
                    _DIMENSION.Add(New TPROPERTY(100, "AcDbAlignedDimension", "Entity Sub Class Marker_2", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _DIMENSION.AddVector(12, TVECTOR.Zero, "Insertion point For clones Of a dimension—Baseline And Continue(In OCS)")
                    _DIMENSION.AddVector(13, TVECTOR.Zero, "Definition point For linear And angular dimensions(In WCS)")
                    _DIMENSION.AddVector(14, TVECTOR.Zero, "Definition point For linear And angular dimensions(In WCS)")
                    _DIMENSION.AddVector(15, TVECTOR.Zero, "Definition point For diameter, radius, And angular dimensions(In WCS)")
                    _DIMENSION.AddVector(16, TVECTOR.Zero, "Point defining dimension arc For angular dimensions(In OCS)")
                    _DIMENSION.Add(New TPROPERTY(40, 0, "Leader length", dxxPropertyTypes.dxf_Double, bSetSuppressedValue:=True))
                    _DIMENSION.Add(New TPROPERTY(50, 0, "Angle Of rotated, horizontal, Or vertical dimensions", dxxPropertyTypes.Angle, bSetSuppressedValue:=True))
                    _DIMENSION.Add(New TPROPERTY(52, 0, "Dim Line Oblique Angle", dxxPropertyTypes.dxf_Double, bSetSuppressedValue:=True))

                    _DIMENSION.Add(New TPROPERTY(100, "", "Entity Sub Class Marker_3", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _DIMENSION.Add(New TPROPERTY(hidx + 1, dxxDimTypes.LinearHorizontal, "*DimType", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))
                    _DIMENSION.Add(New TPROPERTY(hidx + 2, 0!, "*TextOffset", dxxPropertyTypes.dxf_Double, bNonDXF:=True))
                    _DIMENSION.Add(New TPROPERTY(hidx + 3, 0!, "*TickOffset", dxxPropertyTypes.dxf_Double, True, bNonDXF:=True))
                    _DIMENSION.Add(New TPROPERTY(hidx + 4, False, "*UserPositionedText", dxxPropertyTypes.Switch, bNonDXF:=True))
                    _DIMENSION.Add(New TPROPERTY(hidx + 5, "", "*BlockName", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    _DIMENSION.AddVector(hidx + 6, TVECTOR.Zero, "*Vector1")
                    _DIMENSION.AddVector(hidx + 7, TVECTOR.Zero, "*Vector2")
                    _DIMENSION.ReIndex(True)
                End If
                Return New TPROPERTIES(_DIMENSION)
            End Get
        End Property

        Private Shared _ELLIPSE As TPROPERTIES
        Friend Shared ReadOnly Property Props_ELLIPSE As TPROPERTIES

            Get
                If _ELLIPSE.Count = 0 Then
                    Dim hidx As Integer = 0
                    _ELLIPSE = Get_CommonProps("ELLIPSE", hidx)
                    _ELLIPSE.Add(New TPROPERTY(100, "AcDbEllipse", "Entity Sub Class Marker_1", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _ELLIPSE.AddVector(10, TVECTOR.Zero, "Center")
                    _ELLIPSE.AddVector(11, TVECTOR.Zero, "Endpoint Of major axis, relative To the center(In WCS)")
                    _ELLIPSE.AddVector(210, TVECTOR.WorldZ, "Extrusion Direction", bIsDirection:=True)
                    _ELLIPSE.Add(New TPROPERTY(40, 0.5, "Ratio Of minor axis To major axis", dxxPropertyTypes.dxf_Double))
                    _ELLIPSE.Add(New TPROPERTY(41, 0, "Start parameter", dxxPropertyTypes.dxf_Double))  'this value is 0.0 for a full ellipse
                    _ELLIPSE.Add(New TPROPERTY(42, 2 * Math.PI, "End parameter", dxxPropertyTypes.dxf_Double)) 'this value is 2pi for a full ellipse

                    _ELLIPSE.Add(New TPROPERTY(hidx + 1, 0, "*StartAngle", dxxPropertyTypes.Angle, bNonDXF:=True))
                    _ELLIPSE.Add(New TPROPERTY(hidx + 2, 360, "*EndAngle", dxxPropertyTypes.dxf_Double, bNonDXF:=True))
                    _ELLIPSE.Add(New TPROPERTY(hidx + 3, 0.5!, "*MinorRadius", dxxPropertyTypes.dxf_Double, bNonDXF:=True))
                    _ELLIPSE.Add(New TPROPERTY(hidx + 4, 1.0!, "*MajorRadius", dxxPropertyTypes.dxf_Double, bNonDXF:=True))
                    _ELLIPSE.Add(New TPROPERTY(hidx + 5, False, "*Clockwise", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _ELLIPSE.ReIndex(True)
                End If
                Return New TPROPERTIES(_ELLIPSE)
            End Get
        End Property
        Private Shared _ENDBLK As TPROPERTIES
        Friend Shared ReadOnly Property Props_ENDBLK As TPROPERTIES
            Get
                If _ENDBLK.Count = 0 Then ' If _ENDBLK.IsEmpty Then
                    _ENDBLK = New TPROPERTIES("ENDBLK")
                    _ENDBLK.Add(New TPROPERTY(0, "ENDBLK", "Entity Type", dxxPropertyTypes.dxf_String))
                    _ENDBLK.Add(New TPROPERTY(5, "0", "Handle", dxxPropertyTypes.Handle))
                    _ENDBLK.Add(New TPROPERTY(330, "0", "Owner Handle", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _ENDBLK.Add(New TPROPERTY(100, "AcDbEntity", "Entity Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _ENDBLK.Add(New TPROPERTY(67, 0, "IsPaperSpace", dxxPropertyTypes.Switch, bSetSuppressedValue:=True))
                    _ENDBLK.Add(New TPROPERTY(8, "0", "LayerName", dxxPropertyTypes.dxf_String))
                    _ENDBLK.Add(New TPROPERTY(100, "AcDbBlockEnd", "Entity Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _ENDBLK.ReIndex(True)
                End If
                Return New TPROPERTIES(_ENDBLK)
            End Get
        End Property

        Private Shared _HATCH As TPROPERTIES
        Friend Shared ReadOnly Property Props_HATCH As TPROPERTIES
            Get
                If _HATCH.Count = 0 Then ' If _HATCH.IsEmpty Then
                    Dim hidx As Integer = 0
                    _HATCH = Get_CommonProps("HATCH", hidx)
                    _HATCH.Add(New TPROPERTY(100, "AcDbHatch", "Entity Sub Class Marker_1", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _HATCH.AddVector(10, TVECTOR.Zero, "Elevation Point") '(Z ordinate = Elevation)
                    _HATCH.AddVector(210, TVECTOR.WorldZ, "Extrusion Direction", bIsDirection:=True)
                    _HATCH.Add(New TPROPERTY(2, "", "Pattern Name", dxxPropertyTypes.dxf_String))
                    _HATCH.Add(New TPROPERTY(70, False, "Solid Fill Flag", dxxPropertyTypes.Switch))
                    _HATCH.Add(New TPROPERTY(63, dxxColors.Undefined, "Pattern Fill Color(ACL)", dxxPropertyTypes.Color, bSetSuppressedValue:=True))
                    _HATCH.Add(New TPROPERTY(71, False, "Associative Flag", dxxPropertyTypes.Switch))
                    _HATCH.Add(New TPROPERTY(91, 0, "Boundary Loop Count", dxxPropertyTypes.dxf_Integer))
                    _HATCH.Add(New TPROPERTY(75, dxxHatchMethods.Normal, "Hatch Style", dxxPropertyTypes.dxf_Integer))
                    _HATCH.Add(New TPROPERTY(76, dxxHatchTypes.User, "Hatch Type", dxxPropertyTypes.dxf_Integer))
                    _HATCH.Add(New TPROPERTY(52, 0!, "Pattern Angle", dxxPropertyTypes.Angle))
                    _HATCH.Add(New TPROPERTY(41, 1.0!, "Pattern Step/Scale", dxxPropertyTypes.dxf_Double, bSetSuppressedValue:=True))
                    _HATCH.Add(New TPROPERTY(73, False, "Annotated Boundary Flag", dxxPropertyTypes.Switch, bSetSuppressedValue:=True))
                    _HATCH.Add(New TPROPERTY(77, False, "Doubled Flag", dxxPropertyTypes.Switch))
                    _HATCH.Add(New TPROPERTY(78, 0, "Pattern Line Count", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive))
                    _HATCH.Add(New TPROPERTY(47, 0, "Pixel Sizel", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive, bSetSuppressedValue:=True))
                    _HATCH.Add(New TPROPERTY(98, 1, "Seed Point Count", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive))
                    _HATCH.AddVector(11, TVECTOR.Zero, "Offset Vector(MPolygon)", bScaleable:=True)
                    _HATCH.Add(New TPROPERTY(99, 0, "Degenerated Boundary Loop Count(MPolygon)", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive, bSetSuppressedValue:=True))
                    _HATCH.AddVector(10, TVECTOR.Zero, "Seed Point(s)")
                    _HATCH.Add(New TPROPERTY(450, False, "Gradient Flag", dxxPropertyTypes.Switch, bSetSuppressedValue:=True))
                    _HATCH.Add(New TPROPERTY(451, 0, "Reserved", dxxPropertyTypes.Switch, bSetSuppressedValue:=True))
                    _HATCH.Add(New TPROPERTY(452, True, "Gradient Style Flag", dxxPropertyTypes.Switch, bSetSuppressedValue:=True))
                    _HATCH.Add(New TPROPERTY(453, 0, "Gradient Color Count", dxxPropertyTypes.dxf_Integer, dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive, bSetSuppressedValue:=True))
                    _HATCH.Add(New TPROPERTY(460, 0, "Gradient Rotation Angle", dxxPropertyTypes.Angle, dxxPropertyTypes.dxf_Integer, bSetSuppressedValue:=True))
                    _HATCH.Add(New TPROPERTY(461, 0, "Gradient Shift", dxxPropertyTypes.dxf_Integer, dxxPropertyTypes.dxf_Integer, bSetSuppressedValue:=True))
                    _HATCH.Add(New TPROPERTY(462, 0, "Gradient Color Tint", dxxPropertyTypes.dxf_Integer, bSetSuppressedValue:=True))
                    _HATCH.Add(New TPROPERTY(463, 0, "Reserved 2", dxxPropertyTypes.dxf_Integer, bSetSuppressedValue:=True))
                    _HATCH.Add(New TPROPERTY(470, "LINEAR", "String Type", dxxPropertyTypes.dxf_String, bSetSuppressedValue:=True))

                    _HATCH.Add(New TPROPERTY(hidx + 1, 1.0!, "*ScaleFactor", dxxPropertyTypes.dxf_Single, bNonDXF:=True))
                    _HATCH.Add(New TPROPERTY(hidx + 2, dxxHatchStyle.dxfHatchUserDefined, "*Style", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))
                    _HATCH.ReIndex(True)
                End If
                Return New TPROPERTIES(_HATCH)
            End Get
        End Property


        Private Shared _HOLE As TPROPERTIES
        Friend Shared ReadOnly Property Props_HOLE As TPROPERTIES
            Get
                If _HOLE.Count = 0 Then ' If _HOLE.IsEmpty Then
                    Dim hidx As Integer
                    _HOLE = Get_CommonProps("HOLE", hidx)
                    _HOLE.Add(New TPROPERTY(40, 1, "Radius", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero))
                    _HOLE.Add(New TPROPERTY(41, 0!, "Length", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                    _HOLE.Add(New TPROPERTY(42, 0!, "MinorRadius", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                    _HOLE.Add(New TPROPERTY(50, 0!, "Rotation", dxxPropertyTypes.Angle))
                    _HOLE.Add(New TPROPERTY(43, 0!, "Depth", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                    _HOLE.Add(New TPROPERTY(44, 0!, "DownSet", dxxPropertyTypes.dxf_Double))
                    _HOLE.Add(New TPROPERTY(45, 0!, "Inset", dxxPropertyTypes.dxf_Double))
                    _HOLE.Add(New TPROPERTY(70, False, "Welded Bolt", dxxPropertyTypes.Switch))
                    _HOLE.Add(New TPROPERTY(71, False, "IsSquare", dxxPropertyTypes.Switch))
                    _HOLE.AddVector(10, TVECTOR.Zero, "Center")
                    _HOLE.AddVector(210, TVECTOR.WorldZ, "Extrusion Direction", bIsDirection:=True)
                    _HOLE.AddVector(211, TVECTOR.WorldX, "X Direction", bIsDirection:=True)
                    _HOLE.Add(New TPROPERTY(12, "", "BlockName", dxxPropertyTypes.dxf_String))

                    _HOLE.Add(New TPROPERTY(hidx + 1, "", "*Descriptor", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    _HOLE.ReIndex(True)
                End If
                Return New TPROPERTIES(_HOLE)
            End Get
        End Property

        Private Shared _INSERT As TPROPERTIES
        Friend Shared ReadOnly Property Props_INSERT As TPROPERTIES
            Get
                If _INSERT.Count = 0 Then ' If _INSERT.IsEmpty Then
                    Dim hidx As Integer = 0
                    _INSERT = Get_CommonProps("INSERT", hidx)
                    _INSERT.Add(New TPROPERTY(100, "AcDbBlockReference", "Entity Sub Class Marker_1", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _INSERT.Add(New TPROPERTY(66, False, "Attribute Follow Flag", dxxPropertyTypes.Switch, bSetSuppressedValue:=True))
                    _INSERT.Add(New TPROPERTY(2, "", "Block Name", dxxPropertyTypes.dxf_String))
                    _INSERT.AddVector(10, TVECTOR.Zero, "Insertion Pt")
                    _INSERT.Add(New TPROPERTY(41, 1, "X Scale Factor", dxxPropertyTypes.dxf_Double, bScalable:=True, bSetSuppressedValue:=True))
                    _INSERT.Add(New TPROPERTY(42, 1, "Y Scale Factor", dxxPropertyTypes.dxf_Double, bScalable:=True, bSetSuppressedValue:=True))
                    _INSERT.Add(New TPROPERTY(43, 1, "Z Scale Factor", dxxPropertyTypes.dxf_Double, bScalable:=True, bSetSuppressedValue:=True))
                    _INSERT.Add(New TPROPERTY(50, 0, "Rotation Angle", dxxPropertyTypes.Angle, bSetSuppressedValue:=True))
                    _INSERT.AddVector(210, TVECTOR.WorldZ, "Extrusion Direction", bIsDirection:=True)
                    _INSERT.Add(New TPROPERTY(70, 1, "Columns", dxxPropertyTypes.dxf_Integer, bSetSuppressedValue:=True))
                    _INSERT.Add(New TPROPERTY(71, 1, "Rows", dxxPropertyTypes.dxf_Integer, bSetSuppressedValue:=True))
                    _INSERT.Add(New TPROPERTY(44, 0, "Column Space", dxxPropertyTypes.dxf_Double, bScalable:=True, bSetSuppressedValue:=True))
                    _INSERT.Add(New TPROPERTY(45, 0, "Row Space", dxxPropertyTypes.dxf_Double, bScalable:=True, bSetSuppressedValue:=True))
                    _INSERT.Add(New TPROPERTY(hidx + 1, "", "*SourceBlockGUID", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    _INSERT.Add(New TPROPERTY(hidx + 2, "", "*BlockHandle", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    _INSERT.Add(New TPROPERTY(hidx + 3, "", "*BlockRecordHandle", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    _INSERT.Add(New TPROPERTY(hidx + 4, False, "*UniformScale", dxxPropertyTypes.Switch, bNonDXF:=True))
                    _INSERT.ReIndex(True)
                End If
                Return New TPROPERTIES(_INSERT)
            End Get
        End Property

        Private Shared _LEADER As TPROPERTIES
        Friend Shared ReadOnly Property Props_LEADER As TPROPERTIES
            Get
                If _LEADER.Count = 0 Then ' If _LEADER.IsEmpty Then
                    Dim hidx As Integer
                    _LEADER = Get_CommonProps("LEADER", hidx)
                    _LEADER.Add(New TPROPERTY(100, "AcDbLeader", "Entity Sub Class Marker_1", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _LEADER.Add(New TPROPERTY(3, "Standard", "DimStyle Name", dxxPropertyTypes.dxf_String) With {.Key = "DimStyle"})
                    _LEADER.Add(New TPROPERTY(71, True, "ArrowHead Flag", dxxPropertyTypes.Switch))
                    _LEADER.Add(New TPROPERTY(72, 0, "Path Type", dxxPropertyTypes.dxf_Integer))
                    _LEADER.Add(New TPROPERTY(73, dxxLeaderTypes.NoReactor, "Leader Type", dxxPropertyTypes.dxf_Integer))
                    _LEADER.Add(New TPROPERTY(74, False, "Hook Direction Flag", dxxPropertyTypes.Switch))
                    _LEADER.Add(New TPROPERTY(75, True, "Hook Flag", dxxPropertyTypes.Switch))
                    _LEADER.Add(New TPROPERTY(40, 0, "Text Annotation Height", dxxPropertyTypes.dxf_Double, bScalable:=True, bSetSuppressedValue:=True))
                    _LEADER.Add(New TPROPERTY(41, 0, "Text Annotation Width", dxxPropertyTypes.dxf_Double, bScalable:=True, bSetSuppressedValue:=True))
                    _LEADER.Add(New TPROPERTY(76, 0, "Vertex Count", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive))
                    _LEADER.Add(New TPROPERTY(77, dxxColors.Undefined, "ByBlock Color", dxxPropertyTypes.dxf_Integer))
                    _LEADER.Add(New TPROPERTY(340, "0", "Annotation Handle", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _LEADER.AddVector(210, TVECTOR.WorldZ, "Extrusion Direction", bIsDirection:=True)
                    _LEADER.AddVector(211, TVECTOR.WorldX, "Horizontal Direction", bIsDirection:=True)
                    _LEADER.AddVector(212, Nothing, "Block Offset", bScaleable:=True)
                    _LEADER.AddVector(213, Nothing, "Annotation Offset", bScaleable:=True)

                    _LEADER.Add(New TPROPERTY(hidx + 1, 0, "*TextHeight", dxxPropertyTypes.dxf_Double, bScalable:=True, bNonDXF:=True))
                    _LEADER.Add(New TPROPERTY(hidx + 2, "0", "*TextLayer", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    _LEADER.Add(New TPROPERTY(hidx + 3, "", "*TextString", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    _LEADER.Add(New TPROPERTY(hidx + 4, 0!, "*BlockRotation", dxxPropertyTypes.Angle, bNonDXF:=True))
                    _LEADER.Add(New TPROPERTY(hidx + 5, 1.0!, "*BlockScale", dxxPropertyTypes.dxf_Double, bNonDXF:=True))
                    _LEADER.Add(New TPROPERTY(hidx + 6, False, "*HasArrowHead", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _LEADER.Add(New TPROPERTY(hidx + 7, False, "*HasHook", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _LEADER.Add(New TPROPERTY(hidx + 8, False, "*IsSymbol", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _LEADER.Add(New TPROPERTY(hidx + 9, False, "*PopLeft", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _LEADER.Add(New TPROPERTY(hidx + 10, dxxVerticalJustifications.Center, "*TextJustification", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))
                    _LEADER.Add(New TPROPERTY(hidx + 11, "", "*BlockName", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    _LEADER.Add(New TPROPERTY(hidx + 12, "", "*SourceBlockGUID", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    _LEADER.AddVector(hidx + 13, TVECTOR.Zero, "*Vector1")
                    _LEADER.AddVector(hidx + 31, TVECTOR.Zero, "*Vector2")

                    _LEADER.ReIndex(True)
                End If
                Return New TPROPERTIES(_LEADER)
            End Get
        End Property

        Private Shared _LINE As TPROPERTIES
        Friend Shared ReadOnly Property Props_LINE As TPROPERTIES
            Get

                If _LINE.Count = 0 Then ' If _LINE.IsEmpty Then
                    Dim hidx As Integer = 0
                    _LINE = Get_CommonProps("LINE", hidx)
                    _LINE.Add(New TPROPERTY(100, "AcDbLine", "Entity Sub Class Marker_1", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _LINE.Add(New TPROPERTY(39, 0, "Thickness", dxxPropertyTypes.dxf_Double, bScalable:=True, bSetSuppressedValue:=True))
                    _LINE.AddVector(10, TVECTOR.Zero, "StartPt")
                    _LINE.AddVector(11, TVECTOR.Zero, "EndPt")
                    _LINE.AddVector(210, TVECTOR.WorldZ, "Extrusion Direction", bIsDirection:=True)
                    _LINE.Add(New TPROPERTY(hidx + 1, 0!, "*StartWidth", dxxPropertyTypes.dxf_Single, True, bNonDXF:=True))
                    _LINE.Add(New TPROPERTY(hidx + 2, 0!, "*EndWidth", dxxPropertyTypes.dxf_Single, True, bNonDXF:=True))
                    _LINE.ReIndex(True)
                End If
                Return New TPROPERTIES(_LINE)
            End Get
        End Property

        Private Shared _POINT As TPROPERTIES
        Friend Shared ReadOnly Property Props_POINT As TPROPERTIES
            Get
                If _POINT.Count = 0 Then ' If _POINT.IsEmpty Then
                    _POINT = Get_CommonProps("POINT")
                    _POINT.Add(New TPROPERTY(100, "AcDbPoint", "Entity Sub Class Marker_1", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _POINT.AddVector(10, TVECTOR.Zero, "Ordinates")
                    _POINT.Add(New TPROPERTY(39, 0, "Thickness", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bScalable:=True, bSetSuppressedValue:=True))

                    _POINT.AddVector(210, TVECTOR.WorldZ, "Extrusion Direction", bIsDirection:=True)
                    _POINT.Add(New TPROPERTY(50, 0, "X Axis Rotation", dxxPropertyTypes.Angle, bSetSuppressedValue:=True))
                    _POINT.ReIndex(True)
                End If
                Return New TPROPERTIES(_POINT)
            End Get
        End Property

        Private Shared _LWPOLYLINE As TPROPERTIES
        Friend Shared ReadOnly Property Props_LWPOLYLINE As TPROPERTIES
            Get
                If _LWPOLYLINE.Count = 0 Then ' If _LWPOLYLINE.IsEmpty Then
                    Dim hidx As Integer = 0
                    _LWPOLYLINE = Get_CommonProps("LWPOLYLINE", hidx)
                    _LWPOLYLINE.Add(New TPROPERTY(100, "AcDbPolyline", "Entity Sub Class Marker_1", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _LWPOLYLINE.Add(New TPROPERTY(90, 0, "Vertex Count", dxxPropertyTypes.dxf_Integer))
                    _LWPOLYLINE.Add(New TPROPERTY(70, 0, "Polyline Flag(Bit coded)", dxxPropertyTypes.BitCode))
                    _LWPOLYLINE.Add(New TPROPERTY(43, 0, "Constant Width", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bScalable:=True, bSetSuppressedValue:=True))
                    _LWPOLYLINE.Add(New TPROPERTY(38, 0, "Elevation", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bScalable:=True, bSetSuppressedValue:=True))
                    _LWPOLYLINE.Add(New TPROPERTY(39, 0, "Thickness", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bScalable:=True, bSetSuppressedValue:=True))
                    _LWPOLYLINE.AddVector(210, TVECTOR.WorldZ, "Extrusion Direction", bIsDirection:=True)
                    _LWPOLYLINE.Add(New TPROPERTY(hidx + 1, False, "*Closed", dxxPropertyTypes.Switch, bNonDXF:=True))
                    _LWPOLYLINE.Add(New TPROPERTY(hidx + 2, False, "*PlineGen", dxxPropertyTypes.Switch, bNonDXF:=True))
                    _LWPOLYLINE.Add(New TPROPERTY(hidx + 3, False, "*3D", dxxPropertyTypes.Switch, bNonDXF:=True))
                    _LWPOLYLINE.Add(New TPROPERTY(hidx + 4, False, "*SuppressAdditionalSegments", dxxPropertyTypes.Switch, bNonDXF:=True))
                    _LWPOLYLINE.ReIndex(True)
                End If
                Return New TPROPERTIES(_LWPOLYLINE)
            End Get
        End Property


        Private Shared _POLYGON As TPROPERTIES
        Friend Shared ReadOnly Property Props_POLYGON As TPROPERTIES
            Get
                If _POLYGON.Count = 0 Then ' If _POLYGON.IsEmpty Then

                    _POLYGON = Props_LWPOLYLINE
                    Dim hidx = _POLYGON.HiddenMembers.Count
                    _POLYGON.Add(New TPROPERTY(hidx + 1, "", "*BlockName", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    _POLYGON.Add(New TPROPERTY(hidx + 2, "", "*SourceBlockGUID", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    _POLYGON.Name = "POLYGON"
                    _POLYGON.SetValGC(0, "POLYGON")
                    _POLYGON.SetVal("*Closed", True)
                    _POLYGON.ReIndex(True)
                End If
                Return New TPROPERTIES(_POLYGON)
            End Get
        End Property

        Private Shared _REGION As TPROPERTIES

        Friend Shared ReadOnly Property Props_REGION As TPROPERTIES
            Get
                If _REGION.Count = 0 Then

                    _REGION = Get_CommonProps("REGION")
                    _REGION.Add(New TPROPERTY(100, "AcDbModelerGeometry", "Entity Sub Class Marker_1", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _REGION.Add(New TPROPERTY(70, 1, "Modeler format version number", dxxPropertyTypes.dxf_Integer))
                    _REGION.Add(New TPROPERTY(1, "", "Proprietary Data", dxxPropertyTypes.dxf_String))
                    _REGION.Add(New TPROPERTY(3, "", "Proprietary Data(additional)", dxxPropertyTypes.dxf_String, bSetSuppressedValue:=True))
                    _REGION.ReIndex(True)
                End If
                Return New TPROPERTIES(_REGION)
            End Get
        End Property

        Private Shared _SEQEND As TPROPERTIES
        Friend Shared ReadOnly Property Props_SEQEND As TPROPERTIES
            Get
                If _SEQEND.Count = 0 Then

                    _SEQEND = Get_CommonProps("SEQEND")
                    '1      _SEQEND.Add( 100, "AcDbPoint", "Entity Sub Class Marker_1", , ClassMarker
                    '2        props_AddVector _SEQEND, 10,Nothing, "Ordinates"
                    '3        props_Add _SEQEND, 39, 0, "Thickness", 0, , , True
                    '4        props_AddVector _SEQEND, 210, NEW TVECTOR(0, 0, 1), "Extrusion Direction", , , True
                    '5        props_Add _SEQEND, 50, 0, "X Axis Rotation", 0
                    _SEQEND.ReIndex(True)
                End If
                Return New TPROPERTIES(_SEQEND)
            End Get
        End Property

        Private Shared _MTEXT As TPROPERTIES
        Friend Shared ReadOnly Property Props_MTEXT As TPROPERTIES
            Get
                If _MTEXT.Count <= 1 Then
                    Dim hidx As Integer = 0
                    _MTEXT = Get_CommonProps("MTEXT", hidx)
                    _MTEXT.Add(New TPROPERTY(100, "AcDbMText", "Entity Sub Class Marker_1", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _MTEXT.AddVector(10, TVECTOR.Zero, "AligmentPt 1")
                    _MTEXT.Add(New TPROPERTY(40, 0.2, "Text Height", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bScalable:=True))

                    _MTEXT.Add(New TPROPERTY(41, 0!, "Reference Rectangle Width", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bScalable:=True, bSetSuppressedValue:=True))

                    _MTEXT.Add(New TPROPERTY(71, dxxMTextAlignments.TopLeft, "Alignment", dxxPropertyTypes.dxf_Integer))
                    _MTEXT.Add(New TPROPERTY(72, dxxTextDrawingDirections.ByStyle, "Drawing Direction", dxxPropertyTypes.dxf_Integer))
                    _MTEXT.Add(New TPROPERTY(1, "", "Text String", dxxPropertyTypes.dxf_String))
                    _MTEXT.Add(New TPROPERTY(7, "Standard", "Text Style Name", dxxPropertyTypes.dxf_String, bSetSuppressedValue:=True))
                    _MTEXT.AddVector(210, TVECTOR.WorldZ, "Extrusion Direction", bIsDirection:=True)
                    _MTEXT.AddVector(11, TVECTOR.WorldX, "X-Axis Direction", bIsDirection:=True)
                    _MTEXT.Add(New TPROPERTY(42, 0!, "Horizontal Rectangle Width", dxxPropertyTypes.dxf_Double, bScalable:=True))
                    _MTEXT.Add(New TPROPERTY(43, 0!, "Vertical Rectangle Width", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bScalable:=True))
                    _MTEXT.Add(New TPROPERTY(50, 0!, "Text Angle(radians)", dxxPropertyTypes.dxf_Double, bSetSuppressedValue:=True))
                    _MTEXT.Add(New TPROPERTY(73, dxxLineSpacingStyles.AtLeast, "Line Spacing Style", dxxPropertyTypes.dxf_Integer, bSetSuppressedValue:=True))
                    _MTEXT.Add(New TPROPERTY(44, 1.0!, "Line Spacing Factor", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, bSetSuppressedValue:=True))

                    _MTEXT.Add(New TPROPERTY(90, dxxBackgroundFillModes.Off, "Background Fill", dxxPropertyTypes.dxf_Integer, bSetSuppressedValue:=True))
                    _MTEXT.Add(New TPROPERTY(63, dxxColors.Undefined, "Background Fill Color(ACL)", dxxPropertyTypes.Color, bSetSuppressedValue:=True))
                    _MTEXT.Add(New TPROPERTY(421, 0, "Background Fill Color(Integer)", dxxPropertyTypes.dxf_Integer, bSetSuppressedValue:=True))
                    _MTEXT.Add(New TPROPERTY(431, "", "Background Fill Color Name", dxxPropertyTypes.dxf_String, bSetSuppressedValue:=True))
                    _MTEXT.Add(New TPROPERTY(45, 1, "Fill Box Scale(1-1.5)", dxxPropertyTypes.dxf_Double, bSetSuppressedValue:=True) With {.Max = 1.5, .Min = 1})
                    _MTEXT.Add(New TPROPERTY(441, 0, "Fill Transparency", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive, bSetSuppressedValue:=True))
                    _MTEXT.Add(New TPROPERTY(hidx + 1, False, "*IsDimensionText", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _MTEXT.Add(New TPROPERTY(hidx + 2, dxxTextDrawingDirections.ByStyle, "*DrawingDirection", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))
                    _MTEXT.Add(New TPROPERTY(hidx + 3, 0, "*DimensionTextAngle", dxxPropertyTypes.Angle, bNonDXF:=True))
                    _MTEXT.ReIndex(True)
                End If
                Return New TPROPERTIES(_MTEXT)
            End Get
        End Property

        Private Shared _SHAPE As TPROPERTIES
        Friend Shared ReadOnly Property Props_SHAPE As TPROPERTIES
            Get

                If _SHAPE.Count = 0 Then
                    Dim hidx As Integer = 0
                    _SHAPE = Get_CommonProps("SHAPE", hidx)
                    _SHAPE.Add(New TPROPERTY(100, "AcDbShape", "Entity Sub Class Marker_1", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _SHAPE.Add(New TPROPERTY(39, 0, "Thickness", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bScalable:=True, bSetSuppressedValue:=True))
                    _SHAPE.AddVector(10, TVECTOR.Zero, "Insertion Pt")
                    _SHAPE.Add(New TPROPERTY(40, 0, "Size", dxxPropertyTypes.dxf_Double, bScalable:=True))
                    _SHAPE.Add(New TPROPERTY(2, "", "Shape Name", dxxPropertyTypes.dxf_String))
                    _SHAPE.Add(New TPROPERTY(50, 0, "Rotation Angle", dxxPropertyTypes.Angle, bSetSuppressedValue:=True))
                    _SHAPE.Add(New TPROPERTY(41, 1, "Width Factor", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, bSetSuppressedValue:=True))
                    _SHAPE.Add(New TPROPERTY(51, 0, "Oblique Angle", dxxPropertyTypes.Angle, bSetSuppressedValue:=True))
                    _SHAPE.AddVector(210, TVECTOR.WorldZ, "Extrusion Direction", bIsDirection:=True)
                    _SHAPE.Add(New TPROPERTY(hidx + 1, False, "*SaveExplode", dxxPropertyTypes.Switch, bNonDXF:=True))
                    _SHAPE.Add(New TPROPERTY(hidx + 2, "", "*FileName", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    _SHAPE.Add(New TPROPERTY(hidx + 3, "", "*ShapeNumber", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))
                    _SHAPE.Add(New TPROPERTY(hidx + 4, "", "*ShapeCommands", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    _SHAPE.ReIndex(True)
                End If
                Return New TPROPERTIES(_SHAPE)
            End Get
        End Property

        Private Shared _SOLID As TPROPERTIES
        Friend Shared ReadOnly Property Props_SOLID As TPROPERTIES
            Get
                If _SOLID.Count = 0 Then ' If _SOLID.IsEmpty Then
                    Dim hidx As Integer = 0
                    _SOLID = Get_CommonProps("SOLID", hidx)
                    _SOLID.Add(New TPROPERTY(100, "AcDbTrace", "Entity Sub Class Marker_1", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _SOLID.AddVector(10, TVECTOR.Zero, "Vertex 1")
                    _SOLID.AddVector(11, TVECTOR.Zero, "Vertex 2")
                    _SOLID.AddVector(12, TVECTOR.Zero, "Vertex 3")
                    _SOLID.AddVector(13, TVECTOR.Zero, "Vertex 4")
                    _SOLID.Add(New TPROPERTY(39, 0, "Thickness", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bScalable:=True, bSetSuppressedValue:=True))
                    _SOLID.AddVector(210, TVECTOR.WorldZ, "Extrusion Direction", bIsDirection:=True)
                    _SOLID.Add(New TPROPERTY(hidx + 1, True, "*Filled", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _SOLID.Add(New TPROPERTY(hidx + 2, True, "*Triangular", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _SOLID.ReIndex(True)
                End If
                Return New TPROPERTIES(_SOLID)
            End Get
        End Property
        Friend Shared Function Properties_Setting_DIMOVERRIDES(Optional aName As String = "", Optional aGUID As String = "") As dxoProperties

            '^the default properties for a DIMOVERRIDES
            'dimstyles have r14 prperties to begin the non applicable

            Dim _rVal As dxoProperties = Nothing
            If String.IsNullOrWhiteSpace(aName) Then aName = "Standard"
            Try
                _rVal = New dxoProperties(dxfEnums.DisplayName(dxxReferenceTypes.DIMSTYLE).ToUpper) From
                 {
                    New dxoProperty(105, "", "Handle", dxxPropertyTypes.Handle),
                    New dxoProperty(2, aName, "Name", dxxPropertyTypes.dxf_String)
              }

                _rVal.Append(dxpProperties.DimStyleProps, bAddClones:=True)
                _rVal.Add(New TPROPERTY(1, aGUID, "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True})
                _rVal.ReIndex(True)

                Return _rVal
            Catch ex As Exception
                Return _rVal
            End Try

        End Function
        Friend Shared Function Properties_Setting_DIM(aGUID As String) As dxoProperties

            Return New dxoProperties("DimSettings", True) From
                {
                    New dxoProperty(1, dxxDrawingUnits.English, "DrawingUnits", dxxPropertyTypes.dxf_Integer, bNonDXF:=True),
                    New dxoProperty(2, "", "LeaderLayer", dxxPropertyTypes.dxf_String, bNonDXF:=True),
                    New dxoProperty(3, dxxColors.Undefined, "LeaderLayerColor", dxxPropertyTypes.Color, bNonDXF:=True),
                    New dxoProperty(4, "", "DimLayer", dxxPropertyTypes.dxf_String, bNonDXF:=True),
                    New dxoProperty(5, dxxColors.Undefined, "DimLayerColor", dxxPropertyTypes.Color, bNonDXF:=True),
                    New dxoProperty(6, 0.05, "DimTickLength", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bNonDXF:=True),
                    New dxoProperty(7, 1.0, "DrawingScale", dxxPropertyTypes.dxf_Double,, aValControl:=mzValueControls.PositiveNonZero, bNonDXF:=True),
                    New dxoProperty(8, dxxVerticalJustifications.Center, "LeaderTextJustification", dxxPropertyTypes.dxf_Integer, bNonDXF:=True)
                }
        End Function
        Friend Shared Function Properties_Settings_HEADER(aGUID As String) As dxoProperties

            Dim _rVal As dxoProperties = Nothing
            Try

                '^the default properties for a HEADER
                '^executed on object initialization to populate the properties collection
                Dim dstyleprops As dxoProperties = DimStyleProps


                _rVal = New dxoProperties("HEADER")
                '===========================================================================
                Dim enumVals As Dictionary(Of String, Integer) = dxfEnums.EnumValues(GetType(dxxHeaderVars))
                Dim nm As String



                For Each nm In enumVals.Keys
                    Dim ival As dxxHeaderVars = DirectCast(enumVals.Item(nm), dxxHeaderVars)
                    Dim p2 As dxoProperty = Nothing
                    Dim p3 As dxoProperty = Nothing
                    Dim newprop As dxoProperty = dxpProperties.HeaderProperty(ival, dstyleprops, p2, p3)
                    'Dim nameprop As New dxoProperty(9, newprop.Name, $"{newprop.Name}_NAME", dxxPropertyTypes.dxf_String) With {.GroupName = newprop.Name}

                    '_rVal.Add(nameprop)
                    'newprop.Key = newprop.Name
                    'newprop.Name += "_VALUE"
                    'newprop.Key = nameprop.GroupName
                    _rVal.Add(newprop)
                    If p2 IsNot Nothing Then _rVal.Add(p2)
                    If p3 IsNot Nothing Then _rVal.Add(p3)



                Next

                _rVal.Add(New dxoProperty(1, aGUID, "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True})
                _rVal.SetVal("$TDCREATE", dxfUtils.DateToJulian(Now))
                _rVal.SetVal("$FINGERPRINTGUID", "{1EA0BA20-4020-11D7-B716-0002A559B82A}") ' dxfUtils.CreateGUID()
                _rVal.SetVal("$VERSIONGUID", "{4D1CF5B6-90EE-430C-A181-1CCD4F9101A7}") 'dxfUtils.CreateGUID()
                Dim hcnt As Integer = _rVal.FindAll(Function(x) x.Hidden).Count
                _rVal.Add(New dxoProperty(hcnt + 1, "HEADER", "*Name", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                Return _rVal
            Catch ex As Exception
                If dxfUtils.RunningInIDE Then MessageBox.Show($"{Reflection.MethodBase.GetCurrentMethod.Name }  ERROR - {ex.Message}", "Error", MessageBoxButtons.OK, icon:=MessageBoxIcon.Warning)
                Return _rVal

            End Try

        End Function

        Friend Shared Function Properties_Setting_SCREEN(aGUID As String) As dxoProperties
            '^the default properties for a screen settings object
            Return New dxoProperties("ScreenSettings", True) From
            {
                New dxoProperty(1, False, "Suppressed", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True),
            New dxoProperty(2, False, "BoundingRectangles", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True),
            New dxoProperty(3, False, "OCSs", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True),
            New dxoProperty(4, False, "ExtentPts", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True),
            New dxoProperty(5, False, "ExtentRectangle", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True),
            New dxoProperty(6, False, "TextBoxes", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True),
            New dxoProperty(7, dxxColors.Blue, "EntitySymbolColor", dxxPropertyTypes.Color, bNonDXF:=True),
            New dxoProperty(8, dxxColors.Blue, "TextColor", dxxPropertyTypes.Color, bNonDXF:=True),
            New dxoProperty(9, dxxColors.Blue, "AxisColor", dxxPropertyTypes.Color, bNonDXF:=True),
            New dxoProperty(10, dxxColors.Blue, "RectangleColor", dxxPropertyTypes.Color, bNonDXF:=True),
            New dxoProperty(11, dxxColors.Grey, "ExtentRectangleColor", dxxPropertyTypes.Color, bNonDXF:=True),
            New dxoProperty(12, dxxColors.Blue, "PointColor", dxxPropertyTypes.Color, bNonDXF:=True),
            New dxoProperty(13, 0.05, "OCSSize", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, bNonDXF:=True),
            New dxoProperty(14, 0.075, "TextSize", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, bNonDXF:=True),
            New dxoProperty(15, 4, "PointSize", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.PositiveNonZero, bNonDXF:=True),
            New dxoProperty(16, dxxColors.Blue, "CircleColor", dxxPropertyTypes.Color, bNonDXF:=True),
            New dxoProperty(17, dxxColors.Blue, "PointerColor", dxxPropertyTypes.Color, bNonDXF:=True),
            New dxoProperty(18, dxxColors.Blue, "LineColor", dxxPropertyTypes.Color, bNonDXF:=True),
            New dxoProperty(19, dxfLinetypes.Continuous, "LineType", dxxPropertyTypes.dxf_String, bNonDXF:=True),
            New dxoProperty(20, 1.0, "LTScale", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, bNonDXF:=True),
                New dxoProperty(21, aGUID, "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True}
            }


        End Function

        Friend Shared Function Properties_Setting_SYMBOL(aGUID As String) As dxoProperties
            Return New dxoProperties("SymbolSettings", True) From
                {
            New dxoProperty(1, 1.0!, "FeatureScale", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.NonZero, bScalable:=True, bNonDXF:=True),
            New dxoProperty(2, 0.2!, "TextHeight", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.NonZero, bScalable:=True, bNonDXF:=True),
            New dxoProperty(3, 0.09!, "TextGap", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.NonZero, bScalable:=True, bNonDXF:=True),
            New dxoProperty(4, 0.18!, "ArrowSize", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.NonZero, bScalable:=True, bNonDXF:=True),
            New dxoProperty(5, "", "LayerName", dxxPropertyTypes.dxf_String, bNonDXF:=True),
            New dxoProperty(6, "", "TextStyle", dxxPropertyTypes.dxf_String, bNonDXF:=True),
            New dxoProperty(7, dxxColors.BlackWhite, "LayerColor", dxxPropertyTypes.Color, bNonDXF:=True),
            New dxoProperty(8, dxxColors.ByBlock, "LineColor", dxxPropertyTypes.Color, bNonDXF:=True),
            New dxoProperty(9, dxxColors.ByBlock, "TextColor", dxxPropertyTypes.Color, bNonDXF:=True),
            New dxoProperty(10, dxxRectangularAlignments.TopLeft, "ArrowTextAlignment", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.PositiveNonZero, bNonDXF:=True),
            New dxoProperty(11, dxxArrowHeadTypes.ClosedFilled, "ArrowHead", dxxPropertyTypes.dxf_Integer, bNonDXF:=True),
            New dxoProperty(12, dxxArrowStyles.AngledHalf, "ArrowStyle", dxxPropertyTypes.dxf_Integer, bNonDXF:=True),
            New dxoProperty(13, dxxArrowTails.Undefined, "ArrowTails", dxxPropertyTypes.dxf_Integer, bNonDXF:=True),
            New dxoProperty(14, 0, "AxisStyle", dxxPropertyTypes.dxf_Integer, bNonDXF:=True),
            New dxoProperty(15, False, "BoxText", dxxPropertyTypes.Switch, bNonDXF:=True),
                New dxoProperty(16, aGUID, "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True}
            }


        End Function
        Friend Shared Function Properties_Setting_TEXT(aGUID As String) As dxoProperties
            Return New dxoProperties("TextSettings", True) From
                {
            New dxoProperty(1, "", "LayerName", dxxPropertyTypes.dxf_String, bNonDXF:=True),
            New dxoProperty(2, dxxColors.Undefined, "LayerColor", dxxPropertyTypes.Color, bNonDXF:=True),
            New dxoProperty(3, dxxColors.Undefined, "Color", dxxPropertyTypes.Color, bNonDXF:=True),
            New dxoProperty(4, dxxLineWeights.ByDefault, "LineWeight", dxxPropertyTypes.dxf_Integer, bNonDXF:=True),
             New dxoProperty(5, aGUID, "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True}
            }

        End Function

        Friend Shared Function Properties_Setting_TABLE(aGUID As String) As dxoProperties

            '^the default properties for a dxeTable
            Dim _rVal As New dxoProperties("TableSettings", True)
            Dim i As Integer = 1
            _rVal.Add(New dxoProperty(i, 1.0, "FeatureScale", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, bScalable:=True, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, "", "LayerName", dxxPropertyTypes.dxf_String, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, "", "TextStyleName", dxxPropertyTypes.dxf_String, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, "", "TitleTextStyle", dxxPropertyTypes.dxf_String, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, "", "FooterTextStyle", dxxPropertyTypes.dxf_String, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, "", "HeaderTextStyle", dxxPropertyTypes.dxf_String, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, "", "BlockName", dxxPropertyTypes.dxf_String, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, "", "Title", dxxPropertyTypes.dxf_String, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, "", "Footer", dxxPropertyTypes.dxf_String, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, "|", "Delimiter", dxxPropertyTypes.dxf_String, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, dxxColors.BlackWhite, "LayerColor", dxxPropertyTypes.Color, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, dxxColors.ByLayer, "BorderColor", dxxPropertyTypes.Color, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, dxxColors.ByLayer, "GridColor", dxxPropertyTypes.Color, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, dxxColors.ByLayer, "TextColor", dxxPropertyTypes.Color, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, dxxColors.Undefined, "HeaderTextColor", dxxPropertyTypes.Color, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, dxxColors.Undefined, "TitleTextColor", dxxPropertyTypes.Color, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, dxxColors.Undefined, "FooterTextColor", dxxPropertyTypes.Color, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, dxxTableGridStyles.All, "GridStyle", dxxPropertyTypes.dxf_Integer, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, dxxHorizontalJustifications.Left, "TitleAlignment", dxxPropertyTypes.dxf_Integer, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, dxxHorizontalJustifications.Left, "FooterAlignment", dxxPropertyTypes.dxf_Integer, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, dxxRectangularAlignments.TopLeft, "Alignment", dxxPropertyTypes.dxf_Integer, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, dxxRectangularAlignments.MiddleCenter, "CellAlignment", dxxPropertyTypes.dxf_Integer, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, 0, "HeaderRow", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, 0, "HeaderCol", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, 0, "RowCount", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, 0, "ColCount", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, 0, "GridLineThickness", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, 0, "BorderLineThickness", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, 0, "RotationAngle", dxxPropertyTypes.Angle, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, 0.2, "TextSize", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, 1.0, "HeaderTextScale", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, 1.0, "TitleTextScale", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, 1.0, "FooterTextScale", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, 0.05, "TextGap", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, 0, "ColumnGap", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, 0, "RowGap", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, 0, "HeaderWidthFactor", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, 0, "TextWidthFactor", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, 0, "TitleWidthFactor", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, 0, "FooterWidthFactor", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, False, "SuppressBorder", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, False, "SaveAsBlock", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, False, "SaveAttributes", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True)) : i += 1
            _rVal.Add(New dxoProperty(i, False, "SaveAsGroup", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True)) : i += 1
            _rVal.AddVector(i, TVECTOR.WorldZ, "Extrusion Direction", bIsDirection:=True) : i += 1
            _rVal.AddVector(i, TVECTOR.Zero, "Insertion Pt") : i += 1
            _rVal.Add(New dxoProperty(i, aGUID, "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True})
            Return _rVal
        End Function



        Private Shared _SETTINGS_SYMBOL As TPROPERTIES
        Friend Shared ReadOnly Property Props_Setting_SYMBOL As TPROPERTIES
            Get
                If _SETTINGS_SYMBOL.Count = 0 Then ' If _SETTINGS_SYMBOL.IsEmpty Then
                    _SETTINGS_SYMBOL = New TPROPERTIES("SymbolSettings", True)
                    _SETTINGS_SYMBOL.Add(New TPROPERTY(1, 1.0!, "FeatureScale", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.NonZero, bScalable:=True, bNonDXF:=True))

                    _SETTINGS_SYMBOL.Add(New TPROPERTY(2, 0.2!, "TextHeight", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.NonZero, bScalable:=True, bNonDXF:=True))
                    _SETTINGS_SYMBOL.Add(New TPROPERTY(3, 0.09!, "TextGap", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.NonZero, bScalable:=True, bNonDXF:=True))
                    _SETTINGS_SYMBOL.Add(New TPROPERTY(4, 0.18!, "ArrowSize", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.NonZero, bScalable:=True, bNonDXF:=True))
                    _SETTINGS_SYMBOL.Add(New TPROPERTY(5, "", "LayerName", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    _SETTINGS_SYMBOL.Add(New TPROPERTY(6, "", "TextStyle", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    _SETTINGS_SYMBOL.Add(New TPROPERTY(7, dxxColors.BlackWhite, "LayerColor", dxxPropertyTypes.Color, bNonDXF:=True))
                    _SETTINGS_SYMBOL.Add(New TPROPERTY(8, dxxColors.ByBlock, "LineColor", dxxPropertyTypes.Color, bNonDXF:=True))
                    _SETTINGS_SYMBOL.Add(New TPROPERTY(9, dxxColors.ByBlock, "TextColor", dxxPropertyTypes.Color, bNonDXF:=True))
                    _SETTINGS_SYMBOL.Add(New TPROPERTY(10, dxxRectangularAlignments.TopLeft, "ArrowTextAlignment", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.PositiveNonZero, bNonDXF:=True))
                    _SETTINGS_SYMBOL.Add(New TPROPERTY(11, dxxArrowHeadTypes.ClosedFilled, "ArrowHead", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))
                    _SETTINGS_SYMBOL.Add(New TPROPERTY(12, dxxArrowStyles.AngledHalf, "ArrowStyle", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))
                    _SETTINGS_SYMBOL.Add(New TPROPERTY(13, dxxArrowTails.Undefined, "ArrowTails", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))
                    _SETTINGS_SYMBOL.Add(New TPROPERTY(14, 0, "AxisStyle", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))
                    _SETTINGS_SYMBOL.Add(New TPROPERTY(15, False, "BoxText", dxxPropertyTypes.Switch, bNonDXF:=True))
                End If
                Return New TPROPERTIES(_SETTINGS_SYMBOL)
            End Get
        End Property
        Private Shared _SETTINGS_SCREEN As TPROPERTIES
        Friend Shared ReadOnly Property Props_Setting_SCREEN As TPROPERTIES
            Get
                '^the default properties for a screen settings object
                If _SETTINGS_SCREEN.Count = 0 Then ' If _SETTINGS_SCREEN.IsEmpty Then
                    _SETTINGS_SCREEN = New TPROPERTIES("ScreenSettings", True)
                    _SETTINGS_SCREEN.Add(New TPROPERTY(1, False, "Suppressed", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _SETTINGS_SCREEN.Add(New TPROPERTY(2, False, "BoundingRectangles", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _SETTINGS_SCREEN.Add(New TPROPERTY(3, False, "OCSs", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _SETTINGS_SCREEN.Add(New TPROPERTY(4, False, "ExtentPts", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _SETTINGS_SCREEN.Add(New TPROPERTY(5, False, "ExtentRectangle", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _SETTINGS_SCREEN.Add(New TPROPERTY(6, False, "TextBoxes", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True))
                    _SETTINGS_SCREEN.Add(New TPROPERTY(7, dxxColors.Blue, "EntitySymbolColor", dxxPropertyTypes.Color, bNonDXF:=True))
                    _SETTINGS_SCREEN.Add(New TPROPERTY(8, dxxColors.Blue, "TextColor", dxxPropertyTypes.Color, bNonDXF:=True))
                    _SETTINGS_SCREEN.Add(New TPROPERTY(9, dxxColors.Blue, "AxisColor", dxxPropertyTypes.Color, bNonDXF:=True))
                    _SETTINGS_SCREEN.Add(New TPROPERTY(10, dxxColors.Blue, "RectangleColor", dxxPropertyTypes.Color, bNonDXF:=True))
                    _SETTINGS_SCREEN.Add(New TPROPERTY(11, dxxColors.Grey, "ExtentRectangleColor", dxxPropertyTypes.Color, bNonDXF:=True))
                    _SETTINGS_SCREEN.Add(New TPROPERTY(12, dxxColors.Blue, "PointColor", dxxPropertyTypes.Color, bNonDXF:=True))
                    _SETTINGS_SCREEN.Add(New TPROPERTY(13, 0.05, "OCSSize", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, bNonDXF:=True))
                    _SETTINGS_SCREEN.Add(New TPROPERTY(14, 0.075, "TextSize", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, bNonDXF:=True))
                    _SETTINGS_SCREEN.Add(New TPROPERTY(15, 4, "PointSize", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.PositiveNonZero, bNonDXF:=True))
                    _SETTINGS_SCREEN.Add(New TPROPERTY(16, dxxColors.Blue, "CircleColor", dxxPropertyTypes.Color, bNonDXF:=True))
                    _SETTINGS_SCREEN.Add(New TPROPERTY(17, dxxColors.Blue, "PointerColor", dxxPropertyTypes.Color, bNonDXF:=True))
                    _SETTINGS_SCREEN.Add(New TPROPERTY(18, dxxColors.Blue, "LineColor", dxxPropertyTypes.Color, bNonDXF:=True))
                    _SETTINGS_SCREEN.Add(New TPROPERTY(19, dxfLinetypes.Continuous, "LineType", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    _SETTINGS_SCREEN.Add(New TPROPERTY(20, 1.0, "LTScale", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, bNonDXF:=True))

                End If
                Return New TPROPERTIES(_SETTINGS_SCREEN)
            End Get
        End Property
        Private Shared _SETTINGS_DIM As TPROPERTIES
        Friend Shared ReadOnly Property Props_Setting_DIM As TPROPERTIES
            Get

                If _SETTINGS_DIM.Count = 0 Then ' If _SETTINGS_DIM.IsEmpty Then
                    _SETTINGS_DIM = New TPROPERTIES("DimSettings", True)
                    _SETTINGS_DIM.Add(New TPROPERTY(1, dxxDrawingUnits.English, "DrawingUnits", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))

                    _SETTINGS_DIM.Add(New TPROPERTY(2, "", "LeaderLayer", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    _SETTINGS_DIM.Add(New TPROPERTY(3, dxxColors.Undefined, "LeaderLayerColor", dxxPropertyTypes.Color, bNonDXF:=True))
                    _SETTINGS_DIM.Add(New TPROPERTY(4, "", "DimLayer", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    _SETTINGS_DIM.Add(New TPROPERTY(5, dxxColors.Undefined, "DimLayerColor", dxxPropertyTypes.Color, bNonDXF:=True))
                    _SETTINGS_DIM.Add(New TPROPERTY(6, 0.05, "DimTickLength", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bNonDXF:=True))
                    _SETTINGS_DIM.Add(New TPROPERTY(7, 1.0, "DrawingScale", dxxPropertyTypes.dxf_Double,, aValControl:=mzValueControls.PositiveNonZero, bNonDXF:=True))
                    _SETTINGS_DIM.Add(New TPROPERTY(8, dxxVerticalJustifications.Center, "LeaderTextJustification", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))
                End If
                Return New TPROPERTIES(_SETTINGS_DIM)
            End Get
        End Property

        Private Shared _SETTINGS_TABLE As TPROPERTIES
        Friend Shared ReadOnly Property Props_Setting_TABLE As TPROPERTIES
            Get
                '^the default properties for a dxeTable
                If _SETTINGS_TABLE.Count = 0 Then ' If _SETTINGS_TABLE.IsEmpty Then
                    _SETTINGS_TABLE = New TPROPERTIES("TableSettings", True)
                    Dim i As Integer = _SETTINGS_TABLE.Add(New TPROPERTY(1, 1.0, "FeatureScale", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, bScalable:=True, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, "", "LayerName", dxxPropertyTypes.dxf_String, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, "", "TextStyleName", dxxPropertyTypes.dxf_String, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, "", "TitleTextStyle", dxxPropertyTypes.dxf_String, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, "", "FooterTextStyle", dxxPropertyTypes.dxf_String, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, "", "HeaderTextStyle", dxxPropertyTypes.dxf_String, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, "", "BlockName", dxxPropertyTypes.dxf_String, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, "", "Title", dxxPropertyTypes.dxf_String, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, "", "Footer", dxxPropertyTypes.dxf_String, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, "|", "Delimiter", dxxPropertyTypes.dxf_String, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, dxxColors.BlackWhite, "LayerColor", dxxPropertyTypes.Color, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, dxxColors.ByLayer, "BorderColor", dxxPropertyTypes.Color, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, dxxColors.ByLayer, "GridColor", dxxPropertyTypes.Color, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, dxxColors.ByLayer, "TextColor", dxxPropertyTypes.Color, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, dxxColors.Undefined, "HeaderTextColor", dxxPropertyTypes.Color, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, dxxColors.Undefined, "TitleTextColor", dxxPropertyTypes.Color, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, dxxColors.Undefined, "FooterTextColor", dxxPropertyTypes.Color, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, dxxTableGridStyles.All, "GridStyle", dxxPropertyTypes.dxf_Integer, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, dxxHorizontalJustifications.Left, "TitleAlignment", dxxPropertyTypes.dxf_Integer, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, dxxHorizontalJustifications.Left, "FooterAlignment", dxxPropertyTypes.dxf_Integer, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, dxxRectangularAlignments.TopLeft, "Alignment", dxxPropertyTypes.dxf_Integer, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, dxxRectangularAlignments.MiddleCenter, "CellAlignment", dxxPropertyTypes.dxf_Integer, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, 0, "HeaderRow", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, 0, "HeaderCol", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, 0, "RowCount", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, 0, "ColCount", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, 0, "GridLineThickness", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, 0, "BorderLineThickness", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, 0, "RotationAngle", dxxPropertyTypes.Angle, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, 0.2, "TextSize", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, 1.0, "HeaderTextScale", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, 1.0, "TitleTextScale", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, 1.0, "FooterTextScale", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, 0.05, "TextGap", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, 0, "ColumnGap", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, 0, "RowGap", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, 0, "HeaderWidthFactor", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, 0, "TextWidthFactor", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, 0, "TitleWidthFactor", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, 0, "FooterWidthFactor", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, False, "SuppressBorder", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, False, "SaveAsBlock", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, False, "SaveAttributes", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.Add(New TPROPERTY(i, False, "SaveAsGroup", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True)).Index + 1
                    i = _SETTINGS_TABLE.AddVector(i, TVECTOR.WorldZ, "Extrusion Direction", bIsDirection:=True).Index + 1
                    i = _SETTINGS_TABLE.AddVector(i, TVECTOR.Zero, "Insertion Pt").Index + 1
                End If
                Return New TPROPERTIES(_SETTINGS_TABLE)
            End Get
        End Property

        Private Shared _SETTINGS_TEXT As TPROPERTIES
        Friend Shared ReadOnly Property Props_Setting_TEXT As TPROPERTIES
            Get
                If _SETTINGS_TEXT.Count = 0 Then ' If _SETTINGS_TEXT.IsEmpty Then
                    _SETTINGS_TEXT = New TPROPERTIES("TextSettings", True)
                    _SETTINGS_TEXT.Add(New TPROPERTY(1, "", "LayerName", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    _SETTINGS_TEXT.Add(New TPROPERTY(2, dxxColors.Undefined, "LayerColor", dxxPropertyTypes.Color, bNonDXF:=True))
                    _SETTINGS_TEXT.Add(New TPROPERTY(3, dxxColors.Undefined, "Color", dxxPropertyTypes.Color, bNonDXF:=True))
                    _SETTINGS_TEXT.Add(New TPROPERTY(4, dxxLineWeights.ByDefault, "LineWeight", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))
                End If
                Return New TPROPERTIES(_SETTINGS_TEXT)
            End Get
        End Property





        Private Shared _SYMBOL As TPROPERTIES
        Friend Shared ReadOnly Property Props_SYMBOL As TPROPERTIES
            Get

                If _SYMBOL.Count = 0 Then ' If _SYMBOL.IsEmpty Then
                    _SYMBOL = Get_CommonProps("SYMBOL")
                    _SYMBOL.Add(New TPROPERTY(1, "", "Text1", dxxPropertyTypes.dxf_String, bSetSuppressedValue:=True))
                    _SYMBOL.Add(New TPROPERTY(2, "", "Text2", dxxPropertyTypes.dxf_String, bSetSuppressedValue:=True))
                    _SYMBOL.Add(New TPROPERTY(3, "", "Text3", dxxPropertyTypes.dxf_String, bSetSuppressedValue:=True))
                    _SYMBOL.Add(New TPROPERTY(4, "", "Text4", dxxPropertyTypes.dxf_String, bSetSuppressedValue:=True))
                    _SYMBOL.Add(New TPROPERTY(5, False, "Flag1", dxxPropertyTypes.Switch, bSetSuppressedValue:=True))
                    _SYMBOL.Add(New TPROPERTY(6, False, "Flag2", dxxPropertyTypes.Switch, bSetSuppressedValue:=True))
                    _SYMBOL.Add(New TPROPERTY(7, False, "Flag3", dxxPropertyTypes.Switch, bSetSuppressedValue:=True))
                    _SYMBOL.Add(New TPROPERTY(8, 0, "Height", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bScalable:=True))
                    _SYMBOL.Add(New TPROPERTY(9, 0, "Length", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bScalable:=True))
                    _SYMBOL.Add(New TPROPERTY(10, 0, "Rotation", dxxPropertyTypes.Angle, bSetSuppressedValue:=True))
                    _SYMBOL.Add(New TPROPERTY(11, 0, "Span", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bScalable:=True))
                    _SYMBOL.Add(New TPROPERTY(12, "", "BlockName", dxxPropertyTypes.dxf_String))
                    _SYMBOL.Add(New TPROPERTY(13, dxxBubbleTypes.Circular, "BubbleType", dxxPropertyTypes.dxf_Integer))
                    _SYMBOL.Add(New TPROPERTY(14, dxxSymbolTypes.Undefined, "SymbolType", dxxPropertyTypes.dxf_Integer))
                    _SYMBOL.Add(New TPROPERTY(15, dxxWeldTypes.Fillet, "WeldType", dxxPropertyTypes.dxf_Integer))
                    _SYMBOL.Add(New TPROPERTY(16, dxxArrowTypes.Pointer, "ArrowType", dxxPropertyTypes.dxf_Integer))
                    _SYMBOL.AddVector(17, TVECTOR.WorldZ, "Extrusion Direction", bIsDirection:=True)
                    _SYMBOL.AddVector(18, TVECTOR.Zero, "Insertion Pt")
                    Dim settings As TPROPERTIES = GetReferenceProps(dxxSettingTypes.SYMBOLSETTINGS)
                    _SYMBOL.Append(settings, bExcludeHidden:=True, aSkipList:="LayerName")
                    _SYMBOL.SetVal("LayerName", "0")
                    _SYMBOL.SetVal("TextStyle", "Standard")
                    _SYMBOL.ReIndex(True)
                End If
                Return New TPROPERTIES(_SYMBOL)
            End Get
        End Property

        Private Shared _TABLE As TPROPERTIES
        Friend Shared ReadOnly Property Props_TABLE As TPROPERTIES
            Get
                If _TABLE.Count = 0 Then
                    _TABLE = Get_CommonProps("TABLE")
                    _TABLE.Append(New TTABLEENTRY(dxxSettingTypes.TABLESETTINGS).Props)
                    _TABLE.ReIndex(True)
                End If
                Return New TPROPERTIES(_TABLE)
            End Get
        End Property
        Private Shared _VIEWPORT As TPROPERTIES

        Friend Shared ReadOnly Property Props_VIEWPORT As TPROPERTIES
            Get
                If _VIEWPORT.Count = 0 Then ' If _VIEWPORT.IsEmpty Then
                    _VIEWPORT = Get_CommonProps("VIEWPORT")
                    _VIEWPORT.Add(New TPROPERTY(100, "AcDbViewport", "Entity Sub Class Marker_1", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _VIEWPORT.AddVector(10, TVECTOR.Zero, "Center")
                    _VIEWPORT.Add(New TPROPERTY(40, 0, "Width In paper space units", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                    _VIEWPORT.Add(New TPROPERTY(41, 0, "Height In paper space units", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                    _VIEWPORT.Add(New TPROPERTY(68, 1, "Viewport status field", dxxPropertyTypes.dxf_Integer))
                    _VIEWPORT.Add(New TPROPERTY(69, 1, "Viewport ID", dxxPropertyTypes.dxf_Integer))
                    _VIEWPORT.AddVector(12, TVECTOR.Zero, "View center point(In DCS)", bTwoD:=True)
                    _VIEWPORT.AddVector(13, TVECTOR.Zero, "Snap base point", bTwoD:=True)
                    _VIEWPORT.AddVector(14, TVECTOR.Zero, "Snap Spacing", bTwoD:=True)
                    _VIEWPORT.AddVector(15, TVECTOR.Zero, "Grid Spacing", bTwoD:=True)
                    _VIEWPORT.AddVector(16, TVECTOR.WorldZ, "View direction vector(In WCS)", bIsDirection:=True)
                    _VIEWPORT.AddVector(17, TVECTOR.Zero, "View target point(In WCS)")
                    _VIEWPORT.Add(New TPROPERTY(42, 0, "Perspective lens length", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                    _VIEWPORT.Add(New TPROPERTY(43, 0, "Front clip plane Z value", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                    _VIEWPORT.Add(New TPROPERTY(44, 0, "Back clip plane Z value", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                    _VIEWPORT.Add(New TPROPERTY(45, 0, "View height(In model space units)", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                    _VIEWPORT.Add(New TPROPERTY(50, 0, "Snap angle", dxxPropertyTypes.Angle))
                    _VIEWPORT.Add(New TPROPERTY(51, 0, "View twist angle", dxxPropertyTypes.Angle))
                    _VIEWPORT.Add(New TPROPERTY(72, 1000, "Circle zoom percent", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                    _VIEWPORT.Add(New TPROPERTY(90, 819232, "Viewport status bit-coded flags", dxxPropertyTypes.dxf_Integer))
                    _VIEWPORT.Add(New TPROPERTY(340, "0", "Hard-pointer ID/handle To entity that serves As the viewport's clipping boundary", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _VIEWPORT.Add(New TPROPERTY(1, "", "Plot style sheet name assigned to this viewport", dxxPropertyTypes.dxf_String))
                    _VIEWPORT.Add(New TPROPERTY(281, dxxRenderModes.TwoDOptimized, "Render mode", dxxPropertyTypes.dxf_Integer))
                    _VIEWPORT.Add(New TPROPERTY(71, 1, "UCS per viewport flag", dxxPropertyTypes.Switch))
                    _VIEWPORT.Add(New TPROPERTY(74, 0, "Display UCS icon at UCS origin flag:", dxxPropertyTypes.Switch, bSetSuppressedValue:=True))
                    _VIEWPORT.AddVector(110, TVECTOR.Zero, "UCS Origin")
                    _VIEWPORT.AddVector(111, TVECTOR.WorldX, "UCS X-Axis", bIsDirection:=True)
                    _VIEWPORT.AddVector(112, TVECTOR.WorldY, "UCS Y-Axis", bIsDirection:=True)
                    _VIEWPORT.Add(New TPROPERTY(345, "0", "ID/handle of AcDbUCSTableRecord if UCS is a named UCS", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _VIEWPORT.Add(New TPROPERTY(346, "0", "ID/handle of AcDbUCSTableRecord of base UCS if UCS is orthographic", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _VIEWPORT.Add(New TPROPERTY(79, dxxOrthoGraphicTypes.NonOrthographic, "Orthographic type of UCS", dxxPropertyTypes.dxf_Integer))
                    _VIEWPORT.Add(New TPROPERTY(146, 0, "Elevation", dxxPropertyTypes.dxf_Double))
                    _VIEWPORT.Add(New TPROPERTY(170, dxxShadePlotModes.AsDisplayed, "ShadePlot mode", dxxPropertyTypes.dxf_Integer))
                    _VIEWPORT.Add(New TPROPERTY(61, 5, "Frequency of major grid lines compared to minor grid lines", dxxPropertyTypes.Undefined))
                    _VIEWPORT.Add(New TPROPERTY(332, "0", "Background ID/Handle", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _VIEWPORT.Add(New TPROPERTY(333, "0", "Shade plot ID/Handle", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _VIEWPORT.Add(New TPROPERTY(348, "0", "Visual style ID/Handle", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _VIEWPORT.Add(New TPROPERTY(292, 1, "Default lighting flag", dxxPropertyTypes.Switch))
                    _VIEWPORT.Add(New TPROPERTY(282, dxxLightingTypes.TwoDistant, "Default lighting type", dxxPropertyTypes.dxf_Integer))
                    _VIEWPORT.Add(New TPROPERTY(141, 0, "View brightness", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                    _VIEWPORT.Add(New TPROPERTY(142, 0, "View contrast", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                    _VIEWPORT.Add(New TPROPERTY(63, 250, "Ambient light color 1", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive, aSuppressedVal:=0))
                    _VIEWPORT.Add(New TPROPERTY(421, 3355443, "Ambient light color 2", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive, aSuppressedVal:=0))
                    _VIEWPORT.Add(New TPROPERTY(431, 0, "Ambient light color 3", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive, aSuppressedVal:=0))
                    _VIEWPORT.Add(New TPROPERTY(361, "0", "Sun ID/Handle", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _VIEWPORT.Add(New TPROPERTY(335, "0", "Soft pointer reference to viewport object", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _VIEWPORT.Add(New TPROPERTY(343, "0", "Soft pointer reference to viewport object", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _VIEWPORT.Add(New TPROPERTY(344, "0", "Soft pointer reference to viewport object", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _VIEWPORT.Add(New TPROPERTY(91, "0", "Soft pointer reference to viewport object", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _VIEWPORT.ReIndex(True)
                End If
                Return New TPROPERTIES(_VIEWPORT)
            End Get
        End Property

        Private Shared _ENTRY_APPID As TPROPERTIES
        Friend Shared ReadOnly Property Props_Entry_APPID As TPROPERTIES
            Get
                '^the default properties for an APPID
                Try
                    If _ENTRY_APPID.Count = 0 Then
                        _ENTRY_APPID = New TPROPERTIES(dxfEnums.DisplayName(dxxReferenceTypes.APPID).ToUpper)
                        _ENTRY_APPID.Add(New TPROPERTY(0, "APPID", "Table Entry Type", dxxPropertyTypes.dxf_String) With {.DoNotCopy = True})
                        _ENTRY_APPID.Add(New TPROPERTY(5, "", "Handle", dxxPropertyTypes.Handle))
                        _ENTRY_APPID.Add(New TPROPERTY(330, "", "Table Handle", dxxPropertyTypes.Pointer))
                        _ENTRY_APPID.Add(New TPROPERTY(100, "AcDbSymbolTableRecord", "Table Record SubClass Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                        _ENTRY_APPID.Add(New TPROPERTY(100, "AcDbRegAppTableRecord", "Sub Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                        _ENTRY_APPID.Add(New TPROPERTY(2, "", "Name", dxxPropertyTypes.dxf_String))
                        _ENTRY_APPID.Add(New TPROPERTY(70, 0, "Bitflag", dxxPropertyTypes.BitCode))
                        _ENTRY_APPID.Add(New TPROPERTY(0, "", "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True})
                    End If

                Catch ex As Exception
                    Throw ex
                End Try
                Return New TPROPERTIES(_ENTRY_APPID)
            End Get
        End Property


        Friend Shared Function Properties_Entry_APPID(Optional aName As String = "", Optional aGUID As String = "") As dxoProperties
            Try

                Return New dxoProperties(dxfEnums.DisplayName(dxxReferenceTypes.APPID).ToUpper) From
                 {
                    New dxoProperty(0, "APPID", "Table Entry Type", dxxPropertyTypes.dxf_String) With {.DoNotCopy = True},
                    New dxoProperty(5, "", "Handle", dxxPropertyTypes.Handle),
                    New dxoProperty(330, "", "Table Handle", dxxPropertyTypes.Pointer),
                    New dxoProperty(100, "AcDbSymbolTableRecord", "Table Record SubClass Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True},
                    New dxoProperty(100, "AcDbRegAppTableRecord", "Sub Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True},
                    New dxoProperty(2, aName, "Name", dxxPropertyTypes.dxf_String),
                    New dxoProperty(70, 0, "Bitflag", dxxPropertyTypes.BitCode),
                    New dxoProperty(0, aGUID, "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True}
                }

            Catch ex As Exception
                Return New dxoProperties(dxfEnums.DisplayName(dxxReferenceTypes.APPID).ToUpper)
            End Try

        End Function
        Friend Shared Function Properties_Entry_BLOCK_RECORD(Optional aName As String = "", Optional aGUID As String = "") As dxoProperties

            '^the default properties for a BLOCK_RECORD

            Try

                Return New dxoProperties(dxfEnums.DisplayName(dxxReferenceTypes.BLOCK_RECORD).ToUpper) From
                 {
                New dxoProperty(0, "BLOCK_RECORD", "Table Entry Type", dxxPropertyTypes.dxf_String) With {.DoNotCopy = True},
                New dxoProperty(5, "", "Handle", dxxPropertyTypes.Handle),
               New dxoProperty(330, "", "Table Handle", dxxPropertyTypes.Pointer),
                New dxoProperty(100, "AcDbSymbolTableRecord", "Table Record SubClass Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True},
                New dxoProperty(100, "AcDbBlockTableRecord", "Sub Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True},
                New dxoProperty(2, aName, "Name", dxxPropertyTypes.dxf_String),
                New dxoProperty(340, "0", "Layout Handle", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True),
                New dxoProperty(70, dxxInsertUnits.Unitless, "Insert Units", dxxPropertyTypes.dxf_Integer),
                New dxoProperty(280, True, "Explodable", dxxPropertyTypes.Switch),
                New dxoProperty(281, False, "UniformScale", dxxPropertyTypes.Switch),
                New dxoProperty(1, aGUID, "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True},
                New dxoProperty(2, "", "*LayoutName", dxxPropertyTypes.dxf_String, bNonDXF:=True)
                }


            Catch ex As Exception
                Return New dxoProperties(dxfEnums.DisplayName(dxxReferenceTypes.BLOCK_RECORD).ToUpper)
            End Try

        End Function
        Friend Shared Function Properties_Entry_DIMSTYLE(Optional aName As String = "", Optional aGUID As String = "") As dxoProperties

            '^the default properties for a DIMSTYLE
            'dimstyles have r14 prperties to begin the non applicable
            'properties are turned off at dxfwrite time for R12
            Dim _rVal As dxoProperties = Nothing
            If String.IsNullOrWhiteSpace(aName) Then aName = "Standard"
            Try
                _rVal = New dxoProperties(dxfEnums.DisplayName(dxxReferenceTypes.DIMSTYLE).ToUpper) From
                 {
                    New dxoProperty(0, "DIMSTYLE", "Table Entry Type", dxxPropertyTypes.dxf_String) With {.DoNotCopy = True},
                    New dxoProperty(105, "", "Handle", dxxPropertyTypes.Handle),
                    New dxoProperty(330, "", "Table Handle", dxxPropertyTypes.Pointer),
                    New dxoProperty(100, "AcDbSymbolTableRecord", "Table Record SubClass Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True},
                    New dxoProperty(100, "AcDbDimStyleTableRecord", "Sub Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True},
                    New dxoProperty(2, aName, "Name", dxxPropertyTypes.dxf_String),
                    New dxoProperty(70, 0, "Bit Code", dxxPropertyTypes.BitCode)
              }
                _rVal.Append(dxpProperties.DimStyleProps, bAddClones:=True)
                _rVal.Add(New TPROPERTY(1, aGUID, "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True})
                _rVal.ReIndex(True)

                Return _rVal
            Catch ex As Exception
                Return _rVal
            End Try

        End Function

        Private Shared _DimStyleProps As dxoProperties
        Friend Shared ReadOnly Property DimStyleProps As dxoProperties
            Get
                If _DimStyleProps Is Nothing Then
                    _DimStyleProps = New dxoProperties("DIMSTYLEPROPS")
                    Dim enumVals As Dictionary(Of String, Integer) = dxfEnums.EnumValues(GetType(dxxDimStyleProperties))


                    For Each nm As String In enumVals.Keys
                        Dim ival As dxxDimStyleProperties = enumVals.Item(nm)
                        If ival >= dxxDimStyleProperties.DIMPOST Then

                            _DimStyleProps.Add(dxpProperties.DimStyleProperty(ival))
                        End If
                    Next

                End If

                Return _DimStyleProps

            End Get
        End Property

        Friend Shared Function Properties_Entry_LAYER(Optional aName As String = "", Optional aGUID As String = "") As dxoProperties

            '^the default properties for a LAYER
            Try

                If String.IsNullOrWhiteSpace(aName) Then aName = "0"
                Return New dxoProperties(dxfEnums.DisplayName(dxxReferenceTypes.LAYER).ToUpper) From
                {
                    New dxoProperty(0, "LAYER", "Table Entry Type", dxxPropertyTypes.dxf_String) With {.DoNotCopy = True},
                    New dxoProperty(5, "", "Handle", dxxPropertyTypes.Handle),
                    New dxoProperty(330, "", "Table Handle", dxxPropertyTypes.Pointer),
                    New dxoProperty(100, "AcDbSymbolTableRecord", "Table Record SubClass Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True},
                    New dxoProperty(100, "AcDbLayerTableRecord", "Sub Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True},
                    New dxoProperty(2, aName, "Name", dxxPropertyTypes.dxf_String),
                    New dxoProperty(70, 0, "Bit Code", dxxPropertyTypes.BitCode),
                    New dxoProperty(62, dxxColors.BlackWhite, "Color", dxxPropertyTypes.Color),
                    New dxoProperty(6, dxfLinetypes.Continuous, "Linetype", dxxPropertyTypes.dxf_String),
                    New dxoProperty(290, 1, "Plot Flag", dxxPropertyTypes.Switch, bSetSuppressedValue:=True),
                    New dxoProperty(370, dxxLineWeights.ByDefault, "LineWeight", dxxPropertyTypes.dxf_Integer),
                    New dxoProperty(390, "0", "Plot Style Pointer", dxxPropertyTypes.Pointer),
                    New dxoProperty(347, "0", "Material Pointer", dxxPropertyTypes.Pointer),
                    New dxoProperty(1, aGUID, "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True},
                    New dxoProperty(2, 0, "*Transparency", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive, bNonDXF:=True),
                    New dxoProperty(0, "", "*Description", dxxPropertyTypes.dxf_String, bNonDXF:=True)
                    }


            Catch ex As Exception
                Return New dxoProperties(dxfEnums.DisplayName(dxxReferenceTypes.LAYER).ToUpper)
            End Try
        End Function


        Friend Shared Function Properties_Entry_LTYPE(Optional aName As String = "", Optional aGUID As String = "", Optional aDescription As String = "") As dxoProperties


            Try
                Return New dxoProperties(dxfEnums.DisplayName(dxxReferenceTypes.LTYPE).ToUpper) From
                    {
                           New dxoProperty(0, "LTYPE", "Table Entry Type", dxxPropertyTypes.dxf_String) With {.DoNotCopy = True},
                New dxoProperty(5, "", "Handle", dxxPropertyTypes.Handle),
                New dxoProperty(330, "", "Table Handle", dxxPropertyTypes.Pointer),
                New dxoProperty(100, "AcDbSymbolTableRecord", "Table Record SubClass Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True},
                New dxoProperty(100, "AcDbLinetypeTableRecord", "Sub Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True},
                New dxoProperty(2, aName, "Name", dxxPropertyTypes.dxf_String),
                New dxoProperty(70, 0, "Bit Code", dxxPropertyTypes.BitCode),
                New dxoProperty(3, aDescription, "Description", dxxPropertyTypes.dxf_String),
                New dxoProperty(72, 65, "Alignment Code", dxxPropertyTypes.dxf_Integer),
                New dxoProperty(73, 0, "Element Count", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive),
                New dxoProperty(40, 0, "Pattern Length", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive),
                New dxoProperty(1, aGUID, "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True}
                    }


            Catch ex As Exception
                Return New dxoProperties(dxfEnums.DisplayName(dxxReferenceTypes.LTYPE).ToUpper)
            End Try

        End Function
        Friend Shared Function Properties_Entry_STYLE(Optional aName As String = "", Optional aGUID As String = "") As dxoProperties

            '^the default properties for an STYLE
            Try

                Dim font As dxoFont = dxfGlobals.GetFont("Arial.ttf", "txt.shx", bAttemptLoadIfNotFound:=True)
                If String.IsNullOrWhiteSpace(aName) Then aName = "Standard"
                Return New dxoProperties(dxfEnums.DisplayName(dxxReferenceTypes.STYLE).ToUpper) From
                 {
                     New dxoProperty(0, "STYLE", "Table Entry Type", dxxPropertyTypes.dxf_String) With {.DoNotCopy = True},
                New dxoProperty(5, "", "Handle", dxxPropertyTypes.Handle),
                New dxoProperty(330, "", "Table Handle", dxxPropertyTypes.Pointer),
                New dxoProperty(100, "AcDbSymbolTableRecord", "Table Record SubClass Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True},
                New dxoProperty(100, "AcDbTextStyleTableRecord", "Sub Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True},
                New dxoProperty(2, aName, "Name", dxxPropertyTypes.dxf_String, aEnumName:=dxxStyleProperties.NAME),
                New dxoProperty(70, 0, "Bit Code", dxxPropertyTypes.BitCode),
                New dxoProperty(40, 0, "Text Height", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, aEnumName:=dxxStyleProperties.TEXTHT),
                New dxoProperty(41, 1.0, "Width Factor", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, aEnumName:=dxxStyleProperties.WIDTHFACTOR),
                New dxoProperty(50, 0, "Oblique Angle", dxxPropertyTypes.Angle, aEnumName:=dxxStyleProperties.OBLIQUE),
                New dxoProperty(71, 0, "Generation Flag", dxxPropertyTypes.BitCode, aEnumName:=dxxStyleProperties.GENFLAG),
                New dxoProperty(42, 0, "Last Height", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, aEnumName:=dxxStyleProperties.LASTHT),
                New dxoProperty(3, font.Name, "Font", dxxPropertyTypes.dxf_String, aEnumName:=dxxStyleProperties.FONTNAME),
                New dxoProperty(4, "", "Big Font File", dxxPropertyTypes.dxf_String, aEnumName:=dxxStyleProperties.BIGFONT),
                New dxoProperty(1, aGUID, "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True},
                New dxoProperty(2, font.DefaultStyle.StyleName, "*FontStyle", dxxPropertyTypes.dxf_String, bNonDXF:=True, aEnumName:=dxxStyleProperties.FONTSTYLE),
                New dxoProperty(3, font.DefaultStyle.TTFStyle, "*FontStyleType", dxxPropertyTypes.dxf_Integer, bNonDXF:=True, aEnumName:=dxxStyleProperties.FONTSTYLETYPE),
                New dxoProperty(4, True, "*IsShape", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True, aEnumName:=dxxStyleProperties.SHAPEFLAG),
                New dxoProperty(5, 1.0, "*LineSpacingFactor", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, bNonDXF:=True, aEnumName:=dxxStyleProperties.LINESPACING),
                New dxoProperty(6, dxxLineSpacingStyles.AtLeast, "*LineSpacingStyle", dxxPropertyTypes.dxf_Integer, bNonDXF:=True, aEnumName:=dxxStyleProperties.LINESPACINGSTYLE),
                New dxoProperty(7, font.Index, "*FontIndex", dxxPropertyTypes.dxf_Integer, bNonDXF:=True, aEnumName:=dxxStyleProperties.FONTINDEX),
                New dxoProperty(8, font.Style(1).FileName, "*FontFileName", dxxPropertyTypes.dxf_String, bNonDXF:=True, aEnumName:=dxxStyleProperties.FONTNAME),
                New dxoProperty(9, False, "*Backwards", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True, aEnumName:=dxxStyleProperties.BACKWARDS),
                New dxoProperty(10, False, "*Upsidedown", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True, aEnumName:=dxxStyleProperties.UPSIDEDOWN),
                New dxoProperty(11, False, "*Vertical", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True, aEnumName:=dxxStyleProperties.VERTICAL),
                New dxoProperty(12, False, "*XRefResolved", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True, aEnumName:=dxxStyleProperties.XREFRESOLVED),
                New dxoProperty(13, False, "*XRefDependant", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True, aEnumName:=dxxStyleProperties.XREFDEPENANT),
                New dxoProperty(14, False, "*IsReferenced", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True, aEnumName:=dxxStyleProperties.ISREFERENCED)
                }

            Catch ex As Exception
                Return New dxoProperties(dxfEnums.DisplayName(dxxReferenceTypes.STYLE).ToUpper)
            End Try

        End Function
        Friend Shared Function Properties_Entry_UCS(Optional aName As String = "", Optional aGUID As String = "") As dxoProperties

            If String.IsNullOrWhiteSpace(aName) Then aName = "World"
            '^the default properties for a UCS
            Dim _rVal As dxoProperties = Nothing
            Try
                _rVal = New dxoProperties(dxfEnums.DisplayName(dxxReferenceTypes.UCS).ToUpper) From
                 {
                    New dxoProperty(0, "UCS", "Table Entry Type", dxxPropertyTypes.dxf_String) With {.DoNotCopy = True},
                    New dxoProperty(5, "", "Handle", dxxPropertyTypes.Handle),
                    New dxoProperty(330, "", "Table Handle", dxxPropertyTypes.Pointer),
                    New dxoProperty(100, "AcDbSymbolTableRecord", "Table Record SubClass Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True},
                    New dxoProperty(100, "AcDbUCSTableRecord", "Sub Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True},
                    New dxoProperty(2, aName, "Name", dxxPropertyTypes.dxf_String),
                    New dxoProperty(70, 0, "Bit Code", dxxPropertyTypes.BitCode)
                }
                _rVal.AddVector(10, TVECTOR.Zero, "Origin")
                _rVal.AddVector(11, TVECTOR.WorldX, "X Axis Direction", bIsDirection:=True)
                _rVal.AddVector(12, TVECTOR.WorldY, "Y Axis Direction", bIsDirection:=True)
                _rVal.Add(New dxoProperty(79, 0, "Dummy(Always 0)", dxxPropertyTypes.dxf_Integer))
                _rVal.Add(New dxoProperty(146, 0, "Elevation", dxxPropertyTypes.dxf_Double))
                _rVal.Add(New dxoProperty(346, "0", "Base UCS Handle", dxxPropertyTypes.Pointer))
                _rVal.Add(New dxoProperty(71, dxxOrthoGraphicTypes.NonOrthographic, "Orthographic Type", dxxPropertyTypes.dxf_Integer))
                _rVal.AddVector(13, TVECTOR.Zero, "Relative Origin")
                _rVal.Add(New dxoProperty(1, aGUID, "*GUID", dxxPropertyTypes.dxf_String) With {.DoNotCopy = True})

                Return _rVal

            Catch ex As Exception
                Return _rVal
            End Try
        End Function

        Friend Shared Function Properties_Entry_VIEW(Optional aName As String = "", Optional aGUID As String = "") As dxoProperties
            Dim _rVal As dxoProperties = Nothing
            If String.IsNullOrWhiteSpace(aName) Then aName = "Top"
            '^the default properties for a VIEW
            Try
                _rVal = New dxoProperties(dxfEnums.DisplayName(dxxReferenceTypes.VIEW).ToUpper) From
                    {
                    New dxoProperty(0, "VIEW", "Table Entry Type", dxxPropertyTypes.dxf_String) With {.DoNotCopy = True},
               New dxoProperty(5, "", "Handle", dxxPropertyTypes.Handle),
               New dxoProperty(330, "", "Table Handle", dxxPropertyTypes.Pointer),
               New dxoProperty(100, "AcDbSymbolTableRecord", "Table Record SubClass Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True},
               New dxoProperty(100, "AcDbViewTableRecord", "Sub Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True},
               New dxoProperty(2, aName, "Name", dxxPropertyTypes.dxf_String),
               New dxoProperty(70, 0, "Bit Code", dxxPropertyTypes.BitCode),
               New dxoProperty(40, 0, "View Height(DCS) ", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive)
                }

                _rVal.AddVector(10, TVECTOR.Zero, "Center", True)
                _rVal.Add(New dxoProperty(41, 0, "View Width(DCS) ", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                _rVal.AddVector(11, TVECTOR.WorldZ, "Direction From Target", bIsDirection:=True)
                _rVal.AddVector(12, New TVECTOR(0, 0, 0), "Target Point")
                _rVal.Add(New dxoProperty(42, 50.0, "Lens Length", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                _rVal.Add(New dxoProperty(43, 0, "Front Clipping Plane Ofset From Target", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                _rVal.Add(New dxoProperty(44, 0, "Back Clipping Plane Ofset From Target", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                _rVal.Add(New dxoProperty(50, 0, "Twist Angle", dxxPropertyTypes.Angle))
                _rVal.Add(New dxoProperty(71, 0, "View Mode", dxxPropertyTypes.dxf_Integer))
                _rVal.Add(New dxoProperty(281, dxxRenderModes.TwoDOptimized, "Render Mode", dxxPropertyTypes.dxf_Integer))
                _rVal.Add(New dxoProperty(72, True, "Has UCS", dxxPropertyTypes.Switch))
                _rVal.AddVector(110, TVECTOR.Zero, "UCS Origin")
                _rVal.AddVector(111, TVECTOR.WorldX, "UCS X Axis Direction", bIsDirection:=True)
                _rVal.AddVector(111, TVECTOR.WorldY, "UCS Y Axis Direction", bIsDirection:=True)
                _rVal.Add(New dxoProperty(79, dxxOrthoGraphicTypes.NonOrthographic, "UCS Orthographic Type", dxxPropertyTypes.dxf_Integer))
                _rVal.Add(New dxoProperty(146, 0, "UCS Elevation", dxxPropertyTypes.dxf_Double))
                _rVal.Add(New dxoProperty(345, "0", "UCS Hande", dxxPropertyTypes.Pointer))
                _rVal.Add(New dxoProperty(346, "0", "Base UCS Hande", dxxPropertyTypes.Pointer))
                _rVal.Add(New dxoProperty(1, aGUID, "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True})
                Return _rVal

            Catch ex As Exception
                Return _rVal
            End Try

        End Function


        Friend Shared Function Properties_Entry_VPORT(Optional aName As String = "", Optional aGUID As String = "") As dxoProperties
            If String.IsNullOrWhiteSpace(aName) Then aName = "*Active"
            '^the default properties for a VPORT
            Dim _rVal As New dxoProperties(dxfEnums.DisplayName(dxxReferenceTypes.VPORT).ToUpper)
            Try


                _rVal.Add(New dxoProperty(0, "VPORT", "Table Entry Type", dxxPropertyTypes.dxf_String) With {.DoNotCopy = True})
                _rVal.Add(New dxoProperty(5, "", "Handle", dxxPropertyTypes.Handle))
                _rVal.Add(New dxoProperty(330, "", "Table Handle", dxxPropertyTypes.Pointer))
                _rVal.Add(New dxoProperty(100, "AcDbSymbolTableRecord", "Table Record SubClass Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                _rVal.Add(New dxoProperty(100, "AcDbViewportTableRecord", "Sub Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                _rVal.Add(New dxoProperty(2, aName, "Name", dxxPropertyTypes.dxf_String))
                _rVal.Add(New dxoProperty(70, 0, "BitFlag", dxxPropertyTypes.BitCode))
                _rVal.AddPoint(TVECTOR.Zero, "Lower Left", 10)
                _rVal.AddPoint(New TVECTOR(1, 1), "Upper Right", 11)
                _rVal.AddPoint(TVECTOR.Zero, "View Center", 12)
                _rVal.AddPoint(TVECTOR.Zero, "Snap Base", 13)
                _rVal.AddPoint(New TVECTOR(0.5, 0.5), "Snap Spacing", 14)
                _rVal.AddPoint(New TVECTOR(0.5, 0.5), "Grid Spacing", 15)
                _rVal.AddVector(16, TVECTOR.WorldZ, "View Direction", bIsDirection:=True)
                _rVal.AddVector(17, TVECTOR.Zero, "Target Point")
                _rVal.Add(New dxoProperty(40, 10.0, "Height", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero))
                _rVal.Add(New dxoProperty(41, 2.0, "Aspect Ratio", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                _rVal.Add(New dxoProperty(42, 50.0!, "Lens Length", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero))
                _rVal.Add(New dxoProperty(43, 0!, "Front Clip Offset", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                _rVal.Add(New dxoProperty(44, 0!, "Back Clip Offset", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                _rVal.Add(New dxoProperty(50, 0!, "Snap Rotation", dxxPropertyTypes.Angle))
                _rVal.Add(New dxoProperty(51, 0!, "Twist", dxxPropertyTypes.Angle))
                _rVal.Add(New dxoProperty(68, 0, "APP", dxxPropertyTypes.dxf_Integer, bSetSuppressedValue:=True))
                _rVal.Add(New dxoProperty(69, 0, "APPID", dxxPropertyTypes.dxf_Integer, bSetSuppressedValue:=True))
                _rVal.Add(New dxoProperty(71, 0, "View Mode", dxxPropertyTypes.BitCode))
                _rVal.Add(New dxoProperty(72, 1000.0, "Circle Zoom Percentage", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                _rVal.Add(New dxoProperty(73, 1, "Fast Zoom Setting", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                _rVal.Add(New dxoProperty(74, 3, "UCS ICon Setting", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                _rVal.Add(New dxoProperty(75, False, "Snap On/Off", dxxPropertyTypes.Switch))
                _rVal.Add(New dxoProperty(76, False, "Grid On/Off", dxxPropertyTypes.Switch))
                _rVal.Add(New dxoProperty(77, dxxSnapStyles.Rectangular, "Snap Style", dxxPropertyTypes.dxf_Integer))
                _rVal.Add(New dxoProperty(78, 0, "Snap isopair", dxxPropertyTypes.Undefined))
                _rVal.Add(New dxoProperty(281, dxxRenderModes.TwoDOptimized, "Render Mode", dxxPropertyTypes.dxf_Integer))
                _rVal.Add(New dxoProperty(65, True, "UCS Persist", dxxPropertyTypes.Switch))


                _rVal.AddVector(110, TVECTOR.Zero, "UCS Origin")
                _rVal.AddVector(111, TVECTOR.WorldX, "UCS X Axis", bIsDirection:=True)
                _rVal.AddVector(112, TVECTOR.WorldY, "UCS Y Axis", bIsDirection:=True)
                _rVal.AddVector(113, TVECTOR.WorldZ, "UCS Z Axis", bSuppressed:=True, bIsDirection:=True)
                _rVal.Add(New dxoProperty(79, dxxOrthoGraphicTypes.NonOrthographic, "Ortho Type", dxxPropertyTypes.dxf_Integer))
                _rVal.Add(New dxoProperty(345, "0", "Named UCS Handle", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                _rVal.Add(New dxoProperty(346, "0", "Named Ortho UCS Handle", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                _rVal.Add(New dxoProperty(146, 0, "Elevation", dxxPropertyTypes.dxf_Double))
                _rVal.Add(New dxoProperty(170, dxxShadePlotModes.AsDisplayed, "Shade Plot Setting", dxxPropertyTypes.dxf_Integer))
                _rVal.Add(New dxoProperty(332, "0", "Background Handle", "0", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                _rVal.Add(New dxoProperty(333, "0", "Shade Plot Handle", "0", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                _rVal.Add(New dxoProperty(348, "0", "Visual Style Handle", "0", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                _rVal.Add(New dxoProperty(60, 2, "Minor Grid Lines", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.PositiveNonZero))
                _rVal.Add(New dxoProperty(61, 5, "Major Grid Lines", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.PositiveNonZero))
                _rVal.Add(New dxoProperty(292, True, "Default Lighting On", dxxPropertyTypes.Switch))
                _rVal.Add(New dxoProperty(282, dxxLightingTypes.TwoDistant, "Default Lighting Type", dxxPropertyTypes.dxf_Integer))
                _rVal.Add(New dxoProperty(141, 0D, "Brightness", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                _rVal.Add(New dxoProperty(142, 0D, "Contrast", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                _rVal.Add(New dxoProperty(63, 250, "Ambient Color 1", dxxPropertyTypes.dxf_Integer, aSuppressedVal:=0))
                _rVal.Add(New dxoProperty(421, 3355443, "Ambient Color 2", dxxPropertyTypes.dxf_Integer, aSuppressedVal:=0))
                _rVal.Add(New dxoProperty(422, 0, "Ambient Color 3", dxxPropertyTypes.dxf_Integer, aSuppressedVal:=0))

                _rVal.Add(New dxoProperty(1, aGUID, "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True})
                _rVal.Add(New dxoProperty(2, 0, "*Width", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bNonDXF:=True))

            Catch ex As Exception
                Throw ex
            End Try
            Return _rVal

        End Function

        Private Shared _ENTRY_BLOCK_RECORD As TPROPERTIES
        Friend Shared ReadOnly Property Props_Entry_BLOCK_RECORD As TPROPERTIES
            Get
                '^the default properties for a BLOCK_RECORD

                Try
                    If _ENTRY_BLOCK_RECORD.Count = 0 Then
                        _ENTRY_BLOCK_RECORD = New TPROPERTIES(dxfEnums.DisplayName(dxxReferenceTypes.BLOCK_RECORD).ToUpper)
                        _ENTRY_BLOCK_RECORD.Add(New TPROPERTY(0, "BLOCK_RECORD", "Table Entry Type", dxxPropertyTypes.dxf_String) With {.DoNotCopy = True})
                        _ENTRY_BLOCK_RECORD.Add(New TPROPERTY(5, "", "Handle", dxxPropertyTypes.Handle))
                        _ENTRY_BLOCK_RECORD.Add(New TPROPERTY(330, "", "Table Handle", dxxPropertyTypes.Pointer))
                        _ENTRY_BLOCK_RECORD.Add(New TPROPERTY(100, "AcDbSymbolTableRecord", "Table Record SubClass Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                        _ENTRY_BLOCK_RECORD.Add(New TPROPERTY(100, "AcDbBlockTableRecord", "Sub Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                        _ENTRY_BLOCK_RECORD.Add(New TPROPERTY(2, "", "Name", dxxPropertyTypes.dxf_String))
                        _ENTRY_BLOCK_RECORD.Add(New TPROPERTY(340, "0", "Layout Handle", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                        _ENTRY_BLOCK_RECORD.Add(New TPROPERTY(70, dxxInsertUnits.Unitless, "Insert Units", dxxPropertyTypes.dxf_Integer))
                        _ENTRY_BLOCK_RECORD.Add(New TPROPERTY(280, True, "Explodable", dxxPropertyTypes.Switch))
                        _ENTRY_BLOCK_RECORD.Add(New TPROPERTY(281, False, "UniformScale", dxxPropertyTypes.Switch))

                        _ENTRY_BLOCK_RECORD.Add(New TPROPERTY(1, "", "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True})
                        _ENTRY_BLOCK_RECORD.Add(New TPROPERTY(2, "", "*LayoutName", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    End If

                Catch ex As Exception
                    Throw ex
                End Try
                Return New TPROPERTIES(_ENTRY_BLOCK_RECORD)
            End Get
        End Property


        Private Shared _ENTRY_DIMSTYLE As TPROPERTIES
        Friend Shared ReadOnly Property Props_Entry_DIMSTYLE As TPROPERTIES
            Get
                '^the default properties for a DIMSTYLE
                'dimstyles have r14 prperties to begin the non applicable
                'properties are turned off at dxfwrite time for R12
                Try
                    If _ENTRY_DIMSTYLE.Count = 0 Then ' If _ENTRY_DIMSTYLE.IsEmpty Then
                        _ENTRY_DIMSTYLE = New TPROPERTIES(dxfEnums.DisplayName(dxxReferenceTypes.DIMSTYLE).ToUpper)
                        _ENTRY_DIMSTYLE.Add(New TPROPERTY(0, "DIMSTYLE", "Table Entry Type", dxxPropertyTypes.dxf_String) With {.DoNotCopy = True})
                        _ENTRY_DIMSTYLE.Add(New TPROPERTY(105, "", "Handle", dxxPropertyTypes.Handle))
                        _ENTRY_DIMSTYLE.Add(New TPROPERTY(330, "", "Table Handle", dxxPropertyTypes.Pointer))
                        _ENTRY_DIMSTYLE.Add(New TPROPERTY(100, "AcDbSymbolTableRecord", "Table Record SubClass Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                        _ENTRY_DIMSTYLE.Add(New TPROPERTY(100, "AcDbDimStyleTableRecord", "Sub Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                        _ENTRY_DIMSTYLE.Add(New TPROPERTY(2, "Standard", "Name", dxxPropertyTypes.dxf_String))
                        _ENTRY_DIMSTYLE.Add(New TPROPERTY(70, 0, "Bit Code", dxxPropertyTypes.BitCode))
                        Dim enumVals As Dictionary(Of String, Integer) = dxfEnums.EnumValues(GetType(dxxDimStyleProperties))


                        For Each nm As String In enumVals.Keys
                            Dim ival As dxxDimStyleProperties = enumVals.Item(nm)
                            If ival >= dxxDimStyleProperties.DIMPOST Then  'And ival <= dxxDimStyleProperties.DIMLWE Then

                                _ENTRY_DIMSTYLE.Add(dxpProperties.DimStyleProperty(ival))
                            End If
                        Next
                        _ENTRY_DIMSTYLE.Add(New TPROPERTY(1, "", "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True})
                        _ENTRY_DIMSTYLE.ReIndex()
                    End If

                Catch ex As Exception
                    Throw ex
                End Try
                Return New TPROPERTIES(_ENTRY_DIMSTYLE)
            End Get
        End Property

        Private Shared _ENTRY_LAYER As TPROPERTIES
        Friend Shared ReadOnly Property Props_Entry_LAYER As TPROPERTIES
            Get
                '^the default properties for a LAYER
                Try
                    If _ENTRY_LAYER.Count = 0 Then
                        _ENTRY_LAYER = New TPROPERTIES(dxfEnums.DisplayName(dxxReferenceTypes.LAYER).ToUpper)
                        _ENTRY_LAYER.Add(New TPROPERTY(0, "LAYER", "Table Entry Type", dxxPropertyTypes.dxf_String) With {.DoNotCopy = True})
                        _ENTRY_LAYER.Add(New TPROPERTY(5, "", "Handle", dxxPropertyTypes.Handle))
                        _ENTRY_LAYER.Add(New TPROPERTY(330, "", "Table Handle", dxxPropertyTypes.Pointer))
                        _ENTRY_LAYER.Add(New TPROPERTY(100, "AcDbSymbolTableRecord", "Table Record SubClass Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                        _ENTRY_LAYER.Add(New TPROPERTY(100, "AcDbLayerTableRecord", "Sub Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                        _ENTRY_LAYER.Add(New TPROPERTY(2, "0", "Name", dxxPropertyTypes.dxf_String))
                        _ENTRY_LAYER.Add(New TPROPERTY(70, 0, "Bit Code", dxxPropertyTypes.BitCode))
                        _ENTRY_LAYER.Add(New TPROPERTY(62, dxxColors.BlackWhite, "Color", dxxPropertyTypes.Color))
                        _ENTRY_LAYER.Add(New TPROPERTY(6, dxfLinetypes.Continuous, "Linetype", dxxPropertyTypes.dxf_String))
                        _ENTRY_LAYER.Add(New TPROPERTY(290, 1, "Plot Flag", dxxPropertyTypes.Switch, bSetSuppressedValue:=True))
                        _ENTRY_LAYER.Add(New TPROPERTY(370, dxxLineWeights.ByDefault, "LineWeight", dxxPropertyTypes.dxf_Integer))
                        _ENTRY_LAYER.Add(New TPROPERTY(390, "0", "Plot Style Pointer", dxxPropertyTypes.Pointer))
                        _ENTRY_LAYER.Add(New TPROPERTY(347, "0", "Material Pointer", dxxPropertyTypes.Pointer))
                        '************* HIDDEN *****************************
                        _ENTRY_LAYER.Add(New TPROPERTY(1, "", "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True})
                        _ENTRY_LAYER.Add(New TPROPERTY(2, 0, "*Transparency", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive, bNonDXF:=True))
                        _ENTRY_LAYER.Add(New TPROPERTY(0, "", "*Description", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    End If

                Catch ex As Exception
                    Throw ex
                End Try
                Return New TPROPERTIES(_ENTRY_LAYER)
            End Get
        End Property

        Private Shared _ENTRY_STYLE As TPROPERTIES
        Friend Shared ReadOnly Property Props_Entry_STYLE As TPROPERTIES
            Get
                '^the default properties for an STYLE
                Try
                    If _ENTRY_STYLE.Count = 0 Then ' If _ENTRY_STYLE.IsEmpty Then

                        Dim font As dxoFont = dxfGlobals.GetFont("Arial.ttf", "txt.shx", bAttemptLoadIfNotFound:=True)

                        _ENTRY_STYLE = New TPROPERTIES(dxfEnums.DisplayName(dxxReferenceTypes.STYLE).ToUpper)
                        _ENTRY_STYLE.Add(New TPROPERTY(0, "STYLE", "Table Entry Type", dxxPropertyTypes.dxf_String) With {.DoNotCopy = True})
                        _ENTRY_STYLE.Add(New TPROPERTY(5, "", "Handle", dxxPropertyTypes.Handle))
                        _ENTRY_STYLE.Add(New TPROPERTY(330, "", "Table Handle", dxxPropertyTypes.Pointer))
                        _ENTRY_STYLE.Add(New TPROPERTY(100, "AcDbSymbolTableRecord", "Table Record SubClass Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                        _ENTRY_STYLE.Add(New TPROPERTY(100, "AcDbTextStyleTableRecord", "Sub Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                        _ENTRY_STYLE.Add(New TPROPERTY(2, "Standard", "Name", dxxPropertyTypes.dxf_String, aEnumName:=dxxStyleProperties.NAME))

                        _ENTRY_STYLE.Add(New TPROPERTY(70, 0, "Bit Code", dxxPropertyTypes.BitCode))
                        _ENTRY_STYLE.Add(New TPROPERTY(40, 0, "Text Height", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, aEnumName:=dxxStyleProperties.TEXTHT))
                        _ENTRY_STYLE.Add(New TPROPERTY(41, 1.0, "Width Factor", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, aEnumName:=dxxStyleProperties.WIDTHFACTOR))
                        _ENTRY_STYLE.Add(New TPROPERTY(50, 0, "Oblique Angle", dxxPropertyTypes.Angle, aEnumName:=dxxStyleProperties.OBLIQUE))
                        _ENTRY_STYLE.Add(New TPROPERTY(71, 0, "Generation Flag", dxxPropertyTypes.BitCode, aEnumName:=dxxStyleProperties.GENFLAG))
                        _ENTRY_STYLE.Add(New TPROPERTY(42, 0, "Last Height", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, aEnumName:=dxxStyleProperties.LASTHT))
                        _ENTRY_STYLE.Add(New TPROPERTY(3, font.Name, "Font", dxxPropertyTypes.dxf_String, aEnumName:=dxxStyleProperties.FONTNAME))
                        _ENTRY_STYLE.Add(New TPROPERTY(4, "", "Big Font File", dxxPropertyTypes.dxf_String, aEnumName:=dxxStyleProperties.BIGFONT))

                        '********** HIDDEN **************
                        _ENTRY_STYLE.Add(New TPROPERTY(1, "", "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True})
                        _ENTRY_STYLE.Add(New TPROPERTY(2, font.DefaultStyle.StyleName, "*FontStyle", dxxPropertyTypes.dxf_String, bNonDXF:=True, aEnumName:=dxxStyleProperties.FONTSTYLE))
                        _ENTRY_STYLE.Add(New TPROPERTY(3, font.DefaultStyle.TTFStyle, "*FontStyleType", dxxPropertyTypes.dxf_Integer, bNonDXF:=True, aEnumName:=dxxStyleProperties.FONTSTYLETYPE))
                        _ENTRY_STYLE.Add(New TPROPERTY(4, True, "*IsShape", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True, aEnumName:=dxxStyleProperties.SHAPEFLAG))
                        _ENTRY_STYLE.Add(New TPROPERTY(5, 1.0, "*LineSpacingFactor", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero, bNonDXF:=True, aEnumName:=dxxStyleProperties.LINESPACING))
                        _ENTRY_STYLE.Add(New TPROPERTY(6, dxxLineSpacingStyles.AtLeast, "*LineSpacingStyle", dxxPropertyTypes.dxf_Integer, bNonDXF:=True, aEnumName:=dxxStyleProperties.LINESPACINGSTYLE))
                        _ENTRY_STYLE.Add(New TPROPERTY(7, font.Index, "*FontIndex", dxxPropertyTypes.dxf_Integer, bNonDXF:=True, aEnumName:=dxxStyleProperties.FONTINDEX))
                        _ENTRY_STYLE.Add(New TPROPERTY(8, font.Style(1).FileName, "*FontFileName", dxxPropertyTypes.dxf_String, bNonDXF:=True, aEnumName:=dxxStyleProperties.FONTNAME))
                        _ENTRY_STYLE.Add(New TPROPERTY(9, False, "*Backwards", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True, aEnumName:=dxxStyleProperties.BACKWARDS))
                        _ENTRY_STYLE.Add(New TPROPERTY(10, False, "*Upsidedown", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True, aEnumName:=dxxStyleProperties.UPSIDEDOWN))
                        _ENTRY_STYLE.Add(New TPROPERTY(11, False, "*Vertical", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True, aEnumName:=dxxStyleProperties.VERTICAL))
                        _ENTRY_STYLE.Add(New TPROPERTY(12, False, "*XRefResolved", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True, aEnumName:=dxxStyleProperties.XREFRESOLVED))
                        _ENTRY_STYLE.Add(New TPROPERTY(13, False, "*XRefDependant", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True, aEnumName:=dxxStyleProperties.XREFDEPENANT))
                        _ENTRY_STYLE.Add(New TPROPERTY(14, False, "*IsReferenced", dxxPropertyTypes.dxf_Boolean, bNonDXF:=True, aEnumName:=dxxStyleProperties.ISREFERENCED))
                    End If


                Catch ex As Exception
                    Throw ex
                End Try
                Return New TPROPERTIES(_ENTRY_STYLE)
            End Get
        End Property


        Private Shared _ENTRY_UCS As TPROPERTIES
        Friend Shared ReadOnly Property Props_Entry_UCS As TPROPERTIES
            Get

                '^the default properties for a UCS
                Try
                    If _ENTRY_UCS.Count = 0 Then
                        _ENTRY_UCS = New TPROPERTIES(dxfEnums.DisplayName(dxxReferenceTypes.UCS).ToUpper)
                        _ENTRY_UCS.Add(New TPROPERTY(0, "UCS", "Table Entry Type", dxxPropertyTypes.dxf_String) With {.DoNotCopy = True})
                        _ENTRY_UCS.Add(New TPROPERTY(5, "", "Handle", dxxPropertyTypes.Handle))
                        _ENTRY_UCS.Add(New TPROPERTY(330, "", "Table Handle", dxxPropertyTypes.Pointer))
                        _ENTRY_UCS.Add(New TPROPERTY(100, "AcDbSymbolTableRecord", "Table Record SubClass Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                        _ENTRY_UCS.Add(New TPROPERTY(100, "AcDbUCSTableRecord", "Sub Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                        _ENTRY_UCS.Add(New TPROPERTY(2, "", "Name", dxxPropertyTypes.dxf_String))
                        _ENTRY_UCS.Add(New TPROPERTY(70, 0, "Bit Code", dxxPropertyTypes.BitCode))
                        _ENTRY_UCS.AddVector(10, TVECTOR.Zero, "Origin")
                        _ENTRY_UCS.AddVector(11, TVECTOR.WorldX, "X Axis Direction", bIsDirection:=True)
                        _ENTRY_UCS.AddVector(12, TVECTOR.WorldY, "Y Axis Direction", bIsDirection:=True)
                        _ENTRY_UCS.Add(New TPROPERTY(79, 0, "Dummy(Always 0)", dxxPropertyTypes.dxf_Integer))
                        _ENTRY_UCS.Add(New TPROPERTY(146, 0, "Elevation", dxxPropertyTypes.dxf_Double))
                        _ENTRY_UCS.Add(New TPROPERTY(346, "0", "Base UCS Handle", dxxPropertyTypes.Pointer))
                        _ENTRY_UCS.Add(New TPROPERTY(71, dxxOrthoGraphicTypes.NonOrthographic, "Orthographic Type", dxxPropertyTypes.dxf_Integer))
                        _ENTRY_UCS.AddVector(13, TVECTOR.Zero, "Relative Origin")
                        _ENTRY_UCS.Add(New TPROPERTY(1, "", "*GUID", dxxPropertyTypes.dxf_String) With {.DoNotCopy = True})
                    End If

                Catch ex As Exception
                    Throw ex
                End Try
                Return New TPROPERTIES(_ENTRY_UCS)
            End Get
        End Property

        Private Shared _ENTRY_VIEW As TPROPERTIES
        Friend Shared ReadOnly Property Props_Entry_VIEW As TPROPERTIES
            Get
                '^the default properties for a VIEW
                Try
                    If _ENTRY_VIEW.Count = 0 Then
                        _ENTRY_VIEW = New TPROPERTIES(dxfEnums.DisplayName(dxxReferenceTypes.VIEW).ToUpper)
                        _ENTRY_VIEW.Add(New TPROPERTY(0, "VIEW", "Table Entry Type", dxxPropertyTypes.dxf_String) With {.DoNotCopy = True})
                        _ENTRY_VIEW.Add(New TPROPERTY(5, "", "Handle", dxxPropertyTypes.Handle))
                        _ENTRY_VIEW.Add(New TPROPERTY(330, "", "Table Handle", dxxPropertyTypes.Pointer))
                        _ENTRY_VIEW.Add(New TPROPERTY(100, "AcDbSymbolTableRecord", "Table Record SubClass Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                        _ENTRY_VIEW.Add(New TPROPERTY(100, "AcDbViewTableRecord", "Sub Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                        _ENTRY_VIEW.Add(New TPROPERTY(2, "", "Name", dxxPropertyTypes.dxf_String))
                        _ENTRY_VIEW.Add(New TPROPERTY(70, 0, "Bit Code", dxxPropertyTypes.BitCode))
                        _ENTRY_VIEW.Add(New TPROPERTY(40, 0, "View Height(DCS) ", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                        _ENTRY_VIEW.AddVector(10, TVECTOR.Zero, "Center", True)
                        _ENTRY_VIEW.Add(New TPROPERTY(41, 0, "View Width(DCS) ", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                        _ENTRY_VIEW.AddVector(11, TVECTOR.WorldZ, "Direction From Target", bIsDirection:=True)
                        _ENTRY_VIEW.AddVector(12, New TVECTOR(0, 0, 0), "Target Point")
                        _ENTRY_VIEW.Add(New TPROPERTY(42, 50.0, "Lens Length", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                        _ENTRY_VIEW.Add(New TPROPERTY(43, 0, "Front Clipping Plane Offset From Target", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                        _ENTRY_VIEW.Add(New TPROPERTY(44, 0, "Back Clipping Plane Offset From Target", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                        _ENTRY_VIEW.Add(New TPROPERTY(50, 0, "Twist Angle", dxxPropertyTypes.Angle))
                        _ENTRY_VIEW.Add(New TPROPERTY(71, dxxViewModes.Off, "View Mode", dxxPropertyTypes.dxf_Integer))
                        _ENTRY_VIEW.Add(New TPROPERTY(281, dxxRenderModes.TwoDOptimized, "Render Mode", dxxPropertyTypes.dxf_Integer))
                        _ENTRY_VIEW.Add(New TPROPERTY(72, True, "Has UCS", dxxPropertyTypes.Switch))
                        _ENTRY_VIEW.AddVector(110, TVECTOR.Zero, "UCS Origin")
                        _ENTRY_VIEW.AddVector(111, TVECTOR.WorldX, "UCS X Axis Direction", bIsDirection:=True)
                        _ENTRY_VIEW.AddVector(111, TVECTOR.WorldY, "UCS Y Axis Direction", bIsDirection:=True)
                        _ENTRY_VIEW.Add(New TPROPERTY(79, dxxOrthoGraphicTypes.NonOrthographic, "UCS Orthographic Type", dxxPropertyTypes.dxf_Integer))
                        _ENTRY_VIEW.Add(New TPROPERTY(146, 0, "UCS Elevation", dxxPropertyTypes.dxf_Double))
                        _ENTRY_VIEW.Add(New TPROPERTY(345, "0", "UCS Hande", dxxPropertyTypes.Pointer))
                        _ENTRY_VIEW.Add(New TPROPERTY(346, "0", "Base UCS Hande", dxxPropertyTypes.Pointer))
                        _ENTRY_VIEW.Add(New TPROPERTY(1, "", "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True})
                    End If

                Catch ex As Exception
                    Throw ex
                End Try
                Return New TPROPERTIES(_ENTRY_VIEW)
            End Get
        End Property

        Private Shared _ENTRY_VPORT As TPROPERTIES
        Friend Shared ReadOnly Property Props_Entry_VPORT As TPROPERTIES
            '^the default properties for a VPORT
            Get

                Try
                    If _ENTRY_VPORT.Count = 0 Then ' If _ENTRY_VPORT.IsEmpty Then
                        _ENTRY_VPORT = New TPROPERTIES(dxfEnums.DisplayName(dxxReferenceTypes.VPORT).ToUpper)
                        _ENTRY_VPORT.Add(New TPROPERTY(0, "VPORT", "Table Entry Type", dxxPropertyTypes.dxf_String) With {.DoNotCopy = True})
                        _ENTRY_VPORT.Add(New TPROPERTY(5, "", "Handle", dxxPropertyTypes.Handle))
                        _ENTRY_VPORT.Add(New TPROPERTY(330, "", "Table Handle", dxxPropertyTypes.Pointer))
                        _ENTRY_VPORT.Add(New TPROPERTY(100, "AcDbSymbolTableRecord", "Table Record SubClass Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                        _ENTRY_VPORT.Add(New TPROPERTY(100, "AcDbViewportTableRecord", "Sub Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                        _ENTRY_VPORT.Add(New TPROPERTY(2, "", "Name", dxxPropertyTypes.dxf_String))
                        _ENTRY_VPORT.Add(New TPROPERTY(70, 0, "BitFlag", dxxPropertyTypes.BitCode))
                        _ENTRY_VPORT.AddPoint(TVECTOR.Zero, "Lower Left", 10)
                        _ENTRY_VPORT.AddPoint(New TVECTOR(1, 1), "Upper Right", 11)
                        _ENTRY_VPORT.AddPoint(TVECTOR.Zero, "View Center", 12)
                        _ENTRY_VPORT.AddPoint(TVECTOR.Zero, "Snap Base", 13)
                        _ENTRY_VPORT.AddPoint(New TVECTOR(0.5, 0.5), "Snap Spacing", 14)
                        _ENTRY_VPORT.AddPoint(New TVECTOR(0.5, 0.5), "Grid Spacing", 15)
                        _ENTRY_VPORT.AddVector(16, TVECTOR.WorldZ, "View Direction", bIsDirection:=True)
                        _ENTRY_VPORT.AddVector(17, TVECTOR.Zero, "Target Point")
                        _ENTRY_VPORT.Add(New TPROPERTY(40, 10.0, "Height", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero))
                        _ENTRY_VPORT.Add(New TPROPERTY(41, 2.0, "Aspect Ratio", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                        _ENTRY_VPORT.Add(New TPROPERTY(42, 50.0!, "Lens Length", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.PositiveNonZero))
                        _ENTRY_VPORT.Add(New TPROPERTY(43, 0!, "Front Clip Offset", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                        _ENTRY_VPORT.Add(New TPROPERTY(44, 0!, "Back Clip Offset", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                        _ENTRY_VPORT.Add(New TPROPERTY(50, 0!, "Snap Rotation", dxxPropertyTypes.Angle))
                        _ENTRY_VPORT.Add(New TPROPERTY(51, 0!, "Twist", dxxPropertyTypes.Angle))
                        _ENTRY_VPORT.Add(New TPROPERTY(68, 0, "APP", dxxPropertyTypes.dxf_Integer, bSetSuppressedValue:=True))
                        _ENTRY_VPORT.Add(New TPROPERTY(69, 0, "APPID", dxxPropertyTypes.dxf_Integer, bSetSuppressedValue:=True))
                        _ENTRY_VPORT.Add(New TPROPERTY(71, 0, "View Mode", dxxPropertyTypes.BitCode))
                        _ENTRY_VPORT.Add(New TPROPERTY(72, 1000.0, "Circle Zoom Percentage", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                        _ENTRY_VPORT.Add(New TPROPERTY(73, 1, "Fast Zoom Setting", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                        _ENTRY_VPORT.Add(New TPROPERTY(74, 3, "UCS ICon Setting", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                        _ENTRY_VPORT.Add(New TPROPERTY(75, False, "Snap On/Off", dxxPropertyTypes.Switch))
                        _ENTRY_VPORT.Add(New TPROPERTY(76, False, "Grid On/Off", dxxPropertyTypes.Switch))
                        _ENTRY_VPORT.Add(New TPROPERTY(77, dxxSnapStyles.Rectangular, "Snap Style", dxxPropertyTypes.dxf_Integer))
                        _ENTRY_VPORT.Add(New TPROPERTY(78, 0, "Snap isopair", dxxPropertyTypes.Undefined))
                        _ENTRY_VPORT.Add(New TPROPERTY(281, dxxRenderModes.TwoDOptimized, "Render Mode", dxxPropertyTypes.dxf_Integer))
                        _ENTRY_VPORT.Add(New TPROPERTY(65, True, "UCS Persist", dxxPropertyTypes.Switch))
                        _ENTRY_VPORT.AddVector(110, TVECTOR.Zero, "UCS Origin")
                        _ENTRY_VPORT.AddVector(111, TVECTOR.WorldX, "UCS X Axis", bIsDirection:=True)
                        _ENTRY_VPORT.AddVector(112, TVECTOR.WorldY, "UCS Y Axis", bIsDirection:=True)
                        _ENTRY_VPORT.AddVector(113, TVECTOR.WorldZ, "UCS Z Axis", bSuppressed:=True, bIsDirection:=True)
                        _ENTRY_VPORT.Add(New TPROPERTY(79, dxxOrthoGraphicTypes.NonOrthographic, "Ortho Type", dxxPropertyTypes.dxf_Integer))
                        _ENTRY_VPORT.Add(New TPROPERTY(345, "0", "Named UCS Handle", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                        _ENTRY_VPORT.Add(New TPROPERTY(346, "0", "Named Ortho UCS Handle", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                        _ENTRY_VPORT.Add(New TPROPERTY(146, 0, "Elevation", dxxPropertyTypes.dxf_Double))
                        _ENTRY_VPORT.Add(New TPROPERTY(170, dxxShadePlotModes.AsDisplayed, "Shade Plot Setting", dxxPropertyTypes.dxf_Integer))
                        _ENTRY_VPORT.Add(New TPROPERTY(332, "0", "Background Handle", "0", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                        _ENTRY_VPORT.Add(New TPROPERTY(333, "0", "Shade Plot Handle", "0", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                        _ENTRY_VPORT.Add(New TPROPERTY(348, "0", "Visual Style Handle", "0", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                        _ENTRY_VPORT.Add(New TPROPERTY(60, 2, "Minor Grid Lines", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.PositiveNonZero))
                        _ENTRY_VPORT.Add(New TPROPERTY(61, 5, "Major Grid Lines", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.PositiveNonZero))
                        _ENTRY_VPORT.Add(New TPROPERTY(292, True, "Default Lighting On", dxxPropertyTypes.Switch))
                        _ENTRY_VPORT.Add(New TPROPERTY(282, dxxLightingTypes.TwoDistant, "Default Lighting Type", dxxPropertyTypes.dxf_Integer))
                        _ENTRY_VPORT.Add(New TPROPERTY(141, 0D, "Brightness", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                        _ENTRY_VPORT.Add(New TPROPERTY(142, 0D, "Contrast", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                        _ENTRY_VPORT.Add(New TPROPERTY(63, 250, "Ambient Color 1", dxxPropertyTypes.dxf_Integer, aSuppressedVal:=0))
                        _ENTRY_VPORT.Add(New TPROPERTY(421, 3355443, "Ambient Color 2", dxxPropertyTypes.dxf_Integer, aSuppressedVal:=0))
                        _ENTRY_VPORT.Add(New TPROPERTY(422, 0, "Ambient Color 3", dxxPropertyTypes.dxf_Integer, aSuppressedVal:=0))

                        _ENTRY_VPORT.Add(New TPROPERTY(1, "", "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True})
                        _ENTRY_VPORT.Add(New TPROPERTY(2, 0, "*Width", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive, bNonDXF:=True))
                    End If

                Catch ex As Exception
                    Throw ex
                End Try
                Return New TPROPERTIES(_ENTRY_VPORT)

            End Get
        End Property



        Private Shared _ENTRY_LTYPE As TPROPERTIES
        Friend Shared ReadOnly Property Props_Entry_LTYPE As TPROPERTIES
            Get

                Try
                    If _ENTRY_LTYPE.Count = 0 Then
                        _ENTRY_LTYPE = New TPROPERTIES(dxfEnums.DisplayName(dxxReferenceTypes.LTYPE).ToUpper)
                        _ENTRY_LTYPE.Add(New TPROPERTY(0, "LTYPE", "Table Entry Type", dxxPropertyTypes.dxf_String) With {.DoNotCopy = True})
                        _ENTRY_LTYPE.Add(New TPROPERTY(5, "", "Handle", dxxPropertyTypes.Handle))
                        _ENTRY_LTYPE.Add(New TPROPERTY(330, "", "Table Handle", dxxPropertyTypes.Pointer))
                        _ENTRY_LTYPE.Add(New TPROPERTY(100, "AcDbSymbolTableRecord", "Table Record SubClass Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                        _ENTRY_LTYPE.Add(New TPROPERTY(100, "AcDbLinetypeTableRecord", "Sub Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                        _ENTRY_LTYPE.Add(New TPROPERTY(2, "", "Name", dxxPropertyTypes.dxf_String))
                        _ENTRY_LTYPE.Add(New TPROPERTY(70, 0, "Bit Code", dxxPropertyTypes.BitCode))
                        _ENTRY_LTYPE.Add(New TPROPERTY(3, "", "Description", dxxPropertyTypes.dxf_String))
                        _ENTRY_LTYPE.Add(New TPROPERTY(72, 65, "Alignment Code", dxxPropertyTypes.dxf_Integer))
                        _ENTRY_LTYPE.Add(New TPROPERTY(73, 0, "Element Count", dxxPropertyTypes.dxf_Integer, aValControl:=mzValueControls.Positive))
                        _ENTRY_LTYPE.Add(New TPROPERTY(40, 0, "Pattern Length", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                        _ENTRY_LTYPE.Add(New TPROPERTY(1, "", "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True})
                    End If

                Catch ex As Exception
                    Throw ex
                End Try
                Return New TPROPERTIES(_ENTRY_LTYPE)
            End Get

        End Property

        Private Shared _CommonObjectProps As TPROPERTIES
        Friend Shared ReadOnly Property Props_CommonObj As TPROPERTIES
            Get
                If _CommonObjectProps.Count < 0 Then
                    _CommonObjectProps.Add(New TPROPERTY(0, "", "Object Type Name", dxxPropertyTypes.dxf_String))
                    _CommonObjectProps.Add(New TPROPERTY(5, "", "Handle", dxxPropertyTypes.Handle))
                    _CommonObjectProps.Add(New TPROPERTY(330, "", "Owner", dxxPropertyTypes.Pointer))
                    _CommonObjectProps.Add(New TPROPERTY(100, "", "SubClass Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _CommonObjectProps.Add(New TPROPERTY(1, "", "*NAME", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                    _CommonObjectProps.Add(New TPROPERTY(2, "", "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True})
                    _CommonObjectProps.Add(New TPROPERTY(3, "", "*REFERENCE", dxxPropertyTypes.dxf_String, bNonDXF:=True))
                End If
                Return New TPROPERTIES(_CommonObjectProps)

            End Get
        End Property

        Private Shared _OBJECT_CELLSTYPEMAP As TPROPERTIES
        Friend Shared ReadOnly Property Props_Object_CELLSTYPEMAP As TPROPERTIES
            Get

                If _OBJECT_CELLSTYPEMAP.Count = 0 Then ' If _OBJECT_CELLSTYPEMAP.IsEmpty Then
                    _OBJECT_CELLSTYPEMAP = Get_ObjectCommon("CELLSTYLEMAP", "AcDbCellStyleMap")
                    _OBJECT_CELLSTYPEMAP.AddByString("90=3,300=CELLSTYLE")
                    _OBJECT_CELLSTYPEMAP.AddByString("1=TABLEFORMAT_BEGIN,90=5,170=1,91=0,92=32768,62=257,93=1")
                    _OBJECT_CELLSTYPEMAP.AddByString("300=CONTENTFORMAT,1=CONTENTFORMAT_BEGIN,90=0,91=0,92=512,93=0,300=,40=0,140=1,94=5,62=0,[TextStyle]340=0,144=0.25,309=CONTENTFORMAT_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("171=1,301=MARGIN,1=CELLMARGIN_BEGIN,40=0.06,40=0.06,40=0.06,40=0.06,40=0.18,40=0.18,309=CELLMARGIN_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("94=6,95=1")
                    _OBJECT_CELLSTYPEMAP.AddByString("302=GRIDFORMAT,1=GRIDFORMAT_BEGIN,90=0,91=1,62=0,92=-2,340=0,93=0,40=0.045,309=GRIDFORMAT_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("95=2")
                    _OBJECT_CELLSTYPEMAP.AddByString("302=GRIDFORMAT,1=GRIDFORMAT_BEGIN,90=0,91=1,62=0,92=-2,340=0,93=0,40=0.045,309=GRIDFORMAT_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("95=4")
                    _OBJECT_CELLSTYPEMAP.AddByString("302=GRIDFORMAT,1=GRIDFORMAT_BEGIN,90=0,91=1,62=0,92=-2,340=0,93=0,40=0.045,309=GRIDFORMAT_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("95=8")
                    _OBJECT_CELLSTYPEMAP.AddByString("302=GRIDFORMAT,1=GRIDFORMAT_BEGIN,90=0,91=1,62=0,92=-2,340=0,93=0,40=0.045,309=GRIDFORMAT_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("95=16")
                    _OBJECT_CELLSTYPEMAP.AddByString("302=GRIDFORMAT,1=GRIDFORMAT_BEGIN,90=0,91=1,62=0,92=-2,340=0,93=0,40=0.045,309=GRIDFORMAT_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("95=32")
                    _OBJECT_CELLSTYPEMAP.AddByString("302=GRIDFORMAT,1=GRIDFORMAT_BEGIN,90=0,91=1,62=0,92=-2,340=0,93=0,40=0.045,309=GRIDFORMAT_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("309=TABLEFORMAT_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("1=CELLSTYLE_BEGIN,90=1,91=1,300=_TITLE,309=CELLSTYLE_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("300=CELLSTYLE")
                    _OBJECT_CELLSTYPEMAP.AddByString("1=TABLEFORMAT_BEGIN,90=5,170=1,91=0,92=0,62=257,93=1")
                    _OBJECT_CELLSTYPEMAP.AddByString("300=CONTENTFORMAT,1=CONTENTFORMAT_BEGIN,90=0,91=0,92=512,93=0,300=,40=0,140=1,94=5,62=0,[TextStyle]340=0,144=0.18,309=CONTENTFORMAT_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("171=1,301=MARGIN")
                    _OBJECT_CELLSTYPEMAP.AddByString("1=CELLMARGIN_BEGIN,40=0.06,40=0.06,40=0.06,40=0.06,40=0.18,40=0.18,309=CELLMARGIN_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("94=6,95=1,302=GRIDFORMAT")
                    _OBJECT_CELLSTYPEMAP.AddByString("1=GRIDFORMAT_BEGIN,90=0,91=1,62=0,92=-2,340=0,93=0,40=0.045,309=GRIDFORMAT_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("95=2,302=GRIDFORMAT")
                    _OBJECT_CELLSTYPEMAP.AddByString("1=GRIDFORMAT_BEGIN,90=0,91=1,62=0,92=-2,340=0,93=0,40=0.045,309=GRIDFORMAT_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("95=4,302=GRIDFORMAT")
                    _OBJECT_CELLSTYPEMAP.AddByString("1=GRIDFORMAT_BEGIN,90=0,91=1,62=0,92=-2,340=0,93=0,40=0.045,309=GRIDFORMAT_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("95=8,302=GRIDFORMAT")
                    _OBJECT_CELLSTYPEMAP.AddByString("1=GRIDFORMAT_BEGIN,90=0,91=1,62=0,92=-2,340=0,93=0,40=0.045,309=GRIDFORMAT_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("95=16,302=GRIDFORMAT")
                    _OBJECT_CELLSTYPEMAP.AddByString("1=GRIDFORMAT_BEGIN,90=0,91=1,62=0,92=-2,340=0,93=0,40=0.045,309=GRIDFORMAT_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("95=32,302=GRIDFORMAT")
                    _OBJECT_CELLSTYPEMAP.AddByString("1=GRIDFORMAT_BEGIN,90=0,91=1,62=0,92=-2,340=0,93=0,40=0.045,309=GRIDFORMAT_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("309=TABLEFORMAT_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("1=CELLSTYLE_BEGIN,90=2,91=1,300=_HEADER,309=CELLSTYLE_END,300=CELLSTYLE")
                    _OBJECT_CELLSTYPEMAP.AddByString("1=TABLEFORMAT_BEGIN,90=5,170=1,91=0,92=0,62=257,93=1,300=CONTENTFORMAT,")
                    _OBJECT_CELLSTYPEMAP.AddByString("1=CONTENTFORMAT_BEGIN,90=0,91=0,92=512,93=0,300=,40=0,140=1,94=2,62=0,[TextStyle]340=0,144=0.18,309=CONTENTFORMAT_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("171=1,301=MARGIN")
                    _OBJECT_CELLSTYPEMAP.AddByString("1=CELLMARGIN_BEGIN,40=0.06,40=0.06,40=0.06,40=0.06,40=0.18,40=0.18,309=CELLMARGIN_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("94=6,95=1,302=GRIDFORMAT")
                    _OBJECT_CELLSTYPEMAP.AddByString("1=GRIDFORMAT_BEGIN,90=0,91=1,62=0,92=-2,340=0,93=0,40=0.045,309=GRIDFORMAT_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("95=2,302=GRIDFORMAT")
                    _OBJECT_CELLSTYPEMAP.AddByString("1=GRIDFORMAT_BEGIN,90=0,91=1,62=0,92=-2,340=0,93=0,40=0.045,309=GRIDFORMAT_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("95=4,302=GRIDFORMAT")
                    _OBJECT_CELLSTYPEMAP.AddByString("1=GRIDFORMAT_BEGIN,90=0,91=1,62=0,92=-2,340=0,93=0,40=0.045,309=GRIDFORMAT_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("95=8,302=GRIDFORMAT")
                    _OBJECT_CELLSTYPEMAP.AddByString("1=GRIDFORMAT_BEGIN,90=0,91=1,62=0,92=-2,340=0,93=0,40=0.045,309=GRIDFORMAT_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("95=16,302=GRIDFORMAT")
                    _OBJECT_CELLSTYPEMAP.AddByString("1=GRIDFORMAT_BEGIN,90=0,91=1,62=0,92=-2,340=0,93=0,40=0.045,309=GRIDFORMAT_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("95=32,302=GRIDFORMAT")
                    _OBJECT_CELLSTYPEMAP.AddByString("1=GRIDFORMAT_BEGIN,90=0,91=1,62=0,92=-2,340=0,93=0,40=0.045,309=GRIDFORMAT_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("309=TABLEFORMAT_END")
                    _OBJECT_CELLSTYPEMAP.AddByString("1=CELLSTYLE_BEGIN,90=3,91=2,300=_DATA,309=CELLSTYLE_END")
                    _OBJECT_CELLSTYPEMAP.ReIndex(True)
                End If
                Return New TPROPERTIES(_OBJECT_CELLSTYPEMAP)
            End Get
        End Property

        Private Shared _OBJECT_DICTIONARY As TPROPERTIES
        Friend Shared ReadOnly Property Props_Object_DICTIONARY As TPROPERTIES
            Get
                '^the default properties for an DICTIONARY
                If _OBJECT_DICTIONARY.Count = 0 Then ' If _OBJECT_DICTIONARY.IsEmpty Then
                    _OBJECT_DICTIONARY = Get_ObjectCommon("DICTIONARY", "AcDbDictionary")
                    _OBJECT_DICTIONARY.Add(New TPROPERTY(280, 0, "Hard Owner Flag", 0, dxxPropertyTypes.Switch))
                    _OBJECT_DICTIONARY.Add(New TPROPERTY(281, 1, "Duplicate Record Cloning Flag", dxxPropertyTypes.Switch))
                    _OBJECT_DICTIONARY.ReIndex(True)
                End If
                Return New TPROPERTIES(_OBJECT_DICTIONARY)
            End Get
        End Property


        Private Shared _OBJECT_DICTIONARYVAR As TPROPERTIES
        Private Shared ReadOnly Property Props_Object_DICTIONARYVAR As TPROPERTIES
            Get
                If _OBJECT_DICTIONARYVAR.Count = 0 Then ' If _OBJECT_DICTIONARYVAR.IsEmpty Then
                    _OBJECT_DICTIONARYVAR = Get_ObjectCommon("DICTIONARYVAR", "DictionaryVariables")
                    _OBJECT_DICTIONARYVAR.Add(New TPROPERTY(280, 0, "Object Schema Number", dxxPropertyTypes.dxf_Integer))
                    _OBJECT_DICTIONARYVAR.Add(New TPROPERTY(1, "", "Value", dxxPropertyTypes.dxf_String))
                    _OBJECT_DICTIONARYVAR.ReIndex(True)
                End If
                Return New TPROPERTIES(_OBJECT_DICTIONARYVAR)
            End Get
        End Property

        Private Shared _OBJECT_DICTIONARYWDFLT As TPROPERTIES
        Friend Shared ReadOnly Property Props_Object_DICTIONARYWDFLT As TPROPERTIES
            Get
                If _OBJECT_DICTIONARYWDFLT.Count = 0 Then ' If _OBJECT_DICTIONARYWDFLT.IsEmpty Then
                    _OBJECT_DICTIONARYWDFLT = Get_ObjectCommon("ACDBDICTIONARYWDFLT", "AcDbDictionary")
                    _OBJECT_DICTIONARYWDFLT.Add(New TPROPERTY(281, 1, "Duplicate Cloning Flag", dxxPropertyTypes.Undefined))
                    _OBJECT_DICTIONARYWDFLT.Add(New TPROPERTY(100, "AcDbDictionaryWithDefault", "Sub Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _OBJECT_DICTIONARYWDFLT.Add(New TPROPERTY(340, "0", "Default Entry Handle", dxxPropertyTypes.Pointer))
                    _OBJECT_DICTIONARYWDFLT.ReIndex(True)
                End If
                Return New TPROPERTIES(_OBJECT_DICTIONARYWDFLT)
            End Get
        End Property

        Private Shared _OBJECT_GROUP As TPROPERTIES
        Friend Shared ReadOnly Property Props_Object_Group As TPROPERTIES
            Get
                '^the default properties for a GROUP
                If _OBJECT_GROUP.Count = 0 Then ' If _OBJECT_GROUP.IsEmpty Then
                    _OBJECT_GROUP = Get_ObjectCommon("GROUP", "AcDbGroup")
                    _OBJECT_GROUP.Add(New TPROPERTY(300, "", "Description", dxxPropertyTypes.dxf_String))
                    _OBJECT_GROUP.Add(New TPROPERTY(70, False, "Named Flag", dxxPropertyTypes.Switch))
                    _OBJECT_GROUP.Add(New TPROPERTY(71, True, "Selectable Flag", dxxPropertyTypes.Switch))
                    _OBJECT_GROUP.ReIndex(True)
                End If
                Return New TPROPERTIES(_OBJECT_GROUP)
            End Get
        End Property

        Private Shared _OBJECT_LAYOUT As TPROPERTIES
        Friend Shared ReadOnly Property Props_Object_LAYOUT() As TPROPERTIES
            Get
                '^the default properties for a LAYOUT
                If _OBJECT_LAYOUT.Count = 0 Then ' If _OBJECT_LAYOUT.IsEmpty Then
                    _OBJECT_LAYOUT = Get_ObjectProperties(dxxObjectTypes.PlotSetting, "Plotsettings Name")
                    _OBJECT_LAYOUT.SetVal("Object Type Name", "LAYOUT")
                    _OBJECT_LAYOUT.Add(New TPROPERTY(100, "AcDbLayout", "Sub Class Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                    _OBJECT_LAYOUT.Add(New TPROPERTY(1, "", "Name", dxxPropertyTypes.dxf_String))
                    _OBJECT_LAYOUT.Add(New TPROPERTY(70, 1, "Bit-Flag", dxxPropertyTypes.BitCode))
                    _OBJECT_LAYOUT.Add(New TPROPERTY(71, 1, "Tab Order", dxxPropertyTypes.dxf_Integer))
                    _OBJECT_LAYOUT.AddVector(10, TVECTOR.Zero, "LimMin", True)
                    _OBJECT_LAYOUT.AddVector(11, TVECTOR.Zero, "LimMax", True)
                    _OBJECT_LAYOUT.AddVector(12, TVECTOR.Zero, "Insert Base")
                    _OBJECT_LAYOUT.AddVector(14, TVECTOR.Zero, "MinExtents")
                    _OBJECT_LAYOUT.AddVector(15, TVECTOR.Zero, "MaxExtents")
                    _OBJECT_LAYOUT.Add(New TPROPERTY(146, 0, "Elevation", dxxPropertyTypes.dxf_Double))
                    _OBJECT_LAYOUT.AddVector(13, TVECTOR.Zero, "UCS Origin")
                    _OBJECT_LAYOUT.AddVector(16, TVECTOR.WorldX, "UCS X Direction", bIsDirection:=True)
                    _OBJECT_LAYOUT.AddVector(17, TVECTOR.WorldY, "UCS Y Direction", bIsDirection:=True)
                    _OBJECT_LAYOUT.Add(New TPROPERTY(76, dxxOrthoGraphicTypes.NonOrthographic, "Orthographic Type", dxxPropertyTypes.dxf_Integer))
                    _OBJECT_LAYOUT.Add(New TPROPERTY(330, "0", "Space Block Record Handle", dxxPropertyTypes.Pointer))
                    _OBJECT_LAYOUT.Add(New TPROPERTY(331, "0", "Current V-Port", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _OBJECT_LAYOUT.Add(New TPROPERTY(345, "0", "Current UCS", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _OBJECT_LAYOUT.Add(New TPROPERTY(346, "0", "Base UCS", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _OBJECT_LAYOUT.ReIndex(True)
                End If
                Return New TPROPERTIES(_OBJECT_LAYOUT)
            End Get

        End Property
        Private Shared _OBJECT_MATERIAL As TPROPERTIES

        Friend Shared ReadOnly Property Props_Object_MATERIAL() As TPROPERTIES
            Get
                If _OBJECT_MATERIAL.Count = 0 Then ' If _OBJECT_MATERIAL.IsEmpty Then
                    Dim idenM As TMATRIX4 = TMATRIX4.Identity()
                    _OBJECT_MATERIAL = Get_ObjectCommon("MATERIAL", "AcDbMaterial")
                    _OBJECT_MATERIAL.Add(New TPROPERTY(1, "", "Name", dxxPropertyTypes.dxf_String))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(2, "", "Description", dxxPropertyTypes.dxf_String))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(70, 0, "Ambient color method", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(40, 1, "Ambient color factor", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(90, 0, "Ambient color value", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(71, 0, "Diffuse color method", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(41, 1, "Diffuse color factor", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(91, 0, "Diffuse color value", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(42, 1, "Diffuse map blend factor", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(72, 1, "Diffuse map source", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(3, "", "Diffuse map file name", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(73, 1, "Diffuse map Projection method", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(74, 1, "Diffuse map Tiling method", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(75, 1, "Diffuse map Auto transform method", dxxPropertyTypes.Undefined))
                    idenM.Name = "Diffuse map Transform matrix"
                    _OBJECT_MATERIAL.AddMatrix(43, "", idenM)
                    _OBJECT_MATERIAL.Add(New TPROPERTY(44, 0.5, "Specular gloss factor", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(76, 0, "Specular gloss method", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(45, 1, "Specular color factor", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(92, 0, "Specular color", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(46, 0.5, "Specular map blend factor", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(77, 1, "Specular gloss method", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(4, "", "Specular map file name", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(78, 1, "Specular projection method", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(79, 1, "Specular tiling method", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(170, 1, "Specular auto transform method", dxxPropertyTypes.Undefined))
                    idenM.Name = "Specular map Transform matrix"
                    _OBJECT_MATERIAL.AddMatrix(47, "", idenM)
                    _OBJECT_MATERIAL.Add(New TPROPERTY(48, 1, "Reflection map blend factor", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(171, 1, "Reflection map source", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(6, "", "Reflection map file name", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(172, 1, "Reflection map projection method", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(173, 1, "Reflection map tiling method", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(174, 1, "Reflection map auto transform method", dxxPropertyTypes.Undefined))
                    idenM.Name = "Reflection map Transform matrix"
                    _OBJECT_MATERIAL.AddMatrix(49, "", idenM)
                    _OBJECT_MATERIAL.Add(New TPROPERTY(140, 1, "Opacity percent", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(141, 1, "Opacity map blend factor", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(175, 1, "Opacity map source", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(7, "", "Opacity map file name", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(176, 1, "Opacity map projection method", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(177, 1, "Opacity map tiling method", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(178, 1, "Opacity map auto transform method", dxxPropertyTypes.Undefined))
                    idenM.Name = "Opacity map Transform matrix"
                    _OBJECT_MATERIAL.AddMatrix(142, "", idenM)
                    _OBJECT_MATERIAL.Add(New TPROPERTY(143, 1, "Bump map blend factor", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(179, 1, "Bump map source", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(8, "", "Bump map file name", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(270, 1, "Bump map projection method", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(271, 1, "Bump map tiling method", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(272, 1, "Bump map auto transform method", dxxPropertyTypes.Undefined))
                    idenM.Name = "Bump map Transform matrix"
                    _OBJECT_MATERIAL.AddMatrix(144, "", idenM)
                    _OBJECT_MATERIAL.Add(New TPROPERTY(145, 1, "Refraction index", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(146, 1, "Refraction map blend factor", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(273, 1, "Refraction map source", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(9, "", "Refraction map file name", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(274, 1, "Refraction map projection method", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(275, 1, "Refraction map tiling method", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(276, 1, "Refraction map auto transform method", dxxPropertyTypes.Undefined))
                    idenM.Name = "Refraction map Transform matrix"
                    _OBJECT_MATERIAL.AddMatrix(147, "", idenM)
                    _OBJECT_MATERIAL.Add(New TPROPERTY(460, 1, "Color Bleed Scale", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(461, 1, "Indirect Dump Scale", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(462, 1, "Reflectance Scale", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(463, 1, "Transmittance Scale", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(290, 0, "Two-sided Material", dxxPropertyTypes.Switch))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(464, 1, "Luminance", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(270, 1, "Luminance Mode", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(271, 1, "Normal map method", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(465, 1, "Normal Map Strength", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(42, 1, "Normal map blend factor", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(72, 1, "Normal map source", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(3, "", "Normal map file name", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(73, 1, "Normal map projection method", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(74, 1, "Normal map tiling method", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(75, 1, "Normal map auto transform method", dxxPropertyTypes.Undefined))
                    idenM.Name = "Normal map Transform matrix"
                    _OBJECT_MATERIAL.AddMatrix(43, "", idenM)
                    _OBJECT_MATERIAL.Add(New TPROPERTY(293, 0, "Materials Anonymous", dxxPropertyTypes.Switch))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(272, 1, "Global Illumination Mode", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(273, 1, "Final Gather Mode", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(300, "", "GenProcName", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(291, False, "GenProcValBool", dxxPropertyTypes.Switch))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(271, 0, "GenProcValInt", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(469, 0, "GenProcValReal", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(301, "", "GenProcValText", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(292, False, "GenProcTableEnd", dxxPropertyTypes.Switch))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(62, 0, "GenProcValColorIndex", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(420, 0, "GenProcValColorRGB", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(430, "", "GenProcValColorName", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(270, 1, "Map UTile", dxxPropertyTypes.Undefined))
                    '        props_Add _OBJECT_MATERIAL, 271, 1, "Map VTile"
                    _OBJECT_MATERIAL.Add(New TPROPERTY(90, 1, "Self -Illuminaton", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(468, 1, "Reflectivity", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(93, 1, "Illumination Model", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.Add(New TPROPERTY(94, 63, "Channel Flags", dxxPropertyTypes.Undefined))
                    _OBJECT_MATERIAL.ReIndex(True)
                End If
                Return New TPROPERTIES(_OBJECT_MATERIAL)
            End Get
        End Property

        Private Shared _OBJECT_PLOTSETTINGS As TPROPERTIES
        Friend Shared ReadOnly Property Props_Object_PLOTSETTINGS As TPROPERTIES
            Get
                '^the default properties for a PLOTSETTINGS
                If _OBJECT_PLOTSETTINGS.Count = 0 Then ' If _OBJECT_PLOTSETTINGS.IsEmpty Then
                    _OBJECT_PLOTSETTINGS = Get_ObjectCommon("PLOTSETTINGS", "AcDbPlotSettings")
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(1, "", "Name", dxxPropertyTypes.dxf_String))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(2, "None", "Printer Or config file", dxxPropertyTypes.dxf_String))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(4, "", "Paper Size", dxxPropertyTypes.dxf_String))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(6, "", "View Name", dxxPropertyTypes.dxf_String))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(40, 0, "Margin-Left(mm)", dxxPropertyTypes.dxf_Double))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(41, 0, "Margin-Bottom(mm)", dxxPropertyTypes.dxf_Double))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(42, 0, "Margin-Right(mm)", dxxPropertyTypes.dxf_Double))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(43, 0, "Margin-Top(mm)", dxxPropertyTypes.dxf_Double))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(44, 0, "Paper Width(mm)", dxxPropertyTypes.dxf_Double))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(45, 0, "Paper Height(mm)", dxxPropertyTypes.dxf_Double))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(46, 0, "Paper Origin X(mm)", dxxPropertyTypes.dxf_Double))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(47, 0, "Paper Origin Y(mm)", dxxPropertyTypes.dxf_Double))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(48, 0, "Window LL.X(mm)", dxxPropertyTypes.dxf_Double))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(49, 0, "Window UR.Y(mm)", dxxPropertyTypes.dxf_Double))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(140, 0, "Window LL.Y(mm)", dxxPropertyTypes.dxf_Double))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(141, 0, "Window UR.X(mm)", dxxPropertyTypes.dxf_Double))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(142, 1.0, "Print Scale Numerator", dxxPropertyTypes.dxf_Double))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(143, 1.0, "Print Scale Denominator", dxxPropertyTypes.dxf_Double))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(70, 688, "Layout Bit Flag", dxxPropertyTypes.BitCode))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(72, dxxPaperUnits.Inches, "Plot Paper Units", dxxPropertyTypes.dxf_Integer))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(73, dxxPaperRotations.Zero, "Plot Rotation", dxxPropertyTypes.dxf_Integer))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(74, dxxPaperPlotTypes.LastDisplay, "Plot Type Flag", dxxPropertyTypes.dxf_Integer))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(7, "", "Style Sheet", dxxPropertyTypes.dxf_String))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(75, 16, "Standard Scale Type", dxxPropertyTypes.dxf_Integer))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(76, dxxShadePlotModes.AsDisplayed, "Shade Plot Mode", dxxPropertyTypes.dxf_Integer, bSetSuppressedValue:=True))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(77, dxxShadePlotResolutions.Normal, "Shade Plot Resolution", dxxPropertyTypes.dxf_Integer, bSetSuppressedValue:=True))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(78, 300, "Shade Plot Custom DPI", dxxPropertyTypes.dxf_Integer))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(147, 1.0, "Plot Scale Factor", dxxPropertyTypes.dxf_Double, aValControl:=mzValueControls.Positive))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(148, 0, "Image Origin.X", dxxPropertyTypes.dxf_Double))
                    _OBJECT_PLOTSETTINGS.Add(New TPROPERTY(149, 0, "Image Origin.Y", dxxPropertyTypes.dxf_Double))
                    _OBJECT_PLOTSETTINGS.ReIndex(True)
                End If
                Return New TPROPERTIES(_OBJECT_PLOTSETTINGS)
            End Get

        End Property

        Private Shared _OBJECT_MLEADERSTYLE As TPROPERTIES
        Friend Shared ReadOnly Property Props_Object_MLEADERSTYLE As TPROPERTIES
            Get

                If _OBJECT_MLEADERSTYLE.Count = 0 Then ' If _OBJECT_MLEADERSTYLE.IsEmpty Then
                    _OBJECT_MLEADERSTYLE = Get_ObjectCommon("MLEADERSTYLE", "AcDbMLeaderStyle")
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(170, 2, "Leader Linetype"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(171, dxxLineWeights.ByLayer, "Leader Lineweight"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(172, 1, "Content type"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(90, 2, "Max Leader Points"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(40, 0, "First Segment Angle"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(41, 0, "Second Segment Angle"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(173, 1, "Text Left Attachment type"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(91, -1056964608, "Line Color"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(340, "", "Line type Handle"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(92, -2, "Text Color"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(290, 1, "Enable Landing", dxxPropertyTypes.Switch))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(42, 0.09, "Landing Gap"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(291, 1, "Enable Dogleg", dxxPropertyTypes.Switch))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(43, 0.36, "Landing Distance"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(3, "Standard", "Text Style Name"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(341, "0", "Arrowhead Handle", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(44, 0.18, "Text Height"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(300, "", "Default text"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(342, "0", "Text Style Handle", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(174, 1, "Text Angle Type"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(178, 1, "Text Align In IP"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(175, 1, "Text Alignment Type"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(176, 0, "Block Content Connection Type"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(93, -1056964608, "WinColor_1", dxxPropertyTypes.dxf_Integer))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(45, 0.18, "Arrowhead size"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(292, 0, "Switch", dxxPropertyTypes.Switch))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(297, 0, "Switch", dxxPropertyTypes.Switch))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(46, 0.18, "Size"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(94, -1056964608, "WinColor_2", dxxPropertyTypes.dxf_Integer))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(47, 1, "Scale_1"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(49, 1, "Scale_2"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(140, 1, "Num_1"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(293, 1, "Switch", dxxPropertyTypes.Switch))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(141, 1, "Num_2"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(294, 1, "Switch", dxxPropertyTypes.Switch))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(177, 0, "Num_3"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(142, 0, "Num_4"))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(295, 1, "Switch_1", dxxPropertyTypes.Switch))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(296, 1, "Switch_2", dxxPropertyTypes.Switch))
                    _OBJECT_MLEADERSTYLE.Add(New TPROPERTY(143, 0.125, "Break Size"))
                    _OBJECT_MLEADERSTYLE.ReIndex(True)
                End If
                Return New TPROPERTIES(_OBJECT_MLEADERSTYLE)
            End Get
        End Property

        Private Shared _OBJECT_PROXYOBJECT As TPROPERTIES
        Friend Shared ReadOnly Property Props_Object_PROXYOBJECT As TPROPERTIES
            Get
                '^the default properties for ACAD_PROXY_OBJECT
                If _OBJECT_PROXYOBJECT.Count = 0 Then ' If _OBJECT_PROXYOBJECT.IsEmpty Then
                    _OBJECT_PROXYOBJECT = Get_ObjectCommon("ACAD_PROXY_OBJECT", "AcDbProxyObject")
                    _OBJECT_PROXYOBJECT.Add(New TPROPERTY(90, 499, "Proxy Class ID(Always 499) "))
                    _OBJECT_PROXYOBJECT.Add(New TPROPERTY(91, "", "Class ID's"))
                    _OBJECT_PROXYOBJECT.Add(New TPROPERTY(310, "", "Binary Data", dxxPropertyTypes.dxf_String))
                    _OBJECT_PROXYOBJECT.Add(New TPROPERTY(330, "", "Object Handle 330", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _OBJECT_PROXYOBJECT.Add(New TPROPERTY(340, "", "Object Handle 340", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _OBJECT_PROXYOBJECT.Add(New TPROPERTY(350, "", "Object Handle 350", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _OBJECT_PROXYOBJECT.Add(New TPROPERTY(360, "", "Object Handle 360", dxxPropertyTypes.Pointer, bSetSuppressedValue:=True))
                    _OBJECT_PROXYOBJECT.Add(New TPROPERTY(94, 0, "End Object Handlez"))
                    _OBJECT_PROXYOBJECT.Add(New TPROPERTY(95, 0, "Object Drawing Format"))
                    _OBJECT_PROXYOBJECT.Add(New TPROPERTY(70, 0, "Custom Object Data Format"))
                    _OBJECT_PROXYOBJECT.ReIndex(True)
                End If
                Return New TPROPERTIES(_OBJECT_PROXYOBJECT)
            End Get
        End Property

        Private Shared _OBJECT_MLINESTYLE As TPROPERTIES
        Friend Shared ReadOnly Property Props_Object_MLINESTYLE As TPROPERTIES
            Get
                '^the default properties for a MLINESTYLE
                If _OBJECT_MLINESTYLE.Count = 0 Then ' If _OBJECT_MLINESTYLE.IsEmpty Then
                    _OBJECT_MLINESTYLE = Get_ObjectCommon("MLINESTYLE", "AcDbMlineStyle")
                    _OBJECT_MLINESTYLE.Add(New TPROPERTY(2, "", "Name"))
                    _OBJECT_MLINESTYLE.Add(New TPROPERTY(70, 0, "Bit-Flag"))
                    _OBJECT_MLINESTYLE.Add(New TPROPERTY(3, "", "Description"))
                    _OBJECT_MLINESTYLE.Add(New TPROPERTY(62, dxxColors.ByLayer, "Fill Color", dxxPropertyTypes.Color))
                    _OBJECT_MLINESTYLE.Add(New TPROPERTY(51, 90.0, "Start Angle", dxxPropertyTypes.Angle))
                    _OBJECT_MLINESTYLE.Add(New TPROPERTY(52, 90.0, "End Angle", dxxPropertyTypes.Angle))
                    _OBJECT_MLINESTYLE.Add(New TPROPERTY(71, 0, "Element Count", dxxPropertyTypes.dxf_Integer))
                    _OBJECT_MLINESTYLE.ReIndex(True)
                End If
                Return New TPROPERTIES(_OBJECT_MLINESTYLE)
            End Get
        End Property

        Private Shared _OBJECT_PLACEHOLDER As TPROPERTIES
        Friend Shared ReadOnly Property Props_Object_PLACEHOLDER As TPROPERTIES
            Get

                If _OBJECT_PLACEHOLDER.Count = 0 Then ' If Props_Object_PROXYOBJECT.IsEmpty Then
                    _OBJECT_PLACEHOLDER = Get_ObjectCommon("ACDBPLACEHOLDER", "AcDbPlaceHolder")
                    _OBJECT_PLACEHOLDER.ReIndex(True)
                End If
                Return New TPROPERTIES(_OBJECT_PLACEHOLDER)
            End Get
        End Property


        Private Shared _OBJECT_SCALE As TPROPERTIES
        Friend Shared ReadOnly Property Props_Object_SCALE As TPROPERTIES
            Get

                If _OBJECT_SCALE.Count = 0 Then ' If Props_Object_PROXYOBJECT.IsEmpty Then
                    _OBJECT_SCALE = Get_ObjectCommon("SCALE", "AcDbScale")
                    _OBJECT_SCALE.Add(New TPROPERTY(70, 0, "Bit Code"))
                    _OBJECT_SCALE.Add(New TPROPERTY(300, "", "Name"))
                    _OBJECT_SCALE.Add(New TPROPERTY(140, 0, "Numerator"))
                    _OBJECT_SCALE.Add(New TPROPERTY(141, 0, "Denominator"))
                    _OBJECT_SCALE.Add(New TPROPERTY(290, 0, "Flag?"))
                    _OBJECT_SCALE.ReIndex(True)
                End If
                Return New TPROPERTIES(_OBJECT_SCALE)
            End Get
        End Property
        Private Shared _OBJECT_TABLESTYLE As TPROPERTIES
        Friend Shared ReadOnly Property Props_Object_TABLESTYLE As TPROPERTIES
            Get
                '^the default properties for ACAD_PROXY_OBJECT
                If _OBJECT_TABLESTYLE.Count = 0 Then ' If Props_Object_PROXYOBJECT.IsEmpty Then
                    _OBJECT_TABLESTYLE = Get_ObjectCommon("TABLESTYLE", "AcDbTableStyle")
                    _OBJECT_TABLESTYLE.Add(New TPROPERTY(3, "", "Description"))
                    _OBJECT_TABLESTYLE.Add(New TPROPERTY(70, 0, "Flow Direction", dxxPropertyTypes.dxf_Integer, aDecodeString:="0=Up,1=Down"))
                    _OBJECT_TABLESTYLE.Add(New TPROPERTY(71, 0, "Bit Code", dxxPropertyTypes.dxf_Integer))
                    _OBJECT_TABLESTYLE.Add(New TPROPERTY(40, 0.06, "Horizontal Cell Margin", dxxPropertyTypes.dxf_Double))
                    _OBJECT_TABLESTYLE.Add(New TPROPERTY(41, 0.06, "Vertical Cell Margin", dxxPropertyTypes.dxf_Double))
                    _OBJECT_TABLESTYLE.Add(New TPROPERTY(280, 0, "Title Suppression", dxxPropertyTypes.Switch))
                    _OBJECT_TABLESTYLE.Add(New TPROPERTY(281, 0, "Column Header Suppression", dxxPropertyTypes.Switch))
                    _OBJECT_TABLESTYLE.ReIndex(True)
                End If
                Return New TPROPERTIES(_OBJECT_TABLESTYLE)
            End Get
        End Property

        Private Shared _OBJECT_TABLESTYLE_CELL As TPROPERTIES
        Friend Shared ReadOnly Property Props_Object_TABLESTYLE_CELL As TPROPERTIES
            Get

                '^the default properties for ACAD_PROXY_OBJECT
                If _OBJECT_TABLESTYLE_CELL.Count = 0 Then ' If Props_Object_PROXYOBJECT.IsEmpty Then
                    Dim i As Integer
                    Dim gc1 As Integer
                    Dim gc2 As Integer
                    Dim gc3 As Integer
                    gc1 = 274
                    gc2 = 284
                    gc3 = 64
                    _OBJECT_TABLESTYLE_CELL = New TPROPERTIES("Cell Settings")
                    _OBJECT_TABLESTYLE_CELL.Add(7, "Standard", "Cell Text Style", dxxPropertyTypes.dxf_String)
                    _OBJECT_TABLESTYLE_CELL.Add(140, 0.25, "Cell Text Height", dxxPropertyTypes.dxf_Double)
                    _OBJECT_TABLESTYLE_CELL.Add(170, 5, "Cell Alignment")
                    _OBJECT_TABLESTYLE_CELL.Add(62, dxxColors.ByBlock, "Cell Color", dxxPropertyTypes.Color)
                    _OBJECT_TABLESTYLE_CELL.Add(63, dxxColors.BlackWhite, "Cell Fill Color", dxxPropertyTypes.Color)
                    _OBJECT_TABLESTYLE_CELL.Add(283, False, "Cell Background Color Switch", dxxPropertyTypes.Switch)
                    _OBJECT_TABLESTYLE_CELL.Add(90, 0, "Cell Data Type")
                    _OBJECT_TABLESTYLE_CELL.Add(91, 0, "Cell Unit Type")
                    _OBJECT_TABLESTYLE_CELL.Add(1, "", "Cell Value", dxxPropertyTypes.dxf_String)
                    For i = 1 To 6
                        _OBJECT_TABLESTYLE_CELL.Add(gc1, dxxLineWeights.ByBlock, $"LineWeight Border {i}")
                        _OBJECT_TABLESTYLE_CELL.Add(gc2, True, $"Visible Flag Border {i}", dxxPropertyTypes.Switch)
                        _OBJECT_TABLESTYLE_CELL.Add(gc3, dxxColors.ByBlock, $"Color Border {i}")
                        gc1 += 1
                        gc2 += 1
                        gc3 += 1
                    Next i
                    _OBJECT_TABLESTYLE_CELL.Append(Get_ObjectCommon.HiddenMembers)
                End If
                Return New TPROPERTIES(_OBJECT_TABLESTYLE_CELL)
            End Get
        End Property

        Private Shared _OBJECT_VISUALSTYLE As TPROPERTIES
        Friend Shared ReadOnly Property Props_Object_VISUALSTYLE As TPROPERTIES
            Get
                '^the default properties for ACAD_PROXY_OBJECT
                If _OBJECT_VISUALSTYLE.Count = 0 Then ' If Props_Object_PROXYOBJECT.IsEmpty Then
                    _OBJECT_VISUALSTYLE = Get_ObjectCommon("VISUALSTYLE", "AcDbVisualStyle")
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(2, "", "Description"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(70, 7, "Style Type"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(71, 1, "Face Light Model"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(72, 0, "Face Light Quality"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(73, 1, "Face Color Mode"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(90, 0, "Face Modifiers"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(40, -0.6, "Face Opacity Level"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(41, -30.0#, "Face Specular Level"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(62, 5, "Color 1"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(63, 7, "Color 2"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(421, 16777215, "Face Style Mono Color"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(74, 0, "Edge Style Model"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(91, 4, "Edge Style"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(64, 7, "Edge Intersection Color"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(65, 257, "Edge Obscured Color"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(75, 1, "Edge Obscured Linetype"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(175, 1, "Edge Intersection Linetype"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(42, 1.0#, "Edge Crease Angle"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(92, 8, "Edge Modifiers"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(66, 7, "Edge Color"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(424, 0, "Color??", 0))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(43, 1.0#, "Edge Opacity Level"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(76, 1, "Edge Width"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(77, 6, "Edge Overhang"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(78, 2, "Edge Jitter"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(67, 7, "Edge Silhouette Color"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(79, 5, "Edge Silhouette Width"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(170, 0, "Edge Halo Gap"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(171, 0, "Edge Isoline Count"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(290, 0, "Edge Hide Precision Flag", dxxPropertyTypes.Switch))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(174, 0, "Edge Style Apply Flag"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(93, 1, "Display Settings Flag"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(44, 0#, "Brightness"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(173, 0, "Shadow Type"))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(291, 0, "Internal Use Only Flag", dxxPropertyTypes.Switch))
                    _OBJECT_VISUALSTYLE.Add(New TPROPERTY(45, 0#, "Single?"))
                    _OBJECT_VISUALSTYLE.ReIndex(True)
                End If
                Return New TPROPERTIES(_OBJECT_VISUALSTYLE)
            End Get
        End Property
        Private Shared _OBJECT_SORTENTSTABLE As TPROPERTIES
        Friend Shared ReadOnly Property Props_Object_SORTENTSTABLE() As TPROPERTIES
            Get
                '^the default properties for SORTENTSTABLE
                If _OBJECT_SORTENTSTABLE.Count = 0 Then
                    _OBJECT_SORTENTSTABLE = Get_ObjectCommon("SORTENTSTABLE", "AcDbSortentsTable")
                    _OBJECT_SORTENTSTABLE.Add(330, "", "Space Handle")
                    _OBJECT_SORTENTSTABLE.ReIndex(True)
                End If
                Return New TPROPERTIES(_OBJECT_SORTENTSTABLE)
            End Get
        End Property

        Private Shared _OBJECT_XRECORD As TPROPERTIES
        Friend Shared ReadOnly Property Props_Object_XRECORD() As TPROPERTIES
            Get

                If _OBJECT_XRECORD.Count = 0 Then ' If Props_Object_PROXYOBJECT.IsEmpty Then
                    _OBJECT_XRECORD = Get_ObjectCommon("XRECORD", "AcDbXrecord")
                    _OBJECT_XRECORD.Add(280, 1, "Duplicate Cloning Flag")
                    _OBJECT_XRECORD.ReIndex(True)
                End If
                Return New TPROPERTIES(_OBJECT_XRECORD)
            End Get
        End Property
#End Region 'Internal Properties


#Region "Internal Shared Methods"
        Friend Shared Function Get_BlockProps(Optional aName As String = "", Optional aGUID As String = "", Optional aBlock As dxfBlock = Nothing) As TPROPERTIES

            Dim _rVal As TPROPERTIES = Props_Block
            If aBlock IsNot Nothing Then aName = aBlock.Name
            _rVal.GCValueSet(2, aName)
            _rVal.GCValueSet(3, aName)
            If aGUID <> "" Then _rVal.GUID = aGUID
            If aBlock Is Nothing Then Return _rVal
            _rVal.GCValueSet(5, aBlock.Handle)
            _rVal.GCValueSet(330, aBlock.BlockRecordHandle)

            _rVal.GCValueSet(67, IIf(aBlock.Name.ToUpper.StartsWith("*PAPER_"), 1, 0))
            _rVal.GCValueSet(8, aBlock.LayerName)
            _rVal.GCValueSet(70, aBlock.TypeFlag)
            _rVal.SetVectorGC(10, aBlock.InsertionPtV)
            _rVal.GCValueSet(1, aBlock.XRefPath)
            _rVal.GCValueSet(4, aBlock.Description)
            _rVal.GUID = aBlock.GUID
            Return _rVal
        End Function

        Friend Shared Function Get_CommonProps(aEntityType As String) As TPROPERTIES
            Dim _rVal As TPROPERTIES = Props_CommonEnt

            _rVal.Name = aEntityType.Trim().ToUpper()
            Dim p1 As TPROPERTY = _rVal.Item(1)
            p1.Value = _rVal.Name
            _rVal.SetItem(1, p1)
            Return _rVal
        End Function

        Friend Shared Function Get_CommonProps(aEntityType As String, ByRef rHiddenIndex As Integer) As TPROPERTIES

            Dim _rVal As TPROPERTIES = Get_CommonProps(aEntityType)
            rHiddenIndex = _CommonHidden
            Return _rVal
        End Function
        Friend Shared Function Get_TextProperties(Optional aEntTypeName As String = "") As TPROPERTIES
            Dim _rVal As New TPROPERTIES(Props_DTEXT)

            If Not String.IsNullOrWhiteSpace(aEntTypeName) Then
                _rVal.Name = aEntTypeName.Trim().ToUpper()
                _rVal.SetValGC(0, _rVal.Name)

            End If
            Return _rVal
        End Function
        Friend Shared Function GetReferenceProperties(aEntryType As dxxReferenceTypes, Optional aName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aDescription As String = "", Optional aHandle As String = "", Optional aGUID As String = "", Optional aProps As TPROPERTIES? = Nothing) As dxoProperties

            If String.IsNullOrWhiteSpace(aGUID) Then aGUID = dxfEvents.NextEntryGUID(aEntryType) Else aGUID = aGUID.Trim()
            If String.IsNullOrWhiteSpace(aName) Then aEntryType.DefaultMemberName()
            Dim rProps As dxoProperties = Nothing
            Select Case aEntryType
                Case dxxReferenceTypes.APPID
                    rProps = Properties_Entry_APPID(aName, aGUID)
                Case dxxReferenceTypes.VPORT

                    rProps = Properties_Entry_VPORT(aName, aGUID)
                Case dxxReferenceTypes.BLOCK_RECORD
                    rProps = Properties_Entry_BLOCK_RECORD(aName, aGUID)
                Case dxxReferenceTypes.LTYPE

                    Dim bSuppressDefaults = TLISTS.Contains(aName, "Continuous,ByBlock,ByLayer,Invisible")
                    If Not bSuppressDefaults Then
                        rProps = New dxoProperties(dxfLinetypes.GetCurrentDefinition(aName).Props)
                    Else
                        rProps = Properties_Entry_LTYPE(aName, aGUID, aDescription)
                    End If
                Case dxxReferenceTypes.LAYER

                    rProps = Properties_Entry_LAYER(aName, aGUID)

                Case dxxReferenceTypes.STYLE

                    rProps = Properties_Entry_STYLE(aName, aGUID)
                Case dxxReferenceTypes.DIMSTYLE

                    rProps = Properties_Entry_DIMSTYLE(aName, aGUID)
                Case dxxReferenceTypes.UCS

                    rProps = Properties_Entry_UCS(aName, aGUID)
                Case dxxReferenceTypes.VIEW

                    rProps = Properties_Entry_VIEW(aName, aGUID)

                Case Else
                    rProps = New dxoProperties("UNDEFINED")
            End Select
            rProps.FileObjectType = dxxFileObjectTypes.TableEntry
            rProps.Name = dxfEnums.DisplayName(aEntryType).ToUpper
            If aProps.HasValue Then
                rProps.CopyVals(aProps.Value, aGCsToSkip:=Nothing, False, bSkipHandles:=False, bSkipPointers:=False)
            End If
            If Not String.IsNullOrWhiteSpace(aHandle) Then rProps.Handle = aHandle
            If aColor <> dxxColors.Undefined Then rProps.SetVal("Color", aColor)
            If Not String.IsNullOrWhiteSpace(aName) Then rProps.SetVal("Name", aName)
            If Not String.IsNullOrWhiteSpace(aGUID) Then rProps.GUID = aGUID

            Select Case aEntryType
                Case dxxReferenceTypes.LTYPE
                    If String.Compare(aName, dxfLinetypes.Continuous, True) = 0 Then
                        rProps.SetVal("Description", "Solid Line")
                    ElseIf String.Compare(aName, dxfLinetypes.ByBlock, True) = 0 Then
                        rProps.SetVal("Description", "Linetype Inherited From Owning Block")
                    ElseIf String.Compare(aName, dxfLinetypes.ByLayer, True) = 0 Then
                        rProps.SetVal("Description", "Linetype Inherited From Owning Layer")
                    End If

            End Select

            Return rProps
        End Function
        Friend Shared Function GetSettingsProperties(aSettingType As dxxSettingTypes, Optional aName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aHandle As String = "", Optional aGUID As String = "", Optional aProps As TPROPERTIES? = Nothing) As dxoProperties

            If String.IsNullOrWhiteSpace(aGUID) Then aGUID = dxfEvents.NextSettingGUID(aSettingType) Else aGUID = aGUID.Trim()

            Dim rProps As dxoProperties = Nothing
            Select Case aSettingType

                Case dxxSettingTypes.DIMSETTINGS
                    rProps = Properties_Setting_DIM(aGUID)
                Case dxxSettingTypes.LINETYPESETTINGS
                    rProps = New dxoProperties("LinetypeSettings", True) From {New dxoProperty(1, dxxLinetypeLayerFlag.ForceToLayer, "Setting", dxxPropertyTypes.dxf_Integer, bNonDXF:=True), New dxoProperty(2, aGUID, "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True}}
                Case dxxSettingTypes.HEADER
                    rProps = Properties_Settings_HEADER(aGUID)
                Case dxxSettingTypes.SCREENSETTINGS
                    rProps = Properties_Setting_SCREEN(aGUID)
                Case dxxSettingTypes.SYMBOLSETTINGS
                    rProps = Properties_Setting_SYMBOL(aGUID)
                Case dxxSettingTypes.TABLESETTINGS
                    rProps = Properties_Setting_TABLE(aGUID)
                Case dxxSettingTypes.TEXTSETTINGS
                    rProps = Properties_Setting_TEXT(aGUID)
                Case dxxSettingTypes.DIMOVERRIDES
                    rProps = Properties_Setting_DIMOVERRIDES("Standard", aGUID)
                    aName = "Standard"
                Case Else
                    rProps = New dxoProperties("UNDEFINED")
            End Select
            rProps.FileObjectType = dxxFileObjectTypes.TableEntry
            rProps.Name = dxfEnums.DisplayName(aSettingType).ToUpper
            If aProps.HasValue Then
                rProps.CopyVals(aProps.Value, aGCsToSkip:=Nothing, False, bSkipHandles:=False, bSkipPointers:=False)
            End If
            If Not String.IsNullOrWhiteSpace(aHandle) Then rProps.Handle = aHandle
            If aColor <> dxxColors.Undefined Then rProps.SetVal("Color", aColor)
            If Not String.IsNullOrWhiteSpace(aName) Then rProps.SetVal("Name", aName)
            If Not String.IsNullOrWhiteSpace(aGUID) Then rProps.GUID = aGUID


            Return rProps
        End Function
        Friend Shared Function GetReferenceProps(aEntryType As dxxReferenceTypes, Optional aName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aString As String = "", Optional aHandle As String = "") As TPROPERTIES
            Dim rProps As TPROPERTIES
            aName = aName.Trim()
            Select Case aEntryType
                Case dxxReferenceTypes.APPID
                    rProps = dxpProperties.Props_Entry_APPID
                Case dxxReferenceTypes.VPORT
                    If String.IsNullOrWhiteSpace(aName) Then aName = "*Active"
                    rProps = dxpProperties.Props_Entry_VPORT
                Case dxxReferenceTypes.BLOCK_RECORD
                    rProps = dxpProperties.Props_Entry_BLOCK_RECORD
                Case dxxReferenceTypes.LTYPE
                    If String.IsNullOrWhiteSpace(aName) Then aName = dxfLinetypes.Continuous
                    Dim bSuppressDefaults = TLISTS.Contains(aName, "Continuous,ByBlock,ByLayer,Invisible")
                    If Not bSuppressDefaults Then rProps = dxfLinetypes.GetCurrentDefinition(aName).Props.Clone Else rProps = dxpProperties.Props_Entry_LTYPE
                    If Not String.IsNullOrWhiteSpace(aString) Then rProps.SetVal("Description", aString)
                Case dxxReferenceTypes.LAYER
                    If String.IsNullOrWhiteSpace(aName) Then aName = "0"
                    rProps = dxpProperties.Props_Entry_LAYER
                Case dxxReferenceTypes.STYLE
                    If String.IsNullOrWhiteSpace(aName) Then aName = "Standard"
                    rProps = dxpProperties.Props_Entry_STYLE
                Case dxxReferenceTypes.DIMSTYLE
                    If String.IsNullOrWhiteSpace(aName) Then
                        aName = "Standard"
                    End If
                    rProps = dxpProperties.Props_Entry_DIMSTYLE
                Case dxxReferenceTypes.UCS
                    If String.IsNullOrWhiteSpace(aName) Then aName = "World"
                    rProps = dxpProperties.Props_Entry_UCS
                Case dxxReferenceTypes.VIEW
                    If String.IsNullOrWhiteSpace(aName) Then aName = "Top"
                    rProps = dxpProperties.Props_Entry_VIEW
                Case dxxSettingTypes.SCREENSETTINGS
                    rProps = dxpProperties.Props_Setting_SCREEN
                Case dxxSettingTypes.DIMSETTINGS
                    rProps = dxpProperties.Props_Setting_DIM
                Case dxxSettingTypes.SYMBOLSETTINGS
                    rProps = dxpProperties.Props_Setting_SYMBOL
                Case dxxSettingTypes.TABLESETTINGS
                    rProps = dxpProperties.Props_Setting_TABLE
                Case dxxSettingTypes.TEXTSETTINGS
                    rProps = dxpProperties.Props_Setting_TEXT
                Case dxxSettingTypes.LINETYPESETTINGS
                    rProps = New TPROPERTIES("LinetypeSettings", True)
                    rProps.Add(New TPROPERTY(1, dxxLinetypeLayerFlag.ForceToLayer, "Setting", dxxPropertyTypes.dxf_Integer, bNonDXF:=True))

                Case dxxSettingTypes.HEADER
                    aName = "HEADER"
                    rProps = New TPROPERTIES(dxpProperties.Properties_Settings_HEADER(""))
                Case Else
                    rProps = New TPROPERTIES("UNDEFINED")
            End Select
            rProps.Name = dxfEnums.DisplayName(aEntryType).ToUpper

            If Not String.IsNullOrWhiteSpace(aHandle) Then rProps.Handle = aHandle
            If aColor <> dxxColors.Undefined Then rProps.SetVal("Color", aColor)
            If String.IsNullOrWhiteSpace(aName) Then Return rProps
            rProps.SetVal("Name", aName)
            Select Case aEntryType
                Case dxxReferenceTypes.LTYPE
                    If String.Compare(aName, dxfLinetypes.Continuous, True) = 0 Then
                        rProps.SetVal("Description", "Solid Line")
                    ElseIf String.Compare(aName, dxfLinetypes.ByBlock, True) = 0 Then
                        rProps.SetVal("Description", "Linetype Inherited From Owning Block")
                    ElseIf String.Compare(aName, dxfLinetypes.ByLayer, True) = 0 Then
                        rProps.SetVal("Description", "Linetype Inherited From Owning Layer")
                    End If

            End Select



            Return rProps
        End Function

        Friend Shared Function Get_ObjectCommon(Optional aName As String = "", Optional aSubClassName As String = "") As TPROPERTIES
            Dim _rVal As TPROPERTIES = Props_CommonObj
            _rVal.SetValGC(0, aName)
            _rVal.SetValGC(100, aSubClassName)

            Return _rVal
        End Function

        Friend Shared Function Get_TableProps(aTableType As dxxReferenceTypes) As TPROPERTIES
            Try
                Dim sClass As String = String.Empty
                Dim tName As String = aTableType.ToString
                sClass = TTABLE.GetSubClassName(aTableType)
                Dim rProps As New TPROPERTIES($"TABLE-{tName}")
                rProps.Add(New TPROPERTY(0, "TABLE", "Table Type", dxxPropertyTypes.dxf_String))
                rProps.Add(New TPROPERTY(2, tName, "Entry Type", dxxPropertyTypes.dxf_String))
                rProps.Add(New TPROPERTY(5, "", "Handle", dxxPropertyTypes.Handle))
                rProps.Add(New TPROPERTY(330, "0", "Pointer", dxxPropertyTypes.Pointer))
                rProps.Add(New TPROPERTY(100, "AcDbSymbolTable", "Table Record SubClass Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                rProps.Add(New TPROPERTY(70, 0, "Member Count", dxxPropertyTypes.dxf_Integer))
                Select Case aTableType
                    Case dxxReferenceTypes.DIMSTYLE
                        rProps.Add(New TPROPERTY(100, "AcDbDimStyleTable", "Table Reference SubClass Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                        rProps.Add(New TPROPERTY(71, 0, "Primary Member Count", 0, dxxPropertyTypes.dxf_Integer))
                End Select

                rProps.Add(New TPROPERTY(1, "", "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True})
                Return rProps
            Catch ex As Exception
                Throw ex
            End Try

        End Function
        Friend Shared Function Properties_Table(aTableType As dxxReferenceTypes, Optional aGUID As String = "") As dxoProperties
            Try
                Dim sClass As String = String.Empty
                Dim tName As String = aTableType.ToString
                sClass = TTABLE.GetSubClassName(aTableType)
                Dim rProps As New dxoProperties($"TABLE-{tName}") From
                    {
                        New dxoProperty(0, "TABLE", "Table Type", dxxPropertyTypes.dxf_String),
                          New dxoProperty(2, tName, "Entry Type", dxxPropertyTypes.dxf_String),
                        New dxoProperty(5, "", "Handle", dxxPropertyTypes.Handle),
                        New dxoProperty(330, "0", "Pointer", dxxPropertyTypes.Pointer),
                        New dxoProperty(100, "AcDbSymbolTable", "Table Record SubClass Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True},
                        New dxoProperty(70, 0, "Member Count", dxxPropertyTypes.dxf_Integer)
                    }

                Select Case aTableType
                    Case dxxReferenceTypes.DIMSTYLE
                        rProps.Add(New dxoProperty(100, "AcDbDimStyleTable", "Table Reference SubClass Marker", dxxPropertyTypes.ClassMarker) With {.DoNotCopy = True})
                        rProps.Add(New dxoProperty(71, 0, "Primary Member Count", 0, dxxPropertyTypes.dxf_Integer))
                End Select

                rProps.Add(New dxoProperty(1, aGUID, "*GUID", dxxPropertyTypes.dxf_String, bNonDXF:=True) With {.DoNotCopy = True})
                rProps.FileObjectType = dxxFileObjectTypes.Table
                Return rProps
            Catch ex As Exception
                Throw ex
            End Try

        End Function
        Friend Shared Function Get_EntityProperties(aGraphicType As dxxGraphicTypes, aGUID As String, Optional aHandle As String = "", Optional aOwnerHandle As String = "", Optional aLayerName As String = "", Optional aSpaceFlag As Boolean? = Nothing, Optional aTextType As dxxTextTypes = dxxTextTypes.Undefined) As dxoProperties
            Return New dxoProperties(Get_EntityProps(aGraphicType, aGUID, aHandle, aOwnerHandle, aLayerName, aSpaceFlag, aTextType))
        End Function


        Friend Shared Function Get_EntityProps(aGraphicType As dxxGraphicTypes, aGUID As String, Optional aHandle As String = "", Optional aOwnerHandle As String = "", Optional aLayerName As String = "", Optional aSpaceFlag As Boolean? = Nothing, Optional aTextType As dxxTextTypes = dxxTextTypes.Undefined) As TPROPERTIES
            Dim rProps As New TPROPERTIES("ENT PROPS")
            Select Case aGraphicType
                Case dxxGraphicTypes.Arc
                    rProps = dxpProperties.Props_ARC
                Case dxxGraphicTypes.Bezier
                    rProps = dxpProperties.Props_BEZIER
                Case dxxGraphicTypes.Dimension
                    rProps = dxpProperties.Props_DIMENSION
                Case dxxGraphicTypes.Ellipse
                    rProps = dxpProperties.Props_ELLIPSE
                Case dxxGraphicTypes.Hatch
                    rProps = dxpProperties.Props_HATCH
                Case dxxGraphicTypes.Hole
                    rProps = dxpProperties.Props_HOLE
                Case dxxGraphicTypes.Insert
                    rProps = dxpProperties.Props_INSERT
                Case dxxGraphicTypes.Leader
                    rProps = dxpProperties.Props_LEADER
                Case dxxGraphicTypes.Line
                    rProps = dxpProperties.Props_LINE
                Case dxxGraphicTypes.Point
                    rProps = dxpProperties.Props_POINT
                Case dxxGraphicTypes.Polygon
                    rProps = dxpProperties.Props_POLYGON
                Case dxxGraphicTypes.Polyline
                    rProps = dxpProperties.Props_LWPOLYLINE
                Case dxxGraphicTypes.EndBlock
                    rProps = dxpProperties.Props_ENDBLK
                Case dxxGraphicTypes.Shape
                    rProps = dxpProperties.Props_SHAPE
                Case dxxGraphicTypes.Solid
                    rProps = dxpProperties.Props_SOLID
                Case dxxGraphicTypes.Symbol
                    rProps = dxpProperties.Props_SYMBOL
                Case dxxGraphicTypes.Table
                    rProps = dxpProperties.Props_TABLE
                Case dxxGraphicTypes.Text
                    Select Case aTextType
                        Case dxxTextTypes.AttDef
                            rProps = dxpProperties.Props_ATTDEF
                        Case dxxTextTypes.Attribute
                            rProps = dxpProperties.Props_ATTRIB
                        Case Else
                            rProps = dxpProperties.Props_DTEXT
                    End Select

                Case dxxGraphicTypes.SequenceEnd
                    rProps = dxpProperties.Props_SEQEND

                Case dxxGraphicTypes.MText
                    rProps = dxpProperties.Props_MTEXT
                Case dxxGraphicTypes.Viewport
                    rProps = dxpProperties.Props_VIEWPORT
            End Select
            If rProps.Count = 0 Then
                rProps = Get_CommonProps(dxfEnums.Description(aGraphicType))
            End If
            rProps.Owner = aGUID
            rProps.SetVal("*GUID", aGUID)
            rProps.SetVal("*GraphicType", aGraphicType)
            rProps.SetVal("*EntityType", aGraphicType.EntityType())
            rProps.Name = aGUID
            If aSpaceFlag.HasValue Then rProps.SetVal("IsPaperSpace", aSpaceFlag.Value)
            If aHandle <> "" Then rProps.Handle = aHandle
            If aOwnerHandle <> "" Then rProps.SetVal("Owner Handle", aOwnerHandle)
            If aLayerName <> "" Then rProps.SetVal("LayerName", aLayerName)
            Return rProps
        End Function




        Friend Shared Function DimStyleProperty(aDimStyleProp As dxxDimStyleProperties) As TPROPERTY
            Dim rProp As New TPROPERTY(aDimStyleProp.PropertyName())
            Dim pt As dxxPropertyTypes = aDimStyleProp.PropertyType()
            Dim def As Object = ""
            Dim sup As Boolean = False
            Dim vc As mzValueControls = mzValueControls.Undefined
            Dim gc As Integer = aDimStyleProp.GroupCode()
            Dim max As Double? = Nothing
            Dim min As Double? = Nothing
            Dim dcd As String = String.Empty
            Dim evt As Type = Nothing

            rProp.EnumName = aDimStyleProp
            rProp.Description = dxfEnums.Description(aDimStyleProp)


            Select Case pt
                Case dxxPropertyTypes.Switch
                    def = False
                Case dxxPropertyTypes.Pointer
                    def = "0"
                Case dxxPropertyTypes.dxf_String, dxxPropertyTypes.Handle, dxxPropertyTypes.ClassMarker, dxxPropertyTypes.dxf_Variant
                    def = ""
                Case dxxPropertyTypes.dxf_Single, dxxPropertyTypes.dxf_Double, dxxPropertyTypes.Angle
                    def = 0.0D
                Case dxxPropertyTypes.dxf_Integer, dxxPropertyTypes.dxf_Long, dxxPropertyTypes.BitCode
                    def = 0
                Case dxxPropertyTypes.dxf_Boolean
                    def = False

                Case dxxPropertyTypes.Color
                    def = dxxColors.ByLayer
                    vc = mzValueControls.Positive
            End Select


            Select Case aDimStyleProp
                Case dxxDimStyleProperties.DIMPOST ' = 7
                    sup = True
                Case dxxDimStyleProperties.DIMAPOST ' = 8
                    sup = True
                Case dxxDimStyleProperties.DIMSCALE ' = 9
                    def = 1 : vc = mzValueControls.PositiveNonZero
                Case dxxDimStyleProperties.DIMASZ ' = 10
                    def = 0.18 : sup = True
                Case dxxDimStyleProperties.DIMEXO ' = 11
                    def = 0.0625 : sup = True
                Case dxxDimStyleProperties.DIMDLI ' = 12
                    def = 0.38 : sup = True
                Case dxxDimStyleProperties.DIMEXE ' = 13
                    def = 0.18 : sup = True
                Case dxxDimStyleProperties.DIMRND ' = 14
                    sup = True
                Case dxxDimStyleProperties.DIMDLE ' = 15
                    sup = True
                Case dxxDimStyleProperties.DIMTP ' = 16
                    def = 0.1 : sup = True : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMTM ' = 17
                    def = 0.1 : sup = True : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMFXL ' = 18
                    def = 1 : sup = True : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMJOGANG ' = 19
                    def = Math.PI * 45 / 180 : sup = True : vc = mzValueControls.Positive
                    min = 5 * Math.PI / 180 : max = 90 * Math.PI / 180
                Case dxxDimStyleProperties.DIMTFILL ' = 20
                    def = 1 : sup = True : vc = mzValueControls.PositiveNonZero
                    max = 2 : min = 0
                Case dxxDimStyleProperties.DIMTFILLCLR ' = 21
                    def = dxxColors.ByBlock : sup = True : vc = mzValueControls.None
                    max = 256 : min = 0
                Case dxxDimStyleProperties.DIMTOL ' = 22
                    sup = True
                Case dxxDimStyleProperties.DIMLIM ' = 23
                    sup = True
                Case dxxDimStyleProperties.DIMTIH ' = 24
                    def = True : sup = True
                Case dxxDimStyleProperties.DIMTOH ' = 25
                    def = True : sup = True
                Case dxxDimStyleProperties.DIMSE1 ' = 26
                    sup = True
                Case dxxDimStyleProperties.DIMSE2 ' = 27
                    sup = True
                Case dxxDimStyleProperties.DIMTAD ' = 28
                    def = dxxDimTadSettings.Centered : sup = True : vc = mzValueControls.Positive
                    min = 0 : max = 4 : evt = GetType(dxxDimTadSettings)
                    evt = GetType(dxxDimTadSettings)
                Case dxxDimStyleProperties.DIMZIN ' = 29
                    def = dxxZeroSuppression.None : sup = True : vc = mzValueControls.Positive
                    min = 0 : max = 15 : evt = GetType(dxxZeroSuppression)
                    'this one is Linear And architecteral combined
                Case dxxDimStyleProperties.DIMAZIN ' = 30
                    def = dxxZeroSuppression.None : sup = True : vc = mzValueControls.Positive
                    min = 0 : max = 12 : evt = GetType(dxxZeroSuppression)
                    evt = GetType(dxxZeroSuppression)
                Case dxxDimStyleProperties.DIMARCSYM ' = 31
                    sup = True : vc = mzValueControls.Positive
                    min = 0 : max = 2
                Case dxxDimStyleProperties.DIMTXT ' = 32
                    def = 0.12 : vc = mzValueControls.PositiveNonZero
                Case dxxDimStyleProperties.DIMCEN ' = 33
                    sup = True : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMTSZ ' = 34
                    sup = True : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMALTF ' = 35
                    def = 25.4 : sup = True : vc = mzValueControls.PositiveNonZero
                Case dxxDimStyleProperties.DIMLFAC ' = 36
                    def = 1.0 : vc = mzValueControls.NonZero
                Case dxxDimStyleProperties.DIMTVP ' = 37
                    sup = True : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMTFAC ' = 38
                    def = 0.1 : sup = True : vc = mzValueControls.Positive
                    min = 0
                Case dxxDimStyleProperties.DIMGAP ' = 39
                    def = 0.09 : sup = True : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMALTRND ' = 40
                    sup = True : vc = mzValueControls.Positive
                Case dxxDimStyleProperties.DIMALT ' = 41
                    sup = True : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMALTD ' = 42
                    def = 2 : sup = True : vc = mzValueControls.Positive
                    min = 0 : max = 8
                Case dxxDimStyleProperties.DIMTOFL ' = 43
                    sup = True : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMSAH ' = 44
                    sup = True : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMTIX ' = 45
                    sup = True : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMSOXD ' = 46
                    sup = True : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMCLRD ' = 47
                    def = dxxColors.ByBlock : sup = True : vc = mzValueControls.None
                    evt = GetType(dxxColors)
                Case dxxDimStyleProperties.DIMCLRE ' = 48
                    def = dxxColors.ByBlock : sup = True : vc = mzValueControls.None
                    evt = GetType(dxxColors)
                Case dxxDimStyleProperties.DIMCLRT ' = 49
                    def = dxxColors.ByBlock : sup = True : vc = mzValueControls.None
                    evt = GetType(dxxColors)
                Case dxxDimStyleProperties.DIMADEC ' = 50
                    sup = True : vc = mzValueControls.None
                    min = -1 : max = 8
                Case dxxDimStyleProperties.DIMUNIT ' = 51  (obsolete)
                    def = dxxLinearUnitFormats.Decimals : sup = True : vc = mzValueControls.Positive
                    min = 1 : max = 6 : evt = GetType(dxxLinearUnitFormats)
                Case dxxDimStyleProperties.DIMDEC ' = 52
                    def = 4 : sup = True : vc = mzValueControls.Positive
                    min = 0 : max = 8
                Case dxxDimStyleProperties.DIMTDEC ' = 53
                    def = 4 : sup = True : vc = mzValueControls.Positive
                    min = 0 : max = 8
                Case dxxDimStyleProperties.DIMALTU ' = 54
                    def = dxxLinearUnitFormats.Decimals : sup = True : vc = mzValueControls.Positive
                    min = 1 : max = 6 : evt = GetType(dxxLinearUnitFormats)
                Case dxxDimStyleProperties.DIMALTTD ' = 55
                    def = 2 : sup = True : vc = mzValueControls.Positive
                    min = 0 : max = 8
                Case dxxDimStyleProperties.DIMAUNIT ' = 56
                    def = dxxAngularUnits.DegreesDecimal : sup = True : vc = mzValueControls.Positive
                    min = 0 : max = 3 : evt = GetType(dxxAngularUnits)
                Case dxxDimStyleProperties.DIMFRAC ' = 57
                    def = dxxDimFractionStyles.Horizontal : sup = True : vc = mzValueControls.Positive
                    min = 0 : max = 2 : evt = GetType(dxxDimFractionStyles)
                Case dxxDimStyleProperties.DIMLUNIT ' = 58
                    def = dxxLinearUnitFormats.Decimals : sup = True : vc = mzValueControls.PositiveNonZero
                    min = 1 : max = 6 : evt = GetType(dxxLinearUnitFormats)
                Case dxxDimStyleProperties.DIMDSEP ' = 59
                    def = Asc(".") : sup = True : vc = mzValueControls.Positive
                    min = 0 : max = 256
                Case dxxDimStyleProperties.DIMTMOVE ' = 60
                    def = dxxDimTextMovementTypes.DimLineWithText : sup = True : vc = mzValueControls.Positive
                    min = 0 : max = 2 : evt = GetType(dxxDimTextMovementTypes)
                Case dxxDimStyleProperties.DIMJUST ' = 61
                    def = dxxDimJustSettings.Centered : sup = True : vc = mzValueControls.Positive
                    min = 0 : max = 4 : evt = GetType(dxxDimJustSettings)
                Case dxxDimStyleProperties.DIMSD1 ' = 62
                    sup = True : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMSD2 ' = 63
                    sup = True : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMTOLJ ' = 64
                    def = dxxVerticalJustifications.Center : sup = True : vc = mzValueControls.Positive
                    min = 0 : max = 2 : evt = GetType(dxxVerticalJustifications)
                Case dxxDimStyleProperties.DIMTZIN ' = 65
                    def = dxxZeroSuppression.None : sup = True : vc = mzValueControls.Positive
                    evt = GetType(dxxZeroSuppression)
                Case dxxDimStyleProperties.DIMALTZ ' = 66
                    def = dxxZeroSuppression.None : sup = True : vc = mzValueControls.Positive
                Case dxxDimStyleProperties.DIMALTTZ ' = 67
                    def = dxxZeroSuppression.None : sup = True : vc = mzValueControls.Positive
                    evt = GetType(dxxZeroSuppression)
                Case dxxDimStyleProperties.DIMFIT ' = 68
                    def = dxxDimFit.BestFitWithLeader : sup = True : vc = mzValueControls.Positive
                    evt = GetType(dxxDimFit)
                Case dxxDimStyleProperties.DIMUPT ' = 69
                    sup = True : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMATFIT ' = 70
                    def = dxxDimFit.BestFitWithLeader : sup = True : vc = mzValueControls.Positive
                    evt = GetType(dxxDimFit)
                Case dxxDimStyleProperties.DIMFXLON ' = 71
                    sup = True : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMTXSTY ' = 72
                    sup = True : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMLDRBLK ' = 73
                    sup = True : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMBLK ' = 74
                    sup = True : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMBLK1 ' = 75
                    sup = True : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMBLK2 ' = 76
                    sup = True : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMLTYPE ' = 77
                    sup = True : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMLTEX1 ' = 78
                    sup = True : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMLTEX2 ' = 79
                    sup = True : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMLWD ' = 80
                    def = dxxLineWeights.ByBlock : sup = True : vc = mzValueControls.None
                    evt = GetType(dxxLineWeights)
                Case dxxDimStyleProperties.DIMLWE ' = 81
                    def = dxxLineWeights.ByBlock : sup = True : vc = mzValueControls.None
                    evt = GetType(dxxLineWeights)
                Case dxxDimStyleProperties.DIMTXTDIRECTION
                    def = 0 : sup = True
                    'hidden properties follow
                Case dxxDimStyleProperties.DIMZEROSUPPRESSION
                    def = dxxZeroSuppression.None : vc = mzValueControls.None
                    evt = GetType(dxxZeroSuppression)
                Case dxxDimStyleProperties.DIMTOLZEROSUPPRESSION ' = 90
                    def = dxxZeroSuppression.None : vc = mzValueControls.None
                    evt = GetType(dxxZeroSuppression)
                Case dxxDimStyleProperties.DIMZEROSUPPRESSION_ARCH ' = 91
                    def = dxxZeroSuppressionsArchitectural.ZeroFeetAndZeroInches : vc = mzValueControls.None
                    evt = GetType(dxxZeroSuppressionsArchitectural)
                Case dxxDimStyleProperties.DIMPREFIX ' = 92
                    vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMSUFFIX ' = 93
                    vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMAPREFIX ' = 94
                    vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMASUFFIX ' = 95
                    vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMTANGLE ' = 96
                    vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMTXSTY_NAME ' = 82
                    def = "Standard" : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMLDRBLK_NAME ' = 83
                    def = "_ClosedFilled" : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMBLK_NAME ' = 84
                    def = "_ClosedFilled" : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMBLK1_NAME ' = 85
                    vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMBLK2_NAME ' = 86
                    vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMLTYPE_NAME ' = 87
                    def = dxfLinetypes.ByBlock : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMLTEX1_NAME ' = 88
                    def = dxfLinetypes.ByBlock : vc = mzValueControls.None
                Case dxxDimStyleProperties.DIMLTEX2_NAME ' = 89
                    def = dxfLinetypes.ByBlock : vc = mzValueControls.None
            End Select
            If evt IsNot Nothing And String.IsNullOrEmpty(dcd) Then
                dcd = dxfEnums.ValueNameList(evt)
                rProp.DecodeString = dcd
            End If
            If gc < 0 And Left(rProp.Name, 1) <> "*" Then rProp.Name = "*" & rProp.Name
            rProp.EnumValueType = evt
            rProp.GroupCode = gc
            rProp.Max = max
            rProp.Min = min
            If pt = dxxPropertyTypes.dxf_Single And vc = mzValueControls.Undefined Then vc = mzValueControls.Positive
            If vc = mzValueControls.Undefined Then vc = mzValueControls.None
            rProp.ValueControl = vc
            rProp.PropType = pt
            rProp.SetVal(def)
            rProp.LastValue = rProp.Value
            If sup Then rProp.SuppressedValue = rProp.Value
            rProp.Description = dxfEnums.Description(aDimStyleProp)
            If rProp.Hidden Then
                rProp.NonDXF = True
                If rProp.Name.StartsWith("*") Then rProp.Key = rProp.Name.Substring(1, rProp.Name.Length - 1)

            Else
                'rProp.Key = rProp.Name.Substring(1, rProp.Name.Length - 1)
            End If
            Return rProp
        End Function

        Friend Shared Function HeaderProperty(aHeaderVar As dxxHeaderVars, ByRef ioDSProps As dxoProperties, ByRef rProp2 As dxoProperty, ByRef rProp3 As dxoProperty) As dxoProperty
            Dim pname As String = dxfEnums.PropertyName(aHeaderVar).ToUpper
            Dim rProp As New dxoProperty(pname)
            Dim enugc As Integer = dxfEnums.GroupCode(aHeaderVar)
            If ioDSProps Is Nothing Then ioDSProps = DimStyleProps
            rProp2 = Nothing
            rProp3 = Nothing
            rProp.GroupCode = enugc
            If aHeaderVar <> dxxHeaderVars.DIMASO And aHeaderVar <> dxxHeaderVars.DIMSHO And aHeaderVar <> dxxHeaderVars.DIMSTYLE Then
                If pname.StartsWith("$DIM") Then
                    If ioDSProps.Count <= 0 Then ioDSProps = DimStyleProps
                    Dim dspname As String = pname.Substring(1, pname.Length - 1)
                    Dim aProp As dxoProperty = Nothing
                    If dspname <> "DIMASSOC" Then aProp = ioDSProps.Item(dspname)
                    If aProp IsNot Nothing Then
                        rProp = New dxoProperty(aProp)
                        rProp.Name = pname
                        rProp._EnumName = aHeaderVar
                        rProp.GroupCode = enugc
                        'these are handles for dimstyles so switch them to strings
                        If aProp.GroupCode >= 300 And aProp.GroupCode <= 347 Then
                            rProp.PropertyType = dxxPropertyTypes.dxf_String
                            Select Case aHeaderVar
                                Case dxxHeaderVars.DIMBLK, dxxHeaderVars.DIMBLK1, dxxHeaderVars.DIMBLK2, dxxHeaderVars.DIMLDRBLK
                                    rProp.Value = "_ClosedFilled"
                                Case dxxHeaderVars.DIMTXSTY
                                    rProp.Value = "Standard"
                                Case dxxHeaderVars.DIMLTYPE, dxxHeaderVars.DIMLTEX1, dxxHeaderVars.DIMLTEX2
                                    rProp.Value = dxfLinetypes.ByBlock
                            End Select
                            rProp.SuppressedValue = ""
                        End If
                        Return rProp
                    End If
                End If
            End If


            rProp._EnumName = aHeaderVar
            rProp.Description = dxfEnums.Description(aHeaderVar)
            Dim def As Object = ""
            Dim isvector As Boolean = False
            Dim isdirection As Boolean = False
            Dim pt As dxxPropertyTypes = dxfEnums.HeaderPropertyType(aHeaderVar, isvector, isdirection)
            Dim sup As Boolean = False
            Dim vc As mzValueControls = mzValueControls.Undefined
            Dim gc As Integer
            Dim max As Double? = Nothing
            Dim min As Double? = Nothing
            Dim dcd As String = String.Empty
            Dim evt As Type = Nothing
            Dim b2D As Boolean = False

            Select Case pt
                Case dxxPropertyTypes.Switch
                    def = False
                Case dxxPropertyTypes.dxf_String, dxxPropertyTypes.Handle, dxxPropertyTypes.Pointer, dxxPropertyTypes.ClassMarker, dxxPropertyTypes.dxf_Variant
                    def = ""
                Case dxxPropertyTypes.dxf_Single, dxxPropertyTypes.dxf_Double, dxxPropertyTypes.Angle
                    def = 0.0D
                Case dxxPropertyTypes.dxf_Integer, dxxPropertyTypes.dxf_Long, dxxPropertyTypes.BitCode
                    def = 0
                Case dxxPropertyTypes.dxf_Boolean
                    def = False

                Case dxxPropertyTypes.Color
                    def = dxxColors.ByLayer
                    vc = mzValueControls.Positive
            End Select

            Select Case aHeaderVar
                Case dxxHeaderVars.ACADVER ' = 1
                    gc = 1 : def = "AC1021"
                Case dxxHeaderVars.ACADMAINTVER ' = 2
                    gc = 90 : def = 26
                Case dxxHeaderVars.DWGCODEPAGE ' = 3
                    gc = 3 : def = "ANSI_1252"
                Case dxxHeaderVars.LASTSAVEDBY, dxxHeaderVars.HYPERLINKBASE, dxxHeaderVars.STYLESHEET, dxxHeaderVars.PROJECTNAME
                    gc = 1
                Case dxxHeaderVars.INSBASE, dxxHeaderVars.PINSBASE
                    gc = 10 : def = TVECTOR.Zero
                Case dxxHeaderVars.EXTMIN, dxxHeaderVars.PEXTMIN
                    gc = 10 : def = New TVECTOR(10 ^ 20, 10 ^ 20, 10 ^ 20)
                Case dxxHeaderVars.EXTMAX, dxxHeaderVars.PEXTMAX
                    gc = 10 : def = New TVECTOR(-10 ^ 20, -10 ^ 20, -10 ^ 20)
                Case dxxHeaderVars.LIMMIN, dxxHeaderVars.PLIMMIN, dxxHeaderVars.LIMMAX, dxxHeaderVars.PLIMMAX
                    gc = 10 : def = TVECTOR.Zero : b2D = True
                Case dxxHeaderVars.ORTHOMODE ' = 10
                    gc = 70
                Case dxxHeaderVars.REGENMODE ' = 11
                    gc = 70 : def = True
                Case dxxHeaderVars.FILLMODE ' = 12
                    gc = 70 : def = True
                Case dxxHeaderVars.QTEXTMODE ' = 13
                    gc = 70
                Case dxxHeaderVars.MIRRTEXT ' = 14
                    gc = 70 : def = True
                Case dxxHeaderVars.LTSCALE ' = 15
                    gc = 40 : def = 1.0 : vc = mzValueControls.PositiveNonZero
                Case dxxHeaderVars.ATTMODE ' = 16
                    gc = 70 : def = dxxAttributeVisibilityModes.Normal
                    evt = GetType(dxxAttributeVisibilityModes)
                Case dxxHeaderVars.TEXTSIZE ' = 17
                    gc = 40 : def = 0.2 : vc = mzValueControls.PositiveNonZero
                Case dxxHeaderVars.TRACEWID ' = 18
                    gc = 40 : def = 0.05 : vc = mzValueControls.PositiveNonZero
                Case dxxHeaderVars.TEXTSTYLE ' = 19
                    gc = 7 : def = "Standard"
                Case dxxHeaderVars.CLAYER ' = 20
                    gc = 8 : def = "0"
                Case dxxHeaderVars.CELTYPE ' = 21
                    gc = 6 : def = dxfLinetypes.ByLayer
                Case dxxHeaderVars.CECOLOR ' = 22
                    gc = 62
                Case dxxHeaderVars.CELTSCALE ' = 23
                    gc = 40 : def = 1.0 : vc = mzValueControls.PositiveNonZero
                Case dxxHeaderVars.DISPSILH ' = 24
                    gc = 70
                Case dxxHeaderVars.DIMASSOC
                    gc = 200 : def = 2 : vc = mzValueControls.Positive
                    min = 0 : max = 2
                Case dxxHeaderVars.DIMASO, dxxHeaderVars.DIMSHO
                    gc = 70 : def = True

                Case dxxHeaderVars.DIMSTYLE ' = 61
                    gc = 2 : def = "Standard"
                Case dxxHeaderVars.LUNITS ' = 101
                    gc = 70 : def = dxxLinearUnitFormats.Decimals : vc = mzValueControls.PositiveNonZero
                    min = 1 : max = 5
                    evt = GetType(dxxAttributeVisibilityModes)
                Case dxxHeaderVars.LUPREC ' = 102
                    gc = 70 : def = 4 : vc = mzValueControls.Positive
                    min = 0 : max = 8
                Case dxxHeaderVars.SKETCHINC ' = 103
                    gc = 40 : def = 0.1 : vc = mzValueControls.None
                Case dxxHeaderVars.FILLETRAD ' = 104
                    gc = 40 : vc = mzValueControls.Positive
                Case dxxHeaderVars.AUNITS ' = 105
                    gc = 70 : def = dxxAngularUnits.DegreesDecimal : vc = mzValueControls.Positive
                    min = 0 : max = 3
                    evt = GetType(dxxAngularUnits)
                Case dxxHeaderVars.AUPREC ' = 106
                    gc = 70 : vc = mzValueControls.Positive
                    min = 0 : max = 8
                Case dxxHeaderVars.MENU ' = 107
                    gc = 1 : def = "." : vc = mzValueControls.None
                Case dxxHeaderVars.ELEVATION, dxxHeaderVars.PELEVATION, dxxHeaderVars.THICKNESS ' = 108-110
                    gc = 40 : vc = mzValueControls.None
                Case dxxHeaderVars.LIMCHECK, dxxHeaderVars.PLIMCHECK
                    gc = 70
                Case dxxHeaderVars.CHAMFERA, dxxHeaderVars.CHAMFERB
                    gc = 40 : def = 0.5 : vc = mzValueControls.Positive
                Case dxxHeaderVars.CHAMFERC
                    gc = 40 : def = 1.0 : vc = mzValueControls.Positive
                Case dxxHeaderVars.CHAMFERD
                    gc = 40 : vc = mzValueControls.Positive
                Case dxxHeaderVars.SKPOLY ' = 116
                    gc = 70
                Case dxxHeaderVars.TDCREATE, dxxHeaderVars.TDUCREATE, dxxHeaderVars.TDUPDATE, dxxHeaderVars.TDUUPDATE, dxxHeaderVars.TDINDWG, dxxHeaderVars.TDUSRTIMER ' = 117-122
                    gc = 40 : vc = mzValueControls.None
                Case dxxHeaderVars.USRTIMER ' = 123
                    gc = 70 : def = True
                Case dxxHeaderVars.ANGBASE ' = 124
                    gc = 50
                Case dxxHeaderVars.ANGDIR ' = 125
                    gc = 70 : def = dxxAngularDirections.CounterClockwise : vc = mzValueControls.Positive
                    min = 0 : max = 1
                    evt = GetType(dxxAngularDirections)
                Case dxxHeaderVars.PDMODE ' = 126
                    gc = 70 : def = dxxPointModes.Dot : vc = mzValueControls.Positive
                    min = dxxPointModes.Dot : max = dxxPointModes.CircSqrTick : evt = GetType(dxxPointModes)
                Case dxxHeaderVars.PDSIZE ' = 127
                    gc = 40 : vc = mzValueControls.None
                Case dxxHeaderVars.PLINEWID ' = 128
                    gc = 40 : vc = mzValueControls.Positive
                Case dxxHeaderVars.SPLFRAME ' = 128
                    gc = 70
                Case dxxHeaderVars.SPLINETYPE ' = 130
                    gc = 70 : def = dxxSplineTypes.Cubic : vc = mzValueControls.PositiveNonZero
                    min = 5 : max = 6 : evt = GetType(dxxSplineTypes)
                Case dxxHeaderVars.SPLINESEGS ' = 131
                    gc = 70 : def = 8 : vc = mzValueControls.None
                    min = -32768 : max = 32767
                Case dxxHeaderVars.HANDSEED ' = 133
                    gc = 5 : def = "0"
                Case dxxHeaderVars.SURFTAB1, dxxHeaderVars.SURFTAB2 ' = 134-135
                    gc = 70 : def = 6 : vc = mzValueControls.PositiveNonZero
                    min = 2 : max = 32766
                Case dxxHeaderVars.SURFTYPE ' = 136
                    gc = 70 : def = 6 : vc = mzValueControls.Positive
                    min = 0 : max = 32767
                Case dxxHeaderVars.SURFU, dxxHeaderVars.SURFV ' = 137,138
                    gc = 70 : def = 6 : vc = mzValueControls.Positive
                    min = 0 : max = 200
                Case dxxHeaderVars.UCSBASE, dxxHeaderVars.UCSNAME, dxxHeaderVars.PUCSBASE, dxxHeaderVars.PUCSNAME
                    gc = 2
                Case dxxHeaderVars.UCSORG, dxxHeaderVars.PUCSORG
                    gc = 10 : def = TVECTOR.Zero
                Case dxxHeaderVars.UCSXDIR, dxxHeaderVars.PUCSXDIR ' = 142
                    gc = 10 : def = TVECTOR.WorldX
                Case dxxHeaderVars.UCSYDIR, dxxHeaderVars.PUCSYDIR ' = 143
                    gc = 10 : def = TVECTOR.WorldY
                Case dxxHeaderVars.UCSORTHOREF, dxxHeaderVars.PUCSORTHOREF ' = 144
                    gc = 2
                Case dxxHeaderVars.UCSORTHOVIEW, dxxHeaderVars.PUCSORTHOVIEW
                    gc = 70 : def = dxxOrthograpicViews.Undefined
                    min = 0 : max = 6 : evt = GetType(dxxOrthograpicViews)
                Case dxxHeaderVars.UCSORGTOP, dxxHeaderVars.UCSORGBOTTOM, dxxHeaderVars.UCSORGLEFT, dxxHeaderVars.UCSORGRIGHT, dxxHeaderVars.UCSORGFRONT, dxxHeaderVars.UCSORGBACK
                    gc = 10 : def = TVECTOR.Zero
                Case dxxHeaderVars.PUCSORGTOP, dxxHeaderVars.PUCSORGBOTTOM, dxxHeaderVars.PUCSORGLEFT, dxxHeaderVars.PUCSORGRIGHT, dxxHeaderVars.PUCSORGFRONT, dxxHeaderVars.PUCSORGBACK
                    gc = 10 : def = TVECTOR.Zero
                Case dxxHeaderVars.USERI1, dxxHeaderVars.USERI2, dxxHeaderVars.USERI3, dxxHeaderVars.USERI4, dxxHeaderVars.USERI5
                    gc = 70
                Case dxxHeaderVars.USERR1, dxxHeaderVars.USERR2, dxxHeaderVars.USERR3, dxxHeaderVars.USERR4, dxxHeaderVars.USERR5
                    gc = 70
                Case dxxHeaderVars.WORLDVIEW ' = 175
                    gc = 70 : def = True
                Case dxxHeaderVars.SHADEDGE ' = 176
                    gc = 70 : def = dxxShadeEdgeSettings.FaceAndEdges : vc = mzValueControls.Positive
                    min = 0 : max = 3 : evt = GetType(dxxShadeEdgeSettings)
                Case dxxHeaderVars.SHADEDIF ' = 177
                    gc = 70 : def = 70 : vc = mzValueControls.PositiveNonZero
                    min = 1 : max = 100
                Case dxxHeaderVars.TILEMODE ' = 178
                    gc = 70 : def = True
                Case dxxHeaderVars.MAXACTVP ' = 179
                    gc = 70 : def = 64 : vc = mzValueControls.PositiveNonZero
                    min = 2 : max = 64
                Case dxxHeaderVars.UNITMODE, dxxHeaderVars.PLINEGEN, dxxHeaderVars.MEASUREMENT, dxxHeaderVars.LWDISPLAY
                    gc = 70
                Case dxxHeaderVars.PSLTSCALE, dxxHeaderVars.VISRETAIN, dxxHeaderVars.PROXYGRAPHICS
                    gc = 70 : def = True
                Case dxxHeaderVars.TREEDEPTH ' = 190
                    gc = 70 : def = 3020 : vc = mzValueControls.None
                    min = -32768 : max = 32767
                Case dxxHeaderVars.CMLSTYLE
                    gc = 2 : def = "Standard"
                Case dxxHeaderVars.CMLJUST ' = 192
                    gc = 70 : def = dxxVerticalJustifications.Bottom : vc = mzValueControls.Positive
                    min = 0 : max = 3 : evt = GetType(dxxVerticalJustifications)
                Case dxxHeaderVars.CMLSCALE ' = 193
                    gc = 40 : def = 1.0 : vc = mzValueControls.None
                Case dxxHeaderVars.CELWEIGHT ' = 196
                    gc = 370 : def = dxxLineWeights.ByLayer
                Case dxxHeaderVars.ENDCAPS ' = 197
                    gc = 280 : def = dxxEndCaps.None : vc = mzValueControls.Positive
                    min = 0 : max = 3 : evt = GetType(dxxEndCaps)
                Case dxxHeaderVars.JOINSTYLE ' = 198
                    gc = 280 : def = dxxJoinStyles.None : vc = mzValueControls.Positive
                    min = 0 : max = 3 : evt = GetType(dxxJoinStyles)
                Case dxxHeaderVars.INSUNITS ' = 200
                    gc = 70 : def = dxxDrawingUnitBasis.Inches : vc = mzValueControls.Positive
                    min = 0 : max = 20 : evt = GetType(dxxDrawingUnitBasis)
                Case dxxHeaderVars.XEDIT, dxxHeaderVars.PSTYLEMODE, dxxHeaderVars.EXTNAMES, dxxHeaderVars.HIDETEXT
                    gc = 290 : def = True
                Case dxxHeaderVars.CEPSNID ' = 132
                    gc = 390 : def = "0" : sup = True
                Case dxxHeaderVars.CEPSNTYPE ' = 204
                    gc = 380 : def = dxxPlotStyleTypes.ByLayer : vc = mzValueControls.Positive
                    min = 0 : max = 3 : evt = GetType(dxxPlotStyleTypes)
                Case dxxHeaderVars.FINGERPRINTGUID
                    gc = 2 : def = "{1EA0BA20-4020-11D7-B716-0002A559B82A}"
                Case dxxHeaderVars.VERSIONGUID
                    gc = 2 : def = "{4D1CF5B6-90EE-430C-A181-1CCD4F9101A7}"
                Case dxxHeaderVars.PSVPSCALE, dxxHeaderVars.NORTHDIRECTION
                    gc = 40 : vc = mzValueControls.Positive
                Case dxxHeaderVars.OLESTARTUP, dxxHeaderVars.INTERSECTIONDISPLAY, dxxHeaderVars.CAMERADISPLAY
                    gc = 290
                Case dxxHeaderVars.SORTENTS ' = 211
                    gc = 280 : def = dxxSortEntsCode.All : vc = mzValueControls.Positive
                    evt = GetType(dxxSortEntsCode)
                Case dxxHeaderVars.INDEXCTL ' = 212
                    gc = 280 : vc = mzValueControls.Positive
                    min = 0 : max = 3
                Case dxxHeaderVars.XCLIPFRAME ' = 214
                    gc = 290 : vc = mzValueControls.Positive
                    min = 0 : max = 2
                Case dxxHeaderVars.HALOGAP ' = 215
                    gc = 280 : vc = mzValueControls.Positive
                    min = 0 : max = 100
                Case dxxHeaderVars.OBSCOLOR, dxxHeaderVars.INTERSECTIONCOLOR
                    gc = 70 : def = 257 : vc = mzValueControls.Positive
                    min = 0 : max = 257
                Case dxxHeaderVars.OBSLTYPE ' = 217
                    gc = 280 : def = dxxLineStyles.Off : vc = mzValueControls.Positive
                    min = 0 : max = 11 : evt = GetType(dxxLineStyles)
                Case dxxHeaderVars.INTERSECTIONDISPLAY
                    gc = 280
                Case dxxHeaderVars.SOLIDHIST
                    gc = 280 : def = True
                Case dxxHeaderVars.DIMASSOC ' = 220
                    gc = 280 : def = 2 : vc = mzValueControls.Positive
                    min = 0 : max = 2
                Case dxxHeaderVars.LENSLENGTH ' = 223
                    gc = 40 : def = 50.0 : vc = mzValueControls.PositiveNonZero
                Case dxxHeaderVars.CAMERAHEIGHT ' = 224
                    gc = 40 : vc = mzValueControls.None
                Case dxxHeaderVars.STEPSPERSEC ' = 225
                    gc = 40 : def = 2.0 : vc = mzValueControls.PositiveNonZero
                    min = 1 : max = 30
                Case dxxHeaderVars.STEPSIZE ' = 226
                    gc = 40 : def = 6.0 : vc = mzValueControls.None
                    min = 10 ^ -6 : max = 10 ^ 6
                Case dxxHeaderVars.THREEDDWFPREC ' = 227
                    gc = 40 : def = 2.0 : vc = mzValueControls.None
                Case dxxHeaderVars.PSOLWIDTH ' = 228
                    gc = 40 : def = 0.25 : vc = mzValueControls.PositiveNonZero
                Case dxxHeaderVars.PSOLHEIGHT ' = 229
                    gc = 40 : def = 4.0 : vc = mzValueControls.PositiveNonZero
                Case dxxHeaderVars.LOFTANG1, dxxHeaderVars.LOFTANG2
                    gc = 40 : def = Math.PI / 2 : vc = mzValueControls.Positive
                    min = 0 : max = 2 * Math.PI
                Case dxxHeaderVars.LOFTMAG1, dxxHeaderVars.LOFTMAG2
                    gc = 40 : vc = mzValueControls.Positive
                Case dxxHeaderVars.LOFTPARAM ' = 234
                    gc = 70 : def = dxxLoftParams.NoTwist + dxxLoftParams.NoTwist + dxxLoftParams.Align : vc = mzValueControls.Positive
                    evt = GetType(dxxLoftParams)
                Case dxxHeaderVars.LOFTNORMALS ' = 235
                    gc = 280 : def = dxxLoftNormals.SmoothFit : vc = mzValueControls.Positive
                    min = 0 : max = 6 : evt = GetType(dxxLoftNormals)
                Case dxxHeaderVars.LATITUDE
                    gc = 40 : def = 37.795 : vc = mzValueControls.None
                Case dxxHeaderVars.LONGITUDE
                    gc = 40 : def = -122.394 : vc = mzValueControls.None
                Case dxxHeaderVars.TIMEZONE ' = 239
                    gc = 70 : def = dxxTimeZones.Pacific : vc = mzValueControls.None
                    evt = GetType(dxxTimeZones)
                Case dxxHeaderVars.LIGHTGLYPHDISPLAY, dxxHeaderVars.TILEMODELIGHTSYNCH
                    gc = 280 : def = True
                Case dxxHeaderVars.CMATERIAL ' = 242
                    gc = 347
                Case dxxHeaderVars.SHOWHIST
                    gc = 280 : def = 1 : vc = mzValueControls.Positive
                    min = 0 : max = 2
                Case dxxHeaderVars.DWFFRAME ' = 245
                    gc = 280 : def = 2 : vc = mzValueControls.Positive
                    min = 0 : max = 2
                Case dxxHeaderVars.DGNFRAME ' = 246
                    gc = 280 : vc = mzValueControls.Positive
                    min = 0 : max = 2
                Case dxxHeaderVars.REALWORLDSCALE ' = 247
                    gc = 40 : def = 1.0 : vc = mzValueControls.None
                Case dxxHeaderVars.INTERFERECOLOR ' = 248
                    gc = 62 : def = dxxColors.Red = 1
                Case dxxHeaderVars.INTERFEREOBJVS ' = 249
                    gc = 345 : def = "0"
                Case dxxHeaderVars.INTERFEREVPVS ' = 250
                    gc = 346 : def = "0"
                Case dxxHeaderVars.CSHADOW ' = 251
                    gc = 280 : def = dxxShadowModes.CastReceives : vc = mzValueControls.Positive
                    min = 0 : max = 3 : evt = GetType(dxxShadowModes)
                Case dxxHeaderVars.SHADOWPLANELOCATION ' = 252
                    gc = 40 : vc = mzValueControls.None
                Case dxxHeaderVars.UCSMODE ' = (-1 * 1)
                    gc = -1 : def = dxxUCSIconModes.Origin : vc = mzValueControls.Positive
                    min = 0 : max = 2 : evt = GetType(dxxUCSIconModes)
                Case dxxHeaderVars.UCSSIZE ' = (-1 * 2)
                    gc = -2 : def = 5 : vc = mzValueControls.PositiveNonZero
                    min = 5 : max = 50
                Case dxxHeaderVars.UCSCOLOR ' = (-1 * 3)
                    gc = -3 : def = dxxColors.BlackWhite
                Case dxxHeaderVars.LWDEFAULT ' = (-1 * 4)
                    gc = -4 : def = dxxLineWeights.LW_025 : vc = mzValueControls.Positive
                    min = 0 : max = 211 : evt = GetType(dxxLineWeights)
                Case dxxHeaderVars.LWSCALE ' = (-1 * 5)
                    gc = -5 : def = 0.5 : vc = mzValueControls.PositiveNonZero
                    max = 1
            End Select


            If evt IsNot Nothing And String.IsNullOrEmpty(dcd) Then
                dcd = dxfEnums.ValueNameList(evt)
                rProp.DecodeString = dcd
            End If
            If gc < 0 And Left(rProp.Name, 1) <> "*" Then rProp.Name = "*" & rProp.Name
            rProp._EnumValueType = evt
            If enugc = 0 Then rProp.GroupCode = gc Else rProp.GroupCode = enugc


            If (pt = dxxPropertyTypes.dxf_Single Or pt = dxxPropertyTypes.dxf_Double) And vc = mzValueControls.Undefined Then vc = mzValueControls.Positive
            If vc = mzValueControls.Undefined Then vc = mzValueControls.None
            rProp.ValueControl = vc
            rProp.PropertyType = pt
            rProp._EnumValueType = aHeaderVar.GetType()
            If rProp.Hidden Then
                rProp.NonDXF = True
                If rProp.Name.StartsWith("*") Then rProp.Key = rProp.Name.Substring(1, rProp.Name.Length - 1)

            Else
                rProp.Key = rProp.Name.Substring(1, rProp.Name.Length - 1)
            End If
            rProp.SetVal(def)





            If rProp.Description = "" Then rProp.Description = dxfEnums.Description(aHeaderVar)
            rProp.GroupName = rProp.Name

            If isvector Then
                Dim v As TVECTOR = DirectCast(def, TVECTOR)
                rProp.PropertyType = dxxPropertyTypes.dxf_Double
                rProp.IsOrdinate = True
                rProp.SetVal(v.X)
                If sup Then rProp.SuppressedValue = rProp.Value
                rProp2 = New dxoProperty(rProp) With {.Key = $"{rProp.Key}_Y"}

                rProp2.Name = $"{rProp.Name}_Y"
                rProp2.Value = v.Y
                rProp2.GroupCode += 10
                rProp2.GroupName = rProp.Name


                If Not b2D Then

                    rProp3 = New dxoProperty(rProp) With {.Key = $"{rProp.Key}_Z"}

                    rProp3.Name = $"{rProp.Name}_Z"
                    rProp3.Value = v.X
                    rProp3.GroupCode += 20
                    rProp3.GroupName = rProp.Name
                End If
            Else
                rProp.SetVal(def)
                If sup Then rProp.SuppressedValue = rProp.Value
                If max IsNot Nothing Then rProp.Max = max
                If max IsNot Nothing Then rProp.Min = min

            End If
            rProp.LastValue = rProp.Value

            Return rProp
        End Function

        Friend Shared Function Get_ObjectProperties(aObjectType As dxxObjectTypes, Optional aName As String = "", Optional aPropertyString As String = "", Optional aHandle As String = "") As TPROPERTIES

            aName = aName.Trim()
            Dim rProps As New TPROPERTIES(aName)
            Select Case aObjectType
                Case dxxObjectTypes.MLeaderStyle
                    rProps = dxpProperties.Props_Object_MLEADERSTYLE
                    If aPropertyString <> "" Then rProps.SetSuppressionByGC(aPropertyString, False, True, "0, 1, 5, 100, 330")
                Case dxxObjectTypes.SortEntsTable
                    'rProps = Props_Object_SORTENTSTABLE()
                Case dxxObjectTypes.Material
                    rProps = dxpProperties.Props_Object_MATERIAL
                    If aPropertyString <> "" Then rProps.SetSuppressionByGC(aPropertyString, False, True, "0, 1, 5, 100, 330")
                Case dxxObjectTypes.Scale
                    rProps = dxpProperties.Props_Object_SCALE
                    If aPropertyString <> "" Then rProps.UpdateByString(aPropertyString)
                Case dxxObjectTypes.XRecord
                    rProps = dxpProperties.Props_Object_XRECORD
                    If aPropertyString <> "" Then rProps.AddByString(aPropertyString)
                Case dxxObjectTypes.VisualStyle
                    rProps = dxpProperties.Props_Object_VISUALSTYLE
                    If aPropertyString <> "" Then rProps.UpdateByString(aPropertyString)
                Case dxxObjectTypes.DictionaryVar
                    rProps = dxpProperties.Props_Object_DICTIONARYVAR
                    If aPropertyString <> "" Then rProps.UpdateByString(aPropertyString)
                Case dxxObjectTypes.PlaceHolder
                    rProps = dxpProperties.Props_Object_PLACEHOLDER
                Case dxxObjectTypes.CellStyleMap
                    rProps = dxpProperties.Props_Object_CELLSTYPEMAP
                Case dxxObjectTypes.Dictionary
                    rProps = dxpProperties.Props_Object_DICTIONARY
                Case dxxObjectTypes.DictionaryWDFLT
                    rProps = dxpProperties.Props_Object_DICTIONARYWDFLT
                Case dxxObjectTypes.Layout
                    rProps = dxpProperties.Props_Object_LAYOUT
                Case dxxObjectTypes.PlotSetting
                    rProps = dxpProperties.Props_Object_PLOTSETTINGS
                Case dxxObjectTypes.ProxyObject
                    rProps = dxpProperties.Props_Object_PROXYOBJECT
                Case dxxObjectTypes.TableStyle
                    rProps = dxpProperties.Props_Object_TABLESTYLE
                Case dxxObjectTypes.TableCell
                    rProps = dxpProperties.Props_Object_TABLESTYLE_CELL
                    If aPropertyString <> "" Then rProps.UpdateByString(aPropertyString)
                Case dxxObjectTypes.Group
                    rProps = dxpProperties.Props_Object_Group
                    If Not String.IsNullOrEmpty(aName) Then rProps.SetVal("Description", aName)
                Case dxxObjectTypes.MLineStyle
                    rProps = dxpProperties.Props_Object_MLINESTYLE
                    'If aPropertyString <> "" Then rProps.AddByString(aPropertyString)
                Case Else
                    rProps = Get_ObjectCommon(aName)
            End Select
            If aName <> "" Then
                rProps.SetVal("Name", aName)
                If rProps.Count > 0 Then rProps.SetVal("*NAME", aName)
            End If
            If aHandle <> "" Then
                rProps.SetVal("Handle", aHandle)
            End If
            Return rProps
        End Function
#End Region 'Internal Shared Methods

#Region "Methods"





#End Region 'Methods

    End Class
End Namespace
