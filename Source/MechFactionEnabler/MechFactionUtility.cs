using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace MechFactionEnabler
{
    /// <summary>
    /// Adds the vanilla Mechanoid faction to a save that was generated without it.
    /// Every entry point (load prompt, dev-mode action) goes through <see cref="TryAddMechanoidFaction"/>
    /// so the safety checks live in exactly one place.
    /// </summary>
    public static class MechFactionUtility
    {
        public const string LogPrefix = "[Mech Faction Enabler] ";

        /// <summary>
        /// The existing mechanoid faction in this save, or null. Also counts a modded
        /// faction whose def declares <c>replacesFaction</c> = Mechanoid, so we never
        /// add vanilla mechs on top of a mod that already supplies its own.
        /// </summary>
        public static Faction ExistingMechanoidFaction
        {
            get
            {
                FactionManager manager = Find.FactionManager;
                if (manager == null)
                {
                    return null;
                }
                FactionDef mechDef = FactionDefOf.Mechanoid;
                return manager.AllFactionsListForReading.FirstOrDefault(f =>
                    f?.def != null && (f.def == mechDef || f.def.replacesFaction == mechDef));
            }
        }

        public static bool MechanoidFactionPresent => ExistingMechanoidFaction != null;

        /// <summary>
        /// Returns null when it's safe to add the faction, otherwise a human-readable reason why not.
        /// </summary>
        public static string CannotAddReason()
        {
            if (Current.ProgramState != ProgramState.Playing || Find.World == null || Find.FactionManager == null)
            {
                return "MFE_NotInGame".Translate();
            }
            if (FactionDefOf.Mechanoid == null)
            {
                return "MFE_NoMechDef".Translate();
            }
            Faction existing = ExistingMechanoidFaction;
            if (existing != null)
            {
                return "MFE_AlreadyPresent".Translate(existing.Name.Named("FACTION"));
            }
            return null;
        }

        /// <summary>
        /// Generates and registers the Mechanoid faction. Idempotent: calling it again once the
        /// faction exists does nothing and returns false.
        /// </summary>
        public static bool TryAddMechanoidFaction(out Faction faction, out string failReason)
        {
            faction = null;
            failReason = CannotAddReason();
            if (failReason != null)
            {
                return false;
            }

            try
            {
                // NewGeneratedFaction: assigns a fresh loadID, name, colour and calls
                // TryMakeInitialRelationsWith for every faction already in the manager.
                // Mechanoids are hidden, so it does not place a settlement on the map.
                faction = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(FactionDefOf.Mechanoid));

                // Add() is a no-op for the same instance, and calls RecacheFactions(),
                // so Faction.OfMechanoids starts returning the new faction immediately.
                Find.FactionManager.Add(faction);

                int fixedRelations = EnsureRelations(faction);

                Log.Message(LogPrefix + $"Added faction '{faction.Name}' (loadID {faction.loadID}). " +
                            $"Relations with player: {faction.RelationKindWith(Faction.OfPlayer)}. " +
                            $"Backfilled {fixedRelations} missing relation(s).");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(LogPrefix + "Failed to add Mechanoid faction: " + ex);
                failReason = "MFE_Failed".Translate();
                return false;
            }
        }

        /// <summary>
        /// Belt-and-braces: NewGeneratedFaction should already have created relations with every
        /// faction. This only fills gaps, never overwrites. TryMakeInitialRelationsWith writes BOTH
        /// sides but only checks ours, so it's only called when both sides are missing — otherwise
        /// it could add a duplicate entry on the other faction.
        /// </summary>
        private static int EnsureRelations(Faction added)
        {
            int count = 0;
            List<Faction> all = Find.FactionManager.AllFactionsListForReading;
            for (int i = 0; i < all.Count; i++)
            {
                Faction other = all[i];
                if (other == null || other == added)
                {
                    continue;
                }
                bool ours = added.RelationWith(other, allowNull: true) != null;
                bool theirs = other.RelationWith(added, allowNull: true) != null;
                if (!ours && !theirs)
                {
                    added.TryMakeInitialRelationsWith(other);
                    count++;
                }
                else if (ours != theirs)
                {
                    Log.Warning(LogPrefix + $"One-sided relation between {added.Name} and {other.Name}; leaving it alone.");
                }
            }
            return count;
        }

        /// <summary>
        /// Shared UI wrapper used by both the load prompt and the dev-mode action.
        /// </summary>
        public static void AddWithFeedback()
        {
            if (TryAddMechanoidFaction(out Faction faction, out string failReason))
            {
                Messages.Message("MFE_Added".Translate(faction.Name.Named("FACTION")), MessageTypeDefOf.PositiveEvent, historical: false);
                Find.WindowStack.Add(new Dialog_MessageBox("MFE_AddedSaveReload".Translate()));
            }
            else
            {
                Messages.Message(failReason, MessageTypeDefOf.RejectInput, historical: false);
            }
        }

        /// <summary>Dumps faction state to the log. Handy when someone reports a problem.</summary>
        public static void LogStatus()
        {
            Faction existing = ExistingMechanoidFaction;
            string cached = Faction.OfMechanoids?.Name ?? "null";
            string lines = string.Join("\n", Find.FactionManager.AllFactionsListForReading.Select(f =>
                $"  - {f.Name} [{f.def?.defName}] hidden={f.Hidden} loadID={f.loadID}"));
            Log.Message(LogPrefix + $"Mechanoid faction: {(existing == null ? "MISSING" : existing.Name)}; " +
                        $"Faction.OfMechanoids cache: {cached}\nAll factions:\n{lines}");
        }
    }
}
