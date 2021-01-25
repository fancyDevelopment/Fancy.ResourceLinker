using System.Text.Json;

namespace Fancy.ResourceLinker.Json
{
    /// <summary>
    /// Extensions for JsonSerializerOptions
    /// </summary>
    public static class JsonSerializerOptionsExtensions
    {
        /// <summary>
        /// Adds the resource converter an instance of JsonSerializerOptions.
        /// </summary>
        /// <param name="options">The options.</param>
        public static void AddResourceConverter(this JsonSerializerOptions options, bool writePrivates = true)
        {
            options.Converters.Add(new ResourceJsonConverterFactory(writePrivates));
        }
    }
}
