using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Input;

namespace PuzzleDefense
{
    public class Game1 : Game
    {
        public enum Layers
        {
            BackFX,
            Main,
            FrontFX,
            HUD,
            Debug,
        }
        public const int ScreenW = 1920;
        public const int ScreenH = 1080;

        ScreenPlay _screenPlay;

        public static KeyboardState Key;

        public static Texture2D _texBG00;
        public static Texture2D _texLine;
        public static Texture2D _texCircle;
        public static Texture2D _texCursorA;


        public static SpriteFont _fontMain;

        public Game1()
        {
            WindowManager.Init(this, ScreenW, ScreenH);

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
        }

        protected override void Initialize()
        {
            _screenPlay = new ScreenPlay();

            ScreenManager.Init(_screenPlay, Enums.GetList<Layers>());

            ScreenManager.SetLayerParameter((int)Layers.Main, samplerState : SamplerState.LinearWrap);
            ScreenManager.SetLayerParameter((int)Layers.BackFX, samplerState : SamplerState.LinearWrap, blendState : BlendState.Additive);
            ScreenManager.SetLayerParameter((int)Layers.FrontFX, samplerState : SamplerState.LinearWrap, blendState : BlendState.Additive);

            _texLine = GFX.CreateLineTextureAA(GraphicsDevice, 10, 5, 3);
            _texCircle = GFX.CreateCircleTextureAA(GraphicsDevice, 100, 4);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _texBG00 = Content.Load<Texture2D>("Images/background00");
            _texCursorA = Content.Load<Texture2D>("Images/CursorA");

            _fontMain = Content.Load<SpriteFont>("Fonts/fontMain");
        }

        protected override void Update(GameTime gameTime)
        {
            Key = Keyboard.GetState();

            WindowManager.Update(gameTime);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (ButtonControl.OnePress("ToggleFullScreen", Key.IsKeyDown(Keys.F11)))
                WindowManager.ToggleFullscreen();

            ScreenManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            ScreenManager.DrawScreen(gameTime);
            ScreenManager.ShowScreen(gameTime, blendState : BlendState.AlphaBlend, samplerState: SamplerState.LinearWrap);

            base.Draw(gameTime);
        }
    }
}
