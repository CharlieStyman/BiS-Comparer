﻿using System;
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
	public class MainWindowViewModel : INotifyPropertyChanged
	{
		public MainWindowViewModel(SettingsViewModel settingsViewModel)
		{
			BiSComparerModel = new BiSComparerModel(this, settingsViewModel);
			LoadBiSListsCommand = new SimpleCommand { ExecuteDelegate = X => LoadBiSLists() };

			Difficulties = settingsViewModel.Constants.RaidDifficulties;
		}

		public BiSComparerModel BiSComparerModel { get; private set; }

		public ObservableCollection<CharInfo> CharInfos
		{
			get { return m_charInfos; }
			set
			{
				if (m_charInfos != value)
				{
					m_charInfos = value;
					OnPropertyChanged(new PropertyChangedEventArgs("CharInfos"));

					if (CharInfosChanged != null)
					{
						CharInfosChanged(this, EventArgs.Empty);
					}
				}
			}
		}

		public event EventHandler CharInfosChanged;

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

		public string Error
		{
			get { return m_error; }
			set
			{
				if (m_error != value)
				{
					m_error = value;
					OnPropertyChanged(new PropertyChangedEventArgs("Error"));
				}
			}
		}

		public string[] Difficulties { get; private set; }

		public string Difficulty
		{
			get { return m_difficulty; }
			set
			{
				if (m_difficulty != value)
				{
					m_difficulty = value;
					OnDifficultyChanged();
					OnPropertyChanged(new PropertyChangedEventArgs("Difficulty"));
				}
			}
		}

		private void OnDifficultyChanged()
		{
			// Difficulty has changed, re-compare items.
			UpdateCharInfosAndBossInfos(Difficulty, ignoreObtained:true);
			OnPropertyChanged(new PropertyChangedEventArgs("BossInfos"));
			OnPropertyChanged(new PropertyChangedEventArgs("CharInfos"));
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
				Properties.Settings.Default.LastFile = BisFilePath;
				Properties.Settings.Default.Save();

				new Thread(delegate ()
					{
						Error = PopulateCharInfosAndBossInfos(BisFilePath);
					}).Start();
			}
		}

		public string PopulateCharInfosAndBossInfos(string BisFilePath)
		{
			string error = string.Empty;
			string difficulty = string.Empty;
			if (!string.IsNullOrEmpty(BisFilePath))
			{
				// Rearrange char infos to be ordered by number of items still needed.
				BiSComparerModel.LoadCharInfosFromFile(BisFilePath, out difficulty, out error);
				UpdateCharInfosAndBossInfos(difficulty, ignoreObtained:false);
			}

			m_difficulty = difficulty;
			OnPropertyChanged(new PropertyChangedEventArgs("Difficulty"));

			return error;
		}

		private void UpdateCharInfosAndBossInfos(string difficulty, bool ignoreObtained)
		{
			BiSComparerModel.UpdateBiSList(difficulty);
			BiSComparerModel.UpdateItemsNeeded(difficulty, ignoreObtained);
			CharInfos = new ObservableCollection<CharInfo>(BiSComparerModel.CharInfos.OrderByDescending(o => o.ItemsNeededCount));

			IEnumerable<BossInfo> bossInfos = BiSComparerModel.GetBossInfos(CharInfos);
			BossInfos = new ObservableCollection<BossInfo>(bossInfos);
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

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, e);
			}
		}

		private ObservableCollection<CharInfo> m_charInfos = new ObservableCollection<CharInfo>();
		private ObservableCollection<BossInfo> m_bossInfos = new ObservableCollection<BossInfo>();

		private string m_progressText;
		private double m_progressValue;
		private string m_error;
		private string m_difficulty;
		private Visibility m_progressVisibility = Visibility.Hidden;
		private Visibility m_inverseProgressVisibility = Visibility.Visible;
	}
}
