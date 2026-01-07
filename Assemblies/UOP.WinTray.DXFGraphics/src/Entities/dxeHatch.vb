Imports UOP.DXFGraphics.Utilities
Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxeHatch
        Inherits dxfEntity
#Region "Members"
#End Region 'Members
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxGraphicTypes.Hatch)
        End Sub

        Public Sub New(aEntity As dxeHatch, Optional bCloneInstances As Boolean = False)
            MyBase.New(dxxGraphicTypes.Hatch, aEntityToCopy:=aEntity, bCloneInstances:=bCloneInstances)

        End Sub


        Friend Sub New(aSubEntity As TENTITY, Optional bNewGUID As Boolean = False)
            MyBase.New(aSubEntity, bNewGUID:=bNewGUID)
        End Sub

        Friend Sub New(aObject As TOBJECT)
            MyBase.New(dxxGraphicTypes.Hatch)
            DefineByObject(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public WriteOnly Property Boundary As dxfEntity
            Set(value As dxfEntity)
                BoundingEntities.Clear()
                '^used to set the bounding edges to those of a polygon or polyline
                If value Is Nothing Then Return
                Dim wuz As Boolean
                Dim aCurves As TBOUNDLOOPS = dxfHatches.BoundLoops_Entity(value, PlaneV)
                If aCurves.Count <= 0 Or value.GraphicType = dxxGraphicTypes.Hatch Or value.GraphicType = dxxGraphicTypes.Insert Or value.GraphicType = dxxGraphicTypes.Leader Or value.GraphicType = dxxGraphicTypes.Symbol Or value.GraphicType = dxxGraphicTypes.Point Or value.GraphicType = dxxGraphicTypes.Solid Or value.GraphicType = dxxGraphicTypes.Table Or value.GraphicType = dxxGraphicTypes.Hole Or value.GraphicType = dxxGraphicTypes.Dimension Then
                    Return
                    Throw New Exception("Unhatchable Boundary Passed")
                Else
                    wuz = SuppressEvents
                    PlaneV = value.Plane.Strukture
                    value.UpdatePath()
                    BoundingEntities.Add(value.Clone)
                    OriginV = value.BoundingRectangle.CenterV
                    IsDirty = True
                    SuppressEvents = wuz
                End If
            End Set
        End Property
        Public Property BoundingEntities As colDXFEntities
            Get
                Return AddSegs
            End Get
            Set(value As colDXFEntities)
                Dim asegs As colDXFEntities = AddSegs
                If Not value Is Nothing Then
                    asegs.Populate(value.ArcsAndLines(True, bIncludePolylines:=True), False)
                Else
                    asegs.Clear()
                End If
                AddSegs = asegs
            End Set
        End Property

        ''' <summary>
        ''' a collection of entities that are included as part of the entities geometry
        ''' </summary>
        ''' '''<remarks>for a hatch entity, the addsegs are the entities that form the boundary bound the hatch area </remarks>
        ''' <returns></returns>
        ''' 
        Friend Overrides Property AddSegs As colDXFEntities
            Get
                If MyBase.AddSegs Is Nothing Then MyBase.AddSegs = New colDXFEntities()
                Return MyBase.AddSegs

            End Get
            Set(value As colDXFEntities)
                MyBase.AddSegs = value

            End Set
        End Property

        Public ReadOnly Property Center As dxfVector
            Get
                Center = BoundingRectangle.Center
                Return Center
            End Get
        End Property
        Public Property Doubled As Boolean
            Get
                Return PropValueB("Doubled Flag")
            End Get
            Set(value As Boolean)
                SetPropVal("Doubled Flag", value, True)
            End Set
        End Property
        Friend ReadOnly Property HatchPatternV As THATCHPATTERN
            Get
                Return dxfHatches.PatternFromHatch(New TPROPERTIES(Properties))
            End Get
        End Property
        Public Property LineStep As Double
            Get
                Return PropValueD("Pattern Step/Scale")
            End Get
            Set(value As Double)
                value = Math.Round(Math.Abs(value), 6)
                SetPropVal("Pattern Step/Scale", value, True)
            End Set
        End Property
        Public Property Method As dxxHatchMethods
            Get
                Return PropValueI("Hatch Style")
            End Get
            Set(value As dxxHatchMethods)
                If value >= 0 And value <= 3 Then
                    SetPropVal("Hatch Style", value, True)
                End If
            End Set
        End Property
        Public ReadOnly Property MidPt As dxfVector
            Get
                Return Center
            End Get
        End Property
        Friend ReadOnly Property MidPtV As TVECTOR
            Get
                Return BoundingRectangle.CenterV
            End Get
        End Property
        Public Property Origin As dxfVector
            Get
                '^the point used as the starting point of the hatch generation
                '~if this point is not set then the center of area of the boundary polygon is used by default
                Return DefPts.Vector1
            End Get
            Set(value As dxfVector)
                '^the point used as the starting point of the hatch generation
                '~if this point is not set then the center of area of the boundary polygon is used by default
                MyBase.DefPts.SetVector(value, 1)
            End Set
        End Property
        Friend Property OriginV As TVECTOR
            Get
                '^the point used as the starting point of the hatch generation
                '~if this point is not set then the center of area of the boundary polygon is used by default
                Return DefPts.VectorGet(1)
            End Get
            Set(value As TVECTOR)
                '^the point used as the starting point of the hatch generation
                '~if this point is not set then the center of area of the boundary polygon is used by default
                DefPts.VectorSet(1, value)
            End Set
        End Property
        Public Property PatternName As String
            Get
                Select Case HatchStyle
                    Case dxxHatchStyle.dxfHatchSolidFill
                        Return "SOLID"
                    Case dxxHatchStyle.dxfHatchUserDefined
                        Return "_USER"
                    Case dxxHatchStyle.dxfHatchPreDefined
                        Return PropValueStr("Pattern Name")
                    Case Else
                        Return String.Empty
                End Select
            End Get
            Set(value As String)
                value = Trim(UCase(value))
                If value = String.Empty Then Return
                If value = "SOLID" Then
                    HatchStyle = dxxHatchStyle.dxfHatchSolidFill
                Else
                    Dim aPat As New THATCHPATTERN
                    Dim idx As Integer
                    aPat = dxfGlobals.goHatchPatterns.GetByName(value, idx)
                    If idx < 0 Then
                        Throw New Exception("Un-Known Hatch Pattern Requested '" & value & "'")
                    Else
                        HatchStyle = dxxHatchStyle.dxfHatchPreDefined
                        SetPropVal("Pattern Name", aPat.Name, True)
                        IsDirty = True
                    End If
                End If
            End Set
        End Property
        Public Property Rotation As Double
            Get
                '^an Rotation to apply to the hatch
                Return PropValueD("Pattern Angle")
            End Get
            Set(value As Double)
                '^a rotation to apply to the hatch
                value = TVALUES.NormAng(value, False, True, True)
                SetPropVal("Pattern Angle", value, True)
            End Set
        End Property
        Public Property ScaleFactor As Double
            Get
                '^the scale factor for the hatch
                '~only applies when Style = dxfHatchPreDefined
                Return PropValueD("*ScaleFactor")
            End Get
            Set(value As Double)
                '^the scale factor for the hatch
                '~only applies when Style = dxfHatchPreDefined
                If value <= 0 Then value = 1
                SetPropVal("*ScaleFactor", value, True)
            End Set
        End Property
        Public Property HatchStyle As dxxHatchStyle
            Get
                Return PropValueI("*Style")
            End Get
            Set(value As dxxHatchStyle)
                If value < dxxHatchStyle.dxfHatchUserDefined Or value > dxxHatchStyle.dxfHatchPreDefined Then Return
                SetPropVal("*Style", value, True)
            End Set
        End Property
#End Region 'Properties
#Region "MustOverride Entity Methods"
        Friend Overrides Sub DefineByObject(aObj As TOBJECT, Optional bNoHandles As Boolean = False, Optional aStyle As dxfTableEntry = Nothing, Optional aBlock As dxfBlock = Nothing)
            Reactors.Append(aObj.Reactors, bClear:=True)
            ExtendedData.Append(aObj.ExtendedData, bClear:=True)
            Dim myProps As New TPROPERTIES(Properties)
            myProps.CopyValuesByGC(aObj.Properties, bIgnoreHandles:=bNoHandles)
            SetProps(myProps)
            If Not bNoHandles Then Handle = aObj.Properties.GCValueStr(5, Handle)
        End Sub
#End Region 'MustOverride Entity Methods
#Region "Methods"

        ''' <summary>
        ''' returns a clone of the entity transfered to the passed plane
        ''' </summary>
        ''' <remarks>the entities defining vectors are converted to vectors with respect to the entities OCS plane and then redefines them based on the passed plane </remarks>
        ''' <param name="aPlane"></param>
        '''<param name="aFromPlane">the plane to define the definition points with respect to prior to transfer to the new plane. If not passed the entities OCS is used </param>
        ''' <returns></returns>
        Public Overloads Function TransferedToPlane(aPlane As dxfPlane,optional aFromPlane as dxfPlane = Nothing) As dxeHatch
            Dim _rVal As dxeHatch = Me.Clone()
            _rVal.TransferToPlane(aPlane, aFromPlane)
            Return _rVal
        End Function


        ''' <summary>
        ''' Returns a new object with properties matching those of the cloned object
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Function Clone() As dxeHatch
            Return New dxeHatch(Me)
        End Function
        Public Overloads Function Clone(aNewBoundary As dxePolyline, Optional aLineStep? As Double = Nothing, Optional aRotation? As Double = Nothing) As dxeHatch
            Dim _rVal As dxeHatch = DirectCast(MyBase.Clone, dxeHatch)
            '^returns an new object with the same properties as the cloned object

            If aNewBoundary IsNot Nothing Then _rVal.Boundary = aNewBoundary Else _rVal.BoundingEntities = New colDXFEntities(BoundingEntities)
            If aLineStep IsNot Nothing Then LineStep = aLineStep.Value
            If aRotation IsNot Nothing Then Rotation = aRotation.Value
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
                If aInstance <= 0 Or i = aInstance Then _rVal.Add(DXFProps(aInstances, i, aOCS, tname))
            Next i
            _rVal.Name = "HATCH"
            If iCnt > 1 Then
                _rVal.Name += $"-{ iCnt } INSTANCES"
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
            If aInstance = 0 Then aInstance = 1
            _rVal = New TPROPERTYARRAY(aInstance:=aInstance)
            Dim myProps As New TPROPERTIES(ActiveProperties())
            Dim aTrs As New TTRANSFORMS
            Dim aPl As TPLANE = PlaneV
            Dim scl As Double = 1
            Dim ang As Double = 0
            Dim bInv As Boolean
            Dim bLft As Boolean
            Dim bOCS As TPLANE
            If aInstance > 1 Then
                If aInstances Is Nothing Then aInstances = Instances
                aTrs = aInstances.Transformations(aInstance - 1, scl, ang, bInv, bLft)
                TTRANSFORMS.Apply(aTrs, aPl)
                bOCS = TPLANE.ArbitraryCS(aPl.ZDirection)
                rTypeName = Trim(myProps.Item(1).Value)
            Else
                myProps.Handle = Handle
                aOCS = TPLANE.ArbitraryCS(aPl.ZDirection)
                bOCS = aOCS
                myProps.SetVector("Extrusion Direction", aOCS.ZDirection.Normalized) 'Round(aOCS.ZDirection.Z, 6) = 1, True
                SetProps(myProps)
                UpdateCommonProperties("HATCH")
                myProps = New TPROPERTIES(Properties)
                rTypeName = "HATCH"
            End If
            Dim v1 As TVECTOR
            Dim aPat As THATCHPATTERN = dxfHatches.PatternFromHatch(myProps)
            Dim hMth As dxxHatchMethods = myProps.ValueI("Hatch Style")
            Dim hStyle As dxxHatchStyle = myProps.ValueStr("*Style") 'Style
            Dim bDlb As Boolean = myProps.ValueB("Doubled Flag")
            Dim SF As Double = myProps.ValueD("*ScaleFactor") 'ScaleFactor
            Dim loopProps As New TPROPERTIES
            Dim rot As Double
            v1 = New TVECTOR(0, 0, aPl.Origin.Z)
            myProps.SetVector("Elevation Point", New TVECTOR(0, 0, aPl.Origin.Z))
            loopProps = dxfHatches.GetBoundaryLoopProps(Me, bOCS, aTrs)
            rot = myProps.ValueD("Pattern Angle")
            If SF <= 0 Then
                myProps.SetVal("*ScaleFactor", 1)
                SF = 1
            End If
            myProps.SetVal("Boundary Loop Count", BoundaryLoops.Count)
            myProps.SetVal("Solid Fill Flag", hStyle = dxxHatchStyle.dxfHatchSolidFill)
            myProps.SetVal("Associative Flag", False)
            myProps.SetVal("Hatch Style", hMth)
            If hStyle = dxxHatchStyle.dxfHatchUserDefined Then myProps.SetVal("Hatch Type", 0) Else myProps.SetVal("Hatch Type", 1)
            myProps.SetSuppressed("Doubled Flag,Pattern Step/Scale,Pattern Line Count", ",", hStyle = dxxHatchStyle.dxfHatchSolidFill)
            myProps.Value("Doubled Flag", bDlb)
            If hStyle = dxxHatchStyle.dxfHatchPreDefined Then
                myProps.SetVal("Pattern Line Count", aPat.HatchLineCnt)
                myProps.SetVal("Pattern Step/Scale", SF * scl)
            ElseIf hStyle = dxxHatchStyle.dxfHatchUserDefined Then
                myProps.SetVal("Pattern Step/Scale", myProps.ValueD("Pattern Step/Scale") * scl)
                If Not bDlb Then myProps.SetVal("Pattern Line Count", 1) Else myProps.SetVal("Pattern Line Count", 2)
            Else
                myProps.SetVal("Pattern Step/Scale", 0.0)
            End If
            If hMth = dxxHatchMethods.Normal Then
                myProps.SetVal("Pattern Name", aPat.Name)
            ElseIf hMth = dxxHatchMethods.Ignore Then
                myProps.SetVal("Pattern Name", aPat.Name & ",_I")
            ElseIf hMth = dxxHatchMethods.Outer Then
                myProps.SetVal("Pattern Name", aPat.Name & ",_O")
            End If
            myProps = myProps.ReplaceByGC(loopProps, 91, True)
            If hStyle <> dxxHatchStyle.dxfHatchSolidFill Then
                myProps = myProps.ReplaceByGC(dxfHatches.GetHatchLineProps(myProps, scl, ang), 78, True)
            End If
            _rVal.Add(myProps, rTypeName, True, True)
            If aInstance < 1 Then SetProps(myProps)
            Return _rVal
        End Function
        Public Function HatchLines(Optional aImage As dxfImage = Nothing, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aCollector As colDXFEntities = Nothing) As colDXFEntities
            Dim _rVal As colDXFEntities
            If aCollector Is Nothing Then
                _rVal = New colDXFEntities
            Else
                _rVal = aCollector
            End If
            If HatchStyle = dxxHatchStyle.dxfHatchSolidFill Then Return _rVal
            UpdatePath(aImage:=aImage)
            Dim aPths As TPATHS
            Dim aPth As TPATH
            Dim i As Integer
            Dim lp As Integer
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim aLn As New dxeLine With {.DisplayStructure = DisplayStructure}
            Dim j As Integer
            Dim aLoop As TVECTORS
            aLn.LCLSet(aLayer, aColor, aLineType)
            aPths = Paths
            For i = 1 To aPths.Count
                aPth = aPths.Item(i)
                For lp = 1 To aPth.LoopCount
                    aLoop = aPth.Looop(lp)
                    For j = 2 To aLoop.Count
                        v2 = aLoop.Item(j)
                        If v2.Code = dxxVertexStyles.LINETO Then
                            v1 = aLoop.Item(j - 1)
                            aLn.SetVectors(v1, v2)
                            _rVal.Add(aLn, bAddClone:=True)
                        End If
                    Next j
                    aPth.SetLoop(lp, aLoop)
                Next lp
            Next i
            Return _rVal
        End Function

#End Region 'Methods
    End Class 'dxeHatch
End Namespace
