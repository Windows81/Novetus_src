﻿#region Usings
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
#endregion

#region SDKApps
enum SDKApps
{
    ClientSDK,
    AssetSDK,
    ItemCreationSDK,
    ClientScriptDoc,
    SplashTester,
    ScriptGenerator,
    LegacyPlaceConverter,
    DiogenesEditor,
    ClientScriptTester,
    XMLContentEditor
}
#endregion

#region Novetus SDK Launcher
public partial class NovetusSDK : Form
{
    #region Constructor
    public NovetusSDK()
    {
        InitializeComponent();
    }
    #endregion

    #region Form Events
    private void NovetusSDK_Load(object sender, EventArgs e)
    {
        if (!File.Exists(GlobalPaths.DataDir + "\\RSG.exe"))
        {
            DisableApp(SDKApps.ScriptGenerator);
        }

        if (!File.Exists(GlobalPaths.DataDir + "\\Roblox_Legacy_Place_Converter.exe"))
        {
            DisableApp(SDKApps.LegacyPlaceConverter);
        }

        if (!GlobalFuncs.IsClientValid("ClientScriptTester"))
        {
            DisableApp(SDKApps.ClientScriptTester);
        }

        Text = "Novetus SDK " + GlobalVars.ProgramInformation.Version;
        label1.Text = GlobalVars.ProgramInformation.Version;
    }

    private void NovetusSDK_Close(object sender, CancelEventArgs e)
    {
        GlobalFuncs.Config(GlobalPaths.ConfigDir + "\\" + GlobalPaths.ConfigName, true);
#if LAUNCHER
        GlobalFuncs.ReadClientValues(null);
#else
        GlobalFuncs.ReadClientValues();
#endif
    }

    private void listView1_SelectedIndexChanged(object sender, EventArgs e)
    {
        int selectedIndex = 0;

        if (listView1.SelectedIndices.Count > 0)
        {
            selectedIndex = listView1.SelectedIndices[0];
        }

        LaunchSDKAppByIndex(selectedIndex);
    }
    #endregion

    #region Functions
    void DisableApp(SDKApps app)
    {
        ListViewItem appItem = listView1.Items[(int)app];
        appItem.Text = appItem.Text + " (Disabled)";
    }

    void LaunchSDKAppByIndex(int index)
    {
        ListViewItem appItem = listView1.Items[index];

        if (appItem.Text.Contains("Disabled"))
        {
            string errorText = GlobalVars.ProgramInformation.IsLite ?
                "This application has been disabled to save space. Please download the Full version of Novetus to use all SDK tools." :
                "This application has been disabled.";

            MessageBox.Show(errorText, "Novetus SDK - App Disabled", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        SDKApps selectedApp = (SDKApps)index;

        switch (selectedApp)
        {
            case SDKApps.AssetSDK:
                AssetSDK asset = new AssetSDK();
                asset.Show();
                break;
            case SDKApps.ItemCreationSDK:
                ItemCreationSDK icsdk = new ItemCreationSDK();
                icsdk.Show();
                break;
            case SDKApps.ClientScriptDoc:
                ClientScriptDocumentation csd = new ClientScriptDocumentation();
                csd.Show();
                break;
            case SDKApps.SplashTester:
                SplashTester st = new SplashTester();
                st.Show();
                break;
            case SDKApps.ScriptGenerator:
                Process proc = new Process();
                proc.StartInfo.FileName = GlobalPaths.DataDir + "\\RSG.exe";
                proc.Start();
                break;
            case SDKApps.LegacyPlaceConverter:
                Process proc2 = new Process();
                proc2.StartInfo.FileName = GlobalPaths.DataDir + "\\Roblox_Legacy_Place_Converter.exe";
                proc2.Start();
                break;
            case SDKApps.DiogenesEditor:
                DiogenesEditor dio = new DiogenesEditor();
                dio.Show();
                break;
            case SDKApps.ClientScriptTester:
                MessageBox.Show("Note: If you want to test a specific way of loading a client, select the ClientScript Tester in the 'Versions' tab of the Novetus Launcher, then launch it through any way you wish.", "Novetus SDK - Client Script Tester Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
#if LAUNCHER
                GlobalFuncs.LaunchRBXClient("ClientScriptTester", ScriptType.Client, false, false, null, null);
#else
                GlobalFuncs.LaunchRBXClient("ClientScriptTester", ScriptType.Client, false, false, null);
#endif
                GlobalVars.GameOpened = ScriptType.None;
                break;
            case SDKApps.XMLContentEditor:
                XMLContentEditor xml = new XMLContentEditor();
                xml.Show();
                break;
            default:
                ClientinfoEditor cie = new ClientinfoEditor();
                cie.Show();
                break;
        }
    }
    #endregion
}
#endregion
