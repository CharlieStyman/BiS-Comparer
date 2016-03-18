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
		public string CharName { get; set; }

		public ObservableCollection<Item> BisItems { get; set; }

		public ObservableCollection<Item> CurrentItems { get; set; }

		public List<Item> ItemsNeeded { get; set; }

		public int ItemsNeededCount{ get; set;}
	}
}
