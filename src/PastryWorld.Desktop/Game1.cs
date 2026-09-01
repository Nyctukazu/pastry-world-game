using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.ImGuiNet;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;

using PastryWorld.Core.Level;   
using PastryWorld.Engine;
using PastryWorld.Editor;
using PastryWorld.Editor.Level;
using PastryWorld.Maps;
using PastryWorld.Tools;
using System;
using System.Collections.Generic;
using PastryWorld.Core.Animation;

namespace PastryWorld.Desktop;

public class Game1 : Game
{   
    private AnimationSet _animationSet = new AnimationSet();
    private TileMapRenderer _mapRenderer;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private WorldEditorSystem _editorSystem;
    private ImGuiRenderer _imGuiRenderer;
    private KeyboardState _previousKeyboardState;
    private MapData _mapData = new MapData();
    private Camera2D _camera;
    private Texture2D _pixel;
    private TileRegistry _tileRegistry;
    private RenderTarget2D _nativeCanvas;
    private Rectangle _destinationRect;
    private float _scale = 1f;
    private const int BaseCanvasHeight = 360;
    private bool _resizePending;
    private readonly Dictionary<int, Texture2D> _groupTextures = new();
    private Effect _blendEffects;
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;

        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.ApplyChanges();

        Window.ClientSizeChanged += (_, _) => _resizePending = true;

    }

    protected override void Initialize()
    {
        _imGuiRenderer = new ImGuiRenderer(this);
        _imGuiRenderer.RebuildFontAtlas();

        RecreateNativeCanvas();

        _camera = new Camera2D(new Viewport(0, 0, _nativeCanvas.Width, _nativeCanvas.Height));
        _mapRenderer = new TileMapRenderer();
        _tileRegistry = new TileRegistry();

        _editorSystem = new WorldEditorSystem(_imGuiRenderer, _camera, _mapData, _tileRegistry, _animationSet);
        
        base.Initialize();

    }

    private void RecreateNativeCanvas()
    {
        int windowWidth = Math.Max(1, Window.ClientBounds.Width);
        int windowHeight = Math.Max(1, Window.ClientBounds.Height);

        float aspect = windowWidth / (float)windowHeight;
        int newWidth = Math.Max(1, (int)MathF.Round(BaseCanvasHeight * aspect));

        if (_nativeCanvas != null && _nativeCanvas.Width == newWidth && _nativeCanvas.Height == BaseCanvasHeight)
        {
            _resizePending = false;
            return;
        }

        _nativeCanvas?.Dispose();
        _nativeCanvas = new RenderTarget2D(GraphicsDevice, newWidth, BaseCanvasHeight);

        _camera?.UpdateViewport(new Viewport(0, 0, _nativeCanvas.Width, _nativeCanvas.Height));

        _resizePending = false;
    }

    protected override void LoadContent()
    {
        
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
        _blendEffects = Content.Load<Effect>("Effects/BlendEffects");
        _blendEffects.Parameters["MatrixTransform"].SetValue(
            Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1)
        );
        _editorSystem.LoadContent();

        var loader = new TilesetLoader(GraphicsDevice);
        var tilesets = loader.LoadAllTilesets(_tileRegistry);
        _groupTextures.Clear();
        
        foreach (var (group, texture) in tilesets)
        {
            Console.WriteLine($"[Game1] Processing loop for: {group.Name}");
            
            try 
            {
                IntPtr imGuiTextureId = _imGuiRenderer.BindTexture(texture);
                _editorSystem.RegisterTilesetGroup(group, texture, imGuiTextureId);
                _groupTextures[group.Id] = texture;
                
                Console.WriteLine($"[Game1] Successfully registered {group.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Game1 ERROR] Failed to register {group.Name}: {ex.Message}");
            }
        }
    }

    protected override void Update(GameTime gameTime)
    {
        if (_resizePending)
        {
            RecreateNativeCanvas();
        }
        KeyboardState keyState = Keyboard.GetState();
        if (keyState.IsKeyDown(Keys.F1) && _previousKeyboardState.IsKeyUp(Keys.F1))
        {
            _editorSystem.ToggleMode();
        }
        _previousKeyboardState = keyState;

        if (!_editorSystem.IsActive)
        {
            
        }
        else
        {
            _editorSystem.Update(gameTime, _destinationRect, _scale);
        }

        base.Update(gameTime);
    }


    protected override void Draw(GameTime gameTime)
    {

        GraphicsDevice.SetRenderTarget(_nativeCanvas);
        GraphicsDevice.Clear(Color.CornflowerBlue);

        XnaMatrix viewMatrix = _camera.GetViewMatrix();
        XnaMatrix editorViewMatrix = _editorSystem.GetFinalViewMatrix(viewMatrix);

        _spriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: editorViewMatrix    
        );

        if (!_editorSystem.IsAnimationToolActive)
        {
            _mapRenderer.Draw(_spriteBatch, _mapData, _tileRegistry, _groupTextures);
        }
        
        _editorSystem.DrawWorld(_spriteBatch, _nativeCanvas.Bounds, viewMatrix, _pixel);

        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        _destinationRect = GraphicsDevice.Viewport.Bounds;
        _scale = GraphicsDevice.Viewport.Height / (float)_nativeCanvas.Height;

        int destWidth = (int)MathF.Round(_nativeCanvas.Width * _scale);
        int destHeight = (int)MathF.Round(_nativeCanvas.Height * _scale);

        int destX = (GraphicsDevice.Viewport.Width - destWidth) / 2;
        int destY = (GraphicsDevice.Viewport.Height - destHeight) / 2;

        _destinationRect = new Rectangle(destX, destY, destWidth, destHeight);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_nativeCanvas, _destinationRect, Color.White);
        _spriteBatch.End();


        _editorSystem.DrawUI(gameTime);

        base.Draw(gameTime);
    }

}
