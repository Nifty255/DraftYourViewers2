using System.Collections;
using System.IO;
using UnityEngine;
using CodeNifty.DraftYourViewers2;

public class LoadBundle : MonoBehaviour
{
    private static char slash = Path.DirectorySeparatorChar;
    private AssetBundle assetBundle;
    private GameObject package;
    private UnityDummyDraftActor actor;
    private DraftManager manager;

    void Start()
    {
        StartCoroutine(LoadApp());
    }

    private IEnumerator LoadApp()
    {
        //string assetPath = $"{KSPUtil.ApplicationRootPath}{slash}GameData{slash}Mods{slash}Draft Your Viewers 2{slash}Assets.bundle";
        string assetPath = $"{Application.persistentDataPath}{slash}draftyourviewers2";
        AssetBundleCreateRequest loadBundleOp = AssetBundle.LoadFromFileAsync(assetPath);
        yield return loadBundleOp;
        assetBundle = loadBundleOp.assetBundle;
        if (assetBundle == null)
        {
            CodeNifty.DraftYourViewers2.Logger.LogError("Couldn't load bundle Sadge");
            yield break;
        }

        AssetBundleRequest loadPrefabOp = assetBundle.LoadAssetAsync<GameObject>("Assets/UI/DYV2Package.prefab");
        yield return loadPrefabOp;

        package = Instantiate(loadPrefabOp.asset as GameObject);
        DontDestroyOnLoad(package);

        actor = package.AddComponent<UnityDummyDraftActor>();

        manager = package.GetComponent<DraftManager>();
        manager.draftActor = actor;
    }
}
