using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Numerics;
using System.Runtime.InteropServices;
using Vortice;
using Vortice.D3DCompiler;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Direct3D11.Debug;
using Vortice.DXGI;
using Color4 = Vortice.Mathematics.Color4;

namespace Client.MirGraphics
{
    public class DX_Vortice
    {
        #region 可替换部分(不改变量名、方法名、参数名，改类型和实现)

        public static ID3D11Device Device;
        public static ID2D1RenderTarget Sprite;
        public static ID2D1RenderTarget Line;

        public static ID3D11RenderTargetView CurrentSurface;
        public static ID3D11RenderTargetView MainSurface;
        public static SwapChainFullscreenDescription Parameters;

        public static ID3D11Texture2D RadarTexture;
        public static ID3D11Texture2D PoisonDotBackground;
        public static List<ID3D11Texture2D> Lights = new List<ID3D11Texture2D>();

        public static ID3D11Texture2D FloorTexture, LightTexture;
        public static ID3D11RenderTargetView FloorSurface, LightSurface;

        public static ID3D11PixelShader GrayScalePixelShader;
        public static ID3D11PixelShader NormalPixelShader;
        public static ID3D11PixelShader MagicPixelShader;

        //-----------------------------------------------------

        public static IDXGIFactory2 DxgiFactory;
        public static ID3D11DeviceContext DeviceContext;
        public static ID3D11Texture2D BackBuffer;
        public static IDXGISwapChain1 DXGISwapChain;
        public static IDXGISurface DXGISurface;
        public static RenderTargetProperties SpriteRenderTargetProperties;
        public static SwapChainDescription1 swapChainDescription;
        public static ID2D1Factory1 D2DFactory;

        /// <summary>
        /// 封装类
        /// </summary>
        public class DX_Direct3D9Exception : Exception
        {

        }
        /// <summary>
        /// 获取设备像素着色器
        /// </summary>
        public static ID3D11PixelShader DX_DevicePixelShader
        {
            get { return GrayScalePixelShader; }
            set { GrayScalePixelShader = value; }
        }
        /// <summary>
        /// 窗口模式
        /// </summary>
        public static bool DX_ParametersWindowed
        {
            get { return Parameters.Windowed; }
            set { Parameters.Windowed = value; }
        }
        /// <summary>
        /// 从文件加载像素着色器
        /// </summary>
        public static void DX_LoadPixelShader(string path, ID3D11PixelShader pixelShader)
        {
            var shaderNormalPath = Settings.ShadersPath + "normal.hlsl";
            var shaderGrayScalePath = Settings.ShadersPath + "grayscale.hlsl";
            var shaderMagicPath = Settings.ShadersPath + "magic.hlsl";
            if (File.Exists(shaderNormalPath))
            {
                var compilationResult = Compiler.CompileFromFile(
                    shaderNormalPath,
                    "main",
                    "ps_4_0",
                    ShaderFlags.OptimizationLevel3
                );
                NormalPixelShader = Device.CreatePixelShader(compilationResult.Span);
            }
            if (File.Exists(shaderGrayScalePath))
            {
                var compilationResult = Compiler.CompileFromFile(
                    shaderGrayScalePath,
                    "main",
                    "ps_4_0",
                    ShaderFlags.OptimizationLevel3
                );
                GrayScalePixelShader = Device.CreatePixelShader(compilationResult.Span);
            }
            if (File.Exists(shaderMagicPath))
            {
                var compilationResult = Compiler.CompileFromFile(
                    shaderMagicPath,
                    "main",
                    "ps_4_0",
                    ShaderFlags.OptimizationLevel3
                );
                MagicPixelShader = Device.CreatePixelShader(compilationResult.Span);
            }
        }
        /// <summary>
        /// 设备初始化
        /// </summary>
        public static void DX_InitDevice()
        {
            DxgiFactory = DXGI.CreateDXGIFactory1<IDXGIFactory2>();
            var hardwareAdapter = GetHardwareAdapter(DxgiFactory).ToList().FirstOrDefault();
            if (hardwareAdapter == null)
            {
                throw new InvalidOperationException("Cannot detect D3D11 adapter");
            }
            Vortice.Direct3D.FeatureLevel[] featureLevels = new[]
            {
                Vortice.Direct3D.FeatureLevel.Level_11_1,
                Vortice.Direct3D.FeatureLevel.Level_11_0,
                Vortice.Direct3D.FeatureLevel.Level_10_1,
                Vortice.Direct3D.FeatureLevel.Level_10_0,
                Vortice.Direct3D.FeatureLevel.Level_9_3,
                Vortice.Direct3D.FeatureLevel.Level_9_2,
                Vortice.Direct3D.FeatureLevel.Level_9_1,
            };
            IDXGIAdapter1 adapter = hardwareAdapter;
            DeviceCreationFlags creationFlags = DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug;
            var result = D3D11.D3D11CreateDevice
            (
                adapter,
                DriverType.Unknown,
                creationFlags,
                featureLevels,
                out ID3D11Device d3D11Device, out Vortice.Direct3D.FeatureLevel featureLevel,
                out ID3D11DeviceContext d3D11DeviceContext
            );
            if (result.Failure)
            {
                result = D3D11.D3D11CreateDevice(
                    IntPtr.Zero,
                    DriverType.Warp,
                    creationFlags,
                    featureLevels,
                    out d3D11Device, out featureLevel, out d3D11DeviceContext);
                result.CheckError();
            }
            d3D11Device.QueryInterface<ID3D11Debug>().FeatureMask = 0x1;
            Device = d3D11Device;
            DeviceContext = d3D11DeviceContext;

            Vortice.DXGI.Format colorFormat = Vortice.DXGI.Format.B8G8R8A8_UNorm;//B8G8R8A8_UNorm、R8G8B8A8_UNorm、
            const int FrameCount = 2;//大部分应用来说，至少需要两个缓存
            swapChainDescription = new()
            {
                Width = (uint)Program.Form.Width,
                Height = (uint)Program.Form.Height,
                Format = colorFormat,
                BufferCount = FrameCount,
                BufferUsage = Vortice.DXGI.Usage.RenderTargetOutput,
                SampleDescription = Vortice.DXGI.SampleDescription.Default,
                Scaling = Vortice.DXGI.Scaling.Stretch,
                SwapEffect = Vortice.DXGI.SwapEffect.FlipDiscard,
                AlphaMode = Vortice.DXGI.AlphaMode.Ignore
            };
            Parameters = new SwapChainFullscreenDescription
            {
                Windowed = true
            };
            DXGISwapChain = DxgiFactory.CreateSwapChainForHwnd(Device, Program.Form.Handle, swapChainDescription, Parameters);
            DxgiFactory.MakeWindowAssociation(Program.Form.Handle, Vortice.DXGI.WindowAssociationFlags.IgnoreAltEnter);
            D2DFactory = Vortice.Direct2D1.D2D1.D2D1CreateFactory<ID2D1Factory1>();
        }
        private static IEnumerable<IDXGIAdapter1> GetHardwareAdapter(IDXGIFactory2 factory)
        {
            IDXGIFactory6? factory6 = factory.QueryInterfaceOrNull<IDXGIFactory6>();
            if (factory6 != null)
            {
                for (int adapterIndex = 0;
                     factory6.EnumAdapterByGpuPreference((uint)adapterIndex, GpuPreference.HighPerformance,
                         out IDXGIAdapter1? adapter).Success;
                     adapterIndex++)
                {
                    if (adapter == null)
                    {
                        continue;
                    }
                    AdapterDescription1 desc = adapter.Description1;
                    if ((desc.Flags & AdapterFlags.Software) != AdapterFlags.None)
                    {
                        adapter.Dispose();
                        continue;
                    }
                    yield return adapter;
                }
                factory6.Dispose();
            }
            for (int adapterIndex = 0;
                 factory.EnumAdapters1((uint)adapterIndex, out IDXGIAdapter1? adapter).Success;
                 adapterIndex++)
            {
                AdapterDescription1 desc = adapter.Description1;
                if ((desc.Flags & AdapterFlags.Software) != AdapterFlags.None)
                {
                    adapter.Dispose();

                    continue;
                }
                yield return adapter;
            }
        }

        /// <summary>
        /// 精灵初始化
        /// </summary>
        public static void DX_InitSprite()
        {
            BackBuffer = DXGISwapChain.GetBuffer<ID3D11Texture2D>(0);
            DXGISurface = BackBuffer.QueryInterface<IDXGISurface>();
            SpriteRenderTargetProperties = new RenderTargetProperties(
                new Vortice.DCommon.PixelFormat(Vortice.DXGI.Format.B8G8R8A8_UNorm, Vortice.DCommon.AlphaMode.Premultiplied));
            Sprite = D2DFactory.CreateDxgiSurfaceRenderTarget(DXGISurface, SpriteRenderTargetProperties);
            Line = D2DFactory.CreateDxgiSurfaceRenderTarget(DXGISurface, SpriteRenderTargetProperties);
        }
        /// <summary>
        /// 表面初始化
        /// </summary>
        public static void DX_InitRenderTarget()
        {
            MainSurface = DX_DeviceGetBackBuffer00();
            CurrentSurface = MainSurface;
            DX_SetRenderTarget(0,ref MainSurface);
        }
        /// <summary>
        /// 获取后缓冲区
        /// </summary>
        /// <returns></returns>
        public static ID3D11RenderTargetView DX_DeviceGetBackBuffer00()
        {
            return Device.CreateRenderTargetView(BackBuffer);
        }
        /// <summary>
        /// 获取后缓冲区流
        /// </summary>
        /// <returns></returns>
        public static Stream DX_DeviceGetBackBuffer00Stream()
        {
            var backbuffer = DXGISwapChain.GetBuffer<ID3D11Texture2D>(0);
            var mapped = DeviceContext.Map(backbuffer, 0, Vortice.Direct3D11.MapMode.Read, Vortice.Direct3D11.MapFlags.None);
            var totalSize = mapped.RowPitch * backbuffer.Description.Height;//二维纹理长度计算
            byte[] data = new byte[totalSize];
            Marshal.Copy(mapped.DataPointer, data, 0, (int)totalSize);
            var stream = new MemoryStream();
            stream.Write(data, 0, data.Length);
            stream.Position = 0;
            return stream;
        }
        /// <summary>
        /// 初始化雷达纹理
        /// </summary>
        public static void DX_InitRadarTexture()
        {
            //RadarTexture = DX_NewTextureNoneManaged(2, 2);
            //var stream = DX_TextureLockRectangle(RadarTexture);
            using (Bitmap image = new Bitmap(2, 2, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            using (Graphics graphics = Graphics.FromImage(image))
            {
                graphics.Clear(Color.White);

                nint datapoint = 0;
                RadarTexture = CreateTextureFromBytes(GetBitmapPixelData(image), 2, 2, ref datapoint);
            }
            //DX_TextureUnlockRectangle(RadarTexture);
        }
        /// <summary>
        /// 初始化毒雾纹理
        /// </summary>
        public static void DX_InitPoisonDotBackgroundTexture()
        {
            //PoisonDotBackground = DX_NewTextureNoneManaged(5, 5);
            //var stream = DX_TextureLockRectangle(PoisonDotBackground);
            using (Bitmap image = new Bitmap(5, 5, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            using (Graphics graphics = Graphics.FromImage(image))
            {
                graphics.Clear(Color.White);
                nint datapoint = 0;
                PoisonDotBackground = CreateTextureFromBytes(GetBitmapPixelData(image), 2, 2, ref datapoint);
            }
            //DX_TextureUnlockRectangle(PoisonDotBackground);
        }
        /// <summary>
        /// 初始化灯光纹理
        /// </summary>
        public static ID3D11Texture2D DX_InitLightTexture(int width, int height)
        {
            ID3D11Texture2D light = null;
            //var light = DX_NewTextureNoneManaged(width, height);

            //var stream = DX_TextureLockRectangle(light);
            using (Bitmap image = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (Graphics graphics = Graphics.FromImage(image))
                {
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        //path.AddEllipse(new Rectangle(0, 0, width, height));
                        //using (PathGradientBrush brush = new PathGradientBrush(path))
                        //{
                        //    graphics.Clear(Color.FromArgb(0, 0, 0, 0));
                        //    brush.SurroundColors = new[] { Color.FromArgb(0, 255, 255, 255) };
                        //    brush.CenterColor = Color.FromArgb(255, 255, 255, 255);
                        //    graphics.FillPath(brush, path);
                        //    graphics.Save();
                        //}

                        path.AddEllipse(new Rectangle(0, 0, width, height));
                        using (PathGradientBrush brush = new PathGradientBrush(path))
                        {
                            Color[] blendColours = { Color.White,
                                                     Color.FromArgb(255,210,210,210),
                                                     Color.FromArgb(255,160,160,160),
                                                     Color.FromArgb(255,70,70,70),
                                                     Color.FromArgb(255,40,40,40),
                                                     Color.FromArgb(0,0,0,0)};

                            float[] radiusPositions = { 0f, .20f, .40f, .60f, .80f, 1.0f };

                            ColorBlend colourBlend = new ColorBlend();
                            colourBlend.Colors = blendColours;
                            colourBlend.Positions = radiusPositions;

                            graphics.Clear(Color.FromArgb(0, 0, 0, 0));
                            brush.InterpolationColors = colourBlend;
                            brush.SurroundColors = blendColours;
                            brush.CenterColor = Color.White;
                            graphics.FillPath(brush, path);
                            graphics.Save();
                        }
                        nint datapoint = 0;
                        light = CreateTextureFromBytes(GetBitmapPixelData(image), 2, 2, ref datapoint);
                    }
                }
            }
            //DX_TextureUnlockRectangle(light);
            return light;
        }
        /// <summary>
        /// Vector3转换
        /// </summary>
        private static Vector3? ParseVector3(System.Numerics.Vector3? data)
        {
            return new Vector3(data.Value.X, data.Value.Y, data.Value.Z);
        }
        /// <summary>
        /// 设备状态重置
        /// </summary>
        public static void DX_AttemptReset(Action action1, Action action2)
        {
            try
            {
                var reason = Device.DeviceRemovedReason;
                if (!reason.Success)
                {
                    action2?.Invoke();
                }
            }
            catch (Exception ex)
            {
                action1?.Invoke();
            }
        }
        /// <summary>
        /// 设备重置
        /// </summary>
        public static void DX_DeviceReset(Size clientSize)
        {
            DX_ParametersWindowed = !Settings.FullScreen;
            //Device.Reset(Parameters);//实现：1、释放现有资源renderTargetView?.Dispose()，2、调整交换链；2、重建渲染管线；
        }
        /// <summary>
        /// 设置渲染目标
        /// </summary>
        public static void DX_SetRenderTarget(int targetindex,ref ID3D11RenderTargetView surface)
        {
            surface = Device.CreateRenderTargetView(DXGISwapChain.GetBuffer<ID3D11Texture2D>((uint)targetindex));
        }
        /// <summary>
        /// 清屏
        /// </summary>
        public static void DX_DeviceClearTarget00(Color color)
        {
            DeviceContext.ClearRenderTargetView(CurrentSurface, ToColor4_Vortice(color));
        }
        public static Vortice.Mathematics.Color4 ToColor4_Vortice(System.Drawing.Color color)
        {
            return new Vortice.Mathematics.Color4(
                color.R / 255f,
                color.G / 255f,
                color.B / 255f,
                color.A / 255f
            );
        }
        /// <summary>
        /// 立即呈现
        /// </summary>
        public static void DX_DevicePresent()
        {
            DXGISwapChain.Present(1, PresentFlags.None); //垂直同步间隔为1
        }
        /// <summary>
        /// 开始场景
        /// </summary>
        public static void DX_DeviceBeginScene()
        {
            //不实现
        }
        /// <summary>
        /// 结束场景
        /// </summary>
        public static void DX_DeviceEndScene()
        {
            //不实现
        }
        /// <summary>
        /// 开始渲染（无状态）
        /// </summary>
        public static void DX_SpriteBeginDoNotSaveState()
        {
            // 显式设置所需状态（不依赖自动保存）
            DeviceContext.OMSetBlendState(null); // 使用默认混合状态
            DeviceContext.OMSetDepthStencilState(null); // 禁用深度测试
            DeviceContext.RSSetState(null); // 使用默认光栅化状态
            Sprite.BeginDraw();
        }
        /// <summary>
        /// 开始渲染
        /// </summary>
        public static void DX_SpriteBeginAlphaBlend()
        {
            DeviceContext.OMSetBlendState(Device.CreateBlendState(new Vortice.Direct3D11.BlendDescription
            {
                RenderTarget = new Vortice.Direct3D11.BlendDescription.RenderTarget__FixedBuffer()
                {
                    e0 = new Vortice.Direct3D11.RenderTargetBlendDescription
                    {
                        BlendEnable = true,
                        SourceBlend = Vortice.Direct3D11.Blend.SourceAlpha,
                        DestinationBlend = Vortice.Direct3D11.Blend.InverseSourceAlpha,
                        BlendOperation = Vortice.Direct3D11.BlendOperation.Add,
                        SourceBlendAlpha = Vortice.Direct3D11.Blend.One,
                        DestinationBlendAlpha = Vortice.Direct3D11.Blend.Zero,
                        BlendOperationAlpha = Vortice.Direct3D11.BlendOperation.Add,
                        RenderTargetWriteMask = Vortice.Direct3D11.ColorWriteEnable.All
                    }
                }
            }));
            Sprite.BeginDraw();
        }
        /// <summary>
        /// 结束渲染
        /// </summary>
        public static void DX_SpriteEnd()
        {
            Sprite.EndDraw();
        }
        /// <summary>
        /// 立即渲染
        /// </summary>
        public static void DX_Flush()
        {
            DeviceContext.Flush();
        }
        /// <summary>
        /// 画图像
        /// </summary>
        public static void DX_Draw(ID3D11Texture2D texture, Rectangle? sourceRect, System.Numerics.Vector3? position, System.Drawing.Color color)
        {
            // 1. 处理源矩形转换
            RawRectF? d2dSourceRect = null;
            if (sourceRect.HasValue)
            {
                d2dSourceRect = new RawRectF(
                    sourceRect.Value.Left,
                    sourceRect.Value.Top,
                    sourceRect.Value.Right,
                    sourceRect.Value.Bottom);
            }

            // 2. 计算目标矩形
            RawRectF d2dDestRect;
            if (position.HasValue)
            {
                int titleBarHeight = Program.Form.Height - Settings.ScreenHeight;

                // 获取DPI缩放
                float dpiX, dpiY;
                Sprite.GetDpi(out dpiX, out dpiY);
                float scaleX = dpiX / 96.0f;
                float scaleY = dpiY / 96.0f;

                // 计算缩放后的位置（考虑中心点偏移）
                float width = texture.Description.Width;
                float height = texture.Description.Height;

                d2dDestRect = new RawRectF(
                    (position.Value.X ) * scaleX,
                    (position.Value.Y  + titleBarHeight) * scaleY,
                    (position.Value.X  + width) * scaleX,
                    (position.Value.Y  + height + titleBarHeight) * scaleY);
            }
            else
            {
                // 全屏绘制
                d2dDestRect = new RawRectF(0, 0, Sprite.Size.Width, Sprite.Size.Height);
            }

            // 3. 设置绘制参数
            float opacity = color.A;
            BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.Linear;

            using var surface = texture.QueryInterface<IDXGISurface>();
            var bitmapProps = new Vortice.Direct2D1.BitmapProperties
            {
                PixelFormat = SpriteRenderTargetProperties.PixelFormat
            };
            using var d2dBitmap = Sprite.CreateSharedBitmap(surface, bitmapProps);

            // 4. 执行绘制
            Sprite.DrawBitmap(
                d2dBitmap,
                d2dDestRect,
                opacity,
                interpolationMode,
                d2dSourceRect);



            ////if (texture.Description.BindFlags == BindFlags.RenderTarget) //红屏
            ////{
            ////    return;//测试
            ////}
            ////else if (texture.Description.BindFlags == BindFlags.ShaderResource)
            ////{
            ////    var srvDesc = new Vortice.Direct3D11.ShaderResourceViewDescription
            ////    {
            ////        Format = texture.Description.Format,
            ////        ViewDimension = Vortice.Direct3D.ShaderResourceViewDimension.Texture2D,
            ////        Texture2D = { MipLevels = 1 }
            ////    };
            ////    using var shaderResourceView = Device.CreateShaderResourceView(texture, srvDesc);
            ////    DeviceContext.PSSetShaderResource(0, shaderResourceView);
            ////}

            //using var surface = texture.QueryInterface<IDXGISurface>();
            //var bitmapProps = new Vortice.Direct2D1.BitmapProperties
            //{
            //    PixelFormat = SpriteRenderTargetProperties.PixelFormat
            //};
            //using var d2dBitmap = Sprite.CreateSharedBitmap(surface, bitmapProps);

            //// 获取DPI缩放比例
            //float dpiX = Sprite.Dpi.Width;
            //float dpiY = Sprite.Dpi.Height;

            //// 计算目标矩形
            //var destRect = CalculateDestinationRect(
            //    d2dBitmap.PixelSize,
            //    sourceRect,
            //    position,
            //    dpiX, dpiY);

            //// 转换源矩形
            //RawRectF? sourceRectF = sourceRect.HasValue
            //    ? new RawRectF(
            //        sourceRect.Value.Left,
            //        sourceRect.Value.Top,
            //        sourceRect.Value.Right,
            //        sourceRect.Value.Bottom)
            //    : null;

            //// 计算透明度
            //float opacity = (color.R + color.G + color.B) / 3.0f * color.A;

            //// 执行绘制
            //Sprite.DrawBitmap(
            //    d2dBitmap,
            //    destRect,
            //    opacity,
            //    Vortice.Direct2D1.BitmapInterpolationMode.Linear,
            //    sourceRectF);
        }
        private static RawRectF CalculateDestinationRect(
                        Vortice.Mathematics.SizeI textureSize,
                        Rectangle? sourceRect,
                        System.Numerics.Vector3? position,
                        float dpiX, float dpiY)
        {
            // 计算实际使用的纹理区域
            int width = sourceRect.HasValue ? sourceRect.Value.Width : textureSize.Width;
            int height = sourceRect.HasValue ? sourceRect.Value.Height : textureSize.Height;

            // 转换为DIPs(设备无关像素)
            float dipWidth = width * (96.0f / dpiX);
            float dipHeight = height * (96.0f / dpiY);

            // 应用位置偏移
            float left = position.HasValue ? position.Value.X : 0;
            float top = position.HasValue ? position.Value.Y : 0;

            return new RawRectF(
                left,
                top,
                left + dipWidth,
                top + dipHeight);
        }
        /// <summary>
        /// 画线
        /// </summary>
        public static void DX_LineDraw(Vector2[] points, System.Drawing.Color color)
        {
            ID2D1SolidColorBrush brush = Sprite.CreateSolidColorBrush(
                new Vortice.Mathematics.Color4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f)
            );
            for (int i = 0; i < points.Length - 1; i++)
            {
                Sprite.DrawLine(
                    new Vector2(points[i].X, points[i].Y),
                    new Vector2(points[i + 1].X, points[i + 1].Y),
                    brush
                );
            }
        }
        /// <summary>
        /// 设置透明度
        /// </summary>
        public static void DX_SetOpacity(float opacity)
        {
            DX_SetRenderStateTrue();
            if (opacity >= 1 || opacity < 0)
            {
                DeviceContext.OMSetBlendState(Device.CreateBlendState(new Vortice.Direct3D11.BlendDescription
                {
                    RenderTarget = new Vortice.Direct3D11.BlendDescription.RenderTarget__FixedBuffer()
                    {
                        e0 = new Vortice.Direct3D11.RenderTargetBlendDescription
                        {
                            SourceBlend = Vortice.Direct3D11.Blend.SourceAlpha,//SourceBlend=SourceAlpha
                            DestinationBlend = Vortice.Direct3D11.Blend.InverseSourceAlpha,//DestinationBlend=InverseSourceAlpha
                            SourceBlendAlpha = Vortice.Direct3D11.Blend.One,//SourceBlendAlpha=One

                            BlendEnable = true,  // 启用混合，对应D3DRS_ALPHABLENDENABLE
                            BlendOperation = Vortice.Direct3D11.BlendOperation.Add,        // 混合操作
                            DestinationBlendAlpha = Vortice.Direct3D11.Blend.Zero,  // 目标Alpha混合因子
                            BlendOperationAlpha = Vortice.Direct3D11.BlendOperation.Add,   // Alpha混合操作
                            RenderTargetWriteMask = ColorWriteEnable.All // 写入掩码
                        }
                    }
                }), new Vortice.Mathematics.Color4(1.0f, 1.0f, 1.0f, 1.0f));//BlendFactor=Color.FromArgb(255, 255, 255, 255).ToArgb()// RGBA(255,255,255,255)

                //DX_DeviceSetRenderStateARGB(255, 255, 255, 255);
            }
            else
            {
                DeviceContext.OMSetBlendState(Device.CreateBlendState(new Vortice.Direct3D11.BlendDescription
                {
                    RenderTarget = new Vortice.Direct3D11.BlendDescription.RenderTarget__FixedBuffer()
                    {
                        e0 = new Vortice.Direct3D11.RenderTargetBlendDescription
                        {
                            BlendEnable = true,  // 对应D3DRS_ALPHABLENDENABLE
                            SourceBlend = Vortice.Direct3D11.Blend.BlendFactor,//SourceBlend=BlendFactor
                            DestinationBlend = Vortice.Direct3D11.Blend.InverseBlendFactor,//DestinationBlend=InverseBlendFactor
                            SourceBlendAlpha = Vortice.Direct3D11.Blend.SourceAlpha,//SourceBlendAlpha=SourceAlpha

                            BlendOperation = Vortice.Direct3D11.BlendOperation.Add,        // 混合操作
                            DestinationBlendAlpha = Vortice.Direct3D11.Blend.Zero,  // 目标Alpha混合因子
                            BlendOperationAlpha = Vortice.Direct3D11.BlendOperation.Add,   // Alpha混合操作
                            RenderTargetWriteMask = ColorWriteEnable.All // 写入掩码
                        }
                    }
                }), new Vortice.Mathematics.Color4((byte)(255 * opacity), (byte)(255 * opacity), (byte)(255 * opacity), (byte)(255 * opacity)));

                //DX_DeviceSetRenderStateARGB((byte)(255 * opacity), (byte)(255 * opacity), (byte)(255 * opacity), (byte)(255 * opacity));
            }
        }
        /// <summary>
        /// 设置渲染状态
        /// </summary>
        public static void DX_DeviceSetRenderStateSourceAlphaOne()
        {
            DeviceContext.OMSetBlendState(Device.CreateBlendState(new Vortice.Direct3D11.BlendDescription
            {
                RenderTarget = new Vortice.Direct3D11.BlendDescription.RenderTarget__FixedBuffer()
                {
                    e0 = new Vortice.Direct3D11.RenderTargetBlendDescription
                    {
                        SourceBlend = Vortice.Direct3D11.Blend.SourceAlpha,
                        DestinationBlend = Vortice.Direct3D11.Blend.One,

                        BlendEnable = true,  // 启用混合，对应D3DRS_ALPHABLENDENABLE
                        BlendOperation = Vortice.Direct3D11.BlendOperation.Add,        // 混合操作
                        SourceBlendAlpha = Vortice.Direct3D11.Blend.One,    // 源Alpha混合因子
                        DestinationBlendAlpha = Vortice.Direct3D11.Blend.Zero,  // 目标Alpha混合因子
                        BlendOperationAlpha = Vortice.Direct3D11.BlendOperation.Add,   // Alpha混合操作
                        RenderTargetWriteMask = ColorWriteEnable.All // 写入掩码
                    }
                }
            }));
        }
        /// <summary>
        /// 设置渲染状态
        /// </summary>
        public static void DX_DeviceSetRenderStateAddBlendFactorInverseSourceColor()
        {
            DeviceContext.OMSetBlendState(Device.CreateBlendState(new Vortice.Direct3D11.BlendDescription
            {
                RenderTarget = new Vortice.Direct3D11.BlendDescription.RenderTarget__FixedBuffer()
                {
                    e0 = new Vortice.Direct3D11.RenderTargetBlendDescription
                    {
                        BlendOperation = Vortice.Direct3D11.BlendOperation.Add,
                        SourceBlend = Vortice.Direct3D11.Blend.BlendFactor,
                        DestinationBlend = Vortice.Direct3D11.Blend.InverseSourceColor,

                        BlendEnable = true,  // 启用混合，对应D3DRS_ALPHABLENDENABLE
                        SourceBlendAlpha = Vortice.Direct3D11.Blend.One,    // 源Alpha混合因子
                        DestinationBlendAlpha = Vortice.Direct3D11.Blend.Zero,  // 目标Alpha混合因子
                        BlendOperationAlpha = Vortice.Direct3D11.BlendOperation.Add,   // Alpha混合操作
                        RenderTargetWriteMask = ColorWriteEnable.All // 写入掩码
                    }
                }
            }));
        }
        /// <summary>
        /// 设置渲染状态
        /// </summary>
        public static void DX_DeviceSetRenderStateZeroSourceColor()
        {
            DeviceContext.OMSetBlendState(Device.CreateBlendState(new Vortice.Direct3D11.BlendDescription
            {
                RenderTarget = new Vortice.Direct3D11.BlendDescription.RenderTarget__FixedBuffer()
                {
                    e0 = new Vortice.Direct3D11.RenderTargetBlendDescription
                    {
                        SourceBlend = Vortice.Direct3D11.Blend.Zero,
                        DestinationBlend = Vortice.Direct3D11.Blend.SourceColor,

                        BlendEnable = true,  // 启用混合，对应D3DRS_ALPHABLENDENABLE
                        BlendOperation = Vortice.Direct3D11.BlendOperation.Add,        // 混合操作
                        SourceBlendAlpha = Vortice.Direct3D11.Blend.One,    // 源Alpha混合因子
                        DestinationBlendAlpha = Vortice.Direct3D11.Blend.Zero,  // 目标Alpha混合因子
                        BlendOperationAlpha = Vortice.Direct3D11.BlendOperation.Add,   // Alpha混合操作
                        RenderTargetWriteMask = ColorWriteEnable.All // 写入掩码
                    }
                }
            }));
        }
        /// <summary>
        /// 设置渲染状态
        /// </summary>
        public static void DX_SetRenderStateTrue()
        {
            //Device.SetRenderState(RenderState.AlphaBlendEnable, true);
            //注意，Direct3D11需要完整定义所有混合参数，必须明确指定SrcBlend/DestBlend等参数
            DeviceContext.OMSetBlendState(Device.CreateBlendState(new Vortice.Direct3D11.BlendDescription
            {
                RenderTarget = new Vortice.Direct3D11.BlendDescription.RenderTarget__FixedBuffer()
                {
                    e0 = new Vortice.Direct3D11.RenderTargetBlendDescription
                    {
                        BlendEnable = true,  // 启用混合，对应D3DRS_ALPHABLENDENABLE
                        SourceBlend = Vortice.Direct3D11.Blend.SourceAlpha,    // 源混合因子
                        DestinationBlend = Vortice.Direct3D11.Blend.InverseSourceAlpha,// 目标混合因子
                        BlendOperation = Vortice.Direct3D11.BlendOperation.Add,        // 混合操作
                        SourceBlendAlpha = Vortice.Direct3D11.Blend.One,    // 源Alpha混合因子
                        DestinationBlendAlpha = Vortice.Direct3D11.Blend.Zero,  // 目标Alpha混合因子
                        BlendOperationAlpha = Vortice.Direct3D11.BlendOperation.Add,   // Alpha混合操作
                        RenderTargetWriteMask = ColorWriteEnable.All // 写入掩码
                    }
                }
            }));
        }
        /// <summary>
        /// 设置渲染状态
        /// </summary>
        public static void DX_DeviceSetRenderStateARGB(int a, int r, int g, int b)
        {
            Device.ImmediateContext.OMSetBlendState(Device.CreateBlendState(new Vortice.Direct3D11.BlendDescription
            {
                RenderTarget = new Vortice.Direct3D11.BlendDescription.RenderTarget__FixedBuffer()
                {
                    e0 = new Vortice.Direct3D11.RenderTargetBlendDescription
                    {
                        BlendEnable = true,  // 对应D3DRS_ALPHABLENDENABLE
                    }
                }
            }), new Vortice.Mathematics.Color4((byte)(255 * r), (byte)(255 * g), (byte)(255 * b), (byte)(255 * a)));
        }

        public static unsafe void ConvertSetRenderStateAdvanced(
                            bool enableAlphaBlend,
                            Vortice.Direct3D11.Blend srcBlend,
                            Vortice.Direct3D11.Blend destBlend,
                            Vortice.Direct3D11.BlendOperation blendOp,
                            Vortice.Direct3D11.Blend srcBlendAlpha,
                            Vortice.Direct3D11.Blend destBlendAlpha,
                            Vortice.Direct3D11.BlendOperation blendOpAlpha)
        {
            var blendDesc = new Vortice.Direct3D11.BlendDescription
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false,
                RenderTarget = new Vortice.Direct3D11.BlendDescription.RenderTarget__FixedBuffer()
                {
                    e0 = new RenderTargetBlendDescription
                    {
                        BlendEnable = enableAlphaBlend,
                        SourceBlend = srcBlend,
                        DestinationBlend = destBlend,
                        BlendOperation = blendOp,
                        SourceBlendAlpha = srcBlendAlpha,
                        DestinationBlendAlpha = destBlendAlpha,
                        BlendOperationAlpha = blendOpAlpha,
                        RenderTargetWriteMask = ColorWriteEnable.All
                    }
                }
            };
            var blendState = Device.CreateBlendState(blendDesc);
            DeviceContext.OMSetBlendState(blendState, null, uint.MaxValue);
        }

        /// <summary>
        /// 给像素着色器设置常量值
        /// </summary>
        public static void DX_SetNormal(float blend, Color tintcolor)
        {
            MakePSSetConstantBuffer(blend, tintcolor);
        }
        /// <summary>
        /// 给像素着色器设置常量值
        /// </summary>
        public static void DX_SetGrayscale(float blend, Color tintcolor)
        {
            MakePSSetConstantBuffer(blend, tintcolor);
        }
        /// <summary>
        /// 给像素着色器设置常量值
        /// </summary>
        public static void DX_SetBlendMagic(float blend, Color tintcolor)
        {
            MakePSSetConstantBuffer(blend, tintcolor);
        }
        private static void MakePSSetConstantBuffer(float blend, System.Drawing.Color tintcolor)
        {
            // 创建常量缓冲区
            var buffer = new Vortice.Direct3D11.BufferDescription
            {
                // 缓冲区字节大小（需16字节对齐）
                ByteWidth = (uint)Marshal.SizeOf<MatrixBuffer>(),
                // 绑定到管线阶段
                BindFlags = Vortice.Direct3D11.BindFlags.VertexBuffer,
                // 特殊选项 //MiscFlags.Shared?
                MiscFlags = Vortice.Direct3D11.ResourceOptionFlags.None,
                // 结构化缓冲的步长
                StructureByteStride = 0,
                //如果需要频繁更新的纹理
                CPUAccessFlags = Vortice.Direct3D11.CpuAccessFlags.Write | Vortice.Direct3D11.CpuAccessFlags.Read,
                // 资源使用模式
                Usage = Vortice.Direct3D11.ResourceUsage.Dynamic,
            };
            var constantBuffer = Device.CreateBuffer(buffer);
            var matrixData = new MatrixBuffer
            {
                param1 = new Vector4(1.0F, 1.0F, 1.0F, blend),
                param2 = new Vector4(tintcolor.R / 255, tintcolor.G / 255, tintcolor.B / 255, 1.0F)
            };
            // 更新缓冲区数据，绑定到管线
            DeviceContext.UpdateSubresource(in matrixData, constantBuffer);
            DeviceContext.PSSetConstantBuffer(0, constantBuffer);
            DeviceContext.PSSetConstantBuffer(1, constantBuffer);
            DeviceContext.IASetVertexBuffer(0, constantBuffer, 0);//add
        }
        [StructLayout(LayoutKind.Sequential)]
        struct MatrixBuffer
        {
            public Vector4 param1;
            public Vector4 param2;
        }
        /// <summary>
        /// 新建纹理（渲染目标）
        /// </summary>
        public static ID3D11Texture2D DX_NewTextureRenderTargetDefault(int width, int height)
        {
            //ControlTexture = new Texture(DXManager.Device, Size.Width, Size.Height, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            // 创建Direct3D11兼容的渲染目标纹理
            var textureDesc = new Vortice.Direct3D11.Texture2DDescription
            {
                Width = (uint)width,
                Height = (uint)height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Vortice.DXGI.Format.B8G8R8A8_UNorm, // 对应A8R8G8B8格式
                SampleDescription = new Vortice.DXGI.SampleDescription(1, 0),
                Usage = Vortice.Direct3D11.ResourceUsage.Default, // Pool.Default等效参数
                BindFlags = Vortice.Direct3D11.BindFlags.RenderTarget, // Usage.RenderTarget映射
                CPUAccessFlags = Vortice.Direct3D11.CpuAccessFlags.None,// Default资源通常不需要CPU访问
                MiscFlags = Vortice.Direct3D11.ResourceOptionFlags.None, //MiscFlags.Shared?
            };
            var texture = Device.CreateTexture2D(textureDesc);
            return texture;
            //注意：创建后需额外调用CreateRenderTargetView才能作为渲染目标使用
        }
        /// <summary>
        /// 新建纹理（托管）
        /// </summary>
        public static ID3D11Texture2D DX_NewTextureNoneManaged(int width, int height)
        {
            var radarDesc = new Texture2DDescription
            {
                Width = (uint)width,
                Height = (uint)height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Vortice.DXGI.Format.B8G8R8A8_UNorm,
                SampleDescription = new Vortice.DXGI.SampleDescription(1, 0),
                Usage = Vortice.Direct3D11.ResourceUsage.Dynamic,
                BindFlags = Vortice.Direct3D11.BindFlags.ShaderResource,
                CPUAccessFlags = CpuAccessFlags.Write,
                MiscFlags = ResourceOptionFlags.None
            };
            return Device.CreateTexture2D(radarDesc);
        }
        /// <summary>
        /// 获取表面等级
        /// </summary>
        public static ID3D11RenderTargetView DX_GetSurfaceLevel0(ID3D11Texture2D texture)
        {
            var rtvDesc = new Vortice.Direct3D11.RenderTargetViewDescription
            {
                Format = texture.Description.Format,
                ViewDimension = Vortice.Direct3D11.RenderTargetViewDimension.Texture2D,
                Texture2D = new Vortice.Direct3D11.Texture2DRenderTargetView
                {
                    MipSlice = 0
                }
            };
            return Device.CreateRenderTargetView(texture, rtvDesc);
        }
        /// <summary>
        /// 锁纹理目标区域（准备复制数据进入）
        /// </summary>
        public static MappedSubresource DX_TextureLockRectangle(ID3D11Texture2D texture)
        {
            return DeviceContext.Map(texture, 0, Vortice.Direct3D11.MapMode.WriteDiscard, Vortice.Direct3D11.MapFlags.None);
        }
        /// <summary>
        /// 解锁纹理目标区域
        /// </summary>
        public static void DX_TextureUnlockRectangle(ID3D11Texture2D texture)
        {
            DeviceContext.Unmap(texture, 0);
        }
        /// <summary>
        /// 获取纹理目标区域指针
        /// </summary>
        public static nint DX_DataRectanglePointer(MappedSubresource stream)
        {
            return stream.DataPointer;
        }
        /// <summary>
        /// 矩阵缩放
        /// </summary>
        public static Matrix4x4 DX_MatrixScaling(float x, float y)
        {
            //创建缩放矩阵（Z轴缩放值设为1.0f，与SlimDX的0等效）
            return Matrix4x4.CreateScale(x, y, 1.0f);
        }
        /// <summary>
        /// 精灵转换矩阵
        /// </summary>
        public static Matrix4x4 DX_SpriteTransform
        {
            set { SpriteTransform(value); }
        }
        public static void SpriteTransform(Matrix4x4 matrix)
        {
            //通过常量缓冲区将创建的矩阵传递到着色器
            var bufferDesc = new BufferDescription
            {
                Usage = ResourceUsage.Dynamic,
                BindFlags = BindFlags.ConstantBuffer,
                ByteWidth = (uint)Marshal.SizeOf<Matrix4x4>(),
                CPUAccessFlags = CpuAccessFlags.Write
            };
            var constantBuffer = Device.CreateBuffer(bufferDesc);
            var dataStream = DeviceContext.Map(constantBuffer, MapMode.WriteDiscard);
            unsafe
            {
                var matrixPtr = (Matrix4x4*)dataStream.DataPointer;// 等效于SlimDX.Sprite.Transform = matrix
                *matrixPtr = matrix; //创建缩放矩阵（Z轴缩放值设为1.0f，与SlimDX的0等效）
            }
            DeviceContext.Unmap(constantBuffer);
            DeviceContext.VSSetConstantBuffer(0, constantBuffer);
        }
        /// <summary>
        /// 矩阵ID
        /// </summary>
        public static Matrix4x4 DX_MatrixIdentity()
        {
            return Matrix4x4.Identity;
        }

        public static byte[] DecompressDataNew(byte[] compressedData)
        {
            byte[] decompressedData;
            using (var inputStream = new MemoryStream(compressedData))
            using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                gzipStream.CopyTo(outputStream);
                decompressedData = outputStream.ToArray();
            }
            return decompressedData;
        }

        public static ID3D11Texture2D CreateTextureFromBytes(byte[] data, uint width, uint height, ref nint point)
        {
            var texDesc = new Vortice.Direct3D11.Texture2DDescription
            {
                Width = (uint)width,
                Height = (uint)height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Vortice.DXGI.Format.B8G8R8A8_UNorm, // 对应A8R8G8B8格式
                SampleDescription = new Vortice.DXGI.SampleDescription(1, 0),
                Usage = Vortice.Direct3D11.ResourceUsage.Dynamic,// Pool.Managed等效配置
                BindFlags = Vortice.Direct3D11.BindFlags.ShaderResource,// Usage.None默认绑定
                CPUAccessFlags = Vortice.Direct3D11.CpuAccessFlags.Write,
            };

            var initData = new Vortice.Direct3D11.SubresourceData(Marshal.AllocHGlobal(data.Length), (uint)width * 4, 0);
            point = initData.DataPointer;

            try
            {
                Marshal.Copy(data, 0, initData.DataPointer, data.Length);
                return Device.CreateTexture2D(texDesc, new[] { initData });
            }
            finally
            {
                Marshal.FreeHGlobal(initData.DataPointer);
            }
        }

        public static byte[] GetBitmapPixelData(Bitmap bitmap)
        {
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
            try
            {
                int bytes = Math.Abs(bmpData.Stride) * bitmap.Height;
                byte[] rgbValues = new byte[bytes];
                Marshal.Copy(bmpData.Scan0, rgbValues, 0, bytes);
                return rgbValues;
            }
            finally
            {
                bitmap.UnlockBits(bmpData);
            }
        }

        #endregion
    }
}
