using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
using UnityEngine;         // Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
using Verse;               // RimWorld universal objects are here (like 'Building')
//using Verse.AI;          // Needed when you do something with the AI
//using Verse.Sound;       // Needed when you do something with Sound
//using Verse.Noise;       // Needed when you do something with Noises
using RimWorld;            // RimWorld specific functions are found here (like 'Building_Battery')
                           //using RimWorld.Planet;   // RimWorld specific functions for world creation
                           //using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 


//https://ludeon.com/forums/index.php?topic=6048.msg58872#msg58872

namespace RimworldThermometer
{
    [StaticConstructorOnStartup]
    public class RThermometer : Building
    {
        float lastDetectedTemperature = 0;

        public string TemperatureMode = "Personnel";

        public Dictionary<string, Tuple<int, int>> Modes = new Dictionary<string, Tuple<int, int>>() {
            { "Personnel" , new Tuple<int, int>(0, 100) },
            { "Food" , new Tuple<int, int>(-10,20) },
            { "Extreme" , new Tuple<int, int>(-300, 300) }
        };

        public bool DisplayEnabled = true;

        public Color ColdColour = Color.blue;// new Color(181, 220, 255);
        //public Color ColdColour =  new Color(181, 220, 255, 1);
        public Color HotColour = Color.red;// new Color(255, 220, 181);

        public string NextMode()
        {
            var keys = Modes.Keys.ToList();
            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i] == TemperatureMode)
                    return keys[(i + 1) % keys.Count];
            }
            return Modes.Keys.First();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (base.Faction != Faction.OfPlayer)
            {
                DisplayEnabled = false;
            }
        }

        public override void Tick()
        {
            lastDetectedTemperature = this.AmbientTemperature;
            base.Tick();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (base.Faction == Faction.OfPlayer)
            {
                foreach (var key in Modes.Keys)
                {
                    if (TemperatureMode == key)
                    {
                        var nextKey = NextMode();
                        yield return new Command_Action
                        {
                            defaultLabel = $"{key} Mode",
                            defaultDesc = $"{key} Mode",
                            icon = ContentFinder<Texture2D>.Get($"Thermometer/mode_{key.ToLower()}", true),
                            action = delegate ()
                            {
                                this.TemperatureMode = nextKey;
                            },
                            hotKey = KeyBindingDefOf.Misc2
                        };
                    }
                }
                yield return new Command_Toggle
                {
                    defaultLabel = $"Display",
                    defaultDesc = $"Display",
                    icon = ContentFinder<Texture2D>.Get("Thermometer/DisplayMode", true),
                    isActive = (() => DisplayEnabled),
                    toggleAction = delegate ()
                    {
                        this.DisplayEnabled = !DisplayEnabled;
                    },
                    hotKey = KeyBindingDefOf.Misc2
                };
            }
            yield break;
            yield break;
        }

        public float RangeLerp(float input, float min, float max)
        {
            return Math.Max(0, Math.Min(1, ((((input - min) * 100.0f) / (max - min)) / 100.0f)));
        }

        public override void DrawGUIOverlay()
        {
            if (DisplayEnabled)
            {
                var text = GenText.ToStringTemperature(lastDetectedTemperature);
                Text.Font = GameFont.Tiny;
                float halfHeight = Text.CalcSize(text).y * 0.25f;

                var ranges = Modes[TemperatureMode];
                var normal = RangeLerp(lastDetectedTemperature, ranges.Item1, ranges.Item2);

                var labelColor = Color.Lerp(ColdColour, HotColour, normal);
                labelColor.r += 0.5f; // Brighten
                labelColor.g += 0.5f; // Brighten
                labelColor.b += 0.5f; // Brighten
                var position = DrawPos.MapToUIPosition();
                position.y -= halfHeight;

                GenMapUI.DrawThingLabel(position, text, labelColor);
            }
        }

        /// <summary>
        /// This string will be shown when the object is selected (focus)
        /// </summary>
        /// <returns></returns>
        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();
            var ranges = Modes[TemperatureMode];
            stringBuilder.AppendLine($"Mode: {TemperatureMode}");
            stringBuilder.AppendLine($"Range: {ranges.Item1}/{ranges.Item2}");
            stringBuilder.Append($"Temperature: {GenText.ToStringTemperature(lastDetectedTemperature)}");
            return stringBuilder.ToString();
        }
    }
}
