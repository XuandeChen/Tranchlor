' Xuande 10/06/2020 nouvelle class modifié à partir de frmFEM
Public Class NodeTrans

    Public Property NodeNumber As Integer
    Public Property x As Double
    Public Property y As Double
    Public Property z As Double

    Public Property Bord As Boolean
    Public Property NumExpo As Integer

    Public Sub New(nn As Integer, x As Double, y As Double, z As Double, brd As Boolean)
        _NodeNumber = nn
        _x = x
        _y = y
        _z = z
        _Bord = brd
    End Sub



End Class
