// Gwendolyn Hart
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Charlie.Code.Items
{
	public class ItemHolder
	{
		// reference to the world
		World world;

		// dictionary that holds all the items
		Dictionary<string, string[]> itemParams;
		public Dictionary<string, string[]> ItemParams { get { return itemParams; } }

		// constructor
		public ItemHolder(World world)
		{
			// reference to the world
			this.world = world;

			// make the dictionary
			itemParams = new Dictionary<string, string[]>();
			// load the parameters
			LoadItemFiles();
		}

		// go through all the item files and put the parameters they contain into the parameter list
		private void LoadItemFiles()
		{
			// get all the files in the appropriate directory
			List<string> fileNames = new List<string>();
			foreach (string filePath in Directory.GetFiles(Directory.GetCurrentDirectory() + "/Content/Defines/Items/"))
			{
				fileNames.Add(Path.GetFileNameWithoutExtension(filePath));
			}

			// go through all the items
			foreach (string fileName in fileNames)
			{
				// load the stream
				StreamReader stream = new StreamReader(Directory.GetCurrentDirectory() + "/Content/Defines/Items/" + fileName + ".txt");

				// put the parameters into the dictionary
				string[] parameters = stream.ReadToEnd().Split('\n');
				for (int i = 0; i < parameters.Length; i++) 
				{
					if (parameters[i][parameters[i].Length - 1] == (char)13)
					{
						parameters[i] = parameters[i].Substring(0, parameters[i].Length - 1);

					}
				}
				itemParams.Add(fileName, parameters);

				// close the stream
				stream.Close();
			}
		}

		// construct an item from its parameters
		public Item MakeItem(string name)
		{
			// if parameters for the given item exist
			if (itemParams.ContainsKey(name))
			{
				string[] parameters = itemParams[name];

				// continue based on the type specified in the first parameter
				string[] typeParam = parameters[0].Split(':');
				if (typeParam[0].ToLower() == "type")
				{
					// holds the default constructor arguments for the items
					Dictionary<string, object> args;
					switch (typeParam[1].ToLower())
					{
						// build a weapon
						#region Weapon
						case "weapon":
							// make the default weapon constructor arguments
							args = new Dictionary<string, dynamic>()
							{
								{ "name", "[Name]" },
								{ "img", null },
								{ "desc", "[Description]" },
								{ "atk", 1 },
								{ "range", 16 },
								{ "cost", 0},
								{ "anim", Weapon.WeaponAnim.melee },
								{ "projectiles", new List<string[]> () }
							};

							// go through and reassign the arguments as appropriate
							for (int i = 1; i < parameters.Length; i++)
							{
								string[] param = parameters[i].Split(':');
								switch (param[0].ToLower())
								{
									case "name":
										args["name"] = param[1];
										break;

									case "img":
										args["img"] = world.ImageHolder.Items[param[1]];
										break;

									case "desc":
										args["desc"] = param[1];
										break;

									case "atk":
										args["atk"] = int.Parse(param[1]);
										break;

									case "range":
										args["range"] = int.Parse(param[1]);
										break;

									case "cost":
										args["cost"] = int.Parse(param[1]);
										break;

									case "anim":
										foreach (Weapon.WeaponAnim weapType in Enum.GetValues(typeof(Weapon.WeaponAnim)))
										{
											if (weapType.ToString() == param[1])
											{
												args["anim"] = weapType;
												break;
											}
										}
										break;

									case "projectile":
										string[] projectile = new string[param.Length - 1];
										for (int ii = 0; ii < projectile.Length; ii++) 
										{
											projectile[ii] = param[ii + 1];
										}
										((List<string[]>)args["projectiles"]).Add(projectile);
										break;

									default:
										break;
								}
							}
							
							// make the weapon to return
							Weapon weap = new Weapon(
								world,
								name, 
								(string)args["name"], 
								(Texture2D)args["img"], 
								(string)args["desc"], 
								(int)args["atk"], 
								(int)args["range"], 
								(int)args["cost"],
								(Weapon.WeaponAnim)args["anim"],
								(List<string[]>)args["projectiles"]);
							return weap;
						#endregion

						// build a potion
						#region Potion
						case "potion":
							// make default potion constructor arguments
							args = new Dictionary<string, object>()
							{
								{ "name", "[Name]" },
								{ "img", null },
								{ "desc", "[Description]" },
								{ "cat", Potion.Category.health },
								{ "value", 0 }
							};

							// reassign the arguments as appropriate
							for (int i = 1; i < parameters.Length; i++)
							{
								string[] param = parameters[i].Split(':');
								switch (param[0].ToLower())
								{
									case "name":
										args["name"] = param[1];
										break;

									case "img":
										args["img"] = world.ImageHolder.Items[param[1]];
										break;

									case "desc":
										args["desc"] = param[1];
										break;

									case "cat":
										foreach (Potion.Category cat in Enum.GetValues(typeof(Potion.Category)))
										{
											if (cat.ToString() == param[1])
											{
												args["cat"] = cat;
												break;
											}
										}
										break;

									case "value":
										args["value"] = int.Parse(param[1]);
										break;

									default:
										break;
								}
							}

							// make the potion to return
							Potion potion = new Potion(
								world,
								name,
								(string)args["name"],
								(Texture2D)args["img"],
								(string)args["desc"],
								(Potion.Category)args["cat"],
								(int)args["value"]);
							return potion;
						#endregion

						// build a miscellaneous object
						#region Misc
						case "misc":
							// make default misc constructor arguments
							args = new Dictionary<string, object>()
							{
								{ "name", "[Name]" },
								{ "img", null },
								{ "desc", "[Description]" }
							};

							// reassign the arguments as appropriate
							for (int i = 1; i < parameters.Length; i++)
							{
								string[] param = parameters[i].Split(':');
								switch (param[0].ToLower())
								{
									case "name":
										args["name"] = param[1];
										break;

									case "img":
										args["img"] = world.ImageHolder.Items[param[1]];
										break;

									case "desc":
										args["desc"] = param[1];
										break;

									default:
										break;
								}
							}

							// make the item to return
							Item item = new Item(
								name,
								(string)args["name"],
								(Texture2D)args["img"],
								(string)args["desc"]);
							return item;
						#endregion

						// not a valid type, ergo return null
						default:
							return null;
					}
				}
				else
				{
					// the first parameter is not type, abandon mission
					return null;
				}
			}
			else
			{
				return null;
			}
		}
	}
}
