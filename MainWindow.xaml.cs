using System.Windows;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Controls;
using System;
using System.Windows.Documents;
using BiSComparer.ViewModels;
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
			MainWindowViewModel = new MainWindowViewModel();
			BiSEditorViewModel = new BiSEditorViewModel(MainWindowViewModel);
			CharacterSummaryViewModel = new CharacterSummaryViewModel(MainWindowViewModel);
			BossSummaryViewModel = new BossSummaryViewModel(MainWindowViewModel);

			InitializeComponent();
			DataContext = this;
		}

		public MainWindowViewModel MainWindowViewModel { get; private set; }

		public BiSEditorViewModel BiSEditorViewModel { get; private set; }

		public CharacterSummaryViewModel CharacterSummaryViewModel { get; private set; }

		public BossSummaryViewModel BossSummaryViewModel { get; private set; }

		public SettingsViewModel SettingsViewModel { get; private set; }


	}
}
