using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;

namespace StreetSovereings_
{
    internal class Renderer
    {
        public class Game : GameWindow
        {
            private readonly CubeManager _cubeManager = new CubeManager();

            private readonly float[] _vertices =
            {
                // Positions          // Colors
                -0.5f, -0.5f, -0.5f,  1.0f, 0.0f, 0.0f, // Red
                 0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.0f, // Green
                 0.5f,  0.5f, -0.5f,  0.0f, 0.0f, 1.0f, // Blue
                -0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 0.0f, // Yellow
                -0.5f, -0.5f,  0.5f,  1.0f, 0.0f, 1.0f, // Magenta
                 0.5f, -0.5f,  0.5f,  0.0f, 1.0f, 1.0f, // Cyan
                 0.5f,  0.5f,  0.5f,  1.0f, 1.0f, 1.0f, // White
                -0.5f,  0.5f,  0.5f,  0.0f, 0.0f, 0.0f, // Black
            };

            private readonly uint[] _indices =
            {
                0, 1, 2, 2, 3, 0,
                4, 5, 6, 6, 7, 4,
                0, 1, 5, 5, 4, 0,
                2, 3, 7, 7, 6, 2,
                0, 3, 7, 7, 4, 0,
                1, 2, 6, 6, 5, 1,
            };

            private int _vao;
            private int _vbo;
            private int _ebo;
            private int _shaderProgram;

            private float _rotation;

            public Game() : base(GameWindowSettings.Default, new NativeWindowSettings
            {
                Title = "Street Sovereigns",
                Size = new Vector2i(800, 600)
            })
            {
            }

            protected override void OnLoad()
            {
                base.OnLoad();
                GL.ClearColor(Color4.CornflowerBlue);

                // Create VAO
                _vao = GL.GenVertexArray();
                GL.BindVertexArray(_vao);

                // Create VBO
                _vbo = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

                // Create EBO
                _ebo = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

                // Vertex attributes
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
                GL.EnableVertexAttribArray(1);

                // Unbind VBO and VAO
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindVertexArray(0);

                // Compile shaders and link the program
                _shaderProgram = CreateShaderProgram();

                GL.Enable(EnableCap.DepthTest);

                // Add a default cube
                AddCube(0.0f, 0.0f, 0.0f, new Vector4(1.0f, 0.0f, 0.0f, 1.0f), 1.0f);
            }

            protected override void OnRenderFrame(FrameEventArgs args)
            {
                base.OnRenderFrame(args);

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                // Use the shader program
                GL.UseProgram(_shaderProgram);

                // Update and set the transformation matrices
                _rotation += 0.0005f;

                var view = Matrix4.LookAt(new Vector3(1.5f, 1.5f, 1.5f), Vector3.Zero, Vector3.UnitY);
                var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), Size.X / (float)Size.Y, 0.1f, 100.0f);

                GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "view"), false, ref view);
                GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "projection"), false, ref projection);

                foreach (var cube in _cubeManager.GetCubes())
                {
                    var model = Matrix4.CreateTranslation(cube.Position) * Matrix4.CreateRotationY(_rotation) * Matrix4.CreateRotationX(_rotation);
                    GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "model"), false, ref model);

                    // Set the cube's color
                    GL.Uniform4(GL.GetUniformLocation(_shaderProgram, "color"), cube.Color);

                    // Bind VAO and draw
                    GL.BindVertexArray(_vao);
                    GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
                }

                SwapBuffers();
            }

            private int CreateShaderProgram()
            {
                string vertexShaderSource = @"
                #version 330 core
                layout (location = 0) in vec3 aPosition;
                layout (location = 1) in vec3 aColor;

                uniform mat4 model;
                uniform mat4 view;
                uniform mat4 projection;

                out vec3 ourColor;

                void main()
                {
                    gl_Position = projection * view * model * vec4(aPosition, 1.0);
                    ourColor = aColor;
                }";

                string fragmentShaderSource = @"
                #version 330 core
                in vec3 ourColor;
                out vec4 color;

                void main()
                {
                    color = vec4(ourColor, 1.0);
                }";

                int vertexShader = GL.CreateShader(ShaderType.VertexShader);
                GL.ShaderSource(vertexShader, vertexShaderSource);
                GL.CompileShader(vertexShader);
                CheckShaderCompileStatus(vertexShader);

                int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
                GL.ShaderSource(fragmentShader, fragmentShaderSource);
                GL.CompileShader(fragmentShader);
                CheckShaderCompileStatus(fragmentShader);

                int shaderProgram = GL.CreateProgram();
                GL.AttachShader(shaderProgram, vertexShader);
                GL.AttachShader(shaderProgram, fragmentShader);
                GL.LinkProgram(shaderProgram);
                CheckProgramLinkStatus(shaderProgram);

                GL.DeleteShader(vertexShader);
                GL.DeleteShader(fragmentShader);

                return shaderProgram;
            }

            private void CheckShaderCompileStatus(int shader)
            {
                GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
                if (status != (int)All.True)
                {
                    string infoLog = GL.GetShaderInfoLog(shader);
                    throw new Exception($"Shader compilation failed: {infoLog}");
                }
            }

            private void CheckProgramLinkStatus(int program)
            {
                GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int status);
                if (status != (int)All.True)
                {
                    string infoLog = GL.GetProgramInfoLog(program);
                    throw new Exception($"Program link failed: {infoLog}");
                }
            }

            public void AddCube(float x, float y, float z, Vector4 rgba, float mass)
            {
                _cubeManager.AddCube(x, y, z, rgba, mass);
            }
        }
    }
}
