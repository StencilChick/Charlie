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

namespace Charlie.Code
{
	// thing that handles dialogue
	public class Dialogue
	{
		// reference to the world object
		World world;

		// the different lines of dialouge taken from the stream
		string[] dialogueLines;
		// which line the dialogue is currently on
		int line;

		// value for scrolling text
		float textScroll;


		// current speaker
		string speaker;
		// what the speaker is currently saying
		string message;


		// whether currently reading part of a text block
		bool readingBlock;


		// what colour should be drawn for each person's name (defualts to white if the name is not present in the dictionary)
		Dictionary<string, Color> nameColours;


		// choices that can currently be selected and their respective values
		Dictionary<string, string> choices;
		// which choice is currently selected
		int selectedChoice;

		// current block of text selected
		string curBlock;

		// construct the dialogue
		public Dialogue(World world, string fileName)
		{
			// set the reference to the world object
			this.world = world;

			// set all these things to values
			nameColours = new Dictionary<string, Color>();
			choices = new Dictionary<string, string>();
			selectedChoice = 0;
			curBlock = "";
			readingBlock = false;

			// open up the file for this dialogue and get all the lines of saying stuff
			OpenDialogue(fileName);
		}

		// update function that mostly does controls for the dialogue
		public void Update(GameTime gameTime)
		{
			// do text scroll
			if (textScroll < message.Length) { textScroll += 30f * gameTime.ElapsedGameTime.Milliseconds / 1000; }

			// controls
			if (choices.Count > 0)
			{
				// what to do when there are choices to be choices
				if (world.InputHandler.KeyPressed(Keys.Up)) { selectedChoice--; if (selectedChoice < 0) { selectedChoice = choices.Count - 1; } world.AudioHandler.PlaySound("Menu"); }
				if (world.InputHandler.KeyPressed(Keys.Down)) { selectedChoice++; if (selectedChoice >= choices.Count) { selectedChoice = 0; } world.AudioHandler.PlaySound("Menu"); }

				// go forward with the currently selected choice
				if (world.InputHandler.KeyPressed(Keys.X))
				{
					// set the choice value
					curBlock = choices[choices.Keys.ElementAt<string>(selectedChoice)];

					world.AudioHandler.PlaySound("Menu");

					// clear the choices and continue
					selectedChoice = 0;
					choices = new Dictionary<string, string>();
					NextLine();
				}
			}
			else
			{
				// what to do all the rest of the dang time
				if (world.InputHandler.KeyPressed(Keys.X))
				{
					world.AudioHandler.PlaySound("Menu");

					if (textScroll < message.Length)
					{
						// the user pressed X while text was still scrolling, so scroll till end
						textScroll = message.Length;
					}
					else
					{
						// user pressed X while text was finished scrolling, so go to the next line of dialogue
						NextLine();
					}
				}
			}

			// make sure the dialogue initiated
			if (line < 0) { NextLine(); }
		}

		// drawey function that draws things
		public void Draw()
		{
			// draw the message
			if (speaker != "NARRATOR")
			{
				// draw the message normally
				world.SpriteBatch.Draw(world.ImageHolder.GUI["dialogueBox"], new Vector2(45, 350), Color.White);
				world.GUI.Text(
					world.ImageHolder.Fonts["SpriteFont1"],
					message.Substring(0, (int)textScroll),
					new Rectangle(50, 355, 544, 100),
					Color.White);
			}
			else
			{
				// draw backdrop for the narration
				world.SpriteBatch.Draw(world.ImageHolder.GUI["black"], new Rectangle(0, 0, 640, 100), Color.White);

				// draw the message in narrator-mode
				world.GUI.Text(
					world.ImageHolder.Fonts["SpriteFont1"],
					message.Substring(0, (int)textScroll),
					new Rectangle(30, 30, 580, 100),
					Color.White);
			}

			// draw the speaker's name
			if (speaker != "NARRATOR")
			{
				Color speakerColour;
				if (nameColours.ContainsKey(speaker)) { speakerColour = nameColours[speaker]; } else { speakerColour = Color.White; }
				world.GUI.Box(
					new Rectangle(30, 320, (int)world.ImageHolder.Fonts["SpriteFont1"].MeasureString(speaker).X + 3, 20),
					speaker,
					speakerColour,
					world.ImageHolder.GUI["dialogueBorder"],
					world.ImageHolder.GUI["dialogueCorner"],
					world.ImageHolder.GUI["dialogueBackdrop"]);
			}

			// draw choices to select if there are choices to be selected
			for (int i = 0; i < choices.Count; i++)
			{
				string choiceMsg = choices.Keys.ElementAt<string>(i);
				int choiceLen = (int)world.ImageHolder.Fonts["SpriteFont1"].MeasureString(choiceMsg).X;
				world.GUI.Box(
					new Rectangle(640 / 2 - choiceLen / 2, 290 - 40 * choices.Count + 40 * i, choiceLen, 20),
					choiceMsg,
					Color.White,
					world.ImageHolder.GUI["dialogueBorder"],
					world.ImageHolder.GUI["dialogueCorner"],
					world.ImageHolder.GUI["dialogueBackdrop"]);

				// draw arrows next to this if it is selected
				if (selectedChoice == i)
				{
					world.SpriteBatch.Draw(
						world.ImageHolder.GUI["choiceArrow"], 
						new Rectangle(640 / 2 - choiceLen / 2 - 25, 290 - 40 * choices.Count + 40 * i + 5, 10, 10), 
						Color.White);

					world.SpriteBatch.Draw(
						world.ImageHolder.GUI["choiceArrow"],
						new Rectangle(640 / 2 + choiceLen / 2 + 15, 290 - 40 * choices.Count + 40 * i + 5, 10, 10),
						new Rectangle(0, 0, 12, 12),
						Color.White,
						0f,
						new Vector2(0, 0),
						SpriteEffects.FlipHorizontally,
						0);
				}
			}
		}


		// method for opening a dialogue file
		private void OpenDialogue(string fileName)
		{
			// find which directory to work in based on whether this is in debug mode
			string directory;
			if (System.Diagnostics.Debugger.IsAttached)
			{
				directory = Directory.GetCurrentDirectory() + "../../../../Content/Defines/Dialogue/" + fileName + ".txt";
			}
			else
			{
				directory = Directory.GetCurrentDirectory() + "/Content/Defines/Dialogue/" + fileName + ".txt";
			}

			// get the stream of the file name and get the dialogue lines from that
			StreamReader stream = new StreamReader(directory);
			dialogueLines = stream.ReadToEnd().Split('\n');
			stream.Close();

			// set line to negative one so that the NextLine function when called for the first time will move to line zero, which is the first
			line = -1;
			// set text scroll to zero
			textScroll = 0;

			speaker = "";
			message = "";
		}


		// method for moving to the next line of dialogue
		private void NextLine()
		{
			// move to the next line
			line++;

			// if the end of the dialogue has been reached, then end this cruel charade
			if (line == dialogueLines.Length) { world.Dialogue = null; return; }
			// otherwise, parse the current line
			else
			{
				string curLine = dialogueLines[line];

				// ignore this line if it a comment, empty, or what the heck ever
				if (curLine.Trim().Length == 0 || curLine.Trim().Length == 1 || curLine.Trim()[0] == '#') { NextLine(); }
				else
				{
					// check if this line is part of a block
					if (curLine[0] == '\t' || curLine.Substring(0, 2) == "  ")
					{
						if (readingBlock)
						{
							curLine = curLine.Trim();
						}
						else
						{
							NextLine();
							return;
						}
					}

					// split the into its parameters
					string[] parameters = curLine.Split(':');
					// process the parameters
					ProcessParameters(parameters);
				}
			}
		}

		// takes the parameters of a line in string format and discerns what they're telling the game to do
		private void ProcessParameters(string[] parameters)
		{
			// check for a command in the first or second parameters
			switch (parameters[0].ToLower())
			{
				// the end dialogue command
				case "/end":
					world.Dialogue = null;
					return;

				// return to the main menu
				case "/menu":
					world.gameState = World.GameState.mainMenu;
					world.MainMenu.UpdateSaveInfo();

					world.Player.Inventory.Clear();
					world.Player.ClearDialogueVariables();
					world.Player.ClearTakenItems();
					world.Map = null;

					world.Dialogue = null;
					return;

				// go to the credits screen
				case "/credits":
					world.gameState = World.GameState.end;

					world.Player.Inventory.Clear();
					world.Player.ClearDialogueVariables();
					world.Player.ClearTakenItems();
					world.Map = null;

					world.Dialogue = null;

					world.AudioHandler.PlaySong("");
					return;

				// go to line command
				case "goto":
					line = int.Parse(parameters[1].Trim()) - 1;
					NextLine();
					break;

				// sets the current block
				case "block":
					curBlock = parameters[1].Trim();
					NextLine();
					break;

				// add a dialogue choice
				case "choice":
					choices.Add(parameters[2], parameters[1]);
					NextLine();
					break;
				case "choiceif":
					// only add this choice if a dialogue variable is set to a certain value
					if (EqualsStatementTrue(parameters[2]))
					{
						choices.Add(parameters[3], parameters[1]);
					}
					NextLine();
					break;

				// set a dialogue variable
				case "set":
					world.Player.SetDialogueVariable(parameters[1].Trim(), parameters[2].Trim());
					NextLine();
					break;

				// set a new text block value if a dialogue variable equals a certain value
				case "if":
					if (EqualsStatementTrue(parameters[1]))
					{
						curBlock = parameters[2];
					}
					NextLine();
					break;
				// same thing, but if the player has a certain item at a certain or higher quantitry
				case "ifitem":
					string[] protasis = parameters[1].Split('=');
					Console.WriteLine(world.Player.Inventory.ItemQuantity(protasis[0]));
					if (world.Player.Inventory.ItemQuantity(protasis[0].Trim()) >= int.Parse(protasis[1]))
					{
						curBlock = parameters[2];
					}
					NextLine();
					break;


				// performs a scene transition
				case "transition":
					string[] roomCoords = parameters[2].Split(' ');
					string[] playerCoords = parameters[3].Split(' ');

					if (parameters[1].Trim() != "") { world.Map = new MapCode.Map(world, parameters[1].Trim()); }
					world.Map.SetActiveRoom(int.Parse(roomCoords[0]), int.Parse(roomCoords[1]));
					world.Player.SetPosition(int.Parse(playerCoords[0]) * 32, int.Parse(playerCoords[1]) * 32);

					world.AudioHandler.PlaySong(world.Map.ActiveRoom.Music);

					NextLine();
					break;

				// add a number of an item to the inventory5
				case "additem":
					world.Player.Inventory.AddItem(world.ItemHolder.MakeItem(parameters[1].Trim()), int.Parse(parameters[2].Trim()));
					world.AudioHandler.PlaySound("Item");
					NextLine();
					break;

				// remove a number of an item from the inventory
				case "removeitem":
					world.Player.Inventory.RemoveItem(world.ItemHolder.MakeItem(parameters[1].Trim()), int.Parse(parameters[2].Trim()));
					NextLine();
					break;

				// turn the UI on and off
				case "drawui":
					world.Player.UIActive = !(parameters[1].Trim().ToLower() == "false");
					NextLine();
					break;

				// no recognisable command has been found from the first parameter, ergo look in the second
				default:
					if (parameters.Length == 1)
					{
						// text block command
						if (curBlock.Trim() == parameters[0].Trim()) { readingBlock = true; }
						else { readingBlock = false; }
						NextLine();
					}
					else
					{
						switch (parameters[1].ToLower())
						{
							// set colour command
							case "color":
								SetNameColour(parameters[0], parameters[2]);
								NextLine();
								break;
							case "colour":
								SetNameColour(parameters[0], parameters[2]);
								NextLine();
								break;

							// still no recognisable command, so assume a dialogue command
							// (and if it's not a dialogue command, then, hey, free error printing)
							default:
								speaker = parameters[0];
								message = parameters[1];
								if (choices.Count == 0) { textScroll = 0; } else { textScroll = message.Length; }
								break;
						}
					}
					break;
			}
		}


		// sets or adds a new colour value for a given name
		private void SetNameColour(string name, string colourParam)
		{
			// get a colour value from the colour parameter
			string[] colourVals = colourParam.Split(' ');
			Color colour = new Color(int.Parse(colourVals[0]), int.Parse(colourVals[1]), int.Parse(colourVals[2]));

			// set the name to the colour value
			if (nameColours.Keys.Contains<string>(name))
			{
				// name value already exists, so reassign it
				nameColours[name] = colour;
			}
			else
			{
				// name value does not exist, so add it
				nameColours.Add(name, colour);
			}
		}

		private bool EqualsStatementTrue(string statement)
		{
			string[] condition = statement.Split('=');
			if (world.Player.GetDialogueVariable(condition[0].Trim()) == condition[1].Trim())
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
