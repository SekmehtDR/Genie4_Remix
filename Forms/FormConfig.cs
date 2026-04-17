using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace GenieClient
{
    public partial class FormConfig
    {
        private GenieClient.Genie.Config _lichConfig;

        public FormConfig()
        {
            InitializeComponent();
        }

        private void OK_Button_Click(object sender, EventArgs e)
        {
            ApplyChanges();
            ApplyLichSettings();
            Close();
        }

        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ApplyChanges()
        {
            // MsgBox("Applying changes")
            if (UcWindows1.ItemChanged)
            {
                if (Interaction.MsgBox("Current window item has been changed. Apply changes?", MsgBoxStyle.YesNo) == MsgBoxResult.Yes)
                {
                    UcWindows1.ApplyChanges();
                }
            }

            if (UcAliases1.ItemChanged)
            {
                if (Interaction.MsgBox("Current alias has been changed. Apply changes?", MsgBoxStyle.YesNo) == MsgBoxResult.Yes)
                {
                    UcAliases1.ApplyChanges();
                }
            }

            if (UcSubs1.ItemChanged)
            {
                if (Interaction.MsgBox("Current substitute has been changed. Apply changes?", MsgBoxStyle.YesNo) == MsgBoxResult.Yes)
                {
                    UcSubs1.ApplyChanges();
                }
            }

            if (UcMacros1.ItemChanged)
            {
                if (Interaction.MsgBox("Current macro has been changed. Apply changes?", MsgBoxStyle.YesNo) == MsgBoxResult.Yes)
                {
                    UcMacros1.ApplyChanges();
                }
            }

            if (UcTriggers1.ItemChanged)
            {
                if (Interaction.MsgBox("Current trigger has been changed. Apply changes?", MsgBoxStyle.YesNo) == MsgBoxResult.Yes)
                {
                    UcTriggers1.ApplyChanges();
                }
            }

            if (UcIgnore1.ItemChanged)
            {
                if (Interaction.MsgBox("Current ignore/gag has been changed. Apply changes?", MsgBoxStyle.YesNo) == MsgBoxResult.Yes)
                {
                    UcIgnore1.ApplyChanges();
                }
            }

            if (UcVariables1.ItemChanged)
            {
                if (Interaction.MsgBox("Current variable has been changed. Apply changes?", MsgBoxStyle.YesNo) == MsgBoxResult.Yes)
                {
                    UcVariables1.ApplyChanges();
                }
            }

            if (UcPreset1.ItemChanged)
            {
                if (Interaction.MsgBox("Current preset has been changed. Apply changes?", MsgBoxStyle.YesNo) == MsgBoxResult.Yes)
                {
                    UcPreset1.ApplyChanges();
                }
            }

            if (UcName1.ItemChanged)
            {
                if (Interaction.MsgBox("Current name has been changed. Apply changes?", MsgBoxStyle.YesNo) == MsgBoxResult.Yes)
                {
                    UcName1.ApplyChanges();
                }
            }

            if (UcHighlightStrings1.ItemChanged)
            {
                if (Interaction.MsgBox("Current highlight has been changed. Apply changes?", MsgBoxStyle.YesNo) == MsgBoxResult.Yes)
                {
                    UcHighlightStrings1.ApplyChanges();
                }
            }

            // If TypeOf Me.ParentForm Is FormMain Then
            // CType(Me.ParentForm, FormMain).SaveXMLConfig()
            // End If

            UcMacros1.SaveToFile();
            UcAliases1.SaveToFile();
            UcSubs1.SaveToFile();
            UcTriggers1.SaveToFile();
            UcIgnore1.SaveToFile();
            UcVariables1.SaveToFile();
            UcPreset1.SaveToFile();
            UcName1.SaveToFile();
            UcHighlightStrings1.SaveToFile();
        }

        private void FormConfig_Load(object sender, EventArgs e)
        {
            if (ParentForm is FormMain)
            {
                FormMain oFormMain = (FormMain)ParentForm;
                UcWindows1.FormParent = oFormMain;
                UcPreset1.FormParent = oFormMain;
                UcWindowSettings1.FormParent = oFormMain;
                UcWindowSettings1.RefreshSettings();
                UcMacros1.MacroList = oFormMain.m_oGlobals.MacroList;
                UcAliases1.AliasList = oFormMain.m_oGlobals.AliasList;
                UcSubs1.Globals = oFormMain.m_oGlobals;
                UcSubs1.SubstituteList = oFormMain.m_oGlobals.SubstituteList;
                UcTriggers1.Globals = oFormMain.m_oGlobals;
                UcTriggers1.TriggerList = oFormMain.m_oGlobals.TriggerList;
                UcIgnore1.Globals = oFormMain.m_oGlobals;
                UcIgnore1.IgnoreList = oFormMain.m_oGlobals.GagList;
                UcVariables1.VariableList = oFormMain.m_oGlobals.VariableList;
                UcPreset1.PresetList = oFormMain.m_oGlobals.PresetList;
                UcName1.NameList = oFormMain.m_oGlobals.NameList;
                UcHighlightStrings1.Globals = oFormMain.m_oGlobals;
                UcHighlightStrings1.HighlightList = oFormMain.m_oGlobals.HighlightList;
                UcHighlightStrings1.HighlightLineBeginsWith = oFormMain.m_oGlobals.HighlightBeginsWithList;
                UcHighlightStrings1.HighlightRegExp = oFormMain.m_oGlobals.HighlightRegExpList;
                UcPreset1.ColorDialogPicker.CustomColors = oFormMain.m_oGlobals.Config.PickerColors;
                UcName1.ColorDialogPicker.CustomColors = oFormMain.m_oGlobals.Config.PickerColors;
                UcHighlightStrings1.ColorDialogPicker.CustomColors = oFormMain.m_oGlobals.Config.PickerColors;
                UcHighlightStrings1.ItemChanged = false;
                UcWindows1.PopulateList();

                _lichConfig = oFormMain.m_oGlobals.Config;
                _TextBoxRubyPath.Text = _lichConfig.RubyPath;
                _TextBoxLichPath.Text = _lichConfig.LichPath;
                _TextBoxLichArguments.Text = _lichConfig.LichArguments;
                _TextBoxLichServer.Text = _lichConfig.LichServer;
                _TextBoxLichPort.Text = _lichConfig.LichPort.ToString();
                _TextBoxStartTimeout.Text = _lichConfig.LichStartPause.ToString();
            }
        }

        private void ApplyLichSettings()
        {
            if (_lichConfig == null) return;
            _lichConfig.RubyPath = _TextBoxRubyPath.Text.Trim();
            _lichConfig.LichPath = _TextBoxLichPath.Text.Trim();
            _lichConfig.LichArguments = _TextBoxLichArguments.Text.Trim();
            _lichConfig.LichServer = _TextBoxLichServer.Text.Trim();
            if (int.TryParse(_TextBoxLichPort.Text.Trim(), out int port)) _lichConfig.LichPort = port;
            if (int.TryParse(_TextBoxStartTimeout.Text.Trim(), out int timeout)) _lichConfig.LichStartPause = timeout;
            _lichConfig.Save();
        }

        private void ButtonBrowseRuby_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog();
            dlg.Title = "Select Ruby Executable";
            dlg.Filter = "Executables (*.exe)|*.exe|All Files (*.*)|*.*";
            if (File.Exists(_TextBoxRubyPath.Text)) dlg.InitialDirectory = Path.GetDirectoryName(_TextBoxRubyPath.Text);
            if (dlg.ShowDialog() == DialogResult.OK) _TextBoxRubyPath.Text = dlg.FileName;
        }

        private void ButtonBrowseLich_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog();
            dlg.Title = "Select Lich Script";
            dlg.Filter = "Ruby/Lich Files (*.rb;*.rbw)|*.rb;*.rbw|All Files (*.*)|*.*";
            if (File.Exists(_TextBoxLichPath.Text)) dlg.InitialDirectory = Path.GetDirectoryName(_TextBoxLichPath.Text);
            if (dlg.ShowDialog() == DialogResult.OK) _TextBoxLichPath.Text = dlg.FileName;
        }

        private void ButtonTestPaths_Click(object sender, EventArgs e)
        {
            void Check(Label resultLabel, string label, string path)
            {
                bool ok = File.Exists(path);
                resultLabel.ForeColor = ok ? Color.Green : Color.OrangeRed;
                resultLabel.Text = ok ? $"[OK]   {label}: {path}" : $"[FAIL] {label}: {path}";
            }

            Check(_LabelRubyResult, "Ruby", _TextBoxRubyPath.Text.Trim());
            Check(_LabelLichResult, "Lich", _TextBoxLichPath.Text.Trim());
        }
        public void OpenToHighlight(string text)
        {
            TabControlMain.SelectedTab = TabPageHighlights;
            TabControl2.SelectedIndex = 0;
            UcHighlightStrings1.PrepopulateNew(text);
        }

        public void OpenToTrigger(string text)
        {
            TabControlMain.SelectedTab = TabPageTriggers;
            UcTriggers1.PrepopulateNew(text);
        }

        public void OpenToSubstitute(string text)
        {
            TabControlMain.SelectedTab = TabPageSubs;
            UcSubs1.PrepopulateNew(text);
        }

        public void OpenToGag(string text)
        {
            TabControlMain.SelectedTab = TabPageIgnores;
            UcIgnore1.PrepopulateNew(text);
        }

        public void OpenToAlias(string text)
        {
            TabControlMain.SelectedTab = TabPageAliases;
            UcAliases1.PrepopulateNew(text);
        }
    }
}