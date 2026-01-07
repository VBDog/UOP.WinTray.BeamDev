Imports System.Windows.Controls
Imports UOP.DXFGraphics.dxfGlobals
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities
Imports Vanara.PInvoke
Imports Vanara.PInvoke.Kernel32


Namespace UOP.DXFGraphics
    Public Class dxsLinetype
        Public Property Name As String
        Public Property LayerName As String
        Public Property LayerColor As dxxColors
        Public Property LayerLineWeight As dxxLineWeights

        Friend Sub New()
            Name = String.Empty
            LayerName = String.Empty
            LayerColor = dxxColors.BlackWhite
            LayerLineWeight = dxxLineWeights.ByDefault
        End Sub
        Friend Sub New(aName As String, aLayerName As String, aLayerColor As dxxColors, aLayerLineWeight As dxxLineWeights)
            If String.IsNullOrWhiteSpace(aName) Then Name = String.Empty Else Name = aName.Trim()
            If String.IsNullOrWhiteSpace(aLayerName) Then LayerName = String.Empty Else LayerName = aLayerName.Trim()
            If aLayerColor.IsLogical() Then LayerColor = dxxColors.BlackWhite Else LayerColor = aLayerColor
            If aLayerLineWeight.IsLogical Then LayerLineWeight = dxxLineWeights.ByDefault Else LayerLineWeight = aLayerLineWeight
        End Sub
    End Class


    Public Class dxsLinetypes
        Inherits dxfSettingObject
        Implements IEnumerable(Of dxsLinetype)
#Region "Fields"
        Private _Members As List(Of dxsLinetype)
#End Region 'Fields


#Region "Constructors"


        Friend Sub New()
            MyBase.New(dxxSettingTypes.LINETYPESETTINGS)
            _Members = New List(Of dxsLinetype)
        End Sub
        Friend Sub New(aImage As dxfImage)
            MyBase.New(dxxSettingTypes.LINETYPESETTINGS, aImage)
            _Members = New List(Of dxsLinetype)
        End Sub

#End Region 'Constructors

#Region "Properties"
        Public Property Setting As dxxLinetypeLayerFlag
            Get
                Return DirectCast(Properties.ValueI("Setting", aDefault:=dxxLinetypeLayerFlag.Suppressed), dxxLinetypeLayerFlag)
            End Get
            Set(value As dxxLinetypeLayerFlag)
                If value = dxxLinetypeLayerFlag.Undefined Then Return

                Dim lval As dxxLinetypeLayerFlag = Setting
                If Properties.SetVal("Setting", value) Then
                    Notify(New dxoProperty(1, value, $"Setting", dxxPropertyTypes.dxf_Integer, aLastVal:=lval, bNonDXF:=True))
                End If

            End Set
        End Property

        Public ReadOnly Property Count As Integer
            Get
                If _Members Is Nothing Then Return 0 Else Return _Members.Count
            End Get
        End Property

        Public ReadOnly Property Names As List(Of String)
            Get
                Dim _rVal As New List(Of String)
                For Each mem In Me
                    _rVal.Add(mem.Name)
                Next

                Return _rVal
            End Get
        End Property
#End Region 'Properties

#Region "Methods"

        Public Function LineColor(aLinetypeName As String, Optional aDefaultReturn As dxxColors? = Nothing) As dxxColors
            Dim mem As dxsLinetype = Member(aLinetypeName)
            If mem Is Nothing Then
                If aDefaultReturn.HasValue Then Return aDefaultReturn.Value Else Return dxxColors.Undefined
            Else
                Return mem.LayerColor
            End If


        End Function
        Public Function LineWeight(aLinetypeName As String, Optional aDefaultReturn As dxxLineWeights? = Nothing) As dxxLineWeights
            Dim mem As dxsLinetype = Member(aLinetypeName)
            If mem Is Nothing Then
                If aDefaultReturn.HasValue Then Return aDefaultReturn.Value Else Return dxxLineWeights.Undefined
            Else
                Return mem.LayerLineWeight
            End If
        End Function

        Public Function LineLayer(aLinetypeName As String, Optional aDefaultReturn As String = Nothing) As String
            Dim mem As dxsLinetype = Member(aLinetypeName)
            If mem Is Nothing Then
                If Not String.IsNullOrWhiteSpace(aDefaultReturn) Then Return aDefaultReturn.Trim Else Return String.Empty
            Else
                Return mem.LayerName
            End If
        End Function
        Friend Function TryGet(aLinetypeName As String, ByRef rMember As dxsLinetype) As Boolean
            rMember = Member(aLinetypeName)
            Return rMember IsNot Nothing
        End Function

        Friend Function Apply(aSetting As dxxLinetypeLayerFlag, aImage As dxfImage, aLineType As String, ByRef rLayer As String, ByRef rColor As dxxColors) As Boolean

            If String.IsNullOrWhiteSpace(aLineType) Then Return False
            aLineType = aLineType.Trim()
            If dxoLinetype.LTIsLogical(aLineType) Then Return False


            'make sure we have a setting for the linetype
            Dim mem As dxsLinetype = Nothing
            If Not TryGet(aLineType, mem) Or Not GetImage(aImage) Then Return False

            Dim _rVal As Boolean
            If aSetting < 0 Or aSetting > 2 Then aSetting = Setting
            If aSetting = dxxLinetypeLayerFlag.Suppressed Or Count = 0 Then Return False

            Dim aLt As dxoLinetype = Nothing
            aLineType = mem.Name
            mem.Name = aImage.GetOrAdd(dxxReferenceTypes.LTYPE, mem.Name, rEntry:=aLt)
            Dim memlayer As dxoLayer = MemberLayer(mem.Name, aSetting = dxxLinetypeLayerFlag.ForceToLayer, aImage:=aImage)

            mem.LayerName = aImage.GetOrAdd(dxxReferenceTypes.LAYER, mem.LayerName, aColor:=mem.LayerColor, aLineType:=mem.Name)

            If aSetting = dxxLinetypeLayerFlag.ForceToLayer Then
                rColor = dxxColors.ByLayer
                rLayer = mem.LayerName
            ElseIf aSetting = dxxLinetypeLayerFlag.ForceToColor Then
                rColor = mem.LayerColor
            End If
            Return _rVal
        End Function

        Public Function ApplyTo(aEntities As IEnumerable(Of dxfEntity), Optional aSetting As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined, Optional aImage As dxfImage = Nothing) As Boolean

            If aEntities Is Nothing Then Return False
            If aEntities.Count <= 0 Or Count <= 0 Then Return False
            If Not GetImage(aImage) Then Return False
            If aSetting < 0 Or aSetting > 1 Then aSetting = Setting
            If aSetting = dxxLinetypeLayerFlag.Suppressed Then Return False
            Dim _rVal As Boolean = False
            Dim ltypes As New List(Of String)()
            Dim passedents As List(Of dxfEntity) = aEntities.ToList()
            For Each mem In Me
                Dim ltype As String = mem.Name
                Dim ents As List(Of dxfEntity) = passedents.FindAll(Function(x) String.Compare(x.Linetype, ltype, True) = 0)
                For Each ent As dxfEntity In ents
                    If ApplyTo(ent, aSetting, aImage) Then _rVal = True
                Next
            Next

            Return _rVal

        End Function
        Public Function ApplyTo(aEntity As dxfEntity, Optional aSetting As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined, Optional aImage As dxfImage = Nothing) As Boolean

            If aEntity Is Nothing Or Count = 0 Then Return False
            If Not GetImage(aImage) Then Return False

            If aSetting < 0 Or aSetting > 1 Then aSetting = Setting
            If aSetting = dxxLinetypeLayerFlag.Suppressed Then Return False

            Dim dsp As TDISPLAYVARS = aEntity.DisplayStructure
            Dim _rVal As Boolean = ApplyTo(dsp, aSetting, aImage)
            If _rVal Then aEntity.DisplayStructure = dsp

            If aEntity.GraphicType = dxxGraphicTypes.Polygon Then
                Dim aPGon As dxePolygon = DirectCast(aEntity, dxePolygon)
                If ApplyTo(aPGon.Vertices, aPGon.Linetype, aSetting, aImage) Then
                    _rVal = True
                    aPGon.IsDirty = True
                End If
                If ApplyTo(aPGon.AdditionalSegments, aSetting, aImage) Then
                    _rVal = True
                    If Not aPGon.SuppressAdditionalSegments Then
                        aPGon.IsDirty = True
                    End If
                End If
            End If
            Return _rVal
        End Function

        Public Function ApplyTo(aVertices As IEnumerable(Of dxfVector), Optional aDefaultLinetype As String = "", Optional aSetting As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined, Optional aImage As dxfImage = Nothing) As Boolean
            Dim _rVal As Boolean
            If aVertices Is Nothing Then Return _rVal
            If aVertices.Count <= 0 Or Count <= 0 Then Return _rVal
            Dim lname As String
            Dim lclr As dxxColors
            Dim aLineType As String

            Dim j As Integer
            Dim bSetting As dxxLinetypeLayerFlag
            Dim alt As String
            aDefaultLinetype = Trim(aDefaultLinetype)
            If aDefaultLinetype = "" Then aDefaultLinetype = dxfLinetypes.ByLayer
            If Not GetImage(aImage) Then Return _rVal
            If aSetting < 0 Or aSetting > 1 Then aSetting = Setting
            If aSetting = dxxLinetypeLayerFlag.Suppressed Then Return _rVal
            For j = 1 To Count
                Dim mem As dxsLinetype = _Members(j - 1)
                alt = mem.Name
                If String.IsNullOrWhiteSpace(alt) Then Continue For

                lclr = mem.LayerColor
                lname = mem.LayerName
                bSetting = aSetting
                If bSetting = dxxLinetypeLayerFlag.ForceToLayer And lname = "" Then bSetting = dxxLinetypeLayerFlag.ForceToColor
                'add or update the layer in the image layer table
                If bSetting = dxxLinetypeLayerFlag.ForceToLayer Then
                    lname = aImage.GetOrAdd(dxxReferenceTypes.LAYER, lname, aColor:=lclr, aLineType:=alt)
                End If
                bSetting = aSetting
                If bSetting = dxxLinetypeLayerFlag.ForceToLayer And lname = "" Then bSetting = dxxLinetypeLayerFlag.ForceToColor
                If bSetting = dxxLinetypeLayerFlag.ForceToLayer Then
                    lname = aImage.GetOrAdd(dxxReferenceTypes.LAYER, lname, aColor:=lclr, aLineType:=alt)
                End If
                For Each v1 As dxfVector In aVertices
                    If v1 Is Nothing Then Continue For
                    aLineType = v1.Linetype
                    If String.IsNullOrWhiteSpace(aLineType) Then aLineType = aDefaultLinetype Else aLineType = aLineType.Trim()
                    If (String.Compare(aLineType, dxfLinetypes.ByBlock, True) <> 0) And (String.Compare(aLineType, dxfLinetypes.ByLayer, True) <> 0) Then
                        If String.Compare(aLineType, alt, True) = 0 Then
                            If bSetting = dxxLinetypeLayerFlag.ForceToLayer Then
                                lname = aImage.GetOrAdd(dxxReferenceTypes.LAYER, lname, aColor:=lclr, aLineType:=mem.Name)
                                If String.Compare(v1.LayerName, lname) <> 0 Then _rVal = True
                                If String.Compare(v1.Linetype, dxfLinetypes.ByLayer) <> 0 Then _rVal = True
                                If v1.Color <> dxxColors.ByLayer Then _rVal = True
                                v1.LayerName = lname
                                v1.Linetype = dxfLinetypes.ByLayer
                                v1.Color = dxxColors.ByLayer
                            ElseIf aSetting = dxxLinetypeLayerFlag.ForceToColor Then
                                If v1.Color <> lclr Then _rVal = True
                                v1.Color = lclr
                            End If
                        End If
                    End If
                Next

            Next j
            Return _rVal
        End Function

        Friend Sub ApplyTo(aPaths As TPATHS, aImage As dxfImage, Optional aSetting As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined)
            If aPaths.Count <= 0 Or Count <= 0 Then Return
            If Not GetImage(aImage) Then Return
            Dim lname As String
            Dim lclr As dxxColors
            Dim aLineType As String

            Dim bSetting As dxxLinetypeLayerFlag
            Dim pnames As List(Of String) = Names

            If aSetting < 0 Or aSetting > 1 Then aSetting = Setting
            If aSetting = dxxLinetypeLayerFlag.Suppressed Then Return
            For i As Integer = 1 To aPaths.Count
                Dim aPth As TPATH = aPaths.Item(i)
                aLineType = aPth.Linetype
                If aLineType = "" Then aLineType = dxfLinetypes.ByLayer
                If (String.Compare(aLineType, dxfLinetypes.ByBlock, True) <> 0) And (String.Compare(aLineType, dxfLinetypes.ByLayer, True) <> 0) Then
                    Dim pidx As Integer = pnames.FindIndex(Function(x) String.Compare(x, aLineType, True) = 0)
                    If pidx >= 0 Then
                        Dim mem As dxsLinetype = _Members(pidx)

                        If mem.Name <> "" Then
                            lclr = mem.LayerColor
                            lname = mem.LayerName
                            bSetting = aSetting
                            If lname = "" And bSetting = dxxLinetypeLayerFlag.ForceToLayer Then bSetting = dxxLinetypeLayerFlag.ForceToColor
                            If bSetting = dxxLinetypeLayerFlag.ForceToLayer Then
                                lname = aImage.GetOrAdd(dxxReferenceTypes.LAYER, lname, aColor:=lclr, aLineType:=mem.Name)
                                aPth.LayerName = lname
                                aPth.Linetype = dxfLinetypes.ByLayer
                                aPth.Color = dxxColors.ByLayer
                            ElseIf aSetting = dxxLinetypeLayerFlag.ForceToColor Then
                                aPth.Color = lclr
                            End If
                            aPaths.SetItem(i, aPth)
                        End If
                    End If
                End If
            Next i
        End Sub

        Public Function ApplyTo(aBlock As dxfBlock, Optional aSetting As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined, Optional aImage As dxfImage = Nothing) As Boolean

            If aBlock Is Nothing Then Return False
            If aBlock.Entities.Count <= 0 Or Count <= 0 Then Return False
            If Not GetImage(aImage) Then Return False
            If aSetting < 0 Or aSetting > 1 Then aSetting = Setting
            If aSetting = dxxLinetypeLayerFlag.Suppressed Then Return False

            Return ApplyTo(aBlock.Entities, aSetting, aImage)

        End Function

        Friend Function ApplyTo(ByRef ioSettings As TDISPLAYVARS, Optional aSetting As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined, Optional aImage As dxfImage = Nothing) As Boolean

            If Count = 0 Then Return False
            If Not GetImage(aImage) Then Return False
            If aSetting < 0 Or aSetting > 1 Then aSetting = Setting
            If aSetting = dxxLinetypeLayerFlag.Suppressed Then Return False
            Dim mem As dxsLinetype = Nothing

            If Not TryGet(ioSettings.Linetype, mem) Then Return False
            Dim memlayer As dxoLayer = MemberLayer(mem.Name, True, aImage)
            If memlayer Is Nothing Then Return False


            Dim aLayername As String = ioSettings.LayerName
            Dim lname As String = aLayername
            If String.IsNullOrWhiteSpace(lname) Then lname = dxfLinetypes.ByLayer
            Dim _rVal As Boolean

            If aSetting = dxxLinetypeLayerFlag.ForceToLayer Then
                If String.Compare(ioSettings.LayerName, memlayer.Name, True) <> 0 Then _rVal = True
                If String.Compare(ioSettings.Linetype, dxfLinetypes.ByLayer, True) <> 0 Then _rVal = True
                If ioSettings.Color <> dxxColors.ByLayer Then _rVal = True
                ioSettings.LayerName = memlayer.Name
                ioSettings.Linetype = dxfLinetypes.ByLayer
                ioSettings.Color = dxxColors.ByLayer
            ElseIf aSetting = dxxLinetypeLayerFlag.ForceToColor Then
                If ioSettings.Color <> memlayer.Color Then _rVal = True
                ioSettings.Color = memlayer.Color

            End If

            Return _rVal
        End Function

        Public Function ApplyTo(aSettings As dxfDisplaySettings, Optional aSetting As dxxLinetypeLayerFlag = dxxLinetypeLayerFlag.Undefined, Optional aImage As dxfImage = Nothing) As Boolean
            If aSettings Is Nothing Or Count = 0 Then Return False
            If Not GetImage(aImage) Then Return False
            If aSetting < 0 Or aSetting > 1 Then aSetting = Setting
            If aSetting = dxxLinetypeLayerFlag.Suppressed Then Return False

            Dim struc As TDISPLAYVARS = aSettings.Strukture
            Dim _rVal As Boolean = ApplyTo(struc, aImage:=aImage)
            aSettings.Strukture = struc
            Return _rVal


        End Function

        Friend Sub Add(aMember As dxsLinetype)
            If _Members Is Nothing Then _Members = New List(Of dxsLinetype)

            If aMember Is Nothing Then Return
            If String.IsNullOrWhiteSpace(aMember.Name) Then Return
            If String.IsNullOrWhiteSpace(aMember.LayerName) Then aMember.LayerName = aMember.Name.ToUpper()
            If _Members.IndexOf(aMember) >= 0 Then Return
            If _Members.FindIndex(Function(x) String.Compare(x.Name, aMember.Name) = 0) >= 0 Then Return

            If aMember.LayerColor.IsLogical Then aMember.LayerColor = dxxColors.BlackWhite
            If aMember.LayerLineWeight.IsLogical Then aMember.LayerLineWeight = dxxLineWeights.ByDefault

            Dim lt As dxoLinetype = dxfLinetypes.DefaultDef(aMember.Name)
            If lt Is Nothing Then Return
            If lt.IsLogical Then Return
            _Members.Add(aMember)

            Notify(New dxoProperty(40, Count, "Count", dxxPropertyTypes.dxf_Integer, Count - 1))

        End Sub
        Public Sub Add(aLineTypeName As String, aLayerName As String, Optional aLayerColor As dxxColors = dxxColors.BlackWhite, Optional aLayerLineWeight As dxxLineWeights = dxxLineWeights.ByDefault)
            If String.IsNullOrWhiteSpace(aLayerName) Then aLayerName = aLineTypeName.ToUpper()
            Add(New dxsLinetype(aLineTypeName, aLayerName, aLayerColor, aLayerLineWeight))
        End Sub
        Public Sub Update(aLineTypeName As String, aLayerName As String, aLayerColor As dxxColors, aLayerLineWeight As dxxLineWeights)
            Dim mem As dxsLinetype = Member(aLineTypeName)
            If mem Is Nothing Then Return
            If Not aLayerColor.IsLogical Then
                If mem.LayerColor <> aLayerColor Then
                    Dim lval As dxxColors = mem.LayerColor
                    mem.LayerColor = aLayerColor
                    Notify(New dxoProperty(1, mem.LayerColor, $"{mem.Name}.LayerColor", dxxPropertyTypes.Color, aLastVal:=lval, bNonDXF:=True))
                End If

            End If
            If Not aLayerLineWeight.IsLogical Then
                If aLayerLineWeight <> mem.LayerLineWeight Then
                    Dim lval As dxxLineWeights = mem.LayerLineWeight
                    mem.LayerLineWeight = aLayerLineWeight
                    Notify(New dxoProperty(1, mem.LayerLineWeight, $"{mem.Name}.LayerLineWeight", dxxPropertyTypes.dxf_Integer, aLastVal:=lval, bNonDXF:=True))
                End If

            End If
            If Not String.IsNullOrWhiteSpace(aLayerName) Then
                If String.Compare(aLayerName.Trim(), mem.LayerName, True) <> 0 Then
                    Dim lval As String = mem.LayerLineWeight
                    mem.LayerName = aLayerName.Trim()
                    Notify(New dxoProperty(1, mem.LayerName, $"{mem.Name}.LayerName", dxxPropertyTypes.dxf_String, aLastVal:=lval, bNonDXF:=True))
                End If
            End If


            UpdateMemberLayer(mem)

        End Sub

        Friend Sub UpdateMemberLayer(aMember As dxsLinetype, Optional bAddIfNotFound As Boolean = False)

            If aMember Is Nothing Then Return

            Dim memlay As dxoLayer = MemberLayer(aMember.Name, bAddIfNotFound)
            If memlay IsNot Nothing Then
                memlay.Name = aMember.LayerName
                memlay.Color = aMember.LayerColor
                memlay.Linetype = aMember.Name
                memlay.LineWeight = aMember.LayerLineWeight
            End If

        End Sub


        Friend Function Member(aLineTypeName As String) As dxsLinetype
            If _Members Is Nothing Then Return Nothing
            Return _Members.Find(Function(x) String.Compare(x.Name, aLineTypeName, True) = 0)
        End Function

        Friend Function MemberLayer(aLineTypeName As String, Optional bAddIfNotFound As Boolean = False, Optional aImage As dxfImage = Nothing) As dxoLayer
            If Not GetImage(aImage) Then Return Nothing
            Dim mem As dxsLinetype = Member(aLineTypeName)
            If mem Is Nothing Then Return Nothing
            Dim _rVal As dxoLayer = aImage.Layers.Find(Function(x) String.Compare(x.Name, mem.Name, True) = 0)
            If _rVal Is Nothing And bAddIfNotFound Then
                _rVal = aImage.Layers.Add(mem.LayerName, aColor:=mem.LayerColor, aLinetype:=mem.Name, aLineWeight:=mem.LayerLineWeight)
            End If
            If _rVal IsNot Nothing Then
                mem.LayerName = _rVal.Name
                mem.LayerColor = _rVal.Color
                mem.LayerLineWeight = _rVal.LineWeight

            End If
            Return _rVal
        End Function


        Public Sub Clear()
            _Members = New List(Of dxsLinetype)
        End Sub
#End Region 'Methods


#Region "IEnumerable Implementation"
        Public Function GetEnumerator() As IEnumerator(Of dxsLinetype) Implements IEnumerable(Of dxsLinetype).GetEnumerator
            Return DirectCast(_Members, IEnumerable(Of dxsLinetype)).GetEnumerator()
        End Function
        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return DirectCast(_Members, IEnumerable).GetEnumerator()
        End Function
#End Region 'IEnumerable Implementation

    End Class

End Namespace