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
    Public Sub Analyse(ByRef _NNodes As Integer, ByRef _NElements As Integer, ByRef _Nodes() As NodeTrans, ByRef _Elements() As ElementTrans)
        ' start of computations
        MsgBox("Calcul diffusion 2D", MsgBoxStyle.OkOnly And MsgBoxStyle.Information)
        'Computational parameter control
        NNodes = _NNodes
        NElements = _NElements
        Nodes = _Nodes
        Elements = _Elements
        Dim nDof As Integer = NNodes
        Dim H_int As Double = 1 'initial relative humidity
        Dim H_bound As Double = 0.25 'boundary relative humidity
        Dim DiffCoeff As Double = 0.0002
        Dim dt As Double = 1 'time interval (s)
        Dim tmax As Double = 100 'end time (s)
        Dim ind As Double = tmax / dt
        Dim T(ind) As Double 'time vector (days)
        Dim Hm(ind, NNodes - 1) As Double 'Matrix for stockage of computation results (days, number of nodes)
        Dim Hold(NNodes - 1) As Double
        Dim Hnew(NNodes - 1) As Double
        Dim jj As Long
        Dim nFic1 As Short
        Dim outfile(1) As String
        Dim Hsauv As Single = 20 'ouput time inteval (s)
        Dim PosNode() As Integer
        Dim Hteller As Double
        Hteller = CDbl(0)
        'Globlal time loop
        Dim ti As Integer
        Dim i, j As Integer
        For ti = 0 To ind

            'step 0: Creating output .txt computation result file 2020-07-17 Xuande 
            outfile(1) = "R_H_" & ".txt"
            nFic1 = CShort(FreeFile())
            FileOpen(CInt(nFic1), outfile(1), OpenMode.Output)
            'step 0: Initialize output titres for result .txt files
            Print(nFic1, "Relative Humidity Field,", Int(tmax / Hsauv), "_", nDof, TAB)
            For jj = 0 To nDof - 1
                Print(CInt(nFic1), jj + CShort(1), ",", TAB)
            Next jj
            PrintLine(CInt(nFic1), " ")
            Print(CInt(nFic1), "0", ",", "0", ",", TAB)
            For jj = 0 To nDof - 1
                Print(CInt(nFic1), Hnew(jj), ",", TAB)
            Next jj
            PrintLine(CInt(nFic1), " ")

            ' step 1: initialisation
            T(ti) = 0 + dt * (ti - 0)
            If ti = 0 Then
                For i = 0 To nDof - 1
                    Hold(i) = H_int
                Next
            Else
                Hold = Hnew
            End If

            'step 2: elemental and global Matrix constructions
            Dim LHS(,) As Double
            Dim R(,) As Double
            Dim RHS() As Double
            Dim bg(nDof - 1, nDof - 1) As Double 'Global b matrix
            Dim Ag(nDof - 1, nDof - 1) As Double 'Global A matrix
            Dim cie As CIETrans
            Dim he As HETrans
            Dim k As Integer
            Dim ie As Integer
            'Matrix assembling
            For i = 0 To NElements - 1
                Dim Hele() As Double
                cie = New CIETrans(
                          Nodes(Elements(i).Node1 - 1).x, Nodes(Elements(i).Node1 - 1).y,
                          Nodes(Elements(i).Node2 - 1).x, Nodes(Elements(i).Node2 - 1).y,
                          Nodes(Elements(i).Node3 - 1).x, Nodes(Elements(i).Node3 - 1).y,
                          Nodes(Elements(i).Node4 - 1).x, Nodes(Elements(i).Node4 - 1).y,
                          DiffCoeff)
                he = New HETrans(
                          Hold(Elements(i).Node1 - 1), Hold(Elements(i).Node2 - 1),
                          Hold(Elements(i).Node3 - 1), Hold(Elements(i).Node4 - 1)
                          )
                AssembleKg(cie.getbe, bg, i)
                AssembleKg(cie.getAe, Ag, i)

            Next

            'step 3: check boundary conditions on each noeuds then construct elemental humidity vector / à reviser pour calcul d'une structure complet Xuande.2020.07.10
            For ie = 0 To NNodes - 1
                If Nodes(ie).Bord = True Then
                    Hold(ie) = H_bound
                End If
            Next

            'step 4: now, we have assembled Hg_old, Ag and bg , to get LHS and RHS
            LHS = getLHS(NNodes, Ag, bg, dt)
            R = getRHS(NNodes, Ag, bg, dt)

            'step 5: matrix & vector mulplification 
            RHS = MultiplyMatrixWithVector(R, Hold)
            'step 6: now with LHS*x = RHS, using Gauss Elimination we can get the resolution for the new field of humidity Hnew
            Hnew = GetX(LHS, RHS)

            'step 7: data stockage
            For j = 0 To NNodes - 1
                Hm(ti, j) = Hnew(j)
            Next
            'step 7: result .txt file update

            If ti >= CLng(Hteller) Then '1 an ou 365 jours
                Hteller = Hteller + CDbl(Hsauv)
                RegisterH(nFic1, ti, nDof, Hnew)
                PrintLine(CInt(nFic1), " ")
            End If

            FileClose(CInt(nFic1))
        Next
        Beep()
        MsgBox("Fin du calcul diffusion 2D", MsgBoxStyle.OkOnly And MsgBoxStyle.Information, "Fin")

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
        Print(CInt(nFic1), Temps / 365, ",", Temps, ",", TAB)
        For j = 0 To Dofs - 1
            Print(CInt(nFic1), H_new(j), ",", TAB) '% humidité relative dans le béton
        Next j
    End Sub
End Class
