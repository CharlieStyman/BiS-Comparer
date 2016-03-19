using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.ComponentModel;
using BiSComparer.ViewModels;

namespace BiSComparer.Views
{
	/// <summary>
	/// Interaction logic for BossSummaryView.xaml
	/// </summary>
	public partial class BossSummaryView : UserControl
	{
		public BossSummaryView()
		{
			InitializeComponent();
		}

		private void bossListViewColumnHeader_Click(object sender, RoutedEventArgs e)
		{
			GridViewColumnHeader column = (sender as GridViewColumnHeader);
			string sortBy = column.Tag.ToString();
			if (listViewSortCol != null)
			{
				AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
				bosslistView.Items.SortDescriptions.Clear();
			}

			ListSortDirection newDir = ListSortDirection.Ascending;
			if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
				newDir = ListSortDirection.Descending;

			listViewSortCol = column;
			listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
			AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
			bosslistView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
		}

		private GridViewColumnHeader listViewSortCol = null;
		private SortAdorner listViewSortAdorner = null;
	}
}
