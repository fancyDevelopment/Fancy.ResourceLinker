﻿using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Fancy.ResourceLinker.Models;

/// <summary>
/// Base class of a resource which can be linked to other resources.
/// </summary>
public abstract class DynamicResourceBase : DynamicObject, IEnumerable<KeyValuePair<string, object?>>, IResource
{
    /// <summary>
    /// The static keys.
    /// </summary>
    private ICollection<string> _staticKeys;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceBase"/> class.
    /// </summary>
    public DynamicResourceBase()
    {
        // Initialize metadata dictionaries
        Links = new Dictionary<string, ResourceLink>();
        Actions = new Dictionary<string, ResourceAction>();
        Sockets = new Dictionary<string, ResourceSocket>();

        // Get all static (compile time) properties of this type
        _staticKeys = GetType()
                     .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                     .Where(p => p.GetIndexParameters().Length == 0)
                     .Select(p => p.Name)
                     .ToList();

        // Remove the control keys of the dictionary interface
        _staticKeys.Remove("Keys");
        _staticKeys.Remove("Values");
        _staticKeys.Remove("Count");
        _staticKeys.Remove("IsReadOnly");
        _staticKeys.Remove("Item");
    }

    /// <summary>
    /// Gets or sets the links of this resource.
    /// </summary>
    /// <value>
    /// The links.
    /// </value>
    [JsonPropertyName("_links")]
    [NotMapped]
    public Dictionary<string, ResourceLink> Links { get; internal set; }

    /// <summary>
    /// Gets or sets the actions of this resource.
    /// </summary>
    /// <value>
    /// The actions.
    /// </value>
    [JsonPropertyName("_actions")]
    [NotMapped]
    public Dictionary<string, ResourceAction> Actions { get; internal set; }

    /// <summary>
    /// Gets the sockets.
    /// </summary>
    /// <value>
    /// The sockets.
    /// </value>
    [JsonPropertyName("_sockets")]
    [NotMapped]
    public Dictionary<string, ResourceSocket> Sockets { get; internal set; }

    /// <summary>
    /// Gets a collection containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
    /// </summary>
    public ICollection<string> Keys
    {
        get
        {
            List<string> keys = new List<string>(StaticKeys);
            keys.AddRange(DynamicProperties.Keys);
            return keys;
        }
    }

    /// <summary>
    /// Gets a collection containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
    /// </summary>
    public ICollection<object?> Values
    {
        get
        {
            List<object?> values = new List<object?>();

            foreach (string key in StaticKeys)
            {
                values.Add(GetType().GetProperty(key)!.GetValue(this));
            }

            values.AddRange(DynamicProperties.Values);

            return values;
        }
    }

    /// <summary>
    /// Gets the number of elements contained in the collection.
    /// </summary>
    public int Count => StaticKeys.Count + DynamicProperties.Keys.Count;

    /// <summary>
    /// Gets the dynamic properties.
    /// </summary>
    /// <value>
    /// The dynamic properties.
    /// </value>
    internal Dictionary<string, object?> DynamicProperties { get; } = new Dictionary<string, object?>();

    /// <summary>
    /// Gets the static keys.
    /// </summary>
    /// <value>
    /// The static keys.
    /// </value>
    ICollection<string> IResource.StaticKeys { get => _staticKeys; }

    // The couterpart to the explicit interface implementation
    private ICollection<string> StaticKeys { get => _staticKeys; }

    /// <summary>
    /// Gets or sets the <see cref="System.Object"/> with the specified key.
    /// </summary>
    /// <value>
    /// The <see cref="System.Object"/>.
    /// </value>
    /// <param name="key">The key.</param>
    /// <returns>The object.</returns>
    public object? this[string key]
    {
        get
        {
            if (StaticKeys.Contains(key))
            {
                return GetType().GetProperty(key)!.GetValue(this);
            }
            else
            {
                return DynamicProperties[key];
            }
        }
        set
        {
            if (StaticKeys.Contains(key))
            {
                GetType().GetProperty(key)!.SetValue(this, value);
            }
            else
            {
                DynamicProperties[key] = value;
            }
        }
    }

    /// <summary>
    /// Gets a specific key and tries to convert the value to an integer.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The value converted to integer.</returns>
    public int GetAsInt(string key)
    {
        object? value = this[key];

        if (value != null) return Convert.ToInt32(value);
        else throw new KeyNotFoundException($"The specified key '{key}' does not exist on this instance!");
    }

    /// <summary>
    /// Gets a specific key and tries to convert the value to a double.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The value converted to double.</returns>
    public double GetAsDouble(string key)
    {
        object? value = this[key];

        if (value != null) return Convert.ToDouble(value);
        else throw new KeyNotFoundException($"The specified key '{key}' does not exist on this instance.");
    }

    /// <summary>
    /// Gets a specific key and tries to convert the value to a double.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The value converted to double.</returns>
    public string GetAsString(string key)
    {
        object? value = this[key];

        if (value != null) return Convert.ToString(value) ?? throw new InvalidOperationException($"Cannot convert value of key '{key}' to string.");
        else throw new KeyNotFoundException($"The specified key '{key}' does not exist on this instance.");
    }

    /// <summary>
    /// Gets a specific key and tries to cast the value to a list of dynamic resources.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The value casted to a list of dynamic resources.</returns>
    public DynamicResource GetAsResource(string key)
    {
        object? value = this[key];

        if (value != null) return (DynamicResource)value;
        else throw new KeyNotFoundException($"The specified key '{key}' does not exist on this instance.");
    }

    /// <summary>
    /// Gets a specific key and tries to cast the value to a list of dynamic resources.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The value casted to a list of dynamic resources.</returns>
    public IEnumerable<DynamicResource> GetAsResourceList(string key)
    {
        object? value = this[key];

        if (value != null) return ((IEnumerable<object>)value).Select(e => (DynamicResource)e);
        else throw new KeyNotFoundException($"The specified key '{key}' does not exist on this instance!");
    }

    /// <summary>
    /// Adds a link.
    /// </summary>
    /// <param name="rel">The relation.</param>
    /// <param name="href">The href.</param>
    public void AddLink(string rel, string href)
    {
        Links[rel] = new ResourceLink(href);
    }

    /// <summary>
    /// Adds an action.
    /// </summary>
    /// <param name="rel">The relation.</param>
    /// <param name="method">The method.</param>
    /// <param name="href">The URL to the action.</param>
    public void AddAction(string rel, string method, string href)
    {
        Actions[rel] = new ResourceAction(method, href);
    }

    /// <summary>
    /// Adds a socket.
    /// </summary>
    /// <param name="rel">The relation.</param>
    /// <param name="href">The href.</param>
    /// <param name="method">The method.</param>
    /// <param name="token">The token.</param>
    public void AddSocket(string rel, string href, string method, string token)
    {
        Sockets[rel] = new ResourceSocket(href, method, token);
    }

    /// <summary>
    /// Adds a socket.
    /// </summary>
    /// <param name="rel">The relation.</param>
    /// <param name="href">The href.</param>
    /// <param name="method">The method.</param>
    public void AddSocket(string rel, string href, string method)
    {
        Sockets.Add(rel, new ResourceSocket(href, method));
    }

    /// <summary>
    /// Removes the metadata of links, actions and sockets completely from this instance.
    /// </summary>
    public void ClearMetadata()
    {
        Links.Clear();
        Actions.Clear();
        Sockets.Clear();
    }

    /// <summary>
    /// Tries to get a dynamic member.
    /// </summary>
    /// <param name="binder">The binder.</param>
    /// <param name="result">The result.</param>
    /// <returns>true if the member could be retrieved; otherwise, false.</returns>
    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        if (Keys.Contains(binder.Name))
        {
            result = this[binder.Name];
            return true;
        }

        result = null;
        return false;
    }

    /// <summary>
    /// Tries to set a dynamic member.
    /// </summary>
    /// <param name="binder">The binder.</param>
    /// <param name="value">the value to set.</param>
    /// <returns>
    /// true if the member could be set; otherwise, false.
    /// </returns>
    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        this[binder.Name] = value;
        return true;
    }

    /// <summary>
    /// Returns the enumeration of all dynamic member names.
    /// </summary>
    /// <returns>
    /// A sequence that contains dynamic member names.
    /// </returns>
    public override IEnumerable<string> GetDynamicMemberNames()
    {
        return DynamicProperties.Keys;
    }

    /// <summary>
    /// Adds an element with the provided key and value to the collection.
    /// </summary>
    /// <param name="key">The object to use as the key of the element to add.</param>
    /// <param name="value">The object to use as the value of the element to add.</param>
    public void Add(string key, object value)
    {
        if (Keys.Contains(key))
        {
            throw new ArgumentException("An item with the same key has already been added. Key: " + key);
        }

        DynamicProperties.Add(key, value);
    }

    /// <summary>
    /// Determines whether the dictionary contains an element with the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <returns>
    /// true if the dictionary contains an element with the key; otherwise, false.
    /// </returns>
    public bool ContainsKey(string key)
    {
        return Keys.Contains(key);
    }

    /// <summary>
    /// Removes the element with the specified key from the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns>
    /// true if the element is successfully removed; otherwise, false.  This method also returns false if 
    /// <paramref name="key">key</paramref> was not found in the dynamic properties. It throws an exception if 
    /// you try to remove a static key.
    /// </returns>
    /// <exception cref="System.ArgumentException">Static keys can not be removed. Key: " + key</exception>
    public bool Remove(string key)
    {
        if (StaticKeys.Contains(key))
        {
            throw new ArgumentException("Static keys can not be removed. Key: " + key);
        }

        return DynamicProperties.Remove(key);
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose value to get.</param>
    /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise,
    /// the default value for the type of the value parameter.</param>
    /// <returns>
    /// true if the object contains an element with the specified key; otherwise, false.
    /// </returns>
    public bool TryGetValue(string key, out object? value)
    {
        if (Keys.Contains(key))
        {
            value = this[key];
            return true;
        }
        else
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// An enumerator that can be used to iterate through the collection.
    /// </returns>
    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        return new DynamicResourceEnumerator(this);
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    /// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}