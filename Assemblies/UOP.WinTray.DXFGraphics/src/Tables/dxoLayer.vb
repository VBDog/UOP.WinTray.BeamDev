Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxoLayer
        Inherits dxfTableEntry
        Implements ICloneable
#Region "Members"
#End Region 'Members
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxReferenceTypes.LAYER)
        End Sub
        Public Sub New(aName As String)
            MyBase.New(dxxReferenceTypes.LAYER, aName)
        End Sub
        Friend Sub New(aEntry As TTABLEENTRY)
            MyBase.New(dxxReferenceTypes.LAYER, aEntry.Name, aGUID:=aEntry.GUID)
            If aEntry.EntryType = dxxReferenceTypes.LAYER Then Properties.CopyVals(aEntry)

        End Sub
        Public Sub New(aEntry As dxoLayer)
            MyBase.New(aEntry)
        End Sub
#End Region 'Constructors
#Region "dxfHandleOwner"
        Friend Overrides Property Suppressed As Boolean
            Get
                Return False
            End Get
            Set(value As Boolean)
                value = False
            End Set
        End Property
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Throw New NotImplementedException
        End Function
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Throw New NotImplementedException
        End Function
        Public Overrides ReadOnly Property FileObjectType As dxxFileObjectTypes
            Get
                Return dxxFileObjectTypes.TableEntry
            End Get
        End Property
#End Region 'dxfHandleOwner
#Region "Properties"
        Public Property Color As dxxColors
            Get
                '^the layer's color
                Return PropValueI(dxxLayerProperties.Color, bAbsVal:=True)
            End Get
            Set(value As dxxColors)
                '^the layer's' color (1 to 255)

                PropValueSet(dxxLayerProperties.Color, value)
            End Set
        End Property
        Public Property Color64 As Color
            Get
                '^the color in windows 64 format
                Return dxfColors.ACLToWin64(Color)
            End Get
            Set(value As Color)
                '^the color in windows 64 format
                Dim idx As dxxColors
                Dim aColor As dxfColor = dxfColors.NearestACLColor(value, idx, True)
                If idx = dxxColors.ByBlock Or idx = dxxColors.ByLayer Then Return
                If dxfColors.ColorIsReal(idx) And idx <> dxxColors.Undefined Then
                    Color = idx
                Else
                    Color = aColor.ToWin32
                End If
            End Set
        End Property
        Public Property Description As String
            Get
                '^the layer's description
                Return PropValueStr(dxxLayerProperties.Description)
            End Get
            Set(value As String)
                PropValueSet(dxxLayerProperties.Description, value)
            End Set
        End Property
        Friend Property DisplayVars As TDISPLAYVARS
            Get
                Return New TDISPLAYVARS(aLayer:=Name, aLineType:=Linetype, aColor:=Color, aLineweight:=LineWeight, aLTScale:=1)
            End Get
            Set(value As TDISPLAYVARS)

                Linetype = value.Linetype
                Color = value.Color
                If dxfEnums.Validate(GetType(dxxLineWeights), value.LineWeight, bSkipNegatives:=True) Then LineWeight = value.LineWeight
            End Set
        End Property
        Public Property Frozen As Boolean
            Get
                Return PropValueB(dxxLayerProperties.Frozen)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxLayerProperties.Frozen, value)
            End Set
        End Property
        Public Property FrozenInNewViewports As Boolean
            Get
                Return PropValueB(dxxLayerProperties.FrozenViewport)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxLayerProperties.FrozenViewport, value)
            End Set
        End Property
        Public Property LayerStatus As dxxLayerStatus
            Get
                '^the status of the layer
                '~normal, frozen, locked etc.
                '~matches AutoCAD layer status
                Return PropValueI(dxxLayerProperties.Status)
            End Get
            Set(value As dxxLayerStatus)
                '^the status of the layer
                '~normal, frozen, locked etc.
                '~matches AutoCAD layer status
                PropValueSet(dxxLayerProperties.Status, value)
            End Set
        End Property

        ''' <summary>
        ''' the linetype name assigned to the layer
        ''' </summary>
        ''' <remarks>can not be a logical value</remarks>
        ''' <returns></returns>
        Public Property Linetype As String
            Get
                Return PropValueStr(dxxLayerProperties.Linetype)
            End Get
            Set(value As String)

                If String.IsNullOrWhiteSpace(value) Then Return
                PropValueSet(dxxLayerProperties.Linetype, value.Trim())
            End Set
        End Property
        ''' <summary>
        ''' the lineweight assigned to the layer
        ''' </summary>
        ''' <remarks>can not be a logical value</remarks>
        ''' <returns></returns>
        Public Property LineWeight As dxxLineWeights
            Get
                Return PropValueI(dxxLayerProperties.LineWeight)
            End Get
            Set(value As dxxLineWeights)
                If Not value.IsLogical() Then PropValueSet(dxxLayerProperties.LineWeight, value)
            End Set
        End Property
        Public ReadOnly Property LineWeightDescription As String
            Get
                Return dxfEnums.Description(LineWeight)
            End Get
        End Property
        Public Property Locked As Boolean
            Get
                Return PropValueB(dxxLayerProperties.Locked)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxLayerProperties.Locked, value)
            End Set
        End Property
        Public Property PlotFlag As Boolean
            Get
                '^controls if the layer gets printed
                Return PropValueB(dxxLayerProperties.PlotFlag)
            End Get
            Set(value As Boolean)
                '^controls if the layer gets printed
                PropValueSet(dxxLayerProperties.PlotFlag, value)
            End Set
        End Property
        Public Property Transparency As Integer
            Get
                '^the transparency value for this layer
                Return PropValueI(dxxLayerProperties.Transparency)
            End Get
            Set(value As Integer)
                If value >= 0 And value <= 90 Then
                    PropValueSet(dxxLayerProperties.Transparency, value)
                End If
            End Set
        End Property
        Public Property Visible As Boolean
            Get
                Return PropValueB(dxxLayerProperties.Visible)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxLayerProperties.Visible, value)
            End Set
        End Property
        Public Property XRefDependant As Boolean
            Get
                Return PropValueB(dxxLayerProperties.XRefDependant)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxLayerProperties.XRefDependant, value)
            End Set
        End Property
        Public Property XRefResolved As Boolean
            Get
                Return PropValueB(dxxLayerProperties.XRefResolved)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxLayerProperties.XRefResolved, value)
            End Set
        End Property
#End Region 'Properties
        Friend Overrides Function PropValueSetByName(aName As String, aValue As Object, Optional aOccur As Integer = 0, Optional bSuppressEvnts As Boolean = False) As Boolean
            If aValue Is Nothing Or String.IsNullOrWhiteSpace(aName) Then Return False

            Select Case aName.ToUpper()
                Case "COLOR"
                    If TypeOf aValue IsNot dxxColors Then
                        Return False
                    End If
                    Dim value As dxxColors = DirectCast(aValue, dxxColors)
                    If value.IsLogical() Then Return False

                    aValue = value
                Case "LINEWEIGHT"
                    If TypeOf aValue IsNot dxxLineWeights Then
                        Return False
                    End If
                    Dim value As dxxLineWeights = DirectCast(aValue, dxxLineWeights)
                    If value.IsLogical() Then Return False
                    aValue = value
                Case "LINETYPE"
                    If TypeOf aValue IsNot String Then
                        Return False
                    End If
                    Dim value As String = DirectCast(aValue, String)
                    If dxoLinetype.LTIsLogical(value) Then Return False
                    aValue = value
            End Select
            Return MyBase.PropValueSetByName(aName, aValue, aOccur, bSuppressEvnts)

        End Function

        Public Shadows Function Clone() As dxoLayer
            Return New dxoLayer(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return Clone()
        End Function
#Region "Methods"

#End Region 'Methods
    End Class 'dxoLayer
End Namespace
