using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace курсач_ТСвСИ
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        List<int[,]> List_MOBF = new List<int[,]>();
        Random rand = new Random();

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = ".bmp|*.bmp";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = new Bitmap(openFileDialog1.FileName);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //ШАГ1
            int HeightImage = pictureBox1.Image.Height;
            int WidthImage = pictureBox1.Image.Width;
            int[,] C_matrix = new int[WidthImage, HeightImage];
            int[,] MOBF = new int[WidthImage, HeightImage];
            //List<int[,]> List_MOBF = new List<int[,]>(); //N_fi

            for (int i = 0; i < WidthImage; i++)
            {
                for (int k = 0; k < HeightImage; k++)
                {
                    switch (comboBox1.Text)
                    {
                        case "Красный":
                            C_matrix[i, k] = ((Bitmap)pictureBox1.Image).GetPixel(i, k).R;
                            break;
                        case "Зелёный":
                            C_matrix[i, k] = ((Bitmap)pictureBox1.Image).GetPixel(i, k).G;
                            break;
                        case "Синий":
                            C_matrix[i, k] = ((Bitmap)pictureBox1.Image).GetPixel(i, k).B;
                            break;
                        default:
                            MessageBox.Show("Не был выбран цветовой канал!");
                            return;
                    }
                    //ШАГ2
                    if ((i + k) % 2 == 0)
                    {
                        MOBF[i,k] = 1;
                    }
                    else
                    {
                        MOBF[i, k] = -1;
                    }

                }
            }

            PutArrayInDGV(C_matrix, dataGridView1);
            PutArrayInDGV(MOBF, dataGridView2);

            //ШАГ3
            int N_fi = textBox1.Text.Length * 8; //объём ЦВЗ
            int n = Convert.ToInt32(Math.Floor(Math.Sqrt((HeightImage * WidthImage) / N_fi)));
            textBox3.Text = "Nф = " + N_fi;
            textBox4.Text = "n = " + n;

            //ШАГ4
            int d = Convert.ToInt32(Math.Log(N_fi, 2));
            int s = rand.Next(1, N_fi - 1);
            int[] C = Vrnd(s, d);
            Autocorrelation(C);
            PutArrayInDGV(C, dataGridView3);

            //ШАГ5
            int column1 = 0;
            int column2 = n;
            int row1;
            int row2;
            //List<int[,]> List_MOBF = new List<int[,]>(); //N_fi
            //int[,,] List_MOBF = new int[N_fi, MOBF.GetLength(0), MOBF.GetLength(1)];

            for (int i = 0; i < N_fi; i++)
            {
                List_MOBF.Add(new int[MOBF.GetLength(0), MOBF.GetLength(1)]);
            }

            for (int i = 0; i < N_fi + 1; i++)
            {
                row1 = (n * (i)) % WidthImage; //возможно проблемка из-за *0
                row2 = row1 + n;

                List_MOBF.Insert(C[i], PerenosOblasti(MOBF, row1, row2, column1, column2));

                if (row2 == WidthImage)
                {
                    column1 += n;
                    column2 += n;
                }
            }

            pictureBox3.Image = CreateBlackBitmap(List_MOBF[C[0]], 0);
            pictureBox4.Image = CreateBlackBitmap(List_MOBF[C[1]], 0);
            pictureBox5.Image = CreateBlackBitmap(List_MOBF[C[2]], 0);
            pictureBox6.Image = CreateBlackBitmap(List_MOBF[C[3]], 0);
            PutArrayInDGV(List_MOBF[C[0]], dataGridView4);
            PutArrayInDGV(List_MOBF[C[1]], dataGridView5);
            PutArrayInDGV(List_MOBF[C[2]], dataGridView6);
            PutArrayInDGV(List_MOBF[C[3]], dataGridView7);
            FirstDisplayedScrolling(List_MOBF[C[0]], dataGridView4);
            FirstDisplayedScrolling(List_MOBF[C[1]], dataGridView5);
            FirstDisplayedScrolling(List_MOBF[C[2]], dataGridView6);
            FirstDisplayedScrolling(List_MOBF[C[3]], dataGridView7);

            //ШАГ6
            int[] SOM = new int[N_fi];

            for (int i = 0; i < N_fi; i++)
            {
                for (int x = 0; x < WidthImage; x++)
                {
                    for (int y = 0; y < HeightImage; y++)
                    {
                        SOM[i] += List_MOBF[i][x, y] * C_matrix[x, y];
                    }
                }
                chart1.Series[0].Points.Add(SOM[i]);
            }

            int maxSOM = Math.Abs(SOM[0]);

            for (int i = 0; i < N_fi; i++)
            {
                if (Math.Abs(SOM[i]) > maxSOM)
                {
                    maxSOM = Math.Abs(SOM[i]);
                }
            }

            textBox5.Text = "Максимальная абсолютная погрешность ортогональности △ = " + maxSOM;

            //ШАГ7
            int[] BOM_stegoText = TextToBinaru(textBox1.Text);

            PutArrayInDGV(Encoding.Default.GetBytes(textBox1.Text).Select(x => Convert.ToInt32(x)).ToArray(), dataGridView10);
            PutArrayInDGV(BOM_stegoText, dataGridView8);

            //ШАГ8
            for (int i = 0; i < BOM_stegoText.Length; i++)
            {
                if (BOM_stegoText[i] == 0)
                {
                    BOM_stegoText[i] = -1;
                }
            }

            int[,] E = new int[WidthImage, HeightImage];

            for (int x = 0; x < WidthImage; x++)
            {
                for (int y = 0; y < HeightImage; y++)
                {
                    for (int i = 0; i < N_fi; i++)
                    {
                        E[x,y] += List_MOBF[i][x, y] * BOM_stegoText[i];
                    }
                }
            }

            PutArrayInDGV(E, dataGridView9);
            pictureBox7.Image = CreateBlackBitmap(E, -1);

            //ШАГ9
            int nf = Convert.ToInt32(Math.Pow(n, 2));
            int Kg;
            for (Kg = 1; nf * Kg < maxSOM; Kg++) { }
            Kg--;

            //ШАГ10
            int[,] Cnorm = new int[WidthImage, HeightImage];
            for (int x = 0; x < WidthImage; x++)
            {
                for (int y = 0; y < HeightImage; y++)
                {
                    Cnorm[x, y] = (C_matrix[x, y] * (255 - 2 * Kg) / 255 + Kg);                
                }
            }

            int[,] S = new int[WidthImage, HeightImage];

            for (int x = 0; x < WidthImage; x++)
            {
                for (int y = 0; y < HeightImage; y++)
                {
                    S[x, y] = Cnorm[x, y] + Kg * E[x, y];
                }
            }

            Bitmap NewBitmap = new Bitmap(WidthImage, HeightImage);
            Bitmap CnormBitmap = new Bitmap(WidthImage, HeightImage);

            for (int x = 0; x < WidthImage; x++)
            {
                for (int y = 0; y < HeightImage; y++)
                {
                    switch (comboBox1.Text)
                    {
                        case "Красный":
                            NewBitmap.SetPixel(x, y, Color.FromArgb(
                                S[x, y],
                                ((Bitmap)pictureBox1.Image).GetPixel(x, y).G,
                                ((Bitmap)pictureBox1.Image).GetPixel(x, y).B));

                            CnormBitmap.SetPixel(x, y, Color.FromArgb(
                                Cnorm[x, y],
                                ((Bitmap)pictureBox1.Image).GetPixel(x, y).G,
                                ((Bitmap)pictureBox1.Image).GetPixel(x, y).B));
                            break;
                        case "Зелёный":
                            NewBitmap.SetPixel(x, y, Color.FromArgb(
                                ((Bitmap)pictureBox1.Image).GetPixel(x, y).R,
                                S[x, y],
                                ((Bitmap)pictureBox1.Image).GetPixel(x, y).B));

                            CnormBitmap.SetPixel(x, y, Color.FromArgb(
                                ((Bitmap)pictureBox1.Image).GetPixel(x, y).R,
                                Cnorm[x, y],
                                ((Bitmap)pictureBox1.Image).GetPixel(x, y).B));
                            break;
                        case "Синий":
                            NewBitmap.SetPixel(x, y, Color.FromArgb(
                                ((Bitmap)pictureBox1.Image).GetPixel(x, y).R,
                                ((Bitmap)pictureBox1.Image).GetPixel(x, y).G,
                                S[x, y]));

                            CnormBitmap.SetPixel(x, y, Color.FromArgb(
                                ((Bitmap)pictureBox1.Image).GetPixel(x, y).R,
                                ((Bitmap)pictureBox1.Image).GetPixel(x, y).G,
                                Cnorm[x, y]));
                            break;
                        default:
                            MessageBox.Show("Не был выбран цветовой канал!");
                            return;
                    }
                }
            }

            pictureBox2.Image = NewBitmap;

            pictureBox8.Image = pictureBox1.Image;
            pictureBox9.Image = CnormBitmap;
            pictureBox10.Image = pictureBox2.Image;
        }

        private void FirstDisplayedScrolling(int[,] pixels, DataGridView tabl)
        {
            int m = pixels.GetLength(0);
            int n = pixels.GetLength(1);

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (pixels[i, j] != 0)
                    {
                        tabl.FirstDisplayedScrollingRowIndex = i - 1;
                        tabl.FirstDisplayedScrollingColumnIndex = j - 1;
                        return;
                    }
                }
            }
        }

        private Bitmap CreateBlackBitmap(int[,] pixels, int valueBlack)
        {
            Bitmap result = new Bitmap(pixels.GetLength(0), pixels.GetLength(1));

            for (int x = 0; x < pixels.GetLength(0); x++)
            {
                for (int y = 0; y < pixels.GetLength(1); y++)
                {
                    if (pixels[x, y] != valueBlack)
                    {
                        result.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        result.SetPixel(x, y, Color.Black);
                    }
                }
            }

            return result;
        }
         
        private int[] TextToBinaru(String text)
        {
            int[] Binaru = new int[text.Length * 8];
            String Byte = "";
            byte[] Bytes = Encoding.Default.GetBytes(text);

            for (int i = 0; i < text.Length; i++)
            {
                Byte = Convert.ToString(Bytes[i], 2);

                for (int j = 0; j < 8; j++)
                {
                    if (Byte.Length < (8 - j))
                    {
                        Binaru[i * 8 + j] = 0;
                    }
                    else
                    {
                        Binaru[i * 8 + j] = Convert.ToInt32(Byte.Substring(j - (8 - Byte.Length), 1));
                    }
                }
            }

            return Binaru;
        }

        private int[] Vrnd(int s, int d)
        {
            int[] Rdec = new int[Convert.ToInt32(Math.Pow(2, d))]; //или d
            int[] Rbin;
            int[] nu = new int[d];
            int[] R;
            int bit;

            for (int i = 0; i < d; i++)
            {
                nu[i] = rand.Next(0, 2);
            }

            Rdec[0] = s;
            Rbin = D2B(Rdec[0]);

            for (int i = 1; i < Math.Pow(2, d) - 1; i++)
            {
                bit = 0;

                for (int j = 0; j < Rbin.Length; j++)
                {
                    if (nu[j] == 1)
                    {
                        bit = Rbin[j] ^ bit;
                    }
                }


                R = (int[])Rbin.Clone();
                Rbin[0] = bit;

                for (int j = 1; j < Rbin.Length; j++)
                {
                    Rbin[j] = R[j - 1];
                }

                Rdec[i] = B2D(Rbin);
            }

            Rdec[Convert.ToInt32(Math.Pow(2, d) - 1)] = Convert.ToInt32(Math.Pow(2, d));
            return Rdec;
        }

        private int[] D2B(int Rdec) {
            string RbinText = Convert.ToString(Rdec, 2);
            int[] Rbin = new int[RbinText.Length];

            for (int i = 0; i < RbinText.Length; i++)
            {
                Rbin[i] = Convert.ToInt32(RbinText.Substring(i, 1));
            }

            return Rbin;
        }

        private int B2D(int[] Rbin)
        {
            string RbinText = "";

            for (int i = 0; i < Rbin.Length; i++)
            {
                RbinText += Rbin[i];
            }

            return Convert.ToInt32(RbinText, 2); ;
        }

        private int[,] PerenosOblasti(int[,] Input, int row1, int row2, int column1, int column2)
        {//row2 и
            int[,] Output = new int[Input.GetLength(0), Input.GetLength(1)];

            for (int i = row1; i < row2; i++)
            {
                for (int j = column1; j < column2; j++)
                {
                    Output[i, j] = Input[i, j];
                }
            }

            return Output;
        }

        private void PutArrayInDGV(int[,] array, DataGridView tabl)
        {
            int m = array.GetLength(0);
            int n = array.GetLength(1);

            tabl.Columns.Clear();
            tabl.Rows.Clear();

            tabl.ColumnCount = n;
            tabl.RowCount = m;

            for (int i = 0; i < n; i++)
            {
                tabl.Columns[i].HeaderText = (i + 1).ToString();
            }

            for (int i = 0; i < m; i++)
            {
                tabl.Rows[i].HeaderCell.Value = (i + 1).ToString();
                for (int j = 0; j < n; j++)
                {
                    tabl.Rows[i].Cells[j].Value = array[i, j];
                }
            }
        }

        private void PutArrayInDGV(int[] array, DataGridView tabl)
        {
            int n = array.Length;

            tabl.Columns.Clear();
            tabl.Rows.Clear();

            tabl.ColumnCount = 1;
            tabl.RowCount = n;

            for (int i = 0; i < n; i++)
            {
                tabl.Rows[i].HeaderCell.Value = (i + 1).ToString();
                tabl.Rows[i].Cells[0].Value = array[i];
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int HeightImage = pictureBox2.Image.Height;
            int WidthImage = pictureBox2.Image.Width;
            int[,] S_matrix = new int[WidthImage, HeightImage];
            int[,] MOBF = new int[WidthImage, HeightImage];
            int N_fi = textBox1.Text.Length * 8; //объём ЦВЗ
            int n = Convert.ToInt32(Math.Floor(Math.Sqrt((HeightImage * WidthImage) / N_fi)));
            int d = Convert.ToInt32(Math.Log(N_fi, 2));
            int s = rand.Next(1, N_fi);
            //List<int[,]> List_MOBF = new List<int[,]>(); //N_fi
            
            
            for (int i = 0; i < WidthImage; i++)
            {
                for (int k = 0; k < HeightImage; k++)
                {
                    switch (comboBox1.Text)
                    {
                        case "Красный":
                            S_matrix[i, k] = ((Bitmap)pictureBox2.Image).GetPixel(i, k).R;
                            break;
                        case "Зелёный":
                            S_matrix[i, k] = ((Bitmap)pictureBox2.Image).GetPixel(i, k).G;
                            break;
                        case "Синий":
                            S_matrix[i, k] = ((Bitmap)pictureBox2.Image).GetPixel(i, k).B;
                            break;
                        default:
                            MessageBox.Show("Не был выбран цветовой канал!");
                            return;
                    }
                }
            }

            int[] Mvec_bin = new int[N_fi];
            int[] nu = new int[N_fi];

            for (int i = 0; i < N_fi; i++)
            {
                for (int x = 0; x < WidthImage; x++)
                {
                    for (int y = 0; y < HeightImage; y++)
                    {
                        nu[i] += S_matrix[x, y] * List_MOBF[i][x, y];
                    }
                }

                if (nu[i] > 0)
                {
                    Mvec_bin[i] = 1;
                }
                else
                {
                    if (nu[i] < 0)
                    {
                        Mvec_bin[i] = 0;
                    }
                    else
                    {
                        Mvec_bin[i] = rand.Next(0, 2);
                    }
                }
            }

            textBox2.Text = BinaruToText(Mvec_bin);
            //textBox2.Text = textBox1.Text; 

            Ocenka();
        }

        private String BinaruToText(int[] Binaru)
        {
            String Byte;
            String text = "";
            byte[] Bytes = new byte[Binaru.Length / 8];

            for (int i = 0; i < Binaru.Length / 8; i++)
            {
                Byte = "";

                for (int j = 0; j < 8; j++)
                {
                    Byte += Binaru[(i * 8) + j];
                }

                Bytes[i] = Convert.ToByte(Convert.ToInt32(Byte, 2));
            }

            text = Encoding.Default.GetString(Bytes);

            return text;
        }

        private void Ocenka()
        {
            int HeightImage = pictureBox2.Image.Height;
            int WidthImage = pictureBox2.Image.Width;
            byte[] ishod = new byte[WidthImage * HeightImage];
            byte[] zapolnennnoe = new byte[WidthImage * HeightImage];

            for (int i = 0; i < WidthImage; i++)
            {
                for (int k = 0; k < HeightImage; k++)
                {
                    switch (comboBox1.Text)
                    {
                        case "Красный":
                            ishod[i * HeightImage + k] = ((Bitmap)pictureBox1.Image).GetPixel(i, k).R;
                            zapolnennnoe[i * HeightImage + k] = ((Bitmap)pictureBox2.Image).GetPixel(i, k).R;
                            break;
                        case "Зелёный":
                            ishod[i * HeightImage + k] = ((Bitmap)pictureBox1.Image).GetPixel(i, k).G;
                            zapolnennnoe[i * HeightImage + k] = ((Bitmap)pictureBox2.Image).GetPixel(i, k).G;
                            break;
                        case "Синий":
                            ishod[i * HeightImage + k] = ((Bitmap)pictureBox1.Image).GetPixel(i, k).B;
                            zapolnennnoe[i * HeightImage + k] = ((Bitmap)pictureBox2.Image).GetPixel(i, k).B;
                            break;
                        default:
                            MessageBox.Show("Не был выбран цветовой канал!");
                            return;
                    }
                }
            }

            label14.Text = "Среднеквадратичная ошибка : " + MSE(ishod, zapolnennnoe);
            label13.Text = "Нормировання среднеквадратичная ошибка : " + NMSE(ishod, zapolnennnoe);
        }

        public static double MSE(byte[] image1, byte[] image2)
        {
            double result = 0;
            double addition;
            int length = image1.Length;

            for (int i = 0; i < length; i++)
            {
                addition = Math.Pow(image1[i] - image2[i], 2);
                result += addition;
            }

            return result / length;
        }

        public static double NMSE(byte[] image1, byte[] image2)
        {
            byte[] imageNull = new byte[image1.Length];
            double result = MSE(image1, image2) / MSE(image1, imageNull);
            return result;
        }

        public void Autocorrelation(int[] data)
        {
            int n = data.Length;
            chart2.Series[0].Points.Clear();

            for (int i = 0; i < n; i++)
            {
                double summ1 = 0;
                double summ2 = 0;
                double summ3 = 0;

                int n_iteration = n - i;

                int[] x1 = data.Skip(i).ToArray();
                int[] x2 = data.Take(n_iteration).ToArray();

                double mean1 = x1.Average();
                double mean2 = x2.Average();

                for (int k = 0; k < n_iteration; k++)
                {
                    summ1 += (x1[k] - mean1) * (x2[k] - mean2);
                    summ2 += Math.Pow((x1[k] - mean1), 2);
                    summ3 += Math.Pow((x2[k] - mean2), 2);
                }

                double korrellFunction = summ1 / (Math.Sqrt(summ2 * summ3));
                chart2.Series[0].Points.AddY(korrellFunction);

                if (i > 800)
                {
                    break;
                }
            }
        }
    }
}
