using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;  // For Vector4, Vector2, etc.
using System;

namespace StreetSovereigns
{
    public class Game : GameWindow
    {
        private static readonly float[] VertexData =
        {
            // Vertex
            0.0f,  0.5f, 0.0f, 
           -0.5f, -0.5f, 0.0f,
            0.5f, -0.5f, 0.0f 
        };

        public Game() : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(Color4.CornflowerBlue);

            uint vao = (uint)GL.GenVertexArray();
            uint vbo = (uint)GL.GenBuffer();

            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, VertexData.Length * sizeof(float), VertexData, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.BindVertexArray(1); //bindings for vao (for kiNgchev)

            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            SwapBuffers();
        }

        [STAThread]
        public static void Main()
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
