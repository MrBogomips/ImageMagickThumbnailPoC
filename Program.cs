﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ImageMagick;

namespace images_poc {

    class Program {
        static readonly string IPhoneImagesPath;
        static readonly string ThumbnailsWithExif;
        static readonly string ThumbnailsWithoutExif;

        static Program () {
            const string IPhoneImagesRelPath = "Images/iPhone";
            const string ThumbnailsRelWithExif = "Thumbnails/WithExif";
            const string ThumbnailsRelWithoutExif = "Thumbnails/WithoutExif";
            var wd = Directory.GetCurrentDirectory ();
            Func<string, string> getAbsDir = (string relDir) => Path.Combine (wd, relDir);

            IPhoneImagesPath = getAbsDir (IPhoneImagesRelPath);
            ThumbnailsWithExif = getAbsDir (ThumbnailsRelWithExif);
            ThumbnailsWithoutExif = getAbsDir (ThumbnailsRelWithoutExif);
        }

        static void Main (string[] args) {
            MakeThumbnailsWithExifDir (IPhoneImagesPath, ThumbnailsWithExif, new Size (300, 400));
            MakeThumbnailsWithoutExifDir (IPhoneImagesPath, ThumbnailsWithoutExif, new Size (300, 400));
            DumpDirectoryImage (IPhoneImagesPath);
            DumpDirectoryImage (ThumbnailsWithExif);
            DumpDirectoryImage (ThumbnailsWithoutExif);
        }

        static void DumpDirectoryImage (string path) {
            Console.WriteLine ("Directory: {0}", path);
            foreach (var f in Directory.GetFiles (path)) {
                DumpImageInfo (f);
            }
        }

        static void DumpImageInfo (string path) {
            try {
                using (MagickImage image = new MagickImage (path)) {
                    var exif = image.GetExifProfile ();
                    if (exif == null) {
                        Console.WriteLine ("File: {0} NO EXIF DATA", path);
                    } else {
                        var orientation = exif.GetValue (ExifTag.Orientation);

                        Console.WriteLine ("File: {0}\n\tOrientation: {1}", path, orientation);
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine ("File: {0}\n\tError: {1}", path, ex);
            }
        }

        static void MakeThumbnailsWithExifDir (string srcDir, string destDir, Size size) {
            foreach (var srcFile in Directory.GetFiles (srcDir)) {
                var fileName = Path.GetFileName (srcFile);
                var destFile = Path.Combine (destDir, fileName);
                MakeThumbnailWithExif (srcFile, destFile, size);
            }
        }

        static void MakeThumbnailsWithoutExifDir (string srcDir, string destDir, Size size) {
            foreach (var srcFile in Directory.GetFiles (srcDir)) {
                var fileName = Path.GetFileName (srcFile);
                var destFile = Path.Combine (destDir, fileName);
                MakeThumbnailWithoutExif (srcFile, destFile, size);
            }
        }

        static void MakeThumbnailWithExif (string srcFile, string destFile, Size size) {
            using (MagickImage image = new MagickImage (srcFile)) {
                image.MakeThumbnailWithExif (size);
                image.Write (GetThumbnailFileName (destFile, size));
            }
        }

        static void MakeThumbnailWithoutExif (string srcFile, string destFile, Size size) {
            using (MagickImage image = new MagickImage (srcFile)) {
                image.MakeThumbnailWithoutExif (size);
                image.Write (GetThumbnailFileName (destFile, size));
            }
        }

        static string GetThumbnailFileName (string fileName, Size size) {
            var directory = Path.GetDirectoryName (fileName);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension (fileName);
            var extension = Path.GetExtension (fileName);
            var destFileName = string.Format ("{0} {1}x{2}.{3}", fileNameWithoutExt, size.Width, size.Height, extension);
            return Path.Combine (directory, destFileName);
        }
    }
}