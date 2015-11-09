using System.Xml;
using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core.Asset_Management;

using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Interface;
using Bopscotch.Interface.Content;

namespace Bopscotch.Scenes.NonGame
{
    public class CreditsScene : ContentSceneWithBackDialog
    {
        public CreditsScene()
            : base()
        {
            BackgroundTextureName = Background_Texture_Name;
            _contentFileName = Credits_Content_Elements_File;
        }

        private const string Background_Texture_Name = "background-4";
        private const string Credits_Content_Elements_File = "Content/Files/Content/{0}/credits.xml";
    }
}
