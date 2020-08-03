Imports System.Linq

Module Functions2D
    'get the LHS matrix for Gauss matrix resolution
    Public Function getLHS(ByRef LHS As Double(,), ByRef NNodes As Integer, ByRef A(,) As Double, ByRef b(,) As Double, ByRef dt As Double)
        ReDim LHS(NNodes - 1, NNodes - 1)
        Dim i, j As Integer
        For i = 0 To NNodes - 1
            For j = 0 To NNodes - 1
                LHS(i, j) = A(i, j) / 2 + b(i, j) / dt
            Next
        Next
    End Function

    'Public Function getNewLHS(ByRef NNodes As Integer, ByRef phi As Integer, ByRef A(,) As Double, ByRef b(,) As Double, ByRef dt As Double)
    '    Dim LHS(NNodes - 1, NNodes - 1) As Double
    '    Dim i, j As Integer
    '    For i = 0 To NNodes - 1
    '        For j = 0 To NNodes - 1
    '            LHS(i, j) = phi * A(i, j) / 2 + b(i, j) / dt
    '        Next
    '    Next
    '    Return LHS
    'End Function
    'get the RHS matrix for Gauss matrix resolution
    Public Function getRHS(ByRef RHS As Double(,), ByRef NNodes As Integer, ByRef A(,) As Double, ByRef b(,) As Double, ByRef dt As Double)
        ReDim RHS(NNodes - 1, NNodes - 1)
        Dim i, j As Integer
        For i = 0 To NNodes - 1
            For j = 0 To NNodes - 1
                RHS(i, j) = b(i, j) / dt - A(i, j) / 2
            Next
        Next
    End Function

    'Public Function getNewR(ByRef NNodes As Integer, ByRef phi As Integer, ByRef A(,) As Double, ByRef b(,) As Double, ByRef dt As Double)
    '    Dim R(NNodes - 1, NNodes - 1) As Double
    '    Dim i, j As Integer
    '    For i = 0 To NNodes - 1
    '        For j = 0 To NNodes - 1
    '            R(i, j) = b(i, j) / dt - phi * A(i, j) / 2
    '        Next
    '    Next
    '    Return R
    'End Function
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
    Public Function GetX(ByRef X As Double(), ByRef A(,) As Double, ByRef b() As Double)
        Dim aRows As Integer = A.GetLength(0)
        Dim aCols As Integer = A.GetLength(1)
        Dim bRows As Integer = b.GetLength(0)
        Dim m As Double
        Dim i, j, k As Integer
        Dim sum As Double
        Dim s As Integer = bRows
        ReDim X(s - 1)

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
    End Function

    ''liquid water transport functions: (Equation and formula from Marc Mainguy, 2001)
    'relative permeability function (Equation and formula from Marc Mainguy, 2001 & derived from VanGnuchten, 1980) 
    Public Function Getkr(ByRef saturation As Double, ByRef beta As Double) As Double
        Dim S As Double = saturation
        Dim sa_irr As Double = 0.00001
        Dim sb_irr As Double = 0.00001
        Dim s_eff As Double = (S - sb_irr) / (1 - sa_irr - sb_irr)
        Dim kr_b_max As Double = 1
        Dim kr As Double = kr_b_max * s_eff ^ beta
        Return kr
    End Function

    'liquid capillary pressure function (Equation and formula from Marc Mainguy, 2001)
    Public Function Getpc(ByRef saturation As Double, ByRef pc_0 As Double, ByRef m As Double) As Double
        Dim S As Double = saturation
        Dim n As Double = 1 / (1 - m)
        Dim s_pc_irr As Double = 0.00001
        Dim s_pc_max As Double = 1
        Dim Sb_pc As Double = (S - s_pc_irr) / (s_pc_max - s_pc_irr)
        Dim pc As Double = pc_0 * ((Sb_pc ^ (-1 / m) - 1) ^ (1 / n))
        Return pc
    End Function
    'derivation of the capillary pressure function 
    Public Function GetdpcdS(ByRef saturation As Double, ByRef pc_0 As Double, ByRef m As Double) As Double
        Dim dpcdS As Double
        Dim S As Double = saturation
        Dim n As Double = 1 / (1 - m)
        dpcdS = (pc_0 * (1 / S ^ (1 / m) - 1) ^ (1 / n)) / (m * n * S * (S ^ (1 / m) - 1))
        Return dpcdS
    End Function

    'liquid-water contribution to the moisture diffusivity [cm2/s] 
    Public Function GetDl(ByRef K As Double, ByRef yita_l As Double, ByRef dpcdS As Double, ByRef kr As Double) As Double
        Dim Dl As Double
        Dl = -dpcdS * K * kr / yita_l
        Return Dl
    End Function

    'water vapor transport functions: 
    'diffusion coefficient of water vapor  [mm2/s] (Equation and formula from Bazant, 2001)
    Public Function GetDh(ByRef DT0 As Double, ByRef alpha_0 As Double, ByRef Hc As Double, ByRef T As Double, ByRef H As Double) As Double
        Dim n As Integer = 4
        Dim Q As Double = 40000 ' [mol/J] energie d'activation du modele Arrhenius
        Dim R As Double = 8.31451
        Dim T_0 As Double = 20  '[C] temperature de base du beton lors de la determination de Q et de DT_0

        Dim Dh As Double = DT0 * ((alpha_0 + (1 - alpha_0) / (1 + ((1 - H) / (1 - Hc)) ^ n)) * Math.Exp((Q / R * (1 / T_0 - 1 / T))))
        Return Dh
    End Function
    'diffusion coefficient of water vapor or dry air in wet air [cm2/s] (Equation and formula from Marc Mainguy, 2001)
    Public Function GetD(ByRef Tk As Double, ByRef pg As Double) As Double
        Dim Dva As Double
        Dim p_atm As Double = 101325 'atmosphere pressure [pa]
        Dim T0 As Double = 273 '0 degree [K]
        Dim D As Double
        Dva = 0.217 * p_atm * ((Tk / T0) ^ 1.88)
        D = Dva / pg
        Return D
    End Function
    'diffusion coefficient of water vapor or dry air in wet air [cm2/s] 
    Public Function GetDv(ByRef rho_v As Double, ByRef rho_l As Double, ByRef dpcdS As Double, ByRef f As Double, ByRef D As Double, ByRef pv As Double) As Double
        Dim Dv As Double
        Dv = D / pv * (rho_v / rho_l) ^ 2 * dpcdS * f
        Return Dv
    End Function
    'resistance factor function considering the tortuosity
    Public Function Getf(ByRef phi As Double, ByRef saturation As Double) As Double
        Dim S As Double = saturation
        Dim f As Double
        f = phi ^ (4 / 3) * (1 - S) ^ (10 / 3)
        Return f
    End Function
    ' saturated water function 
    Public Function GetWsat(ByRef C As Double, ByRef alpha As Double, ByRef W_C_ratio As Double, ByRef phi As Double) As Double
        Dim Wsat As Double = C * W_C_ratio - (0.17 * alpha * C) + 10 * phi
        Return Wsat
    End Function
    'isotherm function 
    Public Function GetHtoS(ByRef H As Double, ByRef type As Integer, ByRef C As Double, ByRef W_C_ratio As Double, ByRef Tk As Double, ByRef day As Double, ByRef rho_l As Double, ByRef rho_c As Double, ByRef alpha As Double, ByRef w As Double) As Double
        Dim NT As Double = 1
        Dim VT As Double = 1
        Dim C0 As Double = 855
        ' the adsorption curve from [Xi 1994]
        Dim Cbet As Double = Cfunc(C0, Tk)
        Dim Vm As Double = Vmfunc(day, W_C_ratio, type, VT)
        Dim n As Double = nfunc(day, W_C_ratio, type, NT)
        Dim k As Double = kfunc(C, n)
        Dim wc1 As Double = (C * k * Vm * 1) / ((1 - k * 1) * (1 + (C - 1) * k * 1)) 'when H = 100% = 1
        Dim phi As Double = wc1 * (rho_c / rho_l) 'intermediate term
        Dim wc As Double = (C * k * Vm * H) / ((1 - k * H) * (1 + (C - 1) * k * H))
        Dim Sa As Double = wc / wc1
        ' the desorption curve from [Roelfstra 1989]
        Dim Tc As Double = Tk - 273 'temperature in [C]
        Dim Ht As Double = 1 - 0.161 * alpha
        Dim c1 As Double = 0.125
        Dim c2 As Double = 0.173 - (0.431 * (Tc - 20) / 25)
        Dim c3 As Double = 0.06 + 0.392 * ((Tc - 20) / 25)
        Dim c4 As Double = 0.17 + (0.035 + 0.029 * (W_C_ratio - 0.4) / 0.15) * ((Tc - 20) / 25)
        Dim Sd As Double
        Dim wr1 As Double
        Dim wr As Double
        Dim Mat1(2, 2) As Double
        Mat1(0, 0) = Ht ^ 2
        Mat1(0, 1) = 1 - 2 * Ht
        Mat1(0, 2) = Ht ^ 2 - Ht
        Mat1(1, 0) = -2 * Ht
        Mat1(1, 1) = 2 * Ht
        Mat1(1, 2) = 1 - Ht ^ 2
        Mat1(2, 0) = 1
        Mat1(2, 1) = -1
        Mat1(2, 2) = Ht - 1
        Dim Vec1(2) As Double
        Vec1(0) = W_C_ratio - c4 * alpha
        Vec1(1) = (c1 + c2 * Ht + c3 * Ht ^ 2) * alpha
        Vec1(2) = (c2 + 2 * c3 * Ht) * alpha
        Dim Vec2() As Double = MultiplyMatrixWithVector(Mat1, Vec1)
        Dim Vec3(2) As Double
        Vec3(0) = 1
        Vec3(1) = H
        Vec3(2) = H ^ 2
        If 1 > H And H >= Ht Then
            wr1 = C * 1 / ((1 - Ht) ^ 2) * (Vec2(0) * 1 + Vec2(1) * 1 + Vec2(2) * 1)
            wr = C * 1 / ((1 - Ht) ^ 2) * (Vec2(0) * Vec3(0) + Vec2(1) * Vec3(1) + Vec2(2) * Vec3(2))
            Sd = wr / wr1
        ElseIf Ht > H And H >= 0.35 Then
            wr1 = C * (c1 + c2 * 1 + c3 * 1 ^ 2) * alpha
            wr = C * (c1 + c2 * H + c3 * H ^ 2) * alpha
            Sd = wr / wr1
        Else
            wr1 = C * (c1 + c2 * 1 + c3 * 1 ^ 2) * alpha
            wr = ((-(400 * c1 / 49) + c3) * H + 40 * c1 / 7 + c2) * alpha * H
            Sd = wr / wr1
        End If
        ' the hysteretic curve
        Dim S As Double
        S = w * Sd + (1 - w) * Sa 'deactivate for test
        Return S
        ' Return H 'activate or deactivate for test
    End Function
    Public Function nfunc(ByRef day As Double, ByRef W_C_ratio As Double, ByRef type As Integer, ByRef NT As Double) As Double
        Dim N_Ct As Double
        If type = 1 Then
            N_Ct = 1.1
        ElseIf type = 2 Then
            N_Ct = 1
        ElseIf type = 3 Then
            N_Ct = 1.15
        Else
            N_Ct = 1.5
        End If
        Dim N_t As Double
        If day > 5 Then
            N_t = 2.5 - 15 / day
        Else
            N_t = 5.5
        End If
        Dim N_EC As Double = 0.33 + 2.2 * W_C_ratio
        Dim n As Double = NT * N_EC * N_Ct * N_t
        Return n
    End Function
    Public Function kfunc(ByRef C As Double, ByRef n As Double) As Double
        Dim k As Double = ((1 - 1 / n) * C - 1) / (C - 1)
        Return k
    End Function
    Public Function Cfunc(ByRef C0 As Double, ByRef T As Double) As Double
        Dim C As Double = Math.Exp(C0 / T)
        Return C
    End Function
    Public Function Vmfunc(ByRef day As Double, ByRef W_C_ratio As Double, ByRef type As Integer, ByRef VT As Double) As Double
        Dim V_Ct As Double
        If type = 1 Then
            V_Ct = 0.9
        ElseIf type = 2 Then
            V_Ct = 1
        ElseIf type = 3 Then
            V_Ct = 0.85
        Else
            V_Ct = 0.6
        End If
        Dim V_t As Double
        If day > 5 Then
            V_t = 0.068 - 0.22 / day
        Else
            V_t = 0.024
        End If
        Dim V_EC As Double = 0.85 + 0.45 * W_C_ratio
        Dim Vm As Double = VT * V_EC * V_Ct * V_t 'capacite de la monocouche
        Return Vm
    End Function
    'get average of humidity on an element 
    Public Function GetAvgH(ByRef He() As Double) As Double
        Dim H_avg As Double
        H_avg = He.Average()
        Return H_avg
    End Function

End Module