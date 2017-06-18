using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleDotNetAPI.Models.WoW;

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

		public virtual int GetRelicAddedIlevel(WoWItem relic, int difficultyBonus, int ilevelBonus)
		{
			int ilevel = ilevelBonus - 1472;

			// Emerald Nightmare relic bonus Ids:

			// LFR    3379
			// Normal 1807
			// Heroic 1805
			// Mythic 1806
			List<int> emeraldNightmareRaidBonusIds = new List<int>() { 3379, 1807, 1805, 1806 };

			// Nighthold relic bonus Ids:

			// First 3 bosses

			// LFR    3446
			// Normal 3443
			// Heroic 3444
			// Mythic 3445
			List<int> nightholdRaidBonusIds1 = new List<int>() { 3443, 3444, 3445, 3446 };

			// Middle 6 bosses

			// LFR    3520
			// Normal 3514
			// Heroic 3516
			// Mythic 3518
			List<int> nightholdRaidBonusIds2 = new List<int>() { 3520, 3514, 3516, 3518 };

			// Guldan

			// LFR    3521
			// Normal 3515
			// Heroic 3517
			// Mythic 3519
			List<int> nightholdRaidBonusIds3 = new List<int>() { 3521, 3515, 3517, 3519 };


			// TODO - Update these bonus Ids once Wowhead is updated for 7.2.5
			// Tomb of Sargeras bonus Ids:

			// First 3 bosses

			// LFR    3446
			// Normal 3443
			// Heroic 3444
			// Mythic 3445
			List<int> tombBonusIds1 = new List<int>() { 3443, 3444, 3445, 3446 };

			// Middle 6 bosses

			// LFR    3520
			// Normal 3514
			// Heroic 3516
			// Mythic 3518
			List<int> tombRaidBonusIds2 = new List<int>() { 3520, 3514, 3516, 3518 };

			// Guldan

			// LFR    3521
			// Normal 3515
			// Heroic 3517
			// Mythic 3519
			List<int> tombRaidBonusIds3 = new List<int>() { 3521, 3515, 3517, 3519 };

			// These are raid relics, the second bonus id is the ilevel relative to normal,
			// but we only have the LFR ilevel, so add 15 to the base ilevel plus an amount
			// relative to the base ilevel at that point in the raid.
			if (nightholdRaidBonusIds1.Contains(difficultyBonus))
			{
				ilevel += 15 + 5;
			}
			else if (nightholdRaidBonusIds2.Contains(difficultyBonus) || emeraldNightmareRaidBonusIds.Contains(difficultyBonus))
			{
				ilevel += 15 + 0;
			}
			else if (nightholdRaidBonusIds3.Contains(difficultyBonus))
			{
				ilevel += 15 - 5;
			}

			return ilevel;
		}

		#endregion

	}
}
