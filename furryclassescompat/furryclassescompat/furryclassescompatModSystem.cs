using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Newtonsoft.Json.Linq;

namespace furryclassescompat
{
    public class furryclassescompatModSystem : ModSystem
    {
        private FurryCompatConfig _config;

        public override void Start(ICoreAPI api)
        {
            try
            {
                _config = api.LoadModConfig<FurryCompatConfig>("FurryClassesCompat.json");
                if (_config == null)
                {
                    _config = new FurryCompatConfig();
                    api.StoreModConfig(_config, "FurryClassesCompat.json");
                }
            }
            catch
            {
                _config = new FurryCompatConfig();
            }
        }

        public override void AssetsLoaded(ICoreAPI api)
        {
            if (_config == null) return;

            foreach (var entry in _config.AllowedClasses)
            {
                string animal = entry.Key;
                string[] classesToAdd = entry.Value;

                AssetLocation loc = new AssetLocation("furry", $"config/customplayermodels/{animal}.json");
                IAsset asset = api.Assets.TryGet(loc);

                if (asset != null)
                {
                    api.Logger.Notification($"[FurryClassesCompat] Successfully found asset for {animal}.");
                    try
                    {
                        JsonObject json = JsonObject.FromJson(Encoding.UTF8.GetString(asset.Data));

                        if (json[animal].Exists && json[animal]["AvailableClasses"].Exists)
                        {
                            List<string> existing = [.. json[animal]["AvailableClasses"].AsArray<string>()];
                            bool modified = false;

                            foreach (string classId in classesToAdd)
                            {
                                if (!existing.Contains(classId, StringComparer.OrdinalIgnoreCase))
                                {
                                    existing.Add(classId);
                                    modified = true;
                                }
                            }

                            if (modified)
                            {
                                json[animal].Token["AvailableClasses"] = JToken.FromObject(existing.ToArray());
                                asset.Data = Encoding.UTF8.GetBytes(json.ToString());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        api.Logger.Error($"[FurryClassesCompat] Failed to patch {loc}: {e.Message}");
                    }
                }
            }
        }
    }
}
