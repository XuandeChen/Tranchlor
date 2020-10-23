Imports System.Linq

Public Class Exposition

    Public Property Humidite() As Double()
    Public Property Sel() As Double()
    Public Property Temperature() As Double()

    Private Dt As Single
    Private Name As String
    Private File As String = ""

    'Public TempMin As Single = 40
    'Public TempMax As Single = -30
    'Public TempEcart As Single = 0
    'Public Msel As Single = 0

    Public Sub New(ByRef FileExpo As String)

        Dim nFic As Short = CShort(FreeFile())
        Dim NbreEn As Integer

        If FileExpo.Contains(".txt") = True Then

            Try
                FileOpen(CInt(nFic), FileExpo, OpenMode.Input, OpenAccess.Read)
                FileClose(CInt(nFic))

                Dim Arr() As String = Split(FileExpo, "\"c)
                Name = Arr.Last()
                File = FileExpo

            Catch ex As Exception
                End
            End Try

        Else

            Try

                Dim DBCon As New DBconnexion
                DBCon.DBRequest("SELECT '" + FileExpo + "' FROM ExpositionList")
                Name = FileExpo

            Catch ex As Exception
                End
            End Try

        End If

    End Sub

    Public Sub WriteExpo(ByRef ind As Integer)

        Dim nFic As Short = CShort(FreeFile())
        Dim NbreEn As Integer

        If File <> "" Then

            Try

                FileOpen(CInt(nFic), File, OpenMode.Input, OpenAccess.Read)
                Input(CInt(nFic), NbreEn)
                Input(CInt(nFic), Dt)

                ReDim Humidite(ind - 1)
                ReDim Sel(ind - 1)
                ReDim Temperature(ind - 1)

                Dim j As Integer = 0
                Dim nbboucle As Integer = 1
                For i As Integer = 0 To ind - 1

                    If i <= (NbreEn - 1) Then
                        Input(CInt(nFic), Humidite(i))
                        Input(CInt(nFic), Sel(i))
                        Input(CInt(nFic), Temperature(i))
                        j += 1

                    ElseIf i <= (NbreEn * nbboucle - 1) Then
                        Humidite(i) = Humidite(j)
                        Sel(i) = Sel(j)
                        Temperature(i) = Temperature(j)
                        j += 1
                    Else
                        If nbboucle = 1 Then MsgBox("Exposition File too short. Copying the exposition in loop.")
                        j = 0
                        nbboucle += 1
                        Humidite(i) = Humidite(j)
                        Sel(i) = Sel(j)
                        Temperature(i) = Temperature(j)
                        j += 1
                    End If


                Next

                FileClose(CInt(nFic))

            Catch ex As Exception
                End
            End Try

        Else

            Try

                Dim DBCon As New DBconnexion
                Dim Expo As New MaterialsData

                DBCon.DBRequest("SELECT TOP " + CStr(ind) + " * FROM [" + Name + "]")
                DBCon.MatFill(Expo, Name)

                Dim ExpoTable()() As Object = Expo.Tables(Name).Rows.Cast(Of DataRow).Select(Function(dr) dr.ItemArray).ToArray

                NbreEn = ExpoTable.Length()
                Dt = 3600

                ReDim Humidite(ind - 1)
                ReDim Sel(ind - 1)
                ReDim Temperature(ind - 1)

                Dim j As Integer = 0
                Dim nbboucle As Integer = 1
                For i As Integer = 0 To ind - 1

                    If i < (NbreEn - 1) * nbboucle Then
                        j += 1
                    Else
                        MsgBox("Exposition File too short. Copying the exposition in loop.")
                        j = 1
                        nbboucle += 1
                    End If

                    Humidite(j - 1) = ExpoTable(j - 1)(1)
                    Sel(j - 1) = ExpoTable(j - 1)(2)
                    Temperature(j - 1) = ExpoTable(j - 1)(3)

                Next


            Catch ex As Exception
                End
            End Try

        End If

    End Sub

End Class
