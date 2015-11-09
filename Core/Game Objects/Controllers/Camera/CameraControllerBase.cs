using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Motion;

namespace Leda.Core.Game_Objects.Controllers.Camera
{
    public abstract class CameraControllerBase
    {
        private Vector2 _topLeftWorldPosition;
        private bool _bulkObjectsWithinRenderableAreaCheckInProgress;

        private float _renderableAreaLeft;
        private float _renderableAreaRight;
        private float _renderableAreaTop;
        private float _renderableAreaBottom;

        private Rectangle _viewport;
        private float _viewportLeft;
        private float _viewportRight;
        private float _viewportTop;
        private float _viewportBottom;
        private float _renderClippingMargin;

        protected List<ICameraLinked> _objectsOnCamera;
        protected List<ICameraRelativeWithOffCameraDispose> _objectsToDisposeOfOnceOffCamera;

        public Vector2 WorldPosition { set { SetCameraPosition(value); } get { return _topLeftWorldPosition; } }
        public virtual Rectangle Viewport { get { return _viewport; } set { _viewport = value; UpdateViewportExtremities(); } }
        public int RenderClippingMargin { set { _renderClippingMargin = value; UpdateViewportExtremities(); } }

        private void UpdateViewportExtremities()
        {
            _viewportLeft = _viewport.X - _renderClippingMargin;
            _viewportRight = _viewport.X + _viewport.Width + _renderClippingMargin;
            _viewportTop = _viewport.Y - _renderClippingMargin;
            _viewportBottom = _viewport.Y + _viewport.Height + _renderClippingMargin;
        }

        public CameraControllerBase()
        {
            _objectsOnCamera = new List<ICameraLinked>();
            _objectsToDisposeOfOnceOffCamera = new List<ICameraRelativeWithOffCameraDispose>();
            _topLeftWorldPosition = Vector2.Zero;

            RenderClippingMargin = 0;
        }

        public void AddCameraLinkedObject(ICameraLinked toAdd)
        {
            if (!_objectsOnCamera.Contains(toAdd)) { _objectsOnCamera.Add(toAdd); }
            if (toAdd is ICameraRelativeWithOffCameraDispose) { _objectsToDisposeOfOnceOffCamera.Add((ICameraRelativeWithOffCameraDispose)toAdd); }
        }

        public void RemoveCameraLinkedObject(ICameraLinked toRemove)
        {
            if (_objectsOnCamera.Contains(toRemove)) { _objectsOnCamera.Remove(toRemove); }

            ICameraRelativeWithOffCameraDispose disposable = toRemove as ICameraRelativeWithOffCameraDispose;
            if ((disposable != null) && (_objectsToDisposeOfOnceOffCamera.Contains(disposable))) { _objectsToDisposeOfOnceOffCamera.Remove(disposable); }
        }

        private void SetCameraPosition(Vector2 worldPosition)
        {
            for (int i = _objectsOnCamera.Count - 1; i >=0 ; i--) { _objectsOnCamera[i].CameraPosition = worldPosition; }

            HandleOffCameraDisposableObjectStateChanges();

            _topLeftWorldPosition = worldPosition;
        }

        private void HandleOffCameraDisposableObjectStateChanges()
        {
            Rectangle positionedViewport = new Rectangle(
                (int)WorldPosition.X + Viewport.X, 
                (int)WorldPosition.Y + Viewport.Y, 
                Viewport.Width,
                Viewport.Height);

            int objectCount = _objectsToDisposeOfOnceOffCamera.Count;
            for (int i = 0; i < objectCount; i++)
            {
                if (ObjectIsInScope(_objectsToDisposeOfOnceOffCamera[i], positionedViewport)) 
                { 
                    _objectsToDisposeOfOnceOffCamera[i].HasBeenInShot = true; 
                }
                else if (_objectsToDisposeOfOnceOffCamera[i].HasBeenInShot) 
                { 
                    _objectsToDisposeOfOnceOffCamera[i].ReadyForDisposal = true; 
                }
            }
        }

        private bool ObjectIsInScope(ICameraRelativeWithOffCameraDispose objectToCheck, Rectangle cameraViewport)
        {
            Rectangle inShotTester = new Rectangle(
                (int)objectToCheck.WorldPosition.X + objectToCheck.OutOfShotTolerance.X,
                (int)objectToCheck.WorldPosition.Y + objectToCheck.OutOfShotTolerance.Y,
                objectToCheck.OutOfShotTolerance.Width,
                objectToCheck.OutOfShotTolerance.Height);

            return ((cameraViewport.Intersects(inShotTester)) || (cameraViewport.Contains(inShotTester)));
        }

        public void BeginBulkObjectWithinRenderableAreaCheck()
        {
            UpdateRenderableAreaLimits();
            _bulkObjectsWithinRenderableAreaCheckInProgress = true;
        }

        private void UpdateRenderableAreaLimits()
        {
            _renderableAreaLeft = WorldPosition.X + _viewportLeft;
            _renderableAreaTop = WorldPosition.Y + _viewportTop;
            _renderableAreaRight = _renderableAreaLeft + _viewportRight;
            _renderableAreaBottom = _renderableAreaTop + _viewportBottom;
        }

        public bool ObjectIsWithinRenderableArea(ICameraRelative objectToCheck)
        {
            if (!_bulkObjectsWithinRenderableAreaCheckInProgress) { UpdateRenderableAreaLimits(); }

            if ((objectToCheck.WorldPosition.X < _renderableAreaLeft) || (objectToCheck.WorldPosition.X > _renderableAreaRight)) { return false; }
            if ((objectToCheck.WorldPosition.Y < _renderableAreaTop) || (objectToCheck.WorldPosition.Y > _renderableAreaBottom)) { return false; }

            return true;
        }

        public void EndBulkObjectWithinRenderableAreaCheck()
        {
            _bulkObjectsWithinRenderableAreaCheckInProgress = false;
        }
    }
}
