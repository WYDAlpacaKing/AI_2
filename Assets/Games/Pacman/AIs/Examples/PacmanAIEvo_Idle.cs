using UnityEngine;
using AlanZucconi.Pacman.Evo;

namespace AlanZucconi.Pacman
{
    [CreateAssetMenu
    (
        fileName = "PacmanAIEvo_Idle",
        menuName = "Pacman/Examples/PacmanAIEvo_Idle"
    )]
    public class PacmanAIEvo_Idle : PacmanAIEvo
    {
        public override void Initialise()
        {
            // PacmanAIEvo agents have access to float Weights[],
            // which the Evolution library can use to optimise your parameters
            // The size of the array can be changed from the inspector on the ScriptableObject

            // This code prints the parameters when the agent is initialised
            for (int i = 0; i < Weights.Length; i++)
                Debug.Log($"{i}\t{Weights[i]}");
        }

        public override Action Move()
        {
            // In here you can use Weights[] to change the behaviour of your agent

            return Action.None;
        }
    }
}