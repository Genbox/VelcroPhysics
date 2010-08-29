using Microsoft.Xna.Framework;

namespace FarseerPhysics
{
    public class Vertices
    {
        private Vector2[] array;

        public Vertices(int size)
        {
            array = new Vector2[size];
            Count = size;
        }

        public Vertices(Vertices vertices) : this(vertices.Count)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                this[i] = vertices[i];
            }

            Count = vertices.Count;
        }

        public int Count
        {
            get;
            set;
        }

        public void Add(Vector2 vertex)
        {
            Count++;

            if (array.Length <= Count)
            {
                array = new Vector2[array.Length * 2];
            }

            this[Count] = vertex;
        }


        public Vector2 this[int index]
        {
            get
            {
                return array[index];
            }
            set
            {
                array[index] = value;
            }
        }
    }
}
