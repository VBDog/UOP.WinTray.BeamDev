Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxoFileHandlerEventArg
        Inherits System.EventArgs
        Public EventType As dxxFileHandlerEvents
        Public SubEventType As dxxFileHandlerEvents
        Public LastPath As String
        Public CurrentPath As String
        Public Sub New()
            MyBase.New()
        End Sub
        Public Sub New(aEventType As dxxFileHandlerEvents, aSubEventType As dxxFileHandlerEvents, aLastPath As String, aCurrentPath As String)
            MyBase.New()
            EventType = aEventType
            SubEventType = aSubEventType
            LastPath = aLastPath
            CurrentPath = aCurrentPath
        End Sub
    End Class
    Public Class dxfImageRenderEventArg
        Inherits System.EventArgs
        Public ImageGUID As String
        Public Begin As Boolean
        Public SetFeatureScales As Boolean
        Public RegeneratePaths As Boolean
        Public ZoomExtents As Boolean
        Public ExtentBufferPercentage As Double
        Public Sub New()
            MyBase.New()
        End Sub
        Public Sub New(aImageGUID As String, aBegin As Boolean, bZoomExtents As Boolean, bSetFeatureScales As Boolean, bRegeneratePaths As Boolean, Optional aExtentBufferPercentage As Double = 0)
            MyBase.New()
            ImageGUID = aImageGUID
            Begin = aBegin
            SetFeatureScales = bSetFeatureScales
            RegeneratePaths = bRegeneratePaths
            ZoomExtents = bZoomExtents
            ExtentBufferPercentage = aExtentBufferPercentage
        End Sub
        Public Overrides Function ToString() As String
            Return "Render:[Begin=" & Begin.ToString & "] [ZoomExents=" & ZoomExtents.ToString & "] [RegeneratePaths=" & RegeneratePaths.ToString & "] [SetFeatueScale=" & SetFeatureScales.ToString & "] [ExtentBufferPercentage=" & ExtentBufferPercentage.ToString & "]"
        End Function
    End Class
    Public Class dxfImageErrorEventArg
        Inherits System.EventArgs
        Public ImageGUID As String
        Public ErrorType As dxxFileErrorTypes
        Public ErrorString As String
        Public Sub New()
            MyBase.New()
        End Sub
        Public Sub New(aImageGUID As String, aErrorType As dxxFileErrorTypes, aErrorString As String)
            MyBase.New()
            ImageGUID = aImageGUID
            ErrorType = aErrorType
            ErrorString = aErrorString
        End Sub
        Public Overrides Function ToString() As String
            Return "Error:[Type=" & dxfEnums.Description(ErrorType) & "] [Err=" & ErrorString & "]"
        End Function
    End Class
    Public Class dxfViewChangeEventArg
        Inherits System.EventArgs
        Public ImageGUID As String
        Public ViewRectangle As dxfRectangle
        Public Sub New()
            MyBase.New()
            ViewRectangle = New dxfRectangle
        End Sub
        Public Sub New(aImageGUID As String, aViewRectangle As dxfRectangle)
            MyBase.New()
            ImageGUID = aImageGUID
            ViewRectangle = New dxfRectangle(aViewRectangle)
        End Sub
        Public Overrides Function ToString() As String
            Return "ViewChange:[View Center:" + ViewRectangle.Center.CoordinatesR(4, bSuppressZ:=True) + "Width:" + ViewRectangle.Width.ToString("0.0000") + "Height:" + ViewRectangle.Height.ToString("0.0000") + "]"
        End Function
    End Class
    Public Class dxfImageScreenEventArg
        Inherits System.EventArgs
        Public ImageGUID As String
        Public EventType As dxxScreenEventTypes
        Public ScreenEntity As dxoScreenEntity
        Public ErrorString As String
        Public ImageEntity As dxfEntity
        Public Bitmap As dxfBitmap
        Public Sub New()
            MyBase.New()
        End Sub
        Friend Sub New(aImageGUID As String, aEventType As dxxScreenEventTypes, Optional aScreenEntity As dxoScreenEntity = Nothing, Optional aImageEntity As dxfEntity = Nothing)
            MyBase.New()
            ImageGUID = aImageGUID
            EventType = aEventType
            ScreenEntity = aScreenEntity
            ImageEntity = aImageEntity
        End Sub
        Public Overrides Function ToString() As String
            Return "Screen Event:[Type=" & dxfEnums.Description(EventType) & "]"
        End Function
    End Class
End Namespace
