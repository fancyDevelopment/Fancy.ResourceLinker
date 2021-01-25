using Fancy.ResourceLinker.Models;
using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fancy.ResourceLinker.Json
{
    /// <summary>
    /// A factory for the generic <see cref="ResourceJsonConverter{T}"/> to create concrete typed convertes.
    /// </summary>
    /// <seealso cref="System.Text.Json.Serialization.JsonConverterFactory" />
    public class ResourceJsonConverterFactory : JsonConverterFactory
    {
        /// <summary>
        /// The write privates.
        /// </summary>
        private readonly bool _writePrivates;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceJsonConverterFactory"/> class.
        /// </summary>
        /// <param name="writePrivates">if set to <c>true</c> the convertes write private json fields.</param>
        public ResourceJsonConverterFactory(bool writePrivates)
        {
            _writePrivates = writePrivates;
        }

        /// <summary>
        /// Determines whether the converter instance can convert the specified object type.
        /// </summary>
        /// <param name="typeToConvert">The type of the object to check whether it can be converted by this converter instance.</param>
        /// <returns>
        ///   <see langword="true" /> if the instance can convert the specified object type; otherwise, <see langword="false" />.
        /// </returns>
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsSubclassOf(typeof(ResourceBase));
        }

        /// <summary>
        /// Creates a converter for a specified type.
        /// </summary>
        /// <param name="typeToConvert">The type handled by the converter.</param>
        /// <param name="options">The serialization options to use.</param>
        /// <returns>
        /// A converter for which <typeparamref name="T" /> is compatible with <paramref name="typeToConvert" />.
        /// </returns>
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                typeof(ResourceJsonConverter<>).MakeGenericType(new Type[] { typeToConvert }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { _writePrivates },
                culture: null);

            return converter;
        }
    }
}