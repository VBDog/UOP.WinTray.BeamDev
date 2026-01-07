Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices
Imports System.Security.Cryptography
Imports System.Windows.Controls
Imports System.Windows.Documents
Imports System.Windows.Media



Imports UOP.DXFGraphics.dxfGlobals
Imports UOP.DXFGraphics.Fonts.dxfFonts

Imports UOP.DXFGraphics.Utilities
Imports Vanara.PInvoke



Namespace UOP.DXFGraphics.Structures
    Friend Structure TTRANSFORMATION
        Implements ICloneable

#Region "Members"
        Private _ScaleVector As TVECTOR
        Private _TranslationVector As TVECTOR
        Private _RotationVector As TVECTOR
        Private _TransformationMatrix As TMATRIX4
        Private _ProjectionPlane As TPLANE

#End Region 'Members
#Region "Constructors"
        Public Sub New(aProjectionPlane As TPLANE)
            'init --------------------------------------------------
            _ScaleVector = New TVECTOR(1, 1, 1)
            _TranslationVector = TVECTOR.Zero
            _RotationVector = TVECTOR.Zero
            _ProjectionPlane = New TPLANE(aProjectionPlane)
            _TransformationMatrix = TranslationMatrix * RotationMatrix * ScaleMatrix

            'init --------------------------------------------------
        End Sub
        Public Sub New(aTransform As TTRANSFORMATION)
            'init --------------------------------------------------
            _ScaleVector = New TVECTOR(aTransform._ScaleVector)
            _TranslationVector = New TVECTOR(aTransform._TranslationVector)
            _RotationVector = New TVECTOR(aTransform._RotationVector)
            _ProjectionPlane = New TPLANE(aTransform._ProjectionPlane)
            _TransformationMatrix = TranslationMatrix * RotationMatrix * ScaleMatrix

            'init --------------------------------------------------
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Property ProjectionPlane As TPLANE
            Get
                Return _ProjectionPlane
            End Get
            Set(value As TPLANE)
                _ProjectionPlane = value
            End Set
        End Property
        Public Property ScaleVector As TVECTOR
            Get
                Return _ScaleVector
            End Get
            Set(value As TVECTOR)
                If value.Equals(_ScaleVector, 4) Then Return
                _TransformationMatrix = Nothing
                _TransformationMatrix = TranslationMatrix * RotationMatrix * ScaleMatrix

            End Set

        End Property
        Public Property TranslationVector As TVECTOR
            Get
                Return _TranslationVector
            End Get
            Set(value As TVECTOR)

                If value.Equals(_TranslationVector, 4) Then Return
                _TranslationVector = value
                _TransformationMatrix = TranslationMatrix * RotationMatrix * ScaleMatrix


            End Set
        End Property
        Public Property RotationVector As TVECTOR
            ' Roll = X, Pitch = Y, Yaw = Z in degrees
            Get
                Return _RotationVector
            End Get
            Set(value As TVECTOR)

                If value.Equals(_RotationVector, 4) Then Return
                _RotationVector = value
                _TransformationMatrix = TranslationMatrix * RotationMatrix * ScaleMatrix

            End Set
        End Property
        Public ReadOnly Property TransformationMatrix As TMATRIX4
            Get
                If _TransformationMatrix.IsNull() Then
                    _TransformationMatrix = TranslationMatrix * RotationMatrix * ScaleMatrix
                End If
                Return _TransformationMatrix
            End Get
        End Property
        Public ReadOnly Property RotationMatrix As TMATRIX4
            Get
                Return TMATRIX4.YawMatrix(_RotationVector.X) * TMATRIX4.RollMatrix(_RotationVector.Z) * TMATRIX4.PitchMatrix(_RotationVector.Y)
            End Get
        End Property
        Public ReadOnly Property TranslationMatrix As TMATRIX4
            Get
                Dim _rVal As TMATRIX4 = TMATRIX4.Identity
                _rVal.Col4 = New TVECTRIX(_TranslationVector.X, _TranslationVector.Y, _TranslationVector.Z, 1)
                Return _rVal
            End Get
        End Property
        Public ReadOnly Property ScaleMatrix As TMATRIX4
            Get
                Dim _rVal As TMATRIX4 = TMATRIX4.Identity
                _rVal.A.X = _ScaleVector.X
                _rVal.B.Y = _ScaleVector.Y
                _rVal.C.Z = _ScaleVector.Z
                Return _rVal
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function TransForm(aVector As TVECTOR) As TVECTOR

            Dim v1 As New TVECTOR(aVector)
            If Not TPLANE.IsNull(_ProjectionPlane) Then
                v1 = aVector.ProjectedTo(_ProjectionPlane).WithRespectTo(_ProjectionPlane)
            End If
            Return TransformationMatrix.Multiply(v1)
        End Function

        Public Overrides Function ToString() As String
            Return $"TTRANSFORMATION"
        End Function

        Public Function Clone() As TTRANSFORMATION
            Return New TTRANSFORMATION(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TTRANSFORMATION(Me)
        End Function
#End Region 'Methods
    End Structure
    Friend Structure TTRANSFORM
        Implements ICloneable
#Region "Members"
        Public Angle As Double
        Public FactorY As Double
        Public FactorZ As Double
        Public MirrorAxis As TLINE
        Public Radians As Boolean
        Public RotationAxis As TVECTOR
        Public RotationCenter As TVECTOR
        Public ScaleCenter As TVECTOR
        Public ScaleFactor As Double
        Public ScalePlane As TPLANE
        Public SuppressEvents As Boolean
        Public TransformType As dxxTransformationTypes
        Public Translation As TVECTOR
        Public AddRotationToMembers As Boolean
#End Region 'Members
#Region "Constructors"
        Public Sub New(aTransalation As TVECTOR)

            'init --------------------------------------------
            Angle = 0
            FactorY = 0
            FactorZ = 0
            MirrorAxis = TLINE.Null
            Radians = False
            RotationAxis = TVECTOR.Zero
            RotationCenter = TVECTOR.Zero
            ScaleCenter = TVECTOR.Zero
            ScaleFactor = 0
            ScalePlane = TPLANE.World
            SuppressEvents = False
            TransformType = dxxTransformationTypes.Undefined
            Translation = TVECTOR.Zero
            AddRotationToMembers = False
            'init --------------------------------------------

            TransformType = dxxTransformationTypes.Translation
            Translation = aTransalation
        End Sub
        Public Sub New(aType As dxxTransformationTypes)
            'init --------------------------------------------
            Angle = 0
            FactorY = 0
            FactorZ = 0
            MirrorAxis = TLINE.Null
            Radians = False
            RotationAxis = TVECTOR.Zero
            RotationCenter = TVECTOR.Zero
            ScaleCenter = TVECTOR.Zero
            ScaleFactor = 0
            ScalePlane = TPLANE.World
            SuppressEvents = False
            TransformType = aType
            Translation = TVECTOR.Zero
            AddRotationToMembers = False
            'init --------------------------------------------

        End Sub

        Public Sub New(aTransform As TTRANSFORM)
            'init --------------------------------------------
            Angle = aTransform.Angle
            FactorY = aTransform.FactorY
            FactorZ = aTransform.FactorZ
            MirrorAxis = New TLINE(aTransform.MirrorAxis)
            Radians = aTransform.Radians
            RotationAxis = New TVECTOR(aTransform.RotationAxis)
            RotationCenter = New TVECTOR(aTransform.RotationCenter)
            ScaleCenter = New TVECTOR(aTransform.ScaleCenter)
            ScaleFactor = aTransform.ScaleFactor
            ScalePlane = TPLANE.World
            SuppressEvents = aTransform.SuppressEvents
            TransformType = aTransform.TransformType
            Translation = New TVECTOR(aTransform.Translation)
            AddRotationToMembers = False
            'init --------------------------------------------

        End Sub

#End Region 'Constructors

        Public ReadOnly Property IsUndefined
            Get
                If TransformType = dxxTransformationTypes.Undefined Then Return True
                Select Case TransformType
                    Case dxxTransformationTypes.Mirror
                        Return Math.Round(MirrorAxis.Length, 6) <= 0
                    Case dxxTransformationTypes.Rotation
                        Return TVECTOR.IsNull(RotationAxis, 6) Or Angle = 0
                    Case dxxTransformationTypes.Scale
                        Return ScaleFactor = 1
                    Case dxxTransformationTypes.Translation
                        Return TVECTOR.IsNull(Translation, 8)
                    Case Else
                        Return True
                End Select
            End Get
        End Property

#Region "Properties"
        Public ReadOnly Property ScaleVector As TVECTOR
            Get
                Return New TVECTOR(ScaleFactor, FactorY, FactorZ)
            End Get
        End Property


#End Region 'Properties

#Region "Methods"
        Public Overrides Function ToString() As String
            Dim _rVal As String = $"{dxfEnums.Description(TransformType)}"
            Select Case TransformType
                Case dxxTransformationTypes.Rotation
                    _rVal = $"{_rVal} - {Angle:0.0#}"
                Case dxxTransformationTypes.Scale
                    _rVal = $"{_rVal} - {ScaleFactor:0.0#}"

                Case dxxTransformationTypes.Translation
                    _rVal = $"{_rVal} - {Translation.Coordinates(4)}"

            End Select
            Return _rVal
        End Function

        Public Function Clone() As TTRANSFORM
            Return New TTRANSFORM(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TTRANSFORM(Me)
        End Function
        Public Sub Invert()
            Select Case TransformType
                Case dxxTransformationTypes.Mirror
                Case dxxTransformationTypes.Translation
                    Translation *= -1
                Case dxxTransformationTypes.Rotation
                    Angle = -Angle
                Case dxxTransformationTypes.Scale
                    If ScaleFactor <> 0 Then ScaleFactor = 1 / ScaleFactor
                    If FactorY <> 0 Then FactorY = 1 / FactorY
                    If FactorZ <> 0 Then FactorZ = 1 / FactorZ
            End Select
        End Sub

        Public Function Rotate(aEntity As dxfEntity, Optional bNoDirections As Boolean = False) As Boolean
            Dim _rVal As Boolean
            Dim aOrigin As TVECTOR = RotationCenter
            Dim aAxis As TVECTOR = RotationAxis
            Dim aFlg As Boolean
            Dim aXs As TVECTOR = aAxis.Normalized(aFlg)
            If aFlg Then Return False

            Dim aDefPts As dxpDefPoints = aEntity.DefPts


            Dim gType As dxxGraphicTypes = aEntity.GraphicType

            Dim bNoDirs As Boolean
            Dim ang1 As Double
            Dim ang2 As Double

            Dim aAngle As Double = TVALUES.NormAng(Angle, Radians, True, True)
            If aAngle = 0 Then Return _rVal
            If Not Radians Then aAngle *= Math.PI / 180

            Dim bIsZ1 As Boolean = aXs.Equals(aDefPts.Plane.ZDirection, True, 3)
            Dim bIsZ2 As Boolean
            If bIsZ1 Then
                bIsZ2 = aOrigin.Equals(aDefPts.Plane.Origin, True, 3)
            End If
            Dim spn As Double = 0
            Dim v1 As TVECTOR
            bNoDirs = (bNoDirections Or bIsZ1) And Not gType = dxxGraphicTypes.Ellipse
            Select Case gType
                Case dxxGraphicTypes.Insert
                    If bIsZ1 Then
                        aEntity.Properties.AddToValue(50, aAngle * 180 / Math.PI, bSuppressEvents:=True)

                    End If
                Case dxxGraphicTypes.Hole
                    If bIsZ2 Then
                        aEntity.Properties.AddToValue(50, aAngle * 180 / Math.PI, bSuppressEvents:=True)
                    End If
                Case dxxGraphicTypes.Text
                    If bIsZ1 Then
                        aEntity.Properties.SetVal(50, TVALUES.NormAng(aEntity.Properties.ValueD(50) * 180 / Math.PI + aAngle * 180 / Math.PI, False, True, True) * Math.PI / 180, bSuppressEvents:=True)
                        bNoDirs = True
                    End If
                Case dxxGraphicTypes.MText
                    If bIsZ2 Then
                        aEntity.Properties.SetVal(50, TVALUES.NormAng(aEntity.Properties.ValueD(50) + (aAngle * 180 / Math.PI), False, True, True), bSuppressEvents:=True)
                    End If

                Case dxxGraphicTypes.Arc
                    Dim aAl As TSEGMENT = aEntity.ArcLineStructure
                    v1 = aAl.ArcStructure.StartPt
                    spn = aAl.ArcStructure.SpannedAngle
                    If spn >= 359.99 Then spn = 360
                    If spn <= 0.01 Then spn = 360
                    ang1 = aEntity.Properties.ValueD(50)

                    If bIsZ2 And spn < 360 Then
                        ang1 = TVALUES.NormAng(ang1 + (aAngle * 180 / Math.PI), False, True, True)
                        ang2 = TVALUES.NormAng(ang1 + spn, False, True, True)
                        aEntity.Properties.SetVal(50, ang1, bSuppressEvents:=True)
                        aEntity.Properties.SetVal(51, ang2, bSuppressEvents:=True)
                        spn = 360
                    End If
            End Select
            Dim plane As TPLANE = aEntity.PlaneV
            If plane.RotateAbout(aOrigin, aXs, aAngle, bInRadians:=True, bRotateOrigin:=True, bRotateDirections:=Not bNoDirs, bSuppressNorm:=True) Then _rVal = True
            aDefPts.Plane = plane

            If TTRANSFORM.Apply(Me, aDefPts, bSuppressEvents:=True) Then _rVal = True



            Select Case gType
                Case dxxGraphicTypes.Leader
                    'Dim pt As TVECTOR =  aEntity.Properties.ValueV("*Vector1")
                    'TTRANSFORM.Apply(Me, pt)
                    'aEntity.Properties.SetVector("*Vector1", pt, bSuppressEvents:=True)
                Case dxxGraphicTypes.Dimension
                    Dim pt As TVECTOR = aEntity.Properties.ValueV("*Vector1")
                    TTRANSFORM.Apply(Me, pt)
                    aEntity.Properties.SetVector("*Vector1", pt, bSuppressEvents:=True)
                    pt = aEntity.Properties.ValueV("*Vector2")
                    TTRANSFORM.Apply(Me, pt)
                    aEntity.Properties.SetVector("*Vector1", pt, bSuppressEvents:=True)
            End Select
            'save the changes back to the entity

            TTRANSFORM.Apply(Me, aEntity.Instances)
            Dim comps As TCOMPONENTS = aEntity.Components
            TTRANSFORM.Apply(Me, comps)
            aEntity.Components = comps


            If Not aEntity.IsDirty Then
                If aEntity.IsText Then aEntity.Strings.RotateAbout(aOrigin, aXs, aAngle, bInRadians:=True, bRotateOrigin:=True, bRotateDirections:=Not bNoDirs, bSuppressNorm:=True)
            End If

            If gType = dxxGraphicTypes.Polygon Then
                aEntity.IsDirty = True
            End If
            If gType = dxxGraphicTypes.Arc Then
                If spn < 360 Then
                    ang1 = aDefPts.Plane.XDirection.AngleTo(aDefPts.Plane.Origin.DirectionTo(v1), aDefPts.Plane.ZDirection) ' aAl.ArcStructure.StartAngle
                    ang2 = TVALUES.NormAng(ang1 + spn, False, True, True)
                    aEntity.Properties.SetVal("Start Angle", ang1, bSuppressEvents:=True) 'startangle
                    aEntity.Properties.SetVal("End Angle", ang2, bSuppressEvents:=True)
                End If
            End If


            Return _rVal
        End Function
        Public Function Rotate(aEntity As TENTITY, Optional bNoDirections As Boolean = False) As Boolean
            Dim _rVal As Boolean
            Dim aOrigin As TVECTOR = RotationCenter
            Dim aAxis As TVECTOR = RotationAxis
            Dim aFlg As Boolean
            Dim aXs As TVECTOR = aAxis.Normalized(aFlg)
            If aFlg Then Return False
            Dim bRadians As Boolean = Radians
            Dim aAngle As Double = TVALUES.NormAng(Angle, bRadians, True, True)
            If aAngle = 0 Then Return _rVal
            If Not Radians Then aAngle *= Math.PI / 180
            Dim aDefPts As TDEFPOINTS = aEntity.DefPts
            Dim aProps As TPROPERTIES = aEntity.Props
            Dim aAl As TSEGMENT
            Dim v1 As TVECTOR
            Dim gType As dxxGraphicTypes = aProps.ValueI("*GraphicType")
            Dim spn As Double
            Dim bIsZ1 As Boolean
            Dim bIsZ2 As Boolean
            Dim bNoDirs As Boolean
            Dim ang1 As Double
            Dim ang2 As Double
            Dim aTrns As New TTRANSFORMS(Me)
            bIsZ1 = aXs.Equals(aDefPts.Plane.ZDirection, True, 3)
            If bIsZ1 Then
                bIsZ2 = aOrigin.Equals(aDefPts.Plane.Origin, True, 3)
            End If
            If gType = dxxGraphicTypes.Arc Then
                aAl = TPROPERTIES.ArcLineStructure(aEntity)
                v1 = aAl.ArcStructure.StartPt
                spn = aAl.ArcStructure.SpannedAngle
                If spn >= 359.99 Then spn = 360
                If spn <= 0.01 Then spn = 360
                ang1 = aProps.GCValueD(50)
            End If
            '    If aEntity.Instances.Count <= 0 Then
            bNoDirs = (bNoDirections Or bIsZ1) And Not gType = dxxGraphicTypes.Ellipse
            '    End If
            Select Case gType
                Case dxxGraphicTypes.Insert
                    If bIsZ1 Then
                        aProps.GCValueSet(50, aProps.GCValueD(50) + (aAngle * 180 / Math.PI))
                    End If
                Case dxxGraphicTypes.Hole
                    If bIsZ2 Then
                        aProps.GCValueSet(50, aProps.GCValueD(50) + (aAngle * 180 / Math.PI))
                    End If
                Case dxxGraphicTypes.Text, dxxGraphicTypes.MText
                    If bIsZ2 Then
                        aProps.GCValueSet(50, TVALUES.NormAng(aProps.GCValueD(50) + (aAngle * 180 / Math.PI), False, True, True))
                    End If
                Case dxxGraphicTypes.Arc
                    If bIsZ2 And spn < 360 Then
                        ang1 = TVALUES.NormAng(ang1 + (aAngle * 180 / Math.PI), False, True, True)
                        ang2 = TVALUES.NormAng(ang1 + spn, False, True, True)
                        aProps.GCValueSet(50, ang1)
                        aProps.GCValueSet(51, ang2)
                        spn = 360
                    End If
            End Select
            If aDefPts.Plane.RotateAbout(aOrigin, aXs, aAngle, True, True, Not bNoDirs, True) Then _rVal = True

            Dim dfpntcnt As Integer = dxfEntity.DefPointCount(aDefPts.GraphicType)
            If dfpntcnt >= 1 Then
                If aDefPts.DefPt1.RotateAbout(aOrigin, aXs, aAngle, True, True) Then _rVal = True
            End If
            If dfpntcnt >= 2 Then
                If aDefPts.DefPt2.RotateAbout(aOrigin, aXs, aAngle, True, True) Then _rVal = True
            End If
            If dfpntcnt >= 3 Then
                If aDefPts.DefPt3.RotateAbout(aOrigin, aXs, aAngle, True, True) Then _rVal = True
            End If
            If dfpntcnt >= 4 Then
                If aDefPts.DefPt4.RotateAbout(aOrigin, aXs, aAngle, True, True) Then _rVal = True
            End If
            If dfpntcnt >= 5 Then
                If aDefPts.DefPt5.RotateAbout(aOrigin, aXs, aAngle, True, True) Then _rVal = True
            End If
            If dfpntcnt >= 6 Then
                If aDefPts.DefPt6.RotateAbout(aOrigin, aXs, aAngle, True, True) Then _rVal = True
            End If
            If dfpntcnt >= 7 Then
                If aDefPts.DefPt7.RotateAbout(aOrigin, aXs, aAngle, True, True) Then _rVal = True
            End If
            If gType = dxxGraphicTypes.Arc Then
                If spn < 360 Then
                    If v1.RotateAbout(aOrigin, aXs, aAngle, True, True) Then _rVal = True
                End If
            End If
            For i As Integer = 1 To aDefPts.Verts.Count
                v1 = aDefPts.Verts.Vector(i)
                If v1.RotateAbout(aOrigin, aXs, aAngle, True, True) Then
                    _rVal = True
                    aDefPts.Verts.SetVector(v1, i)
                End If
            Next i
            Select Case gType
                Case dxxGraphicTypes.Leader
                    Dim pt As TVECTOR = aProps.Vector("*Vector1")
                    TTRANSFORM.Apply(Me, pt)
                    aProps.SetVector("*Vector1", pt)
                Case dxxGraphicTypes.Dimension
                    Dim pt As TVECTOR = aProps.Vector("*Vector1")
                    TTRANSFORM.Apply(Me, pt)
                    aProps.SetVector("*Vector1", pt)
                    pt = aProps.Vector("*Vector2")
                    TTRANSFORM.Apply(Me, pt)
                    aProps.SetVector("*Vector2", pt)
            End Select
            'save the changes back to the entity
            aEntity.Props = aProps
            aEntity.DefPts = aDefPts
            TTRANSFORM.Apply(Me, aEntity.Components.BoundaryLoops, bNoDirs)
            TTRANSFORM.Apply(Me, aEntity.Instances, bNoDirs)
            If Not aProps.IsDirty Then
                TTRANSFORMS.Apply(aTrns, aEntity.Components.Segments, bNoDirs)
                TTRANSFORMS.Apply(aTrns, aEntity.Components.Paths, False, bNoDirs)
                If aEntity.IsText Then aEntity.Components.SubStrings.RotateAbout(aOrigin, aXs, aAngle, True, True, Not bNoDirs, True)
                'aEntity.Components.Paths.Bounds = .Paths.ExtentVectors.Bounds(aDefPts.Plane)
            End If
            If gType = dxxGraphicTypes.Polygon Then
                aEntity.Props.IsDirty = True
            End If
            If gType = dxxGraphicTypes.Arc Then
                If spn < 360 Then
                    ang1 = aDefPts.Plane.XDirection.AngleTo(aDefPts.Plane.Origin.DirectionTo(v1), aDefPts.Plane.ZDirection) ' aAl.ArcStructure.StartAngle
                    ang2 = TVALUES.NormAng(ang1 + spn, False, True, True)
                    aEntity.Props.SetVal("Start Angle", ang1) 'startangle
                    aEntity.Props.SetVal("End Angle", ang2)
                End If
            End If
            Return _rVal
        End Function
        Friend Function Scale(aEntity As dxfEntity) As Boolean
            Dim _rVal As Boolean
            Dim aScaleFactor As Double = ScaleFactor
            If aScaleFactor = 0 Then Return False
            Dim aTrns As New TTRANSFORMS(Me)
            Dim aScl As TVECTOR
            Dim aRefPt As TVECTOR
            Dim aDefPts As dxpDefPoints = aEntity.DefPts


            Dim aYScale As Double
            Dim aZScale As Double
            Dim gType As dxxGraphicTypes
            aRefPt = ScaleCenter
            aYScale = FactorY
            aZScale = FactorZ
            If aScaleFactor = 0 Then Return _rVal
            If aYScale = 0 Then aYScale = aScaleFactor
            If aZScale = 0 Then aZScale = aScaleFactor
            If aScaleFactor = 1 And aYScale = 1 And aZScale = 1 Then Return False Else _rVal = True
            aScl = New TVECTOR(aScaleFactor, aYScale, aZScale)
            gType = aEntity.GraphicType
            'If aDefPts.Plane.Origin.Scale(aScl.X, aRefPt, aScl.Y, aScl.Z) Then _rVal = True

            If TTRANSFORM.Apply(Me, aDefPts, bSuppressEvents:=True) Then _rVal = True


            aEntity.Properties.Scale(Math.Abs(aScl.X), bSuppressEvents:=True)
            Select Case gType
                Case dxxGraphicTypes.Text
                    If aScl.Y - aScl.X <> 0 Then
                        aEntity.Properties.SetVal(41, aEntity.Properties.ValueB(41) * Math.Abs(Math.Abs(aScl.Y) - Math.Abs(aScl.X)))  'width factor)
                    End If
                Case dxxGraphicTypes.Symbol, dxxGraphicTypes.Table
                    Dim style As TTABLEENTRY = aEntity.Style
                    style.Props.Multiply("FEATURESCALE", ",", Math.Abs(aScl.X))
                    aEntity.Style = style
                    aEntity.IsDirty = True

                Case dxxGraphicTypes.Leader
                    aEntity.DimStyle.Properties.Multiply(40, Math.Abs(aScl.X), bSuppressEvents:=True)
                    Dim pt As TVECTOR = aEntity.Properties.ValueV("*Vector1")
                    TTRANSFORM.Apply(Me, pt)
                    aEntity.Properties.SetVector("*Vector1", pt, bSuppressEvents:=True)
                Case dxxGraphicTypes.Dimension
                    aEntity.DimStyle.Properties.Multiply(40, Math.Abs(aScl.X), bSuppressEvents:=True)
                    Dim pt As TVECTOR = aEntity.Properties.ValueV("*Vector1")
                    TTRANSFORM.Apply(Me, pt)
                    aEntity.Properties.SetVector("*Vector1", pt, bSuppressEvents:=True)
                    pt = aEntity.Properties.ValueV("*Vector2")
                    TTRANSFORM.Apply(Me, pt)
                    aEntity.Properties.SetVector("*Vector2", pt, bSuppressEvents:=True)
            End Select

            TTRANSFORM.Apply(Me, aEntity.Instances)

            Dim aComps As TCOMPONENTS = aEntity.Components
            TTRANSFORM.Apply(Me, aComps)
            aEntity.Components = aComps

            If Not aEntity.IsDirty Then
                If aEntity.IsText Then aEntity.Strings.Scale(aScl.X, aRefPt, aScl.Y, aScl.Z)
            End If
            Return _rVal
        End Function

        Friend Function Scale(aEntity As TENTITY) As Boolean
            Dim _rVal As Boolean
            Dim aScaleFactor As Double = ScaleFactor
            If aScaleFactor = 0 Then Return False
            Dim aTrns As New TTRANSFORMS(Me)
            Dim aScl As TVECTOR
            Dim aRefPt As TVECTOR = ScaleCenter
            Dim aDefPts As TDEFPOINTS = aEntity.DefPts
            Dim aProps As TPROPERTIES = aEntity.Props
            Dim aYScale As Double = FactorY
            Dim aZScale As Double = FactorZ
            Dim gType As dxxGraphicTypes = aEntity.GraphicType
            If aYScale = 0 Then aYScale = aScaleFactor
            If aZScale = 0 Then aZScale = aScaleFactor
            If aScaleFactor = 1 And aYScale = 1 And aZScale = 1 Then Return False Else _rVal = True
            aScl = New TVECTOR(aScaleFactor, aYScale, aZScale)
            gType = aProps.ValueI("*GraphicType")
            aDefPts.Plane.Origin.Scale(aScl.X, aRefPt, aScl.Y, aScl.Z)
            Dim dfpntcnt As Integer = dxfEntity.DefPointCount(gType)
            If dfpntcnt >= 1 Then aDefPts.DefPt1.Scale(aScl.X, aRefPt, aScl.Y, aScl.Z)
            If dfpntcnt >= 2 Then aDefPts.DefPt2.Scale(aScl.X, aRefPt, aScl.Y, aScl.Z)
            If dfpntcnt >= 3 Then aDefPts.DefPt3.Scale(aScl.X, aRefPt, aScl.Y, aScl.Z)
            If dfpntcnt >= 4 Then aDefPts.DefPt4.Scale(aScl.X, aRefPt, aScl.Y, aScl.Z)
            If dfpntcnt >= 5 Then aDefPts.DefPt5.Scale(aScl.X, aRefPt, aScl.Y, aScl.Z)
            If dfpntcnt >= 6 Then aDefPts.DefPt6.Scale(aScl.X, aRefPt, aScl.Y, aScl.Z)
            If dfpntcnt >= 6 Then aDefPts.DefPt7.Scale(aScl.X, aRefPt, aScl.Y, aScl.Z)
            If TENTITY.HasVertices(aDefPts.GraphicType) Then
                For i As Integer = 1 To aDefPts.Verts.Count
                    aDefPts.Verts.SetVector(aDefPts.Verts.Vector(i).Scaled(aScl.X, aRefPt, aScl.Y, aScl.Z), i)
                    aDefPts.Verts.SetRadius(aDefPts.Verts.Radius(i) * Math.Abs(aScl.X), i)
                    aDefPts.Verts.SetStartWidth(aDefPts.Verts.StartWidth(i) * Math.Abs(aScl.X), i)
                    aDefPts.Verts.SetEndWidth(aDefPts.Verts.EndWidth(i) * Math.Abs(aScl.X), i)
                Next i
            End If
            aProps.Scale(Math.Abs(aScl.X))
            Select Case gType
                Case dxxGraphicTypes.Text
                    If aScl.Y - aScl.X <> 0 Then
                        aProps.GCValueSet(41, aProps.GCValueD(41) * Math.Abs(Math.Abs(aScl.Y) - Math.Abs(aScl.X)))  'width factor
                    End If
                Case dxxGraphicTypes.Symbol, dxxGraphicTypes.Table
                    aEntity.Style.Props.Multiply("FEATURESCALE", ",", Math.Abs(aScl.X))
                    aProps.IsDirty = True
                Case dxxGraphicTypes.Leader
                    aEntity.Style.Props.Multiply("DIMSCALE", ",", Math.Abs(aScl.X))
                    Dim pt As TVECTOR = aProps.Vector("*Vector1")
                    TTRANSFORM.Apply(Me, pt)
                    aProps.SetVector("*Vector1", pt)
                Case dxxGraphicTypes.Dimension
                    aEntity.Style.Props.Multiply("DIMSCALE", ",", Math.Abs(aScl.X))
                    Dim pt As TVECTOR = aProps.Vector("*Vector1")
                    TTRANSFORM.Apply(Me, pt)
                    aProps.SetVector("*Vector1", pt)
                    pt = aProps.Vector("*Vector2")
                    TTRANSFORM.Apply(Me, pt)
                    aProps.SetVector("*Vector2", pt)
            End Select

            Dim comps As TCOMPONENTS = aEntity.Components
            TTRANSFORM.Apply(Me, comps)
            aEntity.Components = comps

            TTRANSFORM.Apply(Me, aEntity.Instances)
            If Not aProps.IsDirty Then
                If aEntity.IsText Then aEntity.Components.SubStrings.Scale(aScl.X, aRefPt, aScl.Y, aScl.Z)
            End If
            aEntity.Props = aProps
            aEntity.DefPts = aDefPts
            Return _rVal
        End Function

        Public Function Translate(aEntity As TENTITY) As Boolean
            Dim _rVal As Boolean
            _rVal = Not TVECTOR.IsNull(Translation, 8)
            If Not _rVal Then Return _rVal
            Dim aTrans As TVECTOR = Translation
            Dim aDefPts As TDEFPOINTS = aEntity.DefPts
            Dim aProps As TPROPERTIES = aEntity.Props
            Dim gType As dxxGraphicTypes = aEntity.GraphicType
            Dim bDirty As Boolean = aProps.IsDirty Or aDefPts.IsDirty Or aEntity.Components.Paths.Count <= 0
            aDefPts.Plane.Origin += aTrans

            Dim dfpntcnt As Integer = dxfEntity.DefPointCount(aDefPts.GraphicType)
            If dfpntcnt >= 1 Then aDefPts.DefPt1 += aTrans
            If dfpntcnt >= 2 Then aDefPts.DefPt2 += aTrans
            If dfpntcnt >= 3 Then aDefPts.DefPt3 += aTrans
            If dfpntcnt >= 4 Then aDefPts.DefPt4 += aTrans
            If dfpntcnt >= 5 Then aDefPts.DefPt5 += aTrans
            If dfpntcnt >= 6 Then aDefPts.DefPt6 += aTrans
            If dfpntcnt >= 7 Then aDefPts.DefPt7 += aTrans
            For i As Integer = 1 To aDefPts.Verts.Count
                aDefPts.Verts.SetVector(aDefPts.Verts.Vector(i) + aTrans, i)
            Next i
            TTRANSFORM.Apply(Me, aEntity.Components.BoundaryLoops)
            TTRANSFORM.Apply(Me, aEntity.Instances)
            If Not bDirty Then
                If aEntity.Components.Segments.Count > 0 Then
                    TTRANSFORM.Apply(Me, aEntity.Components.Segments)
                End If
                TTRANSFORM.Apply(Me, aEntity.Components.Paths, False, False)
                If aEntity.IsText Then
                    aEntity.Components.SubStrings.Translate(aTrans)
                End If
            End If
            Select Case gType
                Case dxxGraphicTypes.Leader
                    Dim pt As TVECTOR = aProps.Vector("*Vector1")
                    TTRANSFORM.Apply(Me, pt)
                    aProps.SetVector("*Vector1", pt)
                Case dxxGraphicTypes.Dimension
                    Dim pt As TVECTOR = aProps.Vector("*Vector1")
                    TTRANSFORM.Apply(Me, pt)
                    aProps.SetVector("*Vector1", pt)
                    pt = aProps.Vector("*Vector2")
                    TTRANSFORM.Apply(Me, pt)
                    aProps.SetVector("*Vector2", pt)
            End Select
            ' aEntity.Components.Paths
            aEntity.DefPts = aDefPts
            aEntity.Props = aProps
            Return _rVal
        End Function
        Public Function Translate(aEntity As dxfEntity) As Boolean
            Dim _rVal As Boolean
            If aEntity Is Nothing Then Return False
            If TVECTOR.IsNull(Translation, 8) Then Return False
            Dim aTrans As TVECTOR = Translation
            Dim aDefPts As dxpDefPoints = aEntity.DefPts

            Dim aPlane As TPLANE = aDefPts.Plane
            Dim gType As dxxGraphicTypes = aEntity.GraphicType
            Dim bDirty As Boolean = aEntity.IsDirty Or aEntity.Components.Paths.Count <= 0
            aPlane.Origin += aTrans
            aDefPts.Plane = aPlane

            _rVal = TTRANSFORM.Apply(Me, aDefPts, bSuppressEvents:=True)

            Dim comps As TCOMPONENTS = aEntity.Components
            TTRANSFORM.Apply(Me, comps)
            aEntity.Components = comps
            TTRANSFORM.Apply(Me, aEntity.Instances)

            If Not bDirty Then

                If aEntity.IsText Then

                    aEntity.Strings.Translate(aTrans)
                End If
            End If

            Select Case gType
                Case dxxGraphicTypes.Leader
                    'Dim v1 As TVECTOR = aEntity.Properties.ValueV("*Vector1")
                    'TTRANSFORM.Apply(Me, v1)
                    'aEntity.Properties.SetVector("*Vector1", v1, bSuppressEvents:=True)
                Case dxxGraphicTypes.Dimension
                    Dim v1 As TVECTOR = aEntity.Properties.ValueV("*Vector1")
                    TTRANSFORM.Apply(Me, v1)
                    aEntity.Properties.SetVector("*Vector1", v1, bSuppressEvents:=True)
                    v1 = aEntity.Properties.ValueV("*Vector2")
                    TTRANSFORM.Apply(Me, v1)
                    aEntity.Properties.SetVector("*Vector2", v1, bSuppressEvents:=True)

            End Select


            ' aEntity.Components.Paths
            ' aEntity.SetProps(aProps)

            Return _rVal
        End Function

        Public Function Mirror(aEntity As dxfEntity) As Boolean
            Dim bFlag As Boolean
            Dim aFlag As Boolean
            Dim aAxis As TLINE = MirrorAxis
            Dim lnDir As TVECTOR = aAxis.SPT.DirectionTo(aAxis.EPT, False, rDirectionIsNull:=bFlag)
            If bFlag Then Return False
            Dim _rVal As Boolean
            Dim aImage As dxfImage = Nothing
            Dim aDefPts As dxpDefPoints = aEntity.DefPts
            Dim aPl As TPLANE
            Dim aAl As TSEGMENT = Nothing
            Dim aUCS As TPLANE
            Dim txtPln As TPLANE
            Dim aTrns As New TTRANSFORMS(Me)

            Dim ap1 As TVECTOR
            Dim ap2 As TVECTOR
            Dim gType As dxxGraphicTypes = aEntity.GraphicType
            Dim bArc As Boolean = (gType = dxxGraphicTypes.Ellipse) Or (gType = dxxGraphicTypes.Arc)
            Dim bPlanar As Boolean = aDefPts.Plane.Contains(aAxis)
            Dim bCorText As Boolean
            Dim bMirDirs As Boolean = False
            Dim ang1 As Double
            Dim ang2 As Double
            Dim tang As Double
            Dim bDontDoIt As Boolean
            If bArc Then
                aAl = aEntity.ArcLineStructure
                ap1 = aAl.ArcStructure.StartPt
                ap2 = aAl.ArcStructure.EndPt
            End If
            If gType = dxxGraphicTypes.Ellipse Or aEntity.IsText Then bMirDirs = True
            If aEntity.IsText Then
                txtPln = aDefPts.Plane
                tang = aEntity.Properties.ValueD(50)
                If tang <> 0 Then txtPln.Revolve(tang)
                If goEvents.GetHeaderVariable(aEntity.ImageGUID, "$MIRRTEXT", aImage, 1) <> 1 Then
                    If aImage IsNot Nothing Then aUCS = aImage.obj_UCS Else aUCS = TPLANE.World
                    If aUCS.Contains(txtPln.LineH(0, 10)) Then
                        Dim iLine As New TLINE(txtPln.Origin, txtPln.Origin + (txtPln.XDirection * 10))
                        Dim on1 As Boolean = False
                        Dim on2 As Boolean = False
                        Dim xst As Boolean = False
                        ap1 = iLine.IntersectionPt(aAxis, aFlag, bFlag, on1, on2, xst)
                        If Not aFlag And Not bFlag Then
                            ang1 = ap1.DirectionTo(txtPln.Origin).AngleTo(lnDir, txtPln.ZDirection)
                            If TTRANSFORM.CreateRotation(ap1, 2 * ang1, False, txtPln.ZDirection, True).Rotate(aEntity, True) Then
                                _rVal = True
                            End If
                            bCorText = True
                            aDefPts = aEntity.DefPts
                            bDontDoIt = True
                        Else
                            If bFlag Then
                                Return _rVal
                            Else
                                ap1 = txtPln.Origin.ProjectedTo(aAxis)
                                If TTRANSFORM.CreateRotation(ap1, 180, False, txtPln.ZDirection, True).Rotate(aEntity, True) Then
                                    _rVal = True
                                End If
                                aDefPts = aEntity.DefPts
                                bCorText = True
                                bDontDoIt = True
                            End If
                        End If
                    End If
                End If
            End If
            If Not bDontDoIt Then
                If aDefPts.Plane.Mirror(aAxis, bMirrorOrigin:=True, bMirrorDirections:=bMirDirs, bSuppressCheck:=True) Then _rVal = True

                If TTRANSFORM.Apply(Me, aDefPts, bSuppressEvents:=True) Then _rVal = True



                Dim comps As TCOMPONENTS = aEntity.Components
                TTRANSFORM.Apply(Me, comps)
                aEntity.Components = comps

                TTRANSFORM.Apply(Me, aEntity.Instances)

                Dim aComps = aEntity.Components

                If Not aEntity.IsDirty Then

                    If aEntity.IsText Then aEntity.Strings.Mirror(aAxis, True, bMirDirs, True)
                End If
                aEntity.Components = aComps
            End If
            aPl = aDefPts.Plane
            If _rVal And bArc And aAl.ArcStructure.SpannedAngle < 359.99 Then
                ang1 = aPl.XDirection.AngleTo(aPl.Origin.DirectionTo(ap1), aPl.ZDirection)
                ang2 = aPl.XDirection.AngleTo(aPl.Origin.DirectionTo(ap2), aPl.ZDirection)
                If gType = dxxGraphicTypes.Arc Then
                    Dim arc As dxeArc = aEntity

                    'invert the arc so the end pts are mirrored perpendicularly
                    aEntity.Properties.SetVal("Start Angle", ang1, bSuppressEvents:=True)
                    aEntity.Properties.SetVal("End Angle", ang2, bSuppressEvents:=True)
                    aEntity.Properties.SetVal("*Clockwise", Not aEntity.Properties.ValueB("*Clockwise"), bSuppressEvents:=True)
                ElseIf gType = dxxGraphicTypes.Ellipse Then
                    'just update the angles because the plane was flipped
                    aEntity.Properties.SetVal("*Start Angle", ang1, bSuppressEvents:=True)
                    aEntity.Properties.SetVal("*End Angle", ang2, bSuppressEvents:=True)
                End If
            End If
            Select Case gType
                Case dxxGraphicTypes.Leader
                    'Dim pt As TVECTOR = aProps.Vector("*Vector1")
                    'TTRANSFORM.Apply(Me, pt)
                    'aProps.SetVector("*Vector1", pt)
                Case dxxGraphicTypes.Dimension
                    Dim pt As TVECTOR = aEntity.Properties.ValueV("*Vector1")
                    TTRANSFORM.Apply(Me, pt)
                    aEntity.Properties.SetVector("*Vector1", pt, bSuppressEvents:=True)
                    pt = aEntity.Properties.ValueV("*Vector2")
                    TTRANSFORM.Apply(Me, pt)
                    aEntity.Properties.SetVector("*Vector2", pt, bSuppressEvents:=True)
            End Select

            If bCorText Then
                aDefPts = aEntity.DefPts
                txtPln = aDefPts.Plane
                tang = aEntity.Properties.ValueD(50)
                If tang >= 90 And tang < 270 Then
                    TTRANSFORM.CreateRotation(txtPln.Origin, 180, False, txtPln.ZDirection, True).Rotate(aEntity, True)
                End If
            End If

            Return _rVal
        End Function
        Public Function Mirror(aEntity As TENTITY) As Boolean
            Dim bFlag As Boolean
            Dim aFlag As Boolean
            Dim aAxis As TLINE = MirrorAxis
            Dim lnDir As TVECTOR = aAxis.SPT.DirectionTo(aAxis.EPT, False, rDirectionIsNull:=bFlag)
            If bFlag Then Return False
            Dim _rVal As Boolean
            Dim aImage As dxfImage = Nothing
            Dim aDefPts As TDEFPOINTS = aEntity.DefPts
            Dim aPl As TPLANE
            Dim aAl As TSEGMENT = Nothing
            Dim aUCS As TPLANE
            Dim txtPln As TPLANE
            Dim aTrns As New TTRANSFORMS(Me)
            Dim aProps As TPROPERTIES = aEntity.Props
            Dim ap1 As TVECTOR
            Dim ap2 As TVECTOR
            Dim gType As dxxGraphicTypes = aEntity.GraphicType
            Dim bArc As Boolean = (gType = dxxGraphicTypes.Ellipse) Or (gType = dxxGraphicTypes.Arc)
            Dim bPlanar As Boolean = aDefPts.Plane.Contains(aAxis)
            Dim bCorText As Boolean
            Dim bMirDirs As Boolean = False
            Dim ang1 As Double
            Dim ang2 As Double
            Dim tang As Double
            Dim bDontDoIt As Boolean
            If bArc Then
                aAl = TPROPERTIES.ArcLineStructure(aEntity)
                ap1 = aAl.ArcStructure.StartPt
                ap2 = aAl.ArcStructure.EndPt
            End If
            If gType = dxxGraphicTypes.Ellipse Or aEntity.IsText Then bMirDirs = True
            If aEntity.IsText Then
                txtPln = aDefPts.Plane
                tang = aProps.GCValueD(50)
                'tang = aProps.Value(dxfGlobals.CommonProps + 6, dxxPropertyTypes.dxf_Single)
                If tang <> 0 Then txtPln.Revolve(tang)
                If goEvents.GetHeaderVariable(aEntity.ImageGUID, "$MIRRTEXT", aImage, 1) <> 1 Then
                    If aImage IsNot Nothing Then aUCS = aImage.obj_UCS Else aUCS = TPLANE.World
                    If aUCS.Contains(txtPln.LineH(0, 10)) Then
                        Dim iLine As New TLINE(txtPln.Origin, txtPln.Origin + (txtPln.XDirection * 10))
                        Dim on1 As Boolean = False
                        Dim on2 As Boolean = False
                        Dim xst As Boolean = False
                        ap1 = iLine.IntersectionPt(aAxis, aFlag, bFlag, on1, on2, xst)
                        If Not aFlag And Not bFlag Then
                            ang1 = ap1.DirectionTo(txtPln.Origin).AngleTo(lnDir, txtPln.ZDirection)
                            If TTRANSFORM.CreateRotation(ap1, 2 * ang1, False, txtPln.ZDirection, True).Rotate(aEntity, True) Then
                                _rVal = True
                            End If
                            bCorText = True
                            aDefPts = aEntity.DefPts
                            aProps = aEntity.Props
                            bDontDoIt = True
                        Else
                            If bFlag Then
                                Return _rVal
                            Else
                                ap1 = txtPln.Origin.ProjectedTo(aAxis)
                                If TTRANSFORM.CreateRotation(ap1, 180, False, txtPln.ZDirection, True).Rotate(aEntity, True) Then
                                    _rVal = True
                                End If
                                aDefPts = aEntity.DefPts
                                aProps = aEntity.Props
                                bCorText = True
                                bDontDoIt = True
                            End If
                        End If
                    End If
                End If
            End If
            If Not bDontDoIt Then

                Dim dfpntcnt As Integer = dxfEntity.DefPointCount(gType)
                If aDefPts.Plane.Mirror(aAxis, bMirrorOrigin:=True, bMirrorDirections:=bMirDirs, bSuppressCheck:=True) Then _rVal = True
                If dfpntcnt >= 1 Then
                    If aDefPts.DefPt1.Mirror(aAxis, bSuppressCheck:=True) Then _rVal = True
                End If
                If dfpntcnt >= 2 Then
                    If aDefPts.DefPt2.Mirror(aAxis, bSuppressCheck:=True) Then _rVal = True
                End If
                If dfpntcnt >= 3 Then
                    If aDefPts.DefPt3.Mirror(aAxis, bSuppressCheck:=True) Then _rVal = True
                End If
                If dfpntcnt >= 4 Then
                    If aDefPts.DefPt4.Mirror(aAxis, bSuppressCheck:=True) Then _rVal = True
                End If
                If dfpntcnt >= 5 Then
                    If aDefPts.DefPt5.Mirror(aAxis, bSuppressCheck:=True) Then _rVal = True
                End If
                If dfpntcnt >= 6 Then
                    If aDefPts.DefPt6.Mirror(aAxis, bSuppressCheck:=True) Then _rVal = True
                End If
                If dfpntcnt >= 7 Then
                    If aDefPts.DefPt7.Mirror(aAxis, bSuppressCheck:=True) Then _rVal = True
                End If
                If bArc Then
                    If ap1.Mirror(aAxis, bSuppressCheck:=True) Then _rVal = True
                    If ap2.Mirror(aAxis, bSuppressCheck:=True) Then _rVal = True
                End If

                For i As Integer = 1 To aDefPts.Verts.Count
                    ap1 = aDefPts.Verts.Vector(i)
                    If ap1.Mirror(aAxis, bSuppressCheck:=True) Then
                        _rVal = True
                    End If
                    aDefPts.Verts.SetVector(ap1, i)
                    If aDefPts.Verts.Radius(i) <> 0 Then
                        aDefPts.Verts.SetInverted(Not aDefPts.Verts.Inverted(i), i)
                    End If
                Next i

                TTRANSFORM.Apply(Me, aEntity.Components.BoundaryLoops)
                TTRANSFORM.Apply(Me, aEntity.Instances)
                If Not aProps.IsDirty Then
                    TTRANSFORM.Apply(Me, aEntity.Components.Segments)
                    TTRANSFORM.Apply(Me, aEntity.Components.Paths, bMirDirs, False)
                    If aEntity.IsText Then aEntity.Components.SubStrings.Mirror(aAxis, True, bMirDirs, True)
                End If
            End If
            Select Case gType
                Case dxxGraphicTypes.Leader
                    Dim pt As TVECTOR = aProps.Vector("*Vector1")
                    TTRANSFORM.Apply(Me, pt)
                    aProps.SetVector("*Vector1", pt)
                Case dxxGraphicTypes.Dimension
                    Dim pt As TVECTOR = aProps.Vector("*Vector1")
                    TTRANSFORM.Apply(Me, pt)
                    aProps.SetVector("*Vector1", pt)
                    pt = aProps.Vector("*Vector2")
                    TTRANSFORM.Apply(Me, pt)
                    aProps.SetVector("*Vector2", pt)
            End Select
            aEntity.DefPts = aDefPts
            aEntity.Props = aProps
            aPl = aDefPts.Plane
            If _rVal And bArc And aAl.ArcStructure.SpannedAngle < 359.99 Then
                ang1 = aPl.XDirection.AngleTo(aPl.Origin.DirectionTo(ap1), aPl.ZDirection)
                ang2 = aPl.XDirection.AngleTo(aPl.Origin.DirectionTo(ap2), aPl.ZDirection)
                If gType = dxxGraphicTypes.Arc Then
                    'invert the arc so the end pts are mirrored perpendicularly
                    aEntity.Props.SetVal("*Start Angle", ang1)
                    aEntity.Props.SetVal("*End Angle", ang2)
                    aEntity.Props.InvertSwitch("*Clockwise")
                ElseIf gType = dxxGraphicTypes.Ellipse Then
                    'just update the angles because the plane was flipped
                    aEntity.Props.SetVal("*StartAngle", ang1)
                    aEntity.Props.SetVal("*EndAngle", ang2)
                End If
            End If
            If bCorText Then
                aDefPts = aEntity.DefPts
                aProps = aEntity.Props
                txtPln = aDefPts.Plane
                tang = aProps.GCValueD(50)
                'tang = aProps.Value(dxfGlobals.CommonProps + 6, dxxPropertyTypes.dxf_Single)
                If tang >= 90 And tang < 270 Then
                    TTRANSFORM.CreateRotation(txtPln.Origin, 180, False, txtPln.ZDirection, True).Rotate(aEntity, True)
                End If
            End If
            Return _rVal
        End Function
#End Region 'Methods
#Region "Shared Methods"

        Public Shared ReadOnly Property Null
            Get
                Return New TTRANSFORM(aType:=dxxTransformationTypes.Undefined)
            End Get
        End Property
        Public Shared Function TranslationVector(aVectorObj As iVector, Optional aPlane As dxfPlane = Nothing) As TVECTOR
            '^Creates a vector containing X,Y,Z displacements. If a plane is passed the returned translations are with respect to the
            '^X,Y and Z directions of the plane
            Return TTRANSFORM.TranslationVector(New TVECTOR(aVectorObj), aPlane)
        End Function
        Public Shared Function TranslationVector(aVector As TVECTOR, Optional aPlane As dxfPlane = Nothing) As TVECTOR
            '^Creates a vector containing X,Y,Z displacements. If a plane is passed the returned translations are with respect to the
            '^X,Y and Z directions of the plane
            Dim _rVal As TVECTOR = aVector
            If Not dxfPlane.IsNull(aPlane) Then
                _rVal = New TPLANE(aPlane).Vector(_rVal.X, _rVal.Y, _rVal.Z)
            End If
            Return _rVal
        End Function
        Public Shared Function CreatePolarTranslation(aBasePt As iVector, aAngle As Double, aDistance As Double, Optional aAlternateBase As iVector = Nothing, Optional aPlane As dxfPlane = Nothing, Optional bInRadians As Boolean = False, Optional bSuppressEvents As Boolean = False) As TTRANSFORM
            Dim rTr As New TTRANSFORM(dxxTransformationTypes.Translation)
            '#2the base point to use as the center
            '#3the direction angle to move in
            '#4the distance to move
            '#5the coordinate system to use to determine the direction based on the angle
            '^moves the entity to a point on a plane aligned with the XY plane of the passed coordinate system and centered at the base point.
            '~if the base point is nothing passed the center of the entities coordinate system is used.
            '~if the coordinate system is not passed the world coordinate system is used.
            If aDistance = 0 Then Return rTr
            Dim aPl As New TPLANE(aPlane)
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim aDir As TVECTOR

            aDir = aPl.Direction(aAngle, bInRadians)
            If aBasePt IsNot Nothing Then
                v1 = New TVECTOR(aBasePt)
            ElseIf aAlternateBase IsNot Nothing Then
                v1 = New TVECTOR(aAlternateBase)
            Else
                v1 = aPl.Origin
            End If
            v2 = v1 + (aDir * aDistance)
            v1 = (v2 - v1)
            rTr = TTRANSFORM.CreateTranslation(v1, bSuppressEvents)
            Return rTr
        End Function
        Public Shared Function CreatePlanarRotation(aOrigin As TVECTOR, aPlane As TPLANE, aAngle As Double, bInRadians As Boolean, bPlane As dxfPlane, aAxis As dxxAxisDescriptors, ByRef rAxis As dxeLine, Optional bSuppressEvents As Boolean = False, Optional bAddRotationToMembers As Boolean = False) As TTRANSFORM

            Dim aPl As TPLANE
            Dim aDir As TVECTOR
            If bPlane IsNot Nothing Then aPl = New TPLANE(bPlane) Else aPl = aPlane
            If aAxis = dxxAxisDescriptors.X Then
                aDir = aPl.XDirection
            ElseIf aAxis = dxxAxisDescriptors.Y Then
                aDir = aPl.YDirection
            Else
                aDir = aPl.ZDirection
            End If
            If rAxis Is Nothing Then rAxis = New dxeLine
            rAxis.StartPtV = aOrigin
            rAxis.EndPtV = aOrigin + aDir * 100
            Return TTRANSFORM.CreateRotation(rAxis, aAngle, bInRadians, bSuppressEvents, bAddRotationToMembers)
        End Function
        Public Shared Function CreateMirror(aSP As TVECTOR, aEP As TVECTOR, Optional bSuppressEvents As Boolean = False) As TTRANSFORM
            If aSP.DistanceTo(aEP, 5) <= 0.001 Then Return Nothing
            Dim aTr As New TTRANSFORM(dxxTransformationTypes.Mirror)
            aTr.MirrorAxis.SPT = aSP
            aTr.MirrorAxis.EPT = aEP
            aTr.SuppressEvents = bSuppressEvents
            Return aTr
        End Function
        Public Shared Function CreateMirror(aLine As TLINE, Optional bSuppressEvents As Boolean = False) As TTRANSFORM
            If aLine.Length <= 0 Then Return TTRANSFORM.Null
            Return TTRANSFORM.CreateMirror(aLine.SPT, aLine.EPT, bSuppressEvents)
        End Function
        Public Shared Function CreateMirror(aLine As iLine, Optional bSuppressEvents As Boolean = False) As TTRANSFORM
            If aLine Is Nothing Then Return TTRANSFORM.Null
            Return TTRANSFORM.CreateMirror(New TLINE(aLine), bSuppressEvents)
        End Function
        Public Shared Function CreateProjection(aDirection As TVECTOR, aDistance As Double, Optional bNormalize As Boolean = False, Optional bSupressEvents As Boolean = False) As TTRANSFORM
            If aDistance = 0 Then Return New TTRANSFORM(dxxTransformationTypes.Translation)
            Dim aDir As TVECTOR
            Dim aFlag As Boolean
            aDir = aDirection
            If bNormalize Then aDir.Normalize(aFlag) Else aFlag = TVECTOR.IsNull(aDirection)
            If aFlag Or aDistance = 0 Then
                Return New TTRANSFORM(dxxTransformationTypes.Translation)
            Else
                Return TTRANSFORM.CreateTranslation(aDir * aDistance, bSupressEvents)
            End If
        End Function
        Public Shared Function CreateProjection(aDirection As dxfDirection, aDistance As Double, Optional bNormalize As Boolean = False, Optional bSupressEvents As Boolean = False) As TTRANSFORM
            Return CreateProjection(New TVECTOR(aDirection), TVALUES.To_DBL(aDistance), bNormalize, bSupressEvents)
        End Function

        Public Shared Function CreateProjection(aDirection As iVector, aDistance As Double, Optional bNormalize As Boolean = False, Optional bSupressEvents As Boolean = False) As TTRANSFORM
            Return CreateProjection(New TVECTOR(aDirection), TVALUES.To_DBL(aDistance), bNormalize, bSupressEvents)
        End Function
        Public Shared Function CreateRotation(aPoint As iVector, aPlane As dxfPlane, aAngle As Double, bInRadians As Boolean, Optional bSuppressEvents As Boolean = False, Optional aAxisDescriptor As dxxAxisDescriptors = dxxAxisDescriptors.Z, Optional bAddRotationToMembers As Boolean = False) As TTRANSFORM
            Dim rAxis As dxeLine = Nothing
            Return CreateRotation(aPoint, aPlane, aAngle, bInRadians, rAxis, bSuppressEvents, aAxisDescriptor, bAddRotationToMembers)
        End Function
        Public Shared Function CreateRotation(aPoint As iVector, aPlane As dxfPlane, aAngle As Double, bInRadians As Boolean, ByRef rAxis As dxeLine, Optional bSuppressEvents As Boolean = False, Optional aAxisDescriptor As dxxAxisDescriptors = dxxAxisDescriptors.Z, Optional bAddRotationToMembers As Boolean = False) As TTRANSFORM
            If aPoint Is Nothing Then
                If dxfPlane.IsNull(aPlane) Then aPoint = dxfVector.Zero Else aPoint = New dxfVector(aPlane.Origin)
            End If
            rAxis = dxfPlane.CreateAxis(aPlane, aPoint, aAxisDescriptor)
            Return TTRANSFORM.CreateRotation(rAxis, aAngle, bInRadians, bSuppressEvents, bAddRotationToMembers)
        End Function
        Public Shared Function CreateRotation(aLine As iLine, aPlane As dxfPlane, aAngle As Double, bInRadians As Boolean, Optional bSuppressEvents As Boolean = False, Optional bAddRotationToMembers As Boolean = False) As TTRANSFORM
            Dim rAxis As dxeLine = Nothing
            Return CreateRotation(aLine, aPlane, aAngle, bInRadians, rAxis, bSuppressEvents, bAddRotationToMembers)
        End Function
        Public Shared Function CreateRotation(aLine As iLine, aPlane As dxfPlane, aAngle As Double, bInRadians As Boolean, ByRef rAxis As dxeLine, Optional bSuppressEvents As Boolean = False, Optional bAddRotationToMembers As Boolean = False) As TTRANSFORM
            If aLine IsNot Nothing Then

                Dim l1 As New TLINE(aLine)

                If l1.Length = 0 Then
                    Return CreateRotation(aLine.StartPt, aPlane, aAngle, bInRadians, rAxis, bSuppressEvents)
                Else
                    rAxis = New dxeLine(l1)
                End If
            Else
                If dxfPlane.IsNull(aPlane) Then
                    rAxis = dxfPlane.CreateAxis(New dxfPlane, dxfVector.Zero)
                Else
                    rAxis = dxfPlane.CreateAxis(aPlane, dxfVector.Zero)
                End If
            End If
            Return TTRANSFORM.CreateRotation(rAxis, aAngle, bInRadians, bSuppressEvents, bAddRotationToMembers)
        End Function
        Public Shared Function CreateRotation(aCenter As TVECTOR, aAngle As Double, bInRadians As Boolean, aAxis As TVECTOR, Optional bSuppressEvents As Boolean = False, Optional bAddRotationToMembers As Boolean = False) As TTRANSFORM
            Return New TTRANSFORM(dxxTransformationTypes.Rotation) With {
            .RotationCenter = aCenter,
            .RotationAxis = aAxis.Normalized,
            .Angle = aAngle,
            .Radians = bInRadians,
            .SuppressEvents = bSuppressEvents,
           .AddRotationToMembers = bAddRotationToMembers
        }
        End Function
        Public Shared Function CreateRotation(aLine As iLine, aAngle As Double, bInRadians As Boolean, Optional bSuppressEvents As Boolean = False, Optional bAddRotationToMembers As Boolean = False) As TTRANSFORM
            Dim aTr As New TTRANSFORM(dxxTransformationTypes.Rotation)
            If aAngle = 0 Or aLine Is Nothing Then Return aTr
            aTr.RotationCenter = New TVECTOR(aLine.StartPt)
            aTr.RotationAxis = (New TVECTOR(aLine.EndPt) - New TVECTOR(aLine.StartPt)).Normalized
            aTr.Angle = aAngle
            aTr.Radians = bInRadians
            aTr.SuppressEvents = bSuppressEvents
            aTr.AddRotationToMembers = bAddRotationToMembers
            Return aTr
        End Function
        Public Shared Function CreateTranslation(Optional aChangeX As Double = 0, Optional aChangeY As Double = 0, Optional aChangeZ As Double = 0, Optional aPlane As dxfPlane = Nothing, Optional aBasePt As dxfVector = Nothing, Optional bSuppressEvents As Boolean = False) As TTRANSFORM
            Dim _rVal As New TTRANSFORM(dxxTransformationTypes.Translation)
            Dim dsp As New TVECTOR(aChangeX, aChangeY, aChangeZ)
            If TVECTOR.IsNull(dsp) Then Return _rVal
            _rVal.TransformType = dxxTransformationTypes.Translation
            If Not dxfPlane.IsNull(aPlane) Then
                If aBasePt IsNot Nothing Then
                    Dim v1 As TVECTOR = aBasePt.Strukture
                    Dim v2 As TVECTOR = New TPLANE(aPlane).VectorRelative(v1, dsp.X, dsp.Y, dsp.Z)
                    dsp = v2 - v1
                Else
                    dsp = New TPLANE(aPlane).Vector(aChangeX, aChangeY, aChangeZ, 0)
                End If
            End If
            _rVal.Translation = dsp
            _rVal.SuppressEvents = bSuppressEvents
            Return _rVal
        End Function
        Public Shared Function CreateTranslation(aVector As TVECTOR, Optional bSuppressEvents As Boolean = False, Optional aPlane As dxfPlane = Nothing) As TTRANSFORM
            Return New TTRANSFORM(dxxTransformationTypes.Translation) With {
        .Translation = TTRANSFORM.TranslationVector(aVector, aPlane),
            .SuppressEvents = bSuppressEvents
        }
        End Function

        Public Shared Function CreateTranslation(aVector As iVector, Optional bSuppressEvents As Boolean = False, Optional aPlane As dxfPlane = Nothing) As TTRANSFORM
            Return New TTRANSFORM(dxxTransformationTypes.Translation) With {
        .Translation = TTRANSFORM.TranslationVector(aVector, aPlane),
            .SuppressEvents = bSuppressEvents
        }
        End Function

        Public Shared Function CreateFromTo(aBasePointXY As iVector, aDestinationPointXY As iVector, Optional aXChange As Double = 0, Optional aYChange As Double = 0, Optional aZChange As Double = 0, Optional bSuppressEvents As Boolean = False) As TTRANSFORM
            Dim _rVal As New TTRANSFORM(dxxTransformationTypes.Translation)

            If aBasePointXY Is Nothing And aDestinationPointXY Is Nothing Then Return _rVal
            Dim v1 As TVECTOR = New TVECTOR(aBasePointXY)
            Dim v2 As TVECTOR = New TVECTOR(aDestinationPointXY) + New TVECTOR(aXChange, aYChange, aZChange)

            _rVal = TTRANSFORM.CreateTranslation(v2 - v1, bSuppressEvents)
            Return _rVal
        End Function
        Public Shared Function CreateScale(aScaleCenter As iVector, aScaleFactorX As Double, Optional aScaleFactorY As Double? = Nothing, Optional aScaleFactorZ As Double? = Nothing, Optional aPlane As dxfPlane = Nothing, Optional bSuppressEvents As Boolean = False) As TTRANSFORM
            Return CreateScale(New TVECTOR(aScaleCenter), aScaleFactorX, aScaleFactorY, aScaleFactorZ, aPlane, bSuppressEvents)
        End Function

        Public Shared Function CreateScale(aScaleCenter As TVECTOR, aScaleFactorX As Double, Optional aScaleFactorY As Double? = Nothing, Optional aScaleFactorZ As Double? = Nothing, Optional aPlane As dxfPlane = Nothing, Optional bSuppressEvents As Boolean = False) As TTRANSFORM
            Dim aTr As New TTRANSFORM(dxxTransformationTypes.Scale)
            If aScaleFactorX = 0 Then aScaleFactorX = 1
            Dim sfy As Double = aScaleFactorX
            Dim sfz As Double = aScaleFactorX


            If aScaleFactorY.HasValue Then
                If aScaleFactorY.Value <> 0 Then sfy = aScaleFactorY.Value
            End If
            If aScaleFactorZ.HasValue Then
                If aScaleFactorZ.Value <> 0 Then sfz = aScaleFactorZ.Value
            End If
            Dim v1 As New TVECTOR(aScaleFactorX, sfy, sfz)

            If v1.Y = 0 Then v1.Y = v1.X
            If v1.Z = 0 Then v1.Z = v1.X
            aTr.TransformType = dxxTransformationTypes.Scale
            aTr.ScaleCenter = aScaleCenter
            aTr.ScaleFactor = v1.X
            aTr.FactorY = v1.Y
            aTr.FactorZ = v1.Z
            If Not dxfPlane.IsNull(aPlane) Then aTr.ScalePlane = New TPLANE(aPlane)
            aTr.SuppressEvents = bSuppressEvents
            Return aTr
        End Function
        Public Shared Function Apply(aTransform As TTRANSFORM, ByRef ioPlane As TPLANE, Optional bSuppressScale As Boolean = False, Optional bSuppressRotation As Boolean = False) As Boolean
            Dim _rVal As Boolean = False
            If TTRANSFORM.Apply(aTransform, ioPlane.Origin, bSuppressScale, bSuppressRotation) Then _rVal = True
            If aTransform.TransformType = dxxTransformationTypes.Rotation And Not bSuppressRotation Then
                If ioPlane.RotateAbout(ioPlane.Origin, aTransform.RotationAxis, aTransform.Angle, aTransform.Radians) Then _rVal = True
            End If
            Return _rVal
        End Function
        Public Shared Function Apply(aTransform As TTRANSFORM, ByRef ioCharacterBox As TCHARBOX, Optional bSuppressScale As Boolean = False, Optional bSuppressRotation As Boolean = False) As Boolean
            Dim _rVal As Boolean = False
            If TTRANSFORM.Apply(aTransform, ioCharacterBox.BasePt, bSuppressScale, bSuppressRotation) Then _rVal = True
            If aTransform.TransformType = dxxTransformationTypes.Rotation And Not bSuppressRotation Then
                If ioCharacterBox.XDirection.RotateAbout(ioCharacterBox.BasePt, aTransform.RotationAxis, aTransform.Angle, aTransform.Radians) Then _rVal = True
                If ioCharacterBox.YDirection.RotateAbout(ioCharacterBox.BasePt, aTransform.RotationAxis, aTransform.Angle, aTransform.Radians) Then _rVal = True
            End If
            Return _rVal
        End Function
        Public Shared Function Apply(aTransform As TTRANSFORM, aEntity As dxfEntity, Optional bSuppressEvnts As Boolean = False) As Boolean
            Dim _rVal As Boolean = False
            If aEntity Is Nothing Then Return False
            'On Error Resume Next
            Dim hp As New TVECTOR(aEntity.HandlePt)
            Dim gType As dxxGraphicTypes = aEntity.GraphicType
            Dim aDefPts As dxpDefPoints = aEntity.DefPts
            Dim aProps As New TPROPERTIES(aEntity.ActiveProperties())
            Dim bDirty As Boolean = aProps.IsDirty Or aDefPts.IsDirty Or aEntity.Components.Paths.Count <= 0
            Dim statwuz As dxxEntityStates = aEntity.State
            Dim aPlane As TPLANE = aDefPts.Plane
            Try
                aEntity.State = dxxEntityStates.Transforming



                Select Case aTransform.TransformType
         '--------------------------------------------------------------------
                    Case dxxTransformationTypes.Translation
                        '--------------------------------------------------------------------

                        _rVal = aTransform.Translate(aEntity)

         '--------------------------------------------------------------------
                    Case dxxTransformationTypes.Rotation
                        '--------------------------------------------------------------------

                        _rVal = aTransform.Rotate(aEntity)
         '--------------------------------------------------------------------
                    Case dxxTransformationTypes.Mirror
                        '--------------------------------------------------------------------
                        _rVal = aTransform.Mirror(aEntity)
         '--------------------------------------------------------------------
                    Case dxxTransformationTypes.Scale
                        '--------------------------------------------------------------------
                        _rVal = aTransform.Scale(aEntity)
                End Select
                'Application.DoEvents()
                Dim perstSubEnts As List(Of dxfEntity) = aEntity.PersistentSubEntities

                If aEntity.GraphicType = dxxGraphicTypes.Leader Then
                    aEntity.ReactorEntity = Nothing
                Else
                    If perstSubEnts IsNot Nothing Then


                        If perstSubEnts.Count > 0 Then
                            For Each pent As dxfEntity In perstSubEnts
                                If Apply(aTransform, pent, True) Then _rVal = True
                            Next

                        End If

                    End If
                End If

                If _rVal And Not bSuppressEvnts Then
                    If Not aTransform.SuppressEvents Then
                        Dim newctr As New TVECTOR(aEntity.HandlePt)
                        Dim evnt As dxfEntityEvent = Nothing
                        evnt = New dxfEntityEvent(aEntity, dxxEntityEventTypes.DefPoints, aEntity.CollectionGUID, aEntity.ImageGUID, aEntity.BlockGUID, aEntity.GUID) With {
                            .PropertyName = "Position",
                            .OldValue = hp.Coordinates,
                            .NewValue = newctr.Coordinates
                            }
                        goEvents.NotifyDependents(aEntity, evnt)
                    End If
                End If
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            Finally

                aEntity.State = statwuz
            End Try

            Select Case gType
                Case dxxGraphicTypes.Insert
                    Dim isert As dxeInsert = DirectCast(aEntity, dxeInsert)
                    If isert.Attributes.Count > 0 Then
                        If Apply(aTransform, isert, isert.Attributes, bSuppressEvnts) Then _rVal = True
                    End If
                    '    Case dxxGraphicTypes.Polygon
                    '        Dim pg As dxePolygon = aEntity
                    '        If pg.AdditionalSegments.Transform(aTransform, True) Then _rVal = True
                    '        If pg.Holes.Transform(aTransform, True) Then _rVal = True
                    '    Case dxxGraphicTypes.Leader
                    '        Dim ldr As dxeLeader = aEntity
                    '        Dim ent As dxfEntity = ldr.ReactorEntity
                    '        If ent  IsNot Nothing Then
                    '            If TTRANSFORM.Apply(aTransform, ent, True) Then _rVal = True
                    '        End If
            End Select
            Return _rVal
        End Function
        Public Shared Function Apply(aTransform As TTRANSFORM, ByRef ioEntity As TENTITY, Optional bSuppressEvnts As Boolean = False) As Boolean
            Dim _rVal As Boolean
            'On Error Resume Next
            If ioEntity.Props.Count <= 0 Then Return False
            ioEntity.Props.SetVal("*State", dxxEntityStates.Transforming)
            Select Case aTransform.TransformType
         '--------------------------------------------------------------------
                Case dxxTransformationTypes.Translation
                    '--------------------------------------------------------------------
                    _rVal = aTransform.Translate(ioEntity)
         '--------------------------------------------------------------------
                Case dxxTransformationTypes.Rotation
                    '--------------------------------------------------------------------
                    _rVal = aTransform.Rotate(ioEntity)
         '--------------------------------------------------------------------
                Case dxxTransformationTypes.Mirror
                    '--------------------------------------------------------------------
                    _rVal = aTransform.Mirror(ioEntity)
         '--------------------------------------------------------------------
                Case dxxTransformationTypes.Scale
                    '--------------------------------------------------------------------
                    _rVal = aTransform.Scale(ioEntity)
            End Select
            'Application.DoEvents()
            ioEntity.Props.SetVal("*State", dxxEntityStates.Steady)
            If _rVal And Not bSuppressEvnts Then
                If Not aTransform.SuppressEvents Then
                    'Dim otype As dxxGraphicTypes
                    Dim aGUID As String
                    If Not String.IsNullOrWhiteSpace(ioEntity.OwnerGUID) Then
                        aGUID = ioEntity.OwnerGUID.Trim
                        '    otype = ioEntity.GraphicType
                        '    If otype <> dxxGraphicTypes.Undefined Then
                        '        Select Case otype
                        '            Case dxxGraphicTypes.Hatch, dxxGraphicTypes.Polygon, dxxGraphicTypes.Leader
                        '                goEvents.DirtyOwner(Nothing, aGUID, ioEntity.Props.Value(-1).ToString())
                        '        End Select
                        '    End If
                    End If
                    'If Not String.IsNullOrWhiteSpace(ioEntity.BlockGUID) Then
                    '    goEvents.DirtyBlock(Nothing, arBlockGUID:=ioEntity.BlockGUID.Trim)
                    'End If
                End If
            End If
            Return _rVal
        End Function
        Public Shared Function Apply(aTransform As TTRANSFORM, ByRef ioVertices As TVERTICES) As TVERTICES
            Dim rChanged As Boolean = False
            Return Apply(aTransform, ioVertices, rChanged)
        End Function
        Public Shared Function Apply(aTransform As TTRANSFORM, ByRef ioVertices As TVERTICES, ByRef rChanged As Boolean) As TVERTICES
            Dim _rVal As TVERTICES = ioVertices
            'On Error Resume Next

            rChanged = False
            If ioVertices.Count = 0 Then Return _rVal
            Dim i As Long
            '**UNUSED VAR** Dim bFlag As Boolean
            For i = 1 To ioVertices.Count
                If TTRANSFORM.Apply(aTransform, ioVertices.Item(i + 1)) Then rChanged = True
            Next i
            Return _rVal
        End Function
        Public Shared Function Apply(aTransform As TTRANSFORM, ByRef ioVectors As TVECTORS) As Boolean
            Dim _rVal As Boolean

            For i As Integer = 1 To ioVectors.Count
                Dim v1 As TVECTOR = ioVectors.Item(i)
                If TTRANSFORM.Apply(aTransform, v1) Then _rVal = True
                ioVectors.SetItem(i, v1)
            Next i
            Return _rVal
        End Function

        Public Shared Function Apply(aTransform As TTRANSFORM, ioVectors As IEnumerable(Of iVector), Optional bSuppressEvents As Boolean = False) As Boolean

            If ioVectors Is Nothing Then Return False
            Dim _rVal As Boolean
            For Each vector As iVector In ioVectors
                Dim v1 As New TVERTEX(vector)
                If TTRANSFORM.Apply(aTransform, v1) Then _rVal = True
                If TypeOf vector Is dxfVector Then
                    Dim dxfv As dxfVector = DirectCast(vector, dxfVector)
                    dxfv.SetStructure(v1, bSuppressEvents)
                Else
                    vector.X = v1.X
                    vector.Y = v1.Y
                    vector.Z = v1.Z
                End If

            Next
            Return _rVal
        End Function
        Public Shared Function Apply(aTransform As TTRANSFORM, aInsert As dxeInsert, aAttribs As dxfAttributes, Optional bSuppressEvents As Boolean = False) As Boolean

            If aAttribs Is Nothing Then Return False
            Dim _rVal As Boolean
            For Each attr As dxfAttribute In aAttribs
                If Apply(aTransform, aInsert, attr, bSuppressEvents) Then _rVal = True
            Next
            Return _rVal
        End Function
        Public Shared Function Apply(aTransform As TTRANSFORM, aInsert As dxeInsert, aAttrib As dxfAttribute, Optional bSuppressEvents As Boolean = False) As Boolean

            If aAttrib Is Nothing Then Return False
            Dim _rVal As Boolean
            Dim prefix As String = String.Empty
            If aInsert IsNot Nothing Then prefix = aInsert.GUID
            Dim attr As dxeText = New dxeText(aAttrib, prefix, True)
            _rVal = Apply(aTransform, attr, bSuppressEvents)
            aAttrib.Properties.CopyVals(attr.Properties, aNamesToSkip:=New List(Of String)({"*TEXTTYPE"}), bSkipHandles:=True, bSkipPointers:=True)
            Return _rVal
        End Function

        Public Shared Function Apply(aTransform As TTRANSFORM, ByRef ioVertex As TVERTEX) As Boolean
            'On Error Resume Next

            Dim vectr As TVECTOR = ioVertex.Vector
            Dim _rVal As Boolean = TTRANSFORM.Apply(aTransform, vectr)
            ioVertex.Vector = vectr

            Select Case aTransform.TransformType
                Case dxxTransformationTypes.Scale
                    If ioVertex.Radius <> 0 Or ioVertex.StartWidth <> 0 Or ioVertex.EndWidth <> 0 Then _rVal = True
                    ioVertex.Radius = Math.Abs(ioVertex.Radius * aTransform.ScaleFactor)
                    ioVertex.StartWidth *= aTransform.ScaleFactor
                    ioVertex.EndWidth *= aTransform.ScaleFactor
                Case dxxTransformationTypes.Mirror
                    ioVertex.Inverted = Not ioVertex.Inverted
                Case dxxTransformationTypes.Rotation
                    If aTransform.AddRotationToMembers Then
                        If aTransform.Radians Then
                            ioVertex.Rotation += aTransform.Angle * 180 / Math.PI
                        Else
                            ioVertex.Rotation += aTransform.Angle
                        End If
                    End If
            End Select
            Return _rVal
        End Function

        Public Shared Function Apply(aTransform As TTRANSFORM, aDefPts As dxpDefPoints, Optional bSuppressScale As Boolean = False, Optional bSuppressRotation As Boolean = False, Optional bSuppressEvents As Boolean = True) As Boolean
            If aDefPts Is Nothing Then Return False
            Dim _rVal As Boolean = False
            Dim dfpntcnt As Integer = aDefPts.DefPtCnt
            If dfpntcnt >= 1 Then
                If TTRANSFORM.Apply(aTransform, aDefPts.Vector1, bSuppressScale:=bSuppressScale, bSuppressRotation:=bSuppressRotation, bSuppressEvents:=bSuppressEvents) Then _rVal = True
            End If
            If dfpntcnt >= 2 Then
                If TTRANSFORM.Apply(aTransform, aDefPts.Vector2, bSuppressScale:=bSuppressScale, bSuppressRotation:=bSuppressRotation, bSuppressEvents:=bSuppressEvents) Then _rVal = True
            End If
            If dfpntcnt >= 3 Then
                If TTRANSFORM.Apply(aTransform, aDefPts.Vector3, bSuppressScale:=bSuppressScale, bSuppressRotation:=bSuppressRotation, bSuppressEvents:=bSuppressEvents) Then _rVal = True
            End If
            If dfpntcnt >= 4 Then
                If TTRANSFORM.Apply(aTransform, aDefPts.Vector4, bSuppressScale:=bSuppressScale, bSuppressRotation:=bSuppressRotation, bSuppressEvents:=bSuppressEvents) Then _rVal = True
            End If
            If dfpntcnt >= 5 Then
                If TTRANSFORM.Apply(aTransform, aDefPts.Vector5, bSuppressScale:=bSuppressScale, bSuppressRotation:=bSuppressRotation, bSuppressEvents:=bSuppressEvents) Then _rVal = True
            End If
            If dfpntcnt >= 6 Then
                If TTRANSFORM.Apply(aTransform, aDefPts.Vector6, bSuppressScale:=bSuppressScale, bSuppressRotation:=bSuppressRotation, bSuppressEvents:=bSuppressEvents) Then _rVal = True
            End If
            If dfpntcnt >= 7 Then
                If TTRANSFORM.Apply(aTransform, aDefPts.Vector7, bSuppressScale:=bSuppressScale, bSuppressRotation:=bSuppressRotation, bSuppressEvents:=bSuppressEvents) Then _rVal = True
            End If
            If aDefPts.HasVertices Then
                If aDefPts.Vertices.Count > 0 Then
                    'aTransform.ScalePlane = aDefPts.Plane
                    If TTRANSFORM.Apply(aTransform, aDefPts.Vertices, bSuppressEvents:=bSuppressEvents) Then _rVal = True
                End If
            End If


            Return _rVal
        End Function

        Public Shared Function Apply(aTransform As TTRANSFORM, aVector As dxfVector, Optional bSuppressScale As Boolean = False, Optional bSuppressRotation As Boolean = False, Optional bSuppressEvents As Boolean = False) As Boolean
            If aVector Is Nothing Then Return False
            Dim vec As New TVECTOR(aVector)
            Dim _rVal As Boolean = Apply(aTransform, vec, bSuppressScale, bSuppressRotation)
            aVector.SetStructure(vec, bSuppressEvents)
            Return _rVal
        End Function
        Public Shared Function Apply(aTransform As TTRANSFORM, ByRef ioVector As TVECTOR, Optional bSuppressScale As Boolean = False, Optional bSuppressRotation As Boolean = False) As Boolean
            Dim _rVal As Boolean
            'On Error Resume Next
            Dim aCS As dxfPlane = Nothing
            Select Case aTransform.TransformType
                Case dxxTransformationTypes.Mirror
                    If ioVector.Mirror(aTransform.MirrorAxis) Then _rVal = True
                Case dxxTransformationTypes.Translation
                    If aTransform.Translation.X <> 0 Or aTransform.Translation.Y <> 0 Or aTransform.Translation.Z <> 0 Then
                        _rVal = True
                        ioVector += aTransform.Translation
                        'ioVector.X += aTransform.Translation.X
                        'ioVector.Y += aTransform.Translation.Y
                        'ioVector.Z += aTransform.Translation.Z
                    End If
                Case dxxTransformationTypes.Rotation
                    If Not bSuppressRotation Then
                        If ioVector.RotateAbout(aTransform.RotationCenter, aTransform.RotationAxis, aTransform.Angle, aTransform.Radians) Then _rVal = True
                    End If
                Case dxxTransformationTypes.Scale
                    If Not bSuppressScale Then
                        If ioVector.Scale(aTransform.ScaleFactor, aTransform.ScaleCenter, aTransform.FactorY, aTransform.FactorZ, New dxfPlane(aTransform.ScalePlane)) Then _rVal = True
                    End If
            End Select
            Return _rVal
        End Function
        Public Shared Function Apply(aTransform As TTRANSFORM, ioPoints As TPOINTS) As TPOINTS
            Dim rChanged As Boolean = False
            Return Apply(aTransform, ioPoints, rChanged)
        End Function
        Public Shared Function Apply(aTransform As TTRANSFORM, ioPoints As TPOINTS, ByRef rChanged As Boolean) As TPOINTS
            Dim _rVal As New TPOINTS()
            Dim bChnged As Boolean

            For i = 1 To ioPoints.Count
                Dim aPt As TPOINT = ioPoints.Item(i)
                aPt = Apply(aTransform, aPt, bChnged)
                If bChnged = True Then rChanged = True
                _rVal.Add(aPt)
            Next
            Return _rVal
        End Function
        Public Shared Function Apply(aTransform As TTRANSFORM, ioPoint As TPOINT) As TPOINT
            Dim v1 As New TVECTOR(ioPoint)
            Dim rChanged As Boolean = TTRANSFORM.Apply(aTransform, v1)
            Return New TPOINT(v1)
        End Function
        Public Shared Function Apply(aTransform As TTRANSFORM, ioPoint As TPOINT, ByRef rChanged As Boolean) As TPOINT
            Dim v1 As New TVECTOR(ioPoint)
            rChanged = TTRANSFORM.Apply(aTransform, v1)
            Return New TPOINT(v1)
        End Function
        Public Shared Function Apply(aTransform As TTRANSFORM, ioArcLine As TSEGMENT, Optional bMirDirs As Boolean = False, Optional bNoDirections As Boolean = False) As TSEGMENT
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim d1 As Double
            Dim d2 As Double
            Dim _rVal As TSEGMENT = ioArcLine
            'If ioArcLine.IsArc Then als_UpdateArcPoints(ioArcLine)
            Select Case aTransform.TransformType
                Case dxxTransformationTypes.Mirror
                    If _rVal.IsArc Then
                        _rVal.ArcStructure.Plane.Mirror(aTransform.MirrorAxis.SPT, aTransform.MirrorAxis.EPT, True, bMirDirs, True)
                        _rVal.ArcStructure.StartPt.Mirror(aTransform.MirrorAxis, True)
                        _rVal.ArcStructure.EndPt.Mirror(aTransform.MirrorAxis, True)
                        If Not bMirDirs Then _rVal.ArcStructure.ClockWise = Not _rVal.ArcStructure.ClockWise
                    Else
                        _rVal.LineStructure.SPT.Mirror(aTransform.MirrorAxis, True)
                        _rVal.LineStructure.EPT.Mirror(aTransform.MirrorAxis, True)
                    End If
                Case dxxTransformationTypes.Translation
                    If _rVal.IsArc Then
                        _rVal.ArcStructure.Plane.Origin += aTransform.Translation
                        '.StartPt += aTransform.Translation
                        '.EndPt += aTransform.Translation
                    Else
                        _rVal.LineStructure.SPT += aTransform.Translation
                        _rVal.LineStructure.EPT += aTransform.Translation
                    End If
                Case dxxTransformationTypes.Rotation
                    If _rVal.IsArc Then
                        _rVal.ArcStructure.Plane.RotateAbout(aTransform.RotationCenter, aTransform.RotationAxis, aTransform.Angle, aTransform.Radians, True, Not bNoDirections, True)
                        _rVal.ArcStructure.StartPt.RotateAbout(aTransform.RotationCenter, aTransform.RotationAxis, aTransform.Angle, aTransform.Radians, True)
                        _rVal.ArcStructure.EndPt.RotateAbout(aTransform.RotationCenter, aTransform.RotationAxis, aTransform.Angle, aTransform.Radians, True)
                    Else
                        _rVal.LineStructure.SPT.RotateAbout(aTransform.RotationCenter, aTransform.RotationAxis, aTransform.Angle, aTransform.Radians, True)
                        _rVal.LineStructure.EPT.RotateAbout(aTransform.RotationCenter, aTransform.RotationAxis, aTransform.Angle, aTransform.Radians, True)
                    End If
                Case dxxTransformationTypes.Scale
                    If _rVal.IsArc Then
                        _rVal.ArcStructure.Plane.Origin.Scale(aTransform.ScaleFactor, aTransform.ScaleCenter, aTransform.FactorY, aTransform.FactorZ)
                        _rVal.ArcStructure.StartWidth *= Math.Abs(aTransform.ScaleFactor)
                        _rVal.ArcStructure.EndWidth *= Math.Abs(aTransform.ScaleFactor)
                        If _rVal.ArcStructure.Elliptical Then
                            v1 = dxfUtils.EllipsePoint(_rVal.ArcStructure.Plane.Origin, _rVal.ArcStructure.Radius * 2, _rVal.ArcStructure.MinorRadius * 2, 0, _rVal.ArcStructure.Plane)
                            v2 = dxfUtils.EllipsePoint(_rVal.ArcStructure.Plane.Origin, _rVal.ArcStructure.Radius * 2, _rVal.ArcStructure.MinorRadius * 2, 90, _rVal.ArcStructure.Plane)
                        Else
                            v1 = _rVal.ArcStructure.Plane.AngleVector(0, _rVal.ArcStructure.Radius, False)
                            v2 = _rVal.ArcStructure.Plane.AngleVector(90, _rVal.ArcStructure.Radius, False)
                        End If
                        v1.Scale(aTransform.ScaleFactor, _rVal.ArcStructure.Plane.Origin, aTransform.FactorY, aTransform.FactorZ)
                        v2.Scale(aTransform.ScaleFactor, _rVal.ArcStructure.Plane.Origin, aTransform.FactorY, aTransform.FactorZ)
                        d1 = _rVal.ArcStructure.Plane.Origin.DistanceTo(v1)
                        d2 = _rVal.ArcStructure.Plane.Origin.DistanceTo(v2)
                        TVALUES.SortTwoValues(True, d1, d2)
                        _rVal.ArcStructure.MinorRadius = d1
                        _rVal.ArcStructure.Radius = d2
                        _rVal.ArcStructure.Elliptical = Math.Round(d1, 5) <> Math.Round(d2, 5)
                    Else
                        _rVal.LineStructure.SPT.Scale(aTransform.ScaleFactor, aTransform.ScaleCenter, aTransform.FactorY, aTransform.FactorZ)
                        _rVal.LineStructure.EPT.Scale(aTransform.ScaleFactor, aTransform.ScaleCenter, aTransform.FactorY, aTransform.FactorZ)
                    End If
            End Select
            Return _rVal
            'If ioArcLine.IsArc Then als_UpdateArcPoints(ioArcLine)
        End Function

        Public Shared Sub Apply(aTransform As TTRANSFORM, ByRef ioComponents As TCOMPONENTS, Optional bNoDirections As Boolean = False)


            Dim segs As TSEGMENTS = ioComponents.Segments
            For i As Integer = 1 To segs.Count
                Dim als As TSEGMENT = segs.Item(i)
                TTRANSFORM.Apply(aTransform, als, True)
                segs.SetItem(i, als)
                ''Application.DoEvents()
            Next i
            ioComponents.Segments = segs

            Dim paths As TPATHS = ioComponents.Paths
            TTRANSFORM.Apply(aTransform, paths, False, False)
            ioComponents.Paths = paths

            Dim bndloops As TBOUNDLOOPS = ioComponents.BoundaryLoops
            For j As Integer = 1 To bndloops.Count
                Dim looop As TBOUNDLOOP = bndloops.Item(j)
                For i As Integer = 1 To segs.Count
                    Dim als As TSEGMENT = segs.Item(i)
                    TTRANSFORM.Apply(aTransform, als, True)
                    segs.SetItem(i, als)
                    ''Application.DoEvents()
                Next i
                looop.Segments = segs
                bndloops.SetItem(j, looop)
                ''Application.DoEvents()
            Next j
            ioComponents.BoundaryLoops = bndloops

        End Sub
        Public Shared Sub Apply(aTransform As TTRANSFORM, ByRef ioArcLines As TSEGMENTS, Optional bNoDirections As Boolean = False)

            If ioArcLines.Count <= 0 Then Return
            Dim als As TSEGMENT
            For i As Integer = 1 To ioArcLines.Count
                als = ioArcLines.Item(i)
                als = TTRANSFORM.Apply(aTransform, als)
                ioArcLines.SetItem(i, als)
                ''Application.DoEvents()
            Next i
        End Sub
        Public Shared Sub Apply(aTransform As TTRANSFORM, ByRef ioLoops As TBOUNDLOOPS, Optional bNoDirections As Boolean = False)

            For i As Integer = 1 To ioLoops.Count
                Dim looop As TBOUNDLOOP = ioLoops.Item(i)
                Dim segs As TSEGMENTS = looop.Segments
                TTRANSFORM.Apply(aTransform, segs, bNoDirections)
                looop.Segments = segs
                ioLoops.SetItem(i, looop)
            Next i
        End Sub
        Public Shared Sub Apply(aTransform As TTRANSFORM, ByRef ioPaths As TPATHS, bMirDirs As Boolean, bNoDirections As Boolean, Optional bConvertToWorld As Boolean = False)
            If ioPaths.Count <= 0 Then Return
            Dim aPath As TPATH
            For i As Integer = 1 To ioPaths.Count
                aPath = ioPaths.Item(i)
                If bConvertToWorld Then aPath.ConvertToWorld()
                TTRANSFORM.Apply(aTransform, aPath, bMirDirs, bNoDirections, False)
                ioPaths.SetItem(i, aPath)
            Next
            TTRANSFORM.Apply(aTransform, ioPaths.ExtentVectors)
            'TTRANSFORM.Apply(aTransform, ioPaths.Bounds, False, bNoDirections)
        End Sub
        Public Shared Sub Apply(aTransform As TTRANSFORM, ByRef ioPath As TPATH, bMirDirs As Boolean, bNoDirections As Boolean, Optional bConvertToWorld As Boolean = False)
            If ioPath.LoopCount <= 0 Then Return
            Dim li As Integer
            Dim vi As Integer
            Dim v1 As TVECTOR
            Dim v2 As TVECTOR
            Dim aLoop As TVECTORS
            If ioPath.Relative And bConvertToWorld Then ioPath.ConvertToWorld()
            Select Case aTransform.TransformType
                '===================================================================
                Case dxxTransformationTypes.Rotation
                    '===================================================================
                    ioPath.Plane.RotateAbout(aTransform.RotationCenter, aTransform.RotationAxis, aTransform.Angle, aTransform.Radians, True, Not bNoDirections, True)
                    If Not ioPath.Relative Then
                        For li = 1 To ioPath.LoopCount
                            aLoop = ioPath.Looop(li)
                            For vi = 1 To aLoop.Count
                                v1 = aLoop.Item(vi)
                                v1.RotateAbout(aTransform.RotationCenter, aTransform.RotationAxis, aTransform.Angle, aTransform.Radians, True)
                                aLoop.SetItem(vi, v1)
                            Next vi
                            ioPath.SetLoop(li, aLoop)
                            'Application.DoEvents()
                        Next li
                    End If
                '===================================================================
                Case dxxTransformationTypes.Translation
                    '===================================================================
                    ioPath.Plane.Origin += aTransform.Translation
                    If Not ioPath.Relative Then
                        For li = 1 To ioPath.LoopCount
                            aLoop = ioPath.Looop(li)
                            aLoop.Translate(aTransform.Translation)
                            ioPath.SetLoop(li, aLoop)
                            'Application.DoEvents()
                        Next li
                    End If
                '===================================================================
                Case dxxTransformationTypes.Scale
                    '===================================================================
                    'v1 = ioPath.Plane.Origin
                    For li = 1 To ioPath.LoopCount
                        aLoop = ioPath.Looop(li)
                        For vi = 1 To aLoop.Count
                            v2 = aLoop.Item(vi)
                            If ioPath.Relative Then
                                v2.X *= aTransform.ScaleFactor
                                v2.Y *= aTransform.FactorY
                                v2.Z *= aTransform.FactorZ
                            Else
                                v2.Scale(aTransform.ScaleFactor, aTransform.ScaleCenter, aTransform.FactorY, aTransform.FactorZ)
                            End If
                            aLoop.SetItem(vi, v2)
                        Next vi
                        ioPath.SetLoop(li, aLoop)
                        'Application.DoEvents()
                    Next li
                    ioPath.Plane.Origin.Scale(aTransform.ScaleFactor, aTransform.ScaleCenter, aTransform.FactorY, aTransform.FactorZ)
                '===================================================================
                Case dxxTransformationTypes.Mirror
                    '===================================================================
                    ioPath.Plane.Mirror(aTransform.MirrorAxis.SPT, aTransform.MirrorAxis.EPT, True, bMirDirs, True)
                    If Not ioPath.Relative Then
                        For li = 1 To ioPath.LoopCount
                            aLoop = ioPath.Looop(li)
                            For vi = 1 To aLoop.Count
                                v2 = aLoop.Item(vi)
                                v2.Mirror(aTransform.MirrorAxis, True)
                                aLoop.SetItem(vi, v2)
                            Next vi
                            ioPath.SetLoop(li, aLoop)
                            'Application.DoEvents()
                        Next li
                        'Application.DoEvents()
                    End If
            End Select
        End Sub
        Public Shared Sub Apply(aTransform As TTRANSFORM, ByRef ioInstance As TINSTANCE, aPlane As TPLANE, Optional bNoDirections As Boolean = False)
            Dim zDir As TVECTOR = aPlane.ZDirection
            If aTransform.TransformType = dxxTransformationTypes.Scale Then
                ioInstance.XOffset *= aTransform.ScaleFactor
                ioInstance.YOffset *= aTransform.ScaleFactor
            ElseIf aTransform.TransformType = dxxTransformationTypes.Rotation Then
                If bNoDirections Then
                    Dim aDir As New TVECTOR(ioInstance.XOffset, ioInstance.YOffset)
                    Dim d1 As Double = Math.Sqrt(ioInstance.XOffset ^ 2 + ioInstance.YOffset ^ 2)
                    aDir.RotateAbout(zDir, aTransform.Angle, False, True)
                    aDir *= d1
                    ioInstance.XOffset = aDir.X
                    ioInstance.YOffset = aDir.Y
                End If
            ElseIf aTransform.TransformType = dxxTransformationTypes.Mirror Then
                Dim mLn As TLINE = aTransform.MirrorAxis
                Dim v1 As TVECTOR = aPlane.Origin.Clone
                Dim v2 As New TVECTOR(aPlane, ioInstance.XOffset, ioInstance.YOffset)
                v1.Mirror(mLn, bSuppressCheck:=True)
                v2.Mirror(mLn, bSuppressCheck:=True)
                v1 = v2 - v1
                ioInstance.XOffset = v1.X
                ioInstance.YOffset = v1.Y
            End If
        End Sub
        Public Shared Sub Apply(aTransform As TTRANSFORM, ByRef ioInstances As TINSTANCES, Optional bNoDirections As Boolean = False)
            TTRANSFORM.Apply(aTransform, ioInstances.Plane, False, bSuppressRotation:=bNoDirections)
            Dim plane As TPLANE = ioInstances.Plane
            If ioInstances.IsParent Then
                TTRANSFORM.Apply(aTransform, ioInstances.ParentPlane, False, bSuppressRotation:=bNoDirections)
                plane = ioInstances.ParentPlane
            End If

            Dim aInst As TINSTANCE
            For i As Integer = 1 To ioInstances.Count
                aInst = ioInstances.Item(i)
                TTRANSFORM.Apply(aTransform, aInst, plane)
                ioInstances.Update(i, aInst)
            Next
        End Sub
        Public Shared Sub Apply(aTransform As TTRANSFORM, aInstances As dxoInstances, Optional bNoDirections As Boolean = False)
            If aInstances Is Nothing Then Return
            Dim plane As TPLANE = aInstances.Plane
            TTRANSFORM.Apply(aTransform, plane, False, bSuppressRotation:=bNoDirections)
            aInstances.Plane = plane


            If aInstances.IsParent Then
                plane = aInstances.ParentPlane
                TTRANSFORM.Apply(aTransform, plane, False, bSuppressRotation:=bNoDirections)
                aInstances.ParentPlane = plane

            End If

            For Each inst As dxoInstance In aInstances
                TTRANSFORM.Apply(aTransform, inst, plane)
            Next
        End Sub

        Public Shared Sub Apply(aTransform As TTRANSFORM, aInstance As dxoInstance, aPlane As TPLANE, Optional bNoDirections As Boolean = False)
            If aInstance Is Nothing Then Return
            Dim zDir As TVECTOR = aPlane.ZDirection
            If aTransform.TransformType = dxxTransformationTypes.Scale Then
                aInstance.XOffset *= aTransform.ScaleFactor
                aInstance.YOffset *= aTransform.ScaleFactor
            ElseIf aTransform.TransformType = dxxTransformationTypes.Rotation Then
                If bNoDirections Then
                    Dim aDir As New TVECTOR(aInstance.XOffset, aInstance.YOffset)
                    Dim d1 As Double = Math.Sqrt(aInstance.XOffset ^ 2 + aInstance.YOffset ^ 2)
                    aDir.RotateAbout(zDir, aTransform.Angle, False, True)
                    aDir *= d1
                    aInstance.XOffset = aDir.X
                    aInstance.YOffset = aDir.Y
                End If
            ElseIf aTransform.TransformType = dxxTransformationTypes.Mirror Then
                Dim mLn As TLINE = aTransform.MirrorAxis
                Dim v1 As TVECTOR = aPlane.Origin.Clone
                Dim v2 As New TVECTOR(aPlane, aInstance.XOffset, aInstance.YOffset)
                v1.Mirror(mLn, bSuppressCheck:=True)
                v2.Mirror(mLn, bSuppressCheck:=True)
                v1 = v2 - v1
                aInstance.XOffset = v1.X
                aInstance.YOffset = v1.Y
            End If
        End Sub
#End Region 'Shared Methods
    End Structure 'TTRANSFORM
    Friend Structure TTRANSFORMS
        Implements ICloneable
#Region "Members"
        Private _Init As Boolean
        Private _Members() As TTRANSFORM
#End Region 'Members
#Region "Constructors"
        Public Sub New(aTransform As TTRANSFORM)
            'init ------------------------------------------
            _Init = True
            ReDim _Members(-1)
            Name = String.Empty
            'init ------------------------------------------
            Add(aTransform)
        End Sub
        Public Sub New(aTransforms As TTRANSFORMS)
            'init ------------------------------------------
            _Init = True
            ReDim _Members(-1)
            Name = String.Empty
            'init ------------------------------------------
            For i As Integer = 1 To aTransforms.Count
                Add(New TTRANSFORM(aTransforms.Item(i)))
            Next
        End Sub

        Public Sub New(aTransforms As List(Of TTRANSFORM))
            'init ------------------------------------------
            _Init = True
            ReDim _Members(-1)
            Name = String.Empty
            'init ------------------------------------------
            For Each aTr As TTRANSFORM In aTransforms
                Add(aTr)
            Next
        End Sub
        Public Sub New(aName As String)
            'init ------------------------------------------
            _Init = True
            ReDim _Members(-1)
            Name = String.Empty
            'init ------------------------------------------
            Name = aName
        End Sub

#End Region 'Constructors
#Region "Properties"

        Public Property Name As String

        Public ReadOnly Property Count As Integer
            Get
                If Not _Init Then
                    _Init = True
                    ReDim _Members(-1)
                End If
                Return _Members.Count
            End Get
        End Property
#End Region 'Properties
#Region "Methods"

        Public Overrides Function ToString() As String
            Return $"TTRANSFORMS[{Count}]"
        End Function

        Public Function Clone() As TTRANSFORMS
            Return New TTRANSFORMS(Me)
        End Function
        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TTRANSFORMS(Me)
        End Function
        Public Sub Clear()
            _Init = True
            ReDim _Members(-1)
        End Sub
        Public Function Item(aIndex As Integer) As TTRANSFORM
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return New TTRANSFORM(dxxTransformationTypes.Undefined)
            End If
            Return _Members(aIndex - 1)
        End Function

        Public Function Item(aTransformType As dxxTransformationTypes, Optional aOccur As Integer = 1) As TTRANSFORM
            Dim _rVal As New TTRANSFORM(dxxTransformationTypes.Undefined)
            Dim cnt As Integer = 0
            For i As Integer = 1 To Count
                If _Members(i - 1).TransformType = aTransformType Then
                    cnt += 1
                    If (aOccur <= 1 Or cnt = aOccur) Then
                        _rVal = _Members(i - 1)
                        Exit For
                    End If

                End If
            Next i
            Return _rVal
        End Function

        Public Sub Update(aIndex As Integer, aTransform As TTRANSFORM)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1) = aTransform
        End Sub
        Public Sub SetEventSuppression(aIndex As Integer, aSupressVal As Boolean)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1).SuppressEvents = aSupressVal
        End Sub
        Public Sub Add(aTransform As TTRANSFORM)
            If Count + 1 > Integer.MaxValue Then Exit Sub
            System.Array.Resize(_Members, Count + 1)
            _Members(_Members.Count - 1) = aTransform
        End Sub
#End Region 'Methods
#Region "Shared Methods"

        Public Shared ReadOnly Property Null As TTRANSFORMS
            Get
                Return New TTRANSFORMS("")
            End Get
        End Property
        Public Shared Function Invert(aTransforms As TTRANSFORMS) As TTRANSFORMS
            Dim rTs As New TTRANSFORMS
            Dim i As Integer
            Dim aTr As TTRANSFORM
            For i = 1 To aTransforms.Count
                aTr = Force.DeepCloner.DeepClonerExtensions.DeepClone(Of TTRANSFORM)(aTransforms.Item(i))
                aTr.Invert()
                rTs.Add(aTr)
            Next i
            Return rTs
        End Function
        Public Shared Function Apply(aTransforms As TTRANSFORMS, ByRef ioPlane As TPLANE, Optional bSuppressScale As Boolean = False, Optional bSuppressRotation As Boolean = False) As Boolean
            Dim _rVal As Boolean
            Dim i As Integer
            For i = 1 To aTransforms.Count
                If TTRANSFORM.Apply(aTransforms.Item(i), ioPlane.Origin, bSuppressScale, bSuppressRotation) Then _rVal = True
                If aTransforms.Item(i).TransformType = dxxTransformationTypes.Rotation And Not bSuppressRotation Then
                    If ioPlane.RotateAbout(ioPlane.Origin, aTransforms.Item(i).RotationAxis, aTransforms.Item(i).Angle, aTransforms.Item(i).Radians) Then _rVal = True
                End If
            Next i
            Return _rVal
        End Function
        Public Shared Function Apply(aTransforms As TTRANSFORMS, ByRef ioVector As TVECTOR) As Boolean
            Dim _rVal As Boolean
            Dim i As Integer
            For i = 1 To aTransforms.Count
                If TTRANSFORM.Apply(aTransforms.Item(i), ioVector) Then _rVal = True
            Next i
            Return _rVal
        End Function
        Public Shared Sub Apply(aTransforms As TTRANSFORMS, ByRef ioPath As TPATH, bMirDirs As Boolean, bNoDirections As Boolean, Optional bConvertToWorld As Boolean = False)
            If aTransforms.Count <= 0 Then Return
            For i As Integer = 1 To aTransforms.Count
                TTRANSFORM.Apply(aTransforms.Item(i), ioPath, bMirDirs, bNoDirections, bConvertToWorld)
            Next i
        End Sub
        Public Shared Sub Apply(aTransforms As TTRANSFORMS, ByRef ioPaths As TPATHS, bMirDirs As Boolean, bNoDirections As Boolean, Optional bConvertToWorld As Boolean = False)
            If aTransforms.Count <= 0 Then Return
            Dim i As Integer
            Dim j As Integer
            Dim aTr As TTRANSFORM
            Dim aPth As TPATH
            For i = 1 To aTransforms.Count
                aTr = aTransforms.Item(i)
                For j = 1 To ioPaths.Count
                    aPth = ioPaths.Item(j)
                    If bConvertToWorld Then aPth.ConvertToWorld()
                    TTRANSFORM.Apply(aTr, aPth, bMirDirs, bNoDirections, False)
                    ioPaths.SetItem(j, aPth)
                Next j
            Next i
        End Sub
        Public Shared Function Apply(aTransforms As TTRANSFORMS, ByRef ioVertex As TVERTEX) As Boolean
            Dim _rVal As Boolean
            If aTransforms.Count <= 0 Then Return _rVal
            Dim i As Integer
            For i = 1 To aTransforms.Count
                If TTRANSFORM.Apply(aTransforms.Item(i), ioVertex) Then _rVal = True
            Next i
            Return _rVal
        End Function
        Public Shared Function Apply(aTransforms As TTRANSFORMS, ByRef ioVertices As TVERTICES) As Boolean
            Dim _rVal As Boolean
            If aTransforms.Count <= 0 Then Return _rVal
            Dim i As Integer
            Dim j As Integer
            Dim vrt As TVERTEX
            For j = 1 To ioVertices.Count
                vrt = ioVertices.Item(j)
                For i = 1 To aTransforms.Count
                    If TTRANSFORM.Apply(aTransforms.Item(i), vrt) Then _rVal = True
                Next i
                ioVertices.SetItem(j, vrt)
            Next j
            Return _rVal
        End Function
        Public Shared Sub Apply(aTransforms As TTRANSFORMS, ByRef ioArcLines As TSEGMENTS, Optional bNoDirections As Boolean = False)
            If ioArcLines.Count <= 0 Or aTransforms.Count <= 0 Then Return
            Dim als As TSEGMENT
            For i As Integer = 1 To aTransforms.Count
                For j As Integer = 1 To ioArcLines.Count
                    als = ioArcLines.Item(i)
                    TTRANSFORM.Apply(aTransforms.Item(i), als)
                    ioArcLines.SetItem(i, als)
                Next j
            Next i
        End Sub
        Public Shared Function Apply(aTransforms As TTRANSFORMS, ByRef ioVectors As TVECTORS) As Boolean
            Dim _rVal As Boolean
            If aTransforms.Count <= 0 Then Return False
            Dim i As Integer
            Dim j As Integer
            Dim v1 As TVECTOR
            For j = 1 To ioVectors.Count
                v1 = ioVectors.Item(j)
                For i = 1 To aTransforms.Count
                    If TTRANSFORM.Apply(aTransforms.Item(i), v1) Then _rVal = True
                Next i
                ioVectors.SetItem(j, v1)
            Next j
            Return _rVal
        End Function
        Public Shared Function Apply(aTransforms As TTRANSFORMS, aVectors As colDXFVectors) As Boolean
            Dim _rVal As Boolean
            If aVectors Is Nothing Then Return False
            If aTransforms.Count <= 0 Then Return False
            Dim v1 As dxfVector
            For j As Integer = 1 To aVectors.Count
                v1 = aVectors.Item(j)
                For i As Integer = 1 To aTransforms.Count
                    If TTRANSFORM.Apply(aTransforms.Item(i), v1) Then _rVal = True
                Next i
            Next j
            Return _rVal
        End Function
        Public Shared Sub Apply(aTransforms As TTRANSFORMS, ByRef ioInstances As TINSTANCES, Optional bNoDirections As Boolean = False)
            If aTransforms.Count <= 0 Or ioInstances.Count <= 0 Then Return
            Dim aTr As TTRANSFORM
            Dim zDir As TVECTOR
            Dim aPln As TPLANE
            Dim aInst As TINSTANCE
            TTRANSFORMS.Apply(aTransforms, ioInstances.Plane, False, bNoDirections)
            aPln = ioInstances.Plane
            If ioInstances.IsParent Then
                TTRANSFORMS.Apply(aTransforms, ioInstances.ParentPlane, False, bNoDirections)
                aPln = ioInstances.ParentPlane
            End If
            zDir = aPln.ZDirection.Normalized

            For i As Integer = 1 To ioInstances.Count
                aInst = ioInstances.Item(i)
                For j As Integer = 1 To aTransforms.Count
                    aTr = aTransforms.Item(j)
                    TTRANSFORM.Apply(aTr, aInst, aPln)
                Next j
                ioInstances.Update(i, aInst)
            Next i
        End Sub
        Public Shared Sub Apply(aTransforms As TTRANSFORMS, ByRef ioArcLines As TSEGMENT, Optional bMirDirs As Boolean = False, Optional bNoDirections As Boolean = False)
            If aTransforms.Count <= 0 Then Return
            Dim i As Integer
            For i = 1 To aTransforms.Count
                TTRANSFORM.Apply(aTransforms.Item(i), ioArcLines, bMirDirs, bNoDirections)
                'Application.DoEvents()
            Next i
        End Sub
        Public Shared Function Apply(aTransforms As TTRANSFORMS, aEntity As dxfEntity, Optional bSuppressEvnts As Boolean = False) As Boolean
            Dim _rVal As Boolean
            'On Error Resume Next
            If aEntity Is Nothing Or aTransforms.Count <= 0 Then Return False


            For i As Integer = 1 To aTransforms.Count
                Dim aTransform As TTRANSFORM = aTransforms.Item(i)
                If bSuppressEvnts Then aTransform.SuppressEvents = True
                If TTRANSFORM.Apply(aTransform, aEntity, bSuppressEvnts) Then _rVal = True
            Next i
            Return _rVal
        End Function
        Public Shared Function Apply(aTransforms As TTRANSFORMS, ByRef ioEntities As colDXFEntities, Optional bReturnClone As Boolean = False, Optional bSuppressEvnts As Boolean = False) As Boolean
            Dim _rVal As Boolean
            'On Error Resume Next
            If ioEntities Is Nothing Or aTransforms.Count <= 0 Then Return _rVal

            Dim bEntities As colDXFEntities = IIf(bReturnClone, New colDXFEntities(), Nothing)

            For i As Integer = 1 To ioEntities.Count
                Dim iEnt As dxfEntity = ioEntities.Item(i, bReturnClone:=bReturnClone)

                For j As Integer = 1 To aTransforms.Count
                    If TTRANSFORM.Apply(aTransforms.Item(j), iEnt, bSuppressEvnts) Then _rVal = True
                Next j
                If bReturnClone Then bEntities.AddToCollection(iEnt, bSuppressEvnts:=True)
            Next i
            If bReturnClone Then ioEntities.SwapCollection(bEntities)
            Return _rVal
        End Function
        Public Shared Function Apply(aTransforms As TTRANSFORMS, ByRef ioEntities As List(Of dxfEntity), Optional bReturnClone As Boolean = False, Optional bSuppressEvnts As Boolean = False) As Boolean
            Dim _rVal As Boolean
            'On Error Resume Next
            If ioEntities Is Nothing Or aTransforms.Count <= 0 Then Return _rVal
            Dim bEntities As List(Of dxfEntity) = IIf(bReturnClone, New List(Of dxfEntity), Nothing)

            For i As Integer = 1 To ioEntities.Count
                Dim iEnt As dxfEntity = ioEntities(i - 1)
                If iEnt Is Nothing Then Continue For
                If bReturnClone Then iEnt = iEnt.Clone

                For j As Integer = 1 To aTransforms.Count
                    If TTRANSFORM.Apply(aTransforms.Item(j), iEnt, bSuppressEvnts) Then _rVal = True
                Next j
                If bReturnClone Then bEntities.Add(iEnt)
            Next i
            If bReturnClone Then ioEntities = bEntities
            Return _rVal
        End Function
        Public Shared Function CreateRotateToPlane(aFromPlane As TPLANE, aToPane As TPLANE, Optional aRotation As Double = 0.0, Optional bIncludeTranslationToPlane As Boolean = False) As TTRANSFORMS
            Dim _rVal As New TTRANSFORMS
            Dim bPl As New TPLANE("")
            Dim aX1 As TVECTOR
            Dim ang1 As Double
            Dim aX2 As TVECTOR
            Dim ang2 As Double
            bPl = aToPane
            If aRotation <> 0 Then bPl.Revolve(aRotation, False)
            'get the axis and the angles
            bPl = aFromPlane.RotatedTo(bPl, aX1, ang1, aX2, ang2)
            If ang1 <> 0 Then
                _rVal.Add(TTRANSFORM.CreateRotation(aFromPlane.Origin, ang1, False, aX1, True))
            End If
            If ang2 <> 0 Then
                _rVal.Add(TTRANSFORM.CreateRotation(aFromPlane.Origin, ang2, False, aX2, True))
            End If
            If bIncludeTranslationToPlane Then
                _rVal.Add(TTRANSFORM.CreateTranslation(aToPane.Origin - aFromPlane.Origin))
            End If
            Return _rVal
        End Function
#End Region 'Shared Methods
    End Structure 'TTRANSFORMS

End Namespace
