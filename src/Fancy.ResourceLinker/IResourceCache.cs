using System;
using System.Text;

namespace Fancy.ResourceLinker
{
    public interface IResourceCache
    {
        void Write<TResource>(string key, TResource data) where TResource : class;

        bool TryRead<TResource>(string key, TimeSpan maxDataAge, out TResource data) where TResource : class;
    }
}
