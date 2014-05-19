using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
// Ed Amidon

namespace Charlie.Code.Items
{
	public class Item : Object
	{
        // attributes
		string fileName;
        string name;
		string desc;

		public string FileName
		{
			get { return fileName; }
			set { fileName = value; }
		}

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

		public string Desc
		{
			get { return desc; }
			set { desc = value; }
		}

		public Item(string fileName, string name, Texture2D image, string desc)
			: base(image)
        {
			this.fileName = fileName;
			this.name = name;
			this.desc = desc;
		}

		// for comparing items to eachother
		public override bool Equals(object obj)
		{
			if (obj.GetType() == typeof(Item) || obj.GetType().BaseType == typeof(Item))
			{
				return this.fileName == ((Item)obj).fileName && this.position == ((Object)obj).Position;
			}
			else
			{
				return base.Equals(obj);
			}
		}
	}
}
