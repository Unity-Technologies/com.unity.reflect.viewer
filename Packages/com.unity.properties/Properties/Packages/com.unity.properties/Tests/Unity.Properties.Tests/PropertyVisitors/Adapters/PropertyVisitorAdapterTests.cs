#pragma warning disable 649
using System.Text;
using NUnit.Framework;
using Unity.Properties.Adapters;
using UnityEngine;

namespace Unity.Properties.Tests
{
    [TestFixture]
    class PropertyVisitorAdapterTests
    {
        public enum VisitStatus
        {
            Unhandled,
            Handled,
            Stop
        }
        
        struct Container
        {
            public int Value;
        }

        class TestVisitor : PropertyVisitor
        {
            public readonly StringBuilder Builder = new StringBuilder();
            string m_Message;

            public TestVisitor(string message)
            {
                m_Message = message;
            }

            public void Reset()
            {
                Builder.Clear();
            }

            public override string ToString()
            {
                return Builder.ToString();
            }

            protected override void VisitProperty<TContainer, TValue>(Property<TContainer, TValue> property,
                ref TContainer container, ref TValue value)
            {
                Builder.Append(m_Message);
            }
        }

        class SingleAdapter : IVisit<Container, int>
        {
            TestVisitor Visitor;

            public SingleAdapter(TestVisitor visitor)
            {
                Visitor = visitor;
            }

            public void Visit(VisitContext<Container, int> context, ref Container container, ref int value)
            {
                Visitor.Builder.Append("IVisit<Container, int>");
            }
        }

        class MultiAdapter
            : IVisit<Container, int>
                , IVisit<int>
        {
            TestVisitor Visitor;

            public MultiAdapter(TestVisitor visitor)
            {
                Visitor = visitor;
            }

            public void Visit(VisitContext<Container, int> context, ref Container container, ref int value)
            {
                Visitor.Builder.Append("IVisit<Container, int>");
            }

            public void Visit<TContainer>(VisitContext<TContainer, int> context, ref TContainer container,
                ref int value)
            {
                Visitor.Builder.Append("IVisit<int>");
            }
        }

        class WrappingAdapter : IVisit<Container, int>
        {
            TestVisitor Visitor;

            public WrappingAdapter(TestVisitor visitor)
            {
                Visitor = visitor;
            }

            public void Visit(VisitContext<Container, int> context, ref Container container, ref int value)
            {
                Visitor.Builder.Append("This sentence: \"");
                context.ContinueVisitationWithoutAdapters(ref container, ref value);
                Visitor.Builder.Append("\" is simply not true");
            }
        }

        class ChainedAdapter : IVisit<Container, int>
        {
            TestVisitor Visitor;
            VisitStatus Status;

            public ChainedAdapter(TestVisitor visitor, VisitStatus status)
            {
                Visitor = visitor;
                Status = status;
            }

            public void Visit(VisitContext<Container, int> context, ref Container container, ref int value)
            {
                Visitor.Builder.Append(value);
                switch (Status)
                {
                    case VisitStatus.Stop:
                        break;
                    case VisitStatus.Unhandled:
                        context.ContinueVisitation(ref container, ref value);
                        break;
                    case VisitStatus.Handled:
                        context.ContinueVisitationWithoutAdapters(ref container, ref value);
                        break;
                }
            }
        }

        class MultiContinueAdapter : IVisit<Container, int>
        {
            TestVisitor Visitor;
            int m_Repeat;
            bool m_WithAdapters;

            public MultiContinueAdapter(TestVisitor visitor, int repeat, bool withAdapters)
            {
                Visitor = visitor;
                m_Repeat = repeat;
                m_WithAdapters = withAdapters;
            }

            public void Visit(VisitContext<Container, int> context, ref Container container, ref int value)
            {
                for (var i = 0; i < m_Repeat; ++i)
                {
                    if (m_WithAdapters)
                        context.ContinueVisitation(ref container, ref value);
                    else
                        context.ContinueVisitationWithoutAdapters(ref container, ref value);
                }
            }
        }

        class MessageAdapter : IVisit<Container, int>
        {
            TestVisitor Visitor;
            string m_Message;

            public MessageAdapter(TestVisitor visitor, string message)
            {
                Visitor = visitor;
                m_Message = message;
            }

            public void Visit(VisitContext<Container, int> context, ref Container container, ref int value)
            {
                Visitor.Builder.Append(m_Message);
            }
        }

        class StopAdapter : IVisit<Container, int>
        {
            public StopAdapter()
            {
            }

            public void Visit(VisitContext<Container, int> context, ref Container container, ref int value)
            {
            }
        }

        struct ExcludeContainer
        {
            public int Value1;
            public int Value2;
            public int Value3;
            public float Value4;
            public string Value5;
            public Vector2Int Value2Int;
        }

        class ExcludeAdapter 
            : IExclude<int>
                , IExclude<float>
                , IVisit<int>
                , IVisit<float>
        {
            TestVisitor Visitor;
            
            public ExcludeAdapter(TestVisitor visitor)
            {
                Visitor = visitor;
            }
            
            public bool IsExcluded<TContainer>(ExcludeContext<TContainer, int> context, ref TContainer container, ref int value)
            {
                return true;
            }

            public bool IsExcluded<TContainer>(ExcludeContext<TContainer, float> context, ref TContainer container, ref float value)
            {
                return false;
            }
            
            public void Visit<TContainer>(VisitContext<TContainer, int> context, ref TContainer container, ref int value)
            {
                Visitor.Builder.Append("int");
            }

            public void Visit<TContainer>(VisitContext<TContainer, float> context, ref TContainer container, ref float value)
            {
                Visitor.Builder.Append("float");
            }
        }

        interface IInterface
        {
            string Name { get; set; }
        }

        class InterfaceA : IInterface
        {
            public string Name { get; set; } = "A";
        }

        class InterfaceB : InterfaceA
        {
            public new string Name { get; set; } = "B";
        }

        class InterfaceC : InterfaceB
        {
            public new string Name { get; set; } = "C";
        }

        class ContainerWithInterface
        {
            public InterfaceA InterfaceA = new InterfaceA();
            public InterfaceB InterfaceB = new InterfaceB();
            public InterfaceC InterfaceC = new InterfaceC();
        }

        class ContravariantAdapter
            : Unity.Properties.Adapters.Contravariant.IVisit<ContainerWithInterface, InterfaceA>
                , Unity.Properties.Adapters.Contravariant.IVisit<ContainerWithInterface, InterfaceB>
                , Unity.Properties.Adapters.Contravariant.IVisit<ContainerWithInterface, IInterface>
        {
            TestVisitor Visitor;

            public ContravariantAdapter(TestVisitor visitor)
            {
                Visitor = visitor;
            }

            public void Visit(VisitContext<ContainerWithInterface> context, ref ContainerWithInterface container,
                InterfaceA value)
            {
                Visitor.Builder.Append("A");
                context.ContinueVisitation(ref container);
            }

            public void Visit(VisitContext<ContainerWithInterface> context, ref ContainerWithInterface container,
                InterfaceB value)
            {
                Visitor.Builder.Append("B");
                context.ContinueVisitation(ref container);
            }

            public void Visit(VisitContext<ContainerWithInterface> context, ref ContainerWithInterface container,
                IInterface value)
            {
                Visitor.Builder.Append("_");
                context.ContinueVisitation(ref container);
            }
        }

        [Test]
        public void PropertyVisitor_WithoutAdapters_CallsDefaultBehaviour()
        {
            var container = new Container();
            var visitor = new TestVisitor("No adapters were reached");

            PropertyContainer.Accept(visitor, ref container);
            Assert.That(visitor.ToString(), Is.EqualTo("No adapters were reached"));
        }

        [Test]
        public void PropertyVisitor_WithSingleAdapter_CallsAdapter()
        {
            var container = new Container();
            var visitor = new TestVisitor("Failure");
            visitor.AddAdapter(new SingleAdapter(visitor));

            PropertyContainer.Accept(visitor, ref container);
            Assert.That(visitor.ToString(), Is.EqualTo("IVisit<Container, int>"));
            visitor.Reset();
            PropertyContainer.Accept(visitor, ref container);
            Assert.That(visitor.ToString(), Is.EqualTo("IVisit<Container, int>"));
        }

        [Test]
        public void PropertyVisitor_WithMultiAdapter_CallsMostTypedAdapter()
        {
            var container = new Container();
            var visitor = new TestVisitor("Failure");
            visitor.AddAdapter(new MultiAdapter(visitor));

            PropertyContainer.Accept(visitor, ref container);
            Assert.That(visitor.ToString(), Is.EqualTo("IVisit<Container, int>"));
            visitor.Reset();
            PropertyContainer.Accept(visitor, ref container);
            Assert.That(visitor.ToString(), Is.EqualTo("IVisit<Container, int>"));
        }

        [Test]
        public void PropertyVisitor_WithWrappingAdapter_CallsAdapterAndDefault()
        {
            var container = new Container();
            var visitor = new TestVisitor("Failure is inevitable");
            visitor.AddAdapter(new WrappingAdapter(visitor));

            PropertyContainer.Accept(visitor, ref container);
            Assert.That(visitor.ToString(),
                Is.EqualTo("This sentence: \"Failure is inevitable\" is simply not true"));
        }

        [TestCase(2, VisitStatus.Handled)]
        [TestCase(2, VisitStatus.Handled, VisitStatus.Stop)]
        [TestCase(2, VisitStatus.Handled, VisitStatus.Unhandled)]
        [TestCase(2, VisitStatus.Handled, VisitStatus.Handled)]
        [TestCase(2, VisitStatus.Unhandled)]
        [TestCase(3, VisitStatus.Unhandled, VisitStatus.Unhandled)]
        [TestCase(4, VisitStatus.Unhandled, VisitStatus.Unhandled, VisitStatus.Unhandled)]
        [TestCase(3, VisitStatus.Unhandled, VisitStatus.Handled)]
        [TestCase(2, VisitStatus.Unhandled, VisitStatus.Stop)]
        [TestCase(1, VisitStatus.Stop, VisitStatus.Stop)]
        [TestCase(1, VisitStatus.Stop, VisitStatus.Unhandled)]
        [TestCase(1, VisitStatus.Stop, VisitStatus.Handled)]
        public void PropertyVisitor_WhenChainingAdapters_RespectVisitation(int count, params VisitStatus[] statuses)
        {
            var container = new Container();

            var expectedHasDefault = true;
            foreach (var status in statuses)
            {
                if (status == VisitStatus.Stop)
                {
                    expectedHasDefault = false;
                    break;
                }

                if (status == VisitStatus.Handled)
                    break;
            }

            var visitor = new TestVisitor(expectedHasDefault ? "0" : "");

            foreach (var status in statuses)
            {
                visitor.AddAdapter(new ChainedAdapter(visitor, status));
            }

            PropertyContainer.Accept(visitor, ref container);
            Assert.That(visitor.ToString(), Is.EqualTo(new string('0', count)));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void PropertyVisitor_WhenCallingMultipleAdapters_RespectVisitation(bool withAdapters)
        {
            for (var count = 0; count < 10; ++count)
            {
                var container = new Container();
                var visitor = new TestVisitor("1");

                visitor.AddAdapter(new MultiContinueAdapter(visitor, count, withAdapters));
                visitor.AddAdapter(new MessageAdapter(visitor, "0"));

                PropertyContainer.Accept(visitor, ref container);

                var result = withAdapters ? new string('0', count) : new string('1', count);
                Assert.That(visitor.ToString(), Is.EqualTo(result));
            }
        }

        [Test]
        public void PropertyVisitor_WithContravariantAdapters_CallsMostTyped()
        {
            var container = new ContainerWithInterface();
            var visitor = new TestVisitor("");
            visitor.AddAdapter(new ContravariantAdapter(visitor));

            PropertyContainer.Accept(visitor, ref container);
            Assert.That(visitor.ToString(), Is.EqualTo("ABA"));
        }

        class MixedContainer
        {
            public IInterface Interface = new InterfaceA();
        }
        
        class MixedAdapter
            : IVisit<MixedContainer, IInterface>
        {
            TestVisitor Visitor;

            public MixedAdapter(TestVisitor visitor)
            {
                Visitor = visitor;
            }

            public void Visit(VisitContext<MixedContainer, IInterface> context, ref MixedContainer container, ref IInterface value)
            {
                Visitor.Builder.Append("A");
                context.ContinueVisitation(ref container, ref value);
            }
        }

        class MixedContravariantAdapter
            : Unity.Properties.Adapters.Contravariant.IVisit<MixedContainer, IInterface>
        {
            TestVisitor Visitor;

            public MixedContravariantAdapter(TestVisitor visitor)
            {
                Visitor = visitor;
            }
            
            public void Visit(VisitContext<MixedContainer> context, ref MixedContainer container, IInterface value)
            {
                Visitor.Builder.Append("A");
                context.ContinueVisitation(ref container);
            }
        }

        [Test]
        public void PropertyVisitor_WithMixedAdapters_CanIterate()
        {
            var container = new MixedContainer();
            var visitor = new TestVisitor("");
            visitor.AddAdapter(new MixedContravariantAdapter(visitor));
            visitor.AddAdapter(new MixedAdapter(visitor));

            PropertyContainer.Accept(visitor, ref container);
            Assert.That(visitor.ToString(), Is.EqualTo("AA"));
        }

        [Test]
        public void PropertyVisitor_WithInertAdapter_ContinuesAsExpected()
        {
            var container = new Container();
            var visitor = new TestVisitor("");
            visitor.AddAdapter(new MixedContravariantAdapter(visitor));
            visitor.AddAdapter(new MixedAdapter(visitor));
            visitor.AddAdapter(new MultiAdapter(visitor));

            PropertyContainer.Accept(visitor, ref container);
            Assert.That(visitor.ToString(), Is.EqualTo("IVisit<Container, int>"));
        }
        
        [Test]
        public void PropertyVisitor_WithExcludeAdapter_StopsVisitationWhenRequired()
        {
            var container = new ExcludeContainer();
            var visitor = new TestVisitor("X");
            visitor.AddAdapter(new ExcludeAdapter(visitor));

            PropertyContainer.Accept(visitor, ref container);
            Assert.That(visitor.ToString(), Does.Not.Contain("int"));
            Assert.That(visitor.ToString(), Does.Contain("float"));
        }
    }
}
#pragma warning restore 649
