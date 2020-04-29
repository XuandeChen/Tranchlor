Module declarations
    Dim Picture_Name As String
    Dim Picture_Number As Short
    Dim frm As MDIChlor

    Public Option2d As Boolean = False

    'Demarrage du programme
    Public Sub main()
        Localize()
        Initialize_Data()
        frm = New MDIChlor()
        frm.ShowDialog()

    End Sub

    'Auteurs du programme
    Public Sub Initialize_Data()
        'Store
        Picture_Name = "Guido & David"
        Picture_Number = CShort(0)
    End Sub
    Public Sub Localize()
        My.Application.ChangeCulture("en-US")
    End Sub


End Module
