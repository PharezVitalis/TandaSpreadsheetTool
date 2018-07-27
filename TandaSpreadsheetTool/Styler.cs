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

            cBxRotaAlign.DropDownStyle = ComboBoxStyle.DropDownList;
            cBxNameAlign.DropDownStyle = ComboBoxStyle.DropDownList;
            cBxHeadAlign.DropDownStyle = ComboBoxStyle.DropDownList;
            cBxDiv.DropDownStyle = ComboBoxStyle.DropDownList;

            lblColWidthVal.Text = "";
            lblBrightVal.Text = "";

            SetFormToStyle();
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
            pnlShiftHeadCl.BackColor = GetColorFromByte(currentStyle.tlShiftHeadCl);
            pnlTotalShiftFdCl.BackColor = GetColorFromByte(currentStyle.tlShiftFieldCl);
            ckBxShiftAnalysis.Checked = currentStyle.shiftAnalysis;

            fontD.Font = new Font(currentStyle.font, currentStyle.fontSize, GetFontStyle());
            // need to set: col width 

            cBxDiv.SelectedIndex = (int)currentStyle.divBy;

            switch (currentStyle.useVertDates)
            {
                case UseVerticalDates.AUTO:
                    rbtnVertDateAuto.Checked = true;
                    break;
                case UseVerticalDates.FALSE:
                    rbtnVertDateNo.Checked = true;
                    break;
                case UseVerticalDates.TRUE:
                    rbtnVertDateYes.Checked = true;
                    break;
            }

            cBxHeadAlign.SelectedIndex = (int)currentStyle.headAlign-1;
            cBxNameAlign.SelectedIndex = (int)currentStyle.nameAlign-1;
            cBxRotaAlign.SelectedIndex = (int)currentStyle.rotaAlign - 1;

            tkBarColumnWidth.Value = (int)Math.Round(currentStyle.colWidth * 100);
            lblColWidthVal.Text = Convert.ToString(((double)tkBarColumnWidth.Value) / 100);

            tkBarBrightness.Value = Convert.ToInt32(currentStyle.minBrightness * 100);
            lblBrightVal.Text = tkBarBrightness.Value + "%";
            var brightness = Convert.ToByte(Math.Round((double)currentStyle.minBrightness  * 255));
            pnlMinBright.BackColor = Color.FromArgb(brightness, brightness, brightness);



            Refresh();
        }

        void SetStyleToForm()
        {
            currentStyle.nameHeadingCl = GetByteFromColor(pnlNameHeadCL.BackColor);
            currentStyle.nameFieldCl = GetByteFromColor(pnlNameField.BackColor);
            currentStyle.rotaFieldCl = GetByteFromColor(pnlRotaField.BackColor);
            currentStyle.rotaEmptyCl = GetByteFromColor(pnlRotaEmptyCl.BackColor);
            currentStyle.dayNameCl = GetByteFromColor(pnlDayName.BackColor);
            currentStyle.dateCl = GetByteFromColor(pnlDate.BackColor);
            currentStyle.tlShiftFieldCl = GetByteFromColor(pnlTotalShiftFdCl.BackColor);
            currentStyle.tlShiftHeadCl = GetByteFromColor(pnlShiftHeadCl.BackColor);

            currentStyle.boldHeadings = ckBxBoldHead.Checked;
            

            currentStyle.minBrightness = (float)tkBarBrightness.Value / 100;
            currentStyle.colWidth = (float)tkBarColumnWidth.Value / 100;
            

            currentStyle.underLineFs = fontD.Font.Underline;
            currentStyle.strikeThroughFs = fontD.Font.Strikeout;
            currentStyle.fontSize = (int)fontD.Font.Size;
            currentStyle.boldFs = fontD.Font.Bold;
            currentStyle.italicFs = fontD.Font.Italic;
            currentStyle.font = fontD.Font.Name;

           

            if (rbtnVertDateAuto.Checked)
            {
                currentStyle.useVertDates = UseVerticalDates.AUTO;
            }
            else if (rbtnVertDateYes.Checked)
            {
                currentStyle.useVertDates = UseVerticalDates.TRUE;
            }
            else
            {
                currentStyle.useVertDates = UseVerticalDates.FALSE;
            }

            currentStyle.headAlign = (NPOI.SS.UserModel.HorizontalAlignment)cBxHeadAlign.SelectedIndex + 1;
            currentStyle.nameAlign = (NPOI.SS.UserModel.HorizontalAlignment)cBxNameAlign.SelectedIndex + 1;
            currentStyle.rotaAlign = (NPOI.SS.UserModel.HorizontalAlignment)cBxRotaAlign.SelectedIndex + 1;

            currentStyle.divBy = (SpreadSheetDiv)cBxDiv.SelectedIndex;
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
           if (clrD.ShowDialog() == DialogResult.OK)
            {
                pnlNameHeadCL.BackColor = clrD.Color;
            }
        }

        private void btnNameFieldCL_Click(object sender, EventArgs e)
        {
            if (clrD.ShowDialog() == DialogResult.OK)
            {
               pnlNameField.BackColor = clrD.Color;
            }
        }

        private void btnRotaFieldCl_Click(object sender, EventArgs e)
        {
            if (clrD.ShowDialog() == DialogResult.OK)
            {
               pnlRotaField.BackColor = clrD.Color;
            }
        }

        private void btnRotaEmptyCl_Click(object sender, EventArgs e)
        {
            if (clrD.ShowDialog() == DialogResult.OK)
            {
                pnlRotaEmptyCl.BackColor = clrD.Color;
            }
        }       

        private FontStyle GetFontStyle()
        {
            var style = 0;

            if (currentStyle.italicFs)
            {
                style += 2;
            }

            if (currentStyle.boldFs)
            {
                style += 1;
            }

            if (currentStyle.strikeThroughFs)
            {
                style += 8;
            }

            if (currentStyle.underLineFs)
            {
                style += 4;
            }

            return (FontStyle)style;
        }

        private void btnDayNameCl_Click(object sender, EventArgs e)
        {
            if (clrD.ShowDialog() == DialogResult.OK)
            {
                pnlDayName.BackColor = clrD.Color;
            }

        }

        private void btnDateCl_Click(object sender, EventArgs e)
        {
            if (clrD.ShowDialog() == DialogResult.OK)
            {
                pnlDate.BackColor = clrD.Color;
            }
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            SetStyleToForm();
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
            lblBrightVal.Text = tkBarBrightness.Value + "%";
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

        private void btnFonts_Click(object sender, EventArgs e)
        {
            
           
            if (fontD.ShowDialog() != DialogResult.OK)
            {
                fontD.Font = new Font(currentStyle.font, currentStyle.fontSize, GetFontStyle());

            }
            
            
            
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            lblColWidthVal.Text = Convert.ToString(((double)tkBarColumnWidth.Value)/ 100);
        }

        private void btnShiftHeadCl_Click(object sender, EventArgs e)
        {
            if (clrD.ShowDialog() == DialogResult.OK)
            {
                pnlShiftHeadCl.BackColor = clrD.Color;
            }
           
        }

        private void btnTotShiftFd_Click(object sender, EventArgs e)
        {
            if (clrD.ShowDialog() == DialogResult.OK)
            {
                pnlTotalShiftFdCl.BackColor = clrD.Color;
            }
        }
    }
}
