Imports Microsoft.VisualBasic.Logging
Imports Microsoft.Win32
Imports System.Data.SqlClient

Public Class DBconnexion

    Public Connexion As New SqlConnection
    Public Command As New SqlCommand
    Public user As String

    'Public Sub UserLog()

    '    Using frm As New FrmLogin
    '        frm.ShowDialog()
    '    End Using

    'End Sub

    Public Sub VerifyConnexion()

        If Connexion.State = ConnectionState.Open Then
            Connexion.Close()
        End If

        Try
            Connexion.Open()
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

    End Sub

    Public Sub DBRequest(ByRef Request As String)

        Command.Connection = Connexion
        Command.CommandText = Request
        Command.ExecuteNonQuery()

    End Sub

    Public Sub MatFill(ByRef Mat As MaterialsData, ByRef Table As String)

        Dim DAdapter As New SqlDataAdapter(Command)
        DAdapter.Fill(Mat, Table)

    End Sub

    Public Sub DBUpdate(ByRef Mat As MaterialsData, ByRef Table As String)

        Dim DAdapter As New SqlDataAdapter(Command)
        Dim CmdBuilder As New SqlCommandBuilder(DAdapter)
        DAdapter.Update(Mat, Table)

    End Sub

    Public Sub DBRead(ByRef Value As Double)

        Dim reader As SqlDataReader = Command.ExecuteReader()
        reader.Read()
        Value = CDbl(reader.GetString(0))

    End Sub

    Public Sub Dispose()

        Command.Dispose()

    End Sub


End Class