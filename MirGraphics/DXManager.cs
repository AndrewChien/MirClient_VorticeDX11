using Client.MirControls;
using Client.MirScenes;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using static Client.MirGraphics.DX_Vortice;
using DX_Texture = Vortice.Direct3D11.ID3D11Texture2D;
using DX_Surface = Vortice.Direct3D11.ID3D11RenderTargetView;
//using Blend = SlimDX.Direct3D9.Blend;
//using DX_Texture = SlimDX.Direct3D9.Texture;
//using DX_Surface = SlimDX.Direct3D9.Surface;

namespace Client.MirGraphics
{
    class DXManager
    {
        public static List<MImage> TextureList = new List<MImage>();
        public static List<MirControl> ControlList = new List<MirControl>();

        public static bool DeviceLost;
        public static float Opacity = 1F;
        public static bool Blending;
        public static float BlendingRate;
        public static BlendMode BlendingMode;
        public static bool GrayScale;

        public static Point[] LightSizes =
        {
            new Point(125,95),
            new Point(205,156),
            new Point(285,217),
            new Point(365,277),
            new Point(445,338),
            new Point(525,399),
            new Point(605,460),
            new Point(685,521),
            new Point(765,581),
            new Point(845,642),
            new Point(925,703)
        };

        public static void Create()
        {
            DX_InitDevice();
            LoadTextures();
            LoadPixelsShaders();
        }

        private static unsafe void LoadPixelsShaders()
        {
            var shaderNormalPath = Settings.ShadersPath + "normal.ps";
            var shaderGrayScalePath = Settings.ShadersPath + "grayscale.ps";
            var shaderMagicPath = Settings.ShadersPath + "magic.ps";

            if (System.IO.File.Exists(shaderNormalPath))
            {
                DX_LoadPixelShader(shaderNormalPath, NormalPixelShader);
            }
            if (System.IO.File.Exists(shaderGrayScalePath))
            {
                DX_LoadPixelShader(shaderGrayScalePath, GrayScalePixelShader);
            }
            if (System.IO.File.Exists(shaderMagicPath))
            {
                DX_LoadPixelShader(shaderMagicPath, MagicPixelShader);
            }
        }

        private static unsafe void LoadTextures()
        {
            DX_InitSprite();

            DX_InitRenderTarget();
            
            //if (RadarTexture == null || RadarTexture.Disposed)
            if (RadarTexture == null)
            {
                DX_InitRadarTexture();
            }
            //if (PoisonDotBackground == null || PoisonDotBackground.Disposed)
            if (PoisonDotBackground == null)
            {
                DX_InitPoisonDotBackgroundTexture();
            }
            CreateLights();
        }

        private unsafe static void CreateLights()
        {

            for (int i = Lights.Count - 1; i >= 0; i--)
                Lights[i].Dispose();

            Lights.Clear();

            for (int i = 1; i < LightSizes.Length; i++)
            {
                // int width = 125 + (57 *i);
                //int height = 110 + (57 * i);
                int width = LightSizes[i].X;
                int height = LightSizes[i].Y;


                //light.Disposing += (o, e) => Lights.Remove(light);
                Lights.Add(DX_InitLightTexture(width, height));
            }
        }

        public static void SetSurface(DX_Surface surface)
        {
            if (CurrentSurface == surface)
                return;

            DX_Flush();

            CurrentSurface = surface;

            DX_SetRenderTarget(0,ref surface);
        }

        public static void SetGrayscale(bool value)
        {
            GrayScale = value;

            if (value == true)
            {
                if (DX_DevicePixelShader == GrayScalePixelShader)
                    return;
                DX_Flush();
                DX_DevicePixelShader = GrayScalePixelShader;
            }
            else
            {
                if (DX_DevicePixelShader == null)
                    return;
                DX_Flush();
                DX_DevicePixelShader = null;
            }
        }

        public static void DrawOpaque(DX_Texture texture, Rectangle? sourceRect, System.Numerics.Vector3? position, System.Drawing.Color color, float opacity)
        {
            //color.Alpha = opacity;
            Draw(texture, sourceRect, position, color);
        }

        public static void Draw(DX_Texture texture, Rectangle? sourceRect, System.Numerics.Vector3? position, System.Drawing.Color color)
        {
            DX_Draw(texture, sourceRect, position, color);
            CMain.DPSCounter++;
        }

        public static void AttemptReset()
        {
            DeviceLost = false;
            DX_AttemptReset(ResetDevice, () => DeviceLost = false);
        }

        public static void ResetDevice()
        {
            CleanUp();
            DeviceLost = true;

            //if (Parameters == null)
            //    return;

            Size clientSize = Program.Form.ClientSize;

            if (clientSize.Width == 0 || clientSize.Height == 0)
                return;

            DX_DeviceReset(clientSize);

            LoadTextures();
        }

        public static void AttemptRecovery()
        {
            try
            {
                DX_SpriteEnd();
            }
            catch
            {
            }

            try
            {
                DX_DeviceEndScene();
            }
            catch
            {
            }

            try
            {
                DX_InitRenderTarget();
            }
            catch
            {
            }
        }
        public static void SetOpacity(float opacity)
        {
            if (Opacity == opacity)
                return;

            DX_Flush();

            DX_SetOpacity(opacity);

            Opacity = opacity;
            DX_Flush();
        }
        public static void SetBlend(bool value, float rate = 1F, BlendMode mode = BlendMode.NORMAL)
        {
            if (value == Blending && BlendingRate == rate && BlendingMode == mode)
                return;

            Blending = value;
            BlendingRate = rate;
            BlendingMode = mode;

            DX_Flush();
            DX_SpriteEnd();
            if (Blending)
            {
                DX_SpriteBeginDoNotSaveState();
                DX_SetRenderStateTrue();
                switch (BlendingMode)
                {
                    case BlendMode.INVLIGHT:
                        DX_DeviceSetRenderStateAddBlendFactorInverseSourceColor();
                        break;
                    default:
                        DX_DeviceSetRenderStateSourceAlphaOne();
                        break;
                }
                DX_DeviceSetRenderStateARGB((byte)(255 * BlendingRate), (byte)(255 * BlendingRate),
                                                                (byte)(255 * BlendingRate), (byte)(255 * BlendingRate));
            }
            else
                DX_SpriteBeginAlphaBlend();
            DX_SetRenderTarget(0,ref CurrentSurface);
        }

        public static void SetNormal(float blend, Color tintcolor)
        {
            if (DX_DevicePixelShader == NormalPixelShader)
                return;

            DX_Flush();
            DX_DevicePixelShader = NormalPixelShader;
            DX_SetNormal(blend, tintcolor);
            DX_Flush();
        }

        public static void SetGrayscale(float blend, Color tintcolor)
        {
            if (DX_DevicePixelShader == GrayScalePixelShader)
                return;

            DX_Flush();
            DX_DevicePixelShader = GrayScalePixelShader;
            DX_SetGrayscale(blend, tintcolor);
            DX_Flush();
        }

        public static void SetBlendMagic(float blend, Color tintcolor)
        {
            if (DX_DevicePixelShader == MagicPixelShader || MagicPixelShader == null)
                return;

            DX_Flush();
            DX_DevicePixelShader = MagicPixelShader;
            DX_SetBlendMagic(blend, tintcolor);
            DX_Flush();
        }

        public static void Clean()
        {
            for (int i = TextureList.Count - 1; i >= 0; i--)
            {
                MImage m = TextureList[i];

                if (m == null)
                {
                    TextureList.RemoveAt(i);
                    continue;
                }

                if (CMain.Time <= m.CleanTime) continue;

                m.DisposeTexture();
            }

            for (int i = ControlList.Count - 1; i >= 0; i--)
            {
                MirControl c = ControlList[i];

                if (c == null)
                {
                    ControlList.RemoveAt(i);
                    continue;
                }

                if (CMain.Time <= c.CleanTime) continue;

                c.DisposeTexture();
            }
        }

        private static void CleanUp()
        {
            if (Sprite != null)
            {
                //if (!Sprite.Disposed)
                {
                    Sprite.Dispose();
                }

                Sprite = null;
            }

            if (Line != null)
            {
                //if (!Line.Disposed)
                {
                    Line.Dispose();
                }

                Line = null;
            }

            if (CurrentSurface != null)
            {
                //if (!CurrentSurface.Disposed)
                {
                    CurrentSurface.Dispose();
                }

                CurrentSurface = null;
            }

            if (PoisonDotBackground != null)
            {
                //if (!PoisonDotBackground.Disposed)
                {
                    PoisonDotBackground.Dispose();
                }

                PoisonDotBackground = null;
            }

            if (RadarTexture != null)
            {
                //if (!RadarTexture.Disposed)
                {
                    RadarTexture.Dispose();
                }

                RadarTexture = null;
            }

            if (FloorTexture != null)
            {
                //if (!FloorTexture.Disposed)
                {
                    FloorTexture.Dispose();
                }

                FloorTexture = null;
                GameScene.Scene.MapControl.FloorValid = false;
                
                //if (FloorSurface != null && !FloorSurface.Disposed)
                if (FloorSurface != null)
                {
                    FloorSurface.Dispose();
                }

                FloorSurface = null;
            }

            if (LightTexture != null)
            {
                //if (!LightTexture.Disposed)
                    LightTexture.Dispose();

                LightTexture = null;
                
                //if (LightSurface != null && !LightSurface.Disposed)
                if (LightSurface != null)
                {
                    LightSurface.Dispose();
                }

                LightSurface = null;
            }

            if (Lights != null)
            {
                for (int i = 0; i < Lights.Count; i++)
                {
                    //if (!Lights[i].Disposed)
                        Lights[i].Dispose();
                }
                Lights.Clear();
            }

            for (int i = TextureList.Count - 1; i >= 0; i--)
            {
                MImage m = TextureList[i];

                if (m == null) continue;

                m.DisposeTexture();
            }
            TextureList.Clear();


            for (int i = ControlList.Count - 1; i >= 0; i--)
            {
                MirControl c = ControlList[i];

                if (c == null) continue;

                c.DisposeTexture();
            }
            ControlList.Clear();
        }

        public static void Dispose()
        {
            CleanUp();

            //Device.Direct3D?.Dispose();

            if (Program.Form.WindowState != FormWindowState.Normal)
            {
                Device.Dispose();
            }

            NormalPixelShader?.Dispose();
            GrayScalePixelShader?.Dispose();
            MagicPixelShader?.Dispose();
        }



        //#region SlimDX可替换部分

        //public static Device Device;
        //public static Sprite Sprite;
        //public static Line Line;

        //public static Surface CurrentSurface;
        //public static Surface MainSurface;
        //public static PresentParameters Parameters;

        //public static Texture RadarTexture;
        //public static Texture PoisonDotBackground;
        //public static List<Texture> Lights = new List<Texture>();

        //public static Texture FloorTexture, LightTexture;
        //public static Surface FloorSurface, LightSurface;

        //public static PixelShader GrayScalePixelShader;
        //public static PixelShader NormalPixelShader;
        //public static PixelShader MagicPixelShader;

        ///// <summary>
        ///// 封装类
        ///// </summary>
        //public class DX_Direct3D9Exception : Direct3D9Exception
        //{

        //}
        ///// <summary>
        ///// 
        ///// </summary>
        //public static PixelShader DX_DevicePixelShader
        //{
        //    get { return Device.PixelShader; }
        //    set { Device.PixelShader = value; }
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        //public static bool DX_ParametersWindowed
        //{
        //    get { return Parameters.Windowed; }
        //    set { Parameters.Windowed = value; }
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        //public static void DX_InitDevice()
        //{
        //    Parameters = new PresentParameters
        //    {
        //        BackBufferFormat = Format.X8R8G8B8,
        //        PresentFlags = PresentFlags.LockableBackBuffer,
        //        BackBufferWidth = Settings.ScreenWidth,
        //        BackBufferHeight = Settings.ScreenHeight,
        //        SwapEffect = SwapEffect.Discard,
        //        PresentationInterval = Settings.FPSCap ? PresentInterval.One : PresentInterval.Immediate,
        //        Windowed = !Settings.FullScreen,
        //    };


        //    Direct3D d3d = new Direct3D();

        //    Capabilities devCaps = d3d.GetDeviceCaps(0, DeviceType.Hardware);
        //    DeviceType devType = DeviceType.Reference;
        //    CreateFlags devFlags = CreateFlags.HardwareVertexProcessing;

        //    if (devCaps.VertexShaderVersion.Major >= 2 && devCaps.PixelShaderVersion.Major >= 2)
        //        devType = DeviceType.Hardware;

        //    if ((devCaps.DeviceCaps & DeviceCaps.HWTransformAndLight) != 0)
        //        devFlags = CreateFlags.HardwareVertexProcessing;


        //    if ((devCaps.DeviceCaps & DeviceCaps.PureDevice) != 0)
        //        devFlags |= CreateFlags.PureDevice;


        //    Device = new Device(d3d, d3d.Adapters.DefaultAdapter.Adapter, devType, Program.Form.Handle, devFlags, Parameters);

        //    Device.SetDialogBoxMode(true);
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        //public static void DX_InitSprite()
        //{
        //    Sprite = new Sprite(Device);
        //    Line = new Line(Device) { Width = 1F };
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        //public static void DX_InitRenderTarget()
        //{
        //    MainSurface = DX_DeviceGetBackBuffer00();
        //    CurrentSurface = MainSurface;
        //    DX_SetRenderTarget(0, MainSurface);
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //public static Surface DX_DeviceGetBackBuffer00()
        //{
        //    return Device.GetBackBuffer(0, 0);
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //public static DataStream DX_DeviceGetBackBuffer00Stream()
        //{
        //    Surface backbuffer = DX_DeviceGetBackBuffer00();
        //    return Surface.ToStream(backbuffer, ImageFileFormat.Png);
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        //public static void DX_InitRadarTexture()
        //{
        //    RadarTexture = DX_NewTextureNoneManaged(2, 2);

        //    DataRectangle stream = DX_TextureLockRectangle(RadarTexture);
        //    using (Bitmap image = new Bitmap(2, 2, 8, PixelFormat.Format32bppArgb, DX_DataRectanglePointer(stream)))
        //    using (Graphics graphics = Graphics.FromImage(image))
        //        graphics.Clear(Color.White);
        //    DX_TextureUnlockRectangle(RadarTexture);
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        //public static void DX_InitPoisonDotBackgroundTexture()
        //{
        //    PoisonDotBackground = DX_NewTextureNoneManaged(5, 5);

        //    DataRectangle stream = DX_TextureLockRectangle(PoisonDotBackground);
        //    using (Bitmap image = new Bitmap(5, 5, 20, PixelFormat.Format32bppArgb, DX_DataRectanglePointer(stream)))
        //    using (Graphics graphics = Graphics.FromImage(image))
        //        graphics.Clear(Color.White);
        //    DX_TextureUnlockRectangle(PoisonDotBackground);
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="width"></param>
        ///// <param name="height"></param>
        ///// <returns></returns>
        //public static Texture DX_InitLightTexture(int width, int height)
        //{
        //    Texture light = DX_NewTextureNoneManaged(width, height);

        //    DataRectangle stream = DX_TextureLockRectangle(light);
        //    using (Bitmap image = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, DX_DataRectanglePointer(stream)))
        //    {
        //        using (Graphics graphics = Graphics.FromImage(image))
        //        {
        //            using (GraphicsPath path = new GraphicsPath())
        //            {
        //                //path.AddEllipse(new Rectangle(0, 0, width, height));
        //                //using (PathGradientBrush brush = new PathGradientBrush(path))
        //                //{
        //                //    graphics.Clear(Color.FromArgb(0, 0, 0, 0));
        //                //    brush.SurroundColors = new[] { Color.FromArgb(0, 255, 255, 255) };
        //                //    brush.CenterColor = Color.FromArgb(255, 255, 255, 255);
        //                //    graphics.FillPath(brush, path);
        //                //    graphics.Save();
        //                //}

        //                path.AddEllipse(new Rectangle(0, 0, width, height));
        //                using (PathGradientBrush brush = new PathGradientBrush(path))
        //                {
        //                    Color[] blendColours = { Color.White,
        //                                             Color.FromArgb(255,210,210,210),
        //                                             Color.FromArgb(255,160,160,160),
        //                                             Color.FromArgb(255,70,70,70),
        //                                             Color.FromArgb(255,40,40,40),
        //                                             Color.FromArgb(0,0,0,0)};

        //                    float[] radiusPositions = { 0f, .20f, .40f, .60f, .80f, 1.0f };

        //                    ColorBlend colourBlend = new ColorBlend();
        //                    colourBlend.Colors = blendColours;
        //                    colourBlend.Positions = radiusPositions;

        //                    graphics.Clear(Color.FromArgb(0, 0, 0, 0));
        //                    brush.InterpolationColors = colourBlend;
        //                    brush.SurroundColors = blendColours;
        //                    brush.CenterColor = Color.White;
        //                    graphics.FillPath(brush, path);
        //                    graphics.Save();
        //                }
        //            }
        //        }
        //    }

        //    DX_TextureUnlockRectangle(light);

        //    return light;
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        //public static void DX_Flush()
        //{
        //    Sprite.Flush();
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="targetindex"></param>
        ///// <param name="surface"></param>
        //public static void DX_SetRenderTarget(int targetindex, Surface surface)
        //{
        //    Device.SetRenderTarget(targetindex, surface);
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="texture"></param>
        ///// <param name="sourceRect"></param>
        ///// <param name="position"></param>
        ///// <param name="color"></param>
        //public static void DX_Draw(Texture texture, Rectangle? sourceRect, System.Numerics.Vector3? position, Color4 color)
        //{
        //    Sprite.Draw(texture, sourceRect, Vector3.Zero, ParseVector3(position), color);
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="vertexList"></param>
        ///// <param name="color"></param>
        //public static void DX_LineDraw(Vector2[] vertexList, Color4 color)
        //{
        //    Line.Draw(vertexList, color);
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="data"></param>
        ///// <returns></returns>
        //private static Vector3? ParseVector3(System.Numerics.Vector3? data)
        //{
        //    return new Vector3(data.Value.X, data.Value.Y, data.Value.Z);
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        //public static void DX_AttemptReset(Action action1, Action action2)
        //{
        //    try
        //    {
        //        Result result = Device.TestCooperativeLevel();

        //        if (result.Code == ResultCode.DeviceLost.Code) return;

        //        if (result.Code == ResultCode.DeviceNotReset.Code)
        //        {
        //            action1?.Invoke();
        //            return;
        //        }

        //        if (result.Code != ResultCode.Success.Code) return;

        //        action2?.Invoke();
        //    }
        //    catch
        //    {
        //    }
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="clientSize"></param>
        //public static void DX_DeviceReset(Size clientSize)
        //{
        //    DX_ParametersWindowed = !Settings.FullScreen;
        //    Parameters.BackBufferWidth = clientSize.Width;
        //    Parameters.BackBufferHeight = clientSize.Height;
        //    Parameters.PresentationInterval = Settings.FPSCap ? PresentInterval.Default : PresentInterval.Immediate;
        //    Device.Reset(Parameters);
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        //public static void DX_SpriteEnd()
        //{
        //    Sprite.End();
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        //public static void DX_DeviceEndScene()
        //{
        //    Device.EndScene();
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        //public static void DX_SpriteBeginDoNotSaveState()
        //{
        //    Sprite.Begin(SpriteFlags.DoNotSaveState);
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        //public static void DX_SpriteBeginAlphaBlend()
        //{
        //    Sprite.Begin(SpriteFlags.AlphaBlend);
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="opacity"></param>
        //public static void DX_SetOpacity(float opacity)
        //{
        //    DX_SetRenderStateTrue();
        //    if (opacity >= 1 || opacity < 0)
        //    {
        //        Device.SetRenderState(RenderState.SourceBlend, SlimDX.Direct3D9.Blend.SourceAlpha);
        //        Device.SetRenderState(RenderState.DestinationBlend, SlimDX.Direct3D9.Blend.InverseSourceAlpha);
        //        Device.SetRenderState(RenderState.SourceBlendAlpha, Blend.One);
        //        DX_DeviceSetRenderStateARGB(255, 255, 255, 255);
        //    }
        //    else
        //    {
        //        Device.SetRenderState(RenderState.SourceBlend, Blend.BlendFactor);
        //        Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseBlendFactor);
        //        Device.SetRenderState(RenderState.SourceBlendAlpha, Blend.SourceAlpha);
        //        DX_DeviceSetRenderStateARGB((byte)(255 * opacity), (byte)(255 * opacity), (byte)(255 * opacity), (byte)(255 * opacity));
        //    }
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        //public static void DX_DeviceSetRenderStateSourceAlphaOne()
        //{
        //    Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
        //    Device.SetRenderState(RenderState.DestinationBlend, Blend.One);
        //}
        //public static void DX_DeviceSetRenderStateAddBlendFactorInverseSourceColor()
        //{
        //    Device.SetRenderState(RenderState.BlendOperation, BlendOperation.Add);
        //    Device.SetRenderState(RenderState.SourceBlend, Blend.BlendFactor);
        //    Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceColor);
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        //public static void DX_DeviceSetRenderStateZeroSourceColor()
        //{
        //    Device.SetRenderState(RenderState.SourceBlend, Blend.Zero);
        //    Device.SetRenderState(RenderState.DestinationBlend, Blend.SourceColor);
        //}
        //public static void DX_SetRenderStateTrue()
        //{
        //    Device.SetRenderState(RenderState.AlphaBlendEnable, true);
        //}
        //public static void DX_DeviceSetRenderStateARGB(int a, int r, int g, int b)
        //{
        //    Device.SetRenderState(RenderState.BlendFactor, Color.FromArgb(a, r, g, b).ToArgb());
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="blend"></param>
        ///// <param name="tintcolor"></param>
        //public static void DX_SetNormal(float blend, Color tintcolor)
        //{
        //    Device.SetPixelShaderConstant(0, new Vector4[] { new Vector4(1.0F, 1.0F, 1.0F, blend) });
        //    Device.SetPixelShaderConstant(1, new Vector4[] { new Vector4(tintcolor.R / 255, tintcolor.G / 255, tintcolor.B / 255, 1.0F) });
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="blend"></param>
        ///// <param name="tintcolor"></param>
        //public static void DX_SetGrayscale(float blend, Color tintcolor)
        //{
        //    Device.SetPixelShaderConstant(0, new Vector4[] { new Vector4(1.0F, 1.0F, 1.0F, blend) });
        //    Device.SetPixelShaderConstant(1, new Vector4[] { new Vector4(tintcolor.R / 255, tintcolor.G / 255, tintcolor.B / 255, 1.0F) });
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="blend"></param>
        ///// <param name="tintcolor"></param>
        //public static void DX_SetBlendMagic(float blend, Color tintcolor)
        //{
        //    Device.SetPixelShaderConstant(0, new Vector4[] { new Vector4(1.0F, 1.0F, 1.0F, blend) });
        //    Device.SetPixelShaderConstant(1, new Vector4[] { new Vector4(tintcolor.R / 255, tintcolor.G / 255, tintcolor.B / 255, 1.0F) });
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="color"></param>
        ///// <param name="zdepth"></param>
        ///// <param name="stencil"></param>
        //public static void DX_DeviceClearTarget00(Color color)
        //{
        //    Device.Clear(ClearFlags.Target, color, 0, 0);
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        //public static void DX_DeviceBeginScene()
        //{
        //    Device.BeginScene();
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        //public static void DX_DevicePresent()
        //{
        //    Device.Present();
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="width"></param>
        ///// <param name="height"></param>
        ///// <returns></returns>
        //public static Texture DX_NewTextureRenderTargetDefault(int width, int height)
        //{
        //    return new Texture(Device, width, height, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="width"></param>
        ///// <param name="height"></param>
        ///// <returns></returns>
        //public static Texture DX_NewTextureNoneManaged(int width, int height)
        //{
        //    return new Texture(Device, width, height, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="texture"></param>
        ///// <returns></returns>
        //public static Surface DX_GetSurfaceLevel0(Texture texture)
        //{
        //    return texture.GetSurfaceLevel(0);
        //}
        //public static DataRectangle DX_TextureLockRectangle(Texture texture)
        //{
        //    return texture.LockRectangle(0, LockFlags.Discard);
        //}
        //public static void DX_TextureUnlockRectangle(Texture texture)
        //{
        //    texture.UnlockRectangle(0);
        //}
        //public static nint DX_DataRectanglePointer(DataRectangle dataRectangle)
        //{
        //    return dataRectangle.Data.DataPointer;
        //}
        //public static SlimDX.Matrix DX_MatrixScaling(float x, float y)
        //{
        //    return SlimDX.Matrix.Scaling(x, y, 0);
        //}
        //public static SlimDX.Matrix DX_SpriteTransform
        //{
        //    get { return Sprite.Transform; }
        //    set { Sprite.Transform = value; }
        //}
        //public static SlimDX.Matrix DX_MatrixIdentity()
        //{
        //    return SlimDX.Matrix.Identity;
        //}











        //#endregion
    }
}