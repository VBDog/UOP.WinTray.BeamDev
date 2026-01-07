Imports System.Security.AccessControl
Imports SharpDX.Direct2D1
Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxoTables
        Inherits List(Of dxfTable)
        Implements IEnumerable(Of dxfTable)
        Implements IDisposable


        Private disposedValue As Boolean

        Public Sub New()

            '-------------------- init
            Add(New dxoAPPIDs())
            Add(New dxoBlockRecords())
            Add(New dxoDimStyles())
            Add(New dxoLayers())
            Add(New dxoLayers())
            Add(New dxoLineTypes())
            Add(New dxoStyles())
            Add(New dxoUCSs())
            Add(New dxoViews())
            Add(New dxoViewPorts())
            '-------------------- init
        End Sub
        Public Sub New(bAddDefaultMembers As Boolean, Optional aImage As dxfImage = Nothing)

            If aImage IsNot Nothing Then ImageGUID = aImage.GUID
            '-------------------- init
            Add(New dxoAPPIDs(bAddDefaultMembers:=bAddDefaultMembers, aImage:=aImage))
            Add(New dxoLineTypes(bAddDefaultMembers:=bAddDefaultMembers, aImage:=aImage))
            Add(New dxoLayers(bAddDefaultMembers:=bAddDefaultMembers, aImage:=aImage))
            Add(New dxoStyles(bAddDefaultMembers:=bAddDefaultMembers, aImage:=aImage))
            Add(New dxoDimStyles(bAddDefaultMembers:=bAddDefaultMembers, aImage:=aImage))
            Add(New dxoBlockRecords(bAddDefaultMembers:=bAddDefaultMembers, aImage:=aImage))
            Add(New dxoUCSs(bAddDefaultMembers:=bAddDefaultMembers, aImage:=aImage))
            Add(New dxoViews(bAddDefaultMembers:=bAddDefaultMembers, aImage:=aImage))
            Add(New dxoViewPorts(bAddDefaultMembers:=bAddDefaultMembers, aImage:=aImage))
            '-------------------- init


        End Sub

        Friend Property ImageGUID As String = String.Empty

        Public ReadOnly Property APPIDs As dxoAPPIDs
            Get
                Dim _rVal As dxfTable = Nothing
                If Not TryGetTable(dxxReferenceTypes.APPID, _rVal, True) Then Return Nothing Else Return DirectCast(_rVal, dxoAPPIDs)
            End Get
        End Property

        Public ReadOnly Property ViewPorts As dxoViewPorts
            Get
                Dim _rVal As dxfTable = Nothing
                If Not TryGetTable(dxxReferenceTypes.VPORT, _rVal, True) Then Return Nothing Else Return DirectCast(_rVal, dxoViewPorts)
            End Get
        End Property
        Public ReadOnly Property BlockRecords As dxoBlockRecords
            Get
                Dim _rVal As dxfTable = Nothing
                If Not TryGetTable(dxxReferenceTypes.BLOCK_RECORD, _rVal, True) Then Return Nothing Else Return DirectCast(_rVal, dxoBlockRecords)
            End Get
        End Property
        Public ReadOnly Property LineTypes As dxoLineTypes
            Get
                Dim _rVal As dxfTable = Nothing
                If Not TryGetTable(dxxReferenceTypes.LTYPE, _rVal, True) Then Return Nothing Else Return DirectCast(_rVal, dxoLineTypes)
            End Get
        End Property
        Public ReadOnly Property Layers As dxoLayers
            Get
                Dim _rVal As dxfTable = Nothing
                If Not TryGetTable(dxxReferenceTypes.LAYER, _rVal, True) Then Return Nothing Else Return DirectCast(_rVal, dxoLayers)
            End Get
        End Property

        Public ReadOnly Property Styles As dxoStyles
            Get
                Dim _rVal As dxfTable = Nothing
                If Not TryGetTable(dxxReferenceTypes.STYLE, _rVal, True) Then Return Nothing Else Return DirectCast(_rVal, dxoStyles)
            End Get
        End Property
        Public ReadOnly Property DimStyles As dxoDimStyles
            Get
                Dim _rVal As dxfTable = Nothing
                If Not TryGetTable(dxxReferenceTypes.DIMSTYLE, _rVal, True) Then Return Nothing Else Return DirectCast(_rVal, dxoDimStyles)
            End Get
        End Property

        Public ReadOnly Property UCSs As dxoUCSs
            Get
                Dim _rVal As dxfTable = Nothing
                If Not TryGetTable(dxxReferenceTypes.UCS, _rVal, True) Then Return Nothing Else Return DirectCast(_rVal, dxoUCSs)
            End Get
        End Property
        Public ReadOnly Property Views As dxoViews
            Get
                Dim _rVal As dxfTable = Nothing
                If Not TryGetTable(dxxReferenceTypes.VIEW, _rVal, True) Then Return Nothing Else Return DirectCast(_rVal, dxoViews)
            End Get
        End Property


        Public Function Entry(aTableType As dxxReferenceTypes, aEntryName As String) As dxfTableEntry
            If String.IsNullOrWhiteSpace(aEntryName) Then Return Nothing
            Dim mytable As dxfTable = Table(aTableType)
            If mytable Is Nothing Then Return Nothing
            Return mytable.Find(Function(x) String.Compare(x.Name, aEntryName, True) = 0)
        End Function

        Friend Function Entry(aTableType As dxxReferenceTypes, aEntryName As String, aImage As dxfImage) As dxfTableEntry
            If String.IsNullOrWhiteSpace(aEntryName) Then Return Nothing
            Dim mytable As dxfTable = Table(aTableType, aImage)
            If mytable Is Nothing Then Return Nothing
            Return mytable.Find(Function(x) String.Compare(x.Name, aEntryName, True) = 0)
        End Function

        Public Function ReferenceExists(aRefType As dxxReferenceTypes, aName As String) As Boolean
            Dim tbl As dxfTable = Nothing
            If Not TryGetTable(aRefType, tbl) Then Return False Else Return tbl.MemberExists(aName)
        End Function

        Friend Sub UpdateTable(aTable As TTABLE)
            If aTable.Props.Count <= 0 Or aTable.TableType = dxxReferenceTypes.UNDEFINED Then Return
            Dim mytable As dxfTable = Table(aTable.TableType)
            If mytable Is Nothing Then Return
            mytable.Properties.CopyValues(mytable.Properties)

        End Sub

        Friend Function GetMatchingEntry(aMember As dxfTableEntry, ByRef rTable As dxfTable, ByRef rMemberIndex As Integer) As dxfTableEntry
            rMemberIndex = 0
            rTable = Nothing
            If aMember Is Nothing Then Return Nothing


            Select Case True
                Case TypeOf aMember Is dxoAPPID
                    rTable = APPIDs


                Case TypeOf aMember Is dxoBlockRecord
                    rTable = BlockRecords
                Case TypeOf aMember Is dxoDimStyle
                    rTable = DimStyles

                Case TypeOf aMember Is dxoLayer
                    rTable = Layers

                Case TypeOf aMember Is dxoStyle
                    rTable = Styles

                Case TypeOf aMember Is dxoUCS
                    rTable = UCSs

                Case TypeOf aMember Is dxoView
                    rTable = Views

                Case TypeOf aMember Is dxoViewPort
                    rTable = ViewPorts
                Case Else
                    Return Nothing

            End Select
            If rTable Is Nothing Then Return Nothing
            Dim rEntry As dxfTableEntry = rTable.GetMatchingEntry(aMember)
            If Not rEntry Is Nothing Then rMemberIndex = rTable.IndexOf(aMember)
            Return rEntry
        End Function

        Public Function TryGetTable(aTableType As dxxReferenceTypes, ByRef rTable As dxfTable, Optional bAddIfNotFound As Boolean = False) As Boolean

            rTable = Find(Function(x) x.TableType = aTableType)
            If rTable Is Nothing And bAddIfNotFound Then
                Select Case aTableType
                    Case dxxReferenceTypes.APPID
                        rTable = New dxoAPPIDs(True) With {.ImageGUID = ImageGUID}
                    Case dxxReferenceTypes.VPORT
                        rTable = New dxoViewPorts(True) With {.ImageGUID = ImageGUID}
                    Case dxxReferenceTypes.BLOCK_RECORD
                        rTable = New dxoBlockRecords(True) With {.ImageGUID = ImageGUID}
                    Case dxxReferenceTypes.LTYPE
                        rTable = New dxoLineTypes(True) With {.ImageGUID = ImageGUID}
                    Case dxxReferenceTypes.LAYER
                        rTable = New dxoLayers(True) With {.ImageGUID = ImageGUID}
                    Case dxxReferenceTypes.STYLE
                        rTable = New dxoStyles(True) With {.ImageGUID = ImageGUID}
                    Case dxxReferenceTypes.DIMSTYLE
                        rTable = New dxoDimStyles(True) With {.ImageGUID = ImageGUID}
                    Case dxxReferenceTypes.UCS
                        rTable = New dxoUCSs(True) With {.ImageGUID = ImageGUID}
                    Case dxxReferenceTypes.VIEW
                        rTable = New dxoViews(True) With {.ImageGUID = ImageGUID}
                End Select

                Add(rTable)
            End If
            If rTable IsNot Nothing Then
                If Not String.IsNullOrWhiteSpace(ImageGUID) Then rTable.ImageGUID = ImageGUID
                rTable.Index = IndexOf(rTable) + 1
            End If
            Return rTable IsNot Nothing
        End Function
        Public Function Table(aTableType As dxxReferenceTypes) As dxfTable

            Select Case aTableType
                Case dxxReferenceTypes.APPID
                    Return APPIDs
                Case dxxReferenceTypes.VPORT
                    Return ViewPorts
                Case dxxReferenceTypes.BLOCK_RECORD
                    Return BlockRecords
                Case dxxReferenceTypes.LTYPE
                    Return LineTypes
                Case dxxReferenceTypes.LAYER
                    Return Layers
                Case dxxReferenceTypes.STYLE
                    Return Styles
                Case dxxReferenceTypes.DIMSTYLE
                    Return DimStyles
                Case dxxReferenceTypes.UCS
                    Return UCSs
                Case dxxReferenceTypes.VIEW
                    Return Views
                Case Else
                    Return Nothing
            End Select

        End Function

        Friend Function Table(aTableType As dxxReferenceTypes, aImage As dxfImage) As dxfTable
            Dim _rVal As dxfTable = Table(aTableType)
            If _rVal IsNot Nothing And aImage IsNot Nothing Then _rVal.SetImage(aImage, False)
            Return _rVal
        End Function
        Public Shadows Sub Add(aTable As dxfTable)
            If aTable Is Nothing Then Return
            If FindIndex(Function(x) x.TableType = aTable.TableType) >= 0 Then Return
            MyBase.Add(aTable)

        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    For Each table As dxfTable In Me
                        table.Dispose()

                    Next

                End If
                Clear()
                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        ' Protected Overrides Sub Finalize()
        '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class

End Namespace
