' Xuande 10/06/2020 nouvelle class modifiée à partir de frmFEM
Public Class NodeTrans

    Public Property NodeNumber As Integer
    Public Property x As Double
    Public Property y As Double
    Public Property z As Double

    Public Property NumExpo As Integer
    Public Property TypeExpo As String 'Xuande 2020.10.27

    Private HR(1) As Double
    Private Sl(1) As Double
    Private Wl(1) As Double
    Private T(1) As Double 'temperature
    Private Cl(1) As Double 'concentration de chlore
    Private Node_w(1) As Double ' isotherm indicator
    Private Jx_cap(1) As Double 'xuande, flux de capillarite direction x
    Private Jy_cap(1) As Double 'xuande, flux de capillarite direction y
    Private Jx_dif(1) As Double 'xuande, flux de diffusion hydrique direction x
    Private Jy_dif(1) As Double 'xuande, flux de diffusion hydrique direction y
    Private Jx_thm(1) As Double 'xuande, flux de diffusion thermique direction x
    Private Jy_thm(1) As Double 'xuande, flux de diffusion thermique direction y
    Private Jx_ion(1) As Double 'xuande, flux ionique direction x
    Private Jy_ion(1) As Double 'xuande, flux ionique direction y
    Public Sub New(nn As Integer, x As Double, y As Double, z As Double, NumExpo As Integer, TypeExpo As String)

        _NodeNumber = nn
        _x = x
        _y = y
        _z = z
        _NumExpo = NumExpo
        _TypeExpo = TypeExpo

    End Sub

    Public Sub SetFieldsNew(ByRef HRval As Double, ByRef Slval As Double, ByRef Wlval As Double, ByRef Tval As Double, ByRef Clval As Double)

        HR(1) = HRval
        Sl(1) = Slval
        Wl(1) = Wlval
        T(1) = Tval
        Cl(1) = Clval

    End Sub
    'isotherm indicator , xuande 2020.08.28
    Public Sub SetIndicatorNew(ByRef w_val As Double)

        Node_w(1) = w_val

    End Sub
    'flux , xuande 2020.08.28
    Public Sub SetThermoFluxNew(ByRef Jx_val As Double, ByRef Jy_val As Double)

        Jx_thm(1) = Jx_val
        Jy_thm(1) = Jy_val

    End Sub
    Public Sub SetCapillaryFluxNew(ByRef Jx_val As Double, ByRef Jy_val As Double)

        Jx_cap(1) = Jx_val
        Jy_cap(1) = Jy_val

    End Sub
    Public Sub SetDiffusionFluxNew(ByRef Jx_val As Double, ByRef Jy_val As Double)

        Jx_dif(1) = Jx_val
        Jy_dif(1) = Jy_val

    End Sub
    Public Sub SetIonicFluxNew(ByRef Jx_val As Double, ByRef Jy_val As Double)

        Jx_ion(1) = Jx_val
        Jy_ion(1) = Jy_val

    End Sub
    Public Sub SetFieldsOld(ByRef HRval As Double, ByRef Slval As Double, ByRef Wlval As Double, ByRef Tval As Double, ByRef Clval As Double)

        HR(0) = HRval
        Sl(0) = Slval
        Wl(0) = Wlval
        T(0) = Tval
        Cl(0) = Clval

    End Sub
    'isotherm indicator , xuande 2020.08.28
    Public Sub SetIndicatorOld(ByRef w_val As Double)

        Node_w(0) = w_val

    End Sub
    'flux , xuande 2020.08.28
    Public Sub SetThermoFluxOld(ByRef Jx_val As Double, ByRef Jy_val As Double)

        Jx_thm(0) = Jx_val
        Jy_thm(0) = Jy_val

    End Sub
    Public Sub SetCapillaryFluxOld(ByRef Jx_val As Double, ByRef Jy_val As Double)

        Jx_cap(0) = Jx_val
        Jy_cap(0) = Jy_val

    End Sub
    Public Sub SetDiffusionFluxOld(ByRef Jx_val As Double, ByRef Jy_val As Double)

        Jx_dif(0) = Jx_val
        Jy_dif(0) = Jy_val

    End Sub
    Public Sub SetIonicFluxOld(ByRef Jx_val As Double, ByRef Jy_val As Double)

        Jx_ion(0) = Jx_val
        Jy_ion(0) = Jy_val

    End Sub
    Public Sub SetFieldsNewToOld()

        HR(0) = HR(1)
        Sl(0) = Sl(1)
        Wl(0) = Wl(1)
        T(0) = T(1)
        Cl(0) = Cl(1)

    End Sub
    'isotherm indicator , xuande 2020.08.28
    Public Sub SetIndicatorNewToOld()

        Node_w(0) = Node_w(1)

    End Sub

    'flux , xuande 2020.08.28
    Public Sub SetThermoFluxNewToOld()

        Jx_thm(0) = Jx_thm(1)
        Jy_thm(0) = Jy_thm(1)

    End Sub
    Public Sub SetCapillaryFluxNewToOld()

        Jx_cap(0) = Jx_cap(1)
        Jy_cap(0) = Jy_cap(1)

    End Sub
    Public Sub SetDiffusionFluxNewToOld()

        Jx_dif(0) = Jx_dif(1)
        Jy_dif(0) = Jy_dif(1)

    End Sub
    Public Sub SetIonicFluxNewToOld()

        Jx_ion(0) = Jx_ion(1)
        Jy_ion(0) = Jy_ion(1)

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
    'flux , xuande 2020.08.28
    Public Function GetThermoFluxXOld() As Double

        Return Jx_thm(0)

    End Function
    Public Function GetThermoFluxYOld() As Double

        Return Jy_thm(0)

    End Function
    Public Function GetCapillaryFluxXOld() As Double

        Return Jx_cap(0)

    End Function
    Public Function GetCapillaryFluxYOld() As Double

        Return Jy_cap(0)

    End Function
    Public Function GetDiffusionFluxXOld() As Double

        Return Jx_dif(0)

    End Function
    Public Function GetDiffusionFluxYOld() As Double

        Return Jy_dif(0)

    End Function
    Public Function GetIonicFluxXOld() As Double

        Return Jx_ion(0)

    End Function
    Public Function GetIonicFluxYOld() As Double

        Return Jy_ion(0)

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

    'flux , xuande 2020.08.28
    Public Function GetThermoFluxXNew() As Double

        Return Jx_thm(1)

    End Function
    Public Function GetThermoFluxYNew() As Double

        Return Jy_thm(1)

    End Function
    Public Function GetCapillaryFluxXNew() As Double

        Return Jx_cap(1)

    End Function
    Public Function GetCapillaryFluxYNew() As Double

        Return Jy_cap(1)

    End Function
    Public Function GetDiffusionFluxXNew() As Double

        Return Jx_dif(1)

    End Function
    Public Function GetDiffusionFluxYNew() As Double

        Return Jy_dif(1)

    End Function
    Public Function GetIonicFluxXNew() As Double

        Return Jx_ion(1)

    End Function
    Public Function GetIonicFluxYNew() As Double

        Return Jy_ion(1)

    End Function
End Class
