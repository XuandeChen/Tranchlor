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
        ' start of computations
        MsgBox("Calcul diffusion 2D", MsgBoxStyle.OkOnly And MsgBoxStyle.Information, "Start")
        'Computational parameter control
        NNodes = _NNodes
        NElements = _NElements
        Nodes = _Nodes
        Elements = _Elements
        Dim nDof As Integer = NNodes
        Dim H_int As Double = 0.999 'initial relative humidity
        Dim H_bound As Double = 0.6 'boundary relative humidity
        Dim DiffCoeff As Double = 0.000217  ' m2/s
        Dim dt As Double = 3600 'time interval (day)
        Dim tmax As Double = 259200 'end time (s) = 3 days
        Dim ind As Double = tmax / dt
        Dim T(ind) As Double 'time vector (days)
        Dim Hm(ind, NNodes - 1) As Double 'Matrix for stockage of computation results (days, number of nodes)
        Dim H_old(NNodes - 1) As Double
        Dim H_new(NNodes - 1) As Double
        Dim jj As Long
        Dim nFic1 As Short
        Dim outfile(1) As String
        Dim T_sauv As Single = 14400 'ouput time inteval (s) = 3 hours

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
        Dim i, j, k As Integer
        For ti = 0 To ind
            ' step 1: initialisation
            T(ti) = 0 + dt * (ti - 0)
            If ti = 0 Then
                For i = 0 To nDof - 1
                    H_old(i) = H_int
                Next
            Else
                H_old = H_new ' H_t  = H_t-1
            End If


            'step 2: check boundary conditions on each noeuds then construct elemental humidity vector / à reviser pour calcul d'une structure complet Xuande.2020.07.27
            Dim ie As Integer
            For ie = 0 To NNodes - 1
                If Nodes(ie).Bord = True Then
                    H_new(ie) = H_bound
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

            'Matrix assembling
            For i = 0 To NElements - 1
                cie = New CIETrans(
                          Nodes(Elements(i).Node1 - 1).x, Nodes(Elements(i).Node1 - 1).y,
                          Nodes(Elements(i).Node2 - 1).x, Nodes(Elements(i).Node2 - 1).y,
                          Nodes(Elements(i).Node3 - 1).x, Nodes(Elements(i).Node3 - 1).y,
                          Nodes(Elements(i).Node4 - 1).x, Nodes(Elements(i).Node4 - 1).y,
                          DiffCoeff)
                he = New HETrans(
                          H_old(Elements(i).Node1 - 1), H_old(Elements(i).Node2 - 1),
                          H_old(Elements(i).Node3 - 1), H_old(Elements(i).Node4 - 1)
                          )
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

            FileClose(CInt(nFic1))
        Next
        Beep()
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
