using UnityEngine;

namespace WurstMod.Any
{
    public class AnvilPrefab : ComponentProxy
    {
        public string Guid;
        public string Bundle;
        public string AssetName;

        public override void InitializeComponent()
        {
            var asset = new Anvil.AssetID();
            asset.Guid = Guid;
            asset.Bundle = Bundle;
            asset.AssetName = AssetName;
            Debug.Log("Asset bundle name: " + Bundle);

            var callback = AnvilManager.LoadAsync(asset);
            callback.CompleteNow();

            var prefab = Instantiate(callback.Result, transform.position, transform.rotation);
            prefab.SetActive(true);
        }

    }
}