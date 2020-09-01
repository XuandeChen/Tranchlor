' Xuande 10/06/2020 nouvelle class modifié à partir de frmFEM
Public Class NodeTrans

    Public Property NodeNumber As Integer
    Public Property x As Double
    Public Property y As Double
    Public Property z As Double

    Public Property NumExpo As Integer

    Private HR(1) As Double
    Private Sl(1) As Double
    Private Wl(1) As Double

    Public Sub New(nn As Integer, x As Double, y As Double, z As Double, NumExpo As Integer)

        _NodeNumber = nn
        _x = x
        _y = y
        _z = z
        _NumExpo = NumExpo

    End Sub

    Public Sub SetFieldsNew(ByRef HRval As Double, ByRef Slval As Double, ByRef Wlval As Double)

        HR(1) = HRval
        Sl(1) = Slval
        Wl(1) = Wlval

    End Sub

    Public Sub SetFieldsOld(ByRef HRval As Double, ByRef Slval As Double, ByRef Wlval As Double)

        HR(0) = HRval
        Sl(0) = Slval
        Wl(0) = Wlval

    End Sub

    Public Sub SetFieldsNewToOld()

        HR(0) = HR(1)
        Sl(0) = Sl(1)
        Wl(0) = Wl(1)

    End Sub

    Public Function GetHROld() As Double

        Return HR(0)

    End Function

    Public Function GetSOld() As Double

        Return Sl(0)

    End Function

    Public Function GetWOld() As Double

        Return Wl(0)

    End Function

    Public Function GetHRNew() As Double

        Return HR(1)

    End Function

    Public Function GetSNew() As Double

        Return Sl(1)

    End Function

    Public Function GetWNew() As Double

        Return Wl(1)

    End Function

End Class
