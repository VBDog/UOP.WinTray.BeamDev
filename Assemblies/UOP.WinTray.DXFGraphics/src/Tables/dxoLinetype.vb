Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxoLinetype
        Inherits dxfTableEntry
        Implements ICloneable
#Region "Members"
#End Region 'Members
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxReferenceTypes.LTYPE)
        End Sub
        Friend Sub New(aEntry As TTABLEENTRY)
            MyBase.New(dxxReferenceTypes.LTYPE, aEntry.Name, aGUID:=aEntry.GUID)
            If aEntry.EntryType = dxxReferenceTypes.LTYPE Then Properties.CopyVals(aEntry)
        End Sub
        Public Sub New(aName As String)
            MyBase.New(dxxReferenceTypes.LTYPE, aName)
        End Sub
        Public Sub New(aEntry As dxoLinetype)
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
        Public ReadOnly Property IsLogical As Boolean
            Get
                Return dxoLinetype.LTIsLogical(Name)
            End Get
        End Property

        Public Property Description As String
            Get
                Return PropValueStr(dxxLinetypeProperties.Description)
            End Get
            Friend Set(value As String)
                PropValueSet(dxxLinetypeProperties.Description, value)
            End Set
        End Property
        Public ReadOnly Property ElementCount As Integer
            Get
                Return PropValueI(dxxLinetypeProperties.Elements)
            End Get
        End Property
        Public ReadOnly Property Elements As List(Of dxoProperty)
            Get
                Return Properties.FindAll(Function(x) x.Name.StartsWith("Element("))

            End Get
        End Property
        Public ReadOnly Property PatternLength As Double
            Get
                Return PropValueD(dxxLinetypeProperties.PatternLength)
            End Get
        End Property
        Public Property XRefDependant As Boolean
            Get
                Return PropValueB(dxxLinetypeProperties.XRefDependant)
            End Get
            Friend Set(value As Boolean)
                PropValueSet(dxxLinetypeProperties.XRefDependant, value)
            End Set
        End Property
        Public Property XRefResolved As Boolean
            Get
                Return PropValueB(dxxLinetypeProperties.XRefResolved)
            End Get
            Friend Set(value As Boolean)
                PropValueSet(dxxLinetypeProperties.XRefResolved, value)
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Friend Function GetLinetpyeStyleData(ByRef rPatternLength As Double, Optional bRegen As Boolean = False) As TVALUES
            Dim _rVal As TVALUES = Values
            'On Error Resume Next
            Dim i As Integer
            Dim idx As Integer
            Dim aProps As dxoProperties
            Dim eCnt As Integer
            Dim ltname As String = Name
            Dim conv As Double
            Dim gcprops As List(Of dxoProperty) = Properties.Members(49)

            eCnt = Properties.ValueI(73)
            aProps = Reactors.Member("ElementLengths", True)
            If _rVal.Count <> eCnt Or eCnt <> gcprops.Count Then bRegen = True

            If bRegen Then
                _rVal = New TVALUES
                eCnt = gcprops.Count
                If ltname.ToUpper().Contains("ISO") Then conv = 1 / 25.4 Else conv = 1
                Properties.SetVal(73, eCnt)
                If gcprops.Count > 0 Then
                    Dim tot As Double = 0


                    For i = 1 To gcprops.Count
                        Dim sval As Single = TVALUES.To_SNG(gcprops.Item(i - 1).Value) * conv
                        tot += sval
                        _rVal.Add(sval)

                    Next i
                    _rVal.BaseValue = tot

                End If
                If idx >= 0 Then Reactors.ClearMember("ElementLengths")
                Reactors.Add(New dxoProperties(gcprops) With {.Name = "ElementLengths"}, "ElementLengths")

                _rVal.Defined = True
            End If
            Values = _rVal

            rPatternLength = TVALUES.To_DBL(_rVal.BaseValue)
            Return _rVal
        End Function
        Friend Overrides Function PropValueSetByName(aName As String, aValue As Object, Optional aOccur As Integer = 0, Optional bSuppressEvnts As Boolean = False) As Boolean
            If aValue Is Nothing Or String.IsNullOrWhiteSpace(aName) Then Return False
            'reviewv changes for validity before proceding
            Select Case aName.ToUpper()
                Case "a property name here"

            End Select
            Return MyBase.PropValueSetByName(aName, aValue, aOccur, bSuppressEvnts)

        End Function

        Public Shadows Function Clone() As dxoLinetype
            Return New dxoLinetype(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return Clone()

        End Function
#End Region 'Methods

#Region "Shared Methods"
        Public Shared Function LTIsLogical(aLinetype As String)
            Return (String.Compare(aLinetype, dxfLinetypes.ByBlock, True) = 0 Or String.Compare(aLinetype, dxfLinetypes.ByLayer, True) = 0 Or String.Compare(aLinetype, dxfLinetypes.Invisible, True) = 0)
        End Function
#End Region 'Shared Methods
    End Class 'dxoLinetype
End Namespace
