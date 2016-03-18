using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiSComparer
{
	public class Item
	{
		public Item(string slot, string name, string source, int iLevel, bool obtained, bool isWf)
		{
			Slot = slot;
			Name = name;
			Source = source;
			Ilevel = iLevel;
			Obtained = obtained;
			Character = string.Empty;
			IsWarforged = isWf;
			Sources = Constants.s_bosses;
		}

		public Item(string slot, string name, string source, int iLevel, bool obtained, string character)
		{
			Slot = slot;
			Name = name;
			Source = source;
			Ilevel = iLevel;
			Obtained = obtained;
			Character = character;
			Sources = Constants.s_bosses;
		}

		public string Name { get; set; }
		public string Source { get; set; }

		public string Slot { get; set; }

		public int Ilevel { get; set; }

		public bool Obtained { get; set; }

		public string Character { get; set; }

		public bool IsWarforged { get; set; }

		public string[] Sources { get; set; }
	}
}
