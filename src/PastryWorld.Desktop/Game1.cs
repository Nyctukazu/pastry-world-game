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
    private int _scale;
    private TilesetLoader _tilesetLoader;
    private readonly Dictionary<int, Texture2D> _groupTextures = new();
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;

        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.ApplyChanges();

    }

    protected override void Initialize()
    {
        _imGuiRenderer = new ImGuiRenderer(this);
        _imGuiRenderer.RebuildFontAtlas();

        _camera = new Camera2D(GraphicsDevice.Viewport);
        _mapRenderer = new TileMapRenderer();
        _tileRegistry = new TileRegistry();

        _editorSystem = new WorldEditorSystem(_imGuiRenderer, _camera, _mapData, _tileRegistry, _animationSet);
        _nativeCanvas = new RenderTarget2D(GraphicsDevice, 640, 360);
        base.Initialize();

    }

    protected override void LoadContent()
    {
        
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);

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

        _mapRenderer.Draw(_spriteBatch, _mapData, _tileRegistry, _groupTextures);


        _editorSystem.DrawWorld(_spriteBatch, _nativeCanvas.Bounds, viewMatrix, _pixel);

        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        int scaleX = GraphicsDevice.Viewport.Width / _nativeCanvas.Width;
        int scaleY = GraphicsDevice.Viewport.Height / _nativeCanvas.Height;
        _scale = Math.Max(1, Math.Min(scaleX, scaleY));

        _destinationRect = Utilities.GetCenteredLetterboxRect(_nativeCanvas.Bounds, _scale, GraphicsDevice.Viewport);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_nativeCanvas, _destinationRect, Color.White);
        _spriteBatch.End();


        _editorSystem.DrawUI(gameTime);

        base.Draw(gameTime);
    }

}
