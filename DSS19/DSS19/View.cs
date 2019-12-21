using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace DSS19
{
    public partial class App : Form
    {

        private Controller C;
        TextBoxTraceListener _textBoxListener;

        public App()
        {
            InitializeComponent();
            _textBoxListener = new TextBoxTraceListener(txtConsole);
            Trace.Listeners.Add(_textBoxListener);
            C = new Controller();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            cleanView();
        }

        private void cleanView()
        {
            txtConsole.Text = "";
        }

        private void readDBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            readDb();
        }

        private void readDb()
        {
            txtConsole.AppendText("Read Db clicked \n");
            //C.readDb(txtCustomer.Text);
        }

        private void selectSomeClientsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtConsole.AppendText("Select some clients \n");
            //C.readCustomerList();
        }

        private void selectCust1OrderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtConsole.AppendText("Select cust1 \n");
            //C.readCustomerOrder();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            loadOrdersTrendChart();
        }

        private async void loadOrdersTrendChart()
        {
            txtConsole.AppendText("Load Db button clicked \n");
            string customers = C.loadOrdersTrend();
            Bitmap bmp = await C.readCustomerOrdersChart("chartOrders.py", customers);
            pictureBox2.Image = bmp;
        }
        private void toolStripLabel1_Click(object sender, EventArgs e)
        {
            loadArima();
        }

        private async void loadArima()
        {
            string customer = C.readSingleCutomer();
            Bitmap bmp = await C.readCustomerOrdersChart("arima_forecast.py", customer);
            pictureBox2.Image = bmp;
            // string prevision = await C.
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void txtConsole_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void txtCustomer_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void App_Load(object sender, EventArgs e)
        {

        }
    }
}
