// Gwendolyn Hart
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Charlie.Code
{
	// menu for the game over and end game screens
	public class EndMenu : IMenu
	{
		// reference to the world
		World world;

		// constructor
		public EndMenu(World world)
		{
			this.world = world;
		}

		// update function
		public void Update(GameTime gameTime)
		{
			// go to the main menu
			if (world.InputHandler.KeyPressed(Keys.X) || world.InputHandler.KeyPressed(Keys.Enter))
			{
				world.Map = null;

				world.Player.ClearDialogueVariables();
				world.Player.ClearTakenItems();
				world.Player.Inventory.Clear();

				world.MainMenu.UpdateSaveInfo();
				world.gameState = World.GameState.mainMenu;

				world.AudioHandler.PlaySound("Menu");
			}
		}

		// draw function
		public void Draw()
		{
			if (world.Player.Health > 0)
			{
				world.SpriteBatch.Draw(world.ImageHolder.GUI["end"], new Vector2(0, 0), Color.White);
			}
			else
			{
				world.SpriteBatch.Draw(world.ImageHolder.GUI["game_over"], new Vector2(0, 0), Color.White);
			}
		}
	}
}
