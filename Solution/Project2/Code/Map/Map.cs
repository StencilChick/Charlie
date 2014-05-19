// made by Gwendolyn Hart
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Charlie.Code.Characters;
using Charlie.Code.Activators;
using Charlie.Code.Items;

namespace Charlie.Code.MapCode
{
    public class Map
    {
        // reference to the world object
        World world;

		// the file name of the map
		string filename;
		public string FileName { get { return filename; } }

        // the name of the map
        string name;
        public string Name 
		{ 
			get { return name; }
			set { name = value; }
		}

        // the tileset of this map
        string tileset;
        public string Tileset { get { return tileset; } }

        // the rooms in the map
        Dictionary<Vector2, Room> rooms;
        public Dictionary<Vector2, Room> Rooms { get { return rooms; } }
        Vector2 activeRoomPos; // coordinates of the currently active room
		public Vector2 ActiveRoomPos { get { return activeRoomPos; } }

		// the currently active room
		public Room ActiveRoom
		{
			get { return rooms[activeRoomPos]; }
		}

		// timer for displaying the name of the map
		float nameDispTimer;


        // alright, let's do this thing!
        public Map(World world, string fileName)
        {
            this.world = world;
			this.filename = fileName;
            rooms = new Dictionary<Vector2, Room>();

            // Leeeeerrooooy Jeeeenkiiins
			if (!LoadFile(fileName))
			{
				// if the file could not be loaded, build the map with a single empty room
				name = "New Map";
				tileset = world.ImageHolder.Tilesets.Keys.ElementAt<string>(0);
				rooms = new Dictionary<Vector2, Room>() {
					{ new Vector2(0, 0), new Room() }
				};
			}

            // set the active room to something
            activeRoomPos = new List<Vector2>(rooms.Keys)[0];

			// set the timer to a value
			nameDispTimer = 4.0f;
        }
		// do things like that thing above this thing
		public Map(World world, string fileName, Vector2 roomPos)
		{
			this.world = world;
			this.filename = fileName;
			rooms = new Dictionary<Vector2, Room>();

			LoadFile(fileName);

			if (rooms.ContainsKey(roomPos))
			{
				activeRoomPos = roomPos;
			}
			else
			{
				activeRoomPos = rooms.Keys.ElementAt<Vector2>(0);
			}

			nameDispTimer = 4.0f;
		}

        // takes the name of a file to be read which defines the map
        private bool LoadFile(string fileName)
        {
            // current room being added to
            Room room = null;
            Vector2 roomPos = new Vector2(0, 0); // the position of the room on the map

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

            // attempt to load the file
			StreamReader stream;
			try
			{
				// get the stream of the file
				stream = new StreamReader(directory);
			}
			catch
			{
				// return false if the file cannot be loaded
				return false;
			}

            // go through all the lines and construct the map line by line
            while (true)
            {
                // the line in a string
                string line = stream.ReadLine();

                // if the line is null, the end of the file has been reached
                if (line == null) 
                { 
                    // if there is room constructed, add it to the map
					if (room != null) { rooms.Add(roomPos, room); room.CleanObjects(); }

                    // break the loop
                    break; 
                }
                // otherwise, parse the line
                else
                {
                    // seperate the line into the parameters for its instruction
                    string[] parameters = line.Split(':');

                    // figure out what to do
                    if (parameters[0] == "@")
                    {
                        // set the name of the map
                        name = parameters[1];
                    }
                    else if (parameters[0] == "%")
                    {
                        // set the tileset of the map
                        tileset = parameters[1];
                    }
					else if (parameters[0] == "sp")
					{
						if (world.gameState == World.GameState.editor)
						{
							world.Editor.StartPos = new Vector2(int.Parse(parameters[1]), int.Parse(parameters[2]));
							world.Editor.StartRoomPos = new Vector2(int.Parse(parameters[3]), int.Parse(parameters[4]));
						}
					}
					else if (parameters[0] == "#")
					{
						// begin the construction of a new room

						// if this is not the first room being constructed
						if (room != null)
						{
							// add the current room to the map before beginning to construct the new one
							rooms.Add(roomPos, room);
							room.CleanObjects();
						}

						// make the new room
						room = new Room();
						// get the position of the new room
						roomPos = new Vector2(int.Parse(parameters[1]), int.Parse(parameters[2]));
					}
					else if (parameters[0] == "*")
					{
						// set the music to a value
						room.Music = parameters[1];
					}
					else if (parameters[0] == "/")
					{
						// add a tile to the current room
						room.AddTile(new Tile(
							world.ImageHolder.Tilesets[tileset],
							new Vector2(int.Parse(parameters[1]), int.Parse(parameters[2])) * 32,
							new Vector2(int.Parse(parameters[3]), int.Parse(parameters[4])),
							parameters[5] != "False"
							));
					}
					else if (parameters[0] == "n")
					{
						// add an npc to the room
						NPC npc = world.CharacterHolder.MakeNPC(parameters[1], parameters[2], parameters[6], parameters[7] != "False", parameters[8] != "False");
						npc.SetPosition(int.Parse(parameters[3]) * 32, int.Parse(parameters[4]) * 32);
						npc.SetDirection((Character.Direction)int.Parse(parameters[5]));
						room.AddObject(npc);
					}
					else if (parameters[0] == "m")
					{
						// add a monster to the room
						Monster mon = world.CharacterHolder.MakeMonster(parameters[1], parameters[2]);
						mon.SetPosition(int.Parse(parameters[3]) * 32, int.Parse(parameters[4]) * 32);
						mon.SetDirection((Character.Direction)int.Parse(parameters[5]));
						room.AddObject(mon);
					}
					else if (parameters[0] == "t")
					{
						// add a transition to the room
						room.AddObject(new Transition(
							world,
							new Vector2(int.Parse(parameters[1]), int.Parse(parameters[2])),
							new Vector2(int.Parse(parameters[3]), int.Parse(parameters[4])),
							new Vector2(int.Parse(parameters[5]), int.Parse(parameters[6])),
							parameters[7]));
					}
					else if (parameters[0] == "d")
					{
						// add a dialogue trigger to the room
						room.AddObject(new DialogueTrigger(
							world,
							new Vector2(int.Parse(parameters[1]), int.Parse(parameters[2])) * 32,
							parameters[3]));
					}
					else if (parameters[0] == "i")
					{
						// check to see that this item has not yet been taken by the player
						if (!world.Player.IsItemTaken(name, roomPos, new Vector2(int.Parse(parameters[2]), int.Parse(parameters[3])) * 32))
						{
							// add the item to the room
							Item item = world.ItemHolder.MakeItem(parameters[1]);
							item.SetPosition(int.Parse(parameters[2]) * 32, int.Parse(parameters[3]) * 32);
							room.AddObject(item);
						}
					}
					else if (parameters[0] == "s")
					{
						// add a save point
						SavePoint savepoint = new SavePoint(world);
						savepoint.SetPosition(int.Parse(parameters[1]) * 32, int.Parse(parameters[2]) * 32);
						room.AddObject(savepoint);
					}
                }
            }

            // close the stream
            stream.Close();
			// return true, signifying success
			return true;
        }

		// you should know what this does by now
		public void Update(GameTime gameTime)
		{
			// update the name timer
			if (nameDispTimer > -1) { nameDispTimer -= (float)(gameTime.ElapsedGameTime.Milliseconds * 1.0 / 1000); }

			// clean out disposed objects from the last frame
			ActiveRoom.CleanObjects();

			// go through all the things and make them do things
			foreach (Object obj in ActiveRoom.Objects)
			{
				// update npcs
				if (obj.GetType() == typeof(NPC))
				{
					((NPC)obj).Update(gameTime);
				}
				// update monsters
				else if (obj.GetType() == typeof(Monster))
				{
					((Monster)obj).Update(gameTime);
				}
				// update projectiles
				else if (obj.GetType() == typeof(Projectile))
				{
					((Projectile)obj).Update(gameTime);
				}
			}
		}

        // draws the current room to the screen
        public void Draw(SpriteBatch spriteBatch)
        {
            // draw each tile
            foreach (Tile t in rooms[activeRoomPos].Tiles)
            {
                t.Draw(spriteBatch);
            }

			// get the list of the objects ordered into proper draw order
			List<Object> objects = ActiveRoom.OrderedObjects(world);
			// draw all the objects
			foreach (Object obj in objects)
			{
				if (obj.GetType() == typeof(Item) || obj.GetType().BaseType == typeof(Item))
				{
					// draw a chest in place of an item
					spriteBatch.Draw(world.ImageHolder.Items["chest"], obj.Position, Color.White);
				}
				else
				{
					obj.Draw(spriteBatch);
				}
			}

        }

        // draws the current room to the screen, but smaller for the editor
        public void EditorDraw(SpriteBatch spriteBatch, float scale, Vector2 offset)
		{
			#region Tiles
			// draw each tile
            foreach (Tile t in rooms[activeRoomPos].Tiles)
            {
				// draw the tile
                t.Draw(
                    spriteBatch,
                    scale,
                    offset
                    );

				// draw the collision box over the tile if it is solid
				if (t.Solid && world.Editor.PlacingColliders)
				{
					spriteBatch.Draw(
						world.ImageHolder.GUI["editor"],
						new Rectangle(
							(int)(t.Position.X * scale + offset.X),
							(int)(t.Position.Y * scale + offset.Y),
							(int)(32 * scale),
							(int)(32 * scale)
							),
						new Rectangle(32, 0, 32, 32),
						Color.White
						);
				}

				// draw the grid over the tile if applicable
				if (world.Editor.DrawGrid)
				{
					spriteBatch.Draw(
						world.ImageHolder.GUI["editor"],
						new Rectangle(
							(int)(t.Position.X * scale + offset.X),
							(int)(t.Position.Y * scale + offset.Y),
							(int)(32 * scale),
							(int)(32 * scale)
							),
						new Rectangle(0, 0, 32, 32),
						Color.White
						);
				}

				// draw the start box to mark this tile if it is where the player will start during testing
				if (world.Map.activeRoomPos == world.Editor.StartRoomPos)
				{
					if (t.Position == world.Editor.StartPos)
					{
						spriteBatch.Draw(
							world.ImageHolder.GUI["editor"],
							new Rectangle(
								(int)(t.Position.X * scale + offset.X),
								(int)(t.Position.Y * scale + offset.Y),
								(int)(32 * scale),
								(int)(32 * scale)
								),
							new Rectangle(64, 0, 32, 32),
							Color.White
							);
					}
				}
			}
			#endregion

			// get all the objects in the room ordered by draw order
			List<Object> objects = ActiveRoom.OrderedObjectsWithoutPlayer();
			// draw each object in the room
			foreach (Object obj in objects)
			{
				// statement for objects that don't have visible sprites to be drawn
				if (obj.GetType() == typeof(Transition))
				{
					spriteBatch.Draw(
						world.ImageHolder.GUI["editor"],
						new Rectangle(
							(int)(obj.Position.X * scale + offset.X),
							(int)(obj.Position.Y * scale + offset.Y),
							(int)(32 * scale),
							(int)(32 * scale)),
						new Rectangle(96, 0, 32, 32),
						Color.White);
				}
				else if (obj.GetType() == typeof(DialogueTrigger))
				{
					spriteBatch.Draw(
						world.ImageHolder.GUI["editor"],
						new Rectangle(
							(int)(obj.Position.X * scale + offset.X),
							(int)(obj.Position.Y * scale + offset.Y),
							(int)(32 * scale),
							(int)(32 * scale)),
						new Rectangle(128, 0, 32, 32),
						Color.White);
				}
				else
				{
					obj.Draw(spriteBatch, scale, offset);
				}
			}
        }

		/// <summary>
		/// Sets the room with the given coordinates to the active room
		/// </summary>
		/// <param name="x">the x position of the room</param>
		/// <param name="y">the y position of the room</param>
		public void SetActiveRoom(int x, int y)
		{
			Vector2 v = new Vector2(x, y);
			if (rooms.ContainsKey(v))
			{
				activeRoomPos = v;
			}
		}


		/// <summary>
		/// Adds a new room to the map at the given coordinates
		/// </summary>
		/// <param name="x">the x position of the room</param>
		/// <param name="y">the y position of the room</param>
		public void MakeNewRoom(int x, int y)
		{
			// make sure the room doesn't already exist
			if (!rooms.ContainsKey(new Vector2(x, y)))
			{
				rooms.Add(new Vector2(x, y), new Room());
			}

			// go to the newly created room
			SetActiveRoom(x, y);
		}


		/// <summary>
		/// Assigns a new tileset to this map (for use in the level editor)
		/// </summary>
		/// <param name="tileset"> the name of the tileset</param>
		public void AssignTileset(string tileset)
		{
			this.tileset = tileset;
			foreach (Room room in rooms.Values)
			{
				foreach (Tile tile in room.Tiles)
				{
					tile.AssignImage(world.ImageHolder.Tilesets[tileset]);
				}
			}
		}


		public void DrawMapName()
		{
			if (nameDispTimer > -2)
			{
				int nameLength = (int)world.ImageHolder.Fonts["SpriteFont1"].MeasureString(name).X;

				int offset;
				if (nameDispTimer < 0)
				{
					offset = (int)(Math.Abs(nameDispTimer) * (nameLength + 20));
				}
				else if (nameDispTimer > 3)
				{
					offset = (int)((nameDispTimer - 3) * (nameLength + 20));
				}
				else
				{
					offset = 0;
				}

				world.GUI.Box(
					new Rectangle(640 - nameLength - 15 + offset, 10, nameLength, 20), 
					name, 
					Color.White,
					world.ImageHolder.GUI["dialogueBorder"],
					world.ImageHolder.GUI["dialogueCorner"],
					world.ImageHolder.GUI["dialogueBackdrop"]);
			}
		}
    }
}
