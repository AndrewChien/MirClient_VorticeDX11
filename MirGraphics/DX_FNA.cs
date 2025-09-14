using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.MirGraphics
{
    public class DX_FNA
    {
        //#region SlimDX可替换部分

        ////设备
        //public static Device Device;
        //public static PresentParameters Parameters;

        ////精灵
        //public static Sprite Sprite;
        //public static Line Line;

        ////公用表面
        //public static Surface CurrentSurface;
        //public static Surface MainSurface;

        ////纹理
        //public static Texture RadarTexture;
        //public static Texture PoisonDotBackground;
        //public static List<Texture> Lights = new List<Texture>();

        ////地板灯光纹理、表面
        //public static Texture FloorTexture, LightTexture;
        //public static Surface FloorSurface, LightSurface;

        ////像素着色器
        //public static PixelShader GrayScalePixelShader;
        //public static PixelShader NormalPixelShader;
        //public static PixelShader MagicPixelShader;

        //#region 属性访问器

        ///// <summary>
        ///// 封装类
        ///// </summary>
        //public class DX_Direct3D9Exception : Direct3D9Exception
        //{

        //}
        ///// <summary>
        ///// 获取设备像素着色器
        ///// </summary>
        //public static PixelShader DX_DevicePixelShader
        //{
        //    get { return Device.PixelShader; }
        //    set { Device.PixelShader = value; }
        //}
        ///// <summary>
        ///// 窗口模式
        ///// </summary>
        //public static bool DX_ParametersWindowed
        //{
        //    get { return Parameters.Windowed; }
        //    set { Parameters.Windowed = value; }
        //}

        //#endregion

        //#region 像素着色器加载

        ///// <summary>
        ///// 从文件加载像素着色器
        ///// </summary>
        ///// <param name="path"></param>
        ///// <param name="pixelShader"></param>
        //public static void DX_LoadPixelShader(string path, PixelShader pixelShader)
        //{
        //    using (var gs = ShaderBytecode.AssembleFromFile(path, ShaderFlags.None))
        //        pixelShader = new PixelShader(Device, gs);
        //}
        ///// <summary>
        ///// 给像素着色器设置常量值
        ///// </summary>
        ///// <param name="blend"></param>
        ///// <param name="tintcolor"></param>
        //public static void DX_SetNormal(float blend, Color tintcolor)
        //{
        //    Device.SetPixelShaderConstant(0, new Vector4[] { new Vector4(1.0F, 1.0F, 1.0F, blend) });
        //    Device.SetPixelShaderConstant(1, new Vector4[] { new Vector4(tintcolor.R / 255, tintcolor.G / 255, tintcolor.B / 255, 1.0F) });
        //}
        ///// <summary>
        ///// 给像素着色器设置常量值
        ///// </summary>
        ///// <param name="blend"></param>
        ///// <param name="tintcolor"></param>
        //public static void DX_SetGrayscale(float blend, Color tintcolor)
        //{
        //    Device.SetPixelShaderConstant(0, new Vector4[] { new Vector4(1.0F, 1.0F, 1.0F, blend) });
        //    Device.SetPixelShaderConstant(1, new Vector4[] { new Vector4(tintcolor.R / 255, tintcolor.G / 255, tintcolor.B / 255, 1.0F) });
        //}
        ///// <summary>
        ///// 给像素着色器设置常量值
        ///// </summary>
        ///// <param name="blend"></param>
        ///// <param name="tintcolor"></param>
        //public static void DX_SetBlendMagic(float blend, Color tintcolor)
        //{
        //    Device.SetPixelShaderConstant(0, new Vector4[] { new Vector4(1.0F, 1.0F, 1.0F, blend) });
        //    Device.SetPixelShaderConstant(1, new Vector4[] { new Vector4(tintcolor.R / 255, tintcolor.G / 255, tintcolor.B / 255, 1.0F) });
        //}

        //#endregion

        //#region 设备、精灵、表面、后缓冲区、后缓冲区流、纹理初始化

        ///// <summary>
        ///// 设备初始化
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
        ///// 设备状态重置
        ///// </summary>
        //public static void DX_AttemptReset(Action action1, Action action2)
        //{
        //    try
        //    {
        //        Result result = Device.TestCooperativeLevel();

        //        if (result.Code == ResultCode.DeviceLost.Code)
        //            return;

        //        if (result.Code == ResultCode.DeviceNotReset.Code)
        //        {
        //            action1?.Invoke();
        //            return;
        //        }

        //        if (result.Code != ResultCode.Success.Code)
        //            return;

        //        action2?.Invoke();
        //    }
        //    catch
        //    {
        //    }
        //}
        ///// <summary>
        ///// 设备重置
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
        ///// 精灵初始化
        ///// </summary>
        //public static void DX_InitSprite()
        //{
        //    Sprite = new Sprite(Device);
        //    Line = new Line(Device) { Width = 1F };
        //}
        ///// <summary>
        ///// 表面初始化
        ///// </summary>
        //public static void DX_InitRenderTarget()
        //{
        //    MainSurface = DX_DeviceGetBackBuffer00();
        //    CurrentSurface = MainSurface;
        //    DX_SetRenderTarget(0, MainSurface);
        //}
        ///// <summary>
        ///// 获取后缓冲区
        ///// </summary>
        ///// <returns></returns>
        //public static Surface DX_DeviceGetBackBuffer00()
        //{
        //    return Device.GetBackBuffer(0, 0);
        //}
        ///// <summary>
        ///// 获取后缓冲区流
        ///// </summary>
        ///// <returns></returns>
        //public static DataStream DX_DeviceGetBackBuffer00Stream()
        //{
        //    Surface backbuffer = DX_DeviceGetBackBuffer00();
        //    return Surface.ToStream(backbuffer, ImageFileFormat.Png);
        //}
        ///// <summary>
        ///// 初始化雷达纹理
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
        ///// 初始化毒雾纹理
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
        ///// 初始化灯光纹理
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

        //#endregion

        //#region 渲染执行相关

        ///// <summary>
        ///// 设置渲染目标
        ///// </summary>
        ///// <param name="targetindex"></param>
        ///// <param name="surface"></param>
        //public static void DX_SetRenderTarget(int targetindex, Surface surface)
        //{
        //    Device.SetRenderTarget(targetindex, surface);
        //}
        ///// <summary>
        ///// 清屏
        ///// </summary>
        ///// <param name="color"></param>
        ///// <param name="zdepth"></param>
        ///// <param name="stencil"></param>
        //public static void DX_DeviceClearTarget00(Color color)
        //{
        //    Device.Clear(ClearFlags.Target, color, 0, 0);
        //}
        ///// <summary>
        ///// 立即呈现
        ///// </summary>
        //public static void DX_DevicePresent()
        //{
        //    Device.Present();
        //}
        ///// <summary>
        ///// 开始场景
        ///// </summary>
        //public static void DX_DeviceBeginScene()
        //{
        //    Device.BeginScene();
        //}
        ///// <summary>
        ///// 结束场景
        ///// </summary>
        //public static void DX_DeviceEndScene()
        //{
        //    Device.EndScene();
        //}
        ///// <summary>
        ///// 开始渲染（无状态）
        ///// </summary>
        //public static void DX_SpriteBeginDoNotSaveState()
        //{
        //    Sprite.Begin(SpriteFlags.DoNotSaveState);
        //}
        ///// <summary>
        ///// 开始渲染
        ///// </summary>
        //public static void DX_SpriteBeginAlphaBlend()
        //{
        //    Sprite.Begin(SpriteFlags.AlphaBlend);
        //}
        ///// <summary>
        ///// 结束渲染
        ///// </summary>
        //public static void DX_SpriteEnd()
        //{
        //    Sprite.End();
        //}
        ///// <summary>
        ///// 立即渲染
        ///// </summary>
        //public static void DX_Flush()
        //{
        //    Sprite.Flush();
        //}
        ///// <summary>
        ///// 画图像
        ///// </summary>
        ///// <param name="texture"></param>
        ///// <param name="sourceRect"></param>
        ///// <param name="position"></param>
        ///// <param name="color"></param>
        //public static void DX_Draw(Texture texture, Rectangle? sourceRect, System.Numerics.Vector3? position, System.Drawing.Color color)
        //{
        //    Sprite.Draw(texture, sourceRect, Vector3.Zero, ParseVector3(position), color);
        //}
        ///// <summary>
        ///// 画线
        ///// </summary>
        ///// <param name="vertexList"></param>
        ///// <param name="color"></param>
        //public static void DX_LineDraw(Vector2[] vertexList, Color4 color)
        //{
        //    Line.Draw(vertexList, color);
        //}
        ///// <summary>
        ///// Vector3转换
        ///// </summary>
        ///// <param name="data"></param>
        ///// <returns></returns>
        //private static Vector3? ParseVector3(System.Numerics.Vector3? data)
        //{
        //    return new Vector3(data.Value.X, data.Value.Y, data.Value.Z);
        //}

        //#endregion

        //#region 设置渲染状态

        ///// <summary>
        ///// 设置透明度
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
        ///// 设置渲染状态
        ///// </summary>
        //public static void DX_DeviceSetRenderStateSourceAlphaOne()
        //{
        //    Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
        //    Device.SetRenderState(RenderState.DestinationBlend, Blend.One);
        //}
        ///// <summary>
        ///// 设置渲染状态
        ///// </summary>
        //public static void DX_DeviceSetRenderStateAddBlendFactorInverseSourceColor()
        //{
        //    Device.SetRenderState(RenderState.BlendOperation, BlendOperation.Add);
        //    Device.SetRenderState(RenderState.SourceBlend, Blend.BlendFactor);
        //    Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceColor);
        //}
        ///// <summary>
        ///// 设置渲染状态
        ///// </summary>
        //public static void DX_DeviceSetRenderStateZeroSourceColor()
        //{
        //    Device.SetRenderState(RenderState.SourceBlend, Blend.Zero);
        //    Device.SetRenderState(RenderState.DestinationBlend, Blend.SourceColor);
        //}
        ///// <summary>
        ///// 设置渲染状态
        ///// </summary>
        //public static void DX_SetRenderStateTrue()
        //{
        //    Device.SetRenderState(RenderState.AlphaBlendEnable, true);
        //}
        ///// <summary>
        ///// 设置渲染状态
        ///// </summary>
        ///// <param name="a"></param>
        ///// <param name="r"></param>
        ///// <param name="g"></param>
        ///// <param name="b"></param>
        //public static void DX_DeviceSetRenderStateARGB(int a, int r, int g, int b)
        //{
        //    Device.SetRenderState(RenderState.BlendFactor, Color.FromArgb(a, r, g, b).ToArgb());
        //}

        //#endregion

        //#region 纹理创建、赋值

        ///// <summary>
        ///// 新建纹理（渲染目标）
        ///// </summary>
        ///// <param name="width"></param>
        ///// <param name="height"></param>
        ///// <returns></returns>
        //public static Texture DX_NewTextureRenderTargetDefault(int width, int height)
        //{
        //    return new Texture(Device, width, height, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
        //}
        ///// <summary>
        ///// 新建纹理（托管）
        ///// </summary>
        ///// <param name="width"></param>
        ///// <param name="height"></param>
        ///// <returns></returns>
        //public static Texture DX_NewTextureNoneManaged(int width, int height)
        //{
        //    return new Texture(Device, width, height, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
        //}
        ///// <summary>
        ///// 获取表面等级
        ///// </summary>
        ///// <param name="texture"></param>
        ///// <returns></returns>
        //public static Surface DX_GetSurfaceLevel0(Texture texture)
        //{
        //    return texture.GetSurfaceLevel(0);
        //}
        ///// <summary>
        ///// 锁纹理目标区域（准备复制数据进入）
        ///// </summary>
        ///// <param name="texture"></param>
        ///// <returns></returns>
        //public static DataRectangle DX_TextureLockRectangle(Texture texture)
        //{
        //    return texture.LockRectangle(0, LockFlags.Discard);
        //}
        ///// <summary>
        ///// 解锁纹理目标区域
        ///// </summary>
        ///// <param name="texture"></param>
        //public static void DX_TextureUnlockRectangle(Texture texture)
        //{
        //    texture.UnlockRectangle(0);
        //}
        ///// <summary>
        ///// 获取纹理目标区域指针
        ///// </summary>
        ///// <param name="dataRectangle"></param>
        ///// <returns></returns>
        //public static nint DX_DataRectanglePointer(DataRectangle dataRectangle)
        //{
        //    return dataRectangle.Data.DataPointer;
        //}

        //#endregion

        //#region 矩阵缩放

        ///// <summary>
        ///// 矩阵缩放
        ///// </summary>
        ///// <param name="x"></param>
        ///// <param name="y"></param>
        ///// <returns></returns>
        //public static SlimDX.Matrix DX_MatrixScaling(float x, float y)
        //{
        //    return SlimDX.Matrix.Scaling(x, y, 0);
        //}
        ///// <summary>
        ///// 精灵转换矩阵
        ///// </summary>
        //public static SlimDX.Matrix DX_SpriteTransform
        //{
        //    get { return Sprite.Transform; }
        //    set { Sprite.Transform = value; }
        //}
        ///// <summary>
        ///// 矩阵ID
        ///// </summary>
        ///// <returns></returns>
        //public static SlimDX.Matrix DX_MatrixIdentity()
        //{
        //    return SlimDX.Matrix.Identity;
        //}

        //#endregion

        //#endregion
    }
}
