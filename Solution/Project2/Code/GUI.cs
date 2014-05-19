// made by Gwendolyn Hart
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Charlie.Code
{
    // holds the methods for drawing all sorts of gui-type stuff
    public class GUI
    {
        // reference to the world object
        World world;

        // constructor
        public GUI(World world)
        {
            this.world = world;
        }


		/// <summary>
		/// Draws a box with text to the screen
		/// </summary>
		/// <param name="rect">the area of the box</param>
		/// <param name="msg">the text for the box to display</param>
		public void Box(Rectangle rect, string msg)
		{
			// draw the box
			DrawBox(rect, world.ImageHolder.GUI["textBorder"], world.ImageHolder.GUI["textCorner"], world.ImageHolder.GUI["textBackdrop"]);
			// write the string over the box
			//world.SpriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], msg, new Vector2(rect.X, rect.Y), Color.White);
			DrawCentredText(rect, world.ImageHolder.Fonts["SpriteFont1"], msg, Color.White);
		}
		public void Box(Rectangle rect, string msg, Color colour, Texture2D border, Texture2D corner, Texture2D backdrop)
		{
			// draw the box
			DrawBox(rect, border, corner, backdrop);
			// write the string voer the box
			//world.SpriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], msg, new Vector2(rect.X, rect.Y), colour);
			DrawCentredText(rect, world.ImageHolder.Fonts["SpriteFont1"], msg, colour);
		}

		public void Label(SpriteFont font, string msg, Rectangle rect, Color colour)
		{
			DrawCentredText(rect, font, msg, colour);
		}
		/// <summary>
		/// Draws text to the screen wrapped within a given rectangle
		/// </summary>
		/// <param name="font">the font to be used for the text</param>
		/// <param name="msg">the string to render</param>
		/// <param name="rect">the bounds of the string</param>
		/// <param name="color">the colour of the text</param>
		public void Text(SpriteFont font, string msg, Rectangle rect, Color color)
		{
			// find the average length of the letters in this font
			int charLen = 0;
			for (int c = 97; c <= 122; c++)
			{
				charLen += (int)font.MeasureString("" + (char)c).X;
			}
			charLen = (int)(charLen * 1.0 / 26);

			// wrap the text to the length of the rectangle
			msg = WrapText(msg, rect.Width / charLen);

			// draw the text to the screen
			world.SpriteBatch.DrawString(font, msg, new Vector2(rect.X, rect.Y), color);
		}

        /// <summary>
        /// Draws a box to the screen and allows the user to edits its contents
        /// </summary>
        /// <param name="rect">the area of the box</param>
        /// <param name="value">the value for the user to modefy</param>
        public string TextField(Rectangle rect, string value)
        {
			// genderate the ID for this textbox
			int ID = 3 * (rect.X + 1) + 5 * (rect.Y + 1);

			// draw the box
			if (world.InputHandler.ActiveTextboxID == ID)
			{
				DrawBox(rect, world.ImageHolder.GUI["textBorder_active"], world.ImageHolder.GUI["textCorner_active"], world.ImageHolder.GUI["textBackdrop_active"]);
			}
			else
			{
				DrawBox(rect, world.ImageHolder.GUI["textBorder"], world.ImageHolder.GUI["textCorner"], world.ImageHolder.GUI["textBackdrop"]);
			}
			// write the string over the box
			world.SpriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], value, new Vector2(rect.X, rect.Y), Color.White);

			// check whether this textbox is active
			if (world.InputHandler.ActiveTextboxID == ID)
			{
				// modefy the string value
				value = world.InputHandler.AddTextInputToString(value);

				// if the user clicks outside this textbox, set the active id back to zero
				if (!rect.Intersects(new Rectangle((int)world.InputHandler.MousePosition.X, (int)world.InputHandler.MousePosition.Y, 0, 0)) && world.InputHandler.MouseButtonPressed(0))
				{
					world.InputHandler.ActiveTextboxID = 0;
				}
			}
			else
			{
				// tell input handler to take text input if the user clicks inside this box
				if (rect.Intersects(new Rectangle((int)world.InputHandler.MousePosition.X, (int)world.InputHandler.MousePosition.Y, 0, 0)) && world.InputHandler.MouseButtonPressed(0))
				{
					world.InputHandler.ActiveTextboxID = ID;
				}
			}

			// return the potentially modefied string value
			return value;
        }
		/// <summary>
		/// Draws a box to the screen and allows the user to edits its contents
		/// </summary>
		/// <param name="rect">the area of the box</param>
		/// <param name="value">the value for the user to modefy</param>
		/// <param name="maxLength">the maximum length the value can be</param>
		/// <returns></returns>
		public string TextField(Rectangle rect, string value, int maxLength)
		{
			// genderate the ID for this textbox
			int ID = 3 * (rect.X + 1) + 5 * (rect.Y + 1);

			// draw the box
			if (world.InputHandler.ActiveTextboxID == ID)
			{
				DrawBox(rect, world.ImageHolder.GUI["textBorder_active"], world.ImageHolder.GUI["textCorner_active"], world.ImageHolder.GUI["textBackdrop_active"]);
			}
			else
			{
				DrawBox(rect, world.ImageHolder.GUI["textBorder"], world.ImageHolder.GUI["textCorner"], world.ImageHolder.GUI["textBackdrop"]);
			}
			// write the string over the box
			world.SpriteBatch.DrawString(world.ImageHolder.Fonts["SpriteFont1"], value, new Vector2(rect.X, rect.Y), Color.White);

			// check whether this textbox is active
			if (world.InputHandler.ActiveTextboxID == ID)
			{
				// modefy the string value
				value = world.InputHandler.AddTextInputToString(value);
				// keep the value from exceeding maximum length
				if (value.Length > maxLength) { value = value.Substring(0, maxLength); }

				// if the user clicks outside this textbox, set the active id back to zero
				if (!rect.Intersects(new Rectangle((int)world.InputHandler.MousePosition.X, (int)world.InputHandler.MousePosition.Y, 0, 0)) && world.InputHandler.MouseButtonPressed(0))
				{
					world.InputHandler.ActiveTextboxID = 0;
				}
			}
			else
			{
				// tell input handler to take text input if the user clicks inside this box
				if (rect.Intersects(new Rectangle((int)world.InputHandler.MousePosition.X, (int)world.InputHandler.MousePosition.Y, 0, 0)) && world.InputHandler.MouseButtonPressed(0))
				{
					world.InputHandler.ActiveTextboxID = ID;
				}
			}

			// return the potentially modefied string value
			return value;
		}

        /// <summary>
        /// Draws a button to the screen and returns a value of true if the user clicks on it
        /// </summary>
        /// <param name="rect">the area of the button</param>
        /// <param name="message">what text the button should display</param>
        /// <returns>a boolean value which is true only on the frame in which the user clicks on the button</returns>
        public bool Button(Rectangle rect, string message)
        {
            // draw the box
            if (rect.Intersects(new Rectangle((int)world.InputHandler.MousePosition.X, (int)world.InputHandler.MousePosition.Y, 0, 0)) && world.InputHandler.MouseButtonHeld(0))
            {
                // the user is clicking on this button
                DrawBox(rect, world.ImageHolder.GUI["buttonBorder_clicked"], world.ImageHolder.GUI["buttonCorner_clicked"], world.ImageHolder.GUI["buttonBackdrop_clicked"]);
            }
            else
            {
                // the user is not clicking on this button
                DrawBox(rect, world.ImageHolder.GUI["buttonBorder"], world.ImageHolder.GUI["buttonCorner"], world.ImageHolder.GUI["buttonBackdrop"]);
            }
            // write the string over the box
            DrawCentredText(rect, world.ImageHolder.Fonts["SpriteFont1"], message, Color.White);
            
            // return true if the user has just clicked on this button this very frame
            if (rect.Intersects(new Rectangle((int)world.InputHandler.MousePosition.X, (int)world.InputHandler.MousePosition.Y, 0, 0)) && world.InputHandler.MouseButtonPressed(0))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
		/// <summary>
		/// Draws an image to the screen as a button
		/// </summary>
		/// <param name="image">the image to draw as a button</param>
		/// <param name="position">the position of the button</param>
		/// <param name="message">the label for the button</param>
		/// <returns>a boolean value based on whether the user has clicked on the button</returns>
		public bool Button(Texture2D image, Vector2 position, string message)
		{
			// draw the box
			world.SpriteBatch.Draw(image, position, Color.White);
			// draw the text
			DrawCentredText(new Rectangle((int)position.X, (int)position.Y, image.Width, image.Height), world.ImageHolder.Fonts["SpriteFont1"], message, Color.White);

			// return true if the user has just clicked on this button
			if (new Rectangle((int)position.X, (int)position.Y, image.Width, image.Height).Intersects(new Rectangle((int)world.InputHandler.MousePosition.X, (int)world.InputHandler.MousePosition.Y, 0, 0)) && world.InputHandler.MouseButtonPressed(0))
			{
				return true;
			}
			else
			{
				return false;
			}
		}


        // draws an edge around a rectangle
        private void DrawBox(Rectangle rect, Texture2D border, Texture2D corner, Texture2D backdrop)
        {
            // backdrop
            world.SpriteBatch.Draw(
                backdrop,
                rect,
                Color.White
                );

            // top border
            world.SpriteBatch.Draw(
                border,
                new Rectangle(rect.X, rect.Y - border.Height, rect.Width, border.Height),
                Color.White
                );
            // bottom border
            world.SpriteBatch.Draw(
                border,
                new Rectangle(rect.X, rect.Y + rect.Height, rect.Width, border.Height),
                border.Bounds,
                Color.White,
                0f,
                new Vector2(0, 0),
                SpriteEffects.FlipVertically,
                0
                );
            // left border
            world.SpriteBatch.Draw(
                border,
                new Rectangle(rect.X, rect.Y, rect.Height, border.Height),
                border.Bounds,
                Color.White,
                (float)(Math.PI / 2),
                new Vector2(0, 0),
                SpriteEffects.FlipVertically,
                0
                );
            // right border
            world.SpriteBatch.Draw(
                border,
                new Rectangle(rect.X + rect.Width + border.Height, rect.Y, rect.Height, border.Height),
                border.Bounds,
                Color.White,
                (float)(Math.PI / 2),
                new Vector2(0, 0),
                SpriteEffects.None,
                0
                );
            
            // top left corner
            world.SpriteBatch.Draw(
                corner,
                new Rectangle(rect.X - corner.Width, rect.Y - corner.Height, corner.Width, corner.Height),
                Color.White
                );
            // top right corner
            world.SpriteBatch.Draw(
                corner,
                new Rectangle(rect.X + rect.Width, rect.Y - corner.Height, corner.Width, corner.Height),
                corner.Bounds,
                Color.White,
                0f,
                new Vector2(0, 0),
                SpriteEffects.FlipHorizontally,
                0
                );
            // bottom left corner
            world.SpriteBatch.Draw(
                corner,
                new Rectangle(rect.X - corner.Width, rect.Y + rect.Height, corner.Width, corner.Height),
                corner.Bounds,
                Color.White,
                0f,
                new Vector2(0, 0),
                SpriteEffects.FlipVertically,
                0
                );
            // bottom right corner
            world.SpriteBatch.Draw(
                corner,
                new Rectangle(rect.X + rect.Width, rect.Y + rect.Height, corner.Width, corner.Height),
                corner.Bounds,
                Color.White,
                0f,
                new Vector2(0, 0),
                SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally,
                0
                );
        }

		// centres a single line of text in a rectangle
		private void DrawCentredText(Rectangle rect, SpriteFont font, string msg, Color colour)
		{
			float stringLen = font.MeasureString(msg).X;
			world.SpriteBatch.DrawString(
				font,
				msg,
				new Vector2((int)(rect.X + rect.Width / 2 - stringLen / 2), rect.Y),
				colour);
		}

		// inserts line breaks into a string in order to make it fit the given length in characters
		private string WrapText(string msg, int maxLen)
		{
			// what to return at the end
			string finalMsg = "";

			// each word in the original message (plus attached punctuation)
			string[] words = msg.Split(' ');

			// go through each word and seperate them into lines
			string line = "";
			foreach (string word in words)
			{
				// if adding this word to the line is not within the maximum length of the line
				if ((line + word + " ").Length > maxLen)
				{
					// start a new line
					finalMsg += line + "\n";
					line = "";

				}

				// add this word and a space after it to the line
				line += word + " ";
			}
			finalMsg += line;

			// return the finalised message
			return finalMsg;
		}
    }
}
