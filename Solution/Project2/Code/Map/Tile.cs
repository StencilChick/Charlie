// made by Gwendolyn Hart
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlie.Code;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Charlie.Code.MapCode
{
    public class Tile : Object
    {
        // true if this tile is solid and thus will be collided with
        bool solid;
        public bool Solid { get { return solid; } }

        public Tile(Texture2D image, Vector2 position, Vector2 coords, bool solid)
            : base(image, position, coords)
        {
            this.solid = solid;
        }

		/// <summary>
		/// Assigns a new image to the tile (for editor use)
		/// </summary>
		/// <param name="image">the new tileset</param>
		public void AssignImage(Texture2D image)
		{
			this.image = image;
		}
		/// <summary>
		/// Changes the tile's solidity from false to true and vice versa (for editor use)
		/// </summary>
		public void ToggleSolidity()
		{
			if (solid) { solid = false; }
			else { solid = true; }
		}
    }
}
