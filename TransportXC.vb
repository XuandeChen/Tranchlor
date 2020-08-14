'Option Explicit On
'Imports System
'Imports System.ComponentModel
'Imports System.IO
'Imports System.Linq
'' Developper's note. Module pour calcul Transport Hydrique 2D. Xuande.2020.07.15
'' implémentation des équations de capillarité, de perméabilité et d'isotherme de adsorption/désorption
'Public Class TransportXC
'    Private NNodes, NElements As Integer
'    Private Nodes() As NodeTrans
'    Private Elements() As ElementTrans
'    Public Sub AnalyseTransport(Directory)
'        'Material parameters, can be converted from user defined input
'        Dim pg As Double = 101325 'atmosphere pressure(pa)
'        Dim rho_v As Double = 1 'density of vapor (kg/m3)
'        Dim rho_l As Double = 1000 'density of liquid (kg/m3)
'        Dim rho_c As Double = 2500 'density of concrete (kg/m3)
'        Dim pc_0 As Double = 13.3546 ' parameter for ordinary concrete (Mpa)
'        Dim m As Double = 0.4396 ' parameter for ordinary concrete 
'        Dim beta As Double = 2.2748 ' parameter for ordinary concrete / for cement paste 2.1684
'        Dim KK As Double = 0.00000000000000892 'intrinsic permeability (mm2), given by the user
'        Dim yita_l As Double = 0.00089 'viscosity of water (kg/m.s=pa.s)
'        Dim phi As Double = 0.12 'porosity (-)
'        Dim type As Integer = 1 'cement type (-)
'        Dim W_C_ratio As Double = 0.5 'porosity (-)
'        Dim C As Double = 350 'density of cement (kg/m3)
'        Dim Water_tot As Double = W_C_ratio * C 'density of cement (kg/m3)
'        Dim day As Double
'        Dim alpha As Double = 0.95 'hydration degree (-)
'        Dim Tk As Double = 293 'temperature in (K), attention, faudrait l'inserer dans le boucle parce que cela va varier en fonction de temps et espace, XC 2020.07.30
'        Dim Tc As Double = Tk - 273 'temperature in (C)
'        Dim wsat As Double = GetWsat(Water_tot, C) 'saturated water mass (kg/m3)
'        'Dim Water As Double = C * W_C_ratio 'saturated water content (-)

'        'Geometry parameters for boundary check program
'        Dim X_upper As Double = 75 'mm, upper bound of X coordinate
'        Dim X_lower As Double = -75 'mm, upper bound of X coordinate
'        Dim Y_upper As Double = 75 'mm, upper bound of Y coordinate
'        Dim Y_lower As Double = -75 'mm, upper bound of Y coordinate
'        Dim Expo_X_upper As Boolean = False 'exposure on right most side
'        Dim Expo_X_lower As Boolean = True 'exposure on left most side
'        Dim Expo_Y_upper As Boolean = False 'exposure on top most side
'        Dim Expo_Y_lower As Boolean = False 'exposure on bottom most side
'        Dim X_node As Double
'        Dim Y_node As Double

'        'Computation parameters
'        Dim nDof As Integer = NNodes
'        Dim w As Double = 0 'indicator for isotherm curve, judge if we choose to use desorption (w = 1) or adsorption curve (w = 0) 
'        Dim H_int As Double = 0.75 'initial relative humidity
'        Dim S_int As Double = GetHtoS(H_int, type, C, W_C_ratio, Tk, day, rho_l, rho_c, alpha, w) 'initial saturation field
'        Dim H_bound As Double = 0.9999 'boundary relative humidity
'        Dim S_bound As Double = GetHtoS(H_bound, type, C, W_C_ratio, Tk, day, rho_l, rho_c, alpha, w) 'boundary saturation field
'        Dim dt As Double = 3600    'time interval (s)
'        Dim tmax As Double = 3600 * 24 * 3 'end time (s)  100day / 72h
'        Dim ind As Double = tmax / dt
'        Dim T(ind) As Double 'time vector (days)
'        Dim H_mat(ind, NNodes - 1) As Double 'Matrix for stockage of computation results (days, number of nodes)
'        Dim S_mat(ind, NNodes - 1) As Double 'Matrix for stockage of computation results (days, number of nodes)
'        Dim H_old(NNodes - 1) As Double
'        Dim H_new(NNodes - 1) As Double
'        Dim S_old(NNodes - 1) As Double
'        Dim S_new(NNodes - 1) As Double
'        Dim jj As Long
'        Dim w_old(NNodes - 1) As Double
'        Dim w_new(NNodes - 1) As Double
'        Dim nFic1 As Short
'        Dim nFic2 As Short
'        Dim outfile(2) As String
'        Dim T_sauv As Single = 3600  'ouput time inteval (s) 4h /10day
'        Dim i, j, k As Integer

'        'step 0: Creating output .txt computation result file 2020-07-27 Xuande 
'        outfile(1) = Directory & "\" & "R_S_CapillaryModel" & ".txt"
'        outfile(2) = Directory & "\" & "R_W_CapillaryModel" & ".txt"
'        nFic1 = CShort(FreeFile())
'        FileOpen(CInt(nFic1), outfile(1), OpenMode.Output)
'        nFic2 = CShort(FreeFile())
'        FileOpen(CInt(nFic2), outfile(2), OpenMode.Output)

'        'step 0: Initialize output titres for result .txt files
'        Print(nFic1, "S", ",", nDof, ",", TAB)
'        For jj = 0 To nDof - 1
'            Print(CInt(nFic1), jj + CShort(1), ",", TAB)
'        Next jj
'        PrintLine(CInt(nFic1), " ")

'        Print(nFic2, "W", ",", nDof, ",", TAB)
'        For jj = 0 To nDof - 1
'            Print(CInt(nFic2), jj + CShort(1), ",", TAB)
'        Next jj
'        PrintLine(CInt(nFic2), " ")

'        'SRange = New Range

'        'For i = 0 To NElements - 1
'        '    ReDim Elements(i).S(ind + 1)
'        '    Elements(i).S(0) = S_int * 100
'        '    SRange.AddValue(Elements(i).S(0))
'        '    ReDim Time(ind + 1)
'        '    Time(0) = 0
'        'Next
'        'Globlal time loop
'        Dim ti As Integer

'        For ti = 0 To ind

'            'Me.Invoke(Sub()
'            '              LabelProgress.Text = CStr(ti) + " / " + CStr(ind)
'            '              Me.Refresh()
'            '          End Sub)

'            ' step 1: initialisation saturation field and boundary check
'            T(ti) = 0 + dt * (ti - 0)
'            day = CDbl(ti * dt / 3600 / 24)
'            If ti = 0 Then
'                Print(CInt(nFic1), "0", ",", "0", ",", TAB)
'                Print(CInt(nFic2), "0", ",", "0", ",", TAB)
'                For i = 0 To nDof - 1
'                    H_old(i) = H_int
'                    S_old(i) = S_int
'                    ' boundary check program, 2020.08.03
'                    If Nodes(i).Bord = True Then

'                        ' check whether the current boundary is exposed to a boundary condition
'                        X_node = Nodes(i).x
'                        Y_node = Nodes(i).y
'                        S_bound = GetHtoS(H_bound, type, C, W_C_ratio, Tk, day, rho_l, rho_c, alpha, w)
'                        If Math.Abs(X_node - X_lower) <= 0.00001 And Expo_X_lower = True Then
'                            S_old(i) = S_bound
'                        ElseIf Math.Abs(X_node - X_upper) <= 0.00001 And Expo_X_upper = True Then
'                            S_old(i) = S_bound
'                        ElseIf Math.Abs(Y_node - Y_lower) <= 0.00001 And Expo_Y_lower = True Then
'                            S_old(i) = S_bound
'                        ElseIf Math.Abs(Y_node - Y_upper) <= 0.00001 And Expo_Y_upper = True Then
'                            S_old(i) = S_bound
'                        End If
'                    End If
'                    w_old(i) = wsat * S_old(i)

'                    Print(CInt(nFic1), S_old(i), ",", TAB)
'                    Print(CInt(nFic2), w_old(i), ",", TAB)
'                Next
'                PrintLine(CInt(nFic1), " ")
'                PrintLine(CInt(nFic2), " ")
'            Else
'                'regular loop
'                For i = 0 To nDof - 1
'                    H_old(i) = H_new(i)
'                    S_old(i) = S_new(i)
'                    If Nodes(i).Bord = True Then
'                        ' check whether the current boundary is exposed to a boundary condition
'                        X_node = Nodes(i).x
'                        Y_node = Nodes(i).y
'                        S_bound = GetHtoS(H_bound, type, C, W_C_ratio, Tk, day, rho_l, rho_c, alpha, w)
'                        If Math.Abs(X_node - X_lower) <= 0.00001 And Expo_X_lower = True Then
'                            S_old(i) = S_bound
'                        ElseIf Math.Abs(X_node - X_upper) <= 0.00001 And Expo_X_upper = True Then
'                            S_old(i) = S_bound
'                        ElseIf Math.Abs(Y_node - Y_lower) <= 0.00001 And Expo_Y_lower = True Then
'                            S_old(i) = S_bound
'                        ElseIf Math.Abs(Y_node - Y_upper) <= 0.00001 And Expo_Y_upper = True Then
'                            S_old(i) = S_bound
'                        End If
'                        'H_old(i_node) = H_bound
'                    End If
'                    w_old(i) = wsat * S_old(i)
'                Next
'            End If

'            'step 2: elemental and global Matrix constructions
'            Dim LHS(,) As Double
'            Dim R(,) As Double
'            Dim RHS() As Double
'            Dim bg(nDof - 1, nDof - 1) As Double 'Global b matrix
'            Dim Ag(nDof - 1, nDof - 1) As Double 'Global A matrix
'            Dim cie As CIETrans
'            Dim se As SETrans 'element saturation vector
'            Dim D As Double = GetD(Tk, pg)
'            Dim Dv As Double
'            Dim Dl As Double
'            Dim S_avg As Double 'element average saturation
'            Dim H_avg As Double
'            Dim kr As Double
'            Dim pc As Double
'            Dim dpcdS As Double
'            Dim f As Double
'            Dim S_ele() As Double
'            Dim De As Double

'            'Matrix assembling
'            For i = 0 To NElements - 1
'                se = New SETrans(
'                          S_old(Elements(i).Node1 - 1), S_old(Elements(i).Node2 - 1),
'                          S_old(Elements(i).Node3 - 1), S_old(Elements(i).Node4 - 1)
'                          )
'                S_ele = se.getSe
'                S_avg = GetAvgH(S_ele)
'                kr = Getkr(S_avg, beta)
'                pc = Getpc(S_avg, pc_0, m)
'                dpcdS = GetdpcdS(S_avg, pc_0, m)
'                Dl = GetDl(KK, yita_l, dpcdS, kr)
'                f = Getf(phi, S_avg)
'                Dv = GetDv(rho_v, rho_l, dpcdS, f, D, pg)
'                De = (Dv + Dl) / phi
'                cie = New CIETrans(
'                          Nodes(Elements(i).Node1 - 1).x, Nodes(Elements(i).Node1 - 1).y,
'                          Nodes(Elements(i).Node2 - 1).x, Nodes(Elements(i).Node2 - 1).y,
'                          Nodes(Elements(i).Node3 - 1).x, Nodes(Elements(i).Node3 - 1).y,
'                          Nodes(Elements(i).Node4 - 1).x, Nodes(Elements(i).Node4 - 1).y,
'                          De)
'                AssembleKg(cie.getbe, bg, i)
'                AssembleKg(cie.getAe, Ag, i)
'            Next

'            'step 3: now, we have assembled Sg_old, Ag and bg , to get LHS and RHS
'            getLHS(LHS, NNodes, Ag, bg, dt)
'            getRHS(R, NNodes, Ag, bg, dt)
'            RHS = MultiplyMatrixWithVector(R, S_old)

'            'step 4: now with LHS*x = RHS, using Gauss Elimination we can get the resolution for the new field of humidity Hnew
'            GetX(S_new, LHS, RHS)

'            'step 5: data stockage
'            For j = 0 To NNodes - 1

'                If S_new(j) >= 1 Then
'                    S_new(j) = 1
'                ElseIf S_new(j) <= 0 Then
'                    S_new(j) = 0
'                End If

'                If Nodes(j).Bord = True Then
'                    ' check whether the current boundary is exposed to a boundary condition
'                    X_node = Nodes(j).x
'                    Y_node = Nodes(j).y
'                    S_bound = GetHtoS(H_bound, type, C, W_C_ratio, Tk, day, rho_l, rho_c, alpha, w)
'                    If Math.Abs(X_node - X_lower) <= 0.00001 And Expo_X_lower = True Then
'                        S_new(j) = S_bound
'                    ElseIf Math.Abs(X_node - X_upper) <= 0.00001 And Expo_X_upper = True Then
'                        S_new(j) = S_bound
'                    ElseIf Math.Abs(Y_node - Y_lower) <= 0.00001 And Expo_Y_lower = True Then
'                        S_new(j) = S_bound
'                    ElseIf Math.Abs(Y_node - Y_upper) <= 0.00001 And Expo_Y_upper = True Then
'                        S_new(j) = S_bound
'                    End If
'                End If
'                w_new(j) = wsat * S_new(j)
'                'S_mat(ti, j) = S_new(j)

'            Next

'            'step 6: plot 2D image and export result .txt file 
'            'For i = 0 To NElements - 1

'            '    Elements(i).S(ti + 1) = (S_new(Elements(i).Node1 - 1) + S_new(Elements(i).Node2 - 1) + S_new(Elements(i).Node3 - 1) + S_new(Elements(i).Node4 - 1)) * 100 / 4
'            '    SRange.AddValue(Elements(i).S(ti + 1))

'            'Next

'            'Time(ti + 1) = (ti + 1) * dt / 3600 ' Time in hour

'            If ti = 0 Then 'first step check
'                RegisterS(nFic1, dt, nDof, S_new)
'                PrintLine(CInt(nFic1), " ")

'                RegisterS(nFic2, dt, nDof, w_new)
'                PrintLine(CInt(nFic2), " ")

'            ElseIf (ti * dt / T_sauv) = Int(ti * dt / T_sauv) Then ' check register time
'                RegisterS(nFic1, ti * dt, nDof, S_new)
'                PrintLine(CInt(nFic1), " ")

'                RegisterS(nFic2, ti * dt, nDof, w_new)
'                PrintLine(CInt(nFic2), " ")
'            End If

'        Next
'        FileClose(CInt(nFic1))
'        FileClose(CInt(nFic2))


'        MsgBox("Fin du calcul transport 2D", MsgBoxStyle.OkOnly And MsgBoxStyle.Information, "End")
'        'Analysed = True

'        'Me.Invoke(Sub()
'        '              LabelProgress.Visible = False
'        '          End Sub)

'    End Sub

'    'Assembling global matrix /water transport
'    Private Sub AssembleKg(ByRef ke(,) As Double, ByRef Kg(,) As Double, ElementNo As Integer)
'        Dim i, j As Integer
'        Dim dofs() As Integer = {getDOF(Elements(ElementNo).Node1 - 1),
'                                 getDOF(Elements(ElementNo).Node2 - 1),
'                                 getDOF(Elements(ElementNo).Node3 - 1),
'                                 getDOF(Elements(ElementNo).Node4 - 1)}
'        Dim dofi, dofj As Integer
'        For i = 0 To 3 'each dof of the Se
'            dofi = dofs(i)
'            For j = 0 To 3
'                dofj = dofs(j)
'                Kg(dofi, dofj) = Kg(dofi, dofj) + ke(i, j)
'            Next
'        Next
'    End Sub
'    Private Sub RegisterS(ByRef nFic1 As Short, ByRef Temps As Decimal, ByRef Dofs As Short, ByRef S_new() As Double)
'        Dim j As Short
'        'Register values
'        Print(CInt(nFic1), Temps / 3600, ",", Temps, ",", TAB)
'        For j = 0 To Dofs - 1
'            Print(CInt(nFic1), S_new(j), ",", TAB)
'        Next j
'    End Sub
'End Class

