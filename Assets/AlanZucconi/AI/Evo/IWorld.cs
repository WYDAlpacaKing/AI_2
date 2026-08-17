using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.LightTransport;

namespace AlanZucconi.AI.Evo
{
    public interface IWorld <T> where T : IGenome
    {
        // Resets the world
        void ResetSimulation();

        // Assign a genome to this world
        void SetGenome(T genome);

        // Retries the genome that was assigned
        //T GetGenome();

        void StartSimulation();

        // If the situation is done
        bool IsDone();

        // Gets the genome score
        // (only when IsDone())
        float GetScore();
    }
}