using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sungiant.Blimey.PsmRuntime
{
	public class VitaControllerImplementation
		: VitaController
	{
		internal void Update(GameTime time)
		{
			base.Reset();

			var gamePadData = Sce.Pss.Core.Input.GamePad.GetData(0);
			
			if (gamePadData.Buttons.HasFlag(Sce.Pss.Core.Input.GamePadButtons.Down))
				base.DPad.Down = ButtonState.Pressed;

			if (gamePadData.Buttons.HasFlag(Sce.Pss.Core.Input.GamePadButtons.Up))
				base.DPad.Up = ButtonState.Pressed;

			if (gamePadData.Buttons.HasFlag(Sce.Pss.Core.Input.GamePadButtons.Left))
				base.DPad.Left = ButtonState.Pressed;

			if (gamePadData.Buttons.HasFlag(Sce.Pss.Core.Input.GamePadButtons.Right))
				base.DPad.Right = ButtonState.Pressed;

			if (gamePadData.Buttons.HasFlag(Sce.Pss.Core.Input.GamePadButtons.Cross))
				base.Buttons.Cross = ButtonState.Pressed;

			if (gamePadData.Buttons.HasFlag(Sce.Pss.Core.Input.GamePadButtons.Square))
				base.Buttons.Square = ButtonState.Pressed;

			if (gamePadData.Buttons.HasFlag(Sce.Pss.Core.Input.GamePadButtons.Triangle))
				base.Buttons.Triangle = ButtonState.Pressed;

			if (gamePadData.Buttons.HasFlag(Sce.Pss.Core.Input.GamePadButtons.Circle))
				base.Buttons.Circle = ButtonState.Pressed;

			if (gamePadData.Buttons.HasFlag(Sce.Pss.Core.Input.GamePadButtons.Start))
				base.Buttons.Start = ButtonState.Pressed;

			if (gamePadData.Buttons.HasFlag(Sce.Pss.Core.Input.GamePadButtons.Select))
				base.Buttons.Select = ButtonState.Pressed;

			if (gamePadData.Buttons.HasFlag(Sce.Pss.Core.Input.GamePadButtons.Right))
				base.Buttons.RightShoulder = ButtonState.Pressed;

			if (gamePadData.Buttons.HasFlag(Sce.Pss.Core.Input.GamePadButtons.Left))
				base.Buttons.LeftShoulder = ButtonState.Pressed;

		}
	}
}

