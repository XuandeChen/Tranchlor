Imports System.Linq

Public Class OutputFile2D

    Dim NbFiles As Single
    Dim outfile() As String
    Dim nFic() As Short

    Public Sub New(ByRef directory As String, ByRef NbFiles As Single, ByRef nDof As Integer)

        ReDim outfile(NbFiles - 1)
        ReDim nFic(NbFiles - 1)

        ''Creating output .txt computation result file 2020-07-17 Xuande 
        outfile(0) = directory & "\" & "R_H_DiffusionModel" & ".txt"
        outfile(1) = directory & "\" & "R_W_DiffusionModel" & ".txt"
        outfile(2) = directory & "\" & "R_S_DiffusionModel" & ".txt"

        For i As Integer = 0 To NbFiles - 1
            nFic(i) = CShort(FreeFile())
            FileOpen(CInt(nFic(i)), outfile(i), OpenMode.Output)
        Next

        Print(nFic(0), "RH", ",", nDof, ",", "Average RH", ",", "dH", ",", TAB)
        Print(nFic(1), "W", ",", nDof, ",", "Average W", ",", "dW", ",", TAB)
        Print(nFic(2), "S", ",", nDof, ",", "Average S", ",", "dS", ",", TAB)

        For i As Integer = 0 To NbFiles - 1
            For jj As Integer = 0 To nDof - 1
                Print(CInt(nFic(i)), jj + CShort(1), ",", TAB)
            Next jj
            PrintLine(CInt(nFic(i)), " ")
            Print(CInt(nFic(i)), "0", ",", "0", ",", TAB)
        Next

    End Sub

    Public Sub WriteLine(ByRef H As Double, ByRef w As Double, ByRef S As Double)

        Print(CInt(nFic(0)), H, ",", "0", ",", TAB)
        Print(CInt(nFic(1)), w, ",", "0", ",", TAB)
        Print(CInt(nFic(2)), S, ",", "0", ",", TAB)

    End Sub

    Public Sub WriteFirstLine(ByRef H As Double, ByRef w As Double, ByRef S As Double)

        Print(CInt(nFic(0)), H, ",", TAB)
        Print(CInt(nFic(1)), w, ",", TAB)
        Print(CInt(nFic(2)), S, ",", TAB)

    End Sub

    Public Sub WriteBlankLine()

        For i As Integer = 0 To NbFiles - 1
            PrintLine(CInt(nFic(i)), " ")
        Next

    End Sub


    Public Sub WriteHR(ByRef Temps As Double, ByRef Dofs As Integer,
                           ByRef d_avg As Double, ByRef avg As Double, ByRef Nodes() As NodeTrans)

        Dim i As Integer = 0

        'Register field values
        Print(CInt(nFic(i)), Temps / 3600, ",", Temps, ",", TAB)
        Print(CInt(nFic(i)), avg, ",", d_avg, ",", TAB)

        For j As Integer = 0 To Dofs - 1
            Print(CInt(nFic(i)), Nodes(j).GetHRNew(), ",", TAB) '% humidité relative dans le béton
        Next j

        PrintLine(CInt(nFic(i)), " ")

    End Sub

    Public Sub WriteW(ByRef Temps As Double, ByRef Dofs As Integer,
                           ByRef d_avg As Double, ByRef avg As Double, ByRef Nodes() As NodeTrans)

        Dim i As Integer = 1

        'Register field values
        Print(CInt(nFic(i)), Temps / 3600, ",", Temps, ",", TAB)
        Print(CInt(nFic(i)), avg, ",", d_avg, ",", TAB)

        For j As Integer = 0 To Dofs - 1
            Print(CInt(nFic(i)), Nodes(j).GetWNew(), ",", TAB) '% humidité relative dans le béton
        Next j

        PrintLine(CInt(nFic(i)), " ")

    End Sub

    Public Sub WriteS(ByRef Temps As Double, ByRef Dofs As Integer,
                           ByRef d_avg As Double, ByRef avg As Double, ByRef Nodes() As NodeTrans)

        Dim i As Integer = 2

        'Register field values
        Print(CInt(nFic(i)), Temps / 3600, ",", Temps, ",", TAB)
        Print(CInt(nFic(i)), avg, ",", d_avg, ",", TAB)

        For j As Integer = 0 To Dofs - 1
            Print(CInt(nFic(i)), Nodes(j).GetSNew(), ",", TAB) '% humidité relative dans le béton
        Next j

        PrintLine(CInt(nFic(i)), " ")

    End Sub


    Protected Overrides Sub Finalize()

        For i As Integer = 0 To NbFiles - 1
            FileClose(CInt(nFic(i)))
        Next

        MyBase.Finalize()

    End Sub
End Class
