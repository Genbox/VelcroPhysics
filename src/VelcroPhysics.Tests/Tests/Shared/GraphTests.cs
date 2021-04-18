using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Tests.Code;
using Xunit;

namespace Genbox.VelcroPhysics.Tests.Tests.Shared
{
    public class GraphTests
    {
        [Fact]
        public void TestEmptyConstruction()
        {
            Graph<int> graph = new Graph<int>();
            Assert.Equal(0, graph.Count);
            Assert.Null(graph.First);

            Graph<Dummy> graph2 = new Graph<Dummy>();
            Assert.Equal(0, graph2.Count);
            Assert.Null(graph2.First);
        }

        [Fact]
        public void TestAddValueType()
        {
            Graph<int> graph = new Graph<int>();
            GraphNode<int> node = graph.Add(10);

            Assert.Equal(10, node.Item);
            Assert.Equal(node, node.Prev);
            Assert.Equal(node, node.Next);
        }

        [Fact]
        public void TestAddReferenceType()
        {
            Graph<Dummy> graph = new Graph<Dummy>();

            Dummy instance = new Dummy();

            GraphNode<Dummy> node = graph.Add(instance);

            Assert.Equal(instance, node.Item);
            Assert.Equal(node, node.Prev);
            Assert.Equal(node, node.Next);
        }

        [Fact]
        public void TestRemoveValueType()
        {
            Graph<int> graph = new Graph<int>();
            GraphNode<int> node = graph.Add(10);

            Assert.Equal(1, graph.Count);

            graph.Remove(node);

            Assert.Equal(0, graph.Count);

            //Check that the node was cleared;
            Assert.Null(node.Prev);
            Assert.Null(node.Next);
        }

        [Fact]
        public void TestRemoveReferenceType()
        {
            Graph<Dummy> graph = new Graph<Dummy>();
            GraphNode<Dummy> node = graph.Add(new Dummy());

            Assert.Equal(1, graph.Count);

            graph.Remove(node);

            Assert.Equal(0, graph.Count);

            //Check that the node was cleared;
            Assert.Null(node.Prev);
            Assert.Null(node.Next);
        }

        [Fact]
        public void TestIteration()
        {
            Graph<int> graph = new Graph<int>();

            for (int i = 0; i < 10; i++)
            {
                graph.Add(i);
            }

            Assert.Equal(10, graph.Count);

            int count = 0;

            foreach (int i in graph)
            {
                Assert.Equal(count++, i);
            }

            Assert.Equal(10, count);
        }
    }
}