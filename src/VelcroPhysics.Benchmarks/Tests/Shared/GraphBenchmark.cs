using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Genbox.VelcroPhysics.Benchmarks.Code;
using Genbox.VelcroPhysics.Benchmarks.Code.TestClasses;
using Genbox.VelcroPhysics.Shared;

namespace Genbox.VelcroPhysics.Benchmarks.Tests.Shared
{
    public class GraphBenchmark : UnmeasuredBenchmark
    {
        private Graph<Dummy> _graph;
        private List<Dummy> _list;

        [GlobalSetup]
        public void Setup()
        {
            _list = ConstructList();
            _graph = ConstructGraph();
        }

        [Benchmark]
        public List<Dummy> ConstructList()
        {
            List<Dummy> list = new List<Dummy>(1000);

            for (int i = 0; i < 1000; i++)
            {
                list.Add(new Dummy());
            }

            return list;
        }

        [Benchmark]
        public Graph<Dummy> ConstructGraph()
        {
            Graph<Dummy> graph = new Graph<Dummy>();

            for (int i = 0; i < 1000; i++)
            {
                graph.Add(new Dummy());
            }

            return graph;
        }

        [Benchmark]
        public int IterateList()
        {
            int i = 0;
            foreach (Dummy d in _list)
            {
                i++;
            }

            return i;
        }

        [Benchmark]
        public int IterateGraph()
        {
            int i = 0;
            foreach (Dummy d in _graph)
            {
                i++;
            }

            return i;
        }

        [Benchmark]
        public void RemoveFromList()
        {
            Dummy d = new Dummy();
            _list.Add(d);
            _list.Remove(d);
        }

        [Benchmark]
        public void RemoveFromGraph()
        {
            Dummy d = new Dummy();
            _graph.Add(d);
            _graph.Remove(d);
        }

        [Benchmark]
        public void RemoveFromGraphFast()
        {
            Dummy d = new Dummy();
            GraphNode<Dummy> node = _graph.Add(d);
            _graph.Remove(node);
        }
    }
}