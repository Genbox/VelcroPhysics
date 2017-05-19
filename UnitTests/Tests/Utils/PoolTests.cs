using System.Collections.Generic;
using System.Linq;
using UnitTests.TestObjects;
using VelcroPhysics.Shared;
using Xunit;

namespace UnitTests.Tests.Utils
{
    public class PoolTests
    {
        [Fact]
        public void GetWhileAdding()
        {
            Pool<PoolObject> pool = new Pool<PoolObject>(() => new PoolObject(), 1);

            bool first = true;

            //We use the fact that it is lazy loaded to re-add the same item
            IEnumerable<PoolObject> many = pool.GetManyFromPool(10);
            foreach (PoolObject obj in many)
            {
                if (first)
                {
                    Assert.True(obj.IsNew);
                    first = false;
                }
                else
                {
                    Assert.False(obj.IsNew);
                }

                pool.ReturnToPool(obj);
            }
        }

        [Fact]
        public void GetManyAcrossBoundary()
        {
            Pool<PoolObject> pool = new Pool<PoolObject>(() => new PoolObject(), 6);

            //We get twice as many as in pool
            List<PoolObject> many = pool.GetManyFromPool(12).ToList();
            foreach (PoolObject obj in many)
            {
                Assert.True(obj.IsNew);
            }

            Assert.Equal(12, many.Count);
        }

        [Fact]
        public void GetManyNewAndPooled()
        {
            Pool<PoolObject> pool = new Pool<PoolObject>(() => new PoolObject(), 10);

            //Empty whole pool
            List<PoolObject> many = pool.GetManyFromPool(10).ToList();
            foreach (PoolObject obj in many)
            {
                Assert.True(obj.IsNew);
            }
            Assert.Equal(10, many.Count);

            many = pool.GetManyFromPool(10).ToList();
            foreach (PoolObject obj in many)
            {
                Assert.True(obj.IsNew);
                pool.ReturnToPool(obj);
            }
            Assert.Equal(10, many.Count);

            many = pool.GetManyFromPool(10).ToList();
            foreach (PoolObject obj in many)
            {
                Assert.False(obj.IsNew);
            }
            Assert.Equal(10, many.Count);
        }

        [Fact]
        public void GetOnePooled()
        {
            Pool<PoolObject> pool = new Pool<PoolObject>(() => new PoolObject(), 1);
            PoolObject obj = pool.GetFromPool();

            Assert.True(obj.IsNew);

            pool.ReturnToPool(obj);
            obj = pool.GetFromPool();

            Assert.False(obj.IsNew);
        }

        [Fact]
        public void GetOneNew()
        {
            Pool<PoolObject> pool = new Pool<PoolObject>(() => new PoolObject(), 0);
            PoolObject obj = pool.GetFromPool();

            Assert.True(obj.IsNew);

            obj = pool.GetFromPool();
            Assert.True(obj.IsNew);
        }
    }
}
