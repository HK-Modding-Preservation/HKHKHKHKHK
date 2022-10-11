using Modding;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HKHKHKHKHK.MonoBehaviours;
using UnityEngine;
using UObject = UnityEngine.Object;
using SFCore.Generics;

namespace HKHKHKHKHK
{
    public class Hkhkhkhkhk : FullSettingsMod<HhhhhSaveSettings, HhhhhGlobalSettings>
    {
        internal static Hkhkhkhkhk Instance;

        public override string GetVersion() => SFCore.Utils.Util.GetVersion(Assembly.GetExecutingAssembly());

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");
            Instance = this;

            InitGlobalSettings();
            InitCallbacks();

            Log("Initialized");
        }

        private void InitGlobalSettings()
        {
            // Found in a project, might help saving, don't know, but who cares
            // Global Settings
        }

        private void InitSaveSettings(SaveGameData data)
        {
            // Found in a project, might help saving, don't know, but who cares
            // Save Settings
        }

        private void InitCallbacks()
        {
            // Hooks
            On.HeroController.Start += HeroControllerOnStart;
        }

        private void HeroControllerOnStart(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);
            var comp = self.gameObject.AddComponent<VvvvvHandler>();
        }

        private void SaveTotGlobalSettings()
        {
            SaveGlobalSettings();
        }

        private void PrintDebug(GameObject go, string tabindex = "", int parentCount = 0)
        {
            Transform parent = go.transform.parent;
            for (int i = 0; i < parentCount; i++)
            {
                if (parent == null) continue;

                Log(tabindex + "DEBUG parent: " + parent.gameObject.name);
                parent = parent.parent;
            }
            Log(tabindex + "DEBUG Name: " + go.name);
            foreach (var comp in go.GetComponents<Component>())
            {
                Log(tabindex + "DEBUG Component: " + comp.GetType());
            }
            for (int i = 0; i < go.transform.childCount; i++)
            {
                PrintDebug(go.transform.GetChild(i).gameObject, tabindex + "\t");
            }
        }

        private static void SetInactive(GameObject go)
        {
            if (go == null) return;

            Object.DontDestroyOnLoad(go);
            go.SetActive(false);
        }

        private static void SetInactive(Object go)
        {
            if (go != null)
            {
                Object.DontDestroyOnLoad(go);
            }
        }
    }
}