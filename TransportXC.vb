Option Explicit On
Imports System
Imports System.ComponentModel
Imports System.IO
Imports System.Linq
' Module pour calcul Transport Hydrique 2D Xuande.2020.07.15
' implémentation des équations de capillarité, de perméabilité et d'isotherme de adsorption/désorption
Public Class TransportXC
    Private NNodes, NElements As Integer
    Private Nodes() As NodeTrans
    Private Elements() As ElementTrans
    Public Sub Analyse(ByRef _NNodes As Integer, ByRef _NElements As Integer, ByRef _Nodes() As NodeTrans, ByRef _Elements() As ElementTrans, ByRef directory As String)
        ' computational parameter control
        NNodes = _NNodes
        NElements = _NElements
        Nodes = _Nodes
        Elements = _Elements
        'Material parameters, can be converted from user defined input
        Dim pg As Double = 101325 'atmosphere pressure(pa)
        Dim rho_v As Double = 1 'density of vapor (kg/m3)
        Dim rho_l As Double = 1000 'density of liquid (kg/m3)
        Dim rho_c As Double = 2500 'density of concrete (kg/m3)
        Dim pc_0 As Double = 28000 ' parameter for ordinary concrete (pa)
        Dim m As Double = 0.5 ' parameter for ordinary concrete / for cement paste 37.5479
        Dim beta As Double = 2 ' parameter for ordinary concrete / for cement paste 2.1684
        Dim KK As Double = 0.000000000002 'intrinsic permeability (m2)
        Dim yita_l As Double = 0.0011 'viscosity of water (kg/m.s)
        Dim phi As Double = 0.05 'porosity (-)
        Dim type As Integer = 3 'cement type (-)
        Dim W_C_ratio As Double = 0.5 'porosity (-)
        Dim C As Double = 375 'density of cement (kg/m3)
        Dim day As Double = 90 'age (day)
        Dim w As Double = 0 'indicator for isotherm curve, judge if we choose to use desorption (w = 1) or adsorption curve (w = 0) 
        Dim alpha As Double = 0.6 'hydration degree (-)
        Dim Tk As Double = 293 'temperature in (K), attention, faudrait l'inserer dans le boucle parce que cela va varier en fonction de temps et espace, XC 2020.07.30
        Dim Tc As Double = Tk - 273 'temperature in (C)
        Dim wsat As Double = GetWsat(C, alpha, W_C_ratio, phi) 'saturated water mass (kg/m3)
        'Computation parameters
        Dim nDof As Integer = NNodes
        Dim H_int As Double = 0.25 'initial relative humidity
        Dim S_int As Double = GetHtoS(H_int, type, C, W_C_ratio, Tk, day, rho_l, rho_c, alpha, w) 'initial saturation field
        Dim H_bound As Double = 0.999 'boundary relative humidity
        Dim S_bound As Double = GetHtoS(H_bound, type, C, W_C_ratio, Tk, day, rho_l, rho_c, alpha, w) 'boundary saturation field
        Dim dt As Double = 3600 'time interval (s)
        Dim tmax As Double = 259200 'end time (s) 72h
        Dim ind As Double = tmax / dt
        Dim T(ind) As Double 'time vector (days)
        Dim S_mat(ind, NNodes - 1) As Double 'Matrix for stockage of computation results (days, number of nodes)
        Dim Hold(NNodes - 1) As Double
        Dim Hnew(NNodes - 1) As Double
        Dim S_old(NNodes - 1) As Double
        Dim S_new(NNodes - 1) As Double
        Dim jj As Long
        Dim nFic1 As Short
        Dim outfile(1) As String
        Dim T_sauv As Single = 14400 'ouput time inteval (s) 4h
        'step 0: Creating output .txt computation result file 2020-07-27 Xuande 
        outfile(1) = directory & "\" & "R_S" & ".txt"
        nFic1 = CShort(FreeFile())
        FileOpen(CInt(nFic1), outfile(1), OpenMode.Output)

        'step 0: Initialize output titres for result .txt files
        Print(nFic1, "S", ",", nDof, ",", TAB)
        For jj = 0 To nDof - 1
            Print(CInt(nFic1), jj + CShort(1), ",", TAB)
        Next jj
        PrintLine(CInt(nFic1), " ")
        Print(CInt(nFic1), "0", ",", "0", ",", TAB)
        For jj = 0 To nDof - 1
            Print(CInt(nFic1), S_int, ",", TAB)
        Next jj
        PrintLine(CInt(nFic1), " ")

        'Globlal time loop
        Dim ti As Integer
        Dim i, j， k As Integer
        For ti = 0 To CInt(ind)

            ' step 1: initialisation saturation field
            T(ti) = 0 + dt * (ti - 0)
            If ti = 0 Then
                For i = 0 To nDof - 1
                    Hold(i) = H_int
                    S_old(i) = S_int
                Next
            Else
                Hold = Hnew
                S_old = S_new
            End If

            'step 2: check boundary conditions on each noeuds then construct elemental humidity vector / à reviser pour calcul d'une structure complet Xuande.2020.07.20
            Dim ie As Integer
            For ie = 0 To NNodes - 1
                If Nodes(ie).Bord = True Then
                    S_bound = GetHtoS(H_bound, type, C, W_C_ratio, Tk, day, rho_l, rho_c, alpha, w)
                    S_old(ie) = S_bound
                End If
            Next

            'step 3: elemental and global Matrix constructions
            Dim LHS(,) As Double
            Dim R(,) As Double
            Dim RHS() As Double
            Dim bg(nDof - 1, nDof - 1) As Double 'Global b matrix
            Dim Ag(nDof - 1, nDof - 1) As Double 'Global A matrix
            Dim cie As CIETrans
            Dim se As SETrans 'element saturation vector
            Dim D As Double = GetD(Tk, pg)
            Dim Dv As Double
            Dim Dl As Double
            Dim S_avg As Double 'element average saturation
            Dim H_avg As Double
            Dim kr As Double
            Dim pc As Double
            Dim dpcdS As Double
            Dim f As Double
            'Matrix assembling
            For i = 0 To NElements - 1
                Dim S_ele() As Double
                se = New SETrans(
                          S_old(Elements(i).Node1 - 1), S_old(Elements(i).Node2 - 1),
                          S_old(Elements(i).Node3 - 1), S_old(Elements(i).Node4 - 1)
                          )
                S_ele = se.getSe
                S_avg = GetAvgH(S_ele)
                kr = Getkr(S_avg, beta)
                pc = Getpc(S_avg, pc_0, m)
                dpcdS = GetdpcdS(S_avg, pc_0, m)
                Dl = GetDl(KK, yita_l, dpcdS, kr)
                f = Getf(phi, S_avg)
                Dv = GetDv(rho_v, rho_l, dpcdS, f, D, pg)
                Dim De As Double = (Dv + Dl) / phi
                cie = New CIETrans(
                          Nodes(Elements(i).Node1 - 1).x, Nodes(Elements(i).Node1 - 1).y,
                          Nodes(Elements(i).Node2 - 1).x, Nodes(Elements(i).Node2 - 1).y,
                          Nodes(Elements(i).Node3 - 1).x, Nodes(Elements(i).Node3 - 1).y,
                          Nodes(Elements(i).Node4 - 1).x, Nodes(Elements(i).Node4 - 1).y,
                          De)
                AssembleKg(cie.getbe, bg, i)
                AssembleKg(cie.getAe, Ag, i)
            Next

            'step 4: now, we have assembled Hg_old, Ag and bg , to get LHS and RHS
            'LHS = getNewLHS(NNodes, phi, Ag, bg, dt)
            'R = getNewR(NNodes, phi, Ag, bg, dt)
            getLHS(LHS, NNodes, Ag, bg, dt)
            getRHS(R, NNodes, Ag, bg, dt)
            RHS = MultiplyMatrixWithVector(R, S_old)

            'step 5: now with LHS*x = RHS, using Gauss Elimination we can get the resolution for the new field of humidity Hnew
            GetX(S_new, LHS, RHS)
            ' compute Hnew as well
            'Hnew = S_new.GetHtoS

            'step 6: data stockage
            For j = 0 To NNodes - 1
                S_mat(ti, j) = S_new(j)
            Next

            'step 7: result .txt file update
            If (ti * dt / T_sauv) = Int(ti * dt / T_sauv) Then ' check register time 
                RegisterS(nFic1, ti * dt, nDof, S_new)
                PrintLine(CInt(nFic1), " ")
            End If
        Next
        FileClose(CInt(nFic1))
        Beep()
        MsgBox("Fin du Calcul Transport Hydrique 2D", MsgBoxStyle.OkOnly And MsgBoxStyle.Information, "End")

    End Sub

    'Assembling global matrix /water transport
    Private Sub AssembleKg(ByRef ke(,) As Double, ByRef Kg(,) As Double, ElementNo As Integer)
        Dim i, j As Integer
        Dim dofs() As Integer = {getDOF(Elements(ElementNo).Node1 - 1),
                                 getDOF(Elements(ElementNo).Node2 - 1),
                                 getDOF(Elements(ElementNo).Node3 - 1),
                                 getDOF(Elements(ElementNo).Node4 - 1)}
        Dim dofi, dofj As Integer
        For i = 0 To 3 'each dof of the Se
            dofi = dofs(i)
            For j = 0 To 3
                dofj = dofs(j)
                Kg(dofi, dofj) = Kg(dofi, dofj) + ke(i, j)
            Next
        Next
    End Sub
    Private Sub RegisterS(ByRef nFic1 As Short, ByRef Temps As Decimal, ByRef Dofs As Short, ByRef S_new() As Double)
        Dim j As Short
        'Register values
        Print(CInt(nFic1), Temps / 3600, ",", Temps, ",", TAB)
        For j = 0 To Dofs - 1
            Print(CInt(nFic1), S_new(j), ",", TAB)
        Next j
    End Sub
End Class

