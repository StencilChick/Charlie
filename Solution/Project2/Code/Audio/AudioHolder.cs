// Gwendolyn Hart
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Charlie.Code.Audio
{
	public class AudioHolder
	{
		// reference to the world
		World world;

		// the songs that play in the background
		Dictionary<string, SoundEffect> songs;
		public Dictionary<string, SoundEffect> Songs { get { return songs; } }
		// the sound effects that play when sound effecty things happen
		Dictionary<string, SoundEffect> sounds;
		public Dictionary<string, SoundEffect> Sounds { get { return sounds; } }

		// constructor
		public AudioHolder(World world)
		{
			this.world = world;
		}

		// function for loading all the audio files
		public void LoadContent()
		{
			songs = LoadFiles("Songs");
			sounds = LoadFiles("Sounds");
		}

		// load sound files
		private Dictionary<string, SoundEffect> LoadFiles(string folder)
		{
			// value to return at the end
			Dictionary<string, SoundEffect> value = new Dictionary<string, SoundEffect>();

			// get a list of all the song files
			string directory = Directory.GetCurrentDirectory() + "/Content/Audio/" + folder + "/";
			foreach (string filename in Directory.GetFiles(directory))
			{
				using (var stream = TitleContainer.OpenStream(filename))
				{
					try
					{
						value.Add(
							Path.GetFileNameWithoutExtension(filename),
							SoundEffect.FromStream(stream));
					}
					catch { }
				}
			}

			// the value to return
			return value;
		}
	}
}
