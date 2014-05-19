// made by Gwendolyn Hart
// also Mandy Ryll
// also also Dakota Herold
#region Using Statements
using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;

using Charlie.Code;
using Charlie.Code.MapCode;
using Charlie.Code.Characters;
using Charlie.Code.Items;
using Charlie.Code.Audio;
#endregion

namespace Charlie
{
    /// <summary>
    /// My name is the Main Type, class of classes:
	/// Look on my works, ye Mighty, and despair!
    /// </summary>
    public class World : Game
    {
        GraphicsDeviceManager graphics;

        SpriteBatch spriteBatch;
        public SpriteBatch SpriteBatch { get { return spriteBatch; } }

        // different states the game can be in
        public enum GameState
        {
            mainMenu, gameMenu, gamePlay, editor, end
        }
        public GameState gameState;

        // input handler
        InputHandler inputHandler;
        public InputHandler InputHandler { get { return inputHandler; } }

        // graphics holder
        ImageHolder imageHolder;
        public ImageHolder ImageHolder { get { return imageHolder; } }

        // the thing that draws all the gui elements
        GUI gui;
        public GUI GUI { get { return gui; } }

		// the things that holds all the different characters that can be put in a map
		CharacterHolder characterHolder;
		public CharacterHolder CharacterHolder { get { return characterHolder; } }

		// the things that holds all the different items
		ItemHolder itemHolder;
		public ItemHolder ItemHolder { get { return itemHolder; } }

		// the class for the main menu
		MainMenu mainMenu;
		public MainMenu MainMenu { get { return mainMenu; } }

		// the class for the game menu
		GameMenu gameMenu;
		public GameMenu GameMenu { get { return gameMenu; } }

		// the class for the end menu
		EndMenu endMenu;
		public EndMenu EndMenu { get { return endMenu; } }

        // the current map that everyone walks around in and whatnot
        Map map;
        public Map Map { 
			get { return map; } set { map = value; }
		}

		// the player
		Player player;
		public Player Player { get { return player; } }

		// the dialogue thing
		Dialogue dialogue;
		public Dialogue Dialogue {
			get { return dialogue; } set { dialogue = value; }
		}

		// the two things for audio
		AudioHolder audioHolder;
		public AudioHolder AudioHolder { get { return audioHolder; } }
		AudioHandler audioHandler;
		public AudioHandler AudioHandler { get { return audioHandler; } }

        // the editor for building maps and levels and whatnot
        Editor editor;
        public Editor Editor { get { return editor; } }

        public World()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            // set size of the screen
            graphics.PreferredBackBufferWidth = 640;
            graphics.PreferredBackBufferHeight = 480;

            // set the game state
            gameState = GameState.mainMenu;

            // initialise everything that can be initialised without graphics
            inputHandler = new InputHandler(this);
            imageHolder = new ImageHolder();
			audioHolder = new AudioHolder(this);
			audioHandler = new AudioHandler(this);
			characterHolder = new CharacterHolder(this);
			itemHolder = new ItemHolder(this);
            gui = new GUI(this);
			player = new Player(this);
            editor = new Editor(this);
			mainMenu = new MainMenu(this);
			gameMenu = new GameMenu(this);
			endMenu = new EndMenu(this);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            imageHolder.LoadImages(this);
			audioHolder.LoadContent();

			player.SetImage();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (Microsoft.Xna.Framework.Input.GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            // TODO: Add your update logic here
            KeyboardState keyState = Keyboard.GetState();
            // update the input handler before anything else
            inputHandler.Update(Keyboard.GetState(), Mouse.GetState());


            // do things based on the different states
            switch (gameState) 
			{
				// stuff for the main menu
				case GameState.mainMenu:
					// play the music of the main menu
					audioHandler.PlaySong("YERMSAYN");

					// update the main menu
					mainMenu.Update(gameTime);
					break;

				// stuff for the menu you have in gameplay
				case GameState.gameMenu:
					gameMenu.Update(gameTime);
					break;

				// stuff for gameplay
				case GameState.gamePlay:
					// update the player
					player.Update(gameTime);

					// update all that junk on the map
					map.Update(gameTime);

					// update the dialogue if it exists
					if (dialogue != null) { dialogue.Update(gameTime); }

					// allow opening of the game menu when dialouge is not happening
					else
					{
						if (inputHandler.KeyPressed(Keys.Enter))
						{
							gameState = GameState.gameMenu;
							gameMenu.UpdateItems();
						}
					}

					// if the player is null, go to the game over screen
					if (player == null)
					{
						gameState = GameState.end;
					}

					// if testing a map, escape key returns to the editor
					if (editor.Testing && inputHandler.KeyPressed(Keys.Escape))
					{
						gameState = GameState.editor;
						editor.Testing = false;

						// reset all these things
						player.ClearDialogueVariables();
						player.ClearTakenItems();
						dialogue = null;

						audioHandler.PlaySong("");

						// reload the map in order to set its values to default (killed enemies are unkilled, activators unactivated, etc.)
						map = new Map(this, editor.FileName, map.ActiveRoomPos);
					}
					break;

				// stuff for updating the editor
				case GameState.editor:
					editor.Update();
					break;

				// stuff for the end game
				case GameState.end:
					endMenu.Update(gameTime);
					break;
			}


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			switch (gameState) 
			{
				// stuff for drawing the map
				case GameState.mainMenu:
					mainMenu.Draw();
					break;

				case GameState.gameMenu:
					gameMenu.Draw();
					break;

				// draw gameplay stuff
				case GameState.gamePlay:
					// draw the map
					map.Draw(spriteBatch);

					// draw the dialogue if it exists
					if (dialogue != null) { dialogue.Draw(); }

					// draw the player
					player.DrawUI(spriteBatch);
					break;

				// draw all the editor stuff
				case GameState.editor:
					editor.Draw(spriteBatch);
					break;

				// draw stuff for the end game credits
				case GameState.end:
					endMenu.Draw();
					break;
			}

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
