Imports System.Runtime.InteropServices
Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics

    Public Class dxfAttribute
        Implements iHandleOwner
        Implements ICloneable

#Region "Fields"
        Private _Handlez As THANDLES
#End Region 'Fields


#Region "Constructors"

        Public Sub New()
            Init()
        End Sub
        Public Sub New(aAttrib As dxfAttribute, Optional bCopyHandles As Boolean = False)
            Init()
            If aAttrib Is Nothing Then Return
            _Properties.CopyVals(aAttrib.Properties, aNamesToSkip:=New List(Of String)({"*TEXTTYPE"}), bSkipHandles:=Not bCopyHandles, bSkipPointers:=Not bCopyHandles)
            If bCopyHandles Then Handle = aAttrib.Handle
        End Sub

        Friend Sub New(aAttrib As TPROPERTY)
            Init()
            Tag = aAttrib.Name
            Prompt = aAttrib.Prompt
            Value = aAttrib.ValueS
            Mark = aAttrib.Mark
        End Sub

        Public Sub New(aAttrib As dxeText, Optional aOwnerGUID As String = Nothing)
            Init(aOwnerGUID)
            If aAttrib Is Nothing Then Return
            If aAttrib.TextType = dxxTextTypes.Multiline Then Return
            aAttrib.UpdateCommonProperties(bUpdateProperties:=True)

            _Properties.CopyVals(aAttrib.Properties, aNamesToSkip:=New List(Of String)({"*TEXTTYPE"}))
            SourceGUID = aAttrib.GUID
            Handle = aAttrib.Handle

        End Sub

        Private Sub Init(Optional aOwnerGUID As String = Nothing)
            _Handlez = New THANDLES(dxfEvents.NextEntityGUID(dxxGraphicTypes.Text, aGUIDPrefix:=aOwnerGUID))
            _Properties = dxpProperties.Get_EntityProperties(dxxGraphicTypes.Text, aGUID:=GUID, aTextType:=dxxTextTypes.Attribute)
            _Properties.Handle = Handle
            _Properties.GUID = GUID
            'AddHandler _Properties.PropertyChanged(aMember)
        End Sub
#End Region 'Constructors
#Region "Event Handlers"
        Private Sub PropertyChangeEventHandler(aProperty As dxoProperty) Handles _Properties.PropertyChanged
            If aProperty Is Nothing Then Return
            If aProperty.GroupCode = 40 Then
                If aProperty.ValueD > 300 Then
                    Console.WriteLine(aProperty.Signature)
                End If

            End If
        End Sub

#End Region 'Event Handlers

#Region "Properties"
        Public Property Tag As String
            Get
                Return Properties.ValueS(2)
            End Get
            Friend Set(value As String)
                If value Is Nothing Then value = ""
                Properties.SetVal(2, value.Trim().Replace(" ", "_"))
            End Set
        End Property
        Public Property Prompt As String
            Get
                Return Properties.ValueS(3)
            End Get
            Friend Set(value As String)
                If value Is Nothing Then value = ""
                Properties.SetVal(3, value.Trim())
            End Set
        End Property
        Public Property Value As String
            Get
                Return Properties.ValueS(1)
            End Get
            Set(value As String)
                If value Is Nothing Then value = ""
                Properties.SetVal(1, value)
            End Set
        End Property

        Friend Property Mark As Boolean
        Public Property Color As dxxColors
            Get
                Return Properties.ValueI(62, aDefault:=dxxColors.ByLayer)
            End Get
            Set(value As dxxColors)
                If value <> dxxColors.Undefined Then
                    Properties.SetVal(62, value)
                End If
            End Set
        End Property


        Public Property TextHeight As Double
            Get
                Return Properties.ValueD(40)
            End Get
            Set(value As Double)
                Properties.SetVal(40, Math.Abs(value))
            End Set
        End Property
        Public Property Rotation As Double
            Get
                Dim _rVal As Double = Properties.ValueD(50)
                Return _rVal * 180 / Math.PI
            End Get
            Set(value As Double)
                value = TVALUES.NormAng(value, False, True)
                Properties.SetVal(50, value * Math.PI / 180)
            End Set
        End Property
        Public Property WidthFactor As Double
            Get
                Return Properties.ValueD(41, 1)
            End Get
            Set(value As Double)
                Properties.SetVal(41, TVALUES.LimitedValue(Math.Abs(value), 0.01, 100))
            End Set
        End Property
        Public Property TextStyleName As String
            Get
                Return Properties.ValueS(7, 1)
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then Return
                Properties.SetVal(7, value.Trim())
            End Set
        End Property
        Public Property UpsideDown As Boolean
            Get
                Return Properties.ValueB("*UpsideDown")
            End Get
            Set(value As Boolean)
                Properties.SetVal("*UpsideDown", value, True)
            End Set
        End Property

        Public Property Backwards As Boolean
            Get
                Return Properties.ValueB("*Backwards")
            End Get
            Set(value As Boolean)
                Properties.SetVal("*Backwards", value)
            End Set
        End Property

        Public Property ObliqueAngle As Double
            '^the text's oblique angle
            '~-85 to 85
            Get
                Return Properties.ValueD(51)
            End Get
            Set(value As Double)

                Properties.SetVal(51, TVALUES.ObliqueAngle(value))
            End Set
        End Property

        Private WithEvents _Properties As dxoProperties
        Public Property Properties As dxoProperties
            Get
                Return _Properties
            End Get
            Friend Set(value As dxoProperties)
                _Properties = value
            End Set
        End Property

        Public ReadOnly Property Signature
            Get
                Return TagValueString("=")
            End Get
        End Property
#End Region 'Properties

#Region "Methods"


        Public Overrides Function ToString() As String
            Return $"dxfAttribute [{Tag}]:[{Value}]"
        End Function
        Public Function TagValueString(Optional aDelimiter As Char = ",") As String
            Return $"{Tag}{aDelimiter}{Value}"
        End Function


        Public Function Clone() As dxfAttribute
            Return New dxfAttribute(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New dxfAttribute(Me)
        End Function

#End Region 'Methods
#Region "iHandleOwner"
        Public Property ReactorGUID As String Implements iHandleOwner.ReactorGUID
            Get
                Return _Handlez.ReactorGUID
            End Get
            Set(value As String)
                _Handlez.ReactorGUID = value
            End Set
        End Property

        Public Property BlockGUID As String Implements iHandleOwner.BlockGUID
            Get
                Return _Handlez.BlockGUID
            End Get
            Set(value As String)
                _Handlez.BlockGUID = value
            End Set
        End Property

        Public Property CollectionGUID As String Implements iHandleOwner.CollectionGUID
            Get
                Return _Handlez.CollectionGUID
            End Get
            Set(value As String)
                _Handlez.CollectionGUID = value
            End Set
        End Property

        Public Property BlockCollectionGUID As String Implements iHandleOwner.BlockCollectionGUID
            Get
                Return _Handlez.BlockCollectionGUID
            End Get
            Set(value As String)
                _Handlez.BlockCollectionGUID = value
            End Set
        End Property

        Public Property Handle As String Implements iHandleOwner.Handle
            Get
                Return _Handlez.Handle
            End Get
            Set(value As String)
                _Handlez.Handle = value
                Properties.Handle = _Handlez.Handle

            End Set
        End Property

        Public Property ImageGUID As String Implements iHandleOwner.ImageGUID
            Get
                Return _Handlez.ImageGUID
            End Get
            Set(value As String)
                _Handlez.ImageGUID = value
            End Set
        End Property

        Public Property Index As Integer Implements iHandleOwner.Index
            Get
                Return _Handlez.Index
            End Get
            Set(value As Integer)
                _Handlez.Index = value

            End Set
        End Property

        Public Property OwnerGUID As String Implements iHandleOwner.OwnerGUID
            Get
                Return _Handlez.OwnerGUID
            End Get
            Set(value As String)
                _Handlez.OwnerGUID = value
            End Set
        End Property

        Public Property SourceGUID As String Implements iHandleOwner.SourceGUID
            Get
                Return _Handlez.SourceGUID
            End Get
            Set(value As String)
                _Handlez.SourceGUID = value
            End Set
        End Property

        Public Property Domain As dxxDrawingDomains Implements iHandleOwner.Domain
            Get
                Return _Handlez.Domain
            End Get
            Set(value As dxxDrawingDomains)
                _Handlez.Domain = value
            End Set
        End Property

        Public Property Identifier As String Implements iHandleOwner.Identifier
            Get
                Return _Handlez.Identifier
            End Get
            Set(value As String)
                _Handlez.Identifier = value
            End Set
        End Property

        Public Property ObjectType As dxxFileObjectTypes Implements iHandleOwner.ObjectType
            Get
                Return FileObjectType
            End Get
            Set(value As dxxFileObjectTypes)
                value = FileObjectType
            End Set
        End Property

        Public Property OwnerType As dxxFileObjectTypes Implements iHandleOwner.OwnerType
            Get
                Return _Handlez.OwnerType
            End Get
            Set(value As dxxFileObjectTypes)
                _Handlez.OwnerType = value
            End Set
        End Property

        Public Property Name As String Implements iHandleOwner.Name
            Get
                Return _Handlez.Name
            End Get
            Friend Set(value As String)
                _Handlez.Name = value
            End Set
        End Property

        Public Property GUID As String Implements iHandleOwner.GUID
            Get
                Return _Handlez.GUID
            End Get
            Friend Set(value As String)
                _Handlez.GUID = value
                Properties.GUID = _Handlez.GUID

            End Set
        End Property

        Friend Property Suppressed As Boolean Implements iHandleOwner.Suppressed
            Get
                Return _Handlez.Suppressed
            End Get
            Set(value As Boolean)
                _Handlez.Suppressed = value
            End Set
        End Property

        Friend Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix Implements iHandleOwner.DXFFileProperties
            Throw New NotImplementedException
        End Function

        Friend Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As dxfPlane, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As dxoPropertyArray Implements iHandleOwner.DXFProps
            Throw New NotImplementedException
        End Function



        Public ReadOnly Property FileObjectType As dxxFileObjectTypes
            Get
                Return dxxFileObjectTypes.Attribute
            End Get
        End Property
#End Region 'dxfHandleOwner
    End Class

    Public Class dxfAttributes
        Inherits List(Of dxfAttribute)
        Implements IEnumerable(Of dxfAttribute)

#Region "Properties"
        Public Property IsDirty As Boolean


#End Region 'Properties
#Region "Constructors"
        Public Sub New()
            _Delimiter = "~"
        End Sub

        Public Sub New(aAttribs As IEnumerable(Of dxfAttribute))

            _Delimiter = "~"
            If aAttribs Is Nothing Then Return
            If TypeOf aAttribs Is dxfAttributes Then
                Dim datts As dxfAttributes = DirectCast(aAttribs, dxfAttributes)
                Delimiter = datts.Delimiter
                IsDirty = datts.IsDirty
            End If

            For Each item As dxfAttribute In aAttribs
                If item IsNot Nothing Then MyBase.Add(New dxfAttribute(item))
            Next
        End Sub
        Public Sub New(aAttribs As IEnumerable(Of dxeText))

            _Delimiter = "~"
            If aAttribs Is Nothing Then Return


            For Each item As dxeText In aAttribs
                If item IsNot Nothing Then

                    If item.TextType Then MyBase.Add(New dxfAttribute(item))
                End If
            Next
        End Sub

#End Region 'Constructors
#Region "Properties"

        Private _Delimiter As Char
        Public Property Delimiter As Char
            Get

                Return _Delimiter
            End Get
            Set(value As Char)
                If String.IsNullOrWhiteSpace(value) Then value = "~"

                _Delimiter = value
            End Set
        End Property

        Public Property TagValues As List(Of String)
            Get
                Dim _rVal As New List(Of String)
                Dim delim As Char = Delimiter

                For Each attr As dxfAttribute In Me

                    _rVal.Add(attr.TagValueString(delim))
                Next
                Return _rVal
            End Get
            Set(value As List(Of String))

                If value Is Nothing Then Return
                If Count <= 0 Then Return

                Dim delim As Char = Delimiter
                For Each att As dxfAttribute In Me
                    att.Mark = False
                Next
                For Each str As String In value
                    If String.IsNullOrWhiteSpace(str) Then Continue For
                    If Not str.Contains(delim) Then Continue For
                    Dim sVals() As String = str.Split(delim)
                    If sVals.Length < 2 Then Continue For

                    Dim aTag As String = sVals(0)
                    Dim aVal As String = sVals(1)
                    Dim tagmems As List(Of dxfAttribute) = FindAll(Function(x) String.Compare(x.Tag, aTag, True) = 0 And Not x.Mark)
                    If tagmems.Count > 0 Then
                        tagmems(0).Value = aVal
                        tagmems(0).Mark = True

                    End If

                Next
            End Set
        End Property
#End Region 'Properties
#Region "Methods"

        Public Sub SetMarks(aMark As Boolean)
            For Each item As dxfAttribute In Me
                item.Mark = aMark
            Next
        End Sub
        Public Function TagValueString(Optional aDelimiter As Char? = Nothing) As String
            Dim _rVal As String = String.Empty
            Dim delim As Char = Delimiter
            If aDelimiter.HasValue Then delim = aDelimiter.Value
            For Each Item As dxfAttribute In Me
                If _rVal <> String.Empty Then _rVal += delim
                _rVal += Item.Signature
            Next

            Return _rVal
        End Function
        Friend Overloads Sub Add(aAtt As TPROPERTY)
            Add(New dxfAttribute(aAtt))
        End Sub
        Friend Overloads Sub Add(aAtt As dxfAttribute)
            If aAtt Is Nothing Then Return
            If String.IsNullOrEmpty(aAtt.Tag) Then Return
            MyBase.Add(aAtt)
            IsDirty = True
        End Sub

        Public Function AddTagValue(aTag As String, aValue As Object, Optional aPrompt As String = "", Optional aCopies As Integer = 0) As dxfAttribute
            If String.IsNullOrWhiteSpace(aTag) Then Return Nothing
            If aValue Is Nothing Then aValue = ""
            If aPrompt IsNot Nothing Then aPrompt = String.Empty Else aPrompt = aPrompt.Trim()
            aTag = aTag.Trim().Replace(" ", "_")
            Dim _rVal As New dxfAttribute() With {.Tag = aTag, .Prompt = aPrompt, .Value = aValue.ToString().Trim()}
            Add(_rVal)
            For i As Integer = 2 To aCopies
                Add(New dxfAttribute(_rVal))
            Next i
            Return _rVal
        End Function
        Public Sub Append(aNewMems As IEnumerable(Of dxfAttribute), Optional bAddClones As Boolean = False)

            If aNewMems Is Nothing Then Return
            For Each att As dxfAttribute In aNewMems
                If att Is Nothing Then Continue For
                If Not bAddClones Then Add(att) Else Add(New dxfAttribute(att))
            Next
        End Sub

        Public Sub Populate(aNewMems As IEnumerable(Of dxfAttribute), Optional bAddClones As Boolean = False)
            Clear()
            If aNewMems Is Nothing Then Return
            For Each att As dxfAttribute In aNewMems
                If att Is Nothing Then Continue For
                If Not bAddClones Then Add(att) Else Add(New dxfAttribute(att))
            Next
        End Sub

        Friend Shared Function TagValueString(aAttribs As TPROPERTIES, Optional aDelimiter As String = ",") As String
            Dim _rVal As String = String.Empty
            Dim aAtt As TPROPERTY
            For i As Integer = 1 To aAttribs.Count
                aAtt = aAttribs.Item(i)
                If _rVal <> "" Then _rVal += aDelimiter
                _rVal += aAtt.Name & "=" & aAtt.Value
            Next i
            Return _rVal
        End Function


        ''' <summary>
        ''' set the values of the members with the passed tag to the null sting
        ''' </summary>
        ''' <remarks>if a null tag string is passed all members values are set to the null string.</remarks>
        ''' <param name="aTag">the tag to clear</param>
        ''' <param name="aOccurance">the occurance number of the attribute to apply the the null string to</param>
        ''' <returns></returns>
        Public Function ClearValues(aTag As String, Optional aOccurance As Integer = 0) As Boolean

            Dim _rVal As Boolean = False
            If String.IsNullOrWhiteSpace(aTag) Then
                For Each attr As dxfAttribute In Me
                    If Not String.IsNullOrWhiteSpace(attr.Value) Then
                        _rVal = True
                        IsDirty = True
                    End If
                    attr.Value = String.Empty
                Next
            Else
                _rVal = SetValue(aTag, aValue:=String.Empty, aOccurance)
            End If

            Return _rVal

        End Function
        Public Function Clone() As dxfAttributes

            Return New dxfAttributes(Me)
        End Function
        Public Function CopyValues(aValues As dxfAttributes, Optional bAddNewMembers As Boolean = False, Optional bCopyProps As Boolean = True) As Boolean
            Dim _rVal As Boolean = False
            If aValues Is Nothing Then Return False
            Dim mymems As List(Of List(Of dxfAttribute)) = MemberSets()
            For Each tagset As List(Of dxfAttribute) In mymems
                Dim occr As Integer = 0

                For Each item As dxfAttribute In tagset
                    occr += 1
                    Dim hermem As dxfAttribute = aValues.Item(item.Tag, occr)
                    If hermem Is Nothing Then Exit For
                    If bCopyProps Then
                        item.Properties.CopyVals(IndependantGroupCodes, hermem.Properties)
                    Else
                        If String.Compare(hermem.Value, item.Value, True) <> 0 Then
                            _rVal = True
                            IsDirty = True
                        End If
                        item.Value = hermem.Value

                    End If
                Next

            Next

            If bAddNewMembers Then
                mymems = aValues.MemberSets()
                For Each tagset As List(Of dxfAttribute) In mymems
                    Dim occr As Integer = 0

                    For Each item As dxfAttribute In tagset
                        occr += 1
                        Dim mymem As dxfAttribute = Me.Item(item.Tag, occr)
                        If mymem Is Nothing Then
                            _rVal = True
                            IsDirty = True
                            MyBase.Add(New dxfAttribute(item))
                        End If

                    Next

                Next
            End If


            Return _rVal
        End Function

        Public Function IsEqual(aValues As dxfAttributes, Optional bDontCompareValues As Boolean = False) As Boolean

            If aValues Is Nothing Then Return False
            Dim mytags As List(Of String) = Nothing
            Dim hertags As List(Of String) = Nothing
            Dim mymems As List(Of List(Of dxfAttribute)) = MemberSets(mytags)
            Dim hermems As List(Of List(Of dxfAttribute)) = MemberSets(hertags)
            If mytags.Count <> hertags.Count Then Return False
            For Each tag As String In mytags
                If hertags.FindIndex(Function(x) String.Compare(x, tag, True) = 0) < 0 Then Return False
            Next
            If bDontCompareValues Then Return True
            Dim _rVal As Boolean = True

            For Each tagset As List(Of dxfAttribute) In mymems
                Dim occr As Integer = 0
                Dim tag As String = tagset(0).Tag
                For Each item As dxfAttribute In tagset
                    occr += 1
                    Dim hermem As dxfAttribute = aValues.Item(item.Tag, occr)
                    If hermem Is Nothing Then Return False
                    If String.Compare(hermem.Value, item.Value, True) <> 0 Then Return False
                Next
            Next
            Return _rVal
        End Function

        Public Function GetValue(aTag As String, Optional aOccurance As Integer = 1) As String
            Dim mem As dxfAttribute = Item(aTag, aOccurance)
            Return IIf(mem Is Nothing, String.Empty, mem.Value)
        End Function
        Friend Sub PrintToDebug()
            Dim tagmems As List(Of List(Of dxfAttribute)) = MemberSets()
            For Each tagset As List(Of dxfAttribute) In tagmems
                For Each item As dxfAttribute In tagset
                    System.Diagnostics.Debug.WriteLine(item.Signature)
                Next

            Next

        End Sub

        Public Function TagList() As List(Of String)
            Dim _rVal As New List(Of String)
            For Each mem As dxfAttribute In Me
                If _rVal.FindIndex(Function(x) String.Compare(x, mem.Tag, True) = 0) < 0 Then _rVal.Add(mem.Tag)
            Next
            _rVal.Sort()
            Return _rVal
        End Function

        Public Function MemberSets() As List(Of List(Of dxfAttribute))
            Dim _rVal As New List(Of List(Of dxfAttribute))
            Dim tags As List(Of String) = TagList()
            For Each tag As String In tags
                _rVal.Add(Members(tag))
            Next
            Return _rVal
        End Function
        Public Function MemberSets(ByRef rTags As List(Of String)) As List(Of List(Of dxfAttribute))
            Dim _rVal As New List(Of List(Of dxfAttribute))
            rTags = TagList()
            For Each tag As String In rTags
                Dim mems As List(Of dxfAttribute) = Members(tag)
                If mems.Count > 0 Then _rVal.Add(mems)
            Next
            Return _rVal
        End Function
        Public Overloads Sub Remove(aIndex As Integer)
            If aIndex < 1 Or aIndex > Count Then Return
            MyBase.RemoveAt(aIndex - 1)
        End Sub
        Public Shadows Function Item(aTag As String, Optional aOccurance As Integer = 1) As dxfAttribute
            If String.IsNullOrWhiteSpace(aTag) Then Return Nothing

            Dim _rVal As dxfAttribute = Nothing

            aTag = aTag.Trim().Replace(" ", "_")
            If Count <= 0 Then Return Nothing
            Dim attmems As List(Of dxfAttribute) = Members(aTag)
            If attmems.Count <= 0 Then Return Nothing
            If aOccurance <= 1 Then Return attmems(0)
            If aOccurance > attmems.Count Then Return Nothing
            Return attmems(aOccurance - 1)

        End Function
        Public Shadows Function Item(aIndex As Integer) As dxfAttribute
            If aIndex < 1 Or aIndex > Count Then Return Nothing
            Return MyBase.Item(aIndex - 1)
        End Function
        Public Shadows Function Members(aTag As String) As List(Of dxfAttribute)
            If String.IsNullOrWhiteSpace(aTag) Then Return Nothing

            Dim _rVal As dxfAttribute = Nothing

            aTag = aTag.Trim().Replace(" ", "_")
            If Count <= 0 Then Return Nothing
            Return FindAll(Function(x) String.Compare(x.Tag, aTag, False) = 0)


        End Function
        ''' <summary>
        ''' Sets the value of the indicated attribute to the passed value
        ''' </summary>
        ''' <remarks>passing a occurance value of zero sets all the attributes with a matching tag to the passed value</remarks>
        ''' <param name="aTag">the tag of the attribute to apply the passed value to</param>
        ''' <param name="aValue">the value to assign to the attribute(s)</param>
        ''' <param name="aOccurance">the occurance number of the attribute to apply the passed value to</param>
        ''' <param name="bAddItNotFound">if no members have the passed tag and this argument is true, an new attribute with the passed tag value is added </param>
        ''' <returns>true if one of the members was changed</returns>
        Public Function SetValue(aTag As String, aValue As String, Optional aOccurance As Integer = 1, Optional bAddItNotFound As Boolean = False) As Boolean

            Dim rFound As Boolean
            Return SetValue(aTag:=aTag, aValue:=aValue, aOccurance:=aOccurance, bAddItNotFound:=bAddItNotFound, rFound:=rFound)
        End Function
        ''' <summary>
        ''' Sets the value of the indicated attribute to the passed value
        ''' </summary>
        ''' <remarks>passing a occurance value of zero sets all the attributes with a matching tag to the passed value</remarks>
        ''' <param name="aTag">the tag of the attribute to apply the passed value to</param>
        ''' <param name="aValue">the value to assign to the attribute(s)</param>
        ''' <param name="aOccurance">the occurance number of the attribute to apply the passed value to</param>
        ''' <param name="bAddItNotFound">if no members have the passed tag and this argument is true, an new attribute with the passed tag value is added </param>
        ''' <param name="rFound">returns false if no members have the passed tag and occurance  </param>
        ''' <returns>true if one of the members was changed</returns>
        Public Function SetValue(aTag As String, aValue As Object, aOccurance As Integer, bAddItNotFound As Boolean, ByRef rFound As Boolean) As Boolean


            If String.IsNullOrWhiteSpace(aTag) Then Return False
            aTag = aTag.Trim().Replace(" ", "_")

            Dim _rVal As Boolean = False
            Dim sval As String = String.Empty
            If aValue IsNot Nothing Then sval = aValue.ToString().Trim()
            If aOccurance <= 0 Then
                Dim mems As List(Of dxfAttribute) = Members(aTag)
                rFound = mems.Count <= 0
                For Each mem As dxfAttribute In mems
                    If String.Compare(sval, mem.Value, False) <> 0 Then _rVal = True
                    mem.Value = sval
                Next
            Else
                Dim aMem As dxfAttribute = Item(aTag, aOccurance)
                rFound = aMem IsNot Nothing
                If Not rFound Then
                    If bAddItNotFound Then
                        Add(New dxfAttribute() With {.Tag = aTag, .Value = sval})
                        IsDirty = True
                        Return True
                    End If
                    Return False
                End If
                _rVal = String.Compare(sval, aMem.Value, False) <> 0
                aMem.Value = sval
            End If

            If _rVal Then IsDirty = True
            Return _rVal

        End Function
        Public Function Tag(aIndex As Integer) As String
            If aIndex >= 1 And aIndex <= Count Then
                Return Item(aIndex).Tag
            Else
                Return String.Empty
            End If
        End Function
#End Region 'Methods



#Region "Shared Methods"
        Friend Shared Function AssignToText(aAttribs As TPROPERTIES, aAttrib As dxeText, Optional bResetMarks As Boolean = False) As Boolean
            Dim _rVal As Boolean
            Dim aProp As TPROPERTY
            If bResetMarks Then
                For j As Integer = 1 To aAttribs.Count
                    aProp = aAttribs.Item(j)
                    aProp.Mark = False
                    aAttribs.SetItem(j, aProp)
                Next j
            End If
            For j As Integer = 1 To aAttribs.Count
                aProp = aAttribs.Item(j)
                If String.Compare(aProp.Name, aAttrib.AttributeTag, True) = 0 Then
                    If aProp.Mark = False Then
                        aProp.Mark = True
                        aAttribs.SetItem(j, aProp)
                        _rVal = aAttrib.TextString <> aProp.StringValue
                        aAttrib.TextString = aAttribs.ValueStr(j)
                        Exit For
                    End If
                End If
            Next j
            Return _rVal
        End Function
        Friend Shared Function AddTagValue(ByRef aAttribs As TPROPERTIES, aTag As String, aValue As Object, Optional aPrompt As String = Nothing, Optional aCopies As Integer = 0) As TPROPERTY
            If String.IsNullOrWhiteSpace(aTag) Then Return TPROPERTY.Null
            If aValue Is Nothing Then aValue = ""
            Dim _rVal As New TPROPERTY(1, aValue.ToString().Trim(), aTag.Trim().Replace(" ", "_"), dxxPropertyTypes.dxf_String)
            If aPrompt IsNot Nothing Then _rVal.Prompt = aPrompt.Trim
            aAttribs.Add(_rVal)
            For i As Integer = 2 To aCopies
                aAttribs.Add(_rVal)
            Next i
            Return _rVal
        End Function


        Friend Shared Function AssignToTextEntities(aAttribs As TPROPERTIES, aEntities As IEnumerable(Of dxfEntity)) As Boolean
            Dim rAttribs As dxfAttributes = Nothing
            Return AssignToTextEntities(aAttribs, aEntities, rAttribs)
        End Function
        Friend Shared Function AssignToTextEntities(aAttribs As TPROPERTIES, aEntities As IEnumerable(Of dxfEntity), ByRef rAttribs As dxfAttributes) As Boolean
            Dim _rVal As Boolean
            rAttribs = New dxfAttributes
            If aEntities Is Nothing Or aAttribs.Count <= 0 Then Return _rVal

            Dim cnt As Integer

            For Each aEnt As dxfEntity In aEntities
                If aEnt.GraphicType = dxxGraphicTypes.Text Then
                    Dim aAttrib As dxeText = aEnt
                    If aAttrib.TextType = dxxTextTypes.AttDef Or aAttrib.TextType = dxxTextTypes.Attribute Then
                        If dxfAttributes.AssignToText(aAttribs, aAttrib, cnt = 0) Then _rVal = True
                        rAttribs.Add(aAttrib.AttribV)
                        cnt += 1
                    End If
                End If
            Next
            Return _rVal
        End Function
        Friend Shared Function ClearValues(ByRef ioAttribs As TPROPERTIES, Optional aTag As String = "") As Integer
            If String.IsNullOrWhiteSpace(aTag) Then aTag = ""
            Dim _rVal As Integer
            '#1the tag to clear
            '^set the values of the members with the passed tag to the null sting
            '~if a null string is passed all members values are set to the null string.
            '~returns the number of members whose values changed from the operation
            aTag = aTag.Trim.Replace(" ", "_")
            Dim aProp As TPROPERTY
            For i As Integer = 1 To ioAttribs.Count
                aProp = ioAttribs.Item(i)
                If aTag = "" Then
                    If aProp.Value <> "" Then _rVal += 1
                    aProp.Value = ""
                Else
                    If String.Compare(aProp.Name, aTag, True) = 0 Then
                        If aProp.Value <> "" Then _rVal += 1
                        aProp.Value = ""
                    End If
                End If
                ioAttribs.SetItem(i, aProp)
            Next i
            Return _rVal
        End Function

        Friend Shared Function GetAttribute(aAttribs As TPROPERTIES, aTag As String, aOccurance As Integer, ByRef rIndex As Integer) As TPROPERTY
            Dim _rVal As New TPROPERTY
            rIndex = 0
            aTag = Replace(Trim(aTag), " ", "_")
            If aAttribs.Count <= 0 Then Return _rVal
            Dim i As Integer
            Dim aMem As New TPROPERTY
            Dim cnt As Integer
            Dim bRev As Boolean
            Dim si As Integer
            Dim ei As Integer
            Dim stp As Integer
            bRev = aOccurance < 0
            If aOccurance = 0 Then aOccurance = 1
            If Not bRev Then
                si = 1
                ei = aAttribs.Count
                stp = 1
            Else
                si = aAttribs.Count
                ei = 1
                stp = -1
            End If
            For i = si To ei Step stp
                aMem = aAttribs.Item(i)
                If String.Compare(aMem.Name, aTag, True) = 0 Then
                    cnt += 1
                    If cnt = Math.Abs(aOccurance) Then
                        rIndex = i
                        _rVal = aMem
                        Exit For
                    End If
                End If
            Next i
            Return _rVal
        End Function

        Friend Shared Function GetAttribute(ByRef ioAttribs As IEnumerable(Of dxfAttribute), aTag As String, aOccurance As Integer) As dxfAttribute
            If String.IsNullOrWhiteSpace(aTag) Or ioAttribs Is Nothing Then Return Nothing

            Dim _rVal As dxfAttribute = Nothing

            aTag = aTag.Trim().Replace(" ", "_")
            If ioAttribs.Count <= 0 Then Return Nothing
            Dim attmems As List(Of dxfAttribute) = ioAttribs.ToList().FindAll(Function(x) String.Compare(x.Tag, aTag, True) = 0)
            If attmems.Count <= 0 Then Return Nothing
            If aOccurance <= 1 Then Return attmems(0)
            If aOccurance > attmems.Count Then Return Nothing
            Return attmems(aOccurance - 1)

        End Function
        Friend Shared Function GetByTag(aAttribs As TPROPERTIES, aTag As String) As TPROPERTIES
            Dim _rVal As New TPROPERTIES(aTag)
            If String.IsNullOrWhiteSpace(aTag) Or aAttribs.Count <= 0 Then Return _rVal
            aTag = aTag.Trim.Replace(" ", "_")
            Dim aMem As TPROPERTY
            For i As Integer = 1 To aAttribs.Count
                aMem = aAttribs.Item(i)
                If String.Compare(aMem.Name, aTag, True) = 0 Then
                    _rVal.Add(aMem)
                End If
            Next i
            Return _rVal
        End Function
        Friend Shared Function GetTags(aAttribs As TPROPERTIES) As String
            Dim _rVal As String = String.Empty
            For i As Integer = 1 To aAttribs.Count
                TLISTS.Add(_rVal, aAttribs.Item(i).Name)
            Next i
            Return _rVal
        End Function

        Private Shared _IndependantPropertyNames As List(Of String)
        Public Shared ReadOnly Property IndependantPropertyNames As List(Of String)
            Get
                If _IndependantPropertyNames Is Nothing Then
                    _IndependantPropertyNames = dxpProperties.Get_EntityProperties(dxxGraphicTypes.Text, String.Empty, aTextType:=dxxTextTypes.DText).NamesByGroupCode(IndependantGroupCodes)
                End If
                Return _IndependantPropertyNames
            End Get
        End Property

        Private Shared _IndependantGroupCodes As List(Of Integer)
        Public Shared ReadOnly Property IndependantGroupCodes As List(Of Integer)
            Get
                If _IndependantGroupCodes Is Nothing Then
                    _IndependantGroupCodes = New List(Of Integer)({1, 6, 7, 8, 40, 41, 50, 51, 62, 72, 74})
                End If
                Return _IndependantGroupCodes
            End Get
        End Property
#End Region 'Shared Methods
    End Class 'dxfAttributes
End Namespace
