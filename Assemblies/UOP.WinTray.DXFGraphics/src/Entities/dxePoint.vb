Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxePoint
        Inherits dxfEntity
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxGraphicTypes.Point)
        End Sub

        Public Sub New(aEntity As dxePoint, Optional bCloneInstances As Boolean = False)
            MyBase.New(dxxGraphicTypes.Point, aEntityToCopy:=aEntity, bCloneInstances:=bCloneInstances)

        End Sub

        Public Sub New(aPoint As iVector, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aPlane As dxfPlane = Nothing)
            MyBase.New(dxxGraphicTypes.Point, aDisplaySettings)

            PlaneV = New TPLANE(aPlane, aPoint)
            CenterV = New TVECTOR(aPoint).ProjectedTo(PlaneV)
        End Sub
        Public Sub New(aX As Double, aY As Double, aZ As Double, Optional aDisplaySettings As dxfDisplaySettings = Nothing)
            MyBase.New(dxxGraphicTypes.Point, aDisplaySettings)
            CenterV = New TVECTOR(aX, aY, aZ)
        End Sub
        Friend Sub New(aVector As TVECTOR, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aLayer As String = "", Optional aIdentifier As String = "")
            MyBase.New(dxxGraphicTypes.Point, aDisplaySettings)
            CenterV = aVector
            If Not String.IsNullOrWhiteSpace(aLayer) Then LayerName = aLayer
            Identifier = aIdentifier
        End Sub

        Friend Sub New(aEntityStructure As TENTITY, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional bNewGUID As Boolean = False)
            MyBase.New(dxxGraphicTypes.Point, aDisplaySettings)
            If aEntityStructure.GraphicType = GraphicType Then
                Properties.CopyValues(aEntityStructure.Props, "5", aNamesToSkip:="*GUID")
                DefPts.Copy(aEntityStructure.DefPts)
            End If


        End Sub
        Friend Sub New(aObject As TOBJECT)
            MyBase.New(dxxGraphicTypes.Point)
            DefineByObject(aObject)
        End Sub

#End Region 'Constructors
#Region "Properties"
        Public Property Center As dxfVector
            Get
                '^the point center
                Return DefPts.Vector1
            End Get
            Set(value As dxfVector)
                '^the point center
                MyBase.DefPts.SetVector(value, 1)
            End Set
        End Property
        Friend Property CenterV As TVECTOR
            Get
                '^the point center
                Return DefPts.VectorGet(1)
            End Get
            Set(value As TVECTOR)
                '^the point center
                DefPts.VectorSet(1, value)

            End Set
        End Property
        Public Property Coordinates As String
            '^returns a text string containing the Point's coordinates
            '~points have a basic coordinate string like "(12.12,25.70,18.00)" but vertices
            Get
                Return Vector.Coordinates(0)
            End Get
            Set(value As String)
                '^returns a text string containing the Point's coordinates
                '~points have a basic coordinate string like "(12.12,25.70,18.00)" but vertices
                '~have more vertex definition information in the comma deliminated string like
                '~"(12.12,25.70,18.00, vertexttype,radius,inverted,color,linetype)"
                Vector = TVECTOR.DefineByString(value, Vector)
            End Set
        End Property
        Friend ReadOnly Property Core As dxfVector
            Get
                Return DefPts.Vector1
            End Get
        End Property
        Friend Property Vector As TVECTOR
            Get
                Return DefPts.VectorGet(1)
            End Get
            Set(value As TVECTOR)
                DefPts.VectorSet(1, value)
            End Set
        End Property
#End Region 'Properties
#Region "MustOverride Entity Methods"

        Friend Overrides Sub DefineByObject(aObj As TOBJECT, Optional bNoHandles As Boolean = False, Optional aStyle As dxfTableEntry = Nothing, Optional aBlock As dxfBlock = Nothing)
            Reactors.Append(aObj.Reactors, bClear:=True)
            ExtendedData.Append(aObj.ExtendedData, bClear:=True)
            DisplayStructure = aObj.DisplayVars
            PlaneV = TPLANE.ArbitraryCS(aObj.Properties.GCValueV(210, TVECTOR.WorldZ))
            Vector = aObj.Properties.GCValueV(10)
            If Not bNoHandles Then Handle = aObj.Properties.GCValueStr(5)
        End Sub

        ''' <summary>
        ''' returns a clone of the entity transfered to the passed plane
        ''' </summary>
        ''' <remarks>the entities defining vectors are converted to vectors with respect to the entities OCS plane and then redefines them based on the passed plane </remarks>
        ''' <param name="aPlane"></param>
        '''<param name="aFromPlane">the plane to define the definition points with respect to prior to transfer to the new plane. If not passed the entities OCS is used </param>
        ''' <returns></returns>
        Public Overloads Function TransferedToPlane(aPlane As dxfPlane, Optional aFromPlane As dxfPlane = Nothing) As dxePoint
            Dim _rVal As dxePoint = Me.Clone()
            _rVal.TransferToPlane(aPlane, aFromPlane)
            Return _rVal
        End Function

        ''' <summary>
        ''' Returns a new object with properties matching those of the cloned object
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Function Clone() As dxePoint
            Return New dxePoint(Me)
        End Function
#End Region 'MustOverride Entity Methods
#Region "Methods"
        Public Function CoordinatesR(Optional aPrecis As Integer = 3) As String
            '^returns a text string containing the Point's coordinates rounded to the passed precisions
            Return Vector.Coordinates(aPrecis)
        End Function
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
            Dim myProps As New TPROPERTIES(ActiveProperties())
            Dim aTrs As New TTRANSFORMS
            Dim aPl As TPLANE = PlaneV
            Dim scl As Double = 1
            Dim ang As Double = 0
            Dim bInv As Boolean
            Dim bLft As Boolean
            If aInstance > 1 Then
                If aInstances Is Nothing Then aInstances = Instances
                aTrs = aInstances.Transformations(aInstance - 1, scl, ang, bInv, bLft)
                TTRANSFORMS.Apply(aTrs, aPl)
                rTypeName = Trim(myProps.Item(1).Value)
            Else
                myProps.Handle = Handle
                aOCS = TPLANE.ArbitraryCS(aPl.ZDirection)
                myProps.SetVectorGC(210, aOCS.ZDirection, bNormalize:=Math.Round(aOCS.ZDirection.Z, 6) = 1)
                SetProps(myProps)
                UpdateCommonProperties("POINT")
                rTypeName = "POINT"
                myProps = New TPROPERTIES(Properties)
            End If
            myProps.SetVectorGC(10, aPl.Origin)
            myProps.SetVal("X Axis Rotation", aPl.XDirection.AngleTo(aOCS.XDirection, aOCS.XDirection))
            If aInstance = 1 Then SetProps(myProps)
            _rVal.Add(myProps, rTypeName, True, True)
            Return _rVal
        End Function

#End Region 'Methods
#Region "Shared Methods"

#End Region 'Shared Methods
    End Class 'dxePoint
End Namespace
