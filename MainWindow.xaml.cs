﻿using System.Windows;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Controls;
using System;
using System.Windows.Documents;
using BiSComparer.ViewModels;
using System.IO;
using System.Threading;
using System.Linq;

namespace BiSComparer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			SettingsViewModel = new SettingsViewModel();
			MainWindowViewModel = new MainWindowViewModel(SettingsViewModel);
			BiSEditorViewModel = new BiSEditorViewModel(MainWindowViewModel, SettingsViewModel);
			CharacterSummaryViewModel = new CharacterSummaryViewModel(MainWindowViewModel);
			BossSummaryViewModel = new BossSummaryViewModel(MainWindowViewModel);

			string lastFile = Properties.Settings.Default.LastFile;
			bool loadLastFile = Properties.Settings.Default.LoadLastFile;

			InitializeComponent();
			DataContext = this;

			if (!string.IsNullOrEmpty(lastFile))
			{
				if (File.Exists(lastFile))
				{
					if (loadLastFile)
					{
						new Thread(delegate ()
						{
							MainWindowViewModel.PopulateCharInfosAndBossInfos(lastFile);
							MainWindowViewModel.BisFilePath = lastFile;

							CharacterSummaryViewModel.SelectedCharacter = MainWindowViewModel.CharInfos.FirstOrDefault();
							BossSummaryViewModel.SelectedBoss = MainWindowViewModel.BossInfos.FirstOrDefault();
							BiSEditorViewModel.SelectedCharacter = BiSEditorViewModel.CharInfos.FirstOrDefault();
						}).Start();
					}
				}
			}
		}

		public MainWindowViewModel MainWindowViewModel { get; private set; }

		public BiSEditorViewModel BiSEditorViewModel { get; private set; }

		public CharacterSummaryViewModel CharacterSummaryViewModel { get; private set; }

		public BossSummaryViewModel BossSummaryViewModel { get; private set; }

		public SettingsViewModel SettingsViewModel { get; private set; }


	}
}
