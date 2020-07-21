Option Explicit On
Imports System
Imports System.ComponentModel
Imports System.IO
Imports System.Linq
' Module pour calcul Transport Hydrique 2D Xuande.2020.07.15
' implémentation des équations de capillarité, de perméabilité et d'isotherme de adsorption/désorption
Public Class HydriqueXC
    Private NNodes, NElements As Integer
    Private Nodes() As NodeTrans
    Private Elements() As ElementTrans
    Public Sub Analyse(ByRef _NNodes As Integer, ByRef _NElements As Integer, ByRef _Nodes() As NodeTrans, ByRef _Elements() As ElementTrans)
        ' start of computations
        MsgBox("Calcul Transport Hydrique 2D", MsgBoxStyle.OkOnly And MsgBoxStyle.Information, "Start")

        ' computational parameter control
        NNodes = _NNodes
        NElements = _NElements
        Nodes = _Nodes
        Elements = _Elements
        Dim nDof As Integer = NNodes
        Dim H_int As Double = 1 'initial relative humidity
        Dim S_int As Double 'initial relative humidity
        Dim H_bound As Double = 0.25 'boundary relative humidity
        Dim dt As Double = 1 'time interval (s)
        Dim tmax As Double = 100 'end time (s)
        Dim ind As Double = tmax / dt
        Dim T(ind) As Double 'time vector (days)
        Dim S_mat(ind, NNodes - 1) As Double 'Matrix for stockage of computation results (days, number of nodes)
        Dim Hold(NNodes - 1) As Double
        Dim Hnew(NNodes - 1) As Double

        Dim temperature As Double = 273 '(K)
        Dim pg As Double = 101325 'atmosphere pressure(pa)
        Dim rho_v As Double = 1 'density of vapor (kg/m3)
        Dim rho_l As Double = 1000 'density of liquid (kg/m3)
        Dim alpha As Double = 18.6237 ' parameter for ordinary concrete / for cement paste 37.5479
        Dim beta As Double = 2.2748 ' parameter for ordinary concrete / for cement paste 2.1684
        Dim KK As Double = 1.0E-18 'intrinsic permeability (m2)
        Dim yita_l As Double = 0.0011 'viscosity of water (kg/m.s)
        Dim phi As Double = 0.05 'porosity (-)

        Dim S_old(NNodes - 1) As Double
        Dim S_new(NNodes - 1) As Double
        Dim jj As Long
        Dim nFic1 As Short
        Dim outfile(1) As String
        Dim S_sauv As Single = 20 'ouput time inteval (s)
        Dim PosNode() As Integer
        Dim Hteller As Double
        Hteller = CDbl(0)
        'Globlal time loop
        Dim ti As Integer
        Dim i, j， k As Integer
        For ti = 0 To ind

            'step 0: Creating output .txt computation result file 2020-07-17 Xuande 
            outfile(1) = "R_H_" & ".txt"
            nFic1 = CShort(FreeFile())
            FileOpen(CInt(nFic1), outfile(1), OpenMode.Output)

            'step 0: Initialize output titres for result .txt files
            Print(nFic1, "Relative Humidity Field,", Int(tmax / S_sauv), "_", nDof, TAB)
            For jj = 0 To nDof - 1
                Print(CInt(nFic1), jj + CShort(1), ",", TAB)
            Next jj
            PrintLine(CInt(nFic1), " ")
            Print(CInt(nFic1), "0", ",", "0", ",", TAB)
            For jj = 0 To nDof - 1
                Print(CInt(nFic1), Hnew(jj), ",", TAB)
            Next jj
            PrintLine(CInt(nFic1), " ")
            '''-----------------------------------------------------------------------------------------------
            '''-----------------------------------------------------------------------------------------------
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

            'step 2: elemental and global Matrix constructions
            Dim LHS(,) As Double
            Dim R(,) As Double
            Dim RHS() As Double
            Dim bg(nDof - 1, nDof - 1) As Double 'Global b matrix
            Dim Ag(nDof - 1, nDof - 1) As Double 'Global A matrix

            Dim cie As CIETrans
            Dim he As HETrans
            Dim D As Double
            Dim Dv As Double
            Dim Dl As Double
            Dim se As Double 'element average saturation
            Dim Havg As Double
            Dim kr As Double
            Dim pc As Double
            Dim dpcdS As Double
            Dim f As Double

            Dim be(,) As Double
            Dim Ae(,) As Double
            Dim ie As Integer
            'Matrix assembling
            For i = 0 To NElements - 1
                Dim Hele() As Double
                he = New HETrans(
                          Hold(Elements(i).Node1 - 1), Hold(Elements(i).Node2 - 1),
                          Hold(Elements(i).Node3 - 1), Hold(Elements(i).Node4 - 1)
                          )
                Hele = he.getHe
                D = GetD(temperature, pg)
                Havg = GetAvgH(Hele)
                se = GetHtoS(Havg)
                kr = Getkr(se, beta)
                pc = Getpc(se, alpha, beta)
                dpcdS = GetdpcdS(se, alpha, beta)
                Dl = GetDl(KK, yita_l, dpcdS, kr)
                f = Getf(phi, se)
                Dv = GetDv(rho_v, rho_l, dpcdS, f, D, pg)
                D = Dv + Dl
                cie = New CIETrans(
                          Nodes(Elements(i).Node1 - 1).x, Nodes(Elements(i).Node1 - 1).y,
                          Nodes(Elements(i).Node2 - 1).x, Nodes(Elements(i).Node2 - 1).y,
                          Nodes(Elements(i).Node3 - 1).x, Nodes(Elements(i).Node3 - 1).y,
                          Nodes(Elements(i).Node4 - 1).x, Nodes(Elements(i).Node4 - 1).y,
                          D)
                be = cie.getbe
                AssembleKg(be, bg, i)
                Ae = cie.getAe
                AssembleKg(cie.getAe, Ag, i)
            Next

            'step 3: check boundary conditions on each noeuds then construct elemental humidity vector / à reviser pour calcul d'une structure complet Xuande.2020.07.20
            For ie = 0 To NNodes - 1
                If Nodes(ie).Bord = True Then
                    S_old(ie) = H_bound
                End If
            Next

            'step 4: now, we have assembled Hg_old, Ag and bg , to get LHS and RHS
            LHS = getNewLHS(NNodes, phi, Ag, bg, dt)
            R = getNewR(NNodes, phi, Ag, bg, dt)

            'step 5: matrix & vector mulplification 
            RHS = MultiplyMatrixWithVector(R, S_old)

            'step 6: now with LHS*x = RHS, using Gauss Elimination we can get the resolution for the new field of humidity Hnew
            S_new = GetX(LHS, RHS)
            ' compute Hnew as well
            'Hnew = S_new.GetHtoS

            'step 7: data stockage
            For j = 0 To NNodes - 1
                S_mat(ti, j) = S_new(j)
            Next

            'step 7: result .txt file update
            If ti >= CLng(Hteller) Then '1 an ou 365 jours
                Hteller = Hteller + CDbl(S_sauv)
                RegisterS(nFic1, ti, nDof, S_new)
                PrintLine(CInt(nFic1), " ")
            End If

            FileClose(CInt(nFic1))
        Next
        Beep()
        MsgBox("Fin du Calcul Transport Hydrique 2D", MsgBoxStyle.OkOnly And MsgBoxStyle.Information, "Fin")

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
        Print(CInt(nFic1), Temps / 365, ",", Temps, ",", TAB)
        For j = 0 To Dofs - 1
            Print(CInt(nFic1), S_new(j), ",", TAB) '% humidité relative dans le béton
        Next j
    End Sub
End Class

