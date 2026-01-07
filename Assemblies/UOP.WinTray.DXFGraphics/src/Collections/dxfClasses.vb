Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class colDXFClasses
        Inherits List(Of dxfClass)

        Public Sub New()
            MyBase.New()
            ImageGUID = ""
        End Sub

        Friend Sub New(aClasses As TDXFCLASSES)
            MyBase.New()
            For i As Integer = 1 To aClasses.Count
                Add(New dxfClass(aClasses.Item(i)))
            Next
            ImageGUID = aClasses.ImageGUID
        End Sub

        Public Property ImageGUID As String

        Public Shadows Function Item(aIndex As Integer) As dxfClass
            If aIndex < 0 Or aIndex > Count Then Return Nothing
            Dim _rVal As dxfClass = MyBase.Item(aIndex - 1)
            _rVal.Index = aIndex
            _rVal.ImageGUID = ImageGUID
            Return _rVal
        End Function

        Public Shadows Function IndexOf(aClass As dxfClass) As Integer
            Return MyBase.IndexOf(aClass) + 1
        End Function

        Public Shadows Sub Add(aClass As dxfClass)
            If aClass Is Nothing Then Return
            aClass.Index = Count + 1
            aClass.ImageGUID = ImageGUID
            MyBase.Add(aClass)
        End Sub

        Friend Shadows Sub Add(aName As String, aClassName As String, Optional aAppName As String = "ObjectDBX Classes", Optional aProxyVal As Integer = 0, Optional aProxyFlag As Boolean = False, Optional aEntityFlag As Boolean = False, Optional aObjectName As String = "")
            Dim aCls As New dxfClass(aName)

            aCls.Properties.SetVal("Name", aClassName)
            aCls.Properties.SetVal("Application", aAppName)
            aCls.Properties.SetVal("Proxy Flag", aProxyVal)
            aCls.Properties.SetVal("Was Proxy Switch", aProxyFlag)
            aCls.Properties.SetVal("Entity Flag", aEntityFlag)
            aCls.ObjectName = Trim(aObjectName)
            Add(aCls)
        End Sub

        Public Overrides Function ToString() As String
            Return $"dxfClasses"
        End Function
        Public Function Clone() As colDXFClasses
            Return New colDXFClasses(Structure_Get())
        End Function
        Friend Sub Structure_Set(aClasses As TDXFCLASSES)

            ImageGUID = aClasses.ImageGUID
            Clear()
            For i As Integer = 1 To aClasses.Count
                Add(New dxfClass(aClasses.Item(i)))
            Next
            ImageGUID = aClasses.ImageGUID
        End Sub

        Friend Function Structure_Get() As TDXFCLASSES
            Dim _rVal As New TDXFCLASSES With {.ImageGUID = ImageGUID}
            For Each mem As dxfClass In Me
                _rVal.Add(mem.Structure_Get)
                Return _rVal

            Next
            Return _rVal
        End Function
    End Class

    Public Class dxfClass
#Region "Members"
        Public ImageGUID As String

        Private _ObjectName As String

        Public Property Properties As dxoProperties
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aName As String = "")
            Properties = dxfClass.DefaultProperties(aName)
            _ObjectName = ""
            ImageGUID = ""
            Index = 0
        End Sub
        Friend Sub New(aClass As TDXFCLASS)
            Properties = dxfClass.DefaultProperties(aClass.Name)
            Properties.CopyVals(New dxoProperties(aClass._Props), bSkipHandles:=False, bSkipPointers:=False, bCopyNewMembers:=True)
            _ObjectName = ""
            ImageGUID = ""


        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Name As String
            Get
                Return Properties.ValueS("Name")
            End Get
        End Property
        Public Property Handle As String
            Get
                Return Properties.ValueS(5)
            End Get
            Set(value As String)
                Properties.SetVal(5, value)
            End Set
        End Property

        Public ReadOnly Property SubClass As String
            Get
                Return Properties.ValueS("SubClass")
            End Get
        End Property
        Public Property Index As Integer


        Public Property ObjectName As String
            Get
                Return _ObjectName
            End Get
            Set(value As String)
                _ObjectName = value
            End Set
        End Property
#End Region 'Properties
#Region "Methods"

        Public Overrides Function ToString() As String
            Return $"dxfClass [{Name}]"
        End Function

        Friend Sub Structure_Set(aClass As TDXFCLASS)
            Properties = New dxoProperties(aClass._Props)
            ObjectName = aClass.ObjectName
            _Index = aClass.Index
            ImageGUID = aClass.ImageGUID

        End Sub

        Friend Function Structure_Get() As TDXFCLASS
            Return New TDXFCLASS With
            {
             ._Props = New TPROPERTIES(Properties),
                .ObjectName = ObjectName,
            .Index = Index,
            .ImageGUID = ImageGUID
            }


        End Function
        Public Function Clone() As dxfClass
            Return New dxfClass(Structure_Get())
        End Function
#End Region 'Methods
#Region "Shared Methods"
        Public Shared Function DefaultProperties(Optional aName As String = "") As dxoProperties
            Dim _rVal As New dxoProperties

            _rVal.AddTo(0, "CLASS", dxxPropertyTypes.dxf_String)
            _rVal.AddTo(1, aName, "Name", dxxPropertyTypes.dxf_String)
            _rVal.AddTo(2, "", "SubClass", dxxPropertyTypes.dxf_String)
            _rVal.AddTo(3, "", "Application", dxxPropertyTypes.dxf_String)
            _rVal.AddTo(90, 0, "Proxy Flag")
            _rVal.AddTo(91, 0, "Ref Count")
            _rVal.AddTo(280, False, "Was Proxy Switch", dxxPropertyTypes.Switch)
            _rVal.AddTo(281, False, "Entity Flag", dxxPropertyTypes.Switch)

            Return _rVal
        End Function

        Private Shared Property _DefaultClass As colDXFClasses

        Public Shared Function DefaultClasses(Optional aVersion As dxxACADVersions = dxxACADVersions.R2000) As colDXFClasses
            Dim _rVal As New colDXFClasses
            If _DefaultClass Is Nothing Then
                _DefaultClass.Add("ACDBDICTIONARYWDFLT", "AcDbDictionaryWithDefault")
                _DefaultClass.Add("ACDBPLACEHOLDER", "AcDbPlaceHolder")
                _DefaultClass.Add("ARCALIGNEDTEXT", "AcDbArcAlignedText", aEntityFlag:=True)
                _DefaultClass.Add("DICTIONARYVAR", "AcDbDictionaryVar", aObjectName:="DictionaryVariables")
                _DefaultClass.Add("CELLSTYLEMAP", "AcDbCellStyleMap", aProxyVal:=1152)
                _DefaultClass.Add("HATCH", "AcDbHatch", aEntityFlag:=True)
                _DefaultClass.Add("IDBUFFER", "AcDbIdBuffer")
                _DefaultClass.Add("IMAGE", "AcDbRasterImage", aProxyVal:=127, aEntityFlag:=True)
                _DefaultClass.Add("IMAGEDEF", "AcDbRasterImageDef")
                _DefaultClass.Add("IMAGEDEF_REACTOR", "AcDbRasterImageDefReactor", aProxyVal:=1)
                _DefaultClass.Add("LAYER_INDEX", "AcDbLayerIndex")
                _DefaultClass.Add("LAYOUT", "AcDbLayout")
                _DefaultClass.Add("LWPOLYLINE", "AcDbPolyline", aEntityFlag:=True)
                _DefaultClass.Add("MATERIAL", "AcDbMaterial", aProxyVal:=1153)
                _DefaultClass.Add("MLEADERSTYLE", "AcDbMLeaderStyle", "ACDB_MLEADERSTYLE_CLASS", aProxyVal:=4095)
                _DefaultClass.Add("OBJECT_PTR", "CAseDLPNTAbleRecord", aProxyVal:=1)
                _DefaultClass.Add("OLE2FRAME", "AcDbOleFrame2", aEntityFlag:=True)
                _DefaultClass.Add("PLOTSETTINGS", "AcDbPlotSettings")
                _DefaultClass.Add("RASTERVARIABLES", "AcDbRasterVariables")
                _DefaultClass.Add("RTEXT", "RText", aEntityFlag:=True)
                _DefaultClass.Add("SCALE", "AcDbScale", aProxyVal:=1153)
                _DefaultClass.Add("SORTENTSTABLE", "AcDbSortentsTable")
                _DefaultClass.Add("SPATIAL_INDEX", "AcDbSpatialIndex")
                _DefaultClass.Add("SPATIAL_FILTER", "AcDbSpatialFilter")
                _DefaultClass.Add("TABLESTYLE", "AcDbTableStyle", aProxyVal:=4095)
                _DefaultClass.Add("VISUALSTYLE", "AcDbVisualStyle", aProxyVal:=4095)
                _DefaultClass.Add("WIPEOUT", "AcDbWipeout", aProxyVal:=4095, aEntityFlag:=True)
                _DefaultClass.Add("WIPEOUTVARIABLES", "AcDbWipeoutVariables")
            End If
            Return _DefaultClass.Clone

        End Function
#End Region 'Shared MEthods



    End Class


#Region "Structures"
    Friend Structure TDXFCLASS
        Implements ICloneable
#Region "Members"
        Public ImageGUID As String
        Private _Index As Integer
        Public ObjectName As String
        Friend _Props As TPROPERTIES
#End Region 'Members
#Region "Constructors"
        Public Sub New(Optional aName As String = "")
            'init --------------------------------------
            ImageGUID = ""
            _Index = 0
            ObjectName = ""
            _Props = New TPROPERTIES(dxfClass.DefaultProperties(aName))
            'init --------------------------------------
        End Sub

        Public Sub New(aClass As TDXFCLASS)
            'init --------------------------------------
            ImageGUID = aClass.ImageGUID
            _Index = aClass.Index
            ObjectName = aClass.ObjectName
            _Props = New TPROPERTIES(aClass._Props)
            'init --------------------------------------
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Name As String
            Get
                Return _Props.ValueStr("Name")
            End Get
        End Property
        Public ReadOnly Property SubClass As String
            Get
                Return _Props.ValueStr("SubClass")
            End Get
        End Property
        Public Property Index As Integer
            Get
                Return _Index
            End Get
            Friend Set(value As Integer)
                _Index = value
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Public Function Clone() As TDXFCLASS
            Return New TDXFCLASS(Me)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TDXFCLASS(Me)
        End Function
#End Region 'Methods
#Region "Shared Methods"

#End Region 'Shared MEthods
    End Structure 'TDXFCLASS
    Friend Structure TDXFCLASSES
        Implements ICloneable
#Region "Members"
        Private _Init As Boolean
        Private _ImageGUID As String
        Friend _Members() As TDXFCLASS
        Public ReadFromFile As Boolean
#End Region 'Members
#Region "Constructors"

        Public Sub New(Optional aImageGUID As String = "")
            'init --------------------
            ReDim _Members(-1)
            _Init = True
            _ImageGUID = aImageGUID
            ReadFromFile = False
            'init --------------------
        End Sub
        Public Sub New(aClasses As TDXFCLASSES, Optional bReturnEmpty As Boolean = False, Optional aImageGUID As String = Nothing)
            'init --------------------
            ReDim _Members(-1)
            _Init = True
            _ImageGUID = ""
            ReadFromFile = aClasses.ReadFromFile
            'init --------------------

            If aClasses._Init And Not bReturnEmpty Then _Members = aClasses._Members.Clone()
            If Not String.IsNullOrWhiteSpace(aImageGUID) Then ImageGUID = aClasses.ImageGUID Else ImageGUID = aImageGUID

        End Sub
#End Region 'Constructors
        Public Property ImageGUID As String
            Get
                Return _ImageGUID
            End Get
            Set(value As String)
                If value <> _ImageGUID Then
                    _ImageGUID = value
                    For i As Integer = 1 To Count
                        _Members(i - 1).Index = i
                        _Members(i - 1).ImageGUID = _ImageGUID
                    Next
                End If
            End Set
        End Property
        Public ReadOnly Property Count As Integer
            Get
                If Not _Init Then
                    _Init = True
                    ReDim _Members(-1)
                End If
                Return _Members.Count
            End Get
        End Property
#Region "Methods"
        Public Function Item(aIndex As Integer) As TDXFCLASS
            If aIndex < 1 Or aIndex > Count Then
                If dxfUtils.RunningInIDE Then Throw New IndexOutOfRangeException()
                Return New TDXFCLASS("")
            End If
            _Members(aIndex - 1).Index = aIndex
            _Members(aIndex - 1).ImageGUID = ImageGUID
            Return _Members(aIndex - 1)
        End Function
        Public Sub SetItem(aIndex As Integer, value As TDXFCLASS)
            If aIndex < 1 Or aIndex > Count Then Return
            _Members(aIndex - 1) = value
            _Members(aIndex - 1).Index = aIndex
            _Members(aIndex - 1).ImageGUID = ImageGUID
        End Sub
        Public Sub Add(aClass As TDXFCLASS)
            If Count >= Integer.MaxValue Or aClass._Props.Count <= 0 Then Return
            System.Array.Resize(_Members, Count + 1)
            _Members(_Members.Count - 1) = aClass
            _Members(_Members.Count - 1).ImageGUID = ImageGUID
        End Sub
        Friend Sub Add(aName As String, aClassName As String, Optional aAppName As String = "ObjectDBX Classes", Optional aProxyVal As Integer = 0, Optional aProxyFlag As Boolean = False, Optional aEntityFlag As Boolean = False, Optional aObjectName As String = "")
            Dim aCls As New TDXFCLASS(aName)
            aCls._Props.SetVal("Name", aClassName)
            aCls._Props.SetVal("Application", aAppName)
            aCls._Props.SetVal("Proxy Flag", aProxyVal)
            aCls._Props.SetVal("Was Proxy Switch", aProxyFlag)
            aCls._Props.SetVal("Entity Flag", aEntityFlag)
            aCls.ObjectName = Trim(aObjectName)
            Add(aCls)
        End Sub
        Public Sub Clear()
            _Init = True
            ReDim _Members(-1)

        End Sub
        Public Function Clone(Optional bReturnEmpty As Boolean = False) As TDXFCLASSES
            Return New TDXFCLASSES(Me, bReturnEmpty)

        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return New TDXFCLASSES(Me)
        End Function

        Public Function GetByName(aName As String) As TDXFCLASS
            Dim _rVal As New TDXFCLASS("") With {.Index = -1}

            aName = Trim(aName)
            If Count <= 0 Or String.IsNullOrWhiteSpace(aName) Then Return _rVal
            Dim i As Integer
            Dim aMem As TDXFCLASS
            For i = 1 To Count
                aMem = Item(i)
                If String.Compare(aMem.Name, aName, ignoreCase:=True) = 0 Or String.Compare(aMem.ObjectName, aName, ignoreCase:=True) = 0 Then
                    Return aMem
                End If
            Next i
            Return _rVal
        End Function
        Friend Function Contains(aName As String, ByRef rIndex As Integer) As Boolean
            rIndex = 0
            aName = Trim(aName)
            If Count <= 0 Or aName = "" Then Return False
            Dim _rVal As Boolean
            Dim i As Integer
            Dim aMem As TDXFCLASS
            For i = 1 To Count
                aMem = Item(i)
                If String.Compare(aMem.Name, aName, True) = 0 Or String.Compare(aMem.ObjectName, aName, True) = 0 Then
                    rIndex = i
                    _rVal = True
                    Exit For
                End If
            Next i
            Return _rVal
        End Function
#End Region 'Methods
#Region "Shared Methods"

#End Region
    End Structure 'TDXFCLASSES
#End Region 'Structures

End Namespace
