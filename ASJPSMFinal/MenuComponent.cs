using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text;

namespace ASJPSMFinal
{
    public class MenuComponent : DrawableGameComponent
    {
        private SpriteBatch spriteBatch;
        private SpriteFont regularFont, hilightFont;
        private string[] menuItems;
        private Texture2D backgroundImage;

        public int SelectedIndex { get; set; } 
        private Vector2 position;
        private Color regularColor = Color.White;
        private Color hilightColor = Color.Red;

        private KeyboardState oldState;

        public MenuComponent(Game game,
            SpriteBatch spriteBatch,
            SpriteFont regularFont,
            SpriteFont hilightFont,
            string[] menuItems) : base(game)
        {
            this.spriteBatch = spriteBatch;
            this.regularFont = regularFont;
            this.hilightFont = hilightFont;
            this.menuItems = menuItems;
            position = new Vector2(Shared.stage.X / 2, Shared.stage.Y / 2);

            // Load the background image
            backgroundImage = game.Content.Load<Texture2D>("Background Image");
        }


        public override void Update(GameTime gameTime)
        {
            KeyboardState ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.Down) && oldState.IsKeyUp(Keys.Down))
            {
                SelectedIndex++;
                if (SelectedIndex == menuItems.Length)
                {
                    SelectedIndex = 0;
                }
            }

            if (ks.IsKeyDown(Keys.Up) && oldState.IsKeyUp(Keys.Up))
            {
                SelectedIndex--;
                if (SelectedIndex == -1)
                {
                    SelectedIndex = menuItems.Length - 1;
                }
            }

            oldState = ks;

            base.Update(gameTime);
        }
        public void ChangeMenuItem(int index, string newItem)
        {
            if (index < 0 || index >= menuItems.Length)
                throw new ArgumentOutOfRangeException("index", "The menu index is out of range.");

            menuItems[index] = newItem;

        }
        public override void Draw(GameTime gameTime)
        {
            float scale = 1.5f; // Scale factor for selected item to increase its size
            spriteBatch.Begin();

            spriteBatch.Draw(backgroundImage, new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), Color.White);

            // Set the initial position to the top-left of the screen.
            Vector2 tempPos = new Vector2(1, 1);

            for (int i = 0; i < menuItems.Length; i++)
            {
                if (SelectedIndex == i)
                {
                    // Draw the selected menu item with a hilight color and a larger size
                    spriteBatch.DrawString(hilightFont, menuItems[i], tempPos, hilightColor, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                    tempPos.Y += hilightFont.LineSpacing * scale; // Increment position for next item
                }
                else
                {
                    // Draw non-selected menu items with regular size and color
                    spriteBatch.DrawString(regularFont, menuItems[i], tempPos, regularColor);
                    tempPos.Y += regularFont.LineSpacing; // Increment position for next item
                }
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
