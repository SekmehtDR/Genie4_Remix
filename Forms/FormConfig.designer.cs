using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;

namespace GenieClient
{
    [DesignerGenerated()]
    public partial class FormConfig : Form
    {

        // Form overrides dispose to clean up the component list.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            if (disposing && components is object)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        // Required by the Windows Form Designer
        #pragma warning disable 0649
        private System.ComponentModel.IContainer components;
        #pragma warning restore 0649

        // NOTE: The following procedure is required by the Windows Form Designer
        // It can be modified using the Windows Form Designer.  
        // Do not modify it using the code editor.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormConfig));
            this._TableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this._Cancel_Button = new System.Windows.Forms.Button();
            this._OK_Button = new System.Windows.Forms.Button();
            this._Panel1 = new System.Windows.Forms.Panel();
            this._TabControlMain = new System.Windows.Forms.TabControl();
            this._TabPageWindows = new System.Windows.Forms.TabPage();
            this._TabControlWindows = new System.Windows.Forms.TabControl();
            this._TabPage1 = new System.Windows.Forms.TabPage();
            this._UcWindows1 = new GenieClient.UCWindows();
            this._TabPage2 = new System.Windows.Forms.TabPage();
            this._UcWindowSettings1 = new GenieClient.UCWindowSettings();
            this._TabPageHighlights = new System.Windows.Forms.TabPage();
            this._TabControl2 = new System.Windows.Forms.TabControl();
            this._TabPageString = new System.Windows.Forms.TabPage();
            this._UcHighlightStrings1 = new GenieClient.UCHighlightStrings();
            this._TabPageNames = new System.Windows.Forms.TabPage();
            this._UcName1 = new GenieClient.UCName();
            this._TabPagePreset = new System.Windows.Forms.TabPage();
            this._UcPreset1 = new GenieClient.UCPreset();
            this._TabPageTriggers = new System.Windows.Forms.TabPage();
            this._UcTriggers1 = new GenieClient.UCTriggers();
            this._TabPageSubs = new System.Windows.Forms.TabPage();
            this._UcSubs1 = new GenieClient.UCSubs();
            this._TabPageIgnores = new System.Windows.Forms.TabPage();
            this._UcIgnore1 = new GenieClient.UCIgnore();
            this._TabPageAliases = new System.Windows.Forms.TabPage();
            this._UcAliases1 = new GenieClient.UCAliases();
            this._TabPageMacros = new System.Windows.Forms.TabPage();
            this._UcMacros1 = new GenieClient.UCMacros();
            this._TabPageVars = new System.Windows.Forms.TabPage();
            this._UcVariables1 = new GenieClient.UCVariables();
            this._TabPageVariables = new System.Windows.Forms.TabPage();
            this._TabPageLich = new System.Windows.Forms.TabPage();
            this._LabelRubyPath = new System.Windows.Forms.Label();
            this._TextBoxRubyPath = new System.Windows.Forms.TextBox();
            this._ButtonBrowseRuby = new System.Windows.Forms.Button();
            this._LabelLichPath = new System.Windows.Forms.Label();
            this._TextBoxLichPath = new System.Windows.Forms.TextBox();
            this._ButtonBrowseLich = new System.Windows.Forms.Button();
            this._LabelLichArguments = new System.Windows.Forms.Label();
            this._TextBoxLichArguments = new System.Windows.Forms.TextBox();
            this._LabelLichServer = new System.Windows.Forms.Label();
            this._TextBoxLichServer = new System.Windows.Forms.TextBox();
            this._LabelLichPort = new System.Windows.Forms.Label();
            this._TextBoxLichPort = new System.Windows.Forms.TextBox();
            this._LabelStartTimeout = new System.Windows.Forms.Label();
            this._TextBoxStartTimeout = new System.Windows.Forms.TextBox();
            this._LabelTimeoutUnit = new System.Windows.Forms.Label();
            this._ButtonTestPaths = new System.Windows.Forms.Button();
            this._LabelRubyResult = new System.Windows.Forms.Label();
            this._LabelLichResult = new System.Windows.Forms.Label();
            this._TableLayoutPanel1.SuspendLayout();
            this._Panel1.SuspendLayout();
            this._TabControlMain.SuspendLayout();
            this._TabPageWindows.SuspendLayout();
            this._TabControlWindows.SuspendLayout();
            this._TabPage1.SuspendLayout();
            this._TabPage2.SuspendLayout();
            this._TabPageHighlights.SuspendLayout();
            this._TabControl2.SuspendLayout();
            this._TabPageString.SuspendLayout();
            this._TabPageNames.SuspendLayout();
            this._TabPagePreset.SuspendLayout();
            this._TabPageTriggers.SuspendLayout();
            this._TabPageSubs.SuspendLayout();
            this._TabPageIgnores.SuspendLayout();
            this._TabPageAliases.SuspendLayout();
            this._TabPageMacros.SuspendLayout();
            this._TabPageVars.SuspendLayout();
            this._TabPageLich.SuspendLayout();
            this.SuspendLayout();
            // 
            // _TableLayoutPanel1
            // 
            this._TableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._TableLayoutPanel1.ColumnCount = 2;
            this._TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this._TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this._TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this._TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this._TableLayoutPanel1.Controls.Add(this._Cancel_Button, 0, 0);
            this._TableLayoutPanel1.Controls.Add(this._OK_Button, 0, 0);
            this._TableLayoutPanel1.Location = new System.Drawing.Point(593, 3);
            this._TableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._TableLayoutPanel1.Name = "_TableLayoutPanel1";
            this._TableLayoutPanel1.RowCount = 1;
            this._TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._TableLayoutPanel1.Size = new System.Drawing.Size(180, 33);
            this._TableLayoutPanel1.TabIndex = 1;
            // 
            // _Cancel_Button
            // 
            this._Cancel_Button.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._Cancel_Button.Location = new System.Drawing.Point(96, 3);
            this._Cancel_Button.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._Cancel_Button.Name = "_Cancel_Button";
            this._Cancel_Button.Size = new System.Drawing.Size(78, 27);
            this._Cancel_Button.TabIndex = 1;
            this._Cancel_Button.Text = "Cancel";
            this._Cancel_Button.Click += new System.EventHandler(this.Cancel_Button_Click);
            // 
            // _OK_Button
            // 
            this._OK_Button.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._OK_Button.Location = new System.Drawing.Point(6, 3);
            this._OK_Button.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._OK_Button.Name = "_OK_Button";
            this._OK_Button.Size = new System.Drawing.Size(78, 27);
            this._OK_Button.TabIndex = 0;
            this._OK_Button.Text = "OK";
            this._OK_Button.Click += new System.EventHandler(this.OK_Button_Click);
            // 
            // _Panel1
            // 
            this._Panel1.Controls.Add(this._TableLayoutPanel1);
            this._Panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._Panel1.Location = new System.Drawing.Point(0, 554);
            this._Panel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._Panel1.Name = "_Panel1";
            this._Panel1.Size = new System.Drawing.Size(776, 40);
            this._Panel1.TabIndex = 2;
            // 
            // _TabControlMain
            // 
            this._TabControlMain.Controls.Add(this._TabPageWindows);
            this._TabControlMain.Controls.Add(this._TabPageHighlights);
            this._TabControlMain.Controls.Add(this._TabPageTriggers);
            this._TabControlMain.Controls.Add(this._TabPageSubs);
            this._TabControlMain.Controls.Add(this._TabPageIgnores);
            this._TabControlMain.Controls.Add(this._TabPageAliases);
            this._TabControlMain.Controls.Add(this._TabPageMacros);
            this._TabControlMain.Controls.Add(this._TabPageVars);
            this._TabControlMain.Controls.Add(this._TabPageLich);
            this._TabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this._TabControlMain.Location = new System.Drawing.Point(0, 0);
            this._TabControlMain.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._TabControlMain.Name = "_TabControlMain";
            this._TabControlMain.SelectedIndex = 0;
            this._TabControlMain.Size = new System.Drawing.Size(776, 554);
            this._TabControlMain.TabIndex = 3;
            // 
            // _TabPageWindows
            // 
            this._TabPageWindows.Controls.Add(this._TabControlWindows);
            this._TabPageWindows.Location = new System.Drawing.Point(4, 24);
            this._TabPageWindows.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._TabPageWindows.Name = "_TabPageWindows";
            this._TabPageWindows.Size = new System.Drawing.Size(768, 526);
            this._TabPageWindows.TabIndex = 8;
            this._TabPageWindows.Text = "Layout";
            this._TabPageWindows.UseVisualStyleBackColor = true;
            // 
            // _TabControlWindows
            // 
            this._TabControlWindows.Controls.Add(this._TabPage1);
            this._TabControlWindows.Controls.Add(this._TabPage2);
            this._TabControlWindows.Dock = System.Windows.Forms.DockStyle.Fill;
            this._TabControlWindows.Location = new System.Drawing.Point(0, 0);
            this._TabControlWindows.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._TabControlWindows.Name = "_TabControlWindows";
            this._TabControlWindows.SelectedIndex = 0;
            this._TabControlWindows.Size = new System.Drawing.Size(768, 526);
            this._TabControlWindows.TabIndex = 1;
            // 
            // _TabPage1
            // 
            this._TabPage1.Controls.Add(this._UcWindows1);
            this._TabPage1.Location = new System.Drawing.Point(4, 24);
            this._TabPage1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._TabPage1.Name = "_TabPage1";
            this._TabPage1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._TabPage1.Size = new System.Drawing.Size(760, 498);
            this._TabPage1.TabIndex = 0;
            this._TabPage1.Text = "Windows";
            this._TabPage1.UseVisualStyleBackColor = true;
            // 
            // _UcWindows1
            // 
            this._UcWindows1.Dock = System.Windows.Forms.DockStyle.Fill;
            this._UcWindows1.FormParent = null;
            this._UcWindows1.Location = new System.Drawing.Point(4, 3);
            this._UcWindows1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this._UcWindows1.Name = "_UcWindows1";
            this._UcWindows1.Size = new System.Drawing.Size(752, 492);
            this._UcWindows1.TabIndex = 0;
            // 
            // _TabPage2
            // 
            this._TabPage2.Controls.Add(this._UcWindowSettings1);
            this._TabPage2.Location = new System.Drawing.Point(4, 24);
            this._TabPage2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._TabPage2.Name = "_TabPage2";
            this._TabPage2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._TabPage2.Size = new System.Drawing.Size(758, 496);
            this._TabPage2.TabIndex = 1;
            this._TabPage2.Text = "Settings";
            this._TabPage2.UseVisualStyleBackColor = true;
            // 
            // _UcWindowSettings1
            // 
            this._UcWindowSettings1.Dock = System.Windows.Forms.DockStyle.Fill;
            this._UcWindowSettings1.FormParent = null;
            this._UcWindowSettings1.Location = new System.Drawing.Point(4, 3);
            this._UcWindowSettings1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this._UcWindowSettings1.Name = "_UcWindowSettings1";
            this._UcWindowSettings1.Size = new System.Drawing.Size(750, 490);
            this._UcWindowSettings1.TabIndex = 0;
            // 
            // _TabPageHighlights
            // 
            this._TabPageHighlights.Controls.Add(this._TabControl2);
            this._TabPageHighlights.Location = new System.Drawing.Point(4, 24);
            this._TabPageHighlights.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._TabPageHighlights.Name = "_TabPageHighlights";
            this._TabPageHighlights.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._TabPageHighlights.Size = new System.Drawing.Size(768, 526);
            this._TabPageHighlights.TabIndex = 1;
            this._TabPageHighlights.Text = "Highlights";
            this._TabPageHighlights.UseVisualStyleBackColor = true;
            // 
            // _TabControl2
            // 
            this._TabControl2.Controls.Add(this._TabPageString);
            this._TabControl2.Controls.Add(this._TabPageNames);
            this._TabControl2.Controls.Add(this._TabPagePreset);
            this._TabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this._TabControl2.Location = new System.Drawing.Point(4, 3);
            this._TabControl2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._TabControl2.Name = "_TabControl2";
            this._TabControl2.SelectedIndex = 0;
            this._TabControl2.Size = new System.Drawing.Size(760, 520);
            this._TabControl2.TabIndex = 0;
            // 
            // _TabPageString
            // 
            this._TabPageString.Controls.Add(this._UcHighlightStrings1);
            this._TabPageString.Location = new System.Drawing.Point(4, 24);
            this._TabPageString.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._TabPageString.Name = "_TabPageString";
            this._TabPageString.Size = new System.Drawing.Size(752, 492);
            this._TabPageString.TabIndex = 4;
            this._TabPageString.Text = "Strings";
            this._TabPageString.UseVisualStyleBackColor = true;
            // 
            // _UcHighlightStrings1
            // 
            this._UcHighlightStrings1.Dock = System.Windows.Forms.DockStyle.Fill;
            this._UcHighlightStrings1.Globals = null;
            this._UcHighlightStrings1.HighlightLineBeginsWith = null;
            this._UcHighlightStrings1.HighlightList = null;
            this._UcHighlightStrings1.HighlightRegExp = null;
            this._UcHighlightStrings1.ItemChanged = false;
            this._UcHighlightStrings1.Location = new System.Drawing.Point(0, 0);
            this._UcHighlightStrings1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this._UcHighlightStrings1.Name = "_UcHighlightStrings1";
            this._UcHighlightStrings1.Size = new System.Drawing.Size(750, 487);
            this._UcHighlightStrings1.TabIndex = 2;
            // 
            // _TabPageNames
            // 
            this._TabPageNames.Controls.Add(this._UcName1);
            this._TabPageNames.Location = new System.Drawing.Point(4, 24);
            this._TabPageNames.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._TabPageNames.Name = "_TabPageNames";
            this._TabPageNames.Size = new System.Drawing.Size(752, 492);
            this._TabPageNames.TabIndex = 2;
            this._TabPageNames.Text = "Names";
            this._TabPageNames.UseVisualStyleBackColor = true;
            // 
            // _UcName1
            // 
            this._UcName1.Dock = System.Windows.Forms.DockStyle.Fill;
            this._UcName1.Location = new System.Drawing.Point(0, 0);
            this._UcName1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this._UcName1.Name = "_UcName1";
            this._UcName1.NameList = null;
            this._UcName1.Size = new System.Drawing.Size(752, 492);
            this._UcName1.TabIndex = 1;
            // 
            // _TabPagePreset
            // 
            this._TabPagePreset.Controls.Add(this._UcPreset1);
            this._TabPagePreset.Location = new System.Drawing.Point(4, 24);
            this._TabPagePreset.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._TabPagePreset.Name = "_TabPagePreset";
            this._TabPagePreset.Size = new System.Drawing.Size(752, 492);
            this._TabPagePreset.TabIndex = 3;
            this._TabPagePreset.Text = "Presets";
            this._TabPagePreset.UseVisualStyleBackColor = true;
            // 
            // _UcPreset1
            // 
            this._UcPreset1.Dock = System.Windows.Forms.DockStyle.Fill;
            this._UcPreset1.FormParent = null;
            this._UcPreset1.Location = new System.Drawing.Point(0, 0);
            this._UcPreset1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this._UcPreset1.Name = "_UcPreset1";
            this._UcPreset1.PresetList = null;
            this._UcPreset1.Size = new System.Drawing.Size(752, 492);
            this._UcPreset1.TabIndex = 3;
            // 
            // _TabPageTriggers
            // 
            this._TabPageTriggers.Controls.Add(this._UcTriggers1);
            this._TabPageTriggers.Location = new System.Drawing.Point(4, 24);
            this._TabPageTriggers.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._TabPageTriggers.Name = "_TabPageTriggers";
            this._TabPageTriggers.Size = new System.Drawing.Size(768, 526);
            this._TabPageTriggers.TabIndex = 6;
            this._TabPageTriggers.Text = "Triggers";
            this._TabPageTriggers.UseVisualStyleBackColor = true;
            // 
            // _UcTriggers1
            // 
            this._UcTriggers1.Dock = System.Windows.Forms.DockStyle.Fill;
            this._UcTriggers1.Globals = null;
            this._UcTriggers1.Location = new System.Drawing.Point(0, 0);
            this._UcTriggers1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this._UcTriggers1.Name = "_UcTriggers1";
            this._UcTriggers1.Size = new System.Drawing.Size(766, 524);
            this._UcTriggers1.TabIndex = 0;
            this._UcTriggers1.TriggerList = null;
            // 
            // _TabPageSubs
            // 
            this._TabPageSubs.Controls.Add(this._UcSubs1);
            this._TabPageSubs.Location = new System.Drawing.Point(4, 24);
            this._TabPageSubs.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._TabPageSubs.Name = "_TabPageSubs";
            this._TabPageSubs.Size = new System.Drawing.Size(768, 526);
            this._TabPageSubs.TabIndex = 2;
            this._TabPageSubs.Text = "Substitutes";
            this._TabPageSubs.UseVisualStyleBackColor = true;
            // 
            // _UcSubs1
            // 
            this._UcSubs1.Dock = System.Windows.Forms.DockStyle.Fill;
            this._UcSubs1.Globals = null;
            this._UcSubs1.Location = new System.Drawing.Point(0, 0);
            this._UcSubs1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this._UcSubs1.Name = "_UcSubs1";
            this._UcSubs1.Size = new System.Drawing.Size(766, 524);
            this._UcSubs1.SubstituteList = null;
            this._UcSubs1.TabIndex = 0;
            // 
            // _TabPageIgnores
            // 
            this._TabPageIgnores.Controls.Add(this._UcIgnore1);
            this._TabPageIgnores.Location = new System.Drawing.Point(4, 24);
            this._TabPageIgnores.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._TabPageIgnores.Name = "_TabPageIgnores";
            this._TabPageIgnores.Size = new System.Drawing.Size(768, 526);
            this._TabPageIgnores.TabIndex = 3;
            this._TabPageIgnores.Text = "Gags";
            this._TabPageIgnores.UseVisualStyleBackColor = true;
            // 
            // _UcIgnore1
            // 
            this._UcIgnore1.Dock = System.Windows.Forms.DockStyle.Fill;
            this._UcIgnore1.Globals = null;
            this._UcIgnore1.IgnoreList = null;
            this._UcIgnore1.Location = new System.Drawing.Point(0, 0);
            this._UcIgnore1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this._UcIgnore1.Name = "_UcIgnore1";
            this._UcIgnore1.Size = new System.Drawing.Size(766, 524);
            this._UcIgnore1.TabIndex = 1;
            // 
            // _TabPageAliases
            // 
            this._TabPageAliases.Controls.Add(this._UcAliases1);
            this._TabPageAliases.Location = new System.Drawing.Point(4, 24);
            this._TabPageAliases.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._TabPageAliases.Name = "_TabPageAliases";
            this._TabPageAliases.Size = new System.Drawing.Size(768, 526);
            this._TabPageAliases.TabIndex = 4;
            this._TabPageAliases.Text = "Aliases";
            this._TabPageAliases.UseVisualStyleBackColor = true;
            // 
            // _UcAliases1
            // 
            this._UcAliases1.AliasList = null;
            this._UcAliases1.Dock = System.Windows.Forms.DockStyle.Fill;
            this._UcAliases1.Location = new System.Drawing.Point(0, 0);
            this._UcAliases1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this._UcAliases1.Name = "_UcAliases1";
            this._UcAliases1.Size = new System.Drawing.Size(768, 526);
            this._UcAliases1.TabIndex = 0;
            // 
            // _TabPageMacros
            // 
            this._TabPageMacros.Controls.Add(this._UcMacros1);
            this._TabPageMacros.Location = new System.Drawing.Point(4, 24);
            this._TabPageMacros.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._TabPageMacros.Name = "_TabPageMacros";
            this._TabPageMacros.Size = new System.Drawing.Size(768, 526);
            this._TabPageMacros.TabIndex = 5;
            this._TabPageMacros.Text = "Macros";
            this._TabPageMacros.UseVisualStyleBackColor = true;
            // 
            // _UcMacros1
            // 
            this._UcMacros1.Dock = System.Windows.Forms.DockStyle.Fill;
            this._UcMacros1.Location = new System.Drawing.Point(0, 0);
            this._UcMacros1.MacroList = null;
            this._UcMacros1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this._UcMacros1.Name = "_UcMacros1";
            this._UcMacros1.Size = new System.Drawing.Size(768, 526);
            this._UcMacros1.TabIndex = 0;
            // 
            // _TabPageVars
            // 
            this._TabPageVars.Controls.Add(this._UcVariables1);
            this._TabPageVars.Location = new System.Drawing.Point(4, 24);
            this._TabPageVars.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._TabPageVars.Name = "_TabPageVars";
            this._TabPageVars.Size = new System.Drawing.Size(768, 526);
            this._TabPageVars.TabIndex = 9;
            this._TabPageVars.Text = "Variables";
            this._TabPageVars.UseVisualStyleBackColor = true;
            // 
            // _UcVariables1
            // 
            this._UcVariables1.Dock = System.Windows.Forms.DockStyle.Fill;
            this._UcVariables1.Location = new System.Drawing.Point(0, 0);
            this._UcVariables1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this._UcVariables1.Name = "_UcVariables1";
            this._UcVariables1.Size = new System.Drawing.Size(768, 526);
            this._UcVariables1.TabIndex = 0;
            this._UcVariables1.VariableList = null;
            //
            // _TabPageLich
            //
            this._TabPageLich.Controls.Add(this._LabelRubyPath);
            this._TabPageLich.Controls.Add(this._TextBoxRubyPath);
            this._TabPageLich.Controls.Add(this._ButtonBrowseRuby);
            this._TabPageLich.Controls.Add(this._LabelLichPath);
            this._TabPageLich.Controls.Add(this._TextBoxLichPath);
            this._TabPageLich.Controls.Add(this._ButtonBrowseLich);
            this._TabPageLich.Controls.Add(this._LabelLichArguments);
            this._TabPageLich.Controls.Add(this._TextBoxLichArguments);
            this._TabPageLich.Controls.Add(this._LabelLichServer);
            this._TabPageLich.Controls.Add(this._TextBoxLichServer);
            this._TabPageLich.Controls.Add(this._LabelLichPort);
            this._TabPageLich.Controls.Add(this._TextBoxLichPort);
            this._TabPageLich.Controls.Add(this._LabelStartTimeout);
            this._TabPageLich.Controls.Add(this._TextBoxStartTimeout);
            this._TabPageLich.Controls.Add(this._LabelTimeoutUnit);
            this._TabPageLich.Controls.Add(this._ButtonTestPaths);
            this._TabPageLich.Controls.Add(this._LabelRubyResult);
            this._TabPageLich.Controls.Add(this._LabelLichResult);
            this._TabPageLich.Location = new System.Drawing.Point(4, 24);
            this._TabPageLich.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._TabPageLich.Name = "_TabPageLich";
            this._TabPageLich.Size = new System.Drawing.Size(768, 526);
            this._TabPageLich.TabIndex = 10;
            this._TabPageLich.Text = "Lich";
            this._TabPageLich.UseVisualStyleBackColor = true;
            //
            // _LabelRubyPath
            //
            this._LabelRubyPath.AutoSize = true;
            this._LabelRubyPath.Location = new System.Drawing.Point(15, 25);
            this._LabelRubyPath.Name = "_LabelRubyPath";
            this._LabelRubyPath.Size = new System.Drawing.Size(63, 15);
            this._LabelRubyPath.Text = "Ruby Path (rubyw.exe):";
            //
            // _TextBoxRubyPath
            //
            this._TextBoxRubyPath.Location = new System.Drawing.Point(155, 22);
            this._TextBoxRubyPath.Name = "_TextBoxRubyPath";
            this._TextBoxRubyPath.Size = new System.Drawing.Size(370, 23);
            this._TextBoxRubyPath.TabIndex = 0;
            //
            // _ButtonBrowseRuby
            //
            this._ButtonBrowseRuby.Location = new System.Drawing.Point(535, 21);
            this._ButtonBrowseRuby.Name = "_ButtonBrowseRuby";
            this._ButtonBrowseRuby.Size = new System.Drawing.Size(75, 25);
            this._ButtonBrowseRuby.TabIndex = 1;
            this._ButtonBrowseRuby.Text = "Browse...";
            this._ButtonBrowseRuby.Click += new System.EventHandler(this.ButtonBrowseRuby_Click);
            //
            // _LabelLichPath
            //
            this._LabelLichPath.AutoSize = true;
            this._LabelLichPath.Location = new System.Drawing.Point(15, 65);
            this._LabelLichPath.Name = "_LabelLichPath";
            this._LabelLichPath.Size = new System.Drawing.Size(57, 15);
            this._LabelLichPath.Text = "Lich Path (lich.rbw):";
            //
            // _TextBoxLichPath
            //
            this._TextBoxLichPath.Location = new System.Drawing.Point(155, 62);
            this._TextBoxLichPath.Name = "_TextBoxLichPath";
            this._TextBoxLichPath.Size = new System.Drawing.Size(370, 23);
            this._TextBoxLichPath.TabIndex = 2;
            //
            // _ButtonBrowseLich
            //
            this._ButtonBrowseLich.Location = new System.Drawing.Point(535, 61);
            this._ButtonBrowseLich.Name = "_ButtonBrowseLich";
            this._ButtonBrowseLich.Size = new System.Drawing.Size(75, 25);
            this._ButtonBrowseLich.TabIndex = 3;
            this._ButtonBrowseLich.Text = "Browse...";
            this._ButtonBrowseLich.Click += new System.EventHandler(this.ButtonBrowseLich_Click);
            //
            // _LabelLichArguments
            //
            this._LabelLichArguments.AutoSize = true;
            this._LabelLichArguments.Location = new System.Drawing.Point(15, 105);
            this._LabelLichArguments.Name = "_LabelLichArguments";
            this._LabelLichArguments.Size = new System.Drawing.Size(70, 15);
            this._LabelLichArguments.Text = "Arguments:";
            //
            // _TextBoxLichArguments
            //
            this._TextBoxLichArguments.Location = new System.Drawing.Point(155, 102);
            this._TextBoxLichArguments.Name = "_TextBoxLichArguments";
            this._TextBoxLichArguments.Size = new System.Drawing.Size(455, 23);
            this._TextBoxLichArguments.TabIndex = 4;
            //
            // _LabelLichServer
            //
            this._LabelLichServer.AutoSize = true;
            this._LabelLichServer.Location = new System.Drawing.Point(15, 145);
            this._LabelLichServer.Name = "_LabelLichServer";
            this._LabelLichServer.Size = new System.Drawing.Size(45, 15);
            this._LabelLichServer.Text = "Server:";
            //
            // _TextBoxLichServer
            //
            this._TextBoxLichServer.Location = new System.Drawing.Point(155, 142);
            this._TextBoxLichServer.Name = "_TextBoxLichServer";
            this._TextBoxLichServer.Size = new System.Drawing.Size(200, 23);
            this._TextBoxLichServer.TabIndex = 5;
            //
            // _LabelLichPort
            //
            this._LabelLichPort.AutoSize = true;
            this._LabelLichPort.Location = new System.Drawing.Point(15, 185);
            this._LabelLichPort.Name = "_LabelLichPort";
            this._LabelLichPort.Size = new System.Drawing.Size(32, 15);
            this._LabelLichPort.Text = "Port:";
            //
            // _TextBoxLichPort
            //
            this._TextBoxLichPort.Location = new System.Drawing.Point(155, 182);
            this._TextBoxLichPort.Name = "_TextBoxLichPort";
            this._TextBoxLichPort.Size = new System.Drawing.Size(80, 23);
            this._TextBoxLichPort.TabIndex = 6;
            //
            // _LabelStartTimeout
            //
            this._LabelStartTimeout.AutoSize = true;
            this._LabelStartTimeout.Location = new System.Drawing.Point(15, 225);
            this._LabelStartTimeout.Name = "_LabelStartTimeout";
            this._LabelStartTimeout.Size = new System.Drawing.Size(86, 15);
            this._LabelStartTimeout.Text = "Start Timeout:";
            //
            // _TextBoxStartTimeout
            //
            this._TextBoxStartTimeout.Location = new System.Drawing.Point(155, 222);
            this._TextBoxStartTimeout.Name = "_TextBoxStartTimeout";
            this._TextBoxStartTimeout.Size = new System.Drawing.Size(50, 23);
            this._TextBoxStartTimeout.TabIndex = 7;
            //
            // _LabelTimeoutUnit
            //
            this._LabelTimeoutUnit.AutoSize = true;
            this._LabelTimeoutUnit.Location = new System.Drawing.Point(212, 225);
            this._LabelTimeoutUnit.Name = "_LabelTimeoutUnit";
            this._LabelTimeoutUnit.Size = new System.Drawing.Size(52, 15);
            this._LabelTimeoutUnit.Text = "seconds";
            //
            // _ButtonTestPaths
            //
            this._ButtonTestPaths.Location = new System.Drawing.Point(155, 265);
            this._ButtonTestPaths.Name = "_ButtonTestPaths";
            this._ButtonTestPaths.Size = new System.Drawing.Size(90, 27);
            this._ButtonTestPaths.TabIndex = 8;
            this._ButtonTestPaths.Text = "Test Paths";
            this._ButtonTestPaths.Click += new System.EventHandler(this.ButtonTestPaths_Click);
            //
            // _LabelRubyResult
            //
            this._LabelRubyResult.AutoSize = true;
            this._LabelRubyResult.Location = new System.Drawing.Point(155, 300);
            this._LabelRubyResult.Name = "_LabelRubyResult";
            this._LabelRubyResult.TabIndex = 9;
            this._LabelRubyResult.Text = "";
            //
            // _LabelLichResult
            //
            this._LabelLichResult.AutoSize = true;
            this._LabelLichResult.Location = new System.Drawing.Point(155, 320);
            this._LabelLichResult.Name = "_LabelLichResult";
            this._LabelLichResult.TabIndex = 10;
            this._LabelLichResult.Text = "";
            //
            // _TabPageVariables
            //
            this._TabPageVariables.Location = new System.Drawing.Point(4, 22);
            this._TabPageVariables.Name = "_TabPageVariables";
            this._TabPageVariables.Size = new System.Drawing.Size(654, 378);
            this._TabPageVariables.TabIndex = 9;
            this._TabPageVariables.Text = "Variables";
            this._TabPageVariables.UseVisualStyleBackColor = true;
            // 
            // FormConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(776, 594);
            this.Controls.Add(this._TabControlMain);
            this.Controls.Add(this._Panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormConfig";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configuration";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FormConfig_Load);
            this._TableLayoutPanel1.ResumeLayout(false);
            this._Panel1.ResumeLayout(false);
            this._TabControlMain.ResumeLayout(false);
            this._TabPageWindows.ResumeLayout(false);
            this._TabControlWindows.ResumeLayout(false);
            this._TabPage1.ResumeLayout(false);
            this._TabPage2.ResumeLayout(false);
            this._TabPageHighlights.ResumeLayout(false);
            this._TabControl2.ResumeLayout(false);
            this._TabPageString.ResumeLayout(false);
            this._TabPageNames.ResumeLayout(false);
            this._TabPagePreset.ResumeLayout(false);
            this._TabPageTriggers.ResumeLayout(false);
            this._TabPageSubs.ResumeLayout(false);
            this._TabPageIgnores.ResumeLayout(false);
            this._TabPageAliases.ResumeLayout(false);
            this._TabPageMacros.ResumeLayout(false);
            this._TabPageVars.ResumeLayout(false);
            this._TabPageLich.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private TableLayoutPanel _TableLayoutPanel1;

        internal TableLayoutPanel TableLayoutPanel1
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _TableLayoutPanel1;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_TableLayoutPanel1 != null)
                {
                }

                _TableLayoutPanel1 = value;
                if (_TableLayoutPanel1 != null)
                {
                }
            }
        }

        private Button _Cancel_Button;

        internal Button Cancel_Button
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _Cancel_Button;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_Cancel_Button != null)
                {
                    _Cancel_Button.Click -= Cancel_Button_Click;
                }

                _Cancel_Button = value;
                if (_Cancel_Button != null)
                {
                    _Cancel_Button.Click += Cancel_Button_Click;
                }
            }
        }

        private Button _OK_Button;

        internal Button OK_Button
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _OK_Button;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_OK_Button != null)
                {
                    _OK_Button.Click -= OK_Button_Click;
                }

                _OK_Button = value;
                if (_OK_Button != null)
                {
                    _OK_Button.Click += OK_Button_Click;
                }
            }
        }

        private Panel _Panel1;

        internal Panel Panel1
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _Panel1;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_Panel1 != null)
                {
                }

                _Panel1 = value;
                if (_Panel1 != null)
                {
                }
            }
        }

        private TabControl _TabControlMain;

        internal TabControl TabControlMain
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _TabControlMain;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_TabControlMain != null)
                {
                }

                _TabControlMain = value;
                if (_TabControlMain != null)
                {
                }
            }
        }

        private TabPage _TabPageHighlights;

        internal TabPage TabPageHighlights
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _TabPageHighlights;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_TabPageHighlights != null)
                {
                }

                _TabPageHighlights = value;
                if (_TabPageHighlights != null)
                {
                }
            }
        }

        private TabPage _TabPageSubs;

        internal TabPage TabPageSubs
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _TabPageSubs;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_TabPageSubs != null)
                {
                }

                _TabPageSubs = value;
                if (_TabPageSubs != null)
                {
                }
            }
        }

        private TabPage _TabPageIgnores;

        internal TabPage TabPageIgnores
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _TabPageIgnores;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_TabPageIgnores != null)
                {
                }

                _TabPageIgnores = value;
                if (_TabPageIgnores != null)
                {
                }
            }
        }

        private TabPage _TabPageAliases;

        internal TabPage TabPageAliases
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _TabPageAliases;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_TabPageAliases != null)
                {
                }

                _TabPageAliases = value;
                if (_TabPageAliases != null)
                {
                }
            }
        }

        private TabPage _TabPageMacros;

        internal TabPage TabPageMacros
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _TabPageMacros;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_TabPageMacros != null)
                {
                }

                _TabPageMacros = value;
                if (_TabPageMacros != null)
                {
                }
            }
        }

        private TabPage _TabPageTriggers;

        internal TabPage TabPageTriggers
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _TabPageTriggers;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_TabPageTriggers != null)
                {
                }

                _TabPageTriggers = value;
                if (_TabPageTriggers != null)
                {
                }
            }
        }

        private TabControl _TabControl2;

        internal TabControl TabControl2
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _TabControl2;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_TabControl2 != null)
                {
                }

                _TabControl2 = value;
                if (_TabControl2 != null)
                {
                }
            }
        }

        private TabPage _TabPageNames;

        internal TabPage TabPageNames
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _TabPageNames;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_TabPageNames != null)
                {
                }

                _TabPageNames = value;
                if (_TabPageNames != null)
                {
                }
            }
        }

        private TabPage _TabPageWindows;

        internal TabPage TabPageWindows
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _TabPageWindows;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_TabPageWindows != null)
                {
                }

                _TabPageWindows = value;
                if (_TabPageWindows != null)
                {
                }
            }
        }

        private TabPage _TabPageVariables;

        internal TabPage TabPageVariables
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _TabPageVariables;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_TabPageVariables != null)
                {
                }

                _TabPageVariables = value;
                if (_TabPageVariables != null)
                {
                }
            }
        }

        private TabPage _TabPageLich;
        private Label _LabelRubyPath;
        private TextBox _TextBoxRubyPath;
        private Button _ButtonBrowseRuby;
        private Label _LabelLichPath;
        private TextBox _TextBoxLichPath;
        private Button _ButtonBrowseLich;
        private Label _LabelLichArguments;
        private TextBox _TextBoxLichArguments;
        private Label _LabelLichServer;
        private TextBox _TextBoxLichServer;
        private Label _LabelLichPort;
        private TextBox _TextBoxLichPort;
        private Label _LabelStartTimeout;
        private TextBox _TextBoxStartTimeout;
        private Label _LabelTimeoutUnit;
        private Button _ButtonTestPaths;
        private Label _LabelRubyResult;
        private Label _LabelLichResult;

        private UCWindows _UcWindows1;

        internal UCWindows UcWindows1
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _UcWindows1;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_UcWindows1 != null)
                {
                }

                _UcWindows1 = value;
                if (_UcWindows1 != null)
                {
                }
            }
        }

        private UCMacros _UcMacros1;

        internal UCMacros UcMacros1
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _UcMacros1;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_UcMacros1 != null)
                {
                }

                _UcMacros1 = value;
                if (_UcMacros1 != null)
                {
                }
            }
        }

        private UCAliases _UcAliases1;

        internal UCAliases UcAliases1
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _UcAliases1;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_UcAliases1 != null)
                {
                }

                _UcAliases1 = value;
                if (_UcAliases1 != null)
                {
                }
            }
        }

        private UCSubs _UcSubs1;

        internal UCSubs UcSubs1
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _UcSubs1;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_UcSubs1 != null)
                {
                }

                _UcSubs1 = value;
                if (_UcSubs1 != null)
                {
                }
            }
        }

        private UCTriggers _UcTriggers1;

        internal UCTriggers UcTriggers1
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _UcTriggers1;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_UcTriggers1 != null)
                {
                }

                _UcTriggers1 = value;
                if (_UcTriggers1 != null)
                {
                }
            }
        }

        private UCIgnore _UcIgnore1;

        internal UCIgnore UcIgnore1
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _UcIgnore1;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_UcIgnore1 != null)
                {
                }

                _UcIgnore1 = value;
                if (_UcIgnore1 != null)
                {
                }
            }
        }

        private TabPage _TabPageVars;

        internal TabPage TabPageVars
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _TabPageVars;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_TabPageVars != null)
                {
                }

                _TabPageVars = value;
                if (_TabPageVars != null)
                {
                }
            }
        }

        private UCVariables _UcVariables1;

        internal UCVariables UcVariables1
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _UcVariables1;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_UcVariables1 != null)
                {
                }

                _UcVariables1 = value;
                if (_UcVariables1 != null)
                {
                }
            }
        }

        private UCName _UcName1;

        internal UCName UcName1
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _UcName1;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_UcName1 != null)
                {
                }

                _UcName1 = value;
                if (_UcName1 != null)
                {
                }
            }
        }

        private TabPage _TabPagePreset;

        internal TabPage TabPagePreset
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _TabPagePreset;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_TabPagePreset != null)
                {
                }

                _TabPagePreset = value;
                if (_TabPagePreset != null)
                {
                }
            }
        }

        private UCPreset _UcPreset1;

        internal UCPreset UcPreset1
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _UcPreset1;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_UcPreset1 != null)
                {
                }

                _UcPreset1 = value;
                if (_UcPreset1 != null)
                {
                }
            }
        }

        private TabPage _TabPageString;

        internal TabPage TabPageString
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _TabPageString;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_TabPageString != null)
                {
                }

                _TabPageString = value;
                if (_TabPageString != null)
                {
                }
            }
        }

        private UCHighlightStrings _UcHighlightStrings1;

        internal UCHighlightStrings UcHighlightStrings1
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _UcHighlightStrings1;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_UcHighlightStrings1 != null)
                {
                }

                _UcHighlightStrings1 = value;
                if (_UcHighlightStrings1 != null)
                {
                }
            }
        }

        private TabControl _TabControlWindows;

        internal TabControl TabControlWindows
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _TabControlWindows;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_TabControlWindows != null)
                {
                }

                _TabControlWindows = value;
                if (_TabControlWindows != null)
                {
                }
            }
        }

        private TabPage _TabPage1;

        internal TabPage TabPage1
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _TabPage1;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_TabPage1 != null)
                {
                }

                _TabPage1 = value;
                if (_TabPage1 != null)
                {
                }
            }
        }

        private TabPage _TabPage2;

        internal TabPage TabPage2
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _TabPage2;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_TabPage2 != null)
                {
                }

                _TabPage2 = value;
                if (_TabPage2 != null)
                {
                }
            }
        }

        private UCWindowSettings _UcWindowSettings1;

        internal UCWindowSettings UcWindowSettings1
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _UcWindowSettings1;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_UcWindowSettings1 != null)
                {
                }

                _UcWindowSettings1 = value;
                if (_UcWindowSettings1 != null)
                {
                }
            }
        }
    }
}