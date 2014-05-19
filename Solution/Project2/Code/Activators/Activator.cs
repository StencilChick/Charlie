// made by Gwendolyn Hart
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Charlie.Code.Activators
{
	public abstract class Activator : Object
	{
		protected World world;

		public Activator(World world, Texture2D image, int x, int y)
			: base(image, new Vector2(x, y))
		{
			this.world = world;
		}

		// what the activator does when activated
		public abstract void Activate();

		// tell whether a character is standing over this activator
		public bool IsCharacterOver(Characters.Character character)
		{
			//return character.Position.X > position.X - 5 && character.Position.X < position.X + 5 && character.Position.Y > position.Y - 5 && character.Position.Y < position.Y + 5;
			return CheckCollision(character.Position + new Vector2(16, 16));
		}
	}
}
