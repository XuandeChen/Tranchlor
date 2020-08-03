Option Explicit On
Imports System
Imports System.ComponentModel
Imports System.IO
Imports System.Linq

Public Class frmTrans2D
    'This program shows the complete implementation
    'of a finite element program to analyse water diffusion problems in 2D.
    '

    'Written by Xuande Chen, Thomas Sanchez, David Conciatori
    '20nd July 2020
    'Quebec, QC
    'Canada

    Private Para As Short

    Private NNodes, NElements, Nbloc, NPointLoads, NSupports As Integer
    Private ElasticityModulus, Thickness, PoissonRatio As Double

    Private Nodes() As NodeTrans
    Private Elements() As ElementTrans
    Private Time() As Double ' heure
    Private diffusion As DiffusionXC
    Private transport As TransportXC

    Private MeshFileOk As Boolean = False

    Private PointLoads() As PointLoad
    Private Supports() As Support
    Private Deformations() As Double 'the deformation vector
    Private Analysed As Boolean = False

    Private ShowDeformations As Boolean = True
    Private ShowModel As Boolean = True
    Private ShowNodeNumbers As Boolean = False
    Private ShowElementsOnDeformedShape As Boolean = True

    Private colorMap As ColorMap
    Private HRRange As Range
    Private SRange As Range

    Private Directory As String

    'Private EpsilonXRange, EpsilonYRange, GammaXYRange As Range
    Private Enum Results
        None
        HR
        SigmaY
        TauXY
        EpsilonX
        EpsilonY
        GammaXY
    End Enum
    Private ShowResult As Results = Results.None

    'Graphics
    Private zoom As Double = 1.0
    Private DeformationZoom As Double = 50.0
    Private gr As Graphics

    Private Sub frmbtFem_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        bmp = New Bitmap(pbModel.Width, pbModel.Height, pbModel.CreateGraphics)
        gr = Graphics.FromImage(bmp)
        TimeScrollBar.Visible = False
        LabelT1.Visible = False
        LabelTVal.Visible = False
        LabelProgress.Visible = False
    End Sub

    Private bmp As Bitmap

    Private Sub OpenMeshFile_Click(sender As Object, e As EventArgs) Handles OpenMeshFile.Click

        Dim d As New OpenFileDialog
        'd.Title = "Open FEM file"
        d.Title = "Open Mesh file"
        'd.Filter = "FEm Files (*.fem)|*.fem|All Files|*.*"
        d.Filter = "Mesh Files (*.msh)|*.msh"

        If d.ShowDialog = DialogResult.OK Then
            ClearAnalysisOutput()
            If ReadFile(d.FileName) = False Then
                MeshFileOk = False
                MsgBox("Error with Mesh file.", MsgBoxStyle.OkOnly And MsgBoxStyle.Information, "Mesh file")
                Return
            End If
            '        MsgBox("Mesh file imported successfully!", MsgBoxStyle.OkOnly And MsgBoxStyle.Information, "Mesh file")
            MeshFileOk = True
            Directory = Path.GetDirectoryName(d.FileName) ' Thomas : Ligne pour récuperer le chemin du fichier
            DrawModel()
        End If
    End Sub

    Private Sub ClearAnalysisOutput()
        Erase Deformations 'Clear the earlier analysis output
        TimeScrollBar.Visible = False
        LabelT1.Visible = False
        LabelTVal.Visible = False
        LabelProgress.Visible = False
        Analysed = False
    End Sub
    ' mechanical computations
    'Private Sub Analyse()
    '    'Calculate statistics
    '    Dim nDof As Integer = NNodes * 2


    '    'make global stiffness matrix
    '    'we create elemental stiffness matrix and place it in proper position
    '    'in the global stiffness matrix
    '    'saving as a square matrix would be too taxing on ram.
    '    'hence, we save the stiffness matrix in half band form

    '    Dim HalfBandWidth As Integer = getHalfBandWidth()
    '    Dim Kg(nDof - 1, HalfBandWidth - 1) As Double 'Global stiffness matrix

    '    Dim cst As CST
    '    Dim i As Integer
    '    For i = 0 To NElements - 1
    '        cst = New CST(Nodes(Elements(i).Node1 - 1).x, Nodes(Elements(i).Node1 - 1).y,
    '                      Nodes(Elements(i).Node2 - 1).x, Nodes(Elements(i).Node2 - 1).y,
    '                      Nodes(Elements(i).Node3 - 1).x, Nodes(Elements(i).Node3 - 1).y,
    '                      Thickness, ElasticityModulus, PoissonRatio)


    '        If testKe(cst.getKe) = False Then
    '            MsgBox("ke test failed")
    '        End If

    '        AssembleKg(cst.getKe, Kg, i)

    '    Next
    '    'now, we have assembled Kg in banded form

    '    'make the load vector
    '    Dim r(nDof - 1) As Double
    '    Dim dof As Integer 'dof
    '    For i = 0 To NPointLoads - 1
    '        dof = getDOFx(PointLoads(i).Node - 1)
    '        r(dof) += PointLoads(i).Fx

    '        dof = getDOFy(PointLoads(i).Node - 1)
    '        r(dof) += PointLoads(i).Fy

    '    Next


    '    'Apply boundary conditions by penalty approach
    '    Dim Large As Double
    '    Dim pow As Double = MaxKgiiPower(Kg)
    '    Large = 10 ^ (Math.Ceiling(pow) + 10)
    '    For i = 0 To NSupports - 1
    '        If Supports(i).RestraintX = 1 Then
    '            dof = getDOFx(Supports(i).Node - 1)
    '            Kg(dof, 0) = Kg(dof, 0) * Large
    '        End If
    '        If Supports(i).RestraintY = 1 Then
    '            dof = getDOFy(Supports(i).Node - 1)
    '            Kg(dof, 0) = Kg(dof, 0) * Large
    '        End If

    '    Next


    '    'Solve the set of equations to get displacements.

    '    'Deformations = SolveWithPivoting(Kg, r)


    '    Dim bs As New btGauss(Kg)
    '    bs.SolveSerial(r)
    '    Deformations = r

    '    'Now that we have deformations, lets calculate stresses and strains..
    '    SigmaXRange = New Range : SigmaYRange = New Range : TauXYRange = New Range
    '    EpsilonXRange = New Range : EpsilonYRange = New Range : GammaXYRange = New Range

    '    'first is to calculate strains
    '    'strain = [B] * d.
    '    'since the element is a constant strain triangle, the strain value will be 
    '    'constant for each element

    '    Dim B(2, 5) As Double
    '    Dim n1, n2, n3 As Integer
    '    Dim dofs(5) As Integer
    '    Dim disp(5) As Double
    '    Dim DMatrix(,) As Double
    '    Dim j As Integer
    '    For i = 0 To NElements - 1
    '        cst = New CST(Nodes(Elements(i).Node1 - 1).x, Nodes(Elements(i).Node1 - 1).y,
    '                      Nodes(Elements(i).Node2 - 1).x, Nodes(Elements(i).Node2 - 1).y,
    '                      Nodes(Elements(i).Node3 - 1).x, Nodes(Elements(i).Node3 - 1).y,
    '                      Thickness, ElasticityModulus, PoissonRatio)
    '        B = cst.getB

    '        'extract the connectivity nodes
    '        n1 = Elements(i).Node1 - 1
    '        n2 = Elements(i).Node2 - 1
    '        n3 = Elements(i).Node3 - 1

    '        'find the degree of freedoms associated
    '        dofs(0) = getDOFx(n1) : dofs(1) = getDOFy(n1)
    '        dofs(2) = getDOFx(n2) : dofs(3) = getDOFy(n2)
    '        dofs(4) = getDOFx(n3) : dofs(5) = getDOFy(n3)

    '        'extract the displacements
    '        For j = 0 To 5
    '            disp(j) = Deformations(dofs(j))
    '        Next

    '        'Calculate the strain vector
    '        'epsilon = {ex, ey, gamma xy}T
    '        Elements(i).Strains = MultiplyMatrixWithVector(B, disp)

    '        'Calculate stresses
    '        'Sigma = D * Epsilon
    '        DMatrix = cst.getD

    '        Elements(i).Stresses = MultiplyMatrixWithVector(DMatrix, Elements(i).Strains)
    '        SigmaXRange.AddValue(Elements(i).Stresses(0))
    '        SigmaYRange.AddValue(Elements(i).Stresses(1))
    '        TauXYRange.AddValue(Elements(i).Stresses(2))

    '        EpsilonXRange.AddValue(Elements(i).Strains(0))
    '        EpsilonYRange.AddValue(Elements(i).Strains(1))
    '        GammaXYRange.AddValue(Elements(i).Strains(2))

    '    Next

    '    'now, we have all results
    '    'enjoy! :o)

    'End Sub
    ' Program for computation 2D diffusion 
    Public Sub Analyse()
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
        Dim day As Double
        Dim D0 As Double = 0.00031  ' mm2/s
        Dim alpha_0 As Double = 0.05
        Dim Hc As Double = 0.75
        Dim w As Double = 1 'indicator for isotherm curve, judge if we choose to use desorption (w = 1) or adsorption curve (w = 0) 
        Dim alpha As Double = 0.6 'hydration degree (-)
        Dim Tk As Double = 293 'temperature in (K), attention, faudrait l'inserer dans le boucle parce que cela va varier en fonction de temps et espace, XC 2020.07.30
        Dim Tc As Double = Tk - 273 'temperature in (C)
        Dim wsat As Double = GetWsat(C, alpha, W_C_ratio, phi) 'saturated water mass (kg/m3)

        'Computation parameters
        Dim nDof As Integer = NNodes
        Dim H_int As Double = 0.999 'initial relative humidity
        Dim S_int As Double = GetHtoS(H_int, type, C, W_C_ratio, Tk, day, rho_l, rho_c, alpha, w) 'initial saturation field
        Dim H_bound As Double = 0.6 'boundary relative humidity
        Dim S_bound As Double = GetHtoS(H_bound, type, C, W_C_ratio, Tk, day, rho_l, rho_c, alpha, w) 'boundary saturation field
        Dim dt As Double = 3600 'time interval (s)
        Dim tmax As Double = 259200 'end time (s) 72h
        Dim ind As Double = tmax / dt
        Dim T(ind) As Double 'time vector (days)
        Dim H_mat(ind, NNodes - 1) As Double 'Matrix for stockage of computation results (days, number of nodes)
        Dim S_mat(ind, NNodes - 1) As Double 'Matrix for stockage of computation results (days, number of nodes)
        Dim H_old(NNodes - 1) As Double
        Dim H_new(NNodes - 1) As Double
        Dim S_old(NNodes - 1) As Double
        Dim S_new(NNodes - 1) As Double
        Dim jj As Long
        Dim nFic1 As Short
        Dim outfile(1) As String
        Dim T_sauv As Single = 14400 'ouput time inteval (s) 4h
        Dim i, j, k As Integer

        'step 0: Creating output .txt computation result file 2020-07-17 Xuande 
        outfile(1) = Directory & "\" & "R_H" & ".txt"
        nFic1 = CShort(FreeFile())
        FileOpen(CInt(nFic1), outfile(1), OpenMode.Output)

        'step 0 Initialize output titres for result .txt files
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

        HRRange = New Range

        For i = 0 To NElements - 1
            ReDim Elements(i).HR(ind + 1)
            Elements(i).HR(0) = H_int * 100
            HRRange.AddValue(Elements(i).HR(0))
            ReDim Time(ind + 1)
            Time(0) = 0
        Next

        Me.Invoke(Sub()
                      LabelProgress.Visible = True
                  End Sub)

        'Globlal time loop

        Dim ti As Integer

        For ti = 0 To ind

            Me.Invoke(Sub()
                          LabelProgress.Text = CStr(ti) + " / " + CStr(ind)
                          Me.Refresh()
                      End Sub)

            ' step 1: initialisation
            T(ti) = 0 + dt * (ti - 0)
            If ti = 0 Then
                For i = 0 To nDof - 1
                    H_old(i) = H_int
                Next
            Else
                H_old = H_new
            End If

            'step 2: check boundary conditions on each node then construct elemental humidity vector / à reviser pour calcul d'une structure complet Xuande.2020.07.27
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
                De = GetDh(D0, alpha_0, Hc, Tc, H_avg)
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
                H_mat(ti, j) = H_new(j)
            Next

            'step 7: plot 2D image and export result .txt file 
            For i = 0 To NElements - 1

                Elements(i).HR(ti + 1) = (H_new(Elements(i).Node1 - 1) + H_new(Elements(i).Node2 - 1) + H_new(Elements(i).Node3 - 1) + H_new(Elements(i).Node4 - 1)) * 100 / 4
                HRRange.AddValue(Elements(i).HR(ti + 1))

            Next

            Time(ti + 1) = (ti + 1) * dt / 3600 ' Time in hour

            If (ti * dt / T_sauv) = Int(ti * dt / T_sauv) Then ' check register time
                RegisterH(nFic1, ti * dt, nDof, H_new)
                PrintLine(CInt(nFic1), " ")
            End If
        Next

        FileClose(CInt(nFic1))

        MsgBox("Fin du calcul diffusion 2D", MsgBoxStyle.OkOnly And MsgBoxStyle.Information, "End")
        Analysed = True

        Me.Invoke(Sub()
                      LabelProgress.Visible = False
                  End Sub)

    End Sub
    Public Sub AnalyseTransport()
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
        Dim day As Double
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
        Dim H_mat(ind, NNodes - 1) As Double 'Matrix for stockage of computation results (days, number of nodes)
        Dim S_mat(ind, NNodes - 1) As Double 'Matrix for stockage of computation results (days, number of nodes)
        Dim H_old(NNodes - 1) As Double
        Dim H_new(NNodes - 1) As Double
        Dim S_old(NNodes - 1) As Double
        Dim S_new(NNodes - 1) As Double
        Dim jj As Long
        Dim nFic1 As Short
        Dim outfile(1) As String
        Dim T_sauv As Single = 14400 'ouput time inteval (s) 4h
        Dim i, j, k As Integer

        'step 0: Creating output .txt computation result file 2020-07-27 Xuande 
        outfile(1) = Directory & "\" & "R_S" & ".txt"
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

        SRange = New Range

        For i = 0 To NElements - 1
            ReDim Elements(i).S(ind + 1)
            Elements(i).S(0) = S_int * 100
            SRange.AddValue(Elements(i).S(0))
            ReDim Time(ind + 1)
            Time(0) = 0
        Next
        'Globlal time loop
        Dim ti As Integer

        For ti = 0 To ind

            Me.Invoke(Sub()
                          LabelProgress.Text = CStr(ti) + " / " + CStr(ind)
                          Me.Refresh()
                      End Sub)

            ' step 1: initialisation saturation field
            T(ti) = 0 + dt * (ti - 0)
            If ti = 0 Then
                For i = 0 To nDof - 1
                    H_old(i) = H_int
                    S_old(i) = S_int
                Next
            Else
                H_old = H_new
                S_old = S_new
            End If
            'step 2: check boundary conditions on each noeuds then construct elemental humidity vector / à reviser pour calcul d'une structure complet Xuande.2020.07.20
            Dim ie As Integer
            day = CDbl(ti * dt / 3600 / 24)
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
            Dim S_ele() As Double
            Dim De As Double

            'Matrix assembling
            For i = 0 To NElements - 1
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
                De = (Dv + Dl) / phi
                cie = New CIETrans(
                          Nodes(Elements(i).Node1 - 1).x, Nodes(Elements(i).Node1 - 1).y,
                          Nodes(Elements(i).Node2 - 1).x, Nodes(Elements(i).Node2 - 1).y,
                          Nodes(Elements(i).Node3 - 1).x, Nodes(Elements(i).Node3 - 1).y,
                          Nodes(Elements(i).Node4 - 1).x, Nodes(Elements(i).Node4 - 1).y,
                          De)
                AssembleKg(cie.getbe, bg, i)
                AssembleKg(cie.getAe, Ag, i)
            Next

            'step 4: now, we have assembled Sg_old, Ag and bg , to get LHS and RHS
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
                If S_new(j) >= 1 Then
                    S_new(j) = 1
                ElseIf S_new(j) <= 0 Then
                    S_new(j) = 0
                End If
                S_mat(ti, j) = S_new(j)
            Next

            'step 7: plot 2D image and export result .txt file 
            For i = 0 To NElements - 1

                Elements(i).S(ti + 1) = (S_new(Elements(i).Node1 - 1) + S_new(Elements(i).Node2 - 1) + S_new(Elements(i).Node3 - 1) + S_new(Elements(i).Node4 - 1)) * 100 / 4
                SRange.AddValue(Elements(i).S(ti + 1))

            Next

            Time(ti + 1) = (ti + 1) * dt / 3600 ' Time in hour

            If (ti * dt / T_sauv) = Int(ti * dt / T_sauv) Then ' check register time 
                RegisterS(nFic1, ti * dt, nDof, S_new)
                PrintLine(CInt(nFic1), " ")
            End If
        Next
        FileClose(CInt(nFic1))

        MsgBox("Fin du calcul transport 2D", MsgBoxStyle.OkOnly And MsgBoxStyle.Information, "End")
        Analysed = True

        Me.Invoke(Sub()
                      LabelProgress.Visible = False
                  End Sub)

    End Sub

    Private Sub RegisterH(ByRef nFic1 As Short, ByRef Temps As Double, ByRef Dofs As Integer, ByRef H_new() As Double)
        'Register values
        Print(CInt(nFic1), Temps / 3600, ",", Temps, ",", TAB)
        For j As Integer = 0 To Dofs - 1
            Print(CInt(nFic1), H_new(j), ",", TAB) '% humidité relative dans le béton
        Next j
    End Sub
    Private Sub RegisterS(ByRef nFic1 As Short, ByRef Temps As Decimal, ByRef Dofs As Short, ByRef S_new() As Double)
        'Register values
        Print(CInt(nFic1), Temps / 3600, ",", Temps, ",", TAB)
        For j As Integer = 0 To Dofs - 1
            Print(CInt(nFic1), S_new(j), ",", TAB)
        Next j
    End Sub
    'Private Function MaxKgiiPower(ByRef Kg(,) As Double) As Double
    '    Dim Max As Double = Double.MinValue
    '    Dim i As Integer
    '    For i = 0 To Kg.GetLength(0) - 1
    '        If Kg(i, 0) > Max Then Max = Kg(i, 0)
    '    Next
    '    Dim p As Double = Math.Log10(Max)
    '    Return p
    'End Function

    ''' <summary>
    ''' Multiplies matrix a by vector b and returns the product
    ''' </summary>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <returns></returns>
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

    'Private Function testKe(ByRef Ke(,) As Double) As Boolean
    '    Dim i, j As Integer
    '    For i = 0 To 5
    '        If Ke(i, i) < 0.0000001 Then Return False
    '    Next
    '    For i = 0 To 5
    '        For j = 0 To 5
    '            If Math.Abs(Ke(i, j) - Ke(j, i)) > 0.00001 Then Return False
    '        Next
    '    Next

    '    Return True
    'End Function




    'Private Sub AssembleKg(ByRef Ke(,) As Double, ByRef Kg(,) As Double, ElementNo As Integer)
    '    Dim i, j As Integer
    '    Dim dofs() As Integer = {getDOFx(Elements(ElementNo).Node1 - 1),
    '                             getDOFy(Elements(ElementNo).Node1 - 1),
    '                             getDOFx(Elements(ElementNo).Node2 - 1),
    '                             getDOFy(Elements(ElementNo).Node2 - 1),
    '                             getDOFx(Elements(ElementNo).Node3 - 1),
    '                             getDOFy(Elements(ElementNo).Node3 - 1)}

    '    'Place the upper triangle of the elemental stiffness matrix in the global
    '    'matrix in proper position
    '    Dim dofi, dofj As Integer
    '    For i = 0 To 5 'each dof of the ke
    '        dofi = dofs(i)
    '        For j = 0 To 5
    '            dofj = dofs(j) - dofi
    '            If dofj >= 0 Then
    '                Kg(dofi, dofj) = Kg(dofi, dofj) + Ke(i, j)
    '            End If
    '        Next
    '    Next

    'End Sub
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

    'Private Function getDOFx(NodeNo As Integer) As Integer
    '    Dim nDofsPerNode As Integer = 2
    '    Return (NodeNo) * nDofsPerNode
    'End Function
    'Private Function getDOFy(NodeNo As Integer) As Integer
    '    Dim nDofsPerNode As Integer = 2
    '    Return NodeNo * nDofsPerNode + 1
    'End Function

    'Private Function getHalfBandWidth() As Integer
    '    Dim i As Integer
    '    Dim n(2) As Integer
    '    Dim MaxDiff As Integer = 0
    '    Dim diff As Integer
    '    For i = 0 To NElements - 1
    '        n(0) = Elements(i).Node1
    '        n(1) = Elements(i).Node2
    '        n(2) = Elements(i).Node3
    '        diff = n.Max - n.Min
    '        If MaxDiff < diff Then MaxDiff = diff
    '    Next

    '    'now we have maxdiff
    '    'half band width is maxdiff * 2. 2 because there are 2 dofs per node

    '    Dim hbw As Integer = (MaxDiff + 1) * 2
    '    If hbw > NNodes * 2 Then hbw = NNodes * 2

    '    Return hbw
    'End Function

    Public Function ReadFile(f As String) As Boolean
        Try

            Dim sr As New StreamReader(f)
            Dim s As String
            Dim Arr(0) As String
            Dim Temp As String
            Dim i As Integer
            Dim j As Integer
            Dim jj As Integer
            Dim k As Integer
            Dim NN(0) As Integer
            Dim XX(0), YY(0), ZZ(0) As Double
            Dim n0 As Integer
            Dim n, n1, n2, n3, n4 As Integer
            Dim x, y, z As Double
            Dim brd As Boolean
            Dim bloc_node As Integer
            Dim bloc_type As Integer
            Dim bloc_element As Integer

            'Skip the version data
            Temp = readLine(sr)

            'Skip the unnecessary first few lines
            Do While Temp <> "$Nodes"
                Temp = sr.ReadLine.Trim
            Loop

            'Read total number of nodes and blocks 
            s = readLine(sr)
            Arr = Split(s, " "c)
            Try
                Nbloc = Integer.Parse(Arr(0))
                NNodes = Integer.Parse(Arr(1))
            Catch ex As Exception
                MsgBox(ex.Message)
                Return False
            End Try

            'Read node coordinates block by block
            ReDim Nodes(NNodes - 1)
            For i = 0 To Nbloc - 1
                'read bloc information for how many nodes should be read inside
                s = readLine(sr)
                Arr = Split(s, " "c)
                bloc_type = Integer.Parse(Arr(0))
                bloc_node = Integer.Parse(Arr(3))
                ReDim NN(bloc_node - 1)
                ReDim XX(bloc_node - 1)
                ReDim YY(bloc_node - 1)
                ReDim ZZ(bloc_node - 1)
                For j = 0 To bloc_node - 1
                    'read first lines of node number
                    s = readLine(sr)
                    Arr = Split(s, " "c)
                    n = Integer.Parse(Arr(0))
                    NN(j) = n
                Next
                'read corresponding lines of coordinates
                For k = 0 To bloc_node - 1
                    'judging wether points of current block belongs to the boundary
                    If bloc_type <= 1 Then
                        brd = True
                    Else
                        brd = False
                    End If
                    'read corresponding lines of coordinates
                    s = readLine(sr)
                    Arr = Split(s, " "c)
                    x = CDbl(Arr(0))
                    XX(k) = x
                    y = CDbl(Arr(1))
                    YY(k) = y
                    z = CDbl(Arr(2))
                    ZZ(k) = z
                    Nodes(NN(k) - 1) = New NodeTrans(NN(k), XX(k), YY(k), ZZ(k), brd)
                Next
            Next

            s = readLine(sr)
            s = readLine(sr)

            'Read total number of elements and blocks 
            s = readLine(sr)
            Arr = Split(s, " "c)
            Try
                Nbloc = Integer.Parse(Arr(0))
                NElements = Integer.Parse(Arr(1))
            Catch ex As Exception
                MsgBox(ex.Message)
                Return False
            End Try

            'Read element connectivity block by block
            For i = 0 To Nbloc - 1
                'read bloc information for how many nodes should be read inside
                s = readLine(sr)
                Arr = Split(s, " "c)
                bloc_type = Integer.Parse(Arr(0))
                bloc_element = Integer.Parse(Arr(3))
                If bloc_type = 0 Then
                    Temp = readLine(sr)
                End If

                If bloc_type = 1 Then
                    For j = 0 To bloc_element - 1
                        Temp = readLine(sr)
                        Arr = Split(Temp, " "c)
                        n0 = Integer.Parse(Arr(0))
                    Next
                End If

                If bloc_type = 2 Then
                    NElements = bloc_element
                    ReDim Elements(NElements - 1)
                    For jj = 0 To bloc_element - 1
                        s = readLine(sr)
                        Arr = Split(s, " "c)
                        n = Integer.Parse(Arr(0)) - n0
                        n1 = Integer.Parse(Arr(1))
                        n2 = Integer.Parse(Arr(2))
                        n3 = Integer.Parse(Arr(3))
                        n4 = Integer.Parse(Arr(4))
                        Elements(jj) = New ElementTrans(n, n1, n2, n3, n4)
                    Next
                End If

            Next
        Catch ex As Exception
            MsgBox(ex.Message)
            Return False
        End Try
        Return True
    End Function

    'Private Function ReadFile(f As String) As Boolean
    '    Try
    '        Dim sr As New StreamReader(f)
    '        Dim s As String
    '        Dim Arr() As String
    '        s = readLine(sr)
    '        Arr = Split(s, ","c)

    '        Try
    '            NNodes = Integer.Parse(Arr(0))
    '            NElements = Integer.Parse(Arr(1))
    '            NPointLoads = Integer.Parse(Arr(2))
    '            NSupports = Integer.Parse(Arr(3))

    '            ElasticityModulus = Double.Parse(Arr(4))
    '            PoissonRatio = Double.Parse(Arr(5))
    '            Thickness = Double.Parse(Arr(6))
    '        Catch ex As Exception
    '            MsgBox(ex.Message)
    '            Return False
    '        End Try

    '        'got the basics.. now, lets read each entity
    '        ReDim Nodes(NNodes - 1)
    '        ReDim Elements(NElements - 1)

    '        Dim i As Integer
    '        Dim n, n1, n2, n3 As Integer
    '        Dim x, y As Double
    '        For i = 0 To NNodes - 1
    '            s = readLine(sr)
    '            s = s.Replace(vbTab, "")

    '            Try
    '                Arr = Split(s, ","c)
    '                n = Integer.Parse(Arr(0))
    '                x = Double.Parse(Arr(1))
    '                y = Double.Parse(Arr(2))
    '                Nodes(i) = New Node(n, x, y)
    '            Catch ex As Exception
    '                MsgBox(ex.Message)
    '                Return False
    '            End Try
    '        Next

    '        'read elements
    '        For i = 0 To NElements - 1
    '            s = readLine(sr)
    '            Try
    '                Arr = Split(s, ","c)
    '                n = Integer.Parse(Arr(0))
    '                n1 = Integer.Parse(Arr(1))
    '                n2 = Integer.Parse(Arr(2))
    '                n3 = Integer.Parse(Arr(3))

    '                Elements(i) = New Element(n, n1, n2, n3)

    '            Catch ex As Exception
    '                MsgBox(ex.Message)
    '                Return False
    '            End Try
    '        Next

    '        'Read Point Loads
    '        ReDim PointLoads(NPointLoads - 1)
    '        Dim Fx, Fy As Double
    '        Dim ln As Integer
    '        For i = 0 To NPointLoads - 1
    '            s = readLine(sr)
    '            Try
    '                Arr = Split(s, ","c)
    '                ln = Integer.Parse(Arr(0))
    '                n = Integer.Parse(Arr(1))
    '                Fx = Double.Parse(Arr(2))
    '                Fy = Double.Parse(Arr(3))

    '                PointLoads(i) = New PointLoad(ln, n, Fx, Fy)

    '            Catch ex As Exception
    '                MsgBox(ex.Message)
    '                Return False
    '            End Try
    '        Next

    '        'Read supports
    '        ReDim Supports(NSupports - 1)
    '        Dim sn, Rx, Ry As Integer
    '        For i = 0 To NSupports - 1
    '            s = readLine(sr)
    '            Try
    '                Arr = Split(s, ","c)
    '                sn = Integer.Parse(Arr(0))
    '                n = Integer.Parse(Arr(1))
    '                Rx = Integer.Parse(Arr(2))
    '                Ry = Integer.Parse(Arr(3))

    '                Supports(i) = New Support(sn, n, Rx, Ry)

    '            Catch ex As Exception
    '                MsgBox(ex.Message)
    '                Return False
    '            End Try
    '        Next


    '    Catch ex As Exception
    '        MsgBox(ex.Message)
    '        Return False
    '    End Try
    '    Return True
    'End Function
    Private Sub btnResetZoom_Click(sender As Object, e As EventArgs) Handles btnResetZoom.Click
        zoom = 1
        hs.Value = 0
        vs.Value = 0
        DrawModel()
    End Sub

    Private Sub vs_Scroll(sender As Object, e As ScrollEventArgs) Handles vs.Scroll
        DrawModel()
    End Sub

    Private Sub hs_Scroll(sender As Object, e As ScrollEventArgs) Handles hs.Scroll
        DrawModel()
    End Sub

    Private Function readLine(ByRef sr As StreamReader) As String
        Dim s As String
        If sr.EndOfStream = True Then Return ""
        While sr.EndOfStream = False
            s = sr.ReadLine.Trim
            If s.Length > 0 Then
                If s.Substring(0, 1) <> "%" Then
                    Return s
                End If
            End If
        End While
        Return ""
    End Function

    Private Sub DrawModel()
        gr.Clear(Color.White)

        Dim eleColor As Color = Color.Lavender
        Dim eleColorDeformed As Color = Color.Turquoise
        Dim supportColor As Color = Color.Black
        Dim pointLoadColor As Color = Color.Red

        Dim MarginX, MarginY As Integer
        MarginX = 50
        MarginY = 50

        'Draw the triangles.
        Dim shiftx As Integer = MarginX - hs.Value
        Dim shifty As Integer = pbModel.Height - MarginY - vs.Value

        Dim i As Integer
        Dim n1, n2, n3, n4 As Integer

        Dim ptsf(3) As PointF


        'Draw the undeformed model

        If ShowModel = True Then
            'Draw the elements .
            For i = 0 To NElements - 1

                n1 = Elements(i).Node1 - 1
                n2 = Elements(i).Node2 - 1
                n3 = Elements(i).Node3 - 1

                n4 = Elements(i).Node4 - 1

                ptsf(0) = New PointF(CSng(Nodes(n1).x * zoom + shiftx), CSng(-Nodes(n1).y * zoom + shifty))
                ptsf(1) = New PointF(CSng(Nodes(n2).x * zoom + shiftx), CSng(-Nodes(n2).y * zoom + shifty))
                ptsf(2) = New PointF(CSng(Nodes(n3).x * zoom + shiftx), CSng(-Nodes(n3).y * zoom + shifty))

                ptsf(3) = New PointF(CSng(Nodes(n4).x * zoom + shiftx), CSng(-Nodes(n4).y * zoom + shifty))


                'Draw the element
                gr.FillPolygon(New SolidBrush(eleColor), ptsf)
                gr.DrawPolygon(New Pen(Color.Black), ptsf)

            Next

            'Draw the node numbers
            If ShowNodeNumbers = True Then
                Dim p As New Pen(Color.Black)
                Dim nx, ny As Integer
                Dim fnt As New Font("Arial", 8)
                Dim brsh As New SolidBrush(Color.Red)
                For i = 0 To NNodes - 1
                    nx = CInt(Nodes(i).x * zoom + shiftx)
                    ny = CInt(-Nodes(i).y * zoom + shifty)
                    Dim r As New Rectangle(nx - 3, ny - 3, 6, 6)
                    gr.DrawEllipse(p, r)

                    gr.DrawString((i + 1).ToString, fnt, brsh, New PointF(nx + 5, ny + 5))

                Next
            End If

            'Draw the supports
            'we draw squares for supports
            ReDim ptsf(3)
            Dim squareHalfSize As Integer = 5
            For i = 0 To NSupports - 1
                n1 = Supports(i).Node - 1
                ptsf(0) = New PointF(CSng(Nodes(n1).x * zoom + shiftx - squareHalfSize),
                                     CSng(-Nodes(n1).y * zoom + shifty - squareHalfSize))
                ptsf(1) = New PointF(CSng(Nodes(n1).x * zoom + shiftx + squareHalfSize),
                                     CSng(-Nodes(n1).y * zoom + shifty - squareHalfSize))
                ptsf(2) = New PointF(CSng(Nodes(n1).x * zoom + shiftx + squareHalfSize),
                                     CSng(-Nodes(n1).y * zoom + shifty + squareHalfSize))
                ptsf(3) = New PointF(CSng(Nodes(n1).x * zoom + shiftx - squareHalfSize),
                                     CSng(-Nodes(n1).y * zoom + shifty + squareHalfSize))
                gr.FillPolygon(New SolidBrush(supportColor), ptsf)

            Next

            'draw the loads
            Dim px, py As Integer
            For i = 0 To NPointLoads - 1
                n1 = PointLoads(i).Node - 1
                If Math.Abs(PointLoads(i).Fx) > 0.0001 Then
                    'if the pointload is significant
                    'draw the arrow
                    px = CInt(Nodes(n1).x * zoom + shiftx)
                    py = CInt(-Nodes(n1).y * zoom + shifty)
                    If PointLoads(i).Fx > 0 Then
                        DrawHArrow(px, py, pointLoadColor, False)
                    Else
                        DrawHArrow(px, py, pointLoadColor, True)
                    End If
                End If

                If Math.Abs(PointLoads(i).Fy) > 0.0001 Then
                    px = CInt(Nodes(n1).x * zoom + shiftx)
                    py = CInt(-Nodes(n1).y * zoom + shifty)
                    If PointLoads(i).Fy > 0 Then
                        DrawVArrow(px, py, pointLoadColor, False)
                    Else
                        DrawVArrow(px, py, pointLoadColor, True)
                    End If

                End If


            Next
        End If
        'Now, plot results if available
        'If Deformations IsNot Nothing AndAlso Deformations.Length > 0 Then

        If Analysed = True Then

            Dim eColor As Color
            'we can plot now.
            If ShowDeformations = True Then
                ReDim ptsf(3)

                'compute the colormap

                'set the colormap if required
                Select Case ShowResult
                    Case Results.None
                        'create a dummy colormap
                        colorMap = New ColorMap(1, 0)
                    Case Results.HR
                        colorMap = New ColorMap(Math.Round(HRRange.Max, 2), Math.Round(HRRange.Min, 2))
                        'TimeScrollBar
                        TimeScrollBar.Maximum = Time.Length()
                        TimeScrollBar.Visible = True
                        LabelT1.Visible = True
                        LabelTVal.Visible = True
                        'Case Results.SigmaY
                        'colorMap = New ColorMap(HRRange.Max, HRRange.Min)
                        'Case Results.TauXY
                        'colorMap = New ColorMap(TauXYRange.Max, TauXYRange.Min)
                        'Case Results.EpsilonX
                        'colorMap = New ColorMap(EpsilonXRange.Max, EpsilonXRange.Min)
                        'Case Results.EpsilonY
                        'colorMap = New ColorMap(EpsilonYRange.Max, EpsilonYRange.Min)
                        'Case Results.GammaXY
                        'colorMap = New ColorMap(GammaXYRange.Max, GammaXYRange.Min)
                    Case Else
                        'just for completeness.. otherwise this block wont ever execute
                        'create a dummy colormap
                        colorMap = New ColorMap(1, 0)
                End Select

                For i = 0 To NElements - 1

                    'ecolor depends on the value of result to be shown

                    Select Case ShowResult
                        Case Results.None
                            eColor = eleColorDeformed
                        Case Results.HR
                            'eColor = colorMap.getColor(Elements(i).Stresses(0))

                            eColor = colorMap.getColor(Elements(i).HR(TimeScrollBar.Value))
                            LabelTVal.Text = CStr(Time(TimeScrollBar.Value))
                            'Case Results.SigmaY
                            'eColor = colorMap.getColor(Elements(i).Stresses(1))
                            'eColor = colorMap.getColor(Elements(i).HR(1))
                            'Case Results.TauXY
                            'eColor = colorMap.getColor(Elements(i).Stresses(2))
                            'Case Results.EpsilonX
                            'eColor = colorMap.getColor(Elements(i).Strains(0))
                            'Case Results.EpsilonY
                            'eColor = colorMap.getColor(Elements(i).Strains(1))
                            'Case Results.GammaXY
                            'eColor = colorMap.getColor(Elements(i).Strains(2))
                        Case Else
                            eColor = eleColor
                    End Select


                    n1 = Elements(i).Node1 - 1
                    n2 = Elements(i).Node2 - 1
                    n3 = Elements(i).Node3 - 1

                    n4 = Elements(i).Node4 - 1

                    'ptsf(0) = New PointF((CSng(Nodes(n1).x * zoom + getDeformationX(n1) * DeformationZoom) + shiftx), CSng((-Nodes(n1).y * zoom - getDeformationY(n1) * DeformationZoom) + shifty))
                    'ptsf(1) = New PointF((CSng(Nodes(n2).x * zoom + getDeformationX(n2) * DeformationZoom) + shiftx), CSng((-Nodes(n2).y * zoom - getDeformationY(n2) * DeformationZoom) + shifty))
                    'ptsf(2) = New PointF((CSng(Nodes(n3).x * zoom + getDeformationX(n3) * DeformationZoom) + shiftx), CSng((-Nodes(n3).y * zoom - getDeformationY(n3) * DeformationZoom) + shifty))
                    'ptsf(3) = New PointF((CSng(Nodes(n4).x * zoom + getDeformationX(n4) * DeformationZoom) + shiftx), CSng((-Nodes(n4).y * zoom - getDeformationY(n4) * DeformationZoom) + shifty))

                    ptsf(0) = New PointF(CSng(Nodes(n1).x * zoom + shiftx), CSng(-Nodes(n1).y * zoom + shifty))
                    ptsf(1) = New PointF(CSng(Nodes(n2).x * zoom + shiftx), CSng(-Nodes(n2).y * zoom + shifty))
                    ptsf(2) = New PointF(CSng(Nodes(n3).x * zoom + shiftx), CSng(-Nodes(n3).y * zoom + shifty))
                    ptsf(3) = New PointF(CSng(Nodes(n4).x * zoom + shiftx), CSng(-Nodes(n4).y * zoom + shifty))

                    'Draw the element
                    gr.FillPolygon(New SolidBrush(eColor), ptsf)
                    If ShowElementsOnDeformedShape = True Then
                        gr.DrawPolygon(New Pen(Color.Black), ptsf)
                    End If

                Next

                'Draw Colormap at right top of the picturebox
                'only if required
                If ShowResult <> Results.None Then
                    Dim cmap As New ColorMap(250, 0)
                    Dim c As Color
                    Dim x1, x2, y As Integer
                    y = 50
                    x1 = bmp.Width - 100
                    x2 = x1 + 50
                    For i = 250 To 0 Step -1
                        c = cmap.getColor(i)
                        y += 1
                        gr.DrawLine(New Pen(c), New Point(x1, y), New Point(x2, y))
                    Next
                    'write the max, min and mid values
                    'max

                    Dim strWidth As Integer
                    Dim MaxWidth, MinWidth, AvgWidth As Single
                    Dim Max, Min, Avg As String
                    Dim barFont As New Font("Arial", 9)
                    Dim barBrush As New SolidBrush(Color.Black)
                    Max = colorMap.Max.ToString
                    Min = colorMap.Min.ToString
                    Avg = ((colorMap.Max + colorMap.Min) / 2).ToString
                    MaxWidth = gr.MeasureString(Max, barFont).Width
                    MinWidth = gr.MeasureString(Min, barFont).Width
                    AvgWidth = gr.MeasureString(Avg, barFont).Width

                    strWidth = CInt(Math.Max(Math.Max(MaxWidth, MinWidth), AvgWidth)) + 10

                    gr.DrawLine(Pens.Black, x1, 50, x1 - 5, 50)
                    gr.DrawString(Max, barFont, barBrush, New PointF(x1 - strWidth, 30))

                    gr.DrawLine(Pens.Black, x1, 175, x1 - 5, 175)
                    gr.DrawString(Avg, barFont, barBrush, New PointF(x1 - strWidth, 155))

                    gr.DrawLine(Pens.Black, x1, 300, x1 - 5, 300)
                    gr.DrawString(Min, barFont, barBrush, New PointF(x1 - strWidth, 320))
                End If


            End If
        End If
        pbModel.Refresh()
    End Sub

    'Private Function getDeformationX(Node As Integer) As Double
    '    Dim dof As Integer = getDOFx(Node)
    '    Return Deformations(dof)
    'End Function
    'Private Function getDeformationY(Node As Integer) As Double
    '    Dim dof As Integer = getDOFy(Node)
    '    Return Deformations(dof)
    'End Function

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs)
        If MsgBox("Are you sure you want to exit?", MsgBoxStyle.OkCancel) = MsgBoxResult.Ok Then
            End
        End If
    End Sub

    Private Sub DrawHArrow(px As Integer, py As Integer, c As Color, leftward As Boolean)
        Dim p As New Pen(c)
        Dim size As Integer = 15
        If leftward = False Then
            'rightward arrow
            gr.DrawLine(p, New Point(px, py), New Point(px - size, py))
            gr.DrawLine(p, New Point(px, py), New Point(CInt(px - size / 4), CInt(py - size / 4)))
            gr.DrawLine(p, New Point(px, py), New Point(CInt(px - size / 4), CInt(py + size / 4)))
        Else
            'leftward arrow
            gr.DrawLine(p, New Point(px, py), New Point(px + size, py))
            gr.DrawLine(p, New Point(px, py), New Point(CInt(px + size / 4), CInt(py - size / 4)))
            gr.DrawLine(p, New Point(px, py), New Point(CInt(px + size / 4), CInt(py + size / 4)))
        End If
    End Sub

    Private Sub AnalyseToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AnalyseToolStripMenuItem.Click

    End Sub

    Private Sub SetDeformationZoomToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SetDeformationZoomToolStripMenuItem.Click
        Dim ret As String = DeformationZoom.ToString

        ret = InputBox("Set Deformation Zoom:", "Deformation Zoom", ret)
        If ret.Length = 0 Then Return
        While IsNumeric(ret) = False
            ret = InputBox("Set Deformation Zoom:", "Deformation Zoom", ret)
            If ret.Length = 0 Then Return
        End While
        DeformationZoom = Double.Parse(ret)
        DrawModel()
    End Sub

    Private Sub ShowNodeNumbersToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ShowNodeNumbersToolStripMenuItem.Click
        ShowNodeNumbers = Not ShowNodeNumbers
        DrawModel()
    End Sub

    'Private Sub ResultsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ResultsToolStripMenuItem.Click
    '    Dim d As New dlgResults
    '    Dim sb As New System.Text.StringBuilder
    '    Dim s As String
    '    Dim dof As Integer
    '    Dim i As Integer


    '    sb.AppendLine("Model Statistics:")
    '    sb.AppendLine("Number of Nodes: " & NNodes.ToString)
    '    sb.AppendLine("Number of Elements: " & NElements.ToString)
    '    sb.AppendLine("Number of Variables: " & (NNodes * 2).ToString)
    '    sb.AppendLine("")

    '    'Deformations
    '    sb.AppendLine("Deformations:")
    '    sb.AppendLine("Node" & vbTab & vbTab & "Ux" & vbTab & vbTab & "Uy")
    '    For i = 0 To NNodes - 1
    '        s = (i + 1).ToString & vbTab & vbTab
    '        dof = getDOFx(i)
    '        s = s & Deformations(dof).ToString & vbTab & vbTab
    '        s = s & Deformations(dof + 1).ToString

    '        sb.AppendLine(s)
    '    Next

    '    'Element Stresses and Strains
    '    sb.AppendLine("Stresses and Strains:")
    '    sb.AppendLine("Element" & vbTab & vbTab & "sx" & vbTab & vbTab & "sy" & vbTab & vbTab & "txy" &
    '    vbTab & vbTab & "ex" & vbTab & vbTab & "ey" & vbTab & vbTab & "gamma_xy")
    '    For i = 0 To NElements - 1
    '        s = (i + 1).ToString & vbTab & vbTab

    '        s = s & Elements(i).HR(0).ToString & vbTab & vbTab
    '        s = s & Elements(i).HR(1).ToString & vbTab & vbTab
    '        s = s & Elements(i).HR(2).ToString & vbTab & vbTab

    '        's = s & Elements(i).Strains(0).ToString & vbTab & vbTab
    '        's = s & Elements(i).Strains(1).ToString & vbTab & vbTab
    '        's = s & Elements(i).Strains(2).ToString

    '        sb.AppendLine(s)
    '    Next

    '    'Write maximum displacements
    '    Dim uxMax As Double = Double.MinValue
    '    Dim uyMax As Double = Double.MinValue
    '    Dim uxMin As Double = Double.MaxValue
    '    Dim uyMin As Double = Double.MaxValue

    '    For i = 0 To Deformations.Length - 1 Step 2
    '        If uxMax < Deformations(i) Then uxMax = Deformations(i)
    '        If uxMin > Deformations(i) Then uxMin = Deformations(i)
    '        If uyMax < Deformations(i + 1) Then uyMax = Deformations(i + 1)
    '        If uyMin > Deformations(i + 1) Then uyMin = Deformations(i + 1)
    '    Next

    '    sb.AppendLine("")
    '    sb.AppendLine("Maximum Displacement in X direction = " & uxMax.ToString)
    '    sb.AppendLine("Minimum Displacement in X direction = " & uxMin.ToString)
    '    sb.AppendLine("Maximum Displacement in Y direction = " & uyMax.ToString)
    '    sb.AppendLine("Minimum Displacement in Y direction = " & uyMin.ToString)

    '    sb.AppendLine("")
    '    sb.AppendLine("Output generated by btFEM at " & DateTime.Now.ToLongDateString)


    '    d.txtResults.Text = sb.ToString


    '    d.ShowDialog()

    'End Sub

    Private Sub ShowModelToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ShowModelToolStripMenuItem.Click
        ShowModel = Not ShowModel
        DrawModel()
    End Sub

    Private Sub ShowElementsOnDeformedShapeToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ShowElementsOnDeformedShapeToolStripMenuItem.Click
        ShowElementsOnDeformedShape = Not ShowElementsOnDeformedShape
        DrawModel()
    End Sub

    Private Sub SaveImageAsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SaveImageAsToolStripMenuItem.Click
        Dim d As New SaveFileDialog
        d.Title = "Save Image As"
        d.Filter = "PNG Files|*.png|JPEG Files|*.jpg|BMP Files|*.bmp|TIFF FIles|*.tiff|All Files|*.*"
        If d.ShowDialog = DialogResult.OK Then
            Try

                Select Case d.FilterIndex
                    Case 1 'png
                        bmp.Save(d.FileName, Imaging.ImageFormat.Png)
                    Case 2 'jpg
                        bmp.Save(d.FileName, Imaging.ImageFormat.Jpeg)
                    Case 3 'bmp
                        bmp.Save(d.FileName, Imaging.ImageFormat.Bmp)
                    Case 4 'tiff
                        bmp.Save(d.FileName, Imaging.ImageFormat.Tiff)
                    Case Else
                        bmp.Save(d.FileName, Imaging.ImageFormat.Bmp)
                End Select
                MsgBox("Image saved successfully to" & vbCrLf & d.FileName)
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        End If
    End Sub

    Private Sub ShowDeformationsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ShowDeformationsToolStripMenuItem.Click
        ShowDeformations = Not ShowDeformations
        DrawModel()
    End Sub

    Private Sub DrawVArrow(px As Integer, py As Integer, c As Color, downward As Boolean)
        Dim p As New Pen(c)
        Dim size As Integer = 15
        If downward = False Then
            'upward arrow
            gr.DrawLine(p, New Point(px, py), New Point(px, py + size))
            gr.DrawLine(p, New Point(px, py), New Point(CInt(px - size / 4), CInt(py + size / 4)))
            gr.DrawLine(p, New Point(px, py), New Point(CInt(px + size / 4), CInt(py + size / 4)))
        Else
            'downward arrow
            gr.DrawLine(p, New Point(px, py), New Point(px, py - size))
            gr.DrawLine(p, New Point(px, py), New Point(CInt(px - size / 4), CInt(py - size / 4)))
            gr.DrawLine(p, New Point(px, py), New Point(CInt(px + size / 4), CInt(py - size / 4)))
        End If


    End Sub

    Private Sub HandleShowResultClick(sender As Object, e As EventArgs) Handles NoneToolStripMenuItem.Click,
            HRToolStripMenuItem.Click, SigmaYToolStripMenuItem.Click, TauXYToolStripMenuItem.Click,
            EpsilonXToolStripMenuItem.Click, EpsilonYToolStripMenuItem.Click, GammaXYToolStripMenuItem.Click

        ShowResult = Results.None

        If sender.Equals(NoneToolStripMenuItem) Then
            ShowResult = Results.None
        ElseIf sender.Equals(HRToolStripMenuItem) Then
            ShowResult = Results.HR
        ElseIf sender.Equals(SigmaYToolStripMenuItem) Then
            ShowResult = Results.SigmaY
        ElseIf sender.Equals(TauXYToolStripMenuItem) Then
            ShowResult = Results.TauXY
        ElseIf sender.Equals(EpsilonXToolStripMenuItem) Then
            ShowResult = Results.EpsilonX
        ElseIf sender.Equals(EpsilonYToolStripMenuItem) Then
            ShowResult = Results.EpsilonY
        ElseIf sender.Equals(GammaXYToolStripMenuItem) Then
            ShowResult = Results.GammaXY
        End If

        DrawModel()
    End Sub

    Private Sub TimeScrollBar_Scroll(sender As Object, e As ScrollEventArgs) Handles TimeScrollBar.Scroll
        DrawModel()
    End Sub

    Private Sub ExitToolStripMenuItem_Click_1(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        Me.Close()
    End Sub
    ' add two labels to distinguish diffusion and capillary transport process 30.07.2020 Xuande
    Private Sub Diffusion2DToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles Diffusion2DToolStripMenuItem.Click
        'check if there is a proper model
        If NElements <= 0 OrElse NNodes <= 0 Then
            'there are no elements defined
            'MsgBox("Please open a proper finite element model")
            MsgBox("Error reading number of elements and nodes, please open a proper mesh file")
            Return
        End If

        'perform analysis using the finite elemeent method

        Dim myThread As System.Threading.Thread

        myThread = New System.Threading.Thread(AddressOf Analyse)

        'frmC.MdiParent = Me
        'frmC.Show()

        If Para <> CShort(1) Then
            myThread.Start()
        End If

        'Analyse(NNodes, NElements, Nodes, Elements, Directory)

        'show the output
        DrawModel()
    End Sub

    Private Sub Transport2DToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles Transport2DToolStripMenuItem.Click
        'check if there is a proper model
        If NElements <= 0 OrElse NNodes <= 0 Then
            'there are no elements defined
            'MsgBox("Please open a proper finite element model")
            MsgBox("Error reading number of elements and nodes, please open a proper mesh file")
            Return
        End If

        'perform analysis using the finite elemeent method

        Dim myThread As System.Threading.Thread

        myThread = New System.Threading.Thread(AddressOf AnalyseTransport)

        'frmC.MdiParent = Me
        'frmC.Show()

        If Para <> CShort(1) Then
            myThread.Start()
        End If

        'Analyse(NNodes, NElements, Nodes, Elements, Directory)

        'show the output
        DrawModel()
    End Sub

    Private Sub pbModel_Paint(sender As Object, e As PaintEventArgs) Handles pbModel.Paint
        If bmp IsNot Nothing Then
            e.Graphics.DrawImage(bmp, 0, 0)
        End If
    End Sub

    Private Sub pbModel_Resize(sender As Object, e As EventArgs) Handles pbModel.Resize
        If bmp IsNot Nothing Then
            bmp.Dispose()
        End If
        If gr IsNot Nothing Then
            gr.Dispose()
        End If
        bmp = New Bitmap(pbModel.Width, pbModel.Height, pbModel.CreateGraphics)
        gr = Graphics.FromImage(bmp)
        DrawModel()
    End Sub

    Private Sub pbModel_MouseDown(sender As Object, e As MouseEventArgs) Handles pbModel.MouseDown
        If e.Button = MouseButtons.Left Then
            'zoom in
            zoom = zoom * 1.1
        ElseIf e.Button = MouseButtons.Right Then
            zoom = zoom / 1.1
        End If
        DrawModel()
    End Sub

    Private Sub ShowToolStripMenuItem_DropDownOpening(sender As Object, e As EventArgs) Handles ShowToolStripMenuItem.DropDownOpening

        NoneToolStripMenuItem.Checked = False
        HRToolStripMenuItem.Checked = False
        SigmaYToolStripMenuItem.Checked = False
        TauXYToolStripMenuItem.Checked = False
        EpsilonXToolStripMenuItem.Checked = False
        EpsilonYToolStripMenuItem.Checked = False
        GammaXYToolStripMenuItem.Checked = False

        Select Case ShowResult
            Case Results.None
                NoneToolStripMenuItem.Checked = True
            Case Results.HR
                HRToolStripMenuItem.Checked = True
            Case Results.SigmaY
                SigmaYToolStripMenuItem.Checked = True
            Case Results.TauXY
                TauXYToolStripMenuItem.Checked = True
            Case Results.EpsilonX
                EpsilonXToolStripMenuItem.Checked = True
            Case Results.EpsilonY
                EpsilonYToolStripMenuItem.Checked = True
            Case Results.GammaXY
                GammaXYToolStripMenuItem.Checked = True
        End Select

    End Sub

    Private Sub ModelToolStripMenuItem_DropDownOpening(sender As Object, e As EventArgs) Handles ModelToolStripMenuItem.DropDownOpening
        ShowModelToolStripMenuItem.Checked = ShowModel
        ShowDeformationsToolStripMenuItem.Checked = ShowDeformations
        ShowNodeNumbersToolStripMenuItem.Checked = ShowNodeNumbers
        ShowElementsOnDeformedShapeToolStripMenuItem.Checked = ShowElementsOnDeformedShape
    End Sub

    Private Sub pbModel_MouseWheel(sender As Object, e As MouseEventArgs) Handles pbModel.MouseWheel

        If e.Delta = 120 Then

            If vs.Value - vs.LargeChange >= 0 Then

                vs.Value -= vs.LargeChange

            ElseIf vs.Value - 1 >= 0 Then

                vs.Value -= 1

            End If

        Else

            If vs.Value + vs.LargeChange <= vs.Maximum Then

                vs.Value += vs.LargeChange

            ElseIf vs.Value + 1 <= vs.Maximum Then

                vs.Value += 1

            End If

        End If

        DrawModel()

    End Sub

End Class
