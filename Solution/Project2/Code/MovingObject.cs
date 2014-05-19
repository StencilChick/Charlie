// made by Gwendolyn Hart
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Charlie.Code.MapCode;
using Charlie.Code.Characters;

namespace Charlie.Code
{
    // something that takes up space but also moves around a bit
    public abstract class MovingObject : Object
    {
		// reference to the world
		protected World world;

        // direction the thing is facing
        public enum Direction
        {
            down, left, up, right
        }
        Direction _direction;
        public Direction direction { get { return _direction; } }

		// how quickly the object moves in pixels per second
		protected float speed;
		public float Speed { get { return speed; } }

        // how far though an animation the object is
        protected float animIndex;
        public float AnimIndex { get { return animIndex; } }
		float animSpeed;

		// whether this object is walking
		protected bool walking;

        // la-de-da constructor
        public MovingObject(World world, Texture2D image, float speed)
            : base(image)
        {
			this.world = world;

            _direction = Direction.down;
			this.speed = speed;
            animIndex = 0;
			animSpeed = 4f;
        }

		// that thing you call every frame to make this thing move
		public void Update(GameTime gameTime)
		{
			// do animation junk
			if (!walking)
			{
				if (animIndex > 0) { animIndex = 0; }
			}
			else
			{
				animIndex += animSpeed * gameTime.ElapsedGameTime.Milliseconds / 1000;
				if (animIndex >= 4) { animIndex -= 4; }

				imageCoords.X = (int)animIndex;
			}
		}

		// check if the object has collided with another
		protected Object Collide()
		{
			// this is just to give a little lee-way to collisions until collision box sizes are implemented
			int padding = 5;

			// for every tile
			foreach (Tile tile in world.Map.ActiveRoom.Tiles)
			{
				// if a tile is solid, then it is collidable
				if (tile.Solid)
				{
					if (tile.CheckCollision(new Rectangle((int)position.X + padding, (int)position.Y + padding, 32 - padding, 32 - padding)))
					{
						return tile;
					}
				}
			}

			// for every object
			foreach (Object obj in world.Map.ActiveRoom.Objects)
			{
				// the object shouldn't collide with itself; that'd just be silly
				if (obj != this && obj.GetType().BaseType != typeof(Activators.Activator))
				{
					if (obj.CheckCollision(new Rectangle((int)position.X + padding, (int)position.Y + padding, 32 - padding, 32 - padding)))
					{
						return obj;
					}
				}
			}

			// for the player object
			if (world.Player != this)
			{
				if (world.Player.CheckCollision(new Rectangle((int)position.X + padding, (int)position.Y + padding, 32 - padding, 32 - padding)))
				{
					return world.Player;
				}
			}

			// if the function has made it this far, there is no collision
			return null;
		}

		// functions for moving about
		public void MoveRight(GameTime gameTime)
		{
			position.X += speed * gameTime.ElapsedGameTime.Milliseconds / 1000;
			if (Collide() != null)
			{
				position.X -= speed * gameTime.ElapsedGameTime.Milliseconds / 1000;
			}
		}
		public void MoveLeft(GameTime gameTime)
		{
			position.X -= speed * gameTime.ElapsedGameTime.Milliseconds / 1000;
			if (Collide() != null)
			{
				position.X += speed * gameTime.ElapsedGameTime.Milliseconds / 1000;
			}
		}
		public void MoveUp(GameTime gameTime)
		{
			position.Y -= speed * gameTime.ElapsedGameTime.Milliseconds / 1000;
			if (Collide() != null)
			{
				position.Y += speed * gameTime.ElapsedGameTime.Milliseconds / 1000;
			}
		}
		public void MoveDown(GameTime gameTime)
		{
			position.Y += speed * gameTime.ElapsedGameTime.Milliseconds / 1000;
			if (Collide() != null)
			{
				position.Y -= speed * gameTime.ElapsedGameTime.Milliseconds / 1000;
			}
		}

		// moves the object in a certain direction
		public void MoveDirection(GameTime gameTime, Vector2 direction)
		{
			position += direction * speed * gameTime.ElapsedGameTime.Milliseconds / 1000;
		}

		// setting the direction
		public void SetDirection(Direction direction)
		{
			this._direction = direction;
			this.imageCoords.Y = (int)direction;
		}

		// make this object in the closest direction of another object
		public void LookAt(Object obj)
		{
			// get the angle of the vector between this object's and the other object's positions
			Vector2 dist = obj.Position - position;
			double angle = Math.Atan(dist.Y / dist.X);

			// convert the angle into an enumerator direction
			if (obj.Position.X < position.X) { angle += Math.PI; }
			angle /= Math.PI;
			if (angle >= -0.5)
			{
				SetDirection(Direction.up);
			}
			if (angle >= -0.25)
			{
				SetDirection(Direction.right);
			}
			if (angle >= 0.25)
			{
				SetDirection(Direction.down);
			}
			if (angle >= 0.75)
			{
				SetDirection(Direction.left);
			}
			if (angle >= 1.25)
			{
				SetDirection(Direction.up);
			}
		}


		// tell the object to begin the walking animation
		public void AnimateWalk()
		{
			walking = true;
		}

		// tell the object to end animation
		public void StopAnimation()
		{
			walking = false;

			animIndex = 0;
			imageCoords = new Vector2(0, (int)direction);
		}


		// vector values for the relative forward and right of the object
		public Vector2 Up
		{
			get
			{
				switch (direction)
				{
					case Direction.up:
						return new Vector2(0, -1);

					case Direction.right:
						return new Vector2(1, 0);

					case Direction.down:
						return new Vector2(0, 1);

					case Direction.left:
						return new Vector2(-1, 0);

					default:
						return new Vector2(0, 0);
				}
			}
		}
		public Vector2 Right
		{
			get
			{
				_direction++; if ((int)_direction >= Enum.GetValues(typeof(Direction)).Length) { _direction = 0; }
				Vector2 value = Up;
				_direction--; if ((int)_direction < 0) { _direction = (Direction)(Enum.GetValues(typeof(Direction)).Length - 1); }
				return value;
			}
		}
    }
}
