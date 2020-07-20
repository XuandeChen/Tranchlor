Option Explicit On
Imports System
Imports System.ComponentModel
Imports System.IO
Imports System.Linq
' Module pour calcul Transport Hydrique 2D Xuande.2020.07.15
Public Class HydriqueXC
    Private NNodes, NElements As Integer
    Private Nodes() As NodeTrans
    Private Elements() As ElementTrans
    Public Sub Analyse(ByRef _NNodes As Integer, ByRef _NElements As Integer, ByRef _Nodes() As NodeTrans, ByRef _Elements() As ElementTrans)

        MsgBox("Calcul Transport Hydrique 2D", MsgBoxStyle.OkOnly And MsgBoxStyle.Information, "Début")

        'Computational parameters setup
        NNodes = _NNodes
        NElements = _NElements
        Nodes = _Nodes
        Elements = _Elements
        Dim S As Double 'Saturation degree
        Dim nDof As Integer = NNodes
        Dim S_int As Double = 1 'initial saturation degree
        Dim H_bound As Double = 0.25 'boundary relative humidity conditions 
        Dim D_vap As Double 'water vapor diffusivity
        Dim D_liq As Double 'liquid water diffusivity
        Dim D As Double 'General diffusion coefficient
        Dim dt As Double = 1 'computation time interval (s)
        Dim tmax As Double = 100 'end time (s)
        Dim ind As Double = tmax / dt
        Dim T(ind) As Double 'time vector (days)
        Dim S_mat(ind, NNodes - 1) As Double 'Matrix for stockage of computation results (time, number of nodes, saturation per node)
        Dim S_old(NNodes - 1) As Double
        Dim S_new(NNodes - 1) As Double
        'Initalization
        Dim ti As Integer
        For ti = 0 To ind
            Dim i, j As Integer
            T(ti) = 0 + dt * (ti - 0)
            ' step 1 initialisation
            If ti = 0 Then
                For i = 0 To nDof - 1
                    S_old(i) = S_int
                Next
            Else
                S_old = S_new
            End If
            'elemental and global Matrix constructions
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
                          D)
                AssembleKg(cie.getbe, bg, i)
                AssembleKg(cie.getAe, Ag, i)

            Next

            'check boundary conditions on each noeuds then construct elemental humidity vector / à reviser pour calcul d'une structure complet Xuande.2020.07.10
            For ie = 0 To NNodes - 1
                If Nodes(ie).Bord = True Then
                    Hold(ie) = H_bound
                End If
            Next

            'now, we have assembled Hg_old, Ag and bg , to get LHS and RHS
            LHS = getLHS(NNodes, Ag, bg, dt)
            R = getRHS(NNodes, Ag, bg, dt)

            'matrix & vector mulplification 
            RHS = MultiplyMatrixWithVector(R, Hold)
            'now with LHS*x = RHS, using Gauss Elimination we can get the resolution for the new field of humidity Hnew
            Hnew = getX(LHS, RHS)
            'result update

            'data stockage
            For j = 0 To NNodes - 1
                Hm(ti, j) = Hnew(j)
            Next
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
End Class

