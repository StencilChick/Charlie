// made by Gwendolyn Hart
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using Charlie.Code.Characters;
using Charlie.Code.Activators;
using Charlie.Code.Items;

namespace Charlie.Code
{
	public class Player : Character
	{
		// whether to draw the UI
		bool uiActive;
		public bool UIActive { get { return uiActive; } set { uiActive = value; } }

		// the save slot this player occupies
		int saveSlot;
		public int SaveSlot
		{
			get { return saveSlot; }
			set { saveSlot = value; }
		}

		// the inventory
		Inventory inventory;
		public Inventory Inventory { get { return inventory; } }
		// equipped items
		Item[] equipedItems;
		public Item[] EquipedItems
		{
			get { return equipedItems; }
			set { equipedItems = value; }
		}

		// variables read and set by the dialogue
		Dictionary<string, string> dialogueVariables;
		// a list of all the items that have already been taken by the player so that the map knows not to rebuild them
		List<string> takenItems;

		// the constructor
		public Player(World world)
			: base(world, "", "", null, (int)(2.75 * 32), 3, 4)
		{
			// item stuff
			inventory = new Inventory(world);
			equipedItems = new Item[3];
			
			// have this stuff be loaded from a save file eventually
			dialogueVariables = new Dictionary<string, string>();
			takenItems = new List<string>();

			uiActive = true;
		}
		
		// thing that let's the player do things
		public void Update(GameTime gameTime)
		{
			// update the base
			base.Update(gameTime);

			// only do things if there is no dialogue stuff open right now
			if (world.Dialogue == null)
			{
				#region controls
				if (stunTime <= 0)
				{
					// movement, by Jove!
					if (!attacking)
					{
						if (world.InputHandler.KeyHeld(Keys.Right))
						{
							MoveRight(gameTime);

							SetDirection(MovingObject.Direction.right);
							AnimateWalk();

							TransitionRight();
						}
						if (world.InputHandler.KeyHeld(Keys.Left))
						{
							MoveLeft(gameTime);

							SetDirection(MovingObject.Direction.left);
							AnimateWalk();

							TransitionLeft();
						}
						if (world.InputHandler.KeyHeld(Keys.Up))
						{
							MoveUp(gameTime);

							SetDirection(MovingObject.Direction.up);
							AnimateWalk();

							TransitionUp();
						}
						if (world.InputHandler.KeyHeld(Keys.Down))
						{
							MoveDown(gameTime);

							SetDirection(MovingObject.Direction.down);
							AnimateWalk();

							TransitionDown();
						}
					}

					// items, by Jove!
					if (world.InputHandler.KeyPressed(Keys.A))
					{
						UseItem(equipedItems[0]);
					}
					else if (world.InputHandler.KeyPressed(Keys.S))
					{
						UseItem(equipedItems[1]);
					}
					else if (world.InputHandler.KeyPressed(Keys.D))
					{
						UseItem(equipedItems[2]);
					}

					// stop animation if the player is just standing still
					if (!attacking && !world.InputHandler.KeyHeld(Keys.Up) && !world.InputHandler.KeyHeld(Keys.Down) && !world.InputHandler.KeyHeld(Keys.Left) && !world.InputHandler.KeyHeld(Keys.Right)) { walking = false; }
					if (!walking)
					{
						StopAnimation();
					}

					// examine a thing that you are looking at
					if (world.InputHandler.KeyPressed(Keys.X))
					{
						// figure out which position the player is examining
						Vector2[] dirList = new Vector2[4] {
						new Vector2(0, 1),
						new Vector2(-1, 0),
						new Vector2(0, -1),
						new Vector2(1, 0)
					};
						Vector2 exPoint = position + new Vector2(16, 16) + 32 * dirList[(int)direction];

						// figure out if an object or npc is in that position and interact with it
						foreach (Object obj in world.Map.ActiveRoom.Objects)
						{
							if (obj.CheckCollision(new Vector2((int)exPoint.X, (int)exPoint.Y)))
							{
								Interact(obj);
								break;
							}
						}
					}
				}
				else
				{
					// transition if the player gets knocked out of the room by the enemy
					TransitionDown();
					TransitionUp();
					TransitionRight();
					TransitionLeft();
				}
				#endregion

				// check if stepped into an activator
				foreach (Object obj in world.Map.ActiveRoom.Objects)
				{
					// if object is an activator which the player is over, activate it
					if (obj.GetType().BaseType == typeof(Activators.Activator))
					{
						Activators.Activator act = (Activators.Activator)obj;
						if (act.IsCharacterOver(this))
						{
							act.Activate();
						}
					}
				}
			}
		}

		// thing what draws all the GUI elements related to the player
		public void DrawUI(SpriteBatch spriteBatch)
		{
			if (uiActive)
			{
				// draw the player's hearts to the screen
				Texture2D heartImage = world.ImageHolder.GUI["heart"];
				for (int i = 0; i < maxHealth; i++)
				{
					if ((int)health >= i + 1)
					{
						spriteBatch.Draw(
							heartImage,
							new Vector2(5 * (i + 1) + heartImage.Width / 2 * i, 5),
							new Rectangle(0, 0, heartImage.Width / 2, heartImage.Height),
							Color.White);
					}
					else
					{
						spriteBatch.Draw(
							heartImage,
							new Vector2(5 * (i + 1) + heartImage.Width / 2 * i, 5),
							new Rectangle(heartImage.Width / 2, 0, heartImage.Width / 2, heartImage.Height),
							Color.White);
					}
				}

				// draw the player's mana to the screen
				Texture2D manaImage = world.ImageHolder.GUI["mana"];
				for (int i = 0; i < maxMana; i++)
				{
					if ((int)mana >= i + 1)
					{
						spriteBatch.Draw(
							manaImage,
							new Vector2(5 * (i + 1) + manaImage.Width / 2 * i, 10 + heartImage.Height),
							new Rectangle(0, 0, manaImage.Width / 2, manaImage.Height),
							Color.White);
					}
					else
					{
						spriteBatch.Draw(
							manaImage,
							new Vector2(5 * (i + 1) + manaImage.Width / 2 * i, 10 + heartImage.Height),
							new Rectangle(manaImage.Width / 2, 0, manaImage.Width / 2, manaImage.Height),
							Color.White);
					}
				}


				// draw item junk
				Texture2D itemSlot = world.ImageHolder.GUI["itemslot"];
				for (int i = 0; i < equipedItems.Length; i++)
				{
					// draw the slot for the item
					spriteBatch.Draw(itemSlot, new Rectangle((int)(320 + (itemSlot.Width + 5) * 1.5) - (itemSlot.Width + 5) * (3 - i), 5, itemSlot.Width, itemSlot.Height), Color.White);
					// draw the item
					if (equipedItems[i] != null)
					{
						spriteBatch.Draw(equipedItems[i].Image, new Vector2((int)(322 + (itemSlot.Width + 5) * 1.5) - (itemSlot.Width + 5) * (3 - i), 7), Color.White);

						// if it's a potion, tell how many of it the player has
						if (equipedItems[i].GetType() == typeof(Potion))
						{
							world.GUI.Label(world.ImageHolder.Fonts["SpriteFont1"], "" + inventory.ItemQuantity(equipedItems[i]), new Rectangle((int)(322 + (itemSlot.Width + 5) * 1.5) - (itemSlot.Width + 5) * (3 - i), 18, 32, 32), Color.White);
						}
					}
				}


				// draw the name of the map
				world.Map.DrawMapName();
			}
		}


		// called during the LoadContent function to set the player's character sheet
		public void SetImage()
		{
			image = world.ImageHolder.CharacterSheets["hero"];
		}


		// function for using an item
		private void UseItem(Item item)
		{
			if (item != null)
			{
				// use a weapon
				if (item.GetType() == typeof(Weapon))
				{
					if (!attacking)
					{
						// set up the attack animation
						animIndex = 0;
						attackAnim = (AttackAnim)((Weapon)item).weaponAnim;
						attacking = true;

						// damage anything hit by the weapon
						Attack((Weapon)item);

						// tell the weapon to fire whatever projectiles it has
						((Weapon)item).FireProjectiles(position, direction);
					}
				}
				// use an item
				else if (item.GetType() == typeof(Potion))
				{
					((Potion)item).Use();
					inventory.RemoveItem(item, 1);

					// if this item has run out, remove it from the equipped items list
					if (inventory.ItemQuantity(item) == 0)
					{
						for (int i = 0; i < equipedItems.Length; i++)
						{
							if (equipedItems[i] == item)
							{
								equipedItems[i] = null;
							}
						}
					}
				}
			}
		}
		
		// function for interacting with an object
		private void Interact(Object obj)
		{
			// determine how to interact with an object based on its type
			if (obj.GetType() == typeof(SavePoint))
			{
				// is a save point, so activate it
				walking = false;
				((SavePoint)obj).Activate();
			}
			else if (obj.GetType() == typeof(NPC))
			{
				// is an NPC, so start a dialogue with it
				NPC npc = (NPC)obj;
				if (!npc.Hostile)
				{
					// stop the player
					StopAnimation();

					// open dialogue
					npc.OpenDialogue();
				}
			}
			else if (obj.GetType().BaseType == typeof(Charlie.Code.Activators.Activator))
			{
				// active an activator object
				((Charlie.Code.Activators.Activator)obj).Activate();
			}
			else if (obj.GetType() == typeof(Item) || obj.GetType().BaseType == typeof(Item))
			{
				// add the item to the inventory and remove it from the map
				inventory.AddItem((Item)obj, 1);
				AddToTakenItems(obj.Position);
				world.Map.ActiveRoom.RemoveObject(obj);

				// play sound effect
				world.AudioHandler.PlaySound("Item");
			}
		}

		// function for attacking stuff
		private void Attack(Weapon weapon)
		{
			// make sure the weapon itself can actually damage anything
			if (weapon.Damage > 0 && weapon.Range > 0)
			{
				// collision rectangle
				Rectangle rect = new Rectangle();
				switch (direction)
				{
					case Direction.up:
						rect = new Rectangle((int)position.X, (int)position.Y - weapon.Range, 32, weapon.Range);
						break;

					case Direction.right:
						rect = new Rectangle((int)position.X + 32, (int)position.Y, weapon.Range, 32);
						break;

					case Direction.down:
						rect = new Rectangle((int)position.X, (int)position.Y + 32, 32, weapon.Range);
						break;

					case Direction.left:
						rect = new Rectangle((int)position.X - weapon.Range, (int)position.Y, weapon.Range, 32);
						break;
				}

				// check the collision rect against objects in the room
				foreach (Object obj in world.Map.ActiveRoom.Objects)
				{
					if (obj.CheckCollision(rect))
					{
						if (obj.GetType() == typeof(Monster))
						{
							((Monster)obj).Damage(weapon.Damage, direction + 2);
						}
						else if (obj.GetType() == typeof(NPC))
						{
							NPC npc = (NPC)obj;
							if (npc.Hostile)
							{
								npc.Damage(weapon.Damage, direction + 2);
							}
						}
					}
				}
			}
		}


		// functions for moving betwix different rooms
		private void TransitionRight()
		{
			if (position.X >= 640)
			{
				position.X = 0;
				world.Map.SetActiveRoom((int)world.Map.ActiveRoomPos.X + 1, (int)world.Map.ActiveRoomPos.Y);
				world.AudioHandler.PlaySong(world.Map.ActiveRoom.Music);

				if (Collide() != null) { PositionFix(); }
			}
		}
		private void TransitionLeft()
		{
			if (position.X <= -32)
			{
				position.X = 640 - 32;
				world.Map.SetActiveRoom((int)world.Map.ActiveRoomPos.X - 1, (int)world.Map.ActiveRoomPos.Y);
				world.AudioHandler.PlaySong(world.Map.ActiveRoom.Music);

				if (Collide() != null) { PositionFix(); }
			}
		}
		private void TransitionUp()
		{
			if (position.Y <= -32)
			{
				position.Y = 480 - 32;
				world.Map.SetActiveRoom((int)world.Map.ActiveRoomPos.X, (int)world.Map.ActiveRoomPos.Y - 1);
				world.AudioHandler.PlaySong(world.Map.ActiveRoom.Music);

				if (Collide() != null) { PositionFix(); }
			}
		}
		private void TransitionDown()
		{
			if (position.Y >= 480)
			{
				position.Y = 0;
				world.Map.SetActiveRoom((int)world.Map.ActiveRoomPos.X, (int)world.Map.ActiveRoomPos.Y + 1);
				world.AudioHandler.PlaySong(world.Map.ActiveRoom.Music);

				if (Collide() != null) { PositionFix(); }
			}
		}

		// fits the player to the closest tile
		private void PositionFix()
		{
			position = new Vector2((float)Math.Round(position.X / 32) * 32, (float)Math.Round(position.Y / 32) * 32);
		}

		// functions for setting and getting dialogue variables
		public void SetDialogueVariable(string var, string value)
		{
			if (dialogueVariables.ContainsKey(var))
			{
				dialogueVariables[var] = value;
			}
			else
			{
				dialogueVariables.Add(var, value);
			}
		}
		public string GetDialogueVariable(string var)
		{
			if (dialogueVariables.ContainsKey(var))
			{
				return dialogueVariables[var];
			}
			else
			{
				return null;
			}
		}

		// clears all the dialogue variables
		public void ClearDialogueVariables()
		{
			dialogueVariables = new Dictionary<string, string>();
		}
		// add a new item to the list of taken items
		private void AddToTakenItems(Vector2 itemPos) 
		{
			takenItems.Add(world.Map.Name + "_" + world.Map.ActiveRoomPos.X + "_" + world.Map.ActiveRoomPos.Y + "_" + itemPos.X + "_" + itemPos.Y);
		}
		// returns true if an item has already been taken
		public bool IsItemTaken(string mapName, Vector2 roomPos, Vector2 itemPos)
		{
			return takenItems.Contains(mapName + "_" + roomPos.X + "_" + roomPos.Y + "_" + itemPos.X + "_" + itemPos.Y);
		}
		// clears the taken items
		public void ClearTakenItems()
		{
			takenItems = new List<string>();
			inventory.Clear();
		}


		// function for saving the player data to a file based on the save slot
		public void Save()
		{
			// get the stream
			StreamWriter stream = new StreamWriter(File.Open(Directory.GetCurrentDirectory() + "/Saves/save" + saveSlot + ".txt", FileMode.Create));

			// write the thing to display on the save slot
			stream.WriteLine(world.Map.Name + ":" + maxHealth + ":" + maxMana);

			// go through all the values of the player that must be saved and write them to the file
			// the player's maximum health and mana
			stream.WriteLine("health:" + maxHealth);
			stream.WriteLine("mana:" + maxMana);

			// save all the items in the inventory
			foreach (Item item in inventory.AllItems)
			{
				stream.WriteLine("item:" + item.FileName + ":" + inventory.ItemQuantity(item));
			}

			// save all the dialogue variables
			foreach (string var in dialogueVariables.Keys)
			{
				stream.WriteLine("dialogue:" + var + ":" + dialogueVariables[var]);
			}

			// save all the taken item values
			foreach (string var in takenItems)
			{
				stream.WriteLine("tItem:" + var);
			}

			// save all the equipped items
			for (int i = 0; i < equipedItems.Length; i++)
			{
				if (equipedItems[i] != null)
				{
					stream.WriteLine("eItem" + i + ":" + equipedItems[i].FileName);
				}
				else
				{
					stream.WriteLine("eItem" + i + ":");
				}
			}

			// the map and where the player is on it
			stream.WriteLine("map:" + world.Map.FileName);
			stream.WriteLine("room:" + world.Map.ActiveRoomPos.X + ":" + world.Map.ActiveRoomPos.Y);
			stream.WriteLine("pos:" + position.X + ":" + position.Y);
			stream.WriteLine("dir:" + (int)direction);

			// close the stream
			stream.Close();
		}

		// function for loading the player data from a save file based on the save slot
		public void Load()
		{
			// open the stream
			StreamReader stream = new StreamReader(Directory.GetCurrentDirectory() + "/Saves/save" + saveSlot + ".txt");

			// go through each line and assign values as appropriate
			while (true)
			{
				// get the next line
				string line = stream.ReadLine();
				if (line == null) { break; }

				// parse the data in the line 
				string[] parameters = line.Split(':');
				switch (parameters[0])
				{
					// set the map
					case "map":
						world.Map = new MapCode.Map(world, parameters[1]);
						break;

					// set the active room
					case "room":
						world.Map.SetActiveRoom(int.Parse(parameters[1]), int.Parse(parameters[2]));
						break;

					// set the position of the player
					case "pos":
						position = new Vector2(float.Parse(parameters[1]), float.Parse(parameters[2]));
						break;

					// set the direction
					case "dir":
						SetDirection((Direction)int.Parse(parameters[1].Trim()));
						break;

					// set maximum health
					case "health":
						maxHealth = int.Parse(parameters[1].Trim());
						health = maxHealth;
						break;

					// set maximum mana
					case "mana":
						maxMana = int.Parse(parameters[1].Trim());
						mana = maxMana;
						break;

					// add an item to the inventory
					case "item":
						inventory.AddItem(world.ItemHolder.MakeItem(parameters[1]), int.Parse(parameters[2].Trim()));
						break;

					// set a dialogue variable
					case "dialogue":
						dialogueVariables.Add(parameters[1], parameters[2].Trim());
						break;

					// add a value to the list of taken item
					case "tItem":
						takenItems.Add(parameters[1].Trim());
						break;

					// else
					default:
						// see if it's a command to set the equipped items
						if (parameters[0].Length >= 5)
						{
							if (parameters[0].Substring(0, 5) == "eItem")
							{
								int i = int.Parse("" + parameters[0][5]);

								// see if the slot is set to anything
								if (parameters[1] != null)
								{
									// put this item in the slot
									equipedItems[i] = world.ItemHolder.MakeItem(parameters[1].Trim());
								}
								else
								{
									// is not set to an item, so this slot happily gets to be null
									equipedItems[i] = null;
								}
							}
						}

						break;
				}
			}

			// close the stream
			stream.Close();
		}
	}
}
