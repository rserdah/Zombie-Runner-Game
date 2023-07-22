using UnityEngine;
using UnityEngine.Events;

public class StatExecutable : MonoBehaviour, IExecutable
{
    [System.Serializable]
    public struct StatEnhancement
    {
        public Player.Stats.StatType statType;
        public float enhancement;
    }

    public StatEnhancement statEnhancement;

    public UnityAction<ITriggerable> ExecuteAction 
    {
        get => (ITriggerable _) => 
        {
            switch(statEnhancement.statType)
            {
                case Player.Stats.StatType.HEALTH:
                    Player.player.stats.health += statEnhancement.enhancement;
                    break;

                case Player.Stats.StatType.STAMINA:
                    Player.player.stats.stamina += statEnhancement.enhancement;
                    break;

                case Player.Stats.StatType.ENERGY:
                    Player.player.stats.energy += statEnhancement.enhancement;
                    break;

                case Player.Stats.StatType.WATER:
                    Player.player.stats.water += statEnhancement.enhancement;
                    break;
            }
        };

        set { }
    }
}
