using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using GwentTracker.Model;
using ReactiveUI;
using Serilog;

namespace GwentTracker.ViewModels
{
    public class CardViewModel : ReactiveObject
    {
        private static readonly HttpClient HttpClient = new HttpClient(); 
        private readonly string _textureStringFormat;

        public CardViewModel(string textureStringFormat)
        {
            _textureStringFormat = textureStringFormat;
            LoadTexture = ReactiveCommand.CreateFromTask(LoadImage);
            LoadTexture.Subscribe(image =>
            {
                Texture = image;
            });
            LoadTexture.ThrownExceptions.Subscribe(e =>
            {
                Log.Error(e, "Error loading bitmap for {url}", TextureUrl);
            });
        }

        private string TextureUrl => string.Format(_textureStringFormat, Index);
        private async Task<IBitmap> LoadImage(CancellationToken cancellationToken)
        {
            try
            {
                var response = await HttpClient.GetAsync(TextureUrl, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    return new Bitmap(stream);
                }
                
                // TODO: load local error bitmap
                return null;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error loading bitmap for {url}", TextureUrl);
                throw;
            }
        }

        public ReactiveCommand<Unit, IBitmap> LoadTexture { get; }
        public int Index { get; set; }
        public string Name { get; set; }
        public string Flavor { get; set; }
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
        public string Region => string.Join(", ", Locations.Select(l => l.Type == "Base Deck" ? l.Type : l.Region).Distinct());
        public IEnumerable<string> DetailedLocations => Locations.Select(l => l.Type == "Base Deck" || l.Region == "Random" ? "" : $"{l.Npc}, {l.Area}, {l.Territory ?? l.Region}").Where(l => l != "");
        public CombatDetails Combat { get; set; }
        public Location[] Locations { get; set; }
    }
}
