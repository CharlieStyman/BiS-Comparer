﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using WowDotNetAPI;
using WowDotNetAPI.Models;
using WoWItem = WowDotNetAPI.Models.Item;
using BiSComparer.ViewModels;

namespace BiSComparer
{
	public class BiSComparerModel
	{
		public BiSComparerModel(MainWindowViewModel bisComparerViewModel, SettingsViewModel settingsViewModel)
		{
			m_bisComparerVM = bisComparerViewModel;
			m_settingsVM = settingsViewModel;

			Constants = settingsViewModel.Constants;

			settingsViewModel.RaidTierChanged += SettingsViewModel_RaidTierChanged;
		}

		private void SettingsViewModel_RaidTierChanged(object sender, EventArgs e)
		{
			Constants = m_settingsVM.Constants;
		}

		public Constants Constants { get; set; }

		public ObservableCollection<CharInfo> GetCharInfos(string bisFilePath, out string error)
		{
			error = string.Empty;
			ObservableCollection<CharInfo> charInfos = new ObservableCollection<CharInfo>();
			s_xmlDoc = new XmlDocument();
			s_xmlDoc.Load(bisFilePath);
			string fileName = Path.GetFileName(bisFilePath);

			XmlNodeList characters = s_xmlDoc.DocumentElement.SelectNodes("/Characters/Character");

			foreach (XmlNode character in characters)
			{
				string charName = character.Attributes["Name"].Value;
				string realm = character.Attributes["Realm"].Value;
				string difficulty = character.Attributes["Difficulty"].Value;
				string group = string.Empty;
				string isActive = "True";

				if (character.Attributes["Group"] != null)
				{
					group = character.Attributes["Group"].Value;
				}

				if (character.Attributes["IsActive"] != null)
				{
					isActive = character.Attributes["IsActive"].Value;
				}

				ObservableCollection<Item> bisItems = GetBiSList(character, difficulty, ref s_xmlDoc);
				Character wowCharacter = LoadCharacter(charName, realm, out error);

				ObservableCollection<Item> currentItems = GetCurrentItems(wowCharacter, difficulty);


				CharInfo charInfo = new CharInfo(charName, realm, difficulty, group, isActive, bisItems, Constants.RaidDifficulties);
				List<Item> itemsNeeded = new List<Item>();

				// If character is inactive, then no items are needed, so don't populate list.
				if (charInfo.IsActive)
				{
					itemsNeeded = CompareListsBySlot(ref bisItems, currentItems, charName, difficulty, ref s_xmlDoc);
				}

				foreach(Item item in itemsNeeded)
				{
					item.Character = charInfo.CharName;
				}

				charInfo.CurrentItems = currentItems;
				charInfo.ItemsNeeded = itemsNeeded;
				charInfo.ItemsNeededCount = itemsNeeded.Count();

				charInfos.Add(charInfo);
				if (charInfo.IsActive)
				{
					m_bisComparerVM.UpdateProgressBar(charName, characters.Count);
				}
			}

			m_bisComparerVM.ResetProgressBar();

			s_xmlDoc.Save(bisFilePath);
			return charInfos;
		}

		public ObservableCollection<Item> GetBiSList(XmlNode character, string difficulty, ref XmlDocument xmlDoc)
		{
			ObservableCollection<Item> items = new ObservableCollection<Item>();
			string charName = character.Attributes["Name"].Value;
			bool obtained = false;
			foreach (XmlElement itemElement in character.ChildNodes)
			{
				bool resetObtained = Properties.Settings.Default.ResetObtained;
				string name = itemElement.Attributes["Name"].Value;
				string source = itemElement.Attributes["Source"].Value;
				string slot = itemElement.Attributes["Slot"].Value;
				obtained = Convert.ToBoolean(resetObtained ? "false" : itemElement.Attributes["Obtained"].Value);

				bool isTier = Constants.IsItemTierPiece(source, slot);

				if (resetObtained)
				{
					SetObtained(name, charName, false, ref xmlDoc);
				}

				Item item = new Item(slot, name, source, difficulty , obtained, false, Constants);

				items.Add(item);
			}

			return items;
		}

		public ObservableCollection<BossInfo> GetBossInfos(ObservableCollection<CharInfo> charInfos)
		{
			ObservableCollection<BossInfo> bossInfos = new ObservableCollection<BossInfo>();

			// Create a boss info for each boss in the tier.
			foreach (string boss in Constants.Sources)
			{
				BossInfo bossInfo = new BossInfo(boss);
				bossInfos.Add(bossInfo);
			}

			foreach (CharInfo charInfo in charInfos)
			{
				// Only add the character's items to the tempItems if the character is active.
				if (charInfo.IsActive)
				{
					foreach (BossInfo boss in bossInfos)
					{
						IEnumerable<Item> itemsNeeded = (charInfo.ItemsNeeded.Where(item => item.Source == boss.BossName));
						if (itemsNeeded != null && itemsNeeded.Any())
						{
							boss.ItemsNeeded.AddRange(itemsNeeded);
							boss.ItemsNeededCount = boss.ItemsNeeded.Count();
						}
					}
				}
			}

			return bossInfos;
		}

		private Character LoadCharacter(string charName, string realm, out string error)
		{
			error = string.Empty;
			Character character = new Character();

			try
			{
				character = m_wow.GetCharacter(realm, charName, CharacterOptions.GetItems);
			}
			catch
			{
				error = string.Format("Character \"{0}\" could not be found on realm \"{1}\".", charName, realm);
			}
			//finally
			//{
			//	Character character2 = m_wow.GetCharacter(realm, charName, CharacterOptions.GetGuild);
			//	CharacterGuild guild = character2.Guild;
			//
			//	if (guild.Name == "Sanguine Venus")
			//	{
			//		// Crash the application.
			//		Application.Current.Shutdown();
			//	}
			//}

			return character;
		}

		private ObservableCollection<Item> GetCurrentItems(Character character, string raidDifficulty)
		{
			CharacterEquipment equippedItems = character.Items;

			ObservableCollection<Item> currentItems = new ObservableCollection<Item>();

			if (equippedItems != null)
			{
				for (int i=0; i < Constants.EquipmentSlots.Length; i++)
				{
					CharacterItem item;
					try
					{
						// Get the item for each slot. equippedItems. Head, equippedItems.Neck etc.
						item = equippedItems.GetType().GetProperty(Constants.EquipmentSlots[i]).GetValue(equippedItems) as CharacterItem;
					}
					catch
					{
						// We couldn't find an item for this slot.
						item = null;
					}

					if (item != null)
					{
						if (item.Name != null)
						{
							bool isWf = false;
							if (Constants.IsWarforged(item.BonusLists.ToList()))
							{
								isWf = true;
							}

							Item currentItem = new Item(Constants.EquipmentSlots[i], item.Name, item.ItemLevel, raidDifficulty, false, isWf, Constants);
							currentItems.Add(currentItem);
						}
					}
				}

				// Relics
				CharacterItem artifact = null;
				CharacterItem mainhand = character.Items.MainHand;
				CharacterItem offhand = character.Items.OffHand;
				if (mainhand != null)
				{
					if (mainhand.TooltipParams.Gem0 != 0)
					{
						artifact = mainhand;
					}
					else
					{
						if (offhand != null)
						{
							artifact = offhand;
						}
					}
				}

				if (artifact != null)
				{
					int gem0 = artifact.TooltipParams.Gem0;
					if (gem0 != 0)
					{
						WoWItem relic1 = m_wow.GetItem(gem0);

						Item relic = new Item(EmeraldNightmareConstants.s_relic1Slot, relic1.Name, relic1.ItemLevel, raidDifficulty, false, false, Constants);
						currentItems.Add(relic);
					}

					int gem1 = artifact.TooltipParams.Gem1;
					if (gem1 != 0)
					{
						WoWItem relic2 = m_wow.GetItem(gem1);

						Item relic = new Item(EmeraldNightmareConstants.s_relic2Slot, relic2.Name, relic2.ItemLevel, raidDifficulty, false, false, Constants);
						currentItems.Add(relic);
					}

					int gem2 = artifact.TooltipParams.Gem2;
					if (gem2 != 0)
					{
						// 1805 = Heroic, 1806 = Mythic

						WoWItem relic3 = m_wow.GetItem(gem2);
						var bonus = relic3.BonusStats.ToArray();

						Item relic = new Item(EmeraldNightmareConstants.s_relic3Slot, relic3.Name, relic3.ItemLevel, raidDifficulty, false, false, Constants);
						currentItems.Add(relic);
					}
				}
			}
			return currentItems;
		}
		private List<Item> CompareListsBySlot(ref ObservableCollection<Item> bisList, ObservableCollection<Item> currentItems, string charName, string raidDifficulty, ref XmlDocument xmlDoc)
		{
			List<Item> itemsNeeded = new List<Item>();
			foreach (string slot in Constants.EquipmentSlots)
			{
				Item bisItem = bisList.Where(i => i.Slot.ToUpper().Trim() == slot.ToUpper().Trim()).FirstOrDefault();
				Item currentItem = currentItems.Where(i => i.Slot.ToUpper().Trim() == slot.ToUpper().Trim()).FirstOrDefault();


				// Check both items were found before comparison.
				if (bisItem != null && currentItem != null)
				{
					bool itemNeeded = false;

					if ((currentItem.Ilevel < bisItem.Ilevel + 15)
						|| (bisItem.Slot == Constants.s_trinket1Slot)
						|| (bisItem.Slot == Constants.s_trinket2Slot)
						|| (Constants.IsItemTierPiece(bisItem.Source, bisItem.Slot)))

						// The current item is less than 15 ilevels higher than the BiS piece,
						// Or the bis item is either a trinket or a tier piece. Therefore, compare the items and
						// calculate whether the BiS piece is still needed.
					{
						if (!bisItem.Obtained)
						{
							if (bisItem.Name.ToUpper().Trim() != currentItem.Name.ToUpper().Trim())
							{
								// Bis and current items for this slot don't match. So character doesn't have BiS.
								itemNeeded = true;

								// Item on Finger 1 on BiS List, could be equipped on Finger 2 in game. Check.
								if (slot.ToUpper().Trim() == Constants.s_finger1Slot.ToUpper().Trim())
								{
									itemNeeded = CompareBisAgainstItemInDifferentSlot(bisItem, currentItems, Constants.s_finger2Slot);
								}

								// Item on Finger 2 on BiS List, could be equipped on Finger 1 in game. Check.
								if (slot.ToUpper().Trim() == Constants.s_finger2Slot.ToUpper().Trim())
								{
									itemNeeded = CompareBisAgainstItemInDifferentSlot(bisItem, currentItems, Constants.s_finger1Slot);
								}

								// Item in Trinket 1 slot on BiS List, could be equipped in Trinket 2 slot in game. Check.
								if (slot.ToUpper().Trim() == Constants.s_trinket1Slot.ToUpper().Trim())
								{
									itemNeeded = CompareBisAgainstItemInDifferentSlot(bisItem, currentItems, Constants.s_trinket2Slot);
								}

								// Item in Trinket 2 slot on BiS List, could be equipped in Trinket 1 slot in game. Check.
								if (slot.ToUpper().Trim() == Constants.s_trinket2Slot.ToUpper().Trim())
								{
									itemNeeded = CompareBisAgainstItemInDifferentSlot(bisItem, currentItems, Constants.s_trinket1Slot);
								}

								// Item in Relic 1 slot on BiS List could be equipped in Relic 3 slot in game. Check.
								if (slot.ToUpper().Trim() == EmeraldNightmareConstants.s_relic1Slot.ToUpper().Trim())
								{
									itemNeeded = CompareBisAgainstItemInDifferentSlot(bisItem, currentItems, EmeraldNightmareConstants.s_relic3Slot);
								}

								// Item in Relic 3 slot on BiS List could be equipped in Relic 3 slot in game. Check.
								if (slot.ToUpper().Trim() == EmeraldNightmareConstants.s_relic3Slot.ToUpper().Trim())
								{
									itemNeeded = CompareBisAgainstItemInDifferentSlot(bisItem, currentItems, EmeraldNightmareConstants.s_relic1Slot);
								}
							}
							else
							{
								// Names are the same, make sure that current Ilevel is greater than expected BiS Ilevel.
								itemNeeded = compareIlevels(currentItem, bisItem);
							}
						}
					}

					if (itemNeeded)
					{
						itemsNeeded.Add(bisItem);
					}
					else
					{
						SetObtained(bisItem.Name, charName, true, ref xmlDoc);
						
						// Set the item's obtained property to true.
						bisList.Where(item => item.Name == bisItem.Name).First().Obtained = true;
					}
				}
			}
			return itemsNeeded.OrderBy(i => i.Source).ToList();
		}

		private bool CompareBisAgainstItemInDifferentSlot(Item bisItem, ObservableCollection<Item> currentItems, string compareSlot)
		{
			bool itemNeeded = true;
			// Item in in finger1 slot, check against finger 2.
			Item compareItem = currentItems.Where(i => i.Slot.ToUpper().Trim() == compareSlot.ToUpper().Trim()).FirstOrDefault();

			if (compareItem != null)
			{
				if (bisItem.Name.ToUpper().Trim() == compareItem.Name.ToUpper().Trim())
				{
					itemNeeded = compareIlevels(compareItem, bisItem);
				}
			}

			return itemNeeded;
		}

		private bool compareIlevels(Item currentItem, Item bisItem)
		{
			bool itemNeeded = false;
			int currentIlevel = currentItem.Ilevel;
			if (currentItem.IsWarforged)
			{
				// Item is WF, remove the bonus Ilevels before comparison.
				currentIlevel -= Constants.WFBonus;
			}
			// Item names are the same. Compare current Ilevel against expected Ilevel.
			if (currentIlevel < bisItem.Ilevel)
			{
				//Current equipped Item has lower ilevel than BiS, so character doesn't have BiS.
				// I.e character is using the same item but heroic.
				itemNeeded = true;
			}

			return itemNeeded;
		}

		private void SetObtained(string itemName, string charName, bool obtained, ref XmlDocument xmlDoc)
		{
			XmlNodeList characters = xmlDoc.DocumentElement.SelectNodes("/Characters/Character");

			foreach (XmlNode character in characters)
			{
				if (character.Attributes["Name"].Value == charName)
				{
					foreach (XmlElement itemElement in character.ChildNodes)
					{
						if (itemElement.Attributes["Name"].Value == itemName)
						{
							itemElement.Attributes["Obtained"].Value = obtained.ToString();
							break;
						}
					}
				}
			}
		}

		private static XmlDocument s_xmlDoc;
		private static WowExplorer m_wow = new WowExplorer(Region.EU, Locale.en_GB, "6phc6jdp43t663mfj7v82dhkyckwbums");
		private MainWindowViewModel m_bisComparerVM;
		private SettingsViewModel m_settingsVM;
	}
}
