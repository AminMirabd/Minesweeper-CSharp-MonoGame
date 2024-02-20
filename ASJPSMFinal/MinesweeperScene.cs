using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SharpDX.XAudio2;
using System;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace ASJPSMFinal
{
    public class MinesweeperScene : GameScene
    {

        private SpriteBatch _spriteBatch;
        private Game1 _game;
        private bool mouseInputLock = false;

        struct CellStruct
        {
            public bool hasFlag;
            public bool hasBomb;
            public bool isUncovered;
            public int neighboringBombs;
            public Rectangle position;
            public Texture2D texture;
        }

        int BOARD_SIZE = 10;
        const int CELL_WIDTH = 48;
        CellStruct[,] cell;

        Texture2D bombTexture, flagTexture, emptyTexture;
        Texture2D[] numbers = new Texture2D[9];
        MouseState mouse, prevMouse;
        enum Gamestates
        {
            GameOverLose,
            GameOverWin,
            Playing
        }
        Gamestates gamestate;
        int flagsPlanted, bombsLocated;

        private SoundEffect safeSound;
        private SoundEffect bombSound;
        private SoundEffect flagSound;
        private Song backGroundMusic;

        public void SetDifficulty(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Easy:
                    Console.WriteLine("Easy");
                    BOARD_SIZE = 10;
                    break;
                case Difficulty.Medium:
                    Console.WriteLine("Medium");
                    BOARD_SIZE = 14;
                    break;
                case Difficulty.Hard:
                    Console.WriteLine("Hard");
                    BOARD_SIZE = 22;
                    break;
            }
            cell = new CellStruct[BOARD_SIZE + 2, BOARD_SIZE + 2];

            LoadContent();
            InitializeBoard();
            PlantBombs();
            CountNeighbors();
            flagsPlanted = 0;
            bombsLocated = 0;


            ResetGame();
        }

        public MinesweeperScene(Game1 game) : base(game)
        {
            _game = game;
            _spriteBatch = new SpriteBatch(game.GraphicsDevice);
            safeSound = game.Content.Load<SoundEffect>("SafeSound");
            bombSound = game.Content.Load<SoundEffect>("BombSound");
            flagSound = game.Content.Load<SoundEffect>("FlagSound");
            backGroundMusic = game.Content.Load<Song>("BackgroundMusic");
            gamestate = Gamestates.Playing;
        }

        protected override void LoadContent()
        {

            _spriteBatch = new SpriteBatch(GraphicsDevice);

            bombTexture = _game.Content.Load<Texture2D>("mine");
            flagTexture = _game.Content.Load<Texture2D>("flag");
            emptyTexture = _game.Content.Load<Texture2D>("empty");

            // Load number textures
            numbers[0] = _game.Content.Load<Texture2D>("0");
            numbers[1] = _game.Content.Load<Texture2D>("1");
            numbers[2] = _game.Content.Load<Texture2D>("2");
            numbers[3] = _game.Content.Load<Texture2D>("3");
            numbers[4] = _game.Content.Load<Texture2D>("4");
            numbers[5] = _game.Content.Load<Texture2D>("5");
            numbers[6] = _game.Content.Load<Texture2D>("6");
            numbers[7] = _game.Content.Load<Texture2D>("7");
            numbers[8] = _game.Content.Load<Texture2D>("8");

            // Initialize cells
            InitializeBoard();
        }
        private void ClearEmptyCells(int row, int col)
        {
            // Base cases to stop the recursion
            if (row <= 0 || row > BOARD_SIZE || col <= 0 || col > BOARD_SIZE)
                return; // Out of bounds
            if (cell[row, col].isUncovered)
                return; // Already uncovered
            if (cell[row, col].hasBomb)
                return; // Hit a bomb - stop clearing

            // Uncover this cell
            cell[row, col].isUncovered = true;

            // If this cell has neighboring bombs, stop clearing further
            if (cell[row, col].neighboringBombs > 0)
                return;

            // Recursively clear neighboring cells
            ClearEmptyCells(row - 1, col);     // Up
            ClearEmptyCells(row + 1, col);     // Down
            ClearEmptyCells(row, col - 1);     // Left
            ClearEmptyCells(row, col + 1);     // Right
            ClearEmptyCells(row - 1, col - 1); // Up-Left
            ClearEmptyCells(row - 1, col + 1); // Up-Right
            ClearEmptyCells(row + 1, col - 1); // Down-Left
            ClearEmptyCells(row + 1, col + 1); // Down-Right
        }
        public override void Update(GameTime gameTime)
        {

            mouse = Mouse.GetState();
            int row, col;

            // Calculate offsets just like in InitializeBoard
            int totalBoardWidth = BOARD_SIZE * CELL_WIDTH;
            int totalBoardHeight = BOARD_SIZE * CELL_WIDTH;
            int offsetX = (_game.GraphicsDevice.Viewport.Width - totalBoardWidth) / 2;
            int offsetY = (_game.GraphicsDevice.Viewport.Height - totalBoardHeight) / 2;

            // Adjust mouse position by subtracting offsets before calculating row and col
            col = (mouse.X - offsetX) / CELL_WIDTH + 1;
            row = (mouse.Y - offsetY) / CELL_WIDTH + 1;

            if (col > BOARD_SIZE || row > BOARD_SIZE) return;
            if (mouseInputLock) return; // Check if mouseInputLock is false before executing the rest of the code

            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
            {
                if (!cell[row, col].isUncovered && !cell[row, col].hasFlag)
                {
                    if (cell[row, col].hasBomb)
                    {
                        // If the cell has a bomb, game over
                        gamestate = Gamestates.GameOverLose;
                        mouseInputLock = true; // Lock mouse input on game over
                        bombSound.Play();
                        MediaPlayer.Stop();
                    }
                    else if (cell[row, col].neighboringBombs > 0)
                    {
                        // If the cell has neighboring bombs, just uncover this one cell
                        cell[row, col].isUncovered = true;
                    }
                    else
                    {
                        // If the cell has no neighboring bombs, clear all connected empty cells
                        ClearEmptyCells(row, col);
                    }
                }
            }
            else if (mouse.RightButton == ButtonState.Pressed && prevMouse.RightButton == ButtonState.Released)
            {
                if (!cell[row, col].isUncovered)
                {
                    cell[row, col].hasFlag = !cell[row, col].hasFlag; // Toggle flag status on right-click
                    if (cell[row, col].hasFlag)
                    {
                        flagsPlanted++;
                        if (cell[row, col].hasBomb) bombsLocated++;
                    }
                    else
                    {
                        flagsPlanted--;
                        if (cell[row, col].hasBomb) bombsLocated--;
                    }
                    flagSound.Play();

                    // Check for win condition
                    if (bombsLocated == BOARD_SIZE)
                    {
                        gamestate = Gamestates.GameOverWin;
                        safeSound.Play();
                    }
                }
            }

            if (gamestate != Gamestates.Playing && Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                ResetGame();
            }

            prevMouse = mouse;
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            _game.GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            for (int r = 1; r <= BOARD_SIZE; r++)
            {
                for (int c = 1; c <= BOARD_SIZE; c++)
                {
                    if (cell[r, c].hasFlag)
                    {
                        _spriteBatch.Draw(flagTexture, cell[r, c].position, Color.White);
                    }
                    else if (cell[r, c].isUncovered)
                    {
                        _spriteBatch.Draw(numbers[cell[r, c].neighboringBombs] , cell[r, c].position, Color.White);
                    }
                    else
                    {
                        _spriteBatch.Draw(emptyTexture, cell[r, c].position, Color.White);
                    }
/*                    if (cell[r, c].hasBomb)
                    {
                        _spriteBatch.Draw(bombTexture, cell[r, c].position, Color.White);
                    }
                    else
                    {
                        _spriteBatch.Draw(numbers[0], cell[r, c].position, Color.White);
                    }*/
                }
            }
            if (gamestate == Gamestates.GameOverLose)
            {
                for (int r = 1; r <= BOARD_SIZE; r++)
                {
                    for (int c = 1; c <= BOARD_SIZE; c++)
                    {
                        if (cell[r, c].hasBomb) { _spriteBatch.Draw(bombTexture, cell[r, c].position, Color.White); }
                    }
                }
            }
            if (gamestate != Gamestates.Playing)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                {
                    ResetGame();
                }
            }

            _spriteBatch.End();
            prevMouse = mouse;
            base.Draw(gameTime);
        }

        private void ResetGame()
        {
            MediaPlayer.Stop();
            // Reinitialize the board
            InitializeBoard();

            // Plant bombs again
            PlantBombs();

            // Recount neighbors
            CountNeighbors();

            // Reset the game state and other relevant counters
            flagsPlanted = 0;
            bombsLocated = 0;
            gamestate = Gamestates.Playing;

            mouseInputLock = false;
            if (!MediaPlayer.State.Equals(MediaState.Playing))
            {
                MediaPlayer.Play(backGroundMusic);
            }
        }

        private void InitializeBoard()
        {
            int totalBoardWidth = BOARD_SIZE * CELL_WIDTH;
            int totalBoardHeight = BOARD_SIZE * CELL_WIDTH;

            // Calculate the starting X and Y to center the board on the screen
            int offsetX = (_game.GraphicsDevice.Viewport.Width - totalBoardWidth) / 2;
            int offsetY = (_game.GraphicsDevice.Viewport.Height - totalBoardHeight) / 2;

            for (int r = 0; r < BOARD_SIZE + 2; r++)
            {
                for (int c = 0; c < BOARD_SIZE + 2; c++)
                {
                    cell[r, c].hasBomb = false;
                    cell[r, c].hasFlag = false;
                    cell[r, c].isUncovered = false;
                    cell[r, c].position.Width = CELL_WIDTH;
                    cell[r, c].position.Height = CELL_WIDTH;

                    // Adjust the position to be centered by adding the offsets
                    cell[r, c].position.X = (c - 1) * CELL_WIDTH + offsetX;
                    cell[r, c].position.Y = (r - 1) * CELL_WIDTH + offsetY;
                    cell[r, c].neighboringBombs = 0;

                    cell[r, c].texture = emptyTexture;
                }
            }


        }

        private const int EASY_BOMBS = 10;
        private const int MEDIUM_BOMBS = 40;
        private const int HARD_BOMBS = 100;

        private void PlantBombs()
        {
            // Determine the number of bombs based on the board size.
            int numBombs;
            switch (BOARD_SIZE)
            {
                case 10: 
                    numBombs = EASY_BOMBS;
                    break;
                case 14: 
                    numBombs = MEDIUM_BOMBS;
                    break;
                case 22: 
                    numBombs = HARD_BOMBS;
                    break;
                default:
                    throw new Exception("Unsupported board size");
            }

            Random rand = new Random();
            bool[] bombs = new bool[BOARD_SIZE * BOARD_SIZE];

            // Plant the bombs at random locations
            for (int i = 0; i < numBombs; i++)
            {
                int position;
                do
                {
                    position = rand.Next(bombs.Length);
                }
                while (bombs[position]); // Ensure this position doesn't already have a bomb
                bombs[position] = true;
            }

            // Assign bombs to cells
            for (int i = 0; i < bombs.Length; i++)
            {
                int row = i / BOARD_SIZE + 1; // Offset by 1 because of the border
                int col = i % BOARD_SIZE + 1; // Offset by 1 because of the border
                cell[row, col].hasBomb = bombs[i];
            }
        }
        private void CountNeighbors()
        {
            for (int r = 1; r <= BOARD_SIZE; r++)
            {
                for (int c = 1; c <= BOARD_SIZE; c++)
                {
                    int count = 0;
                    if (cell[r - 1, c - 1].hasBomb) { count++; }
                    if (cell[r - 1, c].hasBomb) { count++; }
                    if (cell[r - 1, c + 1].hasBomb) { count++; }
                    if (cell[r, c - 1].hasBomb) { count++; }
                    if (cell[r, c + 1].hasBomb) { count++; }
                    if (cell[r + 1, c - 1].hasBomb) { count++; }
                    if (cell[r + 1, c].hasBomb) { count++; }
                    if (cell[r + 1, c + 1].hasBomb) { count++; }

                    cell[r, c].neighboringBombs = count;

                }
            }
            for (int r = 1; r < BOARD_SIZE - 1; r++)
            {
                for (int c = 1; c < BOARD_SIZE - 1; c++)
                {
                    Debug.Write(cell[r, c].neighboringBombs.ToString());
                    Debug.Write("");
                }
            }
        }

    }
}