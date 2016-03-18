using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;
using System.Windows;

namespace BiSComparer
{
	public class BiSComparerViewModel : INotifyPropertyChanged
	{
		public BiSComparerViewModel()
		{
			m_bisComparerModel = new BiSComparerModel(this);
			LoadBiSListsCommand = new SimpleCommand { ExecuteDelegate = X => LoadBiSLists() };
			SaveAndReloadCommand = new SimpleCommand
			{
				ExecuteDelegate = X => SaveAndReloadBiS(),
				CanExecuteDelegate = X => CanSaveAndReloadBiS()
			};
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

		public ObservableCollection<BossInfo> BossInfos
		{
			get { return m_bossInfos; }
			set
			{
				if (m_bossInfos != value)
				{
					m_bossInfos = value;
					OnPropertyChanged(new PropertyChangedEventArgs("BossInfos"));
				}
			}
		}

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

		public string ProgressText
		{
			get { return m_progressText; }
			set
			{
				if (m_progressText != value)
				{
					m_progressText = value;
					OnPropertyChanged(new PropertyChangedEventArgs("ProgressText"));
				}
			}
		}

		public double ProgressValue
		{
			get { return m_progressValue; }
			set
			{
				if (m_progressValue != value)
				{
					m_progressValue = value;
					OnPropertyChanged(new PropertyChangedEventArgs("ProgressValue"));
				}
			}
		}

		public Visibility ProgressVisibility
		{
			get { return m_progressVisibility; }
			set
			{
				if (m_progressVisibility != value)
				{
					m_progressVisibility = value;
					OnPropertyChanged(new PropertyChangedEventArgs("ProgressVisibility"));
				}
			}
		}

		public Visibility InverseProgressVisibility
		{
			get { return m_inverseProgressVisibility; }
			set
			{
				if (m_inverseProgressVisibility != value)
				{
					m_inverseProgressVisibility = value;
					OnPropertyChanged(new PropertyChangedEventArgs("InverseProgressVisibility"));
				}
			}
		}

		public string BisFilePath { get; set; }

		public SimpleCommand LoadBiSListsCommand { get; set; }

		private void LoadBiSLists()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Xml files (*.xml)|*.xml|All files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				BisFilePath = openFileDialog.FileName;
			}

			new Thread(delegate()
				{
					PopulateCharInfosAndBossInfos(BisFilePath);
				}).Start();
		}

		private void PopulateCharInfosAndBossInfos(string BisFilePath)
		{
			if (!string.IsNullOrEmpty(BisFilePath))
			{
				// Rearrange char infos to be ordered by number of items still needed.
				IEnumerable<CharInfo> charInfos = m_bisComparerModel.GetCharInfos(BisFilePath).OrderByDescending(o => o.ItemsNeededCount);
				CharInfos = new ObservableCollection<CharInfo>(charInfos);

				// Rearrange boss infos to be ordered by number of items still needed.
				IEnumerable<BossInfo> bossInfos = m_bisComparerModel.GetBossInfos(CharInfos).OrderByDescending(o => o.ItemsNeededCount);
				BossInfos = new ObservableCollection<BossInfo>(bossInfos);
			}
		}
		public void UpdateProgressBar(string charName, double numberOfCharacters)
		{
			if (ProgressVisibility != Visibility.Visible)
			{
				ProgressVisibility = Visibility.Visible;
				InverseProgressVisibility = Visibility.Hidden;
			}

			ProgressText = "Loading Gear For " + charName;
			ProgressValue += (100 / numberOfCharacters);

		}

		public void ResetProgressBar()
		{
			ProgressVisibility = Visibility.Hidden;
			InverseProgressVisibility = Visibility.Visible;
			ProgressValue = 0;
			ProgressText = "";
		}


		public SimpleCommand SaveAndReloadCommand { get; set; }

		private bool CanSaveAndReloadBiS()
		{
			return (!string.IsNullOrEmpty(BisFilePath));
		}

		private void SaveAndReloadBiS()
		{
			new Thread(delegate()
				{
					m_bisComparerModel.SaveBiSList(BisFilePath, CharInfos);
					PopulateCharInfosAndBossInfos(BisFilePath);
				}).Start();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, e);
			}
		}


		private BiSComparerModel m_bisComparerModel;
		private ObservableCollection<CharInfo> m_charInfos;
		private CharInfo m_selectedCharacter;
		private ObservableCollection<BossInfo> m_bossInfos;
		private BossInfo m_selectedBoss;
		private string m_progressText;
		private double m_progressValue;
		private Visibility m_progressVisibility = Visibility.Hidden;
		private Visibility m_inverseProgressVisibility = Visibility.Visible;
	}
}
