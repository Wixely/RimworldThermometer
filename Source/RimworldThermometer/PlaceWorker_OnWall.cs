using Verse;
//https://github.com/Skullywag/BetterVents/blob/BetterVents1.4/Source/BetterVents/PlaceWorker_OnWall.cs

namespace RimworldThermometer
{
    public class PlaceWorker_OnWall : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            var c = loc;

            var support = c.GetEdifice(map);
            
            
            if (support == null || support.def == null || support.def.graphicData == null)
                return AcceptanceReport.WasAccepted;
            /*

            if ((support.def == null) || (support.def.graphicData == null))
            {
                return (AcceptanceReport)("MessagePlacementOnSupport".Translate());
            }
            var test = (AcceptanceReport)("MessagePlacementOnSupport".Translate());

            */
            return (support.def.graphicData.linkFlags & (LinkFlags.Wall)) != 0 ? AcceptanceReport.WasAccepted : (AcceptanceReport)("Eh, cant place here...");
        }

    }
}
