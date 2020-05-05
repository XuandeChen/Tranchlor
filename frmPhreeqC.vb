Imports IPhreeqcCOM

Public Class frmPhreeqC

    Dim a As Single
    Dim PhrObj As New IPhreeqcCOM.Object
    Dim PhrInput As String
    Dim PhrDatabase As String = "phreeqc.dat"

    Private Sub frmPhreeqC_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        a = Me.Width / 34
        Dim x As Short = a
        Dim y As Short = 18 * a
        'Me.Button1.Location = New System.Drawing.Point(18 * a, 18 * a)
        'Me.Button2.Location = New System.Drawing.Point(0, 18 * a)
    End Sub

    Public Sub design(ByVal Tijd As Integer, ByVal Length As Single, ByVal ymin As Single, ByVal ymax As Single, ByVal yecart As Single, ByRef PosProf() As Decimal, ByRef Told() As Decimal, ByRef Dofs As Short)

        Dim position As Point
        position = New Point(a, a)
        Dim m_Longueur As Single = a * 10             'ne change pas pour les autres graphiques
        Dim m_Hauteur As Single = a * 7.5             'ne change pas pour les autres graphiques

        Dim couleur As Color = System.Drawing.Color.Red
        Dim Message0 As String = ""                       'ne change pas pour les autres graphiques
        Dim Message1 As String = "Concentration [mol/kgw]"
        Dim Message2 As String = "Depth [mm]"             'ne change pas pour les autres grapiques
        Dim Message3 As String = "Concentration Distribution at " & Format(Tijd, "0.00") & " Days"

        Dim x1 As Single = 0                              'ne change pas pour les autres graphiques
        Dim x2 As Single = Length                         'ne change pas pour les autres graphiques
        Dim h1 As Single = (Int(ymin / 10)) * 10
        Dim y As Integer

        If Int(ymax / 10) - ymax / 10 = 0 Then
            y = ymax
        Else
            y = (Int(ymax / 10) + 1) * 10
        End If

        'Dim gr01 As CGraph = New CGraph(Me, PictureBox1, position, m_Longueur, m_Hauteur, couleur, Message0, Message1, Message2, Message3, x1, x2, h1, h2, EctX, EctY, PosProf, T_old, Dofs, -1, -1)
        PlotGraph(position, m_Longueur, m_Hauteur, couleur, Message0, Message1, Message2, Message3, x1, x2, h1, y, Int(Length / 10), yecart, PosProf, Told, Dofs, -1, -1)

    End Sub

    Public Sub PlotGraph(ByVal position As Point, ByVal m_Longueur As Single, ByVal m_Hauteur As Single, ByVal couleur As Color, ByVal message0 As String, ByVal message1 As String, ByVal message2 As String, ByVal message3 As String, ByVal x1 As Single, ByVal x2 As Single, ByVal h1 As Single, ByVal h2 As Single, ByVal EctX As Single, ByVal EctY As Single, ByRef PosProf() As Decimal, ByRef Result() As Decimal, ByVal Dofs As Short, ByRef Gxc As Single, ByRef Dxc As Single)
        Dim m_Image As Bitmap
        Dim m_Position As Point
        Dim m_Gr As Graphics
        Dim m_Pen As Pen
        Const m_Epaisseur As Integer = 4

        Dim m_X1 As Single
        Dim m_X2 As Single
        Dim m_Y2 As Single
        Dim m_Y1 As Single

        Dim i As Short
        Dim x As Single
        Dim y As Single
        Dim msg As String

        Dim m_Facteur As Single

        Try
            m_Facteur = Me.Width / 34

            Me.Invoke(Sub()
                          PictureBox1.Width = m_Longueur
                          PictureBox1.Height = m_Hauteur
                          PictureBox1.Location = position
                      End Sub)

            'pcBox.Width = m_Longueur
            'pcBox.Height = m_Hauteur
            'pcBox.Location = position

            m_Image = New Bitmap(PictureBox1.Width, PictureBox1.Height)
            m_Gr = Graphics.FromImage(m_Image)
            m_Pen = New Pen(couleur, m_Epaisseur)

            m_X1 = m_Facteur
            m_Y1 = PictureBox1.Height - m_Facteur * 1.5
            m_X2 = PictureBox1.Width - 0.5 * m_Facteur
            m_Y2 = m_Facteur

            Dim sh As Single = (m_Y1 - m_Y2) / (h2 - h1)
            Dim ih As Integer = CInt((h2 - h1) / EctY)

            Dim sx As Single = (m_X2 - m_X1) / (x2 - x1)
            Dim ix As Integer = CInt((x2 - x1) / EctX)

            'Construction des lignes d'axes
            Dim H8ft As Single = 0.2118 * CSng(m_Facteur)
            Dim H10ft As Single = 0.2648 * CSng(m_Facteur)
            Dim H12ft As Single = 0.3178 * CSng(m_Facteur)
            Dim fnt8Arial As New Font("Arial", H8ft)
            Dim fnt10Arial As New Font("Arial", H10ft)
            Dim fnt12Arial As New Font("Arial", H12ft)
            Dim pen01 As New Drawing.Pen(System.Drawing.Color.Gray, 1)
            Dim Drawformat0 As New StringFormat
            Dim Drawformat1 As New StringFormat
            Dim Drawformat2 As New StringFormat
            Drawformat0.Alignment = StringAlignment.Near
            Drawformat1.Alignment = StringAlignment.Far
            Drawformat2.Alignment = StringAlignment.Center

            Dim drawPoint04 As New PointF(5 * m_Facteur, 0)
            m_Gr.DrawString(message0, fnt12Arial, Brushes.Black, drawPoint04, Drawformat2)

            Dim drawPoint01 As New PointF(m_X1 - m_Facteur / 6, m_Y2 - 0.5 * m_Facteur)
            m_Gr.DrawString(message1, fnt10Arial, Brushes.Black, drawPoint01, Drawformat0)

            Dim drawPoint03 As New PointF(m_X2, m_Y1 + 0.3 * m_Facteur)
            m_Gr.DrawString(message2, fnt10Arial, Brushes.Black, drawPoint03, Drawformat1)

            Dim drawPoint00 As New PointF(m_Facteur * 5, PictureBox1.Height - 2 * H12ft)
            m_Gr.DrawString(message3, fnt12Arial, Brushes.Black, drawPoint00, Drawformat2)

            For i = 0 To ih
                y = m_Y1 - i * sh * EctY
                Dim drawPoint As New PointF(m_X1, y - 0.5 * H8ft)
                ' Create a Pen object
                m_Gr.DrawLine(pen01, m_X1, y, m_X2, y)
                msg = Format(h1 + CSng(i) * EctY, "0.00")
                m_Gr.DrawString(msg, fnt8Arial, Brushes.Black, drawPoint, Drawformat1)
            Next i

            For i = 0 To ix
                x = m_X1 + i * EctX * sx
                ' Create a Pen object.
                Dim drawPoint02 As New PointF(x, m_Y1)
                m_Gr.DrawLine(pen01, x, m_Y1, x, m_Y2)
                msg = i * EctX + x1
                m_Gr.DrawString(msg, fnt8Arial, Brushes.Black, drawPoint02, Drawformat2)
            Next i

            'dessin des résultats
            Dim x_old As Single
            Dim y_old As Single

            y = m_Y1 - sh * (Result(1) - h1)
            m_Gr.DrawLine(m_Pen, m_X1, y, m_X1, y)
            x_old = m_X1
            y_old = y
            For i = 2 To Dofs
                y = m_Y1 - sh * (Result(i) - h1)
                x = m_X1 + sx * PosProf(i)
                m_Gr.DrawLine(m_Pen, x_old, y_old, x, y)
                x_old = x
                y_old = y
            Next i
            If Gxc >= 0 And Dxc >= 0 Then       'dessin des positions de xc pour la carbonatation
                m_Pen = New Pen(System.Drawing.Color.Cyan, m_Epaisseur)
                m_Gr.DrawLine(m_Pen, m_X1 + sx * Gxc, m_Y1 - sh * (h1 - h1), m_X1 + sx * Gxc, m_Y1 - sh * (h2 - h1))
                m_Gr.DrawLine(m_Pen, m_X1 + sx * (PosProf(Dofs) - Dxc), m_Y1 - sh * (h1 - h1), m_X1 + sx * (PosProf(Dofs) - Dxc), m_Y1 - sh * (h2 - h1))
            End If
        Catch e As Exception
        End Try
        PictureBox1.Image = m_Image
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Save_pictures(1)
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click

        Dim Filtre As String = "PhreeqC files (*.phr)|*.phr"
        Dim Index As Short = 1
        Dim Directoire As Boolean = True
        Dim Titre As String = "Sélectionner le fichier météo"
        Dim OutFile As String
        Dim Canc As Boolean = False

        OpenDialog(OutFile, Canc, Filtre, Index, Directoire, Titre)
        If Canc = True Then GoTo b

        Dim nFic As Integer = Microsoft.VisualBasic.FileSystem.FreeFile()
        FileOpen(nFic, OutFile, OpenMode.Input, OpenAccess.Read, OpenShare.Shared)
        Dim PostFile As String
        FilePost(OutFile, PostFile)
        FileOnly(OutFile)

        Dim posTxt As Integer = Len(OutFile) - 10
        Dim txtFile As String = Mid(OutFile, 7, posTxt)
        Dim line As String

        Dim bFertig As Boolean = False
        Dim i As Integer = 0
        Do While Not bFertig
            Try 'test s'il y a du text ou pas
                Input(nFic, PhrInput)
            Catch
                bFertig = True
            End Try
        Loop
        FileClose(nFic)

        Run(PhrInput, PhrDatabase)

b:
    End Sub

    Sub Run(ByVal input As String, ByVal Dbase As String)

        Me.PhrDatabase = Dbase
        Me.PhrObj.LoadDatabase(PhrDatabase)
        Me.PhrObj.RunString(input)
        'design()

    End Sub

End Class