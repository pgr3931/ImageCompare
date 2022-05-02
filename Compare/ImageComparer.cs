using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ImageCompare
{
    public class ImageComparer
    {
        public static void Compare(string root, ICollection<Tuple<string, string>> images, Action<int> setSearchedImages)
        {
            var lockObject = new object();
            var hashes = new Dictionary<string, string>();
            var count = 0;

            TraverseTreeParallelForEach(root, (f) =>
            {
                if (!f.ToLower().Contains("mp4") && !f.ToLower().Contains("webm"))
                {
                    if (f != null)
                    {
                        var hash = GetHash(f);
                        if (hash != null)
                        {
                            lock (lockObject)
                            {
                                if (hashes.ContainsKey(hash))
                                {
                                    Application.Current.Dispatcher.BeginInvoke(images.Add, Tuple.Create(f, hashes[hash]));
                                }
                                else
                                {
                                    hashes.Add(hash, f);
                                }
                                count++;
                                Application.Current.Dispatcher.BeginInvoke(setSearchedImages, count);
                            }
                        }
                    }
                }
            });

            //Parallel.ForEach(files, (file, _, i) =>
            //{
            //    if (file != null)
            //    {
            //        var hash = GetHash(file);
            //        if (hash != null)
            //        {
            //            lock (lockObject)
            //            {
            //                if (hashes.ContainsKey(hash))
            //                {
            //                    Application.Current.Dispatcher.BeginInvoke(images.Add, Tuple.Create(file, hashes[hash]));
            //                }
            //                else
            //                {
            //                    hashes.Add(hash, file);
            //                }
            //                count++;
            //                Application.Current.Dispatcher.BeginInvoke(setSearchedImages, count);
            //            }
            //        }
            //    }
            //});
        }

        private static string GetHash(string source)
        {
            //create new image with 16x16 pixel
            var bmp = new Bitmap(Image.FromFile(source).GetThumbnailImage(32, 32, null, IntPtr.Zero));
            LockBitmap bmpMin = new(bmp);
            List<int> pixels = new();

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
                    pixels.Add(pixel.GetBrightness() < 0.5f ? 1 : 0);
                    r += pixel.R;
                    g += pixel.G;
                    b += pixel.B;
                    total++;
                }
            }
            bmpMin.UnlockBits();

            bmpMin = null;
            bmp = null;

            r /= total;
            g /= total;
            b /= total;
            pixels.Add(r + g + b);

            return string.Join(",", pixels);
        }

        public static void TraverseFilesParallelForEach(List<string> files, Action<string> action)
        {
            while (files.Count > 0)
            {
                // Execute in parallel if there are enough files in the directory.
                // Otherwise, execute sequentially.Files are opened and processed
                // synchronously but this could be modified to perform async I/O.
                try
                {
                    Parallel.ForEach(files, file => action(file));
                }
                catch (AggregateException ae)
                {
                    ae.Handle((ex) => true);
                }
            }
        }

        public static void TraverseTreeParallelForEach(string root, Action<string> action)
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

                // Execute in parallel if there are enough files in the directory.
                // Otherwise, execute sequentially.Files are opened and processed
                // synchronously but this could be modified to perform async I/O.
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
