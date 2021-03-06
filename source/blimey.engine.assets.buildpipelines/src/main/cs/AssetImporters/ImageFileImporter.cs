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

using System;
using System.IO;
using System.Runtime.InteropServices;
using Fudge;
using Blimey.Platform;
using Blimey.Asset;
using Hjg.Pngcs;
using System.IO;

namespace Blimey.Engine
{
    public class ImageFileImporter
        : AssetImporter <ColourmapAsset>
    {
        public override String [] SupportedSourceFileExtensions
        {
            get { return new [] { "png", "tga" }; }
        }

        static Rgba32[,] ReadPNG (String filename)
        {
            PngReader pngr = FileHelper.CreatePngReader(filename);
            Console.WriteLine(pngr.ToString()); // just information
            var pixmap = new Rgba32[pngr.ImgInfo.Cols, pngr.ImgInfo.Rows];

            for (int row = 0; row < pngr.ImgInfo.Rows; row++)
            {
                ImageLine line = pngr.ReadRowInt(row); // format: RGBRGB... or RGBARGBA...

                for (int col = 0; col < pngr.ImgInfo.Cols;col++)
                {
                    if (pngr.ImgInfo.Indexed)
                        throw new NotSupportedException ("Indexed PNG files are not yet supported.");

                    if (pngr.ImgInfo.Channels != 4)
                        throw new NotSupportedException ("Only PNG files with 4 channels are currently supported.");

                    Int32 pixelARGB = ImageLineHelper.GetPixelToARGB8 (line, col);

                    Byte b = (Byte)((pixelARGB) & 0xff);
                    Byte g = (Byte)((pixelARGB >> 8) & 0xff);
                    Byte r = (Byte)((pixelARGB >> 16) & 0xff);
                    Byte a = (Byte)((pixelARGB >> 24) & 0xff);

                    pixmap [col, row] = new Rgba32(r, g, b, a);
                }
            }

            pngr.End ();

            return pixmap;
        }

        static Rgba32[,] ReadTGA (String filename)
        {
            using (var stream = File.OpenRead (filename))
            {
                Int32 width;
                Int32 height;
                Byte[] data;

                DmitryBrant.ImageFormats.TgaReader.Load (stream, out width, out height, out data);

                Console.WriteLine (string.Format ("Targa info ~ width:{0}, height:{1}, data-length:{2}", width, height, data.LongLength));

                var pixmap = new Rgba32[width, height];

                for (int x = 0; x < width; ++x)
                {
                    for (int y = 0; y < height; ++y)
                    {
                        int i = ((x + (y * width)) * 4);
                        Byte b = data[i + 0];
                        Byte g = data[i + 1];
                        Byte r = data[i + 2];
                        Byte a = data[i + 3]; // 3 is defo A

                        pixmap [x, y] = new Rgba32 (r, g, b, a);
                    }
                }

                return pixmap;
            }
        }

		public override AssetImporterOutput <ColourmapAsset> Import (
			AssetImporterInput input, String platformId)
        {
            var output = new AssetImporterOutput <ColourmapAsset> ();
            var outputResource = new ColourmapAsset ();
            output.OutputAsset = outputResource;

            if (input.Files.Count != 1)
                throw new Exception ("ImageFileImporter only supports one input file.");

            if (!File.Exists (input.Files[0]))
                throw new Exception ("ImageFileImporter cannot find input file.");

            string filename = input.Files[0];

            if (Path.GetExtension (filename).ToLower () == ".png") outputResource.Data = ReadPNG (filename);
            if (Path.GetExtension (filename).ToLower () == ".tga") outputResource.Data = ReadTGA (filename);

            //Debug.DumpToPPM (output.Resource, input.Files[0] + "test.ppm");

            return output;
        }

    }
}
