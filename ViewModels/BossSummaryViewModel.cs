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
	public class BossSummaryViewModel : INotifyPropertyChanged
	{
		public BossSummaryViewModel(MainWindowViewModel mainWindowViewModel)
		{
			MainWindowViewModel = mainWindowViewModel;
		}

		public MainWindowViewModel MainWindowViewModel {get; private set; }

		public event EventHandler SelectedBossChanged;

		public BossInfo SelectedBoss
		{
			get { return m_selectedBoss; }
			set
			{
				if (m_selectedBoss != value)
				{
					m_selectedBoss = value;
					OnPropertyChanged(new PropertyChangedEventArgs("SelectedBoss"));

					if (SelectedBossChanged != null)
					{
						SelectedBossChanged(this, EventArgs.Empty);
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

		private BossInfo m_selectedBoss;
	}
}
