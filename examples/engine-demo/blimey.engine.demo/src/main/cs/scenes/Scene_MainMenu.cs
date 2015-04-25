// ┌────────────────────────────────────────────────────────────────────────┐ \\
// │ __________.__  .__                                                     │ \\
// │ \______   \  | |__| _____   ____ ___.__.                               │ \\
// │  |    |  _/  | |  |/     \_/ __ <   |  |                               │ \\
// │  |    |   \  |_|  |  Y Y  \  ___/\___  |                               │ \\
// │  |______  /____/__|__|_|  /\___  > ____|                               │ \\
// │         \/              \/     \/\/                                    │ \\
// │                                                                        │ \\
// ├────────────────────────────────────────────────────────────────────────┤ \\
// │ Copyright © 2012 - 2015 ~ Blimey Engine (http://www.blimey.io)         │ \\
// ├────────────────────────────────────────────────────────────────────────┤ \\
// │ Authors:                                                               │ \\
// │ ~ Ash Pook (http://www.ajpook.com)                                     │ \\
// ├────────────────────────────────────────────────────────────────────────┤ \\
// │ Permission is hereby granted, free of charge, to any person obtaining  │ \\
// │ a copy of this software and associated documentation files (the        │ \\
// │ "Software"), to deal in the Software without restriction, including    │ \\
// │ without limitation the rights to use, copy, modify, merge, publish,    │ \\
// │ distribute, sublicense, and/or sellcopies of the Software, and to      │ \\
// │ permit persons to whom the Software is furnished to do so, subject to  │ \\
// │ the following conditions:                                              │ \\
// │                                                                        │ \\
// │ The above copyright notice and this permission notice shall be         │ \\
// │ included in all copies or substantial portions of the Software.        │ \\
// │                                                                        │ \\
// │ THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,        │ \\
// │ EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF     │ \\
// │ MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. │ \\
// │ IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY   │ \\
// │ CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,   │ \\
// │ TORT OR OTHERWISE, ARISING FROM,OUT OF OR IN CONNECTION WITH THE       │ \\
// │ SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                 │ \\
// └────────────────────────────────────────────────────────────────────────┘ \\

namespace EngineDemo
{
    using System;
    using Fudge;
    using Abacus.SinglePrecision;
    using Blimey.Platform;
    using Blimey.Asset;
    using Blimey.Engine;
    using System.Collections.Generic;

    // ────────────────────────────────────────────────────────────────────────────────────────────────────────────── //

    public class Act_MainMenu: Act
    {
        const Single scaleBig = 0.075f;
        const Single scaleSmall = 0.05f;

        const Single _totalTime = 2f;
        Single _timer = _totalTime;

        const Single _doneSomethingTime = 0.2f;
        Single _inputTimer = _doneSomethingTime;

		Rgba32 _startCol = Rgba32.LightYellow;
		Rgba32 _endCol = Rgba32.LightBlue;

        static Int32 _selectedIndex = 0;

        List<Material> _menuItemMaterials = new List<Material>();
        List<Entity> _menuActObjects = new List<Entity>();

        Act _returnAct;

        Triple q;

        Texture tex = null;

        public override void Start ()
        {
            var ta = Engine.Assets.Load <TextureAsset> ("assets/blimey_fnt_tex.bba");
            tex = Platform.Graphics.CreateTexture (ta);
            q = new Triple ();
            q.blend = BlendMode.Default;
            q.tex = tex;
            q.v [0].Colour = Rgba32.Blue;
            q.v [0].Position.X = -0.5f;
            q.v [0].Position.Y = 0f;
            q.v [0].UV = new Vector2 (0, 1);
            q.v [1].Colour = Rgba32.Green;
            q.v [1].Position.X = 0f;
            q.v [1].Position.Y = 0.5f;
            q.v [1].UV = new Vector2 (1, 0);
            q.v [2].Colour = Rgba32.Red;
            q.v [2].Position.X = -0.5f;
            q.v [2].Position.Y = 0.5f;
            q.v [2].UV = new Vector2 (0, 0);

            _returnAct = this;

            CommonDemoResources.Create (Platform, Engine);

			this.Configuration.BackgroundColour = _startCol;
            var teaPotModel = new TeapotPrimitive(this.Platform.Graphics);

            var so1 = RandomObjectHelper.CreateShapeGO(this, "Gui", teaPotModel.Mesh, 1);
            var so2 = RandomObjectHelper.CreateShapeGO(this, "Gui", teaPotModel.Mesh, 1);
            var so3 = RandomObjectHelper.CreateShapeGO(this, "Gui", teaPotModel.Mesh, 1);
            var so4 = RandomObjectHelper.CreateShapeGO(this, "Gui", teaPotModel.Mesh, 1);
            var so5 = RandomObjectHelper.CreateShapeGO(this, "Gui", teaPotModel.Mesh, 1);
            var so6 = RandomObjectHelper.CreateShapeGO(this, "Gui", teaPotModel.Mesh, 1);
            var so7 = RandomObjectHelper.CreateShapeGO(this, "Gui", teaPotModel.Mesh, 1);
            var so8 = RandomObjectHelper.CreateShapeGO(this, "Gui", teaPotModel.Mesh, 1);

            so1.Transform.LocalPosition = new Vector3(-0.35f, 0f, 0f);
			so2.Transform.LocalPosition = new Vector3(-0.25f, 0f, 0f);
			so3.Transform.LocalPosition = new Vector3(-0.15f, 0f, 0f);
			so4.Transform.LocalPosition = new Vector3(-0.05f, 0f, 0f);
			so5.Transform.LocalPosition = new Vector3(+0.05f, 0f, 0f);
			so6.Transform.LocalPosition = new Vector3(+0.15f, 0f, 0f);
			so7.Transform.LocalPosition = new Vector3(+0.25f, 0f, 0f);
            so8.Transform.LocalPosition = new Vector3(+0.35f, 0f, 0f);


			_menuItemMaterials.Add(so1.GetTrait<MeshRendererTrait>().Material);
			_menuItemMaterials.Add(so2.GetTrait<MeshRendererTrait>().Material);
			_menuItemMaterials.Add(so3.GetTrait<MeshRendererTrait>().Material);
			_menuItemMaterials.Add(so4.GetTrait<MeshRendererTrait>().Material);
			_menuItemMaterials.Add(so5.GetTrait<MeshRendererTrait>().Material);
			_menuItemMaterials.Add(so6.GetTrait<MeshRendererTrait>().Material);
            _menuItemMaterials.Add(so7.GetTrait<MeshRendererTrait>().Material);
            _menuItemMaterials.Add(so8.GetTrait<MeshRendererTrait>().Material);

			_menuActObjects.Add(so1);
			_menuActObjects.Add(so2);
			_menuActObjects.Add(so3);
			_menuActObjects.Add(so4);
			_menuActObjects.Add(so5);
			_menuActObjects.Add(so6);
            _menuActObjects.Add(so7);
            _menuActObjects.Add(so8);

            this.Engine.InputEventSystem.Tap += this.OnTap;
            this.Engine.InputEventSystem.Flick += this.OnFlick;

        }

        public override void Shutdown ()
        {
            _menuActObjects.Clear();
            _menuActObjects = null;
            _menuItemMaterials = null;
            this.Engine.InputEventSystem.Tap -= this.OnTap;
            this.Engine.InputEventSystem.Flick -= this.OnFlick;
            this.Platform.Graphics.DestroyTexture (q.tex);
            tex.Dispose ();
            tex = null;
            CommonDemoResources.Destroy ();
        }

        void OnFlick(Gesture gesture)
        {
            var v = gesture.TouchTrackers[0].GetVelocity(TouchPositionSpace.NormalisedEngine);

            if (v.X > 0)
            {
                IncreaseSelected();
            }
            else
            {
                DecreaseSelected();
            }
        }

        void OnTap(Gesture gesture)
        {
            _returnAct = GetActForCurrentSelection();
        }

        void IncreaseSelected()
        {
            _selectedIndex++;
            _selectedIndex = MathsUtils.Clamp(_selectedIndex, 0, _menuActObjects.Count - 1);
        }

        void DecreaseSelected()
        {
            _selectedIndex--;
            _selectedIndex = MathsUtils.Clamp(_selectedIndex, 0, _menuActObjects.Count - 1);
        }

        Act CheckForMenuInput()
        {
            if (_inputTimer == 0f)
            {
				if (Platform.Input.GenericGamepad.DPad.Down == ButtonState.Pressed ||
                    Platform.Input.Keyboard.IsFunctionalKeyDown(FunctionalKey.Left))
                {
                    this.DecreaseSelected();
                    _inputTimer = _doneSomethingTime;
                }

				if (Platform.Input.GenericGamepad.DPad.Right == ButtonState.Pressed ||
                    Platform.Input.Keyboard.IsFunctionalKeyDown(FunctionalKey.Right))
                {
                    this.IncreaseSelected();
                    _inputTimer = _doneSomethingTime;

                }

				if (Platform.Input.GenericGamepad.Buttons.South == ButtonState.Pressed ||
                    Platform.Input.Keyboard.IsFunctionalKeyDown(FunctionalKey.Enter))
                {
                    return GetActForCurrentSelection();
                }
            }

            return _returnAct;

        }

        Act GetActForCurrentSelection()
        {
            if (_selectedIndex == 0)
                return new Act_Darius ();

            if (_selectedIndex == 1)
                return new Act_Airports ();

            if (_selectedIndex == 2)
                return new Act_Sprites ();

            if (_selectedIndex == 3)
				return new Act_Particles ();

			if (_selectedIndex == 4)
				return new Act_Text ();

			if (_selectedIndex == 5)
				return new Act_Boids ();

            if (_selectedIndex == 6)
                return new Act_Mushrooms ();

            if (_selectedIndex == 7)
                return new Act_Parallax ();

            return this;
        }

        public override Act Update (AppTime time)
        {
            this.Engine.DebugRenderer.AddGrid ("Debug");

            var menuResult = this.CheckForMenuInput();

            this.Engine.PrimitiveRenderer.AddTriple ("Gui", q);

            if (menuResult != this)
                return menuResult;

            for (int i = 0; i < _menuActObjects.Count; ++i)
            {
                if( i == _selectedIndex )
                {
                    _menuActObjects[i].Transform.LocalScale = new Vector3(scaleBig, scaleBig, scaleBig);
                }
                else
                {
                    _menuActObjects[i].Transform.LocalScale = new Vector3(scaleSmall, scaleSmall, scaleSmall);
                }
            }

            _timer -= time.Delta; if (_timer <= 0f) _timer = 0f;
            _inputTimer -= time.Delta; if (_inputTimer <= 0f) _inputTimer = 0f;

			Rgba32 c = Rgba32.Lerp(_startCol, _endCol, (Maths.Sin(time.Elapsed) / 2f) + 0.5f);
			this.RuntimeConfiguration.ChangeBackgroundColour (c);

            return this;
        }
    }
}

