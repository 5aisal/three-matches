using UnityEngine;

namespace ThreeMatches.Characters
{
    /// <summary>Animator parameter and layer names shared by the rigs and the controller builder.</summary>
    public static class CharacterParams
    {
        public const string SpeedName = "Speed";
        public const string StandName = "Stand";
        public const string InteractName = "Interact";
        public const string VerticalName = "Vertical";
        public const string ScaredName = "Scared";
        public const string SitName = "Sit";
        public const string CoughName = "Cough";
        public const string CarryName = "Carry";

        public const string MatchLayerName = "Match";
        public const string InteractLayerName = "Interact";

        public static readonly int Speed = Animator.StringToHash(SpeedName);
        public static readonly int Stand = Animator.StringToHash(StandName);
        public static readonly int Interact = Animator.StringToHash(InteractName);
        public static readonly int Vertical = Animator.StringToHash(VerticalName);
        public static readonly int Scared = Animator.StringToHash(ScaredName);
        public static readonly int Sit = Animator.StringToHash(SitName);
        public static readonly int Cough = Animator.StringToHash(CoughName);
        public static readonly int Carry = Animator.StringToHash(CarryName);
    }
}
