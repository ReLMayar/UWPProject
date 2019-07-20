using GalaSoft.MvvmLight;
using Design.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Project.DB.Models;
using Project.DB;
using Project.BL;
using System.Windows.Input;
using Design.Commands;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Popups;
using Project.BL.Helpers;
using System.Net;
using Windows.Storage;

namespace Design.ViewModels
{
    public class ViewModel : ViewModelBase
    {
        private Buisness _buisness = new Buisness();
        private BitmapImage _bitmap = new BitmapImage();
        private Frame _rootFrame = new Frame();
        private User _user = new User();
        private Project.DB.Shedule _shedule = new Project.DB.Shedule();
        private ChangeList _changeList = new ChangeList();
        private string error = "";

        public ViewModel()
        {
            #region ObservableCollection

            SettingItems = new ObservableCollection<Setting>(GetSettingItems());

            #endregion      
        }

        #region Menu

        #region Loaded

        public ICommand MainPageLoaded
        {
            get { return new DelegateCommand(_MainPageLoaded); }
        }

        /// <summary>
        /// MainPage Loaded
        /// </summary>
        public async void _MainPageLoaded()
        {
            UserShedule = new ObservableCollection<Day>(await GetUserShedule());
            SheduleSwitchIsOn = SheduleTypeGetter.GetSheduleType();
            Changes = new ObservableCollection<Change>(await GetUserChanges());
            MainPage.NavService.NavigateTo(typeof(Shedule), null);
        }

        #endregion

        #region Menu Items

        public ObservableCollection<MenuItem> MenuItems { get; set; }

        /// <summary>
        /// Menu Item
        /// </summary>
        /// <returns></returns>
        private List<MenuItem> GetMenuItems()
        {
            List<MenuItem> menuItems = new List<MenuItem>();
            menuItems.Add(new MenuItem() { Title = _user.Name + " " + _user.SurName, SymbolIcon = Symbol.OtherUser });
            menuItems.Add(new MenuItem() { Title = "Новости", SymbolIcon = Symbol.Pictures, NavigateTo = typeof(News) });
            menuItems.Add(new MenuItem() { Title = "Расписание", SymbolIcon = Symbol.PreviewLink, NavigateTo = typeof(Shedule) });
            menuItems.Add(new MenuItem() { Title = "Домашнее задание", SymbolIcon = Symbol.Edit, NavigateTo = typeof(Hometask) });
            menuItems.Add(new MenuItem() { Title = "Настройки", SymbolIcon = Symbol.Setting });

            return menuItems;
        }

        #endregion

        #region Hamburger SplitView

        private bool _hamburgerSplitView;

        public bool HamburgerSplitView
        {
            get { return _hamburgerSplitView; }
            set { _hamburgerSplitView = value; RaisePropertyChanged("HamburgerSplitView"); }
        }

        public ICommand HamburgerSplitViewOpen
        {
            get { return new DelegateCommand(_HamburgerSplitViewOpen); }
        }

        /// <summary>
        /// SplitView Open/Close
        /// </summary>
        private void _HamburgerSplitViewOpen()
        {
            HamburgerSplitView = !HamburgerSplitView;
        }

        #endregion

        #endregion

        #region Settings

        #region Setting Items

        public ObservableCollection<Setting> SettingItems { get; set; }

        private List<Setting> GetSettingItems()
        {
            List<Setting> settingItems = new List<Setting>();
            settingItems.Add(new Setting() { Title = "Сменить пароль" });
            settingItems.Add(new Setting() { Title = "Выйти" });

            return settingItems;
        }

        #endregion

        #region Change Password

        private string _oldPassword;
        private string _newPassword;
        private string _confirmPassword;

        public string OldPassword
        {
            get { return _oldPassword; }
            set { _oldPassword = value; RaisePropertyChanged("OldPassword"); }
        }

        public string NewPassword
        {
            get { return _newPassword; }
            set { _newPassword = value; RaisePropertyChanged("NewPassword"); }
        }

        public string ConfirmPassword
        {
            get { return _confirmPassword; }
            set { _confirmPassword = value; RaisePropertyChanged("ConfirmPassword"); }
        }

        public ICommand ChangePassword
        {
            get { return new DelegateCommand(_ChangePassword); }
        }

        /// <summary>
        /// Change password
        /// </summary>
        private async void _ChangePassword()
        {
            string error = "";

            Task<HttpStatusCode> changePassword = null;
            changePassword = Task<HttpStatusCode>.Factory.StartNew(() => _buisness.ChangePassword(out error, OldPassword, NewPassword, ConfirmPassword));
            var result = await changePassword;

            if (result != HttpStatusCode.OK)
                Dialog(error);
            else
                Dialog("Пароль успешно изменён!");

            OldPassword = null;
            NewPassword = null;
            ConfirmPassword = null;
        }

        #endregion

        #region Function Settings

        private Setting _setting;
        private bool _splitViewSettings = false;
        private bool _splitViewChangePassword = false;

        public Setting Setting
        {
            get { return _setting; }
            set { _setting = value; RaisePropertyChanged("Setting"); }
        }

        public bool SplitViewSettings
        {
            get { return _splitViewSettings; }
            set { _splitViewSettings = value; RaisePropertyChanged("SplitViewSettings"); }
        }

        public bool SplitViewChangePassword
        {
            get { return _splitViewChangePassword; }
            set { _splitViewChangePassword = value; RaisePropertyChanged("SplitViewChangePassword"); }
        }

        public ICommand SettingItemTapped
        {
            get { return new DelegateCommand(_SettingItemTapped); }
        }

        public ICommand BackButton
        {
            get { return new DelegateCommand(_BackButton); }
        }

        public ICommand SettingMenu
        {
            get { return new DelegateCommand(_SettingMenu); }
        }

        /// <summary>
        /// Item Tapped
        /// </summary>
        private void _SettingItemTapped()
        {
            if (Setting.Title == "Сменить пароль")
            {
                SplitViewChangePassword = true;
            }
            else { _LogOut(); }
        }

        /// <summary>
        /// Exit
        /// </summary>
        private async void _LogOut()
        {
            try
            {
                await Task<bool>.Factory.StartNew(() => _buisness.Logout(out error));

                _rootFrame.Navigate(typeof(Authorize));

                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                await DeleteTempFiles(storageFolder);

                SplitViewSettings = false;
            }
            catch { Dialog(error); }
        }

        /// <summary>
        /// Back Button Click
        /// </summary>
        private void _BackButton()
        {
            SplitViewSettings = true;
            SplitViewChangePassword = false;

            OldPassword = null;
            NewPassword = null;
            ConfirmPassword = null;
        }

        /// <summary>
        /// Open/Close Setting Menu
        /// </summary>
        private void _SettingMenu()
        {
            SplitViewSettings = !SplitViewSettings;
        }

        /// <summary>
        /// Removes all files in the folder
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        internal static async Task DeleteTempFiles(StorageFolder folder)
        {
            var files = (await folder.GetFilesAsync());
            foreach (var file in files)
            {
                await file.DeleteAsync(StorageDeleteOption.Default);
            }
        }

        #endregion

        #endregion

        #region Hometask

        #region Hometask Items

        private ObservableCollection<Project.DB.Models.Hometask> _hometaskItems;

        public ObservableCollection<Project.DB.Models.Hometask> HometaskItems
        {
            get { return _hometaskItems; }
            set { _hometaskItems = value; RaisePropertyChanged("HometaskItems"); }
        }

        private Visibility _nullHometask = Visibility.Collapsed;

        public Visibility NullHometask
        {
            get { return _nullHometask; }
            set { _nullHometask = value; RaisePropertyChanged("NullHometask"); }
        }

        List<Project.DB.Models.Hometask> hometaskItems = new List<Project.DB.Models.Hometask>();        
        private async Task<List<Project.DB.Models.Hometask>> GetHometaskItems()
        {
            //var result = _buisness.GetHometask(out error, out hometaskItems);

            Task<HttpStatusCode> getHometask = null;
            getHometask = Task<HttpStatusCode>.Factory.StartNew(() => _buisness.GetHometask(out error, out hometaskItems));
            var result = await getHometask;

            if (result != HttpStatusCode.OK)
            {
                NullHometask = Visibility.Visible;
                //Dialog(error);
            }
            else { NullHometask = Visibility.Collapsed; }

            return hometaskItems;
        }

        #endregion

        #region Function Hometask

        private string _selectedlesson;
        private string _value;
        private string _hometaskEmpty = "Домашнее задание не выбрано";
        private double _opacityMainHometask = 1;
        private DateTimeOffset _dateLesson;
        private Visibility _visibileWindowHometask = Visibility.Collapsed;
        private Visibility _gridTitle = Visibility.Collapsed;
        private List<string> _pairItem = new List<string>();
        private Project.DB.Models.Hometask _hometaskSelectedItem;

        public string SelectedLesson
        {
            get { return _selectedlesson; }
            set { _selectedlesson = value;RaisePropertyChanged("SelectedLesson"); }
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; RaisePropertyChanged("Value"); }
        }

        public string HometaskEmpty
        {
            get { return _hometaskEmpty; }
            set { _hometaskEmpty = value; RaisePropertyChanged("HometaskEmpty"); }
        }

        public double OpacityMainHometask
        {
            get { return _opacityMainHometask; }
            set { _opacityMainHometask = value; RaisePropertyChanged("OpacityMainHometask"); }
        }

        public DateTimeOffset DateLesson
        {
            get { return _dateLesson; }
            set { _dateLesson = value; RaisePropertyChanged("DateLesson"); }
        }

        public Visibility VisibileWindowHometask
        {
            get { return _visibileWindowHometask;}
            set { _visibileWindowHometask = value; RaisePropertyChanged("VisibileWindowHometask"); }
        }

        public Visibility GridTitle
        {
            get { return _gridTitle; }
            set { _gridTitle = value; RaisePropertyChanged("GridTitle"); }
        }

        public List<string> PairItem
        {
            get { return _pairItem; }
            set { _pairItem = value; RaisePropertyChanged("PairItem"); }
        }

        public Project.DB.Models.Hometask HometaskSelectedItem
        {
            get { return _hometaskSelectedItem; }
            set { _hometaskSelectedItem = value; RaisePropertyChanged("HometaskSelectedItem"); }
        }

        public ICommand AddHometask
        {
            get { return new DelegateCommand(_AddHometask); }
        }

        public ICommand OpenAddHometask
        {
            get { return new DelegateCommand(_OpenAddHometask); }
        }

        public ICommand DeleteHometask
        {
            get { return new DelegateCommand(_DeleteHometask); }
        }

        public ICommand CloseAddHometask
        {
            get { return new DelegateCommand(_CloseAddHometask); }
        }

        public ICommand HometaskOutput
        {
            get { return new DelegateCommand(_HometaskOutput); }
        }

        /// <summary>
        /// Add hometask
        /// </summary>
        private async void _AddHometask()
        {
            if (SelectedLesson != "" && Value != null)
            {
                var hometask = new Project.DB.Models.Hometask
                {
                    Lesson = SelectedLesson,
                    DateRecord = DateTime.Now,
                    DateLesson = DateLesson.UtcDateTime,
                    Value = Value
                };

                Task<HttpStatusCode> postHometask = null;
                postHometask = Task<HttpStatusCode>.Factory.StartNew(() => _buisness.PostHometask(out error, hometask));
                var result = await postHometask;

                if (result == HttpStatusCode.OK)
                {
                    Dialog("Домашнее задание добавлено!");
                    HometaskItems = new ObservableCollection<Project.DB.Models.Hometask>(await GetHometaskItems());
                }
                else
                    Dialog(error);

                _CloseAddHometask();
                Value = null;
            }
            else
            {
                Dialog("Не все поля заполнены!");
            }
        }

        /// <summary>
        /// Open window
        /// </summary>
        private void _OpenAddHometask()
        {
            GetPairItems();
            VisibileWindowHometask = Visibility.Visible;
            OpacityMainHometask = 0.5;
        }

        /// <summary>
        /// Close window
        /// </summary>
        private void _CloseAddHometask()
        {
            VisibileWindowHometask = Visibility.Collapsed;
            OpacityMainHometask = 1;
            Value = null;
        }

        /// <summary>
        /// Output
        /// </summary>
        private void _HometaskOutput()
        {
            HometaskEmpty = "";
            GridTitle = Visibility.Visible;
        }

        /// <summary>
        /// Pair Items
        /// </summary>
        /// <returns></returns>
        private List<string> GetPairItems()
        {
            List<Day> days = _shedule.SheduleGroups.First(q => q.Name == _user.GroupName).Days;

            foreach (var day in days)
                PairItem.AddRange(day.Pairs.Select(q => q.Lesson));
            PairItem = PairItem.Distinct().ToList();

            return PairItem;
        }

        /// <summary>
        /// Delete Hometask
        /// </summary>
        private async void _DeleteHometask()
        {
            if (HometaskSelectedItem == null)
            {
                Dialog("Домашнее задание не выбрано!");
            }
            else
            {
                Task<HttpStatusCode> deleteHometask = null;
                deleteHometask = Task<HttpStatusCode>.Factory.StartNew(() => _buisness.DeleteHometask(out error, HometaskSelectedItem.HometaskId));
                var result = await deleteHometask;

                if (result == HttpStatusCode.OK)
                {
                    Dialog("Домашнее задание удалено!");
                    HometaskItems = new ObservableCollection<Project.DB.Models.Hometask>(await GetHometaskItems());
                }
                else
                    Dialog(error);
            }
        }

        #endregion

        #region Search

        private string _hometaskAutoSuggestBox;
        private List<string> _hometaskAutoSuggestBoxItemsSource;
        private bool _hometaskIsSuggestionListOpen = false;

        public string HometaskAutoSuggestBox
        {
            get { return _hometaskAutoSuggestBox; }
            set { _hometaskAutoSuggestBox = value; RaisePropertyChanged("HometaskAutoSuggestBox"); }
        }

        public List<string> HometaskAutoSuggestBoxItemsSource
        {
            get { return _hometaskAutoSuggestBoxItemsSource; }
            set { _hometaskAutoSuggestBoxItemsSource = value; RaisePropertyChanged("HometaskAutoSuggestBoxItemsSource"); }
        }

        public bool HometaskIsSuggestionListOpen
        {
            get { return _hometaskIsSuggestionListOpen; }
            set { _hometaskIsSuggestionListOpen = value; RaisePropertyChanged("HometaskIsSuggestionListOpen"); }
        }

        public ICommand HometaskTextChange
        {
            get { return new DelegateCommand(_HometaskTextChange); }
        }

        public ICommand HometaskQuerySubmitted
        {
            get { return new DelegateCommandWithParameters<AutoSuggestBoxQuerySubmittedEventArgs>(_HometaskQuerySubmitted); }
        }

        public ICommand HometaskSuggestionChosen
        {
            get { return new DelegateCommandWithParameters<AutoSuggestBoxSuggestionChosenEventArgs>(_HometaskSuggestionChosen); }
        }

        /// <summary>
        /// Text Change
        /// </summary>
        private async void _HometaskTextChange()
        {
            if (HometaskAutoSuggestBox != "")
            {
                try
                {
                    var term = HometaskAutoSuggestBox.ToLower();
                    var results = hometaskItems.Where(i => i.Lesson.ToLower().Contains(term)).Select(q => q.Lesson).Distinct().ToList();
                    HometaskAutoSuggestBoxItemsSource = results.ToList();
                }
                catch { }
            }
            else
            {
                HometaskItems = new ObservableCollection<Project.DB.Models.Hometask>(await GetHometaskItems());
            }
        }

        /// <summary>
        /// Query Submitted
        /// </summary>
        private void _HometaskQuerySubmitted(AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var term = args.QueryText.ToLower();

            if (args.ChosenSuggestion != null)
            {
                SelectedHometask(args.ChosenSuggestion as string);
            }
            else
            {
                try
                {
                    var results = hometaskItems.Where(i => i.Lesson.ToLower().Contains(term)).ToList();
                    if (results.Count() >= 1)
                        SelectedHometask(results.Select(q => q.Lesson).FirstOrDefault());
                }
                catch
                {
                    hometaskItems = new List<Project.DB.Models.Hometask>();
                    HometaskItems = new ObservableCollection<Project.DB.Models.Hometask>(hometaskItems);
                    NullHometask = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Selected Hometask
        /// </summary>
        /// <param name="hometask"></param>
        private void SelectedHometask(string hometask)
        {
            HometaskAutoSuggestBox = hometask;
            NullHometask = Visibility.Collapsed;
            hometaskItems = hometaskItems.Where(i => i.Lesson.ToLower().Contains(hometask.ToLower())).ToList();
            HometaskItems = new ObservableCollection<Project.DB.Models.Hometask>(hometaskItems);
        }

        /// <summary>
        /// Suggestion Chosen
        /// </summary>
        private void _HometaskSuggestionChosen(AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            NullHometask = Visibility.Collapsed;
            hometaskItems = hometaskItems.Where(i => i.Lesson.ToLower().Contains(args.SelectedItem.ToString().ToLower())).ToList();
            HometaskItems = new ObservableCollection<Project.DB.Models.Hometask>(hometaskItems);
        }

        #endregion

        #endregion

        #region Shedule

        #region Loading

        private bool _typeUser;
        private string _groupName;

        public string GroupName
        {
            get { return _groupName; }
            set { _groupName = value; RaisePropertyChanged("GroupName"); }
        }

        public ICommand SheduleLoading
        {
            get { return new DelegateCommand(_SheduleLoading); }
        }

        /// <summary>
        /// Shedule Loading
        /// </summary>
        private void _SheduleLoading()
        {
            GetGroups(out _sheduleGroups);
            GroupName = _user.GroupName;
            _typeUser = true;
        }

        #endregion

        #region Shedule Items

        public ObservableCollection<Day> GroupShedule { get; set; }
        public ObservableCollection<Day> _userShedule { get; set; }
        private Visibility _nullShedule;

        public ObservableCollection<Day> UserShedule
        {
            get { return _userShedule; }
            set { _userShedule = value; RaisePropertyChanged("UserShedule"); }
        }

        public Visibility NullShedule
        {
            get { return _nullShedule; }
            set { _nullShedule = value; RaisePropertyChanged("NullShedule"); }
        }

        /// <summary>
        /// Gets the user schedule
        /// </summary>
        /// <returns></returns>
        private async Task<List<Day>> GetUserShedule()
        {
            NullShedule = Visibility.Collapsed;
            List<Day> days = new List<Day>();

            Task<HttpStatusCode> getUserShedule = null;
            getUserShedule = Task<HttpStatusCode>.Factory.StartNew(() => _buisness.GetUserShedule(out error, out days, out _shedule));
            var result = await getUserShedule;

            if (result != HttpStatusCode.OK)
            {
                days = new List<Day>();
                NullShedule = Visibility.Visible;
            }
            else
            {
                if (SheduleSwitchIsOn)
                {
                    days = days.Select(q => new Day()
                    {
                        Name = q.Name,
                        Pairs = q.Pairs.Where(p => p.State == "single" || p.State == "denominator").ToList()
                    }).ToList();
                }
                else
                {
                    days = days.Select(q => new Day()
                    {
                        Name = q.Name,
                        Pairs = q.Pairs.Where(p => p.State == "single" || p.State == "numerator").ToList()
                    }).ToList();
                }
            }

            return days;
        }

        /// <summary>
        /// Gets the group schedule
        /// </summary>
        /// <param name="group"> Group name </param>
        /// <returns></returns>
        private async Task<List<Day>> GetGroupShedule(string group)
        {
            NullShedule = Visibility.Collapsed;
            List<Day> days = new List<Day>();

            Task<HttpStatusCode> getGroupShedule = null;
            getGroupShedule = Task<HttpStatusCode>.Factory.StartNew(() => _buisness.GetGroupShedule(out error, out days, group));
            var result = await getGroupShedule;

            if (result != HttpStatusCode.OK)
            {
                days = new List<Day>();
                NullShedule = Visibility.Visible;
            }
            else
            {
                if (SheduleSwitchIsOn)
                {
                    days = days.Select(q => new Day()
                    {
                        Name = q.Name,
                        Pairs = q.Pairs.Where(p => p.State == "single" || p.State == "denominator").ToList()
                    }).ToList();
                }
                else
                {
                    days = days.Select(q => new Day()
                    {
                        Name = q.Name,
                        Pairs = q.Pairs.Where(p => p.State == "single" || p.State == "numerator").ToList()
                    }).ToList();
                }
            }

            return days;
        }

        /// <summary>
        /// Get groups
        /// </summary>
        /// <param name="sheduleGroups"></param>
        /// <returns></returns>
        private List<string> GetGroups(out List<string> sheduleGroups)
        {
            var result = _buisness.GetGroups(out error, out sheduleGroups);

            if (result != HttpStatusCode.OK)
                Dialog(error);

            return sheduleGroups;
        }

        #endregion

        #region Search

        private List<string> _sheduleGroups = new List<string>();
        private string _sheduleAutoSuggestBox;
        private List<string> _sheduleAutoSuggestBoxItemsSource;
        private bool _sheduleIsSuggestionListOpen = false;

        public string SheduleAutoSuggestBox
        {
            get { return _sheduleAutoSuggestBox; }
            set { _sheduleAutoSuggestBox = value; RaisePropertyChanged("SheduleAutoSuggestBox"); }
        }

        public List<string> SheduleAutoSuggestBoxItemsSource
        {
            get { return _sheduleAutoSuggestBoxItemsSource; }
            set { _sheduleAutoSuggestBoxItemsSource = value; RaisePropertyChanged("SheduleAutoSuggestBoxItemsSource"); }
        }

        public bool SheduleIsSuggestionListOpen
        {
            get { return _sheduleIsSuggestionListOpen; }
            set { _sheduleIsSuggestionListOpen = value; RaisePropertyChanged("SheduleIsSuggestionListOpen"); }
        }

        public ICommand SheduleTextChange
        {
            get { return new DelegateCommand(_SheduleTextChange); }
        }

        public ICommand SheduleQuerySubmitted
        {
            get { return new DelegateCommandWithParameters<AutoSuggestBoxQuerySubmittedEventArgs>(_SheduleQuerySubmitted); }
        }

        public ICommand SheduleSuggestionChosen
        {
            get { return new DelegateCommandWithParameters<AutoSuggestBoxSuggestionChosenEventArgs>(_SheduleSuggestionChosen); }
        }

        /// <summary>
        /// Text Change
        /// </summary>
        private async void _SheduleTextChange()
        {
            if (SheduleAutoSuggestBox != "")
            {
                var term = SheduleAutoSuggestBox.ToLower();
                var results = _sheduleGroups.Where(i => i.ToLower().Contains(term)).Select(q => q).Distinct().ToList();
                SheduleAutoSuggestBoxItemsSource = results.ToList();
            }
            else
            {
                UserShedule = new ObservableCollection<Day>(await GetUserShedule());
                Changes = new ObservableCollection<Change>(await GetUserChanges());
                GroupName = _user.GroupName;
                _typeUser = true;
            }
        }

        /// <summary>
        /// Shedule Query Submitted
        /// </summary>
        /// <param name="args"></param>
        private void _SheduleQuerySubmitted(AutoSuggestBoxQuerySubmittedEventArgs args)
        {            
            var term = args.QueryText.ToLower();

            if (args.ChosenSuggestion != null)
            {
                SelectedShedule(args.ChosenSuggestion as string);
            }
            else
            {
                var results = _sheduleGroups.Where(i => i.ToLower().Contains(term)).ToList();
                if (results.Count() >= 1)
                    SelectedShedule(results.Select(q => q).FirstOrDefault());
            }
        }

        /// <summary>
        /// Selected Shedule
        /// </summary>
        /// <param name="shedule"></param>
        private async void SelectedShedule(string group)
        {
            SheduleAutoSuggestBox = group;

            UserShedule = new ObservableCollection<Day>(await GetGroupShedule(group));
            Changes = new ObservableCollection<Change>(await GetChanges(group));
            GroupName = group;
            _typeUser = false;
        }

        /// <summary>
        /// Shedule Suggestion Chosen
        /// </summary>
        private async void _SheduleSuggestionChosen(AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            UserShedule = new ObservableCollection<Day>(await GetGroupShedule(args.SelectedItem.ToString()));
            Changes = new ObservableCollection<Change>(await GetChanges(args.SelectedItem.ToString()));
            GroupName = args.SelectedItem.ToString();
            _typeUser = false;
        }

        #endregion

        #region Changes Shedule

        private Visibility _changesSheduleVisibility;
        private ObservableCollection<Change> _changes { get; set; }
        private DateTime _dateOfChanges;

        public Visibility ChangesSheduleVisibility
        {
            get { return _changesSheduleVisibility; }
            set { _changesSheduleVisibility = value; RaisePropertyChanged("ChangesSheduleVisibility"); }
        }

        public DateTime DateOfChanges
        {
            get { return _dateOfChanges; }
            set { _dateOfChanges = value; RaisePropertyChanged("DateOfChanges"); }
        }

        public ObservableCollection<Change> Changes
        {
            get { return _changes; }
            set { _changes = value; RaisePropertyChanged("Changes"); }
        }

        /// <summary>
        /// Get User Changes Shedule
        /// </summary>
        /// <returns></returns>
        private async Task<List<Change>> GetUserChanges()
        {
            List<Change> pairs = new List<Change>();

            Task<HttpStatusCode> getUserChanges = null;
            getUserChanges = Task<HttpStatusCode>.Factory.StartNew(() => _buisness.GetUserChanges(out error, out pairs, out _changeList));
            var result = await getUserChanges;

            if (result != HttpStatusCode.OK)
                Dialog(error);

            DateOfChanges = _changeList.DateOfChanges;

            if (pairs.Count != 0)
                ChangesSheduleVisibility = Visibility.Visible;
            else
                ChangesSheduleVisibility = Visibility.Collapsed;

            return pairs;
        }

        /// <summary>
        /// Get Changes Shedule
        /// </summary>
        /// <returns></returns>
        private async Task<List<Change>> GetChanges(string group)
        {
            List<Change> pairs = new List<Change>();

            Task<HttpStatusCode> getChanges = null;
            getChanges = Task<HttpStatusCode>.Factory.StartNew(() => _buisness.GetChanges(out error, out pairs, out _changeList, group));
            var result = await getChanges;

            if (result != HttpStatusCode.OK)
                Dialog(error);

            DateOfChanges = _changeList.DateOfChanges;

            if (pairs.Count != 0)
                ChangesSheduleVisibility = Visibility.Visible;
            else
                ChangesSheduleVisibility = Visibility.Collapsed;

            return pairs;
        }

        #endregion

        #region Numerator/Denominator

        private Visibility _sheduleNumeratorVisibility = Visibility.Visible;
        private Visibility _sheduleDenominatorVisibility = Visibility.Collapsed;
        private bool _sheduleSwitchIsOn;

        public Visibility SheduleNumeratorVisibility
        {
            get { return _sheduleNumeratorVisibility; }
            set { _sheduleNumeratorVisibility = value; RaisePropertyChanged("SheduleNumeratorVisibility"); }
        }

        public Visibility SheduleDenominatorVisibility
        {
            get { return _sheduleDenominatorVisibility; }
            set { _sheduleDenominatorVisibility = value; RaisePropertyChanged("SheduleDenominatorVisibility"); }
        }

        public bool SheduleSwitchIsOn
        {
            get { return _sheduleSwitchIsOn; }
            set { _sheduleSwitchIsOn = value; RaisePropertyChanged("SheduleSwitchIsOn"); }
        }

        public ICommand SheduleSwitch
        {
            get { return new DelegateCommand(_SheduleSwitch); }
        }

        /// <summary>
        /// Switch Numerator/Denominator
        /// </summary>
        private async void _SheduleSwitch()
        {
            if (SheduleSwitchIsOn)
            {
                SheduleNumeratorVisibility = Visibility.Collapsed;
                SheduleDenominatorVisibility = Visibility.Visible;
                SheduleSwitchIsOn = true;
            }
            else
            {
                SheduleNumeratorVisibility = Visibility.Visible;
                SheduleDenominatorVisibility = Visibility.Collapsed;
                SheduleSwitchIsOn = false;
            }

            if (_typeUser)
            {
                UserShedule = new ObservableCollection<Day>(await GetUserShedule());
                Changes = new ObservableCollection<Change>(await GetChanges(_user.GroupName));
            }
            else
            {
                GroupShedule = new ObservableCollection<Day>(await GetGroupShedule(SheduleAutoSuggestBox));
                Changes = new ObservableCollection<Change>(await GetChanges(SheduleAutoSuggestBox));
            }
        }

        #endregion

        #endregion

        #region News

        #region News Items

        private ObservableCollection<news> _newsItems;

        public ObservableCollection<news> NewsItems
        {
            get { return _newsItems; }
            set { _newsItems = value; RaisePropertyChanged("NewsItems"); }
        }

        private List<news> GetNewsItems()
        {
            List<news> newsItems = new List<news>();

            try { _buisness.GetNews(out error, out newsItems); }
            catch { Dialog(error); }

            return newsItems;
        }

        #endregion

        #region Function News

        private double _opacityMainNews = 1;
        private Visibility _visibleWindowNews = Visibility.Collapsed;

        public double OpacityMainNews
        {
            get { return _opacityMainNews; }
            set { _opacityMainNews = value; RaisePropertyChanged("OpacityMainNews"); }
        }

        public Visibility VisibleWindowNews
        {
            get { return _visibleWindowNews; }
            set { _visibleWindowNews = value; RaisePropertyChanged("VisibleWindowNews"); }
        }

        public ICommand NewsWindowOpen
        {
            get { return new DelegateCommand(_NewsWindowOpen); }
        }

        public ICommand CloseWindowNews
        {
            get { return new DelegateCommand(_CloseWindowNews); }
        }

        /// <summary>
        /// Open window
        /// </summary>
        private void _NewsWindowOpen()
        {
            VisibleWindowNews = Visibility.Visible;
            OpacityMainNews = 0.5;
        }

        /// <summary>
        /// Close window
        /// </summary>
        private void _CloseWindowNews()
        {
            VisibleWindowNews = Visibility.Collapsed;
            OpacityMainNews = 1;
        }

        #endregion

        #endregion

        #region Authorization

        private string _login;
        private string _password;
        private bool _authorizeProgressRing = false;
        private Visibility _visiblebutton = Visibility.Visible;

        public string Login
        {
            get { return _login; }
            set { _login = value; RaisePropertyChanged("Login"); }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; RaisePropertyChanged("Password"); }
        }

        public bool AuthorizeProgressRing
        {
            get { return _authorizeProgressRing; }
            set { _authorizeProgressRing = value; RaisePropertyChanged("AuthorizeProgressRing"); }
        }

        public Visibility VisibleButton
        {
            get { return _visiblebutton; }
            set { _visiblebutton = value; RaisePropertyChanged("VisibleButton"); }
        }

        public ICommand Authorize
        {
            get{ return new DelegateCommand(_Authorize); }
        }

        public ICommand Register
        {
            get { return new DelegateCommand(_Register); }
        }

        public ICommand RemindPassword
        {
            get { return new DelegateCommand(_RemindPassword); }
        }

        public ICommand AuthorizeLoading
        {
            get { return new DelegateCommand(_AuthorizeLoading); }
        }

        /// <summary>
        /// Authorize user
        /// </summary>
        private async void _Authorize()
        {
            VisibleButton = Visibility.Collapsed;
            AuthorizeProgressRing = true;

            Task<bool> authorize = null;
            authorize = Task<bool>.Factory.StartNew(() => _buisness.Authorize(out error, Login, Password));
            var isAuthorized = await authorize;

            if (isAuthorized)
            {
                Task<HttpStatusCode> getUserInfo = null;
                getUserInfo = Task<HttpStatusCode>.Factory.StartNew(() => _buisness.GetUserInfo(out error, out _user));
                var result = await getUserInfo;

                if (result != HttpStatusCode.OK)
                {
                    Dialog(error);
                }

                Window.Current.Content = _rootFrame;
                _rootFrame.Navigate(typeof(MainPage), null);
                MenuItems = new ObservableCollection<MenuItem>(GetMenuItems());

                AuthorizeProgressRing = false;
                VisibleButton = Visibility.Visible;
                Password = null;
            }
            else
            {
                AuthorizeProgressRing = false;
                VisibleButton = Visibility.Visible;
                Dialog(error);
            }
        }

        /// <summary>
        /// Authentication
        /// </summary>
        private async void Authentication()
        {
            Task<bool> authentication = null;
            authentication = Task<bool>.Factory.StartNew(() => _buisness.Authentication(out error));
            var isAuthorized = await authentication;

            if (isAuthorized)
            {
                Task<HttpStatusCode> getUserInfo = null;
                getUserInfo = Task<HttpStatusCode>.Factory.StartNew(() => _buisness.GetUserInfo(out error, out _user));
                var result = await getUserInfo;

                if (result != HttpStatusCode.OK)
                {
                    Dialog(error);
                }

                Window.Current.Content = _rootFrame;
                _rootFrame.Navigate(typeof(MainPage), null);
                MenuItems = new ObservableCollection<MenuItem>(GetMenuItems());
            }
        }

        /// <summary>
        /// Register user
        /// </summary>
        private void _Register()
        {
            // The URI to launch
            var uriBing = new Uri(@"http://makeeducationbetter.ru");

            // Launch the URI
            var success = Windows.System.Launcher.LaunchUriAsync(uriBing);
        }

        /// <summary>
        /// Remind Password
        /// </summary>
        private void _RemindPassword()
        {
            // The URI to launch
            var uriBing = new Uri(@"http://makeeducationbetter.ru");

            // Launch the URI
            var success = Windows.System.Launcher.LaunchUriAsync(uriBing);
        }

        /// <summary>
        /// Authorize Loading
        /// </summary>
        private void _AuthorizeLoading()
        {
            Authentication();
        }

        #endregion

        #region Navigation

        private MenuItem _selectedMenuItem;

        public MenuItem SelectedMenuItem
        {
            get { return _selectedMenuItem; }
            set { _selectedMenuItem = value; RaisePropertyChanged("SelectedMenuItem"); }
        }

        public ICommand NavigateToPage
        {
            get { return new DelegateCommand(_NavigateToPage); }
        }

        /// <summary>
        /// Navigation
        /// </summary>
        private async void _NavigateToPage()
        {
            if (SelectedMenuItem != null)
            {
                if (SelectedMenuItem.Title == "Настройки") { SplitViewSettings = !SplitViewSettings; }
                else if(SelectedMenuItem.NavigateTo != null)
                {
                    switch (SelectedMenuItem.Title)
                    {
                        case "Новости":
                            NewsItems = new ObservableCollection<news>(GetNewsItems());
                            break;

                        case "Расписание":
                            SheduleSwitchIsOn = SheduleTypeGetter.GetSheduleType();
                            Changes = new ObservableCollection<Change>(await GetUserChanges());
                            break;

                        case "Домашнее задание":
                            HometaskItems = new ObservableCollection<Project.DB.Models.Hometask>(await GetHometaskItems());
                            break;
                    }
                    MainPage.NavService.NavigateTo(SelectedMenuItem.NavigateTo, null);
                }
            }
        }

        #endregion

        #region Output

        /// <summary>
        /// Output
        /// </summary>
        /// <param name="error">Error</param>
        private async void Dialog(string error)
        {
            var dialog = new MessageDialog(error);
            await dialog.ShowAsync();
        }

        #endregion

        #region MainProgressBar

        private Visibility _mainProgressBar = Visibility.Collapsed;

        public Visibility MainProgressBar
        {
            get { return _mainProgressBar; }
            set { _mainProgressBar = value; RaisePropertyChanged("MainProgressBar"); }
        }

        #endregion

    }
}
