Namespace [Global]

    Friend Module VarPtrSupport

#Region "Declares"
        ' a delegate that can point to the VarPtrCallback method
        Private Delegate Function VarPtrCallbackDelegate(
          ByVal address As Integer, ByVal unused1 As Integer,
          ByVal unused2 As Integer, ByVal unused3 As Integer) As Integer

        ' two aliases for the CallWindowProcA Windows API method
        ' notice that 2nd argument is passed by-reference
        Private Declare Function CallWindowProc Lib "user32" _
          Alias "CallWindowProcA" _
          (ByVal wndProc As VarPtrCallbackDelegate, ByRef var As Short,
          ByVal unused1 As Integer, ByVal unused2 As Integer,
          ByVal unused3 As Integer) As Integer
        Private Declare Function CallWindowProc Lib "user32" _
          Alias "CallWindowProcA" _
          (ByVal wndProc As VarPtrCallbackDelegate, ByRef var As Integer,
          ByVal unused1 As Integer, ByVal unused2 As Integer,
          ByVal unused3 As Integer) As Integer
#End Region 'Declares

        ' ...add more overload to support other data types...

        ' the method that is indirectly executed when calling CallVarPtrSupport
        ' notice that 1st argument is declared by-value (this is the
        ' argument that receives the 2nd value passed to CallVarPtrSupport)
        Private Function VarPtrCallback(ByVal address As Integer,
             ByVal unused1 As Integer, ByVal unused2 As Integer,
             ByVal unused3 As Integer) As Integer
            Return address
        End Function

        ' two overloads of VarPtr
        Public Function VarPtr(ByRef var As Short) As Integer
            Return CallWindowProc(AddressOf VarPtrCallback, var, 0, 0, 0)
        End Function
        Public Function VarPtr(ByRef var As Integer) As Integer

            Return CallWindowProc(AddressOf VarPtrCallback, var, 0, 0, 0)
        End Function



    End Module
End Namespace
