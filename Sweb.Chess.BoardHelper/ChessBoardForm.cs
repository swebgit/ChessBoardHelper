using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sweb.Chess.BoardHelper
{
    public partial class ChessBoardForm : Form
    {

        #region Window Dragging Properties and Methods

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        #endregion

        private const int resizeBorderWidth = 7;
        private bool isWhite = true;
        private int opacity = 102;

        public ChessBoardForm()
        {
            InitializeComponent();

            // Set Window style flags. Not sure what all of these are for yet
            this.SetStyle(ControlStyles.ResizeRedraw, true);// | ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
        }

        #region Form Events
                        
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {

        }

        private void OnResize(object sender, EventArgs e)
        {
            
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            this.DrawBoard(e.Graphics, this.Width, this.Height);
        }
        

        /// <summary>
        /// Used to add window resizing functionality to the form even though the border has been disabled
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            const uint WM_NCHITTEST = 0x0084;
            const uint WM_MOUSEMOVE = 0x0200;

            const uint HTLEFT = 10;
            const uint HTRIGHT = 11;
            const uint HTBOTTOMRIGHT = 17;
            const uint HTBOTTOM = 15;
            const uint HTBOTTOMLEFT = 16;
            const uint HTTOP = 12;
            const uint HTTOPLEFT = 13;
            const uint HTTOPRIGHT = 14;

            const int RESIZE_HANDLE_SIZE = 10;
            bool handled = false;

            if (m.Msg == WM_NCHITTEST || m.Msg == WM_MOUSEMOVE)
            {
                var formSize = this.Size;
                var screenPoint = new Point(m.LParam.ToInt32());
                var clientPoint = this.PointToClient(screenPoint);

                var boxes = new Dictionary<uint, Rectangle>() {
                    {HTBOTTOMLEFT, new Rectangle(0, formSize.Height - RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE)},
                    {HTBOTTOM, new Rectangle(RESIZE_HANDLE_SIZE, formSize.Height - RESIZE_HANDLE_SIZE, formSize.Width - 2*RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE)},
                    {HTBOTTOMRIGHT, new Rectangle(formSize.Width - RESIZE_HANDLE_SIZE, formSize.Height - RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE)},
                    {HTRIGHT, new Rectangle(formSize.Width - RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, formSize.Height - 2*RESIZE_HANDLE_SIZE)},
                    {HTTOPRIGHT, new Rectangle(formSize.Width - RESIZE_HANDLE_SIZE, 0, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE) },
                    {HTTOP, new Rectangle(RESIZE_HANDLE_SIZE, 0, formSize.Width - 2*RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE) },
                    {HTTOPLEFT, new Rectangle(0, 0, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE) },
                    {HTLEFT, new Rectangle(0, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, formSize.Height - 2*RESIZE_HANDLE_SIZE) }
                };

                foreach (KeyValuePair<uint, Rectangle> hitBox in boxes)
                {
                    if (hitBox.Value.Contains(clientPoint))
                    {
                        m.Result = (IntPtr)hitBox.Key;
                        handled = true;
                        break;
                    }
                }
            }

            if (!handled)
                base.WndProc(ref m);
        }

        #endregion

        #region Control Events

        private void ContextMenuAction_Clicked(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem != null)
            {
                switch (menuItem.Text.ToUpper())
                {
                    case "EXIT":
                        this.Close();
                        break;
                    case "STAY ON TOP":
                        menuItem.Checked = !menuItem.Checked;
                        this.TopMost = menuItem.Checked;
                        break;
                    default:
                        break;
                }
            }
        }

        private void OpacityMenuItem_Clicked(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem != null)
            {
                double opacityFromMenuItem;
                if (double.TryParse(menuItem.Text.Replace("%", ""), out opacityFromMenuItem))
                {
                    this.opacity = Convert.ToInt32(opacityFromMenuItem / 100 * 255);
                    this.Refresh();
                }
            }
        }

        private void FlipBoard_Clicked(object sender, EventArgs e)
        {
            this.isWhite = !this.isWhite;
            this.Refresh();
        }

        #endregion

        private void DrawBoard(Graphics graphics, int windowWidth, int windowHeight)
        {
            var fileLetters = new string[] { "A", "B", "C", "D", "E", "F", "G", "H" };

            var verticalBoardPadding = windowHeight * 0.05f;
            var horizontalBoardPadding = windowWidth * 0.05f;

            var verticalFontSize = verticalBoardPadding * 0.9f;
            var horizontalFontSize = horizontalBoardPadding * 0.9f;

            var boardHeight = windowHeight - verticalBoardPadding;
            var boardWidth = windowWidth - horizontalBoardPadding;

            var squareHeight = boardHeight / 8.0f;
            var squareWidth = boardWidth / 8.0f;

            // Window Border
            graphics.DrawRectangle(Pens.Black, 0, 0, windowWidth - 1, windowHeight - 1);

            graphics.FillRectangle(new SolidBrush(Color.FromArgb(1, Color.LimeGreen)), 0, 0, windowWidth - 1, windowHeight - 1);

            // Board Border
            graphics.DrawRectangle(Pens.Black, 0, 0, boardWidth, boardHeight);

            for (int row = 0; row < 8; row++)
            {
                for (int file = 0; file < 8; file++)
                {
                    if ((row % 2 == 0 && file % 2 == 1) ||
                        (row % 2 == 1 && file % 2 == 0))
                    {
                        // Draw Square
                        graphics.FillRectangle(new SolidBrush(Color.FromArgb(this.opacity, 0, 0, 0)), file * squareWidth, row * squareHeight, squareWidth, squareHeight);
                    }
                }

                // Draw files and row labels
                graphics.DrawString(fileLetters[this.isWhite ? row : 7 - row], new Font(FontFamily.GenericMonospace, verticalFontSize, GraphicsUnit.Pixel), Brushes.Black, row * squareWidth + (squareWidth / 2) - verticalFontSize / 2, boardHeight + verticalBoardPadding / 2 - verticalFontSize / 2 - 2);
                graphics.DrawString($"{(this.isWhite ? (8 - row) : (row + 1))}", new Font(FontFamily.GenericMonospace, horizontalFontSize, GraphicsUnit.Pixel), Brushes.Black, boardWidth + horizontalBoardPadding / 2 - verticalFontSize / 2, row * squareHeight + squareHeight / 2 - verticalFontSize / 2);
            }
        }
    }
}
