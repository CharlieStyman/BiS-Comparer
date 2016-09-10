using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiSComparer.ViewModels
{
	public class SettingsViewModel : INotifyPropertyChanged
	{
		public SettingsViewModel()
		{
			ResetObtained = Properties.Settings.Default.ResetObtained;
			LoadLastFile = Properties.Settings.Default.LoadLastFile;
			LastFile = Properties.Settings.Default.LastFile;

			if (Properties.Settings.Default.RaidTier != "")
			{
				RaidTier = Properties.Settings.Default.RaidTier;
			}
			else
			{
				RaidTier = s_emeraldNightmare;
			}

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

		public string RaidTier
		{
			get { return m_raidTier; }
			set
			{
				if (m_raidTier != value)
				{
					m_raidTier = value;
					OnPropertyChanged(new PropertyChangedEventArgs("RaidTier"));
					Properties.Settings.Default.RaidTier = value;
					Properties.Settings.Default.Save();

					switch(m_raidTier)
					{
						case ("Hellfire Citadel - WoD"):
							Constants = new HellfireCitadelConstants();
							break;
						case ("The Emerald Nightmare - Legion"):
							Constants = new EmeraldNightmareConstants();
							break;
						case ("Suramar Palace - Legion"):
							Constants = new SuramarPalaceConstants();
							break;
						default:
							Constants = new Constants();
							break;
					}

					if (RaidTierChanged != null)
					{
						RaidTierChanged(this, EventArgs.Empty);
					}
				}
			}
		}

		public event EventHandler RaidTierChanged;

		public string[] RaidTiers
		{
			get { return new string[] { s_suramarPalace, s_emeraldNightmare, s_hellfireCitadel }; }
		}

		public Constants Constants
		{
			get { return m_constants; }
			set
			{
				if (m_constants != value)
				{
					m_constants = value;
				}
			}
		}

		public string Credit { get { return s_credit; } }

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
		private bool m_loadLastFile;
		private string m_lastFile;
		private string m_raidTier;
		private Constants m_constants;

		private static string s_suramarPalace = "Suramar Palace - Legion";
		private static string s_emeraldNightmare = "The Emerald Nightmare - Legion";
		private static string s_hellfireCitadel = "Hellfire Citadel - WoD";

		private static string s_credit = "Developed and maintained by Diazo <The Scarlet Crusade> - Darksorrow - EU";
	}
}
