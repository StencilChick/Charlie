using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Charlie.Code.Items
{
	public class Potion : Item
	{
		// reference to the world
		World world;

		// which category of potion the potion is
		public enum Category
		{
			health, mana, healthincrease, manaincrease
		}
		Category category;

		// the value of the potion's potency
		int value;

		// constructor
		public Potion(World world, string fileName, string name, Texture2D image, string desc, Category category, int value)
			: base(fileName, name, image, desc)
		{
			this.world = world;

			this.category = category;
			this.value = value;
		}

		// what this potion does when used
		public void Use()
		{
			switch (category)
			{
				// healing potion
				case Category.health:
					world.Player.Health += value;
					break;

				// mana restoring potion
				case Category.mana:
					world.Player.Mana += value;
					break;

				// health incresing
				case Category.healthincrease:
					world.Player.MaxHealth += value;
					world.Player.Health += value;
					break;

				// mana increase
				case Category.manaincrease:
					world.Player.MaxMana += value;
					world.Player.Mana += value;
					break;
			}
		}
	}
}
