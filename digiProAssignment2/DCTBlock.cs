using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace digiProAssignment2
{
    public enum BlockType {
        Y = 0,
        Cb = 1,
        Cr = 2
    }

    public class DCTBlock
    {
        public static int[,] luminanceQTable = { 
            {16, 11, 10, 16, 24, 40, 51, 61 },
            {12, 12, 14, 19, 26, 58, 60, 55 },
            {14, 13, 16, 24, 40, 57, 69, 56 },
            {14, 17, 22, 29, 51, 87, 80, 62 },
            {18, 22, 37, 56, 68, 109, 103, 77 },
            {24, 35, 55, 64, 81, 104, 113, 92 },
            {49, 64, 78, 87, 103, 121, 120, 101 },
            {72, 92, 95, 98, 112, 100, 103, 99 }
        };

        public static int[,] chrominanceQTable = { 
            {17, 18, 24, 47, 99, 99, 99, 99 },
            {18, 21, 26, 66, 99, 99, 99, 99 },
            {24, 26, 56, 99, 99, 99, 99, 99 },
            {47, 66, 99, 99, 99, 99, 99, 99 },
            {99, 99, 99, 99, 99, 99, 99, 99 },
            {99, 99, 99, 99, 99, 99, 99, 99 },
            {99, 99, 99, 99, 99, 99, 99, 99 },
            {99, 99, 99, 99, 99, 99, 99, 99 }
        };

        private int index;
        private BlockType type;
        private byte[] block;
        private double[,] doubleBlock;
        private int[] zigzagOrdered = new int[64];
        private List<byte> entropyEncoded = new List<byte>();
        public DCTBlock(byte[] block, int index, BlockType type) {
            this.block = block;
            this.index = index;
            this.type = type;
            doubleBlock = createDoubleBlock(block);
        }

        public DCTBlock(int index, BlockType type) {
            this.block = new byte[64];
            this.index = index;
            this.type = type;
            doubleBlock = new double[8, 8];
        }

        public double[,] createDoubleBlock(byte[] arr) {
            double[,] doubles = new double[8, 8];
            for (int i = 0; i < 64; i++)
            {
                doubles[i / 8, i % 8] = (double)arr[i];
            }
            return doubles;
        }

        public List<double> convertToArrayFromBlock() {             
            List<double> result = new List<double>();
            for (int i = 0; i < 64; i++)
            {
                result.Add(doubleBlock[i / 8, i % 8]);
            }
            return result;
        } 

        public List<byte> getEncodedBlock()
        {
            return entropyEncoded;
        }

        public List<byte> entropyEncoding(List<Tuple<int, int>> data)
        {
            List<byte> result = new List<byte>();
            for (int i = 0; i < data.Count; i++)
            {
                byte runlength = Convert.ToByte(Math.Abs(data[i].Item1));
                byte size = Convert.ToByte(GetRequiredBits(data[i].Item2));
                if (data[i].Item1 > 15)
                {
                    byte temp = (byte)(15 << 4 | 0);
                    result.Add(temp);
                }
                else
                {
                    byte temp = (byte)((runlength << 4) | (size & 0x0F));
                    byte temp1 = (byte)(data[i].Item2);
                    result.Add(temp);
                    result.Add(temp1);
                }
            }
            result.Add(0);
            return result;
        }

        public List<Tuple<int, int>> entropyDecoding(List<byte> data)
        {
            List<Tuple<int, int>> result = new List<Tuple<int, int>>();
            int index = 0;
            int runlength = 0;
            while (data[index] != 0)
            {
                int value = 0;
                byte temp = data[index];
                int temp_runlength = temp >> 4;
                int size = temp & 0x0F;
                if (temp_runlength == 15 && size == 0)
                {
                    runlength += temp_runlength;
                    index++;

                }
                else
                {
                    index++;
                    runlength += temp_runlength;
                    value = ByteToNegativeInt(data[index]);
                    result.Add(new Tuple<int, int>(Math.Abs(runlength), value));
                    runlength = 0;
                }
                if(index < data.Count - 1) { 
                    index++;
                }
                else
                {
                    break;
                }
            }
            return result;
        }

        private  int ByteToNegativeInt(byte binary)
        {
            int result = 0;
            if ((binary & 0x80) == 0x80)
            {
                result = (int)binary;
                result = result - 256;
            }
            else
            {
                result = (int)binary;
            }

            return result;
        }
        private int GetRequiredBits(int value)
        {
            // Calculate the number of bits required to represent the absolute value of 'value'
            return (int)Math.Floor(Math.Log(Math.Abs(value), 2)) + 1;
        }

        private int[] ZigZagMerge(double[,] array)
        {
            int[] result = new int[64]; // Assuming the array is 8x8

            int index = 0;
            int row = 0, col = 0;
            bool goingUp = true;

            // Traverse diagonally
            while (row < array.GetLength(0) && col < array.GetLength(1))
            {
                result[index++] = (int)Math.Round(array[row, col]);

                if (goingUp)
                {
                    if (col == array.GetLength(1) - 1)
                    {
                        row++;
                        goingUp = false;
                    }
                    else if (row == 0)
                    {
                        col++;
                        goingUp = false;
                    }
                    else
                    {
                        row--;
                        col++;
                    }
                }
                else
                {
                    if (row == array.GetLength(0) - 1)
                    {
                        col++;
                        goingUp = true;
                    }
                    else if (col == 0)
                    {
                        row++;
                        goingUp = true;
                    }
                    else
                    {
                        row++;
                        col--;
                    }
                }
            }

            return result;
        }

        public void orderbyZigZag()
        {
            zigzagOrdered = ZigZagMerge(doubleBlock);
        }

        public List<Tuple<int, int>> RunLengthCoding(int[] array)
        {
            List<Tuple<int, int>> result = new List<Tuple<int, int>>();
            double value = 0;
            int runlength = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == 0)
                {
                    runlength++;
                }
                else
                {
                    value = array[i];
                    Tuple<int, int> pair = new Tuple<int, int>(runlength, (int)value);
                    result.Add(pair);
                    runlength = 0;
                }
            }
            return result;
        }

        public int[] RunLengthDecoding(List<Tuple<int, int>> list)
        {
            int[] result = new int[64];
            int index = 0;
            foreach (Tuple<int, int> pair in list)
            {
                for (int i = 0; i < pair.Item1; i++)
                {
                    result[index++] = 0;
                }
                result[index++] = pair.Item2;
            }
            return result;
        }

        public double[,] unZigZag(int[] array)
        {
            double[,] result = new double[8, 8];
            int index = 0;
            int row = 0, col = 0;
            bool goingUp = true;

            // Traverse diagonally
            while (row < 8 && col < 8)
            {
                result[row, col] = array[index++];

                if (goingUp)
                {
                    if (col == 7)
                    {
                        row++;
                        goingUp = false;
                    }
                    else if (row == 0)
                    {
                        col++;
                        goingUp = false;
                    }
                    else
                    {
                        row--;
                        col++;
                    }
                }
                else
                {
                    if (row == 7)
                    {
                        col++;
                        goingUp = true;
                    }
                    else if (col == 0)
                    {
                        row++;
                        goingUp = true;
                    }
                    else
                    {
                        row++;
                        col--;
                    }
                }
            }
            return result;
        }


        public void encode() 
        {

            for (int i = 0; i < 64; i++)
            {
                doubleBlock[i / 8, i % 8] -= 128;
            }
            
            doubleBlock = dctEncode(doubleBlock, 8, 8);
            quantize();
            orderbyZigZag();
            List<Tuple<int, int>> runLength = RunLengthCoding(zigzagOrdered);
            entropyEncoded = entropyEncoding(runLength);

        }

        public void decode(List<byte> data)
        {
            List<byte> bytes = data;
            List<Tuple<int, int>> runLength = entropyDecoding(bytes);
            int[] zigzag = RunLengthDecoding(runLength);
            doubleBlock = unZigZag(zigzag);
            dequantize();
            
            doubleBlock = idct(doubleBlock, 8, 8);
/*            if (this.type == BlockType.Cb)
            {
                Debug.WriteLine("Cb Block: ");
                printBlock();
            }*/
            for (int i = 0; i < 64; i++)
            {
                doubleBlock[i / 8, i % 8] += 128;
                doubleBlock[i / 8, i % 8] = satuaration(doubleBlock[i / 8, i % 8]);
            }
            
        }

        public double satuaration(double value)
        {
            if (value > 255)
            {
                return 255;
            }
            else if (value < 0)
            {
                return 0;
            }
            else
            {
                return value;
            }
        }

        public void quantize()
        {
            int[,] q = new int[8, 8];
            if (this.type == BlockType.Y) {
                q = luminanceQTable;
            } else {
                q = chrominanceQTable;
            }
            for (int i = 0; i < 64;i++) {
                doubleBlock[i / 8, i % 8] = Math.Round(doubleBlock[i / 8, i % 8] / q[i / 8, i % 8]);
            }
        }

        public void dequantize()
        {
            int[,] q = new int[8, 8];
            if (this.type == BlockType.Y)
            {
                q = luminanceQTable;
            }
            else
            {
                q = chrominanceQTable;
            }
            for (int i = 0; i < 64; i++)
            {
                doubleBlock[i / 8, i % 8] = Math.Round(doubleBlock[i / 8, i % 8] * q[i / 8, i % 8]);
            }
        }

        public double[,] dctEncode(double[,] F, int n, int m) {
            double[,] result = new double[n, m];
            for (int u = 0; u < n; u++)
            {
                for (int v = 0; v < m; v++)
                {

                    double sum = 0;
                    for (int x = 0; x < n; x++)
                    {
                        for (int y = 0; y < m; y++)
                        {
                            sum += Math.Cos(((2 * x + 1) * u * Math.PI) / (2 * n)) * Math.Cos(((2 * y + 1) * v * Math.PI) / (2 * m)) * F[x,y];
                        }
                    }
                    sum *= C(u) * C(v) * (2 / Math.Sqrt(n * m));
                    result[u,v] = sum;
                }
            }
            return result;

        }

        public double[,] idct(double[,] H, int n, int m)
        {
            double[,] result = new double[n, m];
            for (int x = 0; x < n; x++)
            {
                for (int y = 0; y < m; y++)
                {
                    double sum = 0;
                    for (int u = 0; u < n; u++)
                    {
                        for (int v = 0; v < m; v++)
                        {
                            sum += 2 * ((C(u) * C(v)) / Math.Sqrt(n * m)) * Math.Cos(((2 * x + 1) * u * Math.PI) / (2 * n)) * Math.Cos(((2 * y + 1) * v * Math.PI) / (2 * m)) * H[u,v];
                        }
                    }
                    result[x, y] = sum;
                }
            }
            return result;
        }

        public void printBlock()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Debug.Write(doubleBlock[i,j] + " ");
                }
                Debug.Write("\n");
            }
        }

        private double C(int u)
        {
            if (u == 0)
            {
                return 1.0 / Math.Sqrt(2);
            }
            else
            {
                return 1;
            }
        }

    }
}
