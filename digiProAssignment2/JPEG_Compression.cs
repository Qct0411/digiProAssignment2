using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace digiProAssignment2
{
    public class JPEG_Compression
    {
        private readonly static double[,] YCrCbtoRGB = {
            { 1, 0, 1.4 },
            { 1, -0.343, -0.711 },
            { 1, 1.765, 0 }
        };
        public static byte[] convertRGBToYCbCr(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            byte[] result = new byte[(int)(width * height * 1.5F + 4)]; // 4 = byte for width and height, 6 = bytes for indices

            double[,] Y = new double[width, height];
            double[,] Cb = new double[width, height];
            double[,] Cr = new double[width, height];
            
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color c = bitmap.GetPixel(i, j);
                    double r = c.R;
                    double g = c.G;
                    double b = c.B;
                    Y[i,j] = 0.299 * r + 0.587 * g + 0.114 * b;
                    Cb[i,j] = - 0.168736 * r - 0.331264 * g + 0.5 * b + 128;
                    Cr[i,j] = 0.5 * r - 0.418688 * g - 0.081312 * b + 128;
                }
            }

            Cb = subsample(Cb);
            Cr = subsample(Cr);
            int index = 0;
            result[index++] = (byte)(width >> 8);     // Store the most significant byte of width
            result[index++] = (byte)(width & 0xFF);   // Store the least significant byte of width
            result[index++] = (byte)(height >> 8);    // Store the most significant byte of height
            result[index++] = (byte)(height & 0xFF);  // Store the least significant byte of height
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    result[index++] = (byte)Y[j, i];
                }
            }

            for (int i = 0; i < height / 2; i++)
            {
                for (int j = 0; j < width / 2; j++)
                {
                    result[index++] = (byte)Cb[j, i];
                }
            }

            for (int i = 0; i < height / 2; i++)
            {
                for (int j = 0; j < width / 2; j++)
                {
                    result[index++] = (byte)Cr[j, i];
                }
            }

            return result;
        }

        public static double[,] subsample(double[,] matrix)
        {
            int width = matrix.GetLength(0);
            int height = matrix.GetLength(1);
            double[,] result = new double[width / 2, height / 2];

            for (int i = 0; i < height / 2; i++)
            {
                for (int j = 0; j < width / 2; j++)
                {
                    result[j, i] = matrix[j * 2, i * 2];
                }
            }

            return result;
        }

        public static Bitmap convertYCbCrToRGB(byte[] data)
        {
            int width = (data[0] << 8) | data[1];
            Debug.Write("actual Width:" + width);
            int height = (data[2] << 8) | data[3];
            Debug.Write("actual Height:" +  height);
            Bitmap result = new Bitmap(width, height);

            double[,] Y = new double[width, height];
            double[,] Cb = new double[width / 2, height / 2];
            double[,] Cr = new double[width / 2, height / 2];

            int index = 4;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Y[j, i] = data[index++];
                }
            }
            for (int i = 0; i < height / 2; i++)
            {
                for (int j = 0; j < width / 2; j++)
                {
                    Cb[j, i] = data[index++];
                }
            }
            for (int i = 0; i < height / 2; i++)
            {
                for (int j = 0; j < width / 2; j++)
                {
                    Cr[j, i] = data[index++];
                }
            }

            Cb = upsample(Cb);
            Cr = upsample(Cr);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int r = (int)(Y[i, j] + 1.402 * (Cr[i, j] - 128));
                    int g = (int)(Y[i, j] - 0.343 * (Cb[i, j] - 128) - 0.711 * (Cr[i, j] - 128));
                    int b = (int)(Y[i, j] + 1.765 * (Cb[i, j] - 128));

                    r = Math.Max(0, Math.Min(255, r));
                    g = Math.Max(0, Math.Min(255, g));
                    b = Math.Max(0, Math.Min(255, b));
                    //Debug.WriteLine(r + " " + g + " " + b);

                    result.SetPixel(i, j, Color.FromArgb(r, g, b));
                }
            }

            return result;
        }

        public static Bitmap convertYCbCrToRGBv2(List<double> ycrcb, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);

            double[,] Y = new double[width, height];
            double[,] Cb = new double[width / 2, height / 2];
            double[,] Cr = new double[width / 2, height / 2];

            int i = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Y[x, y] = ycrcb[i++];
                }
            }
            for (int y = 0; y < height / 2; y++)
            {
                for (int x = 0; x < width / 2; x++)
                {
                    Cb[x, y] = ycrcb[i++];
                }
            }
            for (int y = 0; y < height / 2; y++)
            {
                for (int x = 0; x < width / 2; x++)
                {
                    Cr[x, y] = ycrcb[i++];
                }
            }

            Cb = upsample(Cb);
            Cr = upsample(Cr);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {

                    double yValue = Y[x, y];
                    double cbValue = Cb[x, y] - 128;
                    double crValue = Cr[x, y] - 128;

                    double r = YCrCbtoRGB[0, 0] * yValue +
                       YCrCbtoRGB[0, 1] * cbValue +
                       YCrCbtoRGB[0, 2] * crValue;

                    double g = YCrCbtoRGB[1, 0] * yValue +
                               YCrCbtoRGB[1, 1] * cbValue +
                               YCrCbtoRGB[1, 2] * crValue;

                    double b = YCrCbtoRGB[2, 0] * yValue +
                               YCrCbtoRGB[2, 1] * cbValue +
                               YCrCbtoRGB[2, 2] * crValue;

                    r = Math.Max(0, Math.Min(255, r));
                    g = Math.Max(0, Math.Min(255, g));
                    b = Math.Max(0, Math.Min(255, b));
                    //Debug.WriteLine(r + " " + g + " " + b);

                    result.SetPixel(x, y, Color.FromArgb((int)r, (int)g, (int)b));
                }
            }

            return result;
        }

        private static double[,] upsample(double[,] matrix)
        {
            int width = matrix.GetLength(0);
            int height = matrix.GetLength(1);
            double[,] result = new double[width * 2, height * 2];

            int temp1 = 0;
            for (int i = 0; i < height; i++)
            {
                int temp2 = 0;
                for (int j = 0; j < width; j++)
                {
                    result[temp2, temp1] = matrix[j,i];
                    result[temp2 + 1, temp1] = matrix[j, i];
                    result[temp2, temp1 + 1] = matrix[j, i];
                    
                    result[temp2 + 1, temp1 + 1] = matrix[j, i];
                    temp2 += 2;
                }
                temp1 += 2;
            }

            return result;
        }

        public static List<DCTBlock> splitIntoBlocks(byte[] arr)
        {
            int width = (arr[0] << 8) | arr[1];
            int height = (arr[2] << 8) | arr[3];
            byte[] bytes = new byte[arr.Length - 4];
            Array.Copy(arr, 4, bytes, 0, bytes.Length);
            byte[] block = new byte[64];
            List<DCTBlock> result = new List<DCTBlock>();
            int index = 0;

            for (int i = 0; i < width * height; i++)
            {

                //block[i % 64] = bytes[index++];
                if (i % 64 == 0 && i != 0)
                {
                    DCTBlock dct = new DCTBlock(block, index / 64, BlockType.Y);
                    result.Add(dct);
                    block = new byte[64];
                    Populate(block, (byte)0);
                }

                block[i % 64] = bytes[index++];
                if (i == width * height - 1)
                {
                    DCTBlock dct = new DCTBlock(block, index / 64, BlockType.Y);
                    result.Add(dct);
                    block = new byte[64];
                    Populate(block, (byte)0);
                }
            }
            Debug.WriteLine("Index y: " + result.Count);
            for (int i = 0; i < (height*width)/4; i++)
            {
                //block[i % 64] = bytes[index++];
                if (i % 64 == 0 && i != 0)
                {
                    DCTBlock dct = new DCTBlock(block, index / 64, BlockType.Cb);
                    result.Add(dct);
                    block = new byte[64];
                    Populate(block, (byte)0);
                }
                block[i % 64] = bytes[index++];
                if (i == ((height * width) / 4) - 1)
                {
                    DCTBlock dct = new DCTBlock(block, index / 64, BlockType.Cb);
                    result.Add(dct);
                    block = new byte[64];
                    Populate(block, (byte)0);
                }
            }
            Debug.WriteLine("Index cr: " + result.Count);
            for (int i = 0; i < (height * width) / 4; i++)
            {
                //block[i % 64] = bytes[index++];
                if (i % 64 == 0 && i != 0)
                {
                    DCTBlock dct = new DCTBlock(block, index / 64, BlockType.Cr);
                    result.Add(dct);
                    block = new byte[64];
                    Populate(block, (byte)0);
                }
                block[i % 64] = bytes[index++];
                if (i == ((height * width) / 4) - 1)
                {
                    DCTBlock dct = new DCTBlock(block, index / 64, BlockType.Cr);
                    result.Add(dct);
                    block = new byte[64];
                    Populate(block, (byte)0);
                }
            }
            Debug.WriteLine("Index cb: " + result.Count);
            return result;
        }

        public static byte[] jpegCompression(Bitmap bitmap)
        {
            List<byte> result = new List<byte>();
            byte[] data = convertRGBToYCbCr(bitmap);
            int width = bitmap.Width;
            int height = bitmap.Height;
            List<DCTBlock> blocks = splitIntoBlocks(data);
            result.Add( (byte)(width >> 8));     // Store the most significant byte of width
            result.Add((byte)(width & 0xFF));   // Store the least significant byte of width
            result.Add((byte)(height >> 8));    // Store the most significant byte of height
            result.Add((byte)(height & 0xFF));  // Store the least significant byte of height
            foreach (DCTBlock block in blocks)
            {
                block.encode();
                List<byte> temp = block.getEncodedBlock();
                result.AddRange(temp);
            }
            return result.ToArray();
        }

        public static Bitmap jpegDecompression(byte[] data) {
            int width = (data[0] << 8) | data[1];
            int height = (data[2] << 8) | data[3];
            int index = 0;
            data = data.Skip(4).ToArray();
            List<double> decodedData = new List<double>();
            List<List<byte>> blocks = new List<List<byte>>();
            List<byte> dataBlock = new List<byte>();
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == 0 && data[i + 1] == 0)
                {
                    blocks.Add(dataBlock);
                    dataBlock = new List<byte>();
                    i++;
                }
                else {
                    dataBlock.Add(data[i++]);
                    dataBlock.Add(data[i]);

                }

            }
            List<byte> yvalue = new List<byte>();
            int limit = width * height;
            int count = 0;
            int index2 = 0;
            for (int i = 0; i < (width * height)/64; i++) {
                DCTBlock block = new DCTBlock(index, BlockType.Y);
                block.decode(blocks[index2++]);
                if (count >= limit) // (count < limit) (?)
                {
                    List<double> temp = block.convertToArrayFromBlock();
                    temp.RemoveRange(limit - count, temp.Count - (limit - count));
                    decodedData.AddRange(temp);
                    break;
                }
                else {
                    List<double> temp = block.convertToArrayFromBlock();
                    decodedData.AddRange(temp);
                }
                count += 64;

            }
            Debug.WriteLine("Y Count: " + decodedData.Count);
            count = 0;
            limit = (width * height) / 4;

            for (int i = 0; i < (width * height) / 4 / 64; i++)
            {
                DCTBlock block = new DCTBlock(index, BlockType.Cb);
                block.decode(blocks[index2++]);

                if (count >= limit)
                {
                    List<double> temp = block.convertToArrayFromBlock();
                    temp.RemoveRange(limit - count, temp.Count - (limit - count));
                    decodedData.AddRange(temp);
                    break;
                }
                else
                {
                    List<double> temp = block.convertToArrayFromBlock();
                    decodedData.AddRange(temp);
                }
                count += 64;

            }
            Debug.WriteLine("Cb Count: " + decodedData.Count);
            count = 0;
            limit = (width * height) / 4;
            for (int i = 0; i < (width * height) / 4 / 64; i++)
            {
                DCTBlock block = new DCTBlock(index, BlockType.Cr);
                block.decode(blocks[index2++]);
                
                if (count >= limit)
                {
                    List<double> temp = block.convertToArrayFromBlock();
                    temp.RemoveRange(limit - count, temp.Count - (limit - count));
                    decodedData.AddRange(temp);
                    break;
                }
                else
                {
                    List<double> temp = block.convertToArrayFromBlock();
                    decodedData.AddRange(temp);
                }
                count += 64;
                if (index2 >= blocks.Count)
                {
                    break;
                }

            }
            Debug.WriteLine("Cr Count: " + decodedData.Count);
            return convertYCbCrToRGBv2(decodedData, width, height);
        }

        public static void Populate<T>(T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = value;
            }
        }

    }
}
