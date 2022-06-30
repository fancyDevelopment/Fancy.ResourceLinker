using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using Fancy.ResourceLinker.Json;
using Fancy.ResourceLinker.Models;
using System;

namespace Fancy.ResourceLinker.IntegrationTests
{
    [TestClass]
    public class DynamicResourceSerializerTests
    {

        const string TEST_DATA = @"
        {
            ""intProperty"": 5,
            ""stringProperty"": ""foobar"",
            ""arrayProperty"": [ 5, ""foo"", { ""objInArrProperty"": ""fooInArray"" } ]
        }";

        [TestMethod]
        public void DeserializeAndSerializeComplexObject()
        {
            JsonSerializerOptions serializerOptions = new JsonSerializerOptions();
            serializerOptions.AddResourceConverter(false);

            dynamic? deserializedObject = JsonSerializer.Deserialize<DynamicResource>(TEST_DATA, serializerOptions);

            Assert.IsNotNull(deserializedObject);

            string serializedObject = JsonSerializer.Serialize(deserializedObject, serializerOptions);

            Assert.AreEqual(TEST_DATA.Replace(Environment.NewLine, "").Replace(" ", ""), serializedObject);
        }
    }
}