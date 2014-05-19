using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Ed Amidon

namespace Charlie.Code.Items
{
	public class Inventory
	{
		// reference to the world
		World world;

        // inventory will be a list
		List<InventorySlot> slots = new List<InventorySlot>();

		// constructor
		public Inventory(World world)
		{
			this.world = world;
		}

        // add item method
        public void AddItem(Item item, int num)
        {
			// see if the item is already in the inventory
			foreach (InventorySlot slot in slots)
			{
				if (item.FileName == slot.Item.FileName)
				{
					// add to the count of the already existant item
					slot.Add(num);
					OrderList();
					return;
				}
			}

			// add the item to the inventory
			slots.Add(new InventorySlot(world.ItemHolder.MakeItem(item.FileName), num));

			// reorder the list
			OrderList();
        }

        // remove item
        public void RemoveItem(Item item, int num)
        {
            // search for the item in the inventory and remove it
			foreach (InventorySlot slot in slots)
			{
				if (slot.Item.FileName == item.FileName)
				{
					slot.Subtract(num);
					if (slot.Count <= 0) { slots.Remove(slot); }
					break;
				}
			}
        }

		// returns the quantity of a certain item in the inventory
		public int ItemQuantity(string name)
		{
			foreach (InventorySlot slot in slots)
			{
				if (slot.Item.FileName == name)
				{
					return slot.Count;
				}
			}
			return 0;
		}
		public int ItemQuantity(Item item)
		{
			foreach (InventorySlot slot in slots)
			{
				if (slot.Item == item)
				{
					return slot.Count;
				}
			}
			return 0;
		}

		// see if the inventory has a certain item
		public bool HasItem(string name)
		{
			return ItemQuantity(name) > 0;
		}


		// order the list based on item type
		private void OrderList()
		{
			List<InventorySlot> newList = new List<InventorySlot>();

			// go through all the different types of items in a certain order and add them to the lsit
			List<Type> types = new List<Type>() { typeof(Weapon), typeof(Potion), typeof(Item) };
			foreach (Type type in types)
			{
				foreach (InventorySlot slot in slots)
				{
					if (slot.Item.GetType() == type)
					{
						newList.Add(slot);
					}
				}
			}

			// set the list to the new list
			slots = newList;
		}

		// return a list of the items that can be equipped by the player (really just weapons and potions)
		public List<Item> AllItems
		{
			get 
			{
				List<Item> value = new List<Item>();
				foreach (InventorySlot slot in slots)
				{
					value.Add(slot.Item);
				}
				return value;
			}
		}

		// clears the inventory of all items
		public void Clear()
		{
			slots = new List<InventorySlot>();
		}
	}

	// Gwendolyn Hart
	class InventorySlot
	{
		// the item in this slot and how many of it there are
		Item item;
		public Item Item { get { return item; } }
		int count;
		public int Count { get { return count; } }

		// constructor
		public InventorySlot(Item item)
		{
			this.item = item;
			count = 1;
		}
		public InventorySlot(Item item, int num)
		{
			this.item = item;
			this.count = num;
		}

		// add to the number of items
		public void Add(int num)
		{
			count += num;
		}

		// take away from the number of items
		public void Subtract(int num)
		{
			count -= num;
		}
	}
}
