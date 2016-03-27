using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiSComparer
{
	public class CharInfo
	{
		public CharInfo(string name, string realm, string difficulty, ObservableCollection<Item> bisItems)
		{
			CharName = name;
			Realm = realm;
			Difficulty = difficulty;
			BisItems = bisItems;

			Difficulties = Constants.s_raidDifficulties;
		}

		public string CharName { get; set; }

		public string Realm { get; set; }

		public string Difficulty { get; set; }

		public string[] Difficulties { get; set; }

		public ObservableCollection<Item> BisItems { get; set; }

		public ObservableCollection<Item> CurrentItems { get; set; }

		public List<Item> ItemsNeeded { get; set; }

		public int ItemsNeededCount{ get; set;}
	}
}
