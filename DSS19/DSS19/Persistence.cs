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

namespace DSS19
{
    class Persistence //model
    {
        private string dbpath = "";
        public string factory = "";
        private IDbConnection conn = null;

        public Persistence(string dbpath)
        {
            this.dbpath = dbpath;
        }

        // legge una stringa con i codici clienti da graficare
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
    }
}
