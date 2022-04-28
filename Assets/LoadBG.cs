using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class LoadBG : MonoBehaviour
{

    public Sprite sprite;
    AssetReferenceTexture2D texture;

    void Start()
    {
        Addressables.LoadAssetAsync<Texture2D>("Start Screen").Completed += OnLoadDone;
    }

    private void OnLoadDone(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<Texture2D> obj)
    {
        // In a production environment, you should add exception handling to catch scenarios such as a null result;
        sprite = Sprite.Create(obj.Result, new Rect(0, 0, obj.Result.width, obj.Result.height), new Vector2(0.5f, 0.5f));
        GetComponent<Image>().sprite = sprite;
    }
}
