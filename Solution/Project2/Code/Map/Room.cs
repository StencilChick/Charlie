// made by Gwendolyn Hart
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Charlie.Code.MapCode
{
    public class Room
    {
        // list of tiles in the room
        List<Tile> tiles;
        public List<Tile> Tiles { get { return tiles; } }

        // list of objects
		List<Object> objects;
		public List<Object> Objects { get { return objects; } }

		// list of objects to be added
		List<Object> additionList;
		public List<Object> AdditionList { get { return additionList; } }

		// list of objects to be removed
		List<Object> dispose;
		public List<Object> Dispose { get { return dispose; } }

		// name of the music for this room
		string music;
		public string Music { get { return music; } set { music = value; } }

        // construct!
        public Room()
        {
            // make the various and sundry lists
            tiles = new List<Tile>();

			objects = new List<Object>();
			dispose = new List<Object>();
			additionList = new List<Object>();

			music = "";
        }

        // add a tile to the list of tiles
        public void AddTile(Tile tile)
        {
            tiles.Add(tile);
        }

		// add an object to the list of objects
		public void AddObject(Object obj)
		{
			//objects.Add(obj);
			additionList.Add(obj);
		}
		
		// gets an object at the given coordinates
		public Object FindObjectAt(int x, int y)
		{
			foreach (Object obj in objects)
			{
				if (obj.Position == new Vector2(x, y))
				{
					return obj;
				}
			}
			return null;
		}

		// removes a given object
		public void RemoveObject(Object obj)
		{
			dispose.Add(obj);
		}

		// removes an object at the given coordinates
		public void RemoveObjectAt(int x, int y)
		{
			dispose.Add(FindObjectAt(x, y));
		}

		// removes and adds objects to the room
		public void CleanObjects()
		{
			// remove objects to be disposed of
			foreach (Object obj in dispose)
			{
				objects.Remove(obj);
			}
			dispose.Clear();

			// add objects which must be added
			foreach (Object obj in additionList)
			{
				objects.Add(obj);
			}
			additionList.Clear();
		}


		// returns the objects ordered by position for drawing purposes
		public List<Object> OrderedObjects(World world)
		{
			// the list that will be returned at the end
			List<Object> orderedList = new List<Object>();
			// add that tricksy hobbits of a player to the mix
			orderedList.Add(world.Player);

			// put the elements of the objects list into the ordered list arranged by their y-position
			foreach (Object obj in objects)
			{
				for (int i = 0; i <= orderedList.Count; i++)
				{
					if (i < orderedList.Count)
					{
						if (orderedList[i].Position.Y > obj.Position.Y)
						{
							orderedList.Insert(i, obj);
							break;
						}
					}
					else
					{
						orderedList.Add(obj);
						break;
					}
				}
			}

			// return the ordered list
			return orderedList;
		}
		// returns the objects in the room but not the player ordered by position for drawing purposes (used by the editor)
		public List<Object> OrderedObjectsWithoutPlayer()
		{
			// the list that will be returned at the end
			List<Object> orderedList = new List<Object>();

			// put the elements of the objects list into the ordered list arranged by their y-position
			foreach (Object obj in objects)
			{
				for (int i = 0; i <= orderedList.Count; i++)
				{
					if (i < orderedList.Count)
					{
						if (orderedList[i].Position.Y > obj.Position.Y)
						{
							orderedList.Insert(i, obj);
							break;
						}
					}
					else
					{
						orderedList.Add(obj);
						break;
					}
				}
			}

			// return the ordered list
			return orderedList;
		}
    }
}
