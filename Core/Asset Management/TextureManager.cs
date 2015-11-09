using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core.Game_Objects.Behaviours;

namespace Leda.Core.Asset_Management
{
    public sealed class TextureManager
    {
        private static Dictionary<string, Texture2D> _textures = null;
        private static Dictionary<string, ITextureManaged> _textureManagedObjects = null;

        public static Dictionary<string, Texture2D> Textures { get { return _textures; } }

        public static void AddTexture(string newReference, Texture2D newTexture)
        {
            if (_textures == null) { _textures = new Dictionary<string, Texture2D>(); }

            if (!_textures.ContainsKey(newReference)) { _textures.Add(newReference, newTexture); }
            else { _textures[newReference] = newTexture; }
        }

        public static void AddManagedObject(ITextureManaged toAdd)
        {
            if (_textureManagedObjects == null) { _textureManagedObjects = new Dictionary<string, ITextureManaged>(); }

            if (!_textureManagedObjects.ContainsKey(toAdd.ID)) { _textureManagedObjects.Add(toAdd.ID, toAdd); }
            else { _textureManagedObjects[toAdd.ID] = toAdd; }
        }

        public static void ResaturateObjectTextures()
        {
            if ((_textureManagedObjects != null) && (_textureManagedObjects.Count > 0))
            {
                foreach (KeyValuePair<string, ITextureManaged> kvp in _textureManagedObjects)
                {
                    if ((!string.IsNullOrEmpty(kvp.Value.TextureReference)) && (_textures.ContainsKey(kvp.Value.TextureReference)))
                    {
                        kvp.Value.Texture = _textures[kvp.Value.TextureReference];
                    }
                }
            }
        }
    }
}
