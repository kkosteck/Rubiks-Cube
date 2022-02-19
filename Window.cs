using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Rubiks.Common;
using Rubiks.Cubes;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;

namespace Rubiks
{
    public class Window : GameWindow
    {
        private float[] _vertices;
        private uint[] _indices;

        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private int _elementBufferObject;


        private Shader _shader;

        private Camera _camera;
        private bool _firstMove = true;
        private Vector2 _lastPos;

        private Cube cube;

        private Stopwatch timer = new Stopwatch();
        private int FPS = 60;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) { }
        protected override void OnLoad()
        {
            cube = new Cube(new Vector3(1.0f, 1.0f, -1.0f), 0.5f);
            _vertices = cube.GetVertices();
            _indices = cube.GetIndicies();

            base.OnLoad();

            GL.ClearColor(0.75f, 0.75f, 0.75f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            _shader = new Shader("../../../Shaders/shader.vert", "../../../Shaders/shader.frag");

            GL.VertexAttribPointer(_shader.GetAttribLocation("aPosition"), 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(_shader.GetAttribLocation("rectangleColor"), 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            _camera = new Camera(new Vector3(-1.2f, 2.2f, 2.0f), Size.X / (float)Size.Y);
            _camera.Pitch -= 25;
            _camera.Yaw += 18;
            CursorGrabbed = true;
            timer.Start();
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            if (timer.ElapsedMilliseconds > 1000 / FPS)
            {
                RenderScene();
            }
            GL.BindVertexArray(_vertexArrayObject);


            var model = Matrix4.Identity;
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

            _shader.Use();

            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
            SwapBuffers();
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (!IsFocused)
                return;
            CameraUpdate(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            _camera.Fov -= e.OffsetY;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
            _camera.AspectRatio = Size.X / (float)Size.Y;
        }
        private void ModifyArray(float[] verts)
        {
            _vertices = verts;

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, _vertices.Length * sizeof(float), _vertices);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
        private void RenderScene()
        {
            cube.Update();
            if (cube.rotating == Notation.None)
            {
                if (KeyboardState.IsKeyDown(Keys.R))
                    cube.algorithm.AddRange(cube.LoadAlg());
                else if (KeyboardState.IsKeyDown(Keys.T))
                    cube.GenerateScramble(25);
            }
            else
            {
                ModifyArray(cube.Rotate(cube.rotating));
                timer.Restart();
            }
        }
        private void CameraUpdate(FrameEventArgs e)
        {
            if (KeyboardState.IsKeyDown(Keys.Escape))
                Close();

            const float cameraSpeed = 3.5f;
            const float sensitivity = 0.2f;

            if (KeyboardState.IsKeyDown(Keys.W))
                _camera.Position += _camera.Front * cameraSpeed * (float)e.Time;

            if (KeyboardState.IsKeyDown(Keys.S))
                _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time;
            if (KeyboardState.IsKeyDown(Keys.A))
                _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time;
            if (KeyboardState.IsKeyDown(Keys.D))
                _camera.Position += _camera.Right * cameraSpeed * (float)e.Time;
            if (KeyboardState.IsKeyDown(Keys.Space))
                _camera.Position += _camera.Up * cameraSpeed * (float)e.Time;
            if (KeyboardState.IsKeyDown(Keys.LeftShift))
                _camera.Position -= _camera.Up * cameraSpeed * (float)e.Time;

            if (_firstMove) // This bool variable is initially set to true.
            {
                _lastPos = new Vector2(MouseState.X, MouseState.Y);
                _firstMove = false;
            }
            else
            {
                // Calculate the offset of the mouse position
                var deltaX = MouseState.X - _lastPos.X;
                var deltaY = MouseState.Y - _lastPos.Y;
                _lastPos = new Vector2(MouseState.X, MouseState.Y);
                // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
            }

        }
    }
}