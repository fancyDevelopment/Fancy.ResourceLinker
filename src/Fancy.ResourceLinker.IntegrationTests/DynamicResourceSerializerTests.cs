using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using Fancy.ResourceLinker.Json;
using Fancy.ResourceLinker.Models;
using System;

namespace Fancy.ResourceLinker.IntegrationTests
{
    /// <summary>
    /// Test class to test serialization and deserializsation using the resource converter. 
    /// </summary>
    [TestClass]
    public class DynamicResourceSerializerTests
    {
        /// <summary>
        /// A complex object used within the tests.
        /// </summary>
        const string TEST_DATA = @"
        {
            ""intProperty"": 5,
            ""stringProperty"": ""foobar"",
            ""boolProperty"": true,
            ""objProperty"": { ""subObjProperty"": ""subObjFoobar"" },
            ""nullProperty"": null,
            ""arrayProperty"": [ 5, ""foo"", { ""objInArrProperty"": ""fooInArray"" }, [ ""subarray"", 6, true ] ]
        }";

        /// <summary>
        /// Tests the deserialization of the complex object to a dynamic resource.
        /// </summary>
        [TestMethod]
        public void DeserializeAndSerializeComplexObject()
        {
            JsonSerializerOptions serializerOptions = new JsonSerializerOptions();
            serializerOptions.AddResourceConverter(false);

            dynamic? deserializedObject = JsonSerializer.Deserialize<DynamicResource>(TEST_DATA, serializerOptions);

            Assert.IsNotNull(deserializedObject);
            Assert.AreEqual(5, deserializedObject.IntProperty);
            Assert.AreEqual("foobar", deserializedObject.StringProperty);
            Assert.AreEqual(true, deserializedObject.BoolProperty);
            Assert.AreEqual("subObjFoobar", deserializedObject.ObjProperty.SubObjProperty);
            Assert.AreEqual(null, deserializedObject.NullProperty);
            Assert.AreEqual(5, deserializedObject.ArrayProperty[0]);
            Assert.AreEqual("foo", deserializedObject.ArrayProperty[1]);
            Assert.AreEqual("fooInArray", deserializedObject.ArrayProperty[2].ObjInArrProperty);
            Assert.AreEqual("subarray", deserializedObject.ArrayProperty[3][0]);
            Assert.AreEqual(6 , deserializedObject.ArrayProperty[3][1]);
            Assert.AreEqual(true , deserializedObject.ArrayProperty[3][2]);

            string serializedObject = JsonSerializer.Serialize(deserializedObject, serializerOptions);

            Assert.AreEqual(TEST_DATA.Replace(Environment.NewLine, "").Replace(" ", ""), serializedObject);
        }
    }
}