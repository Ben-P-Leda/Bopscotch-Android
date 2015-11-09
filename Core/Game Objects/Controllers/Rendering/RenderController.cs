using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Game_Objects.Controllers.Camera;

namespace Leda.Core.Game_Objects.Controllers.Rendering
{
    public sealed class RenderController
    {
        private List<RenderLayer> _layers;
        private CameraControllerBase _cameraController;
        private List<IGameObject> _cameraIndependentObjects;

        private bool _useCameraController;
        private int _layerCount;
        private bool _someObjectsAreNotCameraLinked;

        public RenderController() : this(true) { }

        public RenderController(bool useDefaultLayerStructure)
        {
            _layers = new List<RenderLayer>();
            _cameraIndependentObjects = new List<IGameObject>();

            _cameraController = null;
            _useCameraController = false;
            _someObjectsAreNotCameraLinked = false;
            _layerCount = -1;

            if (useDefaultLayerStructure) { CreateDefaultLayerStructure(); }
        }

        private void CreateDefaultLayerStructure()
        {
            for (int i = 0; i < Default_Render_Layer_Count; i++)
            {
                if (Default_Additive_Render_Layers.IndexOf(i.ToString()) > -1) { _layers.Add(new RenderLayer(BlendState.Additive)); }
                else { _layers.Add(new RenderLayer(BlendState.AlphaBlend)); }
            }
        }

        public void ClipOffCameraRendering(CameraControllerBase cameraController, int margin)
        {
            _cameraController = cameraController;
            _cameraController.RenderClippingMargin = margin;
            _useCameraController = true;
        }

        public void AddRenderableObject(ISimpleRenderable toAdd)
        {
            if ((toAdd.RenderLayer > -1) && (toAdd.RenderLayer < _layers.Count) && (!_layers[toAdd.RenderLayer].SimpleObjects.Contains(toAdd)))
            {
                _layers[toAdd.RenderLayer].SimpleObjects.Add(toAdd);
                _layerCount = _layers.Count;
            }

            if ((!(toAdd is ICameraRelative)) && (!_cameraIndependentObjects.Contains(toAdd))) { _cameraIndependentObjects.Add(toAdd); }

            _someObjectsAreNotCameraLinked = (_cameraIndependentObjects.Count > 0);
        }

        public void RemoveRenderableObject(ISimpleRenderable toRemove)
        {
            if ((toRemove.RenderLayer > -1) && (toRemove.RenderLayer < _layers.Count) && (_layers[toRemove.RenderLayer].SimpleObjects.Contains(toRemove)))
            {
                _layers[toRemove.RenderLayer].SimpleObjects.Remove(toRemove);
            }

            if ((!(toRemove is ICameraRelative)) && (_cameraIndependentObjects.Contains(toRemove))) { _cameraIndependentObjects.Remove(toRemove); }

            _someObjectsAreNotCameraLinked = (_cameraIndependentObjects.Count > 0);
        }

        public void RenderObjects(SpriteBatch spriteBatch)
        {
            if (_useCameraController) { _cameraController.BeginBulkObjectWithinRenderableAreaCheck(); }

            for (int i = 0; i < _layerCount; i++)
            {
                int objectCount = _layers[i].SimpleObjects.Count;

                if (objectCount > 0)
                {
                    spriteBatch.Begin(SpriteSortMode.BackToFront, _layers[i].Blending);

                    for (int o = 0; o < objectCount; o++)
                    {
                        if (ObjectShouldBeRendered(_layers[i].SimpleObjects[o])) 
                        { 
                            _layers[i].SimpleObjects[o].Draw(spriteBatch); 
                        }
                    }

                    spriteBatch.End();
                }
            }

            if (_useCameraController) { _cameraController.EndBulkObjectWithinRenderableAreaCheck(); }
        }

        private bool ObjectShouldBeRendered(ISimpleRenderable toRender)
        {
            if (!toRender.Visible) { return false; }

            if (!_useCameraController) { return true; }
            if ((_someObjectsAreNotCameraLinked) && (_cameraIndependentObjects.Contains(toRender))) { return true; }

            return _cameraController.ObjectIsWithinRenderableArea((ICameraRelative)toRender);
        }

        private const int Default_Render_Layer_Count = 5;
        private const string Default_Additive_Render_Layers = "1,3";
    }
}
