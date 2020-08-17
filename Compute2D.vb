
Imports System.Data.SqlClient
Imports System.Linq
Public Class Compute2D

    Dim dt As Double 'time interval (s)
    Dim tmax As Double 'end time (s) 72h
    Dim T_sauv As Double
    Dim directory As String

    Dim H_int As Double  'initial relative humidity in the material
    Dim Tc As Double 'initial temperature in the material
    Dim Tk As Double = Tc + 273 '(K)
    Dim rho_v As Double = 1 'density of vapor (kg/m3)
    Dim rho_l As Double = 1000 'density of liquid (kg/m3)
    Dim D0 As Double
    Dim alpha_0 As Double
    Dim Hc As Double
    Dim type As Integer
    Dim Expo(1) As Exposition

    Public LgLim As Integer = 40
    Public pPH As Double = 12.6
    Public mPh As Double = 6.5
    Public RoW As Integer = 1000        'kg/m3
    Public R As Double = 8.3145        'J/mol.K

    'recuperer depuis database
    Dim C As Double
    Dim W_C_ratio As Double  'ratio E/C (kg/m3)
    Dim Water_tot As Double  'densite eau (kg/m3)
    Dim wsat As Double 'teneur en eau sature (kg/m3)
    Dim phi As Double  'porosity (-)
    Dim w As Double  'indicator for isotherm curve, judge if we choose to use desorption (w = 1) or adsorption curve (w = 0) 
    Dim alpha As Double  'hydration degree (-)
    Dim day As Double  'age du beton
    Dim rho_c As Double  'density of concrete (kg/m3)

    'Dim wc As Double
    'Dim kg As Double
    'Dim kl As Double
    'Dim a As Double
    'Dim b As Double
    'Dim m As Double

    '' Moisture Parameters 
    'Dim aa As Single ' Function coefficient
    'Dim Hc As Single ' Function coefficient
    'Dim ab As Single ' Function coefficient
    'Dim tc As Single ' Function coefficient
    'Dim ImpHydr As Boolean
    '' chloride
    'Dim LambdaT(1) As Single
    'Dim LambdaH(1) As Single
    'Dim aOH As Single
    'Dim EbG As Single
    'Dim toG As Single
    'Dim faG As Single
    '' Moisture variables
    'Dim hMin As Single
    'Dim hEcart As Single
    'Dim wMin As Single
    'Dim wEcart As Single
    'Dim CTmin As Single
    'Dim CTecart As Single
    'Dim CLmin As Single
    'Dim CLecart As Single
    'Dim Tecart As Single
    'Dim H_snap As Single
    'Dim Retard As Single
    '' Structural data
    'Dim TimeMax As Single
    'Dim Length As Single 'length of the layer [mm]
    'Dim Ne As Short 'Number of finite elements
    'Dim Le(1) As Decimal 'element length
    'Dim PosProf(1) As Decimal
    ''Computational data
    'Dim Dofs As Short
    ''computationals values
    'Dim DeltaT As Single
    ''boundary conditions
    'Dim NEXPO As Short
    'Dim NQUAL As Short
    'Dim taff As Single
    'Dim Hsauv As Single
    'Dim Wsauv As Single
    'Dim CTsauv As Single
    'Dim CLsauv As Single
    'Dim Tsauv As Single
    'Dim Carbsauv As Single
    'Dim Filebeton(1) As String
    'Dim Fileres(1) As String
    'Dim PD(1) As Single
    'Dim qGran(1) As Single
    'Dim Dcl(1) As Single
    'Dim SAT(1) As Single
    'Dim ciment(1) As Single
    'Dim FileGexpo(1) As String
    'Dim FileDexpo(1) As String
    'Dim proba(1, 1) As Single
    'Dim ChoixRep As Short
    'Dim nChmt As Short
    'Dim Nbreel(1) As Short
    'Dim LenApp(1) As Single
    'Dim EC(1) As Single
    'Dim tProt(1) As Single
    'Dim Vct(1) As Single
    'Dim Nct(1) As Single
    'Dim capCal As Single
    'Dim Hydr(1) As Single
    'Dim ED(1) As Single
    'Dim ToHydr(1) As Single
    'Dim Ecl(1) As Single
    'Dim ToCl(1) As Single
    'Dim Ctherm(1) As Double
    'Dim Chydr(1) As Double
    'Dim Cion(1) As Double
    'Dim PostFile As String
    'Dim GyCO2 As Single
    'Dim DyCO2 As Single
    'Dim RoA(1) As Single
    'Dim RoC(1) As Single

    Public Sub Read_InitialConditions()

        ''''''''''''''''''''''''''''''''''''''''''''
        Dim Filtre As String = "Text files (CI2D_*.txt)|CI2D_*.txt"
        Dim Index As Short = 1
        Dim Directoire As Boolean = True
        Dim Titre As String = "Sélectionner les conditions initiales du matériau"
        Dim OutFile As String
        Dim Canc As Boolean = False
        Dim nFic As Integer = FreeFile()
        Dim i, j As Short

        OpenDialog(OutFile, Canc, Filtre, Index, Directoire, Titre)
        If Canc = True Then End
        ''''''''''''''''''''''''''''''''''''''''''''
        Dim Postfile As String = frmTrans2D.Directory & "\"
        FileOpen(nFic, OutFile, OpenMode.Input, OpenAccess.Read, OpenShare.Shared)
        FilePost(OutFile, Postfile)
        Input(nFic, H_int)
        Input(nFic, Tc)

        FileClose(nFic)

    End Sub

    Public Sub Read_Expositions(ByRef Nodes() As NodeTrans)

        ''''''''''''''''''''''''''''''''''''''''''''
        Dim Filtre As String = "Text files (EXPO2D_*.txt)|EXPO2D_*.txt"
        Dim Index As Short = 1
        Dim Directoire As Boolean = True
        Dim Titre As String = "Sélectionner les expositions"
        Dim OutFile As String
        Dim Canc As Boolean = False
        Dim nFic As Integer = FreeFile()
        Dim i, j As Short

        OpenDialog(OutFile, Canc, Filtre, Index, Directoire, Titre)
        If Canc = True Then End
        ''''''''''''''''''''''''''''''''''''''''''''
        Dim Postfile As String = frmTrans2D.Directory & "\"
        FileOpen(nFic, OutFile, OpenMode.Input, OpenAccess.Read, OpenShare.Shared)
        FilePost(OutFile, Postfile)

        Dim NbExpo As Integer
        Input(nFic, NbExpo)
        ReDim Expo(NbExpo - 1)

        Dim NodeMin, NodeMax As Integer
        For ii As Integer = 0 To NbExpo - 1

            Dim FileExpo As String
            Input(nFic, FileExpo)

            Expo(ii) = New Exposition(FileExpo)

            Input(nFic, NodeMin)
            Input(nFic, NodeMax)

            For jj As Integer = NodeMin To NodeMax

                Nodes(jj - 1).NumExpo = ii + 1

            Next

        Next

        FileClose(nFic)

    End Sub

    Public Sub Read_Simulation(ByRef Dir As String)

        ''''''''''''''''''''''''''''''''''''''''''''
        Dim Filtre As String = "Text files (SIMU2D_*.txt)|SIMU2D_*.txt"
        Dim Index As Short = 1
        Dim Directoire As Boolean = True
        Dim Titre As String = "Sélectionner les données de simulation"
        Dim OutFile As String
        Dim Canc As Boolean = False
        Dim nFic As Integer = FreeFile()
        Dim i, j As Short

        OpenDialog(OutFile, Canc, Filtre, Index, Directoire, Titre)
        If Canc = True Then End
        ''''''''''''''''''''''''''''''''''''''''''''
        Dim Postfile As String = frmTrans2D.Directory & "\"
        FileOpen(nFic, OutFile, OpenMode.Input, OpenAccess.Read, OpenShare.Shared)
        FilePost(OutFile, Postfile)

        Dim Para1 As Single
        Dim Para2 As Single
        Dim Para3 As Single
        Dim Para4 As Single
        Dim test As Single

        Input(nFic, tmax)
        Input(nFic, dt)
        Input(nFic, T_sauv)

        directory = Dir
        FileClose(nFic)

    End Sub
    Public Sub DBInput(ByRef MatName As String)

        Dim con As New SqlConnection("Data Source=GCI-DACON-01.FSG.ULAVAL.CA;Initial Catalog=\\GCI-DACON-01\TRANSCHLOR\DATABASE\TRANSCHLORMAT.MDF;Integrated Security=True")

        Try

            con.Open()

            Dim sql As String = "SELECT * FROM Materials WHERE Name IN ('" + MatName + "')"
            Dim command As New SqlCommand(sql, con)
            Dim reader As SqlDataReader = command.ExecuteReader()

            While reader.Read()
                type = CDbl(reader("Type").ToString())
                W_C_ratio = CDbl(reader("W/C").ToString())
                rho_c = CDbl(reader("Density").ToString()) 'density of concrete (kg/m3)
                C = CDbl(reader("CementContent").ToString())
                phi = CDbl(reader("Porosity").ToString())
                D0 = CDbl(reader("Dvap").ToString()) * 1000000.0
                alpha_0 = CDbl(reader("alpha0").ToString())
                Hc = CDbl(reader("Hc").ToString())

                'wc = CDbl(reader("W/C").ToString())
                'kg = CDbl(reader("kg").ToString())
                'kl = CDbl(reader("kl").ToString())
                'a = CDbl(reader("a").ToString())
                'b = CDbl(reader("b").ToString())
                'm = CDbl(reader("m").ToString())

            End While

        Catch ex As SqlException

            MsgBox("Database not found")

        End Try

    End Sub

    Public Sub CalculInitialization()
        Water_tot = W_C_ratio * C 'densite eau (kg/m3)
        wsat = GetWsat(Water_tot, C) 'teneur en eau sature (kg/m3)
        w = 1 'indicator for isotherm curve, judge if we choose to use desorption (w = 1) or adsorption curve (w = 0) 
        alpha = 0.85 'hydration degree (-)
        day = 0 'age du beton
    End Sub
    'Coeur du calcul
    Public Sub Compute_All(ByRef frm As frmTrans2D)

        Dim nFic1 As Short
        Dim nFic2 As Short
        Dim nfic3 As Short
        Dim outfile(3) As String
        Dim nDof As Integer = frm.NNodes

        'step 0: Creating output .txt computation result file 2020-07-17 Xuande 
        outfile(1) = directory & "\" & "R_H_DiffusionModel" & ".txt"
        outfile(2) = directory & "\" & "R_W_DiffusionModel" & ".txt"
        outfile(3) = directory & "\" & "R_S_DiffusionModel" & ".txt"
        nFic1 = CShort(FreeFile())
        FileOpen(CInt(nFic1), outfile(1), OpenMode.Output)
        nFic2 = CShort(FreeFile())
        FileOpen(CInt(nFic2), outfile(2), OpenMode.Output)
        nfic3 = CShort(FreeFile())
        FileOpen(CInt(nfic3), outfile(3), OpenMode.Output)

        'step 0 Initialize output titres for result .txt files
        Print(nFic1, "RH", ",", nDof, ",", "Average RH", ",", "dH", ",", TAB)
        For jj As Integer = 0 To nDof - 1
            Print(CInt(nFic1), jj + CShort(1), ",", TAB)
        Next jj
        PrintLine(CInt(nFic1), " ")

        Print(nFic2, "W", ",", nDof, ",", "Average W", ",", "dW", ",", TAB)
        For jj As Integer = 0 To nDof - 1
            Print(CInt(nFic2), jj + CShort(1), ",", TAB)
        Next jj
        PrintLine(CInt(nFic2), " ")

        Print(nfic3, "S", ",", nDof, ",", "Average S", ",", "dS", ",", TAB)
        For jj As Integer = 0 To nDof - 1
            Print(CInt(nFic2), jj + CShort(1), ",", TAB)
        Next jj
        PrintLine(CInt(nfic3), " ")

        Dim ti As Integer
        Dim ind As Double = tmax / dt
        Dim H_old(nDof - 1) As Double
        Dim H_new(nDof - 1) As Double
        Dim S_old(nDof - 1) As Double
        Dim S_new(nDof - 1) As Double
        Dim w_old(nDof - 1) As Double
        Dim w_new(nDof - 1) As Double
        Dim T_old(nDof - 1) As Double

        Dim T(ind) As Double 'time vector (days)

        frm.HRRange = New Range
        frm.SlRange = New Range

        For i As Integer = 0 To frm.NElements - 1
            ReDim frm.Elements(i).HR(ind + 1)
            ReDim frm.Elements(i).Sl(ind + 1)
            frm.Elements(i).HR(0) = H_int * 100
            frm.Elements(i).Sl(0) = GetHtoS(H_int, type, C, W_C_ratio, Tk, day, rho_l, rho_c, alpha, w) * 100
            frm.HRRange.AddValue(frm.Elements(i).HR(0))
            frm.SlRange.AddValue(frm.Elements(i).Sl(0))
            ReDim frm.Time(ind + 1)
            frm.Time(0) = 0
        Next

        'Me.Invoke(Sub()
        frm.LabelProgress.Visible = True
        'End Sub)

        For ti = 0 To ind

            frm.LabelProgress.Text = CStr(ti) + " / " + CStr(ind)
            frm.Refresh()

            ' step 1: initialisation and boundary check
            T(ti) = 0 + dt * (ti - 0)
            If ti = 0 Then

                Print(CInt(nFic1), "0", ",", "0", ",", TAB)
                Print(CInt(nFic2), "0", ",", "0", ",", TAB)
                Print(CInt(nfic3), "0", ",", "0", ",", TAB)

                For i As Integer = 0 To nDof - 1
                    H_old(i) = H_int
                    S_old(i) = GetHtoS(H_old(i), type, C, W_C_ratio, Tk, day, rho_l, rho_c, alpha, w)
                    w_old(i) = wsat * S_old(i) * phi
                Next
                Dim w_avg_0 As Double = w_old.Average()
                Dim H_avg_0 As Double = H_old.Average()
                Dim S_avg_0 As Double = S_old.Average()
                Print(CInt(nFic1), H_avg_0, ",", "0", ",", TAB)
                Print(CInt(nFic2), w_avg_0, ",", "0", ",", TAB)
                Print(CInt(nfic3), S_avg_0, ",", "0", ",", TAB)

                Dim i_node As Integer

                For i_node = 0 To nDof - 1
                    Print(CInt(nFic1), H_old(i_node), ",", TAB)
                    Print(CInt(nFic2), w_old(i_node), ",", TAB)
                    Print(CInt(nfic3), S_old(i_node), ",", TAB)
                    If frm.Nodes(i_node).Bord = True Then
                        ' check whether the current boundary is exposed to a boundary condition
                        Dim X_node As Double = frm.Nodes(i_node).x
                        Dim Y_node As Double = frm.Nodes(i_node).y
                        Dim NbExpo As Integer = frm.Nodes(i_node).NumExpo
                        If NbExpo <> 0 Then
                            H_old(i_node) = Expo(NbExpo - 1).Humidite(ti)
                        End If
                    End If
                    S_old(i_node) = GetHtoS(H_old(i_node), type, C, W_C_ratio, Tk, day, rho_l, rho_c, alpha, w)
                    w_old(i_node) = wsat * S_old(i_node) * phi
                Next

                PrintLine(CInt(nFic1), " ")
                PrintLine(CInt(nFic2), " ")
                PrintLine(CInt(nfic3), " ")
            End If

            'regular loop
            For i_node As Integer = 0 To nDof - 1
                'boundary check program, 2020.08.03 Thomas
                Dim NbExpo As Integer = frm.Nodes(i_node).NumExpo
                If NbExpo <> 0 Then
                    H_old(i_node) = Expo(NbExpo - 1).Humidite(ti)
                End If
                S_old(i_node) = GetHtoS(H_old(i_node), type, C, W_C_ratio, Tk, day, rho_l, rho_c, alpha, w)
                w_old(i_node) = wsat * S_old(i_node) * phi
            Next

            'step 2: elemental and global Matrix constructions
            Dim LHS(,) As Double
            Dim R(,) As Double
            Dim RHS() As Double
            Dim bg(nDof - 1, nDof - 1) As Double 'Global b matrix
            Dim Ag(nDof - 1, nDof - 1) As Double 'Global A matrix
            Dim cie As CIETrans
            Dim cieNew As CIETransNew
            Dim he As HETrans
            Dim H_avg As Double
            Dim H_ele() As Double
            Dim De As Double
            'Matrix assembling
            For i As Integer = 0 To frm.NElements - 1
                he = New HETrans(
                          H_old(frm.Elements(i).Node1 - 1), H_old(frm.Elements(i).Node2 - 1),
                          H_old(frm.Elements(i).Node3 - 1), H_old(frm.Elements(i).Node4 - 1)
                          )
                H_ele = he.getHe
                '' old program using elemental average value to compute diffusion coefficient 2020.08.15 Xuande
                'H_avg = GetAvgH(H_ele)
                'De = GetDh(D0, alpha_0, Hc, Tc, H_avg)
                'cie = New CIETrans(
                '          frm.Nodes(frm.Elements(i).Node1 - 1).x, frm.Nodes(frm.Elements(i).Node1 - 1).y,
                '          frm.Nodes(frm.Elements(i).Node2 - 1).x, frm.Nodes(frm.Elements(i).Node2 - 1).y,
                '          frm.Nodes(frm.Elements(i).Node3 - 1).x, frm.Nodes(frm.Elements(i).Node3 - 1).y,
                '          frm.Nodes(frm.Elements(i).Node4 - 1).x, frm.Nodes(frm.Elements(i).Node4 - 1).y,
                '          De)
                'AssembleKg(cie.getbe, bg, frm.Elements, i)
                'AssembleKg(cie.getAe, Ag, frm.Elements, i)

                '' new program using nodal interpolations instead of mean value on elements to calculate diffusion coefficient 2020.08.15 Xuande
                Dim d1 As Double = GetDh(D0, alpha_0, Hc, Tc, H_ele(0))
                Dim d2 As Double = GetDh(D0, alpha_0, Hc, Tc, H_ele(1))
                Dim d3 As Double = GetDh(D0, alpha_0, Hc, Tc, H_ele(2))
                Dim d4 As Double = GetDh(D0, alpha_0, Hc, Tc, H_ele(3))

                cieNew = New CIETransNew(
                          frm.Nodes(frm.Elements(i).Node1 - 1).x, frm.Nodes(frm.Elements(i).Node1 - 1).y,
                          frm.Nodes(frm.Elements(i).Node2 - 1).x, frm.Nodes(frm.Elements(i).Node2 - 1).y,
                          frm.Nodes(frm.Elements(i).Node3 - 1).x, frm.Nodes(frm.Elements(i).Node3 - 1).y,
                          frm.Nodes(frm.Elements(i).Node4 - 1).x, frm.Nodes(frm.Elements(i).Node4 - 1).y,
                          d1, d2, d3, d4)

                AssembleKg(cieNew.getbe, bg, frm.Elements, i)
                AssembleKg(cieNew.getAe, Ag, frm.Elements, i)

            Next

            'step 3: now, we have assembled Hg_old, Ag and bg , to get LHS and RHS
            getLHS(LHS, frm.NNodes, Ag, bg, dt)
            getRHS(R, frm.NNodes, Ag, bg, dt)
            RHS = MultiplyMatrixWithVector(R, H_old)
            GetX(H_new, LHS, RHS)

            'step 5: data stockage
            For j As Integer = 0 To frm.NNodes - 1
                If H_new(j) >= 1 Then
                    H_new(j) = 1
                ElseIf H_new(j) <= 0 Then
                    H_new(j) = 0
                End If
                Dim NbExpo As Integer = frm.Nodes(j).NumExpo
                If NbExpo <> 0 Then
                    ' check whether the current boundary is exposed to a boundary condition
                    H_new(j) = Expo(NbExpo - 1).Humidite(ti)
                End If
                S_new(j) = GetHtoS(H_new(j), type, C, W_C_ratio, Tk, day, rho_l, rho_c, alpha, w)
                w_new(j) = wsat * S_new(j) * phi
            Next

            'step 6: plot 2D image and export result .txt file 
            For i As Integer = 0 To frm.NElements - 1
                frm.Elements(i).HR(ti + 1) = (H_new(frm.Elements(i).Node1 - 1) + H_new(frm.Elements(i).Node2 - 1) + H_new(frm.Elements(i).Node3 - 1) + H_new(frm.Elements(i).Node4 - 1)) * 100 / 4
                frm.Elements(i).Sl(ti + 1) = (S_new(frm.Elements(i).Node1 - 1) + S_new(frm.Elements(i).Node2 - 1) + S_new(frm.Elements(i).Node3 - 1) + S_new(frm.Elements(i).Node4 - 1)) * 100 / 4
                frm.HRRange.AddValue(frm.Elements(i).HR(ti + 1))
                frm.SlRange.AddValue(frm.Elements(i).Sl(ti + 1))
            Next

            frm.Time(ti + 1) = (ti + 1) * dt / 3600 ' Time in hour

            Dim fieldHnew_average As Double = H_new.Average
            Dim fieldHold_average As Double = H_old.Average
            Dim fieldSnew_average As Double = S_new.Average
            Dim fieldSold_average As Double = S_old.Average
            Dim fieldwnew_average As Double = w_new.Average
            Dim fieldwold_average As Double = w_old.Average
            Dim dH_avg As Double
            dH_avg = fieldHnew_average - fieldHold_average + dH_avg
            Dim dw_avg As Double
            dw_avg = fieldwnew_average - fieldwold_average + dw_avg
            Dim dS_avg As Double
            dS_avg = fieldSnew_average - fieldSold_average + dS_avg

            If (ti * dt / T_sauv) = Int(ti * dt / T_sauv) And Int(ti * dt / T_sauv) > 0 Then ' check register time
                RegisterField(nFic1, ti * dt, nDof, dH_avg, H_new)
                PrintLine(CInt(nFic1), " ")
                RegisterField(nFic2, ti * dt, nDof, dw_avg, w_new)
                PrintLine(CInt(nFic2), " ")
                RegisterField(nfic3, ti * dt, nDof, dS_avg, S_new)
                PrintLine(CInt(nfic3), " ")
            End If
            H_old = H_new
            S_old = S_new
        Next

        FileClose(CInt(nFic1))
        FileClose(CInt(nFic2))
        FileClose(CInt(nfic3))

        MsgBox("End of 2D diffusion", MsgBoxStyle.OkOnly And MsgBoxStyle.Information, "End")
        frm.Analysed = True

        'Me.Invoke(Sub()
        frm.LabelProgress.Visible = False
        'End Sub)
    End Sub

    'Assembling global matrix /water diffusion
    Private Shared Sub AssembleKg(ByRef ke(,) As Double, ByRef Kg(,) As Double, ByRef Elements() As ElementTrans, ElementNo As Integer)
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

    'Enregistrement des données dans les fichiers d'output
    Private Shared Sub RegisterH(ByRef nFic1 As Short, ByRef Temps As Decimal, ByRef Dofs As Short, ByRef H_new() As Double)
        Dim j As Short
        'Register values
        Print(CInt(nFic1), Temps / 3600, ",", Temps, ",", TAB)
        For j = 0 To CShort(Dofs - 1)
            Print(CInt(nFic1), H_new(j), ",", TAB) '% humidité relative dans le béton
        Next j
    End Sub
    Private Sub RegisterField(ByRef nFic1 As Short, ByRef Temps As Double, ByRef Dofs As Integer, ByRef d_avg As Double, ByRef H_new() As Double)
        'Register field values
        Print(CInt(nFic1), Temps / 3600, ",", Temps, ",", TAB)
        'Dim avg_old As Double = H_old.Average()
        Dim avg_new As Double = H_new.Average()
        'Dim delta As Double = avg_new - avg_0
        Print(CInt(nFic1), avg_new, ",", d_avg, ",", TAB)
        For j As Integer = 0 To Dofs - 1
            Print(CInt(nFic1), H_new(j), ",", TAB) '% humidité relative dans le béton
        Next j
    End Sub
End Class
