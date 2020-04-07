namespace GwentTracker
{
    public class TextureInfo
    {
        public string RemotePathFormat { get; }
        public string LocalPathFormat { get; }
        public bool CacheRemote { get; }

        public TextureInfo(string remotePathFormat, string localPathFormat, bool cacheRemote)
        {
            RemotePathFormat = remotePathFormat;
            LocalPathFormat = localPathFormat;
            CacheRemote = cacheRemote;
        }
    }
}