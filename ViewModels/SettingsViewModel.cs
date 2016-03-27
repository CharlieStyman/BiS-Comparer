using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiSComparer.ViewModels
{
	public class SettingsViewModel:INotifyPropertyChanged
	{
		public SettingsViewModel()
		{
			ResetObtained = Properties.Settings.Default.ResetObtained;
			LoadLastFile = Properties.Settings.Default.LoadLastFile;
			LastFile = Properties.Settings.Default.LastFile;
		}

		public bool ResetObtained
		{
			get { return m_resetObtained; }
			set
			{
				if (m_resetObtained != value)
				{
					m_resetObtained = value;
					Properties.Settings.Default.ResetObtained = value;
					Properties.Settings.Default.Save();
				}
			}
		}

		public bool LoadLastFile
		{
			get { return m_loadLastFile; }
			set
			{
				if (m_loadLastFile != value)
				{
					m_loadLastFile = value;
					Properties.Settings.Default.LoadLastFile = value;
					Properties.Settings.Default.Save();
				}
			}
		}

		public string LastFile
		{
			get { return m_lastFile; }
			set
			{
				if (m_lastFile != value)
				{
					m_lastFile = value;
					OnPropertyChanged(new PropertyChangedEventArgs("LastFile"));
					Properties.Settings.Default.LastFile = value;
					Properties.Settings.Default.Save();
				}
			}
		}

		#region PropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, e);
			}
		}

		#endregion

		private bool m_resetObtained;
		private string m_lastFile;
		private bool m_loadLastFile;
	}
}
