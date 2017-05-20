using System.Collections.Generic;
using System.Linq;

namespace BiSComparer
{
	public class TombOfSargerasConstants : Constants
	{
		public TombOfSargerasConstants()
		:base()
		{
		}

		#region Expansion Specific Constants

		public override int MaxLevel
		{
			get { return 110; }
		}

		public override int WFBonus
		{
			get { return 6; }
		}

		#endregion

		#region Raid Specific Constants

		public static string s_goroth = "Goroth";
		public static string s_inquisition = "Demonic Inquisition";
		public static string s_harjatan = "Harjatan";
		public static string s_sisters = "Sisters of the Moon";
		public static string s_mistress = "Mistress Sassz'ine";
		public static string s_host = "The Desolate Host";
		public static string s_maiden = "Maiden of Vigilance";
		public static string s_avatar = "Fallen Avatar";
		public static string s_kiljaeden = "Kil'jaeden";

		public override string[] Sources
		{
			get
			{
				string[] sources = new string[] { s_kiljaeden, s_avatar, s_maiden, s_host, s_mistress, s_sisters, s_harjatan, s_inquisition, s_goroth };
				return sources.Concat(base.Sources).ToArray();
			}
		}

		public static string s_relic1Slot = "relic1";
		public static string s_relic2Slot = "relic2";
		public static string s_relic3Slot = "relic3";

		public override string[] EquipmentSlots
		{
			get
			{
				string[] equipmentSlots = new string[] { s_relic1Slot, s_relic2Slot, s_relic3Slot };
				// The expansion specific slots should go at the bottom of the list.
				return base.EquipmentSlots.Concat(equipmentSlots).ToArray();
			}
		}
		#endregion

		public override int GetMinimumIlevel(string raidDifficulty, string source, bool isTier)
		{
			int iLevel = 0;

			if (isTier)
			{
				switch (raidDifficulty)
				{
					case "Mythic":
						iLevel = 935;
						break;
					case "Heroic":
						iLevel = 920;
						break;
					case "Normal":
						iLevel = 905;
						break;
					default:
						iLevel = 890;
						break;
				}
			}
			else
			{
				if (source == s_goroth || source == s_inquisition || source == s_harjatan)
				{
					switch (raidDifficulty)
					{
						case "Mythic":
							iLevel = 930;
							break;
						case "Heroic":
							iLevel = 915;
							break;
						case "Normal":
							iLevel = 900;
							break;
						default:
							iLevel = 885;
							break;
					}
				}

				if (source == s_sisters || source == s_mistress || source == s_host || source == s_maiden || source == s_avatar)
				{
					switch (raidDifficulty)
					{
						case "Mythic":
							iLevel = 935;
							break;
						case "Heroic":
							iLevel = 920;
							break;
						case "Normal":
							iLevel = 905;
							break;
						default:
							iLevel = 890;
							break;
					}
				}

				if (source == s_kiljaeden)
				{
					switch (raidDifficulty)
					{
						case "Mythic":
							iLevel = 940;
							break;
						case "Heroic":
							iLevel = 925;
							break;
						case "Normal":
							iLevel = 910;
							break;
						default:
							iLevel = 895;
							break;
					}
				}

				if (source == s_legendary)
				{
					iLevel = 970;
				}
			}

			return iLevel;
		}

		public override bool IsItemTierPiece(string source, string slot)
		{
			bool isTier = false;
			if (slot == s_headSlot && source == s_inquisition)
			{
				isTier = true;
			}

			if (slot == s_legsSlot && source == s_mistress)
			{
				isTier = true;
			}

			if (slot == s_handsSlot && source == s_harjatan)
			{
				isTier = true;
			}

			if (slot == s_shoulderSlot && source == s_avatar)
			{
				isTier = true;
			}

			if (slot == s_chestSlot && source == s_kiljaeden)
			{
				isTier = true;
			}

			if (slot == s_backSlot && source == s_host)
			{
				isTier = true;
			}

			return isTier;
		}

		public override bool IsWarforged(List<int> BonusIDs)
		{
			// In WoD Mythic, Heroic and Normal WF Ids are 562, 561, 560 respectively
			return (BonusIDs.Contains(562)) || (BonusIDs.Contains(561)) || (BonusIDs.Contains(560));
		}
	}
}
