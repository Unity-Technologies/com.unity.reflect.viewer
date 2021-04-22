# Writing API documentation

* [Script reference](#api-script-reference)
  * [What to write](#what-to-write)
  * [Tags](#tags)
  * [Simple class example](#simple-class-example)
  * [Code examples](#code-examples)
  * [Links](#links)
  * [Formatting](#formatting)  
* [Manual](#manual)
* [Testing](#testing)
* [More resources](#more-resources)

## API script reference

We use **XMLDoc** which is parsed through [DocFX](https://dotnet.github.io/docfx/) to create and generate API script reference documentation. 

You can use [Markdown syntax](https://dotnet.github.io/docfx/spec/docfx_flavored_markdown.html?tabs=tabid-1%2Ctabid-a) inside the XML tags.

### What to write

Your goal is to be clear, concise, and accurate.

However, let the tech writer worry about the phrasing. Your main job is to identify the necessary information.
 
Our audience of developers want to know the following for each type and member:

* What is this thing?
* Why would I use it?
* When would I use it?
* How would I use it?
* What can go wrong?
* What other things does this relate to?

The summary for a type or member should have the following form:

* "Provides methods for..."
* "Manages the lifespan of entities..."
* "Represents a player..."
* "Calculates the optimal path between..."

Or for "passive" constructs that don't have methods (fields, data structs, properties):

* "A transform from local to world space..."
* "The parent entity in a hierarchy..."
* "Reports the state of..."
* "Indicates whether this object is active..."

(Avoid "gets" and "sets" for properties, since these are hidden by the C# language).

For further information on tone of voice, and styling, see the [Unity Docs Style Guide](https://unity-docs.gitbook.io/style-guide/style/scripting-api).

### Tags
#### Required XML tags

In your API documentation you must include the following XML tags:

* `<summary>` - A one-line description of the API. All APIs must have a summary. The summary shows up in intelligent code completion tools such as Intellisense.
* `<param name="thing1">` - Each function parameter requires this tag.
* `<typeparam name="thing2">` - Each generic type parameter requires this tag.
* `<returns>` - Non-void functions require this tag.
* `<value>` - Fields and properties require this tag.

**Note:** If any of the above required items are missing, there is blank space left in the documentation where that text should be.

You can configure Rider or any other IDEs to add the required tags in a `///` comment block for you.

#### Common optional XML tags
The following XML tags are optional and won't affect the output of the documentation if you choose to omit them. However, it's recommended that you should use them where possible.

* `<remarks>` - Any additional or in-depth information. All but the simplest APIs should have a remarks section.
* `<example>` - For code examples. The code itself goes inside a nested `<code>` element.
* `<see>` - Link to other API types in the same package. Use inline with other text.
* `<seealso>` - Like `<see>`, but also makes a list of see also links. Can be inline or top-level.

**Things to watch out for**

* Because the documentation is in XML, you can't use angle braces `<` `>`. To write the name of a generic function such as `Sum<T>`, use `&lt;` and `&gt;` in text, e.g. `Sum&lt;T&gt;`. In cref links, replace the angle brace with curly braces e.g `<see cref="Sum{T}"/>`.
* When you uses the Package Doc Tools (a package you can install) to generate the documentation to test your changes, always clear your browser cache.
* Most block-level elements don't get nested.

### Simple class example
This example shows you how to use XML tags to create documentation for a simple class.

```
/// <summary>
/// The Calculator class provides basic math calculations.
/// </summary>
/// <remarks>The math functions implemented by Calculator favor speed 
/// over accuracy.
/// Use <see cref="SlowCalculator"/> when you need accurate results.</remarks>
/// <seealso cref="SlowCalculator"/>
public class Calculator
{
    /// <summary>
    /// The storage register of this calculator.
    /// </summary>
    /// <value>Stores the intermediate results of the current calculation.</value>
    public int Register;

    /// <summary>
    /// Adds the operand to the Register.
    /// </summary>
    /// <param name="operand">The number to add to the current sum.</param>
    /// <typeparam name="T">A numeric type.</typeparam>
    /// <returns>The result so far.</returns>
    public T Sum(T operand)
    {
        Register += (int)(object)operand;
        return (T)(object)Register;
    }

    /// <summary>
    /// Gets the result of the current operation.
    /// </summary>
    /// <returns>The current Register value.</returns>
    public int Result()
    {
        return Register;
    }

    /// <summary>
    /// Clears the current calculation, setting Register to zero.
    /// </summary>
    public void Clear()
    {
        Register = 0;
    }
}
```

### Code examples

To include code examples, put them in an `<example>` element:

```
/// <example>
///     The following example shows how to sum:
///     <code>
///         var calc = new Calculator();
///         calc.Sum(73.23);
///         calc.Sum(21.3);
///         print(calc.Result());
///     </code>
/// </example>
```

### Links

**To APIs from XMLdoc**

To link to other APIs in the same package, use `<see cref="APIName">`. The cref attribute is the scoped name. In other words, to link to a class in the same namespace, you can just use the class name; and to link to a method in the current class, you can just use the method name.

There are several ways to reference items with parameter types and numbers. These can be useful to specify particular overloads for the link, for example: 

    public T Sum<T,R,S>(T t, R r, S s)

Could reference any of the following:

    <seealso cref="SlowCalculator.Sum"/>
    <seealso cref="SlowCalculator.Sum{T,R,S}(T,R,S)"/>
    <seealso cref="SlowCalculator.Sum``3(T,R,S)"/>
    <seealso cref="SlowCalculator.Sum``3(``1,``2,``3)"/>

**To URLs**

To link to URLs, use markdown links, such as `[Unity](http://www.unity3d.com)` in running text.

**To Manual articles from XMLdoc**

To link to manual content in the same package, use a relative link: `../manual/topic_file.md`. All the manual files are in the `manual` folder; all the API files are in the `api` folder with no subfolders in either case. Example: `[Entities](../manual/ecs_entities.md)`
You can use `.html` in the link, but then the link checker doesn't validate it.

**To API from Manual**

The same thing works from manual content to API content e.g
`[EntityQuery](../api/Unity.Entities.EntityQuery.html)` In this case, you **must** add `.html` to the end of the link.

**To API with a UID from Manual**

If you want to link directly to members of a class, you must use DocFX's special `xref` link format:
`[EntityQuery.SetFilter()](xref:Unity.Entities.EntityQuery.SetFilter*)`

The xref must be fully qualified and DocFX "mangles" the names, so it can be hard to manually guess links for generic methods and those with parameters. DocFX creates an xrefmap file that contains the uid that you can use to reference a function or other member. 

The xrefmap file is in the root of the generated documentation. If the API exists in already published documentation, you can find it online. For example, the DOTS version is [https://docs.unity3d.com/Packages/com.unity.entities@0.0/xrefmap.yml](https://docs.unity3d.com/Packages/com.unity.entities@0.0/xrefmap.yml). You can also generate the documentation locally and find the file on your hard drive.

For example, the entry for the link above is:

```
- uid: Unity.Entities.EntityQuery.SetFilter*
  name: SetFilter
  href: api/Unity.Entities.EntityQuery.html#Unity_Entities_EntityQuery_SetFilter_
  commentId: Overload:Unity.Entities.EntityQuery.SetFilter
  isSpec: "True"
  fullName: Unity.Entities.EntityQuery.SetFilter
  nameWithType: EntityQuery.SetFilter
```
This links to the first SetFilter function. You can also link to specific overrides, such as `xref:Unity.Entities.EntityQuery.SetFilter``1(``0)`

You can search within this file for an API to determine the correct uid to use in the link.

### Formatting

Markdown is the preference over XML formatting, but both are supported.
* `` `code` ``
* `**bold**`
* `*italic*`

## Manual

Documentation that covers overall concepts and tasks belong in the manual. 

Manual content uses DocFX-flavored markdown ([DFM](https://dotnet.github.io/docfx/spec/docfx_flavored_markdown.html)), which is reasonably close to Github-flavored markdown, to which it adds a few features.

The markdown files for the Manual go in the `Documentation~` folder at the root of each package directory. The table of contents is a bulleted list of links in the file `TableOfContents.md`, in the same folder.

Each package has a separate set of documentation. Linking between packages is currently problematic. Such links must be absolute html links to the published package docs on the web. The problematic aspect is that the published URLs contain a version number. You can change the version to reference @latest, but sometimes a specific version is necessary.

### Manual Code samples

For embedded code samples, place a line containing three backtick characters above and below the code. On the top line, add a space and "c#" (or other programming language code) to provide a hint to the syntax highlighter.

```
\``` c#
for(as many lines as you need)
{
    var code = Goes.Here;
}
\```
``` 

You can just indent one or more lines of code by 4 spaces, but syntax highlighting can be inconsistent.

#### Automatically compiled code examples

Instead of typing code in the markdown file where it potentially can get outdated, you can write your example code in a C# file and reference it from the markdown file. The doc tools extract the code from the C# file and inserts it into the markdown when it generates the manual .html files.

To reference one or more lines of code, use the following markdown syntax (similar to image syntax):

    [!code-lang[name](relative_path#region)]

Where:
* *lang* is the language code. Use  `cs` for C# code.
* *name* is a string. I'm not really sure what it is used for, if anything.
* *relative_path* is the relative path to the code file.
* *region* is the region in the code file that indicates which lines to extract.
 
For example:

    [!code-cs[speedjob](../DocCodeSamples.Tests/ChunkIterationJob.cs#speedjob)]

You should place the example code in the `DocCodeSamples.Tests` assembly. Currently one exists for the Entities package, but we can create more as needed. ~Note the "../package/" element in the example path. This is needed because of the way the doc tools copy the package files when they generate package documentation.~ The need for `package` in the path will be removed in the next (1.2) version of the Doc Tools package.

## Testing

1. Install the *Package Manager DocTools* package in Unity. For information on how to do this, see the [Using DocTools package](https://confluence.unity3d.com/pages/viewpage.action?spaceKey=DOCS&title=Using+the+DocTools+package) page on Confluence.
1. Close and reopen the Package Manage window to refresh its UI.
1. Select the package that contains your changes (e.g. Entities).
1. Click **Generate Documentation**.

The package manager doc tool generates the doc set, starts up a local web server, and opens your new package docs.

When you regenerate, you must clear the browser cache to see your changes. (This could be browser/OS dependent.)

## More resources

Slack:
* #devs-documentation
* #docs-style-guide
* #docs-packman

Unity:
* [In depth Scripting API style guidelines](https://unity-docs.gitbook.io/style-guide/)
* [Unity Doc Style Guide](https://unity-docs.gitbook.io/style-guide/style/scripting-api)

External:
* [DocFX XMLDoc support](https://dotnet.github.io/docfx/spec/triple_slash_comments_spec.html)
* [DocFX Markdown support](https://dotnet.github.io/docfx/spec/docfx_flavored_markdown.html?tabs=tabid-1%2Ctabid-a)
* [Microsoft XMLDoc tutorial with examples](https://docs.microsoft.com/en-us/dotnet/csharp/codedoc)
