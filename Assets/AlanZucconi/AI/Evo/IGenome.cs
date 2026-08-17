using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlanZucconi.AI.Evo
{
    // Creates a random genome

    public interface IGenomeFactory<T>
        where T : IGenome
    {
        T Instantiate();
    }

    public interface IGenome
    {
        // Creates a copy
        IGenome Copy();

        void Mutate();


        // Performs many mutations at once
        void Mutate(int mutations)
        {
            for (int m = 0; m < mutations; m++)
                Mutate();
        }

        // TODO: use this when C# 11.0 will be supported
        // Static interface methods need to have a default implementation
        //static IGenome InstantiateGenome() => default;
    }
}