using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Xml;
using WowDotNetAPI;
using WowDotNetAPI.Models;

namespace BiSComparer
{
	public class BiSComparerModel
	{
		public BiSComparerModel(BiSComparerViewModel bisComparerViewModel)
		{
			m_bisComparerVM = bisComparerViewModel;
		}

		public ObservableCollection<CharInfo> GetCharInfos(string bisFilePath)
		{
			ObservableCollection<CharInfo> charInfos = new ObservableCollection<CharInfo>();
			s_xmlDoc = new XmlDocument();
			s_xmlDoc.Load(bisFilePath);
			XmlNode raidInfo = s_xmlDoc.DocumentElement.SelectSingleNode("/RaidInfo");
			string raidDifficulty = raidInfo.Attributes["Difficulty"].Value;

			bool resetObtained = false;
			if (string.IsNullOrEmpty(Properties.Settings.Default.Difficulty) ||
				!string.Equals(Properties.Settings.Default.Difficulty, raidDifficulty))
			{
				// Raid difficulty has change since the last time xml was loaded.
				Properties.Settings.Default.Difficulty = raidDifficulty;
				Properties.Settings.Default.Save();
				resetObtained = true;
			}

			XmlNodeList characters = s_xmlDoc.DocumentElement.SelectNodes("/RaidInfo/Characters/Character");

			foreach (XmlNode character in characters)
			{
				string charName = character.Attributes["Name"].Value;

				ObservableCollection<Item> bisItems = GetBiSList(character, raidDifficulty, resetObtained, ref s_xmlDoc);
				Character wowCharacter = LoadCharacter(charName);
				ObservableCollection<Item> currentItems = GetCurrentItems(wowCharacter, raidDifficulty);
				List<Item> itemsNeeded = CompareListsBySlot(bisItems, currentItems, charName, raidDifficulty, ref s_xmlDoc);

				CharInfo charInfo = new CharInfo();
				charInfo.CharName = charName;
				charInfo.BisItems = bisItems;
				charInfo.CurrentItems = currentItems;
				charInfo.ItemsNeeded = itemsNeeded;
				charInfo.ItemsNeededCount = itemsNeeded.Count();

				charInfos.Add(charInfo);
				m_bisComparerVM.UpdateProgressBar(charName, characters.Count);
			}

			m_bisComparerVM.ResetProgressBar();

			s_xmlDoc.Save(bisFilePath);
			return charInfos;
		}

		public void SaveBiSList(string bisFilePath, ObservableCollection<CharInfo> charInfos)
		{
			s_xmlDoc.Load(bisFilePath);
			XmlNodeList characters = s_xmlDoc.DocumentElement.SelectNodes("/RaidInfo/Characters/Character");

			foreach (XmlNode character in characters)
			{
				string charName = character.Attributes["Name"].Value;
				CharInfo charInfo = charInfos.Where(c => c.CharName == charName).FirstOrDefault();

				foreach (Item BiSItem in charInfo.BisItems)
				{
					foreach (XmlElement itemElement in character.ChildNodes)
					{
						if (itemElement.Attributes["Slot"].Value == BiSItem.Slot)
						{
							itemElement.Attributes["Name"].Value = BiSItem.Name;
							itemElement.Attributes["Source"].Value = BiSItem.Source;
							itemElement.Attributes["Obtained"].Value = "False";
							break;
						}
					}
				}
			}

			s_xmlDoc.Save(bisFilePath);
		}

		private ObservableCollection<Item> GetBiSList(XmlNode character, string difficulty, bool resetObtained, ref XmlDocument xmlDoc)
		{
			ObservableCollection<Item> items = new ObservableCollection<Item>();
			string charName = character.Attributes["Name"].Value;
			bool obtained = false;
			foreach (XmlElement itemElement in character.ChildNodes)
			{
				string name = itemElement.Attributes["Name"].Value;
				string source = itemElement.Attributes["Source"].Value;
				string slot = itemElement.Attributes["Slot"].Value;
				obtained = Convert.ToBoolean(resetObtained ? "false" : itemElement.Attributes["Obtained"].Value);

				bool isTier = Constants.IsItemTierPiece(source, slot);

				if (resetObtained)
				{
					SetObtained(name, charName, false, ref xmlDoc);
				}

				Item item = new Item(slot, name, source, Constants.GetMinimumIlevel(difficulty, source, isTier), obtained, isWf:false);

				items.Add(item);
			}

			return items;
		}

		public ObservableCollection<BossInfo> GetBossInfos(ObservableCollection<CharInfo> charInfos)
		{
			ObservableCollection<BossInfo> bossInfos = new ObservableCollection<BossInfo>();
			List<Item> tempItems = new List<Item>();

			foreach (CharInfo charInfo in charInfos)
			{
				string charName = charInfo.CharName;
				foreach (Item item in charInfo.ItemsNeeded)
				{
					Item item2 = new Item(item.Slot, item.Name, item.Source, item.Ilevel, item.Obtained, charName);
					tempItems.Add(item2);
				}
			}

			foreach (Item item in tempItems)
			{
				// First item, so create new BossInfo
				if (bossInfos.Count == 0)
				{
					AddItemToBossInfos(item, ref bossInfos);
				}
				else
				{
					// Not first item, so check if BossInfo already exists for item's source
					foreach (BossInfo bossInfo in bossInfos)
					{
						if (bossInfo.BossName == item.Source)
						{
							// BossInfo already exists for the source of this item. Add it.
							bossInfo.ItemsNeeded.Add(item);
							bossInfo.ItemsNeededCount++;
							break;
						}
						else
						{
							// We've already checked all existing and it there isn't an entry for this item source, add it.
							if (bossInfo == bossInfos.Last())
							 {
								AddItemToBossInfos(item, ref bossInfos);
								break;
							}
						}
					}
				}

			}

			return bossInfos;
		}

		private void AddItemToBossInfos(Item item, ref ObservableCollection<BossInfo> bossInfos)
		{
			BossInfo bossInfo = new BossInfo();

			// Theres no boss info for this item, create a new one and add it to bossInfos.
			bossInfo.BossName = item.Source;

			List<Item> items = new List<Item>();
			items.Add(item);
			bossInfo.ItemsNeeded = items;
			bossInfo.ItemsNeededCount++;

			bossInfos.Add(bossInfo);
		}

		private Character LoadCharacter(string charName)
		{
			Character character = new Character();

			try
			{
				character = m_wow.GetCharacter("Darksorrow", charName, CharacterOptions.GetItems);

				if (character.Level < Constants.s_maxLevel)
				{
					throw new Exception(string.Format("Character with name {0} on Darksorrow has a level lower than {1}, checking Genjuros.", charName, Constants.s_maxLevel));
				}
			}
			catch
			{
				try
				{
					character = m_wow.GetCharacter("Genjuros", charName, CharacterOptions.GetItems);

					if (character.Level < Constants.s_maxLevel)
					{
						throw new Exception(string.Format("Character with name {0} on Genjuros has a level lower than {1}, checking Neptulon.", charName, Constants.s_maxLevel));
					}
				}
				catch
				{
					try
					{
						character = m_wow.GetCharacter("Neptulon", charName, CharacterOptions.GetItems);

						if (character.Level < Constants.s_maxLevel)
						{
							throw new Exception(string.Format("Character with name {0} on Genjuros has a level lower than {1}.", charName, Constants.s_maxLevel));
						}
					}
					catch
					{
						throw new Exception(string.Format("Character \"{0}\" could not be found on Darksorrow, Genjuros or Neptulon", charName));
					}
				}
			}

			return character;
		}

		private ObservableCollection<Item> GetCurrentItems(Character character, string raidDifficulty)
		{
			CharacterEquipment equippedItems = character.Items;

			ObservableCollection<Item> currentItems = new ObservableCollection<Item>();

			if (equippedItems != null)
			{
				for (int i=0; i < Constants.s_equipmentSlots.Length; i++)
				{
					// Get the item for each slot. equippedItems.Head, equippedItems.Neck etc.
					CharacterItem item = equippedItems.GetType().GetProperty(Constants.s_equipmentSlots[i]).GetValue(equippedItems) as CharacterItem;
					if (item != null)
					{
						if (item.Name != null)
						{
							bool isWf = false;
							if (Constants.IsWarforged(item.BonusLists.ToList()))
							{
								isWf = true;
							}
							currentItems.Add(new Item(Constants.s_equipmentSlots[i], item.Name, string.Empty, item.ItemLevel, false, isWf));
						}
					}
				}
			}
			return currentItems;
		}
		private List<Item> CompareListsBySlot(ObservableCollection<Item> bisList, ObservableCollection<Item> currentItems, string charName, string raidDifficulty, ref XmlDocument xmlDoc)
		{
			List<Item> itemsNeeded = new List<Item>();
			foreach (string slot in Constants.s_equipmentSlots)
			{
				Item bisItem = bisList.Where(i => i.Slot.ToUpper().Trim() == slot.ToUpper().Trim()).FirstOrDefault();
				Item currentItem = currentItems.Where(i => i.Slot.ToUpper().Trim() == slot.ToUpper().Trim()).FirstOrDefault();

				if (slot.ToUpper().Trim() == Constants.s_offHandSlot)
				{
					if (bisItem == null && currentItem != null)
					{
						// Theres no offhand on the BiS list but theres one currently equipped.
						// Remove offhand from currentItems list.
						currentItems.Remove(currentItem);
					}
					else if (bisItem != null && currentItem == null)
					{
						// Theres an offhand on the BiSlist, but currently there isn't one equipped.
						// Remove offhand from BiSItems list and mark as needed.
						bisList.Remove(bisItem);
						itemsNeeded.Add(bisItem);
					}
				}

				// Check both items were found before comparison.
				if (bisItem != null && currentItem != null)
				{
					bool itemNeeded = false;
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
						}
						else
						{
							// Names are the same, make sure that current Ilevel is greater than expected BiS Ilevel.
							itemNeeded = compareIlevels(currentItem, bisItem);
						}
					}

					if (itemNeeded)
					{
						itemsNeeded.Add(bisItem);
					}
					else
					{
						SetObtained(bisItem.Name, charName, true, ref xmlDoc);
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
				currentIlevel -= Constants.s_wFBonus;
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
			XmlNodeList characters = xmlDoc.DocumentElement.SelectNodes("/RaidInfo/Characters/Character");

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
		private BiSComparerViewModel m_bisComparerVM;
	}
}
