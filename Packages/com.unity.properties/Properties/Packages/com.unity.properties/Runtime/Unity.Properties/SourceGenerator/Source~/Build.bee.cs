using System;
using Bee;
using Bee.CSharpSupport;
using Bee.DotNet;
using Bee.Tools;
using Bee.VisualStudioSolution;
using NiceIO;

class Build
{
    static void Main()
    {
        CSharpProgram.DefaultConfig = new CSharpProgramConfiguration(CSharpCodeGen.Debug, Csc.Latest);

        new VisualStudioSolution
        {
            Path = "Unity.Properties.SourceGenerator.gen.sln",
            Projects =
            {
                SetUp("Unity.Properties.SourceGenerator", isSourceGenerator: true),
                StandaloneBeeDriver.BuildProgramProjectFile
            }
        }.Setup();
    }

    static CSharpProgram SetUp(string directoryName, bool isSourceGenerator = true, params CSharpProgram[] extraReferences)
    {
        var csharpProgram = new CSharpProgram
        {
            Path = $"../{directoryName}.dll",
            Sources = {directoryName},
            References = {new NPath("Infrastructure").Files("*.dll")},
            CopyReferencesNextToTarget = false,
            Framework = {Framework.NetStandard20}
        };

        csharpProgram.References.Add(extraReferences);
        csharpProgram.SetupDefault();
        csharpProgram.ProjectFile.RedirectMSBuildBuildTargetToBee = true;
        
        var bee = new NPath(typeof(Bee.Core.Architecture).Assembly.Location);
        var isWin = Environment.OSVersion.Platform == PlatformID.Win32Windows || Environment.OSVersion.Platform == PlatformID.Win32NT;
        csharpProgram.ProjectFile.BuildCommand.Set(new Shell.ExecuteArgs()
        {
            Arguments = isWin ? "-no-colors" : "bee.exe -no-colors", Executable = isWin ? bee : "mono", WorkingDirectory = bee.Parent
        });
        
        return csharpProgram;
    }
}
