using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlanZucconi.Pacman
{
    public class Ghost : Agent
    {

        // TODO: move main logic here?
        //  And leave only findtarget into GhostAI?

        [Header("Ghost")]
        public Color FrightenedColor = Color.blue;
        public Color EatenColor = Color.white;


        [Space]
        public GhostTimer Timer;

        /*
        // Assumed Ghost agents have a GhostAI script
        // This allows to easily access them
        public new GhostAI AI
        {
            get => (GhostAI) base.AI;
        }
        */

        // How many frames the ghost will be frightened for
        [ReadOnly]
        public int FrightenedTime = 0;


        public bool IsFrightened()
            => FrightenedTime > 0
            && ! IsEaten();

        // Event used to notify the ghost that pacman ate a power pellet
        public void OnPacmanPoweredUp(Pacman pacman)
        {
            FrightenedTime = pacman.PowerPelletTimer; // uses the same timer
        }


        public override void Initialise()
        {
            FrightenedTime = 0;
        }



        public override void UpdateState()
        {
            base.UpdateState();

            // Timer
            FrightenedTime = Mathf.Max(FrightenedTime - 1, 0);

        }


        #region Eating


        public override bool CanEat(Agent agent)
        {
            Pacman pacman = agent as Pacman;
            if (pacman == null)
                return false;

            return CanEat(pacman);
        }

        public bool CanEat(Pacman pacman)
            => !IsFrightened()
            // It doesn't matter if Pacman is powered:
            // if we are not frightened, we can eat them!
            //&& !pacman.IsPoweredUp()
            && !Eaten          // Can't eat if you've been eaten already
            && !pacman.Eaten   // Can't eat if they're been eaten already
            && !Game.Invincible; // Cannot eat pacman if the game is in Invincible mode


        /*
        public bool CanBeEatenBy(Pacman agent)
            => IsFrightened()
            && agent.IsPoweredUp()
            && !Eaten          // Can't eat if you've been eaten already
            && !agent.Eaten;   // Can't eat if they're been eaten already
        */

        public override void OnEatenBy(Agent agent)
        {
            base.OnEatenBy(agent);

            // Resets the frightened timer
            FrightenedTime = 0;
        }

        /*
        public override void Resurrect()
        {
            base.Resurrect();

            // Resets the frightened timer
            FrightenedTime = 0;
        }
        */
        #endregion


        // This agent has collided with another
        // This happens if they both stepped onto the same time
        // Or if the swapped coordinates
        //
        // When there is a collision between two agents,
        // the following methods are called:
        //  a.CollisionWith(b)
        //  b.CollisionWith(a)
        // To avoid executing the same code twice,
        // each agent updates its OWN status in CollisionWith,
        // not the status of the other agent.
        //public virtual void CollisionWith(Agent agent) { }
        /*
        {
            // The agent can eat us
            if (agent.CanEat(this))
            {

                EatenBy(agent);
                //Eaten = true;
                // TODO: Eaten method with change colour?

                // FIXME
                if (this == Game.Player && Game.Invincible)
                    Eaten = false;
            }

            // We can eat them
            if (CanEat(agent))
            {
                //Score++;
                //OriginalScore += OriginalScore_GhostEating;
            }
        }
        */



        public override Color GetColor()
        {
            if (IsEaten())
                return EatenColor;

            if (IsFrightened())
                return FrightenedColor;

            return base.GetColor();
        }
    }
}