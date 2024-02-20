using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;

namespace ASJPSMFinal
{
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        public SpriteBatch _spriteBatch;

        private StartScene startScene;
        private CreditScene creditScene;
        private HelpScene helpScene;
        private MinesweeperScene minesweeperScene;
        private Difficulty _difficulty;

        public const int QUIT = 3;
        public static int ScreenWidth = 600;
        public static int ScreenHeight = 800;
        private Texture2D _screenTexture;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;

            _graphics.ApplyChanges();
        }
/*        public void SetWindowSize(int width, int height)
        {
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.ApplyChanges();
        }*/
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Shared.stage = new Vector2(_graphics.PreferredBackBufferWidth,
                _graphics.PreferredBackBufferHeight);
            base.Initialize();
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            startScene = new StartScene(this);
            this.Components.Add(startScene);

            minesweeperScene = new MinesweeperScene(this);
            this.Components.Add(minesweeperScene);

            helpScene = new HelpScene(this);
            this.Components.Add(helpScene);

            creditScene = new CreditScene(this);
            this.Components.Add(creditScene);

            startScene.show();
        }
        private void hideAllScenes()
        {
            foreach (GameScene item in Components)
            {
                item.hide();
            }
        }
        public void StartNewGame(Difficulty difficulty)
        {
            _difficulty = difficulty;

            if (minesweeperScene == null)
            {
                minesweeperScene = new MinesweeperScene(this);
                this.Components.Add(minesweeperScene);
            }

            minesweeperScene.SetDifficulty(_difficulty);

            hideAllScenes();
            minesweeperScene.show();
        }
        protected override void Update(GameTime gameTime)
        {
            /* if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                 Exit();*/
            int selectedIndex = 0;
            KeyboardState ks = Keyboard.GetState();
            if (startScene.Enabled)
            {
                selectedIndex = startScene.Menu.SelectedIndex;
                if (selectedIndex == 1 && ks.IsKeyDown(Keys.Enter))
                {
                    startScene.hide();
                    helpScene.show();
                }
                else if (selectedIndex == 2 && ks.IsKeyDown(Keys.Enter))
                {
                    startScene.hide();
                    creditScene.show();
                }
                else if (selectedIndex == QUIT && ks.IsKeyDown(Keys.Enter))
                {
                    Exit();
                }

            }
            if (creditScene.Enabled || helpScene.Enabled || minesweeperScene.Enabled)
            {
                if (ks.IsKeyDown(Keys.Escape))
                {
                    MediaPlayer.Stop();
                    hideAllScenes();
                    startScene.show();
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }

    }
}