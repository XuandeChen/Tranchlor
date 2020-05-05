Imports OpenTK
Imports OpenTK.Graphics
Imports OpenTK.Graphics.OpenGL

Public Class Mesh 'Cubic

    Public Dx As Single
    Public Dy As Single
    Public Dz As Single


    Public Sub PlotTriangle()

        GL.Begin(BeginMode.Points)

        'Face 1
        GL.Color3(Color.White)
        GL.Vertex3(0, 0, 0)
        GL.Color3(Color.White)
        GL.Vertex3(Dx, 0, 0)
        GL.Color3(Color.Gray)
        GL.Vertex3(Dx, 0, Dz)
        GL.Color3(Color.LightGray)
        GL.Vertex3(0, 0, Dz)

        GL.Color3(Color.White)
        GL.Vertex3(Dx, Dy, 0)
        GL.Color3(Color.White)
        GL.Vertex3(0, Dy, 0)
        GL.Color3(Color.LightGray)
        GL.Vertex3(0, Dy, Dz)
        GL.Color3(Color.Gray)
        GL.Vertex3(Dx, Dy, Dz)

        'Finish the begin mode with "end"
        GL.End()

    End Sub

End Class
