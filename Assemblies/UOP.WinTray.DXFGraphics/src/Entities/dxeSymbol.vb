Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxeSymbol
        Inherits dxfEntity
#Region "Members"
#End Region 'Members
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxGraphicTypes.Symbol)
            Style = New TTABLEENTRY(dxxSettingTypes.SYMBOLSETTINGS) With {.IsCopied = True}
        End Sub

        Public Sub New(aEntity As dxeSymbol, Optional bCloneInstances As Boolean = False)
            MyBase.New(dxxGraphicTypes.Symbol, aEntityToCopy:=aEntity, bCloneInstances:=bCloneInstances)


        End Sub

        Friend Sub New(aSubEntity As TENTITY, Optional bNewGUID As Boolean = False, Optional aSettings As dxoSettingsSymbol = Nothing)
            MyBase.New(aSubEntity, bNewGUID:=bNewGUID)
            If aSettings IsNot Nothing Then
                Properties.CopyVals(aSettings.Properties, bByName:=True)
            End If
        End Sub

        Friend Sub New(aObject As TOBJECT)
            MyBase.New(dxxGraphicTypes.Symbol)
            DefineByObject(aObject)
        End Sub
        Friend Sub New(aSymbolType As dxxSymbolTypes, aSettings As TTABLEENTRY, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aName As String = "")
            MyBase.New(dxxGraphicTypes.Symbol, aDisplaySettings)
            Dim sty As TTABLEENTRY = New TTABLEENTRY(dxxSettingTypes.SYMBOLSETTINGS)
            If aSettings.EntryType = dxxSettingTypes.SYMBOLSETTINGS And aSettings.Props.Count > 0 Then
                Dim myprops As New TPROPERTIES(Properties)
                myprops.CopyByName(aSettings.Props, bCopyHidden:=False)
                SetProps(myprops)
                sty.Props = aSettings.Props.Clone
            End If
            sty.IsCopied = True
            SetPropVal("BlockName", aName, False)
            SymbolType = aSymbolType
            ArrowHeadType = dxxArrowHeadTypes.ByStyle
            Style = sty

        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Property ArrowHeadType As dxxArrowHeadTypes
            '^the type of arrow head to use
            Get
                Return PropValueI("ArrowHead", dxxArrowHeadTypes.ClosedFilled)
            End Get
            Set(value As dxxArrowHeadTypes)
                If Not dxfEnums.Validate(GetType(dxxArrowHeadTypes), value, bSkipNegatives:=True) Then Return
                SetPropVal("ArrowHead", value, True)
            End Set
        End Property
        Public Property ArrowSize As Double
            '^the size to apply to the arrows (multiplied by FeatureScale)
            Get
                Return PropValueD("ArrowSize")
            End Get
            Friend Set(value As Double)
                If value >= 0 Then
                    SetPropVal("ArrowSize", value, bDirtyOnChange:=True)
                End If
            End Set
        End Property
        Public Property ArrowTails As dxxArrowTails
            Get
                Return PropValueI("ArrowTails", dxxArrowTails.Undefined)
            End Get
            Set(value As dxxArrowTails)
                If value >= 0 And value <= 2 Then
                    SetPropVal("ArrowTails", value, bDirtyOnChange:=True)
                End If
            End Set
        End Property
        Public Property ArrowStyle As dxxArrowStyles
            '^the style of arrow this is
            Get
                Return PropValueI("ArrowStyle")
            End Get
            Set(value As dxxArrowStyles)
                If value < 0 Or value > dxxArrowStyles.StraightFullOpen Then Return
                SetPropVal("ArrowStyle", value, bDirtyOnChange:=True)
            End Set
        End Property
        Public Property ArrowTextAlignment As dxxRectangularAlignments
            Get
                Return PropValueI("ArrowTextAlignment", dxxRectangularAlignments.TopLeft)
            End Get
            Set(value As dxxRectangularAlignments)
                If value >= dxxRectangularAlignments.TopLeft And value <= dxxRectangularAlignments.BottomRight Then
                    SetPropVal("ArrowTextAlignment", value, bDirtyOnChange:=True)
                End If
            End Set
        End Property
        Public Property ArrowType As dxxArrowTypes
            '^the type of arrow this is
            Get
                Return PropValueI("ArrowType")
            End Get
            Friend Set(value As dxxArrowTypes)
                SetPropVal("ArrowType", value, bDirtyOnChange:=True)
            End Set
        End Property
        Public Property ArrowHead As dxxArrowHeadTypes
            Get
                Return PropValueI("ArrowHead", dxxArrowHeadTypes.ClosedFilled)
            End Get
            Set(value As dxxArrowHeadTypes)
                If Not dxfEnums.Validate(GetType(dxxArrowHeadTypes), value, bSkipNegatives:=True) Then Return
                SetPropVal("ArrowHead", value, bDirtyOnChange:=True)
            End Set
        End Property
        Public Property AxisStyle As Integer
            Get
                Return PropValueI("AxisStyle")
            End Get
            Set(value As Integer)
                SetPropVal("AxisStyle", value, bDirtyOnChange:=True)
            End Set
        End Property
        Public Property BlockName As String
            Get
                Dim _rVal As String = PropValueStr("BlockName")
                If _rVal = "" Then _rVal = Name
                If _rVal = "" Then _rVal = GUID
                Return _rVal
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                SetPropVal("BlockName", value)
            End Set
        End Property
        Public Property BoxText As Boolean
            Get
                Return PropValueB("BoxText")
            End Get
            Set(value As Boolean)
                SetPropVal("BoxText", value, bDirtyOnChange:=True)
            End Set
        End Property
        Public Property BubbleType As dxxBubbleTypes
            '^the type of bubble this is
            Get
                Return PropValueI("BubbleType")
            End Get
            Set(value As dxxBubbleTypes)
                SetPropVal("BubbleType", value, True)
            End Set
        End Property
        Friend ReadOnly Property Entities As colDXFEntities
            Get
                Return New colDXFEntities(PathEntities, aImageGUID:=ImageGUID, bIncludeInstances:=False)
            End Get
        End Property
        Public Property FeatureScale As Double
            Get
                Return PropValueD("FeatureScale", 1)
            End Get
            Set(value As Double)
                SetPropVal("FeatureScale", value, bDirtyOnChange:=True)
            End Set
        End Property
        Public Property Flag1 As Boolean
            Get
                Return PropValueB("Flag1")
            End Get
            Set(value As Boolean)
                SetPropVal("Flag1", value, True)
            End Set
        End Property
        Public Property Flag2 As Boolean
            Get
                Return PropValueB("Flag2")
            End Get
            Set(value As Boolean)
                SetPropVal("Flag2", value, True)
            End Set
        End Property
        Public Property Flag3 As Boolean
            Get
                Return PropValueB("Flag3")
            End Get
            Set(value As Boolean)
                SetPropVal("Flag3", value, True)
            End Set
        End Property
        Public Shadows Property Height As Double
            Get
                Return PropValueD("Height")
            End Get
            Set(value As Double)
                SetPropVal("Height", Math.Abs(value), True)
            End Set
        End Property
        Public Property InsertionPt As dxfVector
            Get
                '^the point of the arrow
                Return DefPts.Vector1
            End Get
            Set(value As dxfVector)
                MoveTo(value)
            End Set
        End Property
        Friend Property InsertionPtV As TVECTOR
            Get
                '^the point of the arrow
                Return DefPts.VectorGet(1)
            End Get
            Set(value As TVECTOR)
                DefPts.VectorSet(1, value)
            End Set
        End Property
        Public Property Rotation As Double
            Get
                Return PropValueD("Rotation")
            End Get
            Set(value As Double)
                SetPropVal("Rotation", TVALUES.NormAng(value, False, True), True)
            End Set
        End Property
        Public Property Span As Double
            Get
                Return PropValueD("Span")
            End Get
            Set(value As Double)
                SetPropVal("Span", Math.Abs(value), True)
            End Set
        End Property

        Public Property SymbolType As dxxSymbolTypes
            '^the type of symbol this is
            '~arrow, bubble, weld symbol etc.
            Get
                Return PropValueI("SymbolType")
            End Get
            Set(value As dxxSymbolTypes)
                If value >= dxxSymbolTypes.Arrow And value <= dxxSymbolTypes.Weld Then
                    SetPropVal("SymbolType", value, True)
                End If
            End Set
        End Property
        Public Property Text1 As String
            '^the text that is written above the arrow shaft
            Get
                Return PropValueStr("Text1")
            End Get
            Set(value As String)
                SetPropVal("Text1", value, True)
            End Set
        End Property
        Public Property Text2 As String
            '^the text that is written below the arrow shaft
            Get
                Return PropValueStr("Text2")
            End Get
            Set(value As String)
                SetPropVal("Text2", value, True)
            End Set
        End Property
        Public Property Text3 As String
            '^the text that is written behind the arrow shaft
            Get
                Return PropValueStr("Text3")
            End Get
            Set(value As String)
                SetPropVal("Text3", value, True)
            End Set
        End Property
        Public Property Text4 As String
            '^the text that is written ahead of the arrow
            Get
                Return PropValueStr("Text4")
            End Get
            Set(value As String)
                SetPropVal("Text4", value, True)
            End Set
        End Property
        Public Property TextGap As Double
            Get
                Return PropValueD("TextGap")
            End Get
            Set(value As Double)
                If value < 0 Then BoxText = True
                SetPropVal("TextGap", Math.Abs(value), True)
            End Set
        End Property
        Public Property TextHeight As Double
            Get
                Return PropValueD("TextHeight")
            End Get
            Set(value As Double)
                SetPropVal("TextHeight", Math.Abs(value), True)
            End Set
        End Property
        Public Property TextColor As dxxColors
            '^the text color to apply
            Get
                Return PropValueI("TextColor")
            End Get
            Set(value As dxxColors)
                If value <> dxxColors.Undefined Then
                    SetPropVal("TextColor", value, True)
                End If
            End Set
        End Property
        Public Property LineColor As dxxColors
            '^the line color to apply
            Get
                Return PropValueI("LineColor")
            End Get
            Set(value As dxxColors)
                If value <> dxxColors.Undefined Then
                    SetPropVal("LineColor", value, True)
                End If
            End Set
        End Property
        Friend WriteOnly Property TextStyle As dxoStyle
            Set(value As dxoStyle)
                If Not value Is Nothing Then TextStyleName = value.Name
            End Set
        End Property
        Public Overrides Property TextStyleName As String
            Get
                Return PropValueStr("TextStyle", "Standard")
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                If value <> "" Then SetPropVal("TextStyle", value, True)
            End Set
        End Property
        Friend Property VectorsV As TVECTORS
            Get
                Return DefPts.Vectors
            End Get
            Set(value As TVECTORS)
                DefPts.Vectors = value
            End Set
        End Property
        Public Property WeldType As dxxWeldTypes
            '^the type of weld symbol this is
            Get
                Return PropValueI("WeldType")
            End Get
            Set(value As dxxWeldTypes)
                SetPropVal("WeldType", value, SymbolType = dxxSymbolTypes.Weld)
            End Set
        End Property
#End Region 'Properties
#Region "MustOverride Entity Methods"
        Friend Overrides Sub DefineByObject(aObj As TOBJECT, Optional bNoHandles As Boolean = False, Optional aStyle As dxfTableEntry = Nothing, Optional aBlock As dxfBlock = Nothing)
            Reactors.Append(aObj.Reactors, bClear:=True)
            ExtendedData.Append(aObj.ExtendedData, bClear:=True)
            Throw New Exception("dxeSymbols cannot be defined by Object")
        End Sub

        ''' <summary>
        ''' returns a clone of the entity transfered to the passed plane
        ''' </summary>
        ''' <remarks>the entities defining vectors are converted to vectors with respect to the entities OCS plane and then redefines them based on the passed plane </remarks>
        ''' <param name="aPlane"></param>
        '''<param name="aFromPlane">the plane to define the definition points with respect to prior to transfer to the new plane. If not passed the entities OCS is used </param>
        ''' <returns></returns>
        Public Overloads Function TransferedToPlane(aPlane As dxfPlane,optional aFromPlane as dxfPlane = Nothing) As dxeSymbol
            Dim _rVal As dxeSymbol = Me.Clone()
            _rVal.TransferToPlane(aPlane, aFromPlane)
            Return _rVal
        End Function

        ''' <summary>
        ''' Returns a new object with properties matching those of the cloned object
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Function Clone() As dxeSymbol
            Return New dxeSymbol(Me)
        End Function

#End Region 'MustOverride Entity Methods
#Region "Methods"

        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            'On Error Resume Next
            Dim _rVal As New dxoPropertyMatrix(GroupName, ExtendedData)
            rBlock = Nothing
            If Not GetImage(aImage) Then Return _rVal
            If bUpdatePath Or IsDirty Then UpdatePath(False, aImage)
            Dim i As Integer
            Dim iCnt As Integer
            If PathEntities.Count <= 0 Then Return _rVal
            If aInstances Is Nothing Then aInstances = Instances
            If bSuppressInstances Then iCnt = 1 Else iCnt = aInstances.Count + 1
            Dim aIns As dxeInsert = Nothing
            Dim aLdr As dxeLeader = Nothing
            Dim aAttribs As colDXFEntities = Nothing
            Dim aAttVals As dxfAttributes = Nothing
            Dim bLeader As Boolean
            Dim aBlk As dxfBlock = GetBlock(aImage, aAttVals, aAttribs, bLeader)

            'create the insert object
            If aBlk Is Nothing Then Return _rVal
            rBlock = aBlk
            aIns = New dxeInsert With {
                ._Attributes = aAttVals,
                .Block = aBlk,
                .PlaneV = PlaneV,
                .DisplayStructure = DisplayStructure,
                .InsertionPtV = aBlk.InsertionPtV
            }

            If bLeader Then
                If Vertices.Count > 1 Then
                    aLdr = New dxeLeader(dxxLeaderTypes.LeaderBlock) With {
                        .ImageGUID = aImage.GUID,
                        .DisplayStructure = DisplayStructure,
                        .PlaneV = PlaneV,
                        .VectorsV = New TVECTORS(Vertices),
                        .SuppressHook = True
                    }
                    aLdr.DimStyle.FeatureScaleFactor = FeatureScale
                    aLdr.DimStyle.TextStyleName = TextStyleName
                    aLdr.DimStyle.ArrowHeadLeader = ArrowHead
                    aLdr.DimStyle.TextHeight = TextHeight
                    aLdr.DimStyle.ArrowSize = ArrowSize
                    aLdr.DimStyle.DimLineColor = LineColor
                    aLdr.DimStyle.TextColor = TextColor
                    If BoxText Then
                        aLdr.DimStyle.TextGap = -TextGap
                    Else
                        aLdr.DimStyle.TextGap = TextGap
                    End If
                    aLdr.SetPropVal("*BlockName", aBlk.Name)
                    aLdr.BlockGUID = aBlk.GUID
                    aLdr.IsSymbol = True
                    aLdr.HasHook = False
                    aLdr.SuppressArrowHead = aLdr.DimStyle.ArrowSize <= 0
                    aLdr.HasArrowHead = Not aLdr.SuppressArrowHead
                    aLdr.IsDirty = False
                    aLdr.Insert = aIns
                    aLdr.IsDirty = False
                End If
            End If
            For i = 1 To iCnt
                If aInstance <= 0 Or i = aInstance Then
                    If aLdr IsNot Nothing Then
                        _rVal.Append(aLdr.DXFFileProperties(aInstances, aImage, Nothing, bSuppressInstances, bUpdatePath, i))
                    Else
                        _rVal.Append(aIns.DXFFileProperties(aInstances, aImage, Nothing, bSuppressInstances, bUpdatePath, i))
                    End If
                End If
            Next i
            _rVal.Name = "SYMBOL(" & aBlk.Name & ")"
            If iCnt > 1 Then
                _rVal.Name += "-" & (iCnt) & " INSTANCES"
            End If
            Return _rVal
        End Function
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            If aInstances Is Nothing Then
                aInstances = Instances
                aInstances.IsParent = True
                aInstances.ParentPlane = PlaneV
            End If
            GetImage(aImage)
            Return New TPROPERTYARRAY(Entities.DXFProps(aInstances, aInstance, New dxfPlane(aOCS), rTypeName, aImage))
        End Function
        Public Function GetBlock(aImage As dxfImage) As dxfBlock
            Dim rHasLeader As Boolean = False
            Dim rAttVals As dxfAttributes = Nothing
            Dim rAttributes As colDXFEntities = Nothing

            Return GetBlock(aImage, rAttVals, rAttributes, rHasLeader)
        End Function
        Public Function GetBlock(aImage As dxfImage, ByRef rAttVals As dxfAttributes, ByRef rAttributes As colDXFEntities) As dxfBlock
            Dim rHasLeader As Boolean = False
            Return GetBlock(aImage, rAttVals, rAttributes, rHasLeader)
        End Function
        Public Function GetBlock(aImage As dxfImage, ByRef rAttVals As dxfAttributes, ByRef rAttributes As colDXFEntities, ByRef rHasLeader As Boolean) As dxfBlock
            Dim bEnts As New List(Of dxfEntity)
            Dim aEnt As dxfEntity
            Dim aTxt As dxeText
            Dim bTxt As dxeText
            Dim cnt As Integer
            Dim cnt1 As Integer
            Dim bAtts As Boolean
            Dim iGUID As String = String.Empty
            Dim eGUID As String = GUID
            Dim tEnts As List(Of dxeText)
            rAttVals = New dxfAttributes
            rAttributes = New colDXFEntities
            GetImage(aImage)
            If aImage IsNot Nothing Then iGUID = aImage.GUID
            UpdatePath(aImage:=aImage)
            Dim myEnts As colDXFEntities = Entities
            For i As Integer = 1 To myEnts.Count
                aEnt = myEnts.Item(i)
                If aEnt.IsText Then aEnt.Color = TextColor
                If aEnt.Color = dxxColors.ByBlock Then aEnt.Color = Color
                aEnt.LayerName = LayerName
                If String.Compare(aEnt.Identifier, "LEADER.ARROWHEAD", ignoreCase:=True) = 0 Or String.Compare(aEnt.Identifier, "LEADER.POLYLINE", ignoreCase:=True) = 0 Then
                    rHasLeader = True
                Else
                    If Not aEnt.Suppressed Or (aEnt.Suppressed And dxfUtils.RunningInIDE()) Then
                        If aEnt.GraphicType = dxxGraphicTypes.Text And bAtts Then
                            aTxt = aEnt
                            tEnts = aTxt.SubStrings(dxxTextTypes.AttDef)
                            For Each bTxt In tEnts
                                bTxt.OwnerGUID = eGUID
                                If Not aTxt.Suppressed Then
                                    cnt += 1
                                    bTxt.AttributeTag = $"Text{cnt}"
                                Else
                                    cnt1 += 2
                                    bTxt.AttributeTag = $"Text{10 * cnt}"
                                End If
                                bTxt.Prompt = bTxt.AttributeTag
                                rAttributes.Add(bTxt)
                                rAttVals.Add(New dxfAttribute(bTxt))
                                bEnts.Add(bTxt)
                            Next

                        Else
                            bEnts.Add(aEnt)
                        End If
                    End If
                End If
            Next i
            Dim bname As String = BlockName
            If bname = "" Then
                bname = aImage.HandleGenerator.NextSymbolName(SymbolType, ArrowType)
            End If
            Dim _rVal As New dxfBlock(bname) With {.ImageGUID = iGUID, .LayerName = LayerName}
            If SymbolType = dxxSymbolTypes.Weld Or SymbolType = dxxSymbolTypes.Bubble Then
                InsertionPt.Strukture = New TVECTOR(Vertices.LastVector())
            ElseIf SymbolType = dxxSymbolTypes.Arrow Or SymbolType = dxxSymbolTypes.DetailBubble Then
                InsertionPt.Strukture = InsertionPtV
            End If
            _rVal.Entities.Append(bEnts, True)
            aImage.HandleGenerator.AssignTo(_rVal)
            Return _rVal
        End Function
        Public Function GetSubEnt(aIdentifier As String) As dxfEntity
            If String.IsNullOrWhiteSpace(aIdentifier) Then Return Nothing
            UpdatePath()
            Return PathEntities.Find(Function(x) String.Compare(x.Identifier, aIdentifier, True) = 0)

        End Function
        Friend Function Planarize() As TVECTORS
            Dim aPl As TPLANE = PlaneV
            Dim i As Integer
            Dim vrts As TVECTORS
            vrts = VectorsV
            Dim v1 As TVECTOR
            For i = 1 To vrts.Count
                v1 = vrts.Item(i)
                v1.ProjectTo(aPl)
                vrts.SetItem(i, v1)
            Next i
            VectorsV = vrts
            Return vrts
        End Function

#End Region 'Methods
    End Class 'dxeSymbol
End Namespace
