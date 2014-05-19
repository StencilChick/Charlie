using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Charlie.Code
{
	public class SavePoint : Object
	{
		// reference to the world
		World world;

		// constructor
		public SavePoint(World world)
			: base(world.ImageHolder.Tilesets["_savepoint"])
		{
			this.world = world;
		}

		// function to activate the save point
		public void Activate()
		{
			// heal the player
			world.Player.Health = world.Player.MaxHealth;
			world.Player.Mana = world.Player.MaxMana;

			// save the game if not in editor mode
			if (!world.Editor.Testing)
			{
				world.Player.Save();
				world.Dialogue = new Dialogue(world, "_save");
			}
			else
			{
				world.Dialogue = new Dialogue(world, "_nosave");
			}
		}
	}
}
