using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Charlie.Code.MapCode;
using Charlie.Code.Characters;
using Charlie.Code.Items;
// Ed Amidon

namespace Charlie.Code.Items
{
	public class Weapon : Item
	{
        // attributes
		World world;

		// how much damage the weapon does on contact
		int damage;
		public int Damage { get { return damage; } }

		// how far away the weapon can make contact with something
		int range;
		public int Range { get { return range; } }

		// how much mana this weaon costs
		int cost;
		public int Cost { get { return cost; } }

		// what type of attack the weapon is
		public enum WeaponAnim
		{
			melee, ranged, magic
		};
		WeaponAnim _weaponAnim;
		public WeaponAnim weaponAnim { get { return _weaponAnim; } }
		
		// what projectiles the weapon fires
		List<string[]> projParams;
		public List<string[]> ProjParams { get { return projParams; } }
		
		// constructor
		public Weapon(World world, string fileName, string name, Texture2D image, string desc, int damage, int range, int cost, WeaponAnim weaponType, List<string[]> projParams) 
			: base(fileName, name, image, desc)
		{
			this.world = world;
			this.damage = damage;
			this.range = range;
			this.cost = cost;
			this._weaponAnim = weaponType;
			this.projParams = projParams;
		}

		// plays a sound effect for using this weapon based on the type of weapon it is
		private void PlaySound()
		{
			if (weaponAnim == WeaponAnim.magic)
			{
				world.AudioHandler.PlaySound("Magic");
			}
			else
			{
				world.AudioHandler.PlaySound("Hit");
			}
		}

		// creates the projectiles this weapon fires with position being the position of the one firing the projectiles
		public void FireProjectiles(Vector2 position, MovingObject.Direction direction)
		{
			if (world.Player.Mana >= cost)
			{
				// subtract appropriate mana
				world.Player.Mana -= cost;

				// figure what projectiles to fire
				foreach (string[] parameters in projParams)
				{
					// make the projectile
					Projectile projectile = new Projectile(
						world,
						world.ImageHolder.Projectiles[parameters[0]],
						MovingObject.Direction.up,
						float.Parse(parameters[2]) * 32,
						int.Parse(parameters[3]),
						parameters[4].ToLower() != "false");

					// set the direction of the projectile
					int dirInt = 0;
					Array enumVals = Enum.GetValues(typeof(MovingObject.Direction));
					for (int i = 0; i < enumVals.Length; i++)
					{
						if (enumVals.GetValue(i).ToString() == parameters[1].ToLower())
						{
							dirInt = i;
							break;
						}
					}
					dirInt += (int)direction;
					if (dirInt >= enumVals.Length) { dirInt -= enumVals.Length; }
					projectile.SetDirection((MovingObject.Direction)dirInt);

					// set the position of the projectile
					Vector2 pos = position + projectile.Up * 32;
					projectile.SetPosition((int)pos.X, (int)pos.Y);

					// add the projectile to the map
					world.Map.ActiveRoom.AddObject(projectile);
				}

				// play the sound the weapon makes
				PlaySound();
			}
		}
	}

	// Gwendolyn Hart
	// the projectiles fired by some weapons
	public class Projectile : MovingObject
	{
		// damage it does
		int damage;
		public int Damage { get { return damage; } }

		// whether it collides with the tiles on the map
		bool collidable;
		public bool Collidable { get { return collidable; } }

		public Projectile(World world, Texture2D image, Direction dir, float speed, int damage, bool coll)
			: base(world, image, speed)
		{
			SetDirection(dir);
			this.damage = damage;
			this.collidable = coll;
		}

		public void Update(GameTime gameTime)
		{
			// update the base
			base.Update(gameTime);

			// move about
			MoveDirection(gameTime, Up);

			// check for collision
			Object obj = Collide();
			if (obj != null)
			{
				HandleCollision(obj);
			}

			// check for the projectile leaving the bounds of the screen
			if (!new Rectangle(-32, -32, 640 + 32, 480 + 32).Contains((int)position.X, (int)position.Y))
			{
				Destroy();
			}
		}

		// decides what to do when the projectile collides with something
		private void HandleCollision(Object obj)
		{
			// tiles
			if (obj.GetType() == typeof(Tile))
			{
				if (collidable) { Destroy(); }
			}
			// items
			else if (obj.GetType() == typeof(Item))
			{
				if (collidable) { Destroy(); }
			}
			// characters
			else if (obj.GetType().BaseType == typeof(Character) && obj.GetType() != typeof(NPC))
			{
				((Character)obj).Damage(damage, direction + 2);
				Destroy();
			}
		}

		private void Destroy()
		{
			world.Map.ActiveRoom.RemoveObject(this);
		}
	}
}
