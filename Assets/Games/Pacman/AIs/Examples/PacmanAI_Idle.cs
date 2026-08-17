using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlanZucconi.Pacman
{
    [CreateAssetMenu
    (
        fileName = "PacmanAI_Idle",
        menuName = "Pacman/Examples/Idle"
    )]
    public class PacmanAI_Idle : PacmanAI
    {
        public override Action Move()
        {
            return Action.None;
        }
    }
}