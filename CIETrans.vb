Public Class CIETrans
    'Implementation of the 4*4 Rectangular Elemental Matrix for the chronological integration part
    Private x1, y1, x2, y2, x3, y3, x4, y4, DiffCoeff As Double

    ''' <summary>
    ''' Initializes a new instance of CIE element
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
    End Sub
    ''' Returns the determinant of Jacobian matrix for integration point 1(-1/ Math.sqrt(3),-1/ Math.sqrt(3)) //(zeta,yita)
    Private Function getJ1() As Double
        Dim J1(1, 1) As Double
        Dim Det1 As Double
        J1(0, 0) = 1 / 4 * (-(1 - (-1 / Math.Sqrt(3))) * x1 + (1 - (-1 / Math.Sqrt(3))) * x2 + (1 + (-1 / Math.Sqrt(3))) * x3 - (1 + (-1 / Math.Sqrt(3))) * x4)
        J1(1, 0) = 1 / 4 * (-(1 - (-1 / Math.Sqrt(3))) * x1 - (1 + (-1 / Math.Sqrt(3))) * x2 + (1 + (-1 / Math.Sqrt(3))) * x3 + (1 - (-1 / Math.Sqrt(3))) * x4)
        J1(0, 1) = 1 / 4 * (-(1 - (-1 / Math.Sqrt(3))) * y1 + (1 - (-1 / Math.Sqrt(3))) * y2 + (1 + (-1 / Math.Sqrt(3))) * y3 - (1 + (-1 / Math.Sqrt(3))) * y4)
        J1(1, 1) = 1 / 4 * (-(1 - (-1 / Math.Sqrt(3))) * y1 - (1 + (-1 / Math.Sqrt(3))) * y2 + (1 + (-1 / Math.Sqrt(3))) * y3 - (1 - (-1 / Math.Sqrt(3))) * y4)
        Det1 = J1(0, 0) * J1(1, 1) - J1(1, 0) * J1(0, 1)
        Return Det1
    End Function
    ''' Returns the integration for integration point 1(-1/ Math.sqrt(3),-1/ Math.sqrt(3)) //(zeta,yita)
    Private Function getNTN1() As Double(,)
        Dim NTN1(3, 3) As Double

        NTN1(0, 0) = 1 / 16 * (1 - (-1 / Math.Sqrt(3))) ^ 2 * (1 - (-1 / Math.Sqrt(3))) ^ 2
        NTN1(0, 1) = 1 / 16 * (1 - (-1 / Math.Sqrt(3)) ^ 2) * (1 - (-1 / Math.Sqrt(3))) ^ 2
        NTN1(0, 2) = 1 / 16 * (1 - (-1 / Math.Sqrt(3)) ^ 2) * (1 - (-1 / Math.Sqrt(3)) ^ 2)
        NTN1(0, 3) = 1 / 16 * (1 - (-1 / Math.Sqrt(3))) ^ 2 * (1 - (-1 / Math.Sqrt(3)) ^ 2)

        NTN1(1, 0) = 1 / 16 * (1 - (-1 / Math.Sqrt(3)) ^ 2) * (1 - (-1 / Math.Sqrt(3))) ^ 2
        NTN1(1, 1) = 1 / 16 * (1 + (-1 / Math.Sqrt(3))) ^ 2 * (1 - (-1 / Math.Sqrt(3))) ^ 2
        NTN1(1, 2) = 1 / 16 * (1 + (-1 / Math.Sqrt(3))) ^ 2 * (1 - (-1 / Math.Sqrt(3)) ^ 2)
        NTN1(1, 3) = 1 / 16 * (1 - (-1 / Math.Sqrt(3)) ^ 2) * (1 - (-1 / Math.Sqrt(3)) ^ 2)

        NTN1(2, 0) = 1 / 16 * (1 - (-1 / Math.Sqrt(3)) ^ 2) * (1 - (-1 / Math.Sqrt(3)) ^ 2)
        NTN1(2, 1) = 1 / 16 * (1 + (-1 / Math.Sqrt(3))) ^ 2 * (1 - (-1 / Math.Sqrt(3)) ^ 2)
        NTN1(2, 2) = 1 / 16 * (1 + (-1 / Math.Sqrt(3))) ^ 2 * (1 + (-1 / Math.Sqrt(3))) ^ 2
        NTN1(2, 3) = 1 / 16 * (1 - (-1 / Math.Sqrt(3)) ^ 2) * (1 + (-1 / Math.Sqrt(3))) ^ 2

        NTN1(3, 0) = 1 / 16 * (1 - (-1 / Math.Sqrt(3))) ^ 2 * (1 - (-1 / Math.Sqrt(3)) ^ 2)
        NTN1(3, 1) = 1 / 16 * (1 - (-1 / Math.Sqrt(3)) ^ 2) * (1 - (-1 / Math.Sqrt(3)) ^ 2)
        NTN1(3, 2) = 1 / 16 * (1 - (-1 / Math.Sqrt(3)) ^ 2) * (1 + (-1 / Math.Sqrt(3))) ^ 2
        NTN1(3, 3) = 1 / 16 * (1 - (-1 / Math.Sqrt(3))) ^ 2 * (1 + (-1 / Math.Sqrt(3))) ^ 2
        Return NTN1
    End Function
    ''' Returns the determinant of Jacobian matrix for integration point 2 (1/ Math.sqrt(3),-1/ Math.sqrt(3))
    Private Function getJ2() As Double
        Dim J2(1, 1) As Double
        Dim Det2 As Double
        J2(0, 0) = 1 / 4 * (-(1 - (-1 / Math.Sqrt(3))) * x1 + (1 - (-1 / Math.Sqrt(3))) * x2 + (1 + (-1 / Math.Sqrt(3))) * x3 - (1 + (-1 / Math.Sqrt(3))) * x4)
        J2(1, 0) = 1 / 4 * (-(1 - (1 / Math.Sqrt(3))) * x1 - (1 + (1 / Math.Sqrt(3))) * x2 + (1 + (1 / Math.Sqrt(3))) * x3 + (1 - (1 / Math.Sqrt(3))) * x4)
        J2(0, 1) = 1 / 4 * (-(1 - (-1 / Math.Sqrt(3))) * y1 + (1 - (-1 / Math.Sqrt(3))) * y2 + (1 + (-1 / Math.Sqrt(3))) * y3 - (1 + (-1 / Math.Sqrt(3))) * y4)
        J2(1, 1) = 1 / 4 * (-(1 - (1 / Math.Sqrt(3))) * y1 - (1 + (1 / Math.Sqrt(3))) * y2 + (1 + (1 / Math.Sqrt(3))) * y3 - (1 - (1 / Math.Sqrt(3))) * y4)
        Det2 = J2(0, 0) * J2(1, 1) - J2(1, 0) * J2(0, 1)
        Return Det2
    End Function
    ''' Returns the  the integration for integration point 2 (1/ Math.sqrt(3),-1/ Math.sqrt(3))
    Private Function getNTN2() As Double(,)
        Dim NTN2(3, 3) As Double

        NTN2(0, 0) = 1 / 16 * (1 - (1 / Math.Sqrt(3))) ^ 2 * (1 - (-1 / Math.Sqrt(3))) ^ 2
        NTN2(0, 1) = 1 / 16 * (1 - (1 / Math.Sqrt(3)) ^ 2) * (1 - (-1 / Math.Sqrt(3))) ^ 2
        NTN2(0, 2) = 1 / 16 * (1 - (1 / Math.Sqrt(3)) ^ 2) * (1 - (-1 / Math.Sqrt(3)) ^ 2)
        NTN2(0, 3) = 1 / 16 * (1 - (1 / Math.Sqrt(3))) ^ 2 * (1 - (-1 / Math.Sqrt(3)) ^ 2)

        NTN2(1, 0) = 1 / 16 * (1 - (1 / Math.Sqrt(3)) ^ 2) * (1 - (-1 / Math.Sqrt(3))) ^ 2
        NTN2(1, 1) = 1 / 16 * (1 + (1 / Math.Sqrt(3))) ^ 2 * (1 - (-1 / Math.Sqrt(3))) ^ 2
        NTN2(1, 2) = 1 / 16 * (1 + (1 / Math.Sqrt(3))) ^ 2 * (1 - (-1 / Math.Sqrt(3)) ^ 2)
        NTN2(1, 3) = 1 / 16 * (1 - (1 / Math.Sqrt(3)) ^ 2) * (1 - (-1 / Math.Sqrt(3)) ^ 2)

        NTN2(2, 0) = 1 / 16 * (1 - (1 / Math.Sqrt(3)) ^ 2) * (1 - (-1 / Math.Sqrt(3)) ^ 2)
        NTN2(2, 1) = 1 / 16 * (1 + (1 / Math.Sqrt(3))) ^ 2 * (1 - (-1 / Math.Sqrt(3)) ^ 2)
        NTN2(2, 2) = 1 / 16 * (1 + (1 / Math.Sqrt(3))) ^ 2 * (1 + (-1 / Math.Sqrt(3))) ^ 2
        NTN2(2, 3) = 1 / 16 * (1 - (1 / Math.Sqrt(3)) ^ 2) * (1 + (-1 / Math.Sqrt(3))) ^ 2

        NTN2(3, 0) = 1 / 16 * (1 - (1 / Math.Sqrt(3))) ^ 2 * (1 - (-1 / Math.Sqrt(3)) ^ 2)
        NTN2(3, 1) = 1 / 16 * (1 - (1 / Math.Sqrt(3)) ^ 2) * (1 - (-1 / Math.Sqrt(3)) ^ 2)
        NTN2(3, 2) = 1 / 16 * (1 - (1 / Math.Sqrt(3)) ^ 2) * (1 + (-1 / Math.Sqrt(3))) ^ 2
        NTN2(3, 3) = 1 / 16 * (1 - (1 / Math.Sqrt(3))) ^ 2 * (1 + (-1 / Math.Sqrt(3))) ^ 2
        Return NTN2
    End Function
    ''' Returns the determinant of Jacobian matrix for integration point 3 (1/Math.sqrt(3),1/Math.sqrt(3))
    Private Function getJ3() As Double
        Dim J3(1, 1) As Double
        Dim Det3 As Double
        J3(0, 0) = 1 / 4 * (-(1 - (1 / Math.Sqrt(3))) * x1 + (1 - (1 / Math.Sqrt(3))) * x2 + (1 + (1 / Math.Sqrt(3))) * x3 - (1 + (1 / Math.Sqrt(3))) * x4)
        J3(1, 0) = 1 / 4 * (-(1 - (1 / Math.Sqrt(3))) * x1 - (1 + (1 / Math.Sqrt(3))) * x2 + (1 + (1 / Math.Sqrt(3))) * x3 + (1 - (1 / Math.Sqrt(3))) * x4)
        J3(0, 1) = 1 / 4 * (-(1 - (1 / Math.Sqrt(3))) * y1 + (1 - (1 / Math.Sqrt(3))) * y2 + (1 + (1 / Math.Sqrt(3))) * y3 - (1 + (1 / Math.Sqrt(3))) * y4)
        J3(1, 1) = 1 / 4 * (-(1 - (1 / Math.Sqrt(3))) * y1 - (1 + (1 / Math.Sqrt(3))) * y2 + (1 + (1 / Math.Sqrt(3))) * y3 - (1 - (1 / Math.Sqrt(3))) * y4)
        Det3 = J3(0, 0) * J3(1, 1) - J3(1, 0) * J3(0, 1)
        Return Det3
    End Function
    ''' Returns the integration for integration point 3 (1/ Math.sqrt(3),1/ Math.sqrt(3))
    Private Function getNTN3() As Double(,)
        Dim NTN3(3, 3) As Double

        NTN3(0, 0) = 1 / 16 * (1 - (1 / Math.Sqrt(3))) ^ 2 * (1 - (1 / Math.Sqrt(3))) ^ 2
        NTN3(0, 1) = 1 / 16 * (1 - (1 / Math.Sqrt(3)) ^ 2) * (1 - (1 / Math.Sqrt(3))) ^ 2
        NTN3(0, 2) = 1 / 16 * (1 - (1 / Math.Sqrt(3)) ^ 2) * (1 - (1 / Math.Sqrt(3)) ^ 2)
        NTN3(0, 3) = 1 / 16 * (1 - (1 / Math.Sqrt(3))) ^ 2 * (1 - (1 / Math.Sqrt(3)) ^ 2)

        NTN3(1, 0) = 1 / 16 * (1 - (1 / Math.Sqrt(3)) ^ 2) * (1 - (1 / Math.Sqrt(3))) ^ 2
        NTN3(1, 1) = 1 / 16 * (1 + (1 / Math.Sqrt(3))) ^ 2 * (1 - (1 / Math.Sqrt(3))) ^ 2
        NTN3(1, 2) = 1 / 16 * (1 + (1 / Math.Sqrt(3))) ^ 2 * (1 - (1 / Math.Sqrt(3)) ^ 2)
        NTN3(1, 3) = 1 / 16 * (1 - (1 / Math.Sqrt(3)) ^ 2) * (1 - (1 / Math.Sqrt(3)) ^ 2)

        NTN3(2, 0) = 1 / 16 * (1 - (1 / Math.Sqrt(3)) ^ 2) * (1 - (1 / Math.Sqrt(3)) ^ 2)
        NTN3(2, 1) = 1 / 16 * (1 + (1 / Math.Sqrt(3))) ^ 2 * (1 - (1 / Math.Sqrt(3)) ^ 2)
        NTN3(2, 2) = 1 / 16 * (1 + (1 / Math.Sqrt(3))) ^ 2 * (1 + (1 / Math.Sqrt(3))) ^ 2
        NTN3(2, 3) = 1 / 16 * (1 - (1 / Math.Sqrt(3)) ^ 2) * (1 + (1 / Math.Sqrt(3))) ^ 2

        NTN3(3, 0) = 1 / 16 * (1 - (1 / Math.Sqrt(3))) ^ 2 * (1 - (1 / Math.Sqrt(3)) ^ 2)
        NTN3(3, 1) = 1 / 16 * (1 - (1 / Math.Sqrt(3)) ^ 2) * (1 - (1 / Math.Sqrt(3)) ^ 2)
        NTN3(3, 2) = 1 / 16 * (1 - (1 / Math.Sqrt(3)) ^ 2) * (1 + (1 / Math.Sqrt(3))) ^ 2
        NTN3(3, 3) = 1 / 16 * (1 - (1 / Math.Sqrt(3))) ^ 2 * (1 + (1 / Math.Sqrt(3))) ^ 2
        Return NTN3
    End Function
    ''' Returns the determinant of Jacobian matrix for integration point 4 (-1/Math.sqrt(3),1/Math.sqrt(3))
    Private Function getJ4() As Double
        Dim J4(1, 1) As Double
        Dim Det4 As Double
        J4(0, 0) = 1 / 4 * (-(1 - (1 / Math.Sqrt(3))) * x1 + (1 - (1 / Math.Sqrt(3))) * x2 + (1 + (1 / Math.Sqrt(3))) * x3 - (1 + (1 / Math.Sqrt(3))) * x4)
        J4(1, 0) = 1 / 4 * (-(1 - (-1 / Math.Sqrt(3))) * x1 - (1 + (-1 / Math.Sqrt(3))) * x2 + (1 + (-1 / Math.Sqrt(3))) * x3 + (1 - (-1 / Math.Sqrt(3))) * x4)
        J4(0, 1) = 1 / 4 * (-(1 - (1 / Math.Sqrt(3))) * y1 + (1 - (1 / Math.Sqrt(3))) * y2 + (1 + (1 / Math.Sqrt(3))) * y3 - (1 + (1 / Math.Sqrt(3))) * y4)
        J4(1, 1) = 1 / 4 * (-(1 - (-1 / Math.Sqrt(3))) * y1 - (1 + (-1 / Math.Sqrt(3))) * y2 + (1 + (-1 / Math.Sqrt(3))) * y3 - (1 - (-1 / Math.Sqrt(3))) * y4)
        Det4 = J4(0, 0) * J4(1, 1) - J4(1, 0) * J4(0, 1)
        Return Det4
    End Function
    ''' Returns the integration for integration point 4 (-1/ Math.sqrt(3),1/ Math.sqrt(3))
    Private Function getNTN4() As Double(,)
        Dim NTN4(3, 3) As Double

        NTN4(0, 0) = 1 / 16 * (1 - (-1 / Math.Sqrt(3))) ^ 2 * (1 - (1 / Math.Sqrt(3))) ^ 2
        NTN4(0, 1) = 1 / 16 * (1 - (-1 / Math.Sqrt(3)) ^ 2) * (1 - (1 / Math.Sqrt(3))) ^ 2
        NTN4(0, 2) = 1 / 16 * (1 - (-1 / Math.Sqrt(3)) ^ 2) * (1 - (1 / Math.Sqrt(3)) ^ 2)
        NTN4(0, 3) = 1 / 16 * (1 - (-1 / Math.Sqrt(3))) ^ 2 * (1 - (1 / Math.Sqrt(3)) ^ 2)

        NTN4(1, 0) = 1 / 16 * (1 - (-1 / Math.Sqrt(3)) ^ 2) * (1 - (1 / Math.Sqrt(3))) ^ 2
        NTN4(1, 1) = 1 / 16 * (1 + (-1 / Math.Sqrt(3))) ^ 2 * (1 - (1 / Math.Sqrt(3))) ^ 2
        NTN4(1, 2) = 1 / 16 * (1 + (-1 / Math.Sqrt(3))) ^ 2 * (1 - (1 / Math.Sqrt(3)) ^ 2)
        NTN4(1, 3) = 1 / 16 * (1 - (-1 / Math.Sqrt(3)) ^ 2) * (1 - (1 / Math.Sqrt(3)) ^ 2)

        NTN4(2, 0) = 1 / 16 * (1 - (-1 / Math.Sqrt(3)) ^ 2) * (1 - (1 / Math.Sqrt(3)) ^ 2)
        NTN4(2, 1) = 1 / 16 * (1 + (-1 / Math.Sqrt(3))) ^ 2 * (1 - (1 / Math.Sqrt(3)) ^ 2)
        NTN4(2, 2) = 1 / 16 * (1 + (-1 / Math.Sqrt(3))) ^ 2 * (1 + (1 / Math.Sqrt(3))) ^ 2
        NTN4(2, 3) = 1 / 16 * (1 - (-1 / Math.Sqrt(3)) ^ 2) * (1 + (1 / Math.Sqrt(3))) ^ 2

        NTN4(3, 0) = 1 / 16 * (1 - (-1 / Math.Sqrt(3))) ^ 2 * (1 - (1 / Math.Sqrt(3)) ^ 2)
        NTN4(3, 1) = 1 / 16 * (1 - (-1 / Math.Sqrt(3)) ^ 2) * (1 - (1 / Math.Sqrt(3)) ^ 2)
        NTN4(3, 2) = 1 / 16 * (1 - (-1 / Math.Sqrt(3)) ^ 2) * (1 + (1 / Math.Sqrt(3))) ^ 2
        NTN4(3, 3) = 1 / 16 * (1 - (-1 / Math.Sqrt(3))) ^ 2 * (1 + (1 / Math.Sqrt(3))) ^ 2
        Return NTN4
    End Function

    ''' Returns the integrated [be] matrix by Gauss sommation method
    Public Function getbe() As Double(,)
        Dim be(3, 3) As Double 'initialize a 4x4 vector
        'Compute be now
        Dim J1 As Double = getJ1()
        Dim J2 As Double = getJ2()
        Dim J3 As Double = getJ3()
        Dim J4 As Double = getJ4()
        Dim NTN1(,) As Double = getNTN1()
        Dim NTN2(,) As Double = getNTN2()
        Dim NTN3(,) As Double = getNTN3()
        Dim NTN4(,) As Double = getNTN4()

        be(0, 0) = NTN1(0, 0) * J1 + NTN2(0, 0) * J2 + NTN3(0, 0) * J3 + NTN4(0, 0) * J4
        be(0, 1) = NTN1(0, 1) * J1 + NTN2(0, 1) * J2 + NTN3(0, 1) * J3 + NTN4(0, 1) * J4
        be(0, 2) = NTN1(0, 2) * J1 + NTN2(0, 2) * J2 + NTN3(0, 2) * J3 + NTN4(0, 2) * J4
        be(0, 3) = NTN1(0, 3) * J1 + NTN2(0, 3) * J2 + NTN3(0, 3) * J3 + NTN4(0, 3) * J4

        be(1, 0) = NTN1(1, 0) * J1 + NTN2(1, 0) * J2 + NTN3(1, 0) * J3 + NTN4(1, 0) * J4
        be(1, 1) = NTN1(1, 1) * J1 + NTN2(1, 1) * J2 + NTN3(1, 1) * J3 + NTN4(1, 1) * J4
        be(1, 2) = NTN1(1, 2) * J1 + NTN2(1, 2) * J2 + NTN3(1, 2) * J3 + NTN4(1, 2) * J4
        be(1, 3) = NTN1(1, 3) * J1 + NTN2(1, 3) * J2 + NTN3(1, 3) * J3 + NTN4(1, 3) * J4

        be(2, 0) = NTN1(2, 0) * J1 + NTN2(2, 0) * J2 + NTN3(2, 0) * J3 + NTN4(2, 0) * J4
        be(2, 1) = NTN1(2, 1) * J1 + NTN2(2, 1) * J2 + NTN3(2, 1) * J3 + NTN4(2, 1) * J4
        be(2, 2) = NTN1(2, 2) * J1 + NTN2(2, 2) * J2 + NTN3(2, 2) * J3 + NTN4(2, 2) * J4
        be(2, 3) = NTN1(2, 3) * J1 + NTN2(2, 3) * J2 + NTN3(2, 3) * J3 + NTN4(2, 3) * J4

        be(3, 0) = NTN1(3, 0) * J1 + NTN2(3, 0) * J2 + NTN3(3, 0) * J3 + NTN4(3, 0) * J4
        be(3, 1) = NTN1(3, 1) * J1 + NTN2(3, 1) * J2 + NTN3(3, 1) * J3 + NTN4(3, 1) * J4
        be(3, 2) = NTN1(3, 2) * J1 + NTN2(3, 2) * J2 + NTN3(3, 2) * J3 + NTN4(3, 2) * J4
        be(3, 3) = NTN1(3, 3) * J1 + NTN2(3, 3) * J2 + NTN3(3, 3) * J3 + NTN4(3, 3) * J4

        Return be
    End Function
    ''' Returns the integrated [Ae] matrix by Gauss sommation method
    Public Function getAe() As Double(,)

        Dim B1(,) As Double = getB(-1 / Math.Sqrt(3), -1 / Math.Sqrt(3))
        Dim B2(,) As Double = getB(1 / Math.Sqrt(3), -1 / Math.Sqrt(3))
        Dim B3(,) As Double = getB(1 / Math.Sqrt(3), 1 / Math.Sqrt(3))
        Dim B4(,) As Double = getB(-1 / Math.Sqrt(3), 1 / Math.Sqrt(3))
        Dim J1 As Double = getJ1()
        Dim J2 As Double = getJ2()
        Dim J3 As Double = getJ3()
        Dim J4 As Double = getJ4()

        Dim D As Double = DiffCoeff

        Dim BT1(,) As Double = getBT(B1)
        Dim BT2(,) As Double = getBT(B2)
        Dim BT3(,) As Double = getBT(B3)
        Dim BT4(,) As Double = getBT(B4)
        'Compute Ke now
        Dim Ae1(,) As Double = MultiplyMatrices(BT1, B1)
        Dim Ae2(,) As Double = MultiplyMatrices(BT2, B2)
        Dim Ae3(,) As Double = MultiplyMatrices(BT3, B3)
        Dim Ae4(,) As Double = MultiplyMatrices(BT4, B4)
        Dim Ae(4, 4) As Double
        Dim i, j As Integer
        For i = 0 To 3
            For j = 0 To 3
                Ae(i, j) = Ae1(i, j) * D * J1 + Ae2(i, j) * D * J2 + Ae3(i, j) * D * J3 + Ae4(i, j) * D * J4
            Next
        Next
        Return Ae
    End Function
    Public Function getB(ByRef aa As Double, ByRef bb As Double) As Double(,)
        Dim B(1, 3) As Double
        Dim zeta As Double = aa
        Dim yita As Double = bb

        B(0, 0) = 1 / 4 * (-(1 - yita))
        B(0, 1) = 1 / 4 * (1 - yita)
        B(0, 2) = 1 / 4 * (1 + yita)
        B(0, 3) = 1 / 4 * (-(1 + yita))

        B(1, 0) = 1 / 4 * (-(1 - zeta))
        B(1, 1) = 1 / 4 * (-(1 + zeta))
        B(1, 2) = 1 / 4 * (1 + zeta)
        B(1, 3) = 1 / 4 * (1 - zeta)

        Return B
    End Function
    Private Function getBT(ByRef B(,) As Double) As Double(,)
        'returns the transpose of [B]
        Dim BT(3, 1) As Double
        Dim i, j As Integer
        For i = 0 To 3
            For j = 0 To 1
                BT(j, i) = B(i, j)
            Next
        Next
        Return BT
    End Function
    Private Function MultiplyMatrices(ByRef a(,) As Double, ByRef b(,) As Double) As Double(,)

        Dim aCols As Integer = a.GetLength(1)
        Dim bCols As Integer = b.GetLength(1)
        Dim aRows As Integer = a.GetLength(0)
        Dim ab(aRows - 1, bCols - 1) As Double
        Dim sum As Double
        For i As Integer = 0 To aRows - 1
            For j As Integer = 0 To bCols - 1
                sum = 0.0
                For k As Integer = 0 To aCols - 1
                    sum += a(i, k) * b(k, j)
                Next
                ab(i, j) += sum
            Next
        Next

        Return ab
    End Function



End Class

