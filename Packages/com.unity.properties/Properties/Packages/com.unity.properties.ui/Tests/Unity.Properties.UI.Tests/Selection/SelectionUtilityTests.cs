using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using Unity.Properties.UI.Internal;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Resources = UnityEngine.Resources;

namespace Unity.Properties.UI.Tests
{
    [TestFixture, UI]
    class SelectionUtilityTests
    {
        class Element<T> : VisualElement
        {
            public T m_Content;
        }

        class Content : ContentProvider
        {
            public class Data
            {
                public float Value;
                public Content m_Content;

                public Data(Content content)
                {
                    m_Content = content;
                }
            }

            class DataInspector : Inspector<Data>
            {
                public override VisualElement Build()
                {
                    return new Element<Content> {m_Content = Target.m_Content};
                }
            }

            public Content()
            {
                m_Data = new Data(this);
            }

            public Data m_Data;

            public override string Name => "Title";

            public override object GetContent()
            {
                return m_Data;
            }
        }

        class NullContent : ContentProvider
        {
            public override string Name { get; } = "Null";
            public override object GetContent()
            {
                return null;
            }
        }
        
        class ExceptionContent : ContentProvider
        {
            public override string Name { get; } = "Exception";
            public override object GetContent()
            {
                throw new Exception("Why are you doing this?");
            }
        }

        void CloseAllWindows()
        {
            foreach (var window in Resources.FindObjectsOfTypeAll<ContentWindow>())
            {
                if (window && null != window)
                    window.Close();
            }
        }

        [UnityTest]
        public IEnumerator ShowingContent_InEditorWindow_SurvivesDomainReload()
        {
            CloseAllWindows();
            var content = new Content();
            content.m_Data.Value = 25;
            SelectionUtility.ShowInWindow(content);
            yield return null;
            var window = EditorWindow.GetWindow<ContentWindow>();
            Assert.That(window, Is.Not.Null);
            Assert.That(content.Name, Is.EqualTo(window.titleContent.text));
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            yield return new WaitForDomainReload();
            yield return null;
            window = EditorWindow.GetWindow<ContentWindow>();
            Assert.That(window, Is.Not.Null);
            var element = window.rootVisualElement.Q<Element<Content>>();
            Assert.That(element.m_Content.m_Data.Value, Is.EqualTo(25));
            Assert.That(element.m_Content.Name, Is.EqualTo(window.titleContent.text));
            window.Close(); 
        }
        
        [UnityTest, Ignore("Test is unstable on CI")]
        public IEnumerator ShowingContent_InInspector_SurvivesDomainReload()
        {
            var content = new Content();
            content.m_Data.Value = 25;
            SelectionUtility.ShowInInspector(content);
            for(var i = 0; i < 10; ++i)
                yield return null;
            Assert.That(Selection.activeObject, Is.TypeOf<InspectorContent>());
            var proxy = (InspectorContent) Selection.activeObject;
            Assert.That(proxy, Is.Not.Null);
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            yield return new WaitForDomainReload();
            yield return null;
            proxy = (InspectorContent) Selection.activeObject;
            Assert.That(proxy, Is.Not.Null);
            var element = proxy.Root.Q<Element<Content>>();
            Assert.That(element.m_Content.m_Data.Value, Is.EqualTo(25));
            Selection.activeObject = null;
        }

        class LifecycleContent : ContentProvider
        {
            public ContentStatus Status = ContentStatus.ContentNotReady;
            
            public override string Name { get; }

            protected override ContentStatus GetStatus()
            {
                return Status;
            }

            public override object GetContent()
            {
                return this;
            }
        }

        class LifecycleContentInspector : Inspector<LifecycleContent>
        {
            public override VisualElement Build()
            {
                var root = new Element<LifecycleContent> {m_Content = Target};
                root.Add(new Label("Hello"));
                return root;
            }
        }

        [UnityTest]
        public IEnumerator ShowingContent_InEditorWindow_CanControlLifecycle()
        {
            CloseAllWindows();
            var content = new LifecycleContent();
            content.Status = ContentStatus.ContentNotReady;
            SelectionUtility.ShowInWindow(content);
            var window = EditorWindow.GetWindow<ContentWindow>();
            yield return null;
            Assert.That(window && null != window, Is.EqualTo(true));
            var element = window.rootVisualElement.Q<Element<LifecycleContent>>();
            Assert.That(element, Is.Null);
            yield return null;
            element = window.rootVisualElement.Q<Element<LifecycleContent>>();
            Assert.That(element, Is.Null);

            content.Status = ContentStatus.ContentReady;
            yield return null;
            element = window.rootVisualElement.Q<Element<LifecycleContent>>();
            Assert.That(element, Is.Not.Null);

            content.Status = ContentStatus.ContentNotReady;
            yield return null;
            element = window.rootVisualElement.Q<Element<LifecycleContent>>();
            Assert.That(element, Is.Null);

            content.Status = ContentStatus.ContentUnavailable;
            yield return null;
            Assert.That(window && null != window, Is.EqualTo(false));
        }
        
        [UnityTest, Ignore("Test is unstable on CI")]
        public IEnumerator ShowingContent_InInspectorWindow_CanControlLifecycle()
        {
            Selection.activeObject = null;
            yield return null;
            var content = new LifecycleContent();
            content.Status = ContentStatus.ContentNotReady;
            SelectionUtility.ShowInInspector(content);
            for(var i = 0; i < 10; ++i)
                yield return null;
            var editor = Resources.FindObjectsOfTypeAll<InspectorContentEditor>().FirstOrDefault();
            Assert.That(editor && null != editor, Is.EqualTo(true));
            var element = editor.Target.Root.Q<Element<LifecycleContent>>();
            Assert.That(element, Is.Null);
            yield return null;
            element = editor.Target.Root.Q<Element<LifecycleContent>>();
            Assert.That(element, Is.Null);
            
            content.Status = ContentStatus.ContentReady;
            
            for(var i = 0; i < 10; ++i)
                yield return null;
            element = editor.Target.Root.Q<Element<LifecycleContent>>();
            Assert.That(element, Is.Not.Null);
            
            content.Status = ContentStatus.ContentNotReady;
            for(var i = 0; i < 10; ++i)
                yield return null;
            element = editor.Target.Root.Q<Element<LifecycleContent>>();
            Assert.That(element, Is.Null);
            
            content.Status = ContentStatus.ContentUnavailable;
            for(var i = 0; i < 10; ++i)
                yield return null;
            Assert.That(editor && null != editor, Is.EqualTo(false));
        }

        [UnityTest]
        public IEnumerator ContentProvider_WhenGivenNullContent_Exits()
        {
            var content = new NullContent();
            LogAssert.Expect(LogType.Error, "SelectionUtilityTests.NullContent: Releasing content named 'Null' because it returned null value.");
            SelectionUtility.ShowInWindow(content);
            for(var i = 0; i < 10; ++i)
                yield return null;
            Assert.That(Resources.FindObjectsOfTypeAll<ContentWindow>().Any(), Is.False);
        }
        
        [UnityTest]
        public IEnumerator ContentProvider_WhenContentThrows_Exits()
        {
            var content = new ExceptionContent();
            LogAssert.Expect(LogType.Error, "SelectionUtilityTests.ExceptionContent: Releasing content named 'Exception' because it threw an exception.");
            LogAssert.Expect(LogType.Exception, "Exception: Why are you doing this?");
            SelectionUtility.ShowInWindow(content);
            for(var i = 0; i < 10; ++i)
                yield return null;
            Assert.That(Resources.FindObjectsOfTypeAll<ContentWindow>().Any(), Is.False);
        }
    }
}