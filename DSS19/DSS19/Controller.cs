using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Configuration;
using System.Drawing;
using PyGAP2019;
using System.IO;

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

        public string readAllCustomers()
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

        public string readSingleCustomer()
        {
            return P.readRandomCustomer();
        }

        // restituisce le previsioni di uno specifico customer
        public async Task<string> readCustomersOrdersPrevision(string pythonScript, string customer)
        {
            string fcast = "";

            Trace.WriteLine("Getting the orders chart (prevision) ...");
            pythonScriptPath = System.IO.Path.GetFullPath(pythonScriptPath);
            try
            {
                string list = await pyRunner.getStringsAsync(
                    this.pythonScriptPath,
                    pythonScript,
                    this.pythonScriptPath,
                    this.dbPath,
                    customer);

                string[] lines = list.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string s in lines)
                {
                    if (s.StartsWith("Actual"))
                    {
                        fcast = fcast + Environment.NewLine + s;
                    }
                }

                return fcast;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"[ReadCustomerOrdersChart]: {e.ToString()}");
                return null;
            }
        }
        
        // restituisce una previsione per ogni customer
        public async Task<double> readLastCustomerOrdersPrevision(string pythonScript, string customer)
        {
            string fcast = "";

            pythonScriptPath = System.IO.Path.GetFullPath(pythonScriptPath);
            try
            {
                string list = await pyRunner.getStringsAsync(
                    this.pythonScriptPath,
                    pythonScript,
                    this.pythonScriptPath,
                    this.dbPath,
                    customer);

                string[] lines = list.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string s in lines)
                {
                    if (s.StartsWith("Actual"))
                    {
                        fcast = fcast + Environment.NewLine + s;
                        
                    }
                }

                string result = fcast.Substring(fcast.LastIndexOf(" "));

                //return Math.Round(double.Parse(result));
                return double.Parse(result);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"[ReadCustomerOrdersChart]: {e.ToString()}");
                return 0;
            }
        }

        // restituiesce la lista contenente l'ultima previsione di ciascun coustomer
        public async Task<double[]> getPrevisionsList(string pythonScript)
        {
            string customers = readAllCustomers();
            char[] spearator = { ',' };

            string[] customersList = customers.Split(spearator, StringSplitOptions.RemoveEmptyEntries);

            List<double> previsions = new List<double>();
            foreach (string customer in customersList)
            {
                double prevision = await readLastCustomerOrdersPrevision(pythonScript, customer);
                previsions.Add(prevision);
                Trace.WriteLine(customer +": "+ prevision);

            }
            return previsions.ToArray();
        }

        public async void optimizeGAP()
        {
            if (P.checkFile())
            {
                P.setGFromFile();
            }
            else
            {
                double[] list = await getPrevisionsList("arima_forecast.py");
                P.setGFromData(list);
            }

            P.readGAPinstance(this.dbPath);
            GAPclass G = P.getG();
            double zub = G.simpleContruct();
            Trace.WriteLine($"Constructive, zub = {zub}");
            zub = G.opt10(G.c);
            Trace.WriteLine($"Local search, zub = {zub}");
            zub = G.TabuSearch(30, 100);
            Trace.WriteLine($"Tabu search, zub = {zub}");

        }
    }

}

