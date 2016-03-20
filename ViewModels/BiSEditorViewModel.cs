using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace BiSComparer.ViewModels
{
	public class BiSEditorViewModel : INotifyPropertyChanged
	{
		public BiSEditorViewModel(MainWindowViewModel mainWindowViewModel)
		{
			MainWindowViewModel = mainWindowViewModel;

			SaveAndReloadCommand = new SimpleCommand
			{
				ExecuteDelegate = X => SaveAndReloadBiS(),
				CanExecuteDelegate = X => CanSaveBiS()
			};

			SaveAsCommand = new SimpleCommand
			{
				ExecuteDelegate = X => SaveBiSAs()
			};

			CopyCharacterToClipboardCommand = new SimpleCommand
			{
				ExecuteDelegate = X => CopyCharacterToClipboard(),
				CanExecuteDelegate = X => CanCopyCharacterToClipboard()
			};

			AddCommand = new SimpleCommand
			{
				ExecuteDelegate = X => AddCharacter()
			};

			RemoveCommand = new SimpleCommand
			{
				ExecuteDelegate = X => RemoveCharacter(),
				CanExecuteDelegate = X => CanRemoveCharacter()
			};

			RaidDifficulties = Constants.s_raidDifficulties;

			MainWindowViewModel.BiSComparerModel.raidDifficultyChanged += BiSComparerModel_raidDifficultyChanged;
			MainWindowViewModel.CharInfosChanged += MainWindowViewModel_CharInfosChanged;
		}

		private void BiSComparerModel_raidDifficultyChanged(object sender, EventArgs e)
		{
			RaidDifficulty = MainWindowViewModel.BiSComparerModel.RaidDifficulty;
		}

		private void MainWindowViewModel_CharInfosChanged(object sender, EventArgs e)
		{
			IEnumerable<CharInfo> orderedCharInfos = MainWindowViewModel.CharInfos.OrderBy(c => c.CharName);

			foreach (CharInfo charInfo in orderedCharInfos)
			{
				Item offhand = charInfo.BisItems.Where(i => i.Slot.ToUpper().Trim() == Constants.s_offHandSlot.ToUpper().Trim()).FirstOrDefault();

				if (offhand == null)
				{
					// There is no offhand in the BiS list, Add an empty offhand slot.
					charInfo.BisItems.Add(new Item(Constants.s_offHandSlot, string.Empty, string.Empty));
				}
			}

			CharInfos = new ObservableCollection<CharInfo>(orderedCharInfos);
		}

		public ObservableCollection<CharInfo> CharInfos
		{
			get { return m_charInfos; }
			set
			{
				if (m_charInfos != value)
				{
					m_charInfos = value;
					OnPropertyChanged(new PropertyChangedEventArgs("CharInfos"));
				}
			}
		}

		public string RaidDifficulty
		{
			get { return m_raidDifficulty; }
			set
			{
				if (m_raidDifficulty != value)
				{
					m_raidDifficulty = value;
					OnPropertyChanged(new PropertyChangedEventArgs("RaidDifficulty"));
				}
			}
		}

		public string[] RaidDifficulties { get; private set; }

		public MainWindowViewModel MainWindowViewModel { get; private set; }

		public event EventHandler SelectedCharacterChanged;

		public CharInfo SelectedCharacter
		{
			get { return m_selectedCharacter; }
			set
			{
				if (m_selectedCharacter != value)
				{
					m_selectedCharacter = value;
					OnPropertyChanged(new PropertyChangedEventArgs("SelectedCharacter"));

					if (SelectedCharacterChanged != null)
					{
						SelectedCharacterChanged(this, EventArgs.Empty);
					}
				}
			}
		}

		public SimpleCommand SaveAndReloadCommand { get; set; }

		private bool CanSaveBiS()
		{
			return (!string.IsNullOrEmpty(MainWindowViewModel.BisFilePath));
		}

		private void SaveAndReloadBiS()
		{
		new Thread(delegate ()
			{
				SaveBiSListXml(MainWindowViewModel.BisFilePath, RaidDifficulty, CharInfos, saving:true);
				MainWindowViewModel.PopulateCharInfosAndBossInfos(MainWindowViewModel.BisFilePath);
			}).Start();
		}

		public SimpleCommand SaveAsCommand { get; set; }

		private void SaveBiSAs()
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "Xml files (*.xml)|*.xml|All files (*.*)|*.*";
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				string filePath = saveFileDialog.FileName;
			}

			SaveBiSListXml(MainWindowViewModel.BisFilePath, RaidDifficulty, CharInfos, saving:true);
		}

		public SimpleCommand CopyCharacterToClipboardCommand{ get; set; }

		private bool CanCopyCharacterToClipboard()
		{
			return (SelectedCharacter != null);
		}

		private void CopyCharacterToClipboard()
		{
			ObservableCollection<CharInfo> selectedChar = new ObservableCollection<CharInfo>();
			selectedChar.Add(SelectedCharacter);
			SaveBiSListXml(string.Empty, RaidDifficulty, selectedChar, saving: false);
		}

		public void SaveBiSListXml(string filePath, string difficulty, ObservableCollection<CharInfo> charInfos, bool saving)
		{
			XmlDocument xmlDoc = new XmlDocument();
			XmlElement raidInfo = xmlDoc.CreateElement("RaidInfo");
			XmlAttribute raidDifficulty = xmlDoc.CreateAttribute("Difficulty");
			raidDifficulty.Value = difficulty;
			raidInfo.Attributes.Append(raidDifficulty);
			xmlDoc.AppendChild(raidInfo);

			XmlElement characters = xmlDoc.CreateElement("Characters");
			raidInfo.AppendChild(characters);

			foreach (CharInfo charInfo in charInfos)
			{
				XmlElement character = xmlDoc.CreateElement("Character");

				XmlAttribute charName = xmlDoc.CreateAttribute("Name");
				charName.Value = charInfo.CharName;
				character.Attributes.Append(charName);

				XmlAttribute realm = xmlDoc.CreateAttribute("Realm");
				realm.Value = charInfo.Realm;
				character.Attributes.Append(realm);

				characters.AppendChild(character);

				foreach (Item bisItem in charInfo.BisItems)
				{
					bool addItemToXml = true;

					if (bisItem.Slot == Constants.s_offHandSlot)
					{
						if (bisItem.Name.Trim() == string.Empty)
						{
							// No offhand item has been defined, so don't add it to xml
							addItemToXml = false;
						}
					}

					if (addItemToXml)
					{
						XmlElement item = xmlDoc.CreateElement("Item");

						XmlAttribute slot = xmlDoc.CreateAttribute("Slot");
						slot.Value = bisItem.Slot;
						item.Attributes.Append(slot);

						XmlAttribute itemName = xmlDoc.CreateAttribute("Name");
						itemName.Value = bisItem.Name;
						item.Attributes.Append(itemName);

						XmlAttribute itemSource = xmlDoc.CreateAttribute("Source");
						itemSource.Value = bisItem.Source;
						item.Attributes.Append(itemSource);

						XmlAttribute itemObtained = xmlDoc.CreateAttribute("Obtained");
						itemObtained.Value = "False";
						item.Attributes.Append(itemObtained);

						character.AppendChild(item);
					}
				}
			}

			if (saving)
			{
				xmlDoc.Save(Path.Combine(filePath));
			}
			else
			{
				string charactersNodeString = FormatXmlString(characters.InnerXml);
				Clipboard.SetText(charactersNodeString, TextDataFormat.Text);
			}
		}

		private string FormatXmlString (string xml)
		{
			StringBuilder stringBuilder = new StringBuilder();

			XElement element = XElement.Parse(xml);

			XmlWriterSettings settings = new XmlWriterSettings();
			settings.OmitXmlDeclaration = true;
			settings.Indent = true;

			using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, settings))
			{
				element.Save(xmlWriter);
			}

			return stringBuilder.ToString();
		}

		public SimpleCommand AddCommand { get; set; }
		
		private void AddCharacter()
		{
			ObservableCollection<Item> items = new ObservableCollection<Item>();

			foreach (string slot in Constants.s_equipmentSlots)
			{
				Item item = new Item(slot, string.Empty, string.Empty);
				items.Add(item);
			}

			CharInfo newChar = new CharInfo("Character" + (CharInfos.Count + 1).ToString(), "Realm", items);

			if (CharInfos != null)
			{
				CharInfos.Add(newChar);
			}
			else
			{
				ObservableCollection<CharInfo> charInfos = new ObservableCollection<CharInfo>();
				charInfos.Add(newChar);
				CharInfos = charInfos;
			}
		}

		public SimpleCommand RemoveCommand { get; set; }

		private bool CanRemoveCharacter()
		{
			bool canRemoveCharacter = false;
			if (CharInfos != null)
			{
				canRemoveCharacter = (CharInfos.Count >= 1);
			}

			return canRemoveCharacter;
		}

		private void RemoveCharacter()
		{
			CharInfos.Remove(SelectedCharacter);
		}

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, e);
			}
		}

		#endregion

		#region Member Variables

		private CharInfo m_selectedCharacter;
		private ObservableCollection<CharInfo> m_charInfos = new ObservableCollection<CharInfo>();
		private string m_raidDifficulty;

		#endregion
	}
}
