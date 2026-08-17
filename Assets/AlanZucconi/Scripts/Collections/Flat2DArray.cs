using UnityEngine;

namespace AlanZucconi
{
    // Can be used to serialise 2D arrays
    // But you need to create a non-generic class first
    [System.Serializable]
    public class Flat2DArray<T>
    {
        public int W;
        public int H;

        [SerializeField]
        public T[] Array; // Unity can serialise this

        public Flat2DArray(int w, int h)
        {
            W = w;
            H = h;
            Array = new T[W * H];
        }


        public T this[int x, int y]
        {
            get => Array[Index(x, y)];
            set => Array[Index(x, y)] = value;
        }

        private int Index(int x, int y) => x * W + y;
    }
}