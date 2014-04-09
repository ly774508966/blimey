using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;

namespace Cor
{
    /// <summary>
    /// Takes an bitmap resource and turns it into a Texture asset.
    /// </summary>
    public class TextureAssetBuilder
        : AssetBuilder <ColourmapResource, TextureAsset>
    {
        public override
        AssetBuilderOutput<TextureAsset>
        Process (AssetBuilderInput <ColourmapResource> resource)
        {
            throw new NotImplementedException ();
        }
    }
}
