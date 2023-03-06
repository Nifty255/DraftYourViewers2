#if TARGET_KERBAL
using System.Collections;
using System.IO;
using UnityEngine;
using BepInEx;

namespace CodeNifty.DraftYourViewers2
{
    public static class PluginInfo
    {
        public const string PLUGIN_GUID = "CodeNifty.DraftYourViewers2";
        public const string PLUGIN_NAME = "CodeNifty.DraftYourViewers2";
        public const string PLUGIN_VERSION = "1.0.0";
    }

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static char slash = Path.DirectorySeparatorChar;
        private AssetBundle assetBundle;
        private GameObject package;
        private KerbalDraftActor actor;
        private SceneChangeListener listener;
        private DraftManager manager;

        private void Start()
        {
            
            // Plugin startup logic
            DraftYourViewers2.Logger.LogInfo($"Loading up! First stop, the assets.");
            StartCoroutine(LoadApp());
        }

        private IEnumerator LoadApp()
        {
            string assetPath = $"{Directory.GetCurrentDirectory()}{slash}BepInEx{slash}plugins{slash}Draft Your Viewers 2{slash}Asset.bundle";
            AssetBundleCreateRequest loadBundleOp = AssetBundle.LoadFromFileAsync(assetPath);
            yield return loadBundleOp;
            assetBundle = loadBundleOp.assetBundle;
            if (assetBundle == null)
            {
                DraftYourViewers2.Logger.LogError("Couldn't load bundle Sadge");
                yield break;
            }

            AssetBundleRequest loadPrefabOp = assetBundle.LoadAssetAsync<GameObject>("Assets/UI/DYV2Package.prefab");
            yield return loadPrefabOp;

            package = Instantiate(loadPrefabOp.asset as GameObject);
            DontDestroyOnLoad(package);

            actor = package.AddComponent<KerbalDraftActor>();
            listener = package.AddComponent<SceneChangeListener>();

            manager = package.GetComponent<DraftManager>();
            manager.draftActor = actor;
            manager.savePathGetter = listener;
            listener.draftManager = manager;
        }
    }
}
#endif