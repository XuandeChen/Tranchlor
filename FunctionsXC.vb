Imports System.Linq

Module FunctionsXC
    'get the LHS matrix for Gauss matrix resolution
    Public Function getLHS(ByRef NNodes As Integer, ByRef A(,) As Double, ByRef b(,) As Double, ByRef dt As Double)
        Dim LHS(NNodes - 1, NNodes - 1) As Double
        Dim i, j As Integer
        For i = 0 To NNodes - 1
            For j = 0 To NNodes - 1
                LHS(i, j) = A(i, j) / 2 + b(i, j) / dt
            Next
        Next
        Return LHS
    End Function
    Public Function getNewLHS(ByRef NNodes As Integer, ByRef phi As Integer, ByRef A(,) As Double, ByRef b(,) As Double, ByRef dt As Double)
        Dim LHS(NNodes - 1, NNodes - 1) As Double
        Dim i, j As Integer
        For i = 0 To NNodes - 1
            For j = 0 To NNodes - 1
                LHS(i, j) = phi * A(i, j) / 2 + b(i, j) / dt
            Next
        Next
        Return LHS
    End Function
    'get the RHS matrix for Gauss matrix resolution
    Public Function getRHS(ByRef NNodes As Integer, ByRef A(,) As Double, ByRef b(,) As Double, ByRef dt As Double)
        Dim RHS(NNodes - 1, NNodes - 1) As Double
        Dim i, j As Integer
        For i = 0 To NNodes - 1
            For j = 0 To NNodes - 1
                RHS(i, j) = b(i, j) / dt - A(i, j) / 2
            Next
        Next
        Return RHS
    End Function
    Public Function getNewR(ByRef NNodes As Integer, ByRef phi As Integer, ByRef A(,) As Double, ByRef b(,) As Double, ByRef dt As Double)
        Dim R(NNodes - 1, NNodes - 1) As Double
        Dim i, j As Integer
        For i = 0 To NNodes - 1
            For j = 0 To NNodes - 1
                R(i, j) = b(i, j) / dt - phi * A(i, j) / 2
            Next
        Next
        Return R
    End Function
    'get degree of freedom /water diffusion
    Public Function getDOF(NodeNo As Integer) As Integer
        Dim nDofsPerNode As Integer = 1
        Return (NodeNo) * nDofsPerNode
    End Function
    'matrix multiplying a vector
    Public Function MultiplyMatrixWithVector(ByRef a(,) As Double, ByRef b() As Double) As Double()

        Dim aRows As Integer = a.GetLength(0)
        Dim aCols As Integer = a.GetLength(1)
        Dim ab(aRows - 1) As Double 'output will be a vector
        For i As Integer = 0 To aRows - 1
            ab(i) = 0.0
            For j As Integer = 0 To aCols - 1
                ab(i) += a(i, j) * b(j)
            Next
        Next

        Return ab
    End Function
    'linear matrix system resolution by Gauss elimination
    Public Function GetX(ByRef A(,) As Double, ByRef b() As Double)
        Dim aRows As Integer = A.GetLength(0)
        Dim aCols As Integer = A.GetLength(1)
        Dim bRows As Integer = b.GetLength(0)
        Dim m As Double
        Dim i, j, k As Integer
        Dim sum As Double
        Dim s As Integer = bRows
        Dim X(s - 1) As Double

        For j = 0 To s - 2 Step 1
            For i = s - 1 To j + 1 Step -1
                m = A(i, j) / A(j, j)
                For k = 0 To s - 1 Step 1
                    A(i, k) = A(i, k) - m * A(j, k)
                Next
                'A(i, j) = A(i, j) - m * A(j, j)
                b(i) = b(i) - m * b(j)
            Next
        Next

        X(s - 1) = b(s - 1) / A(s - 1, s - 1)

        For i = s - 2 To 0 Step -1
            sum = 0
            For j = s - 1 To i Step -1
                sum = sum + A(i, j) * X(j)
            Next
            X(i) = (b(i) - sum) / A(i, i)
        Next
        Return X
    End Function

    ''liquid water transport functions: (Equation and formula from Marc Mainguy, 2001)
    'relative permeability function (Equation and formula from Marc Mainguy, 2001 & derived from VanGnuchten, 1980) 
    Public Function Getkr(ByRef saturation As Double, ByRef beta As Double)
        Dim S As Double = saturation
        Dim sa_irr As Double = 0.00001
        Dim sb_irr As Double = 0.00001
        Dim s_eff As Double = (S - sb_irr) / (1 - sa_irr - sb_irr)
        Dim kr_b_max As Double = 1
        Dim kr As Double = kr_b_max * s_eff ^ beta
        Return kr
    End Function

    'liquid capillary pressure function (Equation and formula from Marc Mainguy, 2001)
    Public Function Getpc(ByRef saturation As Double, ByRef pc_0 As Double, ByRef m As Double)
        Dim S As Double = saturation
        Dim n As Double = 1 / (1 - m)
        Dim s_pc_irr As Double = 0.00001
        Dim s_pc_max As Double = 1
        Dim Sb_pc As Double = (S - s_pc_irr) / (s_pc_max - s_pc_irr)
        Dim pc As Double = pc_0 * ((Sb_pc ^ (-1 / m) - 1) ^ (1 / n))
        Return pc
    End Function
    'derivation of the capillary pressure function 
    Public Function GetdpcdS(ByRef saturation As Double, ByRef pc_0 As Double, ByRef m As Double)
        Dim dpcdS As Double
        Dim S As Double = saturation
        Dim n As Double = 1 / (1 - m)
        dpcdS = (pc_0 * (1 / S ^ (1 / m) - 1) ^ (1 / n)) / (m * n * S * (S ^ (1 / m) - 1))
        Return dpcdS
    End Function

    'liquid-water contribution to the moisture diffusivity [cm2/s] 
    Public Function GetDl(ByRef K As Double, ByRef yita_l As Double, ByRef dpcdS As Double, ByRef kr As Double)
        Dim Dl As Double
        Dl = -dpcdS * K * kr / yita_l
        Return Dl
    End Function

    ''water vapor transport functions: (Equation and formula from Marc Mainguy, 2001)
    'diffusion coefficient of water vapor or dry air in wet air [cm2/s]
    Public Function GetD(ByRef temperature As Double, ByRef pg As Double)
        Dim T As Double = temperature
        Dim Dva As Double
        Dim p_atm As Double = 101325 'atmosphere pressure [pa]
        Dim T0 As Double = 273 '0 degree [K]
        Dim D As Double
        Dva = 0.217 * p_atm * (T / T0) ^ 1.88
        D = Dva / pg
        Return D
    End Function
    'diffusion coefficient of water vapor or dry air in wet air [cm2/s] 
    Public Function GetDv(ByRef rho_v As Double, ByRef rho_l As Double, ByRef dpcdS As Double, ByRef f As Double, ByRef D As Double, ByRef pv As Double)
        Dim Dv As Double
        Dv = D / pv * (rho_v / rho_l) ^ 2 * dpcdS * f
        Return Dv
    End Function

    'resistance factor function considering the tortuosity
    Public Function Getf(ByRef phi As Double, ByRef saturation As Double)
        Dim S As Double = saturation
        Dim f As Double
        f = phi ^ (4 / 3) * (1 - S) ^ (10 / 3)
        Return f
    End Function
    '' Isotherm function 
    'need to analyse and complet Xuande 2020.07.20
    'isotherm functions from David C. 2020.07.30 W(H)
    Public Function Wat(ByVal H As Decimal, ByVal Hol As Decimal, ByRef T As Decimal, ByRef Tijd As Decimal, ByRef tPro As Single, ByRef Vct As Single, ByRef Nct As Single, ByRef EsC As Single, ByRef Wsat As Single, ByRef Hydr As Single, ByRef ciment As Single, ByRef Wol As Boolean) As Decimal
        Dim Time As Decimal
        Dim f1 As Decimal
        Dim f2 As Decimal
        Dim f3 As Decimal
        Dim f4 As Decimal
        Dim f5 As Decimal
        Dim f6 As Decimal
        Dim f7 As Decimal
        Dim f8 As Decimal
        Dim f9 As Decimal
        Dim f10 As Decimal
        Dim f11 As Decimal
        Dim f12 As Decimal
        Dim f13 As Decimal
        Dim Para(3, 3) As Decimal
        Dim b(3) As Decimal
        Dim a(3) As Decimal
        Dim wcOld, wcNew, wNew As Decimal
        Dim i As Short
        Dim j As Short
        Dim tst01 As Boolean = False
        Dim VH As Single
        Dim VHol As Single
        Time = Tijd + CDec(tPro)
        VH = CInt(H * 1000) / 1000
        VHol = CInt(Hol * 1000) / 1000
        H = CInt(H * 100) / 100
        Hol = CInt(Hol * 100) / 100
        If T < -100 Then T = -100
        If T > 100 Then T = 100
        If System.Math.Abs(H - Hol) = 0.01 And System.Math.Abs(VH - VHol) < 0.02 Then Hol = H 'court-circuite les petites valeurs proche 0.05
        If H = Hol Then
            If Wol = True Then
                H = H - 0.1
            Else
                H = H + 0.1
            End If
            tst01 = True
        End If
        If H <= Hol Then
            If tst01 = True Then
                H = H + 0.1
                tst01 = False
            End If
            Wol = True
            ' On the desorption branch
            f1 = 0.125                                                          'c1
            f2 = 0.173 - 0.431 * ((T - 20) / 25)                                'c2
            f3 = 0.06 + 0.392 * ((T - 20) / 25)                                 'c3
            f4 = 0.17 + (0.035 + 0.029 * (EsC - 0.4) / 0.15) * ((T - 20) / 25)  'c4

            f5 = 0.389 - 26 * EsC / 15 + 4.4 * EsC ^ 2 - 8 * EsC ^ 3 / 3        'we
            f6 = 1 - 0.161 * Hydr                                               'ht

            Para(1, 1) = f6 ^ 2             'para1
            Para(1, 2) = 1 - 2 * f6         'para2
            Para(1, 3) = Para(1, 1) - f6    'para3
            Para(2, 1) = -2 * f6            'para4
            Para(2, 2) = -Para(2, 1)        'para5
            Para(2, 3) = 1 - Para(1, 1)     'para6
            Para(3, 1) = 1                  'para7
            Para(3, 2) = -1                 'para8
            Para(3, 3) = f6 - 1             'para9

            b(1) = EsC - f4 * Hydr                          'b1
            b(2) = (f1 + f2 * f6 + f3 * f6 ^ 2) * Hydr      'b2
            b(3) = (f2 + 2 * f3 * f6) * Hydr                'b3

            For i = 1 To 3
                a(i) = 0
            Next

            For i = 1 To 3
                For j = 1 To 3
                    a(i) = a(i) + Para(i, j) * b(j)
                Next
            Next

            f7 = 1 / (1 - f6) ^ 2
            For i = 1 To 3
                a(i) = a(i) * f7         'a, b, c
            Next
            f8 = 0                                                  'A
            f9 = Hydr / 0.35 * (-f1 / 0.35 + 0.35 * f3)             'C
            f10 = (f2 + 0.7 * f3) * Hydr - 0.7 * f9                 'B
            f11 = (a(1) + a(2) * 1.0 + a(3) * 1.0 ^ 2) * ciment     'wm
            f11 = ciment * Wsat / f11

            Select Case Hol
                Case Is <= 0.35
                    wcOld = (f8 + f10 * Hol + f9 * Hol ^ 2) * f11
                Case Is < f6
                    wcOld = (f1 + f2 * Hol + f3 * Hol ^ 2) * Hydr * f11
                Case Else
                    wcOld = (a(1) + a(2) * Hol + a(3) * Hol ^ 2) * f11
            End Select
            Select Case H
                Case Is <= 0.35
                    wcNew = (f8 + f10 * H + f9 * H ^ 2) * f11
                Case Is < f6
                    wcNew = (f1 + f2 * H + f3 * H ^ 2) * Hydr * f11
                Case Else
                    wcNew = (a(1) + a(2) * H + a(3) * H ^ 2) * f11
            End Select

            If wcOld > Wsat Then wcOld = Wsat
            If wcNew > Wsat Then wcNew = Wsat
            If wcOld < 0 Then wcOld = 0
            If wcNew < 0 Then wcNew = 0

            Wat = wcNew

        Else
            If tst01 = True Then
                H = H - 0.1
                tst01 = False
            End If
            Wol = False
            ' On the sorption branche
            If Time < CDec(5) Then      'calcul de Vt(t) correstpondant à f2 et Nt(t) = f3
                f2 = CDec(0.024)
                f3 = CDec(5.5)
            Else
                f2 = CDec(0.068) - CDec(0.22) / Time
                f3 = CDec(2.5) + CDec(15) / Time
            End If
            f2 = f2 * CDec(Vct) * (CDec(0.85) + CDec(0.45) * CDec(EsC)) * CDec(1.0) ' calcul de vm
            f3 = f3 * (CDec(0.33) + CDec(2.2) * CDec(EsC)) * CDec(Nct) * CDec(1.0) ' calcul de n
            f4 = CDec(System.Math.Exp(855 / (273.16 + T))) ' calcul de C
            f5 = ((CDec(1) - CDec(1) / f3) * f4 - CDec(1)) / (f4 - CDec(1)) ' calcul de k
            f6 = ciment * (1 + EsC)
            f9 = f6 * f4 * f5 * f2 * CDec(1.0) / ((CDec(1) - f5 * CDec(1.0)) * (CDec(1) + (f4 - CDec(1)) * f5 * CDec(1.0))) 'calcul de wmax
            wcOld = f6 * f4 * f5 * f2 * Hol / ((CDec(1) - f5 * Hol) * (CDec(1) + (f4 - CDec(1)) * f5 * Hol))
            wcNew = f6 * f4 * f5 * f2 * H / ((CDec(1) - f5 * H) * (CDec(1) + (f4 - CDec(1)) * f5 * H))
            wcOld = wcOld * Wsat / f9
            wcNew = wcNew * Wsat / f9

            If wcOld > Wsat Then wcOld = Wsat
            If wcNew > Wsat Then wcNew = Wsat
            If wcOld < 0 Then wcOld = 0
            If wcNew < 0 Then wcNew = 0

            Wat = wcNew

        End If

        'If CDbl(H * 1000) <= CDbl(Hol * 1000) Then Water = Wol
        If Wat > Wsat Then Wat = Wsat
        If Wat < 0 Then Wat = 0
        Hol = H
    End Function
    'isotherm functions modified from David C. and reformed by Xuande C. 2020.07.30 S(H)
    Public Function GetHtoS(ByRef H As Double, ByRef type As Integer, ByRef W_C_ratio As Double, ByRef T As Double, ByRef Day As Double, ByRef rho_l As Double, ByRef rho_c As Double)
        Dim VT As Double = 1
        Dim NT As Double = 1
        Dim C0 As Double = 855

        ' Adsorption BET model [Bazant]
        Dim C As Double = Cfunc(C0, T)
        Dim Vm As Double = Vmfunc(Day, W_C_ratio, type, VT)
        Dim n As Double = nfunc(Day, W_C_ratio, type, NT)
        Dim k As Double = kfunc(C, n)
        Dim wc1 As Double = (C * k * Vm * 1) / ((1 - k * 1) * (1 + (C - 1) * k * 1))
        Dim phi As Double = wc1 * (rho_c / rho_l)
        Dim wc As Double = (C * k * Vm * H) / ((1 - k * H) * (1 + (C - 1) * k * H))
        Dim S As Double = (wc / phi) * (rho_c / rho_l)
        'S = H
        Return S
    End Function
    Public Function Cfunc(ByRef C0 As Double, ByRef T As Integer)
        Dim C As Double = Math.Exp((C0 / T))
        Return C
    End Function

    'get average of humidity on an element 
    Public Function GetAvgH(ByRef He() As Double)
        Dim H_avg As Double
        H_avg = He.Average()
        Return H_avg
    End Function
End Module
