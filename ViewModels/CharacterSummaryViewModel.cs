using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;
using System.Windows;


namespace BiSComparer.ViewModels
{
	public class CharacterSummaryViewModel : INotifyPropertyChanged
	{
		public CharacterSummaryViewModel(MainWindowViewModel mainWindowViewModel)
		{
			MainWindowViewModel = mainWindowViewModel;
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
