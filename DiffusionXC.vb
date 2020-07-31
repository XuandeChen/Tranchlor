Option Explicit On
Imports System
Imports System.ComponentModel
Imports System.IO
Imports System.Linq
' Module de calcul diffusion 2D Xuande.2020.07.20
Public Class DiffusionXC
    Private NNodes, NElements As Integer
    Private Nodes() As NodeTrans
    Private Elements() As ElementTrans
    Public Sub Analyse(ByRef _NNodes As Integer, ByRef _NElements As Integer, ByRef _Nodes() As NodeTrans, ByRef _Elements() As ElementTrans, ByRef directory As String)
        'Computational parameter control
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
        Dim day As Double = 0 'porosity (-)
        Dim Temperature As Double = 20 'temperature (c), attention, faudrait l'inserer dans le boucle parce que cela va varier en fonction de temps et espace, XC 2020.07.30
        Dim D0 As Double = 0.00031  ' mm2/s
        Dim alpha_0 As Double = 0.05
        Dim Hc As Double = 0.75
        'Computation parameters
        Dim nDof As Integer = NNodes
        Dim H_int As Double = 0.999 'initial relative humidity
        Dim H_bound As Double = 0.6 'boundary relative humidity
        Dim dt As Double = 3600 'time interval (day)
        Dim tmax As Double = 259200 'end time (s) = 3 days = 72h
        Dim ind As Double = tmax / dt
        Dim T(ind) As Double 'time vector 
        Dim Hm(ind, NNodes - 1) As Double 'Matrix for stockage of computation results (days, number of nodes)
        Dim H_old(NNodes - 1) As Double
        Dim H_new(NNodes - 1) As Double
        Dim jj As Long
        Dim nFic1 As Short
        Dim outfile(1) As String
        Dim T_sauv As Single = 14400 'ouput time inteval (s) = 3 hours
        Dim i, j, k As Integer

        'step 0: Creating output .txt computation result file 2020-07-17 Xuande 
        outfile(1) = directory & "\" & "R_H" & ".txt"
        nFic1 = CShort(FreeFile())
        FileOpen(CInt(nFic1), outfile(1), OpenMode.Output)

        'step 0: Initialize output titres for result .txt files
        Print(nFic1, "RH", ",", nDof, ",", TAB)
        For jj = 0 To nDof - 1
            Print(CInt(nFic1), jj + CShort(1), ",", TAB)
        Next jj
        PrintLine(CInt(nFic1), " ")
        Print(CInt(nFic1), "0", ",", "0", ",", TAB)
        For jj = 0 To nDof - 1
            Print(CInt(nFic1), H_int, ",", TAB)
        Next jj
        PrintLine(CInt(nFic1), " ")
        'Globlal time loop
        Dim ti As Integer
        For ti = 0 To ind
            ' step 1: initialisation
            T(ti) = 0 + dt * (ti - 0)
            If ti = 0 Then
                For i = 0 To nDof - 1
                    H_old(i) = H_int
                Next
            Else
                H_old = H_new 'H_t  = H_t-1
            End If

            'step 2: check boundary conditions on each noeuds then construct elemental humidity vector / à reviser pour calcul d'une structure complet Xuande.2020.07.27
            Dim ie As Integer
            For ie = 0 To NNodes - 1
                If Nodes(ie).Bord = True Then
                    H_old(ie) = H_bound
                End If
            Next

            'step 3: elemental and global Matrix constructions
            Dim LHS(,) As Double
            Dim R(,) As Double
            Dim RHS() As Double
            Dim bg(nDof - 1, nDof - 1) As Double 'Global b matrix
            Dim Ag(nDof - 1, nDof - 1) As Double 'Global A matrix
            Dim cie As CIETrans
            Dim he As HETrans
            Dim H_avg As Double
            Dim H_ele() As Double
            Dim De As Double
            'Matrix assembling
            For i = 0 To NElements - 1
                he = New HETrans(
                          H_old(Elements(i).Node1 - 1), H_old(Elements(i).Node2 - 1),
                          H_old(Elements(i).Node3 - 1), H_old(Elements(i).Node4 - 1)
                          )
                H_ele = he.getHe
                H_avg = GetAvgH(H_ele)
                De = GetDh(D0, alpha_0, Hc, Temperature, H_avg)
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
            getLHS(LHS, NNodes, Ag, bg, dt)
            getRHS(R, NNodes, Ag, bg, dt)
            RHS = MultiplyMatrixWithVector(R, H_old)

            'step 5: now with LHS*x = RHS, using Gauss Elimination we can get the resolution for the new field of humidity Hnew
            GetX(H_new, LHS, RHS)

            'step 6: data stockage
            For j = 0 To NNodes - 1
                Hm(ti, j) = H_new(j)
            Next

            'step 7: result .txt file update
            If (ti * dt / T_sauv) = Int(ti * dt / T_sauv) Then ' check register time 
                RegisterH(nFic1, ti * dt, nDof, H_new)
                PrintLine(CInt(nFic1), " ")
            End If

        Next
        FileClose(CInt(nFic1))
        MsgBox("Fin du calcul diffusion 2D", MsgBoxStyle.OkOnly And MsgBoxStyle.Information, "End")

    End Sub

    'Assembling global matrix /water diffusion
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
    'Assembling global vector /water diffusion
    Private Function AssembleVg(ByRef ve() As Double, ByRef H() As Double, ElementNo As Integer)
        Dim i As Integer
        Dim Vg() As Double = H
        Dim dofs() As Integer = {getDOF(Elements(ElementNo).Node1 - 1),
                                 getDOF(Elements(ElementNo).Node2 - 1),
                                 getDOF(Elements(ElementNo).Node3 - 1),
                                 getDOF(Elements(ElementNo).Node4 - 1)}
        Dim dofi As Integer
        For i = 0 To 3 'each dof of the ve
            dofi = dofs(i)
            Vg(dofi) = Vg(dofi) + ve(i)
        Next
        Return Vg
    End Function

    'Enregistrement des données dans les fichiers d'output
    Private Sub RegisterH(ByRef nFic1 As Short, ByRef Temps As Decimal, ByRef Dofs As Short, ByRef H_new() As Double)
        Dim j As Short
        'Register values
        Print(CInt(nFic1), Temps / 3600, ",", Temps, ",", TAB)
        For j = 0 To Dofs - 1
            Print(CInt(nFic1), H_new(j), ",", TAB) '% humidité relative dans le béton
        Next j
    End Sub
End Class
