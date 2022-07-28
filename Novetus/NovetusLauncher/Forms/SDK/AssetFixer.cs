﻿#region Usings
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
#endregion

public partial class AssetFixer : Form
{
    #region Private Variables
    public Provider[] contentProviders;
    private string url = "";
    private bool isWebSite = false;
    private RobloxFileType currentType;
    private string path;
    private string name;
    private int errors = 0;
    private bool hasOverrideWarningOpenedOnce = false;
    #endregion

    #region Constructor
    public AssetFixer()
    {
        InitializeComponent();
    }
    #endregion

    #region Form Events

    #region Load/Close Events
    private void AssetSDK_Load(object sender, EventArgs e)
    {
        //shared
        if (File.Exists(GlobalPaths.ConfigDir + "\\" + GlobalPaths.ContentProviderXMLName))
        {
            contentProviders = OnlineClothing.GetContentProviders();

            for (int i = 0; i < contentProviders.Length; i++)
            {
                if (contentProviders[i].URL.Contains("?id="))
                {
                    URLSelection.Items.Add(contentProviders[i].Name);
                }
            }
        }

        URLSelection.Items.Add("https://www.roblox.com/catalog/");
        URLSelection.Items.Add("https://www.roblox.com/library/");
        isWebSite = false;

        URLSelection.SelectedItem = URLSelection.Items[0];

        //asset localizer
        AssetLocalization_SaveBackups.Checked = GlobalVars.UserConfiguration.AssetSDKFixerSaveBackups;
        AssetLocalization_AssetTypeBox.SelectedItem = "RBXL";

        SetAssetCachePaths();

        GlobalFuncs.CreateAssetCacheDirectories();
    }

    void AssetSDK_Close(object sender, CancelEventArgs e)
    {
        SetAssetCachePaths();

        //asset localizer
        AssetLocalization_BackgroundWorker.CancelAsync();
    }

    private void URLSelection_SelectedIndexChanged(object sender, EventArgs e)
    {
        SetURL();
    }

    private void URLOverrideBox_Click(object sender, EventArgs e)
    {
        if (hasOverrideWarningOpenedOnce == false && !GlobalVars.UserConfiguration.DisabledAssetSDKHelp)
        {
            MessageBox.Show("By using the custom URL setting, you will override any selected entry in the default URL list. Keep this in mind before downloading anything with this option.\n\nAlso, the URL must be a asset url with 'asset/?id=' at the end of it in order for the Asset Downloader to work smoothly.", "Asset Fixer - URL Override Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            hasOverrideWarningOpenedOnce = true;
        }
    }

    private void URLOverrideBox_TextChanged(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(URLOverrideBox.Text))
        {
            URLSelection.Enabled = false;
            url = URLOverrideBox.Text;
        }
        else
        {
            URLSelection.Enabled = true;
            SetURL();
        }

        MessageBox.Show(url);
    }

    void SetURL()
    {
        if (URLSelection.SelectedItem.Equals("https://www.roblox.com/catalog/") || URLSelection.SelectedItem.Equals("https://www.roblox.com/library/"))
        {
            url = URLSelection.SelectedItem.ToString();
            isWebSite = true;
        }
        else
        {
            Provider pro = OnlineClothing.FindContentProviderByName(contentProviders, URLSelection.SelectedItem.ToString());
            if (pro != null)
            {
                url = pro.URL;
                isWebSite = false;
            }
        }
    }
    #endregion

    #region Asset Fixer

    public static OpenFileDialog LoadROBLOXFileDialog(RobloxFileType type)
    {
        string typeFilter = "";

        switch (type)
        {
            case RobloxFileType.RBXL:
                typeFilter = "Roblox Level (*.rbxl)|*.rbxl|Roblox Level (*.rbxlx)|*.rbxlx";
                break;
            case RobloxFileType.Script:
                typeFilter = "Lua Script (*.lua)|*.lua";
                break;
            default:
                typeFilter = "Roblox Model (*.rbxm)|*.rbxm";
                break;
        }

        OpenFileDialog openFileDialog1 = new OpenFileDialog();
        openFileDialog1.Filter = typeFilter;
        openFileDialog1.Title = "Open Roblox level or model";

        return openFileDialog1;
    }

    void ProgressChangedEvent()
    {
        AssetFixer_ProgressBar.Value += 1;
        AssetFixer_ProgressLabel.Text = "Progress: " + AssetFixer_ProgressBar.Value.ToString() + "/" + AssetFixer_ProgressBar.Maximum.ToString();
    }

    public void FixURLSOrDownloadFromScript(string filepath, string savefilepath, string inGameDir, bool useURLs, string url)
    {
        string[] file = File.ReadAllLines(filepath);

        int index = 0;

        AssetFixer_ProgressBar.Maximum = file.Length;

        foreach (var line in file)
        {
            ++index;

            try
            {
                if (line.Contains("www.w3.org") || line.Contains("roblox.xsd"))
                {
                    ProgressChangedEvent();
                    continue;
                }

                string oneline = Regex.Replace(line, @"\t|\n|\r", "");
                AssetLocalization_StatusText.Text = (!useURLs ? "Localizing " : "Fixing " ) + oneline;
                AssetLocalization_StatusText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

                if (!useURLs)
                {
                    if (line.Contains("http://") || line.Contains("https://"))
                    {
                        //https://stackoverflow.com/questions/10576686/c-sharp-regex-pattern-to-extract-urls-from-given-string-not-full-html-urls-but
                        List<string> links = new List<string>();
                        var linkParser = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        foreach (Match m in linkParser.Matches(line))
                        {
                            string link = m.Value;
                            links.Add(link);
                        }

                        foreach (string link in links)
                        {
                            string newurl = ((!link.Contains("http://") || !link.Contains("https://")) ? "https://" : "")
                                + "assetdelivery.roblox.com/v1/asset/?id=";
                            string urlReplaced = newurl.Contains("https://") ? link.Replace("http://", "").Replace("https://", "") : link.Replace("http://", "https://");
                            string urlFixed = GlobalFuncs.FixURLString(urlReplaced, newurl);

                            string peram = "id=";

                            if (urlFixed.Contains(peram))
                            {
                                string IDVal = urlFixed.After(peram);
                                RobloxXML.DownloadFilesFromNode(urlFixed, savefilepath, "", IDVal);
                                file[index - 1] = file[index - 1].Replace(link, inGameDir + IDVal);
                            }
                        }
                    }
                }
                else
                {
                    if ((line.Contains("http://") || line.Contains("https://")) && !line.Contains(url))
                    {
                        string oldurl = line;
                        string urlFixed = GlobalFuncs.FixURLString(oldurl, url);

                        string peram = "id=";

                        if (urlFixed.Contains(peram))
                        {
                            file[index - 1] = urlFixed;
                        }
                    }
                }

                ProgressChangedEvent();
            }
            catch (Exception ex)
            {
                GlobalFuncs.LogExceptions(ex);
                errors += 1;
                GlobalFuncs.LogPrint("ASSETFIX|FILE " + path + " LINE #" + (index) + " " + ex.Message, 2);
                GlobalFuncs.LogPrint("ASSETFIX|Asset might be private or unavailable.");
                ProgressChangedEvent();
                continue;
            }
        }

        File.WriteAllLines(filepath, file);
    }

    public void LocalizeAsset(RobloxFileType type, BackgroundWorker worker, string path, string itemname, bool useURLs = false, string remoteurl = "")
    {
        AssetFixer_ProgressLabel.Text = "Loading...";

        bool error = false;
        string[] file = File.ReadAllLines(path);

        foreach (var line in file)
        {
            if (line.Contains("<roblox!"))
            {
                error = true;
                break;
            }
        }

        if (!error && GlobalVars.UserConfiguration.AssetSDKFixerSaveBackups)
        {
            try
            {
                GlobalFuncs.FixedFileCopy(path, path.Replace(".", " - BAK."), false);
            }
            catch (Exception ex)
            {
                GlobalFuncs.LogExceptions(ex);
                return;
            }
        }

        //assume we're a script
        try
        {
            if (error)
            {
                throw new FileFormatException("Cannot load models/places in binary format.");
            }
            else
            {
                FixURLSOrDownloadFromScript(path, GlobalPaths.AssetCacheDirAssets, GlobalPaths.AssetCacheAssetsGameDir, useURLs, url);
            }
        }
        catch (Exception ex)
        {
            GlobalFuncs.LogExceptions(ex);
            MessageBox.Show("Error: Unable to load the asset. " + ex.Message + "\n\nIf the asset is a modern place or model, try converting the place or model to rbxlx/rbxmx format using MODERN Roblox Studio, then convert it using the Roblox Legacy Place Converter. It should then load fine in the Asset Fixer.", "Asset Fixer - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
    }

    private void SetAssetCachePaths(bool perm = false)
    {
        if (perm)
        {
            GlobalPaths.AssetCacheDir = GlobalPaths.DataPath;
            GlobalPaths.AssetCacheDirSky = GlobalPaths.AssetCacheDir + "\\sky";
            GlobalPaths.AssetCacheDirFonts = GlobalPaths.AssetCacheDir + GlobalPaths.DirFonts;
            GlobalPaths.AssetCacheDirSounds = GlobalPaths.AssetCacheDir + GlobalPaths.DirSounds;
            GlobalPaths.AssetCacheDirTextures = GlobalPaths.AssetCacheDir + GlobalPaths.DirTextures;
            GlobalPaths.AssetCacheDirTexturesGUI = GlobalPaths.AssetCacheDirTextures + "\\gui";
            GlobalPaths.AssetCacheDirScripts = GlobalPaths.AssetCacheDir + GlobalPaths.DirScripts;
            GlobalPaths.AssetCacheDirAssets = GlobalPaths.AssetCacheDir + "\\assets";

            GlobalFuncs.CreateAssetCacheDirectories();

            GlobalPaths.AssetCacheGameDir = GlobalPaths.SharedDataGameDir;
            GlobalPaths.AssetCacheFontsGameDir = GlobalPaths.AssetCacheGameDir + GlobalPaths.FontsGameDir;
            GlobalPaths.AssetCacheSkyGameDir = GlobalPaths.AssetCacheGameDir + "sky/";
            GlobalPaths.AssetCacheSoundsGameDir = GlobalPaths.AssetCacheGameDir + GlobalPaths.SoundsGameDir;
            GlobalPaths.AssetCacheTexturesGameDir = GlobalPaths.AssetCacheGameDir + GlobalPaths.TexturesGameDir;
            GlobalPaths.AssetCacheTexturesGUIGameDir = GlobalPaths.AssetCacheTexturesGameDir + "gui/";
            GlobalPaths.AssetCacheScriptsGameDir = GlobalPaths.AssetCacheGameDir + GlobalPaths.ScriptsGameDir;
            GlobalPaths.AssetCacheAssetsGameDir = GlobalPaths.AssetCacheGameDir + "assets/";
        }
        else
        {
            GlobalPaths.AssetCacheDir = GlobalPaths.DataPath + "\\assetcache";
            GlobalPaths.AssetCacheDirSky = GlobalPaths.AssetCacheDir + "\\sky";
            GlobalPaths.AssetCacheDirFonts = GlobalPaths.AssetCacheDir + GlobalPaths.DirFonts;
            GlobalPaths.AssetCacheDirSounds = GlobalPaths.AssetCacheDir + GlobalPaths.DirSounds;
            GlobalPaths.AssetCacheDirTextures = GlobalPaths.AssetCacheDir + GlobalPaths.DirTextures;
            GlobalPaths.AssetCacheDirTexturesGUI = GlobalPaths.AssetCacheDirTextures + "\\gui";
            GlobalPaths.AssetCacheDirScripts = GlobalPaths.AssetCacheDir + GlobalPaths.DirScripts;
            GlobalPaths.AssetCacheDirAssets = GlobalPaths.AssetCacheDir + "\\assets";

            GlobalPaths.AssetCacheGameDir = GlobalPaths.SharedDataGameDir + "assetcache/";
            GlobalPaths.AssetCacheFontsGameDir = GlobalPaths.AssetCacheGameDir + GlobalPaths.FontsGameDir;
            GlobalPaths.AssetCacheSkyGameDir = GlobalPaths.AssetCacheGameDir + "sky/";
            GlobalPaths.AssetCacheSoundsGameDir = GlobalPaths.AssetCacheGameDir + GlobalPaths.SoundsGameDir;
            GlobalPaths.AssetCacheTexturesGameDir = GlobalPaths.AssetCacheGameDir + GlobalPaths.TexturesGameDir;
            GlobalPaths.AssetCacheTexturesGUIGameDir = GlobalPaths.AssetCacheTexturesGameDir + "gui/";
            GlobalPaths.AssetCacheScriptsGameDir = GlobalPaths.AssetCacheGameDir + GlobalPaths.ScriptsGameDir;
            GlobalPaths.AssetCacheAssetsGameDir = GlobalPaths.AssetCacheGameDir + "assets/";
        }
    }

    private void AssetLocalization_AssetTypeBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        switch (AssetLocalization_AssetTypeBox.SelectedIndex)
        {
            case 1:
                currentType = RobloxFileType.RBXM;
                break;
            case 2:
                currentType = RobloxFileType.Script;
                break;
            default:
                currentType = RobloxFileType.RBXL;
                break;
        }
    }

    private void AssetLocalization_ItemNameBox_TextChanged(object sender, EventArgs e)
    {
        name = AssetLocalization_ItemNameBox.Text;
    }

    private void AssetLocalization_SaveBackups_CheckedChanged(object sender, EventArgs e)
    {
        GlobalVars.UserConfiguration.AssetSDKFixerSaveBackups = AssetLocalization_SaveBackups.Checked;
    }

    private void AssetLocalization_LocalizeButton_Click(object sender, EventArgs e)
    {
        if (isWebSite)
        {
            MessageBox.Show("Error: Unable to fix the asset because you chose a URL that cannot be downloaded from. Please choose a different URL.", "Asset Fixer - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        OpenFileDialog robloxFileDialog = LoadROBLOXFileDialog(currentType);

        if (robloxFileDialog.ShowDialog() == DialogResult.OK)
        {
            path = robloxFileDialog.FileName;
            AssetLocalization_BackgroundWorker.RunWorkerAsync();
        }
    }

    // This event handler is where the time-consuming work is done.
    private void AssetLocalization_BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
    {
        BackgroundWorker worker = sender as BackgroundWorker;
        LocalizeAsset(currentType, worker, path, name,
                AssetLocalization_AssetLinks.Checked ? AssetLocalization_AssetLinks.Checked : false,
                AssetLocalization_AssetLinks.Checked ? url : "");
    }

    // This event handler updates the progress.
    private void AssetLocalization_BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
        //Progress Bar doesn't work here, wtf?
    }

    // This event handler deals with the results of the background operation.
    private void AssetLocalization_BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        switch (e)
        {
            case RunWorkerCompletedEventArgs can when can.Cancelled:
                AssetLocalization_StatusText.Text = "Canceled!";
                break;
            case RunWorkerCompletedEventArgs err when err.Error != null:
                AssetLocalization_StatusText.Text = "Error: " + e.Error.Message;
                MessageBox.Show("Error: " + e.Error.Message + "\n\n" + e.Error.StackTrace, "Asset Fixer - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                break;
            default:
                if (errors > 0)
                {
                    AssetLocalization_StatusText.Text = "Completed with " + errors + " errors!";

                    string errorCountString = errors + ((errors == 1 || errors == -1) ? " error" : " errors");

                    MessageBox.Show(errorCountString + " were found. Please look in today's log in \"" + GlobalPaths.LogDir + "\" for more details." +
                            "\n\nSome assets may be removed due to " +
                            "\n- Removal of the asset by the original owner" +
                            "\n- Privatization of the original asset by the owner" +
                            "\n- The asset just isn't available for the user to download (common for models)" +
                            "\n\nYour file may still function, but it may have issues that need to be corrected manually.", "Asset Fixer - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    AssetLocalization_StatusText.Text = "Completed!";
                }
                break;
        }

        AssetLocalization_StatusText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        AssetFixer_ProgressBar.Value = 0;
        AssetFixer_ProgressLabel.Text = "";
    }

    private void AssetLocalization_LocalizePermanentlyBox_Click(object sender, EventArgs e)
    {
        if (AssetLocalization_LocalizePermanentlyBox.Checked && !GlobalVars.UserConfiguration.DisabledAssetSDKHelp)
        {
            DialogResult res = MessageBox.Show("If you toggle this option, the Asset SDK will download all localized files directly into your Novetus data, rather than into the Asset Cache. This means you won't be able to clear these files with the 'Clear Asset Cache' option in the Launcher.\n\nWould you like to continue with the option anyways?", "Asset Fixer - Permanent Localization Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (res == DialogResult.No)
            {
                AssetLocalization_LocalizePermanentlyBox.Checked = false;
            }
        }
    }

    private void AssetLocalization_LocalizePermanentlyBox_CheckedChanged(object sender, EventArgs e)
    {
        LocalizePermanentlyIfNeeded();
    }

    private void AssetLocalization_AssetLinks_CheckedChanged(object sender, EventArgs e)
    {
        if (AssetLocalization_AssetLinks.Checked)
        {
            AssetLocalization_LocalizeButton.Text = AssetLocalization_LocalizeButton.Text.Replace("Localize", "Fix");
            AssetLocalization_LocalizePermanentlyBox.Enabled = false;
            SetAssetCachePaths();
        }
        else
        {
            AssetLocalization_LocalizeButton.Text = AssetLocalization_LocalizeButton.Text.Replace("Fix", "Localize");
            AssetLocalization_LocalizePermanentlyBox.Enabled = true;
            LocalizePermanentlyIfNeeded();
        }
    }

    void LocalizePermanentlyIfNeeded()
    {
        if (AssetLocalization_LocalizePermanentlyBox.Checked)
        {
            AssetLocalization_AssetLinks.Enabled = false;
            SetAssetCachePaths(true);
        }
        else
        {
            AssetLocalization_AssetLinks.Enabled = true;
            SetAssetCachePaths();
        }
    }
    #endregion

    #endregion
}