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

using Charlie.Code.MapCode;

namespace Charlie.Code.Characters
{
	public abstract class Character : MovingObject
	{
		// the name of the file by which this character was defined
		string filename;
		public string FileName { get { return filename; } }

		// an ID for the character to be called by the dialogue
		string id;
		public string ID { get { return id; } set { id = value; } }

		
		// values for health
		protected int health;
		public int Health 
		{ 
			get { return health; }
			set
			{
				if (value > maxHealth)
				{
					health = maxHealth;
				}
				else if (value < 0)
				{
					health = 0;
				}
				else
				{
					health = value;
				}
			}
		}

		protected int maxHealth;
		public int MaxHealth { get { return maxHealth; } set { maxHealth = value; } }

		// values for mana
		protected float mana;
		public int Mana 
		{ 
			get { return (int)mana; }
			set
			{
				if (value > maxMana)
				{
					mana = maxMana;
				}
				else if (value < 0)
				{
					mana = 0;
				}
				else
				{
					mana = value;
				}
			}
		}

		protected int maxMana;
		public int MaxMana { get { return maxMana; } set { maxMana = value; } }

		// speed of mana recovery in units per second
		protected float manaRecovery;

		// for the character being stunned from getting damaged
		protected float stunTime;


		// value for attacking animation
		protected bool attacking = false;
		protected enum AttackAnim
		{
			melee, ranged, magic
		};
		protected AttackAnim attackAnim = AttackAnim.melee;

		// ze constructor
		public Character(World world, string filename, string id, Texture2D image, float speed, int health, int mana)
			: base(world, image, speed)
		{
			this.filename = filename;
			this.id = id;

			// set the health
			this.health = health;
			this.maxHealth = health;

			// set the mana
			this.mana = mana;
			this.maxMana = mana;
			manaRecovery = 0.4f;

			// other
			stunTime = 0;
		}

		public void Update(GameTime gameTime)
		{
			// update the base
			base.Update(gameTime);

			// recover mana
			if (mana < maxMana) { mana += manaRecovery * gameTime.ElapsedGameTime.Milliseconds / 1000; }

			// do the being stunned thing
			if (stunTime > 0)
			{
				walking = false;
				attacking = false;

				// move the character in the appropriate direction
				float oldspeed = speed;
				speed = 2.5f * 32;
				switch (direction)
				{
					case Direction.up:
						MoveDown(gameTime);
						break;

					case Direction.down:
						MoveUp(gameTime);
						break;

					case Direction.right:
						MoveLeft(gameTime);
						break;

					case Direction.left:
						MoveRight(gameTime);
						break;
				}
				speed = oldspeed;

				// subtract form the stun time remaining
				stunTime -= gameTime.ElapsedGameTime.Milliseconds * 1.0f / 1000;
			}
			else
			{
				// do anaimation thing for attacks
				if (attacking)
				{
					// walking being true tells movingObject to go through the steps of animation
					walking = true;

					// see if the animation is done yet
					if (animIndex > 2)
					{
						// animation is done, so stop with the attacking
						animIndex = 0;
						attacking = false;
					}
					else
					{
						// push the animation rightward to the appropriate X-value for the attack
						imageCoords.X = 4 + (int)attackAnim;
					}
				}
			}
		}

		// damage the character for a certain amount of damage
		public void Damage(int value, Direction dir)
		{
			if (stunTime <= 0 || this.GetType() == typeof(Monster))
			{
				health -= value;
				if (health <= 0)
				{
					Destroy();
				}
				else
				{
					stunTime = 0.5f;
					attacking = false;

					if ((int)dir >= Enum.GetValues(typeof(Direction)).Length)
					{
						dir = (Direction)((int)dir - Enum.GetValues(typeof(Direction)).Length);
					}
					SetDirection(dir);
				}
			}
		}

		// destroy this character
		public void Destroy()
		{
			if (this.GetType() != typeof(Player))
			{
				world.Map.ActiveRoom.RemoveObject(this);
			}
			else
			{
				if (!world.Editor.Testing)
				{
					world.gameState = World.GameState.end;
					world.AudioHandler.PlaySound("Death");
					world.AudioHandler.PlaySong("");
				}
				else
				{
					world.gameState = World.GameState.editor;
					world.Editor.Testing = false;

					// reset all these things
					world.Player.ClearDialogueVariables();
					world.Player.ClearTakenItems();
					world.Dialogue = null;

					world.AudioHandler.PlaySong("");

					// reload the map in order to set its values to default (killed enemies are unkilled, activators unactivated, etc.)
					world.Map = new Map(world, world.Editor.FileName, world.Map.ActiveRoomPos);
				}
			}
		}


		// move toward a given position
		public void MoveToward(GameTime gameTime, Vector2 pos)
		{
			Vector2 origin = position + new Vector2(16, 16);

			if (pos.X < origin.X - 13)
			{
				MoveLeft(gameTime);
			}
			else if (pos.X > origin.X + 13)
			{
				MoveRight(gameTime);
			}

			if (pos.Y < origin.Y - 13)
			{
				MoveUp(gameTime);
			}
			else if (pos.Y > origin.Y + 13)
			{
				MoveDown(gameTime);
			}
		}
	}
}
