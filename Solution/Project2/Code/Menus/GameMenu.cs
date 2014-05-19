// Gwendolyn Hart
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Charlie.Code.Items;

namespace Charlie.Code
{
	public class GameMenu : IMenu
	{
		// reference to the world
		World world;

		// the main options of the menu
		enum Option
		{
			exit, slot1, slot2, slot3
		}
		Option option = Option.exit;

		// true if the player is currently browsing items to equip to a slot
		bool browsingItems;
		// which item is currently selected
		int selectedItem;
		// list of the items which can possibly be selected
		List<Item> items;

		// constructor
		public GameMenu(World world)
		{
			this.world = world;

			browsingItems = false;
			selectedItem = 0;
			UpdateItems();
		}

		// update function
		public void Update(GameTime gameTime)
		{
			switch (browsingItems)
			{
				// code for scrolling through items and selecting one
				case true:
					// scrolling between items
					if (world.InputHandler.KeyPressed(Keys.Down))
					{
						selectedItem++;
						if (selectedItem >= items.Count) { selectedItem = -1; }

						world.AudioHandler.PlaySound("Menu");
					}
					if (world.InputHandler.KeyPressed(Keys.Up))
					{
						selectedItem--;
						if (selectedItem < -1) { selectedItem = items.Count - 1; }

						world.AudioHandler.PlaySound("Menu");
					}

					// put the currently selected item into the current slot
					if (world.InputHandler.KeyPressed(Keys.X))
					{
						if (selectedItem >= 0 && selectedItem < items.Count)
						{
							// only add weapons and potions
							if (items[selectedItem].GetType() == typeof(Weapon) || items[selectedItem].GetType() == typeof(Potion))
							{
								world.Player.EquipedItems[(int)option - 1] = items[selectedItem];
							}
						}
						else
						{
							world.Player.EquipedItems[(int)option - 1] = null;
						}
						browsingItems = false;

						world.AudioHandler.PlaySound("Menu");
					}
					// don't put anything into the slot but stop browsing items
					else if (world.InputHandler.KeyPressed(Keys.Z) || world.InputHandler.KeyPressed(Keys.Escape))
					{
						browsingItems = false;

						world.AudioHandler.PlaySound("Menu");
					}
					break;

				// code for switching between the different item slots and exiting
				case false:
					// switching between the options
					if (world.InputHandler.KeyPressed(Keys.Right))
					{
						int opt = (int)option + 1;
						if (opt >= Enum.GetValues(typeof(Option)).Length) { opt = 0; }
						option = (Option)opt;

						world.AudioHandler.PlaySound("Menu");
					}
					if (world.InputHandler.KeyPressed(Keys.Left))
					{
						int opt = (int)option - 1;
						if (opt < 0) { opt = Enum.GetValues(typeof(Option)).Length - 1; }
						option = (Option)opt;

						world.AudioHandler.PlaySound("Menu");
					}

					// selecting an option
					if (world.InputHandler.KeyPressed(Keys.X))
					{
						// exiting the menu
						if (option == Option.exit)
						{
							world.gameState = World.GameState.gamePlay;
						}
						else
						{
							browsingItems = true;
						}

						world.AudioHandler.PlaySound("Menu");
					}
					break;
			}
		}

		// draw function
		public void Draw()
		{
			// draw the background
			world.SpriteBatch.Draw(
				world.ImageHolder.GUI["menuBackdrop"],
				new Rectangle(0, 0, 640, 480),
				Color.White);

			// draw the different options
			world.GUI.Box(new Rectangle(20, 20, 100, 20), "Return", Color.White, world.ImageHolder.GUI["dialogueBorder"], world.ImageHolder.GUI["dialogueCorner"], world.ImageHolder.GUI["dialogueBackdrop"]);
			if ((int)option == 0)
			{
				world.SpriteBatch.Draw(world.ImageHolder.GUI["editorModePointer"], new Rectangle(50, 50, 40, 4), Color.White);
			}

			// draw the different slots
			Texture2D slotImg = world.ImageHolder.GUI["itemslot_large"];
			for (int i = 0; i < 3; i++)
			{
				// the selected slot
				world.SpriteBatch.Draw(slotImg, new Rectangle((320 - 70) + 120 * i, 10, slotImg.Width, slotImg.Height), Color.White);

				// the item over the slot
				if (world.Player.EquipedItems[i] != null)
				{
					world.SpriteBatch.Draw(
						world.Player.EquipedItems[i].Image,
						new Vector2((320 - 70) + 120 * i + 4, 14),
						new Rectangle(0, 0, 32, 32),
						Color.White,
						0f,
						new Vector2(0, 0),
						2f,
						SpriteEffects.None,
						0f);
				}

				// the arrow pointing to the selected slot
				if ((int)option - 1 == i)
				{
					world.SpriteBatch.Draw(
						world.ImageHolder.GUI["editorModePointer"],
						new Rectangle((320 - 70) + 120 * i + 14, 90, 40, 4),
						Color.White);
				}
			}

			// Item Select-a-Tron 900
			Rectangle selectionBounds = new Rectangle(48, 140, 544, 160);
			string selectionMsg = "";
			string descriptionMsg = "";

			world.GUI.Box(selectionBounds, "", Color.White, world.ImageHolder.GUI["dialogueBorder"], world.ImageHolder.GUI["dialogueCorner"], world.ImageHolder.GUI["dialogueBackdrop"]);

			// come up with what to write in the boxes
			if (browsingItems)
			{
				// for the lines in the item selection box
				for (int i = 0; i < (selectionBounds.Height - 20) / 20; i++)
				{
					int index = i - (selectionBounds.Height - 20) / 40 + selectedItem;
					if (index >= 0 && index < items.Count)
					{
						// there is an item in this index
						selectionMsg += "    " + items[index].Name + " (x" + world.Player.Inventory.ItemQuantity(items[index]) + ")\n";
					}
					else
					{
						// there is no item in this index
						selectionMsg += "    --\n";
					}
				}

				// the text for the description box
				if (selectedItem >= 0 && selectedItem < items.Count)
				{
					descriptionMsg = items[selectedItem].Name + "\n" + items[selectedItem].Desc;
				}

				// arrow to show which option is currently selected
				world.SpriteBatch.Draw(world.ImageHolder.GUI["choiceArrow"], new Vector2(50, (int)(selectionBounds.Y + selectionBounds.Height / 2 - 7)), Color.White);
			}

			// selection box for the items
			world.SpriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], selectionMsg, new Vector2(selectionBounds.X, selectionBounds.Y), Color.White);

			// description box for the items
			world.SpriteBatch.Draw(world.ImageHolder.GUI["dialogueBox"], new Vector2(45, 350), Color.White);
			world.GUI.Text(
				world.ImageHolder.Fonts["SpriteFont1"],
				descriptionMsg,
				new Rectangle(50, 355, 544, 100),
				Color.White);
		}


		// update the list of items
		public void UpdateItems()
		{
			items = world.Player.Inventory.AllItems;
		}
	}
}
