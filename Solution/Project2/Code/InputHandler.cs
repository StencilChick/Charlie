// made by Gwendolyn Hart
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Charlie.Code
{
    // handles the finer points of key-pressing interpretation
    public class InputHandler
    {
		// key states for this frame and the last frame
        KeyboardState newKeyState;
        KeyboardState oldKeyState;

        // mouse states for this frame and the last frame
        MouseState newMouseState;
        MouseState oldMouseState;

		// states of the keyboard and mouse
		public KeyboardState KeyBoardState { get { return oldKeyState; } }
		public MouseState MouseState { get { return MouseState; } }

		// the current position of the mouse
		public Vector2 MousePosition
		{
			get
			{
				return new Vector2(newMouseState.X, newMouseState.Y);
			}
		}

		// the id that keeps track of which textbox is active
		int activeTextboxID;
		public int ActiveTextboxID
		{
			get { return activeTextboxID; }
			set { if (value >= 0) { activeTextboxID = value; } }
		}

		// buffer for text input
		string characterBuffer;


        // ze constructor!
        public InputHandler(World world)
        {
            // stuff about states and junk
            newKeyState = new KeyboardState();
            oldKeyState = new KeyboardState();

            newMouseState = new MouseState();
            oldMouseState = new MouseState();

			// set the textbox id to zero (none)
			activeTextboxID = 0;

			// set up event to hook keyboard input
			characterBuffer = "";
			HookKeys(world);
        }

        // update the input handler
        public void Update(KeyboardState keyState)
        {
			// update the keyboard states
            oldKeyState = newKeyState;
            newKeyState = keyState;
        }
        public void Update(KeyboardState keyState, MouseState mouseState)
        {
            // update the keyboard states
            oldKeyState = newKeyState;
            newKeyState = keyState;

            // update the mouse states
            oldMouseState = newMouseState;
            newMouseState = mouseState;
        }

        // true if a key is currently being held
        public bool KeyHeld(Keys key)
        {
            return newKeyState.IsKeyDown(key);
        }
        // true if a key has just been pressed this frame
        public bool KeyPressed(Keys key)
        {
            return newKeyState.IsKeyDown(key) && !oldKeyState.IsKeyDown(key);
        }
        // treu if a key has just been released this frame
        public bool KeyReleased(Keys key)
        {
            return !newKeyState.IsKeyDown(key) && oldKeyState.IsKeyDown(key);
        }


        /// <summary>
        /// Returns true if the designated mouse button is being held
        /// </summary>
        /// <param name="button">0 - Left Button, 1 - Middle Button, 2 - Right Button</param>
        /// <returns>a boolean value</returns>
        public bool MouseButtonHeld(int button)
        {
            if (button == 0)
            {
                return newMouseState.LeftButton == ButtonState.Pressed;
            }
            else if (button == 1)
            {
                return newMouseState.MiddleButton == ButtonState.Pressed;
            }
            else if (button == 2)
            {
                return newMouseState.RightButton == ButtonState.Pressed;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// true if the designated mouse button has just ben pressed this frame
        /// </summary>
        /// <param name="button">0 - Left Button, 1 - Middle Button, 2 - Right Button</param>
        /// <returns>a boolean value</returns>
        public bool MouseButtonPressed(int button)
        {
            if (button == 0)
            {
                return newMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released;
            }
            else if (button == 1)
            {
                return newMouseState.MiddleButton == ButtonState.Pressed && oldMouseState.MiddleButton == ButtonState.Released;
            }
            else if (button == 2)
            {
                return newMouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// true if the designated mouse button has just been released this frame
        /// </summary>
        /// <param name="button">0 - Left Button, 1 - Middle Button, 2 - Right Button</param>
        /// <returns>a boolean value</returns>
        public bool MouseButtonReleased(int button)
        {
            if (button == 0)
            {
                return newMouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed;
            }
            else if (button == 1)
            {
                return newMouseState.MiddleButton == ButtonState.Released && oldMouseState.MiddleButton == ButtonState.Pressed;
            }
            else if (button == 2)
            {
                return newMouseState.RightButton == ButtonState.Released && oldMouseState.RightButton == ButtonState.Pressed;
            }
            else
            {
                return false;
            }
        }


		// stuff for text input from the keyboard
		// credit to Jekev at [http://strikelimit.co.uk/m/?p=421] for showing how to hook keys with OpenTK events
		private void HookKeys(World world)
		{
			OpenTK.GameWindow OTKWindow = null;
			System.Reflection.FieldInfo field = typeof(OpenTKGameWindow).GetField("window", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			OTKWindow = field.GetValue(world.Window) as OpenTK.GameWindow;
			OTKWindow.KeyPress += GetKeypressChar;
		}
		// when taking text input, add the key values the user types to the key buffer
		private void GetKeypressChar(object sender, OpenTK.KeyPressEventArgs args)
		{
			if (activeTextboxID != 0)
			{
				characterBuffer += args.KeyChar;
			}
		}
		/// <summary>
		/// returns the buffer of entered characters and clears it
		/// </summary>
		/// <returns>the characters in a string</returns>
		private string GetCharacterBuffer()
		{
			string value = characterBuffer;
			characterBuffer = "";
			return value;
		}
		/// <summary>
		/// Takes a string and adds the user's text input to the end of it (this includes backspaces)
		/// </summary>
		/// <param name="msg">the string to modefy</param>
		/// <returns>the modified string</returns>
		public string AddTextInputToString(string msg)
		{
			string buffer = GetCharacterBuffer();
			foreach (char c in buffer)
			{
				// check for certain characters
				if (c == '\n' || c == '\t' || c == '\r' || c == '\v')
				{
					// these characters make nothing happen
				}
				else if (c == '\b')
				{
					// have backspaces remove characters from the end
					if (msg.Length > 0) { msg = msg.Substring(0, msg.Length - 1); }
				}
				else
				{
					// let everything else be added to the string
					msg += c;
				}
			}
			return msg;
		}
    }
}
