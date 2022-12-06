using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace BeautyPortionAdmin.Extensions
{
    public static class ExtensionsOfByteArray
    {
        public static ImageSource ToImageSource(this byte[] bytes)
        {
            return bytes == null ? null : ImageSource.FromStream(() => new MemoryStream(bytes));
        }

        public static byte[] ResizeImage(this byte[] bytes, int widthRequest = 480, int quality = 90)
        {
            using var skStream = new SKMemoryStream(bytes);
            using var codec = SKCodec.Create(skStream);
            var info = codec.Info;

            //get the scale that is nearest to what we want(eg: jpg returned 512)
            var supportedScale = codec.GetScaledDimensions((float)widthRequest / info.Width);

            //decode the bitmap at nearest size
            var nearest = new SKImageInfo(supportedScale.Width, supportedScale.Height);

            using var bmp = SKBitmap.Decode(codec, nearest);
            var realScale = (float)info.Height / info.Width;
            var desired = new SKImageInfo(widthRequest, (int)(realScale * widthRequest));

            var resizedBitmap = bmp.Resize(desired, SKFilterQuality.Medium);

            resizedBitmap = HandleOrientation(resizedBitmap, codec.EncodedOrigin);

            var image = SKImage.FromBitmap(resizedBitmap);
            var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);

            return data.ToArray();
        }

        private static SKBitmap HandleOrientation(SKBitmap resizedBitmap, SKEncodedOrigin orientation)
        {
            SKBitmap rotated;
            switch (orientation)
            {
                case SKEncodedOrigin.BottomRight:
                    using (var surface = new SKCanvas(resizedBitmap))
                    {
                        surface.RotateDegrees(180, resizedBitmap.Width / 2, resizedBitmap.Height / 2);
                        surface.DrawBitmap(resizedBitmap.Copy(), 0, 0);
                    }
                    return resizedBitmap;

                case SKEncodedOrigin.RightTop:
                    rotated = new SKBitmap(resizedBitmap.Height, resizedBitmap.Width);

                    using (var surface = new SKCanvas(rotated))
                    {
                        surface.Translate(rotated.Width, 0);
                        surface.RotateDegrees(90);
                        surface.DrawBitmap(resizedBitmap, 0, 0);
                    }

                    return rotated;

                case SKEncodedOrigin.LeftBottom:
                    rotated = new SKBitmap(resizedBitmap.Height, resizedBitmap.Width);

                    using (var surface = new SKCanvas(rotated))
                    {
                        surface.Translate(0, rotated.Height);
                        surface.RotateDegrees(270);
                        surface.DrawBitmap(resizedBitmap, 0, 0);
                    }

                    return rotated;
                default:
                    return resizedBitmap;
            }
        }
    }
}
