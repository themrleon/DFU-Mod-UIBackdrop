using UnityEngine;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;
using System;
using System.Collections.Generic;

namespace UIBackdropMod
{
    //-----------------------------------------------------------------------------------
    public class DaggerfallRestWindowBackdrop : DaggerfallRestWindow
    {
        public DaggerfallRestWindowBackdrop(IUserInterfaceManager uiManager, bool ignoreAllocatedBed = false) : base(uiManager) { }

        protected override void Setup()
        {
            base.Setup();
            ParentPanel.BackgroundColor = Color.clear;
        }
    }

    //-----------------------------------------------------------------------------------
    public class UIBackdropMod : MonoBehaviour
    {
        public static Mod Mod;
        private static List<Panel> backdropPanels = new List<Panel>();

        private Type[] skipWindowTypes = {
            typeof(DaggerfallWorkshop.Game.UserInterfaceWindows.DaggerfallVidPlayerWindow),
            typeof(DaggerfallWorkshop.Game.UserInterfaceWindows.DaggerfallStartWindow),
            typeof(DaggerfallWorkshop.Game.UserInterfaceWindows.DaggerfallUnitySaveGameWindow),
            typeof(DaggerfallWorkshop.Game.UserInterfaceWindows.DaggerfallHUD),
            typeof(DaggerfallWorkshop.Game.UserInterfaceWindows.DaggerfallStartWindow),
            typeof(DaggerfallWorkshop.Game.UserInterfaceWindows.StartNewGameWizard),
            typeof(DaggerfallWorkshop.Game.UserInterfaceWindows.CreateCharRaceSelect),
            typeof(DaggerfallWorkshop.Game.UserInterfaceWindows.CreateCharBiography),
            typeof(DaggerfallWorkshop.Game.UserInterfaceWindows.CreateCharNameSelect),
            typeof(DaggerfallWorkshop.Game.UserInterfaceWindows.CreateCharFaceSelect),
            typeof(DaggerfallWorkshop.Game.UserInterfaceWindows.CreateCharAddBonusStats),
            typeof(DaggerfallWorkshop.Game.UserInterfaceWindows.CreateCharAddBonusSkills),
            typeof(DaggerfallWorkshop.Game.UserInterfaceWindows.CreateCharReflexSelect),
            typeof(DaggerfallWorkshop.Game.UserInterfaceWindows.CreateCharClassQuestions),
            typeof(DaggerfallWorkshop.Game.UserInterfaceWindows.CreateCharSummary)
        };

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            Mod = initParams.Mod;
            new GameObject(Mod.Title).AddComponent<UIBackdropMod>();
            Mod.LoadSettingsCallback = LoadSettings;
            Mod.IsReady = true;
        }

        private void Start()
        {
            for (int i = 0; i < 10; i++)
            {
                backdropPanels.Add(
                    new Panel
                    {
                        BackgroundColor = Color.black,
                        Name = "UIBackdropMod.BackdropPanel",
                        Size = new Vector2(Screen.width, Screen.height)
                    }
                );
            }
            Mod.LoadSettings();
            DaggerfallUI.Instance.UserInterfaceManager.OnWindowChange += OnWindowChangeEvent;
        }

        public void OnWindowChangeEvent(object sender, EventArgs e)
        {
            // Debug.Log("==================================================================");
            Type windowType = (sender as UserInterfaceManager).TopWindow.GetType();
            // Debug.Log("TYPE=" + windowType);

            if (Array.Exists(skipWindowTypes, type => type == windowType))
            {
                // Debug.Log("SKIPPING WINDOW!");
                return;
            }

            int ComponentsCount = (sender as UserInterfaceManager).TopWindow.ParentPanel.Components.Count;

            // Do a backup of the top window's components
            BaseScreenComponent[] backupComponents = new BaseScreenComponent[ComponentsCount];
            for (int i = 1; i < ComponentsCount; i++)
            {
                backupComponents[i] = (sender as UserInterfaceManager).TopWindow.ParentPanel.Components[i];
            }

            // Remove top window's components
            (sender as UserInterfaceManager).TopWindow.ParentPanel.Components.Clear();

            // Start new game and go with custom class to see up to 5 ou even 6 stacked up window list!
            // Disable all backdrops keeping only the one that will be used
            int windowCount = (sender as UserInterfaceManager).WindowCount;
            foreach (Panel panel in backdropPanels)
            {
                panel.Enabled = false;
            }
            backdropPanels[windowCount - 1].Enabled = true;
            (sender as UserInterfaceManager).TopWindow.ParentPanel.Components.Add(backdropPanels[windowCount - 1]);

            // Restore top window's components
            for (int i = 1; i < ComponentsCount; i++)
            {
                (sender as UserInterfaceManager).TopWindow.ParentPanel.Components.Add(backupComponents[i]);
            }
        }

        static void LoadSettings(ModSettings modSettings, ModSettingsChange change)
        {
            if (modSettings != null)
            {
                foreach (Panel panel in backdropPanels)
                {
                    panel.BackgroundColor = modSettings.GetColor("Settings", "BackdropColor");
                }
                UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Rest, modSettings.GetValue<bool>("Settings", "EnableRestWindowBackdrop") ? typeof(DaggerfallRestWindowBackdrop) : typeof(DaggerfallRestWindow));
            }
        }

    }
}
