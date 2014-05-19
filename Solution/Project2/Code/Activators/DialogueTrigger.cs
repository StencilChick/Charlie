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
	class DialogueTrigger : Activator
	{
		// value for the dialogue
		string dialogueName;
		public string DialogueName { get { return dialogueName; } set { dialogueName = value; } }

		// constructor
		public DialogueTrigger(World world, Vector2 position, string dialogueName) 
			: base(world, null, (int)position.X, (int)position.Y)
		{
			this.dialogueName = dialogueName;
		}

		// what to do when activated
		public override void Activate()
		{
			world.Dialogue = new Dialogue(world, dialogueName);
		}
	}
}
