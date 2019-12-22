using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Data;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.Data.Common;
using System.IO;
using PyGAP2019;

namespace DSS19
{
    class Persistence //model
    {
        private string dbpath;
        public string factory;
        private GAPclass G;
        private IDbConnection conn = null;

        public Persistence(string dbpath)
        {
            this.dbpath = dbpath;
            this.factory = "";
            this.G = new GAPclass();
    }

        // restituisce la lista di tutti i customers
        public string readCustomerListORM()
        {
            List<string> lstClienti;
            string ret = "Error reading DB";
            try
            {
            //var ctx = new SQLiteDatabaseContext(dbpath);
                using (var ctx = new SQLiteDatabaseContext(this.dbpath))
                {
                    lstClienti = ctx.Database.SqlQuery<string>("SELECT distinct customer from ordini").ToList();
                }
          
                List<string> lstOutStrings = new List<string>();
                lstClienti.ForEach(c => lstOutStrings.Add("'" + c + "'"));
                ret = string.Join(",", lstOutStrings);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error: {ex.Message}");
            }

            return ret;
        }

        //restituisce un customer random
        public string readRandomCustomer()
        {
            string ret = "Error reading DB";
            try
            {
                //var ctx = new SQLiteDatabaseContext(dbpath);
                using (var ctx = new SQLiteDatabaseContext(this.dbpath))
                {
                   ret = ctx.Database.SqlQuery<string>("SELECT distinct customer from ordini ORDER BY RANDOM() LIMIT 1").SingleOrDefault();
                }

                ret = "'" + ret + "'";
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error: {ex.Message}");
            }

            return ret;
        }

        public Boolean checkFile()
        {
            return File.Exists("GAPreq.dat");
        }

        public void setGFromFile()
        {
            string[] txtData = File.ReadAllLines("GAPreq.dat");
            G.req = Array.ConvertAll<string, int>(txtData, new Converter<string, int>(i => int.Parse(i)));
        }

        public void setGFromData(double[] list)
        {
            G.req = Array.ConvertAll<double, int>(list, new Converter<double, int>(i => System.Convert.ToInt32(i)));
            File.WriteAllLines("GAPreq.dat", G.req.Select(x => x.ToString()));
        }

        public GAPclass getG()
        {
            return this.G;
        }


        // Reads an instance from the db
        public void readGAPinstance(string dbOrdinipath)
        {
            int i, j;
            List<int> lstCap;
            List<double> lstCosts;

            try
            {
                using (var ctx = new SQLiteDatabaseContext(dbOrdinipath))
                {
                    lstCap = ctx.Database.SqlQuery<int>("SELECT cap from capacita").ToList();
                    G.m = lstCap.Count();
                    G.cap = new int[G.m];
                    for (i = 0; i < G.m; i++)
                        G.cap[i] = lstCap[i];

                    lstCosts = ctx.Database.SqlQuery<double>("SELECT cost from costi").ToList();
                    G.n = lstCosts.Count / G.m;
                    G.c = new double[G.m, G.n];
                    G.req = new int[G.n];
                    G.sol = new int[G.n];
                    G.solbest = new int[G.n];
                    G.zub = Double.MaxValue;
                    G.zlb = Double.MinValue;

                    for (i = 0; i < G.m; i++)
                        for (j = 0; j < G.n; j++)
                            G.c[i, j] = lstCosts[i * G.n + j];

                    for (j = 0; j < G.n; j++)
                        G.req[j] = -1;          // placeholder
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("[readGAPinstance] Error:" + ex.Message);
            }

            Trace.WriteLine("Fine lettura dati istanza GAP");
        }
    }
}
