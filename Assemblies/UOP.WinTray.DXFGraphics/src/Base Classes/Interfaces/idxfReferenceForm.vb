Namespace UOP.DXFGraphics
#Region "Methods"
    Public Interface idxfReferenceForm
        Function ShowFormat(aOwner As IWin32Window, aImage As dxfImage, bAllowAdd As Boolean, ByRef rRedraw As Boolean) As Boolean
        Function ShowSelect(aOwner As IWin32Window, aImage As dxfImage, aInitName As String, Optional bComboStyle As Boolean = False, Optional aShowLogicals As Boolean = False, Optional bAllowAdd As Boolean = True) As String
        Sub Dispose()
    End Interface
#End Region 'Methods
End Namespace
