using System.Windows;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using System;
using System.Windows.Documents;
using System.Threading;
using System.Windows.Threading;

namespace BiSComparer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			m_BisCompVm = new BiSComparerViewModel();
			m_BisCompVm.SelectedCharacterChanged += BiSCompVm_SelectedCharacterChanged;
			m_BisCompVm.SelectedBossChanged += BisCompVm_SelectedBossChanged;

			DataContext = m_BisCompVm;
		}

		private void BiSCompVm_SelectedCharacterChanged(object sender, EventArgs e)
		{
			if (m_BisCompVm.SelectedCharacter != null)
			{
				listView.ItemsSource = m_BisCompVm.SelectedCharacter.ItemsNeeded;
				ListViewLoaded(sender, e);
			}
		}
		private void BisCompVm_SelectedBossChanged(object sender, EventArgs e)
		{
			if (m_BisCompVm.SelectedBoss != null)
			{
				bosslistView.ItemsSource = m_BisCompVm.SelectedBoss.ItemsNeeded;
				BossListViewLoaded(sender, e);
			}
		}

		private void ListViewLoaded(object sender, EventArgs e)
		{
			if (listView.ItemsSource != null)
			{
				CollectionView cv = (CollectionView)CollectionViewSource.GetDefaultView(listView.ItemsSource);
				cv.Filter = SourceFilter;
			}
		}
		private void BossListViewLoaded(object sender, EventArgs e)
		{
			if (bosslistView.ItemsSource != null)
			{
				CollectionView cv = (CollectionView)CollectionViewSource.GetDefaultView(bosslistView.ItemsSource);
				cv.Filter = CharFilter;
			}
		}

		private void textFilter_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (listView.ItemsSource != null)
			{
				CollectionViewSource.GetDefaultView(listView.ItemsSource).Refresh();
			}
		}

		private void summaryFilter_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (bosslistView.ItemsSource != null)
			{
				CollectionViewSource.GetDefaultView(bosslistView.ItemsSource).Refresh();
			}
		}

		private bool SourceFilter(object item)
		{
			if (String.IsNullOrEmpty(textFilter.Text))
				return true;

			var Item = (Item)item;

			return (Item.Source.StartsWith(textFilter.Text, StringComparison.OrdinalIgnoreCase));
		}

		private bool CharFilter(object item)
		{
			if (String.IsNullOrEmpty(summaryFilter.Text))
				return true;

			var Item = (Item)item;

			return (Item.Character.StartsWith(summaryFilter.Text, StringComparison.OrdinalIgnoreCase));
		}

		private void listViewColumnHeader_Click(object sender, RoutedEventArgs e)
		{
			GridViewColumnHeader column = (sender as GridViewColumnHeader);
			string sortBy = column.Tag.ToString();
			if (listViewSortCol != null)
			{
				AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
				listView.Items.SortDescriptions.Clear();
			}

			ListSortDirection newDir = ListSortDirection.Ascending;
			if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
				newDir = ListSortDirection.Descending;

			listViewSortCol = column;
			listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
			AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
			listView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
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

		public class SortAdorner : Adorner
		{
			private static Geometry ascGeometry =
					Geometry.Parse("M 0 4 L 3.5 0 L 7 4 Z");

			private static Geometry descGeometry =
					Geometry.Parse("M 0 0 L 3.5 4 L 7 0 Z");

			public ListSortDirection Direction { get; private set; }

			public SortAdorner(UIElement element, ListSortDirection dir)
					: base(element)
			{
				this.Direction = dir;
			}

			protected override void OnRender(DrawingContext drawingContext)
			{
				base.OnRender(drawingContext);

				if (AdornedElement.RenderSize.Width < 20)
					return;

				TranslateTransform transform = new TranslateTransform
						(
								AdornedElement.RenderSize.Width - 15,
								(AdornedElement.RenderSize.Height - 5) / 2
						);
				drawingContext.PushTransform(transform);

				Geometry geometry = ascGeometry;
				if (this.Direction == ListSortDirection.Descending)
					geometry = descGeometry;
				drawingContext.DrawGeometry(Brushes.Black, null, geometry);

				drawingContext.Pop();
			}
		}

		private BiSComparerViewModel m_BisCompVm;
		private GridViewColumnHeader listViewSortCol = null;
		private SortAdorner listViewSortAdorner = null;
	}
}
