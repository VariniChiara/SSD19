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
        public string connectionString = "";
        public string factory = "";
        private IDbConnection conn = null;
       
        private IDbConnection OpenConnection()
        {
            DbProviderFactory dbFactory = DbProviderFactories.GetFactory(factory);
            conn = dbFactory.CreateConnection();
            try
            {
                conn.ConnectionString = connectionString;
                Trace.WriteLine("[PERSISTANCE] Connessione DB aperta");
                conn.Open();
                return conn;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("[PERSISTANCE] errore: " + ex.Message);
            }
            return null;
        }

        private IDataReader ExecuteQuery(string queryText)
        {
            conn = OpenConnection();
            IDbCommand com = conn.CreateCommand();
            try
            {
                com.CommandText = queryText;
                IDataReader reader = com.ExecuteReader();
                Trace.WriteLine("[PERSISTANCE] query done ");
                return reader;
            }
            catch (Exception ex)
            {
                errorLog("executeQuery " + ex.Message);
            }
            return null;
        }

        public void SelectTop100() 
        {
            try 
            {
                string queryText = "select TOP (100) id, customer, time, quant from ordini ";
                if (factory == "System.Data.SQLite")
                {
                    queryText = "select id, customer, time, quant from ordini LIMIT 100 "; //sqlLite
                }

                using (IDataReader reader = ExecuteQuery(queryText))
                {
                    while (reader.Read())
                    {
                        Trace.WriteLine(reader["id"] + " " + reader["customer"] + " " + reader["time"] + " " + reader["quant"]); //view.textConsole = ...
                    }
                }
            }
            catch(Exception ex)
            {
                errorLog("allOrder " + ex.Message);
            } 
            finally
            {
                Trace.WriteLine("[PERSISTANCE] fine lettura dati ");
                conn.Close();
            }
            
        }

        public void SelectCustOrders(string custID) 
        {
            List<int> quantLst = new List<int>();
            try
            {
                string queryText = "SELECT id, customer, time, quant from ordini where customer = \'"+custID+"\'";
                using (IDataReader reader = ExecuteQuery(queryText))
                {
                    while (reader.Read())
                    {
                        Trace.WriteLine(reader["id"] + " " + reader["customer"] + " " + reader["time"] + " " + reader["quant"]); //view.textConsole = ...
                        quantLst.Add(Convert.ToInt32(reader["quant"]));
                    }
                }
            }
            catch (Exception ex)
            {
                errorLog("customerID " + ex.Message);
            } 
            finally
            {
                Trace.WriteLine("Quantità: " + string.Join(",", quantLst));
                Trace.WriteLine("[PERSISTANCE] fine lettura dati ");
                conn.Close();
            }
        }

        public void Insert(string cust)
        {
            try
            {
                string queryText = @"INSERT INTO ordini (customer) VALUES ('"+cust+"') ";
                using (IDataReader reader = ExecuteQuery(queryText))
                {
                    SelectCustOrders(cust); //to verify the insert
                }             
            }
            catch (Exception ex)
            {
                errorLog("insert " + ex.Message);
            } 
            finally
            { 
                Trace.WriteLine("[PERSISTANCE] insert done");
                conn.Close();
            }
        }
        
        public void Delete(string cust )
        {
            try
            {
                string queryText = @"DELETE FROM ordini WHERE customer = '"+cust+"'";
                using (IDataReader reader = ExecuteQuery(queryText))
                {
                    SelectCustOrders(cust); //to verify the insert
                }
            }
            catch (Exception ex)
            {
                errorLog("delete " + ex.Message);
            }
            finally
            {
                Trace.WriteLine("[PERSISTANCE] insert done");
                conn.Close();
            }
        }
        
        public void Update(string oldCust, string newCust)
        {
            try
            {
                string queryText = @"UPDATE ordini SET customer = '"+newCust+"'  WHERE customer = '" + oldCust + "'";
                using(IDataReader reader = ExecuteQuery(queryText))
                {
                    SelectCustOrders(newCust); //to verify the insert
                }
            }
            catch (Exception ex)
            {
                errorLog("update " + ex.Message);
            }
            finally
            {
                Trace.WriteLine("[PERSISTANCE] update done");
                conn.Close();
            }           
        }

        private void errorLog(string errTxt)
        {
            Trace.WriteLine("[PERSISTANCE] errore: " + errTxt);
        }

        // legge una stringa con i codici clienti da graficare
        public string readCustomerListORM(string dbpath, int n)
        {
            List<string> lstClienti;
            string ret = "Error reading DB";
            try
            {
            //var ctx = new SQLiteDatabaseContext(dbpath);
                using (var ctx = new SQLiteDatabaseContext(dbpath))
                {
                    lstClienti = ctx.Database.SqlQuery<string>("SELECT distinct customer from ordini").ToList();
                }
            // legge solo alcuni clienti (si poteva fare tutto nella query)
                List<string> lstOutStrings = new List<string>();
                Random r = new Random(550);
                while (lstOutStrings.Count<n)
                {
                    int randomIndex = r.Next(0, lstClienti.Count); //Choose a random object in the list
                    lstOutStrings.Add("'" + lstClienti[randomIndex] + "'"); //add it to the new, random list
                    lstClienti.RemoveAt(randomIndex); //remove to avoid duplicates
                }
                ret = string.Join(",", lstOutStrings);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error: {ex.Message}");
            }

            return ret;
        }

        //legge tutti gli ordini di un cliente
        public string readCustomerOrderORM(string dbpath, string custID)
        {
            List<int> lstOrder;
            string ret = "Error reading DB";
            try
            {
                using (var ctx = new SQLiteDatabaseContext(dbpath))
                {
                    lstOrder = ctx.Database.SqlQuery<int>("SELECT quant from ordini where customer = '" + custID+"'").ToList();
                }
                Trace.WriteLine(lstOrder.Count);
                ret = "";
                lstOrder.ForEach(o => ret = ret + ","+ o );
            }
            catch (Exception ex)
            {
                errorLog(ex.Message);
            }

            return ret;
        }
    }
}
