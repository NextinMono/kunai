﻿//using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using Hexa.NET.ImGui;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;
using System.IO;
using Hexa.NET.ImPlot;
using IconFonts;
using System.Runtime.InteropServices;

namespace Kunai
{
    public class ImGuiController : IDisposable
    {
        private bool _frameBegun;

        private int _vertexArray;
        private int _vertexBuffer;
        private int _vertexBufferSize;
        private int _indexBuffer;
        private int _indexBufferSize;

        //private Texture _fontTexture;

        private int _fontTexture;

        private int _shader;
        private int _shaderFontTextureLocation;
        private int _shaderProjectionMatrixLocation;

        private int _windowWidth;
        private int _windowHeight;

        private System.Numerics.Vector2 _scaleFactor = System.Numerics.Vector2.One;

        private static bool _khrDebugAvailable = false;

        private int _glVersion;
        private bool _compatibilityProfile;

        /// <summary>
        /// Constructs a new ImGuiController.
        /// </summary>
        /// 
        private const int MdtEffectiveDpi = 0;
        [DllImport("Shcore.dll")]
        private static extern int GetDpiForMonitor(
            IntPtr in_Hmonitor,
            int in_DpiType,
            out uint in_DpiX,
            out uint in_DpiY);

        [DllImport("User32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr in_Hwnd, uint in_DwFlags);

        public static float GetDpiScaling()
        {

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {

                IntPtr hMonitor = MonitorFromWindow(IntPtr.Zero, 2); // Primary monitor
                uint dpiX, dpiY;

                int result = GetDpiForMonitor(hMonitor, MdtEffectiveDpi, out dpiX, out dpiY);

                if (result == 0) // S_OK
                {
                    Console.WriteLine($"DPI Scale: {dpiX}x{dpiY}");
                }
                else
                {
                    Console.WriteLine("Error retrieving DPI.");
                }
                return dpiX / 100.0f;
            }

            return 1;
        }

        public static void SetupImGuiStyle()
        {
            // AdobeInspired stylenexacopic from ImThemes
            var style = ImGui.GetStyle();

            style.Alpha = 1.0f;
            style.DisabledAlpha = 0.6000000238418579f;
            style.WindowPadding = new System.Numerics.Vector2(8.0f, 8.0f);
            style.WindowRounding = 4.0f;
            style.WindowBorderSize = 1.0f;
            style.WindowMinSize = new System.Numerics.Vector2(32.0f, 32.0f);
            style.WindowTitleAlign = new System.Numerics.Vector2(0.0f, 0.5f);
            style.WindowMenuButtonPosition = ImGuiDir.None;
            style.ChildRounding = 4.0f;
            style.ChildBorderSize = 1.0f;
            style.PopupRounding = 4.0f;
            style.PopupBorderSize = 1.0f;
            style.FramePadding = new System.Numerics.Vector2(4.0f, 3.0f);
            style.FrameRounding = 4.0f;
            style.FrameBorderSize = 1.0f;
            style.ItemSpacing = new System.Numerics.Vector2(8.0f, 4.0f);
            style.ItemInnerSpacing = new System.Numerics.Vector2(4.0f, 4.0f);
            style.CellPadding = new System.Numerics.Vector2(4.0f, 2.0f);
            style.IndentSpacing = 21.0f;
            style.ColumnsMinSpacing = 6.0f;
            style.ScrollbarSize = 14.0f;
            style.ScrollbarRounding = 4.0f;
            style.GrabMinSize = 10.0f;
            style.GrabRounding = 20.0f;
            style.TabRounding = 4.0f;
            style.TabBorderSize = 1.0f;
            style.TabMinWidthForCloseButton = 0.0f;
            style.ColorButtonPosition = ImGuiDir.Right;
            style.ButtonTextAlign = new System.Numerics.Vector2(0.5f, 0.5f);
            style.SelectableTextAlign = new System.Numerics.Vector2(0.0f, 0.0f);

            style.Colors[(int)ImGuiCol.Text] = new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TextDisabled] = new System.Numerics.Vector4(0.4980392158031464f, 0.4980392158031464f, 0.4980392158031464f, 1.0f);
            style.Colors[(int)ImGuiCol.WindowBg] = new System.Numerics.Vector4(0.1137254908680916f, 0.1137254908680916f, 0.1137254908680916f, 1.0f);
            style.Colors[(int)ImGuiCol.ChildBg] = new System.Numerics.Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.PopupBg] = new System.Numerics.Vector4(0.0784313753247261f, 0.0784313753247261f, 0.0784313753247261f, 0.9399999976158142f);
            style.Colors[(int)ImGuiCol.Border] = new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 0.1630901098251343f);
            style.Colors[(int)ImGuiCol.BorderShadow] = new System.Numerics.Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.FrameBg] = new System.Numerics.Vector4(0.08627451211214066f, 0.08627451211214066f, 0.08627451211214066f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = new System.Numerics.Vector4(0.1529411822557449f, 0.1529411822557449f, 0.1529411822557449f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgActive] = new System.Numerics.Vector4(0.1882352977991104f, 0.1882352977991104f, 0.1882352977991104f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBg] = new System.Numerics.Vector4(0.1137254908680916f, 0.1137254908680916f, 0.1137254908680916f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgActive] = new System.Numerics.Vector4(0.105882354080677f, 0.105882354080677f, 0.105882354080677f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new System.Numerics.Vector4(0.0f, 0.0f, 0.0f, 0.5099999904632568f);
            style.Colors[(int)ImGuiCol.MenuBarBg] = new System.Numerics.Vector4(0.1137254908680916f, 0.1137254908680916f, 0.1137254908680916f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarBg] = new System.Numerics.Vector4(0.01960784383118153f, 0.01960784383118153f, 0.01960784383118153f, 0.5299999713897705f);
            style.Colors[(int)ImGuiCol.ScrollbarGrab] = new System.Numerics.Vector4(0.3098039329051971f, 0.3098039329051971f, 0.3098039329051971f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new System.Numerics.Vector4(0.407843142747879f, 0.407843142747879f, 0.407843142747879f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new System.Numerics.Vector4(0.5098039507865906f, 0.5098039507865906f, 0.5098039507865906f, 1.0f);
            style.Colors[(int)ImGuiCol.CheckMark] = new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.SliderGrab] = new System.Numerics.Vector4(0.8784313797950745f, 0.8784313797950745f, 0.8784313797950745f, 1.0f);
            style.Colors[(int)ImGuiCol.SliderGrabActive] = new System.Numerics.Vector4(0.9803921580314636f, 0.9803921580314636f, 0.9803921580314636f, 1.0f);
            style.Colors[(int)ImGuiCol.Button] = new System.Numerics.Vector4(0.1490196138620377f, 0.1490196138620377f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new System.Numerics.Vector4(0.2470588237047195f, 0.2470588237047195f, 0.2470588237047195f, 1.0f);
            style.Colors[(int)ImGuiCol.ButtonActive] = new System.Numerics.Vector4(0.3294117748737335f, 0.3294117748737335f, 0.3294117748737335f, 1.0f);
            style.Colors[(int)ImGuiCol.Header] = new System.Numerics.Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 0.3098039329051971f);
            style.Colors[(int)ImGuiCol.HeaderHovered] = new System.Numerics.Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 0.800000011920929f);
            style.Colors[(int)ImGuiCol.HeaderActive] = new System.Numerics.Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 1.0f);
            style.Colors[(int)ImGuiCol.Separator] = new System.Numerics.Vector4(0.4274509847164154f, 0.4274509847164154f, 0.4980392158031464f, 0.5f);
            style.Colors[(int)ImGuiCol.SeparatorHovered] = new System.Numerics.Vector4(0.7490196228027344f, 0.7490196228027344f, 0.7490196228027344f, 0.7803921699523926f);
            style.Colors[(int)ImGuiCol.SeparatorActive] = new System.Numerics.Vector4(0.7490196228027344f, 0.7490196228027344f, 0.7490196228027344f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGrip] = new System.Numerics.Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 0.2000000029802322f);
            style.Colors[(int)ImGuiCol.ResizeGripHovered] = new System.Numerics.Vector4(0.9372549057006836f, 0.9372549057006836f, 0.9372549057006836f, 0.6705882549285889f);
            style.Colors[(int)ImGuiCol.ResizeGripActive] = new System.Numerics.Vector4(0.9764705896377563f, 0.9764705896377563f, 0.9764705896377563f, 0.9490196108818054f);
            style.Colors[(int)ImGuiCol.Tab] = new System.Numerics.Vector4(0.2235294133424759f, 0.2235294133424759f, 0.2235294133424759f, 0.8627451062202454f);
            style.Colors[(int)ImGuiCol.TabHovered] = new System.Numerics.Vector4(0.321568638086319f, 0.321568638086319f, 0.321568638086319f, 0.800000011920929f);
            style.Colors[(int)ImGuiCol.TabSelected] = new System.Numerics.Vector4(0.2745098173618317f, 0.2745098173618317f, 0.2745098173618317f, 1.0f);
            style.Colors[(int)ImGuiCol.TabDimmed] = new System.Numerics.Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1450980454683304f, 0.9725490212440491f);
            style.Colors[(int)ImGuiCol.TabDimmedSelected] = new System.Numerics.Vector4(0.4235294163227081f, 0.4235294163227081f, 0.4235294163227081f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLines] = new System.Numerics.Vector4(0.6078431606292725f, 0.6078431606292725f, 0.6078431606292725f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLinesHovered] = new System.Numerics.Vector4(1.0f, 0.4274509847164154f, 0.3490196168422699f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogram] = new System.Numerics.Vector4(0.8980392217636108f, 0.6980392336845398f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new System.Numerics.Vector4(1.0f, 0.6000000238418579f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TableHeaderBg] = new System.Numerics.Vector4(0.1882352977991104f, 0.1882352977991104f, 0.2000000029802322f, 1.0f);
            style.Colors[(int)ImGuiCol.TableBorderStrong] = new System.Numerics.Vector4(0.3098039329051971f, 0.3098039329051971f, 0.3490196168422699f, 1.0f);
            style.Colors[(int)ImGuiCol.TableBorderLight] = new System.Numerics.Vector4(0.2274509817361832f, 0.2274509817361832f, 0.2470588237047195f, 1.0f);
            style.Colors[(int)ImGuiCol.TableRowBg] = new System.Numerics.Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.TableRowBgAlt] = new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 0.05999999865889549f);
            style.Colors[(int)ImGuiCol.TextSelectedBg] = new System.Numerics.Vector4(0.2588235437870026f, 0.5882353186607361f, 0.9764705896377563f, 0.3499999940395355f);
            style.Colors[(int)ImGuiCol.DragDropTarget] = new System.Numerics.Vector4(1.0f, 1.0f, 0.0f, 0.8999999761581421f);
            style.Colors[(int)ImGuiCol.NavCursor] = new System.Numerics.Vector4(0.2588235437870026f, 0.5882353186607361f, 0.9764705896377563f, 1.0f);
            style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 0.699999988079071f);
            style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new System.Numerics.Vector4(0.800000011920929f, 0.800000011920929f, 0.800000011920929f, 0.2000000029802322f);
            style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new System.Numerics.Vector4(0.800000011920929f, 0.800000011920929f, 0.800000011920929f, 0.3499999940395355f);
        }

        uint[] icons_ranges = new uint[3] { FontAwesome6.IconMin, FontAwesome6.IconMax, 0 };
        public static ImFontPtr DefaultFont;
        public static ImFontPtr FontAwesomeFont;
        unsafe uint* range {
            get {
                fixed (uint* range = &icons_ranges[0])
                    return range;
            }
        }
        public unsafe ImGuiController(int in_Width, int in_Height)
        {
            _windowWidth = in_Width;
            _windowHeight = in_Height;

            int major = GL.GetInteger(GetPName.MajorVersion);
            int minor = GL.GetInteger(GetPName.MinorVersion);

            _glVersion = major * 100 + minor * 10;

            _khrDebugAvailable = (major == 4 && minor >= 3) || IsExtensionSupported("KHR_debug");

            _compatibilityProfile = (GL.GetInteger((GetPName)All.ContextProfileMask) & (int)All.ContextCompatibilityProfileBit) != 0;

            var context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);

            ImPlot.CreateContext();
            ImPlot.SetImGuiContext(ImGui.GetCurrentContext());
            var io = ImGui.GetIO();
            //io.Fonts.AddFontDefault();

                ImFontConfig config = new ImFontConfig
                {
                    MergeMode = 1,
                    OversampleH = 3,
                    OversampleV = 3,
                    GlyphOffset = new System.Numerics.Vector2(0,0),
                    FontDataOwnedByAtlas = 0,
                    PixelSnapH = 1,
                    GlyphMaxAdvanceX = 16,
                    RasterizerMultiply = 2.0f,
                    GlyphRanges = range,
                };
                //io.Fonts.AddFontFromFileTTF(IconFonts.FontA\wesome6.FontIconFileNameFAR, 16, icons_config, new uint[64]{ Icon });
                DefaultFont = io.Fonts.AddFontFromFileTTF(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "RobotoVariable.ttf"), 16 * GetDpiScaling());
            FontAwesomeFont = io.Fonts.AddFontFromFileTTF(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", FontAwesome6.FontIconFileNameFAS), 16 * GetDpiScaling(), null, range);
                io.Fonts.Build();
            
            //unsafe
            //{
            //    io.Fonts.AddFontFromFileTTF(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, IconFonts.FontAwesome6.FontIconFileNameFAR), 16, &icons_config, ranges.Data);
            //
            //}
            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
            // Enable Docking
            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

            CreateDeviceResources();

            SetPerFrameImGuiData(1f / 60f);

            SetupImGuiStyle();
            ImGui.NewFrame();
            _frameBegun = true;
        }

        public void WindowResized(int in_Width, int in_Height)
        {
            _windowWidth = in_Width;
            _windowHeight = in_Height;
        }

        public void DestroyDeviceObjects()
        {
            Dispose();
        }

        public void CreateDeviceResources()
        {
            _vertexBufferSize = 10000;
            _indexBufferSize = 2000;

            int prevVao = GL.GetInteger(GetPName.VertexArrayBinding);
            int prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);

            _vertexArray = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArray);
            LabelObject(ObjectLabelIdentifier.VertexArray, _vertexArray, "ImGui");

            _vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            LabelObject(ObjectLabelIdentifier.Buffer, _vertexBuffer, "VBO: ImGui");
            GL.BufferData(BufferTarget.ArrayBuffer, _vertexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            _indexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);
            LabelObject(ObjectLabelIdentifier.Buffer, _indexBuffer, "EBO: ImGui");
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            RecreateFontDeviceTexture();

            string vertexSource = @"#version 330 core

uniform mat4 projection_matrix;

layout(location = 0) in vec2 in_position;
layout(location = 1) in vec2 in_texCoord;
layout(location = 2) in vec4 in_color;

out vec4 color;
out vec2 texCoord;

void main()
{
    gl_Position = projection_matrix * vec4(in_position, 0, 1);
    color = in_color;
    texCoord = in_texCoord;
}";
            string fragmentSource = @"#version 330 core

uniform sampler2D in_fontTexture;

in vec4 color;
in vec2 texCoord;

out vec4 outputColor;

void main()
{
    outputColor = color * texture(in_fontTexture, texCoord);
}";

            _shader = CreateProgram("ImGui", vertexSource, fragmentSource);
            _shaderProjectionMatrixLocation = GL.GetUniformLocation(_shader, "projection_matrix");
            _shaderFontTextureLocation = GL.GetUniformLocation(_shader, "in_fontTexture");

            int stride = Unsafe.SizeOf<ImDrawVert>();
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 8);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, 16);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            GL.BindVertexArray(prevVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, prevArrayBuffer);

            CheckGlError("End of ImGui setup");
        }

        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        public unsafe void RecreateFontDeviceTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            byte* pixels;
            int width;
            int height;
            ImGui.GetTexDataAsRGBA32(io.Fonts, &pixels, &width, &height, null);

            int mips = (int)Math.Floor(Math.Log(Math.Max(width, height), 2));

            int prevActiveTexture = GL.GetInteger(GetPName.ActiveTexture);
            GL.ActiveTexture(TextureUnit.Texture0);
            int prevTexture2D = GL.GetInteger(GetPName.TextureBinding2D);

            _fontTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _fontTexture);
            GL.TexStorage2D(TextureTarget2d.Texture2D, mips, SizedInternalFormat.Rgba8, width, height);
            LabelObject(ObjectLabelIdentifier.Texture, _fontTexture, "ImGui Text Atlas");

            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, (IntPtr)pixels);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, mips - 1);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            // Restore state
            GL.BindTexture(TextureTarget.Texture2D, prevTexture2D);
            GL.ActiveTexture((TextureUnit)prevActiveTexture);

            io.Fonts.SetTexID(new ImTextureID(_fontTexture));

            io.Fonts.ClearTexData();
        }

        /// <summary>
        /// Renders the ImGui draw list data.
        /// </summary>
        public void Render()
        {
            if (_frameBegun)
            {
                _frameBegun = false;
                ImGui.Render();
                RenderImDrawData(ImGui.GetDrawData());
            }
        }

        /// <summary>
        /// Updates ImGui input and IO configuration state.
        /// </summary>
        public void Update(GameWindow in_Wnd, float in_DeltaSeconds)
        {
            if (_frameBegun)
            {
                ImGui.Render();
            }

            SetPerFrameImGuiData(in_DeltaSeconds);
            UpdateImGuiInput(in_Wnd);

            _frameBegun = true;
            ImGui.NewFrame();
        }

        /// <summary>
        /// Sets per-frame data based on the associated window.
        /// This is called by Update(float).
        /// </summary>
        private void SetPerFrameImGuiData(float in_DeltaSeconds)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new System.Numerics.Vector2(
                _windowWidth / _scaleFactor.X,
                _windowHeight / _scaleFactor.Y);
            io.DisplayFramebufferScale = _scaleFactor;
            io.DeltaTime = in_DeltaSeconds; // DeltaTime is in seconds.
        }

        private readonly List<char> _pressedChars = new List<char>();

        private void UpdateImGuiInput(GameWindow in_Wnd)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            MouseState mouseState = in_Wnd.MouseState;
            KeyboardState keyboardState = in_Wnd.KeyboardState;

            io.MouseDown[0] = mouseState[MouseButton.Left];
            io.MouseDown[1] = mouseState[MouseButton.Right];
            io.MouseDown[2] = mouseState[MouseButton.Middle];
            io.MouseDown[3] = mouseState[MouseButton.Button4];
            io.MouseDown[4] = mouseState[MouseButton.Button5];

            var screenPoint = new Vector2i((int)mouseState.X, (int)mouseState.Y);
            var point = screenPoint;//wnd.PointToClient(screenPoint);
            io.MousePos = new System.Numerics.Vector2(point.X, point.Y);

            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (key == Keys.Unknown)
                {
                    continue;
                }
                io.AddKeyEvent(TranslateKey(key), keyboardState.IsKeyDown(key));
            }

            foreach (var c in _pressedChars)
            {
                io.AddInputCharacter(c);
            }
            _pressedChars.Clear();

            io.KeyCtrl = keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl);
            io.KeyAlt = keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt);
            io.KeyShift = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);
            io.KeySuper = keyboardState.IsKeyDown(Keys.LeftSuper) || keyboardState.IsKeyDown(Keys.RightSuper);
        }

        internal void PressChar(char in_KeyChar)
        {
            _pressedChars.Add(in_KeyChar);
        }

        internal void MouseScroll(OpenTK.Mathematics.Vector2 in_Offset)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            io.MouseWheel = in_Offset.Y;
            io.MouseWheelH = in_Offset.X;
        }

        private unsafe void RenderImDrawData(ImDrawDataPtr in_DrawData)
        {
            if (in_DrawData.CmdListsCount == 0)
            {
                return;
            }

            // Get intial state.
            int prevVao = GL.GetInteger(GetPName.VertexArrayBinding);
            int prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);
            int prevProgram = GL.GetInteger(GetPName.CurrentProgram);
            bool prevBlendEnabled = GL.GetBoolean(GetPName.Blend);
            bool prevScissorTestEnabled = GL.GetBoolean(GetPName.ScissorTest);
            int prevBlendEquationRgb = GL.GetInteger(GetPName.BlendEquationRgb);
            int prevBlendEquationAlpha = GL.GetInteger(GetPName.BlendEquationAlpha);
            int prevBlendFuncSrcRgb = GL.GetInteger(GetPName.BlendSrcRgb);
            int prevBlendFuncSrcAlpha = GL.GetInteger(GetPName.BlendSrcAlpha);
            int prevBlendFuncDstRgb = GL.GetInteger(GetPName.BlendDstRgb);
            int prevBlendFuncDstAlpha = GL.GetInteger(GetPName.BlendDstAlpha);
            bool prevCullFaceEnabled = GL.GetBoolean(GetPName.CullFace);
            bool prevDepthTestEnabled = GL.GetBoolean(GetPName.DepthTest);
            int prevActiveTexture = GL.GetInteger(GetPName.ActiveTexture);
            GL.ActiveTexture(TextureUnit.Texture0);
            int prevTexture2D = GL.GetInteger(GetPName.TextureBinding2D);
            Span<int> prevScissorBox = stackalloc int[4];
            unsafe
            {
                fixed (int* iptr = &prevScissorBox[0])
                {
                    GL.GetInteger(GetPName.ScissorBox, iptr);
                }
            }
            Span<int> prevPolygonMode = stackalloc int[2];
            unsafe
            {
                fixed (int* iptr = &prevPolygonMode[0])
                {
                    GL.GetInteger(GetPName.PolygonMode, iptr);
                }
            }

            if (_glVersion <= 310 || _compatibilityProfile)
            {
                GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
                GL.PolygonMode(MaterialFace.Back, PolygonMode.Fill);
            }
            else
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }

            // Bind the element buffer (thru the VAO) so that we can resize it.
            GL.BindVertexArray(_vertexArray);
            // Bind the vertex buffer so that we can resize it.
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            for (int i = 0; i < in_DrawData.CmdListsCount; i++)
            {
                ImDrawListPtr cmdList = in_DrawData.CmdLists[i];

                int vertexSize = cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
                if (vertexSize > _vertexBufferSize)
                {
                    int newSize = (int)Math.Max(_vertexBufferSize * 1.5f, vertexSize);

                    GL.BufferData(BufferTarget.ArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    _vertexBufferSize = newSize;

                    Console.WriteLine($"Resized dear imgui vertex buffer to new size {_vertexBufferSize}");
                }

                int indexSize = cmdList.IdxBuffer.Size * sizeof(ushort);
                if (indexSize > _indexBufferSize)
                {
                    int newSize = (int)Math.Max(_indexBufferSize * 1.5f, indexSize);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    _indexBufferSize = newSize;

                    Console.WriteLine($"Resized dear imgui index buffer to new size {_indexBufferSize}");
                }
            }

            // Setup orthographic projection matrix into our constant buffer
            ImGuiIOPtr io = ImGui.GetIO();
            Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(
                0.0f,
                io.DisplaySize.X,
                io.DisplaySize.Y,
                0.0f,
                -1.0f,
                1.0f);

            GL.UseProgram(_shader);
            GL.UniformMatrix4(_shaderProjectionMatrixLocation, false, ref mvp);
            GL.Uniform1(_shaderFontTextureLocation, 0);
            CheckGlError("Projection");

            GL.BindVertexArray(_vertexArray);
            CheckGlError("VAO");

            in_DrawData.ScaleClipRects(io.DisplayFramebufferScale);


            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ScissorTest);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);

            // Render command lists
            for (int n = 0; n < in_DrawData.CmdListsCount; n++)
            {
                ImDrawListPtr cmdList = in_DrawData.CmdLists[n];

                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), (IntPtr)cmdList.VtxBuffer.Data);
                CheckGlError($"Data Vert {n}");

                GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, cmdList.IdxBuffer.Size * sizeof(ushort), (IntPtr)cmdList.IdxBuffer.Data);
                CheckGlError($"Data Idx {n}");

                for (int cmdI = 0; cmdI < cmdList.CmdBuffer.Size; cmdI++)
                {
                    var pcmd = cmdList.CmdBuffer[cmdI];
                    if (pcmd.UserCallback != null)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId.Handle);
                        CheckGlError("Texture");

                        // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
                        var clip = pcmd.ClipRect;
                        GL.Scissor((int)clip.X, _windowHeight - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));
                        CheckGlError("Scissor");

                        if ((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
                        {
                            GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(pcmd.IdxOffset * sizeof(ushort)), unchecked((int)pcmd.VtxOffset));
                        }
                        else
                        {
                            GL.DrawElements(BeginMode.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (int)pcmd.IdxOffset * sizeof(ushort));
                        }
                        CheckGlError("Draw");
                    }
                }
            }

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.ScissorTest);

            // Reset state
            GL.BindTexture(TextureTarget.Texture2D, prevTexture2D);
            GL.ActiveTexture((TextureUnit)prevActiveTexture);
            GL.UseProgram(prevProgram);
            GL.BindVertexArray(prevVao);
            GL.Scissor(prevScissorBox[0], prevScissorBox[1], prevScissorBox[2], prevScissorBox[3]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, prevArrayBuffer);
            GL.BlendEquationSeparate((BlendEquationMode)prevBlendEquationRgb, (BlendEquationMode)prevBlendEquationAlpha);
            GL.BlendFuncSeparate(
                (BlendingFactorSrc)prevBlendFuncSrcRgb,
                (BlendingFactorDest)prevBlendFuncDstRgb,
                (BlendingFactorSrc)prevBlendFuncSrcAlpha,
                (BlendingFactorDest)prevBlendFuncDstAlpha);
            if (prevBlendEnabled) GL.Enable(EnableCap.Blend); else GL.Disable(EnableCap.Blend);
            if (prevDepthTestEnabled) GL.Enable(EnableCap.DepthTest); else GL.Disable(EnableCap.DepthTest);
            if (prevCullFaceEnabled) GL.Enable(EnableCap.CullFace); else GL.Disable(EnableCap.CullFace);
            if (prevScissorTestEnabled) GL.Enable(EnableCap.ScissorTest); else GL.Disable(EnableCap.ScissorTest);
            if (_glVersion <= 310 || _compatibilityProfile)
            {
                GL.PolygonMode(MaterialFace.Front, (PolygonMode)prevPolygonMode[0]);
                GL.PolygonMode(MaterialFace.Back, (PolygonMode)prevPolygonMode[1]);
            }
            else
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, (PolygonMode)prevPolygonMode[0]);
            }
        }

        /// <summary>
        /// Frees all graphics resources used by the renderer.
        /// </summary>
        public void Dispose()
        {
            GL.DeleteVertexArray(_vertexArray);
            GL.DeleteBuffer(_vertexBuffer);
            GL.DeleteBuffer(_indexBuffer);

            GL.DeleteTexture(_fontTexture);
            GL.DeleteProgram(_shader);
        }

        public static void LabelObject(ObjectLabelIdentifier in_ObjLabelIdent, int in_GlObject, string in_Name)
        {
            if (_khrDebugAvailable)
                GL.ObjectLabel(in_ObjLabelIdent, in_GlObject, in_Name.Length, in_Name);
        }

        private static bool IsExtensionSupported(string in_Name)
        {
            int n = GL.GetInteger(GetPName.NumExtensions);
            for (int i = 0; i < n; i++)
            {
                string extension = GL.GetString(StringNameIndexed.Extensions, i);
                if (extension == in_Name) return true;
            }

            return false;
        }

        public static int CreateProgram(string in_Name, string in_VertexSource, string in_FragmentSoruce)
        {
            int program = GL.CreateProgram();
            LabelObject(ObjectLabelIdentifier.Program, program, $"Program: {in_Name}");

            int vertex = CompileShader(in_Name, ShaderType.VertexShader, in_VertexSource);
            int fragment = CompileShader(in_Name, ShaderType.FragmentShader, in_FragmentSoruce);

            GL.AttachShader(program, vertex);
            GL.AttachShader(program, fragment);

            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string info = GL.GetProgramInfoLog(program);
                Debug.WriteLine($"GL.LinkProgram had info log [{in_Name}]:\n{info}");
            }

            GL.DetachShader(program, vertex);
            GL.DetachShader(program, fragment);

            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);

            return program;
        }

        private static int CompileShader(string in_Name, ShaderType in_Type, string in_Source)
        {
            int shader = GL.CreateShader(in_Type);
            LabelObject(ObjectLabelIdentifier.Shader, shader, $"Shader: {in_Name}");

            GL.ShaderSource(shader, in_Source);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string info = GL.GetShaderInfoLog(shader);
                Debug.WriteLine($"GL.CompileShader for shader '{in_Name}' [{in_Type}] had info log:\n{info}");
            }

            return shader;
        }

        public static void CheckGlError(string in_Title)
        {
            ErrorCode error;
            int i = 1;
            while ((error = GL.GetError()) != ErrorCode.NoError)
            {
                Debug.Print($"{in_Title} ({i++}): {error}");
            }
        }

        public static ImGuiKey TranslateKey(Keys in_Key)
        {
            if (in_Key >= Keys.D0 && in_Key <= Keys.D9)
                return in_Key - Keys.D0 + ImGuiKey.Key0;

            if (in_Key >= Keys.A && in_Key <= Keys.Z)
                return in_Key - Keys.A + ImGuiKey.A;

            if (in_Key >= Keys.KeyPad0 && in_Key <= Keys.KeyPad9)
                return in_Key - Keys.KeyPad0 + ImGuiKey.Keypad0;

            if (in_Key >= Keys.F1 && in_Key <= Keys.F24)
                return in_Key - Keys.F1 + ImGuiKey.F24;

            switch (in_Key)
            {
                case Keys.Tab: return ImGuiKey.Tab;
                case Keys.Left: return ImGuiKey.LeftArrow;
                case Keys.Right: return ImGuiKey.RightArrow;
                case Keys.Up: return ImGuiKey.UpArrow;
                case Keys.Down: return ImGuiKey.DownArrow;
                case Keys.PageUp: return ImGuiKey.PageUp;
                case Keys.PageDown: return ImGuiKey.PageDown;
                case Keys.Home: return ImGuiKey.Home;
                case Keys.End: return ImGuiKey.End;
                case Keys.Insert: return ImGuiKey.Insert;
                case Keys.Delete: return ImGuiKey.Delete;
                case Keys.Backspace: return ImGuiKey.Backspace;
                case Keys.Space: return ImGuiKey.Space;
                case Keys.Enter: return ImGuiKey.Enter;
                case Keys.Escape: return ImGuiKey.Escape;
                case Keys.Apostrophe: return ImGuiKey.Apostrophe;
                case Keys.Comma: return ImGuiKey.Comma;
                case Keys.Minus: return ImGuiKey.Minus;
                case Keys.Period: return ImGuiKey.Period;
                case Keys.Slash: return ImGuiKey.Slash;
                case Keys.Semicolon: return ImGuiKey.Semicolon;
                case Keys.Equal: return ImGuiKey.Equal;
                case Keys.LeftBracket: return ImGuiKey.LeftBracket;
                case Keys.Backslash: return ImGuiKey.Backslash;
                case Keys.RightBracket: return ImGuiKey.RightBracket;
                case Keys.GraveAccent: return ImGuiKey.GraveAccent;
                case Keys.CapsLock: return ImGuiKey.CapsLock;
                case Keys.ScrollLock: return ImGuiKey.ScrollLock;
                case Keys.NumLock: return ImGuiKey.NumLock;
                case Keys.PrintScreen: return ImGuiKey.PrintScreen;
                case Keys.Pause: return ImGuiKey.Pause;
                case Keys.KeyPadDecimal: return ImGuiKey.KeypadDecimal;
                case Keys.KeyPadDivide: return ImGuiKey.KeypadDivide;
                case Keys.KeyPadMultiply: return ImGuiKey.KeypadMultiply;
                case Keys.KeyPadSubtract: return ImGuiKey.KeypadSubtract;
                case Keys.KeyPadAdd: return ImGuiKey.KeypadAdd;
                case Keys.KeyPadEnter: return ImGuiKey.KeypadEnter;
                case Keys.KeyPadEqual: return ImGuiKey.KeypadEqual;
                case Keys.LeftShift: return ImGuiKey.LeftShift;
                case Keys.LeftControl: return ImGuiKey.LeftCtrl;
                case Keys.LeftAlt: return ImGuiKey.LeftAlt;
                case Keys.LeftSuper: return ImGuiKey.LeftSuper;
                case Keys.RightShift: return ImGuiKey.RightShift;
                case Keys.RightControl: return ImGuiKey.RightCtrl;
                case Keys.RightAlt: return ImGuiKey.RightAlt;
                case Keys.RightSuper: return ImGuiKey.RightSuper;
                case Keys.Menu: return ImGuiKey.Menu;
                default: return ImGuiKey.None;
            }
        }
    }
}