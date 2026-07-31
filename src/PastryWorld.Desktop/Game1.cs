using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.ImGuiNet;

using PastryWorld.Core;   
using PastryWorld.Engine;
using PastryWorld.Editor;

namespace PastryWorld.Desktop;

public class Game1 : Game
{   
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private WorldEditorSystem _editorSystem;
    private ImGuiRenderer _imGuiRenderer;
    private KeyboardState _previousKeyboardState;
    private MapData _mapData = new MapData();
    private Camera2D _camera;
    private Texture2D _pixel;
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

        _editorSystem = new WorldEditorSystem(_imGuiRenderer);
        base.Initialize();

    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

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
            _editorSystem.Update(gameTime, _camera.GetViewMatrix());
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: _camera.GetViewMatrix()    
        );
        _editorSystem.DrawWorld(_spriteBatch, GraphicsDevice.Viewport.Bounds, _camera.GetViewMatrix(), _pixel);

        _spriteBatch.End();
        _editorSystem.DrawUI(gameTime);
        base.Draw(gameTime);
    }
}
