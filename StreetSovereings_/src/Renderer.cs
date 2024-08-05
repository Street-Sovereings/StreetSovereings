using System;
using NAudio.Wave;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StreetSovereings_.src.objects;
using StreetSovereings_.src.controllers.sounds;

namespace StreetSovereings_.src
{
    internal class Renderer
    {
        public class Game : GameWindow
        {
            private enum GameState
            {
                Menu,
                Playing
            }

            private GameState _currentState = GameState.Menu;

            private readonly CubeManager _cubeManager = new CubeManager();
            private readonly PlaneManager _planeManager = new PlaneManager();

            private readonly float[] _vertices =
            {
                // Positions         
                -0.5f, -0.5f, -0.5f,
                 0.5f, -0.5f, -0.5f,
                 0.5f,  0.5f, -0.5f,
                -0.5f,  0.5f, -0.5f,
                -0.5f, -0.5f,  0.5f,
                 0.5f, -0.5f,  0.5f,
                 0.5f,  0.5f,  0.5f,
                -0.5f,  0.5f,  0.5f,
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
            private int _menuShaderProgram;
            private int _frameShaderProgram;

            private IWavePlayer waveOutDeviceWalking;
            private Mp3FileReader mp3FileReader;
            private bool isWalkingSoundPlaying = false;

            public Vector3 _cameraPosition = new Vector3(1.5f, 1.5f, 3f);
            private bool _leftControlPressed = false;

            private float _rotation;

            float speed = 0.001f;
            float _initialSpeed;

            private Vector2 _playButtonPosition = new Vector2(-0.1f, -0.1f);
            private Vector2 _playButtonSize = new Vector2(0.2f, 0.1f);

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
                GL.ClearColor(Color4.Black); // Set background color to black

                waveOutDeviceWalking = new WaveOutEvent();
                waveOutDeviceWalking.PlaybackStopped += OnPlaybackWalkingStopped;

                InitializeBuffers();
                InitializeShaders();

                GL.Enable(EnableCap.DepthTest);

                // Add a default plane
                AddPlane(0.0f, -1.0f, 0.0f, 10.0f, 0.1f, 10.0f, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
            }

            private void InitializeBuffers()
            {
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
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);

                // Unbind VBO and VAO
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindVertexArray(0);
            }

            private void InitializeShaders()
            {
                _shaderProgram = CreateShaderProgram(vertexShaderSource, fragmentShaderSource);
                _menuShaderProgram = CreateShaderProgram(menuVertexShaderSource, menuFragmentShaderSource);
                _frameShaderProgram = CreateShaderProgram(frameVertexShaderSource, frameFragmentShaderSource);
            }

            private void InitializeAudio()
            {
                mp3FileReader?.Dispose();

                mp3FileReader = new Mp3FileReader("./assets/sounds/walking.mp3");
                waveOutDeviceWalking.Init(mp3FileReader);
            }

            private void OnPlaybackWalkingStopped(object sender, StoppedEventArgs e)
            {
                isWalkingSoundPlaying = false;
            }

            protected override void OnUpdateFrame(FrameEventArgs args)
            {
                base.OnUpdateFrame(args);

                var input = KeyboardState;

                if (_currentState == GameState.Menu)
                {
                    if (MouseState.IsButtonDown(MouseButton.Left))
                    {
                        Vector2 mousePosition = new Vector2(MouseState.X, MouseState.Y);
                        mousePosition = ScreenToNormalizedDeviceCoordinates(mousePosition, Size);

                        if (IsMouseOverButton(mousePosition, _playButtonPosition, _playButtonSize))
                        {
                            _currentState = GameState.Playing;
                        }
                    }
                }
                else if (_currentState == GameState.Playing)
                {
                    UpdateGame(input);
                }
            }

            private void UpdateGame(KeyboardState input)
            {
                if (input.IsKeyDown(Keys.W))
                {
                    _cameraPosition += new Vector3(0, 0, -speed);
                    StartWalkingSound();
                }
                else if (input.IsKeyDown(Keys.S))
                {
                    _cameraPosition += new Vector3(0, 0, speed);
                    StartWalkingSound();
                }
                else if (input.IsKeyDown(Keys.A))
                {
                    _cameraPosition += new Vector3(-speed, 0, 0);
                    StartWalkingSound();
                }
                else if (input.IsKeyDown(Keys.D))
                {
                    _cameraPosition += new Vector3(speed, 0, 0);
                    StartWalkingSound();
                }

                if (input.IsKeyDown(Keys.Space))
                {
                    _cameraPosition += new Vector3(0, speed, 0);
                }
                if (input.IsKeyDown(Keys.LeftShift))
                {
                    _cameraPosition += new Vector3(0, -speed, 0);
                }
                if (input.IsKeyPressed(Keys.LeftControl) && !_leftControlPressed)
                {
                    _initialSpeed = speed;
                    speed += speed;
                    _leftControlPressed = true;
                }
                else if (input.IsKeyReleased(Keys.LeftControl) && _leftControlPressed)
                {
                    _leftControlPressed = false;
                    speed = _initialSpeed;
                }
            }

            private void StartWalkingSound()
            {
                if (!isWalkingSoundPlaying)
                {
                    waveOutDeviceWalking.Stop();
                    InitializeAudio();
                    waveOutDeviceWalking.Play();
                    isWalkingSoundPlaying = true;
                }
            }

            protected override void OnRenderFrame(FrameEventArgs args)
            {
                base.OnRenderFrame(args);

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                if (_currentState == GameState.Menu)
                {
                    RenderMenu();
                }
                else if (_currentState == GameState.Playing)
                {
                    RenderGame();
                }

                SwapBuffers();
            }

            private void RenderMenu()
            {
                GL.ClearColor(Color4.Black);
                GL.UseProgram(_menuShaderProgram);

                float[] buttonVertices = {
                    _playButtonPosition.X, _playButtonPosition.Y,
                    _playButtonPosition.X + _playButtonSize.X, _playButtonPosition.Y,
                    _playButtonPosition.X + _playButtonSize.X, _playButtonPosition.Y + _playButtonSize.Y,
                    _playButtonPosition.X, _playButtonPosition.Y + _playButtonSize.Y
                };

                uint[] buttonIndices = { 0, 1, 2, 2, 3, 0 };

                int vbo = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, buttonVertices.Length * sizeof(float), buttonVertices, BufferUsageHint.StaticDraw);

                int ebo = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, buttonIndices.Length * sizeof(uint), buttonIndices, BufferUsageHint.StaticDraw);

                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);

                GL.DrawElements(PrimitiveType.Triangles, buttonIndices.Length, DrawElementsType.UnsignedInt, 0);

                // Draw button frame
                GL.UseProgram(_frameShaderProgram);
                GL.LineWidth(2.0f);
                GL.DrawElements(PrimitiveType.LineLoop, buttonIndices.Length, DrawElementsType.UnsignedInt, 0);

                // Render Text (Placeholder, replace this with your text rendering)
                // RenderText("Play", new Vector2(_playButtonPosition.X + 0.05f, _playButtonPosition.Y + 0.025f));

                GL.DisableVertexAttribArray(0);
                GL.DeleteBuffer(vbo);
                GL.DeleteBuffer(ebo);
            }

            private void RenderGame()
            {
                GL.UseProgram(_shaderProgram);
                GL.ClearColor(Color4.CornflowerBlue);

                var view = Matrix4.LookAt(_cameraPosition, Vector3.Zero, Vector3.UnitY);
                var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), Size.X / (float)Size.Y, 0.1f, 100.0f);

                GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "view"), false, ref view);
                GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "projection"), false, ref projection);

                GL.BindVertexArray(_vao);

                foreach (var cube in _cubeManager.GetCubes())
                {
                    var model = Matrix4.CreateTranslation(cube.Position) * Matrix4.CreateRotationY(_rotation) * Matrix4.CreateRotationX(_rotation);
                    GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "model"), false, ref model);
                    GL.Uniform4(GL.GetUniformLocation(_shaderProgram, "ourColor"), cube.Color);
                    GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
                }

                foreach (var plane in _planeManager.GetPlanes())
                {
                    var model = Matrix4.CreateScale(plane.Size) * Matrix4.CreateTranslation(plane.Position);
                    GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "model"), false, ref model);
                    GL.Uniform4(GL.GetUniformLocation(_shaderProgram, "ourColor"), plane.Color);
                    GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
                }

                GL.BindVertexArray(0);
            }

            private int CreateShaderProgram(string vertexShaderSource, string fragmentShaderSource)
            {
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

            public void AddPlane(float x, float y, float z, float sizeX, float sizeY, float sizeZ, Vector4 rgba)
            {
                _planeManager.AddPlane(x, y, z, sizeX, sizeY, sizeZ, rgba);
            }

            private Vector2 ScreenToNormalizedDeviceCoordinates(Vector2 screenCoordinates, Vector2i screenSize)
            {
                return new Vector2(
                    (2.0f * screenCoordinates.X) / screenSize.X - 1.0f,
                    1.0f - (2.0f * screenCoordinates.Y) / screenSize.Y
                );
            }

            private bool IsMouseOverButton(Vector2 mousePosition, Vector2 buttonPosition, Vector2 buttonSize)
            {
                return mousePosition.X > buttonPosition.X &&
                       mousePosition.X < buttonPosition.X + buttonSize.X &&
                       mousePosition.Y > buttonPosition.Y &&
                       mousePosition.Y < buttonPosition.Y + buttonSize.Y;
            }

            private const string vertexShaderSource = @"
                #version 330 core
                layout (location = 0) in vec3 aPosition;

                uniform mat4 model;
                uniform mat4 view;
                uniform mat4 projection;

                void main()
                {
                    gl_Position = projection * view * model * vec4(aPosition, 1.0);
                }";

            private const string fragmentShaderSource = @"
                #version 330 core
                uniform vec4 ourColor;
                out vec4 color;

                void main()
                {
                    color = ourColor;
                }";

            private const string menuVertexShaderSource = @"
                #version 330 core
                layout (location = 0) in vec2 aPosition;

                void main()
                {
                    gl_Position = vec4(aPosition, 0.0, 1.0);
                }";

            private const string menuFragmentShaderSource = @"
                #version 330 core
                out vec4 color;

                void main()
                {
                    color = vec4(0.0, 0.8, 0.2, 1.0); // Green color for the button
                }";

            private const string frameVertexShaderSource = @"
                #version 330 core
                layout (location = 0) in vec2 aPosition;

                void main()
                {
                    gl_Position = vec4(aPosition, 0.0, 1.0);
                }";

            private const string frameFragmentShaderSource = @"
                #version 330 core
                out vec4 color;

                void main()
                {
                    color = vec4(1.0, 1.0, 1.0, 1.0); // White color for the frame
                }";
        }
    }
}
