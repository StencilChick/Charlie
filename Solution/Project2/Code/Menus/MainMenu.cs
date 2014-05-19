// made by Gwendolyn Hart
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Charlie.Code;
using Charlie.Code.MapCode;
using Charlie.Code.Characters;

namespace Charlie.Code
{
	public class MainMenu : IMenu
	{
		// reference to the world
		World world;

		// options to be selected in the main menu
		enum Option
		{
			newGame, loadGame, editor, exit
		}
		Option option = Option.newGame;

		// states in the game
		enum States
		{
			main, newGame, loadGame
		}
		States state = States.main;

		// which save slot is currently selected
		int saveslot;

		// what to write over each save slot
		string[] slotDisp;

		// true if asking the player whethe to overwrite a save slot for a new game
		bool overwriteDialogue;

		// constructor
		public MainMenu(World world)
		{
			this.world = world;

			saveslot = 0;
			overwriteDialogue = false;
			UpdateSaveInfo();
		}

		public void Update(GameTime gameTime)
		{
			#region Options
			if (state == States.main)
			{
				// enable scrolling betwix different options
				if (world.InputHandler.KeyPressed(Keys.Up))
				{
					int newVal = (int)option - 1;
					if (newVal < 0)
					{
						newVal = Enum.GetValues(typeof(Option)).Length - 1;
					}
					option = (Option)newVal;

					world.AudioHandler.PlaySound("Menu");
				}
				if (world.InputHandler.KeyPressed(Keys.Down))
				{
					int newVal = (int)option + 1;
					if (newVal >= Enum.GetValues(typeof(Option)).Length)
					{
						newVal = 0;
					}
					option = (Option)newVal;

					world.AudioHandler.PlaySound("Menu");
				}

				// what to do when an option is selected
				if (world.InputHandler.KeyPressed(Keys.X))
				{
					switch (option)
					{
						// go to the gameplay
						case Option.newGame:
							state = States.newGame;
							break;

						// go to the load game screen
						case Option.loadGame:
							state = States.loadGame;
							break;

						// go to the editor
						case Option.editor:
							world.AudioHandler.PlaySong("");
							world.gameState = World.GameState.editor;
							break;

						// end the gmae
						case Option.exit:
							world.Exit();
							break;
					}

					world.AudioHandler.PlaySound("Menu");
				}
			}
			#endregion
			#region SaveSlots
			else
			{
				// if the dialogue for overwriting a save file is open
				if (overwriteDialogue)
				{
					if (world.InputHandler.KeyPressed(Keys.Escape))
					{
						overwriteDialogue = false;

						world.AudioHandler.PlaySound("Menu");
					}
					else if (world.InputHandler.KeyPressed(Keys.Enter))
					{
						overwriteDialogue = false;
						NewGame();

						world.AudioHandler.PlaySound("Menu");
					}
				}
				else
				{
					// go back to the main state
					if (world.InputHandler.KeyPressed(Keys.Escape) || world.InputHandler.KeyPressed(Keys.Z))
					{
						state = States.main;

						world.AudioHandler.PlaySound("Menu");
					}

					// scroll between save slots
					if (world.InputHandler.KeyPressed(Keys.Up))
					{
						saveslot--;
						if (saveslot < 0) { saveslot = 2; }

						world.AudioHandler.PlaySound("Menu");
					}
					if (world.InputHandler.KeyPressed(Keys.Down))
					{
						saveslot++;
						if (saveslot > 2) { saveslot = 0; }

						world.AudioHandler.PlaySound("Menu");
					}

					// select a save slot
					if (world.InputHandler.KeyPressed(Keys.X))
					{
						switch (state)
						{
							// begin a new game in the slot
							case States.newGame:
								if (!SaveExists(saveslot))
								{
									NewGame();
								}
								else
								{
									overwriteDialogue = true;
								}

								world.AudioHandler.PlaySound("Menu");
								break;

							// load the game in the slot
							case States.loadGame:
								if (SaveExists(saveslot))
								{
									world.Player.SaveSlot = saveslot;
									world.Player.Load();

									state = States.main;
									world.gameState = World.GameState.gamePlay;

									world.AudioHandler.PlaySong(world.Map.ActiveRoom.Music);

									world.AudioHandler.PlaySound("Menu");
								}
								break;
						}
					}
				}
			}
			#endregion
		}

		public void Draw()
		{
			// do different things based on whether we're looking at save slots or not
			if (state == States.main)
			{
				// Zhu Li, do the thing!
				world.SpriteBatch.Draw(world.ImageHolder.GUI["mainMenu"], new Vector2(0, 0), Color.White);

				world.SpriteBatch.Draw(
					world.ImageHolder.GUI["mainMenuArrow"],
					new Vector2(320 - 50, 330 + 22 * (int)option),
					Color.White);
				world.SpriteBatch.Draw(
					world.ImageHolder.GUI["mainMenuArrow"],
					new Vector2(320 + 60, 330 + 22 * (int)option),
					new Rectangle(0, 0, 6, 10),
					Color.White,
					0f,
					new Vector2(0, 0),
					1f,
					SpriteEffects.FlipHorizontally,
					0f);
			}
			else
			{
				// draw the background
				world.SpriteBatch.Draw(world.ImageHolder.GUI["menuBackdrop"], new Rectangle(0, 0, 640, 480), Color.White);

				// draw the save slots
				for (int i = 0; i < 3; i++)
				{
					// the box
					Rectangle rect = new Rectangle(60, 15 + i * 160, 520, 130);
					world.GUI.Box(
						rect,
						"",
						Color.White,
						world.ImageHolder.GUI["slotBorder"],
						world.ImageHolder.GUI["slotCorner"],
						world.ImageHolder.GUI["slotBackdrop"]);

					// the info in the box
					if (!SaveExists(i))
					{
						// empty slot
						world.GUI.Text(world.ImageHolder.Fonts["SpriteFont1"], i + " - Empty", rect, Color.White);
					}
					else
					{
						// draw the data in this slot
						string[] data = slotDisp[i].Split(':');

						// location and other info
						world.GUI.Text(world.ImageHolder.Fonts["SpriteFont1"], i + " - Occupied\n\n\n\nLocation: " + data[0], rect, Color.White);

						// sprite
						world.SpriteBatch.Draw(world.ImageHolder.CharacterSheets["hero"], new Vector2(60, 55 + i * 160), new Rectangle(0, 0, 32, 32), Color.White);

						// stats
						int health = int.Parse(data[1]);
						int mana = int.Parse(data[2]);

						// health stat
						for (int h = 0; h < health; h++)
						{
							world.SpriteBatch.Draw(
								world.ImageHolder.GUI["heart"],
								new Vector2(640 - 80 - h * 21, 25 + i * 160),
								new Rectangle(0, 0, 18, 16),
								Color.White);
						}

						// mana stat
						for (int m = 0; m < mana; m++)
						{
							world.SpriteBatch.Draw(
								world.ImageHolder.GUI["mana"],
								new Vector2(640 - 80 - m * 21, 50 + i * 160),
								new Rectangle(0, 0, 18, 18),
								Color.White);
						}
					}

					// the arrow pointing to this slot if it is currently selected
					if (saveslot == i)
					{
						world.SpriteBatch.Draw(
							world.ImageHolder.GUI["choiceArrow"],
							new Vector2(20, 80 + 160 * i),
							Color.White);
						world.SpriteBatch.Draw(
							world.ImageHolder.GUI["choiceArrow"],
							new Vector2(605, 80 + 160 * i),
							new Rectangle(0, 0, 12, 12),
							Color.White,
							0f,
							new Vector2(0, 0),
							1f,
							SpriteEffects.FlipHorizontally,
							0);
					}
				}

				// draw the dialogue for overwriting a save slot if it is open
				if (overwriteDialogue)
				{
					world.GUI.Box(
						new Rectangle(320 - 140, 120, 280, 90),
						"Over-Write This Save Slot?\nData will be permanently lost.\n\n[Enter to Accept, Escape to Cancel]",
						Color.White,
						world.ImageHolder.GUI["slotBorder"],
						world.ImageHolder.GUI["slotCorner"],
						world.ImageHolder.GUI["slotBackdrop"]);
				}
			}
		}

		// check to see whether a save file exists
		private bool SaveExists(int i)
		{
			return File.Exists(Directory.GetCurrentDirectory() + "/Saves/save" + i + ".txt");
		}

		// gets save information from the save files
		private string[] GetSaveInfo()
		{
			// value to return 
			string[] value = new string[3];

			// set the value to things
			for (int i = 0; i < 3; i++)
			{
				if (SaveExists(i))
				{
					// get the info from the appropriate save file
					StreamReader stream = new StreamReader(Directory.GetCurrentDirectory() + "/Saves/save" + i + ".txt");
					value[i] = stream.ReadLine();
					stream.Close();
				}
				else
				{
					// no save here, so set the index to null
					value[i] = null;
				}
			}

			// return the value
			return value;
		}

		public void UpdateSaveInfo()
		{
			// put the save game information into the slots
			slotDisp = GetSaveInfo();
		}

		// Mandy Ryll
		// sets everything up for a new game
		private void SetBeginningState()
		{
			StreamReader input = null;
			input = new StreamReader("./Content/Defines/start.txt");
			string[] linesArray = input.ReadToEnd().Split('\n');
            
			world.Map = new Map(world, linesArray[0].Trim());
	
			string[] roomCoords = linesArray[1].Trim().Split(' ');
			world.Map.SetActiveRoom(int.Parse(roomCoords[0]), int.Parse(roomCoords[1]));
	
			string[] playerCoords = linesArray[2].Trim().Split(' ');
			world.Player.SetPosition(int.Parse(playerCoords[0]) * 32, int.Parse(playerCoords[1]) * 32);
	
			Array dirType = Enum.GetValues(typeof(Character.Direction));
			for (int i = 0; i < dirType.Length; i++) 
			{
				if (dirType.GetValue(i).ToString() == linesArray[3].Trim().ToLower()) 
				{
					world.Player.SetDirection((Character.Direction)i);
					break;
				}
			}

			world.Player.MaxHealth = 3;
			world.Player.Health = 3;
			world.Player.MaxMana = 4;
			world.Player.Mana = 4;
		}

		private void NewGame()
		{
			state = States.main;

			world.Player.SaveSlot = saveslot;
			world.gameState = World.GameState.gamePlay;
			SetBeginningState();

			world.AudioHandler.PlaySong(world.Map.ActiveRoom.Music);
		}
	}
}
