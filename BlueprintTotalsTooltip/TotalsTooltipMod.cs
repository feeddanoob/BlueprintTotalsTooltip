﻿using BlueprintTotalsTooltip.TotalsTipSettingsUtilities;
using HugsLib;
using HugsLib.Settings;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace BlueprintTotalsTooltip
{
	class TotalsTooltipMod : ModBase
	{
		#region settings
		public SettingHandle<bool> TrackingVisible { get; private set; }
		public SettingHandle<bool> TrackingForbidden { get; private set; }
		public SettingHandle<CameraZoomRange> ZoomForVisibleTracking { get; private set; }
		public SettingHandle<int> VisibilityMargin { get; private set; }
		public SettingHandle<bool> ClampTipToScreen { get; private set; }
		public SettingHandle<int> TooltipClampMargin { get; private set; }
		public SettingHandle<float> HighlightOpacity { get; private set; }
		public SettingHandle<bool> ShowRowToolTips { get; private set; }
		public SettingHandle<bool> CountInStorage { get; private set; }
		public SettingHandle<bool> CountForbidden { get; private set; }
		public SettingHandle<int> TipXPosition { get; private set; }
		public SettingHandle<int> TipYPosition { get; private set; }
		public SettingHandle<bool> TransferSelection { get; private set; }
		public static SettingHandle<bool> ShouldDrawTooltip { get; private set; }
		#endregion settings

		public TotalsTooltipDrawer TotalsTipDrawer { get; }

		public static readonly KeyBindingDef toggleTipDraw = DefDatabase<KeyBindingDef>.GetNamed("ToggleTracking");

		public override string ModIdentifier
		{
			get { return "BlueprintTotalsTooltip"; }
		}

		public TotalsTooltipMod()
		{
			TotalsTipDrawer = new TotalsTooltipDrawer(this);
		}

		public override void SettingsChanged()
		{
			TotalsTipDrawer.ResolveSettings();
			BlueprintSelectionTransferer.transferring = TransferSelection;
		}

		public override void DefsLoaded()
		{
			TrackingVisible = Settings.GetHandle("trackingVisible", "trackingVisible_title".Translate(), "trackingVisible_desc".Translate(), true);
			TrackingForbidden = Settings.GetHandle("trackingForbidden", "trackingForbidden_title".Translate(), "trackingForbidden_desc".Translate(), false);
			ZoomForVisibleTracking = Settings.GetHandle("zoomForTracking", "zoomForTracking_title".Translate(), "zoomForTracking_desc".Translate(),
				CameraZoomRange.Middle, null, "zoomForTracking_");
			VisibilityMargin = Settings.GetHandle("visibilityMargin", "visibilityMargin_title".Translate(), "visibilityMargin_desc".Translate(), 100,
				Validators.IntRangeValidator(0, UI.screenHeight / 2));
			VisibilityMargin.SpinnerIncrement = 10;
			ClampTipToScreen = Settings.GetHandle("clampTipToScreen", "clampTipToScreen_title".Translate(), "clampTipToScreen_desc".Translate(), true);
			TooltipClampMargin = Settings.GetHandle("clampMargin", "clampMargin_title".Translate(), "clampMargin_desc".Translate(), 10,
				Validators.IntRangeValidator(0, UI.screenHeight / 2));
			TooltipClampMargin.SpinnerIncrement = 10;
			HighlightOpacity = Settings.GetHandle("highlightOpacity", "highlightOpacity_title".Translate(), "highlightOpacity_desc".Translate(), 0.10f);
			HighlightOpacity.CustomDrawer = OpacityCustomDrawer;
			ShowRowToolTips = Settings.GetHandle("showTips", "showTips_title".Translate(), "showTips_desc".Translate(), true);
			CountInStorage = Settings.GetHandle("countInStorage", "countInStorage_title".Translate(), "countInStorage_desc".Translate(), false);
			CountForbidden = Settings.GetHandle("countForbidden", "countForbidden_title".Translate(), "countForbidden_desc".Translate(), false);
			ResolveTipPositionHandlers();
			TransferSelection = Settings.GetHandle("transferSel", "transferSel_title".Translate(), "transferSel_desc".Translate(), true);
			ShouldDrawTooltip = Settings.GetHandle("shouldDrawTooltip", "", "", false);
			ShouldDrawTooltip.NeverVisible = true;
			BlueprintSelectionTransferer.transferring = TransferSelection;
			TotalsTipDrawer.ResolveSettings();
		}

		public override void OnGUI()
		{
			CheckDrawSettingToggle();
			TotalsTipDrawer.OnGUI();
		}

		private void CheckDrawSettingToggle()
		{
			if (toggleTipDraw.KeyDownEvent)
			{
				ShouldDrawTooltip.Value = !ShouldDrawTooltip.Value;
				TooltipToggleAdder.NotifyPlaySettingToggled();
				if (ShouldDrawTooltip.Value)
					SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera(null);
				else
					SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(null);
			}
		}

		private void ResolveTipPositionHandlers()
		{
			TipXPosition = Settings.GetHandle("tooltipXPosition", "tooltipPosition_title".Translate(), null, 8);
			TipXPosition.CustomDrawer = TipXCustomDrawer;
			TipYPosition = Settings.GetHandle("tooltipYPosition", "", "", 2);
			TipYPosition.CustomDrawer = TipXCustomDrawer;
			TipYPosition.VisibilityPredicate = delegate { return false; };
		}

		private bool OpacityCustomDrawer(Rect rect)
		{
			Rect sliderRect = new Rect(rect.x, rect.y, rect.width - 3f, rect.height);
			float horizontalSliderResult = Widgets.HorizontalSlider_NewTemp(sliderRect, HighlightOpacity.Value, 0f, 0.25f, true, null, null, null, 1f / 100f);
			if (horizontalSliderResult != HighlightOpacity.Value)
			{
				HighlightOpacity.Value = horizontalSliderResult;
				return true;
			}
			return false;
		}

		private bool TipXCustomDrawer(Rect rect)
		{
			TipPosSettingsHandler.DrawTipPosSetting(rect, TipXPosition, TipYPosition);
			return TipPosSettingsHandler.settingsChanged;
		}
	}
}
