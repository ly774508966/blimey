using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sungiant.Abacus;

namespace Sungiant.Blimey.PsmRuntime
{
	public class InputManager
		: IInputManager
	{
		public Xbox360Controller GetXbox360Controller(PlayerIndex player) { return null; }
		public MultiTouchController GetTouchScreen() { return _vitaTouchScreen; }
		public VitaController GetVitaController() { return _controls; }
		public GenericGamepad GetGenericGamepad() { return _genericPad; }
		
		
		TouchScreen _vitaTouchScreen;
		VitaControllerImplementation _controls;
		GenericGamepad _genericPad;
		
		public InputManager(IEngine engine, TouchScreen screen)
		{
			_controls = new VitaControllerImplementation();
			_genericPad = new GenericGamepad(this);
			_vitaTouchScreen = screen;
		}
	
		public void Update(GameTime time)
		{
			_vitaTouchScreen.Update(time);
			_controls.Update(time);
			_genericPad.Update(time);
		}
	}
}
