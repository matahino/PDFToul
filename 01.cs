using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrinterApp
{
    public class Form1 : Form
    {
        private PrintDocument printDocument;
        private PrintPreviewDialog printPreviewDialog;
        private Button printPreviewButton;
        private Button clearDrawingButton;
        private Button moveLeftButton;
        private Button moveRightButton;
        private Button moveUpButton;
        private Button moveDownButton;
        private Panel drawingPanel;
        private bool isDrawing = false;
        private Pen drawingPen = new Pen(Color.Black, 2);
        private List<Point> drawingPoints = new List<Point>();
        private Bitmap drawingBitmap;
        private float imageX = 100, imageY = 100;

        public Form1()
        {
            InitializeComponents();
            AttachEventHandlers();
        }

        private void InitializeComponents()
        {
            printDocument = new PrintDocument();
            printPreviewDialog = new PrintPreviewDialog();
            printPreviewButton = new Button { Text = "Print Preview" };
            clearDrawingButton = new Button { Text = "Clear Drawing" };
            moveLeftButton = new Button { Text = "←" };
            moveRightButton = new Button { Text = "→" };
            moveUpButton = new Button { Text = "↑" };
            moveDownButton = new Button { Text = "↓" };
            drawingPanel = new Panel { Dock = DockStyle.Fill };

            Size = new Size(800, 600);
            Controls.Add(drawingPanel);
            Controls.AddRange(new Control[] { printPreviewButton, clearDrawingButton, moveLeftButton, moveRightButton, moveUpButton, moveDownButton });

            foreach (var button in new[] { printPreviewButton, clearDrawingButton, moveLeftButton, moveRightButton, moveUpButton, moveDownButton })
            {
                button.Dock = DockStyle.Top;
            }
        }

        private void AttachEventHandlers()
        {
            printPreviewButton.Click += async (sender, e) => await OpenPrintPreviewAsync();
            clearDrawingButton.Click += (sender, e) => ClearDrawing();
            moveLeftButton.Click += (sender, e) => AdjustImagePosition(-10, 0);
            moveRightButton.Click += (sender, e) => AdjustImagePosition(10, 0);
            moveUpButton.Click += (sender, e) => AdjustImagePosition(0, -10);
            moveDownButton.Click += (sender, e) => AdjustImagePosition(0, 10);
            drawingPanel.Paint += DrawingPanel_Paint;
            drawingPanel.MouseDown += DrawingPanel_MouseDown;
            drawingPanel.MouseMove += DrawingPanel_MouseMove;
            drawingPanel.MouseUp += DrawingPanel_MouseUp;
            printDocument.PrintPage += PrintDocument_PrintPage;
        }

private async Task OpenPrintPreviewAsync()
{
    await Task.Run(() =>
    {
        Invoke(new Action(() =>
        {
            printPreviewDialog.Document = printDocument;
            printPreviewDialog.ShowDialog();
        }));
    }).ConfigureAwait(true); 
}


        private void ClearDrawing()
        {
            if (drawingBitmap != null) drawingBitmap.Dispose();
            drawingBitmap = new Bitmap(drawingPanel.Width, drawingPanel.Height);
            using (var g = Graphics.FromImage(drawingBitmap))
            {
                g.Clear(Color.White);
                if (drawingPoints.Count > 1)
                {
                    g.DrawLines(drawingPen, drawingPoints.ToArray());
                }
            }
            drawingPoints.Clear();
            OpenPrintPreviewAsync();
        }

        private void AdjustImagePosition(float dx, float dy)
        {
            imageX += dx;
            imageY += dy;
            drawingPanel.Invalidate();
        }

        private void DrawingPanel_Paint(object sender, PaintEventArgs e)
        {
            if (drawingBitmap != null)
            {
                e.Graphics.DrawImage(drawingBitmap, 0, 0);
            }
            if (isDrawing && drawingPoints.Count > 1)
            {
                e.Graphics.DrawLines(drawingPen, drawingPoints.ToArray());
            }
        }

        private void DrawingPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDrawing = true;
                drawingPoints.Add(e.Location);
            }
        }

        private void DrawingPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                drawingPoints.Add(e.Location);
                drawingPanel.Invalidate();
            }
        }

        private void DrawingPanel_MouseUp(object sender, MouseEventArgs e)
        {
            isDrawing = false;
            SaveCurrentDrawing();
        }

        private void SaveCurrentDrawing()
        {
            using (var g = Graphics.FromImage(drawingBitmap))
            {
                if (drawingPoints.Count > 1)
                {
                    g.DrawLines(drawingPen, drawingPoints.ToArray());
                }
            }
            drawingPoints.Clear();
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (drawingBitmap != null)
            {
                e.Graphics.DrawImage(drawingBitmap, imageX, imageY, drawingBitmap.Width, drawingBitmap.Height);
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
