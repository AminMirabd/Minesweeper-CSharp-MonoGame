using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ASJPSMFinal
{
    public class StartScene : GameScene
    {
        private MenuComponent menu;
        private string[] difficulties = { "Easy", "Medium", "Hard" };
        private int currentDifficultyIndex = 0;
        private string[] menuItems;
        private KeyboardState oldState;
        public MenuComponent Menu { get => menu; set => menu = value; }

/*        public static Texture2D backgroundImage;*/

        private SpriteBatch spriteBatch;


        public StartScene(Game game) : base(game)
        {
            menuItems = new string[] { "Start game", "Help", "Credit", "Quit", "Difficulty: " + difficulties[currentDifficultyIndex] };
            Game1 g = (Game1)game;
            this.spriteBatch = g._spriteBatch;
            SpriteFont regularFont = g.Content.Load<SpriteFont>("regularFont");
            SpriteFont hilightFont = g.Content.Load<SpriteFont>("hilightFont");

/*            backgroundImage = g.Content.Load<Texture2D>("chess.com background");*/

            menu = new MenuComponent(g, spriteBatch, regularFont, hilightFont, menuItems);
            this.Components.Add(menu);
            oldState = Keyboard.GetState();


        }

/*        public override void Draw(GameTime gameTime)
        {

            base.Draw(gameTime);
            
        }*/

        public override void Update(GameTime gameTime)
        {
            KeyboardState ks = Keyboard.GetState();

            if(ks.IsKeyDown(Keys.Enter) && oldState.IsKeyUp(Keys.Enter))
            {
                if(menu.SelectedIndex == 0)
                {
                    string selectedDifficultyText = menuItems[menuItems.Length - 1];
                    Difficulty selectedDifficulty = ParseDifficulty(selectedDifficultyText);

                    ((Game1)this.Game).StartNewGame(selectedDifficulty);
                }
            }
            else
            {
                if(menu.SelectedIndex == menuItems.Length - 1)
                {
                    if(ks.IsKeyDown(Keys.Right) && oldState.IsKeyUp(Keys.Right))
                    {
                        currentDifficultyIndex = (currentDifficultyIndex + 1) % difficulties.Length;
                        menu.ChangeMenuItem(menuItems.Length - 1, "Difficulty: " + difficulties[currentDifficultyIndex]);
                    }
                    else if (ks.IsKeyDown(Keys.Left) && oldState.IsKeyUp(Keys.Left))
                    {
                        currentDifficultyIndex = (currentDifficultyIndex - 1 + difficulties.Length) % difficulties.Length;
                        menu.ChangeMenuItem(menuItems.Length - 1, "Difficulty: " + difficulties[currentDifficultyIndex]);

                    }
                }
            }

            oldState = ks;

            base.Update(gameTime);
        }
        private Difficulty ParseDifficulty(string difficultyText)
        {
            string difficulty = difficultyText.Split(new[] { ": " }, StringSplitOptions.None)[1];
            return (Difficulty)Enum.Parse(typeof(Difficulty), difficulty);
        }
    }
}
