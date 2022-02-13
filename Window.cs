using System;
using System.Linq;
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

        private float degrees;
        private Vector3 position;

        private Shader _shader;

        private Camera _camera;
        private bool _firstMove = true;
        private Vector2 _lastPos;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) { }
        protected override void OnLoad()
        {
            Cube cube = new Cube(new Vector3(0.0f, 0.0f, 1.0f), 0.5f);
            _vertices = cube.GetVertices();
            _indices = cube.GetIndicies();

            base.OnLoad();

            degrees = 0.0f;
            position = new Vector3(0.0f, 0.0f, 0.0f);

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

            _camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);
            CursorGrabbed = true;
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.BindVertexArray(_vertexArrayObject);

            if (KeyboardState.IsKeyDown(Keys.E))
            {
                degrees += 0.1f;
                degrees %= 360;
            }
            if (KeyboardState.IsKeyDown(Keys.Up))
            {
                position = Vector3.Add(position, new Vector3(0.0f, 0.001f, 0.0f));
            }

            //var transform = Matrix4.Identity;
            //transform = transform * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(degrees));
            //transform = transform * Matrix4.CreateScale(1.1f);
            //transform = transform * Matrix4.CreateTranslation(position);


            var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(degrees));
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

            if (!IsFocused) // Check to see if the window is focused
            {
                return;
            }

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            const float cameraSpeed = 3.5f;
            const float sensitivity = 0.2f;

            if (input.IsKeyDown(Keys.W))
            {
                _camera.Position += _camera.Front * cameraSpeed * (float)e.Time; // Forward
            }

            if (input.IsKeyDown(Keys.S))
            {
                _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time; // Backwards
            }
            if (input.IsKeyDown(Keys.A))
            {
                _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time; // Left
            }
            if (input.IsKeyDown(Keys.D))
            {
                _camera.Position += _camera.Right * cameraSpeed * (float)e.Time; // Right
            }
            if (input.IsKeyDown(Keys.Space))
            {
                _camera.Position += _camera.Up * cameraSpeed * (float)e.Time; // Up
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                _camera.Position -= _camera.Up * cameraSpeed * (float)e.Time; // Down
            }

            // Get the mouse state
            var mouse = MouseState;

            if (_firstMove) // This bool variable is initially set to true.
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                // Calculate the offset of the mouse position
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
            }
        }

        // In the mouse wheel function, we manage all the zooming of the camera.
        // This is simply done by changing the FOV of the camera.
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            _camera.Fov -= e.OffsetY;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
            // We need to update the aspect ratio once the window has been resized.
            _camera.AspectRatio = Size.X / (float)Size.Y;
        }
        private void ModifyArray()
        {
            _vertices = new float[]
            {
                 0.5f,  0.5f, 0.0f,  1.0f, 1.0f, 1.0f,  // top right
                 0.5f, -0.5f, 0.0f,  1.0f, 1.0f, 1.0f,  // bottom right
                -0.5f, -0.5f, 0.0f,  1.0f, 1.0f, 1.0f,  // bottom left
                -0.5f,  0.5f, 0.0f,  1.0f, 1.0f, 1.0f   // top left
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, _vertices.Length * sizeof(float), _vertices);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
    }
}