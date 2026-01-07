
Imports System.Numerics
Imports SharpDX.Direct2D1.Effects
Imports UOP.DXFGraphics.dxfGlobals
Imports UOP.DXFGraphics.Structures
Imports Vanara.PInvoke.Kernel32
Imports Vanara.PInvoke.LANGID

Namespace UOP.DXFGraphics
    Public Class dxeInsert
        Inherits dxfEntity
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxGraphicTypes.Insert)

        End Sub

        Public Sub New(aEntity As dxeInsert, Optional bCloneInstances As Boolean = False)
            MyBase.New(dxxGraphicTypes.Insert, aEntityToCopy:=aEntity, bCloneInstances:=bCloneInstances)

        End Sub


        Public Sub New(Optional aGUIDPrefix As String = "")
            MyBase.New(dxxGraphicTypes.Insert, aGUIDPrefix:=aGUIDPrefix)
        End Sub
        Friend Sub New(aSubEntity As TENTITY, Optional bNewGUID As Boolean = False)
            MyBase.New(aSubEntity, bNewGUID:=bNewGUID)
        End Sub

        Friend Sub New(aObject As TOBJECT)

            MyBase.New(dxxGraphicTypes.Insert)

            DefineByObject(aObject, False, Nothing, Nothing)
        End Sub
        Public Sub New(aBlock As dxfBlock, Optional ainsertionPt As iVector = Nothing, Optional aScaleFactor As Double = 1, Optional aRotation As Double = 0)
            MyBase.New(dxxGraphicTypes.Insert)
            InsertionPtV = New TVECTOR(ainsertionPt)
            If aScaleFactor <> 0 Then ScaleFactor = aScaleFactor
            If aRotation <> 0 Then RotationAngle = aRotation
            Block = aBlock
            If aBlock IsNot Nothing Then
                ImageGUID = aBlock.ImageGUID

            End If
        End Sub
#End Region 'Constructors
#Region "Properties"

        '^If the Entity has dependent entities it will return a list containing them other wise an empty lists should be retured
        Public Overrides Function PersistentSubEntities() As List(Of dxfEntity)
            Return New List(Of dxfEntity)()
        End Function


        Friend Property AttributesVals As TPROPERTIES
            Get
                Dim _rVal As New TPROPERTIES("Attributes")
                Dim myatts As dxfAttributes = Attributes
                For Each att As dxfAttribute In myatts
                    _rVal.Add(New TPROPERTY(3, att.Value, att.Tag, dxxPropertyTypes.dxf_String) With {.Prompt = att.Prompt})
                Next
                Return _rVal
            End Get
            Set(value As TPROPERTIES)
                Dim myatts As dxfAttributes = Attributes
                For i As Integer = 1 To value.Count
                    Dim prop As TPROPERTY = value.Item(i)
                    Dim att As dxfAttribute = myatts.Find(Function(x) String.Compare(x.Tag, prop.Name, True) = 0)
                    If att IsNot Nothing Then
                        att.Value = prop.ValueS
                    End If
                Next

                value.Name = "Attributes"
                value.Owner = GUID
                Dim myStyle As TTABLEENTRY = Style
                myStyle.Props = value
                Style = myStyle
            End Set
        End Property



        Public Property SourceBlockGUID As String
            Get
                Return SourceGUID
            End Get
            Set(value As String)
                SourceGUID = value
                SetPropVal("*SourceBlockGUID", SourceGUID, True)
            End Set
        End Property
        Public WriteOnly Property Block As dxfBlock
            'Get
            '    If String.IsNullOrWhiteSpace(SourceBlockGUID) Then Return Nothing
            '    Dim aImage As dxfImage = Nothing
            '    Dim _rVal As dxfBlock = Nothing

            '    GetBlock(aImage, _rVal)
            '    Return _rVal

            'End Get
            Set(value As dxfBlock)
                Dim bInit As Boolean
                If value Is Nothing Then

                    If Not String.IsNullOrWhiteSpace(SourceBlockGUID) Then IsDirty = True
                    If State <> dxxEntityStates.GeneratingPath Then IsDirty = True
                    SetPropVal("*BlockHandle", String.Empty, False)
                    SetPropVal("*SourceGUID", String.Empty, False)
                    SetPropVal("*SourceBlockGUID", String.Empty, False)
                    SetPropVal(2, "", False)  'block name
                    SetPropVal("*UniformScale", False, False)
                    _Attributes = New dxfAttributes()
                    BlockGUID = String.Empty
                    BlockRecordHandle = String.Empty

                Else
                    bInit = SourceBlockGUID <> value.GUID
                    Dim bname As String = value.Name
                    If bInit Then IsDirty = True
                    Dim chng As Boolean = bInit
                    If SetPropVal("*SourceBlockGUID", value.GUID, False) Then chng = True
                    If SetPropVal("*SourceGUID", value.GUID, False) Then chng = True
                    If SetPropVal(2, bname, False) Then chng = True 'block name
                    If SetPropVal("*BlockHandle", value.Handle, False) Then chng = True
                    BlockRecordHandle = value.BlockRecordHandle
                    If State = dxxEntityStates.Steady Then
                        If String.IsNullOrWhiteSpace(ImageGUID) Then ImageGUID = value.ImageGUID
                        SetPropVal("*UniformScale", value.UniformScale, False)

                        If chng Then
                            Dim oldatts As New dxfAttributes(Attributes)
                            Dim blkatts As dxfAttributes = dxfEntities.ExtractAttributes(value.Entities, aOwnerGUID:=GUID)
                            If blkatts.Count <= 0 Then
                                Attributes.Clear()
                            Else
                                If oldatts.Count > 0 Then
                                    If oldatts.IsEqual(blkatts, bDontCompareValues:=True) Then
                                        blkatts.CopyValues(oldatts)
                                    End If
                                End If
                                Attributes.Populate(blkatts)
                                Attributes.IsDirty = False
                            End If
                        End If
                    End If
                End If
            End Set
        End Property
        Public Property BlockName As String
            Get
                Return PropValueStr("Block Name")
            End Get
            Friend Set(value As String)
                If SetPropVal("Block Name", value, True) Then
                    BlockGUID = ""
                    BlockRecordHandle = ""
                End If
            End Set
        End Property
        Public Property BlockHandle As String
            Get
                Return PropValueStr("*BlockHandle")
            End Get
            Friend Set(value As String)
                SetPropVal("*BlockHandle", value, False)
            End Set
        End Property
        Public Property BlockRecordHandle As String
            Get
                Return PropValueStr("*BlockRecordHandle")
            End Get
            Friend Set(value As String)
                SetPropVal("*BlockRecordHandle", value)
            End Set
        End Property
        Public Property ColSpace As Double
            Get
                '^for arrays of block inserts
                Return PropValueD("Column Space")
            End Get
            Set(value As Double)
                '^for arrays of block inserts
                value = Math.Abs(value)
                SetPropVal("Column Space", value, True)
            End Set
        End Property
        Public Property ColumnCount As Integer
            Get
                '^for arrays of blocks
                Return PropValueI("Columns")
            End Get
            Set(value As Integer)
                '^for arrays of blocks
                If SetPropVal("Columns", value, False) Then
                    If ColSpace > 0 Then IsDirty = True
                End If
            End Set
        End Property
        Public ReadOnly Property HasRowsAndColumns As Boolean
            Get
                '^returns true if the insert has row and column definitions defines
                Return (RowSpace <> 0 And RowCount > 0) Or (ColSpace <> 0 Or ColumnCount > 0)
            End Get
        End Property
        Public ReadOnly Property HasScales As Boolean
            Get
                Return XScaleFactor <> 1 Or YScaleFactor <> 1 Or ZScaleFactor <> 1
            End Get
        End Property
        Public Property InsertionPt As dxfVector
            Get
                '^the point where the entity was inserted
                Return DefPts.Vector1
            End Get
            Set(value As dxfVector)
                '^the point where the entity was inserted
                InsertionPt.MoveTo(value)
            End Set
        End Property
        Friend Property InsertionPtV As TVECTOR
            Get
                '^the point where the entity was inserted
                Return DefPts.VectorGet(1)
            End Get
            Set(value As TVECTOR)
                '^the point where the entity was inserted
                DefPts.VectorSet(1, value)
            End Set
        End Property

        Public Overrides Property IsDirty As Boolean
            Get
                Return MyBase.IsDirty Or Attributes.IsDirty
            End Get
            Friend Set(value As Boolean)
                MyBase.IsDirty = value
                Attributes.IsDirty = False

            End Set
        End Property
        Public Property RotationAngle As Double
            Get
                '^the rotation angle to apply to the inserted block
                Return PropValueD("Rotation Angle")
            End Get
            Set(value As Double)
                '^the rotation angle to apply to the inserted block
                value = TVALUES.NormAng(value, False, True)
                SetPropVal("Rotation Angle", value, True)
            End Set
        End Property
        Public Property RowCount As Integer
            Get
                '^for arrays of blocks
                Return PropValueI("Rows")
            End Get
            Set(value As Integer)
                '^for arrays of blocks
                SetPropVal("Rows", Math.Abs(value), True)
            End Set
        End Property
        Public Property RowSpace As Double
            Get
                '^for arrays of block inserts
                Return PropValueD("Row Space")
            End Get
            Set(value As Double)
                '^for arrays of block inserts
                SetPropVal("Row Space", Math.Abs(value), True)
            End Set
        End Property
        Public Overrides Property Tag As String
            Get
                If (String.IsNullOrWhiteSpace(MyBase.Tag)) Then Return BlockName Else Return MyBase.Tag
            End Get
            Set(value As String)
                MyBase.Tag = value
            End Set
        End Property
        Public WriteOnly Property ScaleFactor As Double
            Set(value As Double)
                If value <> 0 Then
                    XScaleFactor = value
                    YScaleFactor = value
                    ZScaleFactor = value
                End If
            End Set
        End Property
        Public Property UniformScale As Boolean
            Get
                Return PropValueB("*UniformScale")
            End Get
            Set(value As Boolean)
                SetPropVal("*UniformScale", value, True)
            End Set
        End Property

        ''' <summary>
        ''' the X scale factor to apply to the inserted block
        ''' </summary>
        ''' <returns></returns>
        Public Property XScaleFactor As Double
            Get

                Return PropValueD(41)
            End Get
            Set(value As Double)


                Dim curval As Double = Properties.ValueD(41)
                If value = 0 Then value = curval
                If value = 0 Then Return
                If value = curval Then Return
                If Properties.SetVal(41, value) Then
                    If State = dxxEntityStates.Steady Then IsDirty = True
                    If UniformScale Then
                        If Properties.SetVal(42, value) And State = dxxEntityStates.Steady Then IsDirty = True
                        If Properties.SetVal(43, value) And State = dxxEntityStates.Steady Then IsDirty = True
                    End If
                End If



            End Set

        End Property

        ''' <summary>
        ''' the Y scale factor to apply to the inserted block
        ''' </summary>
        ''' <returns></returns>
        Public Property YScaleFactor As Double
            Get
                If Not UniformScale Then
                    Return PropValueD(42)
                Else
                    Return PropValueD(41)  'the x scale
                End If
            End Get
            Set(value As Double)
                Dim curval As Double = Properties.ValueD(42)
                If value = 0 Then value = curval
                If value = 0 Then Return
                If value = curval Then Return
                If Properties.SetVal(42, value) Then
                    If State = dxxEntityStates.Steady Then IsDirty = True
                    If UniformScale Then
                        If Properties.SetVal(41, value) And State = dxxEntityStates.Steady Then IsDirty = True
                        If Properties.SetVal(43, value) And State = dxxEntityStates.Steady Then IsDirty = True
                    End If
                End If
            End Set
        End Property
        ''' <summary>
        ''' the Z scale factor to apply to the inserted block
        ''' </summary>
        ''' <returns></returns>
        Public Property ZScaleFactor As Double
            Get
                If Not UniformScale Then
                    Return PropValueD(43)
                Else
                    Return PropValueD(41)  'the x scale
                End If
            End Get
            Set(value As Double)

                Dim myprops As New TPROPERTIES(Properties)
                Dim curval As Double = myprops.GCValueD(43)
                If value = 0 Then value = curval
                If value = 0 Then Return
                If value = curval Then Return
                myprops.SetValGC(43, value, 1)
                If State <> dxxEntityStates.GeneratingPath Then myprops.IsDirty = True
                If UniformScale Then
                    myprops.SetValGC(41, value, True)
                    myprops.SetValGC(42, value, True)
                End If
                SetProps(myprops)
            End Set
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
#End Region 'Properties
#Region "MustOverride Entity Methods"
        Friend Overrides Sub DefineByObject(aObj As TOBJECT, Optional bNoHandles As Boolean = False, Optional aStyle As dxfTableEntry = Nothing, Optional aBlock As dxfBlock = Nothing)
            Reactors.Append(aObj.Reactors, bClear:=True)
            ExtendedData.Append(aObj.ExtendedData, bClear:=True)
            Dim aPlane As TPLANE = TPLANE.ArbitraryCS(aObj.Properties.GCValueV(210, TVECTOR.WorldZ))
            Block = aBlock
            PlaneV = aPlane
            DisplayStructure = aObj.DisplayVars
            If Not bNoHandles Then Handle = aObj.Properties.GCValueStr(5)
            InsertionPtV = aPlane.WorldVector(aObj.Properties.GCValueV(10))
            XScaleFactor = aObj.Properties.GCValueD(41, 1)
            YScaleFactor = aObj.Properties.GCValueD(42, 1)
            ZScaleFactor = aObj.Properties.GCValueD(43, 1)
            RotationAngle = aObj.Properties.GCValueD(50)
        End Sub

        ''' <summary>
        ''' returns a clone of the entity transfered to the passed plane
        ''' </summary>
        ''' <remarks>the entities defining vectors are converted to vectors with respect to the entities OCS plane and then redefines them based on the passed plane </remarks>
        ''' <param name="aPlane"></param>
        '''<param name="aFromPlane">the plane to define the definition points with respect to prior to transfer to the new plane. If not passed the entities OCS is used </param>
        ''' <returns></returns>
        Public Overloads Function TransferedToPlane(aPlane As dxfPlane,optional aFromPlane as dxfPlane = Nothing) As dxeInsert
            Dim _rVal As dxeInsert = Me.Clone()
            _rVal.TransferToPlane(aPlane, aFromPlane)
            Return _rVal
        End Function


        ''' <summary>
        ''' Returns a new object with properties matching those of the cloned object
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Function Clone() As dxeInsert
            Return New dxeInsert(Me)
        End Function

#End Region 'MustOverride Entity Methods
#Region "Methods"
        Public Function GetAttributes(Optional aImage As dxfImage = Nothing, Optional aBlock As dxfBlock = Nothing, Optional bForFileOutput As Boolean = False) As List(Of dxeText)
            If Not GetBlock(aImage, aBlock) Then Return New List(Of dxeText)
            Dim tforms As TTRANSFORMS = Transforms(aImage, aBlock)
            Dim attdefs As List(Of dxeText) = aBlock.Entities.Texts.FindAll(Function(x) x.EntityType = dxxEntityTypes.Attdef)
            If attdefs.Count <= 0 Then
                Return New List(Of dxeText)()
            End If
            Dim scales As TVECTOR = tforms.Item(dxxTransformationTypes.Scale).ScaleVector
            Dim scaleX As Double = scales.X
            Dim rot As Double = tforms.Item(dxxTransformationTypes.Rotation).Angle
            Dim myPlane As dxfPlane = Plane
            Dim blkPlane As dxfPlane = aBlock.Plane

            Dim _rVal As New List(Of dxeText)()
            For Each attdef As dxeText In attdefs

                Dim iatt As dxfAttribute = Attributes.Find(Function(x) String.Compare(x.SourceGUID, attdef.GUID, True) = 0)
                If iatt Is Nothing Then iatt = Attributes.Item(attdef.AttributeTag)
                If iatt Is Nothing Then
                    iatt = New dxfAttribute(attdef, GUID)
                    Attributes.Add(iatt)
                End If


                Dim newatt As New dxeText(attdef) With {.TextType = dxxTextTypes.Attribute, .TextString = iatt.Value}


                If Not bForFileOutput Then
                    Dim v1 As dxfVector = attdef.Properties.ValueV(10).WithRespectTo(blkPlane)
                    Dim v2 As dxfVector = attdef.Properties.ValueV(11).WithRespectTo(blkPlane)

                    newatt.Properties.CopyVals(iatt.Properties)
                    newatt.Properties.SetVector(10, myPlane.Vector(v1.X * scaleX, v1.Y * scaleX))
                    newatt.Properties.SetVector(11, myPlane.Vector(v2.X * scaleX, v2.Y * scaleX))

                Else
                    newatt.Properties.CopyVals(dxfAttributes.IndependantGroupCodes, iatt.Properties)
                    newatt.Transform(tforms, True)


                End If

                _rVal.Add(newatt)
            Next

            Return _rVal

        End Function

        Friend Function ScaleVector() As TVECTOR
            Dim uniformScale As Boolean = uniformScale
            Dim xscale As Double = Properties.ValueD(41)
            If xscale = 0 Then xscale = 1
            Dim Scls As New TVECTOR(xscale, IIf(uniformScale, xscale, Properties.ValueD(42, aDefault:=xscale)), IIf(uniformScale, xscale, Properties.ValueD(43, aDefault:=xscale)))

            If Scls.Y = 0 Or uniformScale Then Scls.Y = Scls.X
            If Scls.Z = 0 Or uniformScale Then Scls.Z = Scls.X
            Properties.SetVal(41, Scls.X, bDirtyOnChange:=False, bSuppressEvents:=True)
            Properties.SetVal(42, Scls.Y, bDirtyOnChange:=False, bSuppressEvents:=True)
            Properties.SetVal(43, Scls.Z, bDirtyOnChange:=False, bSuppressEvents:=True)
            Return Scls
        End Function

        ''' <summary>
        '''  returns the transformations to apply to the block entities or paths
        ''' </summary>
        ''' <remarks>there are always three transforms Translation, Rotation and Scale </remarks>
        ''' <param name="aImage">the parent image</param>
        ''' <param name="aBlock">the inserts parent block</param>
        ''' <returns></returns>
        Friend Function Transforms(Optional aImage As dxfImage = Nothing, Optional aBlock As dxfBlock = Nothing) As TTRANSFORMS


            If Not GetBlock(aImage, aBlock) Then Return TTRANSFORMS.Null
            Dim _rVal As TTRANSFORMS = TTRANSFORMS.Null
            Dim blockPlane As dxfPlane = aBlock.Plane
            Dim myPlane As dxfPlane = Plane
            Dim rot As Double = Properties.ValueD(50)
            Dim Scls As TVECTOR = ScaleVector()

            rot = TVALUES.NormAng(rot, False, True, True)
            Properties.SetVal(50, rot, bDirtyOnChange:=False, bSuppressEvents:=True)

            'add translation to block origin and to the insert pt
            _rVal.Add(TTRANSFORM.CreateTranslation(myPlane.Origin - blockPlane.Origin, True))

            'add insert rotation
            _rVal.Add(TTRANSFORM.CreateRotation(myPlane.Origin, myPlane, rot, False, aAxisDescriptor:=dxxAxisDescriptors.Z, bAddRotationToMembers:=True))

            'add the scales
            _rVal.Add(TTRANSFORM.CreateScale(myPlane.Origin, Scls.X, Scls.Y, Scls.Z, Nothing, True))

            Return _rVal

        End Function


        Public Function GetBlock(ByRef ioImage As dxfImage, ByRef rBlock As dxfBlock) As Boolean

            rBlock = Nothing
            If Not GetImage(ioImage) Then Return False
            If Not ioImage.Blocks.TryGet(SourceBlockGUID, rBlock, dxxBlockReferenceTypes.GUID) Then
                If Not ioImage.Blocks.TryGet(BlockName, rBlock, dxxBlockReferenceTypes.Name) Then
                    ioImage.Blocks.TryGet(BlockHandle, rBlock, dxxBlockReferenceTypes.Handle)
                End If

            End If

            Dim _rVal As Boolean = rBlock IsNot Nothing
            If _rVal Then
                SourceBlockGUID = rBlock.GUID
            End If
            Return _rVal
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
                _rVal.Name = $"{tname }-{ iCnt } INSTANCES"
            End If
            Return _rVal
        End Function
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY
            '#1the instance to create the properties for
            '#2the OCS plane of the entity (created for first instance)
            '^returns the entities properties updated for output to a DXF file
            'On Error Resume Next
            aInstance = Math.Abs(aInstance)
            If aInstance <= 0 Then aInstance = 1
            GetImage(aImage)
            _rVal = New TPROPERTYARRAY(aInstance:=aInstance)
            rTypeName = "INSERT"
            Dim myProps As New TPROPERTIES(ActiveProperties())
            Dim aTrs As New TTRANSFORMS
            Dim aPl As New TPLANE("")
            Dim scl As Double
            Dim ang As Double
            Dim bInv As Boolean
            Dim bLft As Boolean
            Dim bAtts As Boolean
            Dim aScl As Double
            Dim rAttributes As dxfAttributes = Attributes
            aPl = PlaneV

            bAtts = rAttributes.Count > 0
            scl = 1
            ang = 0
            If aInstance > 1 Then
                If aInstances Is Nothing Then aInstances = Instances
                aTrs = aInstances.Transformations(aInstance - 1, scl, ang, bInv, bLft)
                TTRANSFORMS.Apply(aTrs, aPl, False, True)
            Else
                myProps.Handle = Handle
                aOCS = TPLANE.ArbitraryCS(aPl.ZDirection)
                myProps.SetVectorGC(210, aOCS.ZDirection, 1, bNormalize:=Math.Round(aOCS.ZDirection.Z, 6) = 1)
                myProps.SetVal("Attribute Follow Flag", bAtts)
                'Props = myProps
                UpdateCommonProperties("INSERT")
                myProps = New TPROPERTIES(ActiveProperties())
            End If
            myProps.SetVector("Insertion Pt", aPl.Origin.WithRespectTo(aOCS))
            'xscale
            aScl = myProps.ValueD("X Scale Factor")
            If bLft Then aScl = -aScl
            myProps.SetVal("X Scale Factor", aScl * scl)
            'yscale
            aScl = myProps.ValueD("Y Scale Factor")
            If bInv Then aScl = -aScl
            myProps.SetVal("Y Scale Factor", aScl * scl)
            'zscale
            aScl = myProps.ValueD("Z Scale Factor")
            myProps.SetVal("Z Scale Factor", aScl * scl)
            'rotation
            aScl = myProps.ValueD("Rotation")
            myProps.SetVal("Rotation", TVALUES.NormAng(aScl + ang))
            If aInstance = 1 Then SetProps(myProps)
            If rAttributes.Count > 0 Then
                rTypeName = "INSERT/ATTRIBS"
                _rVal.Name = rTypeName
            End If
            _rVal.Add(myProps, rTypeName, True, True)
            If Attributes.Count > 0 Then
                Dim aProps As New TPROPERTYARRAY
                Dim bProps As New TPROPERTIES
                Dim aTxt As dxeText
                Dim tPl As New TPLANE("")
                For i As Integer = 1 To rAttributes.Count
                    aTxt = New dxeText(rAttributes(i - 1), GUID, True)
                    Dim tname As String = String.Empty
                    aProps = aTxt.DXFProps(aInstances, aInstance, tPl, tname, aImage)
                    _rVal.Append(aProps, True, True)
                    If i = rAttributes.Count Then
                        bProps = dxpProperties.Get_EntityProps(dxxGraphicTypes.SequenceEnd, $"{GUID }.SEQEND")
                        bProps.SetVal("Owner Handle", Handle)
                        _rVal.Add(bProps, "SEQEND")
                    End If
                Next i
            End If
            Return _rVal
        End Function
        Public Function Perimeter(Optional bAsLines As Boolean = False, Optional aCurveDivisions As Double = 20, Optional bClosed As Boolean = False) As dxePolyline
            '^returns the bounding rectangle as a Polyline
            Return BoundingRectangle.Perimeter
        End Function
        Friend Function SetAttributes(ByRef aAttribs As TPROPERTIES) As Boolean
            Dim _rVal As Boolean = False
            Dim tgs As String = dxfAttributes.GetTags(aAttribs)
            If String.IsNullOrWhiteSpace(tgs) Then Return _rVal

            Dim tVals() As String = tgs.Split(",")
            Dim sAttribs As List(Of dxfAttribute)
            Dim aVal As String
            Dim aAttributes As dxfAttributes = Attributes

            For i As Integer = 0 To tVals.Length - 1
                Dim tg As String = tVals(i)
                Dim tAttr As TPROPERTIES = dxfAttributes.GetByTag(aAttribs, tg)
                sAttribs = aAttributes.FindAll(Function(x) String.Compare(x.Tag, tg, True) = 0)
                For j As Integer = 1 To sAttribs.Count
                    If j > tAttr.Count Then
                        aVal = tAttr.Item(tAttr.Count).Value
                    Else
                        aVal = tAttr.Item(j).Value
                    End If
                    Dim att As dxfAttribute = aAttributes.Item(j - 1)
                    If att.Properties.SetVal("Text String", aVal) Then
                        _rVal = True
                    End If
                Next j
            Next i

            If _rVal Then IsDirty = True
            Return _rVal
        End Function
        Friend Function SetAttributes(aAttribs As dxfAttributes) As Boolean
            If aAttribs Is Nothing Then Return False
            Return Attributes.CopyValues(aAttribs, True)

        End Function

#End Region 'Methods
    End Class 'dxeInsert
End Namespace
