Option Explicit On
Imports System
Imports System.ComponentModel
Imports System.IO
Imports System.Linq
' Developper's note. Module de calcul diffusion 2D Xuande.2020.07.20
Public Class DiffusionXC
    Private NNodes, NElements As Integer
    Private Nodes() As NodeTrans
    Private Elements() As ElementTrans
    Public Sub Analyse(Directory)
        'Material parameters, can be converted from user defined input
        Dim pg As Double = 101325 'atmosphere pressure(pa)
        Dim rho_v As Double = 1 'density of vapor (kg/m3)
        Dim rho_l As Double = 1000 'density of liquid (kg/m3)
        Dim rho_c As Double = 2500 'density of concrete (kg/m3)
        Dim pc_0 As Double = 18.6237 ' parameter for ordinary concrete (Mpa)
        Dim m As Double = 0.461 ' parameter for ordinary concrete / for cement paste 37.5479
        Dim beta As Double = 2.2748 ' parameter for ordinary concrete / for cement paste 2.1684
        Dim KK As Double = 0.000000000002 'intrinsic permeability (m2)
        Dim yita_l As Double = 0.00089 'viscosity of water (kg/m.s)
        Dim phi As Double = 0.12 'porosity (-)
        Dim type As Integer = 1 'cement type (-)
        Dim W_C_ratio As Double = 0.4 'porosity (-)
        Dim C As Double = 400 'density of cement (kg/m3)
        Dim day As Double
        Dim D0 As Double = 0.00006  ' mm2/s
        Dim alpha_0 As Double = 0.05
        Dim Hc As Double = 0.75
        Dim w As Double = 0 'indicator for isotherm curve, judge if we choose to use desorption (w = 1) or adsorption curve (w = 0) 
        Dim alpha As Double = 0.6 'hydration degree (-)
        Dim Tk As Double = 293 'temperature in (K), attention, faudrait l'inserer dans le boucle parce que cela va varier en fonction de temps et espace, XC 2020.07.30
        Dim Tc As Double = Tk - 273 'temperature in (C)
        Dim wsat As Double = GetWsat(C, alpha, W_C_ratio, phi) 'saturated water mass (kg/m3)

        'Geometry parameters for boundary check program
        Dim X_upper As Double = 75 '200'mm, upper bound of X coordinate
        Dim X_lower As Double = -75 '200'mm, upper bound of X coordinate
        Dim Y_upper As Double = 75 '100'mm, upper bound of Y coordinate
        Dim Y_lower As Double = -75 '100'mm, upper bound of Y coordinate
        Dim Expo_X_upper As Boolean = False 'exposure on right most side
        Dim Expo_X_lower As Boolean = True 'exposure on left most side
        Dim Expo_Y_upper As Boolean = False 'exposure on top most side
        Dim Expo_Y_lower As Boolean = False 'exposure on bottom most side
        Dim X_node As Double
        Dim Y_node As Double

        'Computation parameters
        Dim nDof As Integer = NNodes
        Dim H_int As Double = 0.75 'initial relative humidity
        Dim S_int As Double = GetHtoS(H_int, type, C, W_C_ratio, Tk, day, rho_l, rho_c, alpha, w) 'initial saturation field
        Dim H_bound As Double = 0.9999 'boundary relative humidity
        Dim S_bound As Double = GetHtoS(H_bound, type, C, W_C_ratio, Tk, day, rho_l, rho_c, alpha, w) 'boundary saturation field
        Dim dt As Double = 3600   'time interval (s)
        Dim tmax As Double = 3600 * 24 * 3 'end time (s) 100 day / 72h
        Dim ind As Double = tmax / dt
        Dim T(ind) As Double 'time vector (days)
        Dim H_mat(ind, NNodes - 1) As Double 'Matrix for stockage of computation results (days, number of nodes)
        Dim S_mat(ind, NNodes - 1) As Double 'Matrix for stockage of computation results (days, number of nodes)
        Dim H_old(NNodes - 1) As Double
        Dim H_new(NNodes - 1) As Double
        Dim S_old(NNodes - 1) As Double
        Dim S_new(NNodes - 1) As Double
        Dim jj As Long
        Dim w_old(NNodes - 1) As Double
        Dim w_new(NNodes - 1) As Double
        Dim nFic1 As Short
        Dim nFic2 As Short
        Dim outfile(2) As String
        'Dim outfile2(1) As String
        Dim T_sauv As Single = 3600  '14400 'ouput time inteval (s) 4h
        Dim i, j, k As Integer

        'step 0: Creating output .txt computation result file 2020-07-17 Xuande 
        outfile(1) = Directory & "\" & "R_H_DiffusionModel" & ".txt"
        outfile(2) = Directory & "\" & "R_W_DiffusionModel" & ".txt"
        nFic1 = CShort(FreeFile())
        FileOpen(CInt(nFic1), outfile(1), OpenMode.Output)
        nFic2 = CShort(FreeFile())
        FileOpen(CInt(nFic2), outfile(2), OpenMode.Output)

        'step 0 Initialize output titles for result .txt files
        Print(nFic1, "RH", ",", nDof, ",", TAB)
        For jj = 0 To nDof - 1
            Print(CInt(nFic1), jj + CShort(1), ",", TAB)
        Next jj
        PrintLine(CInt(nFic1), " ")

        Print(nFic2, "W", ",", nDof, ",", TAB)
        For jj = 0 To nDof - 1
            Print(CInt(nFic2), jj + CShort(1), ",", TAB)
        Next jj
        PrintLine(CInt(nFic2), " ")

        'HRRange = New Range

        'For i = 0 To NElements - 1
        '    ReDim Elements(i).HR(ind + 1)
        '    Elements(i).HR(0) = H_int * 100
        '    HRRange.AddValue(Elements(i).HR(0))
        '    ReDim Time(ind + 1)
        '    Time(0) = 0
        'Next

        'Me.Invoke(Sub()
        '              LabelProgress.Visible = True
        '          End Sub)

        'Globlal time loop

        Dim ti As Integer

        For ti = 0 To ind

            'Me.Invoke(Sub()
            '              LabelProgress.Text = CStr(ti) + " / " + CStr(ind)
            '              Me.Refresh()
            '          End Sub)

            ' step 1: initialisation and boundary check
            T(ti) = 0 + dt * (ti - 0)
            If ti = 0 Then
                Print(CInt(nFic1), "0", ",", "0", ",", TAB)
                Print(CInt(nFic2), "0", ",", "0", ",", TAB)
                For i = 0 To nDof - 1
                    H_old(i) = H_int
                    ' boundary check program, 2020.08.03
                    If Nodes(i).Bord = True Then

                        ' check whether the current boundary is exposed to a boundary condition
                        X_node = Nodes(i).x
                        Y_node = Nodes(i).y
                        If Math.Abs(X_node - X_lower) <= 0.00001 And Expo_X_lower = True Then
                            H_old(i) = H_bound
                        ElseIf Math.Abs(X_node - X_upper) <= 0.00001 And Expo_X_upper = True Then
                            H_old(i) = H_bound
                        ElseIf Math.Abs(Y_node - Y_lower) <= 0.00001 And Expo_Y_lower = True Then
                            H_old(i) = H_bound
                        ElseIf Math.Abs(Y_node - Y_upper) <= 0.00001 And Expo_Y_upper = True Then
                            H_old(i) = H_bound
                        End If
                        'H_old(i_node) = H_bound
                    End If
                    w_old(i) = wsat * GetHtoS(H_old(i), type, C, W_C_ratio, Tk, day, rho_l, rho_c, alpha, w)
                    Print(CInt(nFic1), H_old(i), ",", TAB)
                    Print(CInt(nFic2), w_old(i), ",", TAB)
                Next
                PrintLine(CInt(nFic1), " ")
                PrintLine(CInt(nFic2), " ")
            Else
                For i = 0 To nDof - 1
                    H_old(i) = H_new(i)
                    w_old(i) = w_new(i)
                    ' boundary check program, 2020.08.03
                    If Nodes(i).Bord = True Then

                        ' check whether the current boundary is exposed to a boundary condition
                        X_node = Nodes(i).x
                        Y_node = Nodes(i).y
                        If Math.Abs(X_node - X_lower) <= 0.00001 And Expo_X_lower = True Then
                            H_old(i) = H_bound
                        ElseIf Math.Abs(X_node - X_upper) <= 0.00001 And Expo_X_upper = True Then
                            H_old(i) = H_bound
                        ElseIf Math.Abs(Y_node - Y_lower) <= 0.00001 And Expo_Y_lower = True Then
                            H_old(i) = H_bound
                        ElseIf Math.Abs(Y_node - Y_upper) <= 0.00001 And Expo_Y_upper = True Then
                            H_old(i) = H_bound
                        End If
                        'H_old(i_node) = H_bound
                    End If
                    w_old(i) = wsat * GetHtoS(H_old(i), type, C, W_C_ratio, Tk, day, rho_l, rho_c, alpha, w)

                Next
            End If

            'step 2: elemental and global Matrix constructions
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

            'step 3: now, we have assembled Hg_old, Ag and bg , to get LHS and RHS
            getLHS(LHS, NNodes, Ag, bg, dt)
            getRHS(R, NNodes, Ag, bg, dt)
            RHS = MultiplyMatrixWithVector(R, H_old)

            'step 4: now with LHS*x = RHS, using Gauss Elimination we can get the resolution for the new field of humidity Hnew
            GetX(H_new, LHS, RHS)

            'step 5: data stockage and boundary check 
            For j = 0 To NNodes - 1
                If H_new(j) >= 1 Then
                    H_new(j) = 1
                ElseIf H_new(j) <= 0 Then
                    H_new(j) = 0
                End If

                If Nodes(j).Bord = True Then
                    ' check whether the current boundary is exposed to a boundary condition
                    X_node = Nodes(j).x
                    Y_node = Nodes(j).y
                    If Math.Abs(X_node - X_lower) <= 0.00001 And Expo_X_lower = True Then
                        H_new(j) = H_bound
                    ElseIf Math.Abs(X_node - X_upper) <= 0.00001 And Expo_X_upper = True Then
                        H_new(j) = H_bound
                    ElseIf Math.Abs(Y_node - Y_lower) <= 0.00001 And Expo_Y_lower = True Then
                        H_new(j) = H_bound
                    ElseIf Math.Abs(Y_node - Y_upper) <= 0.00001 And Expo_Y_upper = True Then
                        H_new(j) = H_bound
                    End If
                End If
                w_new(j) = wsat * GetHtoS(H_new(j), type, C, W_C_ratio, Tk, day, rho_l, rho_c, alpha, w)
                'H_mat(ti, j) = H_new(j)

            Next

            'step 6: plot 2D image and export result .txt file 
            'For i = 0 To NElements - 1

            '    Elements(i).HR(ti + 1) = (H_new(Elements(i).Node1 - 1) + H_new(Elements(i).Node2 - 1) + H_new(Elements(i).Node3 - 1) + H_new(Elements(i).Node4 - 1)) * 100 / 4
            '    HRRange.AddValue(Elements(i).HR(ti + 1))

            'Next

            'Time(ti + 1) = (ti + 1) * dt / 3600 ' Time in hour
            If ti = 0 Then 'first step check
                RegisterH(nFic1, dt, nDof, H_new)
                PrintLine(CInt(nFic1), " ")
                RegisterH(nFic2, dt, nDof, w_new)
                PrintLine(CInt(nFic2), " ")

            Else
                If (ti * dt / T_sauv) = Int(ti * dt / T_sauv) And Int(ti * dt / T_sauv) > 0 Then ' check register time
                    RegisterH(nFic1, ti * dt, nDof, H_new)
                    PrintLine(CInt(nFic1), " ")
                    RegisterH(nFic2, ti * dt, nDof, w_new)
                    PrintLine(CInt(nFic2), " ")
                End If
            End If
        Next

        FileClose(CInt(nFic1))
        FileClose(CInt(nFic2))

        MsgBox("End of 2D computation", MsgBoxStyle.OkOnly And MsgBoxStyle.Information, "End")
        'Analysed = True

        'Me.Invoke(Sub()
        '              LabelProgress.Visible = False
        '          End Sub)

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
