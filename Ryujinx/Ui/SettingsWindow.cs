using Gtk;
using Ryujinx.Common.Configuration.Hid;
using Ryujinx.Configuration;
using Ryujinx.Configuration.System;
using Ryujinx.HLE.FileSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using GUI = Gtk.Builder.ObjectAttribute;

namespace Ryujinx.Ui
{
    public class SettingsWindow : Window
    {
        private static ListStore         _gameDirsBoxStore;
        private static VirtualFileSystem _virtualFileSystem;

#pragma warning disable CS0649
#pragma warning disable IDE0044
        [GUI] CheckButton  _errorLogToggle;
        [GUI] CheckButton  _warningLogToggle;
        [GUI] CheckButton  _infoLogToggle;
        [GUI] CheckButton  _stubLogToggle;
        [GUI] CheckButton  _debugLogToggle;
        [GUI] CheckButton  _fileLogToggle;
        [GUI] CheckButton  _guestLogToggle;
        [GUI] CheckButton  _fsAccessLogToggle;
        [GUI] Adjustment   _fsLogSpinAdjustment;
        [GUI] CheckButton  _dockedModeToggle;
        [GUI] CheckButton  _discordToggle;
        [GUI] CheckButton  _vSyncToggle;
        [GUI] CheckButton  _multiSchedToggle;
        [GUI] CheckButton  _fsicToggle;
        [GUI] CheckButton  _ignoreToggle;
        [GUI] CheckButton  _directKeyboardAccess;
        [GUI] ComboBoxText _systemLanguageSelect;
        [GUI] CheckButton  _custThemeToggle;
        [GUI] Entry        _custThemePath;
        [GUI] ToggleButton _browseThemePath;
        [GUI] Label        _custThemePathLabel;
        [GUI] TreeView     _gameDirsBox;
        [GUI] Entry        _addGameDirBox;
        [GUI] Entry        _graphicsShadersDumpPath;
        [GUI] ToggleButton _configureController1;
        [GUI] ToggleButton _configureController2;
        [GUI] ToggleButton _configureController3;
        [GUI] ToggleButton _configureController4;
        [GUI] ToggleButton _configureController5;
        [GUI] ToggleButton _configureController6;
        [GUI] ToggleButton _configureController7;
        [GUI] ToggleButton _configureController8;
        [GUI] ToggleButton _configureControllerH;
#pragma warning restore CS0649
#pragma warning restore IDE0044

        public SettingsWindow(VirtualFileSystem virtualFileSystem) : this(new Builder("Ryujinx.Ui.SettingsWindow.glade"), virtualFileSystem) { }

        private SettingsWindow(Builder builder, VirtualFileSystem virtualFileSystem) : base(builder.GetObject("_settingsWin").Handle)
        {
            builder.Autoconnect(this);

            this.Icon = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.Icon.png");

            _virtualFileSystem = virtualFileSystem;

            //Bind Events
            _configureController1.Pressed += (sender, args) => ConfigureController_Pressed(sender, args, ControllerId.ControllerPlayer1);
            _configureController2.Pressed += (sender, args) => ConfigureController_Pressed(sender, args, ControllerId.ControllerPlayer2);
            _configureController3.Pressed += (sender, args) => ConfigureController_Pressed(sender, args, ControllerId.ControllerPlayer3);
            _configureController4.Pressed += (sender, args) => ConfigureController_Pressed(sender, args, ControllerId.ControllerPlayer4);
            _configureController5.Pressed += (sender, args) => ConfigureController_Pressed(sender, args, ControllerId.ControllerPlayer5);
            _configureController6.Pressed += (sender, args) => ConfigureController_Pressed(sender, args, ControllerId.ControllerPlayer6);
            _configureController7.Pressed += (sender, args) => ConfigureController_Pressed(sender, args, ControllerId.ControllerPlayer7);
            _configureController8.Pressed += (sender, args) => ConfigureController_Pressed(sender, args, ControllerId.ControllerPlayer8);
            _configureControllerH.Pressed += (sender, args) => ConfigureController_Pressed(sender, args, ControllerId.ControllerHandheld);

            //Setup Currents
            if (ConfigurationState.Instance.Logger.EnableFileLog)
            {
                _fileLogToggle.Click();
            }

            if (ConfigurationState.Instance.Logger.EnableError)
            {
                _errorLogToggle.Click();
            }

            if (ConfigurationState.Instance.Logger.EnableWarn)
            {
                _warningLogToggle.Click();
            }

            if (ConfigurationState.Instance.Logger.EnableInfo)
            {
                _infoLogToggle.Click();
            }

            if (ConfigurationState.Instance.Logger.EnableStub)
            {
                _stubLogToggle.Click();
            }

            if (ConfigurationState.Instance.Logger.EnableDebug)
            {
                _debugLogToggle.Click();
            }

            if (ConfigurationState.Instance.Logger.EnableGuest)
            {
                _guestLogToggle.Click();
            }

            if (ConfigurationState.Instance.Logger.EnableFsAccessLog)
            {
                _fsAccessLogToggle.Click();
            }

            if (ConfigurationState.Instance.System.EnableDockedMode)
            {
                _dockedModeToggle.Click();
            }

            if (ConfigurationState.Instance.EnableDiscordIntegration)
            {
                _discordToggle.Click();
            }

            if (ConfigurationState.Instance.Graphics.EnableVsync)
            {
                _vSyncToggle.Click();
            }

            if (ConfigurationState.Instance.System.EnableMulticoreScheduling)
            {
                _multiSchedToggle.Click();
            }

            if (ConfigurationState.Instance.System.EnableFsIntegrityChecks)
            {
                _fsicToggle.Click();
            }

            if (ConfigurationState.Instance.System.IgnoreMissingServices)
            {
                _ignoreToggle.Click();
            }

            if (ConfigurationState.Instance.Hid.EnableKeyboard)
            {
                _directKeyboardAccess.Click();
            }

            if (ConfigurationState.Instance.Ui.EnableCustomTheme)
            {
                _custThemeToggle.Click();
            }

            _systemLanguageSelect.SetActiveId(ConfigurationState.Instance.System.Language.Value.ToString());

            _custThemePath.Buffer.Text           = ConfigurationState.Instance.Ui.CustomThemePath;
            _graphicsShadersDumpPath.Buffer.Text = ConfigurationState.Instance.Graphics.ShadersDumpPath;
            _fsLogSpinAdjustment.Value           = ConfigurationState.Instance.System.FsGlobalAccessLogMode;

            _gameDirsBox.AppendColumn("", new CellRendererText(), "text", 0);
            _gameDirsBoxStore  = new ListStore(typeof(string));
            _gameDirsBox.Model = _gameDirsBoxStore;

            foreach (string gameDir in ConfigurationState.Instance.Ui.GameDirs.Value)
            {
                _gameDirsBoxStore.AppendValues(gameDir);
            }

            if (_custThemeToggle.Active == false)
            {
                _custThemePath.Sensitive      = false;
                _custThemePathLabel.Sensitive = false;
                _browseThemePath.Sensitive    = false;
            }
        }

        //Events
        private void AddDir_Pressed(object sender, EventArgs args)
        {
            if (Directory.Exists(_addGameDirBox.Buffer.Text))
            {
                _gameDirsBoxStore.AppendValues(_addGameDirBox.Buffer.Text);
            }
            else
            {
                FileChooserDialog fileChooser = new FileChooserDialog("Choose the game directory to add to the list", this, FileChooserAction.SelectFolder, "Cancel", ResponseType.Cancel, "Add", ResponseType.Accept);

                if (fileChooser.Run() == (int)ResponseType.Accept)
                {
                    _gameDirsBoxStore.AppendValues(fileChooser.Filename);
                }

                fileChooser.Dispose();
            }

            _addGameDirBox.Buffer.Text = "";

            ((ToggleButton)sender).SetStateFlags(0, true);
        }

        private void RemoveDir_Pressed(object sender, EventArgs args)
        {
            TreeSelection selection = _gameDirsBox.Selection;

            if (selection.GetSelected(out TreeIter treeIter))
            {
                _gameDirsBoxStore.Remove(ref treeIter);
            }

            ((ToggleButton)sender).SetStateFlags(0, true);
        }

        private void CustThemeToggle_Activated(object sender, EventArgs args)
        {
            _custThemePath.Sensitive      = _custThemeToggle.Active;
            _custThemePathLabel.Sensitive = _custThemeToggle.Active;
            _browseThemePath.Sensitive    = _custThemeToggle.Active;
        }

        private void BrowseThemeDir_Pressed(object sender, EventArgs args)
        {
            FileChooserDialog fileChooser = new FileChooserDialog("Choose the theme to load", this, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Select", ResponseType.Accept);

            fileChooser.Filter = new FileFilter();
            fileChooser.Filter.AddPattern("*.css");

            if (fileChooser.Run() == (int)ResponseType.Accept)
            {
                _custThemePath.Buffer.Text = fileChooser.Filename;
            }

            fileChooser.Dispose();

            _browseThemePath.SetStateFlags(0, true);
        }

        private void OpenLogsFolder_Pressed(object sender, EventArgs args)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName        = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs"),
                UseShellExecute = true,
                Verb            = "open"
            });
        }

        private void ConfigureController_Pressed(object sender, EventArgs args, ControllerId controllerId)
        {
            ((ToggleButton)sender).SetStateFlags(0, true);

            ControllerWindow controllerWin = new ControllerWindow(controllerId, _virtualFileSystem);
            controllerWin.Show();
        }

        private void SaveToggle_Activated(object sender, EventArgs args)
        {
            List<string> gameDirs = new List<string>();

            _gameDirsBoxStore.GetIterFirst(out TreeIter treeIter);
            for (int i = 0; i < _gameDirsBoxStore.IterNChildren(); i++)
            {
                gameDirs.Add((string)_gameDirsBoxStore.GetValue(treeIter, 0));

                _gameDirsBoxStore.IterNext(ref treeIter);
            }

            ConfigurationState.Instance.Logger.EnableError.Value               = _errorLogToggle.Active;
            ConfigurationState.Instance.Logger.EnableWarn.Value                = _warningLogToggle.Active;
            ConfigurationState.Instance.Logger.EnableInfo.Value                = _infoLogToggle.Active;
            ConfigurationState.Instance.Logger.EnableStub.Value                = _stubLogToggle.Active;
            ConfigurationState.Instance.Logger.EnableDebug.Value               = _debugLogToggle.Active;
            ConfigurationState.Instance.Logger.EnableGuest.Value               = _guestLogToggle.Active;
            ConfigurationState.Instance.Logger.EnableFsAccessLog.Value         = _fsAccessLogToggle.Active;
            ConfigurationState.Instance.Logger.EnableFileLog.Value             = _fileLogToggle.Active;
            ConfigurationState.Instance.System.EnableDockedMode.Value          = _dockedModeToggle.Active;
            ConfigurationState.Instance.EnableDiscordIntegration.Value         = _discordToggle.Active;
            ConfigurationState.Instance.Graphics.EnableVsync.Value             = _vSyncToggle.Active;
            ConfigurationState.Instance.System.EnableMulticoreScheduling.Value = _multiSchedToggle.Active;
            ConfigurationState.Instance.System.EnableFsIntegrityChecks.Value   = _fsicToggle.Active;
            ConfigurationState.Instance.System.IgnoreMissingServices.Value     = _ignoreToggle.Active;
            ConfigurationState.Instance.Hid.EnableKeyboard.Value               = _directKeyboardAccess.Active;
            ConfigurationState.Instance.Ui.EnableCustomTheme.Value             = _custThemeToggle.Active;
            ConfigurationState.Instance.System.Language.Value                  = Enum.Parse<Language>(_systemLanguageSelect.ActiveId);
            ConfigurationState.Instance.Ui.CustomThemePath.Value               = _custThemePath.Buffer.Text;
            ConfigurationState.Instance.Graphics.ShadersDumpPath.Value         = _graphicsShadersDumpPath.Buffer.Text;
            ConfigurationState.Instance.Ui.GameDirs.Value                      = gameDirs;
            ConfigurationState.Instance.System.FsGlobalAccessLogMode.Value     = (int)_fsLogSpinAdjustment.Value;

            MainWindow.SaveConfig();
            MainWindow.ApplyTheme();
            MainWindow.UpdateGameTable();
            Dispose();
        }

        private void CloseToggle_Activated(object sender, EventArgs args)
        {
            Dispose();
        }
    }
}
