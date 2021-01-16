using Fancy.ResourceLinker.Models;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fancy.ResourceLinker.Json
{
    /// <summary>
    /// A converter to help converting resource objects correctly into json.
    /// </summary>
    /// <typeparam name="T">The concrete type of the resource to convert.</typeparam>
    /// <seealso cref="System.Text.Json.Serialization.JsonConverter{T}" />
    public class ResourceJsonConverter<T> : JsonConverter<T> where T: ResourceBase, new()
    {
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
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;

            if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException("A resource must always be a JSON object.");

            // Create the default result.
            var result = new T();
            result.RemoveMetadata();

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                if (!reader.Read()) throw new JsonException("Incomplete or broken JSON object!");

                // Read the next key
                var key = reader.GetString();

                // Do not deserialize private keys.
                if (key.StartsWith("_")) continue;

                if (!reader.Read()) throw new JsonException("Incomplete or broken JSON object!");

                object value;

                if (result.StaticKeys.Contains(key))
                {
                    // Read a static key
                    value = JsonSerializer.Deserialize(ref reader, result.GetType().GetProperty(key).PropertyType, options);
                }
                else
                {
                    // Read a dynamic key
                    if(reader.TokenType == JsonTokenType.Number)
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
                    else
                    {
                        value = JsonSerializer.Deserialize<JsonElement>(ref reader);
                    }
                }
                 
                result[key] = value;
            }

            return result;
        }

        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, T value,JsonSerializerOptions options)
        {
            if (value is null)
            {
                // Nothing to do
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStartObject();

                // Step through each key add it to json
                foreach (var pair in value)
                {
                    string key = pair.Key;
                    key = char.ToLower(key[0]) + key.Substring(1);
                    if (key == "links" || key == "actions" || key == "sockets") key = "_" + key;
                    writer.WritePropertyName(key);
                    JsonSerializer.Serialize(writer, pair.Value, options);
                }

                writer.WriteEndObject();
            }
        }
    }
}