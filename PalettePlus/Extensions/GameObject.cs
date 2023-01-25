﻿using System.Linq;
using System.Collections.Generic;

using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;

using CSGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

using PalettePlus.Interop;
using PalettePlus.Structs;
using PalettePlus.Palettes;
using PalettePlus.Services;
using Dalamud.Logging;

namespace PalettePlus.Extensions {
	internal static class GameObjectExtensions {
		internal unsafe static ModelParams* UpdateColors(this GameObject actor) {
			var model = Model.GetModel(actor);
			if (model == null) return null;

			Hooks.UpdateColors(model);

			return model->GetModelParams();
		}

		internal static List<Palette> GetPersists(this GameObject actor) {
			var results = new List<Palette>();

			foreach (var persist in PalettePlus.Config.Persistence) {
				if (!persist.Enabled || !persist.IsApplicableTo(actor)) continue;

				var palette = persist.FindPalette();
				if (palette != null) results.Add(palette);
			}

			return results;
		}

		internal unsafe static bool IsValidForPalette(this GameObject obj) {
			var actor = (Actor*)obj.Address;

			if (obj is not Character) return false;

			var isValid = actor != null && actor->ModelId == 0 && actor->GetModel() != null;
			if (!isValid) return false;

			if (obj.ObjectKind == ObjectKind.EventNpc)
				return false;

			return true;
		}

		internal static GameObject? FindOverworldEquiv(this GameObject obj) {
			return PluginServices.ObjectTable.FirstOrDefault(
				ch => ch.ObjectIndex < 200 && ch is Character && ch.Name.ToString() == obj.Name.ToString()
			);
		}

		internal unsafe static void Redraw(this GameObject obj) {
			var actor = (CSGameObject*)obj.Address;
			if (actor == null) return;
			actor->DisableDraw();
			actor->EnableDraw();
		}

		internal static (string,string) GetNameAndWorld(this Character chara) {
			var worldName = "";
			if (chara is PlayerCharacter pc) {
				var world = pc.HomeWorld.GameData;
				if (chara.ObjectIndex > 200 && chara.ObjectIndex < 241) {
					var ovw = (PlayerCharacter?)chara.FindOverworldEquiv();
					if (ovw != null) world = ovw.HomeWorld.GameData;
				}

				if (world != null)
					worldName = world.Name;
			}

			return (chara.Name.ToString(), worldName);
		}
	}
}