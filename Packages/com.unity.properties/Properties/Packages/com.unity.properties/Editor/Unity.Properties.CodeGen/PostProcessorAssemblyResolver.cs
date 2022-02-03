using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Mono.Cecil;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace Unity.Properties.CodeGen
{
    /// <summary>
    /// This code is copied from 'dots/Samples/Packages/com.unity.entities/Unity.Entities.CodeGen/EntitiesILPostProcessor.cs'
    /// </summary>
    class PostProcessorAssemblyResolver : IAssemblyResolver
    {
        readonly string[] m_ReferenceDirectories;
        readonly Dictionary<string, HashSet<string>> m_ReferenceToPathMap;
        readonly Dictionary<string, AssemblyDefinition> m_Cache = new Dictionary<string, AssemblyDefinition>();
        readonly ICompiledAssembly m_CompiledAssembly;
        AssemblyDefinition m_SelfAssembly;

        public PostProcessorAssemblyResolver(ICompiledAssembly compiledAssembly)
        {
            m_CompiledAssembly = compiledAssembly;
            m_ReferenceToPathMap = new Dictionary<string, HashSet<string>>();
            foreach (var reference in compiledAssembly.References)
            {
                var assemblyName = Path.GetFileNameWithoutExtension(reference);
                if (!m_ReferenceToPathMap.TryGetValue(assemblyName, out var fileList))
                {
                    fileList = new HashSet<string>();
                    m_ReferenceToPathMap.Add(assemblyName, fileList);
                }
                fileList.Add(reference);
            }

            m_ReferenceDirectories = m_ReferenceToPathMap.Values.SelectMany(pathSet => pathSet.Select(Path.GetDirectoryName)).Distinct().ToArray();
        }

        public void Dispose()
        {
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            return Resolve(name, new ReaderParameters(ReadingMode.Deferred));
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
#if !UNITY_2020_2_OR_NEWER && !UNITY_DOTSRUNTIME
                lock (m_Cache)
#endif
            {
                if (name.Name == m_CompiledAssembly.Name)
                    return m_SelfAssembly;

                var fileName = FindFile(name);
                if (fileName == null)
                    return null;

                var cacheKey = fileName;
#if !UNITY_2020_2_OR_NEWER && !UNITY_DOTSRUNTIME
                    var lastWriteTime = File.GetLastWriteTime(fileName);
                    cacheKey += lastWriteTime.ToString();
#endif

                if (m_Cache.TryGetValue(cacheKey, out var result))
                    return result;

                parameters.AssemblyResolver = this;

                var ms = MemoryStreamFor(fileName);

                var pdb = fileName + ".pdb";
                if (File.Exists(pdb))
                    parameters.SymbolStream = MemoryStreamFor(pdb);

                var assemblyDefinition = AssemblyDefinition.ReadAssembly(ms, parameters);
                m_Cache.Add(cacheKey, assemblyDefinition);
                return assemblyDefinition;
            }
        }

        string FindFile(AssemblyNameReference name)
        {
            if (m_ReferenceToPathMap.TryGetValue(name.Name, out var paths))
            {
                if (paths.Count == 1)
                    return paths.First();

                // If we have more than one assembly with the same name loaded we now need to figure out which one
                // is being requested based on the AssemblyNameReference
                foreach (var path in paths)
                {
                    var onDiskAssemblyName = AssemblyName.GetAssemblyName(path);
                    if (onDiskAssemblyName.FullName == name.FullName)
                        return path;
                }
                throw new ArgumentException($"Tried to resolve a reference in assembly '{name.FullName}' however the assembly could not be found. Known references which did not match: \n{string.Join("\n", paths)}");
            }

            // Unfortunately the current ICompiledAssembly API only provides direct references.
            // It is very much possible that a postprocessor ends up investigating a type in a directly
            // referenced assembly, that contains a field that is not in a directly referenced assembly.
            // if we don't do anything special for that situation, it will fail to resolve.  We should fix this
            // in the ILPostProcessing api. As a workaround, we rely on the fact here that the indirect references
            // are always located next to direct references, so we search in all directories of direct references we
            // got passed, and if we find the file in there, we resolve to it.
            foreach (var parentDir in m_ReferenceDirectories)
            {
                var candidate = Path.Combine(parentDir, name.Name + ".dll");
                if (File.Exists(candidate))
                {
                    if (!m_ReferenceToPathMap.TryGetValue(candidate, out var referencePaths))
                    {
                        referencePaths = new HashSet<string>();
                        m_ReferenceToPathMap.Add(candidate, referencePaths);
                    }
                    referencePaths.Add(candidate);

                    return candidate;
                }
            }

            return null;
        }

        static MemoryStream MemoryStreamFor(string fileName)
        {
            return Retry(10, TimeSpan.FromSeconds(1), () => {
                byte[] byteArray;
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byteArray = new byte[fs.Length];
                    var readLength = fs.Read(byteArray, 0, (int)fs.Length);
                    if (readLength != fs.Length)
                        throw new InvalidOperationException("File read length is not full length of file.");
                }

                return new MemoryStream(byteArray);
            });
        }

        static MemoryStream Retry(int retryCount, TimeSpan waitTime, Func<MemoryStream> func)
        {
            try
            {
                return func();
            }
            catch (IOException)
            {
                if (retryCount == 0)
                    throw;
                Console.WriteLine($"Caught IO Exception, trying {retryCount} more times");
                Thread.Sleep(waitTime);
                return Retry(retryCount - 1, waitTime, func);
            }
        }

        public void AddAssemblyDefinitionBeingOperatedOn(AssemblyDefinition assemblyDefinition)
        {
            m_SelfAssembly = assemblyDefinition;
        }
    }
}