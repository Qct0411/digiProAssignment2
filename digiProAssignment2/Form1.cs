using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace digiProAssignment2
{
    public partial class Form1 : Form
    {
        private Bitmap? image1;
        public Form1()
        {
            image1 = null;
            
            this.DoubleBuffered = true;
            Paint += new PaintEventHandler(this.Form1_Paint);
            InitializeComponent();
        }

        private void Form1_Paint(object? sender, PaintEventArgs e)
        {
            if (image1 != null)
            {
                double aspectRatio = (double)image1.Width / image1.Height;
                if (aspectRatio > 1)
                {
                    e.Graphics.DrawImage(image1, 0, 0, ClientSize.Width, (int)(ClientSize.Width / aspectRatio));
                }
                else
                {
                    e.Graphics.DrawImage(image1, 0, 0, (int)(ClientSize.Height * aspectRatio), ClientSize.Height);
                }
                //e.Graphics.DrawImage(image1, 0, 0, ClientSize.Width, ClientSize.Height);
            }
        }

        private void fIleToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Open File";
                //dialog.Filter = "bmp files (*.bmp)|*.bmp";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (image1 != null)
                    {
                        image1.Dispose();
                    }
                    if (Path.GetExtension(dialog.FileName) == ".quan")
                    {
                        image1 = JPEG_Compression.jpegDecompression(File.ReadAllBytes(dialog.FileName));
                        //image1 = JPEG_Compression.convertYCbCrToRGB(File.ReadAllBytes(dialog.FileName));
                        Debug.WriteLine("Width: " + image1.Width + ", Height: " + image1.Height);
                    }
                    else
                    {
                        image1 = new Bitmap(dialog.FileName);
                        Debug.WriteLine("Width: " + image1.Width + ", Height: " + image1.Height);
                    }
                    Invalidate();
                }
            }
        }

        private void milestone1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String path = "C:\\Users\\caoqu\\source\\repos\\digiProAssignment2\\digiProAssignment2\\bin\\Debug\\net8.0-windows\\";
            if (File.Exists(path+"image1.quan"))
            {
                if (image1 != null)
                {
                    byte[] data = JPEG_Compression.jpegCompression(image1);
                    //byte[] data = JPEG_Compression.convertRGBToYCbCr(image1);
                    File.WriteAllBytes(path + "image1.quan", data);
                    MessageBox.Show("File saved to " + path);

                    Invalidate();
                }
            }
            else {
                File.Create("image1.quan");
                if (image1 != null)
                {
                    byte[] data = JPEG_Compression.jpegCompression(image1);
                    //byte[] data = JPEG_Compression.convertRGBToYCbCr(image1);
                    File.WriteAllBytes(path + "image1.quan", data);
                    MessageBox.Show("File saved to " + path);

                    Invalidate();
                }
            }
            
        }
    }
}
