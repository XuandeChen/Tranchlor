Imports OpenTK
Imports OpenTK.Graphics
Imports OpenTK.Graphics.OpenGL

Public Class frmMesh

    Dim a As Single

    Private Sub GlControl1_Load(sender As Object, e As EventArgs) Handles GlControl1.Load
        'Control is loaded, set back color
        GL.ClearColor(Color.Black)
        a = Me.Width / 34
        Dim x As Short = a
        Dim y As Short = 18 * a
        'Me.NumericUpDown1.Location = New System.Drawing.Point(18 * a, 18 * a)
        'Me.NumericUpDown2.Location = New System.Drawing.Point(0, 18 * a)

        Dim Position As New Point(a, a)
        Dim m_Longueur As Single = a * 10             'ne change pas pour les autres graphiques
        Dim m_Hauteur As Single = a * 7.5             'ne change pas pour les autres graphiques

        'Me.Invoke(Sub()
        '              GlControl1.Width = Me.
        '              GlControl1.Height = m_Hauteur
        '              GlControl1.Location = position
        '          End Sub)

    End Sub

    Private Sub GlControl1_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles GlControl1.Paint
        'First Clear Buffers
        GL.Clear(ClearBufferMask.ColorBufferBit)
        GL.Clear(ClearBufferMask.DepthBufferBit)

        'Basic Setup for viewing
        Dim perspective As Matrix4 = Matrix4.CreatePerspectiveFieldOfView(1.04, 4 / 3, 1, 10000) 'Setup Perspective
        Dim lookat As Matrix4 = Matrix4.LookAt(100, 100, 0, 0, 0, 0, 0, 1, 0) 'Setup camera
        GL.MatrixMode(MatrixMode.Projection) 'Load Perspective
        GL.LoadIdentity()
        GL.LoadMatrix(perspective)
        GL.MatrixMode(MatrixMode.Modelview) 'Load Camera
        GL.LoadIdentity()
        GL.LoadMatrix(lookat)
        GL.Viewport(0, 0, GlControl1.Width, GlControl1.Height) 'Size of window
        GL.Enable(EnableCap.DepthTest) 'Enable correct Z Drawings
        GL.DepthFunc(DepthFunction.Less) 'Enable correct Z Drawings

        'Rotating
        GL.Rotate(NumericUpDown1.Value, 0, 0, 1)
        GL.Rotate(NumericUpDown2.Value, 0, 1, 0)

        'Draw pyramid, Y is up, Z is twards you, X is left and right
        'Vertex goes (X,Y,Z)
        'GL.Begin(BeginMode.Triangles)

        Dim Triangle As New Mesh
        Triangle.Dx = 50
        Triangle.Dy = 50
        Triangle.Dz = 50
        Triangle.PlotTriangle()

        'Finally...
        'GraphicsContext.CurrentContext.VSync = True 'Caps frame rate as to not over run GPU
        GlControl1.SwapBuffers() 'Takes from the 'GL' and puts into control
    End Sub

    Private Sub NumericUpDown1_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown1.ValueChanged
        'Must be called to update the control window!
        GlControl1.Invalidate()
    End Sub

    Private Sub NumericUpDown2_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NumericUpDown2.ValueChanged
        'Must be called to update the control window!
        GlControl1.Invalidate()
    End Sub

    Private Sub frmMesh_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class