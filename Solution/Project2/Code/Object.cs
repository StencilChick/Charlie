// made by Gwendolyn Hart
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Charlie.Code
{
    // something that sits there and takes up space
    public abstract class Object
    {
        // the position of the object on the screen
        protected Vector2 position;
        public Vector2 Position { get { return position; } }

        // image data for the object
        protected Texture2D image; // the image
        public Texture2D Image { get { return image; } }
        protected Vector2 imageCoords; // which 32x32 block of the image to draw
        public Vector2 ImageCoords { get { return imageCoords; } }

        // constructor taking only the image
        public Object(Texture2D image)
        {
            this.image = image;

            position = new Vector2(0, 0);
            imageCoords = new Vector2(0, 0);
        }
        // constructor taking the image and position
        public Object(Texture2D image, Vector2 position)
        {
            this.image = image;
            this.position = position;

            imageCoords = new Vector2(0, 0);
        }
        public Object(Texture2D image, float x, float y)
        {
            this.image = image;
            this.position = new Vector2(x, y);

            imageCoords = new Vector2(0, 0);
        }
        // constructor taking image, position, and image coordinates
        public Object(Texture2D image, Vector2 position, Vector2 imageCoords)
        {
            this.image = image;
            this.position = position;
            this.imageCoords = imageCoords;
        }

        // draws the object onto a sprite batch
        public void Draw(SpriteBatch spriteBatch)
        {
            // oh yeah, batch that sprite!
			if (image != null)
			{
				spriteBatch.Draw(
					image,
					new Vector2((int)position.X, (int)position.Y),
					new Rectangle((int)imageCoords.X * 32, (int)imageCoords.Y * 32, 32, 32),
					Color.White
					);
			}
        }
        // draws the object scaled down and moved a certain distance from its global position
        // (really only used to for displaying things on the map in the editor
        public void Draw(SpriteBatch spriteBatch, float scale, Vector2 offset)
        {
			if (image != null)
			{
				spriteBatch.Draw(
					image,
					position * scale + offset,
					new Rectangle((int)imageCoords.X * 32, (int)imageCoords.Y * 32, 32, 32),
					Color.White,
					0f,
					new Vector2(0, 0),
					scale,
					SpriteEffects.None,
					0
					);
			}
        }

        // returns true if something would collide with this object
        public bool CheckCollision(Rectangle rect)
        {
            return new Rectangle((int)position.X, (int)position.Y, 32, 32).Intersects(rect);
        }
        public bool CheckCollision(Vector2 point)
        {
            return new Rectangle((int)position.X, (int)position.Y, 32, 32).Contains(new Rectangle((int)point.X, (int)point.Y, 0, 0));
        }


		// set a new position
		public void SetPosition(int x, int y)
		{
			position = new Vector2(x, y);
		}

        // assign new image coordinates
        public void AssignCoords(Vector2 coords)
        {
            imageCoords = coords;
        }
    }
}
