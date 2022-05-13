using Dalamud.Game;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Framework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;

namespace SamplePlugin
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "CrashFFXIVPlugin";

        private DalamudPluginInterface PluginInterface { get; init; }
        private Configuration Configuration { get; init; }

        // directly copied from Dalamud. I'm lazy and didn't want to make a JSON deserializer myself.
        internal record PluginManifest
        {
            [JsonProperty] public string? Author { get; init; }
            [JsonProperty] public string? Name { get; set; }
            [JsonProperty] public string? Punchline { get; init; }
            [JsonProperty] public string? Description { get; init; }
            [JsonProperty] public string? Changelog { get; init; }
            [JsonProperty] public List<string>? Tags { get; init; }
            [JsonProperty] public List<string>? CategoryTags { get; init; }
            [JsonProperty] public bool IsHide { get; init; }
            [JsonProperty] public string? InternalName { get; init; }
            [JsonProperty] public Version? AssemblyVersion { get; init; }
            [JsonProperty] public Version? TestingAssemblyVersion { get; init; }
            [JsonProperty] public bool IsTestingExclusive { get; init; }
            [JsonProperty] public string? RepoUrl { get; init; }
            [JsonProperty] [JsonConverter(typeof(GameVersionConverter))] public GameVersion? ApplicableVersion { get; init; } = GameVersion.Any;
            [JsonProperty] public int? DalamudApiLevel { get; init; } = 6;
            [JsonProperty] public long? DownloadCount { get; init; }
            [JsonProperty] public long? LastUpdate { get; init; }
            [JsonProperty] public string? DownloadLinkInstall { get; init; }
            [JsonProperty] public string? DownloadLinkUpdate { get; init; }
            [JsonProperty] public string? DownloadLinkTesting { get; init; }
            [JsonProperty] public int LoadPriority { get; init; }
            public List<string>? ImageUrls { get; init; }
            public string? IconUrl { get; init; }
            public bool AcceptsFeedback { get; init; } = true;
            public string? FeedbackMessage { get; init; }
            public string? FeedbackWebhook { get; init; }
        }
        internal record LocalPluginManifest : PluginManifest
        {
            public bool Disabled { get; set; }
            public bool Testing { get; set; }
            public string InstalledFromUrl { get; set; } = string.Empty;
            public bool IsThirdParty => !string.IsNullOrEmpty(this.InstalledFromUrl);
            public void Save(FileInfo manifestFile) => File.WriteAllText(manifestFile.FullName, JsonConvert.SerializeObject(this, Formatting.Indented));
            public static LocalPluginManifest Load(FileInfo manifestFile) => JsonConvert.DeserializeObject<LocalPluginManifest>(File.ReadAllText(manifestFile.FullName))!;
        }

        public Plugin(DalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            this.Configuration.NumberOfCrashes++;
            this.Configuration.Save();

            var plogonPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (plogonPath != null)
            {
                File.Create(Path.Combine(plogonPath, ".disabled")).Close();

                // be extra cautious and try disabling the plugin twice.
                FileInfo manifestFile = new FileInfo(Path.Combine(plogonPath, Name + ".json"));
                LocalPluginManifest manifest = LocalPluginManifest.Load(manifestFile);
                manifest.Disabled = true;
                manifest.Save(manifestFile);
            }
            

            unsafe
            {
                var framework = Framework.Instance();
                framework->UIModule = (UIModule*)0;
            }
            

            //System.Environment.FailFast("Silly plogon user chose to crash game.");
        }

        public void Dispose()
        {
        }
    }
}
