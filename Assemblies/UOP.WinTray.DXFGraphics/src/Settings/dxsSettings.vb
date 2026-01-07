Imports System.Security.AccessControl
Imports SharpDX.Direct2D1
Imports System.Windows.Controls
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Namespace UOP.DXFGraphics
    Public Class dxsSettings
        Inherits List(Of dxfSettingObject)
        Implements IEnumerable(Of dxfSettingObject)
        Implements IDisposable


        Private disposedValue As Boolean

        Public Sub New()

            '-------------------- init
            Add(New dxsHeader())

            '-------------------- init
        End Sub
        Public Sub New(Optional aImage As dxfImage = Nothing)

            If aImage IsNot Nothing Then ImageGUID = aImage.GUID
            '-------------------- init
            Add(New dxsHeader(aImage:=aImage))
            Add(New dxsDimOverrides(aImage:=aImage))
            Add(New dxsLinetypes(aImage:=aImage))

            '-------------------- init


        End Sub

        Friend Property ImageGUID As String = String.Empty

        Public ReadOnly Property Header As dxsHeader
            Get
                Dim _rVal As dxfSettingObject = Nothing
                If Not TryGetSetting(dxxSettingTypes.HEADER, _rVal, bAddIfNotFound:=True) Then Return Nothing Else Return DirectCast(_rVal, dxsHeader)
            End Get
        End Property

        Public ReadOnly Property DimStyleOverrides As dxsDimOverrides
            Get
                Dim _rVal As dxfSettingObject = Nothing
                If Not TryGetSetting(dxxSettingTypes.DIMOVERRIDES, _rVal, bAddIfNotFound:=True) Then Return Nothing Else Return DirectCast(_rVal, dxsDimOverrides)
            End Get
        End Property
        Public ReadOnly Property LinetypeLayers As dxsLinetypes
            Get
                Dim _rVal As dxfSettingObject = Nothing
                If Not TryGetSetting(dxxSettingTypes.LINETYPESETTINGS, _rVal, bAddIfNotFound:=True) Then Return Nothing Else Return DirectCast(_rVal, dxsLinetypes)
            End Get
        End Property


        Friend Function SetImage(aImage As dxfImage, bDontReleaseOnNull As Boolean) As Boolean
            Dim img As dxfImage = aImage
            If img IsNot Nothing AndAlso img.Disposed Then
                img = Nothing
            End If
            If img IsNot Nothing Then
                ImageGUID = img.GUID
                If Not bDontReleaseOnNull Then
                    ImageGUID = ""
                End If

            End If
            Dim _rVal As Boolean = False
            For Each Item As dxfSettingObject In Me
                If Item.SetImage(img, bDontReleaseOnNull) Then _rVal = True
            Next
            Return _rVal
        End Function
        Friend Function GetMatchingEntry(aMember As dxfSettingObject, ByRef rMemberIndex As Integer) As dxfSettingObject
            Dim rSettings As dxfSettingObject = Nothing
            rMemberIndex = 0
            If aMember Is Nothing Then Return Nothing

            Select Case True
                Case TypeOf aMember Is dxsHeader
                    rSettings = Header

                Case TypeOf aMember Is dxsDimOverrides
                    rSettings = DimStyleOverrides


                Case TypeOf aMember Is dxsLinetypes
                    rSettings = LinetypeLayers


                Case Else
                    Return Nothing

            End Select
            If Not rSettings Is Nothing Then rMemberIndex = IndexOf(rSettings) + 1
            Return rSettings
        End Function


        Public Function TryGetSetting(aSettingType As dxxSettingTypes, ByRef rSetting As dxfSettingObject, Optional bAddIfNotFound As Boolean = False) As Boolean

            rSetting = Find(Function(x) x.SettingType = aSettingType)
            If rSetting Is Nothing And bAddIfNotFound Then
                Select Case aSettingType
                    Case dxxSettingTypes.HEADER
                        rSetting = New dxsHeader() With {.ImageGUID = ImageGUID}
                    Case dxxSettingTypes.TABLESETTINGS
                    Case dxxSettingTypes.DIMOVERRIDES
                        rSetting = New dxsDimOverrides() With {.ImageGUID = ImageGUID}
                    Case dxxSettingTypes.DIMSETTINGS
                    Case dxxSettingTypes.LINETYPESETTINGS
                        rSetting = New dxsLinetypes() With {.ImageGUID = ImageGUID}
                    Case dxxSettingTypes.SYMBOLSETTINGS
                    Case dxxSettingTypes.TEXTSETTINGS
                End Select

                Add(rSetting)
            End If
            If rSetting IsNot Nothing Then
                If Not String.IsNullOrWhiteSpace(ImageGUID) Then rSetting.ImageGUID = ImageGUID
                rSetting.Index = IndexOf(rSetting) + 1
            End If
            Return rSetting IsNot Nothing
        End Function
        Public Function Setting(aSettingType As dxxReferenceTypes) As dxfSettingObject

            Select Case aSettingType
                Case dxxSettingTypes.HEADER
                    Return Header
                Case dxxReferenceTypes.DIMOVERRIDES
                    Return DimStyleOverrides

                    'Case dxxReferenceTypes.VPORT
                    '    Return ViewPorts
                    'Case dxxReferenceTypes.BLOCK_RECORD
                    '    Return BlockRecords
                    'Case dxxReferenceTypes.LTYPE
                    '    Return LineTypes
                    'Case dxxReferenceTypes.LAYER
                    '    Return Layers
                    'Case dxxReferenceTypes.STYLE
                    '    Return Styles
                    'Case dxxReferenceTypes.DIMSTYLE
                    '    Return DimStyles
                    'Case dxxReferenceTypes.UCS
                    '    Return UCSs
                    'Case dxxReferenceTypes.VIEW
                    '    Return Views
                Case Else
                    Return Nothing
            End Select

        End Function

        Friend Function Setting(aSettingType As dxxReferenceTypes, aImage As dxfImage) As dxfSettingObject

            Dim _rVal As dxfSettingObject = Setting(aSettingType)
            If _rVal IsNot Nothing And aImage IsNot Nothing Then _rVal.SetImage(aImage, False)

            Return _rVal


        End Function
        Public Shadows Sub Add(aSetting As dxfSettingObject)
            If aSetting Is Nothing Then Return
            If FindIndex(Function(x) x.SettingType = aSetting.SettingType) >= 0 Then Return
            MyBase.Add(aSetting)

        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    For Each setting As dxfSettingObject In Me
                        setting.Dispose()

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
