' Xuande 10/06/2020 nouvelle class modifiée à partir de frmFEM
Public Class NodeTrans

    Public Property NodeNumber As Integer
    Public Property x As Double
    Public Property y As Double
    Public Property z As Double

    Public Property NumExpo As Integer

    Private HR(1) As Double
    Private Sl(1) As Double
    Private Wl(1) As Double
    Private T(1) As Double 'temperature
    Private Cl(1) As Double 'concentration de chlore
    Private Node_w(1) As Double ' isotherm indicator
    Public Sub New(nn As Integer, x As Double, y As Double, z As Double, NumExpo As Integer)

        _NodeNumber = nn
        _x = x
        _y = y
        _z = z
        _NumExpo = NumExpo

    End Sub

    Public Sub SetFieldsNew(ByRef HRval As Double, ByRef Slval As Double, ByRef Wlval As Double, ByRef Tval As Double, ByRef Clval As Double)

        HR(1) = HRval
        Sl(1) = Slval
        Wl(1) = Wlval
        T(1) = Tval
        Cl(1) = Clval
    End Sub

    Public Sub SetFieldsOld(ByRef HRval As Double, ByRef Slval As Double, ByRef Wlval As Double, ByRef Tval As Double, ByRef Clval As Double)

        HR(0) = HRval
        Sl(0) = Slval
        Wl(0) = Wlval
        T(0) = Tval
        Cl(0) = Clval
    End Sub

    Public Sub SetFieldsNewToOld()

        HR(0) = HR(1)
        Sl(0) = Sl(1)
        Wl(0) = Wl(1)
        T(0) = T(1)
        Cl(0) = Cl(1)
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
    Public Function GetTOld() As Double

        Return T(0)

    End Function

    Public Function GetClOld() As Double

        Return Cl(0)

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
    Public Function GetTNew() As Double

        Return T(1)

    End Function
    Public Function GetClNew() As Double

        Return Cl(1)

    End Function
End Class
