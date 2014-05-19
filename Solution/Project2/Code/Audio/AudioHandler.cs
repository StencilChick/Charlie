// Gwendolyn Hart
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Charlie.Code.Audio
{
	public class AudioHandler
	{
		// reference to the world
		World world;

		// the currently playing song
		SoundEffectInstance song;
		string songName;

		// constructor
		public AudioHandler(World world)
		{
			this.world = world;

			song = null;
			songName = "";
		}

		// plays a named song
		public void PlaySong(string name)
		{
			// see if the named song is valid
			if (world.AudioHolder.Songs.ContainsKey(name))
			{
				// only create a new song if it is not the same as the current song
				if (name != songName)
				{
					// create an instance of the song for the purpose of looping it forever
					if (song != null) { song.Stop(); }
					song = world.AudioHolder.Songs[name].CreateInstance();
					song.IsLooped = true;
					song.Play();

					songName = name;
				}
			}
			// this is not a valid song, ergo no song is to be played
			else if (song != null)
			{
				song.Stop();
				song = null;

				songName = "";
			}
		}

		// plays a named sound effect
		public void PlaySound(string name)
		{
			// see if named sound effect is valid
			if (world.AudioHolder.Sounds.ContainsKey(name))
			{
				// play the sound effect once
				world.AudioHolder.Sounds[name].Play();
			}
		}

		// returns true if a song is currently playing
		public bool IsPlayingSong
		{
			get { return song != null; }
		}
	}
}
