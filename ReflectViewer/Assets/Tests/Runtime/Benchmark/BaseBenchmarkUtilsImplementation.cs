using System;

namespace ReflectViewerRuntimeTests
{
    [BenchmarkUtilsImplPriority(0)]
    public class BaseBenchmarkUtilsImplementation : BenchmarkUtils
    {
        string BenchmarkUtils.GetScenePath()
        {
            return "Assets/Scenes/Reflect.unity";
        }

        void BenchmarkUtils.PostSceneSetup()
        {

        }
    }

    public interface BenchmarkUtils
    {
        public string GetScenePath();
        public void PostSceneSetup();
    }

    [AttributeUsage(AttributeTargets.Class)]
    sealed class BenchmarkUtilsImplPriorityAttribute : Attribute
    {
        readonly int priority;

        public BenchmarkUtilsImplPriorityAttribute(int priorityInt)
        {
            priority = priorityInt;
        }

        public int Priority
        {
            get { return priority; }
        }
    }
}
