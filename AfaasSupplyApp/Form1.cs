using System;
using System.IO;
using System.Windows.Forms;
using OfficeOpenXml;

namespace AfaasSupplyApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LoadDefaultExcelFilePath();
            LoadDataToGrid();
        }

        private void LoadDefaultExcelFilePath()
        {
            var defaultFilePath = Properties.Settings.Default.DefaultExcelFilePath;

            if (!string.IsNullOrEmpty(defaultFilePath))
            {
                lblSelectedFile.Text = defaultFilePath;
            }
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
                openFileDialog.Title = "Select an Excel File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Properties.Settings.Default.DefaultExcelFilePath = openFileDialog.FileName;
                    Properties.Settings.Default.Save();

                    lblSelectedFile.Text = openFileDialog.FileName;
                    LoadDataToGrid();
                }
            }
        }


        private (double TotalProfit, double TotalCost, double InvoiceAmount, double GST, double PendingTotalInvoice, double PendingTotalProfit) ReadExcelValues(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];

                worksheet.Calculate();

                double totalProfit = Convert.ToDouble(worksheet.Cells["K1"].Value);
                double totalCost = Convert.ToDouble(worksheet.Cells["M1"].Value);
                double invoiceAmount = Convert.ToDouble(worksheet.Cells["O1"].Value);
                double gst = Convert.ToDouble(worksheet.Cells["Q1"].Value);

                double pendingTotalInvoice = worksheet.Cells[2, 6, worksheet.Dimension.End.Row, 6]
                    .Where(cell => worksheet.Cells[cell.Start.Row, 9].Text == "Pending")
                    .Sum(cell => Convert.ToDouble(cell.Text));

                // New logic to sum values of Column G (Profit) where Payment Status is "Pending"
                double pendingTotalProfit = worksheet.Cells[2, 7, worksheet.Dimension.End.Row, 7]
                    .Where(cell => worksheet.Cells[cell.Start.Row, 9].Text == "Pending")
                    .Sum(cell => Convert.ToDouble(cell.Text));

                return (totalProfit, totalCost, invoiceAmount, gst, pendingTotalInvoice, pendingTotalProfit);
            }
        }



        private void LoadDataToGrid()
        {
            string filePath = Properties.Settings.Default.DefaultExcelFilePath;
            var values = ReadExcelValues(filePath);

            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            dataGridView1.Columns.Add("Description", "Description");
            dataGridView1.Columns.Add("Value", "Value");

            dataGridView1.Rows.Add("Total Profit", values.TotalProfit.ToString("N2"));  // Format as number with two decimal places
            dataGridView1.Rows.Add("Total Cost", values.TotalCost.ToString("N2"));
            dataGridView1.Rows.Add("Invoice Amount", values.InvoiceAmount.ToString("N2"));
            dataGridView1.Rows.Add("GST", values.GST.ToString("N2"));
            dataGridView1.Rows.Add("Pending Total Invoice", values.PendingTotalInvoice.ToString("N2"));
            dataGridView1.Rows.Add("Pending Total Profit", values.PendingTotalProfit.ToString("N2"));
        }



        private void AddButton_Click(object sender, EventArgs e)
        {
            using (EditDataForm editForm = new EditDataForm())
            {
                this.Hide();
                editForm.ShowDialog();
                
            }
        }
    }
}
