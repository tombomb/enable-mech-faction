using Verse;

namespace MechFactionEnabler
{
    /// <summary>
    /// When a save without a Mechanoid faction is loaded, asks the player whether to add one.
    /// RimWorld creates one of these per game automatically (Game.FillComponents).
    /// </summary>
    public class GameComponent_MechFactionEnabler : GameComponent
    {
        private bool dontAskAgain;

        public GameComponent_MechFactionEnabler(Game game)
        {
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref dontAskAgain, "dontAskAgain", false);
        }

        public override void LoadedGame()
        {
            // Deliberately LoadedGame, not StartedNewGame: someone who just removed mechs at
            // world creation doesn't want to be asked about it straight away.
            if (dontAskAgain || MechFactionUtility.MechanoidFactionPresent)
            {
                return;
            }
            // LoadedGame runs inside the loading long event; wait until the game is on screen.
            LongEventHandler.ExecuteWhenFinished(ShowPrompt);
        }

        private void ShowPrompt()
        {
            if (MechFactionUtility.MechanoidFactionPresent)
            {
                return;
            }
            Find.WindowStack.Add(new Dialog_MessageBox(
                "MFE_PromptText".Translate(),
                buttonAText: "MFE_PromptAdd".Translate(),
                buttonAAction: MechFactionUtility.AddWithFeedback,
                buttonBText: "MFE_PromptNever".Translate(),
                buttonBAction: () => dontAskAgain = true,
                title: "MFE_PromptTitle".Translate()));
        }

        /// <summary>Re-enables the load prompt (used by the dev-mode action).</summary>
        public void ResetPrompt() => dontAskAgain = false;
    }
}
