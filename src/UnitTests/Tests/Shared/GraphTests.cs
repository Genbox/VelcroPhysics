using VelcroPhysics.Shared;
using Xunit;

namespace UnitTests.Tests.Shared
{
    public class Dummy { }

    public class GraphTests
    {
        [Fact]
        public void TestEmptyConstruction()
        {
            Graph<int> graph = new Graph<int>();
            Assert.Equal(0, graph.Count);
            Assert.Equal(null, graph.First);

            Graph<Dummy> graph2 = new Graph<Dummy>();
            Assert.Equal(0, graph2.Count);
            Assert.Equal(null, graph2.First);
        }

        [Fact]
        public void TestAddValueType()
        {
            Graph<int> graph = new Graph<int>();
            GraphNode<int> node = graph.Add(10);

            Assert.Equal(node.Item, 10);
            Assert.Equal(node.Prev, node);
            Assert.Equal(node.Next, node);
        }

        [Fact]
        public void TestAddReferenceType()
        {
            Graph<Dummy> graph = new Graph<Dummy>();

            Dummy instance = new Dummy();

            GraphNode<Dummy> node = graph.Add(instance);

            Assert.Equal(node.Item, instance);
            Assert.Equal(node.Prev, node);
            Assert.Equal(node.Next, node);
        }

        [Fact]
        public void TestRemoveValueType()
        {
            Graph<int> graph = new Graph<int>();
            GraphNode<int> node = graph.Add(10);

            Assert.Equal(graph.Count, 1);

            graph.Remove(node);

            Assert.Equal(graph.Count, 0);

            //Check that the node was cleared;
            Assert.Equal(node.Prev, null);
            Assert.Equal(node.Next, null);
        }

        [Fact]
        public void TestRemoveReferenceType()
        {
            Graph<Dummy> graph = new Graph<Dummy>();
            GraphNode<Dummy> node = graph.Add(new Dummy());

            Assert.Equal(graph.Count, 1);

            graph.Remove(node);

            Assert.Equal(graph.Count, 0);

            //Check that the node was cleared;
            Assert.Equal(node.Prev, null);
            Assert.Equal(node.Next, null);
        }

        [Fact]
        public void TestIteration()
        {
            Graph<int> graph = new Graph<int>();

            for (int i = 0; i < 10; i++)
            {
                graph.Add(i);
            }

            Assert.Equal(graph.Count, 10);

            int count = 0;

            foreach (int i in graph)
            {
                Assert.Equal(count++, i);
            }

            Assert.Equal(10, count);
        }
    }
}
