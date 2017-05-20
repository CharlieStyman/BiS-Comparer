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

			Constants = constants;
			IsTier = constants.IsItemTierPiece(source, slot);
			Ilevel = constants.GetMinimumIlevel(difficulty, source, IsTier);
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
			Constants = constants;
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
			Constants = constants;
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
			Constants = constants;
			Sources = constants.Sources;
		}

		public string Name { get; set; }

		public string Source { get; set; }

		public string Slot { get; set; }

		public int Ilevel { get; set; }

		public string Difficulty { get; set; }

		public bool Obtained { get; set; }

		public bool ObtainedLFR { get; set; }

		public bool ObtainedNormal { get; set; }

		public bool ObtainedHeroic { get; set; }

		public bool ObtainedMythic { get; set;}

		public string Character { get; set; }

		public bool IsWarforged { get; set; }

		public bool IsTier { get; set; }

		public string[] Sources { get; set; }

		public Constants Constants { get; set; }

		public void SetObtainedDifficulties(int itemLevel)
		{
			ObtainedMythic = (itemLevel >= Constants.GetMinimumIlevel("Mythic", Source, IsTier));
			ObtainedHeroic = (itemLevel >= Constants.GetMinimumIlevel("Heroic", Source, IsTier));
			ObtainedNormal = (itemLevel >= Constants.GetMinimumIlevel("Normal", Source, IsTier));
			ObtainedLFR = (itemLevel >= Constants.GetMinimumIlevel("Raid Finder", Source, IsTier));
		}
	}
}
