# UPM Package Starter Kit

The purpose of this package starter kit is to provide the data structure and development guidelines for new packages meant for the **Unity Package Manager (UPM)**.

This is the first of many steps towards an automated package publishing experience within Unity. This package starter kit is merely a fraction of the creation, edition, validation, and publishing tools that we will end up with.

> **Note:** With 2021.1 and package lifecycle V2 we are introducing [Unity Standards](https://github.cds.internal.unity3d.com/unity/standards/tree/2021.1), an initiative to help us build quality and consistency into our products from the start. This README contains cross-references to Unity Standards, which apply from 2021.1 and up. For more information, see [Working with Unity Standards](https://scholar.internal.unity3d.com/course/working-with-unity-standards).

We hope you enjoy your experience. You can use **#devs-packman-tooling** and **#devs-packman-client** on Slack to provide feedback or ask questions regarding your package development efforts.


## Are you ready to become a package?
The Package Manager is a work-in-progress for Unity and, in that sense, there are a few criteria that must be met for your package to be considered on the package list at this time:

- **Your code accesses public Unity C# APIs only.**  If you have a native code component, it will need to ship with an official editor release.  Internal API access might eventually be possible for Unity made packages, but not at this time.

- **Your code doesn't require security, obfuscation, or conditional access control.**  Anyone should be able to download your package and access the source code.


## Package structure

```none
<root>
  ├── package.json
  ├── README.md
  ├── CHANGELOG.md
  ├── LICENSE.md
  ├── Third Party Notices.md
  ├── QAReport.md
  ├── Editor
  │   ├── Unity.[YourPackageName].Editor.asmdef
  │   └── EditorExample.cs
  ├── Runtime
  │   ├── Unity.[YourPackageName].asmdef
  │   └── RuntimeExample.cs
  ├── Tests
  │   ├── .tests.json
  │   ├── Editor
  │   │   ├── Unity.[YourPackageName].Editor.Tests.asmdef
  │   │   └── EditorExampleTest.cs
  │   └── Runtime
  │        ├── Unity.[YourPackageName].Tests.asmdef
  │        └── RuntimeExampleTest.cs
  ├── Samples
  │   └── Example
  │       ├── .sample.json
  │       └── SampleExample.cs
  └── Documentation~
       ├── your-package-name.md
       └── Images
```

## Develop your package
Package development works best within the Unity Editor.  Here's how to set that up:

1. Clone the Package Starter Kit repository locally.

	- In a console (or terminal) application, choose a place to clone the repository and enter the following:
		```
		git clone git@github.cds.internal.unity3d.com:unity/com.unity.package-starter-kit.git
		```

1. Create a new repository for your package and clone to your desktop.

	- On Github.cds create a new repository with the name of your package (Example: `"com.unity.terrain-builder"`).

	- In a console (or terminal) application, choose a place to clone the repository and perform the following:
		```
		git clone git@github.cds.internal.unity3d.com:unity/com.unity.[sub-group].[your-package-name]
		```
		__Note:__ `sub-group` here means product area (if possible; ex: xr, rendering, ui) and not org/dept within the company (i.e. don't [ship the org chart](https://en.wikipedia.org/wiki/Conway%27s_law))
        
	> **Applicable Unity Standards:** 
	[US-0002](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0002/US-0002.md), 
	[US-0003](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0003/US-0003.md), 
	[US-0005](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0005/US-0005.md),
	[US-0006](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0006/US-0006.md),
	[US-0017](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0017/US-0017.md) 

1. Copy the contents of the Package Starter Kit folder to your new package.  Be careful not to copy the Package Starter Kit *.git* folder over.

1. **Fill in your package information.**

  	Follow the instructions for [filling out your package manifest](#fill-out-your-package-manifest) (*package.json*). 
	
  	 > **Applicable Unity Standard:** [US-0007](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0007/US-0007.md)

 	 Then update the `"createSeparatePackage"` field in the *Tests/.tests.json* file to set up testing for Continuous Integration (CI):

  	* Set it to **false** if you want the tests to remain part of the published package. This is the default value.

  	* Set it to **true** if you want the CI to create a separate package for these tests, and add the metadata at publish time to link the packages together. This allows you to have a large number of tests, or assets, etc. that you don't want to include in your main package, while making it easy to test your package with those tests & fixtures.

	1. Start **Unity**, create a local empty project and import your package into the project.

	1. In a console (or terminal) application, push the package starter kit files you copied in your new package repository to its remote.
		- Add them to your repository's list to version
		```git add .```
		- Commit to your new package's remote master
		```git commit```
		- Push to your new package's remote master
		```git push```

1. **Update the *README.md* file.**

	It should contain all pertinent information for developers using your package, such as:

	* Prerequistes
	* External tools or development libraries
	* Required installed Software
	* Command line examples to build, test, and run your package.
	
    > **Applicable Unity Standard:** [US-0023](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0023/US-0023.md)


1. **Rename and update your documentation file(s).**

	Use the samples included in this starter kit to create preliminary, high-level documentation. Your documentation should introduce users to the features and sample files included in your package. For more information, see [Document your package](#document-your-package).
	
	> **Applicable Unity Standards:** 
	[US-0035](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0035/US-0035.md), 
	[US-0040](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0040/US-0040.md), 
	[US-0041](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0041/US-0041.md), 
	[US-0042](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0042/US-0042.md),
	[US-0046](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0046/US-0046.md), 
	[US-0051](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0051/US-0051.md), 
	[US-0066](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0066/US-0066.md), 
	[US-0068](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0068/US-0068.md) 

1. **Rename and update assembly definition files.**
	
	Choose a name schema to ensure that the name of the assembly built from the assembly definition file (_.asmdef_) will follow the .Net [Framework Design Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/index). For more information, see [Name your assembly definition files](#name-your-assembly-definition-files).
	
1. **Add samples to your package (code & assets).**

	The Package Manager recognizes the *Samples* directory in a package but does not import samples into Unity when the package is added to a project by default. Users can import samples into their */Assets* directory by clicking the **Import in project** button from the [Details view](https://docs.unity3d.com/Manual/upm-ui-details.html) of your package in the Package Manager window.

	If your package contains a sample:

	* Rename the *Samples/Example* folder, and update the *.sample.json* file in it.

	* If your package contains multiple samples, make a copy of the *Samples/Example* folder for each sample, and update each *.sample.json* file accordingly.

	Delete the *Samples* folder altogether if your package does not need samples.

1. **Validate your package using the Validation Suite.**

	Before you publish your package, you need to make sure that it passes all the necessary validation checks by using the Package Validation Suite. The Validation Suite will ensure that your package comforms to Unity's established package standards.  This is *required*.

	For more information, see [Validate your package](#validate-your-package).
	
	> **Applicable Unity Standard:** 
	[US-0019](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0019/US-0019.md)


1. **Follow our design guidelines**

	Follow these design guidelines when creating your package:


	* The [design checklist](https://unitytech.github.io/unityeditor-hig/topics/checklist.html) from Unity's Human Interface Guidelines.

	* The namespace for code in the asmdef *must* match the asmdef name, except the initial `Unity`, which should be replaced with `UnityEngine` or `UnityEditor`.

	* For **Runtime code**, only use the `Unity` namespace for code that has no dependency on anything in `UnityEngine` or `UnityEditor` and instead uses `ECS` and other `Unity`-namespace systems.

1. **Add tests to your package.**

	For **Editor tests**:
	* Write all your Editor Tests in *Tests/Editor*
	* If your tests require access to internal methods, add an *AssemblyInfo.cs* file to your Editor code and use `[assembly: InternalsVisibleTo("Unity.[YourPackageName].Editor.Tests")]`.

	For **Playmode Tests**:
	* Write all your Playmode Tests in *Tests/Runtime*.
	* If your tests require access to internal methods, add an *AssemblyInfo.cs* file to your Runtime code and use `[assembly: InternalsVisibleTo("Unity.[YourPackageName].Tests")]`.

    > **Applicable Unity Standards:** 
    [US-0018](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0018/US-0018.md), 
    [US-0020](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0020/US-0020.md)

1. **Setup your package CI.**

	Make sure your package continues to work against trunk or any other branch by setting up automated testing on every commit. See the [Confluence page](https://confluence.hq.unity3d.com/display/PAK/Setting+up+your+package+CI) that explains how to set up your package CI.

	You can find the CI configuration templates at https://github.cds.internal.unity3d.com/unity/upm-ci-yamato-templates/tree/master/.yamato. The specific configuration files relevant for single package development are named `package-*.yml` and they cover all functions from `pack`, to `test`, `publish` and `promote`.

1. **Update *CHANGELOG.md*.**

	Every new feature or bug fix should have a trace in this file. For more details on the chosen changelog format, see [Keep a Changelog](http://keepachangelog.com/en/1.0.0/).

	> **Applicable Unity Standard:** 
	[US-0039](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0039/US-0039.md)

1. **Validate your package against Unity Standards.**
	
	From 2021.1, **all** packages we ship must be compliant with Unity Standards. It is up to you to track and record compliance against the standards, but at certain critical milestones Release Management will check that compliance. 
	
	For more information, see [Using the Unity Standards checklist](#Using-the-Unity-Standards-checklist).


### Fill out your package manifest

1. Update the following required fields in file *package.json* (see [Confluence](https://confluence.hq.unity3d.com/pages/viewpage.action?pageId=39065257) for more information):

	| **Attribute name** | **Description** |
	|:---|:---|
	| `"name"` | Set the package name, following this naming convention: `"com.unity.[your-package-name]"`, without capital letters. For example, `"com.unity.2d.animation"`. |
	| `"displayName"` | Set the package's user-friendly display name. For example, `"Terrain Builder SDK"`. <br><br>__Note:__ Use a display name that will help users understand what your package is intended for. |
	| `"version"` | Set the package version in `"X.Y.Z"` format, following these [Semantic Versioning](http://semver.org/spec/v2.0.0.html) guidelines:<br>- To introduce a breaking API change, increment the major version (**X**.Y.Z).<br>- To introduce a new feature, increment the minor version (X.**Y**.Z).<br>- To introduce a bug fix, increment the patch version (X.Y.**Z**) |
	| `"unity"` | Set the Unity version your package is compatible with. For example: `"2018.1"`. |
	| `"unityRelease"` | Specify the Unity patch release your package is compatible with. For example: `"0a8"`.<br/><br/>__Note:__ This field is only required when the specific Unity version has a patch release. |
	| `"description"` | This description appears in the Package Manager window when the user selects this package from the list. For best results, use this text to summarize what the package does and how it can benefit the user.<br><br>Special formatting characters are supported, including line breaks (`\n`) and unicode characters such as bullets (`\u25AA`). For more information, see the [Package summaries](https://confluence.unity3d.com/x/oyuFAw) page in Confluence. |


1. Update the following recommended fields in file **package.json**:

	| **Attribute name** | **Description** |
	|:---|:---|
	| `"dependencies"` | List of packages this package depends on.  All dependencies will also be downloaded and loaded in a project with your package.  Here's an example:<br/><br/>`dependencies: {`<br/>&nbsp;&nbsp;&nbsp;&nbsp;`"com.unity.ads": "1.0.0",`<br/>&nbsp;&nbsp;&nbsp;&nbsp;`"com.unity.analytics": "2.0.0"`<br/>`}` |
	| `"keywords"` | An array of keywords related to the package. This field is currently purely informational. |
	| `"type"` | The type of your package. This is used to determine the visibility of your package in the Project Browser and the visibility of its Assets in the Object Picker. The `"tool"` and `"library"` types are used to set your package and its Assets as hidden by default. If not present or set to another value, your package and its Assets are visible by default. |
	| `"hideInEditor"` | A boolean value that overrides the package visibility set by the package type. If set to `false`, the default value, your package and its Assets are **always** visible by default; if set to `true`, your package and its Assets are **always** hidden by default. |

	**Notes**:
	- For packages in development, neither `"type"` nor `"hideInEditor"` are used. The package is **always** visible in the Project Browser and its Assets are **always** visible in the Object Picker.
	- The user is **always** able to toggle the package visibility in the Project Browser, as well as their Assets visibility in the Object Picker.
	
> **Applicable Unity Standard:** 
	[US-0007](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0007/US-0007.md)

### Document your package

You must document your public APIs and provide task-oriented documentation for your features. Before you start documenting, make sure you contact the [appropriate Technical Writer, or Lead on the Documentation team](hhttps://confluence.unity3d.com/display/DOCS/Meet+the+Documentation+team). They can help write the documentation with you. You can also fill out the [Package Docs Questions](https://docs.google.com/document/d/1vI0zbNX9OausEwYhg_kl9CIDNf4taExjON0fyp2cHS0) form, [log a JIRA DOC ticket](https://unity3d.atlassian.net/secure/CreateIssue!default.jspa?selectedProjectId=12200&issuetype=3) and attach the form to that ticket.

For help or clarification, see the **#devs-documentation** Slack channel.

The steps to completing your package's documentation are as follows:

1. Document all of [your public APIs](#document-your-public-apis) and [your features](#document-your-features).
2. [Test your documentation locally](#test-your-documentation-locally).
3. [Get your documentation published](#get-your-documentation-published).

Your package should include the documentation source in your package, but not the generated HTML.

The page in the Unity User Manual that links to package documentation is [Packages](http://docs.hq.unity3d.com/Manual/PackagesList.html).

> **Applicable Unity Standards:** 
[US-0035](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0035/US-0035.md), 
[US-0040](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0040/US-0040.md), 
[US-0041](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0041/US-0041.md), 
[US-0042](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0042/US-0042.md),
[US-0046](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0046/US-0046.md), 
[US-0051](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0051/US-0051.md), 
[US-0066](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0066/US-0066.md), 
[US-0068](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0068/US-0068.md) 

#### Document your public APIs

>**Note:** For even more detailed information on how to create API docs, see the [DOCUMENTING-APIS.md](DOCUMENTING-APIS.md) file in this package. 

API documentation is generated from any [XmlDoc tagged comments](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/xml-documentation-comments) found in the *.cs* files included in the package.  

You can use Visual Studio to autogenerate the correct tags by entering the documentation comments (`///`) in the empty line above your code. Visual Studio automatically detects which tags your code needs and inserts them. For example, if you write a method with two parameters that returns a value, Visual Studio gives you the **summary** tag (for the method description), two **param** tags, and one **returns** tag:

```c#
/// <summary>
///
/// </summary>
/// <param name="One"></param>
/// <param name="Two"></param>
/// <returns></returns>
public bool TestingXmlDoc(int One, int Two)
{

}
```

See the [Editor/EditorExample.cs](Editor/EditorExample.cs) file in this package for examples of how to document classes, methods, properties, and enums.

You need to document all public APIs. If you don't need an API to be accessed by clients, mark it as internal ot hide it using a filter:

1. Add a custom *filter.yml* file to the package *Documentation~* folder.  

2. Add filter rules to your custom *filter.yml* file. Start with the following rules, which make the DocFX configuration consistent with packages that don't have a custom filter:

```yaml
apiRules:
  - exclude:
      # inherited Object methods
      uidRegex: ^System\.Object\..*$
      type: Method
  - exclude:
      # mentioning types from System.* namespace
      uidRegex: ^System\..*$
      type: Type
  - exclude:
      hasAttribute:
        uid: System.ComponentModel.EditorBrowsableAttribute
        ctorArguments:
          - System.ComponentModel.EditorBrowsableState.Never
```

3. Specify classes to exclude by UID or by specifying regular expressions. For information about the rules for the filters, see the [DocFX guide](https://dotnet.github.io/docfx/tutorial/howto_filter_out_unwanted_apis_attributes.html).

> **Applicable Unity Standards:** 
[US-0035](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0035/US-0035.md), 
[US-0041](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0041/US-0041.md), 
[US-0046](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0046/US-0046.md), 
[US-0051](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0051/US-0051.md), 
[US-0066](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0066/US-0066.md), 
[US-0068](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0068/US-0068.md) 

#### Document your features

Write the Manual documentation using [GitHub-flavored Markdown](https://github.com/adam-p/markdown-here/wiki/Markdown-Cheatsheet) in *.md* files stored under the *Documentation~* folder. The *Documentation~* folder is suffixed with `~` to prevent its contents from being loaded in the Editor (recommended). Alternatively, you could use *.Documentation* for the folder name.

This preliminary, high-level documentation should introduce users to the features and sample files included in your package. When your package is ready for this level of documentation, you should [contact the Documentation team](https://confluence.unity3d.com/display/DOCS/Meet+the+Documentation+team) and work with them to get your documentation in order. 

All packages that expose UI in editor or runtime features should use one of the appropriate documentation example guides under the *Documentation~* folder:

* [tools-package-guide.md](Documentation~/tools-package-guide.md) for a package that includes features that augment the Unity Editor or Runtime (modules, tools, and libraries)
* [sample-package-guide.md](Documentation~/sample-package-guide.md) for a package that includes sample files
* [test-package-guide.md](Documentation~/test-package-guide.md) for a package that provides tests

For instructions, see the [documentation guidelines](Documentation~/index.md).

> **Applicable Unity Standards:** 
[US-0035](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0035/US-0035.md), 
[US-0040](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0040/US-0040.md), 
[US-0042](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0042/US-0042.md),
[US-0051](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0051/US-0051.md), 
[US-0066](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0066/US-0066.md), 
[US-0068](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0068/US-0068.md) 

#### Test your documentation locally

As you are developing your documentation, you can see what your documentation will look like by using the DocTools package: **com.unity.package-manager-doctools** (optional).

Once the DocTools package is installed, it displays a **Generate Documentation** button in the Package Manager window's Details view for any installed or embedded packages. To install the extension, see [Using the DocTools package](https://confluence.unity3d.com/display/DOCS/The+DocTools+package) on Confluence.

The DocTools package includes documentation validation, which creates a report for each package in the `Log/DocToolReports` folder under your Unity project folder. To enable running the validation and writing the report, make sure you enable the **Validate** option before generating. After the DocTools package finishes generating the documentation, click the **View Error Report** button.
The DocTools extension is an internal, unreleased package. If you come across arguable results, please discuss them on **#docs-packman**.

#### Get your documentation published

When the documentation is complete, notify the technical writer assigned to your team or project. 

> **Note:** The package will remain in **unreleased** mode until the final documentation is completed.  Users will have access to the developer-generated documentation only in experimental packages.

After the technical writer reviews the documentation, they create a pull request in the package git repository. The package's development team then needs to submit a new package version with the updated docs.

> **Applicable Unity Standards:** 
[US-0043](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0043/US-0043.md), 
[US-0044](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0044/US-0044.md), 
[US-0045](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0045/US-0045.md)


### Name your assembly definition files

If your package contains Editor code, rename and modify [Editor/Unity.SubGroup.YourPackageName.Editor.asmdef](Editor/Unity.SubGroup.YourPackageName.Editor.asmdef). Otherwise, delete the *Editor* directory.

* Name **must** match your package name, suffixed by `.Editor` (for example, `Unity.[YourPackageName].Editor`).
* Assembly **must** reference `Unity.[YourPackageName]` (if you have any runtime code).
* Platforms **must** include `"Editor"`.

If your package contains code that needs to be included in Unity runtime builds, rename and modify [Runtime/Unity.SubGroup.YourPackageName.asmdef](Runtime/Unity.SubGroup.YourPackageName.asmdef). Otherwise, delete the *Runtime* directory.

* Name **must** match your package name (for example,`Unity.[YourPackageName]`)

If your package has Editor code, you **must** have Editor Tests. In that case, rename and modify [Tests/Editor/Unity.SubGroup.YourPackageName.EditorTests.asmdef](Tests/Editor/Unity.SubGroup.YourPackageName.EditorTests.asmdef).

* Name **must** match your package name, suffixed by `.Editor.Tests` (for example, `Unity.[YourPackageName].Editor.Tests`)
* Assembly **must** reference `Unity.[YourPackageName].Editor` and `Unity.[YourPackageName]` (if you have any Runtime code).
* Platforms **must** include `"Editor"`.
* Optional Unity references **must** include `"TestAssemblies"` to allow your Editor Tests to show up in the Test Runner or run on Katana when your package is listed in the project manifest's `testables` field.

If your package has Runtime code, you **must** have Playmode Tests. In that case, rename and modify [Tests/Runtime/Unity.SubGroup.YourPackageName.Tests.asmdef](Tests/Runtime/Unity.SubGroup.YourPackageName.Tests.asmdef).

* Name **must** match your package name, suffixed by `.Tests` (for example, `Unity.[YourPackageName].Tests`)
* Assembly **must** reference `Unity.[YourPackageName]`.
* Optional Unity references **must** include `"TestAssemblies"` to allow your Playmode Tests to show up in the Test Runner or run on Katana when your package is listed in the project manifest's `testables` field.

### Validate your package

To validate your package, you must install the validation suite package into your project.  To do so, follow these steps:

1. Make sure you have `Unity 2019.2` or above.

2. From the Package Manager window, install the latest version of the **Package Validation Suite** with the context set to **Unity Registry**. If you don't see it in the list, make sure the [Enable Preview Packages](https://docs.unity3d.com/Documentation/Manual/class-PackageManager.html) project setting is enabled.
	**Note**: For releases prior to 2020.1, enable **Advanced** &gt; **Show preview packages** instead.

Once the Validation Suite package is installed, a **Validate** button appears in the details pane when you select your installed package from the Package Manager window.

To run the tests:

1. Click the **Validate** button to run a series of tests. A **See Results** button appears after the test run.
2. Click the **See Results** button for additional explanation:
	* If it succeeds, a green bar displays a **Success** message.
	* If it fails, a red bar displays a **Failed** message.

The validation suite is still under development, so if you come across arguable results, please discuss them on **#devs-packman-tooling**.

> **Applicable Unity Standard:** 
[US-0019](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0019/US-0019.md)


## Create an Experimental Package
Experimental Packages are a great way of getting your features in front of Unity Developers in order to get early feedback on functionality and UI designs. Experimental packages need to go through the publishing to production flow, as would any other package, but with diminished requirements:

  * Expected Package structure respected
  * Package loads in Unity Editor without errors
  * License file present - With third party notices file if necessary
  * Test coverage is good - Optional but preferred
  * Public APIs documented, minimal feature docs exists- Optional but preferred

To mark your package as **Experimental**, set the package version in your `package.json` file to use either `0` for the **MAJOR** portion of the version, or suffix the `-exp` tag after the **PATCH** portion of the version. For example:

```json
"version" : "0.1.0"

OR

"version" : "1.2.0-exp"
```

For more information on the Experimental cycle, see the [Experimental](https://confluence.unity3d.com/display/PAK/Experimental) Confluence page.

For an overview of the entire lifecycle of packages, see the [Lifecycle V2](https://confluence.unity3d.com/display/PAK/Lifecycle+V2) Confluence page. 

## Register your package

If you think you are working on a feature that is a good package candidate, please take a minute to advise the Package Review Board in the **#package-review-board** channel. Please make sure that your repo is private until the review board has approved the content of your package (NDA, Platforms, Product Management, etc).

Working with the board of dev directors and with product management, we will schedule the entry of the candidates in the ecosystem, based on technical challenges and on our feature roadmap.


> **Applicable Unity Standard:** 
[US-0067](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0067/US-0067.md)


## Share your package

If you want to share your project with other developers, the steps are similar to what's presented above. On the other developer's machine:

1. Start **Unity** and create a local empty project.

1. Launch the console (or terminal) application, navigate to the newly created project folder, and then clone your repository in the `Packages` directory:

    ```shell
    cd <YourProjectPath>/Packages
    git clone https://github.cdsinternal.unity3d.com/unity/[your-package-name].git com.unity.[sub-group].[your-package-name]
    ```
    > __Note:__ Your directory name must be the name of your package (Example: `"com.unity.terrain-builder"`).

## Make sure your package meets all legal and security requirements

All packages must COMPLETE AND SUBMIT THE [LICENSE OUT FORM](https://docs.google.com/forms/d/e/1FAIpQLSe3H6PARLPIkWVjdB_zMvuIuIVtrqNiGlEt1yshkMCmCMirvA/viewform) to receive approval. It is a simple, streamlined form that tells legal if there are any potential issues that need to be addressed prior to publication.

If your package has third-party elements and its licenses are approved, then all the licenses must be added to the **Third Party Notices.md** file. If you have more than one license, duplicate the `Component Name/License Type/Provide License Details` section for each additional license.

> **Note:** A URL can work as long as it actually points to the reproduced license and the copyright information _(if applicable)_.

If your package does not have third party elements, you can remove the *Third Party Notices.md* file from your package.

> **Applicable Unity Standards:** 
[US-0026](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0026/US-0026.md), 
[US-0027](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0027/US-0027.md), 
[US-0028](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0028/US-0028.md), 
[US-0030](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0030/US-0030.md), 
[US-0031](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0031/US-0031.md), 
[US-0032](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0032/US-0032.md),
[US-0033](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0033/US-0033.md),
[US-0034](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0034/US-0034.md),
[US-0037](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0037/US-0037.md)


## Preparing your package for the Candidates registry

Before publishing your package to production, you must send your package on the Package Manager's internal **candidates** repository.  The candidates repository is monitored by QA and release management, and is where package validation will take place before it is accepted in production.

1. Publishing your changes to the Package Manager's **Candidates** registry happens from Github.cds. To do so, set up your project's Continuous integration (CI), which will be triggered by "Tags" on your branches.

	For information see [the Confluence page](https://confluence.hq.unity3d.com/display/PAK/Setting+up+your+package+CI) that describes how to set up CI for your package.

1. Test your package locally. Once your package is published on the **Candidates** registry, you can test your package in the editor by creating a new project, and editing the project's *manifest.json* file to point to your candidate package:

      ```json
      dependencies: {
        "com.unity.[sub-group].[your-package-name]": "0.1.0"
      },
      "registry": "https://artifactory.prd.cds.internal.unity3d.com/artifactory/api/npm/upm-candidates"
      ```

> **Applicable Unity Standards:** 
[US-0018](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0018/US-0018.md), 
[US-0019](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0019/US-0019.md), 
[US-0020](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0020/US-0020.md), 
[US-0021](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0021/US-0021.md), 
[US-0022](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0022/US-0022.md)

## Using the Unity Standards checklist

The Standards team provides verification checklists for you to copy and use. You can use these to view all the standards that are mandatory or recommended for your package.

Currently there are checklists for:

* Experimental (unsupported) packages
* Supported (pre-release, release-candidate) packages

> Note some standards for experimental packages are recommended, becoming mandatory when you move to a supported package phase.

To get a copy of the checklist:
	
1. Open the template preview: [Unity Standards checklist for packages](https://docs.google.com/spreadsheets/d/1RvSuuU-9I2DhO8eT9S1M5px_2hCd4r5U_VkUmnuwX2c/template/preview) (the same template is used for both experimental and supported packages).

2. Click the **USE TEMPLATE** button to create a copy of the template for you to complete. 

3. Rename your copy of the checklist to match your package name and version. For example, name the checklist `com.unity.mypackage-0.0.1-pre.1`.

4. Save (move) and maintain your copy of the checklist in Release Management’s [Package Standards Checklists folder](https://drive.google.com/drive/u/0/folders/14-oCWTptVpvHz09Rza0Q0iuUgqUvMEwG) in Google Drive. 
	
For more information, see [How to validate your package against Unity Standards](https://scholar.internal.unity3d.com/reader/course/working-with-unity-standards/article/how-to-validate-your-package-against-unity-standards-1).


## Get your package published to Production

The process to promote packages to the **Production** registry changes depending on what phase your package is in:  

* First time **Experimental** and **Prerelease** packages are promoted by Release Management from the **Candidates** registry. After the first version is released, you can promote subsequent versions on your own. 
* Release Candidates and Released packages are promoted to the **Production** registry by Release Management along with every Unity release.

When your package meets the Release Management Criteria, the promotion process can be started by contacting Release Management in **#devs-pkg-promotion**.

**Note:** Release management validates your package content, and checks that the editor/playmode tests pass before promoting the package to production.

> **Applicable Unity Standards:** 
[US-0004](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0004/US-0004.md),
[US-0014](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0014/US-0014.md), 
[US-0015](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0015/US-0015.md), 
[US-0016](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0016/US-0016.md),
[US-0018](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0018/US-0018.md),
[US-0021](https://github.cds.internal.unity3d.com/unity/standards/blob/2021.1/definitions/US-0021/US-0021.md)

### Verified status and bundled packages
If your package is meant to ship with a release of the editor (**Verified Packages** and **Bundled Packages**), follow these steps:
1. To be marked as verified, in trunk, modify the editor manifest (*[root]\External\PackageManager\Editor\manifest.json*) to include your package in the **verified** list.

1. If your package is not verified, but only bundled with the editor, submit one or more Test Project(s) in Ono, so that your new package can be tested in all ABVs moving forward.

	The following steps will create a test project that will run in ABVs, load your package into the project, and run all the tests found in your package. The better your test coverage, the more confident you'll be that your package works with trunk.

	* Create a branch in Ono, based on the latest branch this package must be compatible with (trunk, or release branch).
	* If your package contains **Editor Tests**:
		* In *[root]\Tests\Editor.Tests*, create a new EditorTest Project (for new packages use **YourPackageName**) or use an existing project (for new versions of existing package).

			To get a bare package for an EditorTest Project, click [here](https://oc.unity3d.com/index.php/s/Cldvuy6NpxqYy8y).

		* Modify the project’s *manifest.json* file to include the production version of the package (`name@version`).

		* Your project's *manifest.json* file should contain the following line:

			```json
			"testables" : [ "com.unity.[sub-group].[your-package-name]" ]
			```
	* If your package contains **PlaymodeTests**:
		* In *[root]\Tests\PlaymodeTests*, create a new PlaymodeTest Project (for new packages use **YourPackageName**) or use an existing project (for new versions of existing package).

		* Modify the project’s *manifest.json* file to include the candidate version of the package (`name@version`).

		* Your project's manifest.json file should contain the following line:

			```json
			"testables" : [ "com.unity.[sub-group].[your-package-name]" ]
			```

		* Commit your branch changes to Ono, and run all Windows & Mac Editor/PlayMode tests (not full ABV) in Katana.

1. Once the tests are green on Katana, create your PR, add both **Latest Release Manager** and **Trunk Merge Queue** as reviewers.


## FAQ

**What’s the difference between a core package and a default package?**

A core package is a package that has its code included with the Editor’s core code. This is interesting for packages that plan to change enormously in parallel to editor APIs. By moving package code to the editor’s repo, both core API\functionality changes can be made along with required packages changes in the same PR.
https://docs.google.com/document/d/1CMoanjR3KAdew-6n39JdCFmHkTp1oshs3vkpejapf4Q/edit

A default package is a verified package that gets installed with every new project users create, regardless of the template they use. We should limit the number of default packages we support, as each default package adds to the project loading time. The list of default packages can be found in the editor manifest (https://ono.unity3d.com/unity/unity/files/de904b9ed9b44580ecd1e883f510daaa08182cc5/External/PackageManager/Editor/manifest.json).

**What are the requirements for me to publish an experimental package?**

* [Unity Standards Repository](https://github.cds.internal.unity3d.com/unity/standards/tree/2021.1)

**What are the requirements for me to get my package verified for a version of unity?**

https://docs.google.com/document/d/1oWC9XArVfkGMnqN9azR4hW4Pcd7-kQQw8Oy7ckP43JE/

**How is my verified package tested in Katana?**

https://docs.google.com/document/d/1jwTh71ZGtB2vF0SsHEwivt2FunaJWMGDdQJTpYRj3EE/edit

**How is my template tested in Katana?**

Templates are no longer tested in Katana. Instead, you should enable CI on your repository to test any template package you have with the editor version(s) you want to be compatible with.

In addition, Release Management uses Working Sets to track the status of packages and templates that will go into the next Unity Releases, making this the perfect place to set up the product.

**How do I add samples to my package?**

https://docs.google.com/document/d/1rmxGh6Z9gtbQlGUKCsVBaR0RyHvzq_gsWoYs6sttzYA/edit#heading=h.fg1e3sz56048

**How do I setup CI or publishing options for my package?**
https://confluence.hq.unity3d.com/display/PAK/Setting+up+your+package+CI

**How can I add tests to my package?**

There’s a “Tests” directory in the package starter kit. If you add editor and playmode tests in that directory, they will make up the list of tests for your package.

**The tests in my package bloat my package too much, what are my options?**

https://docs.google.com/document/d/19kKIGFetde5ES-gKXQp_P7bxQ9UgBnBUu58-y7c1rTA/edit

**Can I automate my package publishing yet?**

Automated publishing to the **Candidates** registry is enabled, but promotion to the **Production** registry is restricted depending on the phase your package is in:
* **Experimental** can be promoted to **Production** automatically after the first version has been published by Release Management
* **Prerelease** can be promoted to **Production** automatically after the first version has been published by Release Management
* **Release Candidates** can only be promoted to **Production** by Release Management
* **Released** can only be promoted to **Production** by Release Management

**How do I get a template package started?**

Start with the Project Template Starter Kit (you can request access in **#devs-packman**).
https://github.cds.internal.unity3d.com/unity/com.unity.template-starter-kit

**How do I get my package included in a template?**

First and foremost, your package needs to be on the verified list of packages.  Only verified packages can get added to templates we ship with the editor. Then reach out to the templates community in **#devs-template** to open discussions on adding your package to one or more of our existing templates.

**How can I test my package locally, as a user would?**

https://confluence.hq.unity3d.com/display/PAK/How+to+add+a+git+package+to+your+project

**What tests are included by the validation suite?**

https://docs.google.com/spreadsheets/d/1CdO7D0WSirbZhjnVsdJxJwOPK4UdUDxSRBIqwyjm70w/edit#gid=0

