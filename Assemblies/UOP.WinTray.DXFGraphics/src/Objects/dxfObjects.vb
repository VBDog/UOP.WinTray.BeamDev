
Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxfoMLeaderStyle
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.MLeaderStyle)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoMLeaderStyle With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoSortEntsTable
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.SortEntsTable)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoSortEntsTable With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoMaterial
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.Material)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoMaterial With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoScale
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.Scale)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoScale With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoVisualStyle
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.VisualStyle)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoVisualStyle With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoDictionaryVar
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.DictionaryVar)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoDictionaryVar With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoPlaceHolder
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.PlaceHolder)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoPlaceHolder With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoCellStyleMap
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.CellStyleMap)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoCellStyleMap With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoDictionary
        Inherits dxfObject
#Region "Members"
#End Region 'Members
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.Dictionary)
            Entries = New TDICTIONARYENTRIES(3, 350)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT, Optional aNameGC As Integer = 3, Optional aHandleGC As Integer = 350)
            MyBase.New(aObject)
            Entries = New TDICTIONARYENTRIES(aNameGC, aHandleGC)
        End Sub
        Friend Sub New(aName As String, Optional aNameGC As Integer = 3, Optional aHandleGC As Integer = 350)
            MyBase.New(dxxObjectTypes.Dictionary, aName)
            Entries = New TDICTIONARYENTRIES(aNameGC, aHandleGC)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            Dim myProps As TPROPERTIES = Props.Clone
            myProps.Append(Entries.Properties(False))
            _rVal.Add(myProps, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoDictionary With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function



#End Region 'Methods
    End Class
    Public Class dxfoDictionaryWDFLT
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.DictionaryWDFLT)
            Entries = New TDICTIONARYENTRIES(3, 350)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT, Optional aNameGC As Integer = 3, Optional aHandleGC As Integer = 350)
            MyBase.New(aObject)
            Entries = New TDICTIONARYENTRIES(aNameGC, aHandleGC)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            Dim myProps As TPROPERTIES = Props.Clone
            myProps.Append(Entries.Properties(False))
            _rVal.Add(myProps, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoDictionaryWDFLT With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoXRecord
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.XRecord)
        End Sub
        Public Sub New(aName As String)
            MyBase.New(dxxObjectTypes.XRecord)
            Name = aName
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoXRecord With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoLayout
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.Layout)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoLayout With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoPlotSetting
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.PlotSetting)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoPlotSetting With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoProxyObject
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.ProxyObject)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoProxyObject With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoTableStyle
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.TableStyle)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoTableStyle With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoTableCell
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.TableCell)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoTableCell With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoGroup
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.Group)
            Entries = New TDICTIONARYENTRIES(2, 340)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
            Entries = New TDICTIONARYENTRIES(2, 340)
        End Sub
        Public Sub New(aName As String)
            MyBase.New(dxxObjectTypes.Group)
            Entries = New TDICTIONARYENTRIES(2, 340)
            Name = aName
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public Property Description As String
            Get
                Return Props.GCValueStr(300)
            End Get
            Set(value As String)
                SetProperty(300, value, aOccurance:=1)
            End Set
        End Property
        Public Property Selectable As Boolean
            Get
                Return Props.ValueB(71)
            End Get
            Set(value As Boolean)
                SetProperty(71, value, aOccurance:=1)
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            Dim myProps As TPROPERTIES = Props.Clone
            myProps.Append(Entries.Properties(True))
            _rVal.Add(myProps, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoGroup With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoMLineStyle
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.MLineStyle)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoMLineStyle With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoDataTable
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.DataTable)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoDataTable With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoDimAssoc
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.DimAssoc)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoDimAssoc With {
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoField
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.Field)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoField With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoIDBuffer
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.IDBuffer)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoIDBuffer With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoImageDef
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.ImageDef)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoImageDef With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoImageDefReactor
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.ImageDefReactor)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoImageDefReactor With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoLayerIndex
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.LayerIndex)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoLayerIndex With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoLayerFilter
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.LayerFilter)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoLayerFilter With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoLightList
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.LightList)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoLightList With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoObjectPtr
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.ObjectPtr)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoObjectPtr With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoRasterVariables
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.RasterVariables)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoRasterVariables With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoRenderEnvironment
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.RenderEnvironment)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoRenderEnvironment With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoSectionManager
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.SectionManager)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoSectionManager With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoSpatialIndex
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.SpatialIndex)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoSpatialIndex With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoSpatialFilter
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.SpatialFilter)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoSpatialFilter With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoSunStudy
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.SunStudy)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoSunStudy With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoUnderlayDefinition
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.UnderlayDefinition)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoUnderlayDefinition With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoVBAProject
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.VBAProject)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoVBAProject With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoWipeoutVariables
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.WipeoutVariables)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoWipeoutVariables With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class
    Public Class dxfoUserDefined
        Inherits dxfObject
#Region "Constructors"
        Public Sub New()
            MyBase.New(dxxObjectTypes.UserDefined)
        End Sub
        Friend Sub New(aObject As TDXFOBJECT)
            MyBase.New(aObject)
        End Sub
#End Region 'Constructors
#Region "Properties"
#End Region 'Properties
#Region "Methods"
        Friend Overrides Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Dim _rVal As New TPROPERTYARRAY(GUID)
            _rVal.Add(Props, "Properties")
            Return _rVal
        End Function
        Friend Overrides Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Dim _rVal As New dxoPropertyMatrix("", ExtendedData)
            Dim tname As String = String.Empty
            _rVal.Add(DXFProps(aInstances, 1, TPLANE.World, tname, aImage:=aImage), GUID)
            Return _rVal
        End Function
        Public Overrides Function Clone() As dxfObject
            Dim _rVal As New dxfoUserDefined With {
                .Strukture = Strukture.Clone,
                .Entries = Entries.Clone,
                .Handlez = Handlez
            }
            Return _rVal
        End Function
#End Region 'Methods
    End Class

End Namespace
