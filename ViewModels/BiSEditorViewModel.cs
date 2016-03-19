using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
				CanExecuteDelegate = X => CanSaveAndReloadBiS()
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
		}

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

		private bool CanSaveAndReloadBiS()
		{
			return (!string.IsNullOrEmpty(MainWindowViewModel.BisFilePath));
		}

		private void SaveAndReloadBiS()
		{
			MainWindowViewModel.SaveAndReloadBiS();
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

			CharInfo newChar = new CharInfo("newChar", items);

			if (MainWindowViewModel.CharInfos != null)
			{
				MainWindowViewModel.CharInfos.Add(newChar);
			}
			else
			{
				ObservableCollection<CharInfo> charInfos = new ObservableCollection<CharInfo>();
				charInfos.Add(newChar);
				MainWindowViewModel.CharInfos = charInfos;
			}

			
		}

		public SimpleCommand RemoveCommand { get; set; }

		private bool CanRemoveCharacter()
		{
			bool canRemoveCharacter = false;
			if (MainWindowViewModel.CharInfos != null)
			{
				canRemoveCharacter = (MainWindowViewModel.CharInfos.Count >= 1);
			}

			return canRemoveCharacter;
		}

		private void RemoveCharacter()
		{
			MainWindowViewModel.CharInfos.Remove(SelectedCharacter);
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

		#endregion
	}
}
