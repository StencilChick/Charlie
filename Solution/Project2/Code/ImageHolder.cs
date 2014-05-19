// made by Gwendolyn Hart
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Charlie.Code
{
    // thing that holds all-the-dang graphics in the game
    public class ImageHolder
    {
        // mess o' tilesets
        Dictionary<string, Texture2D> tilesets;
        public Dictionary<string, Texture2D> Tilesets { get { return tilesets; } }

        // mess o' character sheets
        Dictionary<string, Texture2D> characterSheets;
        public Dictionary<string, Texture2D> CharacterSheets { get { return characterSheets; } }

        // mess o' GUI mess
        Dictionary<string, Texture2D> gui;
        public Dictionary<string, Texture2D> GUI { get { return gui; } }

		// mess o' items
		Dictionary<string, Texture2D> items;
		public Dictionary<string, Texture2D> Items { get { return items; } }

		// mess o' projectiles
		Dictionary<string, Texture2D> projectiles;
		public Dictionary<string, Texture2D> Projectiles { get { return projectiles; } }

        // mess o' fonts
        Dictionary<string, SpriteFont> fonts;
        public Dictionary<string, SpriteFont> Fonts { get { return fonts; } }

        
        // constructor
		public ImageHolder()
		{
			// do nothing because everything is done later by LoadImages
		}

        // load all the images
        // (to be called in the LoadContent function of the World object
        public void LoadImages(World world)
        {
            // load the tilesets
            tilesets = LoadImageFiles(world, "Images/Tilesets");
            // load the character sheets
			characterSheets = LoadImageFiles(world, "Images/Characters");
            // load the gui stuff
            gui = LoadImageFiles(world, "Images/GUI");
			// load all the itmes
			items = LoadImageFiles(world, "Images/Items");
			// load all the projectiles
			projectiles = LoadImageFiles(world, "Images/Projectiles");
            // load the fonts
            fonts = LoadFontFiles(world, "Images/Fonts");
        }

        // creates a dictionary holding all the image files in a given directory
        private Dictionary<string, Texture2D> LoadImageFiles(World world, string dir)
        {
            // the dictionary to be returned at the end
            Dictionary<string, Texture2D> images = new Dictionary<string, Texture2D>();

            // get the names of all the files
            List<string> fileNames = new List<string>();
            foreach (string filePath in Directory.GetFiles(Directory.GetCurrentDirectory() + "/Content/" + dir))
            {
                fileNames.Add(Path.GetFileNameWithoutExtension(filePath));
            }

            // load all the files into the dictionary
            foreach (string name in fileNames)
            {
                images.Add(name, world.Content.Load<Texture2D>(dir + "/" + name));
            }

            // return the populated dictionary
            return images;
        }

        // creates a dictionary holding all the sprite font files in a given directory
        private Dictionary<string, SpriteFont> LoadFontFiles(World world, string dir)
        {
            // the dictionary to be returned at the end
            Dictionary<string, SpriteFont> fonts = new Dictionary<string, SpriteFont>();

            // get the names of all the files
            List<string> fileNames = new List<string>();
            foreach (string filePath in Directory.GetFiles(Directory.GetCurrentDirectory() + "/Content/" + dir))
            {
                fileNames.Add(Path.GetFileNameWithoutExtension(filePath));
            }

            // load all the files into the dictionary
            foreach (string name in fileNames)
            {
                fonts.Add(name, world.Content.Load<SpriteFont>(dir + "/" + name));
            }

            // return the populated dictionary
            return fonts;
        }
    }
}
