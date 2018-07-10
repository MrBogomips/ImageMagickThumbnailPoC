using System;
using System.Drawing;

namespace ImageMagick {
    public static class MagickImageExtensions {
        // Not so useful
        public static void MakeThumbnailWithExif (this MagickImage image, Size size) {
            if (image == null) throw new ArgumentNullException (nameof (image));
            if (size == null) throw new ArgumentNullException (nameof (size));

            image.Resize (size.Width, size.Height);
        }
        public static void MakeThumbnailWithoutExif (this MagickImage image, Size size) {
            if (image == null) throw new ArgumentNullException (nameof (image));
            if (size == null) throw new ArgumentNullException (nameof (size));

            NormalizeImageWithExif (image);
            image.Resize (size.Width, size.Height);
        }

        /// <summary>
        /// Strip exif info and rotate/mirror the image accordingly.
        /// This image is suitable for viewers w/o EXIF capabilities.
        /// </summary>
        /// <param name="image">Image.</param>
        static void NormalizeImageWithExif (MagickImage image) {
            var exif = image.GetExifProfile ();
            if (exif == null) return;
            var orientationValue = exif.GetValue (ExifTag.Orientation).Value;
            if (orientationValue == null) return;
            var orientation = (ushort) orientationValue;
            switch (orientation) {
                case 1: // Horizontal (normal)
                    break; // nothing to do
                case 2: // Mirror orizontal
                    image.Flop ();
                    break;
                case 3: // Rotate 180
                    image.Rotate (180);
                    break;
                case 4: // Mirror vertical
                    image.Flip ();
                    break;
                case 5: // Mirror horizontal and rotate 270 CW
                    image.Flop ();
                    image.Rotate (270);
                    break;
                case 6: // Rotate 90 CW
                    image.Rotate (90);
                    break;
                case 7: // Mirror horizontal and rotate 90 CW
                    image.Flop ();
                    image.Rotate (90);
                    break;
                case 8: // Rotate 270 CW
                    image.Rotate (270);
                    break;
                default:
                    throw new InvalidOperationException ("Unexpected EXIF orientation bitpattern " + orientation);
            }
            image.RemoveProfile ("EXIF");
        }
    }
}
