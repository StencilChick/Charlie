using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Charlie.Code
{
	public interface IMenu
	{
		// update method
		void Update(GameTime gameTime);

		// draw method
		void Draw();
	}
}
