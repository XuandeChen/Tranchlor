Public Class Exposition

    Public Property Humidite() As Double()
    Public Property Sel() As Double()
    Public Property Temperature() As Double()

    Public Sub New(ByRef FileExpo As String)

        Dim nFic As Short = FreeFile()
        FileOpen(CInt(nFic), FileExpo, OpenMode.Input, OpenAccess.Read)

        Dim TempMin As Single = 40
        Dim TempMax As Single = -30
        Dim TempEcart As Single = 0

        Dim Dt As Double

        Dim NbreEn As Integer
        Input(CInt(nFic), NbreEn)

        Input(CInt(nFic), Dt)

        ReDim Humidite(NbreEn)
        ReDim Sel(NbreEn)
        ReDim Temperature(NbreEn)
        For j As Integer = 0 To NbreEn - 1

            Input(CInt(nFic), Humidite(j))
            Humidite(j) /= 100

            Input(CInt(nFic), Sel(j))
            Input(CInt(nFic), Temperature(j))
            If Temperature(CLng(j)) > TempMax Then TempMax = Temperature(CLng(j))
            If Temperature(CLng(j)) < TempMin Then TempMin = Temperature(CLng(j))

            Sel(j) = Sel(j) * 35.453 / 58.443    'calcul de cT à partir de co à multiplier par w(0) ou (dofs)
            'If Msel < GSel(j) * Wsat Then Msel = GSel(j) * Wsat

        Next j

        FileClose(CInt(nFic))

    End Sub

End Class
