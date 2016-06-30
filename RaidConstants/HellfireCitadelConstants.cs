using System.Collections.Generic;

namespace BiSComparer
{
	public class HellfireCitadelConstants : Constants
	{
		public HellfireCitadelConstants()
		:base()
		{
		}

		#region Expansion Specific Constants

		public override int MaxLevel
		{
			get { return 100; }
		}

		public override int WFBonus
		{
			get { return 6; }
		}

		#endregion

		#region Raid Specific Constants

		public static string s_hellfireAssault = "Hellfire Assault";
		public static string s_ironReaver = "Iron Reaver";
		public static string s_kormrok = "Kormrok";
		public static string s_hellfireHighCouncil = "Hellfire High Council";
		public static string s_kilrogg = "Kilrogg Deadeye";
		public static string s_gorefiend = "Gorefiend";
		public static string s_iskar = "Shadow-Lord Iskar";
		public static string s_zakuun = "Fel Lord Zakuun";
		public static string s_xhulHorac = "Xhul'horac";
		public static string s_socrethar = "Socrethar";
		public static string s_tyrantVelhari = "Tyrant Velhari";
		public static string s_mannoroth = "Mannoroth";
		public static string s_archimonde = "Archimonde";
		public static string s_quest = "Quest";
		public static string s_mythicDungeon = "Mythic Dungeon";
		public static string s_trash = "Trash";
		public static string s_crafted = "Crafted";

		public override string[] Sources
		{
			get { return new string[] { s_hellfireAssault, s_ironReaver, s_kormrok, s_hellfireHighCouncil, s_kilrogg, s_gorefiend, s_iskar, s_zakuun, s_xhulHorac, s_socrethar, s_tyrantVelhari, s_mannoroth, s_archimonde, s_quest, s_mythicDungeon, s_trash, s_crafted }; }
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
						iLevel = 725;
						break;
					case "Heroic":
						iLevel = 710;
						break;
					default:
						iLevel = 695;
						break;
				}
			}
			else
			{

				if (source == s_hellfireAssault || source == s_ironReaver || source == s_kormrok || source == s_hellfireHighCouncil)
				{
					switch (raidDifficulty)
					{
						case "Mythic":
							iLevel = 720;
							break;
						case "Heroic":
							iLevel = 705;
							break;
						default:
							iLevel = 690;
							break;
					}
				}

				if (source == s_kilrogg || source == s_gorefiend || source == s_iskar || source == s_socrethar || source == s_trash)
				{
					switch (raidDifficulty)
					{
						case "Mythic":
							iLevel = 725;
							break;
						case "Heroic":
							iLevel = 710;
							break;
						default:
							iLevel = 695;
							break;
					}
				}

				if (source == s_zakuun || source == s_xhulHorac || source == s_tyrantVelhari || source == s_mannoroth)
				{
					switch (raidDifficulty)
					{
						case "Mythic":
							iLevel = 730;
							break;
						case "Heroic":
							iLevel = 715;
							break;
						default:
							iLevel = 700;
							break;
					}
				}

				if (source == s_archimonde)
				{
					switch (raidDifficulty)
					{
						case "Mythic":
							iLevel = 735;
							break;
						case "Heroic":
							iLevel = 720;
							break;
						default:
							iLevel = 705;
							break;
					}
				}

				if (source == s_quest)
				{
					return iLevel = 735;
				}

				if (source == s_mythicDungeon)
				{
					return iLevel = 725;
				}

				if (source == s_crafted)
				{
					return iLevel = 715;
				}
			}

			return iLevel;
		}

		public override bool IsItemTierPiece(string source, string slot)
		{
			bool isTier = false;
			if (slot == s_headSlot && source == s_kormrok)
			{
				isTier = true;
			}

			if (slot == s_legsSlot && source == s_gorefiend)
			{
				isTier = true;
			}

			if (slot == s_handsSlot && source == s_socrethar)
			{
				isTier = true;
			}

			if (slot == s_shoulderSlot && source == s_xhulHorac)
			{
				isTier = true;
			}

			if (slot == s_chestSlot && source == s_mannoroth)
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
