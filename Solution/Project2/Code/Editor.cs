// made by Gwendolyn Hart
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Charlie.Code.MapCode;
using Charlie.Code.Characters;
using Charlie.Code.Activators;
using Charlie.Code.Items;

namespace Charlie.Code
{
    // builds various maps in the game
    public class Editor
    {
        // reference to the world object
        World world;

        // different modes the editor can be in
        enum Mode
        {
            tile, obj, npc, misc
        }
        Mode mode;


        // a value for how much to scale the map by
        float mapScale;
        // a vector for how to offset the map from the top-left edge
        Vector2 mapOffset;

		// whether or not to draw the grid over the tiles on the map
		bool drawGrid;
		public bool DrawGrid { get { return drawGrid; } }


		// file name of the current map
		string fileName;
		public string FileName { get { return fileName; } }


		// value for whether a valid object is currently named
		bool objectValid;
		// direction the object should face if applicable
		Character.Direction objectDirection;
		// name of the current object
		string objectName;


		# region Tile_Vars
		// coordinates for which part of the tileset a tile should told to display
        Vector2 tileCoords;
		// whether to change a tile's collision state instead of changing its tile coordinates
		bool placingCols;
		public bool PlacingColliders { get { return placingCols; } }
		#endregion


		#region NPC_Vars
		// names of the npc and monster currently to place on the map which the user has entered into the appropriate dialogue boxes
		string npcName;
		string monsterName;
		// whether the user is placing an npc or a monster
		bool placingMonster;

		// dialogue for the npc to start
		string dialogue;
		// values for whether the npc is an ally or is hostile
		bool ally;
		bool hostile;
		#endregion


		#region Obj_Vars
		// enumerator for the types of objects
		enum Objs
		{
			transition, dialogue, item
		}
		Objs miscObj = Objs.transition;

		// transition values
		Vector2 playerPos;
		Vector2 roomPos;
		string transMapName;

		// dialogue values
		string dialogueName = "";

		// item values
		string itemName = "";
		#endregion

		#region Misc_Vars
		bool placingSavepoint = false;
		#endregion


		// let's the user select and edit an object
		Object selectedObj;


		// true if currently testing a map
		bool testing;
		public bool Testing 
		{ 
			get { return testing; }
			set { testing = value; }
		}
		// values for where to start the player when testing
		Vector2 startPos;
		public Vector2 StartPos { get { return startPos; } set { startPos = value; } }
		Vector2 startRoomPos;
		public Vector2 StartRoomPos { get { return startRoomPos; } set { startRoomPos = value; } }

		
		// constructor
        public Editor(World world)
		{
			this.world = world;

            // set the map mode
            mode = Mode.tile;

            // set the values for displaying the map
            mapScale = 0.5f;
            mapOffset = new Vector2(30, 480 / 2 - 480 * mapScale / 2);
			drawGrid = true;

			// set the file name to a value
			fileName = "file_name";

            // set the coordinates for the tiles to change their image coordinates to
            tileCoords = new Vector2(0, 0);
			// set placing colliders to false
			placingCols = false;

			// values for placing objects
			objectValid = false;
			objectDirection = Character.Direction.down;
			objectName = "";

			// set the stuff for placing npcs and monsters
			npcName = "NPC-Name";
			monsterName = "Monster-Name";
			placingMonster = false;

			// stuff for placing npcs
			dialogue = "";
			ally = false;
			hostile = false;

			// stuff for the objects
			playerPos = new Vector2(0, 0);
			roomPos = new Vector2(0, 0);
			transMapName = "";


			// set this thing to null
			selectedObj = null;


			// values for testing
			testing = false;
			startPos = new Vector2(10 * 32, 7 * 32);
			startRoomPos = new Vector2(0, 0);
		}

        // does the update-y stuff
        public void Update()
        {
			// what to do if the user hits escape
			if (world.InputHandler.KeyPressed(Keys.Escape))
			{
				if (world.Map != null)
				{
					CloseMap();
				}
				else
				{
					world.gameState = World.GameState.mainMenu;
				}
			}

			// check whether the map exists
			if (world.Map != null)
			{
				// clean out disposed objects on the map from the last frame
				world.Map.ActiveRoom.CleanObjects();

				// if holding control, change the starting test position instead of whatever else would normally be changed
				if (world.InputHandler.KeyHeld(Keys.LeftControl) || world.InputHandler.KeyHeld(Keys.RightControl))
				{
					if (world.InputHandler.MouseButtonPressed(0))
					{
						Vector2 mousePos = world.InputHandler.MousePosition;
						if (mousePos.X >= mapOffset.X && mousePos.X < mapOffset.X + 640 * mapScale && mousePos.Y >= mapOffset.Y && mousePos.Y < mapOffset.Y + 480 * mapScale)
						{
							startPos = new Vector2((int)((mousePos.X - mapOffset.X) / (mapScale * 32)) * 32, (int)((mousePos.Y - mapOffset.Y) / (mapScale * 32)) * 32);
							startRoomPos = world.Map.ActiveRoomPos;
						}
					}
				}
				else
				{
					// everyone needs to know where the mouse is
					Vector2 mousePos = world.InputHandler.MousePosition;

					// different controls for the different modes
					switch (mode)
					{
						// tile-placing controls
						#region Tile_Mode
						case Mode.tile:
							// if mouse is held over a tile, change that tile's attributes
							if (mousePos.X >= mapOffset.X && mousePos.X < mapOffset.X + 640 * mapScale && mousePos.Y >= mapOffset.Y && mousePos.Y < mapOffset.Y + 480 * mapScale)
							{
								if (world.InputHandler.MouseButtonHeld(0))
								{
									// determine whether to change the tile's coordinates or collsion state
									if (placingCols)
									{
										// change the tile's collision state
										if (world.InputHandler.MouseButtonPressed(0))
										{
											ChangeTileCollision(new Vector2((int)((mousePos.X - mapOffset.X) / (mapScale * 32)) * 32, (int)((mousePos.Y - mapOffset.Y) / (mapScale * 32)) * 32));
										}
									}
									else
									{
										// change the tile's image coordinates
										ChangeTileCoords(new Vector2((int)((mousePos.X - mapOffset.X) / (mapScale * 32)) * 32, (int)((mousePos.Y - mapOffset.Y) / (mapScale * 32)) * 32));
									}
								}
							}
							break;
						#endregion

						// objects
						#region Obj_Mode
						case Mode.obj:
							if (mousePos.X >= mapOffset.X && mousePos.X < mapOffset.X + 640 * mapScale && mousePos.Y >= mapOffset.Y && mousePos.Y < mapOffset.Y + 480 * mapScale)
							{
								// position of the tile the mouse is over
								Vector2 pos = new Vector2((int)((mousePos.X - mapOffset.X) / (mapScale * 32)) * 32, (int)((mousePos.Y - mapOffset.Y) / (mapScale * 32)) * 32);
								// the mouse has just been pressed
								if (world.InputHandler.MouseButtonPressed(0))
								{
									// make sure the user isn't trying to delete something
									if (!world.InputHandler.KeyHeld(Keys.Delete))
									{
										// check if there's already an object here
										Object objAtPos = world.Map.ActiveRoom.FindObjectAt((int)pos.X, (int)pos.Y);
										if (objAtPos != null)
										{
											// there is an object here, so select it
											selectedObj = objAtPos;
										}
										else
										{
											// there is no object here, so place the user-defined object here
											switch (miscObj)
											{
												// place a transition object
												case Objs.transition:
													world.Map.ActiveRoom.AddObject(new Transition(world, pos, playerPos, roomPos, transMapName));
													break;

												// place a dialogue trigger object
												case Objs.dialogue:
													world.Map.ActiveRoom.AddObject(new DialogueTrigger(world, pos, dialogueName));
													break;

												// place an item
												case Objs.item:
													if (objectValid)
													{
														Item item = world.ItemHolder.MakeItem(objectName);
														item.SetPosition((int)pos.X, (int)pos.Y);
														world.Map.ActiveRoom.AddObject(item);
													}
													break;
											}
										}
									}
									else
									{
										// delete the object where the user is clicking
										world.Map.ActiveRoom.RemoveObjectAt((int)pos.X, (int)pos.Y);

										if (selectedObj != null)
										{
											if (!world.Map.ActiveRoom.Objects.Contains(selectedObj)) { selectedObj = null; }
										}
									}
								}
								// the mouse is being held down
								else if (world.InputHandler.MouseButtonHeld(0))
								{
									if (selectedObj != null && world.Map.ActiveRoom.FindObjectAt((int)pos.X, (int)pos.Y) == null)
									{
										selectedObj.SetPosition((int)pos.X, (int)pos.Y);
									}
								}
							}

							break;
						#endregion

						// npc- / monster-placing controls
						# region NPC_Mode
						case Mode.npc:
							// if the user clicks on a tile, place the npc/monster there
							if (mousePos.X >= mapOffset.X && mousePos.X < mapOffset.X + 640 * mapScale && mousePos.Y >= mapOffset.Y && mousePos.Y < mapOffset.Y + 480 * mapScale)
							{
								// determine the position of the tile the mouse is over
								Vector2 pos = new Vector2((int)((mousePos.X - mapOffset.X) / (mapScale * 32)) * 32, (int)((mousePos.Y - mapOffset.Y) / (mapScale * 32)) * 32);
								if (world.InputHandler.MouseButtonPressed(0))
								{
									if (!world.InputHandler.KeyHeld(Keys.Delete))
									{
										// check wether there's an object in that space
										Object objAtPos = world.Map.ActiveRoom.FindObjectAt((int)pos.X, (int)pos.Y);
										if (objAtPos != null)
										{
											// select this object
											selectedObj = objAtPos;
										} 
										else
										{
											selectedObj = null;

											// add an object to this spot
											if (objectValid)
											{
												// add a character to the map
												Character character;
												if (!placingMonster)
												{
													// define an npc
													character = world.CharacterHolder.MakeNPC(objectName, objectName, dialogue, hostile, ally);
												}
												else
												{
													// define a monster
													character = world.CharacterHolder.MakeMonster(objectName, objectName);
												}
												character.SetPosition((int)pos.X, (int)pos.Y);
												character.SetDirection(objectDirection);

												// slap it onto the map
												world.Map.ActiveRoom.AddObject(character);
											}
										}
									}
									else
									{
										// the user is holding delete, ergo delete an object here instead of placing it
										world.Map.ActiveRoom.RemoveObjectAt((int)pos.X, (int)pos.Y);

										if (selectedObj != null)
										{
											if (!world.Map.ActiveRoom.Objects.Contains(selectedObj)) { selectedObj = null; }
										}
									}
								}
								else if (world.InputHandler.MouseButtonHeld(0))
								{
									if (selectedObj != null && world.Map.ActiveRoom.FindObjectAt((int)pos.X, (int)pos.Y) == null)
									{
										selectedObj.SetPosition((int)pos.X, (int)pos.Y);
									}
								}
							}

							break;
						#endregion

						// miscellaneous mode
						#region Misc_Mode
						case Mode.misc:
							// check whether the mouse is over the map area
							if (mousePos.X >= mapOffset.X && mousePos.X < mapOffset.X + 640 * mapScale && mousePos.Y >= mapOffset.Y && mousePos.Y < mapOffset.Y + 480 * mapScale)
							{
								// act when the left mouse button is clicked
								if (world.InputHandler.MouseButtonPressed(0))
								{
									// the position of the mouse in tile position
									Vector2 pos = new Vector2((int)((mousePos.X - mapOffset.X) / (mapScale * 32)) * 32, (int)((mousePos.Y - mapOffset.Y) / (mapScale * 32)) * 32);

									// see whether the delete key is held
									if (!world.InputHandler.KeyHeld(Keys.Delete))
									{
										// not held, so interact with this space
										Object obj = world.Map.ActiveRoom.FindObjectAt((int)pos.X, (int)pos.Y);
										if (obj != null)
										{
											// there is an object here, so select it
											selectedObj = obj;
										}
										else
										{

											// there is no object here, so place is a save point
											if (placingSavepoint)
											{
												SavePoint savepoint = new SavePoint(world);
												savepoint.SetPosition((int)pos.X, (int)pos.Y);
												world.Map.ActiveRoom.AddObject(savepoint);
											}
										}
									}
									else
									{
										// is held, so delete the object here
										world.Map.ActiveRoom.RemoveObjectAt((int)pos.X, (int)pos.Y);
									}
								}
							}

							break;
						#endregion
					}
				}
			}
        }

        // what to draw to the screen when the editor is active
		// (ye gods, this is a long function! >_>)
        public void Draw(SpriteBatch spriteBatch)
        {
			// check whether the map exists
			if (world.Map == null)
			{
				// exit button
				if (world.GUI.Button(new Rectangle(10, 10, 60, 20), "Return")) { world.gameState = World.GameState.mainMenu; }

				// if the map does not exist yet, draw dialogue for loading it
				spriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], "Name of the Map File:", new Vector2(320 - 128, 175), Color.White);

				fileName = world.GUI.TextField(new Rectangle(320 - 125, 200, 275, 20), fileName, 30);
				// button for creating/loading the map
				if (world.GUI.Button(new Rectangle(320 + 100, 250, 50, 20), "Open") && fileName.Length > 0)
				{
					OpenMap();
				}
			}
			else
			{
				#region Junk_That_Is_Drawn_All_The_Time
				// draw the room for the editor
				world.Map.EditorDraw(spriteBatch, mapScale, mapOffset);

				// draw the save and close buttons
				if (world.GUI.Button(new Rectangle(10, 10, 50, 20), "Close")) { CloseMap(); objectValid = false; return; }
				if (world.GUI.Button(new Rectangle(73, 10, 50, 20), "Save")) { SaveMap(); }

				// draw the button for letting the user test the map
				if (world.GUI.Button(new Rectangle(136, 10, 50, 20), "Test")) { TestMap(); }
				spriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], fileName + ".txt", new Vector2(199, 10), Color.White);

				// draw thing that lets the user name the map
				world.Map.Name = world.GUI.TextField(new Rectangle(10, 43, 200, 20), world.Map.Name, 30);

				// things that names the current room
				spriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], "Room: { " + world.Map.ActiveRoomPos.X + ", " + world.Map.ActiveRoomPos.Y + " }", new Vector2(30, 100), Color.White);

				// draw things to move between the different modes
				if (world.GUI.Button(new Rectangle(640 - 215, 10, 40, 20), "Tiles")) { mode = Mode.tile; objectValid = false; }
				if (world.GUI.Button(new Rectangle(640 - 160, 10, 40, 20), "Objs")) { mode = Mode.obj; objectValid = false; }
				if (world.GUI.Button(new Rectangle(640 - 105, 10, 40, 20), "NPC")) { mode = Mode.npc; objectValid = false; }
				if (world.GUI.Button(new Rectangle(640 - 50, 10, 40, 20), "Misc")) { mode = Mode.misc; objectValid = false; }
				spriteBatch.Draw(world.ImageHolder.GUI["editorModePointer"], new Vector2(640 - 215 + 55 * (int)mode, 35), Color.White);

				// button thingy for turning the grid on and off
				string gridMsg;
				if (drawGrid) { gridMsg = "Grid On"; }
				else { gridMsg = "Grid Off"; }
				if (world.GUI.Button(new Rectangle(640 - 155, 65, 70, 20), gridMsg))
				{
					if (drawGrid) { drawGrid = false; }
					else { drawGrid = true; }
				}
				#endregion

				// draw different editor things for different modes
				switch (mode)
				{
					// tile-placin' mode!  Yeah!
					#region Tile_Mode
					case Mode.tile:
						// allow user to change tilesets
						world.GUI.TextField(new Rectangle(640 - 195, 160, 150, 20), world.Map.Tileset);
						if (world.GUI.Button(new Rectangle(640 - 230, 160, 20, 20), " < ")) { PrevTileset(); }
						if (world.GUI.Button(new Rectangle(640 - 30, 160, 20, 20), " > ")) { NextTileset(); }

						// buttons for changing image coordinates with which to assign a tile
						// X
						spriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], "X: ", new Vector2(640 - 140 - 60, 480 - 260), Color.White);
						world.GUI.Box(new Rectangle(640 - 75 - 60, 480 - 260, 30, 20), "" + tileCoords.X);
						if (world.GUI.Button(new Rectangle(640 - 110 - 60, 480 - 260, 20, 20), " - ")) { tileCoords.X--; }
						if (world.GUI.Button(new Rectangle(640 - 30 - 60, 480 - 260, 20, 20), " + ")) { tileCoords.X++; }
						// Y
						spriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], "Y: ", new Vector2(640 - 140 - 60, 480 - 228), Color.White);
						world.GUI.Box(new Rectangle(640 - 75 - 60, 480 - 228, 30, 20), "" + tileCoords.Y);
						if (world.GUI.Button(new Rectangle(640 - 110 - 60, 480 - 228, 20, 20), " - ")) { tileCoords.Y--; }
						if (world.GUI.Button(new Rectangle(640 - 30 - 60, 480 - 228, 20, 20), " + ")) { tileCoords.Y++; }

						// switch between changing image coordinates and collision states
						string colMsg;
						if (placingCols) { colMsg = "Placing Colliders"; }
						else { colMsg = "Placing Tiles"; }
						if (world.GUI.Button(new Rectangle(640 - 190, 480 - 170, 130, 20), colMsg))
						{
							if (placingCols) { placingCols = false; }
							else { placingCols = true; }
						}

						break;
					#endregion

					// things like map transitions, items, dialogue triggers, items, and whatnot
					# region Obj_Mode
					case Mode.obj:
						// button for cycling between the types of miscellaneous objects
						if (world.GUI.Button(new Rectangle(640 - 190, 160, 140, 20), miscObj.ToString()))
						{
							int miscEnumVal = (int)miscObj + 1;
							if (miscEnumVal >= Enum.GetValues(typeof(Objs)).Length) { miscEnumVal = 0; }
							miscObj = (Objs)miscEnumVal;

							objectValid = false;
						}

						// different dialogues for the type of object to be placed
						switch (miscObj)
						{
							// transition the player to another location
							#region Transition
							case Objs.transition:
								// player position
								spriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], "Player Position", new Vector2(640 - 220, 200), Color.White);
								// x
								if (world.GUI.Button(new Rectangle(640 - 220, 225, 20, 20), " - ")) { playerPos.X--; if (playerPos.X < 0) { playerPos.X = 0; } }
								world.GUI.Box(new Rectangle(640 - 190, 225, 30, 20), "" + playerPos.X);
								if (world.GUI.Button(new Rectangle(640 - 150, 225, 20, 20), " + ")) { playerPos.X++; if (playerPos.X > 19) { playerPos.X = 19; } }
								// y
								if (world.GUI.Button(new Rectangle(640 - 110, 225, 20, 20), " - ")) { playerPos.Y--; if (playerPos.Y < 0) { playerPos.Y = 0; } }
								world.GUI.Box(new Rectangle(640 - 80, 225, 30, 20), "" + playerPos.Y);
								if (world.GUI.Button(new Rectangle(640 - 40, 225, 20, 20), " + ")) { playerPos.Y++; if (playerPos.Y > 14) { playerPos.Y = 14; } }

								// room position
								spriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], "Room Position", new Vector2(640 - 220, 260), Color.White);
								// x
								if (world.GUI.Button(new Rectangle(640 - 220, 285, 20, 20), " - ")) { roomPos.X--; }
								world.GUI.Box(new Rectangle(640 - 190, 285, 30, 20), "" + roomPos.X);
								if (world.GUI.Button(new Rectangle(640 - 150, 285, 20, 20), " + ")) { roomPos.X++; }
								// y
								if (world.GUI.Button(new Rectangle(640 - 110, 285, 20, 20), " - ")) { roomPos.Y--; }
								world.GUI.Box(new Rectangle(640 - 80, 285, 30, 20), "" + roomPos.Y);
								if (world.GUI.Button(new Rectangle(640 - 40, 285, 20, 20), " + ")) { roomPos.Y++; }

								// map name
								transMapName = world.GUI.TextField(new Rectangle(640 - 220, 330, 200, 20), transMapName);

								break;
							#endregion

							// thing that starts a dialogue
							#region Dialouge
							case Objs.dialogue:
								// dialogue name
								spriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], "Dialogue Name", new Vector2(640 - 190, 200), Color.White);
								dialogueName = world.GUI.TextField(new Rectangle(640 - 190, 225, 140, 20), dialogueName);
								break;
							#endregion

							// load and place items on the map
							#region Item
							case Objs.item:
								string prevItemName = itemName;
								itemName = world.GUI.TextField(new Rectangle(640 - 190, 240, 140, 20), itemName);

								// if the user has changed the name in the field, check whether they have named a valid item
								if (prevItemName != itemName)
								{
									if (world.ItemHolder.ItemParams.ContainsKey(itemName))
									{
										// a valid item, ergo set up to place it
										objectValid = true;
										objectName = itemName;
									}
									else
									{
										objectValid = false;
									}
								}

								// if there's a valid item, say it's valid
								if (objectValid)
								{
									world.GUI.Label(
										world.ImageHolder.Fonts["SpriteFont1"],
										"Item Valid",
										new Rectangle(640 - 190, 270, 140, 20),
										Color.White);
								}
								break;
							#endregion
						}

						break;
					#endregion

					// NPC- and/or Monster-placing mode!
					#region NPC_Mode
					case Mode.npc:
						// button for switching between placing an npc or a monster
						string placingMsg;
						if (placingMonster) { placingMsg = "Placing Mons"; }
						else { placingMsg = "Placing NPCs"; }
						if (world.GUI.Button(new Rectangle(640 - 190, 160, 140, 20), placingMsg))
						{
							if (placingMonster) { placingMonster = false; }
							else { placingMonster = true; }

							objectValid = false;
						}

						// text box for getting an npc or monster based on its name
						#region Getting the Name of the Bleedin' Thing
						string prevCharacterName;
						if (placingMonster)
						{
							prevCharacterName = monsterName;
							monsterName = world.GUI.TextField(new Rectangle(640 - 193, 480 - 260, 150, 20), monsterName);
							// only check for the object if the name has been changed
							if (prevCharacterName != monsterName)
							{
								// is this the name of a valid monster?
								if (world.CharacterHolder.MonsterParams.ContainsKey(monsterName))
								{
									objectValid = true;
									objectName = monsterName;
								}
								else
								{
									objectValid = false;
								}
							}
						}
						else
						{
							prevCharacterName = npcName;
							npcName = world.GUI.TextField(new Rectangle(640 - 193, 480 - 260, 150, 20), npcName);
							// only check for object change if the name has been changed
							if (prevCharacterName != npcName)
							{
								if (world.CharacterHolder.NPCParams.ContainsKey(npcName))
								{
									// this name is valid for an npc, therefore set the object holder to that npc object
									objectValid = true;
									objectName = npcName;
								}
								else
								{
									objectValid = false;
								}
							}
						}
						#endregion

						// if the object holder is set to something, draw a bunch of junk
						if (objectValid)
						{
							// draw its sprite for the user to see
							Texture2D characterImg;
							if (placingMonster)
							{
								characterImg = world.CharacterHolder.MakeMonster(objectName, "").Image;
							}
							else
							{
								characterImg = world.CharacterHolder.MakeNPC(objectName, "", "", false, false).Image;
							}
							spriteBatch.Draw(
								characterImg,
								new Vector2(640 - 130, 480 - 230), 
								new Rectangle(0, (int)objectDirection * 32, 32, 32), 
								Color.White);

							// draw buttons for changing its direction
							world.GUI.Box(new Rectangle(640 - 90 - 60, 480 - 185, 70, 20), objectDirection.ToString());
							int dir;
							if (world.GUI.Button(new Rectangle(640 - 120 - 60, 480 - 185, 20, 20), " < "))
							{
								// go to previous direction clockwise
								dir = (int)objectDirection - 1;
								if (dir < 0) { dir = 3; }
								objectDirection = (MovingObject.Direction)dir;
							}
							if (world.GUI.Button(new Rectangle(640 - 10 - 60, 480 - 185, 20, 20), " > "))
							{
								// go to next direction clockwise
								dir = (int)objectDirection + 1;
								if (dir > 3) { dir = 0; }
								objectDirection = (MovingObject.Direction)dir;
							}

							// if the object is an NPC, draw npc-specif things like dialogue and hostility
							if (!placingMonster)
							{
								// text box for dialogue for the npc to start
								dialogue = world.GUI.TextField(new Rectangle(640 - 120 - 60, 480 - 150, 130, 20), dialogue);

								/* hostile NPCs dummied out of the final version
								// button for setting to hostile
								string hMsg;
								if (hostile) { hMsg = "X"; } else { hMsg = ""; }
								if (world.GUI.Button(new Rectangle(640 - 180, 480 - 220, 20, 20), hMsg))
								{
									if (hostile) { hostile = false; } else { hostile = true; }
								}
								spriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], "H", new Vector2(640 - 205, 480 - 220), Color.White);
								// button for setting to ally
								string aMsg;
								if (ally) { aMsg = "X"; } else { aMsg = ""; }
								if (world.GUI.Button(new Rectangle(640 - 70, 480 - 220, 20, 20), aMsg))
								{
									if (ally) { ally = false; } else { ally = true; }
								}
								spriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], "A", new Vector2(640 - 45, 480 - 220), Color.White);
								*/
							}
						}

						break;
					#endregion

					// Miscellaneous mode
					case Mode.misc:
						// the music for the room
						spriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], "Music", new Vector2(640 - 180, 170), Color.White);
						world.Map.ActiveRoom.Music = world.GUI.TextField(new Rectangle(640 - 200, 200, 180, 20), world.Map.ActiveRoom.Music);
						if (world.AudioHolder.Songs.ContainsKey(world.Map.ActiveRoom.Music))
						{
							spriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], "Valid Music", new Vector2(640 - 200, 230), Color.White);
						}

						// placing a save point
						spriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], "Place Save Point", new Vector2(640 - 180, 280), Color.White);
						if (placingSavepoint)
						{
							if (world.GUI.Button(new Rectangle(640 - 160, 310, 20, 20), "X"))
							{
								placingSavepoint = false;
							}
						}
						else
						{
							if (world.GUI.Button(new Rectangle(640 - 160, 310, 20, 20), ""))
							{
								placingSavepoint = true;
							}
						}
						break;
				}

				// draw stuff for editing the selected object if applicable
				#region Selected_Obj
				if (selectedObj != null)
				{
					// draw different displays based on the object's type
					Type type = selectedObj.GetType();
					#region NPC
					if (type == typeof(NPC))
					{
						// the object is of NPC
						NPC npc = (NPC)selectedObj;

						// draw the file name of the object
						spriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], npc.FileName, new Vector2(25, 370), Color.White);

						// text field for the unique ID of the object
						spriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], "ID", new Vector2(140, 370), Color.White);
						npc.ID = world.GUI.TextField(new Rectangle(170, 370, 130, 20), npc.ID);

						// draw its sprite
						spriteBatch.Draw(
							npc.Image,
							new Vector2(50, 400),
							new Rectangle(0, (int)npc.direction * 32, 32, 32),
							Color.White);

						// rotation buttons
						if (world.GUI.Button(new Rectangle(25, 410, 20, 20), " < "))
						{
							int dir = (int)npc.direction - 1;
							if (dir < 0) { dir = 3; }
							npc.SetDirection((MovingObject.Direction)dir);
						}
						if (world.GUI.Button(new Rectangle(87, 410, 20, 20), " > "))
						{
							int dir = (int)npc.direction + 1;
							if (dir > 3) { dir = 0; }
							npc.SetDirection((MovingObject.Direction)dir);
						}

						/* hostile NPCs dummied out of the final version
						// buttons for setting ally and hostile
						spriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], "H", new Vector2(150, 410), Color.White);
						string hMsg;
						if (npc.Hostile) { hMsg = "X"; } else { hMsg = ""; }
						if (world.GUI.Button(new Rectangle(170, 410, 20, 20), hMsg))
						{
							if (npc.Hostile) { npc.Hostile = false; } else { npc.Hostile = true; }
						}

						spriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], "A", new Vector2(235, 410), Color.White);
						string aMsg;
						if (npc.Ally) { aMsg = "X"; } else { aMsg = ""; }
						if (world.GUI.Button(new Rectangle(210, 410, 20, 20), aMsg))
						{
							if (npc.Ally) { npc.Ally = false; } else { npc.Ally = true; }
						} */

						// text field for the dialogue of the object
						npc.Dialogue = world.GUI.TextField(new Rectangle(170, 450, 130, 20), npc.Dialogue);
					}
					#endregion

					#region Transition
					else if (type == typeof(Transition))
					{
						// the object is of Transition
						Transition trans = (Transition)selectedObj;

						// position for the player
						spriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], "Player Pos", new Vector2(35, 365), Color.White);
						// x
						if (world.GUI.Button(new Rectangle(40, 395, 20, 20), " - ")) { trans.playerPos.X--; if (trans.playerPos.X < 0) { trans.playerPos.X = 0; } }
						world.GUI.Box(new Rectangle(70, 395, 30, 20), "" + trans.playerPos.X);
						if (world.GUI.Button(new Rectangle(110, 395, 20, 20), " + ")) { trans.playerPos.X++; if (trans.playerPos.X > 19) { trans.playerPos.X = 19; } }
						// y
						if (world.GUI.Button(new Rectangle(40, 430, 20, 20), " - ")) { trans.playerPos.Y--; if (trans.playerPos.Y < 0) { trans.playerPos.Y = 0; } }
						world.GUI.Box(new Rectangle(70, 430, 30, 20), "" + trans.playerPos.Y);
						if (world.GUI.Button(new Rectangle(110, 430, 20, 20), " + ")) { trans.playerPos.Y++; if (trans.playerPos.Y > 19) { trans.playerPos.Y = 19; } }

						// position for the room
						spriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], "Room Pos", new Vector2(140, 365), Color.White);
						// x
						if (world.GUI.Button(new Rectangle(150, 395, 20, 20), " - ")) { trans.roomPos.X--; }
						world.GUI.Box(new Rectangle(180, 395, 30, 20), "" + trans.roomPos.X);
						if (world.GUI.Button(new Rectangle(220, 395, 20, 20), " + ")) { trans.roomPos.X++; }
						// y
						if (world.GUI.Button(new Rectangle(150, 430, 20, 20), " - ")) { trans.roomPos.Y--; }
						world.GUI.Box(new Rectangle(180, 430, 30, 20), "" + trans.roomPos.Y);
						if (world.GUI.Button(new Rectangle(220, 430, 20, 20), " + ")) { trans.roomPos.Y++; }

						// name for the map
						spriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], "Map Name", new Vector2(260, 365), Color.White);
						trans.mapName = world.GUI.TextField(new Rectangle(270, 395, 100, 20), trans.mapName);
					}
					#endregion

					#region DialogueTrigger
					else if (type == typeof(DialogueTrigger))
					{
						// the object is of dialogue trigger
						DialogueTrigger trigger = (DialogueTrigger)selectedObj;

						world.SpriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], "Dialouge Name", new Vector2(25, 370), Color.White);
						trigger.DialogueName = world.GUI.TextField(new Rectangle(25, 395, 140, 20), trigger.DialogueName);
					}
					#endregion

					#region Item
					else if (type == typeof(Item) || type.BaseType == typeof(Item))
					{
						// the object is of item
						Item item = (Item)selectedObj;

						world.SpriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], item.FileName, new Vector2(25, 370), Color.White);
						world.SpriteBatch.Draw(world.ImageHolder.GUI["itemslot"], new Vector2(25, 390), Color.White);
						world.SpriteBatch.Draw(item.Image, new Vector2(27, 392), Color.White);
					}
					#endregion
				}
				#endregion

				// draw coordinates of the mouse over the map
				if (
					world.InputHandler.MousePosition.X >= mapOffset.X &&
					world.InputHandler.MousePosition.X < mapOffset.X + 640 * mapScale &&
					world.InputHandler.MousePosition.Y >= mapOffset.Y &&
					world.InputHandler.MousePosition.Y < mapOffset.Y + 480 * mapScale)
				{
					

					spriteBatch.DrawString(
						world.ImageHolder.Fonts["SpriteFont1"],
						"(" + (int)((world.InputHandler.MousePosition.X - mapOffset.X) / (mapScale * 32)) + ", " + (int)((world.InputHandler.MousePosition.Y - mapOffset.Y) / (mapScale * 32)) + ")",
						new Vector2(10, 480 - 25),
						Color.White);
				}

				// draw the map thing for switching between different rooms
				DrawRoomMap(new Vector2(640 - 170, 480 - 100));
			}
        }


        // opens a new map file
		private void OpenMap()
		{
			world.Map = new Map(world, fileName);
		}

		// closes the map
		private void CloseMap()
		{
			world.Map = null;

			selectedObj = null;
		}
		
		// save the map to a given file
        private void SaveMap()
        {
            // get the proper directory of the file whether in debug mode or not
            string directory;
            if (System.Diagnostics.Debugger.IsAttached)
            {
                directory = Directory.GetCurrentDirectory() + "../../../../Content/Defines/Maps/" + fileName + ".txt";
            }
            else
            {
                directory = Directory.GetCurrentDirectory() + "/Content/Defines/Maps/" + fileName + ".txt";
            }
            // open the file
            StreamWriter stream = new StreamWriter(directory);
            
            // write name and tileset of the map to the file
            stream.WriteLine("@:" + world.Map.Name);
            stream.WriteLine("%:" + world.Map.Tileset);

			// write starting position things to the file
			stream.WriteLine("sp:" + startPos.X + ":" + startPos.Y + ":" + startRoomPos.X + ":" + startRoomPos.Y);
            
            // write the data of each room to the file line by line
            foreach (Vector2 roomPos in world.Map.Rooms.Keys)
            {
                stream.WriteLine("#:" + roomPos.X + ":" + roomPos.Y);
				stream.WriteLine("*:" + world.Map.Rooms[roomPos].Music);

                // write the data of each tile in the room to the file line by line
                foreach (Tile tile in world.Map.Rooms[roomPos].Tiles)
                {
                    stream.WriteLine("/:" + tile.Position.X / 32 + ":" + tile.Position.Y / 32 + ":" + tile.ImageCoords.X + ":" + tile.ImageCoords.Y + ":" + tile.Solid);
                }

				// write the data of each object in the room to the file
				foreach (Object obj in world.Map.Rooms[roomPos].Objects)
				{
					if (obj.GetType() == typeof(NPC))
					{
						// the object is an npc
						stream.WriteLine("n:" + ((Character)obj).FileName + ":" + ((Character)obj).ID + ":" + obj.Position.X / 32 + ":" + obj.Position.Y / 32 + ":" + (int)((NPC)obj).direction + ":" + ((NPC)obj).Dialogue + ":" + ((NPC)obj).Hostile + ":" + ((NPC)obj).Ally);
					}
					else if (obj.GetType() == typeof(Monster))
					{
						// this object is a monster
						stream.WriteLine("m:" + ((Monster)obj).FileName + ":" + ((Monster)obj).ID + ":" + obj.Position.X / 32 + ":" + obj.Position.Y / 32 + ":" + (int)((Monster)obj).direction);
					}
					else if (obj.GetType() == typeof(Transition))
					{
						// the object is a transition
						stream.WriteLine("t:" + obj.Position.X + ":" + obj.Position.Y + ":" + ((Transition)obj).playerPos.X + ":" + ((Transition)obj).playerPos.Y + ":" + ((Transition)obj).roomPos.X + ":" + ((Transition)obj).roomPos.Y + ":" + ((Transition)obj).mapName);
					}
					else if (obj.GetType() == typeof(DialogueTrigger))
					{
						// the object is a dialouge trigger
						stream.WriteLine("d:" + obj.Position.X / 32 + ":" + obj.Position.Y / 32 + ":" + ((DialogueTrigger)obj).DialogueName);
					}
					else if (obj.GetType() == typeof(Item) || obj.GetType().BaseType == typeof(Item))
					{
						// the object is an item
						stream.WriteLine("i:" + ((Item)obj).FileName + ":" + obj.Position.X / 32 + ":" + obj.Position.Y / 32);
					}
					else if (obj.GetType() == typeof(SavePoint))
					{
						// the object is a save point
						stream.WriteLine("s:" + obj.Position.X / 32 + ":" + obj.Position.Y / 32);
					}
				}
            }

            // close the file
            stream.Close();
        }


		// moves into gameplay mode in order to test the map
		private void TestMap()
		{
			SaveMap();

			world.Map.SetActiveRoom((int)startRoomPos.X, (int)startRoomPos.Y);
			world.Player.SetPosition((int)startPos.X, (int)startPos.Y);
			testing = true;

			world.AudioHandler.PlaySong(world.Map.ActiveRoom.Music);

			world.gameState = World.GameState.gamePlay;
		}


        // change the image coordinates of a tile
        private void ChangeTileCoords(Vector2 tilePosition)
        {
            Room room = world.Map.ActiveRoom;
            // go through and find the tile to reassign; if the tile does not exist yet, add it
            for (int i = 0; i <= room.Tiles.Count; i++)
            {
                // see if there are still tiles to be checked
                if (i < room.Tiles.Count)
                {
                    if (room.Tiles[i].Position == tilePosition)
                    {
                        // this is the correct tile
                        room.Tiles[i].AssignCoords(tileCoords);
                        break;
                    }
                }
                else
                {
                    room.AddTile(new Tile(
                        world.ImageHolder.Tilesets[world.Map.Tileset],
                        tilePosition,
                        tileCoords,
                        false
                        ));
                    break;
                }
            }
        }
		// change the solidity of a tile
		private void ChangeTileCollision(Vector2 tilePosition)
		{
			Room room = world.Map.ActiveRoom;
			// go through and find the tile to reassign; if the tile does not exist, then just do nothing
			foreach (Tile tile in room.Tiles)
			{
				if (tile.Position == tilePosition)
				{
					tile.ToggleSolidity();
					break;
				}
			}
		}

		// changes the tileset to the next or previous in the list of tilesets
		private void NextTileset()
		{
			// get list of all tilesets
			List<string> tilesets = new List<string>(world.ImageHolder.Tilesets.Keys);
			// go through them and assign the next incremental one
			for (int i = 0; i < tilesets.Count; i++)
			{
				if (tilesets[i] == world.Map.Tileset)
				{
					if (i + 1 < tilesets.Count)
					{
						world.Map.AssignTileset(tilesets[i + 1]);
					}
					else
					{
						world.Map.AssignTileset(tilesets[0]);
					}
					break;
				}
			}
		}
		private void PrevTileset()
		{
			// get list of all tilesets
			List<string> tilesets = new List<string>(world.ImageHolder.Tilesets.Keys);
			// go through them and assign the next incremental one
			for (int i = 0; i < tilesets.Count; i++)
			{
				if (tilesets[i] == world.Map.Tileset)
				{
					if (i - 1 >= 0)
					{
						world.Map.AssignTileset(tilesets[i - 1]);
					}
					else
					{
						world.Map.AssignTileset(tilesets[tilesets.Count - 1]);
					}
					break;
				}
			}
		}


		// draw the room mini-map thing
		private void DrawRoomMap(Vector2 position)
		{
			// values for determining how to draw the map
			int size = 3;
			Vector2 activeRoomPos = world.Map.ActiveRoomPos;

			// for all the slots in the area on the map
			for (int x = 0; x < 1 + 2 * size; x++)
			{
				for (int y = 0; y < 1 + 2 * size; y++)
				{
					// see whether this is the centre slot
					if (x == size && y == size)
					{
						// draw a highlighted slot for the centre which is active
						world.GUI.Button(
							world.ImageHolder.GUI["roomRect_active"],
							position + new Vector2(x * 15, y * 12),
							""
							);
					}
					// slots other than the centre one are clickable
					else
					{
						// check whether this room exists
						if (world.Map.Rooms.ContainsKey(new Vector2(activeRoomPos.X - size + x, activeRoomPos.Y - size + y)))
						{
							// if this slot is clicked on, do things with that room that room
							if (
								world.GUI.Button(
									world.ImageHolder.GUI["roomRect_occupied"],
									position + new Vector2(x * 15, y * 12),
									""))
							{
								// check whether delete is held
								if (!world.InputHandler.KeyHeld(Keys.Delete))
								{
									// delete is not held, so move to this room
									world.Map.SetActiveRoom((int)activeRoomPos.X - size + x, (int)activeRoomPos.Y - size + y);
								}
								else
								{
									// delete is held, so delete the room
									world.Map.Rooms.Remove(new Vector2((int)activeRoomPos.X - size + x, (int)activeRoomPos.Y - size + y));
								}
							}

						}
						else
						{
							// if this slot is clicked on, make that room
							if (
								world.GUI.Button(
									world.ImageHolder.GUI["roomRect_unoccupied"],
									position + new Vector2(x * 15, y * 12),
									""))
							{
								world.Map.MakeNewRoom((int)activeRoomPos.X - size + x, (int)activeRoomPos.Y - size + y);
							}
						}
					}
				}
			}
		}
    }
}
