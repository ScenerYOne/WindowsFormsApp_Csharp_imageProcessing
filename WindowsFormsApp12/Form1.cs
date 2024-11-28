using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Digital_Image_Processing
{
    public partial class Form1 : Form
    {
        private Bitmap originalImage;
        private Bitmap processedImage;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                originalImage = new Bitmap(openFileDialog.FileName);

                // Display original image
                pictureBox1.Image = ResizeImageToBox(originalImage, pictureBox1.Width, pictureBox1.Height);

                // Generate histogram for original image
                GenerateHistogram(originalImage, histogram1);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (originalImage == null) return;

            double mean = CalculateMeanGrayLevel(originalImage);
            processedImage = ApplyThresholding(originalImage, (byte)mean);

            // Display processed image
            outputBox.Image = ResizeImageToBox(processedImage, outputBox.Width, outputBox.Height);

            // Generate histogram for processed image
            GenerateHistogram(processedImage, histogram2);
        }

        private void imageQuan_Click(object sender, EventArgs e)
        {
            if (originalImage == null) return;

            Bitmap quantizedImage8 = ApplyQuantization(originalImage, 8);
            Bitmap quantizedImage16 = ApplyQuantization(originalImage, 16);

            // Display quantized image (8 levels)
            outputBox.Image = ResizeImageToBox(quantizedImage8, outputBox.Width, outputBox.Height);

            // Generate histogram for quantized image
            GenerateHistogram(quantizedImage8, histogram2);

            // Show quantization mapping for both levels
            ShowQuantizationMapping(8);
            ShowQuantizationMapping(16);

            // Display comparison popup
            ShowComparisonPopup(quantizedImage8, "Quantization - 8 Levels", quantizedImage16, "Quantization - 16 Levels");
        }

        private void histogram_Click(object sender, EventArgs e)
        {
            if (originalImage == null) return;

            processedImage = ApplyHistogramEqualization(originalImage);

            // Display processed image
            outputBox.Image = ResizeImageToBox(processedImage, outputBox.Width, outputBox.Height);

            // Generate histogram for processed image
            GenerateHistogram(processedImage, histogram2);
        }

        private void imageDifference_Click(object sender, EventArgs e)
        {
            if (originalImage == null || processedImage == null) return;

            Bitmap diffImage = CalculateImageDifference(originalImage, processedImage);

            // Display difference image
            difBox.Image = ResizeImageToBox(diffImage, difBox.Width, difBox.Height);
        }

        private void ShowQuantizationMapping(int levels)
        {
            Form mappingForm = new Form
            {
                Text = $"Quantization Mapping ({levels} Levels)",
                Size = new Size(600, 400),
                StartPosition = FormStartPosition.CenterScreen
            };

            Chart chart = new Chart
            {
                Dock = DockStyle.Fill
            };

            ChartArea chartArea = new ChartArea
            {
                AxisX = { Minimum = 0, Maximum = 255, Title = "r (Input Intensity)" },
                AxisY = { Minimum = 0, Maximum = 255, Title = "s (Quantized Intensity)" }
            };
            chart.ChartAreas.Add(chartArea);

            Series series = new Series
            {
                ChartType = SeriesChartType.StepLine,
                Color = Color.Blue,
                BorderWidth = 2
            };

            int step = 256 / levels;
            for (int i = 0; i <= 256; i += step)
            {
                int quantizedValue = (i / step) * step + step / 2;
                series.Points.AddXY(i, quantizedValue);
                if (i + step < 256)
                {
                    series.Points.AddXY(i + step - 1, quantizedValue);
                }
            }
            chart.Series.Add(series);

            mappingForm.Controls.Add(chart);
            mappingForm.ShowDialog();
        }

        private void ShowComparisonPopup(Bitmap image1, string title1, Bitmap image2, string title2)
        {
            Form popupForm = new Form
            {
                Text = "Image Comparison",
                Size = new Size(800, 400),
                StartPosition = FormStartPosition.CenterScreen
            };

            TableLayoutPanel tableLayout = new TableLayoutPanel
            {
                RowCount = 2,
                ColumnCount = 2,
                Dock = DockStyle.Fill,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Titles
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Images

            Label label1 = new Label
            {
                Text = title1,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            Label label2 = new Label
            {
                Text = title2,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            PictureBox pictureBox1 = new PictureBox
            {
                Image = image1,
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Fill
            };
            PictureBox pictureBox2 = new PictureBox
            {
                Image = image2,
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Fill
            };

            tableLayout.Controls.Add(label1, 0, 0);
            tableLayout.Controls.Add(label2, 1, 0);
            tableLayout.Controls.Add(pictureBox1, 0, 1);
            tableLayout.Controls.Add(pictureBox2, 1, 1);

            popupForm.Controls.Add(tableLayout);
            popupForm.ShowDialog();
        }

        private void GenerateHistogram(Bitmap image, PictureBox histogramBox)
        {
            int[] histogram = new int[256];

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    int gray = (image.GetPixel(x, y).R + image.GetPixel(x, y).G + image.GetPixel(x, y).B) / 3;
                    histogram[gray]++;
                }
            }

            histogramBox.Controls.Clear();

            Chart chart = new Chart
            {
                Dock = DockStyle.Fill
            };

            ChartArea chartArea = new ChartArea
            {
                AxisX = { Minimum = 0, Maximum = 255, Title = "Intensity" },
                AxisY = { Title = "Frequency" }
            };
            chart.ChartAreas.Add(chartArea);

            Series series = new Series
            {
                ChartType = SeriesChartType.Column,
                Color = Color.Blue,
                Name = "Grayscale Histogram"
            };

            for (int i = 0; i < 256; i++)
            {
                series.Points.AddXY(i, histogram[i]);
            }
            chart.Series.Add(series);

            histogramBox.Controls.Add(chart);
        }

        private Bitmap ResizeImageToBox(Bitmap originalImage, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(resizedImage))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(originalImage, 0, 0, width, height);
            }

            return resizedImage;
        }

        private double CalculateMeanGrayLevel(Bitmap image)
        {
            long totalGray = 0;
            int pixelCount = image.Width * image.Height;

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    totalGray += (pixel.R + pixel.G + pixel.B) / 3;
                }
            }

            return (double)totalGray / pixelCount;
        }

        private Bitmap ApplyThresholding(Bitmap image, byte threshold)
        {
            Bitmap result = new Bitmap(image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    int gray = (pixel.R + pixel.G + pixel.B) / 3;
                    int binary = gray >= threshold ? 255 : 0;
                    result.SetPixel(x, y, Color.FromArgb(binary, binary, binary));
                }
            }

            return result;
        }

        private Bitmap ApplyQuantization(Bitmap image, int levels)
        {
            Bitmap result = new Bitmap(image.Width, image.Height);
            int step = 256 / levels;

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    int gray = (pixel.R + pixel.G + pixel.B) / 3;
                    int quantized = (gray / step) * step + step / 2;
                    result.SetPixel(x, y, Color.FromArgb(quantized, quantized, quantized));
                }
            }

            return result;
        }

        private Bitmap ApplyHistogramEqualization(Bitmap image)
        {
            Bitmap result = new Bitmap(image.Width, image.Height);
            int[] histogram = new int[256];

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    int gray = (image.GetPixel(x, y).R + image.GetPixel(x, y).G + image.GetPixel(x, y).B) / 3;
                    histogram[gray]++;
                }
            }

            int[] cdf = new int[256];
            cdf[0] = histogram[0];
            for (int i = 1; i < 256; i++)
            {
                cdf[i] = cdf[i - 1] + histogram[i];
            }

            int totalPixels = image.Width * image.Height;
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    int gray = (image.GetPixel(x, y).R + image.GetPixel(x, y).G + image.GetPixel(x, y).B) / 3;
                    int equalized = cdf[gray] * 255 / totalPixels;
                    result.SetPixel(x, y, Color.FromArgb(equalized, equalized, equalized));
                }
            }

            return result;
        }

        private Bitmap CalculateImageDifference(Bitmap original, Bitmap processed)
        {
            Bitmap result = new Bitmap(original.Width, original.Height);

            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color origPixel = original.GetPixel(x, y);
                    Color procPixel = processed.GetPixel(x, y);

                    int diffR = Math.Abs(origPixel.R - procPixel.R);
                    int diffG = Math.Abs(origPixel.G - procPixel.G);
                    int diffB = Math.Abs(origPixel.B - procPixel.B);

                    result.SetPixel(x, y, Color.FromArgb(diffR, diffG, diffB));
                }
            }

            return result;
        }
    }
}
