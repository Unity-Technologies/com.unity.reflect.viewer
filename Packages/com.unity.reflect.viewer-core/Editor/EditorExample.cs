namespace Unity.XMLDocExample
{
    // -----------------------------------------------------------------------------
    //
    // Use this editor example C# file to develop editor (non-runtime) code.
    //
    // -----------------------------------------------------------------------------

    namespace UnityEditor.Reflect.Viewer.Core
    {
        /// <summary>
        /// Packages require documentation for ALL public Package APIs.
        ///
        /// The summary tags are where you put all basic descriptions.
        /// For example, this is where you would normally provide a general description of the class.
        ///
        /// Inside these tags, you can use normal markdown, such as **bold**, *italics*, and `code` formatting.
        /// </summary>
        /// <remarks>
        /// For more information on using the XML Documentation comments and the supported tags,
        /// see the [Microsoft documentation](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/xml-documentation-comments).
        /// </remarks>
        public class MyPublicEditorExampleClass
        {
            /// <summary>
            /// If you want to document your public enum, use `summary` tags before each value.
            /// </summary>
            public enum MyExampleEnum
            {
                /// <summary>
                /// Description of value 1
                /// </summary>
                First = 0,
                /// <summary>
                /// Description of value 2
                /// </summary>
                Second = 1,
                /// <summary>
                /// Description of value 3
                /// </summary>
                Third = 2,
            }

            /// <summary>
            /// For properties, you can add a description of the property to get/set with the `value` tag.
            /// </summary>
            /// <value> Description of the property </value>
            public MyExampleEnum propertyExample
            {
                get
                {
                    bool hidden = true;
                    bool enabled = true;
                    if (hidden)
                        return MyExampleEnum.First;
                    if (enabled)
                        return MyExampleEnum.Second;
                    return MyExampleEnum.Third;
                }
            }

            /// <summary>
            /// Besides providing a description of what this private method does in the summary tag,
            /// you should also describe each parameter using the `param` tag and document any return values
            /// with the `return` tag.
            /// </summary>
            /// <param name="parameter1"> Description of parameter 1 </param>
            /// <param name="parameter2"> Description of parameter 2 </param>
            /// <param name="parameter3"> Description of parameter 3 </param>
            /// <returns> Description of what the function returns </returns>
            public int CountThingsAndDoStuff(int parameter1, int parameter2, bool parameter3)
            {
                return parameter3 ? (parameter1 + parameter2) : (parameter1 - parameter2);
            }

        }
    }
}

