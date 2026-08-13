using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public static class TransparentLogo
{
    public static void MakeTransparent(string sourcePath, string destinationPath, byte threshold)
    {
        using (var source = new Bitmap(sourcePath))
        using (var bitmap = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb))
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.DrawImage(source, 0, 0, source.Width, source.Height);

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            try
            {
                int width = bitmap.Width;
                int height = bitmap.Height;
                int stride = data.Stride;
                int bytes = Math.Abs(stride) * height;
                var buffer = new byte[bytes];
                Marshal.Copy(data.Scan0, buffer, 0, bytes);

                var visited = new bool[width * height];
                var queue = new Queue<int>();

                void TryEnqueue(int x, int y)
                {
                    if (x < 0 || y < 0 || x >= width || y >= height)
                    {
                        return;
                    }

                    int index = y * width + x;
                    if (visited[index])
                    {
                        return;
                    }

                    int offset = y * stride + x * 4;
                    byte b = buffer[offset];
                    byte g = buffer[offset + 1];
                    byte r = buffer[offset + 2];
                    if (r >= threshold && g >= threshold && b >= threshold)
                    {
                        visited[index] = true;
                        queue.Enqueue(index);
                    }
                }

                for (int x = 0; x < width; x++)
                {
                    TryEnqueue(x, 0);
                    TryEnqueue(x, height - 1);
                }

                for (int y = 0; y < height; y++)
                {
                    TryEnqueue(0, y);
                    TryEnqueue(width - 1, y);
                }

                while (queue.Count > 0)
                {
                    int index = queue.Dequeue();
                    int x = index % width;
                    int y = index / width;
                    int offset = y * stride + x * 4;

                    buffer[offset + 3] = 0;

                    TryEnqueue(x + 1, y);
                    TryEnqueue(x - 1, y);
                    TryEnqueue(x, y + 1);
                    TryEnqueue(x, y - 1);
                }

                Marshal.Copy(buffer, 0, data.Scan0, bytes);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            bitmap.Save(destinationPath, ImageFormat.Png);
        }
    }
}
