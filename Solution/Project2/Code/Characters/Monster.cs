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

using Charlie.Code.Items;

namespace Charlie.Code.Characters
{
	public class Monster : Character
	{
		// true if this monster should move toward the player, false if it should stand still
		bool approaching;

		// how long to wait after damaging the player to move again
		float pause;

		// what the monster uses to fire projectiles
		Weapon attack;
		float attackCooldown;

		// constructor
		public Monster(World world, string fileName, string id, Texture2D image, float speed, int health, bool approaching, List<string[]> projParams)
			: base(world, fileName, id, image, speed, health, 0)
		{
			this.approaching = approaching;

			pause = 0;
			attackCooldown = 1.25f;

			// add the projectile parameters into the attack
			attack = new Weapon(world, "sword", "", null, "", 0, 0, 0, Weapon.WeaponAnim.magic, projParams);
		}

		// update function
		public void Update(GameTime gameTime)
		{
			// update the base
			base.Update(gameTime);

			if (pause <= 0)
			{
				if (stunTime <= 0)
				{
					// look at the player
					LookAt(world.Player);

					if (approaching)
					{
						// run at the player
						MoveToward(gameTime, world.Player.Position);
						walking = true;
					}

					// try to damage the player
					Attack();

					// fire projectiles if appropriate
					if (attack.ProjParams.Count > 0)
					{
						if (attackCooldown <= 0)
						{
							attackCooldown = 2.5f;
							attack.FireProjectiles(position, direction);
						}
						attackCooldown -= gameTime.ElapsedGameTime.Milliseconds * 1f / 1000;
					}
				}
			}
			else
			{
				pause -= gameTime.ElapsedGameTime.Milliseconds * 1.0f / 1000;
			}
		}

		// function for attacking the player
		private void Attack()
		{
			// define the rectangle the player must be within to be damaged by this monster
			Rectangle rect;
			switch (direction)
			{
				case Direction.up:
					rect = new Rectangle((int)position.X, (int)position.Y - 2, 32, 2);
					break;

				case Direction.down:
					rect = new Rectangle((int)position.X, (int)position.Y + 32, 32, 2);
					break;

				case Direction.right:
					rect = new Rectangle((int)position.X + 32, (int)position.Y, 2, 32);
					break;

				case Direction.left:
					rect = new Rectangle((int)position.X - 2, (int)position.Y, 2, 32);
					break;

				default:
					rect = new Rectangle(0, 0, 0, 0);
					break;
			}

			if (world.Player.CheckCollision(rect))
			{
				world.Player.Damage(1, direction + 2);
				pause = 1;
			}
		}
	}
}
