using Fancy.ResourceLinker.Models.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fancy.ResourceLinker.Models.ITest;

/// <summary>
/// A object used to test the serialization.
/// </summary>
/// <seealso cref="Fancy.ResourceLinker.Models.ResourceBase" />
class TestObject : DynamicResourceBase
{
    public int IntProperty { get; set; } = 5;
    public string StringProperty { get; set; } = "foobar";

    [JsonIgnore]
    public string IgnoredStaticProperty { get; set; } = "foo";
}

class NonPubCtorTestObject : DynamicResourceBase
{
    private NonPubCtorTestObject() { }
    public int IntProperty { get; set; } = 5;
    public string StringProperty { get; set; } = "foobar";

    [JsonIgnore]
    public string IgnoredStaticProperty { get; set; } = "foo";
}

/// <summary>
/// Test class to test serialization and deserializsation using the resource converter. 
/// </summary>
[TestClass]
public class ResourceConverterTests
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
        serializerOptions.AddResourceConverter();

        dynamic deserializedObject = JsonSerializer.Deserialize<DynamicResource>(TEST_DATA, serializerOptions)!;

        Assert.IsNotNull(deserializedObject);
        Assert.AreEqual(5, deserializedObject.IntProperty);
        Assert.AreEqual("foobar", deserializedObject.StringProperty);
        Assert.AreEqual(true, deserializedObject.BoolProperty);
        Assert.AreEqual("subObjFoobar", deserializedObject.ObjProperty.SubObjProperty);
        Assert.IsNull(deserializedObject.NullProperty);
        Assert.AreEqual(5, deserializedObject.ArrayProperty[0]);
        Assert.AreEqual("foo", deserializedObject.ArrayProperty[1]);
        Assert.AreEqual("fooInArray", deserializedObject.ArrayProperty[2].ObjInArrProperty);
        Assert.AreEqual("subarray", deserializedObject.ArrayProperty[3][0]);
        Assert.AreEqual(6, deserializedObject.ArrayProperty[3][1]);
        Assert.AreEqual(true, deserializedObject.ArrayProperty[3][2]);

        string serializedObject = JsonSerializer.Serialize(deserializedObject, serializerOptions);

        Assert.AreEqual(TEST_DATA.Replace(Environment.NewLine, "").Replace(" ", ""), serializedObject);
    }

    /// <summary>
    /// Tests the deserialization of the complex object to a dynamic resource.
    /// </summary>
    [TestMethod]
    public void DeserializeIntoObjectWithNonPubCtor()
    {
        JsonSerializerOptions serializerOptions = new JsonSerializerOptions();
        serializerOptions.AddResourceConverter(false, false);

        dynamic deserializedObject = JsonSerializer.Deserialize<NonPubCtorTestObject>(TEST_DATA, serializerOptions)!;

        Assert.IsNotNull(deserializedObject);
        Assert.AreEqual(5, deserializedObject.IntProperty);
        Assert.AreEqual("foobar", deserializedObject.StringProperty);
        Assert.AreEqual(true, deserializedObject.BoolProperty);
        Assert.AreEqual("subObjFoobar", deserializedObject.ObjProperty.SubObjProperty);
        Assert.IsNull(deserializedObject.NullProperty);
        Assert.AreEqual(5, deserializedObject.ArrayProperty[0]);
        Assert.AreEqual("foo", deserializedObject.ArrayProperty[1]);
        Assert.AreEqual("fooInArray", deserializedObject.ArrayProperty[2].ObjInArrProperty);
        Assert.AreEqual("subarray", deserializedObject.ArrayProperty[3][0]);
        Assert.AreEqual(6, deserializedObject.ArrayProperty[3][1]);
        Assert.AreEqual(true, deserializedObject.ArrayProperty[3][2]);
    }

    [TestMethod]
    public void SerializeComplexObjectWithoutEmptyMetadata_InObjectWithNoMetadata()
    {
        TestObject data = new TestObject();

        JsonSerializerOptions serializerOptions = new JsonSerializerOptions();
        serializerOptions.AddResourceConverter(true, true);

        string serializedObject = JsonSerializer.Serialize(data, serializerOptions);

        JsonDocument document = JsonDocument.Parse(serializedObject);

        Assert.AreEqual(5, document.RootElement.GetProperty("intProperty").GetInt32());
        Assert.AreEqual("foobar", document.RootElement.GetProperty("stringProperty").GetString());
        Assert.IsFalse(document.RootElement.TryGetProperty("_links", out var _linksPorperty));
        Assert.IsFalse(document.RootElement.TryGetProperty("_actions", out var _actionsProperty));
        Assert.IsFalse(document.RootElement.TryGetProperty("_sockets", out var _socketsPorperty));
    }

    [TestMethod]
    public void SerializeComplexObjectWithoutEmptyMetadata_InObjectWithMetadata()
    {
        TestObject data = new TestObject();
        data.AddLink("self", "http://my.domain/api/my/object");

        JsonSerializerOptions serializerOptions = new JsonSerializerOptions();
        serializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        serializerOptions.AddResourceConverter(true, true);

        string serializedObject = JsonSerializer.Serialize(data, serializerOptions);

        JsonDocument document = JsonDocument.Parse(serializedObject);

        Assert.AreEqual(5, document.RootElement.GetProperty("intProperty").GetInt32());
        Assert.AreEqual("foobar", document.RootElement.GetProperty("stringProperty").GetString());
        Assert.IsTrue(document.RootElement.TryGetProperty("_links", out var _linksPorperty));
        Assert.AreEqual(_linksPorperty.GetProperty("self").GetProperty("href").GetString(), "http://my.domain/api/my/object");
        Assert.IsFalse(document.RootElement.TryGetProperty("_actions", out var _actionsProperty));
        Assert.IsFalse(document.RootElement.TryGetProperty("_sockets", out var _socketsPorperty));
    }

    [TestMethod]
    public void SerializeComplexObjectWithoutIgnoredProperties()
    {
        TestObject data = new TestObject();

        JsonSerializerOptions serializerOptions = new JsonSerializerOptions();
        serializerOptions.AddResourceConverter(true, true);

        string serializedObject = JsonSerializer.Serialize(data, serializerOptions);

        JsonDocument document = JsonDocument.Parse(serializedObject);

        JsonElement result;
        Assert.IsFalse(document.RootElement.TryGetProperty("ignoredStaticProperty", out result));
    }
}