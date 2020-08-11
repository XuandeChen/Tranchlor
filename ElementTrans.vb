' Xuande 10/06/2020 nouvelle class modifié à partir de frmFEM
Public Class ElementTrans
    Public Property ElementNumber As Integer
    Public Property Node1 As Integer
    Public Property Node2 As Integer
    Public Property Node3 As Integer
    Public Property Node4 As Integer

    'Public Time(100) As Double
    Public HR(100) As Double
    Public Sl(100) As Double

    'Public Stresses(1) As Double
    'Public Strains(1) As Double

    Public Sub New(n As Integer, n1 As Integer, n2 As Integer, n3 As Integer, n4 As Integer)
        _ElementNumber = n
        _Node1 = n1
        _Node2 = n2
        _Node3 = n3
        _Node4 = n4
    End Sub
End Class
