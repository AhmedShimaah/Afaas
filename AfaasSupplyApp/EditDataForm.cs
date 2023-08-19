using OfficeOpenXml;
using System;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace AfaasSupplyApp
{
    public partial class EditDataForm : Form
    {
        public EditDataForm()
        {
            InitializeComponent();
            comboBox1.SelectedItem = "Pending";
        }

        private void searchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                LoadDataFromExcel(searchBox.Text);
            }
        }

        private void LoadDataFromExcel(string invoiceNumber)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            string filePath = Properties.Settings.Default.DefaultExcelFilePath;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int totalRows = worksheet.Dimension.End.Row;
                bool invoiceExists = false;

                for (int row = 1; row <= totalRows; row++)
                {
                    if (worksheet.Cells[row, 3].Text.Equals(invoiceNumber))
                    {
                        textBox1.Text = worksheet.Cells[row, 3].Text;
                        textBox2.Text = worksheet.Cells[row, 2].Text;
                        textBox3.Text = worksheet.Cells[row, 8].Text;
                        textBox4.Text = worksheet.Cells[row, 4].Text;
                        textBox5.Text = worksheet.Cells[row, 6].Text;
                        textBox6.Text = worksheet.Cells[row, 5].Text;
                        comboBox1.SelectedItem = worksheet.Cells[row, 9].Text;

                        invoiceExists = true;
                        break;
                    }
                }

                if (!invoiceExists)
                {
                    MessageBox.Show("This invoice number does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ClearAllFields();
                }
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (!ValidateFields()) return;

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            string filePath = Properties.Settings.Default.DefaultExcelFilePath;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int totalRows = worksheet.Dimension.End.Row;
                bool found = false;

                for (int row = 1; row <= totalRows; row++)
                {
                    if (worksheet.Cells[row, 3].Text.Equals(textBox1.Text))
                    {
                        found = true;
                        var result = MessageBox.Show($"The invoice number exists.\n\nCurrent Data:\n{worksheet.Cells[row, 2].Text}, {worksheet.Cells[row, 3].Text}, ... \n\nNew Data:\n{textBox2.Text}, {textBox3.Text}, ...\n\nDo you want to save changes?", "Confirm", MessageBoxButtons.YesNo);
                        if (result == DialogResult.No) return;

                        worksheet.Cells[row, 2].Value = DateTime.Parse(textBox2.Text);
                        worksheet.Cells[row, 3].Value = textBox1.Text;
                        worksheet.Cells[row, 8].Value = textBox3.Text;
                        worksheet.Cells[row, 4].Value = Convert.ToDouble(textBox4.Text);
                        worksheet.Cells[row, 6].Value = Convert.ToDouble(textBox5.Text);
                        worksheet.Cells[row, 5].Value = Convert.ToDouble(textBox6.Text);
                        worksheet.Cells[row, 9].Value = comboBox1.SelectedItem.ToString();

                        break;
                    }
                }

                if (!found)
                {
                    int newRow = totalRows + 1;
                    while (string.IsNullOrWhiteSpace(worksheet.Cells[newRow, 1].Text) && newRow > 0)
                    {
                        newRow--;
                    }
                    newRow++;

                    worksheet.Cells[newRow, 2].Value = DateTime.Parse(textBox2.Text);
                    worksheet.Cells[newRow, 3].Value = textBox1.Text;
                    worksheet.Cells[newRow, 8].Value = textBox3.Text;
                    worksheet.Cells[newRow, 4].Value = Convert.ToDouble(textBox4.Text);
                    worksheet.Cells[newRow, 6].Value = Convert.ToDouble(textBox5.Text);
                    worksheet.Cells[newRow, 5].Value = Convert.ToDouble(textBox6.Text);
                    worksheet.Cells[newRow, 9].Value = comboBox1.SelectedItem.ToString();
                }

                SetExcelFormulas(worksheet);

                package.Save();
                MessageBox.Show("Data saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearAllFields();
            }
        }

        private void ClearAllFields()
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox5.Clear();
            textBox6.Clear();
            searchBox.Clear();
            comboBox1.SelectedItem = null;
        }

        private void mainMenuButton_Click(object sender, EventArgs e)
        {
            this.Hide();  // Hide this form

            // Check if Form1 is already open
            var form1Instance = Application.OpenForms.OfType<Form1>().FirstOrDefault();

            if (form1Instance != null)
            {
                // If Form1 is already open, bring it to the front
                form1Instance.BringToFront();
            }
            else
            {
                // If Form1 is not open, create a new instance and show it
                Form1 formone = new Form1();
                formone.Show();
            }

            this.Close();  // Close the EditDataForm
        }


        private void deleteButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to delete this row?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                string filePath = Properties.Settings.Default.DefaultExcelFilePath;

                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    int totalRows = worksheet.Dimension.End.Row;
                    bool rowDeleted = false;

                    for (int row = 1; row <= totalRows; row++)
                    {
                        if (worksheet.Cells[row, 3].Text.Equals(textBox1.Text))
                        {
                            worksheet.DeleteRow(row);
                            rowDeleted = true;
                            break;
                        }
                    }

                    if (rowDeleted)
                    {
                        package.Save();
                        MessageBox.Show("Row deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearAllFields();
                    }
                    else
                    {
                        MessageBox.Show("The given invoice number does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private bool ValidateFields()
        {
            if (!ValidateInvoiceNumber(textBox1.Text))
            {
                MessageBox.Show("Invalid Invoice Number Format. It should be in the format 000/YY", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox1.Focus();
                return false;
            }

            if (!DateTime.TryParse(textBox2.Text, out _))
            {
                MessageBox.Show("Invalid Date Format. It should be in the format DD/MM/YYYY", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox2.Focus();
                return false;
            }

            if (textBox3.Text.Length > 20)
            {
                MessageBox.Show("Resort Name exceeds the maximum character limit of 20.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox3.Focus();
                return false;
            }

            if (!double.TryParse(textBox4.Text, out _) || !double.TryParse(textBox5.Text, out _) || !double.TryParse(textBox6.Text, out _))
            {
                MessageBox.Show("Please enter valid numeric values for Total Cost, Invoice Amount, and GST.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Please select a payment status.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                comboBox1.Focus();
                return false;
            }

            return true;
        }

        private bool ValidateInvoiceNumber(string invoice)
        {
            string pattern = @"^([0-9]{1,3})/(\d{2})$";
            if (Regex.IsMatch(invoice, pattern))
            {
                Match match = Regex.Match(invoice, pattern);
                int firstPart = int.Parse(match.Groups[1].Value);
                string secondPart = DateTime.Now.Year.ToString().Substring(2);

                return firstPart >= 1 && firstPart <= 999;
            }
            return false;
        }

        private void SetExcelFormulas(ExcelWorksheet worksheet)
        {
            worksheet.Cells["J1"].Value = "Total Profit";
            worksheet.Cells["L1"].Value = "Total Cost";
            worksheet.Cells["N1"].Value = "Invoice Total";
            worksheet.Cells["P1"].Value = "GST Total";

            int lastRow = worksheet.Dimension.End.Row;

            worksheet.Cells["K1"].Formula = $"SUM(G2:G{lastRow})";
            worksheet.Cells["M1"].Formula = $"SUM(D2:D{lastRow})";
            worksheet.Cells["O1"].Formula = $"SUM(F2:F{lastRow})";
            worksheet.Cells["Q1"].Formula = $"SUM(E2:E{lastRow})";

            // Convert entire columns to "General" number format
            worksheet.Column(7).Style.Numberformat.Format = "General"; // Column G
            worksheet.Column(4).Style.Numberformat.Format = "General"; // Column D
            worksheet.Column(6).Style.Numberformat.Format = "General"; // Column F
            worksheet.Column(5).Style.Numberformat.Format = "General"; // Column E
            worksheet.Column(2).Style.Numberformat.Format = "DD/MM/YYYY";
        }

    }
}
