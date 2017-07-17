using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;
using BattleDotNetAPI;
using BattleDotNetAPI.Models.WoW;
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

		public ObservableCollection<CharInfo> CharInfos {get; set;}

		public void LoadCharInfosFromFile(string bisFilePath, out string difficulty, out string error)
		{
			error = string.Empty;
			difficulty = string.Empty;
			ObservableCollection<CharInfo> charInfos = new ObservableCollection<CharInfo>();
			s_xmlDoc = new XmlDocument();
			s_xmlDoc.Load(bisFilePath);
			string fileName = Path.GetFileName(bisFilePath);

			XmlNodeList characters = s_xmlDoc.DocumentElement.SelectNodes("/Characters/Character");

			foreach (XmlNode character in characters)
			{
				string charName = character.Attributes["Name"].Value;
				string realm = character.Attributes["Realm"].Value;
				string group = string.Empty;
				string isActive = "True";
				difficulty = character.Attributes["Difficulty"].Value;

				if (character.Attributes["Group"] != null)
				{
					group = character.Attributes["Group"].Value;
				}

				if (character.Attributes["IsActive"] != null)
				{
					isActive = character.Attributes["IsActive"].Value;
				}

				ObservableCollection<Item> bisItems = GetBiSList(character, difficulty, ref s_xmlDoc);
				WoWCharacter wowCharacter = LoadCharacter(charName, realm, out error);

				ObservableCollection<Item> currentItems = GetCurrentItems(wowCharacter, difficulty);


				CharInfo charInfo = new CharInfo(charName, realm, difficulty, group, isActive, bisItems, currentItems, Constants.RaidDifficulties);
				charInfos.Add(charInfo);

				if (charInfo.IsActive)
				{
					m_bisComparerVM.UpdateProgressBar(charInfo.CharName, characters.Count);
				}
			}

			CharInfos = charInfos;
			m_bisComparerVM.ResetProgressBar();
		}


		public void UpdateItemsNeeded(string difficulty, bool ignoreObtained)
		{
			foreach (CharInfo charInfo in CharInfos)
			{
				List<Item> itemsNeeded = new List<Item>();

				// If character is inactive, then no items are needed, so don't populate list.
				if (charInfo.IsActive)
				{
					itemsNeeded = CompareListsBySlot(charInfo.BisItems, charInfo.CurrentItems, charInfo.CharName, difficulty, ignoreObtained, ref s_xmlDoc);
				}

				foreach (Item item in itemsNeeded)
				{
					item.Character = charInfo.CharName;
				}

				charInfo.ItemsNeeded = itemsNeeded;
				charInfo.ItemsNeededCount = itemsNeeded.Count();
			}
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
				string source = (itemElement.Attributes["Source"].Value ?? " ");
				string slot = itemElement.Attributes["Slot"].Value;
				obtained = Convert.ToBoolean(resetObtained ? "false" : itemElement.Attributes["Obtained"].Value);

				if (resetObtained)
				{
					SetObtained(name, charName, false, ref xmlDoc);
				}

				Item item = new Item(slot, name, source, difficulty , obtained, false, Constants);

				items.Add(item);
			}

			return items;
		}

		public void UpdateBiSList(string difficulty)
		{
			foreach (CharInfo charInfo in CharInfos)
			{
				foreach (Item item in charInfo.BisItems)
				{
					item.Difficulty = difficulty;
					item.Ilevel = Constants.GetMinimumIlevel(difficulty, item.Source, item.IsTier);
				}
			}
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

		private WoWCharacter LoadCharacter(string charName, string realm, out string error)
		{
			error = string.Empty;
			WoWCharacter character = new WoWCharacter();

			try
			{
				character = m_wow.GetCharacter(realm, charName, CharacterProfileOptions.Items);
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

		private ObservableCollection<Item> GetCurrentItems(WoWCharacter character, string raidDifficulty)
		{
			WoWCharacterItems equippedItems = character.items;

			ObservableCollection<Item> currentItems = new ObservableCollection<Item>();

			if (equippedItems != null)
			{
				for (int i=0; i < Constants.EquipmentSlots.Length; i++)
				{
					WoWCharacterItem item;
					try
					{
						// Get the item for each slot. equippedItems. Head, equippedItems.Neck etc.
						item = (WoWCharacterItem)equippedItems.GetType().GetProperty(Constants.EquipmentSlots[i]).GetValue(equippedItems);
					}
					catch
					{
						// We couldn't find an item for this slot.
						item = null;
					}

					if (item != null)
					{
						if (item.name != null)
						{
							bool isWf = false;
							if (Constants.IsWarforged(item.bonusLists.ToList()))
							{
								isWf = true;
							}

							currentItems.Add(new Item(Constants.EquipmentSlots[i], item.name, (int)item.itemLevel, raidDifficulty, false, isWf, Constants));
						}
					}
				}

				// Relics
				WoWCharacterItem artifact = null;
				WoWCharacterItem mainhand = character.items.mainHand;
				WoWCharacterItem offhand = character.items.offHand;
				if (mainhand.relics.Count > 0)
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

				if (artifact != null)
				{
					try
					{
						for (int i = 0; i < artifact.relics.Count; i++)
						{
							WoWArtifactRelic artifactRelic = artifact.relics[i];
							WoWItem relic = m_wow.GetItem((int)artifact.relics[i].itemId);

							int difficultyBonus = artifactRelic.bonusLists[0];
							int ilevelBonus = artifactRelic.bonusLists[1];
							
							int relicIlevel = (int)relic.itemLevel + Constants.GetRelicAddedIlevel(relic, difficultyBonus, ilevelBonus);
							currentItems.Add(new Item($"relic{i + 1}", relic.name, relicIlevel, raidDifficulty, false, false, Constants));
						}
					}
					catch{}
				}
			}
			return currentItems;
		}
		private List<Item> CompareListsBySlot(ObservableCollection<Item> bisList, ObservableCollection<Item> currentItems, string charName, string raidDifficulty, bool ignoreObtained, ref XmlDocument xmlDoc)
		{
			List<string> relicSlotsToSearch = new List<String>() { EmeraldNightmareConstants.s_relic1Slot, EmeraldNightmareConstants.s_relic2Slot, EmeraldNightmareConstants.s_relic3Slot };
			List<Item> itemsNeeded = new List<Item>();
			foreach (string slot in Constants.EquipmentSlots)
			{
				Item bisItem = bisList.Where(i => i.Slot.ToUpper().Trim() == slot.ToUpper().Trim()).FirstOrDefault();
				Item currentItem = currentItems.Where(i => i.Slot.ToUpper().Trim() == slot.ToUpper().Trim()).FirstOrDefault();

				// Check both items were found before comparison.
				if (bisItem != null && currentItem != null)
				{
					bool itemNeeded = false;

					if ((currentItem.Ilevel < bisItem.Ilevel + 50)
						|| (bisItem.Slot == Constants.s_trinket1Slot)
						|| (bisItem.Slot == Constants.s_trinket2Slot)
						|| (Constants.IsItemTierPiece(bisItem.Source, bisItem.Slot)))

					// The current item is less than 15 ilevels higher than the BiS piece,
					// Or the bis item is either a trinket or a tier piece. Therefore, compare the items and
					// calculate whether the BiS piece is still needed.
					{
						if (ignoreObtained || !bisItem.Obtained)
						{
							if ((bisItem.Name.ToUpper().Trim() != currentItem.Name.ToUpper().Trim()) || bisItem.Slot.Contains("relic"))
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

								if (slot.Contains("relic"))
								{
									itemNeeded = CompareRelicSlots(bisItem, currentItems, ref relicSlotsToSearch);
								}
							}
							else
							{
								// Names are the same, make sure that current Ilevel is greater than expected BiS Ilevel.
								itemNeeded = compareIlevels(currentItem, ref bisItem);
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

		private bool CompareRelicSlots(Item bisItem, ObservableCollection<Item> currentItems, ref List<string> relicSlotsToSearch)
		{
			bool itemNeeded = true;

			foreach (string relicSlot in relicSlotsToSearch)
			{
				if (itemNeeded)
				{
					if (relicSlotsToSearch.Contains(relicSlot))
					{
						itemNeeded = CompareBisAgainstItemInDifferentSlot(bisItem, currentItems, relicSlot);
						if (!itemNeeded)
						{
							relicSlotsToSearch.Remove(relicSlot);
							break;
						}
					}
				}
			}

			return itemNeeded;
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
					itemNeeded = compareIlevels(compareItem, ref bisItem);
				}
			}

			return itemNeeded;
		}

		private bool compareIlevels(Item currentItem, ref Item bisItem)
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

			bisItem.SetObtainedDifficulties(currentIlevel);

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
		private static WoWAPI m_wow = new WoWAPI("6phc6jdp43t663mfj7v82dhkyckwbums", BattleDotNetRegion.EU, BattleDotNetLocale.en_GB);
		private MainWindowViewModel m_bisComparerVM;
		private SettingsViewModel m_settingsVM;
	}
}
