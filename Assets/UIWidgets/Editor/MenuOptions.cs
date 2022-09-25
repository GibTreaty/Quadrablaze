using UnityEditor;

namespace UIWidgets {
	public static class MenuOptions {
		#region Collections
		[MenuItem("GameObject/UI/UIWidgets/Collections/Combobox", false, 1000)]
		public static void CreateCombobox()
		{
			Utilites.CreateWidgetFromAsset("Combobox");
		}

		[MenuItem("GameObject/UI/UIWidgets/Collections/ComboboxIcons", false, 1010)]
		public static void CreateComboboxIcons()
		{
			Utilites.CreateWidgetFromAsset("ComboboxIcons");
		}

		[MenuItem("GameObject/UI/UIWidgets/Collections/ComboboxIconsMultiselect", false, 1020)]
		public static void CreateComboboxIconsMultiselect()
		{
			Utilites.CreateWidgetFromAsset("ComboboxIconsMultiselect");
		}

		[MenuItem("GameObject/UI/UIWidgets/Collections/DirectoryTreeView", false, 1030)]
		public static void CreateDirectoryTreeView()
		{
			Utilites.CreateWidgetFromAsset("DirectoryTreeView");
		}

		[MenuItem("GameObject/UI/UIWidgets/Collections/FileListView", false, 1040)]
		public static void CreateFileListView()
		{
			Utilites.CreateWidgetFromAsset("FileListView");
		}

		[MenuItem("GameObject/UI/UIWidgets/Collections/ListView", false, 1050)]
		public static void CreateListView()
		{
			Utilites.CreateWidgetFromAsset("ListView");
		}

		[MenuItem("GameObject/UI/UIWidgets/Collections/ListViewInt", false, 1060)]
		public static void CreateListViewInt()
		{
			Utilites.CreateWidgetFromAsset("ListViewInt");
		}

		[MenuItem("GameObject/UI/UIWidgets/Collections/ListViewHeight", false, 1070)]
		public static void CreateListViewHeight()
		{
			Utilites.CreateWidgetFromAsset("ListViewHeight");
		}

		//[MenuItem("GameObject/UI/UIWidgets/Collections/ListViewGameObjects", false, 1080)]
		public static void CreateListViewGameObjects()
		{
			Utilites.CreateWidgetFromAsset("ListViewGameObjects");
		}

		[MenuItem("GameObject/UI/UIWidgets/Collections/ListViewIcons", false, 1090)]
		public static void CreateListViewIcons()
		{
			Utilites.CreateWidgetFromAsset("ListViewIcons");
		}

		[MenuItem("GameObject/UI/UIWidgets/Collections/ListViewPaginator", false, 1100)]
		public static void CreateListViewPaginator()
		{
			Utilites.CreateWidgetFromAsset("ListViewPaginator");
		}

		[MenuItem("GameObject/UI/UIWidgets/Collections/TreeView", false, 1110)]
		public static void CreateTreeView()
		{
			Utilites.CreateWidgetFromAsset("TreeView");
		}
		#endregion

	    #region Containers
		[MenuItem("GameObject/UI/UIWidgets/Containers/Accordion", false, 2000)]
		public static void CreateAccordion()
		{
			Utilites.CreateWidgetFromAsset("Accordion");
		}

		[MenuItem("GameObject/UI/UIWidgets/Containers/Tabs", false, 2010)]
		public static void CreateTabs()
		{
			Utilites.CreateWidgetFromAsset("Tabs");
		}

		[MenuItem("GameObject/UI/UIWidgets/Containers/TabsLeft", false, 2020)]
		public static void CreateTabsLeft()
		{
			Utilites.CreateWidgetFromAsset("TabsLeft");
		}

		[MenuItem("GameObject/UI/UIWidgets/Containers/TabsIcons", false, 2030)]
		public static void CreateTabsIcons()
		{
			Utilites.CreateWidgetFromAsset("TabsIcons");
		}

		[MenuItem("GameObject/UI/UIWidgets/Containers/TabsIconsLeft", false, 2040)]
		public static void CreateTabsIconsLeft()
		{
			Utilites.CreateWidgetFromAsset("TabsIconsLeft");
		}
		#endregion

		#region Dialogs
		[MenuItem("GameObject/UI/UIWidgets/Dialogs/DatePicker", false, 3010)]
		public static void CreateDatePicker()
		{
			Utilites.CreateWidgetFromAsset("DatePicker");
		}

		[MenuItem("GameObject/UI/UIWidgets/Dialogs/Dialog Template", false, 3020)]
		public static void CreateDialog()
		{
			Utilites.CreateWidgetFromAsset("DialogTemplate");
		}

		[MenuItem("GameObject/UI/UIWidgets/Dialogs/FileDialog", false, 3030)]
		public static void CreateFileDialog()
		{
			Utilites.CreateWidgetFromAsset("FileDialog");
		}

		[MenuItem("GameObject/UI/UIWidgets/Dialogs/FolderDialog", false, 3040)]
		public static void CreateFolderDialog()
		{
			Utilites.CreateWidgetFromAsset("FolderDialog");
		}

		[MenuItem("GameObject/UI/UIWidgets/Dialogs/Notify Template", false, 3050)]
		public static void CreateNotify()
		{
			Utilites.CreateWidgetFromAsset("NotifyTemplate");
		}

		[MenuItem("GameObject/UI/UIWidgets/Dialogs/PickerBool", false, 3060)]
		public static void CreatePickerBool()
		{
			Utilites.CreateWidgetFromAsset("PickerBool");
		}

		[MenuItem("GameObject/UI/UIWidgets/Dialogs/PickerIcons", false, 3070)]
		public static void CreatePickerIcons()
		{
			Utilites.CreateWidgetFromAsset("PickerIcons");
		}

		[MenuItem("GameObject/UI/UIWidgets/Dialogs/PickerInt", false, 3080)]
		public static void CreatePickerInt()
		{
			Utilites.CreateWidgetFromAsset("PickerInt");
		}

		[MenuItem("GameObject/UI/UIWidgets/Dialogs/PickerString", false, 3090)]
		public static void CreatePickerString()
		{
			Utilites.CreateWidgetFromAsset("PickerString");
		}

		[MenuItem("GameObject/UI/UIWidgets/Dialogs/Popup", false, 3100)]
		public static void CreatePopup()
		{
			Utilites.CreateWidgetFromAsset("Popup");
		}
		#endregion

		#region Input
		[MenuItem("GameObject/UI/UIWidgets/Input/ButtonBig", false, 4000)]
		public static void CreateButtonBig()
		{
			Utilites.CreateWidgetFromAsset("ButtonBig");
		}

		[MenuItem("GameObject/UI/UIWidgets/Input/ButtonSmall", false, 4010)]
		public static void CreateButtonSmall()
		{
			Utilites.CreateWidgetFromAsset("ButtonSmall");
		}

		[MenuItem("GameObject/UI/UIWidgets/Input/Calendar", false, 4020)]
		public static void CreateCalendar()
		{
			Utilites.CreateWidgetFromAsset("Calendar");
		}

		[MenuItem("GameObject/UI/UIWidgets/Input/CenteredSlider", false, 4030)]
		public static void CreateCenteredSlider()
		{
			Utilites.CreateWidgetFromAsset("CenteredSlider");
		}

		[MenuItem("GameObject/UI/UIWidgets/Input/CenteredSliderVertical", false, 4040)]
		public static void CreateCenteredSliderVertical()
		{
			Utilites.CreateWidgetFromAsset("CenteredSliderVertical");
		}

		[MenuItem("GameObject/UI/UIWidgets/Input/ColorPicker", false, 4050)]
		public static void CreateColorPicker()
		{
			Utilites.CreateWidgetFromAsset("ColorPicker");
		}

		[MenuItem("GameObject/UI/UIWidgets/Input/ColorPickerRange", false, 4060)]
		public static void CreateColorPickerRange()
		{
			Utilites.CreateWidgetFromAsset("ColorPickerRange");
		}

		[MenuItem("GameObject/UI/UIWidgets/Input/RangeSlider", false, 4070)]
		public static void CreateRangeSlider()
		{
			Utilites.CreateWidgetFromAsset("RangeSlider");
		}

		[MenuItem("GameObject/UI/UIWidgets/Input/RangeSliderFloat", false, 4080)]
		public static void CreateRangeSliderFloat()
		{
			Utilites.CreateWidgetFromAsset("RangeSliderFloat");
		}

		[MenuItem("GameObject/UI/UIWidgets/Input/RangeSliderVertical", false, 4090)]
		public static void CreateObject()
		{
			Utilites.CreateWidgetFromAsset("RangeSliderVertical");
		}

		[MenuItem("GameObject/UI/UIWidgets/Input/RangeSliderFloatVertical", false, 4100)]
		public static void CreateRangeSliderFloatVertical()
		{
			Utilites.CreateWidgetFromAsset("RangeSliderFloatVertical");
		}

		[MenuItem("GameObject/UI/UIWidgets/Input/Spinner", false, 4110)]
		public static void CreateSpinner()
		{
			Utilites.CreateWidgetFromAsset("Spinner");
		}

		[MenuItem("GameObject/UI/UIWidgets/Input/SpinnerFloat", false, 4120)]
		public static void CreateSpinnerFloat()
		{
			Utilites.CreateWidgetFromAsset("SpinnerFloat");
		}

		[MenuItem("GameObject/UI/UIWidgets/Input/Switch", false, 4130)]
		public static void CreateSwitch()
		{
			Utilites.CreateWidgetFromAsset("Switch");
		}
		#endregion

		[MenuItem("GameObject/UI/UIWidgets/AudioPlayer", false, 5000)]
		public static void CreateAudioPlayer()
		{
			Utilites.CreateWidgetFromAsset("AudioPlayer");
		}

		[MenuItem("GameObject/UI/UIWidgets/Progressbar", false, 5000)]
		public static void CreateProgressbar()
		{
			Utilites.CreateWidgetFromAsset("Progressbar");
		}

		[MenuItem("GameObject/UI/UIWidgets/ScrollRectPaginator", false, 5010)]
		public static void CreateScrollRectPaginator()
		{
			Utilites.CreateWidgetFromAsset("ScrollRectPaginator");
		}

		[MenuItem("GameObject/UI/UIWidgets/ScrollRectNumericPaginator", false, 5020)]
		public static void CreateScrollRectNumericPaginator()
		{
			Utilites.CreateWidgetFromAsset("ScrollRectNumericPaginator");
		}
	}
}