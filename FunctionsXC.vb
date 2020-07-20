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
        Dim m As Double = 1 / beta
        Dim kr As Double
        kr = Math.Sqrt(1 - S) * (1 - S ^ (1 / m)) ^ (2 * m)
        Return kr
    End Function

    'liquid capillary pressure function (Equation and formula from Marc Mainguy, 2001)
    Public Function Getpc(ByRef saturation As Double, ByRef alpha As Double, ByRef beta As Double)
        Dim S As Double = saturation
        'Dim alpha As Double = 18.6237 'BO, for CO alpha = 37.5479
        'Dim beta As Double = 2.2748 'BO, for CO beta = 2.1684
        Dim pc As Double
        pc = alpha * (S ^ (-beta) - 1) ^ (1 - 1 / beta)
        Return pc
    End Function
    'derivation of the capillary pressure function 
    Public Function GetdpcdS(ByRef saturation As Double, ByRef alpha As Double, ByRef beta As Double)
        Dim dpcdS As Double
        Dim S As Double = saturation
        dpcdS = alpha * (S ^ (-beta) - 1) ^ (1 - 1 / beta)
        Return dpcdS
    End Function

    'liquid-water contribution to the moisture diffusivity [cm2/s] 
    Public Function GetDl(ByRef K As Double, ByRef yita_l As Double, ByRef dpcdS As Double, ByRef kr As Double)
        Dim Dl As Double
        Dl = dpcdS * K * kr / yita_l
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
End Module
