﻿using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fancy.ResourceLinker.Models.Json;

/// <summary>
/// A converter to help converting resource objects correctly into json.
/// </summary>
/// <typeparam name="T">The concrete type of the resource to convert.</typeparam>
/// <seealso cref="System.Text.Json.Serialization.JsonConverter{T}" />
public class ResourceJsonConverter<T> : JsonConverter<T> where T : class, IResource
{
    /// <summary>
    /// The a flag to indicate weather the converter shall write json private fields.
    /// </summary>
    private readonly bool _writePrivates;

    /// <summary>
    /// A flag to indicate if empty metadata fields shall be ignored.
    /// </summary>
    private readonly bool _ignoreEmptyMetadata;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceJsonConverter{T}" /> class.
    /// </summary>
    /// <param name="writePrivates">if set to <c>true</c> the converter reads and writes private fields.</param>
    /// <param name="ignoreEmptyMetadata">if set to <c>true</c> ignores empty metadata fields.</param>
    public ResourceJsonConverter(bool writePrivates, bool ignoreEmptyMetadata)
    {
        _writePrivates = writePrivates;
        _ignoreEmptyMetadata = ignoreEmptyMetadata;
    }

    /// <summary>
    /// Reads and converts the JSON to type <typeparamref name="T" />.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    /// <returns>
    /// The converted value.
    /// </returns>
    /// <exception cref="JsonException">
    /// Dictionary must be JSON object.
    /// or
    /// Incomplete JSON object
    /// or
    /// Incomplete JSON object
    /// </exception>
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;

        if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException("A resource must always be a JSON object.");

        // Create the default result.
        T? result = Activator.CreateInstance(typeof(T), true) as T;

        if (result == null)
        {
            throw new JsonException("Unable to create a new instance of " + typeof(T).Name + ". Make sure your class has at least a private parameterless constructor");
        }

        while (true)
        {
            if (!reader.Read()) throw new JsonException("Incomplete or broken JSON object!");

            if (reader.TokenType == JsonTokenType.EndObject) break;

            // Read the next key
            var key = reader.GetString()!;

            // If key is private field remove underscore
            key = key[0] == '_' ? key.Substring(1) : key;

            // Adjust case of first character to .NET standards
            key = char.ToUpper(key[0]) + key.Substring(1);

            if (!reader.Read()) throw new JsonException("Incomplete or broken JSON object!");

            object? value;

            if (result.StaticKeys.Contains(key))
            {
                // Read a static key
                value = JsonSerializer.Deserialize(ref reader, result.GetType().GetProperty(key)!.PropertyType, options);
            }
            else
            {
                // Read a dynamic key
                value = ReadDynamicValue(ref reader, options);
            }
             
            result[key] = value;
        }

        if(!_writePrivates)
        {
            result.ClearMetadata();
        }

        return result;
    }

    /// <summary>
    /// Writes a specified value as JSON.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The value to convert to JSON.</param>
    /// <param name="options">An object that specifies serialization options to use.</param>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            // Nothing to do
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStartObject();

            // Step through each key and add it to json
            foreach (string key in value.Keys)
            {
                // If the current attribute is a static attribute with json ignore, just continue
                PropertyInfo? staticPropertyInfo = value.GetType().GetProperty(key);
                if (staticPropertyInfo != null && Attribute.IsDefined(staticPropertyInfo, typeof(JsonIgnoreAttribute))) continue;

                string jsonKey = char.ToLower(key[0]) + key.Substring(1);
                if (key == "Links" || key == "Actions" || key == "Sockets")
                {
                    jsonKey = "_" + jsonKey;

                    if (_ignoreEmptyMetadata)
                    {
                        IDictionary? metadataDictionary = value[key] as IDictionary;
                        if (metadataDictionary != null && metadataDictionary.Count == 0)
                        {
                            continue;
                        }
                    }
                }

                if (key.StartsWith("_") && !_writePrivates) continue;

                writer.WritePropertyName(jsonKey);
                JsonSerializer.Serialize(writer, value[key], options);
            }

            writer.WriteEndObject();
        }
    }

    /// <summary>
    /// Reads a dynamic value from the json reader. If it finds an object or an array the 
    /// methods proceeds reading in a recursive manner. 
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="options">The options.</param>
    /// <returns>The value.</returns>
    private object? ReadDynamicValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        object? value = null;

        if (reader.TokenType == JsonTokenType.Number)
        {
            value = reader.GetDouble();
        }
        else if (reader.TokenType == JsonTokenType.String)
        {
            value = reader.GetString();
        }
        else if (reader.TokenType == JsonTokenType.False)
        {
            value = false;
        }
        else if (reader.TokenType == JsonTokenType.True)
        {
            value = true;
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            value = JsonSerializer.Deserialize<DynamicResource>(ref reader, options);
        }
        else if(reader.TokenType == JsonTokenType.StartArray)
        {
            IList<object?> arrayValues = new List<object?>();
            while(reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray) break;
                arrayValues.Add(ReadDynamicValue(ref reader, options));
            }
            value = arrayValues;
        } else if(reader.TokenType == JsonTokenType.Null)
        {
            value = null;
        }

        return value;
    }
}