Imports System.Security.Cryptography
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip
Imports System.Windows.Shapes
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxoDrawingTool
#Region "Members"
        Private _ImageGUID As String
        Friend _ImagePtr As WeakReference(Of dxfImage)
#End Region 'Members
#Region "Constructors"
        Friend Sub New(aImage As dxfImage)
            If aImage IsNot Nothing Then
                _ImageGUID = aImage.GUID
                _ImagePtr = New WeakReference(Of dxfImage)(aImage)
            End If
        End Sub
#End Region 'Constructors
#Region "Properties"


        Public ReadOnly Property Image As dxfImage
            Get
                Dim _rVal As dxfImage = Nothing
                If GetImage(_rVal) Then Return _rVal Else Return Nothing
            End Get
        End Property


        Public ReadOnly Property PaperScale As Double
            Get
                Dim img As dxfImage = Nothing
                If Not GetImage(img) Then Return 1
                Return img.Display.PaperScale
            End Get
        End Property

        Public ReadOnly Property aDim As dxoDimTool
            Get
                '^provides access to the parent images DimTool
                Dim myImage As dxfImage = Nothing
                If GetImage(myImage) Then Return myImage.DimTool Else Return New dxoDimTool(Nothing)
            End Get
        End Property
        Public ReadOnly Property aSymbol As dxoSymbolTool
            Get
                '^provides access to the parent images SymbolTool
                Dim myImage As dxfImage = Nothing
                If GetImage(myImage) Then Return myImage.SymbolTool Else Return New dxoSymbolTool(Nothing)
            End Get
        End Property
        Public ReadOnly Property aLeader As dxoLeaderTool
            '^provides access to the parent images leader tool
            Get
                Dim myImage As dxfImage = Nothing
                If GetImage(myImage) Then Return myImage.LeaderTool Else Return New dxoLeaderTool(Nothing)
            End Get
        End Property
        Public Property ImageGUID As String
            Get
                Return _ImageGUID
            End Get
            Friend Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then _ImageGUID = "" Else _ImageGUID = value.Trim
                If _ImagePtr Is Nothing And Not String.IsNullOrWhiteSpace(_ImageGUID) Then
                    Dim img As dxfImage = dxfEvents.GetImage(_ImageGUID)
                    If img IsNot Nothing Then _ImagePtr = New WeakReference(Of dxfImage)(img)
                End If
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Friend Function GetImage(ByRef rImage As dxfImage) As Boolean
            If rImage Is Nothing Then
                Dim img As dxfImage = Nothing
                If Not String.IsNullOrWhiteSpace(ImageGUID) Then
                    If _ImagePtr IsNot Nothing Then
                        If _ImagePtr.TryGetTarget(img) Then

                            rImage = img
                            Return True
                        End If
                    End If
                    rImage = dxfEvents.GetImage(ImageGUID)
                    If rImage IsNot Nothing Then
                        _ImagePtr = New WeakReference(Of dxfImage)(rImage)
                        Return True
                    End If
                End If
            Else
                _ImageGUID = rImage.GUID
                _ImagePtr = New WeakReference(Of dxfImage)(rImage)
            End If
            Return rImage IsNot Nothing
        End Function


        ''' <summary>
        ''' ^returns a triangular 3 vertex closed polyline otherwise or a 3 vertext solid based on the passed input
        ''' </summary>
        ''' ''' <param name="aVectorXYZ">the tip of the triangular pointer</param>
        ''' <param name="aWidth">the base width of the triangular pointer</param>
        ''' <param name="aHeightFactor">the height factor of the triangular pointer. this times the base width determines the height of  the triangular pointer(default is 1 min is 0.1 max is 5)</param>
        ''' <param name="aRotation">a rotation to apply. if null, the rotation property of the point vector</param>
        ''' <param name="bReturnHollow">if True the function returns a 3 vertex closed polyline otherwise it returns a solid</param>
        ''' <param name="aPlane">a working plane</param>
        ''' <param name="aDisplaySettings">the display settings to apply to overide the current image settings</param>
        ''' <param name="bSuppressUCS">flag to suppress conversion of points to the current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress the current elevation when converting points to the current ucs</param>
        ''' <param name="aLTLFlag">an override LinetypeLayer setting to apply </param>

        Public Function aPointer(aVectorXYZ As iVector, aWidth As Double, Optional aHeightFactor As Double = 1.2, Optional aRotation As Double? = Nothing, Optional bReturnHollow As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As dxfEntity
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            Try

                Dim v1 As dxfVector = myImage.CreateVector(aVectorXYZ, bSuppressUCS, bSuppressElevation, aPlane)
                If aDisplaySettings Is Nothing Then
                    aDisplaySettings = New dxfDisplaySettings(dxfImageTool.DisplayStructure(myImage, v1.LayerName, v1.Color, v1.Linetype, aLTLFlag:=aLTLFlag))
                End If
                Return myImage.SaveEntity(myImage.Primatives.Pointer(v1, aWidth, aHeightFactor, aRotation, bReturnHollow, aPlane, aDisplaySettings))

            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function


        ''' <summary>
        ''' ^returns a triangular 3 vertex closed polyline otherwise or a 3 vertext solid based on the passed input
        ''' </summary>
        ''' ''' <param name="aVectorsXYZ">the tips of the triangular pointer</param>
        ''' <param name="aWidth">the base width of the triangular pointer</param>
        ''' <param name="aHeightFactor">the height factor of the triangular pointer. this times the base width determines the height of  the triangular pointer(default is 1 min is 0.1 max is 5)</param>
        ''' <param name="aRotation">a rotation to apply. if null, the rotation property of the point vector</param>
        ''' <param name="bReturnHollow">if True the function returns a 3 vertex closed polyline otherwise it returns a solid</param>
        ''' <param name="aPlane">a working plane</param>
        ''' <param name="bSuppressInstances">flag to suppress creating a single pointer with multiple instances defined. if True, one pointer is created for each point vector</param>
        ''' <param name="aDisplaySettings">the display settings to apply to overide the current image settings</param>
        ''' <param name="bSuppressUCS">flag to suppress conversion of points to the current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress the current elevation when converting points to the current ucs</param>
        ''' <param name="aLTLFlag">an override LinetypeLayer setting to apply </param>

        Public Function aPointers(aVectorsXYZ As IEnumerable(Of iVector), aWidth As Double, Optional aHeightFactor As Double = 1.2, Optional aRotation As Double? = Nothing, Optional bReturnHollow As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional bSuppressInstances As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As dxfEntities
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return New dxfEntities
            If aVectorsXYZ Is Nothing Then Return New dxfEntities
            Try

                Dim _rVal As New dxfEntities
                Dim verts As TVERTICES = myImage.CreateUCSVertices(aVectorsXYZ, bSuppressUCS, bSuppressElevation, aPlane, False, True)
                If verts.Count <= 0 Then Return _rVal
                Dim dsp As dxfDisplaySettings = aDisplaySettings
                Dim ctrs As New colDXFVectors(verts)
                Dim v1 As dxfVector = ctrs.Item(1)
                If dsp Is Nothing Then
                    dsp = New dxfDisplaySettings(dxfImageTool.DisplayStructure(myImage, v1.LayerName, v1.Color, v1.Linetype, aLTLFlag:=aLTLFlag))
                End If
                Dim ptr As dxfEntity = myImage.Primatives.Pointer(v1, aWidth, aHeightFactor, aRotation, bReturnHollow, aPlane, dsp)
                ptr = myImage.SaveEntity(ptr)
                _rVal.Add(ptr)
                If (ctrs.Count > 1) Then

                    If bSuppressInstances Then
                        For i As Integer = 2 To ctrs.Count
                            Dim v2 As dxfVector = ctrs.Item(i)
                            Dim ptr2 As dxfEntity = ptr.Clone
                            ptr2.MoveFromTo(v1, v2)
                            If Not aRotation.HasValue Then
                                Dim ang As Double = TVALUES.NormAng(v2.Rotation - v1.Rotation, False, True, True)
                                If (ang <> 0) Then
                                    ptr2.RotateAbout(ptr.Plane.ZAxis(1, v2), ang)
                                End If
                            End If


                            _rVal.Add(myImage.SaveEntity(ptr2))
                        Next i
                    Else

                        Dim insts As dxoInstances = ptr.Instances


                        For i As Integer = 2 To ctrs.Count
                            Dim v2 As dxfVector = ctrs.Item(i)
                            Dim v3 As dxfVector = v2 - v1
                            Dim inst As TINSTANCE = New TINSTANCE(aXOffset:=v3.X, aYOffset:=v3.Y, aRotation:=v1.Rotation - v2.Rotation)
                            If aRotation.HasValue Then inst.Rotation = 0
                            insts.AddV(inst, True)
                        Next i

                        ptr.Instances = insts '  .DefineWithVectors(ctrs, ctrs(0), False)
                    End If

                    ''ptr.InstancesDefineWithVectors(verts, v1, False, bSuppressRotations:=False)
                End If
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        ''' <summary>
        ''' used to draw an arc to the display
        ''' </summary>
        ''' <remarks>does nothing if the passed arguments are invalid (i.e.  radius = 0)</remarks>
        ''' <param name="aCenterXY">the point to use as the arcs center</param>
        ''' <param name="aRadius">the radius of the arc to be created</param>
        ''' <param name="aStartAngle">the start angle for the arc (define counterclockwise from the positive X axis)</param>
        ''' <param name="aEndAngle">the end angle for the arc (define counterclockwise from the positive X axis)</param>
        ''' <param name="bClockWise">flag indicating if the arc should be define in a clockwise direction (counter clockwise is default)</param>
        ''' <param name="aDisplaySettings">the display settings to apply to overide the current image settings</param>
        ''' <param name="aInstances">new instances to assign to the new line</param>
        ''' <param name="bSuppressUCS">flag to suppress conversion of points to the current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress the current elevation when converting points to the current ucs</param>
        ''' <param name="aLTLFlag">an override LinetypeLayer setting to apply </param>
        ''' <returns></returns>
        Public Function aArc(aCenterXY As iVector, aRadius As Double, Optional aStartAngle As Double = 0.0, Optional aEndAngle As Double = 360, Optional bClockWise As Boolean = False, Optional aInstances As dxoInstances = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As dxeArc

            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            Try
                Dim rad As Double = aRadius
                If rad = 0 Then Return Nothing
                If rad < 0 Then bClockWise = Not bClockWise
                rad = Math.Abs(rad)
                aStartAngle = TVALUES.NormAng(aStartAngle, ThreeSixtyEqZero:=True)
                aEndAngle = TVALUES.NormAng(aEndAngle, ThreeSixtyEqZero:=False)
                If aEndAngle = aStartAngle Then
                    aStartAngle = 0
                    aEndAngle = 360
                End If
                Dim dsp As dxfDisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Arc, aSettingsToCopy:=aDisplaySettings, aLineType:=dxxLinetypeLayerFlag.Undefined)
                Dim aCS As dxfPlane = Nothing
                Dim aCenter As dxfVector = myImage.CreateVector(aCenterXY, bSuppressUCS, bSuppressElevation, aCS)

                Dim _rVal As New dxeArc(aCenter, rad, aStartAngle, aEndAngle, bClockWise, aCS) With
                   {
                        .DisplaySettings = dsp,
                        .Instances = aInstances
                    }
                'save the entity

                Return myImage.SaveEntity()

            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        ''' <summary>
        ''' used to draw an arc between two points to the display
        ''' </summary>
        ''' <remarks>does nothing if the passed arguments are invalid (i.e.  radius = 0)</remarks>
        ''' <param name="aStartPtXY">the point to use as the arcs start point</param>
        ''' <param name="aEndPtXY">the point to use as the arcs end point</param>
        ''' <param name="aRadius">the radius to use to span between the points</param>
        ''' <param name="bClockWise">flag indicating if the arc should be define in a clockwise direction (counter clockwise is default) </param>
        ''' <param name="bReturnLargerArc ">flag to return the larger arc if there are two possible arcs </param>
        ''' <param name="aDisplaySettings">the display settings to apply to overide the current image settings</param>
        ''' <param name="aInstances">new instances to assign to the new line</param>
        ''' <param name="bSuppressUCS">flag to suppress conversion of points to the current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress the current elevation when converting points to the current ucs</param>
        ''' <param name="aLTLFlag">an override LinetypeLayer setting to apply </param>
        ''' <returns></returns>
        Public Function aArc(aStartPtXY As iVector, aEndPtXY As iVector, aRadius As Double, Optional bClockWise As Boolean = False, Optional bReturnLargerArc As Boolean = False, Optional aInstances As dxoInstances = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As dxeArc
            Dim _rVal As dxeArc = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return _rVal
            Try
                Dim aOCS As dxfPlane = Nothing
                Dim sp As TVECTOR = myImage.CreateUCSVector(aStartPtXY, False, False, aOCS)
                Dim ep As TVECTOR = myImage.CreateUCSVector(aEndPtXY, True, True, aOCS)
                Dim bFlag As Boolean = False
                Dim d1 = dxfProjections.DistanceTo(sp, ep)
                If aRadius = 0 Or d1 > 2 * aRadius Then Throw New Exception("Unable to create an arce between the passed points with the requested radius")
                Dim arc As TSEGMENT = dxfPrimatives.ArcBetweenPointsV(aRadius, sp, ep, aOCS.Strukture, bClockWise, bReturnLargerArc, True, bFlag, Nothing)
                If bFlag Then
                    _rVal = New dxeArc(arc)
                Else
                    Throw New Exception("Unable to create an arce between the passed points with the requested radius")
                End If
                _rVal.DisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Arc, aSettingsToCopy:=aDisplaySettings, aLTLFlag:=aLTLFlag)
                _rVal.Instances = aInstances

                'save the entity
                Return myImage.SaveEntity(_rVal)

            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        ''' <summary>
        ''' used to draw a 3 point arc between to the display
        ''' </summary>
        ''' <remarks>does nothing if the passed arguments are invalid (i.e.  radius = 0)</remarks>
        ''' <param name="aStartPtXY">the point to use as the arcs start point</param>
        ''' <param name="aArcPtXY">a point on the new arc (not equal to the start or end points)</param>
        ''' <param name="aEndPtXY">the point to use as the arcs end point</param>
        ''' <param name="aDisplaySettings">the display settings to apply to overide the current image settings</param>
        ''' <param name="aInstances">new instances to assign to the new line</param>
        ''' <param name="bSuppressUCS">flag to suppress conversion of points to the current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress the current elevation when converting points to the current ucs</param>
        ''' <param name="aLTLFlag">an override LinetypeLayer setting to apply </param>
        ''' <returns></returns>

        Public Function aArc(aStartPtXY As iVector, aArcPtXY As iVector, aEndPtXY As iVector, Optional aInstances As dxoInstances = Nothing, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As dxeArc
            Dim _rVal As dxeArc = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            Try
                If aStartPtXY Is Nothing Or aArcPtXY Is Nothing Or aEndPtXY Is Nothing Then Throw New Exception("Undefined Point Object Passed")
                Dim aOCS As dxfPlane = Nothing
                Dim sp As TVECTOR = myImage.CreateUCSVector(aStartPtXY, False, False, aOCS)
                Dim aP As TVECTOR = myImage.CreateUCSVector(aArcPtXY, True, True, aOCS)
                Dim ep As TVECTOR = myImage.CreateUCSVector(aEndPtXY, True, True, aOCS)

                Dim errstr As String = String.Empty
                If aP = sp Then Throw New Exception("The Passed Arc Point Is Coincident to the Passed Start Point")
                If aP = ep Then Throw New Exception("The Passed Arc Point Is Coincident to the Passed End Point")
                If sp = ep Then Throw New Exception("The Passed Start Point Is Coincident to the Passed End Point")
                Dim arc As TARC = dxfPrimatives.ArcThreePointV(sp, aP, ep, True, aOCS.Strukture, errstr)
                If Not String.IsNullOrWhiteSpace(errstr) Then Throw New Exception(errstr)
                _rVal = New dxeArc(arc, myImage.GetDisplaySettings(dxxEntityTypes.Arc, aSettingsToCopy:=aDisplaySettings, aLTLFlag:=aLTLFlag)) With
                 {
                    .Instances = aInstances
                }


                'save the entity

                Return myImage.SaveEntity(_rVal)
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
                Return _rVal
            End Try
        End Function


        ''' <summary>
        ''' used to draw a arc to the display
        ''' </summary>
        ''' <remarks>does nothing if the passed arguments are invalid (i.e. undefined points)</remarks>
        ''' <param name="aArcObj">the arc to draw</param>
        ''' <param name="aDisplaySettings">the display settings to apply</param>
        ''' <param name="aInstances">new instances to assign to the new arc</param>
        ''' <param name="bSuppressUCS">flag to suppress conversion of points to the current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress the current elevation when converting points to the current ucs</param>
        ''' <param name="aLTLFlag">an override ArctypeLayer setting to apply </param>
        ''' <returns></returns>
        Public Function aArc(aArcObj As iArc, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aInstances As dxoInstances = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As dxeArc
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Or aArcObj Is Nothing Then Return Nothing

            'set the arc properties
            Try

                Dim dxarc As dxeArc = New dxeArc(aArcObj, bCloneInstances:=True)
                If dxarc.SpannedAngle = 0 Then Return Nothing
                If aDisplaySettings Is Nothing Then aDisplaySettings = New dxfDisplaySettings(dxarc.DisplaySettings)
                Dim dsp As dxfDisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Arc, aSettingsToCopy:=aDisplaySettings, aLTLFlag:=aLTLFlag)
                dxarc.DisplaySettings = dsp
                If (aInstances IsNot Nothing) Then dxarc.Instances = aInstances

                Return myImage.SaveEntity(dxarc)
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        ''' <summary>
        ''' used to draw arcs to the display
        ''' </summary>
        ''' <remarks>does nothing if the passed arguments are invalid (i.e. undefined points)</remarks>
        ''' <param name="aArcObjs">the arcs to draw</param>
        ''' <param name="aDisplaySettings">the display settings to apply</param>
        ''' <param name="aInstances">new instances to assign to the new arc</param>
        ''' <param name="bSuppressUCS">flag to suppress conversion of points to the current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress the current elevation when converting points to the current ucs</param>
        ''' <param name="aLTLFlag">an override ArctypeLayer setting to apply </param>
        ''' <returns></returns>
        Public Function aArcs(aArcObjs As List(Of iArc), Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aInstances As dxoInstances = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As List(Of dxeArc)
            Dim myImage As dxfImage = Nothing
            Dim _rVal As New List(Of dxeArc)
            If Not GetImage(myImage) Or aArcObjs Is Nothing Then Return _rVal

            'set the arc properties
            Try
                For Each arc As iArc In aArcObjs
                    Dim dxarc As dxeArc = Me.aArc(arc, aDisplaySettings, aInstances, bSuppressUCS, bSuppressElevation, aLTLFlag:=aLTLFlag)
                    If dxarc IsNot Nothing Then _rVal.Add(dxarc)
                Next
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return _rVal
        End Function

        Public Function aArrowHead(aPointXY As iVector, aArrowType As dxxArrowHeadTypes, Optional aAngle As Double = 0.0, Optional aScaleFactor As Double = 1, Optional aArrowIndicator As dxxArrowIndicators = dxxArrowIndicators.One, Optional aDimStyle As String = "", Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxfEntity
            Dim _rVal As dxfEntity = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            '#1the point of the desired arrow head
            '#2the type of arrow head to draw
            '#3the insertion angle of the arrowhead
            '#4the insertion scale of the arrowhead
            '#5the arrow head (1,2 or leader) to use if the arrow type is ByStyle
            '#6the dimstyle to use to get the arrow head other than the current dim style
            '#7a display settigs to override the current immage display settings for new entities

            '^used to draw arrow heads
            '^arrow type "ClosedFilled" Returns a Triangular Solid all others return an insert of the requested arrow block
            Try
                Dim aBlock As dxfBlock = Nothing
                Dim aSl As dxeSolid
                Dim aPt As dxfVector
                Dim aIns As dxeInsert
                Dim bname As String
                Dim aStyle As dxfTableEntry = Nothing
                If aArrowType = dxxArrowHeadTypes.None Then Throw New Exception("Arrow Type Can Not Be 'NoneArrow(Type Can Not Be)) 'None '")
                If aArrowType = dxxArrowHeadTypes.UserDefined Then Throw New Exception("Arrow Type Can Not Be User Defined")
                If aArrowType = dxxArrowHeadTypes.Suppressed Then Throw New Exception("Arrow Type Can Not Be Suppressed")
                If aArrowType = dxxArrowHeadTypes.ByStyle Then
                    If String.IsNullOrWhiteSpace(aDimStyle) Then aDimStyle = myImage.Header.DimStyleName
                    aDimStyle = aDimStyle.Trim()
                    If String.IsNullOrWhiteSpace(aDimStyle) Then aDimStyle = "Standard"

                    If Not myImage.DimStyles.TryGet(aDimStyle, aStyle) Then aStyle = myImage.DimStyle("Standard")
                    If aArrowIndicator = dxxArrowIndicators.Two Then
                        bname = aStyle.Properties.ValueS("*DIMBLK2_NAME") 'aStyle.ArrowHeadBlock2
                    ElseIf aArrowIndicator = dxxArrowIndicators.Leader Then
                        bname = aStyle.Properties.ValueS("*DIMLDRBLK_NAME") 'aStyle.ArrowHeadBlockLeader
                    Else
                        bname = aStyle.Properties.ValueS("*DIMBLK1_NAME") 'aStyle.ArrowHeadBlock1
                    End If
                    If bname = "" Then bname = "_ClosedFilled"
                Else
                    bname = dxfEnums.Description(aArrowType)
                End If
                'get the block
                If bname <> "" Then aBlock = myImage.Blocks.GetByName(bname)
                If aBlock Is Nothing Then Throw New Exception("Unrecognized Arrow Head Requested")
                If aScaleFactor <= 0 Then aScaleFactor = 1
                aPt = myImage.CreateVector(aPointXY, False)
                If String.Compare(bname, "_ClosedFilled", True) <> 0 Then
                    aIns = New dxeInsert(aBlock) With {
                        .PlaneV = myImage.obj_UCS,
                        .DisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Insert, aSettingsToCopy:=aDisplaySettings),
                        .InsertionPtV = aPt.Strukture,
                        .RotationAngle = aAngle,
                        .ScaleFactor = aScaleFactor
                    }
                    _rVal = myImage.SaveEntity(aIns)
                Else
                    aSl = aBlock.Entities.Item(1).Clone
                    aSl.DisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Solid, aSettingsToCopy:=aDisplaySettings)
                    aSl.PlaneV = myImage.obj_UCS
                    aSl.Rescale(aScaleFactor)
                    aSl.Vertices.RotateAbout(New dxfVector, aAngle)
                    aSl.MoveFromTo(New dxfVector, aPt)
                    _rVal = myImage.SaveEntity(aSl)
                End If
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
Err:
        End Function
        Public Function aBezier(StartPtXY As iVector, ControlPt1XY As iVector, ControlPt2XY As iVector, EndPtXY As iVector, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxeBezier
            Dim _rVal As dxeBezier = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return _rVal
            '#1the start point for the new curve
            '#2the first control for the new curve
            '#3the second control point for the new curve
            '#4the end point for the new curve
            '#5a display settigs to override the current immage display settings for new entities
            'draws a b-spline bezier arc using the 4 passed control points
            Try
                Dim aOCS As dxfPlane = Nothing
                Dim sp As TVECTOR = myImage.CreateUCSVector(StartPtXY, False, False, aOCS)
                Dim cp1 As TVECTOR = myImage.CreateUCSVector(ControlPt1XY, True, True, aOCS)
                Dim cp2 As TVECTOR = myImage.CreateUCSVector(ControlPt2XY, True, True, aOCS)
                Dim ep As TVECTOR = myImage.CreateUCSVector(EndPtXY, True, True, aOCS)
                _rVal = New dxeBezier(sp, cp1, cp2, ep, New TPLANE(aOCS), myImage.GetDisplaySettings(dxxEntityTypes.Bezier, aSettingsToCopy:=aDisplaySettings))
                'save the entity
                Return myImage.SaveEntity(_rVal)

            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        Public Function aBezierArcs(BezierPtsXY As IEnumerable(Of iVector), Optional bContiguous As Boolean = True, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As List(Of dxeBezier)
            Dim _rVal As New List(Of dxeBezier)
            If BezierPtsXY Is Nothing Then Return _rVal
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return _rVal
            Try

                Dim BezierPts As New colDXFVectors(myImage.CreateUCSVectors(BezierPtsXY))
                '#1the collection of points to draw arcs from
                '#2flag indicating if each subsequent curve starts at the endpoint of the previous curve
                '#3a layer to put the entities on instead of the current layer
                '#4a color to apply to the entities instead of the current color setting
                '#5a line type to apply to the entities instead of the current line type setting
                'draws a collection of  b-spline bezier curves using then passed set of points
                '^used to draw bezier arcs to the display
                '~returns the dxeBezier objects created.
                '~does nothing if the passed arguments are invalid

                'comput the arcs based on the points
                Dim aArcs As List(Of dxeBezier) = dxfUtils.PointsToBeziers(BezierPts, bContiguous, myImage.UCS, myImage.GetDisplaySettings(dxxEntityTypes.Bezier, aSettingsToCopy:=aDisplaySettings))
                If aArcs.Count <= 0 Then Return _rVal
                For i As Integer = 1 To aArcs.Count
                    _rVal.Add(myImage.SaveEntity(aArcs.Item(i)))
                Next i
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function



        ''' <summary>
        ''' used to draw centerlines on an entity. provides a shorthand way to draw two perpendicular centerlines intersecting at a point.
        ''' </summary>
        ''' <remarks>
        ''' the bounding rectangle of the passed entity is used to determine lengths of the centers. the vertical lines are assigned a Flag = "VERTICAL" and  the horizontal lines are assigned a Flag = "HORIZONTAL"
        ''' </remarks>
        ''' <param name="aRectangle">the entity to draw centerlines for</param>
        ''' <param name="aLengthAdder">a distance to add to the the length of the centerlines</param>
        ''' <param name="aSuppressHorizontal">flag to suppress the horizontal</param>
        ''' <param name="aSuppressVertical">flag to suppress the vertical</param>
        ''' <param name="aRotation">a rotation to apply</param>
        ''' <param name="aDisplaySettings">an override display settings to apply to the new lines</param>
        ''' <param name="aPlane">a plane to draw the lines on other than the current UCS XY plane</param>
        ''' <returns></returns>
        Public Function aCenterlines(aRectangle As dxfRectangle, Optional aLengthAdder As Double = 0.0, Optional aSuppressHorizontal As Boolean = False, Optional aSuppressVertical As Boolean = False, Optional aRotation As Double = 0.0, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aPlane As dxfPlane = Nothing) As List(Of dxeLine)
            Dim _rVal As New List(Of dxeLine)
            If aRectangle Is Nothing Then Return _rVal
            If aSuppressHorizontal And aSuppressVertical Then Return _rVal
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return _rVal

            Try
                If aDisplaySettings Is Nothing Then aDisplaySettings = dxfDisplaySettings.Null(aLinetype:=dxfLinetypes.Center)
                Dim dsp As dxfDisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Line, aSettingsToCopy:=aDisplaySettings)

                Dim aPl As TPLANE
                If dxfPlane.IsNull(aPlane) Then aPl = myImage.obj_UCS Else aPl = New TPLANE(aRectangle)
                Dim bndRec As dxfRectangle = aRectangle.Corners().BoundingRectangle(New dxfPlane(aPl))
                Dim d1 As Double = 0.5 * bndRec.Width
                Dim d2 As Double = 0.5 * bndRec.Height
                Dim tag As String = String.Empty
                If TypeOf aRectangle Is dxfRectangle Then
                    Dim dxfrec As dxfRectangle = DirectCast(aRectangle, dxfRectangle)
                    tag = dxfrec.Tag

                End If

                aPl = New TPLANE(bndRec)

                If Not aSuppressHorizontal And d1 > 0 Then
                    d1 += aLengthAdder
                    Dim l1 As New dxeLine With {
                    .DisplaySettings = dsp,
                    .StartPtV = aPl.Vector(-d1, 0, 0, aRotation:=aRotation),
                    .EndPtV = aPl.Vector(d1, 0, 0, aRotation:=aRotation),
                    .Flag = "HORIZONTAL",
                    .Tag = tag}

                    _rVal.Add(myImage.SaveEntity(l1))
                End If
                If Not aSuppressVertical And d2 > 0 Then
                    d2 += aLengthAdder
                    Dim l2 As New dxeLine With {
                   .DisplaySettings = dsp,
                    .StartPtV = aPl.Vector(0, -d2, 0, aRotation:=aRotation),
                    .EndPtV = aPl.Vector(0, d2, 0, aRotation:=aRotation),
                    .Flag = "VERTICAL",
                    .Tag = tag}
                    _rVal.Add(myImage.SaveEntity(l2))
                End If
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        ''' <summary>
        ''' used to draw centerlines on a collection of entities. provides a shorthand way to draw two perpendicular centerlines intersecting at a point.
        ''' </summary>
        ''' <remarks>
        ''' the bounding rectangle of the passed entity is used to determine lengths of the center lines. the vertical lines are assigned a Flag = "VERTICAL" and  the horizontal lines are assigned a Flag = "HORIZONTAL"
        ''' </remarks>
        ''' <param name="aEntities">the entitties to draw centerlines for</param>
        ''' <param name="aLengthAdder">a distance to add to the the length of the centerlines</param>
        ''' <param name="aSuppressHorizontal">flag to suppress the horizontal</param>
        ''' <param name="aSuppressVertical">flag to suppress the vertical</param>
        ''' <param name="aRotation">a rotation to apply</param>
        ''' <param name="aDisplaySettings">an override display settings to apply to the new lines</param>
        ''' <param name="aPlane">a plane to draw the lines on other than the current UCS XY plane</param>
        ''' <returns></returns>
        Public Function aCenterlines(aEntities As IEnumerable(Of dxfEntity), Optional aLengthAdder As Double = 0.0, Optional aSuppressHorizontal As Boolean = False, Optional aSuppressVertical As Boolean = False, Optional aRotation As Double = 0.0, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aPlane As dxfPlane = Nothing) As List(Of dxeLine)
            Dim _rVal As New List(Of dxeLine)
            If aEntities Is Nothing Then Return _rVal
            If aEntities.Count <= 0 Then Return _rVal
            If aSuppressHorizontal And aSuppressVertical Then Return _rVal
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return _rVal
            Try
                Dim l1 As dxeLine
                If aDisplaySettings Is Nothing Then aDisplaySettings = dxfDisplaySettings.Null(aLinetype:=dxfLinetypes.Center)
                Dim dsp As dxfDisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Line, aSettingsToCopy:=aDisplaySettings)
                Dim aPl As TPLANE

                If dxfPlane.IsNull(aPlane) Then aPl = myImage.obj_UCS Else aPl = New TPLANE(aPlane)
                For Each ent As dxfEntity In aEntities
                    If ent Is Nothing Then Continue For
                    ent.UpdatePath(False, myImage)
                    Dim bndRec As TPLANE = ent.BoundingRectangle(aPl.ToPlane).Strukture
                    Dim d1 As Double = 0.5 * bndRec.Width
                    Dim d2 As Double = 0.5 * bndRec.Height
                    If Not aSuppressHorizontal And d1 > 0 Then
                        d1 += aLengthAdder
                        Dim line As New TLINE(bndRec.Vector(-d1, 0, 0, aRotation:=aRotation), bndRec.Vector(d1, 0, 0, aRotation:=aRotation))
                        l1 = New dxeLine(line, False, 0, dsp) With {
                            .Flag = "HORIZONTAL",
                            .Tag = ent.Tag,
                            .PlaneV = aPl
                            }
                        _rVal.Add(myImage.SaveEntity(l1))
                    End If
                    If Not aSuppressVertical And d2 > 0 Then
                        d2 += aLengthAdder
                        Dim line As New TLINE(bndRec.Vector(0, -d2, 0, aRotation:=aRotation), bndRec.Vector(0, d2, 0, aRotation:=aRotation))
                        l1 = New dxeLine(line, False, 0, dsp) With {
                             .Flag = "VERTICAL",
                            .Tag = ent.Tag}
                        _rVal.Add(myImage.SaveEntity(l1))
                    End If

                Next
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        Public Function aCenterlines(aCentersXYZ As IEnumerable(Of iVector), aRadius As Double, Optional aScaleUp As Double = 1.2, Optional aRotation As Double = 0.0, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As List(Of dxeLine)
            Dim _rVal As New List(Of dxeLine)
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return _rVal
            aRadius = Math.Abs(aRadius)
            If aCentersXYZ Is Nothing Then Return _rVal
            '#1the point to center the lines
            '#2the radius of the circle to draw centerlines for
            '#3the amount to scale the centerlines over the passed radius ( must be greater that or equal to 1)
            '#4a rotation to apply
            '#5a linetype to apply
            '#6if true the points are treated as world points otherwise their ordinates are treated with respect to the current UCS
            '#7if true the current elevation is overridden by the z values of the passed vectors
            '^used to draw centerlines inside of a circle
            '~provides a shorthand way to draw two perpendicular centerlines intersecting at a point
            Dim xDir As TVECTOR
            Dim yDir As TVECTOR
            Dim v1 As TVERTEX
            If aDisplaySettings Is Nothing Then aDisplaySettings = dxfDisplaySettings.Null(aLinetype:=dxfLinetypes.Center)
            Dim dsp As dxfDisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Line, aSettingsToCopy:=aDisplaySettings)
            Dim rad As Double = Math.Abs(aRadius)
            Dim norad = rad <= 0
            If aScaleUp < 1 Then aScaleUp = 1
            rad = aRadius * aScaleUp
            Try

                Dim aCS As dxfPlane = Nothing
                Dim aCenters As TVERTICES = myImage.CreateUCSVertices(aCentersXYZ, bSuppressUCS, bSuppressElevation, aCS, False)
                xDir = aCS.AngularDirectionV(aRotation)
                yDir = aCS.AngularDirectionV(aRotation + 90)
                For i As Integer = 1 To aCenters.Count
                    v1 = aCenters.Item(i)
                    If norad Then
                        rad = Math.Abs(v1.Radius) * aScaleUp
                    End If
                    If rad <= 0 Then Continue For
                    Dim line As New TLINE(v1 + xDir * -rad, v1 + xDir * rad)
                    Dim cl As New dxeLine(line, aDisplaySettings:=dsp) With {.Tag = v1.Tag, .Flag = "HORIZONTAL"}
                    _rVal.Add(myImage.SaveEntity(cl))
                    line = New TLINE(v1 + yDir * -rad, v1 + yDir * rad)
                    cl = New dxeLine(line, aDisplaySettings:=dsp) With {.Tag = v1.Tag, .Flag = "VERTICAL"}
                    _rVal.Add(myImage.SaveEntity(cl))
                Next i
                'save to file

                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        Public Function aCenterlines(aCenterXY As iVector, aRadius As Double, Optional aScaleUp As Double = 1.2, Optional aRotation As Double = 0.0, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As List(Of dxeLine)

            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Or aCenterXY Is Nothing Then Return New List(Of dxeLine)
            aRadius = Math.Abs(aRadius)

            '#1the point to center the lines
            '#2the radius of the circle to draw centerlines for
            '#3the amount to scale the centerlines over the passed radius ( must be greater that or equal to 1)
            '#4a rotation to apply
            '#5a linetype to apply
            '#6if true the points are treated as world points otherwise their ordinates are treated with respect to the current UCS
            '#7if true the current elevation is overridden by the z values of the passed vectors
            '^used to draw centerlines inside of a circle
            '~provides a shorthand way to draw two perpendicular centerlines intersecting at a point
            Return aCenterlines(New List(Of iVector)({aCenterXY}), aRadius, aScaleUp, aRotation, aDisplaySettings, bSuppressUCS, bSuppressElevation)

        End Function

        ''' <summary>
        ''' used to draw centerlines on an entity. provides a shorthand way to draw two perpendicular centerlines intersecting at a point.
        ''' </summary>
        ''' <remarks>
        ''' the bounding rectangle of the passed entity is used to determine lengths of the centers. the vertical lines are assigned a Flag = "VERTICAL" and  the horizontal lines are assigned a Flag = "HORIZONTAL"
        ''' </remarks>
        ''' <param name="aEntity">the entity to draw centerlines for</param>
        ''' <param name="aLengthAdder">a distance to add to the the length of the centerlines</param>
        ''' <param name="aSuppressHorizontal">flag to suppress the horizontal</param>
        ''' <param name="aSuppressVertical">flag to suppress the vertical</param>
        ''' <param name="aRotation">a rotation to apply</param>
        ''' <param name="aDisplaySettings">an override display settings to apply to the new lines</param>
        ''' <param name="aPlane">a plane to draw the lines on other than the current UCS XY plane</param>
        ''' <returns></returns>
        Public Function aCenterlines(aEntity As dxfEntity, Optional aLengthAdder As Double = 0.0, Optional aSuppressHorizontal As Boolean = False, Optional aSuppressVertical As Boolean = False, Optional aRotation As Double = 0.0, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aPlane As dxfPlane = Nothing) As List(Of dxeLine)
            If aEntity Is Nothing Then Return New List(Of dxeLine)
            Dim ents As New List(Of dxfEntity)({aEntity})
            Return aCenterlines(ents, aLengthAdder, aSuppressHorizontal, aSuppressVertical, aRotation, aDisplaySettings, aPlane)
        End Function

        ''' <summary>
        ''' used to draw a circle to the display
        ''' </summary>
        ''' <remarks>if a null radius is passed, the radius property of the centers is used for each circle. circles with no radius are not saved.</remarks>
        ''' <param name="aCenter">the point to use as the circles center</param>
        ''' <param name="aRadius">the radius of the circles to be created</param>
        ''' <param name="aDisplaySettings">a display settings to apply</param>
        ''' <param name="bUsePointLayerColorLinetype">flag to use the display properties of the passed centers to apply to the new circles</param>
        ''' <param name="bSuppressUCS">flag to prevent the application of the current UCS to the new entity</param>
        ''' <param name="bSuppressElevation">flag to prevent the application of the current elevation to the new entity if the current UCS is not suppressed</param>
        ''' <returns></returns>
        Public Function aCircle(aCenter As iVector, aRadius As Double?, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional bUsePointLayerColorLinetype As Boolean = False, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeArc
            Dim _rVal As dxeArc = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            If aCenter Is Nothing Then aCenter = dxfVector.Zero

            Try
                Dim aCS As dxfPlane = Nothing
                Dim rad As Double = 0
                Dim cp As TVERTEX = myImage.CreateUCSVertex(aCenter, bSuppressUCS, bSuppressElevation, aCS)
                Dim aPl As New TPLANE(aCS)
                Dim dsp As New dxfDisplaySettings
                If aRadius.HasValue Then
                    rad = Math.Abs(aRadius.Value)
                End If
                Dim bRadPassed As Boolean = rad <> 0
                If bUsePointLayerColorLinetype Then
                    dsp.Strukture = dxfImageTool.DisplayStructure(myImage, cp.LayerName, cp.Color, cp.Linetype, aLTLFlag:=dxxLinetypeLayerFlag.Undefined)
                Else
                    If aDisplaySettings Is Nothing Then
                        dsp.Strukture = dxfImageTool.DisplayStructure(myImage, aLayer:="", aColor:=dxxColors.Undefined, aLineType:="", aLTLFlag:=dxxLinetypeLayerFlag.Undefined)
                    Else
                        dsp.Strukture = dxfImageTool.DisplayStructure(myImage, aDisplaySettings.LayerName, aDisplaySettings.Color, aDisplaySettings.Linetype, aLTLFlag:=dxxLinetypeLayerFlag.Undefined)
                    End If
                End If
                'get the radius values from the passed vertex
                If rad = 0 Then rad = Math.Abs(cp.Radius)
                If rad = 0 Then Throw New Exception("Zero Radius Passed")
                _rVal = New dxeArc(New dxfVector(cp), rad, aPlane:=aCS, aDisplaySettings:=dsp)
                'save the entity
                _rVal = myImage.SaveEntity(_rVal)
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return _rVal
        End Function
        ''' <summary>
        ''' used to draw dots to the display
        ''' </summary>
        ''' <remarks>if a null radius is passed, the radius property of the centers is used for each circle. circles with no radius are not saved.</remarks>
        ''' <param name="aDotType">the type of dot to create</param>
        ''' <param name="aCenter">the point to use as the circles center</param>
        ''' <param name="aRadius">the radius of the circles to be created</param>
        ''' <param name="aDisplaySettings">a display settings to apply</param>
        ''' <param name="bUsePointLayerColorLinetype">flag to use the display properties of the passed centers to apply to the new circles</param>
        ''' <param name="bSuppressUCS">flag to prevent the application of the current UCS to the new entity</param>
        ''' <param name="bSuppressElevation">flag to prevent the application of the current elevation to the new entity if the current UCS is not suppressed</param>
        ''' <returns></returns>

        Public Function aDot(aDotType As dxxDotShapes, aCenter As iVector, aRadius As Object, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aRotation As Double = 0, Optional bUsePointLayerColorLinetype As Boolean = False, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return _rVal
            Try

                If aDotType = dxxDotShapes.Undefined Then aDotType = dxxDotShapes.Circle
                Dim aCS As dxfPlane = Nothing
                Dim rad As Double
                Dim cp As TVERTEX = myImage.CreateUCSVertex(aCenter, bSuppressUCS, bSuppressElevation, aCS)
                Dim aPl As New TPLANE(aCS)
                Dim dsp As dxfDisplaySettings
                If TVALUES.IsNumber(aRadius) Then
                    rad = TVALUES.ToDouble(aRadius, True, aDefault:=0)
                Else
                    'get the radius values from the passed vertex
                    rad = Math.Abs(cp.Radius)
                End If

                If rad = 0 Then Throw New Exception("Zero Radius Passed")

                Dim bRadPassed As Boolean = rad <> 0
                If bUsePointLayerColorLinetype Then
                    dsp = myImage.GetDisplaySettings(dxxEntityTypes.Solid, aSettingsToCopy:=cp.DisplaySettings(aLinetype:=dxfLinetypes.Continuous))

                Else


                    If aDisplaySettings Is Nothing Then
                        dsp = myImage.GetDisplaySettings(dxxEntityTypes.Solid, aSettingsToCopy:=dxfDisplaySettings.Null(aLinetype:=dxfLinetypes.Continuous))
                    Else
                        dsp = myImage.GetDisplaySettings(dxxEntityTypes.Solid, aSettingsToCopy:=dxfDisplaySettings.Null(aDisplaySettings.LayerName, aDisplaySettings.Color, aLinetype:=dxfLinetypes.Continuous))

                    End If
                End If

                Dim dotents As List(Of dxfEntity) = dxfPrimatives.CreateDot(aDotType, New dxfVector(cp), rad, aRotation, dsp, New dxfPlane(aPl))
                For Each ent As dxfEntity In dotents
                    _rVal.Add(myImage.SaveEntity(ent))
                Next


            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return _rVal

        End Function
        ''' <summary>
        ''' used to draw dots to the display
        ''' </summary>
        ''' <remarks>if a null radius is passed, the radius property of the centers is used for each circle. circles with no radius are not saved.</remarks>
        ''' <param name="aDotType">the type of dot to create</param>
        ''' <param name="aCenters">the points to use as the circles center</param>
        ''' <param name="aRadius">the radius of the circles to be created</param>
        ''' <param name="aDisplaySettings">a display settings to apply</param>
        ''' <param name="bUsePointLayerColorLinetype">flag to use the display properties of the passed centers to apply to the new circles</param>
        ''' <param name="bSuppressUCS">flag to prevent the application of the current UCS to the new entity</param>
        ''' <param name="bSuppressElevation">flag to prevent the application of the current elevation to the new entity if the current UCS is not suppressed</param>
        ''' <returns></returns>

        Public Function aDots(aDotType As dxxDotShapes, aCenters As IEnumerable(Of iVector), aRadius As Double?, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aRotation As Double = 0, Optional bUsePointLayerColorLinetype As Boolean = False, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As List(Of dxfEntity)
            Dim _rVal As New List(Of dxfEntity)
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return _rVal
            If aCenters Is Nothing Then Return _rVal
            Try
                Dim rad As Double

                Dim aCS As dxfPlane = Nothing
                Dim ctrs As TVERTICES = myImage.CreateUCSVertices(aCenters, bSuppressUCS, bSuppressElevation, aCS, False, True)
                Dim dsp As New dxfDisplaySettings
                Dim bCS As dxfPlane
                If ctrs.Count <= 0 Then Return _rVal

                If aRadius.HasValue Then
                    rad = Math.Abs(aRadius.Value)
                End If
                Dim bRadPassed As Boolean = rad <> 0
                'get the radius values from the passed collection

                If aDisplaySettings Is Nothing Then
                    dsp = myImage.GetDisplaySettings(dxxEntityTypes.Solid, aSettingsToCopy:=dxfDisplaySettings.Null(aLinetype:=dxfLinetypes.Continuous))
                Else
                    dsp = myImage.GetDisplaySettings(dxxEntityTypes.Solid, aSettingsToCopy:=dxfDisplaySettings.Null(aDisplaySettings.LayerName, aDisplaySettings.Color, aLinetype:=dxfLinetypes.Continuous))

                End If

                'loop on the unique radii and create a single circle with mutiple instance for each

                For Each iv As iVector In aCenters

                    If iv Is Nothing Then Continue For

                    'see if the passed point has a assigned radius
                    Dim cp As dxfVector
                    If (TypeOf iv Is dxfVector) Then
                        cp = DirectCast(iv, dxfVector)
                    Else
                        cp = New dxfVector(iv)
                    End If

                    If Not bRadPassed Then rad = Math.Round(Math.Abs(cp.Radius), 6)
                    If bUsePointLayerColorLinetype Then
                        dsp = myImage.GetDisplaySettings(dxxEntityTypes.Solid, aSettingsToCopy:=cp.GetDisplaySettings(aLinetype:=dxfLinetypes.Continuous))
                    End If
                    If rad = 0 Then Continue For
                    bCS = New dxfPlane(aCS, cp)
                    Dim dotents As List(Of dxfEntity) = dxfPrimatives.CreateDot(aDotType, cp, rad, aRotation, dsp, bCS)
                    For Each ent As dxfEntity In dotents
                        _rVal.Add(myImage.SaveEntity(ent))
                    Next

                Next
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return _rVal
        End Function
        ''' <summary>
        ''' used to draw circles to the display
        ''' </summary>
        ''' <remarks>if a null radius is passed, the radius property of the centers is used for each circle. circles with no radius are not saved.</remarks>
        ''' <param name="aCenters">the point or points to use as the circles center</param>
        ''' <param name="aRadius">the radius of the circles to be created</param>
        ''' <param name="aDisplaySettings">a display settings to apply</param>
        ''' <param name="bUsePointLayerColorLinetype">flag to use the display properties of the passed centers to apply to the new circles</param>
        ''' <param name="bSuppressUCS">flag to prevent the application of the current UCS to the new entity</param>
        ''' <param name="bSuppressElevation">flag to prevent the application of the current elevation to the new entity if the current UCS is not suppressed</param>
        ''' <returns></returns>

        Public Function aCircles(aCenters As IEnumerable(Of iVector), aRadius As Double?, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional bUsePointLayerColorLinetype As Boolean = False, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined, Optional bSuppressInstances As Boolean = False) As List(Of dxeArc)
            Dim _rVal As New List(Of dxeArc)
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return _rVal
            If aCenters Is Nothing Then Return _rVal
            Try
                Dim rad As Double
                Dim cp As dxfVector
                Dim aCS As dxfPlane = Nothing
                Dim ctrs As TVERTICES = myImage.CreateUCSVertices(aCenters, bSuppressUCS, bSuppressElevation, aCS, False, True)
                Dim dsp As New dxfDisplaySettings
                Dim bCS As dxfPlane
                If ctrs.Count <= 0 Then Return _rVal
                If ctrs.Count = 1 Then bSuppressInstances = True
                If aRadius.HasValue Then
                    rad = Math.Abs(aRadius.Value)
                End If
                Dim bRadPassed As Boolean = rad <> 0
                'get the radius values from the passed collection
                If aDisplaySettings Is Nothing Then
                    dsp.Strukture = dxfImageTool.DisplayStructure(myImage, aLayer:="", aColor:=dxxColors.Undefined, aLineType:="", aLTLFlag:=aLTLFlag)
                Else
                    dsp.Strukture = dxfImageTool.DisplayStructure(myImage, aDisplaySettings.LayerName, aDisplaySettings.Color, aDisplaySettings.Linetype, aLTLFlag:=aLTLFlag)
                End If
                'loop on the unique radii and create a single circle with mutiple instance for each
                Dim arc As dxeArc = Nothing
                Dim insts As dxoInstances = Nothing
                For i As Integer = 1 To ctrs.Count
                    cp = ctrs.Item(i)
                    'see if the passed point has a assigned radius
                    If Not bRadPassed Then rad = Math.Round(Math.Abs(cp.Radius), 6)
                    If bUsePointLayerColorLinetype Then
                        dsp.Strukture = dxfImageTool.DisplayStructure(myImage, cp.LayerName, cp.Color, cp.Linetype, aLTLFlag:=dxxLinetypeLayerFlag.Undefined)
                    End If
                    If rad > 0 Then
                        bCS = New dxfPlane(aCS)
                        bCS.OriginV = cp.Strukture

                        If Not bSuppressInstances Then
                            If (arc Is Nothing) Then
                                arc = New dxeArc(bCS.Strukture, rad, 0, 360, aDisplaySettings:=dsp)

                                insts = arc.Instances
                                insts.Owner = Nothing

                            Else
                                Dim displacement As dxfVector = bCS.Origin - arc.Center
                                Dim scaler As Double = IIf(bRadPassed, 1, rad / arc.Radius)
                                insts.Add(displacement.X, displacement.Y, scaler)
                            End If

                        Else
                            arc = New dxeArc(bCS.Strukture, rad, 0, 360, aDisplaySettings:=dsp)
                            arc = myImage.SaveEntity(arc)
                            _rVal.Add(arc)
                        End If
                        'save to file


                    End If
                Next i
                If Not bSuppressInstances Then
                    arc = myImage.SaveEntity(arc)
                    _rVal.Add(arc)
                    arc.Instances = insts
                End If
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return _rVal
        End Function

        ''' <summary>
        ''' used to draw an ellipse to the display
        ''' </summary>
        ''' <param name="aCenter">the center point for the new ellipse</param>
        ''' <param name="aMajorDia">the width for the new ellipse (must be non zero)</param>
        ''' <param name="aMinorDia">the height for the new ellipse (must be non zero)</param>
        ''' <param name="aRotation">an angle to orient the major axis of the  new ellipse</param>
        ''' <param name="aStartAngle">the start angle of the ellipse (measured from the major axis)</param>
        ''' <param name="aEndAngle">the end angle of the ellipse (measured from the major axis)</param>
        ''' <param name="bClockwise">flag to sweep the ellipse in an clockwise direction, counter clockwise is default</param>
        ''' <param name="aDisplaySettings">the display seetings to apply</param>
        ''' <param name="aPlane">the working plane (the current ucs is used if null is passed)</param>
        ''' <returns></returns>
        Public Function aEllipse(aCenter As iVector, aMajorDia As Double, aMinorDia As Double, Optional aRotation As Single = 0.0, Optional aStartAngle As Single = 0.0, Optional aEndAngle As Single = 360.0, Optional bClockwise As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aPlane As dxfPlane = Nothing) As dxeEllipse
            Dim _rVal As dxeEllipse = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return _rVal
            Try

                aMajorDia = Math.Abs(aMajorDia)
                aMinorDia = Math.Abs(aMinorDia)
                If aMinorDia = 0 Or aMajorDia = 0 Then Throw New Exception("Invalid Diameter Detected")

                Dim aPl As New TPLANE("")
                aRotation = TVALUES.NormAng(aRotation)
                aStartAngle = TVALUES.NormAng(aStartAngle)
                aEndAngle = TVALUES.NormAng(aEndAngle)
                If aEndAngle = aStartAngle Then
                    aStartAngle = 0
                    aEndAngle = 360
                End If
                '    TVALUES.SortTwoValues Not bClockwise, aStartAngle, aEndAngle
                If TVALUES.SortTwoValues(True, aMinorDia, aMajorDia) Then
                    aRotation += 90
                End If
                Dim cp As dxfVector = myImage.CreateVector(aCenter, False, False, rPlane:=aPlane)

                If aRotation <> 0 Then aPlane.Rotate(aRotation)
                Dim dsp As dxfDisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Ellipse, aSettingsToCopy:=aDisplaySettings)

                _rVal = New dxeEllipse(cp, aMajorDia / 2, aMinorDia / 2, aStartAngle, aEndAngle, bClockwise, aPlane, dsp)
                'save the entity
                Return myImage.SaveEntity(_rVal)
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
                Return Nothing
            End Try
            Return Nothing
        End Function
        Public Function aEntities(aEntity As dxfEntity, aPointCol As IEnumerable(Of iVector), Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aXOffset As Double = 0.0, Optional aYOffset As Double = 0.0, Optional aGroupName As String = "", Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As colDXFEntities
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            Dim _rVal As New colDXFEntities
            '#1the entity to draw
            '#2the points to draw the entity at
            '#3a display settings to apply
            '#4a x offset to apply
            '#5a y offset to apply
            '#6a group name to apply
            '^draws a copy of the passed entity at all the passed locations
            Dim pCol As TVECTORS
            Dim aEnt As dxfEntity
            Dim bEnt As dxfEntity
            Dim aCS As dxfPlane = Nothing
            Dim v1 As TVECTOR
            Dim gWz As String = String.Empty
            Dim gname As String = IIf(Not String.IsNullOrWhiteSpace(aGroupName), aGroupName.Trim, "")
            Dim bpt As TVECTOR
            Dim dsp As TVECTOR
            Try
                If aEntity Is Nothing Then Throw New Exception("The Passed Entity Is Undefined")
                aEnt = aEntity.Clone
                If aEnt Is Nothing Then Throw New Exception("The Passed Entity Could Not be Cloned")

                If gname <> "" Then
                    gWz = myImage.GroupName
                    myImage.GroupName = gname
                End If
                pCol = myImage.CreateUCSVectors(aPointCol, bSuppressUCS, bSuppressElevation, aCS, True)
                aEnt.DisplaySettings = myImage.GetDisplaySettings(aEntity.EntityType, aSettingsToCopy:=aDisplaySettings)
                aEnt.PlaneV = New TPLANE(aCS)
                bpt = aEnt.DefinitionPoint(dxxEntDefPointTypes.HandlePt).Strukture
                For i As Integer = 1 To pCol.Count
                    v1 = pCol.Item(i)
                    bEnt = aEnt.Clone
                    dsp = v1 - bpt + New TVECTOR(aXOffset, aYOffset)
                    bEnt.Translate(dsp)
                    _rVal.Add(bEnt)
                Next i
                'save to file
                myImage.SaveEntity(, _rVal)
                If gname <> "" Then myImage.GroupName = gWz
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        Public Function aEntity(aDXFEntity As dxfEntity, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional bDontSave As Boolean = False) As dxfEntity
            If aDXFEntity Is Nothing Then Return Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            Try
                Dim bEnt As dxfEntity
                If Not bDontSave Then
                    bEnt = aDXFEntity.Clone
                Else
                    bEnt = aDXFEntity
                End If

                bEnt.DisplaySettings = myImage.GetDisplaySettings(aDXFEntity.EntityType, aSettingsToCopy:=aDisplaySettings)
                If Not bDontSave Then
                    Return myImage.SaveEntity(bEnt)
                Else
                    Return myImage.Entities.AddToCollection(bEnt, bSuppressEvnts:=True, bDontSave:=True)
                End If
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        Public Function aHatch(aBoundingEntity As dxfEntity, Optional sLineAngle As Double = 45, Optional aLineSpace As Double = 1, Optional aScaleFactor As Double = 0.0, Optional bDoubled As Boolean = False, Optional LastEntType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxeHatch

            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            '#1the entity to hatch
            '#2the angle of the hatch lines
            '#3the spacing between the hatch lines
            '#4a scale factor to apply to the hatch (0 means scale to current display)
            '#5flag indicating that the the hatch lines are also added 90 rotated (net pattern)
            '#6an entity type to use to get the last entity from the stack to hatch is not bound entity is passed
            '#7a layer to put the entity on instead of the current layer
            '#8a color to apply to the entity instead of the current color
            '#9a line type to apply to the entity instead of the current line type setting
            '^used to add a hatch to the last drawn entity or a passed entity.
            '~if the entity is passed as nothing an attempt is made to hatch the last draw entity.
            '~if the entity does not have a defineable hatch region nothing is drawn
            'error on invalid input
            Try
                Dim dsp As dxfDisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Hatch, aSettingsToCopy:=aDisplaySettings)
                'save the drawn entity
                Return myImage.SaveEntity(myImage.EntityTool.Create_Hatch_UserDefined(aBoundingEntity, sLineAngle, aLineSpace, aScaleFactor, bDoubled, dsp.LayerName, dsp.Color, dsp.Linetype, LastEntType))

            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        Public Function aHatch_ByFillStyle(aFillStyle As dxxFillStyles, Optional aBoundingEnt As dxfEntity = Nothing, Optional LastEntType As dxxEntityTypes = dxxEntityTypes.Undefined, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aScaleFactor As Double = 0.0, Optional aLineSpacing As Double = 0.0, Optional aRotation As Double = 0.0) As dxeHatch
            Dim _rVal As dxeHatch = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return _rVal
            '#1the hatch style to use (Solid,Line, Net...etc)
            '#2a entity to use for the hatch boundary (if nothing is passed an attempt is made to hatch the last draw entity)
            '#3the type of the last entity to search for
            '#4the layer to put the hatch on other than the current layer
            '#5a color to apply to the entity instead of the current color
            '^used to add a hatch to the last drawn entity or a passed entity.
            '~a screen hatch scales with the display and appears differently when the file is viewd in AutoCAD
            '~this provides for a much quicker hatch generation
            Try

                Dim dsp As dxfDisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Hatch, aSettingsToCopy:=aDisplaySettings)
                _rVal = myImage.EntityTool.Create_Hatch_ByFillStyle(aFillStyle, aBoundingEnt, aRotation, aLineSpacing, aScaleFactor, dsp.LayerName, dsp.Color, dsp.Linetype, LastEntType)
                'save the drawn entity
                myImage.SaveEntity(_rVal)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        Public Function aHole(aCenterXY As iVector, aHoleType As dxxEntityTypes, aDiameterOrHeight As Double, Optional aLength As Double = 0.0, Optional aRotation As Double = 0.0, Optional IsSquare As Boolean = False, Optional aMinorDia As Double = 0.0, Optional aPlane As dxxStandardPlanes = dxxStandardPlanes.XY, Optional CLineScaleFactorV As Double = 1, Optional CLineScaleFactorH As Double = 1,
                              Optional aExtrusionDirection As dxfDirection = Nothing, Optional aName As String = "", Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxeHole
            Dim _rVal As dxeHole = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return _rVal
            '#1the center of the new hole
            '#2the hole type to Draw
            '#3the diameter of the hole or height of a slot
            '#4the length for a slot
            '#5a angle for the hole
            '#6flag indicating if the hole is square
            '#7minor dia for D shaped holes
            '#8the plane the hole lies on
            '#8a scale factor for the holes vertical centerline
            '#9a scale factor for the holes horizontal centerline
            '#10the normal direction of the plane that the hole lies on (this overrides the plane argument)
            '#11the name to assign to the hole
            '#12a layer to put the entity on instead of the current layer
            '#13a color to apply to the entity instead of the current color
            '#14a linetype to apply to the entity instead of the current linetype setting
            '^used to draw holes and slots
            '~returns the drawn hole
            Try
                'create the entity
                Dim dsp As dxfDisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Hole, aSettingsToCopy:=aDisplaySettings)
                _rVal = myImage.EntityTool.Create_Hole(aCenterXY, aHoleType, aDiameterOrHeight, aLength, aRotation, IsSquare, aMinorDia, aPlane, CLineScaleFactorV, CLineScaleFactorH, aExtrusionDirection, aName, dsp.LayerName, dsp.Color, dsp.Linetype)
                'save the entity
                myImage.SaveEntity(_rVal)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        Public Function aHole2D(aHole As dxeHole, Optional aPlane As dxfPlane = Nothing, Optional aHorClineScale As Double = 0.0, Optional aVertClineScale As Double = 0.0, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As colDXFEntities
            Dim _rVal As colDXFEntities = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return _rVal
            '#1the hole to draw
            '#2a layer to put the entity on instead of the current layer
            '#3a color to apply to the entity instead of the current color
            '#4a linetype to apply to the entity instead of the current linetype setting
            '^used to draw holes and slots
            '~returns all the draw entities
            _rVal = New colDXFEntities
            Try
                If aHole Is Nothing Then Return _rVal
                Dim aPl As TPLANE
                Dim aBnd As dxfEntity
                Dim tDisp As New TDISPLAYVARS
                Dim aExtPts As New TVECTORS
                Dim bHole As dxeHole
                Dim aCl As dxeLine
                Dim dsp As dxfDisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Hole, aSettingsToCopy:=aDisplaySettings)
                Dim cp As TVECTOR = TVECTOR.Zero
                bHole = aHole.Clone
                Dim lng As Double
                Dim aRect As dxfRectangle = Nothing
                Dim bFlag As Boolean
                If dxfPlane.IsNull(aPlane) Then aPl = myImage.obj_UCS Else aPl = New TPLANE(aPlane)
                bHole.CenterV = bHole.CenterV.ProjectedTo(aPl)
                'the hole is planar to the passed plane
                If aPl.ZDirection.Equals(bHole.PlaneV.ZDirection, True, 3) Then
                    tDisp = dxfImageTool.DisplayStructure(myImage, dsp.LayerName, dsp.Color, dsp.Linetype, dxxLinetypeLayerFlag.ForceToColor)
                    aBnd = bHole.BoundingEntity(tDisp.LayerName, tDisp.Color, tDisp.Linetype)
                    aBnd.UpdatePath(False, myImage)
                    aExtPts = aBnd.ExtentPts(True)
                    aBnd.Flag = "BOUNDARY"
                    aBnd.UpdatePath()
                    _rVal.Add(aBnd)
                    cp = bHole.CenterV
                    If aHorClineScale > 0 Then
                        bFlag = True
                        aRect = aPl.MovedTo(cp).ToRectangle
                        aRect.ExpandToVectorsV(aExtPts)
                        tDisp = dxfImageTool.DisplayStructure(myImage, dsp.LayerName, dsp.Color, dxfLinetypes.Center, aLTLFlag:=dxxLinetypeLayerFlag.Undefined)
                        lng = aRect.Width * aHorClineScale
                        aCl = New dxeLine With {
                            .DisplayStructure = tDisp,
                            .Tag = bHole.Tag,
                            .Flag = "HORIZONTAL CENTERLINE",
                            .StartPtV = cp + aPl.XDirection * (-0.5 * lng),
                            .EndPtV = cp + aPl.XDirection * (0.5 * lng)
                        }
                        _rVal.Add(aCl)
                    End If
                    If aVertClineScale > 0 Then
                        If Not bFlag Then
                            aRect = aPl.MovedTo(cp).ToRectangle
                            aRect.ExpandToVectorsV(aExtPts)
                            tDisp = dxfImageTool.DisplayStructure(myImage, dsp.LayerName, dsp.Color, dxfLinetypes.Center, aLTLFlag:=dxxLinetypeLayerFlag.Undefined)
                        End If
                        lng = aRect.Height * aHorClineScale
                        aCl = New dxeLine With {
                            .DisplayStructure = tDisp,
                            .Tag = bHole.Tag,
                            .Flag = "VERTICAL CENTERLINE",
                            .StartPtV = cp + aPl.YDirection * (-0.5 * lng),
                            .EndPtV = cp + aPl.YDirection * (0.5 * lng)
                        }
                        _rVal.Add(aCl)
                    End If
                Else
                End If
                myImage.SaveEntity(, _rVal)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        Public Function aHoleCenterLines(aHoleOrHoles As Object, Optional aScaleFactor As Double = 1.2, Optional bSupressHorizontal As Boolean = False, Optional bSupressVertical As Boolean = False, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aPlane As dxfPlane = Nothing, Optional aCollector As colDXFEntities = Nothing, Optional rHorizontals As colDXFEntities = Nothing, Optional rVerticals As colDXFEntities = Nothing) As colDXFEntities
            Dim _rVal As colDXFEntities = Nothing
            If aCollector Is Nothing Then _rVal = New colDXFEntities Else _rVal = aCollector
            rHorizontals = New colDXFEntities
            rVerticals = New colDXFEntities
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return _rVal
            '#1the hole or holes to draw centerlines for
            '#2the scale factor to apply to the centerlines
            '#3flag to suppress the creation of the horizontal centerlines
            '#4flag to suppress the creation of the vertical centerlines
            '#5the linetype to apply
            '#6the plane to use (the current drawing plane is used by default)
            '^draws centerlines for the passed holes on the indicated plane
            Try
                If aHoleOrHoles Is Nothing Then Return _rVal
                Dim tname As String = String.Empty
                tname = TypeName(aHoleOrHoles)
                Dim aHoles As colDXFEntities
                If String.Compare(tname, "dxeHole", True) = 0 Then
                    aHoles = New colDXFEntities From {
                        aHoleOrHoles
                    }
                    _rVal = myImage.EntityTool.Create_HoleCenterLines(aHoles, aScaleFactor, bSupressHorizontal, bSupressVertical, aLayerName, aColor, aLineType, aPlane, _rVal)
                ElseIf String.Compare(tname, "colDXFEntities", True) = 0 Then
                    aHoles = aHoleOrHoles
                    _rVal = myImage.EntityTool.Create_HoleCenterLines(aHoles, aScaleFactor, bSupressHorizontal, bSupressVertical, aLayerName, aColor, aLineType, aPlane, _rVal)
                End If
                myImage.SaveEntity(Nothing, _rVal)
                If rHorizontals IsNot Nothing Then rHorizontals.Append(_rVal.GetByFlag("Horizontal", bReturnClones:=True))
                If rVerticals IsNot Nothing Then rVerticals.Append(_rVal.GetByFlag("Vertical", bReturnClones:=True))
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        Public Function aHoleCollection(aHoles As colDXFEntities, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aCenterLineScale As Double = 0.0) As colDXFEntities
            Dim rCenterLines As colDXFEntities = Nothing
            Return aHoleCollection(aHoles, rCenterLines, aLayer, aColor, aLineType, aCenterLineScale)
        End Function
        Public Function aHoleCollection(aHoles As colDXFEntities, ByRef rCenterLines As colDXFEntities, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aCenterLineScale As Double = 0.0) As colDXFEntities
            Dim _rVal As colDXFEntities = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return _rVal
            '#1the holes to Draw
            '#2a layer to put the entity on instead of the current layer
            '#3a color to apply to the entity instead of the current color
            '#4a linetype to apply to the entity instead of the current linetype setting
            '#5a scale factor for the holes vertical centerlines
            '#6returns the drawn centerlines
            '^used to draw holes and slots
            '~returns all the draw holes
            Try
                _rVal = New colDXFEntities
                If aCenterLineScale > 0 Then
                    If rCenterLines Is Nothing Then rCenterLines = New colDXFEntities
                End If
                If aHoles Is Nothing Then Return _rVal
                If aHoles.Count <= 0 Then Return _rVal
                Dim pHoles As List(Of dxfEntity) = aHoles.GetByGraphicType(dxxGraphicTypes.Hole).CollectionObj
                Dim bHole As dxeHole
                Dim hLines As colDXFEntities
                Dim hlCol As New colDXFEntities
                For i As Integer = 1 To pHoles.Count
                    bHole = aHoleObj(pHoles.Item(i), aLayer, aColor, aLineType)
                    _rVal.Add(bHole)
                    If aCenterLineScale > 0 Then
                        hlCol.Add(bHole)
                        hLines = myImage.EntityTool.Create_HoleCenterLines(hlCol, aCenterLineScale)
                        rCenterLines.Append(hLines, False)
                        hlCol.Clear()
                    End If
                Next i
                If aCenterLineScale > 0 Then myImage.SaveEntity(Nothing, rCenterLines)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        Public Function aHoleObj(pHole As dxfEntity, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As Object = Nothing) As dxeHole
            Dim _rVal As dxeHole = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return _rVal
            '#1the hole to draw
            '#2a layer to put the entity on instead of the current layer
            '#3a color to apply to the entity instead of the current color
            '#4a linetype to apply to the entity instead of the current linetype setting
            '^used to draw holes and slots
            '~returns all the draw entities
            Try
                If pHole Is Nothing Then Return _rVal
                If pHole.GraphicType <> dxxGraphicTypes.Hole Then Return _rVal
                Dim bHole As dxeHole
                bHole = pHole.Clone
                '
                '   bHole.Center.Add myImage.UCS.Origin
                If aLayer = "" Then aLayer = bHole.LayerName
                aLayer = myImage.GetOrAdd(dxxReferenceTypes.LAYER, aLayer)
                If aColor = dxxColors.Undefined Then aColor = bHole.Color
                If aLineType = "" Then aLineType = bHole.Linetype
                bHole.DisplayStructure = dxfImageTool.DisplayStructure(myImage, aLayer, aColor, aLineType, aLTLFlag:=dxxLinetypeLayerFlag.Undefined)
                myImage.SaveEntity(bHole)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        ''' <summary>
        ''' used to insert defined blocks into the current drawing.
        ''' </summary>
        ''' <remarks>the inserted block must have been created through code and added to the dxfImage.Blocks collection prior to insertion.</remarks>
        ''' <param name="aBlockName">the name of the block to insert into the drawing</param>
        ''' <param name="aInsertPT">the point or points to insert the block</param>
        ''' <param name="aRotationAngle">the angle to insert the block</param>
        ''' <param name="aScaleFactor">the scale factor to apply</param>
        ''' <param name="aDisplaySettings">The display seetings to apply to the new entity</param>
        ''' <param name="aYScale">the Y scale to apply (if not passed then primary scale factor is assumed)</param>
        ''' <param name="aZScale">the Z scale to apply (if not passed then primary scale factor is assumed)</param>
        ''' <param name="aAttributeVals">Attributes to assign to the insterts attributes (if applicable)</param>
        ''' <returns></returns>
        Public Function aInsert(aBlockName As String, aInsertPT As iVector, Optional aRotationAngle As Double? = Nothing, Optional aScaleFactor As Double = 1, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aYScale As Double? = Nothing, Optional aZScale As Double? = Nothing, Optional aAttributeVals As dxfAttributes = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aTag As String = "", Optional aFlag As String = "") As dxeInsert
            Dim _rVal As dxeInsert = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing

            aInsertPT = dxfVector.FromIVector(aInsertPT)
            Try
                _rVal = myImage.EntityTool.Create_Insert(aBlockName, aInsertPT, aRotationAngle, aScaleFactor, aDisplaySettings, aYScale, aZScale, aAttributeVals, myImage, True, bSuppressUCS:=bSuppressUCS, bSuppressElevation:=bSuppressElevation, aTag:=aTag, aFlag:=aFlag)
                _rVal = myImage.SaveEntity(_rVal)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function


        ''' <summary>
        ''' used to insert defined blocks into the current drawing.
        ''' </summary>
        ''' <remarks>the inserted block must have been created through code and added to the dxfImage.Blocks collection prior to insertion.</remarks>
        ''' <param name="aBlockName">the name of the block to insert into the drawing</param>
        ''' <param name="aInsertPTs"> a vectors collection to insert the block into the image </param>
        ''' <param name="aDisplaySettings">the display settings to apply to the new insert</param>
        ''' <param name="aRotationAngle">the rotation to apply to the new insert</param>
        ''' <param name="aScaleFactor">the scale factor to assign to the new insert</param>
        ''' <param name="aAttributeVals">Attributes to assign to the insterts attributes (if applicable)</param>
        ''' <param name="aYScale">the Y scale to apply (if not passed then primary scale factor is assumed)</param>
        ''' <param name="aZScale">the Z scale to apply (if not passed then primary scale factor is assumed)</param>
        ''' <param name="aTag">a tag to assign to the new entity</param>
        ''' <param name="bUseInsertionPtRotations">flag to use the rotations of the passed insertion points PLUS the passed rotation angle as the rotation assigned to the new insert</param>
        ''' <returns></returns>

        Public Function aInserts(aBlockName As String, aInsertPTs As IEnumerable(Of iVector), Optional aRotationAngle As Double? = Nothing, Optional aScaleFactor As Double = 1, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aLTLSetting As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined, Optional aAttributeVals As dxfAttributes = Nothing, Optional aYScale As Double? = Nothing, Optional aZScale As Double? = Nothing, Optional bUseInsertionPtRotations As Boolean? = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aTag As String = "", Optional aFlag As String = "") As List(Of dxeInsert)
            Dim _rVal As New List(Of dxeInsert)
            Dim myImage As dxfImage = Nothing
            If aInsertPTs Is Nothing Or Not GetImage(myImage) Then Return _rVal
            Try
                Dim lname As String = String.Empty
                Dim clr As dxxColors = dxxColors.Undefined
                Dim ltyp As String = String.Empty
                If aDisplaySettings IsNot Nothing Then
                    lname = aDisplaySettings.LayerName
                    clr = aDisplaySettings.Color
                    ltyp = aDisplaySettings.Linetype
                End If

                Dim inserts As List(Of dxeInsert) = myImage.EntityTool.Create_Inserts(aBlockName, aInsertPTs, aRotationAngle, aScaleFactor, lname, aAttributeVals, clr, ltyp, aYScale, aZScale, myImage, True, bUseInsertionPtRotations, aLTLSetting, bSuppressUCS:=bSuppressUCS, bSuppressElevation:=bSuppressElevation, aTag:=aTag, aFlag:=aFlag)
                For Each isert As dxeInsert In inserts
                    _rVal.Add(myImage.SaveEntity(isert))
                Next
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        Public Function aInsert(aBlock As dxfBlock, aInsertPt As iVector, Optional bOverrideExisting As Boolean = False, Optional aRotationAngle As Double = 0, Optional aScaleFactor As Double = 1, Optional bIncludesSubEntityInstances As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aLTLSetting As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined, Optional aAttributeVals As dxfAttributes = Nothing, Optional aYScale As Double? = Nothing, Optional aZScale As Double? = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional bSuppressInstances As Boolean = False) As dxeInsert


            Dim myImage As dxfImage = Nothing
            If aBlock Is Nothing Or Not GetImage(myImage) Then Return Nothing
            If aBlock.Entities.Count = 0 Then Return Nothing

            Try

                Dim existing As dxfBlock = Nothing
                If Not myImage.Blocks.TryGet(aBlock.Name, existing) Then
                    myImage.LinetypeLayers.ApplyTo(aBlock, aLTLSetting, myImage)
                    aBlock = myImage.Blocks.Add(aBlock)

                Else
                    If bOverrideExisting Then
                        myImage.LinetypeLayers.ApplyTo(aBlock, aLTLSetting, myImage)
                        aBlock = myImage.Blocks.Add(aBlock, bOverrideExisting:=True)
                    Else
                        aBlock = existing
                    End If

                End If

                If aBlock Is Nothing Then Return Nothing

                Return Me.aInsert(aBlock.Name, aInsertPt, aRotationAngle, aScaleFactor, aDisplaySettings, aYScale, aZScale, aAttributeVals, bSuppressUCS, bSuppressElevation)

            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing

        End Function


        Public Function aInserts(aBlock As dxfBlock, aInstances As dxoInstances, bOverrideExisting As Boolean, Optional aRotationAngle As Double = 0, Optional aScaleFactor As Double = 1, Optional bIncludesSubEntityInstances As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aLTLSetting As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined, Optional aAttributeVals As dxfAttributes = Nothing, Optional aYScale As Double? = Nothing, Optional aZScale As Double? = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional bSuppressInstances As Boolean = False, Optional aTag As String = "", Optional aFlag As String = "") As List(Of dxeInsert)

            Dim _rVal As New List(Of dxeInsert)
            Dim myImage As dxfImage = Nothing
            If aBlock Is Nothing Or Not GetImage(myImage) Then Return _rVal
            If aBlock.Entities.Count = 0 Then Return _rVal

            Try
                aBlock = myImage.Blocks.Add(aBlock, bOverrideExisting)
                If aBlock Is Nothing Then Return _rVal
                If aInstances Is Nothing Then aInstances = New dxoInstances(aBlock.Instances) Else aInstances = New dxoInstances(aInstances)

                Dim plane As dxfPlane = aInstances.BasePlane
                If Not bSuppressUCS Then
                    plane.AlignTo(myImage.UCS)
                    plane.Origin = myImage.CreateVector(plane.Origin, bSuppressElevation:=bSuppressElevation)
                End If
                If aScaleFactor = 0 Then aScaleFactor = 1
                If aYScale.HasValue Then
                    If aYScale.Value = 0 Then aYScale = aScaleFactor
                Else
                    aYScale = aScaleFactor
                End If
                If aZScale.HasValue Then
                    If aZScale.Value = 0 Then aZScale = aScaleFactor
                Else
                    aZScale = aScaleFactor
                End If

                aDisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Insert, aLTLFlag:=aLTLSetting, aSettingsToCopy:=aDisplaySettings)

                Dim isert As New dxeInsert(aBlock, plane.Origin, aScaleFactor) With
                {
                    .YScaleFactor = aYScale.Value,
                    .ZScaleFactor = aZScale.Value,
                    .RotationAngle = aRotationAngle,
                .DisplaySettings = aDisplaySettings,
                .PlaneV = New TPLANE(plane),
                .Tag = aTag,
                .Flag = aFlag
                }

                If aAttributeVals IsNot Nothing Then
                    isert.Attributes.CopyValues(aAttributeVals)
                End If


                If Not bSuppressInstances Or aInstances.Count = 0 Then
                    'just create one insert with no instances assigned to the new insert
                    aInstances.BasePlane = isert.Plane
                    isert.Instances = aInstances

                    isert = myImage.SaveEntity(isert, aTag:=aTag)
                    _rVal.Add(isert)
                Else
                    'create one insert with instances assigned to the new insert based on the passed instances or the blocks defined instances
                    isert = myImage.SaveEntity(isert, aTag:=aTag)

                    _rVal.Add(isert)

                    Dim ip1 As dxfVector = plane.Origin
                    For i As Integer = 1 To aInstances.Count
                        Dim instance As dxoInstance = aInstances.Item(i)

                        Dim deltaYscale As Double = 1
                        If instance.Inverted Then deltaYscale = -1
                        Dim deltaXscale As Double = 1
                        If instance.LeftHanded Then deltaXscale = -1
                        If instance.ScaleFactor <> 0 Then deltaYscale *= instance.ScaleFactor
                        If instance.ScaleFactor <> 0 Then deltaXscale *= instance.ScaleFactor

                        Dim ip2 As dxfVector = plane.Vector(instance.XOffset, instance.YOffset)
                        Dim isert2 As New dxeInsert(aBlock, ip2, aScaleFactor * deltaXscale) With
                {
                    .YScaleFactor = aYScale.Value * deltaYscale,
                    .ZScaleFactor = aZScale.Value,
                    .RotationAngle = aRotationAngle + instance.Rotation,
                .DisplaySettings = aDisplaySettings}

                        If aAttributeVals IsNot Nothing Then
                            isert2.Attributes.CopyValues(aAttributeVals)
                        End If

                        isert2 = myImage.SaveEntity(isert2)
                        _rVal.Add(isert2)
                    Next

                End If




            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return _rVal

        End Function
        ''' <summary>
        ''' used to insert a new block into the drawing created from the passed polygon.
        ''' </summary>
        ''' <remarks>If the polygon is undefined nothing is added and no errors are thrown. If the requested block name already exists in the drawing blocks then the existing block is used and the polygon block is discarded.</remarks>
        ''' <param name="aPolygon"> polygon to convert to a block and insert as a block</param>
        ''' <param name="aInsertPTs"> a vectors collection to insert the block into the image </param>
        ''' <param name="aRotationAngle">the rotation to apply to the new insert</param>
        ''' <param name="aBlockName">the block name to assign to the created block</param>
        ''' <param name="aScaleFactor">the scale factor to assign to the new insert</param>
        ''' <param name="bIncludesSubEntityInstances">flag to include the entities created by applying the polygons instances to the basic block in the new block</param>
        ''' <param name="aDisplaySettings">the display settings to apply to the new insert</param>
        ''' <param name="aLTLSetting">an override linetype layer setting to apply to the new block entities</param>
        ''' <param name="aYScale">the Y scale to apply (if not passed then primary scale factor is assumed)</param>
        ''' <param name="aZScale">the Z scale to apply (if not passed then primary scale factor is assumed)</param>
        ''' <param name="aTag">a tag to assign to the new entity</param>
        ''' <param name="bUseInsertionPtRotations">flag to use the rotations of the passed insertion points PLUS the passed rotation angle as the rotation assigned to the new insert</param>
        '''  ''' <param name="bSuppressInstances">flag to create multiple inserts rather than a single insert with it's instances defined</param>
        ''' <returns></returns>
        Public Function aInserts(aPolygon As dxePolygon, aInsertPTs As IEnumerable(Of iVector), Optional aRotationAngle As Double? = Nothing, Optional aBlockName As String = Nothing, Optional aScaleFactor As Double = 1, Optional bIncludesSubEntityInstances As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aLTLSetting As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined, Optional aYScale As Double? = Nothing, Optional aZScale As Double? = Nothing, Optional aTag As String = Nothing, Optional bUseInsertionPtRotations As Boolean? = Nothing, Optional bSuppressInstances As Boolean = False) As List(Of dxeInsert)
            Dim _rVal As New List(Of dxeInsert)
            Dim myImage As dxfImage = Nothing
            If aPolygon Is Nothing Or Not GetImage(myImage) Then Return Nothing
            If aInsertPTs Is Nothing Then
                Dim ipts As New List(Of dxfVector)
                ipts.Add(New dxfVector(aPolygon.InsertionPt))
                aInsertPTs = ipts
            End If

            Try
                Dim inserts As List(Of dxeInsert) = myImage.EntityTool.Create_Inserts(aPolygon, aInsertPTs, aRotationAngle, aBlockName, aScaleFactor, bIncludesSubEntityInstances, aDisplaySettings, aLTLSetting, aYScale, aZScale, myImage, True, bSuppressInstances)
                For Each isert As dxeInsert In inserts
                    _rVal.Add(myImage.SaveEntity(isert))
                Next
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return _rVal
        End Function

        ''' <summary>
        ''' used to insert a new block into the drawing created from the passed polygon.
        ''' </summary>
        ''' <remarks>If the polygon is undefined nothing is added and no errors are thrown. If the requested block name already exists in the drawing blocks then the existing block is used and the polygon block is discarded.</remarks>
        ''' <param name="aPolygon"> polygon to convert to a block and insert as a block</param>
        ''' <param name="aInsertPT"></param>
        ''' <param name="aRotationAngle">the rotation to apply to the new insert</param>
        ''' <param name="aBlockName">the block name to assign to the created block</param>
        ''' <param name="aScaleFactor">the scale factor to assign to the new insert</param>
        ''' <param name="bIncludesSubEntityInstances">flag to include the entities created by applying the polygons instances to the basic block in the new block</param>
        ''' <param name="aDisplaySettings">the display settings to apply to the new insert</param>
        ''' <param name="aLTLSetting">an override linetype layer setting to apply to the new block entities</param>
        ''' <param name="aYScale">the Y scale to apply (if not passed then primary scale factor is assumed)</param>
        ''' <param name="aZScale">the Z scale to apply (if not passed then primary scale factor is assumed)</param>
        ''' <returns></returns>
        Public Function aInsert(aPolygon As dxePolygon, aInsertPT As iVector, Optional aRotationAngle As Double? = Nothing, Optional aBlockName As String = Nothing, Optional aScaleFactor As Double = 1, Optional bIncludesSubEntityInstances As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aLTLSetting As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined, Optional aYScale As Double? = Nothing, Optional aZScale As Double? = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeInsert

            Dim myImage As dxfImage = Nothing
            If aPolygon Is Nothing Or Not GetImage(myImage) Then Return Nothing
            Try
                Return myImage.SaveEntity(myImage.EntityTool.Create_Insert(aPolygon, aInsertPT, aRotationAngle, aBlockName, aScaleFactor, bIncludesSubEntityInstances, aDisplaySettings, aLTLSetting, aYScale, aZScale, myImage, True, bSuppressUCS, bSuppressElevation))

            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function


        ''' <summary>
        ''' used to draw a line to the display
        ''' </summary>
        ''' <remarks>does nothing if the passed arguments are invalid (i.e. undefined points)</remarks>
        ''' <param name="aStartPointXY">the start point of the new line</param>
        ''' <param name="aEndPointXY">the end point of the new line</param>
        ''' <param name="aDisplaySettings">a display settings tooverride the current settings</param>
        ''' <param name="aInstances">a instances to assign to the new entity</param>
        ''' <param name="bSuppressUCS">flag to suppress conversion of points to the current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress the current elevation when converting points to the current ucs</param>
        ''' <param name="aLTLFlag">an override LinetypeLayer setting to apply </param>
        ''' <returns></returns>

        Public Function aLine(aStartPointXY As iVector, aEndPointXY As iVector, aDisplaySettings As dxfDisplaySettings, Optional aInstances As dxoInstances = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As dxeLine
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            If aDisplaySettings Is Nothing Then aDisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Line, "", dxxColors.Undefined, "", aLTLFlag, False, "")
            Dim _rVal As dxeLine = Nothing
            Try

                Dim sp As dxfVector = myImage.CreateVector(aStartPointXY, bSuppressUCS, bSuppressElevation)
                Dim ep As dxfVector = myImage.CreateVector(aEndPointXY, bSuppressUCS, bSuppressElevation)
                If dxfProjections.DistanceTo(sp, ep) > 0 Then
                    'save to file
                    _rVal = New dxeLine(sp, ep, aDisplaySettings) With {.Instances = aInstances}
                    Return myImage.SaveEntity(_rVal, Nothing)
                Else
                    Throw New Exception("Zero Length Line Requested")
                End If
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return _rVal
        End Function

        '''<summary>
        ''' used to draw a line to the display
        ''' </summary>
        ''' <remarks>does nothing if the passed arguments are invalid (i.e. undefined points)</remarks>
        ''' <param name="aLineObj">the line to draw</param>
        ''' <param name="aLayer">a layer to put the entity on instead of the current layer</param>
        ''' <param name="aColor">a color to apply to the entity instead of the current color</param>
        ''' <param name="aLineType">a linetype to apply to the entity instead of the current linetype setting</param>
        ''' <param name="aInstances">new instances to assign to the new line</param>
        ''' <param name="bSuppressUCS">flag to suppress conversion of points to the current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress the current elevation when converting points to the current ucs</param>
        ''' <param name="aLTLFlag">an override LinetypeLayer setting to apply </param>
        ''' <returns></returns>
        ''' 
        Public Function aLine(aLineObj As iLine, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aInstances As dxoInstances = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As dxeLine
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Or aLineObj Is Nothing Then Return Nothing
            '#1the line to draw

            Try
                Dim aDisplaySettings As dxfDisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Line, aLayer, aColor, aLineType, aLTLFlag, False, "")
                Return aLine(aLineObj.StartPt, aLineObj.EndPt, aDisplaySettings:=aDisplaySettings, aInstances:=aInstances, bSuppressUCS:=bSuppressUCS, bSuppressElevation:=bSuppressElevation, aLTLFlag:=aLTLFlag)

            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        ''' <summary>
        ''' used to draw a line to the display
        ''' </summary>
        ''' <remarks>does nothing if the passed arguments are invalid (i.e. undefined points)</remarks>
        ''' <param name="aLineObj">the line to draw</param>
        ''' <param name="aDisplaySettings">the display settings to apply</param>
        ''' <param name="aInstances">new instances to assign to the new line</param>
        ''' <param name="bSuppressUCS">flag to suppress conversion of points to the current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress the current elevation when converting points to the current ucs</param>
        ''' <param name="aLTLFlag">an override LinetypeLayer setting to apply </param>
        ''' <returns></returns>
        Public Function aLine(aLineObj As iLine, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aInstances As dxoInstances = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As dxeLine
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Or aLineObj Is Nothing Then Return Nothing

            'set the line properties
            Try
                Dim dsp As dxfDisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Line, aSettingsToCopy:=aDisplaySettings)
                Dim dxline As dxeLine = Nothing
                If TypeOf (aLineObj) Is dxeLine Then
                    dxline = DirectCast(aLineObj, dxeLine)
                    dxline = dxline.Clone(bCloneInstances:=True)
                    dxline.StartPt = myImage.CreateVector(dxline.StartPt, bSuppressUCS, bSuppressElevation)
                    dxline.EndPt = myImage.CreateVector(dxline.EndPt, bSuppressUCS, bSuppressElevation)
                    dxline.DisplaySettings = dsp

                Else
                    dxline = New dxeLine(aLineObj, aDisplaySettings:=dsp)
                    dxline.StartPt = myImage.CreateVector(dxline.StartPt, bSuppressUCS, bSuppressElevation)
                    dxline.EndPt = myImage.CreateVector(dxline.EndPt, bSuppressUCS, bSuppressElevation)

                End If
                If dxline Is Nothing Then Return Nothing
                If dxline.Length = 0 Then Return Nothing
                If (aInstances IsNot Nothing) Then dxline.Instances = aInstances

                Return myImage.SaveEntity(dxline)
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        ''' <summary>
        ''' used to draw a lines to the display
        ''' </summary>
        ''' <remarks>does nothing if the passed arguments are invalid (i.e. undefined points)</remarks>
        ''' <param name="aLinesList">the line to draw</param>
        ''' <param name="aDisplaySettings">the display settings to apply</param>
        ''' <param name="aInstances">new instances to assign to the new line</param>
        ''' <param name="bSuppressUCS">flag to suppress conversion of points to the current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress the current elevation when converting points to the current ucs</param>
        ''' <param name="aLTLFlag">an override LinetypeLayer setting to apply </param>
        ''' <returns></returns>
        Public Function aLines(aLinesList As IEnumerable(Of iLine), Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aInstances As dxoInstances = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As List(Of dxeLine)
            Dim _rVal As New List(Of dxeLine)()

            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Or aLinesList Is Nothing Then Return _rVal

            'set the line properties
            Try
                Dim dsp As dxfDisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Line, aSettingsToCopy:=aDisplaySettings)
                For Each line As iLine In aLinesList
                    Dim dxline As dxeLine = aLine(line, dsp, aInstances, bSuppressUCS, bSuppressElevation, aLTLFlag)
                    If dxline IsNot Nothing Then _rVal.Add(dxline)
                Next


            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return _rVal
        End Function

        ''' <summary>
        ''' used to draw a line to the display
        ''' </summary>
        ''' <remarks>does nothing if the passed arguments are invalid (i.e. undefined points)</remarks>
        ''' <param name="aStartPointXY">the start point of the new line</param>
        ''' <param name="aEndPointXY">the end point of the new line</param>
        ''' <param name="aLayer">a layer to put the entity on instead of the current layer</param>
        ''' <param name="aColor">a color to apply to the entity instead of the current color</param>
        ''' <param name="aLineType">a linetype to apply to the entity instead of the current linetype setting</param>
        ''' <param name="aInstances">new instances to assign to the new line</param>
        ''' <param name="bSuppressUCS">flag to suppress conversion of points to the current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress the current elevation when converting points to the current ucs</param>
        ''' <param name="aLTLFlag">an override LinetypeLayer setting to apply </param>
        ''' <returns></returns>
        Public Function aLine(aStartPointXY As iVector, aEndPointXY As iVector, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aInstances As dxoInstances = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As dxeLine
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            Dim aDisplaySettings As dxfDisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Line, aLayer, aColor, aLineType, aLTLFlag, False, "")
            Try
                Return aLine(aStartPointXY, aEndPointXY, aDisplaySettings:=aDisplaySettings, aInstances:=aInstances, bSuppressUCS:=bSuppressUCS, bSuppressElevation:=bSuppressElevation)
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        ''' <summary>
        ''' used to draw a line to the display using a start point, aDirection and a length
        ''' </summary>
        ''' <remarks>does nothing if the passed arguments are invalid (i.e. undefined points)</remarks>
        ''' <param name="aStartPointXY">the start point of the new line</param>
        ''' <param name="aDirection">the direction to project the line</param>
        ''' <param name="aDistance">the length to make the line. must be non-zero, negative numbers reverse the projection</param>
        ''' <param name="aLayer">a layer to put the entity on instead of the current layer</param>
        ''' <param name="aColor">a color to apply to the entity instead of the current color</param>
        ''' <param name="aLineType">a linetype to apply to the entity instead of the current linetype setting</param>
        ''' <param name="aInstances">new instances to assign to the new line</param>
        ''' <param name="bSuppressUCS">flag to suppress conversion of points to the current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress the current elevation when converting points to the current ucs</param>
        ''' <param name="aLTLFlag">an override LinetypeLayer setting to apply </param>
        Public Function aLine(aStartPointXY As iVector, aDirection As dxfDirection, aDistance As Double, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aInstances As dxoInstances = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As dxeLine
            Dim _rVal As dxeLine = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return _rVal
            If aDistance = 0 Then Return _rVal
            Dim dsp As dxfDisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Line, aLayer, aColor, aLineType, aLTLFlag, False, "")
            'set the line properties
            If aDirection Is Nothing Then aDirection = myImage.UCS.XDirection
            If aDirection.IsZero Then aDirection = myImage.UCS.XDirection
            Try
                Dim sp As dxfVector = myImage.CreateVector(aStartPointXY, bSuppressUCS, bSuppressElevation)
                Dim ep As dxfVector = myImage.CreateVector(sp.Projected(aDirection, aDistance), bSuppressUCS, bSuppressElevation)
                If dxfProjections.DistanceTo(sp, ep) > 0 Then
                    'save to file
                    Return myImage.SaveEntity(New dxeLine(sp, ep, dsp) With {.Instances = aInstances}, Nothing)
                Else
                    Throw New Exception("Zero Length Line Requested")
                End If
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        ''' <summary>
        ''' used to draw a line to the display using a start point, aDirection and a length
        ''' </summary>
        ''' <remarks>does nothing if the passed arguments are invalid (i.e. undefined points)</remarks>
        ''' <param name="aStartPointXY">the start point of the new line</param>
        ''' <param name="aDirection">the direction to project the line</param>
        ''' <param name="aDistance">the length to make the line. must be non-zero, negative numbers reverse the projection</param>
        ''' <param name="aDisplaySettings">the display settings to apply</param>
        ''' <param name="aInstances">new instances to assign to the new line</param>
        ''' <param name="bSuppressUCS">flag to suppress conversion of points to the current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress the current elevation when converting points to the current ucs</param>
        ''' <param name="aLTLFlag">an override LinetypeLayer setting to apply </param>
        Public Function aLine(aStartPointXY As iVector, aDirection As dxfDirection, aDistance As Double, aDisplaySettings As dxfDisplaySettings, Optional aInstances As dxoInstances = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As dxeLine
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            If aDistance = 0 Then Return Nothing
            If aDirection Is Nothing Then aDirection = myImage.UCS.XDirection
            If aDirection.IsZero Then aDirection = myImage.UCS.XDirection
            Dim dsp As dxfDisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Line, aSettingsToCopy:=aDisplaySettings, aLTLFlag:=aLTLFlag)

            Try
                Dim sp As dxfVector = myImage.CreateVector(aStartPointXY, bSuppressUCS, bSuppressElevation)
                Dim ep As dxfVector = myImage.CreateVector(sp.Projected(aDirection, aDistance), bSuppressUCS, bSuppressElevation)
                If dxfProjections.DistanceTo(sp, ep) > 0 Then
                    'save to file
                    Return myImage.SaveEntity(New dxeLine(sp, ep, dsp) With {.Instances = aInstances}, Nothing)
                Else
                    Throw New Exception("Zero Length Line Requested")
                End If
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        ''' <summary>
        ''' used to draw Lines to the display using a start, end or mid pt and a direction and a length
        ''' </summary>
        ''' <remarks> the created line has end points relative to the passed point using the passed rotation and length</remarks>
        ''' <param name="aSegmentPoint">'the point used to create the line relative to based on the indicated reference type</param>
        ''' <param name="aRotation">the rotation angle of the new line with respect to the passed system</param>
        ''' <param name="aLength">the length of the new line (negative values invert the end points)</param>
        ''' <param name="aSegmentPtType">the flag which control how the line is create relative to the passed point</param>
        ''' <param name="aLayer">a layer to put the entity on instead of the current layer</param>
        ''' <param name="aColor">a color to apply to the entity instead of the current color</param>
        ''' <param name="aLineType">a linetype to apply to the entity instead of the current linetype setting</param>
        ''' <param name="aCS">a coordinate system to use instead of the current UCS. Used for directions.</param>
        ''' <param name="bInRadians">flag indicating the passed angle is in radians</param>
        ''' <param name="aInstances">new instances to assign to the new line</param>
        ''' <param name="bSuppressUCS">flag to suppress conversion of points to the current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress the current elevation when converting points to the current ucs</param>
        ''' <param name="aLTLFlag">an override LinetypeLayer setting to apply </param>
        ''' <returns></returns>
        Public Function aLine(aSegmentPoint As iVector, aRotation As Double, aLength As Double, Optional aSegmentPtType As dxxSegmentPointTypes = dxxSegmentPointTypes.MidPt, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aCS As dxfPlane = Nothing, Optional bInRadians As Boolean = False, Optional aInstances As dxoInstances = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As dxeLine
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Or aLength = 0 Then Return Nothing
            Try
                Dim v1 As TVECTOR = myImage.CreateUCSVector(aSegmentPoint, bSuppressUCS, bSuppressElevation, aCS)

                Dim aPl As New TPLANE(aCS)
                Dim dsp As TDISPLAYVARS = dxfImageTool.DisplayStructure(myImage, aLayer, aColor, aLineType, aLTLFlag:=aLTLFlag)
                'set the line properties
                aPl.Origin = v1
                Dim _rVal As dxeLine = Nothing
                If aSegmentPtType = dxxSegmentPointTypes.StartPt Then
                    _rVal = New dxeLine(v1, aPl.AngleVector(v1, aRotation, aLength, bInRadians))
                ElseIf aSegmentPtType = dxxSegmentPointTypes.EndPt Then
                    _rVal = New dxeLine(aPl.AngleVector(v1, aRotation, aLength, bInRadians), v1)
                Else

                    _rVal = New dxeLine(aPl.AngleVector(v1, aRotation + 180, 0.5 * aLength, bInRadians), aPl.AngleVector(v1, aRotation, 0.5 * aLength, bInRadians))
                End If
                _rVal.PlaneV = aPl
                _rVal.DisplayStructure = dsp
                _rVal.Instances = aInstances
                'save to file
                _rVal = myImage.SaveEntity(_rVal)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function


        ''' <summary>
        ''' used to draw Lines to the display using a start, end or mid pt and a direction and a length
        ''' </summary>
        ''' <remarks> the created line has end points relative to the passed point using the passed rotation and length</remarks>
        ''' <param name="aSegmentPoint">'the point used to create the line relative to based on the indicated reference type</param>
        ''' <param name="aRotation">the rotation angle of the new line with respect to the passed system</param>
        ''' <param name="aLength">the length of the new line (negative values invert the end points)</param>
        ''' <param name="aSegmentPtType">the flag which control how the line is create relative to the passed point</param>
        ''' <param name="aDisplaySettings">the display settings to apply</param>
        ''' <param name="aCS">a coordinate system to use instead of the current UCS. Used for directions.</param>
        ''' <param name="bInRadians">flag indicating the passed angle is in radians</param>
        ''' <param name="aInstances">new instances to assign to the new line</param>
        ''' <param name="bSuppressUCS">flag to suppress conversion of points to the current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress the current elevation when converting points to the current ucs</param>
        ''' <param name="aLTLFlag">an override LinetypeLayer setting to apply </param>
        ''' <returns></returns>
        Public Function aLine(aSegmentPoint As iVector, aRotation As Double, aLength As Double, Optional aSegmentPtType As dxxSegmentPointTypes = dxxSegmentPointTypes.MidPt, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aCS As dxfPlane = Nothing, Optional bInRadians As Boolean = False, Optional aInstances As dxoInstances = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As dxeLine
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Or aLength = 0 Then Return Nothing
            Try
                Dim v1 As TVECTOR = myImage.CreateUCSVector(aSegmentPoint, bSuppressUCS, bSuppressElevation, aCS)

                Dim aPl As New TPLANE(aCS)
                Dim dsp As dxfDisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Line, aSettingsToCopy:=aDisplaySettings, aLTLFlag:=aLTLFlag)
                'set the line properties
                aPl.Origin = v1
                Dim _rVal As dxeLine = Nothing
                If aSegmentPtType = dxxSegmentPointTypes.StartPt Then
                    _rVal = New dxeLine(v1, aPl.AngleVector(v1, aRotation, aLength, bInRadians))
                ElseIf aSegmentPtType = dxxSegmentPointTypes.EndPt Then
                    _rVal = New dxeLine(aPl.AngleVector(v1, aRotation, aLength, bInRadians), v1)
                Else

                    _rVal = New dxeLine(aPl.AngleVector(v1, aRotation + 180, 0.5 * aLength, bInRadians), aPl.AngleVector(v1, aRotation, 0.5 * aLength, bInRadians))
                End If
                _rVal.PlaneV = aPl
                _rVal.DisplaySettings = dsp
                _rVal.Instances = aInstances
                'save to file
                _rVal = myImage.SaveEntity(_rVal)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        ''' <summary>
        ''' used to draw a line to the display usng the passed coorinates for the start and end points
        ''' </summary>
        ''' <remarks>does nothing if the passed arguments are invalid (i.e. a zero length line is defined)</remarks>
        ''' <param name="aX1">the X of the start point of the new line</param>
        ''' <param name="aY1">the Y of the start point of the new line</param>
        ''' <param name="aX2">the X of the end point of the new line</param>
        ''' <param name="aY2">the Y of the end point of the new line</param>
        ''' <param name="aLayer">a layer to put the entity on instead of the current layer</param>
        ''' <param name="aColor">a color to apply to the entity instead of the current color</param>
        ''' <param name="aLineType">a linetype to apply to the entity instead of the current linetype setting</param>
        ''' <param name="aPlane">the plane to use to define the new line. a null plane means to use the current image UCS</param>
        ''' <param name="aInstances">new instances to assign to the new line</param>
        ''' <param name="bSuppressUCS">flag to suppress conversion of points to the current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress the current elevation when converting points to the current ucs</param>
        ''' <param name="aLTLFlag">an override LinetypeLayer setting to apply </param>
        ''' <returns></returns>
        Public Function aLine(aX1 As Double, aY1 As Double, aX2 As Double, aY2 As Double, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aPlane As dxfPlane = Nothing, Optional aInstances As dxoInstances = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As dxeLine
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing

            Try


                Dim v1 As TVECTOR
                Dim v2 As TVECTOR
                If Not dxfPlane.IsNull(aPlane) Then
                    v1 = aPlane.VectorV(aX1, aY1)
                    v2 = aPlane.VectorV(aX2, aY2)
                Else
                    v1 = myImage.CreateUCSVector(New TVECTOR(aX1, aY1), bSuppressUCS, bSuppressElevation, aPlane)
                    v2 = myImage.CreateUCSVector(New TVECTOR(aX2, aY2), bSuppressUCS, bSuppressElevation, aPlane)
                End If

                If v1.DistanceTo(v2, 6) = 0 Then Return Nothing
                Dim dsp As TDISPLAYVARS = dxfImageTool.DisplayStructure(myImage, aLayer, aColor, aLineType, aLTLFlag:=aLTLFlag)
                'set the line properties
                Dim _rVal As New dxeLine(v1, v2) With {.PlaneV = New TPLANE(aPlane), .DisplayStructure = dsp, .Instances = aInstances}
                'save to file
                Return myImage.SaveEntity(_rVal)
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        ''' <summary>
        ''' used to draw a line to the display usng the passed coorinates for the start and end points
        ''' </summary>
        ''' <remarks>does nothing if the passed arguments are invalid (i.e. a zero length line is defined)</remarks>
        ''' <param name="aX1">the X of the start point of the new line</param>
        ''' <param name="aY1">the Y of the start point of the new line</param>
        ''' <param name="aX2">the X of the end point of the new line</param>
        ''' <param name="aY2">the Y of the end point of the new line</param>
        ''' <param name="aPlane">the plane to use to define the new line. a null plane meas to use the current image UCS</param>
        ''' <param name="aDisplaySettings">the display settings to apply</param>
        ''' <param name="aInstances">new instances to assign to the new line</param>
        ''' <param name="bSuppressUCS">flag to suppress conversion of points to the current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress the current elevation when converting points to the current ucs</param>
        ''' <param name="aLTLFlag">an override LinetypeLayer setting to apply </param>
        ''' <returns></returns>
        Public Function aLine(aX1 As Double, aY1 As Double, aX2 As Double, aY2 As Double, Optional aPlane As dxfPlane = Nothing, Optional bInRadians As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aInstances As dxoInstances = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As dxeLine
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing

            Try


                Dim v1 As TVECTOR
                Dim v2 As TVECTOR
                If Not dxfPlane.IsNull(aPlane) Then
                    v1 = aPlane.VectorV(aX1, aY1)
                    v2 = aPlane.VectorV(aX2, aY2)
                Else
                    v1 = myImage.CreateUCSVector(New TVECTOR(aX1, aY1), bSuppressUCS, bSuppressElevation, aPlane)
                    v2 = myImage.CreateUCSVector(New TVECTOR(aX2, aY2), bSuppressUCS, bSuppressElevation, aPlane)
                End If
                If v1.DistanceTo(v2, 6) = 0 Then Return Nothing
                Dim dsp As dxfDisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Line, aSettingsToCopy:=aDisplaySettings, aLTLFlag:=aLTLFlag)
                'set the line properties
                Dim _rVal As New dxeLine(v1, v2) With {.PlaneV = New TPLANE(aPlane), .DisplaySettings = dsp, .Instances = aInstances}
                'save to file
                Return myImage.SaveEntity(_rVal)
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function


        Public Function aLines(aVerticesXY As IEnumerable(Of iVector), Optional bClosed As Boolean = False, Optional bUsePtSettings As Boolean = False, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional bDisjoint As Boolean = False, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As List(Of dxeLine)
            Dim _rVal As New List(Of dxeLine)
            If aVerticesXY Is Nothing Then Return _rVal
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return _rVal
            '#1a collection of vectors that will be used as the line start and end pts
            '#2flag to indicate if the first and last vertex should be connected
            '#3flag to used the display settings of the passed points for the created lines
            '#4a layer to put the entity on instead of the current layer
            '#5a color to apply to the entity instead of the current color
            '#6a linetype to apply to the entity instead of the current linetype setting
            '#7if True the layer,color and linetype of the passed vectors is used as the settings for the lines starting at them
            '#8flag to either connect the points end to start or every two points as individual lines
            '^used to draw a collection of lines connecting the passsed points
            '~zero length lines are not save or drawn
            Try
                Dim aOCS As dxfPlane = Nothing
                Dim i As Integer
                Dim v1 As TVERTEX
                Dim v2 As TVERTEX
                Dim vPts As TVERTICES = myImage.CreateUCSVertices(aVerticesXY, bSuppressUCS, bSuppressElevation, aOCS, True)
                Dim l1 As dxeLine
                Dim eSets As TDISPLAYVARS = dxfImageTool.DisplayStructure(myImage, aLayer, aColor, aLineType, aLTLFlag:=dxxLinetypeLayerFlag.Undefined)
                Dim aPl As TPLANE = aOCS.Strukture
                'define the dxf entity properties
                If vPts.Count < 3 Then bClosed = False
                For i = 1 To vPts.Count
                    'get the start and ends
                    v1 = vPts.Item(i)
                    If i + 1 > vPts.Count Then
                        If bClosed Then v2 = vPts.Item(1) Else Exit For
                    Else
                        v2 = vPts.Item(i + 1)
                    End If
                    If v1.Vector.DistanceTo(v2.Vector, 4) > 0 Then
                        l1 = New dxeLine With {.PlaneV = aPl}
                        If Not bUsePtSettings Then
                            l1.DisplayStructure = eSets
                        Else
                            l1.DisplayStructure = dxfImageTool.DisplayStructure(myImage, v1.LayerName, v1.Color, v1.Linetype, aLTLFlag:=dxxLinetypeLayerFlag.Undefined)
                        End If
                        l1.StartPtV = v1.Vector
                        l1.EndPtV = v2.Vector
                        _rVal.Add(myImage.SaveEntity(l1))
                    End If
                    If bDisjoint Then i += 1
                Next i
                'save to file
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        Public Function aPoint(aPointXY As iVector, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxePoint
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            '#1the locating object to draw the point or points
            '#2the layer to use instead of the current layer
            '#3a color to apply instead of the current color
            '^used to draw points to the display
            '~returns the last point added to the entities collection
            Try
                Dim aCS As dxfPlane = Nothing
                Dim aPt As TVECTOR = myImage.CreateUCSVector(aPointXY, bSuppressUCS, bSuppressElevation, aCS)

                Dim _rVal As New dxePoint(aPt, New dxfDisplaySettings(dxfImageTool.DisplayStructure(myImage, aLayer, aColor, dxfLinetypes.Continuous, aLTLFlag:=dxxLinetypeLayerFlag.Undefined))) With {.PlaneV = New TPLANE(aCS)}

                'save to file
                Return myImage.SaveEntity(_rVal)
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        Public Function aPoints(aPointXYs As IEnumerable(Of iVector), Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxePoint
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            '#1the locating object to draw the point or points
            '#2the layer to use instead of the current layer
            '#3a color to apply instead of the current color
            '^used to draw points to the display
            '~returns the last point added to the entities collection
            Dim _rVal As dxePoint = Nothing
            Try
                Dim aCS As dxfPlane = Nothing
                Dim aPts As colDXFVectors = myImage.CreateVectors(aPointXYs, bSuppressUCS, bSuppressElevation, aCS, False, True)
                Dim eCol As New colDXFEntities
                If aPts.Count <= 0 Then Return Nothing
                _rVal = New dxePoint(aPts.Item(1), New dxfDisplaySettings(dxfImageTool.DisplayStructure(myImage, aLayer, aColor, dxfLinetypes.Continuous, aLTLFlag:=dxxLinetypeLayerFlag.Undefined))) With {.PlaneV = New TPLANE(aCS)}
                If aPts.Count > 1 Then _rVal.Instances.DefineWithVectors(aPts, _rVal.Center, False, True)
                'save to file
                Return myImage.SaveEntity(_rVal)
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        Public Function aPoint(Optional aX As Double = 0, Optional aY As Double = 0, Optional aZ As Double = 0, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxePoint
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            '#1X coordinate for the point
            '#2Y coordinate for the point
            '#3Z coordinate for the point
            '#4a layer to put the entity on instead of the current layer
            '#5a color to apply to the entity instead of the current color
            '^used to draw points to the display
            '~returns the dxfVector object created.
            Try
                Dim v1 As TVECTOR = myImage.CreateUCSVector(New TVECTOR(aX, aY, aZ), False, False)
                If aDisplaySettings Is Nothing Then aDisplaySettings = myImage.GetDisplaySettings(dxxEntityTypes.Point, "", dxxColors.Undefined, "", dxxLinetypeLayerFlag.Undefined, bSuppressed:=False, aStyleName:="")
                'save to file
                Return myImage.SaveEntity(New dxePoint(v1, aDisplaySettings))
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function


        Public Function aPolygon(aPolygonObj As dxePolygon, Optional aBlockName As String = "", Optional aLayer As String = "*", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "*", Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLSetting As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined, Optional bSuppressAddSegs As Object = Nothing) As dxePolygon
            Dim _rVal As dxePolygon = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return _rVal
            '[]Me~Using Polygons~helpdocs\UsingPolygons.htm~file
            '#1the dxePolygon object to draw
            '#2the layer name to assign to the new polygon. if a null string is passed the layer name of the passed polygon is retained. if '*' is passed the new polygon is put on the current layer
            '#3the color to assign to the new polygon. if dxxColors.Undefined is passed the new polygon is assigned the current color. if dxxColors.ByBlock is passed the new polygons color is retained.
            '#4the linetype name to assign to the new polygon. if a null string is passed the linetype name of the passed polygon is retained. if '*' is passed the new polygon is assigned the current linetype
            '#5flag suppress defining the new polygon with respect to the current UCS
            '^used to draw a dxePolygon to the screen
            '~dxePolygons are a collection of arcs and lines defined by the polygons vertices collection.
            Try
                _rVal = aPolygonObj.TransferedToImage(myImage, aBlockName, aLayer, aColor, aLineType, bSuppressUCS, bSuppressElevation, aLTLSetting, bSuppressAddSegs)
                _rVal = myImage.SaveEntity(_rVal)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        Public Function aPolygons(aInsertionPtsXY As IEnumerable(Of iVector), aSideCount As Integer, aRadius As Double, Optional aAngle As Double = 0.0, Optional bInscribed As Boolean = True, Optional aShearAngle As Double = 0.0, Optional aSegmentWidth As Double = 0.0, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxePolyline
            Dim rCreated As colDXFEntities = Nothing
            Return aPolygons(aInsertionPtsXY, aSideCount, aRadius, aAngle, bInscribed, aShearAngle, aSegmentWidth, rCreated, aLayer, aColor, aLineType, bSuppressUCS, bSuppressElevation)
        End Function
        Public Function aPolygons(aInsertionPtsXY As IEnumerable(Of iVector), aSideCount As Integer, aRadius As Double, aAngle As Double, bInscribed As Boolean, aShearAngle As Double, aSegmentWidth As Double, ByRef rCreated As colDXFEntities, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxePolyline
            rCreated = New colDXFEntities
            Dim _rVal As dxePolyline = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return _rVal
            '#1the center(s) of the polygons
            '#2the number of sides (3 to 100)
            '#3the radius of the polygons
            '#4 the leg length for the polygons
            '#4the angle to orient the polygons
            '^Draws a regular polygon with the requested number of sides at the passed point(s) at the passed angle.
            '~if bInscribed = True then the polygon is inscribed in a circle of the passed radius else it contains the circle of the passed radius.
            '~if a leg length is passed the hexagon will be sized such that each leg is equal to the passed value.
            Dim ips As TVECTORS
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim aPg As dxePolyline = Nothing
            Try
                If aRadius <= 0 Then Throw New Exception("Invalid Radius Passed")
                If aSideCount < 3 Or aSideCount > 100 Then Throw New Exception("invalide Side Count Requested")
                ips = myImage.CreateUCSVectors(aInsertionPtsXY, bSuppressUCS, bSuppressElevation, bErrorOnEmpty:=False)
                If ips.Count > 0 Then
                    For i = 1 To ips.Count
                        v2 = ips.Item(i)
                        If i = 1 Then
                            aPg = myImage.EntityTool.CreateShape_Polygon(aSideCount, aRadius, New dxfVector(v2), aAngle, Not bInscribed, aShearAngle, aLayer, aColor, aLineType, myImage.UCS, aSegmentWidth)
                        Else
                            aPg.Translate(v2 - v2)
                        End If
                        v1 = v2
                        'save the entity
                        rCreated.Add(myImage.SaveEntity(aPg.Clone))
                    Next i
                    _rVal = rCreated.LastMember
                End If
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        ''' <summary>
        ''' draws a rectangular polyline to the current image
        ''' </summary>
        ''' <param name="aRectangle">the whose corners are use define the returned polyline's  vertice array</param>
        ''' <param name="aDisplaySettings">the display settings to apply to overide the current image settings</param>
        ''' <param name="aInstances">new instances to assign to the new entity</param>
        ''' <param name="aSegmentWidth"> the segment width to assign to the new polyline</param>
        ''' <param name="bSuppressUCS">flag to suppress conversion of points to the current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress the current elevation when converting points to the current ucs</param>
        ''' <param name="aLTLFlag">an override LinetypeLayer setting to apply </param>
        ''' <returns></returns>
        Public Function aPolyline(aRectangle As dxfRectangle, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aInstances As dxoInstances = Nothing, Optional aSegmentWidth As Object = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As dxePolyline
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Or aRectangle Is Nothing Then Return Nothing
            Try
                Return aPolyline(aRectangle.Corners, True, aDisplaySettings:=aDisplaySettings, aInstances:=aInstances, aSegmentWidth:=aSegmentWidth, bSuppressUCS:=bSuppressUCS, bSuppressElevation:=bSuppressElevation, aLTLFlag:=aLTLFlag)
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        ''' <summary>
        ''' draws a polyline to the current image
        ''' </summary>
        ''' <param name="aVertices">the collection of vectors to use define the returned polyline's  vertice array</param>
        ''' <param name="bClosed">flag to return a closed polyline</param>
        ''' <param name="aDisplaySettings">the display settings to apply to overide the current image settings</param>
        ''' <param name="aInstances">new instances to assign to the new entity</param>
        ''' <param name="aSegmentWidth"> the segment width to assign to the new polyline</param>
        ''' <param name="bSuppressUCS">flag to suppress conversion of points to the current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress the current elevation when converting points to the current ucs</param>
        ''' <param name="aLTLFlag">an override LinetypeLayer setting to apply </param>
        ''' <returns></returns>
        Public Function aPolyline(aVertices As IEnumerable(Of iVector), Optional bClosed As Boolean = False, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aInstances As dxoInstances = Nothing, Optional aSegmentWidth As Double? = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As dxePolyline

            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Or aVertices Is Nothing Then Return Nothing
            If aVertices.Count < 2 Then Return Nothing

            Try
                Dim dsp As dxfDisplaySettings = dxfDisplaySettings.Null
                If aDisplaySettings Is Nothing Then
                    dsp.Strukture = dxfImageTool.DisplayStructure(myImage, aLayer:="", aColor:=dxxColors.Undefined, aLineType:="", aLTLFlag:=aLTLFlag)
                Else
                    dsp.Strukture = aDisplaySettings.Strukture
                End If
                Dim _rVal As dxePolyline = myImage.EntityTool.Create_Polyline(aVertices, bClosed, aSegmentWidth, dsp.LayerName, dsp.Color, dsp.Linetype, bSuppressUCS, bSuppressElevation)
                If aInstances IsNot Nothing Then _rVal.Instances = aInstances
                'save to image
                _rVal = myImage.SaveEntity(_rVal)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function


        Public Function aPolylineTrace(aVerticesXY As IEnumerable(Of iVector), aThickness As Double, Optional aClosed As Boolean = False, Optional aApplyFillets As Boolean = False, Optional aFilletEnds As Boolean = False, Optional aRotation As Double = 0.0, Optional aSegmentWidth As Double = 0.0, Optional aPlane As dxfPlane = Nothing, Optional aReturnPolygon As Boolean = False, Optional aLayerName As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "") As dxfPolyline
            Dim _rVal As dxfPolyline = Nothing
            '#1the xy vectors to use for the trace pattern
            '#2the distance between the trace edges
            '#3flag to close the trace
            '#4flag to apply arc fillets to the corners of the trace
            '#5flag to round the ends of the trace (only valid if fillets are appplied and the trace is not closed)
            '#6a rotation to apply
            '#7the segment width for the trace edges
            '#8the plane for the trace
            '#9flag to return a dxePolygon rather than a dxePolyline
            '#10a layer to put the entity on instead of the current layer
            '#11a color to apply to the entity instead of the current color
            '#12a linetype to apply to the entity instead of the current linetype setting
            '^creates a polyline trace along the path of the passed vectors.
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing

            Try
                If aVerticesXY Is Nothing Then Throw New Exception("The Passed Vertices Are Undefined")
                If aVerticesXY.Count <= 1 Then Throw New Exception("At Least Two Vertices Are Required to Create a Trace")
                If dxfPlane.IsNull(aPlane) Then aPlane = myImage.UCS

                _rVal = myImage.Primatives.Trace(aVerticesXY, aThickness, aClosed, aApplyFillets, aFilletEnds, aRotation, aSegmentWidth, aPlane, aReturnPolygon)
                _rVal.DisplaySettings = myImage.GetDisplaySettingsNR(dxxEntityTypes.Polyline, aLayerName:=aLayerName, aColor:=aColor, aLineType:=aLineType)
                Return myImage.SaveEntity(_rVal)
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        Public Function aRectangle(aRectang As iRectangle, Optional bIncludeBaseLine As Boolean = False, Optional aRotation As Single = 0.0, Optional aSegmentWidth As Double = 0.0, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional aLTScale As Integer = 0) As dxePolyline
            Dim _rVal As dxePolyline = Nothing
            '#1a dxfRectange to draw
            '#2a fillet or chamfer distance
            '#3flag indicating that the previous distance is a chamfer
            '#4a rotation angle
            '#5a segment with for the polyline
            '#6a layer to put the entity on instead of the current layer
            '#7a color to apply to the entity instead of the current color
            '#8a linetype to apply to the entity instead of the current linetype setting
            '^used to draw Rectangles
            '~Returns the dxePolyline that is the rectangle.

            If aRectang Is Nothing Then Return Nothing
            Dim dxRec As dxfRectangle = Nothing
            If TypeOf (aRectang) Is dxfRectangle Then
                dxRec = DirectCast(aRectang, dxfRectangle)
            Else
                dxRec = New dxfRectangle(aRectang)
            End If
            If Not dxRec.IsDefined(False) Then Return Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            Try
                Dim vrts As colDXFVectors = dxRec.Corners(bIncludeBaseLine)
                If aRotation <> 0 Then vrts.RotateAbout(aAxis:=dxRec.ZAxis, aAngle:=aRotation)
                If aColor = dxxColors.Undefined Then aColor = dxRec.Color
                If String.IsNullOrWhiteSpace(aLineType) Then aLineType = dxRec.Linetype
                If aLTScale <= 0 Then aLTScale = dxRec.LTScale
                _rVal = New dxePolyline(vrts, True, myImage.GetDisplaySettingsNR(dxxEntityTypes.Polyline, aLayerName:=aLayer, aColor:=aColor, aLineType:=aLineType))
                If aSegmentWidth > 0 Then _rVal.SegmentWidth = aSegmentWidth
                _rVal = myImage.SaveEntity(_rVal)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        ''' <summary>
        ''' used to draw a rectanglular polyline
        ''' </summary>
        ''' <remarks>negative widths and heights can be used to invert the Rectangle about the corner point</remarks>
        ''' <param name="aCenterOrCornerXY">1the center or lower left corner of the new rectanglular polyline</param>
        ''' <param name="aWidth">the width of the new rectanglular polyline</param>
        ''' <param name="aHeight">the height of the new rectanglular polyline</param>
        ''' <param name="aDrawMethod">a enum control to determine if the passed point is thel lower left corner or the center of the rectangle</param>
        ''' <param name="aFilletOrChamfer">a fillet or chamfer distance</param>
        ''' <param name="bChamfer">flag indicating that the previous distance is a chamfer</param>
        ''' <param name="aRotation">a rotation angle</param>
        ''' <param name="aSegmentWidth">a segment with for the polyline</param>
        ''' <param name="aDisplaySettings">the display settings to apply</param>
        ''' <param name="aInstances">new instances to assign to the new entity</param>
        ''' <param name="bSuppressUCS">flag to suppress conversion of points to the current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress the current elevation when converting points to the current ucs</param>
        ''' <param name="aLTLFlag">an override LinetypeLayer setting to apply </param>
        ''' <returns></returns>
        Public Function aRectangle(aCenterOrCornerXY As iVector, aWidth As Double, aHeight As Double, Optional aDrawMethod As dxxRectangleMethods = dxxRectangleMethods.ByCenter, Optional aFilletOrChamfer As Single = 0.0, Optional bChamfer As Boolean = False, Optional aRotation As Single = 0.0, Optional aSegmentWidth As Double = 0.0, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aInstances As dxoInstances = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As dxePolyline
            Dim _rVal As dxePolyline = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            Dim dsp As dxfDisplaySettings = dxfDisplaySettings.Null
            If aDisplaySettings Is Nothing Then
                dsp = myImage.GetDisplaySettingsNR(dxxEntityTypes.Polyline, aLTLFlag)
            Else
                dsp.Strukture = aDisplaySettings.Strukture
            End If
            Try

                _rVal = myImage.EntityTool.CreateShape_Rectangle(aCenterOrCornerXY, aWidth, aHeight, aDrawMethod, aFilletOrChamfer, bChamfer, aRotation, aSegmentWidth, dsp.LayerName, dsp.Color, dsp.Linetype, aInstances:=aInstances, bSuppressUCS:=bSuppressUCS, bSuppressElevation:=bSuppressElevation, aLTLFLag:=aLTLFlag)
                'save to file

                Return myImage.SaveEntity(_rVal)

            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        ''' <summary>
        ''' used to draw a rectanglular polyline
        ''' </summary>
        ''' <remarks>negative widths and heights can be used to invert the Rectangle about the corner point</remarks>
        ''' <param name="aCenters">1the centers the new rectanglular polyline</param>
        ''' <param name="aWidth">the width of the new rectanglular polyline</param>
        ''' <param name="aHeight">the height of the new rectanglular polyline</param>
        ''' <param name="aFilletOrChamfer">a fillet or chamfer distance</param>
        ''' <param name="bChamfer">flag indicating that the previous distance is a chamfer</param>
        ''' <param name="aRotation">a rotation angle</param>
        ''' <param name="aSegmentWidth">a segment with for the polyline</param>
        ''' <param name="aDisplaySettings">the display settings to apply</param>
        ''' <param name="bSuppressInstances">if true one rectangular polyline is returned centered at each of the passed points otherwise a single rectangle is returns with instance defined using the passed points</param>
        ''' <param name="bSuppressUCS">flag to suppress conversion of points to the current UCS</param>
        ''' <param name="bSuppressElevation">flag to suppress the current elevation when converting points to the current ucs</param>
        ''' <param name="aLTLFlag">an override LinetypeLayer setting to apply </param>
        ''' <returns></returns>
        Public Function aRectangles(aCenters As IEnumerable(Of iVector), aWidth As Double, aHeight As Double, Optional aFilletOrChamfer As Single = 0.0, Optional bChamfer As Boolean = False, Optional aRotation As Single = 0.0, Optional aSegmentWidth As Double = 0.0, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional bSuppressInstances As Boolean = False, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFlag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As List(Of dxePolyline)
            Dim _rVal As New List(Of dxePolyline)
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Or aCenters Is Nothing Then Return _rVal
            Dim dsp As dxfDisplaySettings = dxfDisplaySettings.Null
            If aDisplaySettings Is Nothing Then
                dsp = myImage.GetDisplaySettingsNR(dxxEntityTypes.Polyline, aLTLFlag)
            Else
                dsp.Strukture = aDisplaySettings.Strukture
            End If
            Try
                Dim ips As TVERTICES = myImage.CreateUCSVertices(aCenters, bSuppressUCS, bSuppressElevation, bReturnEmpty:=True)
                Dim v0 As TVERTEX = ips.Item(1)
                Dim rectangle As dxePolyline = myImage.EntityTool.CreateShape_Rectangle(New dxfVector(v0), aWidth, aHeight, dxxRectangleMethods.ByCenter, aFilletOrChamfer, bChamfer, aRotation, aSegmentWidth, dsp.LayerName, dsp.Color, dsp.Linetype, aInstances:=Nothing, bSuppressUCS:=True, bSuppressElevation:=True, aLTLFLag:=aLTLFlag)
                'save to file
                If bSuppressInstances Then
                    _rVal.Add(myImage.SaveEntity(rectangle))

                    For i As Integer = 2 To ips.Count
                        Dim v1 As TVERTEX = ips.Item(i)
                        Dim rectangle2 As New dxePolyline(rectangle) With {.Tag = v1.Tag, .Flag = v1.Flag, .Value = v1.Value}
                        _rVal.Add(myImage.SaveEntity(rectangle2))
                    Next
                Else
                    Dim insts As dxoInstances = rectangle.Instances
                    insts.Owner = Nothing
                    For i As Integer = 2 To ips.Count
                        Dim v1 As TVERTEX = ips.Item(i)
                        Dim trans As TVECTOR = v1.Vector - v0.Vector
                        insts.Add(trans.X, trans.Y)
                    Next
                    rectangle.Instances = insts
                    _rVal.Add(myImage.SaveEntity(rectangle))

                End If
                Return _rVal

            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function


        Public Function aShapes(aShapeObj As IEnumerable(Of iShape), Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aSegmentWidth As Double? = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFLag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As List(Of dxePolyline)


            If aShapeObj Is Nothing Then Return New List(Of dxePolyline)
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return New List(Of dxePolyline)


            Dim _rVal As New List(Of dxePolyline)

            Try
                Dim dsp As dxfDisplaySettings = dxfDisplaySettings.Null
                If aDisplaySettings Is Nothing Then
                    dsp.Strukture = dxfImageTool.DisplayStructure(myImage, aLayer:="", aColor:=dxxColors.Undefined, aLineType:="", aLTLFlag:=aLTLFLag)
                Else
                    dsp.Strukture = aDisplaySettings.Strukture
                End If
                For Each shape As iShape In aShapeObj
                    If shape Is Nothing Then Continue For
                    If shape.Vertices.Count < 2 Then Continue For
                    Dim newshape As dxePolyline = myImage.EntityTool.Create_Polyline(shape.Vertices, shape.Closed, aSegmentWidth, dsp.LayerName, dsp.Color, dsp.Linetype, bSuppressUCS, bSuppressElevation)
                    If newshape IsNot Nothing Then
                        _rVal.Add(myImage.SaveEntity(newshape))
                    End If
                Next

            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing

        End Function

        Public Function aShape(aShapeObj As iShape, Optional aDisplaySettings As dxfDisplaySettings = Nothing, Optional aInstances As dxoInstances = Nothing, Optional aSegmentWidth As Double? = Nothing, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False, Optional aLTLFLag As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined) As dxePolyline


            If aShapeObj Is Nothing Then Return Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            If aShapeObj.Vertices.Count < 2 Then Return Nothing

            Dim _rVal As dxePolyline = Nothing

            Try
                Dim dsp As dxfDisplaySettings = dxfDisplaySettings.Null
                If aDisplaySettings Is Nothing Then
                    dsp.Strukture = dxfImageTool.DisplayStructure(myImage, aLayer:="", aColor:=dxxColors.Undefined, aLineType:="", aLTLFlag:=aLTLFLag)
                Else
                    dsp.Strukture = aDisplaySettings.Strukture
                End If

                _rVal = myImage.EntityTool.Create_Polyline(aShapeObj.Vertices, aShapeObj.Closed, aSegmentWidth, dsp.LayerName, dsp.Color, dsp.Linetype, bSuppressUCS, bSuppressElevation)
                If aInstances IsNot Nothing Then _rVal.Instances = aInstances

                Return myImage.SaveEntity(_rVal)
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing

        End Function

        ''' <summary>
        ''' used to draw a shape from the images shapes file array
        ''' </summary>
        ''' <param name="aInsertPTXYl">the insertion point</param>
        ''' <param name="aShapesName">the name of the shape file to get the shape from</param>
        ''' <param name="aShapeNameorNumber">the name or number of the shape to retrieve from the file</param>
        ''' <param name="aHeight">the height to scale the shape up to achieve</param>
        ''' <param name="aRotation">a rotation to apply</param>
        ''' <param name="aWidthFactor">a factor to scale the width of the shape</param>
        ''' <param name="aObliqueAngle">an oblique angle to apply</param>
        ''' <param name="bSaveExploded">flag to save the sub entites of the shape to the image rater than the shape itself</param>
        '''<param name = "aLayer" > a layer To put the entity on instead of the current layer</param>
        ''' <param name="aColor">a color to apply to the entity instead of the current color</param>
        ''' ''' <param name="aLineType">a linetype to apply to the entity instead of the current linetype</param>
        ''' <param name="bSuppressUCS">flag to prevent the application of the current UCS to the new entity</param>
        ''' <param name="bSuppressElevation">flag to prevent the application of the current elevation to the new entity if the current UCS is not suppressed</param>
        ''' <returns></returns>

        Public Function aShape(aInsertPTXYl As iVector, aShapesName As String, aShapeNameorNumber As Object, aHeight As Double, Optional aRotation As Double = 0.0, Optional aWidthFactor As Double = 0.0, Optional aObliqueAngle As Double = 0.0, Optional bSaveExploded As Boolean = False, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLineType As String = "", Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeShape
            Dim _rVal As dxeShape = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            Dim fidx As Integer

            Dim aShp As New TSHAPE("")
            Dim aPlane As dxfPlane = Nothing
            Dim tDSP As New TDISPLAYVARS
            Dim vPts As TVERTICES
            'set the properties of the return solid to match the polyline
            Try
                vPts = myImage.CreateUCSVertices(aInsertPTXYl, bSuppressUCS, bSuppressElevation, aPlane, False)
                If vPts.Count <= 0 Then Return _rVal
                'the array of shapes form the image
                Dim aShapes As TSHAPES = myImage.Shapes.GetShapes(aShapesName, fidx, False, myImage)
                If fidx < 0 Then Throw New Exception("Shape File Not Found")
                If Not aShapes.TryGet(aShapeNameorNumber, aShp) Then
                    Throw New Exception($"Shape '{aShapeNameorNumber} 'Not Found In Shape File '{ aShapes.Name }'")
                End If

                tDSP = dxfImageTool.DisplayStructure(myImage, aLayer, aColor, aLineType, aLTLFlag:=dxxLinetypeLayerFlag.Undefined)
                aShp.FileName = aShapes.FileName
                _rVal = New dxeShape With {
                    .PlaneV = New TPLANE(aPlane),
                    .InsertionPtV = vPts.Item(1).Vector,
                    .DisplayStructure = tDSP,
                    .WidthFactor = aWidthFactor,
                    .ObliqueAngle = aObliqueAngle,
                    .ShapeFileName = aShapes.FileName,
                    .ShapeName = aShp.Name,
                    .ShapeNumber = aShp.ShapeNumber,
                    .ShapeCommands = aShp.PathCommands.ToList(),
                    .SaveExploded = bSaveExploded,
                    .Rotation = aRotation
                }
                If vPts.Count > 1 Then
                    _rVal.Instances.DefineWithVectors(_rVal.InsertionPt, vPts, False)
                End If

                If aHeight > 0 Then _rVal.Height = aHeight

                'save the entity
                _rVal = myImage.SaveEntity(_rVal)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        ''' <summary>
        ''' used to draw a solid to the image
        ''' </summary>
        ''' <remarks>same as a closed Polyline but will be a filled solid when viewed in AutoCAD. a solid can have 3 or 4 vertices only</remarks>
        ''' <param name="PointsXY">a collection of Points that will be used as the vertices of the created solid</param>
        ''' <param name="aLayer">a layer to put the entity on instead of the current layer</param>
        ''' <param name="aColor">a color to apply to the entity instead of the current color</param>
        ''' <returns></returns>
        Public Function aSolid(PointsXY As IEnumerable(Of iVector), Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined) As dxeSolid
            Dim _rVal As dxeSolid = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            'set the properties of the return solid to match the polyline
            Try
                _rVal = myImage.EntityTool.Create_Solid(PointsXY, aLayer, aColor)
                'save the entity
                myImage.SaveEntity(_rVal)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        Public Function aTable(aName As String, aInsertionPt As iVector, aCellData As List(Of String),
                               Optional aCellAlignments As List(Of String) = Nothing, Optional aDelimiter As String = "|",
                               Optional aTableAlign As dxxRectangularAlignments = dxxRectangularAlignments.General,
                               Optional aGridStyle As dxxTableGridStyles = dxxTableGridStyles.Undefined,
                               Optional bScaleToScreen As Boolean = False,
                               Optional aHeaderRow As Object = Nothing, Optional aHeaderCol As Object = Nothing,
                               Optional bSuppressBorders As Boolean = False,
                               Optional aTitle As Object = Nothing, Optional aFooter As Object = Nothing,
                               Optional aTextGap As Double = -1, Optional aColumnGap As Double = -1,
                               Optional aRowGap As Double = -1, Optional aCreateOnly As Boolean = False) As dxeTable

            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            Dim _rVal As dxeTable = Nothing
            '[]Me~Using Tables~HelpDocs\UsingTables.htm~File
            '#1the name to assign to the new table
            '#2the alignment point for the table
            '#3a collection of strings to define the table cell data
            '#4a collection of rectangular alignments to assign to the cells
            '#5 the delimiter that seperates the data in the passed collection
            '#6the alignment code for the table
            '#7the code that controls how grid lines are displayed in the table
            '#8flag to scale the table entities to the current display
            '#9the row to assign as the header row
            '#10the column to assign as the header column
            '#11 flag to suppress the drawing of exterior border
            '#12the title of the table
            '#13the footer string for the table
            '#13the gap to apply around table cells text as a fraction of a single character
            '#14the a length to add to the column widths
            '^used to create and draw tables as groups
            '~the created table is assigned a group name and is treated as a group when saved
            Try
                Dim table As dxeTable = myImage.EntityTool.Create_Table(aName, aInsertionPt, aCellData, aCellAlignments, aDelimiter, aTableAlign, aGridStyle, bScaleToScreen, aHeaderRow, aHeaderCol, bSuppressBorders, aTitle, aFooter, aTextGap, aColumnGap, aRowGap, aSaveAsBLock:=False)
                If Not aCreateOnly Then table = myImage.SaveEntity(table)
                _rVal = table
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return _rVal
        End Function
        Public Function aTableBlk(aName As String, aInsertionPt As iVector, aCellData As List(Of String),
                               Optional aCellAlignments As List(Of String) = Nothing, Optional aDelimiter As String = "|",
                               Optional aTableAlign As dxxRectangularAlignments = dxxRectangularAlignments.General,
                               Optional aGridStyle As dxxTableGridStyles = dxxTableGridStyles.Undefined,
                               Optional bScaleToScreen As Boolean = False,
                               Optional aHeaderRow As Object = Nothing, Optional aHeaderCol As Object = Nothing,
                               Optional bSuppressBorders As Boolean = False,
                               Optional aTitle As Object = Nothing, Optional aFooter As Object = Nothing,
                               Optional aTextGap As Double = -1, Optional aColumnGap As Double = -1,
                               Optional aRowGap As Double = -1) As dxeInsert
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            '[]Me~Using Tables~HelpDocs\UsingTables.htm~File
            '#1the name to assign to the new table
            '#2the alignment point for the table
            '#3a collection of strings to define the table cell data
            '#4a collection of rectangular alignments to assign to the cells
            '#5 the delimiter that seperates the data in the passed collection
            '#6the alignment code for the table
            '#7the code that controls how grid lines are displayed in the table
            '#8flag to scale the table entities to the current display
            '#9the row to assign as the header row
            '#10the column to assign as the header column
            '#11 flag to suppress the drawing of exterior border
            '#12the title of the table
            '#13the footer string for the table
            '#13the gap to apply around table cells text as a fraction of a single character
            '#14the a length to add to the column widths
            '^used to create and draw tables as groups
            '~the created table is assigned a group name and is treated as a group when saved
            Try
                Dim table As dxeTable = myImage.EntityTool.Create_Table(aName, aInsertionPt, aCellData, aCellAlignments, aDelimiter, aTableAlign, aGridStyle, bScaleToScreen, aHeaderRow, aHeaderCol, bSuppressBorders, aTitle, aFooter, aTextGap, aColumnGap, aRowGap, aSaveAsBLock:=True)
                Dim block As dxfBlock = table.ConvertToBlock()
                block = myImage.Blocks.Add(block)
                Return aInsert(block.Name, table.InsertionPt, 0)
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function

        Public Function aText(aAlignmentPt1XY As iVector, aString As Object, aDisplaySettings As dxfDisplaySettings, Optional aTextHeight As Double? = Nothing, Optional aAlignment As dxxMTextAlignments = dxxMTextAlignments.AlignUnknown, Optional aTextAngle As Double? = Nothing, Optional aAlignmentPt2XY As iVector = Nothing, Optional aHeightFactor As Double = 0.0, Optional aTextType As dxxTextTypes = dxxTextTypes.Multiline, Optional aAttributeTag As String = "", Optional aAttributePrompt As String = "", Optional aAttributeType As dxxAttributeTypes = dxxAttributeTypes.None, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeText
            Dim _rVal As dxeText = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            '#1 the point where the text should be aligned
            '#2the string to create a text object for
            '^used to draw  text
            '~creates and returns dxeText object.
            If aDisplaySettings Is Nothing Then aDisplaySettings = dxfDisplaySettings.Null
            Return aText(aAlignmentPt1XY, aString, aTextHeight, aAlignment, aTextAngle, aDisplaySettings.TextStyleName, aDisplaySettings.LayerName, aDisplaySettings.Color, aAlignmentPt2XY, aHeightFactor, aTextType, aAttributeTag, aAttributePrompt, aAttributeType, bSuppressUCS, bSuppressElevation)

        End Function


        Public Function aText(aAlignmentPt1XY As iVector, aString As Object, Optional aTextHeight As Double? = Nothing, Optional aAlignment As dxxMTextAlignments = dxxMTextAlignments.AlignUnknown, Optional aTextAngle As Double? = Nothing, Optional aTextStyle As String = "", Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aAlignmentPt2XY As iVector = Nothing, Optional aHeightFactor As Double = 0.0, Optional aTextType As dxxTextTypes = dxxTextTypes.Multiline, Optional aAttributeTag As String = "", Optional aAttributePrompt As String = "", Optional aAttributeType As dxxAttributeTypes = dxxAttributeTypes.None, Optional bSuppressUCS As Boolean = False, Optional bSuppressElevation As Boolean = False) As dxeText
            Dim _rVal As dxeText = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            '#1 the point where the text should be aligned
            '#2the string to create a text object for
            '#3a text height to apply which overrides the current dxfImage.Header.TextSize value (if style text height is 0)
            '#4a text alignment to apply which overrides the current text styles Alignment value
            '#5a text rotation angle to apply
            '#6a text style to use other than the current text style
            '#7a layer to put the text on insteand of the current text layer setting
            '#8a color to apply instead of the current text color setting
            '^used to draw  text
            '~creates and returns dxeText object.
            Try
                'initialize the dtext object
                _rVal = myImage.EntityTool.Create_Text(aAlignmentPt1XY, aString, aTextHeight, aAlignment, aLayer, aTextStyle, aColor, aAlignmentPt2XY, aTextAngle:=aTextAngle, aHeightFactor:=aHeightFactor, aTextType:=aTextType, bSuppressUCS:=bSuppressUCS, bSuppressElevation:=bSuppressElevation, aAttributeTag:=aAttributeTag, aAttributePrompt:=aAttributePrompt, aAttributeType:=aAttributeType)
                'save the entity
                _rVal = myImage.SaveEntity(_rVal)
                'set the cursor point to the next starting point for text of this style and height
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return _rVal
        End Function
        Public Function aTrace(VertsXY As IEnumerable(Of iVector), Optional aWidth As Double = -1, Optional bClosed As Boolean = False, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined) As colDXFEntities
            Dim _rVal As colDXFEntities = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            '#1a collection of vertices that will be used as the vertices of the trace
            '#2the width of the trace to create
            '#3flag to close the trace
            '#4the layer to put the entity on instead of the current layer setting
            '#5a color to apply to the entity instead of the current color setting
            '^used to draw a Trace
            '~a trace is a four vertex solid
            Try
                'set the properties of the return solid to match the polyline
                _rVal = myImage.EntityTool.Create_Trace(VertsXY, aWidth, bClosed, aLayer, aColor)
                'save to file
                myImage.SaveEntity(Nothing, _rVal)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        Public Function aTraceTo(StartPointXY As iVector, EndPointXY As iVector, Optional aWidth As Double = -1, Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined) As dxeSolid
            Dim _rVal As dxeSolid = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            '#1the start point of the new trace
            '#2the end point of the new trace
            '#3the width for the trace
            '#4a layer to put the entity on instead of the current layer
            '#5a color to apply to the entity instead of the current color
            '^used to draw a Trace
            '~a trace is a four vertex solid
            Try
                Dim sp As TVECTOR
                Dim ep As TVECTOR
                Dim v1 As TVECTOR = TVECTOR.Zero
                Dim v2 As TVECTOR = TVECTOR.Zero
                Dim v3 As TVECTOR = TVECTOR.Zero
                Dim v4 As TVECTOR = TVECTOR.Zero
                Dim aCS As dxfPlane = Nothing
                Dim d1 As Double
                sp = myImage.CreateUCSVector(StartPointXY, False)
                ep = myImage.CreateUCSVector(EndPointXY, False)
                d1 = sp.DistanceTo(ep, 4)
                If d1 = 0 Then Throw New Exception("The Passed Points Are Co-Incident")
                If aWidth <= 0 Then aWidth = myImage.Header.TraceWidth
                If aWidth <= 0 Then aWidth = 0.05
                aCS = myImage.UCS
                aCS.OriginV = sp
                aCS.AlignXTo(New dxfDirection(sp.DirectionTo(ep)))
                v1 = New TPLANE(aCS).Vector(0, 0.5 * aWidth)
                v2 = New TPLANE(aCS).Vector(0, -0.5 * aWidth)
                v3 = New TPLANE(aCS).Vector(d1, -0.5 * aWidth)
                v4 = New TPLANE(aCS).Vector(d1, 0.5 * aWidth)
                _rVal = New dxeSolid With {
                    .SuppressEvents = True,
                   .IsTrace = True,
                    .PlaneV = myImage.obj_UCS,
                    .Vertex1V = v1,
                    .Vertex2V = v2,
                    .Vertex3V = v3,
                    .Vertex4V = v4,
                    .DisplayStructure = dxfImageTool.DisplayStructure(myImage, aLayer, aColor, aLineType:="", aLTLFlag:=dxxLinetypeLayerFlag.Undefined)
                    }
                _rVal.SuppressEvents = False
                'save to file
                Return myImage.SaveEntity(_rVal)
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
Err:
        End Function
        Public Function anArray_Polar(aEntity As dxfEntity, aCenterXY As iVector, ArrayCount As Integer, Optional FillAngle As Double = 360, Optional bRotateEntity As Boolean = True) As colDXFEntities
            Dim _rVal As colDXFEntities = Nothing
            Dim myImage As dxfImage = Nothing
            If Not GetImage(myImage) Then Return Nothing
            '#1the drawing entity to create an array of (only allows dxfLines or dxfCircles)
            '#2the center of the polar array
            '#3the number of entities to create
            '#4 the angle to span with the polar array
            '#5flag to rotate the entities as they are created
            '^used to create arrays of simple drawing entities
            '~only accepts dxfLines or dxfCircles
            Try
                _rVal = New colDXFEntities
                Dim Center As dxfVector
                Dim bEnt As dxfEntity
                Center = myImage.CreateVector(aCenterXY, False)
                If aEntity Is Nothing Then Throw New Exception("Passed Entity Is Undefined")
                bEnt = aEntity.Clone
                If bEnt Is Nothing Then Throw New Exception("Passed Entity Could Not Be Cloned")
                If ArrayCount <= 1 Then Throw New Exception("Invalid Array Count Passed")
                FillAngle = TVALUES.NormAng(FillAngle, False, False)
                If FillAngle <= 0 Then Throw New Exception("Fill Angle Must Be Greater Than Zero")
                Dim sang As Double
                Dim EntName As String
                Dim rAng As Double
                Dim rad As Double
                Dim P As dxfVector
                Dim cnt As Integer
                '**UNUSED VAR** Dim i As Long
                Dim aAng As Double
                cnt = ArrayCount
                EntName = Trim(UCase(dxfUtils.GetEntityTypeName(aEntity.EntityType)))
                rAng = FillAngle / cnt
                rad = Center.DistanceTo(aEntity.DefinitionPoint(dxxEntDefPointTypes.HandlePt))
                sang = Center.AngleTo(aEntity.DefinitionPoint(dxxEntDefPointTypes.HandlePt))
                _rVal.Add(aEntity)
                aAng = sang + rAng
                Do Until _rVal.Count >= cnt
                    P = TPLANE.AngleVec(Center, aAng, rad)
                    bEnt = aEntity.Clone
                    bEnt.MoveTo(P)
                    If bRotateEntity Then bEnt.Rotate(rAng, False)
                    _rVal.Add(bEnt)
                Loop
                'save to file
                myImage.SaveEntity(Nothing, _rVal)
                Return _rVal
            Catch ex As Exception
                xHandleError(Reflection.MethodBase.GetCurrentMethod(), ex, myImage)
            End Try
            Return Nothing
        End Function
        Private Sub xHandleError(aMethod As Reflection.MethodBase, e As Exception, Optional myImage As dxfImage = Nothing)
            If Not GetImage(myImage) Or e Is Nothing Or aMethod Is Nothing Then Return

            myImage.HandleError(aMethod, Me.GetType(), e)
        End Sub
#End Region 'Methods
    End Class 'dxoDrawingTool
End Namespace
