﻿#region Usings
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Linq;
#endregion

namespace Novetus.Core
{
    #region File Formats
    public class FileFormat
    {
        #region Client Information
        public class ClientInfo
        {
            public ClientInfo()
            {
                UsesPlayerName = true;
                UsesID = true;
                Description = "";
                Warning = "";
                LegacyMode = false;
                ClientMD5 = "";
                ScriptMD5 = "";
                Fix2007 = false;
                AlreadyHasSecurity = false;
                ClientLoadOptions = Settings.ClientLoadOptions.Client_2008AndUp;
                SeperateFolders = false;
                UsesCustomClientEXEName = false;
                CustomClientEXEName = "";
                CommandLineArgs = "%args%";
            }

            public bool UsesPlayerName { get; set; }
            public bool UsesID { get; set; }
            public string Description { get; set; }
            public string Warning { get; set; }
            public bool LegacyMode { get; set; }
            public string ClientMD5 { get; set; }
            public string ScriptMD5 { get; set; }
            public bool Fix2007 { get; set; }
            public bool AlreadyHasSecurity { get; set; }
            public bool SeperateFolders { get; set; }
            public bool UsesCustomClientEXEName { get; set; }
            public string CustomClientEXEName { get; set; }
            public Settings.ClientLoadOptions ClientLoadOptions { get; set; }
            public string CommandLineArgs { get; set; }
        }
        #endregion

        #region ConfigBase
        public class ConfigBase
        {
            public INIFile INI;
            private string Section { get; set; }
            private string Path { get; set; }
            private string FileName { get; set; }
            public string FullPath { get;}

            public ConfigBase(string section, string path, string fileName)
            {
                Section = section;
                Path = path;
                FileName = fileName;
                FullPath = Path + "\\" + FileName;

                bool fileExists = File.Exists(FullPath);

                if (!fileExists)
                {
                    CreateFile();
                }
                else
                {
                    INI = new INIFile(FullPath, false);
                }
            }

            public void CreateFile()
            {
                INI = new INIFile(FullPath);
                GenerateDefaults();
                GenerateDefaultsEvent();
            }

            public virtual void GenerateDefaults()
            {
                //defaults go in here.
            }

            public virtual void GenerateDefaultsEvent()
            {
                //generate default event goes in here.
            }

            public void LoadAllSettings(string inputPath)
            {
                File.SetAttributes(Path, FileAttributes.Normal);
                File.Replace(inputPath, Path, null);
            }

            public void SaveSetting(string name)
            {
                SaveSetting(Section, name, "");
            }

            public void SaveSetting(string name, string value)
            {
                SaveSetting(Section, name, value);
            }

            public void SaveSetting(string section, string name, string value)
            {
                INI.IniWriteValue(section, name, value);
                SaveSettingEvent();
            }

            public void SaveSettingInt(string name, int value)
            {
                SaveSettingInt(Section, name, value);
            }

            public void SaveSettingInt(string section, string name, int value)
            {
                INI.IniWriteValue(section, name, value.ToString());
                SaveSettingEvent();
            }

            public void SaveSettingBool(string name, bool value)
            {
                SaveSettingBool(Section, name, value);
            }

            public void SaveSettingBool(string section, string name, bool value)
            {
                INI.IniWriteValue(section, name, value.ToString());
                SaveSettingEvent();
            }

            public virtual void SaveSettingEvent()
            {
                //save setting event goes in here.
            }

            public string ReadSetting(string name)
            {
                string value = INI.IniReadValue(Section, name);

                if (!string.IsNullOrWhiteSpace(value))
                {
                    ReadSettingEvent();
                    return INI.IniReadValue(Section, name);
                }
                else
                {
                    return "";
                }
            }

            public int ReadSettingInt(string name)
            {
                return Convert.ToInt32(ReadSetting(name));
            }

            public bool ReadSettingBool(string name)
            {
                return Convert.ToBoolean(ReadSetting(name));
            }

            public virtual void ReadSettingEvent()
            {
                //read setting event.
            }
        }

        #endregion

        #region Configuration
        public class Config : ConfigBase
        {
            public Config() : base("Config", GlobalPaths.ConfigDir, GlobalPaths.ConfigName) { }

            public Config(string filename) : base("Config", GlobalPaths.ConfigDir, filename) { }

            public override void GenerateDefaults()
            {
                SaveSetting("SelectedClient", "");
                SaveSetting("Map", "");
                SaveSettingBool("CloseOnLaunch", false);
                SaveSettingInt("UserID", NovetusFuncs.GeneratePlayerID());
                SaveSetting("PlayerName", "Player");
                SaveSettingInt("RobloxPort", 53640);
                SaveSettingInt("PlayerLimit", 12);
                SaveSettingBool("UPnP", false);
                SaveSettingBool("DisabledAssetSDKHelp", false);
                SaveSettingBool("DiscordRichPresence", true);
                SaveSetting("MapPath", "");
                SaveSetting("MapPathSnip", "");
                SaveSettingInt("GraphicsMode", (int)Settings.Mode.Automatic);
                SaveSettingInt("QualityLevel", (int)Settings.Level.Automatic);

                if (Util.IsWineRunning())
                {
                    SaveSettingInt("LauncherStyle", (int)Settings.Style.Extended);
                }
                else
                {
                    SaveSettingInt("LauncherStyle", (int)Settings.Style.Stylish);
                }

                SaveSettingBool("AssetSDKFixerSaveBackups", true);
                SaveSetting("AlternateServerIP", "");
                SaveSettingBool("ShowServerNotifications", false);
                SaveSetting("ServerBrowserServerName", "Novetus");
                SaveSetting("ServerBrowserServerAddress", "");
                SaveSettingInt("Priority", (int)ProcessPriorityClass.RealTime);
                SaveSettingBool("FirstServerLaunch", true);
                SaveSettingBool("NewGUI", false);
                SaveSettingBool("URIQuickConfigure", true);
                SaveSettingBool("BootstrapperShowUI", true);
                SaveSettingBool("WebProxyInitialSetupRequired", true);
                SaveSettingBool("WebProxyEnabled", false);
            }
        }
        #endregion

        #region Customization Configuration
        public class CustomizationConfig : ConfigBase
        {
            public CustomizationConfig() : base("Items", GlobalPaths.ConfigDir, GlobalPaths.ConfigNameCustomization) { }
            public CustomizationConfig(string filename) : base("Items", GlobalPaths.ConfigDir, filename) { }

            public override void GenerateDefaults()
            {
                SaveSetting("Items", "Hat1", "NoHat.rbxm");
                SaveSetting("Items", "Hat2", "NoHat.rbxm");
                SaveSetting("Items", "Hat3", "NoHat.rbxm");
                SaveSetting("Items", "Face", "DefaultFace.rbxm");
                SaveSetting("Items", "Head", "DefaultHead.rbxm");
                SaveSetting("Items", "TShirt", "NoTShirt.rbxm");
                SaveSetting("Items", "Shirt", "NoShirt.rbxm");
                SaveSetting("Items", "Pants", "NoPants.rbxm");
                SaveSetting("Items", "Icon", "NBC");
                SaveSetting("Items", "Extra", "NoExtra.rbxm");
                SaveSettingInt("Colors", "HeadColorID", 24);
                SaveSettingInt("Colors", "TorsoColorID", 23);
                SaveSettingInt("Colors", "LeftArmColorID", 24);
                SaveSettingInt("Colors", "RightArmColorID", 24);
                SaveSettingInt("Colors", "LeftLegColorID", 119);
                SaveSettingInt("Colors", "RightLegColorID", 119);
                SaveSetting("Colors", "HeadColorString", "Color [A=255, R=245, G=205, B=47]");
                SaveSetting("Colors", "TorsoColorString", "Color [A=255, R=13, G=105, B=172]");
                SaveSetting("Colors", "LeftArmColorString", "Color [A=255, R=245, G=205, B=47]");
                SaveSetting("Colors", "RightArmColorString", "Color [A=255, R=245, G=205, B=47]");
                SaveSetting("Colors", "LeftLegColorString", "Color [A=255, R=164, G=189, B=71]");
                SaveSetting("Colors", "RightLegColorString", "Color [A=255, R=164, G=189, B=71]");
                SaveSettingBool("Other", "ExtraSelectionIsHat", false);
                SaveSettingBool("Other", "ShowHatsInExtra", false);
                SaveSetting("Other", "CharacterID", "");
            }

            public override void ReadSettingEvent()
            {
                FileManagement.ReloadLoadoutValue();
            }
        }
        #endregion

        #region Program Information
        public class ProgramInfo
        {
            public ProgramInfo()
            {
                Version = "";
                Branch = "";
                DefaultClient = "";
                RegisterClient1 = "";
                RegisterClient2 = "";
                DefaultMap = "";
                VersionName = "";
                //HACK
#if NET4
                NetVersion = ".NET 4.0";
#elif NET481
                NetVersion = ".NET 4.8";
#endif
                InitialBootup = true;
                IsSnapshot = false;
            }

            public string Version { get; set; }
            public string Branch { get; set; }
            public string DefaultClient { get; set; }
            public string RegisterClient1 { get; set; }
            public string RegisterClient2 { get; set; }
            public string DefaultMap { get; set; }
            public string VersionName { get; set; }
            public string NetVersion { get; set; }
            public bool InitialBootup { get; set; }
            public bool IsSnapshot { get; set; }
        }
        #endregion
    }
    #endregion

    #region Part Color Options
    public class PartColor
    {
        public string ColorName;
        public int ColorID;
        public string ColorRGB;
        [XmlIgnore]
        public Color ColorObject;
        [XmlIgnore]
        public string ColorGroup;
        [XmlIgnore]
        public string ColorRawName;
        [XmlIgnore]
        public Bitmap ColorImage;
    }

    [XmlRoot("PartColors")]
    public class PartColors
    {
        [XmlArray("ColorList")]
        public PartColor[] ColorList;
    }

    public class PartColorLoader
    {
        public static PartColor[] GetPartColors()
        {
            if (File.Exists(GlobalPaths.ConfigDir + "\\" + GlobalPaths.PartColorXMLName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PartColors));
                PartColors colors;

                using (FileStream fs = new FileStream(GlobalPaths.ConfigDir + "\\" + GlobalPaths.PartColorXMLName, FileMode.Open))
                {
                    colors = (PartColors)serializer.Deserialize(fs);
                }

                foreach (var item in colors.ColorList)
                {
                    string colorFixed = Regex.Replace(item.ColorRGB, @"[\[\]\{\}\(\)\<\> ]", "");
                    string[] rgbValues = colorFixed.Split(',');
                    item.ColorObject = Color.FromArgb(Convert.ToInt32(rgbValues[0]), Convert.ToInt32(rgbValues[1]), Convert.ToInt32(rgbValues[2]));

                    if (!(item.ColorName.Contains("[") && item.ColorName.Contains("]")))
                    {
                        item.ColorRawName = item.ColorName;
                        item.ColorName = "[Uncategorized]" + item.ColorName;
                    }
                    else
                    {
                        item.ColorRawName = item.ColorName;
                    }

                    int pFrom = item.ColorName.IndexOf("[");
                    int pTo = item.ColorName.IndexOf("]");
                    item.ColorGroup = item.ColorName.Substring(pFrom + 1, pTo - pFrom - 1);
                    item.ColorName = item.ColorName.Replace(item.ColorGroup, "").Replace("[", "").Replace("]", "");
                    item.ColorImage = GeneratePartColorIcon(item, 128);
                }

                return colors.ColorList;
            }
            else
            {
                return null;
            }
        }

        //make faster
        public static void AddPartColorsToListView(PartColor[] PartColorList, ListView ColorView, int imgsize, bool showIDs = false)
        {
            try
            {
                ImageList ColorImageList = new ImageList();
                ColorImageList.ImageSize = new Size(imgsize, imgsize);
                ColorImageList.ColorDepth = ColorDepth.Depth32Bit;
                ColorView.LargeImageList = ColorImageList;
                ColorView.SmallImageList = ColorImageList;

                foreach (var item in PartColorList)
                {
                    var lvi = new ListViewItem(item.ColorName);
                    lvi.Tag = item.ColorID;

                    if (showIDs)
                    {
                        lvi.Text = lvi.Text + " (" + item.ColorID + ")";
                    }

                    var group = ColorView.Groups.Cast<ListViewGroup>().FirstOrDefault(g => g.Header == item.ColorGroup);

                    if (group == null)
                    {
                        group = new ListViewGroup(item.ColorGroup);
                        ColorView.Groups.Add(group);
                    }

                    lvi.Group = group;

                    if (item.ColorImage != null)
                    {
                        ColorImageList.Images.Add(item.ColorName, item.ColorImage);
                        lvi.ImageIndex = ColorImageList.Images.IndexOfKey(item.ColorName);
                    }

                    ColorView.Items.Add(lvi);
                }

                /*foreach (var group in ColorView.Groups.Cast<ListViewGroup>())
                {
                    group.Header = group.Header + " (" + group.Items.Count + ")";
                }*/
            }
            catch (Exception ex)
            {
                Util.LogExceptions(ex);
            }
        }

        public static Bitmap GeneratePartColorIcon(PartColor color, int imgsize)
        {
            try
            {
                Bitmap Bmp = new Bitmap(imgsize, imgsize, PixelFormat.Format32bppArgb);
                using (Graphics gfx = Graphics.FromImage(Bmp))
                using (SolidBrush brush = new SolidBrush(color.ColorObject))
                {
                    gfx.FillRectangle(brush, 0, 0, imgsize, imgsize);
                }

                return Bmp;
            }
            catch (Exception ex)
            {
                Util.LogExceptions(ex);
                return null;
            }
        }

        public static PartColor FindPartColorByName(PartColor[] colors, string query)
        {
            if (File.Exists(GlobalPaths.ConfigDir + "\\" + GlobalPaths.PartColorXMLName))
            {
                return colors.SingleOrDefault(item => query.Contains(item.ColorName));
            }
            else
            {
                return null;
            }
        }

        public static PartColor FindPartColorByID(PartColor[] colors, string query)
        {
            if (File.Exists(GlobalPaths.ConfigDir + "\\" + GlobalPaths.PartColorXMLName))
            {
                return colors.SingleOrDefault(item => query.Contains(item.ColorID.ToString()));
            }
            else
            {
                return null;
            }
        }
    }
    #endregion

    #region Content Provider Options
    public class Provider
    {
        public string Name;
        public string URL;
        public string Icon;
    }

    [XmlRoot("ContentProviders")]
    public class ContentProviders
    {
        [XmlArray("Providers")]
        public Provider[] Providers;
    }

    public class OnlineClothing
    {
        public static Provider[] GetContentProviders()
        {
            if (File.Exists(GlobalPaths.ConfigDir + "\\" + GlobalPaths.ContentProviderXMLName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ContentProviders));
                ContentProviders providers;

                using (FileStream fs = new FileStream(GlobalPaths.ConfigDir + "\\" + GlobalPaths.ContentProviderXMLName, FileMode.Open))
                {
                    providers = (ContentProviders)serializer.Deserialize(fs);
                }

                return providers.Providers;
            }
            else
            {
                return null;
            }
        }

        public static Provider FindContentProviderByName(Provider[] providers, string query)
        {
            if (File.Exists(GlobalPaths.ConfigDir + "\\" + GlobalPaths.ContentProviderXMLName))
            {
                return providers.SingleOrDefault(item => query.Contains(item.Name));
            }
            else
            {
                return null;
            }
        }

        public static Provider FindContentProviderByURL(Provider[] providers, string query)
        {
            if (File.Exists(GlobalPaths.ConfigDir + "\\" + GlobalPaths.ContentProviderXMLName))
            {
                return providers.SingleOrDefault(item => query.Contains(item.URL));
            }
            else
            {
                return null;
            }
        }
    }
    #endregion

    #region Settings
    public class Settings
    {
        public enum Mode
        {
            Automatic = 0,
            OpenGLStable = 1,
            OpenGLExperimental = 2,
            DirectX = 3
        }

        public enum Level
        {
            Automatic = 0,
            VeryLow = 1,
            Low = 2,
            Medium = 3,
            High = 4,
            Ultra = 5,
            Custom = 6
        }

        public enum Style
        {
            None = 0,
            Extended = 1,
            Compact = 2,
            Stylish = 3
        }

        public enum ClientLoadOptions
        {
            Client_2007_NoGraphicsOptions = 0,
            Client_2007 = 1,
            Client_2008AndUp = 2,
            Client_2008AndUp_LegacyOpenGL = 3,
            Client_2008AndUp_QualityLevel21 = 4,
            Client_2008AndUp_NoGraphicsOptions = 5,
            Client_2008AndUp_ForceAutomatic = 6,
            Client_2008AndUp_ForceAutomaticQL21 = 7,
            Client_2008AndUp_HasCharacterOnlyShadowsLegacyOpenGL = 8
        }

        public static ClientLoadOptions GetClientLoadOptionsForBool(bool level)
        {
            switch (level)
            {
                case false:
                    return ClientLoadOptions.Client_2008AndUp;
                default:
                    return ClientLoadOptions.Client_2007_NoGraphicsOptions;
            }
        }

        public static string GetPathForClientLoadOptions(ClientLoadOptions level)
        {
            string localAppdataRobloxPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Roblox";
            string appdataRobloxPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Roblox";

            if (!Directory.Exists(localAppdataRobloxPath))
            {
                Directory.CreateDirectory(localAppdataRobloxPath);
            }

            if (!Directory.Exists(appdataRobloxPath))
            {
                Directory.CreateDirectory(appdataRobloxPath);
            }

            switch (level)
            {
                case ClientLoadOptions.Client_2008AndUp_QualityLevel21:
                case ClientLoadOptions.Client_2008AndUp_LegacyOpenGL:
                case ClientLoadOptions.Client_2008AndUp_NoGraphicsOptions:
                case ClientLoadOptions.Client_2008AndUp_ForceAutomatic:
                case ClientLoadOptions.Client_2008AndUp_ForceAutomaticQL21:
                case ClientLoadOptions.Client_2008AndUp_HasCharacterOnlyShadowsLegacyOpenGL:
                case ClientLoadOptions.Client_2008AndUp:
                    return localAppdataRobloxPath;
                default:
                    return appdataRobloxPath;
            }
        }
    }

    #endregion

    #region Icon Loader

    public class IconLoader
    {
        private OpenFileDialog openFileDialog1;
        private string installOutcome = "";
        public bool CopyToItemDir = false;
        public string ItemDir = "";
        public string ItemName = "";
        public string ItemPath = "";

        public IconLoader()
        {
            openFileDialog1 = new OpenFileDialog()
            {
                FileName = "Select an icon .png file",
                Filter = "Portable Network Graphics image (*.png)|*.png",
                Title = "Open icon .png"
            };
        }

        public void setInstallOutcome(string text)
        {
            installOutcome = text;
        }

        public string getInstallOutcome()
        {
            return installOutcome;
        }

        public void LoadImage()
        {
            string ItemNameFixed = ItemName.Replace(" ", "");
            string dir = CopyToItemDir ? ItemDir + "\\" + ItemNameFixed : GlobalPaths.extradir + "\\icons\\" + GlobalVars.UserConfiguration.ReadSetting("PlayerName");

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Util.FixedFileCopy(openFileDialog1.FileName, dir + ".png", true);

                    if (CopyToItemDir)
                    {
                        ItemPath = ItemDir + "\\" + ItemNameFixed + ".png";
                    }

                    installOutcome = "Icon " + openFileDialog1.SafeFileName + " installed!";
                }
                catch (Exception ex)
                {
                    installOutcome = "Error when installing icon: " + ex.Message;
                    Util.LogExceptions(ex);
                }
            }
        }
    }
    #endregion

    #region File Management
    public class FileManagement
    {
        public static string CreateVersionName(string termspath, int revision)
        {
            string rev = revision.ToString();

            if (rev.Length > 0 && rev.Length >= 4)
            {
                string posString = rev.Substring(rev.Length - 4);

                int pos1 = int.Parse(posString.Substring(0, 2));
                int pos2 = int.Parse(posString.Substring(posString.Length - 2));

                List<string> termList = new List<string>();
                termList.AddRange(File.ReadAllLines(termspath));

                string firstTerm = (termList.ElementAtOrDefault(pos1 - 1) != null) ? termList[pos1 - 1] : termList.First();
                string lastTerm = (termList.ElementAtOrDefault(pos2 - 1) != null) ? termList[pos2 - 1] : termList.Last();

                return firstTerm + " " + lastTerm;
            }

            return "Invalid Revision";
        }

        public static void ReadInfoFile(string infopath, string termspath, string exepath = "")
        {
            //READ
            string versionbranch, defaultclient, defaultmap, regclient1,
                regclient2, extendedversionnumber, extendedversiontemplate,
                extendedversionrevision, isSnapshot,
                initialBootup;

            string verNumber = "Invalid File";

            INIFile ini = new INIFile(infopath, false);

            string section = "ProgramInfo";

            //not using the GlobalVars definitions as those are empty until we fill them in.
            versionbranch = ini.IniReadValue(section, "Branch", "0.0");
            defaultclient = ini.IniReadValue(section, "DefaultClient", "2009E");
            defaultmap = ini.IniReadValue(section, "DefaultMap", "Dev - Baseplate2048.rbxl");
            regclient1 = ini.IniReadValue(section, "UserAgentRegisterClient1", "2007M");
            regclient2 = ini.IniReadValue(section, "UserAgentRegisterClient2", "2009L");
            extendedversionnumber = ini.IniReadValue(section, "ExtendedVersionNumber", "False");
            extendedversiontemplate = ini.IniReadValue(section, "ExtendedVersionTemplate", "%version%");
            extendedversionrevision = ini.IniReadValue(section, "ExtendedVersionRevision", "-1");
            isSnapshot = ini.IniReadValue(section, "IsSnapshot", "False");
            initialBootup = ini.IniReadValue(section, "InitialBootup", "True");

            try
            {
                GlobalVars.ExtendedVersionNumber = Convert.ToBoolean(extendedversionnumber);
                if (GlobalVars.ExtendedVersionNumber)
                {
                    if (!string.IsNullOrWhiteSpace(exepath))
                    {
                        var versionInfo = FileVersionInfo.GetVersionInfo(exepath);
                        verNumber = CreateVersionName(termspath, versionInfo.FilePrivatePart);
                        GlobalVars.ProgramInformation.Version = extendedversiontemplate.Replace("%version%", versionbranch)
                            .Replace("%build%", versionInfo.ProductBuildPart.ToString())
                            .Replace("%revision%", versionInfo.FilePrivatePart.ToString())
                            .Replace("%extended-revision%", (!extendedversionrevision.Equals("-1") ? extendedversionrevision : ""))
                            .Replace("%version-name%", verNumber);
                    }
                    else
                    {
                        verNumber = CreateVersionName(termspath, Assembly.GetExecutingAssembly().GetName().Version.Revision);
                        GlobalVars.ProgramInformation.Version = extendedversiontemplate.Replace("%version%", versionbranch)
                            .Replace("%build%", Assembly.GetExecutingAssembly().GetName().Version.Build.ToString())
                            .Replace("%revision%", Assembly.GetExecutingAssembly().GetName().Version.Revision.ToString())
                            .Replace("%extended-revision%", (!extendedversionrevision.Equals("-1") ? extendedversionrevision : ""))
                            .Replace("%version-name%", verNumber);
                    }

                    bool changelogedit = Convert.ToBoolean(isSnapshot);

                    if (changelogedit)
                    {
                        string changelog = GlobalPaths.BasePath + "\\changelog.txt";
                        if (File.Exists(changelog))
                        {
                            string[] changeloglines = File.ReadAllLines(changelog);
                            if (!changeloglines[0].Equals(GlobalVars.ProgramInformation.Version))
                            {
                                changeloglines[0] = GlobalVars.ProgramInformation.Version;
                                File.WriteAllLines(changelog, changeloglines);
                            }
                        }
                    }
                }
                else
                {
                    GlobalVars.ProgramInformation.Version = versionbranch;
                }

                GlobalVars.ProgramInformation.Branch = versionbranch;
                GlobalVars.ProgramInformation.DefaultClient = defaultclient;
                GlobalVars.ProgramInformation.DefaultMap = defaultmap;
                GlobalVars.ProgramInformation.RegisterClient1 = regclient1;
                GlobalVars.ProgramInformation.RegisterClient2 = regclient2;
                GlobalVars.ProgramInformation.InitialBootup = Convert.ToBoolean(initialBootup);
                GlobalVars.ProgramInformation.VersionName = verNumber;
                GlobalVars.ProgramInformation.IsSnapshot = Convert.ToBoolean(isSnapshot);
                GlobalVars.UserConfiguration.SaveSetting("SelectedClient", GlobalVars.ProgramInformation.DefaultClient);
                GlobalVars.UserConfiguration.SaveSetting("Map", GlobalVars.ProgramInformation.DefaultMap);
                GlobalVars.UserConfiguration.SaveSetting("MapPath", GlobalPaths.MapsDir + @"\\" + GlobalVars.ProgramInformation.DefaultMap);
                GlobalVars.UserConfiguration.SaveSetting("MapPathSnip", GlobalPaths.MapsDirBase + @"\\" + GlobalVars.ProgramInformation.DefaultMap);
            }
            catch (Exception ex)
            {
                Util.LogExceptions(ex);
                ReadInfoFile(infopath, termspath, exepath);
            }
        }

        public static void TurnOffInitialSequence()
        {
            //READ
            INIFile ini = new INIFile(GlobalPaths.ConfigDir + "\\" + GlobalPaths.InfoName, false);
            string section = "ProgramInfo";

            string initialBootup = ini.IniReadValue(section, "InitialBootup", "True");
            if (Convert.ToBoolean(initialBootup) == true)
            {
                ini.IniWriteValue(section, "InitialBootup", "False");
            }
        }

        public static void GenerateTripcode()
        {
            //Powered by https://github.com/davcs86/csharp-uhwid
            string curval = UHWIDEngine.AdvancedUid;
            if (!GlobalVars.PlayerTripcode.Equals(curval))
            {
                GlobalVars.PlayerTripcode = curval;
            }
        }

        public static bool ResetMapIfNecessary()
        {
            if (!File.Exists(GlobalVars.UserConfiguration.ReadSetting("MapPath")))
            {
                GlobalVars.UserConfiguration.SaveSetting("Map", GlobalVars.ProgramInformation.DefaultMap);
                GlobalVars.UserConfiguration.SaveSetting("MapPath", GlobalPaths.MapsDir + @"\\" + GlobalVars.ProgramInformation.DefaultMap);
                GlobalVars.UserConfiguration.SaveSetting("MapPathSnip", GlobalPaths.MapsDirBase + @"\\" + GlobalVars.ProgramInformation.DefaultMap);
                return true;
            }

            return false;
        }

        public static bool InitColors()
        {
            try
            {
                if (File.Exists(GlobalPaths.ConfigDir + "\\" + GlobalPaths.PartColorXMLName))
                {
                    GlobalVars.PartColorList = PartColorLoader.GetPartColors();
                    GlobalVars.PartColorListConv = new List<PartColor>();
                    GlobalVars.PartColorListConv.AddRange(GlobalVars.PartColorList);
                    return true;
                }
                else
                {
                    goto Failure;
                }
            }
            catch (Exception ex)
            {
                Util.LogExceptions(ex);
                goto Failure;
            }

        Failure:
            return false;
        }

        public static bool HasColorsChanged()
        {
            try
            {
                PartColor[] tempList;

                if (File.Exists(GlobalPaths.ConfigDir + "\\" + GlobalPaths.PartColorXMLName))
                {
                    tempList = PartColorLoader.GetPartColors();
                    if (tempList.Length != GlobalVars.PartColorList.Length)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    goto Failure;
                }
            }
            catch (Exception ex)
            {
                Util.LogExceptions(ex);
                goto Failure;
            }

        Failure:
            return false;
        }

#if LAUNCHER
        public static void ResetConfigValues(Settings.Style style)
#else
        public static void ResetConfigValues()
#endif
        {
            bool WebProxySetupComplete = GlobalVars.UserConfiguration.ReadSettingBool("WebProxyInitialSetupRequired");
            bool WebProxy = GlobalVars.UserConfiguration.ReadSettingBool("WebProxyEnabled");

            GlobalVars.UserConfiguration = new FileFormat.Config();
            GlobalVars.UserConfiguration.SaveSetting("SelectedClient", GlobalVars.ProgramInformation.DefaultClient);
            GlobalVars.UserConfiguration.SaveSetting("Map", GlobalVars.ProgramInformation.DefaultMap);
            GlobalVars.UserConfiguration.SaveSetting("MapPath", GlobalPaths.MapsDir + @"\\" + GlobalVars.ProgramInformation.DefaultMap);
            GlobalVars.UserConfiguration.SaveSetting("MapPathSnip", GlobalPaths.MapsDirBase + @"\\" + GlobalVars.ProgramInformation.DefaultMap);
#if LAUNCHER
            GlobalVars.UserConfiguration.SaveSettingInt("LauncherStyle", (int)style);
#endif
            GlobalVars.UserConfiguration.SaveSettingBool("WebProxyInitialSetupRequired", WebProxySetupComplete);
            GlobalVars.UserConfiguration.SaveSettingBool("WebProxyEnabled", WebProxy);
            GlobalVars.UserConfiguration.SaveSettingInt("UserID", NovetusFuncs.GeneratePlayerID());
            ResetCustomizationValues();
        }

        public static void ResetCustomizationValues()
        {
            GlobalVars.UserCustomization = new FileFormat.CustomizationConfig();
            ReloadLoadoutValue();
        }

        public static string GetItemTextureLocalPath(string item, string nameprefix)
        {
            //don't bother, we're offline.
            if (GlobalVars.ExternalIP.Equals("localhost"))
                return "";

            if (!GlobalVars.SelectedClientInfo.CommandLineArgs.Contains("%localizeonlineclothing%"))
                return "";

            if (item.Contains("http://") || item.Contains("https://"))
            {
                string peram = "id=";
                string fullname = nameprefix + "Temp.png";

                if (item.Contains(peram))
                {
                    string id = item.After(peram);
                    fullname = id + ".png";
                }
                else
                {
                    return item;
                }

                Downloader download = new Downloader(item, fullname, "", GlobalPaths.AssetCacheDirAssets);

                try
                {
                    string path = download.GetFullDLPath();
                    download.InitDownloadNoDialog(path);
                    return GlobalPaths.AssetCacheAssetsGameDir + download.fileName;
                }
                catch (Exception ex)
                {
                    Util.LogExceptions(ex);
                }
            }

            return "";
        }

        public static string GetItemTextureID(string item, string name, AssetCacheDefBasic assetCacheDef)
        {
            //don't bother, we're offline.
            if (GlobalVars.ExternalIP.Equals("localhost"))
                return "";

            if (!GlobalVars.SelectedClientInfo.CommandLineArgs.Contains("%localizeonlineclothing%"))
                return "";

            if (item.Contains("http://") || item.Contains("https://"))
            {
                string peram = "id=";
                if (!item.Contains(peram))
                {
                    return item;
                }

                Downloader download = new Downloader(item, name + "Temp.rbxm", "", GlobalPaths.AssetCacheDirAssets);

                try
                {
                    string path = download.GetFullDLPath();
                    download.InitDownloadNoDialog(path);
                    string oldfile = File.ReadAllText(path);
                    string fixedfile = RobloxXML.RemoveInvalidXmlChars(RobloxXML.ReplaceHexadecimalSymbols(oldfile)).Replace("&#9;", "\t").Replace("#9;", "\t");
                    XDocument doc = null;
                    XmlReaderSettings xmlReaderSettings = new XmlReaderSettings { CheckCharacters = false };
                    Stream filestream = Util.GenerateStreamFromString(fixedfile);
                    using (XmlReader xmlReader = XmlReader.Create(filestream, xmlReaderSettings))
                    {
                        xmlReader.MoveToContent();
                        doc = XDocument.Load(xmlReader);
                    }

                    return RobloxXML.GetURLInNodes(doc, assetCacheDef.Class, assetCacheDef.Id[0], item);
                }
                catch (Exception ex)
                {
                    Util.LogExceptions(ex);
                }
            }

            return "";
        }

        public static void ReloadLoadoutValue(bool localizeOnlineClothing = false)
        {
            string hat1 = (!GlobalVars.UserCustomization.ReadSetting("Hat1").EndsWith("-Solo.rbxm")) ? GlobalVars.UserCustomization.ReadSetting("Hat1") : "NoHat.rbxm";
            string hat2 = (!GlobalVars.UserCustomization.ReadSetting("Hat2").EndsWith("-Solo.rbxm")) ? GlobalVars.UserCustomization.ReadSetting("Hat2") : "NoHat.rbxm";
            string hat3 = (!GlobalVars.UserCustomization.ReadSetting("Hat3").EndsWith("-Solo.rbxm")) ? GlobalVars.UserCustomization.ReadSetting("Hat3") : "NoHat.rbxm";
            string extra = (!GlobalVars.UserCustomization.ReadSetting("Extra").EndsWith("-Solo.rbxm")) ? GlobalVars.UserCustomization.ReadSetting("Extra") : "NoExtra.rbxm";

            string baseClothing = GlobalVars.UserCustomization.ReadSettingInt("HeadColorID") + "," +
            GlobalVars.UserCustomization.ReadSettingInt("TorsoColorID") + "," +
            GlobalVars.UserCustomization.ReadSettingInt("LeftArmColorID") + "," +
            GlobalVars.UserCustomization.ReadSettingInt("RightArmColorID") + "," +
            GlobalVars.UserCustomization.ReadSettingInt("LeftLegColorID") + "," +
            GlobalVars.UserCustomization.ReadSettingInt("RightLegColorID") + ",'" +
            GlobalVars.UserCustomization.ReadSetting("TShirt") + "','" +
            GlobalVars.UserCustomization.ReadSetting("Shirt") + "','" +
            GlobalVars.UserCustomization.ReadSetting("Pants") + "','" +
            GlobalVars.UserCustomization.ReadSetting("Face") + "','" +
            GlobalVars.UserCustomization.ReadSetting("Head") + "','" +
            GlobalVars.UserCustomization.ReadSetting("Icon") + "','";

            GlobalVars.Loadout = "'" + hat1 + "','" +
            hat2 + "','" +
            hat3 + "'," +
            baseClothing +
            extra + "'";

            GlobalVars.soloLoadout = "'" + GlobalVars.UserCustomization.ReadSetting("Hat1") + "','" +
            GlobalVars.UserCustomization.ReadSetting("Hat2") + "','" +
            GlobalVars.UserCustomization.ReadSetting("Hat3") + "'," +
            baseClothing +
            GlobalVars.UserCustomization.ReadSetting("Extra") + "'";

            if (localizeOnlineClothing)
            {
                GlobalVars.TShirtTextureID = GetItemTextureID(GlobalVars.UserCustomization.ReadSetting("TShirt"), "TShirt", new AssetCacheDefBasic("ShirtGraphic", new string[] { "Graphic" }));
                GlobalVars.ShirtTextureID = GetItemTextureID(GlobalVars.UserCustomization.ReadSetting("Shirt"), "Shirt", new AssetCacheDefBasic("Shirt", new string[] { "ShirtTemplate" }));
                GlobalVars.PantsTextureID = GetItemTextureID(GlobalVars.UserCustomization.ReadSetting("Pants"), "Pants", new AssetCacheDefBasic("Pants", new string[] { "PantsTemplate" }));
                GlobalVars.FaceTextureID = GetItemTextureID(GlobalVars.UserCustomization.ReadSetting("Face"), "Face", new AssetCacheDefBasic("Decal", new string[] { "Texture" }));

                GlobalVars.TShirtTextureLocal = GetItemTextureLocalPath(GlobalVars.TShirtTextureID, "TShirt");
                GlobalVars.ShirtTextureLocal = GetItemTextureLocalPath(GlobalVars.ShirtTextureID, "Shirt");
                GlobalVars.PantsTextureLocal = GetItemTextureLocalPath(GlobalVars.PantsTextureID, "Pants");
                GlobalVars.FaceTextureLocal = GetItemTextureLocalPath(GlobalVars.FaceTextureID, "Face");
            }
        }

        public static void CreateAssetCacheDirectories()
        {
            if (!Directory.Exists(GlobalPaths.AssetCacheDirAssets))
            {
                Directory.CreateDirectory(GlobalPaths.AssetCacheDirAssets);
            }
        }

        public static void CreateInitialFileListIfNeededMulti()
        {
            if (GlobalVars.NoFileList)
                return;

            string filePath = GlobalPaths.ConfigDir + "\\InitialFileList.txt";

            if (!File.Exists(filePath))
            {
                Util.ConsolePrint("WARNING - No file list detected. Generating Initial File List.", 5);
                Thread t = new Thread(CreateInitialFileList);
                t.IsBackground = true;
                t.Start();
            }
            else
            {
                int lineCount = File.ReadLines(filePath).Count();
                int fileCount = 0;

                string filterPath = GlobalPaths.ConfigDir + @"\\" + GlobalPaths.InitialFileListIgnoreFilterName;
                string[] fileListToIgnore = File.ReadAllLines(filterPath);

                DirectoryInfo dinfo = new DirectoryInfo(GlobalPaths.RootPath);
                FileInfo[] Files = dinfo.GetFiles("*.*", SearchOption.AllDirectories);
                foreach (FileInfo file in Files)
                {
                    DirectoryInfo localdinfo = new DirectoryInfo(file.DirectoryName);
                    string directory = localdinfo.Name;
                    if (!fileListToIgnore.Contains(file.Name, StringComparer.InvariantCultureIgnoreCase) && !fileListToIgnore.Contains(directory, StringComparer.InvariantCultureIgnoreCase))
                    {
                        fileCount++;
                    }
                    else
                    {
                        continue;
                    }
                }

                //MessageBox.Show(lineCount + "\n" + fileCount);
                // commenting this because frankly the CreateInitialFileList thread should be called upon inital bootup of novetus.
                /*if (lineCount != fileCount)
                {
                    Util.ConsolePrint("WARNING - Initial File List is not relevant to file path. Regenerating.", 5);
                    Thread t = new Thread(CreateInitialFileList);
                    t.IsBackground = true;
                    t.Start();
                }*/
            }
        }

        private static void CreateInitialFileList()
        {
            string filterPath = GlobalPaths.ConfigDir + @"\\" + GlobalPaths.InitialFileListIgnoreFilterName;
            string[] fileListToIgnore = File.ReadAllLines(filterPath);
            string FileName = GlobalPaths.ConfigDir + "\\InitialFileList.txt";

            File.Create(FileName).Close();

            using (var txt = File.CreateText(FileName))
            {
                DirectoryInfo dinfo = new DirectoryInfo(GlobalPaths.RootPath);
                FileInfo[] Files = dinfo.GetFiles("*.*", SearchOption.AllDirectories);
                foreach (FileInfo file in Files)
                {
                    DirectoryInfo localdinfo = new DirectoryInfo(file.DirectoryName);
                    string directory = localdinfo.Name;
                    if (!fileListToIgnore.Contains(file.Name, StringComparer.InvariantCultureIgnoreCase) && !fileListToIgnore.Contains(directory, StringComparer.InvariantCultureIgnoreCase))
                    {
                        txt.WriteLine(file.FullName);
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            Util.ConsolePrint("File list generation finished.", 4);
        }
    }
    #endregion
}