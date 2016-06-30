using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiSComparer
{
	public class Item
	{
		/// <summary>
		/// Constructor for BiS List item
		/// </summary>
		public Item(string slot, string name, string source, string difficulty, bool obtained, bool isWf, Constants constants)
		{
			Slot = slot;
			Name = name;
			Source = source;
			Difficulty = difficulty;
			Obtained = obtained;
			IsWarforged = isWf;

			bool isTier = constants.IsItemTierPiece(source, slot);
			Ilevel = constants.GetMinimumIlevel(difficulty, source, isTier);
			Sources = constants.Sources;
			Character = string.Empty;
		}

		/// <summary>
		/// Constructor for current Items.
		/// </summary>
		public Item(string slot, string name, int iLevel, string difficulty, bool obtained, bool isWf, Constants constants)
		{
			Slot = slot;
			Name = name;
			Ilevel = iLevel;
			Difficulty = difficulty;
			Obtained = obtained;
			IsWarforged = isWf;
			Sources = constants.Sources;
			Character = string.Empty;
		}

		/// <summary>
		/// Constructor for BossInfos
		/// </summary>
		public Item(string slot, string name, string source, int iLevel, bool obtained, string character, string difficulty, Constants constants)
		{
			Slot = slot;
			Name = name;
			Source = source;
			Ilevel = iLevel;
			Obtained = obtained;
			Character = character;
			Difficulty = difficulty;
			Sources = constants.Sources;
		}

		///<summary>
		/// Constructor for adding Items to BiS list.
		/// </summary>
		public Item(string slot, string name, string source, Constants constants)
		{
			Slot = slot;
			Name = name;
			Source = source;
			Sources = constants.Sources;
		}

		public string Name { get; set; }

		public string Source { get; set; }

		public string Slot { get; set; }

		public int Ilevel { get; set; }

		public string Difficulty { get; set; }

		public bool Obtained { get; set; }

		public string Character { get; set; }

		public bool IsWarforged { get; set; }

		public string[] Sources { get; set; }
	}
}
