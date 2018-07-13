using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Drawing.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TandaSpreadsheetTool
{
    public partial class StylerForm : Form
    {
        SpreadSheetStyle currentStyle;


        
        

        public StylerForm(SpreadSheetStyle style)
        {
            InitializeComponent();
            currentStyle = style;
            SetFormToStyle();
           
            
            using (var fontCollection = new InstalledFontCollection())
            {
                var fontFamilies = fontCollection.Families;
                foreach(var font in fontFamilies)
                {
                    lstBxFont.Items.Add(font.Name);
                }
            }
        }


        public void UpdateStyle(SpreadSheetStyle newStyle)
        {            
            if (!Visible)
            {
                currentStyle = newStyle;
                SetFormToStyle();
            }            
        }

        

       public void SetFormToStyle()
        {
            pnlDate.BackColor = GetColorFromByte(currentStyle.dateCl);
            pnlDayName.BackColor = GetColorFromByte(currentStyle.dayNameCl);
            pnlNameField.BackColor = GetColorFromByte(currentStyle.nameFieldCl);
            pnlNameHeadCL.BackColor = GetColorFromByte(currentStyle.nameHeadingCl);
            pnlRotaEmptyCl.BackColor = GetColorFromByte(currentStyle.rotaEmptyCl);
            pnlRotaField.BackColor = GetColorFromByte(currentStyle.rotaFieldCl);

           

            var fontSelection = lstBxFont.FindString("Calibri");

            lstBxFont.SelectedIndex = fontSelection != -1 ? fontSelection : lstBxFont.FindString("Arial");
            cBxDiv.SelectedIndex = (int)currentStyle.divBy;
            
            tkBarBrightness.Value = Convert.ToInt32(currentStyle.minBrightness * 100);
            var brightness = Convert.ToByte(Math.Round((double)(currentStyle.minBrightness / 100) * 255));
            pnlMinBright.BackColor = GetColorFromByte(new byte[] { brightness, brightness, brightness });
        }


        private Color GetColorFromByte(byte[] rgb)
        {
            return Color.FromArgb(rgb[0], rgb[1], rgb[2]);
        }

        private byte[] GetByteFromColor(Color color)
        {
            return new byte[]
            {
                Convert.ToByte(color.R),
                Convert.ToByte(color.G),
                Convert.ToByte(color.B)
            };
        }

        public SpreadSheetStyle Style
        {
            get
            {
                return currentStyle;


            }
        }

        private void btnNameHeadCl_Click(object sender, EventArgs e)
        {
           if (cD.ShowDialog() == DialogResult.OK)
            {
                pnlNameHeadCL.BackColor = cD.Color;
            }
        }

        private void btnNameFieldCL_Click(object sender, EventArgs e)
        {
            if (cD.ShowDialog() == DialogResult.OK)
            {
               pnlNameField.BackColor = cD.Color;
            }
        }

        private void btnRotaFieldCl_Click(object sender, EventArgs e)
        {
            if (cD.ShowDialog() == DialogResult.OK)
            {
               pnlRotaField.BackColor = cD.Color;
            }
        }

        private void btnRotaEmptyCl_Click(object sender, EventArgs e)
        {
            if (cD.ShowDialog() == DialogResult.OK)
            {
                pnlRotaEmptyCl.BackColor = cD.Color;
            }
        }
             
        void SetStyleToFormOptions()
        {
            currentStyle.nameHeadingCl = GetByteFromColor(pnlNameHeadCL.BackColor);
            currentStyle.nameFieldCl= GetByteFromColor(pnlNameField.BackColor);
            currentStyle.rotaFieldCl = GetByteFromColor(pnlRotaField.BackColor);
            currentStyle.rotaEmptyCl = GetByteFromColor(pnlRotaEmptyCl.BackColor);
            currentStyle.dayNameCl = GetByteFromColor(pnlDayName.BackColor);
            currentStyle.dateCl = GetByteFromColor(pnlDate.BackColor);

            currentStyle.boldHeadings = ckBxBoldHead.Checked;
            currentStyle.useTeamCls = ckBxTeamColours.Checked;

            currentStyle.minBrightness = (float)tkBarBrightness.Value/100;
            currentStyle.colWidth = (int)nUDColWidth.Value;
            currentStyle.useTeamCls = ckBxTeamColours.Checked;
            currentStyle.font = lstBxFont.GetItemText(lstBxFont.SelectedItem);

            currentStyle.divBy = (SpreadSheetDiv)cBxDiv.SelectedIndex;
        }

        private void btnDayNameCl_Click(object sender, EventArgs e)
        {
            if (cD.ShowDialog() == DialogResult.OK)
            {
                pnlDayName.BackColor = cD.Color;
            }

        }

        private void btnDateCl_Click(object sender, EventArgs e)
        {
            if (cD.ShowDialog() == DialogResult.OK)
            {
                pnlDate.BackColor = cD.Color;
            }
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            SetStyleToFormOptions();
            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            SetFormToStyle();
            DialogResult = DialogResult.Cancel;
        }

        private void tkBarBrightness_Scroll(object sender, EventArgs e)
        { 
            var brightValue= ((double)tkBarBrightness.Value / 100) * 255;
            var brightByte = Convert.ToByte(brightValue);
            pnlMinBright.BackColor = GetColorFromByte(new byte[] { brightByte, brightByte, brightByte });
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            var dResult = MessageBox.Show("Reset Values to Default?", "Reset style", MessageBoxButtons.YesNo);
            if (dResult == DialogResult.Yes)
            {
                currentStyle = SpreadSheetStyle.Default();
                SetFormToStyle();
            }
            
        }
    }
}
