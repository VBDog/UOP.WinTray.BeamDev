Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities
Imports Vanara.PInvoke.Gdi32

Namespace UOP.DXFGraphics
    Public Class dxfVector
        Implements iVector
#Region "Members"
        Private _CollectionGUID As String
        Private _ImageGUID As String
        Private _BlockGUID As String
        Private _OwnerGUID As String
        Private _SuppressEvents As Boolean
        Private _DefPntIndex As Integer
        Private _Struc As TVERTEX
        Private _StrucLast As TVERTEX
        Friend CollectionPtr As WeakReference
        Friend OwnerPtr As WeakReference
#End Region 'Members
#Region "Events"
        Public Event CoordinatesChange(aEvent As dxfVertexEvent)
        Public Event VariableChangeChange(aEvent As dxfVertexEvent)
#End Region 'Events
#Region "Constructors"
        Public Sub New()
            Init()
        End Sub
        Public Sub New(aCoordinates As String, Optional aTag As String = "", Optional aFlag As String = "")
            Init()
            Coordinates = aCoordinates
            If Not String.IsNullOrEmpty(aTag) Then _Struc.Tag = aTag
            If Not String.IsNullOrEmpty(aFlag) Then _Struc.Flag = aFlag
        End Sub
        Public Sub New(Optional aX As Double = 0, Optional aY As Double = 0, Optional aZ As Double = 0, Optional aTag As String = "", Optional aFlag As String = "", Optional aRotation As Double = 0)
            Init()
            _Struc.Vector = New TVECTOR(aX, aY, aZ) With {.Rotation = aRotation}
            If Not String.IsNullOrEmpty(aTag) Then _Struc.Tag = aTag Else _Struc.Tag = ""
            If Not String.IsNullOrEmpty(aFlag) Then _Struc.Flag = aFlag Else _Struc.Flag = ""
        End Sub
        Public Sub New(aPlane As dxfPlane, Optional aX As Double = 0, Optional aY As Double = 0, Optional aZ As Double = 0, Optional aTag As String = "", Optional aFlag As String = "", Optional aRotation As Double = 0)
            Init()
            If dxfPlane.IsNull(aPlane) Then
                _Struc.Vector = New TVECTOR(aX, aY, aZ)
            Else
                _Struc.Vector = aPlane.VectorV(aX, aY, aZ)

            End If

            _Struc.Rotation = aRotation
            If Not String.IsNullOrEmpty(aTag) Then Tag = aTag
            If Not String.IsNullOrEmpty(aFlag) Then Flag = aFlag
        End Sub

        Public Sub New(aDirection As dxfDirection, Optional aProjectionPlane As dxfPlane = Nothing)
            Init()
            If aDirection Is Nothing Then Return
            Tag = aDirection.Tag
            Flag = aDirection.Flag
            SetComponentsV(aDirection.X, aDirection.Y, aDirection.Z, True)
            If Not dxfPlane.IsNull(aProjectionPlane) Then
                ProjectToPlane(aProjectionPlane)
            End If
            VertexLastV = New TVERTEX(VertexV)
        End Sub

        Public Sub New(aVector As iVector, Optional aProjectionPlane As dxfPlane = Nothing)
            Init()
            If aVector IsNot Nothing Then
                If TypeOf aVector Is dxfVector Then
                    Dim v1 As dxfVector = DirectCast(aVector, dxfVector)
                    VertexV = New TVERTEX(v1)

                Else
                    VertexV = New TVERTEX(aVector)

                End If
                Tag = aVector.Tag
                Flag = aVector.Flag
            End If

            If Not dxfPlane.IsNull(aProjectionPlane) Then
                ProjectToPlane(aProjectionPlane)
            End If
            VertexLastV = New TVERTEX(VertexV)
        End Sub

        Public Sub New(aBasePt As iVector, aPlane As dxfPlane, Optional aAngle As Double = 0, Optional aDistance As Double = 0, Optional bInRadians As Boolean = False, Optional aTag As String = "", Optional aFlag As String = "")
            Init()

            If aBasePt IsNot Nothing Then
                If TypeOf aBasePt Is dxfVector Then
                    Dim v1 As dxfVector = DirectCast(aBasePt, dxfVector)
                    VertexV = New TVERTEX(v1.VertexV)

                Else
                    VertexV = New TVECTOR(aBasePt)

                End If

                VertexLastV = New TVERTEX(VertexV)
                If String.IsNullOrEmpty(aTag) Then aTag = aBasePt.Tag
                If String.IsNullOrEmpty(aFlag) Then aFlag = aBasePt.Flag

            End If



            If Not dxfPlane.IsNull(aPlane) And aDistance <> 0 Then
                _Struc.Vector += aPlane.AngularDirectionV(aAngle, bInRadians) * aDistance
            End If
            If Not String.IsNullOrEmpty(aTag) Then _Struc.Tag = aTag Else _Struc.Tag = ""
            If Not String.IsNullOrEmpty(aFlag) Then _Struc.Flag = aFlag Else _Struc.Flag = ""
        End Sub
        Friend Sub New(aStructure As TVERTEX)
            Init()
            _Struc = aStructure
        End Sub
        Friend Sub New(aVector As TVECTOR)
            Init()
            _Struc.Vector = aVector
        End Sub

        Private Sub Init()
            _Struc = New TVERTEX(0, 0, 0) With {.Vars = New TVERTEXVARS(dxfLinetypes.ByBlock)}
            _CollectionGUID = ""
            CollectionPtr = Nothing
            _ImageGUID = ""
            _BlockGUID = ""
            _OwnerGUID = ""
            _DefPntIndex = 0
            _SuppressEvents = False
            OwnerPtr = Nothing
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Property Bulge As Double
            Get
                Return _Struc.Bulge
            End Get
            Set(value As Double)
                _Struc.Bulge = value
            End Set
        End Property
        Public Property Col As Integer
            Get
                Return _Struc.Col
            End Get
            Set(value As Integer)
                _Struc.Col = value
            End Set
        End Property
        Friend Property ColID As Integer
            Get
                Return _Struc.ColID
            End Get
            Set(value As Integer)
                _Struc.ColID = value
            End Set
        End Property
        Friend Property CollectionGUID As String
            '^the guid of the vector collection that this vector is a member of
            Get
                Return _CollectionGUID
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                _CollectionGUID = value
                If value = String.Empty Then CollectionPtr = Nothing
            End Set
        End Property
        Friend Property ImageGUID As String
            '^the guid of the image that this vector is associated to
            Get
                Return _ImageGUID
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                _ImageGUID = value
            End Set
        End Property
        Friend Property BlockGUID As String
            '^the guid of the image.block that this vector is associated to
            Get
                Return _BlockGUID
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                _BlockGUID = value
            End Set
        End Property
        Friend Property OwnerGUID As String
            '^the guid of the image.Entity that this vector is associated to
            Get
                Return _OwnerGUID
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then value = String.Empty Else value = value.Trim
                _OwnerGUID = value
            End Set
        End Property
        Public Property Color As dxxColors
            '^the color of the vector
            Get
                Return _Struc.Color
            End Get
            Set(value As dxxColors)
                If _Struc.Color <> value Then
                    _StrucLast.Vars = _Struc.Vars
                    _Struc.Color = value
                    RaiseVariableChangeEvent(dxxVertexEventTypes.Display, dxxVectorProperties.Color)
                End If
            End Set
        End Property
        Public Property Components As String
            '^returns a text string containing the vector's coordinates
            '~vectors have a basic coordinate string like "(12.12,25.70,18.00)"
            Get
                Return Coordinates
            End Get
            Set(value As String)
                Coordinates = value
            End Set
        End Property
        Public Property Coordinates As String
            '^returns a text string containing the vector's coordinates
            '~vectors have a basic coordinate string like "(12.12,25.70,18.00)"
            Get
                Dim _rVal As String = $"({ _Struc.X },{ _Struc.Y}"
                If _Struc.Z <> 0 Then _rVal += $",{_Struc.Z})" Else _rVal += ")"
                Return _rVal
            End Get
            Set(value As String)
                Strukture = TVECTOR.DefineByString(value, _Struc.Vector)
            End Set
        End Property
        Public ReadOnly Property Direction As dxfDirection
            Get
                Return New dxfDirection(_Struc.Vector.Normalized())
            End Get
        End Property
        Public ReadOnly Property Displacement As dxfVector
            '^returns a vector that is the diffence between the current coordinates and then last coordinates
            Get
                Return New dxfVector(_Struc.Vector - _StrucLast.Vector)
            End Get
        End Property
        Public Property DisplaySettings As dxfDisplaySettings
            '^the object which carries display style information for an vector
            Get
                Return New dxfDisplaySettings With {.Strukture = DisplayStructure}
            End Get
            Set(value As dxfDisplaySettings)
                If Not value Is Nothing Then DisplayStructure = value.Strukture
            End Set
        End Property
        Friend Property DisplayStructure As TDISPLAYVARS
            '^the structure of the object which carries display style information for an vector
            Get
                Return New TDISPLAYVARS With {
                    .Color = _Struc.Color,
                    .Linetype = _Struc.Linetype,
                    .LayerName = _Struc.LayerName,
                    .LTScale = _Struc.LTScale,
                    .Suppressed = _Struc.Suppressed,
                    .LineWeight = dxxLineWeights.ByLayer
                }
            End Get
            Set(value As TDISPLAYVARS)
                _StrucLast.Vars = _Struc.Vars
                _Struc.Vars.Color = value.Color
                _Struc.Vars.Linetype = value.Linetype
                _Struc.Vars.LayerName = value.LayerName
                _Struc.Vars.LTScale = value.LTScale
                _Struc.Vars.Suppressed = value.Suppressed
            End Set
        End Property
        Public Property EndWidth As Double
            '^the ending width of the vector
            Get
                Return _Struc.EndWidth
            End Get
            Set(value As Double)
                If _Struc.EndWidth <> value Then
                    _StrucLast.Vars = _Struc.Vars
                    _Struc.EndWidth = value
                    RaiseVariableChangeEvent(dxxVertexEventTypes.Variable, dxxVectorProperties.EndWidth)
                End If
            End Set
        End Property
        Public Property Flag As String Implements iVector.Flag
            '^an assignable string to use for object identitfication
            Get
                Return _Struc.Flag
            End Get
            Set(value As String)
                If _Struc.Flag <> value Then
                    _StrucLast.Vars = _Struc.Vars
                    _Struc.Flag = value
                    RaiseVariableChangeEvent(dxxVertexEventTypes.Variable, dxxVectorProperties.Flag)
                End If
            End Set
        End Property

        ''' <summary>
        ''' a comma delimited string with the points Tag and Flag
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Handle As String
            Get
                Return $"{Tag},{ Flag}"
            End Get
        End Property
        Public Property Index As Integer
            '^the index of the vector if it is a member of an vector collection
            Get
                Return _Struc.Index
            End Get
            Set(value As Integer)
                _Struc.Index = value
            End Set
        End Property



        Public Property Inverted As Boolean
            Get
                Return _Struc.Inverted
            End Get
            Set(value As Boolean)
                If _Struc.Inverted <> value Then
                    _StrucLast.Vars = _Struc.Vars
                    _Struc.Inverted = value
                    RaiseVariableChangeEvent(dxxVertexEventTypes.Variable, dxxVectorProperties.Inverted)
                End If
            End Set
        End Property
        Public ReadOnly Property LastPosition As dxfVector
            '^returns a clone of the vector with the coorinates set to the vectors last position
            Get
                Dim _rVal As New dxfVector(Me)
                _rVal.SetStructure(_StrucLast.Vector)
                Return _rVal
            End Get
        End Property
        Friend ReadOnly Property LastPositionV As TVECTOR
            Get
                Return _StrucLast.Vector
            End Get
        End Property
        Public Property LayerName As String
            '^the layer name associated to the vector
            '~this layer is used for color and linetype info for ByLayer values and the visiblity of the enity in an image
            Get
                Return _Struc.LayerName
            End Get
            Set(value As String)
                If Not String.IsNullOrWhiteSpace(value) Then value = value.Trim Else value = String.Empty
                If String.Compare(_Struc.LayerName, value, ignoreCase:=True) <> 0 Then
                    _StrucLast.Vars = _Struc.Vars
                    _Struc.LayerName = value
                    RaiseVariableChangeEvent(dxxVertexEventTypes.Display, dxxVectorProperties.LayerName)
                End If
            End Set
        End Property
        Public ReadOnly Property Length As Double
            '^returns the magnitude of the vector
            '~the magniture is the square root of the sum of the squares of the current coorddinates (X^2 + Y^2 + Z^2)
            Get
                Return _Struc.Magnitude
            End Get
        End Property
        Public Property Linetype As String
            '^the linetype name assigned to the vector
            Get
                Return _Struc.Linetype
            End Get
            Set(value As String)
                If Not String.IsNullOrWhiteSpace(value) Then value = value.Trim Else value = String.Empty
                If String.Compare(_Struc.LayerName, value, ignoreCase:=True) <> 0 Then
                    _StrucLast.Vars = _Struc.Vars
                    _Struc.Linetype = value
                    RaiseVariableChangeEvent(dxxVertexEventTypes.Display, dxxVectorProperties.Linetype)
                End If
            End Set
        End Property
        Public Property LTScale As Double
            '^the linetype scale factor of the vector
            '~affects the dispaly of non-continuous lines
            Get
                Return _Struc.LTScale
            End Get
            Set(value As Double)
                If value > 0 Then
                    If _Struc.LTScale <> value Then
                        _StrucLast.Vars = _Struc.Vars
                        _Struc.LTScale = value
                        RaiseVariableChangeEvent(dxxVertexEventTypes.Display, dxxVectorProperties.LTScale)
                    End If
                End If
            End Set
        End Property
        Friend Property Mark As Boolean
            '^used internally to put a mark on the vector
            Get
                Return _Struc.Mark
            End Get
            Set(value As Boolean)
                _Struc.Mark = value
            End Set
        End Property
        Public Property Radius As Double
            '^the radius assigned to the vector
            Get
                Return _Struc.Radius
            End Get
            Set(value As Double)
                If _Struc.Radius <> value Then
                    _StrucLast.Vars = _Struc.Vars
                    _Struc.Radius = value
                    RaiseVariableChangeEvent(dxxVertexEventTypes.Variable, dxxVectorProperties.Radius)
                End If
            End Set
        End Property
        Public Property Rotation As Double
            '^the rotation assigned to the vector
            Get
                Return _Struc.Rotation
            End Get
            Set(value As Double)
                value = TVALUES.NormAng(value)
                If _Struc.Rotation <> value Then
                    _StrucLast.Vector = _Struc.Vector
                    _Struc.Rotation = value
                    RaiseVariableChangeEvent(dxxVertexEventTypes.Variable, dxxVectorProperties.Rotation)
                End If
            End Set
        End Property
        Public Property Row As Integer
            '^the row assigned to the vector
            Get
                Return _Struc.Row
            End Get
            Set(value As Integer)
                If _Struc.Row <> value Then
                    _StrucLast.Vars = _Struc.Vars
                    _Struc.Row = value
                    RaiseVariableChangeEvent(dxxVertexEventTypes.Variable, dxxVectorProperties.Row)
                End If
            End Set
        End Property
        Public Property StartWidth As Double
            '^the start width assigned to the vector
            Get
                Return _Struc.StartWidth
            End Get
            Set(value As Double)
                If _Struc.StartWidth <> value Then
                    _StrucLast.Vars = _Struc.Vars
                    _Struc.StartWidth = value
                    RaiseVariableChangeEvent(dxxVertexEventTypes.Variable, dxxVectorProperties.StartWidth)
                End If
            End Set
        End Property
        Friend Property Strukture As TVECTOR
            '^the TVECTOR structure of the vector
            Get
                Return _Struc.Vector
            End Get
            Set(value As TVECTOR)
                If (X <> value.X) Or (Y <> value.Y) Or (Z <> value.Z) Then
                    Dim tLast As TVECTOR = _StrucLast.Vector
                    _StrucLast.Vector = _Struc.Vector
                    _Struc.Vector = value
                    RaiseCoordinatesChangeEvent(tLast, False)
                End If
            End Set
        End Property
        Public Property Suppressed As Boolean
            '^the suppressed value assigned to the vector
            Get
                Return _Struc.Suppressed
            End Get
            Set(value As Boolean)
                If _Struc.Suppressed <> value Then
                    _StrucLast.Vars = _Struc.Vars
                    _Struc.Suppressed = value
                    RaiseVariableChangeEvent(dxxVertexEventTypes.Variable, dxxVectorProperties.Suppressed)
                End If
            End Set
        End Property
        Friend Property SuppressEvents As Boolean
            Get
                Return _SuppressEvents
            End Get
            Set(value As Boolean)
                _SuppressEvents = value
            End Set
        End Property
        Public Property Tag As String Implements iVector.Tag
            '^an assignable string to use for object identitfication
            Get
                Return _Struc.Tag
            End Get
            Set(value As String)
                If _Struc.Tag <> value Then
                    _StrucLast.Vars = _Struc.Vars
                    _Struc.Tag = value
                    RaiseVariableChangeEvent(dxxVertexEventTypes.Variable, dxxVectorProperties.Tag)
                End If
            End Set
        End Property
        Public Property Value As Double
            '^an assignable double that can be assigned to the vector
            Get
                Return _Struc.Value
            End Get
            Set(value As Double)
                Dim newval As Boolean = _Struc.Value <> value

                If newval Then
                    _StrucLast.Vars = _Struc.Vars
                    _Struc.Value = value
                    RaiseVariableChangeEvent(dxxVertexEventTypes.Variable, dxxVectorProperties.Value)
                End If
            End Set
        End Property

        Friend Property GUID As String
            '^an assignable string to use for object identitfication
            Get
                Return _Struc.GUID
            End Get
            Set(value As String)
                If _Struc.GUID <> value Then
                    _StrucLast.Vars = _Struc.Vars
                    _Struc.GUID = value

                End If
            End Set
        End Property

        Friend Property Vars As TVERTEXVARS
            '^the TVERTEXVARS structure that carries the vertext data of the vector
            Get
                Return _Struc.Vars
            End Get
            Set(value As TVERTEXVARS)
                _Struc.Vars = value
            End Set
        End Property
        Public Property VertexCode As Byte
            '^an assignable byte to which is used in graphic path generation (read only)
            Get
                Return _Struc.Code
            End Get
            Friend Set(value As Byte)
                _Struc.Code = value
            End Set
        End Property
        Friend Property VertexLastV As TVERTEX
            Get
                Return _StrucLast
            End Get
            Set(value As TVERTEX)
                _StrucLast = value
            End Set
        End Property
        Public Property VertexRadius As Double
            '^The radius of an arc beginning at this point in a polyline
            '~negative values indicate the arc would be swept in a clockwise direction
            Get
                If Inverted Then
                    Return -Radius
                Else
                    Return Radius
                End If
            End Get
            Set(value As Double)
                value = Math.Round(value, 8)
                If _Struc.Radius <> Math.Abs(value) Then
                    Radius = Math.Abs(value)
                    Inverted = value < 0
                End If
            End Set
        End Property
        Public Property VertexStyle As dxxVertexStyles
            '^an assignable byte to which is used in graphic path generation (read only)
            Get
                Return TVALUES.To_INT(_Struc.Code)
            End Get
            Set(value As dxxVertexStyles)
                If _Struc.Code <> TVALUES.ToByte(value) Then
                    _Struc.Code = TVALUES.ToByte(value)
                End If
            End Set
        End Property
        Public ReadOnly Property VertexStyleName As String
            Get
                Return TVERTEX.StyleName(VertexStyle)
            End Get
        End Property
        Friend Property VertexV As TVERTEX
            Get
                Return _Struc
            End Get
            Set(value As TVERTEX)
                _Struc = value
            End Set
        End Property
        Public Property X As Double Implements iVector.X
            '^the X coordinate
            Get
                Return _Struc.X
            End Get
            Set(value As Double)
                SetCoordinates(value)
            End Set
        End Property
        Public Property Y As Double Implements iVector.Y
            '^the Y coordinate
            Get
                Return _Struc.Y
            End Get
            Set(value As Double)
                SetCoordinates(aNewY:=value)
            End Set
        End Property
        Public Property Z As Double Implements iVector.Z
            '^the Z coordinate
            Get
                Return _Struc.Z
            End Get
            Set(value As Double)
                SetCoordinates(aNewZ:=value)
            End Set
        End Property
        Friend Property DefPntIndex As Integer
            Get
                Return _DefPntIndex
            End Get
            Set(value As Integer)
                _DefPntIndex = value
            End Set
        End Property
        Private ReadOnly Property MyCollection As colDXFVectors
            Get
                If String.IsNullOrWhiteSpace(CollectionGUID) Or CollectionPtr Is Nothing Then Return Nothing
                Dim _rVal As colDXFVectors = TryCast(CollectionPtr.Target, colDXFVectors)
                If _rVal IsNot Nothing Then
                    If String.Compare(CollectionGUID, _rVal.GUID, ignoreCase:=True) Then
                        ReleaseCollectionReference()
                        _rVal = Nothing
                    End If
                End If
                Return _rVal
            End Get
        End Property
        Private ReadOnly Property MyOwner As dxfHandleOwner
            Get
                If String.IsNullOrWhiteSpace(_OwnerGUID) Or OwnerPtr Is Nothing Then Return Nothing
                Dim _rVal As dxfHandleOwner = TryCast(OwnerPtr.Target, dxfHandleOwner)
                If _rVal IsNot Nothing Then
                    If String.Compare(_OwnerGUID, _rVal.GUID, ignoreCase:=True) Then
                        ReleaseOwnerReference()
                        _rVal = Nothing
                    End If
                End If
                Return _rVal
            End Get
        End Property
#End Region 'Properties
#Region "Methods"
        Friend Sub ReleaseOwnerReference()
            _OwnerGUID = ""
            OwnerPtr = Nothing
        End Sub
        Friend Sub ReleaseCollectionReference()
            CollectionGUID = ""
            CollectionPtr = Nothing
        End Sub

        Public Function GetDisplaySettings(Optional aLayer As String = "", Optional aColor As dxxColors = dxxColors.Undefined, Optional aLinetype As String = "")
            If String.IsNullOrWhiteSpace(aLayer) Then aLayer = LayerName
            If String.IsNullOrWhiteSpace(aLinetype) Then aLinetype = Linetype
            If aColor = dxxColors.Undefined Then aColor = Color

            Return dxfDisplaySettings.Null(aLayer, aColor, aLinetype)
        End Function
        Private Function RaiseCoordinatesChangeEvent(LastPosition As TVECTOR, Optional bSuppressEvnts As Boolean = False) As Boolean
            If SuppressEvents Then bSuppressEvnts = True
            If bSuppressEvnts Then Return False
            Dim msg As New dxfVertexEvent(Me, dxxVertexEventTypes.Position, CollectionGUID, ImageGUID, BlockGUID, OwnerGUID) With
            {
                .PropertyType = dxxVectorProperties.Coordinates,
                .PropertyName = "Coordinates",
                .OldValue = _StrucLast.GetValue(dxxVectorProperties.Coordinates),
                .NewValue = _Struc.GetValue(dxxVectorProperties.Coordinates)
            }
            RaiseEvent CoordinatesChange(msg)
            If Not msg.Undo And Not String.IsNullOrWhiteSpace(CollectionGUID) Then
                Dim mycol As colDXFVectors = MyCollection
                If mycol IsNot Nothing Then
                    mycol.RespondToMemberEvent(msg)
                End If
                'If Not String.IsNullOrWhiteSpace(CollectionGUID) Or (Not String.IsNullOrWhiteSpace(ImageGUID) And Not String.IsNullOrWhiteSpace(OwnerGUID)) Then
                '    If Not goEvents.Notify_Vectors(msg) Then CollectionGUID = ""
                'End If
            End If
            If Not msg.Undo And Not String.IsNullOrWhiteSpace(OwnerGUID) And DefPntIndex > 0 Then
                Dim msg2 As New dxfDefPtEvent(Me, dxxVertexEventTypes.Position, CollectionGUID, ImageGUID, BlockGUID, OwnerGUID) With
            {
                .PropertyType = dxxVectorProperties.Coordinates,
                .PropertyName = "Coordinates",
                .OldValue = _StrucLast.GetValue(dxxVectorProperties.Coordinates),
                .NewValue = _Struc.GetValue(dxxVectorProperties.Coordinates)
            }
                Dim owner As dxfHandleOwner = MyOwner
                If owner IsNot Nothing Then
                    owner.RespondToDefPtChange(msg2)
                    msg.OwnerNotified = True
                End If
            End If
            If msg.Undo Then
                _Struc.Vector = _StrucLast.Vector
                _StrucLast.Vector = LastPosition
            End If
            msg.Vertex = Nothing
            Return msg.Undo
        End Function
        Friend Sub SetGUIDS(aImageGUID As String, aOwnerGUID As String, aBlockGUID As String, bSuppresEvnts As Boolean, Optional aDefPtIndex? As Integer = Nothing)
            ImageGUID = aImageGUID : OwnerGUID = aOwnerGUID : BlockGUID = aBlockGUID
            SuppressEvents = bSuppresEvnts
            If aDefPtIndex IsNot Nothing Then
                _DefPntIndex = aDefPtIndex.Value
            End If
        End Sub
        Public Function Ordinate(aOrdinate As dxxOrdinateDescriptors) As Double
            '^returns the request ordinate (X, Y or Z) X by default
            Select Case aOrdinate
                Case dxxOrdinateDescriptors.Z
                    Return _Struc.Z
                Case dxxOrdinateDescriptors.Y
                    Return _Struc.Y
                Case Else
                    Return _Struc.X
            End Select
        End Function
        Public Function AngleTo(aVectorObj As iVector, Optional aPlane As dxfPlane = Nothing) As Double

            '^the angle between this vector and the passed vector
            '~in degrees
            If aVectorObj Is Nothing Then Return 0
            Dim v1 As New TVECTOR(aVectorObj)

            If v1.Equals(_Struc.Vector, True, 5) Then Return 0
            Dim bPlane As dxfPlane = IIf(dxfPlane.IsNull(aPlane), New dxfPlane, aPlane)
            Return v1.AngleTo(_Struc.Vector, bPlane.ZDirectionV)

        End Function
        Public Function ApplyScale(aScaleFactor As Double) As Boolean
            Dim vNew As TVECTOR = (_Struc.Vector * aScaleFactor)
            Dim _rVal As Boolean = vNew <> _Struc.Vector
            Strukture = vNew
            Return _rVal
        End Function
        Public Function ArcAt(Optional aRadius As Double = 0.0, Optional aStartAngle As Double = 0, Optional aEndAngle As Double = 0, Optional bClockwise As Boolean = False, Optional aPlane As dxfPlane = Nothing, Optional aColor As dxxColors = dxxColors.Undefined) As dxeArc
            '^returns an arce centered on the vector
            Return New dxeArc(Me, aRadius, aStartAngle, aEndAngle, bClockwise, aPlane, New dxfDisplaySettings(aColor:=aColor))
        End Function
        Public Function Clone() As dxfVector
            '^returns a new object with properties matching those of the cloned object
            Return New dxfVector(Me)

        End Function
        Public Function ComponentAlong(aV As iVector) As dxfVector
            Return New dxfVector(ComponentAlongV(New TVECTOR(aV)))
            '^returns vectors component  along the passed vector
        End Function
        Friend Function ComponentAlongV(aV As TVECTOR) As TVECTOR
            '^returns vectors component  along the passed vector
            Return _Struc.Vector.ComponentAlong(aV)
        End Function
        Friend Function ComponentAlongV(aV As TVERTEX) As TVERTEX
            '^returns vectors component  along the passed vector
            Return _Struc.ComponentAlong(aV)
        End Function
        Public Function ComponentOrthogonal(aV As iVector) As dxfVector
            Dim _rVal As dxfVector = Nothing
            '^returns vectors component orthogonal the passed vector
            _rVal = New dxfVector(Me)
            _rVal.Strukture = ComponentOrthogonalV(New TVECTOR(aV))
            Return _rVal
        End Function
        Friend Function ComponentOrthogonalV(aV As TVECTOR) As TVECTOR

            '^returns vectors component orthogonal the passed vector
            Return TVECTOR.Orthogonal(_Struc.Vector, aV, TVECTOR.Zero)
        End Function
        Public Function CoordinatesP(Optional aPrecis As Integer = 3) As String
            '^returns a text string containing the vector's coordinates rounded to the passed precisions
            '~the coordinates are augmented with the name of the vectors vertex style
            aPrecis = TVALUES.LimitedValue(aPrecis, 1, 15)
            Dim _rVal As String = $"({ Math.Round(_Struc.X, aPrecis)},{ Math.Round(_Struc.Y, aPrecis)}"
            If _Struc.Z Then _rVal += $",{Math.Round(_Struc.Z, aPrecis)}"
            _rVal += $"{VertexStyleName})"
            Return _rVal
        End Function
        Public Overrides Function ToString() As String
            Return $"dxfVector {CoordinatesR(4)}"
        End Function

        Public Function CoordinatesR(Optional aPrecis As Integer = 3, Optional aMultiplier As Double = 0.0, Optional bSuppressZ As Boolean = False, Optional bSuppressParens As Boolean = False) As String
            Dim _rVal As String = String.Empty
            '^returns a text string containing the vector's coordinates rounded to the passed precisions
            aPrecis = TVALUES.LimitedValue(aPrecis, 1, 15, 3)
            'On Error Resume Next
            Dim mult As Double
            Dim fmat As String = "0." + New String("0", aPrecis)
            If aMultiplier <> 0 Then mult = aMultiplier Else mult = 1
            If Not bSuppressParens Then _rVal = "("
            _rVal += $"{String.Format(Math.Round(_Struc.X * mult, aPrecis), fmat)}, {String.Format(Math.Round(_Struc.Y * mult, aPrecis), fmat)}"
            If Not bSuppressZ And Math.Round(_Struc.Z * mult) > 0 Then _rVal += $", {String.Format(Math.Round(_Struc.Z * mult, aPrecis), fmat)}"
            If Not bSuppressParens Then _rVal += ")"
            Return _rVal
        End Function
        Public Function CoordinatesV(Optional aPrecis As Integer = 3) As String
            Dim _rVal As String = String.Empty
            '^returns a text string containing the vector's coordinates rounded to the passed precisions
            '~the coordinates are augmented with the name of the vectors vertex TYPE (begin line or begin arc w/radius&clockwise)
            aPrecis = TVALUES.LimitedValue(aPrecis, 1, 15)
            'On Error Resume Next
            Dim aStr As String
            _rVal = $"({ Math.Round(_Struc.X, aPrecis) },{Math.Round(_Struc.Y, aPrecis)}"
            If _Struc.Z <> 0 Then _rVal += $",{ Math.Round(_Struc.Z, aPrecis)}"
            _rVal += ","
            If _Struc.Radius = 0 Then
                aStr = "LINE"
            Else
                If _Struc.Inverted Then
                    aStr = $"ARC_CW,{ Math.Round(_Struc.Radius, aPrecis)}"
                Else
                    aStr = $"ARC_CCW,{ Math.Round(_Struc.Radius, aPrecis)}"
                End If
            End If
            _rVal += $"{aStr})"
            Return _rVal
        End Function
        Public Function Copy(Optional aXChange As Double = 0.0, Optional aYChange As Double = 0.0, Optional aZChange As Double = 0.0) As dxfVector
            Dim _rVal As dxfVector = Nothing
            '^returns a new object with properties matching those of the cloned object
            _rVal = New dxfVector
            Dim aVert As TVERTEX = _Struc
            aVert.Vector = _Struc.Vector + New TVECTOR(aXChange, aYChange, aZChange)
            _rVal.VertexV = aVert
            _rVal.VertexLastV = _StrucLast
            Return _rVal
        End Function
        Public Function CopyDisplayValues(aEntitySet As dxfDisplaySettings, Optional aMatchLayer As String = "", Optional aMatchColor As dxxColors = dxxColors.Undefined, Optional aMatchLineType As String = "") As Boolean
            Dim _rVal As Boolean
            '#1the vector settings to copy
            '#2a layer name that if passed the entities layer name will not be changed unless it currently matches this string
            '#3a color that if defined the entities color will not be changed unless it currently matches this value
            '#4a linetype name that if passed the entities linetype name will not be changed unless it currently matches this string
            '^copies the values of the passed display settings to this entities display settings
            Dim aSettings As dxfDisplaySettings
            aSettings = DisplaySettings
            _StrucLast.Vars = _Struc.Vars
            _rVal = aSettings.CopyDisplayValues(aEntitySet, aMatchLayer, aMatchColor, aMatchLineType)
            DisplayStructure = aSettings.Strukture
            Return _rVal
        End Function
        Public Function CrossProduct(aVectorObj As iVector) As dxfVector
            Dim _rVal As New dxfVector(Me)
            _rVal.Strukture = _Struc.Vector.CrossProduct(New TVECTOR(aVectorObj))
            Return _rVal
        End Function
        Public Sub DefineByString(newval As String)
            '^returns a text string containing the vector's coordinates
            '~vectors have a basic coordinate string like "(12.12,25.70,18.00)"
            Strukture = TVECTOR.DefineByString(newval, _Struc.Vector)
        End Sub
        Public Function DirectionTo(aVectorObj As iVector) As dxfDirection
            Dim rDistance As Double = 0.0
            Return DirectionTo(dxfVector.FromIVector(aVectorObj), rDistance)
        End Function
        Public Function DirectionTo(aVectorObj As iVector, ByRef rDistance As Double) As dxfDirection
            '^returns the direction from this vecotr to the passed vector
            '~the direction is this vector subtracted from the passed vector
            Dim _rVal As dxfDirection = Nothing

            Dim delta As TVECTOR
            If aVectorObj Is Nothing Then
                delta = New TVECTOR(-X, -Y, -Z)
            Else
                delta = New TVECTOR(aVectorObj.X - X, aVectorObj.Y - Y, aVectorObj.Z - Z)
            End If
            rDistance = delta.Magnitude
            If rDistance <> 0 Then
                _rVal = New dxfDirection(delta)
            End If
            Return _rVal

            Return New dxfDirection(_Struc.Vector.DirectionTo(New TVECTOR(aVectorObj), False, rDistance))
        End Function
        Public Function DistanceTo(aVector As iVector, ByRef rDirection As dxfDirection) As Double
            '#1the vector to find the distance
            '#2returns the direction from this point to the passed point if requested
            '^returns the distance from this vector to the passed vector
            rDirection = Nothing

            Dim delta As TVECTOR
            If aVector Is Nothing Then
                delta = New TVECTOR(-X, -Y, -Z)
            Else
                delta = New TVECTOR(aVector.X - X, aVector.Y - Y, aVector.Z - Z)
            End If
            Dim _rVal As Double = delta.Magnitude
            If _rVal <> 0 Then
                rDirection = New dxfDirection(delta)
            End If
            Return _rVal
        End Function
        Public Function DistanceTo(aVector As iVector, Optional aPrecis As Integer = -1) As Double
            '#1the vector to find the distance
            '^returns the distance from this vector to the passed vector
            Dim delta As TVECTOR
            If aVector Is Nothing Then
                delta = New TVECTOR(-X, -Y, -Z)
            Else
                delta = New TVECTOR(aVector.X - X, aVector.Y - Y, aVector.Z - Z)
            End If
            Dim _rVal As Double = delta.Magnitude

            If aPrecis >= 0 Then _rVal = Math.Round(_rVal, TVALUES.LimitedValue(aPrecis, 0, 15))
            Return _rVal
        End Function
        Public Function DistanceTo(aEntity As dxfEntity) As Double
            '#1the vector to find the distance
            '^returns the distance from this vector to the passed entities HandlePt

            If aEntity Is Nothing Then Return 0
            Return DistanceTo(aEntity.HandlePt)
        End Function
        Public Function DistanceTo(aLine As iLine, Optional aPrecis As Integer = -1) As Double
            '#1the line to find the distance to
            '#2returns the orthogonal direction
            '^calculates and returns the orthogonal distance from the vector to the given line

            Return dxfProjections.DistanceTo(Me, aLine, aPrecis)
        End Function

        Public Function DistanceToPlane(aPlane As dxfPlane, Optional aDirection As dxfDirection = Nothing, Optional aIntersect As dxfVector = Nothing) As Double
            '#1the plane to find the distance to
            '#2 and optional project direction. If undefines, the planes Z direction is assumed
            '#3 an optional vector. If passed the vector will be moved to the interection point of this vector projected to the passed plane
            '^calculates and returns the orthigonal distance from the given vector to the given plane
            Dim _rVal As Double = 0


            Try
                If dxfPlane.IsNull(aPlane) Then Throw New Exception("The Passed Plane Is Undefined")
                If aDirection Is Nothing Then aDirection = aPlane.ZDirection
                Dim ip As TVECTOR = dxfProjections.ToPlane(_Struc.Vector, aPlane.Strukture, aDirection.Strukture, rDistance:=_rVal)
                If aIntersect IsNot Nothing Then aIntersect.Strukture = ip
            Catch ex As Exception
                Throw ex
            End Try
            Return _rVal
        End Function
        Friend Function DistanceTo(aVector As TVECTOR, ByRef rDirection As dxfDirection) As Double
            Dim _rVal As Double
            '#1the vector to find the distance
            '#2returns the direction from this point to the passed point if requested
            '^returns the distance from this vector to the passed vector
            rDirection = New dxfDirection(_Struc.Vector.DirectionTo(aVector, False, rDistance:=_rVal))
            Return _rVal
        End Function
        Public Function FractionPoint(aVector As dxfVector, Optional aFraction As Double = 0.5) As dxfVector
            If aVector Is Nothing Then Return Clone()
            '#1the faction of the distance to apply
            '^returns then vector the between this vector and the passed vector the fraction of the distancce between them
            Dim _rVal As New dxfVector(Me)
            _rVal.Strukture = FractionPoint(aVector.Strukture, aFraction)
            Return _rVal
        End Function
        Friend Function FractionPoint(aVector As TVECTOR, Optional aFraction As Double = 0.5) As TVECTOR
            '#1the faction of the distance to apply
            '^returns then vector the between this vector and the passed vector the fraction of the distancce between them
            Return _Struc.Vector.Interpolate(aVector, aFraction)
        End Function
        Public Sub GetComponents(ByRef rX As Double, ByRef rY As Double, ByRef rZ As Double)
            '#1returns the current X coordinate
            '#2returns the current Y coordinateo
            '#3returns the current Z coordinate
            '^returns  the X,Y and Z components of the vector
            rX = _Struc.X
            rY = _Struc.Y
            rZ = _Struc.Z
        End Sub
        Public Function Inverse() As dxfVector
            '^returns a copy of the vector with inverse coordinates
            Return Clone() * -1
        End Function
        Public Sub Invert()
            '^inverts the vectors coordinates
            Strukture = _Struc.Vector * -1
        End Sub
        Public Function IsEqual(aVector As iVector) As Boolean
            '#1 the vector to compare to
            '^Test to see if the passed vector is at the same coordinates as the current vector
            '~the precision is assumed as 6
            Return IsEqual(aVector, 6, False)
        End Function

        Friend Function IsEqual(aVector As TVECTOR, aPrecis As Integer, bCompareInverse As Boolean, ByRef rIsInverseEqual As Boolean) As Boolean
            '#1 the vector to compare to
            '#2the precision to apply to the comparision (0 to 15)
            '#3flag to also compare the vector to the inverse of this one
            '^test to see if the passed vector is at the same coordinates as the current vector
            rIsInverseEqual = False
            Dim _rVal As Boolean = aVector.Equals(_Struc.Vector, aPrecis)
            If bCompareInverse And Not _rVal Then
                rIsInverseEqual = (aVector * -1).Equals(_Struc.Vector, aPrecis)
                _rVal = rIsInverseEqual
            End If
            Return _rVal
        End Function
        Public Function IsEqual(aVector As iVector, Optional aPrecis As Integer = 6, Optional bCompareInverse As Boolean = False) As Boolean
            Dim rIsInverseEqual As Boolean = False
            Return IsEqual(aVector, aPrecis, bCompareInverse, rIsInverseEqual)
        End Function
        Public Function IsEqual(aVector As iVector, aPrecis As Integer, bCompareInverse As Boolean, ByRef rIsInverseEqual As Boolean) As Boolean
            '#1 the vector to compare to
            '#2the precision to apply to the comparision (0 to 15)
            '#3flag to also compare the vector to the inverse of this one
            '^Test to see if the passed vector is at the same coordinates as the current vector
            Return IsEqual(New TVECTOR(aVector), aPrecis, bCompareInverse, rIsInverseEqual)
        End Function
        Public Function IsUnity(Optional aPrecis As Integer = 6) As Boolean
            '#1the precisions to apply (0 to 10)
            '^Test to see if the vector is at 1,1,1
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 10)
            Return Math.Round(_Struc.X, aPrecis) = 1 AndAlso Math.Round(_Struc.Y, aPrecis) = 1 AndAlso Math.Round(_Struc.Z, aPrecis) = 1
        End Function
        Public Function IsZero(Optional aPrecis As Integer = 6) As Boolean
            '#1the precisions to apply (0 to 10)
            '^Test to see if the vector is at 0,0,0
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 10)
            Return Math.Round(_Struc.X, aPrecis) = 0 AndAlso Math.Round(_Struc.Y, aPrecis) = 0 AndAlso Math.Round(_Struc.Z, aPrecis) = 0
        End Function
        Public Sub LCLGet(ByRef rLayerName As String, ByRef rColor As dxxColors, ByRef rLinetype As String)
            'returns the Layer, Color and Linetype of the vector
            rLayerName = LayerName
            rColor = Color
            rLinetype = Linetype
        End Sub
        Public Sub LCLSet(Optional aLayerName As String = "", Optional aColor As dxxColors? = Nothing, Optional aLineType As String = "")
            'sets the Layer, Color and Linetype of the vector
            If Not String.IsNullOrWhiteSpace(aLayerName) Then LayerName = aLayerName
            If aColor.HasValue Then Color = aColor.Value
            If Not String.IsNullOrWhiteSpace(aLineType) Then Linetype = aLineType
        End Sub
        Public Function LiesOnPlane(aPlane As dxfPlane, Optional aFudgeFactor As Double = 0.001) As Boolean
            '#1the plane to test
            '#2the distance which determines if the vector is on the plane or not
            '#3returnd the distance of the vector to the plane
            '^returns True if the perpendicular distance of the vector to the plane is less than or equal to the fudge factor
            Dim rDistance As Double = 0.0
            Return LiesOnPlane(aPlane, aFudgeFactor, rDistance)
        End Function
        Public Function LiesOnPlane(aPlane As dxfPlane, aFudgeFactor As Double, ByRef rDistance As Double) As Boolean
            '#1the plane to test
            '#2the distance which determines if the vector is on the plane or not
            '#3returnd the distance of the vector to the plane
            '^returns True if the perpendicular distance of the vector to the plane is less than or equal to the fudge factor
            rDistance = 0
            If dxfPlane.IsNull(aPlane) Then Return False
            Return _Struc.Vector.LiesOn(aPlane.Strukture, aFudgeFactor, rDistance)
        End Function
        Public Function LineTo(aEndPt As dxfVector, Optional aDisplaySettings As dxfDisplaySettings = Nothing) As dxeLine
            '^returns a line starting at this vector and extending to the passed vector
            If aDisplaySettings Is Nothing Then aDisplaySettings = DisplaySettings
            Return New dxeLine(Me, aEndPt, aDisplaySettings)
        End Function
        Public Function MidPoint(aVector As dxfVector) As dxfVector
            Dim _rVal As New dxfVector(Me)
            '#1the point to get the mid point for
            '^returns then vector the half way between this vector and the passed vector the fraction of the distancce between them
            If aVector IsNot Nothing Then _rVal.Strukture = _Struc.Vector.MidPt(aVector.Strukture)
            Return _rVal
        End Function
        Public Function Mirror(aMirrorAxis As iLine) As Boolean
            If aMirrorAxis Is Nothing Then Return False
            '#1the line object to mirror across
            '^mirrors the point across the passed line
            '~returns True if the point actually moves from this process

            Return Mirror(New TLINE(aMirrorAxis))
        End Function

        Friend Function Mirror(aMirrorAxis As TLINE) As Boolean
            '#1the line to mirror across
            '^mirrors the point across the passed line
            '~returns True if the point actually moves from this process
            If aMirrorAxis.Length = 0 Then Return False

            Dim v1 As TVECTOR = _Struc.Vector.Mirrored(aMirrorAxis)
            Return SetCoordinates(v1.X, v1.Y, v1.Z)
        End Function
        Public Function MirrorPlanar(Optional aMirrorX As Double? = Nothing, Optional aMirrorY As Double? = Nothing, Optional aPlane As dxfPlane = Nothing) As Boolean
            If Not aMirrorX.HasValue And Not aMirrorY.HasValue Then Return False
            Dim _rVal As Boolean = False
            '#1the x coordinate to mirror across
            '#2the y coordinate to mirror across
            '^moves the current coordinates to a vector mirrored across the passed values
            '~only allows orthogonal mirroring.
            Dim v1 As TVECTOR = _Struc.Vector
            Dim aPl As New TPLANE(aPlane)
            Dim aLn As TLINE

            If aMirrorX.HasValue Then
                aLn = aPl.LineV(aMirrorX.Value, 10)
                If v1.Mirror(aLn) Then _rVal = True
            End If
            If aMirrorY.HasValue Then
                aLn = aPl.LineH(aMirrorY.Value, 10)
                If v1.Mirror(aLn) Then _rVal = True
            End If
            If _rVal Then SetComponentsV(v1.X, v1.Y, v1.Z, False)
            Return _rVal
        End Function


        Friend Function Mirror(aLine As TLINE, Optional bSuppressEvnts As Boolean = False) As Boolean
            Dim _rVal As Boolean
            '#1the start vector of the line to mirror across
            '#2the end vector of the line to mirror across
            '#3flag to suppress the change event
            '^mirrors the point across the passed line
            '~returns True if the point actually moves from this process
            Dim v1 As New TVECTOR(Me)

            _rVal = v1.Mirror(aLine)
            If _rVal Then SetComponentsV(v1.X, v1.Y, v1.Z, bSuppressEvnts)
            Return _rVal
        End Function

        Public Function Mirrored(aMirrorAxis As iLine) As dxfVector
            Dim _rVal As New dxfVector(Me)
            '#1the line to mirror across
            '^returns the vector mirrored across the passed line
            _rVal.Mirror(aMirrorAxis)
            Return _rVal
        End Function

        Public Function Move(Optional aXChange As Object = Nothing, Optional aYChange As Object = Nothing, Optional aZChange As Object = Nothing, Optional aPlane As dxfPlane = Nothing) As Boolean
            '#1the X displacement
            '#2the Y displacement
            '#3the Z displacement
            '#4a coordinate system to get the X,Y and Z directions from
            '^used to change the coordinates of the vector by translation
            '~if the coordinate system is nothing then the displacement is added to the current coordinates
            '~otherwise the displacement is applied with respect to the systems X, Y and Z directions
            Return Translate(New TVECTOR(TVALUES.To_DBL(aXChange), TVALUES.To_DBL(aYChange), TVALUES.To_DBL(aZChange)), aPlane:=aPlane)
        End Function
        Public Function MoveFromTo(baseVectorXY As iVector, DestinationVectorXY As iVector) As Boolean
            '^used to move the object from one reference vector to another
            Return Translate(New TVECTOR(DestinationVectorXY) - New TVECTOR(baseVectorXY))
        End Function
        Public Function MovePolar(aAngle As Double, aDistance As Double, Optional aPlane As dxfPlane = Nothing, Optional bInRadians As Boolean = False) As Boolean
            '#1the direction angle to move the vector
            '#2the distance to move the vector
            '^used to change the coordinates of an existing vector via polar methods
            Dim aPl As TPLANE
            If Not dxfPlane.IsNull(aPlane) Then aPl = New TPLANE(aPlane) Else aPl = New TPLANE("World")
            Return SetStructure(aPl.AngleVector(_Struc.Vector, aAngle, aDistance, bInRadians))
        End Function

        Public Function MoveTo(aVector As iVector, Optional aChangeX As Double = 0.0, Optional aChangeY As Double = 0.0, Optional aChangeZ As Double = 0.0) As Boolean
            '#1the destination point
            '#2a x displacement to apply after the move
            '#3a y displacement to apply after the move
            '#4a z displacement to apply after the move
            '^moves the vector from its current insertion point to the passed point
            '~returns True if the vector actually moves from this process
            Return MoveTo(New TVECTOR(aVector), aChangeX, aChangeY, aChangeZ)
        End Function

        Friend Function MoveTo(aVector As TVECTOR, Optional aChangeX As Double = 0, Optional aChangeY As Double = 0.0, Optional aChangeZ As Double = 0.0) As Boolean
            '#1the destination point
            '#2a x displacement to apply after the move
            '#3a y displacement to apply after the move
            '#4a z displacement to apply after the move
            '^moves the vector from its current insertion point to the passed point
            '~returns True if the vector actually moves from this process
            Return SetStructure(aVector + New TVECTOR(aChangeX, aChangeY, aChangeZ))
        End Function
        Public Function MoveToAndProject(DestinationObject As iVector, aDirection As dxfDirection, aDistance As Double) As Boolean
            '#1the object with X, Y , Z properties to move to
            '#2the direction to project in
            '#3the distance to project
            '^used to move an existing vector and project it in the passed direction the requested distance.
            Dim v1 As New TVECTOR(DestinationObject)
            If aDirection IsNot Nothing And aDistance <> 0 Then
                v1 += New TVECTOR(aDirection) * aDistance
            End If
            Return SetStructure(v1)
        End Function

        Friend Function Translate(aDisplacement As iVector, Optional aPlane As dxfPlane = Nothing) As Boolean
            If aDisplacement Is Nothing Then Return False
            '#1the displacement to apply
            '#2flag to suppress the change event
            '#3a coordinate system to get the X,Y and Z directions from
            '^used to change the coordinates of the vector by translation
            '~if the coordinate system is nothing then the displacement is added to the current coordinates
            '~otherwise the displacement is applied with respect to the systems X, Y and Z directions
            Return SetStructure(_Struc.Vector.Translated(New TVECTOR(aDisplacement), aPlane))
        End Function
        Friend Function Translate(aDisplacement As TVECTOR, Optional bSuppressEvnts As Boolean = False, Optional aPlane As dxfPlane = Nothing) As Boolean
            '#1the displacement to apply
            '#2flag to suppress the change event
            '#3a coordinate system to get the X,Y and Z directions from
            '^used to change the coordinates of the vector by translation
            '~if the coordinate system is nothing then the displacement is added to the current coordinates
            '~otherwise the displacement is applied with respect to the systems X, Y and Z directions
            Return SetStructure(_Struc.Vector.Translated(aDisplacement, aPlane))
        End Function
        Public Function Moved(Optional aXChange As Double = 0.0, Optional aYChange As Double = 0.0, Optional aZChange As Double = 0.0, Optional aPlane As dxfPlane = Nothing) As dxfVector
            Dim _rVal As dxfVector = Nothing
            '#1the X displacement
            '#2the Y displacement
            '#3the Z displacement
            '#4a coordinate system to get the X,Y and Z directions from
            '^used to change the coordinates of the vector by translation
            '~if the coordinate system is nothing then the displacement is added to the current coordinates
            '~otherwise the displacement is applied with respect to the systems X, Y and Z directions
            _rVal = Clone()
            _rVal.Move(aXChange, aYChange, aZChange, aPlane)
            Return _rVal
        End Function
        Public Sub Multiply(aFactor As Double)
            '^multiplies the coordinates by the passed factor
            Strukture = (_Struc.Vector * aFactor)
        End Sub
        Public Sub Normalize()
            '^converts the vector to aunit vector
            Strukture = _Struc.Vector.Normalized
        End Sub
        Public Function Normalized() As dxfVector
            Dim _rVal As New dxfVector(Me)
            '^Returns a new vector that is this vector normalized
            _rVal.Normalize()
            Return _rVal
        End Function
        Public Function PointAt(Optional aColor As dxxColors = dxxColors.Undefined) As dxePoint
            '^returns a point centered on the vector
            Dim Dsp As dxfDisplaySettings = DisplaySettings
            If aColor <> dxxColors.Undefined Then Dsp.Color = aColor
            Return New dxePoint(Strukture, Dsp)
        End Function
        Public Function PointEntity(Optional aLayer As String = "") As dxePoint
            Return New dxePoint(_Struc.Vector, DisplaySettings, aLayer)
        End Function
        Public Function PolarVector(aAngle As Double, aDistance As Double, Optional aPlane As dxfPlane = Nothing, Optional bInRadians As Boolean = False, Optional aElevation As Double = 0) As dxfVector
            Dim _rVal As New dxfVector(Me)
            '#1the direction angle to move the vector
            '#2the distance to move the vector
            '#3the plane to use for the directions. If nothing is passed the world XYPlane is used
            '^used to change the coordinates of an existing vector via polar methods

            Dim aPl As New TPLANE(aPlane)

            _rVal.Strukture = aPl.AngleVector(_Struc.Vector, aAngle, aDistance, bInRadians, aElevation)
            Return _rVal
        End Function
        Public Function Project(aDirectionObj As iVector, aDistance As Double) As Boolean
            '#1the direction to project in
            '#2the distance to project
            '^projects the vector in the passed direction the requested distance
            If aDirectionObj Is Nothing Or aDistance = 0 Then Return False
            Return Project(New TVECTOR(aDirectionObj, True), aDistance)
        End Function

        Public Function Project(aDirection As dxfDirection, aDistance As Double) As Boolean
            '#1the direction to project in
            '#2the distance to project
            '^projects the vector in the passed direction the requested distance
            If aDirection Is Nothing Or aDistance = 0 Then Return False
            Return Project(New TVECTOR(aDirection), aDistance)
        End Function
        Public Function ProjectToArc(aArc As dxfArc) As Boolean
            Dim rDirection As dxfDirection = Nothing
            Dim rDistance As Double = 0.0
            Dim rPointIsOnArc As Boolean = False
            Return ProjectToArc(aArc, rDirection, rDistance, rPointIsOnArc)
        End Function
        Public Function ProjectToArc(aArc As dxfArc, ByRef rDirection As dxfDirection, ByRef rDistance As Double, ByRef rPointIsOnArc As Boolean) As Boolean
            'Set rDirection = New dxfDirection
            rDistance = 0
            rPointIsOnArc = False
            If aArc Is Nothing Then Return False
            If String.Compare(TypeName(aArc), "dxeArc", True) <> 0 Then Return False
            Dim v1 As TVECTOR
            v1 = dxfProjections.ToArc(_Struc.Vector, aArc, rDirection, rDistance, rPointIsOnArc)
            Return SetCoordinates(v1.X, v1.Y, v1.Z)
        End Function
        Public Function ProjectToLine(aLineObj As iLine) As Boolean
            Dim rDistance As Double = 0.0
            Dim rPointIsOnLine As Boolean = False
            Dim rDirection As dxfDirection = Nothing
            Return ProjectToLine(aLineObj, rDirection, rDistance, rPointIsOnLine)
        End Function
        Public Function ProjectToLine(aLineObj As iLine, aDirection As dxfDirection) As Boolean
            Dim rDistance As Double = 0.0
            Dim rPointIsOnLine As Boolean = False
            Return ProjectToLine(aLineObj, aDirection, rDistance, rPointIsOnLine)
        End Function
        Public Function ProjectToLine(aLineObj As iLine, aDirection As dxfDirection, ByRef rDistance As Double, ByRef rPointIsOnLine As Boolean) As Boolean
            aDirection = Nothing
            rDistance = 0
            rPointIsOnLine = False
            If aLineObj Is Nothing Then Return False
            Dim dpos As Boolean = False
            Dim v1 As TVERTEX = dxfProjections.ToLine(VertexV, New TLINE(aLineObj), rDistance, rPointIsOnLine, dpos)
            Return MoveTo(New dxfVector(v1))
        End Function
        Friend Function ProjectToLine(aLine As TLINE, ByRef rDirection As TVECTOR, Optional bSuppressEvnts As Boolean = False) As Boolean
            Dim rDistance As Double = 0.0
            Dim rPointIsOnLine As Boolean = False
            Return ProjectToLine(aLine, rDirection, rDistance, rPointIsOnLine, bSuppressEvnts)
        End Function
        Friend Function ProjectToLine(aLine As TLINE, ByRef rDirection As TVECTOR, ByRef rDistance As Double, ByRef rPointIsOnLine As Boolean, Optional bSuppressEvnts As Boolean = False) As Boolean
            Dim dpos As Boolean
            Dim vNew As TVECTOR = dxfProjections.ToLine(_Struc.Vector, aLine, rDistance, rPointIsOnLine, dpos)

            If bSuppressEvnts Then
                Return SetStructure(vNew)
            Else
                Return SetCoordinates(vNew.X, vNew.Y, vNew.Z)
            End If
        End Function
        Public Function ProjectToPlane(aPlane As dxfPlane) As Boolean
            Dim rDistance As Double = 0.0
            Dim aDirection As dxfDirection = Nothing
            Return ProjectToPlane(aPlane, aDirection, rDistance)
        End Function

        Public Function ProjectToPlane(aPlane As dxfPlane, Optional aDirection As iVector = Nothing) As Boolean
            Dim rDistance As Double = 0.0
            Return ProjectToPlane(aPlane, aDirection, rDistance)
        End Function
        Public Function ProjectToPlane(aPlane As dxfPlane, aDirection As iVector, ByRef rDistance As Double) As Boolean
            rDistance = 0
            If dxfPlane.IsNull(aPlane) Then Return False
            Return ProjectToPlane(aPlane.Strukture, New TVECTOR(aDirection), rDistance, False)

        End Function
        Friend Function ProjectToPlane(aPlane As TPLANE, aDirection As TVECTOR, Optional bSuppressEvnts As Boolean = False) As Boolean
            Dim rDistance As Double = 0.0
            Return ProjectToPlane(aPlane, aDirection, rDistance, bSuppressEvnts)
        End Function
        Friend Function ProjectToPlane(aPlane As TPLANE, aDirection As TVECTOR, ByRef rDistance As Double, Optional bSuppressEvnts As Boolean = False) As Boolean
            Dim _rVal As Boolean
            Dim antinorm As Boolean
            Dim v1 As TVECTOR = dxfProjections.ToPlane(_Struc.Vector, aPlane, aDirection, rDistance, antinorm)
            _rVal = v1 <> _Struc.Vector
            If _rVal Then SetComponentsV(v1.X, v1.Y, v1.Z, bSuppressEvnts)
            Return _rVal
        End Function
        Friend Function Project(aDirection As TVECTOR, aDistance As Double, Optional bSuppressEvnts As Boolean = False) As Boolean

            If TVECTOR.IsNull(aDirection, 6) Or aDistance = 0 Then Return False
            '#1the direction to project in
            '#2the distance to project
            '^projects the vector in the passed direction the requested distance

            Dim v1 As TVECTOR = _Struc.Vector + aDirection.Normalized * aDistance
            If bSuppressEvnts Then
                Return SetStructure(v1)
            Else
                Return SetComponentsV(v1.X, v1.Y, v1.Z, bSuppressEvnts)
            End If
        End Function
        Public Function Projected(aDirection As iVector, aDistance As Double) As dxfVector
            Dim _rVal As New dxfVector(Me)
            '#1the direction to project in
            '#2the distance to project
            '^returns a new vector which is this vector projected in the passed direction the requested distance
            _rVal.Project(aDirection, aDistance)
            Return _rVal
        End Function
        Public Function Projected(aDirection As dxfDirection, aDistance As Double) As dxfVector
            Dim _rVal As New dxfVector(Me)
            '#1the direction to project in
            '#2the distance to project
            '^returns a new vector which is this vector projected in the passed direction the requested distance
            _rVal.Project(aDirection, aDistance)
            Return _rVal
        End Function
        Public Function ProjectedToArc(aArc As Object, Optional aDirection As dxfDirection = Nothing) As dxfVector
            Dim rDistance As Double = 0.0
            Dim rPointIsOnArc As Boolean = False
            Return ProjectedToArc(aArc, aDirection, rDistance, rPointIsOnArc)
        End Function
        Public Function ProjectedToArc(aArc As Object, aDirection As dxfDirection, ByRef rDistance As Double) As dxfVector
            Dim rPointIsOnArc As Boolean = False
            Return ProjectedToArc(aArc, aDirection, rDistance, rPointIsOnArc)
        End Function
        Public Function ProjectedToArc(aArc As Object, aDirection As dxfDirection, ByRef rDistance As Double, ByRef rPointIsOnArc As Boolean) As dxfVector
            Dim _rVal As New dxfVector(Me)
            _rVal.ProjectToArc(aArc, aDirection, rDistance, rPointIsOnArc)
            Return _rVal
        End Function
        Public Function ProjectedToLine(aLineObj As iLine) As dxfVector
            Dim rDistance As Double = 0.0
            Dim rPointIsOnLine As Boolean = False
            Dim rDirection As dxfDirection = Nothing
            Return ProjectedToLine(aLineObj, rDirection, rDistance, rPointIsOnLine)
        End Function
        Public Function ProjectedToLine(aLineObj As iLine, aDirection As dxfDirection) As dxfVector
            Dim rDistance As Double = 0.0
            Dim rPointIsOnLine As Boolean = False
            Return ProjectedToLine(aLineObj, aDirection, rDistance, rPointIsOnLine)
        End Function
        Public Function ProjectedToLine(aLineObj As iLine, aDirection As dxfDirection, ByRef rDistance As Double) As dxfVector
            Dim rPointIsOnLine As Boolean = False
            Return ProjectedToLine(aLineObj, aDirection, rDistance, rPointIsOnLine)
        End Function
        Public Function ProjectedToLine(aLineObj As iLine, aDirection As dxfDirection, ByRef rDistance As Double, ByRef rPointIsOnLine As Boolean) As dxfVector
            Dim _rVal As New dxfVector(Me)
            _rVal.ProjectToLine(aLineObj, aDirection, rDistance, rPointIsOnLine)
            Return _rVal
        End Function
        Public Function ProjectedToPlane(aPlane As dxfPlane) As dxfVector
            Dim _rVal As New dxfVector(Me)
            Dim rDistance As Double = 0
            _rVal.ProjectToPlane(aPlane, Nothing, rDistance)
            Return _rVal
        End Function
        Friend Sub ProjectTo(aPlane As TPLANE)

            If Not TPLANE.IsNull(aPlane) Then Strukture = dxfProjections.ToPlane(Strukture, aPlane)

        End Sub
        Friend Sub ProjectTo(aPlane As dxfPlane)

            If Not TPLANE.IsNull(aPlane) Then Strukture = dxfProjections.ToPlane(Strukture, aPlane)

        End Sub
        Friend Function ProjectedToPlane(aPlane As TPLANE) As dxfVector
            Dim _rVal As New dxfVector(Me)
            _rVal.ProjectTo(aPlane)
            Return _rVal
        End Function
        Public Function ProjectedToPlane(aPlane As dxfPlane, aDirection As iVector) As dxfVector
            Dim _rVal As New dxfVector(Me)
            _rVal.Strukture = dxfProjections.ToPlane(Strukture, aPlane, aDirection)
            Return _rVal
        End Function
        Public Function ProjectedToPlane(aPlane As dxfPlane, aDirection As iVector, ByRef rDistance As Double) As dxfVector
            Dim _rVal As New dxfVector(Me)
            _rVal.Strukture = dxfProjections.ToPlane(Strukture, aPlane, aDirection, rDistance)
            Return _rVal
        End Function
        Private Function RaiseVariableChangeEvent(aType As dxxVertexEventTypes, aPropertyType As dxxVectorProperties, Optional bSuppressEvnts As Boolean = False) As Boolean
            If SuppressEvents Then bSuppressEvnts = True
            If bSuppressEvnts Then Return False
            Dim rUndo As Boolean
            Dim invalid As Boolean = False
            Dim aOldValue As Object = _StrucLast.GetValue(aPropertyType, invalid)
            If invalid Then
                rUndo = True
            Else
                Dim aNewValue As Object = _Struc.GetValue(aPropertyType)
                Dim msg As New dxfVertexEvent(Me, aType, CollectionGUID, ImageGUID, BlockGUID, OwnerGUID) With
                {
                .PropertyType = aPropertyType,
                    .PropertyName = dxfEnums.Description(aPropertyType),
                    .OldValue = aOldValue,
                    .NewValue = aNewValue
                }
                RaiseEvent VariableChangeChange(msg)
                If msg.Undo Then rUndo = True
                If Not rUndo Then
                    Dim mycol As colDXFVectors = MyCollection
                    If mycol IsNot Nothing Then
                        mycol.RespondToMemberEvent(msg)
                    End If
                    'If Not String.IsNullOrWhiteSpace(CollectionGUID) Or (Not String.IsNullOrWhiteSpace(ImageGUID) And Not String.IsNullOrWhiteSpace(OwnerGUID)) Then
                    '    If Not goEvents.Notify_Vectors(msg, mycol) Then CollectionGUID = ""
                    '    If msg.Undo Then rUndo = True
                    'End If
                End If
            End If
            If rUndo Then
                If aType = dxxVertexEventTypes.Display Then
                    Dim disp1 As TVERTEXVARS = _StrucLast.Vars
                    _StrucLast.Vars = _Struc.Vars
                    _Struc.Vars = disp1
                ElseIf aType = dxxVertexEventTypes.Variable Then
                    Dim vars1 As TVERTEXVARS = _StrucLast.Vars
                    _StrucLast.Vars = _Struc.Vars
                    _Struc.Vars = vars1
                End If
            End If
            Return rUndo
        End Function
        Public Sub Reflect(aV As iVector)
            '^converts the vector to its refelection off the passed vector
            Strukture = TVECTOR.Reflect(_Struc.Vector, New TVECTOR(aV))
        End Sub
        Public Function Reflected(aV As iVector) As dxfVector
            Dim _rVal As New dxfVector(Me)
            '^returns the vector that is the refelection of this vector off the passed vector
            _rVal.Reflect(aV)
            Return _rVal
        End Function
        Public Function Rescale(aScaleX As Double, Optional aReference As iVector = Nothing, Optional aScaleY As Double? = Nothing, Optional aScaleZ As Double? = Nothing, Optional aPlane As dxfPlane = Nothing) As Boolean
            '#1the factor to scale the vector by
            '#2the reference point to rescale the entities position with respect to
            '^rescales the vector in space and dimension by the passed factor

            Return Rescale(aScaleX, New TVECTOR(aReference), aScaleY, aScaleZ, False, aPlane)
        End Function
        Friend Function Rescale(aScaleX As Double, aReference As TVECTOR, Optional aScaleY As Double? = Nothing, Optional aScaleZ As Double? = Nothing, Optional bSuppressEvnts As Boolean = False, Optional aPlane As dxfPlane = Nothing) As Boolean
            '#1the factor to scale the vector by
            '#2the reference point to rescale the entities position with respect to
            '^rescales the Vector in space and dimension by the passed factors
            Dim v1 As TVECTOR = _Struc.Vector
            Dim _rVal As Boolean = v1.Scale(aScaleX, aReference, aScaleY, aScaleZ, aPlane)
            If _rVal Then SetComponentsV(v1.X, v1.Y, v1.Z, bSuppressEvnts)
            Return _rVal
        End Function
        Public Sub Reset()
            '^sets the coordinates to 0,0,0
            SetCoordinates(0, 0, 0)
        End Sub
        Friend Function ResetVectorV(Optional bSuppressEvnts As Boolean = False) As Boolean
            '^sets the coordinates back to the last position
            Return Translate(_Struc.Vector * -1, bSuppressEvnts)
        End Function

        Public Function RotateAbout(aPoint As iVector, aAngle As Double, Optional bInRadians As Boolean = False, Optional aPlane As dxfPlane = Nothing) As Boolean
            Dim rAxis As dxeLine = dxfPlane.CreateAxis(aPlane, aPoint)
            Return RotateAbout(New TLINE(rAxis), aAngle, bInRadians)
        End Function
        Public Function RotateAbout(aPoint As iVector, aAngle As Double, bInRadians As Boolean, aPlane As dxfPlane, ByRef rAxis As dxeLine) As Boolean
            '#1the center vector or line to rotate about
            '#2the angle to rotate the vector
            '#3flag to indicate if the passed angle is in radians
            '#4the coodinate system to use to get the Z axis which is used as the rotation axis
            '#5returns the line that was used as the rotation axis
            '^rotates the vector around a line starting at the passed vector with a direction equal to the Z Axis of the passed coordinate system
            rAxis = dxfPlane.CreateAxis(aPlane, aPoint)
            Return RotateAboutLine(rAxis, aAngle, bInRadians)
        End Function
        Public Function RotateAboutLine(aLine As iLine, aAngle As Double, Optional bInRadians As Boolean = False) As Boolean
            '#1the line to rotate about
            '#2the angle to rotate
            '#3flag indicating the passed angle is in radians
            '^rotates the vector about the passed axis the requested angle
            If aLine Is Nothing Then Return False

            Dim aL As New TLINE(aLine)
            If aL.Length <> 0 Then
                Return RotateAbout(aL, aAngle, bInRadians)
            Else
                Return False
            End If
        End Function


        Friend Function RotateAbout(aAxis As TLINE, aAngle As Double, Optional bInRadians As Boolean = False, Optional bSuppressEvnts As Boolean = False) As Boolean
            Dim v1 As TVECTOR = _Struc.Vector.RotatedAbout(aAxis.SPT, aAxis.Direction, aAngle, bInRadians)
            Return SetComponentsV(v1.X, v1.Y, v1.Z, bSuppressEvnts)
        End Function

        Public Sub RoundTo(Optional aPrecis As Integer = 6)
            '^rounds the components to the passed precision
            '~precision is from 0 to 15 significant figures
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            Strukture = _Struc.Vector.Rounded(aPrecis)
        End Sub
        Public Function Rounded(Optional aPrecis As Integer = 6) As dxfVector
            '^retuns a copy of the vector with components rounded to the passed precision
            Dim _rVal As New dxfVector(Me)
            _rVal.RoundTo(aPrecis)
            Return _rVal
        End Function
        Public Function Scaled(aScaleFactor As Double) As dxfVector
            Dim _rVal As New dxfVector(Me)
            _rVal.Strukture = _Struc.Vector * aScaleFactor
            Return _rVal
        End Function
        Friend Function SetComponentsV(Optional aNewX As Double? = Nothing, Optional aNewY As Double? = Nothing, Optional aNewZ As Double? = Nothing, Optional bSuppressEvnts As Boolean = False) As Boolean
            Dim _rVal As Boolean
            '#1the value to set the X coordinate to
            '#2the value to set the Y coordinate to
            '#3the value to set the Z coordinate to
            '^sets the X,Y and Z components of the vector to the passed values
            '~unpassed or non-numeric values are ignored
            Dim vNew As New TVECTOR(_Struc.Vector)
            _rVal = vNew.Update(aNewX, aNewY, aNewZ)

            If _rVal Then
                Dim tLast As TVECTOR = _StrucLast.Vector
                _StrucLast.Vector = _Struc.Vector
                _Struc.Vector = vNew
                If RaiseCoordinatesChangeEvent(tLast, bSuppressEvnts) Then _rVal = False
            End If
            Return _rVal
        End Function
        Public Function SetCoordinates(Optional aNewX As Double? = Nothing, Optional aNewY As Double? = Nothing, Optional aNewZ As Double? = Nothing) As Boolean
            '#1the value to set the X coordinate to
            '#2the value to set the Y coordinate to
            '#3the value to set the Z coordinate to
            '^sets the X,Y and Z components of the vector to the passed values
            '~unpassed or non-numeric values are ignored
            Return SetComponentsV(aNewX, aNewY, aNewZ)
        End Function
        Public Function SetDisplayProperty(aVariableType As dxxDisplayProperties, aNewValue As Object, Optional aSearchList As Object = Nothing) As Boolean
            Dim _rVal As Boolean = False
            '#1the name of the display variable to affect (LayerName, Color, Linetype etc.)
            '#2the new value for the  display variable
            '#3a variable value to match
            '#4flag to set any undefined variable to the new value
            '^sets the members indicated display variable to the new value
            '~if a seach value is passed then only members with a current value equal to the search value will be affected.
            '~returns the affected members.
            Dim sValue As String
            Dim sngValue As Double
            Dim bValue As Boolean
            Dim lastVal As Object
            Dim nval As Object
            Dim iVal As Integer
            Dim slist As New TLIST
            If aSearchList IsNot Nothing Then slist = New TLIST(",", aSearchList.ToString)
            Select Case aVariableType
                Case dxxDisplayProperties.Color
                    If Not TVALUES.IsNumber(aNewValue) Then Return _rVal
                    iVal = TVALUES.To_INT(aNewValue)
                    If iVal = -1 Then Return _rVal
                    nval = iVal
                    lastVal = Color
                    If slist.ContainsNumber(lastVal, True, 0) Then
                        If Color <> iVal Then
                            _rVal = True
                            Color = iVal
                        End If
                    End If
                Case dxxDisplayProperties.LineWeight
                    Return False
                Case dxxDisplayProperties.LTScale
                    If Not TVALUES.IsNumber(aNewValue) Then Return _rVal
                    sngValue = TVALUES.To_DBL(aNewValue, False, 6)
                    nval = sngValue
                    If sngValue < 0 Then sngValue = 0
                    lastVal = LTScale
                    If slist.ContainsNumber(lastVal, True, 6) Then
                        If Math.Round(LTScale, 6) <> sngValue Then
                            _rVal = True
                            LTScale = sngValue
                        End If
                    End If
                Case dxxDisplayProperties.LayerName
                    If aNewValue Is Nothing Then aNewValue = ""
                    sValue = aNewValue.ToString().Trim()
                    If sValue = "" Then sValue = "0"
                    nval = sValue
                    lastVal = LayerName
                    If slist.Contains(lastVal, True) Then
                        If String.Compare(lastVal, sValue, True) <> 0 Then
                            _rVal = True
                            LayerName = sValue
                        End If
                    End If
                Case dxxDisplayProperties.Linetype
                    If aNewValue Is Nothing Then aNewValue = ""
                    sValue = aNewValue.ToString().Trim()
                    If sValue = "" Then sValue = dxfLinetypes.ByLayer
                    nval = sValue
                    lastVal = Linetype
                    If slist.Contains(lastVal, True) Then
                        If String.Compare(lastVal, sValue, True) <> 0 Then
                            _rVal = True
                            Linetype = sValue
                        End If
                    End If
                Case dxxDisplayProperties.Suppressed
                    If Not TVALUES.IsBoolean(aNewValue) Then Return _rVal
                    bValue = TVALUES.ToBoolean(aNewValue)
                    nval = bValue
                    lastVal = Suppressed
                    If slist.Contains(lastVal, True) Then
                        If lastVal <> bValue Then
                            _rVal = True
                            Suppressed = bValue
                        End If
                    End If
                Case dxxDisplayProperties.TextStyle
                    Return False
                Case dxxDisplayProperties.DimStyle
                    Return False
            End Select
            Return _rVal
        End Function
        Public Function GetDisplayProperty(aVariableType As dxxDisplayProperties) As Object
            '#2the display property to return
            '~returns the current value of the indicated display property if it is relevant to a vector
            Select Case aVariableType
                Case dxxDisplayProperties.Color
                    Return Color
                Case dxxDisplayProperties.LineWeight
                    Return 0
                Case dxxDisplayProperties.LTScale
                    Return LTScale
                Case dxxDisplayProperties.LayerName
                    Return LayerName
                Case dxxDisplayProperties.Linetype
                    Return Linetype
                Case dxxDisplayProperties.Suppressed
                    Return Suppressed
                Case dxxDisplayProperties.TextStyle
                    Return String.Empty
                Case dxxDisplayProperties.DimStyle
                    Return String.Empty
                Case Else
                    Return Nothing
            End Select
        End Function
        Friend Function SetStructure(newStruc As TVECTOR, Optional bSuppressEvents As Boolean = False) As Boolean
            Dim _rVal As Boolean
            _rVal = (X - newStruc.X <> 0) Or (Y - newStruc.Y <> 0) Or (Z - newStruc.Z <> 0)
            If _rVal Then
                Dim tLast As TVECTOR
                tLast = _StrucLast.Vector
                _StrucLast.Vector = _Struc.Vector
                _Struc.Vector = newStruc
                If RaiseCoordinatesChangeEvent(tLast, bSuppressEvents) Then _rVal = False
            End If
            Return _rVal
        End Function
        Friend Function SetStructure(newStruc As TVERTEX, Optional bSuppressEvents As Boolean = False) As Boolean
            Dim _rVal As Boolean
            _rVal = (X - newStruc.X <> 0) Or (Y - newStruc.Y <> 0) Or (Z - newStruc.Z <> 0)
            If _rVal Then
                Dim tLast As TVECTOR
                tLast = _StrucLast.Vector
                _StrucLast.Vector = _Struc.Vector
                _Struc.Vector = newStruc.Vector
                _Struc.Vars = newStruc.Vars
                If RaiseCoordinatesChangeEvent(tLast, bSuppressEvents) Then _rVal = False
            End If
            Return _rVal
        End Function
        Public Sub TFVGet(ByRef rTag As String, ByRef rFlag As String, ByRef rValue As Double, ByRef rLayerName As String)
            rTag = Tag : rFlag = Flag : rValue = Value : rLayerName = LayerName
        End Sub
        Public Sub TFVSet(Optional aTag As String = Nothing, Optional aFlag As String = Nothing, Optional aValue As Double? = Nothing, Optional aLayerName As String = Nothing)
            If aTag IsNot Nothing Then Tag = aTag
            If aFlag IsNot Nothing Then Flag = aFlag
            If aLayerName IsNot Nothing Then LayerName = aLayerName
            If aValue IsNot Nothing Then
                If aValue.HasValue Then Value = aValue.Value
            End If
        End Sub
        Friend Function Transform(aTransforms As TTRANSFORMS, Optional bSuppressEvnts As Boolean = False) As Boolean
            Dim _rVal As Boolean
            If aTransforms.Count <= 0 Then Return _rVal
            'On Error Resume Next
            Dim vert1 As New TVERTEX(_Struc)
            For i As Integer = 1 To aTransforms.Count
                If TTRANSFORM.Apply(aTransforms.Item(i), vert1) Then _rVal = True
            Next i
            If _rVal Then
                _Struc.Vars = vert1.Vars
                SetComponentsV(vert1.X, vert1.Y, vert1.Z, bSuppressEvnts)
            End If
            Return _rVal
        End Function
        Friend Function Transform(aTransform As TTRANSFORM, Optional bSuppressEvnts As Boolean = False) As Boolean
            Dim vert1 As New TVERTEX(_Struc)
            Dim _rVal As Boolean = TTRANSFORM.Apply(aTransform, vert1)
            If _rVal Then
                _Struc.Vars = vert1.Vars
                SetComponentsV(vert1.Vector.X, vert1.Vector.Y, vert1.Vector.Z, bSuppressEvnts)
            End If
            Return _rVal
        End Function
        Public Function VertexCoordinates(Optional aPlane As dxfPlane = Nothing) As String
            '^the 2D coordinates of the vector with respect to the passed plane augmented with the vertex radius of the vector
            '~ie "(X,Y,VertexRadius)" where the vertex radius indicates the vector is the start of an arc.

            Return VertexCoordinatesV(New TPLANE(aPlane))
        End Function
        Public Sub VertexCoordinatesSet(ByRef newval As String, Optional aPlane As dxfPlane = Nothing)
            '^defines the coordinates of the vector with respect to the passed plane augmented with the vertex radius of the vector
            '~ie "(X,Y,VertexRadius)" where the vertex radius indicates the vector is the start of an arc.

            VertexCoordinatesSetV(newval, New TPLANE(aPlane))
        End Sub
        Friend Sub VertexCoordinatesSetV(ByRef newval As String, aPlane As TPLANE)
            '^defines the coordinates of the vector with respect to the passed plane augmented with the vertex radius of the vector
            '~ie "(X,Y,VertexRadius)" where the vertex radius indicates the vector is the start of an arc.
            Dim v1 As TVECTOR = TVECTOR.DefineByString(newval, _Struc.Vector)
            Strukture = New TVECTOR(aPlane, v1.X, v1.Y)
            VertexRadius = v1.Z
        End Sub
        Friend Function VertexCoordinatesV(aPlane As TPLANE, Optional bSuppressRads As Boolean = False) As String
            '^the 2D coordinates of the vector with respect to the passed plane augmented with the vertex radius of the vector
            '~ie "(X,Y,VertexRadius)" where the vertex radius indicates the vector is the start of an arc.
            Dim v1 As TVECTOR = _Struc.Vector.WithRespectTo(aPlane)
            If Not bSuppressRads Then
                Return $"({v1.X },{v1.Y },{VertexRadius })"
            Else
                Return $"({v1.X },{v1.Y })"
            End If
        End Function
        Public Function ValueString(Optional aPrecis As Integer = 4, Optional aValueSource As String = "VALUE") As String
            '^returns the points X, Y and Value Properties in a comma delimited string
            aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
            If String.IsNullOrWhiteSpace(aValueSource) Then aValueSource = "" Else aValueSource = aValueSource.Trim.ToUpper
            Dim aVal As Double = 0
            Select Case aValueSource
                Case "VALUE", ""
                    aVal = Value
                Case "ROTATION"
                    aVal = Rotation
                Case "RADIUS"
                    aVal = Radius
                Case "BULGE"
                    aVal = Bulge

            End Select
            Return $"({ Math.Round(_Struc.X, aPrecis) },{ Math.Round(_Struc.Y, aPrecis) },{aVal}"

        End Function

        ''' <summary>
        ''' returns a clone of the vector with its coordinates defined relative to the passed plane
        ''' </summary>
        ''' <remarks>the returned vector is not a world vector unless a transfer plane is passed.  </remarks>
        ''' <param name="aPlane">the plane to define the returned vector with respect to, if nothing is passed then the world XY plane is assumed</param>
        ''' <param name="aTransferPlane">an optional plane to transfer the new vector to</param>
        ''' <param name="aTransferElevation">an optional elevation (Z) to apply to the new vector</param>
        ''' <param name="aTransferRotation">if passed the vector is rotaed on the plane</param>
        ''' <param name="aXScale"> an optional X scale to apply</param>
        ''' <param name="aYScale">an optional Y scale to apply</param>
        ''' <param name="bMaintainZ">a flag to maintain Z of the vector </param>
        ''' <returns></returns>
        Public Function WithRespectToPlane(aPlane As dxfPlane, Optional aTransferPlane As dxfPlane = Nothing, Optional aTransferElevation As Double? = Nothing, Optional aTransferRotation As Double? = Nothing, Optional aXScale As Double? = Nothing, Optional aYScale As Double? = Nothing, Optional bMaintainZ As Boolean = False) As dxfVector
            Return dxfVector.VectorWithRespectToPlane(Me, aPlane, aTransferPlane, aTransferElevation, aTransferRotation, aXScale, aYScale, bMaintainZ)

        End Function
#End Region 'Methods
#Region "Shared Methods"
        ''' <summary>
        ''' returns a clone of the vector with its coordinates defined relative to the passed plane
        ''' </summary>
        ''' <remarks>the returned vector is not a world vector unless a transfer plane is passed.  </remarks>
        ''' <param name="aVector">the vector to convert</param>
        ''' <param name="aPlane">the plane to define the returned vector with respect to, if nothing is passed then the world XY plane is assumed</param>
        ''' <param name="aTransferPlane">an optional plane to transfer the new vector to</param>
        ''' <param name="aTransferElevation">an optional elevation (Z) to apply to the new vector</param>
        ''' <param name="aTransferRotation">if passed the vector is rotaed on the plane</param>
        ''' <param name="aXScale"> an optional X scale to apply</param>
        ''' <param name="aYScale">an optional Y scale to apply</param>
        ''' <param name="bMaintainZ">a flag to maintain Z of the vector </param>
        ''' <returns></returns>
        Public Shared Function VectorWithRespectToPlane(aVector As iVector, aPlane As dxfPlane, Optional aTransferPlane As dxfPlane = Nothing, Optional aTransferElevation As Double? = Nothing, Optional aTransferRotation As Double? = Nothing, Optional aXScale As Double? = Nothing, Optional aYScale As Double? = Nothing, Optional bMaintainZ As Boolean = False) As dxfVector

            If aVector Is Nothing Then Return Nothing
            Dim _rVal As dxfVector = dxfVector.FromIVector(aVector, bCloneIt:=True)

            Dim plane As New TPLANE(aPlane)
            _rVal.Strukture = _rVal._Struc.Vector.WithRespectTo(plane)
            Dim z As Double = 0
            If bMaintainZ Then z = aVector.Z
            If aTransferElevation.HasValue Then z = aTransferElevation.Value
            Dim xval As Double = _rVal.X
            Dim yval As Double = _rVal.Y
            If aXScale.HasValue Then xval *= aXScale.Value
            If aYScale.HasValue Then yval *= aYScale.Value
            _rVal.SetCoordinates(xval, yval, z)

            If dxfPlane.IsNull(aTransferPlane) Then Return _rVal
            Dim rot As Double = _rVal.Rotation
            If aTransferRotation.HasValue Then rot = aTransferRotation.Value
            Dim transfer As TVECTOR = aTransferPlane.VectorV(xval, yval, z, aVectorRotation:=rot)
            _rVal.Strukture = transfer
            Return _rVal
        End Function

        Public Shared ReadOnly Property WorldX As dxfVector
            Get
                Return New dxfVector(1, 0, 0)
            End Get
        End Property

        Public Shared ReadOnly Property WorldY As dxfVector
            Get
                Return New dxfVector(0, 1, 0)
            End Get
        End Property

        Public Shared ReadOnly Property WorldZ As dxfVector
            Get
                Return New dxfVector(0, 0, 1)
            End Get
        End Property

        Public Shared Function FromIVector(aIVector As iVector, Optional bReturnSomething As Boolean = False, Optional bCloneIt As Boolean = False) As dxfVector
            If aIVector Is Nothing Then
                If Not bReturnSomething Then Return Nothing
                Return dxfVector.Zero
            End If
            Try
                If TypeOf (aIVector) Is dxfVector Then
                    Dim v1 As dxfVector = DirectCast(aIVector, dxfVector)
                    If bCloneIt Then v1 = New dxfVector(v1)
                    Return v1
                Else
                    Return New dxfVector(aIVector)
                End If

            Catch ex As Exception
                Return Nothing
            End Try

        End Function

        Public Shared Function FromObject(aSource As Object, Optional aProjectionPlane As dxfPlane = Nothing, Optional ChangeX As Double = 0.0, Optional ChangeY As Double = 0.0, Optional ChangeZ As Double = 0.0) As dxfVector
            '#1the object with X, Y , Z properties to move to
            '^returns a point whose coordinates are taken from the passed Object
            '~also allows the point to be moved from the destination coordinates after it is moved.
            '~the passed object must have a numeric X and Y property. Z is optional
            Return New dxfVector(TVECTOR.FromObject(aSource, aProjectionPlane, aChangeX:=ChangeX, aChangeY:=ChangeY, aChangeZ:=ChangeZ))
        End Function
        Public Shared Function PropertyValue(A As dxfVector, aPropEnum As dxxVectorProperties, Optional aPrecis As Integer = -1) As Object
            Dim rUnknowProp As Boolean = False
            Dim rName As String = String.Empty
            Return PropertyValue(A, aPropEnum, aPrecis, rUnknowProp, rName)
        End Function
        Public Shared Function PropertyValue(A As dxfVector, aPropEnum As dxxVectorProperties, aPrecis As Integer, ByRef rUnknowProp As Boolean, ByRef rName As String) As Object
            Return TVERTEX.PropertyValue(A.VertexV, aPropEnum, aPrecis, rUnknowProp, rName)
        End Function
        Public Shared Function SwapVectors(ByRef aVector As dxfVector, ByRef bVector As dxfVector, Optional aBooleanCondition As Boolean? = Nothing) As Boolean
            Dim _rVal As Boolean
            '#1the first vector
            '#2the second vector
            '#3a flag when evaluated as a boolean equals False will prevent the swap from being made
            '^swaps the two vector references if the third argument is not passed or if it evaluates to True
            '~Returns True if the swap was made
            If Not aBooleanCondition.HasValue Then
                _rVal = True
            Else
                _rVal = aBooleanCondition.Value
            End If
            If Not _rVal Then Return _rVal
            Dim aObj As dxfVector
            aObj = aVector
            aVector = bVector
            bVector = aObj
            Return _rVal
        End Function
        Public Shared ReadOnly Property Zero As dxfVector
            Get
                Return New dxfVector(0D, 0D, 0D)
            End Get
        End Property

        Public Shared Function Create(Optional aX As Double = 0, Optional aY As Double = 0, Optional aZ As Double = 0, Optional aTag As String = "", Optional aFlag As String = "", Optional aRotation As Double = 0) As dxfVector
            Return New dxfVector(aX, aY, aZ, aTag, aFlag, aRotation)
        End Function


        Friend Shared Function IsNull(aVector As TVECTOR, Optional aPrecis As Integer = 0) As Boolean

            If aPrecis <= 0 Then
                Return aVector.X = 0 And aVector.Y = 0 And aVector.Z = 0
            Else
                aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
                Return Math.Round(aVector.X, aPrecis) = 0 And Math.Round(aVector.Y, aPrecis) = 0 And Math.Round(aVector.Z, aPrecis) = 0
            End If

        End Function

        Public Shared Function IsNull(aVector As iVector, Optional aPrecis As Integer = 0) As Boolean
            If aVector Is Nothing Then Return True
            If aPrecis <= 0 Then
                Return aVector.X = 0 And aVector.Y = 0 And aVector.Z = 0
            Else
                aPrecis = TVALUES.LimitedValue(aPrecis, 0, 15)
                Return Math.Round(aVector.X, aPrecis) = 0 And Math.Round(aVector.Y, aPrecis) = 0 And Math.Round(aVector.Z, aPrecis) = 0
            End If

        End Function

        Public Shared Function IsNull(aDirection As dxfDirection, Optional aPrecis As Integer? = Nothing) As Boolean
            If aDirection Is Nothing Then Return True
            If Not aPrecis.HasValue Then
                Return aDirection.X = 0 And aDirection.Y = 0 And aDirection.Z = 0
            Else
                Dim precis As Integer = TVALUES.LimitedValue(aPrecis.Value, 0, 15)
                Return Math.Round(aDirection.X, precis) = 0 And Math.Round(aDirection.Y, precis) = 0 And Math.Round(aDirection.Z, precis) = 0
            End If

        End Function
#End Region 'shared methods
#Region "Operators"
        Public Shared Operator +(A As dxfVector, B As dxfVector) As dxfVector
            If A Is Nothing Then Return B
            If B Is Nothing Then Return A
            Return New dxfVector(A.X + B.X, A.Y + B.Y, A.Z + B.Z) With {.VertexCode = A.VertexCode, .Rotation = A.Rotation, .Vars = A.Vars}
        End Operator
        Public Shared Operator -(A As dxfVector, B As dxfVector) As dxfVector
            If A Is Nothing Then Return B
            If B Is Nothing Then Return A
            Return New dxfVector(A.X - B.X, A.Y - B.Y, A.Z - B.Z) With {.VertexCode = A.VertexCode, .Rotation = A.Rotation, .Vars = A.Vars}
        End Operator
        Public Shared Operator *(A As dxfVector, aScaler As Double) As dxfVector
            If A Is Nothing Then Return A
            Return New dxfVector(A.X * aScaler, A.Y * aScaler, A.Z * aScaler) With {.VertexCode = A.VertexCode, .Rotation = A.Rotation, .Vars = A.Vars}
        End Operator
        Public Shared Operator /(A As dxfVector, aScaler As Double) As dxfVector
            If A Is Nothing Then Return A
            Return New dxfVector(A.X / aScaler, A.Y / aScaler, A.Z / aScaler) With {.VertexCode = A.VertexCode, .Rotation = A.Rotation, .Vars = A.Vars}
        End Operator
        Public Shared Operator *(A As dxfVector, B As dxfVector) As dxfVector
            If A Is Nothing Then Return B
            If B Is Nothing Then Return A
            Return New dxfVector(A.X * B.X, A.Y * B.Y, A.Z * B.Z) With {.VertexCode = A.VertexCode, .Rotation = A.Rotation, .Vars = A.Vars}
        End Operator
        Public Shared Operator =(A As dxfVector, B As dxfVector) As Boolean
            If A Is Nothing And B Is Nothing Then Return True
            If A Is Nothing Or B Is Nothing Then Return False
            Return (A.X = B.X) And (A.Y = B.Y) And (A.Z = B.Z)
        End Operator
        Public Shared Operator <>(A As dxfVector, B As dxfVector) As Boolean
            If A Is Nothing And B Is Nothing Then Return False
            If A Is Nothing Or B Is Nothing Then Return True
            Return (A.X <> B.X) Or (A.Y <> B.Y) Or (A.Z <> B.Z)
        End Operator

#End Region 'Operators
    End Class 'dxfVector
End Namespace
