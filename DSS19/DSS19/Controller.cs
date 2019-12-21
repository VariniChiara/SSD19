using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Configuration;
using System.Drawing;
using PyGAP2019;

namespace DSS19
{

    class Controller 
    {
        private Persistence P;
        private string pythonPath;
        private string pythonScriptPath;
        private PythonRunner pyRunner;
        private string dbPath = "";

        public Controller()
        {
            this.dbPath = ConfigurationManager.AppSettings["dbordiniFile"];
            this.pythonPath = ConfigurationManager.AppSettings["pythonPath"];
            this.pythonScriptPath = ConfigurationManager.AppSettings["pyScripts"];
            this.pyRunner = new PythonRunner(this.pythonPath, 20000);
            P = new Persistence(dbPath);

        }

        public string loadOrdersTrend()
        {
            return P.readCustomerListORM();
        }

        public async Task<Bitmap> readCustomerOrdersChart(string pyScript, string customerStr)
        {
            Trace.WriteLine("Getting the orders chart ...");

            pythonScriptPath = System.IO.Path.GetFullPath(pythonScriptPath);
            try
            {
                Bitmap bmp = await this.pyRunner.getImageAsync(
                    this.pythonScriptPath,
                    pyScript,
                    this.pythonScriptPath,
                    this.dbPath,
                    customerStr);
                return bmp;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"[ReadCustomerOrdersChart]: {e.ToString()}");
                return null;
            }
        }

        public string readSingleCutomer()
        {
            return P.readRandomCustomer();
        }

    }
}
