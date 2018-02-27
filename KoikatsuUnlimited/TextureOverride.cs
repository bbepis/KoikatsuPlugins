using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KoikatsuUnlimited
{
    class TextureOverride : Extension
    {
        public override string ID => "com.geneishouko.textureoverride";
        public override string Name => "Texture Override";

        public static string OverridesDir => Path.Combine(BepInEx.Common.Utility.ExecutingDirectory, "overrides");
        List<List<String>> CharacterData;

        public override void Start()
        {
            KoikatsuUnlimited.OverriddenCardChanged += OverriddenCardChanged;
            ResourceRedirector.ResourceRedirector.AssetResolvers.Add(LoadAsset);
        }

        public override void WindowFunction(int windowID)
        {
            if (CharacterData != null)
            {
                int count = 0;
                foreach (List<String> rule in CharacterData)
                {
                    rule[1] = GUI.TextField(new Rect(20, 80 + 40 * count, 100, 20), rule[1]);
                    rule[2] = GUI.TextField(new Rect(140, 80 + 40 * count, 100, 20), rule[2]);
                }
            }
        }

        static bool test = true;
        public bool LoadAsset(string assetBundleName, string assetName, Type type, string manifestAssetBundleName, out AssetBundleLoadAssetOperation result)
        {
            if (assetName == "cw_t_hitomi_010")
                Shared.Debug.DumpStackTrace();
            if (CharacterData == null)
            {
                if (test)
                    BepInEx.BepInLogger.Log("Character has no override data! ", true);
                test = false;
                result = null;
                return false;
            }
            test = true;
            foreach(List<String> rule in CharacterData)
            {
                //BepInEx.BepInLogger.Log($"{rule[1]} == {assetName}?", true);
                if (rule[1] == assetName)
                {
                    string path = Path.Combine(OverridesDir, rule[2]);
                    BepInEx.BepInLogger.Log($"Rule Overriding with {path}. ", true);
                    result = new AssetBundleLoadAssetOperationSimulation(ResourceRedirector.AssetLoader.LoadTexture(path));
                    return true;
                }
            }
            result = null;
            return false;
            /*
                string asset = "cw_t_hitomi_000";
            //BepInEx.BepInLogger.Log($"Test. ", false);
            if (assetName == asset)
            {
                string path = Path.Combine(OverridesDir, $"{asset}.png");
                BepInEx.BepInLogger.Log($"Overriding with {path}. ", true);
                result = new AssetBundleLoadAssetOperationSimulation(ResourceRedirector.AssetLoader.LoadTexture(path));
                return true;
            }
            result = null;
            return false;
            */
        }

        void OverriddenCardChanged(ChaFile character)
        {
            if (character == null)
            {
                CharacterData = null;
                return;
            }
            BepInEx.BepInLogger.Log("OverriddenCardChanged! ", true);
            CharacterData = KoikatsuUnlimited.GetExtensionData(character, ID) as List<List<String>>;
            if (CharacterData == null)
            {
                CharacterData = new List<List<string>>();
                KoikatsuUnlimited.SetExtensionData(character, ID, CharacterData);
            }
            if (CharacterData.IsNullOrEmpty())
            {
                BepInEx.BepInLogger.Log("Filling default override data. ", true);
                CharacterData.Add(new List<string> { "", "cw_t_hitomi_000", $"cw_t_hitomi_000.png"  });
            }
        }
    }
}
