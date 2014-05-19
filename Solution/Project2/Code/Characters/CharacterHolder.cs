// made by Gwendolyn Hart
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Charlie.Code.Characters
{
	public class CharacterHolder
	{
		// reference to the world object
		World world;

		// thing that holds all the parameters to build an npc
		Dictionary<string, string[]> npcParams;
		public Dictionary<string, string[]> NPCParams { get { return npcParams; } }

		// thing that holds all the parameters to build a monster
		Dictionary<string, string[]> monsterParams;
		public Dictionary<string, string[]> MonsterParams { get { return monsterParams; } }

		// la constructor
		public CharacterHolder(World world)
		{
			this.world = world;

			// make the parameter dictionaries
			npcParams = LoadParameters("NPCs");
			monsterParams = LoadParameters("Monsters");
		}

		// load the parameters from the files in a given directory
		private Dictionary<string, string[]> LoadParameters(string dir)
		{
			// the dictionary to be returned at the end
			Dictionary<string, string[]> dict = new Dictionary<string, string[]>();

			// get all the files in the appropriate directory
			List<string> fileNames = new List<string>();
			foreach (string filePath in Directory.GetFiles(Directory.GetCurrentDirectory() + "/Content/Defines/Characters/" + dir))
			{
				fileNames.Add(Path.GetFileNameWithoutExtension(filePath));
			}

			// get the content from each file
			foreach (string nomen in fileNames)
			{
				// get the stream of the file
				StreamReader stream = new StreamReader(Directory.GetCurrentDirectory() + "/Content/Defines/Characters/" + dir + "/" + nomen + ".txt");

				// break the stream into its parameters and add them to the dictionary
				string[] parameters = stream.ReadToEnd().Split('\n');
				// code to get rid of any straggling carriage returns that keep annoyingly showing up
				for (int i = 0; i < parameters.Length; i++)
				{
					if (parameters[i].Length > 0)
					{
						if (parameters[i][parameters[i].Length - 1] == 13)
						{
							parameters[i] = parameters[i].Substring(0, parameters[i].Length - 1);
						}
					}
				}
				// add the parameters to the dictionary
				dict.Add(nomen, parameters);

				// close the stream
				stream.Close();
			}

			// return the dictionary
			return dict;
		}


		// return the npc defined by the given name
		public NPC MakeNPC(string name, string id, string dialogue, bool hostile, bool ally)
		{
			string[] parameters = npcParams[name];
			return new NPC(world, name, id, world.ImageHolder.CharacterSheets[parameters[0]], ally, hostile, dialogue);
		}

		// return the monster defined by the given name
		public Monster MakeMonster(string name, string id)
		{
			// make sure the given file name exists
			if (monsterParams.ContainsKey(name))
			{
				// define the default arguments
				Dictionary<string, object> args = new Dictionary<string, object>()
				{
					{ "img", null },
					{ "speed", 2.5f },
					{ "health", 3 },
					{ "moving", true },
					{ "projectile", new List<string[]>() }
				};

				// get the parameters definined in the file
				string[] parameters = monsterParams[name];

				// go through the given parameters and reassign the arguments as necessary
				foreach (string p in parameters)
				{
					string[] param = p.Split(':');

					switch (param[0].ToLower())
					{
						// reassign image
						case "img":
							args["img"] = world.ImageHolder.CharacterSheets[param[1]];
							break;

						// reassign speed
						case "speed":
							args["speed"] = float.Parse(param[1]);
							break;

						// reassign health
						case "health":
							args["health"] = int.Parse(param[1]);
							break;

						// whether this monster should move or stand still
						case "moving":
							args["moving"] = !(param[1].ToLower() == "false");
							break;

						// add a projectile
						case "projectile":
							((List<string[]>)args["projectile"]).Add(new string[] { param[1], param[2], param[3], param[4], param[5] });
							break;

						// not a valid thing to reassign
						default:
							break;
					}
				}

				// return an NPC built from the arguments
				return new Monster(world, name, id, (Texture2D)args["img"], (float)args["speed"] * 32, (int)args["health"], (bool)args["moving"], (List<string[]>)args["projectile"]);
			}
			else
			{
				// no monster with that file name, so return null
				return null;
			}
		}
	}
}
