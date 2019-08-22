using Newtonsoft.Json.Serialization;
using System;
using System.Linq;

namespace Fancy.ResourceLinker
{
    /// <summary>
    /// A json contract resolver to properly serialize resource objects.
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver" />
    public class ResourceContractResolver : CamelCasePropertyNamesContractResolver
    {
        /// <summary>
        /// Creates a <see cref="T:Newtonsoft.Json.Serialization.JsonDynamicContract" /> for the given type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// A <see cref="T:Newtonsoft.Json.Serialization.JsonDynamicContract" /> for the given type.
        /// </returns>
        protected override JsonDynamicContract CreateDynamicContract(Type objectType)
        {
            JsonDynamicContract dynamicContract = base.CreateDynamicContract(objectType);
            dynamicContract.Properties.ToList().ForEach(p =>
            {
                p.HasMemberAttribute = true;
                p.PropertyName = (p.PropertyName[0].ToString().ToLowerInvariant() + p.PropertyName.Substring(1));
            });
            return dynamicContract;
        }
    }
}
