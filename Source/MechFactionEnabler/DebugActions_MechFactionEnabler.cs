using LudeonTK;
using Verse;

namespace MechFactionEnabler
{
    /// <summary>Dev-mode entries under Debug actions → "Mech Faction Enabler".</summary>
    public static class DebugActions_MechFactionEnabler
    {
        private const string Category = "Mech Faction Enabler";

        [DebugAction(Category, "Add Mechanoid faction", allowedGameStates = AllowedGameStates.Playing)]
        private static void AddMechanoidFaction()
        {
            MechFactionUtility.AddWithFeedback();
        }

        [DebugAction(Category, "Log faction status", allowedGameStates = AllowedGameStates.Playing)]
        private static void LogFactionStatus()
        {
            MechFactionUtility.LogStatus();
            Messages.Message("MFE_StatusLogged".Translate(), RimWorld.MessageTypeDefOf.NeutralEvent, historical: false);
        }

        [DebugAction(Category, "Re-enable load prompt", allowedGameStates = AllowedGameStates.Playing)]
        private static void ResetPrompt()
        {
            Current.Game.GetComponent<GameComponent_MechFactionEnabler>()?.ResetPrompt();
            Messages.Message("MFE_PromptReset".Translate(), RimWorld.MessageTypeDefOf.NeutralEvent, historical: false);
        }
    }
}
