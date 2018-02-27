using Harmony;
using BepInEx;
using KoikatsuUnlimited.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace KoikatsuUnlimited
{
    
    public class KoikatsuUnlimited : BaseUnityPlugin
    {
        public override string ID => "com.geneishouko.koikatsuunlimited";
        public override string Name => "Koikatsu Unlimited Startup Pack";
        public override Version Version => new Version("0.0");

        private static WeakReference OverridingCharacterReference = new WeakReference(null);
        public static ChaFile OverridingCharacter
        {
            get { return OverridingCharacterReference.IsAlive ? OverridingCharacterReference.Target as ChaFile : null; }
            set { OverridingCharacterReference.Target = value; }
        }

        public delegate void CardEventHandler(ChaFile file);
        public static event CardEventHandler OverriddenCardChanged;

        #region Extensions
        private static List<Extension> Extensions = new List<Extension>();
        private static List<string> ExtensionNames = new List<string>();

        public static void AddExtension(Extension extension)
        {
            Extensions.Add(extension);
            ExtensionNames.Add(extension.Name);
        }

        private static void AddBaseExtensions()
        {
            AddExtension(new TextureOverride());
        }

        private static void StartBaseExtensions()
        {
            foreach (Extension extension in Extensions)
            {
                extension.Start();
            }
        }
        #endregion

        #region CharacterData
        private static WeakKeyDictionary<ChaFile, Dictionary<String, object>> CharacterData = new WeakKeyDictionary<ChaFile, Dictionary<String, object>>();

        public static object GetExtensionData(ChaFile character, String id)
        {
            Dictionary<string, object> dict = CharacterData.Get(character);
            object data = null;
            dict.TryGetValue(id, out data);
            return data;
        }

        public static void SetExtensionData(ChaFile character, String id, object data)
        {
            CharacterData.Get(character)[id] = data;
        }
        #endregion

        #region MonoBehaviour
        void Awake()
        {
            AddBaseExtensions();
            InstallLoaderHooks();
        }

        void Start()
        {
            StartBaseExtensions();
            ExtensibleSaveFormat.ExtensibleSaveFormat.CardBeingSaved += OnCardSave;
            ExtensibleSaveFormat.ExtensibleSaveFormat.CardBeingLoaded += OnCardLoad;
        }

        void Update()
        {
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F4))
            {
                showingUI = !showingUI;
            }
        }
        #endregion

        #region UI
        private static Extension CurrentExtension;
        private Rect UI = new Rect(20, 20, 400, 200);
        private float ExtensionButtonWidth = 100;
        private float ExtensionButtonHeight = 20;
        private float ExtensionButtonSpacing = 20;
        bool showingUI = false;

        void OnGUI()
        {
            if (showingUI)
                UI = GUI.Window(Name.GetHashCode() + 0, UI, WindowFunction, "Koikatsu Unlimited");
        }

        void WindowFunction(int windowID)
        {
            int count = Extensions.Count;
            for(int i = 0; i < count; ++i)
            {
                if (GUI.Button(new Rect(ExtensionButtonSpacing + (ExtensionButtonSpacing + ExtensionButtonWidth) * i, ExtensionButtonSpacing, ExtensionButtonWidth, ExtensionButtonHeight), ExtensionNames[i]))
                {
                    CurrentExtension = Extensions[i];
                }
            }
            if (CurrentExtension != null)
                CurrentExtension.WindowFunction(windowID);
            GUI.DragWindow();
        }
        #endregion

        #region AssetLoader
        private WeakKeyDictionary<ChaFile, object> characters = new WeakKeyDictionary<ChaFile, object>();

        static void InstallLoaderHooks()
        {
            var harmony = HarmonyInstance.Create("com.koikatsuunlimited");

            MethodInfo original = AccessTools.Method(typeof(ChaControl), "ReloadAsync");
            var prefix = new HarmonyMethod(typeof(KoikatsuUnlimited).GetMethod("CharacterReloadPrefix"));
            var postfix = new HarmonyMethod(typeof(KoikatsuUnlimited).GetMethod("CharacterReloadPostfix"));
            harmony.Patch(original, prefix, /*postfix*/null);
        }

        public static void CharacterReloadPrefix(ChaControl __instance)
        {
            OverridingCharacter = __instance.chaFile;
            OverriddenCardChanged?.Invoke(OverridingCharacter);
        }

        public static void CharacterReloadPostfix()
        {
            OverridingCharacter = null;
            //OverriddenCardChanged?.Invoke(OverridingCharacter); // Outside of Maker
        }

        #endregion

        #region ExtensibleSaveFormat
        void OnCardSave(ChaFile file)
        {
            ExtensibleSaveFormat.ExtensibleSaveFormat.SetExtendedFormat(file, CharacterData.Get(file));
        }

        static int count = 0;
        void OnCardLoad(ChaFile file)
        {
            BepInEx.BepInLogger.Log("OnCardLoad", true);
            //Shared.Debug.DumpStackTrace();
            var data = ExtensibleSaveFormat.ExtensibleSaveFormat.GetExtendedFormat(file);
            if (data == null)
            {
                data = new Dictionary<string, object>();
                count++;
                BepInEx.BepInLogger.Log($"Created {count} character datas! ", true);
            }
            CharacterData.Set(file, data);
        }
        #endregion
    }

}
