namespace WurstMod.MappingComponents.Generic
{
    public class AnvilPrefab : ComponentProxy
    {
        public string Guid;
        public string Bundle;
        public string AssetName;

        public override void InitializeComponent()
        {
            var callback = AnvilManager.LoadAsync(new Anvil.AssetID {Guid = Guid, Bundle = Bundle, AssetName = AssetName});
            callback.CompleteNow();
            var prefab = Instantiate(callback.Result, transform.position, transform.rotation);
            prefab.SetActive(true);
        }

    }
}