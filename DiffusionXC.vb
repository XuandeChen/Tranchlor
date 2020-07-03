Option Explicit On
Imports System
Imports System.ComponentModel
Imports System.IO
Imports System.Linq
Public Class DiffusionXC
    Private NNodes, NElements As Integer
    Private Nodes() As NodeTrans
    Private Elements() As ElementTrans

    Public Sub Analyse(ByRef _NNodes As Integer, ByRef _NElements As Integer, ByRef _Nodes() As NodeTrans, ByRef _Elements() As ElementTrans)
        'Computational parameter control
        NNodes = _NNodes
        NElements = _NElements
        Nodes = _Nodes
        Elements = _Elements
        Dim nDof As Integer = NNodes
        Dim H_int As Double = 1 'initial relative humidity
        Dim H_bound As Double = 0.25 'boundary relative humidity
        Dim DiffCoeff As Double = 0.0002
        Dim dt As Double = 1 'time interval (days)
        Dim tmax As Double = 100 'end time (days)
        Dim ind As Double = tmax / dt
        Dim Hold(NNodes - 1) As Double
        Dim Hnew(NNodes - 1) As Double
        Dim T(ind) As Double 'time vector (days)
        Dim Hm(ind, NNodes - 1) As Double 'Matrix for stockage of computation results (days)

        'Initalization
        Dim ti As Integer
        For ti = 0 To ind
            T(ti) = 0 + dt * (ti - 0)
            'Matrix constructions
            Dim LHS(,) As Double
            Dim R(,) As Double
            Dim RHS() As Double
            Dim bg(nDof - 1, nDof - 1) As Double 'Global b matrix
            Dim Ag(nDof - 1, nDof - 1) As Double 'Global A matrix
            Dim Hg_old(nDof - 1) As Double 'Global H_old vector
            Dim cie As CIETrans
            Dim he As HETrans
            Dim i As Integer
            Dim j As Integer
            For i = 0 To NElements - 1
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
                AssembleVg(he.getHe, Hg_old, i)
                'apply boundary conditions
            Next
            'now, we have assembled Hg_old, Ag and bg , to get LHS and RHS
            LHS = getLHS(Ag, bg, dt)
            R = getRHS(Ag, bg, dt)
            'matrix & vector mulplification 
            RHS = MultiplyMatrixWithVector(R, Hg_old)
            'now with LHS*x = RHS, using Gauss Elimination we can get the resolution for the new field of humidity Hnew
            Hnew = getX(LHS, RHS)
            'data stockage
            For j = 0 To NNodes - 1
                Hm(ti, j) = Hnew(j)
            Next
        Next

    End Sub

    'Getting the LHS matrix for Gauss matrix resolution
    Private Function getLHS(ByRef A(,) As Double, ByRef b(,) As Double, dt As Double)
        Dim LHS(NNodes - 1, NNodes - 1) As Double
        Dim i, j As Integer
        For i = 0 To NNodes - 1
            For j = 0 To NNodes - 1
                LHS(i, j) = A(i, j) / 2 + b(i, j) / dt
            Next
        Next
        Return LHS
    End Function
    'Getting the RHS matrix for Gauss matrix resolution
    Private Function getRHS(ByRef A(,) As Double, ByRef b(,) As Double, dt As Double)
        Dim RHS(NNodes - 1, NNodes - 1) As Double
        Dim i, j As Integer
        For i = 0 To NNodes - 1
            For j = 0 To NNodes - 1
                RHS(i, j) = b(i, j) / dt - A(i, j) / 2
            Next
        Next
        Return RHS
    End Function


    'Get degree of freedom /water diffusion
    Private Function getDOF(NodeNo As Integer) As Integer
        Dim nDofsPerNode As Integer = 1
        Return (NodeNo) * nDofsPerNode
    End Function

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
                dofj = dofs(j) - dofi
                If dofj >= 0 Then
                    Kg(dofi, dofj) = Kg(dofi, dofj) + ke(i, j)
                End If
            Next
        Next
    End Sub
    'Assembling global vector /water diffusion
    Private Sub AssembleVg(ByRef ve() As Double, ByRef Vg() As Double, ElementNo As Integer)
        Dim i As Integer
        Dim dofs() As Integer = {getDOF(Elements(ElementNo).Node1 - 1),
                                 getDOF(Elements(ElementNo).Node2 - 1),
                                 getDOF(Elements(ElementNo).Node3 - 1),
                                 getDOF(Elements(ElementNo).Node4 - 1)}
        Dim dofi As Integer
        For i = 0 To 3 'each dof of the ve
            dofi = dofs(i)
            Vg(dofi) = Vg(dofi) + ve(i)
        Next

    End Sub
    Private Function MultiplyMatrixWithVector(ByRef a(,) As Double, ByRef b() As Double) As Double()

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
    Private Function getX(ByRef A(,) As Double, ByRef b() As Double)
        Dim X() As Double
        Dim s As Integer
        Dim m As Integer
        Dim i, j As Integer
        Dim sum As Double
        s = A.Length

        For j = 0 To s - 2
            For i = s - 1 To j + 1
                m = A(i, j) / A(j, j)
                'A(i,) = A(i,) - m * A(j,)
                b(i) = b(i) - m * b(j)
            Next

        Next

        X(s) = b(s) / A(s, s)
        For i = s - 2 To 0
            sum = 0
            For j = s - 1 To i
                sum = sum + A(i, j) * X(j)
            Next
            X(i) = (b(i) - sum) / A(i, i)
        Next

        Return X
    End Function
End Class
