﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiSComparer
{
	public class Item
	{
		public Item(string slot, string name, string source, string difficulty, bool obtained, bool isWf)
		{
			Slot = slot;
			Name = name;
			Source = source;
			Difficulty = difficulty;
			Obtained = obtained;
			IsWarforged = isWf;

			bool isTier = Constants.IsItemTierPiece(source, slot);
			Ilevel = Constants.GetMinimumIlevel(difficulty, source, isTier);
			Sources = Constants.s_bosses;
			Character = string.Empty;
		}

		public Item(string slot, string name, string source, int iLevel, bool obtained, string character, string difficulty)
		{
			Slot = slot;
			Name = name;
			Source = source;
			Ilevel = iLevel;
			Obtained = obtained;
			Character = character;
			Difficulty = difficulty;
			Sources = Constants.s_bosses;
		}

		public Item(string slot, string name, string source)
		{
			Slot = slot;
			Name = name;
			Source = source;
			Sources = Constants.s_bosses;
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
