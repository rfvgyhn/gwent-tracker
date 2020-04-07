using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using GwentTracker.Localization;
using GwentTracker.Model;
using ReactiveUI;
using Serilog;

namespace GwentTracker.ViewModels
{
    public class CardViewModel : ReactiveObject
    {
        private readonly TextureInfo _textureInfo;
        private static readonly Lazy<IBitmap> FallbackTexture = new Lazy<IBitmap>(() => LoadLocalBitmap("avares://GwentTracker/Assets/fallback.png"));
        private static readonly HttpClient HttpClient = new HttpClient();
        private readonly Translate _t;

        public CardViewModel(TextureInfo textureInfo)
        {
            _textureInfo = textureInfo;
            _t = new Translate();
            LoadTexture = ReactiveCommand.CreateFromTask(LoadImage);
            LoadTexture.Subscribe(image =>
            {
                Texture = image;
            });
            LoadTexture.ThrownExceptions.Subscribe(e =>
            {
                Log.Error(e, "Error loading bitmap for {url}", RemoteTextureUrl);
            });
        }

        private string RemoteTextureUrl => string.Format(_textureInfo.RemotePathFormat, Index);
        private string LocalTextureUrl => string.Format(_textureInfo.LocalPathFormat, Index);
        private async Task<IBitmap> LoadImage(CancellationToken cancellationToken)
        {
            return LoadLocalBitmap(LocalTextureUrl) 
                   ?? await LoadRemoteBitmap(RemoteTextureUrl, (_textureInfo.CacheRemote, LocalTextureUrl), cancellationToken)
                   ?? FallbackTexture.Value;
        }

        private static IBitmap LoadLocalBitmap(string url)
        {
            try
            {
                if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == "avares")
                {
                    var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                    var bitmap = new Bitmap(assets.Open(uri));

                    Log.Debug("Using embedded texture at {uri}", uri);
                    return bitmap;
                }

                if (!File.Exists(url))
                {
                    Log.Warning("Local texture doesn't exist at {url}", url);
                    return null;
                }

                Log.Debug("Using local texture at {url}", url);
                return new Bitmap(url);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error loading bitmap from {url}", url);
                return null;
            }
        }

        private static async Task<IBitmap> LoadRemoteBitmap(string url, (bool enabled, string path) cache, CancellationToken cancellationToken)
        {
            try
            {
                var response = await HttpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    var bitmap = new Bitmap(stream);

                    if (cache.enabled)
                    {
                        var directoryInfo = new FileInfo(cache.path).Directory;
                        if (directoryInfo != null)
                            Directory.CreateDirectory(directoryInfo.FullName);
                        
                        bitmap.Save(cache.path);
                        Log.Debug("Cached texture at {path}", cache.path);
                    }

                    Log.Debug("Using remote texture at {url}", url);
                    return bitmap;
                }

                Log.Error("Error loading bitmap from {url}: {status}", url, $"{(int)response.StatusCode} - {response.ReasonPhrase}");
                return null;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error loading bitmap from {url}", url);
                return null;
            }
        }
        
        public ReactiveCommand<Unit, IBitmap> LoadTexture { get; }
        public int Index { get; set; }
        public string Name { get; set; }
        public string Flavor { get; set; }
        public int MaxCopies { get; set; }
    
        private int _copies;
        public int Copies
        {
            get => _copies;
            set => this.RaiseAndSetIfChanged(ref _copies, value);
        }
        private bool _obtained;
        public bool Obtained
        {
            get => _obtained;
            set => this.RaiseAndSetIfChanged(ref _obtained, value);
        }
        private bool _isVisible;
        public bool IsHidden
        {
            get => _isVisible;
            set => this.RaiseAndSetIfChanged(ref _isVisible, value);
        }

        private IBitmap _texture;
        public IBitmap Texture
        {
            get => _texture;
            set => this.RaiseAndSetIfChanged(ref _texture, value);
        }
        public string Deck { get; set; }
        public string Type { get; set; }
        public string Location => string.Join(", ", Locations.Select(l => l.Type).Distinct());
        public string Region => string.Join(", ", Locations.Select(l => l.Type == _t["Base Deck"] ? l.Type : l.Region).Distinct());
        public IEnumerable<string> DetailedLocations => Locations.Select(DetailedLocation).Where(l => l != "");
        public CombatDetails Combat { get; set; }
        public Location[] Locations { get; set; }
        public CardSource Source { get; set; }
        
        private string DetailedLocation(Location l)
        {
            if (l.Type == _t["Base Deck"] || l.Region == _t["Random"])
                return "";
            
            var details = new[] {l.Npc, l.Area, l.Territory ?? l.Region}.Where(s => !string.IsNullOrWhiteSpace(s));
            return string.Join(", ", details);
        }
    }
}
