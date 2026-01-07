Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics
    Public Class dxoShapeArray
#Region "Members"
        Private _Struc As TSHAPEARRAY
#End Region 'Members
#Region "Constructors"
        Public Sub New()
            _Struc = New TSHAPEARRAY
        End Sub
        Friend Sub New(aStructure As TSHAPEARRAY)
            _Struc = aStructure
        End Sub
#End Region 'Constructors
#Region "Properties"
        Public ReadOnly Property Count As Integer
            Get
                Return _Struc.Count
            End Get
        End Property
        Friend ReadOnly Property Image As dxfImage
            Get
                If Not String.IsNullOrWhiteSpace(_Struc.ImageGUID) Then Return dxfEvents.GetImage(_Struc.ImageGUID) Else Return Nothing
            End Get
        End Property
        Friend Property Strukture As TSHAPEARRAY
            Get
                Return _Struc
            End Get
            Set(value As TSHAPEARRAY)
                _Struc = value
            End Set
        End Property
#End Region 'Properties
#Region "Methods"
        Friend Sub Add(aShapes As TSHAPES, Optional aImage As dxfImage = Nothing)
            Dim rIndex As Integer = 0
            _Struc.Add(aShapes, aImage, rIndex)
        End Sub
        Friend Sub Add(aShapes As TSHAPES, aImage As dxfImage, ByRef rIndex As Integer)
            _Struc.Add(aShapes, aImage, rIndex)
        End Sub
        Friend Function GetByFileName(aFileName As String, ByRef rIndex As Integer, Optional bLoadIfNotFound As Boolean = False, Optional aImage As dxfImage = Nothing) As TSHAPES
            Return _Struc.GetByFileName(aFileName, rIndex, bLoadIfNotFound, aImage)
        End Function
        Friend Function GetByName(aGroupName As String, ByRef rIndex As Integer, Optional bLoadIfNotFound As Boolean = False, Optional aImage As dxfImage = Nothing) As TSHAPES
            Return _Struc.GetColByName(aGroupName, rIndex, bLoadIfNotFound, aImage)
        End Function
        Friend Function GetShapes(aShapesName As String, ByRef rIndex As Integer, Optional bLoadIfNotFound As Boolean = False, Optional aImage As dxfImage = Nothing) As TSHAPES
            Return _Struc.GetShapes(aShapesName, rIndex, bLoadIfNotFound, aImage)
        End Function
        Public Function Load(aFileName As String, Optional bOverwriteExisting As Boolean = False, Optional bSuppressErrors As Boolean = False) As Integer
            Dim rErrorString As String = String.Empty
            Return Load(aFileName, bOverwriteExisting, rErrorString, False)
        End Function
        Public Function Load(aFileName As String, bOverwriteExisting As Boolean, ByRef rErrorString As String, Optional bSuppressErrors As Boolean = False) As Integer
            rErrorString = ""
            Dim aImage As dxfImage = Nothing
            Dim idx As Integer
            Try
                aImage = Image
                Dim aShapes As TSHAPES = TSHAPES.ReadFromFile(aFileName, rErrorString, True)
                If rErrorString <> "" Then Throw New Exception(rErrorString)
                GetByName(aShapes.Name, idx)
                If Not bOverwriteExisting And idx >= 0 Then Throw New Exception("Shapes '" & aShapes.Name & "' Already Loaded")
                If idx >= 0 Then
                    _Struc.SetItem(idx + 1, aShapes)
                Else
                    _Struc.Add(aShapes)
                End If
                If aImage IsNot Nothing Then
                    aImage.obj_SHAPES = _Struc
                    aImage.Styles.AddShapeStyle(aShapes.FileName, aImage)
                End If
                Return aShapes.Count
            Catch ex As Exception
                rErrorString = ex.Message
                If Not bSuppressErrors Then
                    If aImage IsNot Nothing Then
                        aImage.HandleError("Add", "dxoShapeArray", rErrorString, 1000)
                    Else
                        MessageBox.Show($"{Reflection.MethodBase.GetCurrentMethod.Name }  ERROR - {rErrorString}", "Error", MessageBoxButtons.OK, icon:=MessageBoxIcon.Warning)

                    End If
                End If
                Return 0
            End Try
        End Function
#End Region 'Methods
    End Class 'dxoShapeArray
End Namespace
