// made by Gwendolyn Hart
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;

namespace Charlie.Code.Characters
{
	public class NPC : Character
	{
		// whether the NPC is an ally or not
		bool ally;
		public bool Ally { get { return ally; } set { ally = value; } }

		// whether the NPC is hostile or not
		bool hostile;
		public bool Hostile { get { return hostile; } set { hostile = value; } }

		// the name of the dialogue this npc starts when triggered
		string dialogue;
		public string Dialogue { get { return dialogue; } set { dialogue = value; } }

		// just a thing to remember where the character was facing before dialogue began so they can go back to facing it once it ends
		Direction oldDir;

		// constructor
		public NPC(World world, string fileName, string id, Texture2D image, bool ally, bool hostile, string dialogue)
			: base(world, fileName, id, image, 2.5f * 32, 3, 4)
		{
			this.dialogue = dialogue;
			this.ally = ally;
			this.hostile = hostile;
		}

		// update-y thing
		public void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			// things for facing back where the character was facing before dialogue started after dialgoue ended
			if (world.Dialogue == null) 
			{ 
				SetDirection(oldDir);
			}
		}


		// function for opening dialouge
		public void OpenDialogue()
		{
			if (dialogue != "")
			{
				// face the player
				oldDir = direction;
				LookAt(world.Player);

				world.Dialogue = new Dialogue(world, dialogue);
			}
		}

		// override the set direction thing
		public void SetDirection(Direction direction)
		{
			base.SetDirection(direction);
			oldDir = direction;
		}
	}
}
