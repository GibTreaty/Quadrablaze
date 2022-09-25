namespace YounGenTech.PoolGen {
    /// <summary>Helper interface for when a <see cref="PoolUser"/> is first added to an <see cref="ObjectPool"/> pool</summary>
    public interface IPoolInstantiate {
        void PoolInstantiate(PoolUser user);
    }
}