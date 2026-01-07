Imports UOP.DXFGraphics.Utilities
Imports UOP.DXFGraphics.Structures
Imports Vanara.PInvoke
Imports SharpDX.Direct2D1.Effects

Namespace UOP.DXFGraphics
    Public Class dxeLeader
        Inherits dxfEntity
#Region "Members"
#End Region 'Members
#Region "Constructors"
        Public Sub New(aLeaderType As dxxLeaderTypes)
            MyBase.New(dxxGraphicTypes.Leader)
            SetPropVal("Leader Type", aLeaderType, False)
        End Sub

        Public Sub New(aEntity As dxeLeader, Optional bCloneInstances As Boolean = False)
            MyBase.New(dxxGraphicTypes.Leader, aEntityToCopy:=aEntity, bCloneInstances:=bCloneInstances)
        End Sub

        Friend Sub New(aSubEntity As TENTITY, Optional bNewGUID As Boolean = False, Optional aLeaderType As dxxLeaderTypes = dxxLeaderTypes.NoReactor)
            MyBase.New(aSubEntity, bNewGUID:=bNewGUID)
            If dxfEnums.Validate(GetType(dxxLeaderTypes), aLeaderType) Then SetPropVal("Leader Type", aLeaderType, False)
        End Sub

        Friend Sub New(aObject As TOBJECT)
            MyBase.New(dxxGraphicTypes.Leader)
            DefineByObject(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
        Friend ReadOnly Property ArrowBlockHandle As String
            Get
                Return DimStyle.ArrowHeadBlockLeaderHandle
            End Get
        End Property
        Friend ReadOnly Property ArrowBlockName As String
            Get
                Return DimStyle.ArrowHeadBlockLeader
            End Get
        End Property
        Public Property ArrowHeadBlock As String
            Get
                Return DimStyle.ArrowHeadBlockLeader
            End Get
            Set(value As String)
                DimStyle.ArrowHeadBlockLeader = value
            End Set
        End Property
        Public Property ArrowSize As Double
            Get
                Return DimStyle.ArrowSize
            End Get
            Set(value As Double)
                DimStyle.ArrowSize = value
            End Set
        End Property


        ''' <summary>
        ''' a copy of the object which carries the values of insert object attributes
        ''' </summary>
        ''' <returns></returns>
        Friend Property BlockAttributes As dxfAttributes
            Get
                Return MyBase.Attributes
            End Get
            Set(value As dxfAttributes)
                MyBase.Attributes = value
            End Set
        End Property
        Public Property BlockRotation As Double
            Get
                '^the rotation to apply to the inserted block if the leader is a block leader
                Return PropValueD("*BlockRotation")
            End Get
            Set(value As Double)
                '^the rotation to apply to the inserted block if the leader is a block leader
                value = TVALUES.NormAng(value, False, True)
                If SetPropVal("*BlockRotation", value, False) Then
                    If State <> dxxEntityStates.GeneratingPath Then
                        If LeaderType = dxxLeaderTypes.LeaderBlock Then IsDirty = True
                    End If
                End If
            End Set
        End Property
        Public Property BlockName As String
            Get
                Return PropValueStr("*BlockName")
            End Get
            Friend Set(value As String)
                If SetPropVal("*BlockName", value, True) Then
                    BlockGUID = ""
                End If
            End Set
        End Property
        Public Property BlockScale As Double
            Get
                '^the scale factor to apply to the inserted block if the leader is a block leader
                Return PropValueD("*BlockScale")
            End Get
            Set(value As Double)
                '^the scale factor to apply to the inserted block if the leader is a block leader
                If value <= 0 Then value = 1
                If SetPropVal("*BlockScale", value, True) Then
                    If State <> dxxEntityStates.GeneratingPath Then
                        If LeaderType = dxxLeaderTypes.LeaderBlock Then IsDirty = True
                    End If
                End If
            End Set
        End Property
        Public Property FeatureScaleFactor As Double
            '^the scale factor applied to all dimension features
            Get
                Return DimStyle.FeatureScaleFactor
            End Get
            Set(value As Double)
                DimStyle.FeatureScaleFactor = value
            End Set
        End Property
        Public Property HasArrowHead As Boolean
            Get
                UpdatePath()
                Return PropValueB("*HasArrowHead")
            End Get
            Friend Set(value As Boolean)
                SetPropVal("*HasArrowHead", value, False)
            End Set
        End Property
        Public Property HasHook As Boolean
            Get
                UpdatePath()
                Return PropValueB("*HasHook")
            End Get
            Friend Set(value As Boolean)
                SetPropVal("*HasHook", value, False)
            End Set
        End Property
        Friend Property HookDirection As TVECTOR
            Get
                Return PropVector("*Vector2")
            End Get
            Set(value As TVECTOR)
                PropVectorSet("*Vector2", value)
            End Set
        End Property
        Public ReadOnly Property HookLineDirection As Integer
            Get
                Return PropVector("*Vector2").X
            End Get
        End Property
        Public Property Insert As dxeInsert
            Get
                If LeaderType <> dxxLeaderTypes.LeaderBlock Then Return Nothing
                UpdatePath()
                Return DirectCast(ReactorEntity, dxeInsert)
            End Get
            Friend Set(value As dxeInsert)
                If LeaderType = dxxLeaderTypes.LeaderBlock Then
                    ReactorEntity = value
                End If
            End Set
        End Property
        Public Property InsertionPt As dxfVector
            Get
                '^the very last leader point
                Return Vertices.Item(1, True)
            End Get
            Set(value As dxfVector)
                '^the very last leader point
                Dim v1 As TVECTOR
                If Not value Is Nothing Then v1 = value.Strukture
                If Vertices.Count > 0 Then
                    Vertices.Item(1).Strukture = v1
                Else
                    Vertices.AddV(v1)
                End If
            End Set
        End Property
        Public ReadOnly Property IsBlockLeader As Boolean
            Get
                '^returns true if the leader has block entities defined
                '~block entities are added via AddBlockEntity
                Return EntityType = dxxEntityTypes.LeaderBlock
            End Get
        End Property
        Friend Property IsSymbol As Boolean
            Get
                Return PropValueB("*IsSymbol")
            End Get
            Set(value As Boolean)
                SetPropVal("*IsSymbol", value, False)
            End Set
        End Property
        Friend Property LastRef As TVECTOR
            Get
                Return PropVector("*Vector1")
            End Get
            Set(value As TVECTOR)
                PropVectorSet("*Vector1", value)
            End Set
        End Property
        Friend ReadOnly Property LeaderLines As colDXFEntities
            Get
                UpdatePath()
                Return Vertices.ConnectingLines(False)
            End Get
        End Property
        Public ReadOnly Property LeaderType As dxxLeaderTypes
            Get
                Return PropValueI("Leader Type")
            End Get
        End Property
        Public Property MText As dxeText
            Get
                If LeaderType <> dxxLeaderTypes.LeaderText Then Return Nothing
                UpdatePath()
                Return DirectCast(ReactorEntity, dxeText)
            End Get
            Friend Set(value As dxeText)
                If LeaderType <> dxxLeaderTypes.LeaderText Then Return
                ReactorEntity = value

            End Set
        End Property
        Friend Property PopLeft As Boolean
            Get
                Return PropValueB("*PopLeft")
            End Get
            Set(value As Boolean)
                SetPropVal("*PopLeft", value, True)
            End Set
        End Property
        Public Property SuppressArrowHead As Boolean
            Get
                Return Not PropValueB("ArrowHead Flag")
            End Get
            Set(value As Boolean)
                SetPropVal("ArrowHead Flag", Not value, True)
            End Set
        End Property
        Public Property SuppressHook As Boolean
            Get
                Return PropValueB("Hook Flag")
            End Get
            Set(value As Boolean)
                SetPropVal("Hook Flag", value, True)
            End Set
        End Property
        Public Property TextColor As dxxColors
            '^the leader text color
            Get
                Return DimStyle.TextColor
            End Get
            Set(value As dxxColors)
                DimStyle.TextColor = value
            End Set
        End Property
        Public Property TextGap As Double
            Get
                Return DimStyle.TextGap
            End Get
            Set(value As Double)
                DimStyle.TextGap = value
            End Set
        End Property
        Friend Property TextHeight As Double
            Get
                Return PropValueD("*TextHeight")
            End Get
            Set(value As Double)
                value = Math.Abs(value)
                If SetPropVal("*TextHeight", value, True) Then
                    If LeaderType = dxxLeaderTypes.LeaderText Then
                        If MyBase._MText IsNot Nothing Then
                            MyBase._MText.LayerName = value
                        End If

                    End If
                End If
            End Set
        End Property
        Public Property TextJustification As dxxVerticalJustifications
            '^controls the vertical justification of text to the leader point
            '^this value is ignored if the DimStyle.TextPositionV (DIMTAD) = Above
            Get
                Return PropValueI("*TextJustification")
            End Get
            Set(value As dxxVerticalJustifications)
                If value > dxxVerticalJustifications.Top Then value = dxxVerticalJustifications.Top
                If value < dxxVerticalJustifications.Bottom Then value = dxxVerticalJustifications.Bottom
                SetPropVal("*TextJustification", value, True)
            End Set
        End Property
        Friend Property TextLayer As String
            Get
                Return PropValueStr("*TextLayer")
            End Get
            Set(value As String)
                value = Trim(value)
                If SetPropVal("*TextLayer", value, True) Then
                    If LeaderType = dxxLeaderTypes.LeaderText Then
                        If MyBase._MText IsNot Nothing Then
                            MyBase._MText.LayerName = value
                        End If

                    End If
                End If
            End Set
        End Property
        Friend Property TextString As String
            Get
                Return PropValueStr("*TextString")
            End Get
            Set(value As String)
                If SetPropVal("*TextString", value, True) Then
                    If State <> dxxEntityStates.GeneratingPath Then
                        XOffset = 0
                        YOffset = 0
                        IsDirty = True
                        If LeaderType = dxxLeaderTypes.LeaderText Then
                            MyBase._MText = Nothing
                        ElseIf LeaderType = dxxLeaderTypes.LeaderBlock Then
                            MyBase._Insert = Nothing
                        End If
                    End If
                End If
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


        Public Property XOffset As Double
            '^the X offset of the last leader point form the insertion point of the block if the leader is a block leader
            Get
                Return OffsetVector.X
            End Get
            Set(value As Double)
                Dim pname As String = String.Empty
                Dim v1 As TVECTOR
                v1 = OffsetVector(pname)
                If v1.X <> value Then
                    If State <> dxxEntityStates.GeneratingPath Then IsDirty = True
                End If
                v1.X = value
                PropVectorSet(pname, v1)
            End Set
        End Property
        Public Property YOffset As Double
            '^the Y offset of the last leader point form the insertion point of the block if the leader is a block leader
            Get
                Return OffsetVector.Y
            End Get
            Set(value As Double)
                Dim pname As String = String.Empty
                Dim v1 As TVECTOR
                v1 = OffsetVector(pname)
                If v1.Y <> value Then
                    If State <> dxxEntityStates.GeneratingPath Then IsDirty = True
                End If
                v1.Y = value
                PropVectorSet(pname, v1)
            End Set
        End Property
        Public Overrides ReadOnly Property HasSubEntities As Boolean
            Get
                Select Case LeaderType
                    Case dxxLeaderTypes.LeaderText
                        Return Not String.IsNullOrWhiteSpace(TextString)
                    Case dxxLeaderTypes.LeaderBlock
                        Return Not String.IsNullOrWhiteSpace(BlockName)
                    Case Else
                        Return False

                End Select
            End Get
        End Property
        Public Overrides Property Attributes As dxfAttributes
            Get
                If _Attributes Is Nothing Then _Attributes = New dxfAttributes()
                Return MyBase.Attributes
            End Get
            Friend Set(value As dxfAttributes)
                If value Is Nothing Then
                    MyBase.Attributes = New dxfAttributes()
                Else
                    MyBase.Attributes = value
                End If
            End Set
        End Property

        Public Overrides ReadOnly Property EntityType As dxxEntityTypes
            Get
                Dim ltype As dxxLeaderTypes = LeaderType
                Select Case ltype
                    Case dxxLeaderTypes.LeaderBlock
                        Return dxxEntityTypes.LeaderBlock
                    Case dxxLeaderTypes.LeaderText
                        Return dxxEntityTypes.LeaderText
                    Case dxxLeaderTypes.LeaderTolerance
                        Return dxxEntityTypes.LeaderTolerance
                    Case Else
                        Return dxxEntityTypes.Leader
                End Select

            End Get
        End Property

#End Region 'Properties
#Region "MustOverride Entity Methods"

        Friend Overrides Sub DefineByObject(aObj As TOBJECT, Optional bNoHandles As Boolean = False, Optional aStyle As dxfTableEntry = Nothing, Optional aBlock As dxfBlock = Nothing)
            Reactors.Append(aObj.Reactors, bClear:=True)
            ExtendedData.Append(aObj.ExtendedData, bClear:=True)
            Dim verts As colDXFVectors
            Dim aPlane As TPLANE = TPLANE.ArbitraryCS(aObj.Properties.GCValueV(210, TVECTOR.WorldZ))
            Dim v1 As TVECTOR
            SuppressEvents = True

            'get the dim properties
            PlaneV = aPlane
            DisplayStructure = aObj.DisplayVars
            SetPropVal("Leader Type", aObj.Properties.GCValueI(73, dxxLeaderTypes.NoReactor), False)
            SuppressArrowHead = aObj.Properties.GCValueB(71, False)
            v1 = aObj.Properties.GCValueV(213)
            XOffset = v1.X
            YOffset = v1.Y
            HasHook = aObj.Properties.GCValueB(75, False)
            SuppressHook = (EntityType = dxxEntityTypes.LeaderBlock) Or Not HasHook
            'get the vertices
            verts = New colDXFVectors(aObj.Properties.Vertices(10))
            Vertices = verts
            If Not bNoHandles Then Handle = aObj.Properties.GCValueStr(5)
            SuppressEvents = False
            If aStyle IsNot Nothing Then
                If aStyle.GetType() = GetType(dxoDimStyle) Then
                    DimStyle.Properties.CopyVals(aStyle.Properties, bSkipPointers:=False, bSkipHandles:=False)
                End If
                aStyle.IsCopied = True

            End If
        End Sub
        '^If the leader has a dependent entity it will return a list containing it (text or insert)
        Public Overrides Function PersistentSubEntities() As List(Of dxfEntity)
            Dim reactEnt As dxfEntity = ReactorEntity
            If reactEnt Is Nothing Then Return New List(Of dxfEntity) Else Return New List(Of dxfEntity)({reactEnt})
        End Function

        ''' <summary>
        ''' returns a clone of the entity transfered to the passed plane
        ''' </summary>
        ''' <remarks>the entities defining vectors are converted to vectors with respect to the entities OCS plane and then redefines them based on the passed plane </remarks>
        ''' <param name="aPlane"></param>
        '''<param name="aFromPlane">the plane to define the definition points with respect to prior to transfer to the new plane. If not passed the entities OCS is used </param>
        ''' <returns></returns>
        Public Overloads Function TransferedToPlane(aPlane As dxfPlane,optional aFromPlane as dxfPlane = Nothing) As dxeLeader
            Dim _rVal As dxeLeader = Me.Clone()
            _rVal.TransferToPlane(aPlane, aFromPlane)
            Return _rVal
        End Function


        ''' <summary>
        ''' Returns a new object with properties matching those of the cloned object
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Function Clone() As dxeLeader
            Return New dxeLeader(Me)
        End Function
#End Region 'MustOverride Entity Methods
#Region "Methods"


        Friend Function VertexV(aIndex As Integer) As TVECTOR

            Return VectorsV.Item(aIndex)

        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix(GroupName, ExtendedData)
            'On Error Resume Next
            rBlock = Nothing
            If Not GetImage(aImage) Then Return _rVal
            If bUpdatePath Or IsDirty Then UpdatePath(False, aImage)
            Dim i As Integer
            Dim iCnt As Integer
            Dim aOCS As New TPLANE("")
            Dim tname As String = String.Empty
            If aInstances Is Nothing Then aInstances = Instances
            If bSuppressInstances Then iCnt = 1 Else iCnt = aInstances.Count + 1
            For i = 1 To iCnt
                If aInstance <= 0 Or i = aInstance Then _rVal.Add(DXFProps(aInstances, i, aOCS, tname, aImage))
            Next i
            If iCnt > 1 Then
                _rVal.Name = $"{tname }-{iCnt} INSTANCES"
            End If
            Return _rVal
        End Function
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY
            '#1the instance to create the properties for
            '#2the OCS plane of the entity (created for first instance)
            '^returns the entities properties updated for output to a DXF file
            GetImage(aImage)
            aInstance = Math.Abs(aInstance)
            If aInstance <= 0 Then aInstance = 1
            _rVal = New TPROPERTYARRAY(aInstance:=aInstance)
            Dim myProps As New TPROPERTIES(Properties)
            myProps = myProps.Clone
            Dim aPl As TPLANE = PlaneV
            Dim aTrs As New TTRANSFORMS()
            Try
                If aInstance > 1 Then
                    Dim scl As Double = 1
                    Dim ang As Double = 0
                    Dim bInv As Boolean
                    Dim bLft As Boolean
                    aTrs = New TTRANSFORMS
                    If aInstances Is Nothing Then aInstances = Instances
                    aTrs = aInstances.Transformations(aInstance - 1, scl, ang, bInv, bLft)
                    TTRANSFORMS.Apply(aTrs, aPl)
                    rTypeName = Trim(myProps.Item(1).Value)
                Else
                    myProps.Handle = Handle
                    aOCS = TPLANE.ArbitraryCS(aPl.ZDirection)
                    myProps.Handle = Handle
                    myProps.SetVectorGC(210, aOCS.ZDirection, bSuppress:=Math.Round(aOCS.ZDirection.Z, 6) = 1)
                    'get the style orverrides
                    myProps.ReduceToGC(213)
                    myProps.Append(dxfDimTool.GetOverrideProps(Me, aImage))
                    SetProps(myProps)
                    UpdateCommonProperties("LEADER")
                    myProps = New TPROPERTIES(Properties)
                    myProps.SetVal("DimStyle Name", DimStyleName)
                    rTypeName = "LEADER"
                    SetProps(myProps)
                End If
                myProps.SetVectorGC(210, aPl.ZDirection, bSuppress:=Math.Round(aOCS.ZDirection.Z, 6) = 1)
                myProps.SetVectorGC(211, aPl.XDirection, bSuppress:=Math.Round(aOCS.ZDirection.Z, 6) = 1)
                '    props_SV_Vector myProps, dxfGlobals.CommonProps + 2, aPl.Origin
                '    props_SV myProps, dxfGlobals.CommonProps + 5, aPl.XDirection.AngleTo( aOCS.XDirection, aOCS.XDirection)
                Dim lType As dxxLeaderTypes = LeaderType
                Dim sEnt As dxfEntity = Nothing
                Dim v1 As TVECTOR
                Dim suprs As String
                Dim vPts As New TVECTORS(Vertices)
                Dim SubProps As New TPROPERTYARRAY
                Dim vProps As New TPROPERTIES("Vertices")
                Dim i As Integer
                Dim ePl As New TPLANE("")
                Dim aMText As dxeText = Nothing
                Dim aIns As dxeInsert = Nothing
                Dim bSuppHook As Boolean
                Dim bHookFlg As Boolean
                If vPts.Count <= 0 Then
                    myProps = New TPROPERTIES()
                    Return _rVal
                End If
                For i = 1 To vPts.Count
                    vProps.AddVector(10, vPts.Item(i), $"VERTEX({ i })", True)
                Next i
                If aInstance = 1 Then
                    bSuppHook = SuppressHook
                    bHookFlg = Not HookDirection.Equals(aPl.XDirection, 3)
                    sEnt = ReactorEntity
                    If sEnt Is Nothing Then
                        lType = dxxLeaderTypes.NoReactor
                        myProps.SetVal("Annotation Handle", "0")
                    Else
                        aImage.HandleGenerator.AssignTo(sEnt)
                        myProps.SetVal("Annotation Handle", sEnt.Handle) 'AnnotationHandle
                    End If
                    If lType = dxxLeaderTypes.LeaderText Then
                        suprs = "212"
                        v1 = OffsetVector()
                        v1 = myProps.Vector("Annotation Offset")
                        If TVECTOR.IsNull(v1, 6) Then TLISTS.Add(suprs, 213)
                    ElseIf lType = dxxLeaderTypes.LeaderBlock Then
                        bSuppHook = True
                        suprs = "213"
                        v1 = myProps.Vector("Block Offset")
                        If TVECTOR.IsNull(v1, 6) Then TLISTS.Add(suprs, 212)
                        TLISTS.Add(suprs, "40,41")
                    Else
                        suprs = "212,213"
                        TLISTS.Add(suprs, "40,41")
                    End If
                    myProps.SetVal("Leader Type", lType)
                    myProps.SetVal("Hook Direction Flag", bHookFlg)
                    myProps.SetVal("Hook Flag", Not bSuppHook)
                    SetProps(myProps)
                    myProps.SetSuppressionByGC(suprs, True, False)
                    myProps.SetVal("ByBlock Color", myProps.ValueI("Color")) 'ByBlockColor
                    myProps.SetVal("Vertex Count", vPts.Count)
                    SetProps(myProps)
                Else
                    TTRANSFORMS.Apply(aTrs, vPts)
                End If
                myProps = myProps.ReplaceByGC(vProps, 76, True, 1)
                vProps = New TPROPERTIES()
                If sEnt IsNot Nothing Then
                    sEnt.Instances = Instances
                    'SetPropVal("*ReactorHandle", sEnt.Handle)
                    sEnt.ReactorHandle = Handle
                    sEnt.ReactorGUID = GUID
                    'SubProps = sEnt.DXFProps(aInstances, aInstance = aInstance, aOCS:=ePl, aImage:=aImage)
                    'myProps.Value("*ReactorHandle") = sEnt.Handle
                Else
                    myProps.SetVal("*ReactorHandle", "")
                End If
                If vProps.Count > 0 Then myProps.Append(vProps)
                _rVal.Add(myProps, rTypeName, bSuppressSearch:=True, bDontAddEmpty:=True)
                'If SubProps.Count > 0 Then _rVal.Append(SubProps, bSuppressSearch:=True, bDontAddEmpty:=True)
            Catch ex As Exception
            End Try
            Return _rVal
        End Function
        Friend Function OffsetVector() As TVECTOR
            Dim rPropertyName As String = String.Empty
            Return OffsetVector(rPropertyName)
        End Function
        Friend Function OffsetVector(ByRef rPropertyName As String) As TVECTOR
            If LeaderType = dxxLeaderTypes.LeaderBlock Then
                rPropertyName = "Block Offset"
            Else
                rPropertyName = "Annotation Offset"
            End If
            Return PropVector(rPropertyName)
        End Function
        Friend Function Planarize() As TVECTORS
            Dim _rVal As New TVECTORS()
            Dim aPl As TPLANE = PlaneV
            Dim i As Integer
            Dim vrts As TVECTORS
            vrts = VectorsV
            For i = 1 To vrts.Count
                _rVal.Add(vrts.Item(i).ProjectedTo(aPl))
            Next i
            VectorsV = _rVal
            Return _rVal
        End Function

        Protected Overrides Sub Finalize()
            MyBase.Finalize()
        End Sub
#End Region 'Methods


    End Class 'dxeLeader
End Namespace
