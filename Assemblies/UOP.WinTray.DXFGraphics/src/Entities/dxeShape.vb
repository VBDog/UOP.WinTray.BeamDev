Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxeShape
        Inherits dxfEntity
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxGraphicTypes.Shape)
        End Sub

        Public Sub New(aEntity As dxeShape, Optional bCloneInstances As Boolean = False)
            MyBase.New(dxxGraphicTypes.Shape, aEntityToCopy:=aEntity, bCloneInstances:=bCloneInstances)
        End Sub

        Friend Sub New(aSubEntity As TENTITY, Optional bNewGUID As Boolean = False)
            MyBase.New(aSubEntity, bNewGUID:=bNewGUID)
        End Sub

        Friend Sub New(aObject As TOBJECT)
            MyBase.New(dxxGraphicTypes.Shape)
            DefineByObject(aObject)
        End Sub


#End Region 'Constructors
#Region "Properties"
        Friend ReadOnly Property Entities As colDXFEntities
            Get
                UpdatePath()
                Dim _rVal As New colDXFEntities
                _rVal.ArcLineStructures_Set(Components.Segments)
                Return _rVal
            End Get
        End Property
        Public Function GetBlock(aImage As dxfImage) As dxfBlock
            Dim bEnts As New List(Of dxfEntity)
            Dim iGUID As String = String.Empty
            Dim eGUID As String = GUID
            GetImage(aImage)
            If aImage IsNot Nothing Then iGUID = aImage.GUID
            UpdatePath(aImage:=aImage)
            Dim myEnts As colDXFEntities = Entities
            Dim bname As String = Name
            If bname = "" Then
                bname = aImage.HandleGenerator.NextShapeName()
            End If
            Return New dxfBlock(myEnts, bname) With {.ImageGUID = iGUID, .LayerName = LayerName}
        End Function
        Public Shadows Property Height As Double
            Get
                Return PropValueD("Size")
            End Get
            Set(value As Double)
                If value < 0 Then Return
                SetPropVal("Size", value, True)
            End Set
        End Property
        Public Property InsertionPt As dxfVector
            Get
                '^the point where the entity was inserted
                Return DefPts.Vector1
            End Get
            Set(value As dxfVector)
                '^the point where the entity was inserted
                MoveTo(value)
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
        Public Property ObliqueAngle As Double
            Get
                Return PropValueD("Oblique Angle")
            End Get
            Set(value As Double)
                SetPropVal("Oblique Angle", TVALUES.ObliqueAngle(value), True)
            End Set
        End Property
        Public Property Rotation As Double
            Get
                Return PropValueD("Rotation Angle")
            End Get
            Set(value As Double)
                SetPropVal("Rotation Angle", TVALUES.NormAng(value, False, True, True), True)
            End Set
        End Property
        Public Property SaveExploded As Boolean
            Get
                Return PropValueB("*SaveExplode")
            End Get
            Set(value As Boolean)
                SetPropVal("*SaveExplode", value, True)
            End Set
        End Property
        Public Property ShapeCommands As String
            Get
                Return PropValueStr("*ShapeCommands")
            End Get
            Set(value As String)
                SetPropVal("*ShapeCommands", value, True)
            End Set
        End Property
        Public Property ShapeFileName As String
            Get
                Return PropValueStr("*FileName")
            End Get
            Friend Set(value As String)
                SetPropVal("*FileName", value, True)
            End Set
        End Property
        Public Property ShapeName As String
            Get
                Return PropValueStr("Shape Name")
            End Get
            Set(value As String)
                SetPropVal("Shape Name", value, True)
            End Set
        End Property
        Public Property ShapeNumber As Integer
            Get
                Return PropValueI("*ShapeNumber")
            End Get
            Friend Set(value As Integer)
                SetPropVal("*ShapeNumber", value, True)
            End Set
        End Property
        Public Shadows ReadOnly Property Width As Double
            Get
                UpdatePath()
                Return Bounds.Width
            End Get
        End Property
        Public Property WidthFactor As Double
            Get
                Return PropValueD("Width Factor")
            End Get
            Set(value As Double)
                If value <= 0 Then Return
                SetPropVal("Width Factor", value, True)
            End Set
        End Property
#End Region 'Properties
#Region "MustOverride Entity Methods"


        Friend Overrides Sub DefineByObject(aObj As TOBJECT, Optional bNoHandles As Boolean = False, Optional aStyle As dxfTableEntry = Nothing, Optional aBlock As dxfBlock = Nothing)
           Reactors.Append(aObj.Reactors, bClear:=True)
            ExtendedData.Append(aObj.ExtendedData,bClear:=True )
            Dim aPlane As TPLANE = TPLANE.ArbitraryCS(aObj.Properties.GCValueV(210, TVECTOR.WorldZ))
            DisplayStructure = aObj.DisplayVars
            PlaneV = aPlane
            InsertionPtV = aPlane.WorldVector(aObj.Properties.GCValueV(10))
            ShapeName = aObj.Properties.GCValueStr(2)
            If Not bNoHandles Then Handle = aObj.Properties.GCValueStr(5)
        End Sub

        ''' <summary>
        ''' returns a clone of the entity transfered to the passed plane
        ''' </summary>
        ''' <remarks>the entities defining vectors are converted to vectors with respect to the entities OCS plane and then redefines them based on the passed plane </remarks>
        ''' <param name="aPlane"></param>
        '''<param name="aFromPlane">the plane to define the definition points with respect to prior to transfer to the new plane. If not passed the entities OCS is used </param>
        ''' <returns></returns>
        Public Overloads Function TransferedToPlane(aPlane As dxfPlane,optional aFromPlane as dxfPlane = Nothing) As dxeShape
            Dim _rVal As dxeShape = Me.Clone()
            _rVal.TransferToPlane(aPlane, aFromPlane)
            Return _rVal
        End Function

        ''' <summary>
        ''' Returns a new object with properties matching those of the cloned object
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Function Clone() As dxeShape
            Return New dxeShape(Me)
        End Function
#End Region 'MustOverride Entity Methods
#Region "Methods"
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix(GroupName, ExtendedData)
            'On Error Resume Next
            rBlock = Nothing
            If Not GetImage(aImage) Then Return _rVal
            If bUpdatePath Or IsDirty Then UpdatePath(False, aImage)
            Dim i As Integer
            Dim iCnt As Integer
            Dim aOCS As TPLANE = TPLANE.World
            Dim tname As String = String.Empty
            If aInstances Is Nothing Then aInstances = Instances
            If bSuppressInstances Then iCnt = 1 Else iCnt = aInstances.Count + 1
            For i = 1 To iCnt
                If aInstance <= 0 Or i = aInstance Then _rVal.Add(DXFProps(aInstances, i, aOCS, tname))
            Next i
            If iCnt > 1 Then
                _rVal.Name = tname & "-" & iCnt & " INSTANCES"
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
            _rVal = New TPROPERTYARRAY(aInstance:=aInstance)
            Dim myProps As New TPROPERTIES(Properties)
            Dim aTrs As New TTRANSFORMS
            Dim aPl As TPLANE = PlaneV
            Dim scl As Double = 1
            Dim ang As Double = 0
            Dim bInv As Boolean
            Dim bLft As Boolean
            If aInstance > 1 Then
                If aInstances Is Nothing Then aInstances = Instances
                aTrs = aInstances.Transformations(aInstance - 1, scl, ang, bInv, bLft)
                myProps = New TPROPERTIES(Properties)
                TTRANSFORMS.Apply(aTrs, aPl)
                rTypeName = Trim(myProps.Item(1).Value)
            Else
                myProps = New TPROPERTIES(Properties)
                myProps.Handle = Handle
                aOCS = TPLANE.ArbitraryCS(aPl.ZDirection)
                myProps.SetVectorGC(210, aOCS.ZDirection, bSuppress:=Math.Round(aOCS.ZDirection.Z, 6) = 1)
                rTypeName = "SHAPE"
                SetProps(myProps)
                UpdateCommonProperties(rTypeName)
                myProps = New TPROPERTIES(Properties)
            End If
            Dim sz As Double
            Dim v1 As TVECTOR
            Dim angl As Double
            v1 = InsertionPtV
            sz = Height * scl
            If sz = 0 Then Return _rVal
            angl = Rotation
            If aInstance > 1 Then
                If ang <> 0 Then angl = TVALUES.NormAng(angl + ang)
                TTRANSFORMS.Apply(aTrs, v1)
            End If
            myProps.SetVal("Size", sz)
            myProps.SetVal("Rotation Angle", angl)
            myProps.SetVectorGC(10, v1, 1)
            If aInstance = 1 Then SetProps(myProps)
            _rVal.Add(myProps, rTypeName, True, True)
            Return _rVal
        End Function

#End Region 'Methods
    End Class 'dxeShape
End Namespace
