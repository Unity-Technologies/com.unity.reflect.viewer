using System.Collections.Generic;
using NUnit.Framework;
using Unity.Properties;
using Unity.Serialization.Json.Adapters;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.Serialization.Json.Tests
{
    partial class JsonSerializationTests
    {
        internal class Migration
        {
            [GeneratePropertyBag]
            internal struct TestData
            {
                public int Int32Field;
                public NestedStruct NestedStruct;
            }

            [GeneratePropertyBag]
            internal struct NestedStruct
            {
                public float Float32Field;
                public string StringField;
            }

            class TriggerTestMigration : IJsonMigration<TestData>
            {
                readonly int m_CurrentVersion;

                public TriggerTestMigration(int version)
                {
                    m_CurrentVersion = version;
                    SerializedVersion = version;
                }

                public bool DidTrigger { get; private set; }

                int IJsonMigration<TestData>.Version => m_CurrentVersion;
                public int SerializedVersion { get; private set; }

                TestData IJsonMigration<TestData>.Migrate(JsonMigrationContext context)
                {
                    SerializedVersion = context.SerializedVersion;
                    DidTrigger = true;
                    return default;
                }
            }

            class RootTransferMigration : IJsonMigration<TestData>
            {
                int IJsonMigration<TestData>.Version => 1;

                TestData IJsonMigration<TestData>.Migrate(JsonMigrationContext context)
                {
                    // Transfer the default data
                    var value = context.Read<TestData>(context.SerializedObject);

                    if (context.SerializedVersion == 0)
                    {
                        value.NestedStruct.StringField = "Version1Default";
                    }

                    return value;
                }
            }

            [Test]
            public void ToJson_StructWithUserDefinedMigration_VersionIsCorrectlyWritten()
            {
                var json = JsonSerialization.ToJson(new TestData(), new JsonSerializationParameters
                {
                    UserDefinedMigrations = new List<IJsonMigration>
                    {
                        new TriggerTestMigration(1)
                    }
                });

                Assert.That(json, Contains.Substring("\"$version\": 1"));
            }

            [Test]
            public void ToJson_StructWithGlobalMigration_VersionIsCorrectlyWritten()
            {
                var migration = new TriggerTestMigration(1);

                JsonSerialization.AddGlobalMigration(migration);

                try
                {
                    var json = JsonSerialization.ToJson(new TestData());
                    Assert.That(json, Contains.Substring("\"$version\": 1"));
                }
                finally
                {
                    JsonSerialization.RemoveGlobalMigration(migration);
                }
            }

            [Test]
            [TestCase(@"{}", true, 1, 0)]
            [TestCase(@"{""$version"": 1}", true, 2, 1)]
            [TestCase(@"{""$version"": 1}", true, 3, 1)]
            [TestCase(@"{""$version"": 2}", false, 2, 2)]
            public void FromJson_StructWithUserDefinedMigration_MigrationIsCorrectlyTriggered(string json, bool shouldTrigger, int currentVersion, int expectedSerializedVersion)
            {
                var migration = new TriggerTestMigration(currentVersion);

                JsonSerialization.FromJson<TestData>(json, new JsonSerializationParameters
                {
                    UserDefinedMigrations = new List<IJsonMigration>
                    {
                        migration
                    }
                });

                Assert.That(migration.DidTrigger, Is.EqualTo(shouldTrigger));
                Assert.That(migration.SerializedVersion, Is.EqualTo(expectedSerializedVersion));
            }

            [Test]
            public void ToJson_JsonMigrationWithInvalidVersion_LogsError()
            {
                var migration = new TriggerTestMigration(0);

                LogAssert.Expect(LogType.Error,
                                 $"An error occured while serializing Type=[{typeof(TestData)}] using IJsonMigration=[{typeof(TriggerTestMigration)}]. Serialized version must be greater than 0.");

                JsonSerialization.ToJson(new TestData(), new JsonSerializationParameters
                {
                    UserDefinedMigrations = new List<IJsonMigration>
                    {
                        migration
                    }
                });
            }

            [Test]
            public void FromJson_JsonMigrationWithRootTransfer_ValueIsCorrectlyDeserialized()
            {
                // Serialize the data with no version number, so we can trigger migration on deserialization.
                var json = JsonSerialization.ToJson(new TestData
                {
                    Int32Field = 42,
                    NestedStruct = new NestedStruct
                    {
                        Float32Field = 1.23f
                    }
                });

                // During deserialization we will trigger a migration from version 0 to 1.
                var data = JsonSerialization.FromJson<TestData>(json, new JsonSerializationParameters
                {
                    UserDefinedMigrations = new List<IJsonMigration>
                    {
                        new RootTransferMigration()
                    }
                });

                Assert.That(data.Int32Field, Is.EqualTo(42));
                Assert.That(data.NestedStruct.Float32Field, Is.EqualTo(1.23f));
                Assert.That(data.NestedStruct.StringField, Is.EqualTo("Version1Default"));
            }

            /*
            /// <summary>
            /// [VERSION 0] - We just have an x and y position.
            /// </summary>
            class Player
            {
                public int x;
                public int y;
            }

            /// <summary>
            /// [VERSION 1] - We decided to add name and put x + y in to a position structure.
            /// </summary>
            class Player
            {
                public string Name;
                public Position Position;
            }
            */

            /// <summary>
            /// [VERSION 2] (current) - We decided to re-work the position field in to a nested transform struct. We also added health.
            /// </summary>
            [GeneratePropertyBag]
            internal class Player
            {
                public string Name;
                public int Health;
                public TransformData TransformData;
            }

            [GeneratePropertyBag]
            internal class TransformData
            {
                public Position Position;
            }

            [GeneratePropertyBag]
            internal struct Position
            {
                public int X;
                public int Y;
            }

            class PlayerMigration : IJsonMigration<Player>
            {
                /// <summary>
                /// Returns the currently serialized version for this type. Note this is by the serializer to write the version
                /// and by the deserializer to determine if migration should take place.
                /// </summary>
                int IJsonMigration<Player>.Version => 2;

                Player IJsonMigration<Player>.Migrate(JsonMigrationContext context)
                {
                    var serializedVersion = context.SerializedVersion;
                    var serializedObject = context.SerializedObject;

                    var value = new Player();

                    if (serializedVersion == 0)
                    {
                        // In version 0 we only the x and y fields. They are now nested in the transform.position classes.
                        // We didn't have name or health so can assign some default here.
                        value.Name = "DefaultName";
                        value.Health = 100;
                        value.TransformData = new TransformData
                        {
                            Position = new Position
                            {
                                X = serializedObject["X"].AsInt32(),
                                Y = serializedObject["Y"].AsInt32()
                            }
                        };
                    }

                    if (serializedVersion == 1)
                    {
                        // In version 1 we had the position and name field directly on the player.
                        value.Name = context.Read<string>(serializedObject["Name"]);
                        value.Health = 100;
                        value.TransformData = new TransformData
                        {
                            Position = context.Read<Position>(serializedObject["Position"])
                        };
                    }

                    return value;
                }
            }

            [Test]
            public void FromJson_ClassWithMultipleMigrationVersions_ValueIsCorrectlyDeserialized()
            {
                var parameters = new JsonSerializationParameters
                {
                    UserDefinedMigrations = new List<IJsonMigration> {new PlayerMigration()}
                };
                
                // Deserialize from version 0 -> 2
                {
                    const string json_v0 = @"{""X"": 1, ""Y"": 2}";

                    var data = JsonSerialization.FromJson<Player>(json_v0, parameters);

                    Assert.That(data, Is.Not.Null);
                    Assert.That(data.Name, Is.EqualTo("DefaultName"));
                    Assert.That(data.Health, Is.EqualTo(100));
                    Assert.That(data.TransformData, Is.Not.Null);
                    Assert.That(data.TransformData.Position, Is.Not.Null);
                    Assert.That(data.TransformData.Position.X, Is.EqualTo(1));
                    Assert.That(data.TransformData.Position.Y, Is.EqualTo(2));
                }
                
                // Deserialize from version 1 -> 2
                {
                    const string json_v1 = @"{""$version"": 1, ""Name"": ""Bob"", ""Position"": {""X"": 5, ""Y"": 12}}";

                    var data = JsonSerialization.FromJson<Player>(json_v1, parameters);

                    Assert.That(data, Is.Not.Null);
                    Assert.That(data.Name, Is.EqualTo("Bob"));
                    Assert.That(data.Health, Is.EqualTo(100));
                    Assert.That(data.TransformData, Is.Not.Null);
                    Assert.That(data.TransformData.Position, Is.Not.Null);
                    Assert.That(data.TransformData.Position.X, Is.EqualTo(5));
                    Assert.That(data.TransformData.Position.Y, Is.EqualTo(12));
                }
            }

#pragma warning disable 649
            internal interface IBase
            {
                
            }
            
            [GeneratePropertyBag]
            internal class ConcreteA : IBase
            {
                public int value;
                public int a;
            }

            [GeneratePropertyBag]
            internal class ConcreteB : IBase
            {
                public int value;
                public int b;
            }
#pragma warning restore 649
            
            [GeneratePropertyBag]
            internal class ClassWithInterface
            {
                public IBase Value;
            }
            
            class InterfaceMigration : Adapters.Contravariant.IJsonMigration<IBase>
            {
                public int Version => 1;
                
                public object Migrate(JsonMigrationContext context)
                {
                    var readAsInterface = context.Read<IBase>(context.SerializedObject);
                    Assert.That(readAsInterface, Is.InstanceOf(typeof(ConcreteA)));
                    
                    var readAsConcreteA = context.Read<ConcreteA>(context.SerializedObject);
                    Assert.That(readAsConcreteA, Is.InstanceOf(typeof(ConcreteA)));
                        
                    var readAsConcreteB = context.Read<ConcreteB>(context.SerializedObject);
                    Assert.That(readAsConcreteB, Is.InstanceOf(typeof(ConcreteB)));
                    
                    return null;
                }
            }
            
            [Test]
            public void JsonMigrationContext_ReadAbstractType_ReturnsCorrectInstanceType()
            {
                var json = JsonSerialization.ToJson(new ClassWithInterface
                {
                    Value = new ConcreteA { value = 42, a = 15 }
                });
                
                var parameters = new JsonSerializationParameters
                {
                    UserDefinedMigrations = new List<IJsonMigration> {new InterfaceMigration()}
                };

                JsonSerialization.FromJson<ClassWithInterface>(json, parameters);
            }
        }
    }
}