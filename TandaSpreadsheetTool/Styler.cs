using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TandaSpreadsheetTool
{
    public partial class StylerForm : Form
    {
        SpreadSheetStyle currentStyle;
     
      

        public StylerForm()
        {
            InitializeComponent();

         
        }

        public void UpdateStyle(SpreadSheetStyle newStyle)
        {
            currentStyle = newStyle;

        }

        void UpdateForm()
        {

        }

        private Color GetColorFromByte(byte[] rgb)
        {
            return Color.FromArgb(rgb[0], rgb[1], rgb[2]);
        }

        private void btnNameHeadCl_Click(object sender, EventArgs e)
        {
            var colourDialog = new ColorDialog();
            colourDialog.ShowDialog();
        }
    }
}
