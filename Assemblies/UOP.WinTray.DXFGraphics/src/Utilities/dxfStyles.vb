Imports UOP.DXFGraphics.Structures

Namespace UOP.DXFGraphics.Utilities

    Public Class dxfStyles

        ''' <summary>
        ''' used to apply the dim styles linear format to the passed number
        ''' </summary>
        ''' <param name="aDimStyle">the subject style</param>
        ''' <param name="Num">the number to format</param>
        ''' <param name="bApplyLinearMultiplier"></param>
        ''' <param name="aType"></param>
        ''' <param name="aPrecision"></param>
        ''' <returns></returns>
        Friend Shared Function FormatNumber(aDimStyle As TTABLEENTRY, Num As Object, Optional bApplyLinearMultiplier As Boolean = True, Optional aType As dxxLinearUnitFormats = dxxLinearUnitFormats.Undefined, Optional aPrecision As Integer = -1) As String
            Dim _rVal As String

            Dim bNoLead As Boolean
            Dim bNoTrail As Boolean
            Dim aVal As Double
            Dim bVal As Double
            Dim cWhole As Long
            Dim aFrac As Double
            Dim aStr As String
            Dim bStr As String
            Dim fStr As String
            Dim aRnd As Double = aDimStyle.PropValueD(dxxDimStyleProperties.DIMRND) 'RoundTo
            Dim sRem As Double
            Dim aPrecis As Integer
            Dim aNumer As String = String.Empty
            Dim aDenom As String = String.Empty
            Dim cStackType As dxxDimFractionStyles
            Dim zSupArch As dxxZeroSuppressionsArchitectural
            If Not dxfEnums.Validate(GetType(dxxLinearUnitFormats), aType, dxxLinearUnitFormats.WindowsDesktop.ToString, True) Then
                aType = aDimStyle.PropValueI(dxxDimStyleProperties.DIMLUNIT)
            End If
            If TVALUES.IsNumber(Num) Then aVal = TVALUES.To_DBL(Num)
            If bApplyLinearMultiplier Then aVal *= aDimStyle.PropValueD(dxxDimStyleProperties.DIMLFAC)
            If aRnd > 0 Then
                bVal = Math.Truncate(aVal / aRnd) * aRnd
                sRem = aVal - bVal
                If sRem >= aRnd Then bVal += aRnd
                aVal = bVal
            End If
            Select Case aType
                Case dxxLinearUnitFormats.Scientific
                    _rVal = Format(aVal, "Scientific")
                Case dxxLinearUnitFormats.Engineering
                    _rVal = Format(aVal, aDimStyle.LinearFormat(bNoLead, aPrecision:=aPrecision))
                    If _rVal.StartsWith(".") Then _rVal = _rVal.Substring(1, _rVal.Length - 1)
                    If TVALUES.To_DBL(_rVal) = 0 Then
                        If Not bNoLead Then _rVal = "0.0" Else _rVal = "0"
                    End If
                    _rVal += Chr(34)
                Case dxxLinearUnitFormats.Architectural
                    cStackType = aDimStyle.PropValueD(dxxDimStyleProperties.DIMFRAC) 'FractionStackType
                    zSupArch = aDimStyle.PropValueI(dxxDimStyleProperties.DIMZEROSUPPRESSION_ARCH) ' ZeroSuppressionArchitectural

                    fStr = ""
                    aVal /= 12
                    cWhole = Fix(aVal)
                    aFrac = aVal - cWhole
                    aStr = cWhole.ToString()
                    If cWhole = 0 Then
                        If zSupArch <> dxxZeroSuppressionsArchitectural.IncludeZeroFeetAndSuppressZeroInches Or zSupArch <> dxxZeroSuppressionsArchitectural.IncludeZeroFeetAndZeroInches Then
                            aStr = ""
                        End If
                    End If
                    If aStr <> "" Then aStr += "\"
                    aVal = aFrac * 12
                    cWhole = Fix(aVal)
                    aFrac = aVal - cWhole
                    bStr = cWhole.ToString()
                    If aFrac > 0 Then
                        aPrecis = aDimStyle.PropValueI(dxxDimStyleProperties.DIMDEC) ' LinearPrecision
                        If aPrecis > 0 Then
                            Dim d1 As Double = 0
                            If dxfUtils.ComputeFraction(aFrac, aPrecis, aNumer, aDenom, True, d1) Then
                                If aNumer = aDenom Then
                                    bStr = (cWhole + 1).ToString()
                                Else
                                    If cStackType = dxxDimFractionStyles.NoStack Then
                                        fStr = $" { aNumer}/{ aDenom}"
                                    ElseIf cStackType = dxxDimFractionStyles.Diagonal Then
                                        fStr = dxfUtils.CreateStackedText(aNumer, aDenom, dxxCharacterStackStyles.Diagonal)
                                    Else
                                        fStr = dxfUtils.CreateStackedText(aNumer, aDenom, dxxCharacterStackStyles.Horizontal)
                                    End If
                                End If
                            End If
                        End If
                    End If
                    bStr += fStr
                    If bStr = "0" Then
                        If zSupArch = dxxZeroSuppressionsArchitectural.IncludeZeroFeetAndSuppressZeroInches Or zSupArch = dxxZeroSuppressionsArchitectural.ZeroFeetAndZeroInches Then
                            bStr = ""
                        End If
                    End If
                    If bStr <> "" Then bStr += """"
                    If aStr <> "" Then
                        _rVal = aStr
                        If bStr <> "" Then _rVal += $"-{ bStr}"
                    Else
                        _rVal = bStr
                    End If
                    If _rVal = "" Then _rVal = "0" & Chr(34)
                Case dxxLinearUnitFormats.Fractional
                    cStackType = aDimStyle.PropValueI(dxxDimStyleProperties.DIMFRAC) 'FractionStackType
                    zSupArch = aDimStyle.PropValueI(dxxDimStyleProperties.DIMZEROSUPPRESSION_ARCH) 'ZeroSuppressionArchitectural

                    bStr = ""
                    fStr = ""
                    cWhole = Fix(aVal)
                    aFrac = aVal - cWhole
                    aStr = cWhole.ToString()
                    If cWhole = 0 Then
                        If zSupArch <> dxxZeroSuppressionsArchitectural.IncludeZeroFeetAndSuppressZeroInches Or zSupArch <> dxxZeroSuppressionsArchitectural.IncludeZeroFeetAndZeroInches Then
                            aStr = ""
                        End If
                    End If
                    aVal = aFrac
                    cWhole = Fix(aVal)
                    aFrac = aVal - cWhole
                    If cWhole > 0 Then bStr = cWhole.ToString()
                    If aFrac > 0 Then
                        aPrecis = aDimStyle.PropValueI(dxxDimStyleProperties.DIMDEC) 'LinearPrecision
                        If aPrecis > 0 Then
                            Dim d1 As Double = 0
                            If dxfUtils.ComputeFraction(aFrac, aPrecis, aNumer, aDenom, True, d1) Then
                                If aNumer = aDenom Then
                                    bStr = (cWhole + 1).ToString()
                                Else
                                    If cStackType = dxxDimFractionStyles.NoStack Then
                                        fStr = " " & aNumer & "/" & aDenom
                                    ElseIf cStackType = dxxDimFractionStyles.Diagonal Then
                                        fStr = dxfUtils.CreateStackedText(aNumer, aDenom, dxxCharacterStackStyles.Diagonal)
                                    Else
                                        fStr = dxfUtils.CreateStackedText(aNumer, aDenom, dxxCharacterStackStyles.Horizontal)
                                    End If
                                End If
                            End If
                        End If
                    End If
                    bStr += fStr
                    If bStr = "0" Then
                        If zSupArch = dxxZeroSuppressionsArchitectural.IncludeZeroFeetAndSuppressZeroInches Or zSupArch = dxxZeroSuppressionsArchitectural.ZeroFeetAndZeroInches Then
                            bStr = ""
                        End If
                    End If
                    _rVal = aStr & bStr
                    If _rVal = "" Then _rVal = "0"
                Case Else
                    _rVal = Format(aVal, aDimStyle.LinearFormat(bNoLead, bNoTrail, aPrecision))
                    If _rVal.EndsWith(".") Then _rVal = _rVal.Substring(0, _rVal.Length - 1)

                    If _rVal = "" Then
                        If bNoTrail Then _rVal = "0" Else _rVal = "0.0"
                    End If
            End Select
            If _rVal = "" Then _rVal = "0"
            Dim sep As String = Chr(aDimStyle.PropValueI(dxxDimStyleProperties.DIMDSEP))
            If sep <> "" Then _rVal = _rVal.Replace(".", sep)
            Return _rVal
        End Function


    End Class

End Namespace
