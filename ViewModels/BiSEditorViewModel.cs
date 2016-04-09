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
using Visibility = System.Windows.Visibility;

namespace BiSComparer.ViewModels
{
	public class BiSEditorViewModel : INotifyPropertyChanged
	{

		#region Constructor
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

			ShowImportWindowCommand = new SimpleCommand
			{
				ExecuteDelegate = X => ShowImportWindow(),
				CanExecuteDelegate = X => CanShowImportWindow()
			};

			ImportFromStringCommand = new SimpleCommand
			{
				ExecuteDelegate = X => ImportFromString(),
				CanExecuteDelegate = X => CanImportFromString()
			};

			CloseImportWindowCommand = new SimpleCommand
			{
				ExecuteDelegate = X => CloseImportWindow()
			};
			

			RaidDifficulties = Constants.s_raidDifficulties;
			MainWindowViewModel.CharInfosChanged += MainWindowViewModel_CharInfosChanged;
		}
		#endregion

		#region EventHandlers

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
		#endregion

		#region Properties
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
					OnPropertyChanged(new PropertyChangedEventArgs("CharacterSelected"));

					if (SelectedCharacterChanged != null)
					{
						SelectedCharacterChanged(this, EventArgs.Empty);
					}
				}
			}
		}

		public bool CharacterSelected
		{
			get { return SelectedCharacter != null; }
		}

		public Visibility ImportWindowVisibility
		{
			get { return m_importWindowVisibility; }
			set
			{
				if (m_importWindowVisibility != value)
				{
					m_importWindowVisibility = value;

					OnPropertyChanged(new PropertyChangedEventArgs("ImportWindowVisibility"));
				}
			}
		}
		public Visibility InverseImportWindowVisibility
		{
			get { return m_inverseImportWindowVisibility; }
			set
			{
				if (m_inverseImportWindowVisibility != value)
				{
					m_inverseImportWindowVisibility = value;

					OnPropertyChanged(new PropertyChangedEventArgs("InverseImportWindowVisibility"));
				}
			}
		}

		public string ImportString
		{
			get { return m_importString; }
			set
			{
				if (m_importString != value)
				{
					m_importString = value;

					OnPropertyChanged(new PropertyChangedEventArgs("ImportedCharacter"));
				}
			}
		}

		#endregion

		#region Commands

		public SimpleCommand SaveAndReloadCommand { get; set; }

		private bool CanSaveBiS()
		{
			return (!string.IsNullOrEmpty(MainWindowViewModel.BisFilePath));
		}

		private void SaveAndReloadBiS()
		{
			new Thread(delegate ()
			{
				SaveBiSListXml(MainWindowViewModel.BisFilePath, CharInfos, saving: true);
				MainWindowViewModel.Error = MainWindowViewModel.PopulateCharInfosAndBossInfos(MainWindowViewModel.BisFilePath);
			}).Start();
		}

		public SimpleCommand SaveAsCommand { get; set; }

		private void SaveBiSAs()
		{
			string filePath = null;
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "Xml files (*.xml)|*.xml|All files (*.*)|*.*";
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				filePath = saveFileDialog.FileName;
			}

			SaveBiSListXml(filePath, CharInfos, saving: true);
		}

		public SimpleCommand CopyCharacterToClipboardCommand { get; set; }

		private bool CanCopyCharacterToClipboard()
		{
			return (SelectedCharacter != null);
		}

		private void CopyCharacterToClipboard()
		{
			ObservableCollection<CharInfo> selectedChar = new ObservableCollection<CharInfo>();
			selectedChar.Add(SelectedCharacter);
			SaveBiSListXml(string.Empty, selectedChar, saving: false);
		}

		public SimpleCommand ImportFromStringCommand { get; private set; }

		private void ImportFromString()
		{
			if (ImportString != null)
			{
				XmlDocument xmlDoc = new XmlDocument();
				try
				{
					xmlDoc.LoadXml(ImportString.Trim());
					XmlNode rootElement = xmlDoc.DocumentElement;
					if (rootElement.Name == "Characters")
					{
						foreach (XmlNode character in rootElement.ChildNodes)
						{
							ImportCharacter(character, ref xmlDoc);
						}
					}
					else if (rootElement.Name == "Character")
					{
						ImportCharacter(rootElement, ref xmlDoc);
					}
				}
				catch
				{
					throw new Exception("The imported character XML was not valid. Make sure that the whole Character XML (including the beginning and closing <Character> and </Character> elements are included.");
				}
			}
			ImportWindowVisibility = Visibility.Collapsed;
			InverseImportWindowVisibility = Visibility.Visible;
		}

		private bool CanImportFromString()
		{
			return (ImportString != null);
		}

		public SimpleCommand ShowImportWindowCommand { get; private set; }

		private void ShowImportWindow()
		{
			ImportWindowVisibility = Visibility.Visible;
			InverseImportWindowVisibility = Visibility.Collapsed;
		}

		private bool CanShowImportWindow()
		{
			return (ImportWindowVisibility != Visibility.Visible);
		}

		public SimpleCommand CloseImportWindowCommand { get; private set; }

		private void CloseImportWindow()
		{
			ImportWindowVisibility = Visibility.Collapsed;
			InverseImportWindowVisibility = Visibility.Visible;
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

			CharInfo newChar = new CharInfo("Character" + (CharInfos.Count + 1).ToString(), "Realm", Constants.s_heroic, string.Empty, items);
			AddCharacterToCharInfos(newChar);
			SelectedCharacter = newChar;
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
			SelectedCharacter = CharInfos.LastOrDefault();
		}
		#endregion

		#region Implementation

		public void SaveBiSListXml(string filePath, ObservableCollection<CharInfo> charInfos, bool saving)
		{
			XmlDocument xmlDoc = new XmlDocument();
			XmlElement characters = xmlDoc.CreateElement("Characters");
			xmlDoc.AppendChild(characters);

			foreach (CharInfo charInfo in charInfos)
			{
				XmlElement character = xmlDoc.CreateElement("Character");

				XmlAttribute charName = xmlDoc.CreateAttribute("Name");
				charName.Value = charInfo.CharName;
				character.Attributes.Append(charName);

				XmlAttribute realm = xmlDoc.CreateAttribute("Realm");
				realm.Value = charInfo.Realm;
				character.Attributes.Append(realm);

				XmlAttribute difficulty = xmlDoc.CreateAttribute("Difficulty");
				difficulty.Value = charInfo.Difficulty;
				character.Attributes.Append(difficulty);

				XmlAttribute group = xmlDoc.CreateAttribute("Group");
				group.Value = charInfo.Group;
				character.Attributes.Append(group);

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
						itemObtained.Value = bisItem.Obtained.ToString();
						item.Attributes.Append(itemObtained);

						character.AppendChild(item);
					}
				}
			}

			if (saving)
			{
				if (filePath != null)
				{
					xmlDoc.Save(Path.Combine(filePath));
				}
			}
			else
			{
				string charactersNodeString = FormatXmlString(characters.InnerXml);
				Clipboard.SetText(charactersNodeString, TextDataFormat.Text);
			}
		}

		private void ImportCharacter(XmlNode character, ref XmlDocument xmlDoc)
		{
			string charName = character.Attributes["Name"].Value;
			string realm = character.Attributes["Realm"].Value;
			string difficulty = character.Attributes["Difficulty"].Value;
			string group = string.Empty;

			if (character.Attributes["Group"] != null)
			{
				group = character.Attributes["Group"].Value;
			}

			ObservableCollection<Item> bisList = MainWindowViewModel.BiSComparerModel.GetBiSList(character, difficulty, ref xmlDoc);

			Item offHand = bisList.Where(i => i.Slot == Constants.s_offHandSlot).FirstOrDefault();
			if (offHand == null)
			{
				// Imported BiS doesn't include and offhand, add one.
				bisList.Add(new Item(Constants.s_offHandSlot, string.Empty, string.Empty));
			}

			CharInfo newChar = new CharInfo(charName, realm, difficulty, group, bisList);
			AddCharacterToCharInfos(newChar);
			SelectedCharacter = newChar;
		}

		private void AddCharacterToCharInfos(CharInfo newChar)
		{
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

		#endregion

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
		private Visibility m_importWindowVisibility = Visibility.Collapsed;
		private Visibility m_inverseImportWindowVisibility = Visibility.Visible;
		private string m_importString;

		#endregion
	}
}
