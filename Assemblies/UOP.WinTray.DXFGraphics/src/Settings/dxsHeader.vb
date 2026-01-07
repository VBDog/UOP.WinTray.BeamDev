Imports UOP.DXFGraphics.dxfGlobals
Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxsHeader
        Inherits dxfSettingObject

#Region "Members"

#End Region 'Members
        Friend Sub New()
            MyBase.New(dxxSettingTypes.HEADER)
        End Sub

        Friend Sub New(aImage As dxfImage)
            MyBase.New(dxxSettingTypes.HEADER, aImage)

        End Sub

        Friend Sub New(aEntry As TTABLEENTRY, aImage As dxfImage)
            MyBase.New(dxxSettingTypes.HEADER, aImage)


            If aEntry.EntryType <> dxxSettingTypes.HEADER Then Return
            Properties.CopyValues(aEntry.Props)


        End Sub
#Region "Properties"


        Public Property AcadVersion As dxxACADVersions
            Get
                '^the AutoCAD version used for DXF code generation
                Return goACAD.Versions.GetVersionByName(PropValueI(dxxHeaderVars.ACADVER), True).Version
            End Get
            Set(value As dxxACADVersions)
                Dim idx As Integer = 0
                '^the AutoCAD version used for DXF code generation
                Dim aVer As TACADVERSION = goACAD.Versions.GetVersion(value, idx)
                If aVer.HeaderName <> "" Then
                    PropValueSet(dxxHeaderVars.ACADVER, aVer.HeaderName)

                End If
            End Set
        End Property

        ''' <summary>
        ''' the color assigned to new entities
        ''' </summary>
        ''' <remarks>
        ''' 1 to 255 are interpreted as autocad color indexes others are interpreted as color long values
        ''' </remarks>
        ''' <returns></returns>
        Public Property Color As dxxColors
            Get

                Return PropValueI(dxxHeaderVars.CECOLOR)
            End Get
            Set(value As dxxColors)
                If value <> dxxColors.Undefined Then PropValueSet(dxxHeaderVars.CECOLOR, value)
            End Set
        End Property

        ''' <summary>
        ''' the name of the currently active dimstyle
        ''' </summary>
        ''' <returns></returns>
        Public Property DimStyleName As String
            Get
                Return PropValueStr(dxxHeaderVars.DIMSTYLE, aDefault:="Standard", bReturnDefaultForNullString:=True)

            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then Return
                If PropValueSet(dxxHeaderVars.DIMSTYLE, value.Trim()) Then
                    ResetDimOverrides()
                End If

            End Set
        End Property
        Public ReadOnly Property DisplaySettings As dxfDisplaySettings
            Get
                Return New dxfDisplaySettings With {.Strukture = DisplayVars}
            End Get
        End Property
        Friend ReadOnly Property DisplayVars As TDISPLAYVARS
            Get
                Return New TDISPLAYVARS With {.Color = Color, .DimStyle = DimStyleName, .LayerName = LayerName, .LineWeight = LineWeight, .LTScale = LineTypeScaleEnt, .TextStyle = TextStyleName}
            End Get
        End Property
        Public Property Elevation As Double
            Get
                '^the current elevation relative to the current UCS for the current viewport in the current space.
                Return PropValueD(dxxHeaderVars.ELEVATION)
            End Get
            Set(value As Double)
                '^the current elevation relative to the current UCS for the current viewport in the current space.
                PropValueSet(dxxHeaderVars.ELEVATION, value)
            End Set
        End Property

        ''' <summary>
        ''' the name of the currently active layer
        ''' </summary>
        ''' <returns></returns>
        Public Property LayerName As String
            Get
                Return PropValueStr(dxxHeaderVars.CLAYER, aDefault:="0", bReturnDefaultForNullString:=True)

            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then Return
                PropValueSet(dxxHeaderVars.CLAYER, value.Trim())

            End Set
        End Property

        ''' <summary>
        ''' the name of the currently active linetype
        ''' </summary>
        ''' <returns></returns>
        Public Property Linetype As String
            Get
                Return PropValueStr(dxxHeaderVars.CELTYPE, aDefault:=dxfLinetypes.ByLayer, bReturnDefaultForNullString:=True)
            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then Return
                PropValueSet(dxxHeaderVars.CELTYPE, value.Trim())
            End Set
        End Property

        ''' <summary>
        ''' the global linetype scale factor.
        ''' </summary>
        ''' <remarks>
        ''' The linetype scale factor cannot equal zero.
        ''' </remarks>
        ''' <returns></returns>
        Public Property LineTypeScale As Double
            Get

                Return PropValueD(dxxHeaderVars.LTSCALE)
            End Get
            Set(value As Double)
                value = Math.Abs(value)
                If value <> 0 Then PropValueSet(dxxHeaderVars.LTSCALE, value)
            End Set
        End Property

        ''' <summary>
        ''' the linetype scale factor for new entities
        ''' </summary>
        ''' <remarks>
        ''' The linetype scale factor cannot equal zero.
        ''' </remarks>
        ''' <returns></returns> 
        Public Property LineTypeScaleEnt As Double
            Get
                Return PropValueD(dxxHeaderVars.CELTSCALE)
            End Get
            Set(value As Double)
                PropValueSet(dxxHeaderVars.CELTSCALE, value)
            End Set
        End Property

        ''' <summary>
        ''' the lineweight assigned to new entities
        ''' </summary>
        ''' <returns></returns>
        Public Property LineWeight As dxxLineWeights
            Get
                Return PropValueI(dxxHeaderVars.CELWEIGHT)
            End Get
            Set(value As dxxLineWeights)
                PropValueSet(dxxHeaderVars.CELWEIGHT, value)
            End Set
        End Property

        ''' <summary>
        ''' the lineweight used if an entities lineweight is ByLayer and the Layers lineweight
        ''' is ByDefault
        ''' </summary>
        ''' <returns></returns>
        Public Property LineWeightDefault As dxxLineWeights
            Get
                Return PropValueI(dxxHeaderVars.LWDEFAULT)
            End Get
            Set(value As dxxLineWeights)
                If value > 0 Then PropValueSet(dxxHeaderVars.LWDEFAULT, value)
            End Set
        End Property

        ''' <summary>
        ''' controls if entity lineweights are displayed on the screen
        ''' </summary>
        ''' <returns></returns>
        Public Property LineWeightDisplay As Boolean
            Get

                Return PropValueB(dxxHeaderVars.LWDISPLAY)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxHeaderVars.LWDISPLAY, value)
            End Set
        End Property
        Public Property LineWeightScale As Double
            Get
                '^used to adjust the zoom on lineweights if LineWeightDisplay = True
                Return PropValueD(dxxHeaderVars.LWSCALE)
            End Get
            Set(value As Double)
                '^used to adjust the zoom on lineweights if LineWeightDisplay = True
                If value > 0 Then PropValueSet(dxxHeaderVars.LWSCALE, value)
            End Set
        End Property
        Public Property MirrorText As Boolean
            Get
                Return PropValueB(dxxHeaderVars.MIRRTEXT)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxHeaderVars.MIRRTEXT, value)
            End Set
        End Property

        ''' <summary>
        ''' controls how points are displayed
        ''' </summary>
        ''' <returns></returns>
        Public Property PointMode As dxxPointModes
            Get
                Return PropValueI(dxxHeaderVars.PDMODE)
            End Get
            Set(value As dxxPointModes)
                PropValueSet(dxxHeaderVars.PDMODE, value)
            End Set
        End Property

        ''' <summary>
        ''' controls the display size for point objects.
        ''' </summary>
        ''' <remarks>0 - Creates a point at 5 percent of the drawing area height, greater than zero -  Specifies an absolute size, less than zero -  Specifies a percentage of the viewport size  </remarks>
        ''' <returns></returns>
        Public Property PointSize As Double
            Get
                Return PropValueD(dxxHeaderVars.PDSIZE)
            End Get
            Set(value As Double)
                PropValueSet(dxxHeaderVars.PDSIZE, value)
            End Set
        End Property

        ''' <summary>
        '''controls the width of new polylines
        ''' </summary>
        ''' <remarks>greater than 0</remarks>
        ''' <returns></returns>
        Public Property PolylineWidth As Double
            Get
                Return PropValueD(dxxHeaderVars.PLINEWID)
            End Get
            Set(value As Double)
                If value > 0 Then PropValueSet(dxxHeaderVars.PLINEWID, value)
            End Set
        End Property

        Public Property QuickText As Boolean
            Get
                Return PropValueB(dxxHeaderVars.QTEXTMODE)
            End Get
            Set(value As Boolean)
                PropValueSet(dxxHeaderVars.QTEXTMODE, value)
            End Set
        End Property


        Public Property TextSize As Double
            '^Specifies the height of text, unless the current text style has a fixed height.
            Get
                Return PropValueD(dxxHeaderVars.TEXTSIZE)
            End Get
            Set(value As Double)
                If value <= 0 Then Return
                PropValueSet(dxxHeaderVars.TEXTSIZE, value)
            End Set
        End Property

        ''' <summary>
        ''' the name of the currently active text style
        ''' </summary>
        ''' <returns></returns>
        Public Property TextStyleName As String
            Get
                Return PropValueStr(dxxHeaderVars.TEXTSTYLE, aDefault:="Standard", bReturnDefaultForNullString:=True)

            End Get
            Set(value As String)
                If String.IsNullOrWhiteSpace(value) Then Return
                PropValueSet(dxxHeaderVars.TEXTSTYLE, value.Trim())
            End Set
        End Property

        ''' <summary>
        ''' the default width of new traces
        ''' </summary>
        ''' <returns></returns>
        Public Property TraceWidth As Double
            Get
                Return PropValueD(dxxHeaderVars.TRACEWID)
            End Get
            Set(value As Double)
                If value <= 0 Then Return
                PropValueSet(dxxHeaderVars.TRACEWID, value)
            End Set
        End Property
        Public Property UCSColor As dxxColors
            Get
                '^control the color of the current UCS
                Return PropValueI(dxxHeaderVars.UCSCOLOR)
            End Get
            Set(value As dxxColors)
                '^control the color of the current UCS
                If Not value.IsLogical() Then
                    PropValueSet(dxxHeaderVars.UCSCOLOR, value)
                End If
            End Set
        End Property
        Public Property UCSMode As dxxUCSIconModes
            Get
                '^Displays the OCS icon for the current viewport or layout. OCSICON is both a command and a system variable. The setting is stored as a bitcode using the sum of the following values:
                '~0 No Icon Is displayed
                '~1 On; the icon is displayed in the lower-left corner of the current viewport or layout
                '`2 Origin; if the icon is on, the icon is displayed at the OCS origin, if possible
                Return PropValueI(dxxHeaderVars.UCSMODE)
            End Get
            Set(value As dxxUCSIconModes)
                '^Displays the OCS icon for the current viewport or layout. OCSICON is both a command and a system variable. The setting is stored as a bitcode using the sum of the following values:
                '~0 No Icon Is displayed
                '~1 On; the icon is displayed in the lower-left corner of the current viewport or layout
                '`2 Origin; if the icon is on, the icon is displayed at the OCS origin, if possible
                If value >= 0 And value <= 2 Then
                    PropValueSet(dxxHeaderVars.UCSMODE, value)
                End If
            End Set
        End Property
        Public Property UCSSize As Double
            Get
                '^control the size of the displayed UCS as a percentage of the current screen
                '~5 to 50%
                Return PropValueD(dxxHeaderVars.UCSSIZE)
            End Get
            Set(value As Double)
                '^control the size of the displayed UCS as a percentage of the current screen
                '~5 to 50%
                If value < 5 Then value = 5
                If value > 50 Then value = 50
                PropValueSet(dxxHeaderVars.UCSSIZE, value)
            End Set
        End Property



#End Region 'Properties
#Region "Methods"

        Public Function SetCurrentReferenceName(aRefType As dxxReferenceTypes, aName As String) As Boolean
            Dim rNameWas As String = String.Empty
            Return SetCurrentReferenceName(aRefType, aName, rNameWas)
        End Function
        Public Function SetCurrentReferenceName(aRefType As dxxReferenceTypes, aName As String, ByRef rNameWas As String) As Boolean
            rNameWas = ""
            Dim hdrprop As dxxHeaderVars = -1
            Dim hdrDflt As String = String.Empty
            Select Case aRefType
                Case dxxReferenceTypes.LAYER
                    hdrprop = dxxHeaderVars.CLAYER
                    hdrDflt = "0"
                Case dxxReferenceTypes.DIMSTYLE
                    hdrprop = dxxHeaderVars.DIMSTYLE
                    hdrDflt = "Standard"
                Case dxxReferenceTypes.STYLE
                    hdrprop = dxxHeaderVars.TEXTSTYLE
                    hdrDflt = "Standard"
            End Select
            If String.IsNullOrEmpty(aName) Then aName = hdrDflt
            If aName <> "" And hdrprop <> -1 Then
                Return PropValueSet(hdrprop, aName)
            Else
                Return False
            End If
        End Function


        Public Sub ResetDimOverrides()
            Dim img As dxfImage = MyImage
            If img Is Nothing Then Return
            Dim curstyle As dxoDimStyle = img.DimStyle()
            If curstyle Is Nothing Then Return

            Dim orides As dxsDimOverrides = img.DimStyleOverrides
            orides.UpdateToImage(curstyle.Name, aImage:=img)
            Dim changes As New List(Of dxoProperty)
            PropValueSet(dxxHeaderVars.DIMSTYLE, curstyle.Name, bSuppressEvnts:=True)

            Dim oridekeys As List(Of String) = orides.Properties.Keys(bExcludeHandles:=True, bExcludeDoNotCopy:=True, aPrefixFilter:="DIM")
            Dim myprops As dxoProperties = Properties
            For Each key As String In oridekeys
                Dim myprop As dxoProperty = myprops.Find(Function(x) String.Compare(x.Key, key, True) = 0)
                If myprop Is Nothing Then
                    Continue For
                End If
                Dim oprop As dxoProperty = orides.Properties.Find(Function(x) String.Compare(x.Key, key, True) = 0)
                If myprop.CopyValue(oprop) Then
                    changes.Add(myprop)
                End If
            Next


        End Sub

        Friend Overloads Function PropValueSet(aIndex As dxxHeaderVars, aValue As Object, Optional aOccur As Integer = 0, Optional bSuppressEvnts As Boolean = False) As Boolean


            Dim aProp As dxoProperty = Nothing
            Dim pname As String = aIndex.PropertyName()

            If Not Properties.TryGet(pname, aProp, aOccur, aPrefixsToConsider:=New List(Of String)({"*", "$"})) Or aValue Is Nothing Then
                Return False
            End If

            Dim _rVal As Boolean = False
            Dim bDontChange As Boolean = False
            Dim aError As String = String.Empty
            Dim aImage As dxfImage = MyImage
            Dim v1 As TVECTOR
            Dim aStr As String = String.Empty
            Dim coords As Integer
            Dim zset As Boolean
            If aProp.IsOrdinate Then
                aStr = aValue.ToString().Trim()
                If String.IsNullOrWhiteSpace(aStr) Then Return False

                v1 = TVECTOR.DefineByString(aStr, v1, ",", zset, coords)
                If coords <= 0 Then Return False
                aValue = v1
            Else
                '' aValue = aProp.Value
            End If




            bDontChange = Not dxfSettingObject.ValidateHeaderPropertyChange(ImageGUID, aProp, aValue, aError, aImage)
            If bDontChange Then

                If Not String.IsNullOrWhiteSpace(aError) Then
                    If aImage IsNot Nothing And Not bSuppressEvnts Then
                        aImage.HandleError(Reflection.MethodBase.GetCurrentMethod, Me.GetType(), New Exception(aError.Trim()))
                    End If
                End If
                Return False
            End If

            _rVal = aProp.SetVal(aValue)

            If _rVal And aImage IsNot Nothing And Not bSuppressEvnts Then
                aImage.RespondToSettingChange(Me, aProp)
            End If

            Return _rVal
        End Function

        Public Overloads Function GetPropertyValue(aIndex As dxxHeaderVars, ByRef rExists As Boolean, ByRef rHidden As Boolean) As Object
            rHidden = False
            Dim sKey As String = dxfEnums.PropertyName(aIndex)
            Dim aProp As dxoProperty = Nothing
            rExists = Properties.TryGet(sKey, aProp)
            If Not rExists Then Return String.Empty

            rHidden = aProp.Hidden
            Return aProp.Value
        End Function
        Public Overloads Function GetPropertyValue(aPropertyName As String, ByRef rExists As Boolean, ByRef rHidden As Boolean) As Object
            rHidden = False
            Dim aProp As dxoProperty = Nothing
            rExists = Properties.TryGet(aPropertyName, aProp)
            If Not rExists Then Return String.Empty

            rHidden = aProp.Hidden
            Return aProp.Value
        End Function
        Friend Function DXFProps(aInstances As dxoInstances, aInstance As Integer, aOCS As TPLANE, ByRef rTypeName As String, Optional aImage As dxfImage = Nothing) As TPROPERTYARRAY
            Return New TPROPERTYARRAY(Properties)
        End Function

        Friend Function DXFFileProperties(aInstances As dxoInstances, aImage As dxfImage, ByRef rBlock As dxfBlock, Optional bSuppressInstances As Boolean = False, Optional bUpdatePath As Boolean = True, Optional aInstance As Integer = -1) As dxoPropertyMatrix
            Return New dxoPropertyMatrix(New TPROPERTYARRAY(Properties))
        End Function
#End Region 'Methods
    End Class 'dxsHeader
End Namespace

