using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiSComparer
{
	public class BossInfo
	{
		public BossInfo(string name)
		{
			BossName = name;
			ItemsNeeded = new List<Item>();
			ItemsNeededCount = 0;
		}

		public string BossName { get; set; }

		public List<Item> ItemsNeeded { get; set; }

		public int ItemsNeededCount { get; set; }
	}
}
