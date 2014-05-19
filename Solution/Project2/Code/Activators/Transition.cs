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
	public class Transition : Activator
	{
		// values for moving the player
		public Vector2 playerPos;
		public Vector2 roomPos;
		public string mapName;

		// constructor
		public Transition(World world, Vector2 position, Vector2 playerPos, Vector2 roomPos, string mapName)
			: base(world, null, (int)position.X, (int)position.Y)
		{
			this.playerPos = playerPos;
			this.roomPos = roomPos;
			this.mapName = mapName;
		}

		public override void Activate()
		{
			// if the map name is not equal to "", load the map of the specified name
			if (mapName != "") { world.Map = new MapCode.Map(world, mapName); }
			// set the active room to the room the player should be in
			world.Map.SetActiveRoom((int)roomPos.X, (int)roomPos.Y);
			// set the player's position in the room
			world.Player.SetPosition((int)playerPos.X * 32, (int)playerPos.Y * 32);

			// change the music for the new room
			world.AudioHandler.PlaySong(world.Map.ActiveRoom.Music);
		}
	}
}
