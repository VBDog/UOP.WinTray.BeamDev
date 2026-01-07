Imports UOP.DXFGraphics
Imports UOP.DXFGraphics.Structures
Imports UOP.DXFGraphics.Utilities

Public Class frmInput
    Private oInvalidValues As Collection
    Private vInitValue As Object
    Private bOpCanceled As Boolean
    Private bAllowNegatives As Boolean
    Private bAllowNullInput As Boolean
    Private bString As Boolean
    Private bNumeric As Boolean
    Private bInited As Boolean
    Private sInvalidChars As String
    Private iMaxWhole As Integer
    Private iMaxDeci As Integer
    Private bMinDefined As Boolean
    Private bMaxDefined As Boolean
    Private sMaxValue As Double
    Private sMinValue As Double
    Private bDecimals As Boolean
    Private strFormat As String
    Private bIntegers As Boolean
    Public Function Input_Decimal(aInitValue As Object, ByRef rCanceled As Boolean, Optional aHeader As String = "", Optional aPrompt As String = "", Optional aInfo As String = "",
                                  Optional aAllowZeroInput As Boolean = False, Optional aAllowNegatives As Boolean = False,
                                  Optional aMaxLimit As Double? = Nothing, Optional aMinLimit As Double? = Nothing,
                                  Optional aMaxWhole As Integer = 6, Optional aMaxDeci As Integer = 6,
                                  Optional aEqualReturnCancel As Boolean = False) As Double
        '#1the initial value to display
        '#2a header string (caption) for the input form
        '#3a string to show as a prompt above the input box
        '#4a string to display under the input box
        '#5flag to allow zero as valid input
        '#6flag to allow negative values
        '#7a max limit to apply
        '#8a minimum limit to apply
        '#9the maximimum number of digits to allow to the left of the decimal
        '#10the maximimum number of digits to allow to the right of the decimal
        '#11flag indicating to return the cancel flag as true is the initial value is equal to the inputed value
        '#12returns True if the user cancels the form
        '^shows a form prompting the user to enter a decimal value
        oInvalidValues = New Collection
        bAllowNegatives = aAllowNegatives
        cboInput.Visible = False
        txtInput.Visible = True
        bOpCanceled = True
        bString = False
        bInited = False
        bDecimals = True
        bNumeric = True
        bAllowNullInput = aAllowZeroInput
        bMaxDefined = False
        bMinDefined = False
        sMaxValue = 0
        sMinValue = 0
        If aMinLimit.HasValue Then
            bMinDefined = True
            sMinValue = aMinLimit.Value
            sMaxValue = sMinValue
        End If
        If aMaxLimit.HasValue Then
            bMaxDefined = True
            sMaxValue = aMaxLimit.Value
        End If
        TVALUES.SortTwoValues(True, sMinValue, sMaxValue)
        vInitValue = 0
        If TVALUES.IsNumber(aInitValue) Then vInitValue = TVALUES.To_DBL(aInitValue)
        iMaxWhole = TVALUES.LimitedValue(aMaxWhole, 1, 9)
        If iMaxWhole < 1 Then iMaxWhole = 1
        If iMaxWhole > 9 Then iMaxWhole = 9
        iMaxDeci = aMaxDeci
        If iMaxDeci < 1 Then iMaxDeci = 1
        If iMaxDeci > 12 Then iMaxDeci = 12
        strFormat = "0.0"
        If iMaxDeci > 1 Then strFormat = strFormat & New String("#", iMaxDeci - 1)
        cboInput.Visible = False
        txtInput.Visible = True
        txtInput.Text = Format(vInitValue, strFormat)
        rCanceled = True
        If aHeader <> "" Then
            Text = aHeader
        Else
            Text = "Input Decimal Value"
        End If
        lblCaption.Text = Trim(aPrompt)
        lblInfo.Text = Trim(aInfo)
        Dim _rVal As Double = vInitValue
        ShowDialog()
        rCanceled = bOpCanceled
        If Not rCanceled Then
            _rVal = TVALUES.To_DBL(txtInput.Text)
            If _rVal = vInitValue Then
                If aEqualReturnCancel Then rCanceled = True
            End If
        End If
        Return _rVal
    End Function
    Private Function ValidateInput(ByRef rError As String) As Boolean
        'On Error Resume Next
        rError = string.Empty
        Dim i As Integer
        Dim aVal As Object
        Dim sVal As Double
        Dim aStr As String
        Dim lVal As Long
        If bString Or bNumeric Then
            txtInput.Text = Trim(txtInput.Text)
            aStr = txtInput.Text
            If bNumeric And aStr = "" Then
                aStr = "0"
                If bDecimals Then txtInput.Text = "0.0" Else txtInput.Text = "0"
            End If
        Else
            aStr = cboInput.Text
        End If
        If aStr = "" Then
            If Not bAllowNullInput Then
                rError = "Null Strings Are Not Allowed"
                Return False
            End If
        End If
        If bString Then
            For i = 1 To oInvalidValues.Count
                If String.Compare(oInvalidValues.Item(i), aStr, True) = 0 Then
                    rError = $"{aStr} Is Not An Allowed Input"
                    Return False
                End If
            Next i
            If sInvalidChars <> "" Then
                If dxfUtils.StringContainsCharacters(aStr, sInvalidChars, True, False) Then
                    rError = $"{aStr} Contains Invalid Characters"
                    Return False
                End If
            End If
        Else
            If Not TVALUES.IsNumber(aStr) Then
                rError = $"{aStr} Numeric Input Required"
                Return False
            End If
            sVal = TVALUES.To_DBL(aStr)

            If bIntegers Then
                lVal = TVALUES.To_LNG(sVal)
                If lVal < Integer.MinValue Or lVal > Integer.MaxValue Then
                    rError = $"{aStr} Exceed The Limits of An Integer (-32,768 to 32,767)"
                    Return False
                End If
            End If

            For i = 1 To oInvalidValues.Count
                aVal = TVALUES.To_DBL(oInvalidValues.Item(i))
                If aVal <> sVal Then
                    rError = $"{aStr} Is Not An Allowed Input"
                    Return False
                End If
            Next i
            If bMinDefined Then
                If sVal < sMinValue Then
                    If Not bAllowNullInput Or (bAllowNullInput And sVal <> 0) Then
                        rError = $"{aStr} Is Less Than The Allowed Minimum : { sMinValue:#,0.0####}"
                        Return False
                    End If
                End If
            End If
            If bMaxDefined Then
                If sVal > sMaxValue Then
                    If Not bAllowNullInput Or (bAllowNullInput And sVal <> 0) Then
                        rError = $"{aStr} Is Greater Than The Allowed Maximum : {sMaxValue:#,0.0####}"
                        Return False
                    End If
                End If
            End If
            If ValidateInput Then
                If Not bAllowNullInput And sVal = 0 Then
                    rError = "Zero Is An Invalid Value"
                    Return False
                End If
            End If
        End If
        Return True
    End Function
    Private Sub cmdCancel_Click(sender As Object, e As EventArgs) Handles cmdCancel.Click
        bOpCanceled = True
        Hide()
    End Sub
    Private Sub cmdOK_Click(sender As Object, e As EventArgs) Handles cmdOK.Click
        Dim aErr As String = String.Empty
        If ValidateInput(aErr) Then
            bOpCanceled = False
            Hide()
        Else
            If aErr = "" Then
                aErr = "Invalid Input Detected"
            Else
                aErr = $"Invalid Input Detected: { aErr}"
            End If
            MessageBox.Show($"ERROR - {aErr}", "Error", MessageBoxButtons.OK, icon:=MessageBoxIcon.Warning)

        End If
    End Sub
End Class
