using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlanZucconi.Pacman
{

    public class Pacman : Agent
    {
        // TODO: when resurrecting, the ghosts are not scared anymore!

        [Header("Power Pellet")]
        // Number of frames used by the power pellet
        // Real Pacman moves at 10 tiles per second.
        // Power pellet lasta for 10 seconds.
        // So the pacman can moves 100 tiles while powered
        public int PowerPelletTime = 100; // frames
        // Indicates how many frames of power pellet power remains for the player
        // When zero, there is no power.
        [ReadOnly]
        public int PowerPelletTimer = 0;

        // https://pacman.holenet.info/#LvlSpecs
        [Space]
        [Range(0f, 1f)]
        public float DefaultSpeed = 0.8f;
        [Range(0f, 1f)]
        public float PoweredSpeed = 0.9f;



        [Header("Score")]
        // The original score as measured in Pacman:
        // - pellet: 10 points (x240)
        // - power pellet: 50 points (x4)
        // (= 2,600 points to clear the level)
        // Eating ghosts during a single power pellet time:
        // - first: 200
        // - second: 400
        // - third: 800
        // - fourth: 1600
        // Eating fruits:
        // - first one appears after 70 pellets are eaten
        // - second one appears after 170 pellets are eaten
        // - fruits lasts for 9-10 seconds
        // - cherries are 100 points each
        // https://pacman.holenet.info/#LvlSpecs
        [ReadOnly]
        public int Score = 0;

        public const int Score_Pellet      = 10;
        public const int Score_PowerPellet = 50;
        public const int Score_Fruit       = 100;
        public const int Score_Ghost       = 200;

        [ReadOnly]
        public int Score_ThingsEaten = 0;

        // When powered, every ghosts eaten doubles the score (200, 400, 800, 1600)
        // We use this to keep track how many ghosts have been eaten since the last powerup!
        [HideInInspector]
        public int GhostsEatenCounter = 0;


        
        public override void Initialise()
        {
            PowerPelletTimer = 0;
            Speed = DefaultSpeed;
           

            Score = 0;
            Score_ThingsEaten = 0;
            GhostsEatenCounter = 0;
        }
        

        // Updates the state of the game based on the position of Pacman 
        public override void UpdateState ()
        {
            base.UpdateState ();


            // What item is the player on?
            Item item = Game.Level[Position];


            if (item == Item.Pellet)
            {
                Game.Level[Position] = Item.Ground;

                // Score
                Score_ThingsEaten++;
                Score += Score_Pellet;
            }

            if (item == Item.PowerPellet)
            {
                Game.Level[Position] = Item.Ground;

                PowerUp();

                // Score
                Score_ThingsEaten++;
                Score += Score_PowerPellet;
            }


            // Power pellet timer
            PowerPelletTimer = Mathf.Max(PowerPelletTimer - 1, 0);
            if (PowerPelletTimer == 0)
                PowerDown(); // TODO: not calling it every frame?


            // Pacman speed
            Speed =
                IsPoweredUp()
                ? PoweredSpeed
                : DefaultSpeed;
        }


        public void PowerUp()
        {
            // Powers the player
            PowerPelletTimer = PowerPelletTime;
            //Strength = Strength_PowerPellet;


            // What about this? Can you keep doubling points if you get two power ups in a row?
            GhostsEatenCounter = 0;

            // Notifies all ghosts
            Game.Ghosts
                .ForEach(ghost => ghost.OnPacmanPoweredUp(this));
        }

        public void PowerDown()
        {
            // Resets the counter
            GhostsEatenCounter = 0;
        }

        


        /** If true, the player is under the effect of the power pellet
         * and can eat other agents. */
        public bool IsPoweredUp()
            => PowerPelletTimer > 0
            && ! IsEaten();


        #region Eating
        // TODO interface?

        public override bool CanEat(Agent agent)
        {
            Ghost ghost = agent as Ghost;
            if (ghost == null)
                return false;

            return CanEat(ghost);
        }

        public bool CanEat (Ghost ghost)
            => ghost.IsFrightened()
            && IsPoweredUp()
            && !Eaten          // Can't eat if you've been eaten already
            && !ghost.Eaten;   // Can't eat if they're been eaten already



        /*
        // We are being eaten!
        public override void OnEatenBy(Agent agent)
        {
            base.OnEatenBy(agent);
        }
        */

        public override void OnEating(Agent agent)
        {
            base.OnEating(agent);

            // Updates the score
            Score += Score_Ghost * (int) Mathf.Pow(2, GhostsEatenCounter);
            GhostsEatenCounter++;

            Score_ThingsEaten++;
        }
        #endregion


        //TODO: colour when powered?
    }
}