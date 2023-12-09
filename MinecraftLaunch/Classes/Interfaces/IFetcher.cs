namespace MinecraftLaunch.Classes.Interfaces {
    /// <summary>
    /// 统一搜寻器接口
    /// </summary>
    public interface IFetcher<T> {
        public T Fetch();

        public ValueTask<T> FetchAsync();
    }
}
