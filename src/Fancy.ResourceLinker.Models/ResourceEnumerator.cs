using System.Collections;

namespace Fancy.ResourceLinker.Models;

/// <summary>
/// An enumerator to enumerator through all keys of a <see cref="ResourceBase"/>.
/// </summary>
public class ResourceEnumerator : IEnumerator<KeyValuePair<string, object?>>
{
    /// <summary>
    /// The resource to enumerate.
    /// </summary>
    private ResourceBase _resource;

    /// <summary>
    /// The current element.
    /// </summary>
    private KeyValuePair<string, object?> _current;

    /// <summary>
    /// The current index.
    /// </summary>
    private int _currentIndex = -1;

    /// <summary>
    /// Gets the element in the collection at the current position of the enumerator.
    /// </summary>
    public KeyValuePair<string, object?> Current => _current;

    /// <summary>
    /// Gets the element in the collection at the current position of the enumerator.
    /// </summary>
    object IEnumerator.Current => _current;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceEnumerator"/> class.
    /// </summary>
    /// <param name="resorce">The resorce.</param>
    public ResourceEnumerator(ResourceBase resorce)
    {
        _resource = resorce;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
    }

    /// <summary>
    /// Advances the enumerator to the next element of the collection.
    /// </summary>
    /// <returns>
    /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
    /// </returns>
    public bool MoveNext()
    {
        _currentIndex++;

        if(_currentIndex < _resource.Count)
        {
            SetCurrentIndex();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Sets the enumerator to its initial position, which is before the first element in the collection.
    /// </summary>
    public void Reset()
    {
        _currentIndex = -1;
        SetCurrentIndex();
    }

    /// <summary>
    /// Sets key value pair of the current index to the current element.
    /// </summary>
    private void SetCurrentIndex()
    {
        string key = _resource.Keys.ToList()[_currentIndex];
        object? value = _resource[key];

        _current = new KeyValuePair<string, object?>(key, value);
    }
}