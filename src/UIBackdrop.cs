using UnityEngine;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;
using System;

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
        private static Panel backdropPanel1;
        private static Panel backdropPanel2;
        private static Panel backdropPanel3;

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

            // Add backdrop to the top window's component, but disabling the previous windows backdrop up to two in case there were any
            // Press ESC -> mod settings -> UI Backdrop -> select color to test this scenario
            if ((sender as UserInterfaceManager).WindowCount == 3)
            {
                backdropPanel1.Enabled = false;
                backdropPanel2.Enabled = false;
                (sender as UserInterfaceManager).TopWindow.ParentPanel.Components.Add(backdropPanel3);
            }
            // Press 'i' twice in game to test this scenario
            else if ((sender as UserInterfaceManager).WindowCount == 2)
            {
                backdropPanel2.Enabled = true;
                backdropPanel1.Enabled = false;
                (sender as UserInterfaceManager).TopWindow.ParentPanel.Components.Add(backdropPanel2);
            }
            else if ((sender as UserInterfaceManager).WindowCount == 1)
            {
                backdropPanel1.Enabled = true;
                (sender as UserInterfaceManager).TopWindow.ParentPanel.Components.Add(backdropPanel1);
            }

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
                // Create the panels that will be re-used by up to three windows
                backdropPanel1 = new Panel
                {
                    BackgroundColor = modSettings.GetColor("Settings", "BackdropColor"),
                    Name = "UIBackdropMod.BackdropPanel",
                    Size = new Vector2(Screen.width, Screen.height)
                };
                backdropPanel2 = new Panel
                {
                    BackgroundColor = modSettings.GetColor("Settings", "BackdropColor"),
                    Name = "UIBackdropMod.BackdropPanel",
                    Size = new Vector2(Screen.width, Screen.height)
                };
                backdropPanel3 = new Panel
                {
                    BackgroundColor = modSettings.GetColor("Settings", "BackdropColor"),
                    Name = "UIBackdropMod.BackdropPanel",
                    Size = new Vector2(Screen.width, Screen.height)
                };
                UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Rest, modSettings.GetValue<bool>("Settings", "EnableRestWindowBackdrop") ? typeof(DaggerfallRestWindowBackdrop) : typeof(DaggerfallRestWindow));
            }
        }

    }
}
