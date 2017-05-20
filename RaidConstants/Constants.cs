using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiSComparer
{
	public class Constants
	{
		public Constants()
		{
		}

		#region Expansion Specific Constants
		
		public virtual int MaxLevel { get; set; }

		public virtual int WFBonus { get; set; }

		#endregion

		public static string s_quest = "Quest";
		public static string s_mythicDungeon = "Mythic Dungeon";
		public static string s_trash = "Trash";
		public static string s_crafted = "Crafted";
		public static string s_legendary = "Legendary";

		public virtual string[] Sources
		{
			get { return new string[] { s_quest, s_mythicDungeon, s_trash, s_crafted, s_legendary }; }
		}

		#region WoW Specific Constants

		// DEVNOTE: These constants relate to the properties on the 
		// WowAPI CharacterEquipment class, and the slots in the BiS XML.
		// DO NOT CHANGE WITHOUT GOOD REASON

		public static string s_headSlot = "head";
		public static string s_neckSlot = "neck";
		public static string s_shoulderSlot = "shoulder";
		public static string s_backSlot = "back";
		public static string s_chestSlot = "chest";
		public static string s_wristSlot = "wrist";
		public static string s_handsSlot = "hands";
		public static string s_waistSlot = "waist";
		public static string s_legsSlot = "legs";
		public static string s_feetSlot = "feet";
		public static string s_finger1Slot = "finger1";
		public static string s_finger2Slot = "finger2";
		public static string s_trinket1Slot = "trinket1";
		public static string s_trinket2Slot = "trinket2";


		public virtual string[] EquipmentSlots
		{
			get { return new string[] { s_headSlot, s_neckSlot, s_shoulderSlot, s_backSlot, s_chestSlot, s_wristSlot, s_handsSlot, s_waistSlot, s_legsSlot, s_feetSlot, s_finger1Slot, s_finger2Slot, s_trinket1Slot, s_trinket2Slot}; }
		}

		public static string s_preRaid = "PreRaid";
		public static string s_normal = "Normal";
		public static string s_heroic = "Heroic";
		public static string s_mythic = "Mythic";

		public virtual string[] RaidDifficulties
		{
			get { return new string[] { s_preRaid, s_normal, s_heroic, s_mythic }; }
		}

		#endregion

		#region Methods

		public virtual int GetMinimumIlevel(string raidDifficulty, string source, bool isTier)
		{
			return 0;
		}

		public virtual bool IsItemTierPiece(string source, string slot)
		{
			return false;
		}

		public virtual bool IsWarforged(List<int> BonusIDs)
		{
			return false;
		}

		#endregion

	}
}
