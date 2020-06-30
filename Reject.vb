Public Class Reject
    'Implementation of the 4*4 Rectangular Elemental Matrix /space derivative term
    'Element properties
    Private x1, y1, x2, y2, x3, y3, x4, y4, DiffCoeff As Double

    'upper and lower bounds
    Private xl, xs, yl, ys As Double

    ''' <summary>
    ''' Initializes a new instance of SEM element
    ''' </summary>
    ''' <param name="x1"></param>
    ''' <param name="y1"></param>
    ''' <param name="x2"></param>
    ''' <param name="y2"></param>
    ''' <param name="x3"></param>
    ''' <param name="y3"></param>
    ''' <param name="x4"></param>
    ''' <param name="y4"></param>
    ''' <param name="DiffCoeff"></param>
    Public Sub New(x1 As Double, y1 As Double,
                   x2 As Double, y2 As Double,
                   x3 As Double, y3 As Double,
                   x4 As Double, y4 As Double,
                              DiffCoeff As Double)
        Me.x1 = x1 : Me.y1 = y1
        Me.x2 = x2 : Me.y2 = y2
        Me.x3 = x3 : Me.y3 = y3
        Me.x4 = x4 : Me.y4 = y4
        Me.DiffCoeff = DiffCoeff

        'find the upper and lower bounds
        'upper x bound
        If x1 > x2 Then
            xl = x1
        Else
            xl = x2
            If xl > x3 Then
                xl = xl
            Else
                xl = x3
                If xl > x4 Then
                    xl = x1
                Else
                    xl = x4
                End If
            End If
        End If
        'lower x bound
        If x1 < x2 Then
            xs = x1
        Else
            xs = x2
            If xs < x3 Then
                xs = xs
            Else
                xs = x3
                If xs < x4 Then
                    xs = xs
                Else
                    xs = x4
                End If
            End If
        End If
        'upper y bound
        If y1 > y2 Then
            yl = y1
        Else
            yl = y2
            If yl > y3 Then
                yl = yl
            Else
                yl = y3
                If yl > y4 Then
                    yl = yl
                Else
                    yl = y4
                End If
            End If
        End If
        'lower y bound
        If y1 < y2 Then
            ys = y1
        Else
            ys = y2
            If ys < y3 Then
                ys = ys
            Else
                ys = y3
                If ys < y4 Then
                    ys = ys
                Else
                    ys = y4
                End If
            End If
        End If
    End Sub

    ''' Returns the [Ke] matrix
    Public Function GetSe() As Double(,)

        Dim Se(3, 3) As Double 'initialize a 4x4 array

        'Compute Ke now
        'Diagonal terms
        Se(0, 0) = getInt11(xl, yl, xs, ys)
        Se(1, 1) = getInt22(xl, yl, xs, ys)
        Se(2, 2) = getInt33(xl, yl, xs, ys)
        Se(3, 3) = getInt44(xl, yl, xs, ys)
        'Symetric terms
        Se(0, 1) = getInt12(xl, yl, xs, ys)
        Se(1, 0) = Se(0, 1)
        Se(0, 2) = getInt13(xl, yl, xs, ys)
        Se(2, 0) = Se(0, 2)
        Se(0, 3) = getInt14(xl, yl, xs, ys)
        Se(3, 0) = Se(0, 3)
        Se(1, 2) = getInt23(xl, yl, xs, ys)
        Se(2, 1) = Se(1, 2)
        Se(1, 3) = getInt24(xl, yl, xs, ys)
        Se(3, 1) = Se(1, 3)
        Se(2, 3) = getInt34(xl, yl, xs, ys)
        Se(3, 2) = Se(2, 3)
        Return Se
    End Function

    Public Function getInt11(ByRef upx As Double, ByRef upy As Double, ByRef lowx As Double, ByRef lowy As Double) As Double
        Dim itg11 As Double
        Dim D As Double = DiffCoeff
        itg11 = (3 * D * (upx ^ 2 * lowx * lowy - upx ^ 2 * lowx * upy - upx ^ 2 * upx * lowy + upx ^ 2 * upx * upy - upx * lowx * lowy ^ 2 + upx * lowx * upy ^ 2 + upx * upx * lowy ^ 2 - upx * upx * upy ^ 2 + lowy ^ 2 * lowx * lowy - lowy ^ 2 * lowx * upy - lowy ^ 2 * upx * lowy + lowy ^ 2 * upx * upy - lowy * lowx ^ 2 * lowy + lowy * lowx ^ 2 * upy + lowy * upx ^ 2 * lowy - lowy * upx ^ 2 * upy) + D * (lowx ^ 3 * lowy - lowx ^ 3 * upy + lowx * lowy ^ 3 - lowx * upy ^ 3 - upx ^ 3 * lowy + upx ^ 3 * upy - upx * lowy ^ 3 + upx * upy ^ 3)) / (3 * (lowx - upx) ^ 2 * (lowy - upy) ^ 2)
        Return itg11
    End Function

    Public Function getInt22(ByRef upx As Double, ByRef upy As Double, ByRef lowx As Double, ByRef lowy As Double) As Double
        Dim itg22 As Double
        Dim D As Double = DiffCoeff
        itg22 = (3 * D * (lowx ^ 2 * lowx * lowy - lowx ^ 2 * lowx * upy - lowx ^ 2 * upx * lowy + lowx ^ 2 * upx * upy - lowx * lowx * lowy ^ 2 + lowx * lowx * upy ^ 2 + lowx * upx * lowy ^ 2 - lowx * upx * upy ^ 2 + lowy ^ 2 * lowx * lowy - lowy ^ 2 * lowx * upy - lowy ^ 2 * upx * lowy + lowy ^ 2 * upx * upy - lowy * lowx ^ 2 * lowy + lowy * lowx ^ 2 * upy + lowy * upx ^ 2 * lowy - lowy * upx ^ 2 * upy) + D * (lowx ^ 3 * lowy - lowx ^ 3 * upy + lowx * lowy ^ 3 - lowx * upy ^ 3 - upx ^ 3 * lowy + upx ^ 3 * upy - upx * lowy ^ 3 + upx * upy ^ 3)) / (3 * (lowx - upx) ^ 2 * (lowy - upy) ^ 2)
        Return itg22
    End Function

    Public Function getInt33(ByRef upx As Double, ByRef upy As Double, ByRef lowx As Double, ByRef lowy As Double) As Double
        Dim itg33 As Double
        Dim D As Double = DiffCoeff
        itg33 = (3 * D * (lowx ^ 2 * lowx * lowy - lowx ^ 2 * lowx * upy - lowx ^ 2 * upx * lowy + lowx ^ 2 * upx * upy - lowx * lowx * lowy ^ 2 + lowx * lowx * upy ^ 2 + lowx * upx * lowy ^ 2 - lowx * upx * upy ^ 2 + upy ^ 2 * lowx * lowy - upy ^ 2 * lowx * upy - upy ^ 2 * upx * lowy + upy ^ 2 * upx * upy - upy * lowx ^ 2 * lowy + upy * lowx ^ 2 * upy + upy * upx ^ 2 * lowy - upy * upx ^ 2 * upy) + D * (lowx ^ 3 * lowy - lowx ^ 3 * upy + lowx * lowy ^ 3 - lowx * upy ^ 3 - upx ^ 3 * lowy + upx ^ 3 * upy - upx * lowy ^ 3 + upx * upy ^ 3)) / (3 * (lowx - upx) ^ 2 * (lowy - upy) ^ 2)
        Return itg33
    End Function

    Public Function getInt44(ByRef upx As Double, ByRef upy As Double, ByRef lowx As Double, ByRef lowy As Double) As Double
        Dim itg44 As Double
        Dim D As Double = DiffCoeff
        itg44 = (3 * D * (upx ^ 2 * lowx * lowy - upx ^ 2 * lowx * upy - upx ^ 2 * upx * lowy + upx ^ 2 * upx * upy - upx * lowx * lowy ^ 2 + upx * lowx * upy ^ 2 + upx * upx * lowy ^ 2 - upx * upx * upy ^ 2 + upy ^ 2 * lowx * lowy - upy ^ 2 * lowx * upy - upy ^ 2 * upx * lowy + upy ^ 2 * upx * upy - upy * lowx ^ 2 * lowy + upy * lowx ^ 2 * upy + upy * upx ^ 2 * lowy - upy * upx ^ 2 * upy) + D * (lowx ^ 3 * lowy - lowx ^ 3 * upy + lowx * lowy ^ 3 - lowx * upy ^ 3 - upx ^ 3 * lowy + upx ^ 3 * upy - upx * lowy ^ 3 + upx * upy ^ 3)) / (3 * (lowx - upx) ^ 2 * (lowy - upy) ^ 2)
        Return itg44
    End Function

    Public Function getInt12(ByRef upx As Double, ByRef upy As Double, ByRef lowx As Double, ByRef lowy As Double) As Double
        Dim itg12 As Double
        Dim D As Double = DiffCoeff
        itg12 = (D * (lowx - upx) * (lowy - upy) * (6 * lowy ^ 2 + 2 * lowx ^ 2 + 2 * upx ^ 2 + 2 * lowy ^ 2 + 2 * upy ^ 2 + 6 * lowx * upx - 3 * lowx * lowy - 6 * lowy * lowx - 3 * lowx * upy - 3 * upx * lowy - 6 * lowy * upx - 3 * upx * upy + 2 * lowx * upx + 2 * lowy * upy)) / (6 * (lowx - upx) ^ 2 * (lowy - upy) ^ 2)
        Return itg12
    End Function

    Public Function getInt13(ByRef upx As Double, ByRef upy As Double, ByRef lowx As Double, ByRef lowy As Double) As Double
        Dim itg13 As Double
        Dim D As Double = DiffCoeff
        itg13 = (D * (lowx - upx) * (lowy - upy) * (2 * lowx ^ 2 + 2 * upx ^ 2 + 2 * lowy ^ 2 + 2 * upy ^ 2 + 6 * lowx * upx + 6 * lowy * upy - 6 * lowx * lowy - 3 * lowx * upy - 3 * upx * lowy - 3 * lowy * upx - 3 * upy * lowx - 6 * upx * upy + 2 * lowx * upx + 2 * lowy * upy)) / (6 * (lowx - upx) ^ 2 * (lowy - upy) ^ 2)
        Return itg13
    End Function

    Public Function getInt14(ByRef upx As Double, ByRef upy As Double, ByRef lowx As Double, ByRef lowy As Double) As Double
        Dim itg14 As Double
        Dim D As Double = DiffCoeff
        itg14 = (D * (2 * lowx * lowy ^ 3 + 2 * lowx ^ 3 * lowy - 2 * lowx * upy ^ 3 - 2 * upx * lowy ^ 3 - 2 * lowx ^ 3 * upy - 2 * upx ^ 3 * lowy + 2 * upx * upy ^ 3 + 2 * upx ^ 3 * upy - 6 * upx * lowx * lowy ^ 2 - 3 * lowy * lowx ^ 2 * lowy + 6 * upx ^ 2 * lowx * lowy + 6 * upx * lowx * upy ^ 2 + 6 * upx * upx * lowy ^ 2 + 3 * lowy * lowx ^ 2 * upy + 3 * lowy * upx ^ 2 * lowy - 3 * upy * lowx ^ 2 * lowy - 6 * upx ^ 2 * lowx * upy - 6 * upx ^ 3 * lowy - 6 * upx ^ 2 * upy ^ 2 - 3 * lowy * upx ^ 2 * upy + 3 * upy * lowx ^ 2 * upy + 3 * upy * upx ^ 2 * lowy + 6 * upx ^ 3 * upy - 3 * upy * upx ^ 2 * upy + 6 * lowy * upy * lowx * lowy - 6 * lowy * upy * lowx * upy - 6 * lowy * upy * upx * lowy + 6 * lowy * upy * upx * upy)) / (6 * (lowx - upx) ^ 2 * (lowy - upy) ^ 2)
        Return itg14
    End Function
    Public Function getInt23(ByRef upx As Double, ByRef upy As Double, ByRef lowx As Double, ByRef lowy As Double) As Double
        Dim itg23 As Double
        Dim D As Double = DiffCoeff
        itg23 = D * (lowx - upx) * (lowy - upy) * (8 * lowx ^ 2 + 2 * upx ^ 2 + 2 * lowy ^ 2 + 2 * upy ^ 2 + 6 * lowy * upy - 6 * lowx * lowy - 3 * lowy * lowx - 6 * lowx * upy - 3 * lowy * upx - 3 * upy * lowx + 2 * lowx * upx + 2 * lowy * upy) / (6 * (lowx - upx) ^ 2 * (lowy - upy) ^ 2)
        Return itg23
    End Function
    ' functions needed to update
    Public Function getInt24(ByRef upx As Double, ByRef upy As Double, ByRef lowx As Double, ByRef lowy As Double) As Double
        Dim itg As Double
        Dim D As Double = DiffCoeff
        itg = 3 * D * (upx ^ 2 * lowx * lowy - upx ^ 2 * lowx * upy - upx ^ 2 * upx * lowy + upx ^ 2 * upx * upy - upx * lowx * lowy ^ 2 + upx * lowx * upy ^ 2 + upx * upx * lowy ^ 2 - upx * upx * upy ^ 2 + lowy ^ 2 * lowx * lowy - lowy ^ 2 * lowx * upy - lowy ^ 2 * upx * lowy + lowy ^ 2 * upx * upy - lowy * lowx ^ 2 * lowy + lowy * lowx ^ 2 * upy + lowy * upx ^ 2 * lowy - lowy * upx ^ 2 * upy) + D * (lowx ^ 3 * lowy - lowx ^ 3 * upy + lowx * lowy ^ 3 - lowx * upy ^ 3 - upx ^ 3 * lowy + upx ^ 3 * upy - upx * lowy ^ 3 + upx * upy ^ 3)
        Return itg
    End Function
    ' functions needed to update
    Public Function getInt34(ByRef upx As Double, ByRef upy As Double, ByRef lowx As Double, ByRef lowy As Double) As Double
        Dim itg As Double
        Dim D As Double = DiffCoeff
        itg = 3 * D * (upx ^ 2 * lowx * lowy - upx ^ 2 * lowx * upy - upx ^ 2 * upx * lowy + upx ^ 2 * upx * upy - upx * lowx * lowy ^ 2 + upx * lowx * upy ^ 2 + upx * upx * lowy ^ 2 - upx * upx * upy ^ 2 + lowy ^ 2 * lowx * lowy - lowy ^ 2 * lowx * upy - lowy ^ 2 * upx * lowy + lowy ^ 2 * upx * upy - lowy * lowx ^ 2 * lowy + lowy * lowx ^ 2 * upy + lowy * upx ^ 2 * lowy - lowy * upx ^ 2 * upy) + D * (lowx ^ 3 * lowy - lowx ^ 3 * upy + lowx * lowy ^ 3 - lowx * upy ^ 3 - upx ^ 3 * lowy + upx ^ 3 * upy - upx * lowy ^ 3 + upx * upy ^ 3)
        Return itg
    End Function
End Class
