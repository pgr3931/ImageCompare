using ImageCompare.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ImageCompare
{
    public static class Extensions
    {
        public static bool ContainsPair(this List<Tuple<string, string>> tuples, string key, string value)
        {
            return tuples.Find(t => (t.Item1 == key && t.Item2 == value) || (t.Item1 == value && t.Item2 == key)) != null;
        }
    }

    public class ImageComparer
    {
        public static void Compare(string root, string savePath, string delimiter, ICollection<Tuple<string, string>> images, Action<int> setSearchedImages)
        {
            var skippedImages = IOManager.Load(savePath, delimiter);
            var imageExtensions = new List<string> { "jpg", "jpeg", "png", "jfif", "webp" };
            var hashes = new ConcurrentDictionary<string, string>();
            var count = 0;

            TraverseTreeParallelForEach(root, (f) =>
            {
                if (f != null && imageExtensions.Contains(f[(f.LastIndexOf(".") + 1)..].ToLower()))
                {
                    var hash = GetHash(f);
                    if (hash != null)
                    {
                        var added = hashes.TryAdd(hash, f);
                        if (!added)
                        {
                            var duplicate = hashes[hash];

                            var skippedImage = skippedImages != null && (skippedImages.ContainsPair(f, duplicate) || skippedImages.ContainsPair(duplicate, f));

                            if (!skippedImage)
                            {
                                Application.Current.Dispatcher.BeginInvoke(images.Add, Tuple.Create(f, duplicate));
                            }
                        }
                       
                        Interlocked.Increment(ref count);
                        Application.Current.Dispatcher.BeginInvoke(setSearchedImages, count);
                    }
                }
            });
        }

        private static string? GetHash(string source)
        {
            try
            {
                var width = 32;
                var height = 32;
                //create new image with 16x16 pixel
                var bmp = new Bitmap(Image.FromFile(source).GetThumbnailImage(width, height, null, IntPtr.Zero));
                LockBitmap bmpMin = new(bmp);
                var pixels = new int[width * height + 3];

                int r = 0;
                int g = 0;
                int b = 0;
                int total = 0;

                bmpMin.LockBits();
                for (int j = 0; j < bmpMin.Height; j++)
                {
                    for (int i = 0; i < bmpMin.Width; i++)
                    {
                        //reduce colors to true / false
                        var pixel = bmpMin.GetPixel(i, j);
                        pixels[j * i] = pixel.GetBrightness() < 0.5f ? 1 : 0;
                        r += pixel.R;
                        g += pixel.G;
                        b += pixel.B;
                        total++;
                    }
                }
                bmpMin.UnlockBits();

                pixels[width * height] = r / total;
                pixels[width * height + 1] = g / total;
                pixels[width * height + 2] = b / total;

                return string.Join(",", pixels);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void TraverseTreeParallelForEach(string root, Action<string> action)
        {
            // Data structure to hold names of subfolders to be examined for files.
            Stack<string> dirs = new();

            dirs.Push(root);

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs = Array.Empty<string>();
                string[] files = Array.Empty<string>();

                try
                {
                    subDirs = Directory.GetDirectories(currentDir);
                }
                catch (Exception)
                {
                    continue;
                }

                try
                {
                    files = Directory.GetFiles(currentDir);
                }
                catch (Exception)
                {
                    continue;
                }

                try
                {
                    Parallel.ForEach(files, file => action(file));
                }
                catch (AggregateException ae)
                {
                    ae.Handle((ex) => true);
                }

                // Push the subdirectories onto the stack for traversal.
                // This could also be done before handing the files.
                foreach (string str in subDirs)
                    dirs.Push(str);
            }
        }
    }
}
