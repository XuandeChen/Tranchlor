
Imports System.Data.SqlClient

Public Class Compute2D

    Dim dt As Double 'time interval (s)
    Dim tmax As Double 'end time (s) 72h
    Dim T_sauv As Double
    Dim directory As String

    Dim H_int As Double  'initial relative humidity in the material
    Dim Tc As Double 'initial temperature in the material

    Dim D0 As Double
    Dim alpha_0 As Double
    Dim Hc As Double

    Dim Expo(1) As Exposition

    Public LgLim As Integer = 40
    Public pPH As Double = 12.6
    Public mPh As Double = 6.5
    Public RoW As Integer = 1000        'kg/m3
    Public R As Double = 8.3145        'J/mol.K

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

        'Input(nFic, taff)
        'Input(nFic, Hsauv)
        'Input(nFic, Wsauv)
        'Input(nFic, CTsauv)
        'Input(nFic, CLsauv)
        'Input(nFic, Tsauv)
        'Input(nFic, Carbsauv)
        'Input(nFic, hMin)
        'Input(nFic, hEcart)
        'Input(nFic, wMin)
        'Input(nFic, wEcart)
        'Input(nFic, CLmin)
        'Input(nFic, CLecart)
        'Input(nFic, CTmin)
        'Input(nFic, CTecart)
        'Input(nFic, Tecart)
        'Input(nFic, aa)
        'Input(nFic, Hc)
        'Input(nFic, ab)
        'Input(nFic, tc)
        'Input(nFic, ImpHydr)
        'Input(nFic, H_snap)
        'Input(nFic, Retard)
        'Input(nFic, aOH)
        'Input(nFic, EbG)
        'Input(nFic, toG)
        'Input(nFic, faG)
        'Input(nFic, capCal)
        'Input(nFic, GyCO2)
        'Input(nFic, DyCO2)

        'Input(nFic, NEXPO)
        'ReDim FileGexpo(NEXPO)
        'ReDim FileDexpo(NEXPO)
        'For i = CShort(1) To NEXPO
        '    Input(nFic, FileGexpo(i))
        '    Input(nFic, FileDexpo(i))
        'Next i

        'Input(nFic, NQUAL)
        'ReDim Filebeton(NQUAL)
        'ReDim Fileres(NQUAL)
        'ReDim PD(NQUAL)
        'ReDim Dcl(NQUAL)
        'ReDim qGran(NQUAL)
        'ReDim LambdaH(NQUAL)
        'ReDim LambdaT(NQUAL)
        'ReDim SAT(NQUAL)
        'ReDim ciment(NQUAL)
        'ReDim EC(NQUAL)
        'ReDim tProt(NQUAL)
        'ReDim Vct(NQUAL)
        'ReDim Nct(NQUAL)
        'ReDim Hydr(NQUAL)
        'ReDim ED(NQUAL)
        'ReDim ToHydr(NQUAL)
        'ReDim Ecl(NQUAL)
        'ReDim ToCl(NQUAL)
        'ReDim RoA(NQUAL)
        'ReDim RoC(NQUAL)
        'ReDim proba(NQUAL, 19)

        'For i = CShort(1) To NQUAL
        '    Input(nFic, Filebeton(i))
        '    Input(nFic, Fileres(i))
        '    Input(nFic, PD(i))
        '    Input(nFic, Dcl(i))
        '    Input(nFic, qGran(i))
        '    Input(nFic, LambdaH(i))
        '    Input(nFic, LambdaT(i))
        '    Input(nFic, SAT(i))
        '    Input(nFic, ciment(i))
        '    Input(nFic, EC(i))
        '    Input(nFic, tProt(i))
        '    Input(nFic, Hydr(i))
        '    Input(nFic, Vct(i))
        '    Input(nFic, Nct(i))
        '    Input(nFic, ED(i))
        '    Input(nFic, ToHydr(i))
        '    Input(nFic, Ecl(i))
        '    Input(nFic, ToCl(i))
        '    Input(nFic, RoA(i))
        '    Input(nFic, RoC(i))
        '    For j = 0 To 19
        '        Input(nFic, proba(i, j))
        '    Next
        'Next i

        'ReDim Creadtherm(1, 1)
        'ReDim Creadhydr(1, 1)
        'ReDim Creadion(1, 1)

        'Input(nFic, Nbre1)
        'ReDim Creadtherm(1, Nbre1)
        'For i = 1 To Nbre1
        '    Input(nFic, Creadtherm(0, i))
        '    Input(nFic, Creadtherm(1, i))
        'Next

        'Input(nFic, Nbre2)
        'ReDim Creadhydr(1, Nbre2)
        'For i = 1 To Nbre2
        '    Input(nFic, Creadhydr(0, i))
        '    Input(nFic, Creadhydr(1, i))
        '    Creadhydr(1, i) = Creadhydr(1, i) / 100
        'Next

        'Input(nFic, Nbre3)
        'ReDim Creadion(1, Nbre3)
        'For i = 1 To Nbre3
        '    Input(nFic, Creadion(0, i))
        '    Input(nFic, Creadion(1, i))
        'Next

        FileClose(nFic)

    End Sub

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

        'Dim Para1 As Single
        'Dim Para2 As Single
        'Dim Para3 As Single
        'Dim Para4 As Single
        'Dim test As Single

        Input(nFic, H_int)
        Input(nFic, Tc)

        'Input(nFic, taff)
        'Input(nFic, Hsauv)
        'Input(nFic, Wsauv)
        'Input(nFic, CTsauv)
        'Input(nFic, CLsauv)
        'Input(nFic, Tsauv)
        'Input(nFic, Carbsauv)
        'Input(nFic, hMin)
        'Input(nFic, hEcart)
        'Input(nFic, wMin)
        'Input(nFic, wEcart)
        'Input(nFic, CLmin)
        'Input(nFic, CLecart)
        'Input(nFic, CTmin)
        'Input(nFic, CTecart)
        'Input(nFic, Tecart)
        'Input(nFic, aa)
        'Input(nFic, Hc)
        'Input(nFic, ab)
        'Input(nFic, tc)
        'Input(nFic, ImpHydr)
        'Input(nFic, H_snap)
        'Input(nFic, Retard)
        'Input(nFic, aOH)
        'Input(nFic, EbG)
        'Input(nFic, toG)
        'Input(nFic, faG)
        'Input(nFic, capCal)
        'Input(nFic, GyCO2)
        'Input(nFic, DyCO2)

        'Input(nFic, NEXPO)
        'ReDim FileGexpo(NEXPO)
        'ReDim FileDexpo(NEXPO)
        'For i = CShort(1) To NEXPO
        '    Input(nFic, FileGexpo(i))
        '    Input(nFic, FileDexpo(i))
        'Next i

        'Input(nFic, NQUAL)
        'ReDim Filebeton(NQUAL)
        'ReDim Fileres(NQUAL)
        'ReDim PD(NQUAL)
        'ReDim Dcl(NQUAL)
        'ReDim qGran(NQUAL)
        'ReDim LambdaH(NQUAL)
        'ReDim LambdaT(NQUAL)
        'ReDim SAT(NQUAL)
        'ReDim ciment(NQUAL)
        'ReDim EC(NQUAL)
        'ReDim tProt(NQUAL)
        'ReDim Vct(NQUAL)
        'ReDim Nct(NQUAL)
        'ReDim Hydr(NQUAL)
        'ReDim ED(NQUAL)
        'ReDim ToHydr(NQUAL)
        'ReDim Ecl(NQUAL)
        'ReDim ToCl(NQUAL)
        'ReDim RoA(NQUAL)
        'ReDim RoC(NQUAL)
        'ReDim proba(NQUAL, 19)

        'For i = CShort(1) To NQUAL
        '    Input(nFic, Filebeton(i))
        '    Input(nFic, Fileres(i))
        '    Input(nFic, PD(i))
        '    Input(nFic, Dcl(i))
        '    Input(nFic, qGran(i))
        '    Input(nFic, LambdaH(i))
        '    Input(nFic, LambdaT(i))
        '    Input(nFic, SAT(i))
        '    Input(nFic, ciment(i))
        '    Input(nFic, EC(i))
        '    Input(nFic, tProt(i))
        '    Input(nFic, Hydr(i))
        '    Input(nFic, Vct(i))
        '    Input(nFic, Nct(i))
        '    Input(nFic, ED(i))
        '    Input(nFic, ToHydr(i))
        '    Input(nFic, Ecl(i))
        '    Input(nFic, ToCl(i))
        '    Input(nFic, RoA(i))
        '    Input(nFic, RoC(i))
        '    For j = 0 To 19
        '        Input(nFic, proba(i, j))
        '    Next
        'Next i

        'ReDim Creadtherm(1, 1)
        'ReDim Creadhydr(1, 1)
        'ReDim Creadion(1, 1)

        'Input(nFic, Nbre1)
        'ReDim Creadtherm(1, Nbre1)
        'For i = 1 To Nbre1
        '    Input(nFic, Creadtherm(0, i))
        '    Input(nFic, Creadtherm(1, i))
        'Next

        'Input(nFic, Nbre2)
        'ReDim Creadhydr(1, Nbre2)
        'For i = 1 To Nbre2
        '    Input(nFic, Creadhydr(0, i))
        '    Input(nFic, Creadhydr(1, i))
        '    Creadhydr(1, i) = Creadhydr(1, i) / 100
        'Next

        'Input(nFic, Nbre3)
        'ReDim Creadion(1, Nbre3)
        'For i = 1 To Nbre3
        '    Input(nFic, Creadion(0, i))
        '    Input(nFic, Creadion(1, i))
        'Next

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

        'Input(nFic, taff)
        'Input(nFic, Hsauv)
        'Input(nFic, Wsauv)
        'Input(nFic, CTsauv)
        'Input(nFic, CLsauv)
        'Input(nFic, Tsauv)
        'Input(nFic, Carbsauv)
        'Input(nFic, hMin)
        'Input(nFic, hEcart)
        'Input(nFic, wMin)
        'Input(nFic, wEcart)
        'Input(nFic, CLmin)
        'Input(nFic, CLecart)
        'Input(nFic, CTmin)
        'Input(nFic, CTecart)
        'Input(nFic, Tecart)
        'Input(nFic, aa)
        'Input(nFic, Hc)
        'Input(nFic, ab)
        'Input(nFic, tc)
        'Input(nFic, ImpHydr)
        'Input(nFic, H_snap)
        'Input(nFic, Retard)
        'Input(nFic, aOH)
        'Input(nFic, EbG)
        'Input(nFic, toG)
        'Input(nFic, faG)
        'Input(nFic, capCal)
        'Input(nFic, GyCO2)
        'Input(nFic, DyCO2)

        'Input(nFic, NEXPO)
        'ReDim FileGexpo(NEXPO)
        'ReDim FileDexpo(NEXPO)
        'For i = CShort(1) To NEXPO
        '    Input(nFic, FileGexpo(i))
        '    Input(nFic, FileDexpo(i))
        'Next i

        'Input(nFic, NQUAL)
        'ReDim Filebeton(NQUAL)
        'ReDim Fileres(NQUAL)
        'ReDim PD(NQUAL)
        'ReDim Dcl(NQUAL)
        'ReDim qGran(NQUAL)
        'ReDim LambdaH(NQUAL)
        'ReDim LambdaT(NQUAL)
        'ReDim SAT(NQUAL)
        'ReDim ciment(NQUAL)
        'ReDim EC(NQUAL)
        'ReDim tProt(NQUAL)
        'ReDim Vct(NQUAL)
        'ReDim Nct(NQUAL)
        'ReDim Hydr(NQUAL)
        'ReDim ED(NQUAL)
        'ReDim ToHydr(NQUAL)
        'ReDim Ecl(NQUAL)
        'ReDim ToCl(NQUAL)
        'ReDim RoA(NQUAL)
        'ReDim RoC(NQUAL)
        'ReDim proba(NQUAL, 19)

        'For i = CShort(1) To NQUAL
        '    Input(nFic, Filebeton(i))
        '    Input(nFic, Fileres(i))
        '    Input(nFic, PD(i))
        '    Input(nFic, Dcl(i))
        '    Input(nFic, qGran(i))
        '    Input(nFic, LambdaH(i))
        '    Input(nFic, LambdaT(i))
        '    Input(nFic, SAT(i))
        '    Input(nFic, ciment(i))
        '    Input(nFic, EC(i))
        '    Input(nFic, tProt(i))
        '    Input(nFic, Hydr(i))
        '    Input(nFic, Vct(i))
        '    Input(nFic, Nct(i))
        '    Input(nFic, ED(i))
        '    Input(nFic, ToHydr(i))
        '    Input(nFic, Ecl(i))
        '    Input(nFic, ToCl(i))
        '    Input(nFic, RoA(i))
        '    Input(nFic, RoC(i))
        '    For j = 0 To 19
        '        Input(nFic, proba(i, j))
        '    Next
        'Next i

        'ReDim Creadtherm(1, 1)
        'ReDim Creadhydr(1, 1)
        'ReDim Creadion(1, 1)

        'Input(nFic, Nbre1)
        'ReDim Creadtherm(1, Nbre1)
        'For i = 1 To Nbre1
        '    Input(nFic, Creadtherm(0, i))
        '    Input(nFic, Creadtherm(1, i))
        'Next

        'Input(nFic, Nbre2)
        'ReDim Creadhydr(1, Nbre2)
        'For i = 1 To Nbre2
        '    Input(nFic, Creadhydr(0, i))
        '    Input(nFic, Creadhydr(1, i))
        '    Creadhydr(1, i) = Creadhydr(1, i) / 100
        'Next

        'Input(nFic, Nbre3)
        'ReDim Creadion(1, Nbre3)
        'For i = 1 To Nbre3
        '    Input(nFic, Creadion(0, i))
        '    Input(nFic, Creadion(1, i))
        'Next

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

    Public Sub InitialConditions()

        'Dofs = Ne + CShort(1)

        ''calcul des conditions initiales
        'Dim i As Short
        'Dim j As Short = 0
        'ReDim Ctherm(Dofs + 1)
        'Dim Var, Var1 As Double

        'For i = 1 To Nbre1 - 1
        '    Var = (Creadtherm(1, i + 1) - Creadtherm(1, i)) / (Creadtherm(0, i + 1) - Creadtherm(0, i))
        '    Var1 = Creadtherm(1, i) - Var * Creadtherm(0, i)
        '    Do While PosProf(j) <= Creadtherm(0, i + 1)
        '        Ctherm(j) = Var * PosProf(j) + Var1
        '        j = j + 1
        '        If j > Dofs Then Exit Do
        '    Loop
        '    Ctherm(Dofs + 1) = Ctherm(Dofs)
        'Next
        'j = 0

        'ReDim Chydr(Dofs + 1)
        'For i = 1 To Nbre2 - 1
        '    Var = (Creadhydr(1, i + 1) - Creadhydr(1, i)) / (Creadhydr(0, i + 1) - Creadhydr(0, i))
        '    Var1 = Creadhydr(1, i) - Var * Creadhydr(0, i)
        '    Do While PosProf(j) <= Creadhydr(0, i + 1)
        '        Chydr(j) = Var * PosProf(j) + Var1
        '        j = j + 1
        '        If j > Dofs Then Exit Do
        '    Loop
        '    Chydr(Dofs + 1) = Chydr(Dofs)
        '    If j > Dofs + 1 Then Exit For
        'Next
        'j = 0

        'ReDim Cion(Dofs + 1)
        'For i = 1 To Nbre3 - 1
        '    Var = (Creadion(1, i + 1) - Creadion(1, i)) / (Creadion(0, i + 1) - Creadion(0, i))
        '    Var1 = Creadion(1, i) - Var * Creadion(0, i)
        '    Do While PosProf(j) <= Creadion(0, i + 1)
        '        Cion(j) = Var * PosProf(j) + Var1
        '        j = j + 1
        '        If j > Dofs Then Exit Do
        '    Loop
        '    Cion(Dofs + 1) = Cion(Dofs)
        'Next

        'Le(0) = CDec(1)       'couche limite
        'Le(Dofs) = CDec(1)    'couche limite

    End Sub

    'Coeur du calcul
    Public Sub Compute_All(ByRef frm As frmTrans2D)

        Dim nFic1 As Short
        Dim outfile(1) As String
        Dim nDof As Integer = frm.NNodes

        'step 0: Creating output .txt computation result file 2020-07-17 Xuande 
        outfile(1) = directory & "\" & "R_H" & ".txt"
        nFic1 = CShort(FreeFile())
        FileOpen(CInt(nFic1), outfile(1), OpenMode.Output)

        'step 0 Initialize output titres for result .txt files
        Print(nFic1, "RH", ",", nDof, ",", TAB)
        For jj As Integer = 0 To nDof - 1
            Print(CInt(nFic1), jj + CShort(1), ",", TAB)
        Next jj
        PrintLine(CInt(nFic1), " ")

        Dim ti As Integer
        Dim ind As Double = tmax / dt

        Dim H_old(nDof - 1) As Double
        Dim H_new(nDof - 1) As Double
        Dim H_mat(ind, nDof - 1) As Double 'Matrix for stockage of computation results (days, number of nodes)
        Dim T_old(nDof - 1) As Double

        Dim T(ind) As Double 'time vector (days)

        frm.HRRange = New Range

        For i As Integer = 0 To frm.NElements - 1
            ReDim frm.Elements(i).HR(ind + 1)
            frm.Elements(i).HR(0) = H_int * 100
            frm.HRRange.AddValue(frm.Elements(i).HR(0))
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
                For i As Integer = 0 To nDof - 1

                    H_old(i) = H_int

                Next

                ' boundary check program, 2020.08.03
                Print(CInt(nFic1), "0", ",", "0", ",", TAB)
                Dim i_node As Integer
                Dim iT As Integer = 0

                For i_node = 0 To nDof - 1
                    If frm.Nodes(i_node).Bord = True Then

                        ' check whether the current boundary is exposed to a boundary condition
                        Dim X_node As Double = frm.Nodes(i_node).x
                        Dim Y_node As Double = frm.Nodes(i_node).y

                        Dim NbExpo As Integer = frm.Nodes(i_node).NumExpo

                        If NbExpo <> 0 Then

                            H_old(i_node) = Expo(NbExpo - 1).Humidite(ti)

                        End If

                        'If Math.Abs(X_node - X_lower) <= 0.00001 And Expo_X_lower = True Then
                        '    H_old(i_node) = Expo(1).
                        'ElseIf Math.Abs(X_node - X_upper) <= 0.00001 And Expo_X_upper = True Then
                        '    H_old(i_node) = H_bound
                        'ElseIf Math.Abs(Y_node - Y_lower) <= 0.00001 And Expo_Y_lower = True Then
                        '    H_old(i_node) = H_bound
                        'ElseIf Math.Abs(Y_node - Y_upper) <= 0.00001 And Expo_Y_upper = True Then
                        '    H_old(i_node) = H_bound
                        'End If

                        'H_old(i_node) = H_bound
                    End If

                    Print(CInt(nFic1), H_old(i_node), ",", TAB)

                Next

                PrintLine(CInt(nFic1), " ")
            Else

                For i_node As Integer = 0 To nDof - 1
                    H_old(i_node) = H_new(i_node)
                    ' boundary check program, 2020.08.03
                    Dim NbExpo As Integer = frm.Nodes(i_node).NumExpo

                    If NbExpo <> 0 Then

                        H_old(i_node) = Expo(NbExpo - 1).Humidite(ti)

                    End If

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
            For i As Integer = 0 To frm.NElements - 1
                he = New HETrans(
                          H_old(frm.Elements(i).Node1 - 1), H_old(frm.Elements(i).Node2 - 1),
                          H_old(frm.Elements(i).Node3 - 1), H_old(frm.Elements(i).Node4 - 1)
                          )
                H_ele = he.getHe
                H_avg = GetAvgH(H_ele)
                De = GetDh(D0, alpha_0, Hc, Tc, H_avg)
                cie = New CIETrans(
                          frm.Nodes(frm.Elements(i).Node1 - 1).x, frm.Nodes(frm.Elements(i).Node1 - 1).y,
                          frm.Nodes(frm.Elements(i).Node2 - 1).x, frm.Nodes(frm.Elements(i).Node2 - 1).y,
                          frm.Nodes(frm.Elements(i).Node3 - 1).x, frm.Nodes(frm.Elements(i).Node3 - 1).y,
                          frm.Nodes(frm.Elements(i).Node4 - 1).x, frm.Nodes(frm.Elements(i).Node4 - 1).y,
                          De)
                AssembleKg(cie.getbe, bg, frm.Elements, i)
                AssembleKg(cie.getAe, Ag, frm.Elements, i)
            Next

            'step 3: now, we have assembled Hg_old, Ag and bg , to get LHS and RHS
            getLHS(LHS, frm.NNodes, Ag, bg, dt)
            getRHS(R, frm.NNodes, Ag, bg, dt)
            RHS = MultiplyMatrixWithVector(R, H_old)

            'step 4: now with LHS*x = RHS, using Gauss Elimination we can get the resolution for the new field of humidity Hnew
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

                H_mat(ti, j) = H_new(j)
            Next

            'step 6: plot 2D image and export result .txt file 
            For i As Integer = 0 To frm.NElements - 1

                frm.Elements(i).HR(ti + 1) = (H_new(frm.Elements(i).Node1 - 1) + H_new(frm.Elements(i).Node2 - 1) + H_new(frm.Elements(i).Node3 - 1) + H_new(frm.Elements(i).Node4 - 1)) * 100 / 4
                frm.HRRange.AddValue(frm.Elements(i).HR(ti + 1))

            Next
            frm.Time(ti + 1) = (ti + 1) * dt / 3600 ' Time in hour

            If ti = 0 Then
                RegisterH(nFic1, dt, nDof, H_new)
                PrintLine(CInt(nFic1), " ")
            ElseIf (ti * dt / T_sauv) = Int(ti * dt / T_sauv) Then ' check register time
                RegisterH(nFic1, ti * dt, nDof, H_new)
                PrintLine(CInt(nFic1), " ")
            End If

        Next

        FileClose(CInt(nFic1))
        MsgBox("Fin du calcul diffusion 2D", MsgBoxStyle.OkOnly And MsgBoxStyle.Information, "End")
        frm.Analysed = True


        'Me.Invoke(Sub()
        frm.LabelProgress.Visible = False
        'End Sub)

    End Sub

    'Assembling global matrix /water diffusion
    Private Sub AssembleKg(ByRef ke(,) As Double, ByRef Kg(,) As Double, ByRef Elements() As ElementTrans, ElementNo As Integer)
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
    Private Sub RegisterH(ByRef nFic1 As Short, ByRef Temps As Decimal, ByRef Dofs As Short, ByRef H_new() As Double)
        Dim j As Short
        'Register values
        Print(CInt(nFic1), Temps / 3600, ",", Temps, ",", TAB)
        For j = 0 To Dofs - 1
            Print(CInt(nFic1), H_new(j), ",", TAB) '% humidité relative dans le béton
        Next j
    End Sub


    'Enregistrement des données dans les fichiers d'output
    'Private Sub Register(ByRef nFic1 As Short, ByRef Tijd As Decimal, ByRef Dofs As Short, ByRef H_new() As Decimal, ByRef para As Single)

    '    Dim j As Short
    '    'Register values
    '    Print(CInt(nFic1), Tijd / 365, ",", Tijd, ",", TAB)
    '    For j = CShort(1) To Dofs
    '        Print(CInt(nFic1), H_new(j) + para, ",", TAB) '% humidité relative dans le béton
    '    Next j

    'End Sub

    ''Enregistrement des données dans le fichier d'output pour l'humidité relative
    'Private Sub Regist01(ByRef nFic1 As Short, ByRef Tijd As Decimal, ByRef Dofs As Short, ByRef CL_new() As Decimal, ByRef W() As Decimal, ByRef Ciment As Single, ByRef Para As Short)

    '    Dim j As Short
    '    'Register values
    '    Print(CInt(nFic1), Tijd / 365, ",", Tijd, ",", TAB)
    '    For j = CShort(1) To Dofs
    '        If Para = 3 Then
    '            Print(CInt(nFic1), CL_new(j) * W(j) / 1000, ",", TAB)
    '        Else        'avec traitement probabiliste
    '            Print(CInt(nFic1), CL_new(j) * W(j) / (10 * Ciment), ",", TAB)
    '        End If
    '    Next j

    'End Sub

End Class
