using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiSComparer.ViewModels
{
	public class SettingsViewModel
	{
		public SettingsViewModel()
		{
			ResetObtained = Properties.Settings.Default.ResetObtained;
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

		private bool m_resetObtained;
	}
}
