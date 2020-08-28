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


    Public Sub WriteField(ByRef i As Integer, ByRef Temps As Double, ByRef Dofs As Integer,
                           ByRef d_avg As Double, ByRef Field() As Double)

        'Register field values
        Print(CInt(nFic(i)), Temps / 3600, ",", Temps, ",", TAB)

        Dim avg_new As Double = Field.Average()

        Print(CInt(nFic(i)), avg_new, ",", d_avg, ",", TAB)

        For j As Integer = 0 To Dofs - 1
            Print(CInt(nFic(i)), Field(j), ",", TAB) '% humidité relative dans le béton
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
