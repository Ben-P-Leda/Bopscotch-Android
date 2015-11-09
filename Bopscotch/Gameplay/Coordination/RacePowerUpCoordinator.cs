using System;

using Bopscotch.Gameplay.Objects.Display.Race;
using Bopscotch.Gameplay.Objects.Characters.Player;

namespace Bopscotch.Gameplay.Coordination
{
    public class RacePowerUpCoordinator
    {
        private PowerUpTimer _displayTimer;

        public Definitions.PowerUp AvailablePowerUp { get; private set; }
        public bool CurrentPowerUpAttacksOpponent { get; private set; }

        public Player Player { private get; set; }
        public PowerUpTimer DisplayTimer { set { _displayTimer = value; _displayTimer.CompletionCallback = HandlePowerUpTimerExpiration; } }

        public RacePowerUpCoordinator()
        {
            AvailablePowerUp = Definitions.PowerUp.None;
        }

        private void HandlePowerUpTimerExpiration()
        {
            Player.SetActivePowerUp(Definitions.PowerUp.None);
            _displayTimer.Deactivate();
        }

        public void SetAvailablePowerUpFromTexture(string powerUpTextureName)
        {
            AvailablePowerUp = Definitions.PowerUp.None;

            foreach (Definitions.PowerUp p in Enum.GetValues(typeof(Definitions.PowerUp)))
            {
                if (powerUpTextureName == string.Concat("power-", p).ToLower()) 
                { 
                    AvailablePowerUp = p;
                    CurrentPowerUpAttacksOpponent = AttackPowerUps.Contains(p.ToString().ToLower()); 
                    break;
                }
            }
        }

        public void ActivateAvailablePowerUp()
        {
            if (!CurrentPowerUpAttacksOpponent)
            {
                Player.SetActivePowerUp(AvailablePowerUp);
                _displayTimer.Activate(AvailablePowerUp);
            }

            AvailablePowerUp = Definitions.PowerUp.None;
        }

        private const string AttackPowerUps = "shades,shell,horn";
    }
}
