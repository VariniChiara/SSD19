using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Configuration;

namespace DSS19
{

    class Controller 
    {
        private Persistence P = new Persistence();
        string connectionString;
        string dbPath = "";

        public Controller(string dbpath)
        {
            //string dbpath = @"C:\Users\Enrico\Desktop\ordiniMI2018.sqlite";
            dbPath = dbpath;
            string sdb = ConfigurationManager.AppSettings["dbServer"]; 

            switch (sdb)
            {
                case "SQLiteConn": connectionString = ConfigurationManager.ConnectionStrings["SQLiteConn"].ConnectionString;
                                   connectionString = connectionString.Replace("DBFILE", dbpath); 
                                   P.factory = ConfigurationManager.ConnectionStrings["SQLiteConn"].ProviderName;
                                   break;
                case "LocalDbConn":connectionString = ConfigurationManager.ConnectionStrings["LocalSqlServConn"].ConnectionString; 
                                   P.factory = ConfigurationManager.ConnectionStrings["LocalSqlServConn"].ProviderName;
                                   break;
                case "RemoteSqlServConn": connectionString = ConfigurationManager.ConnectionStrings["RemoteSQLConn"].ConnectionString;
                                          P.factory = ConfigurationManager.ConnectionStrings["RemoteSQLConn"].ProviderName;
                                          break;
            }
            P.connectionString = connectionString;
        }

        public void readDb(string custID) 
        {
            Trace.WriteLine("Controller read DB");
            if(custID == "")
            {
                P.SelectTop100();
            }
            else
            {
                P.SelectCustOrders(custID);
            }
        }

        public void insert(string cust)
        {
            P.Insert(cust);
        }

        public void delete(string cust)
        {
            P.Delete(cust);
        }

        public void update(string oldCust, string newCust)
        {
            P.Update(oldCust, newCust);
        }

        public void readCustomerList()
        {
            string res = P.readCustomerListORM(dbPath, 10);
            Trace.WriteLine(res);
        }

        public void readCustomerOrder()
        {
            string res = P.readCustomerOrderORM(dbPath, "cust1");
            Trace.WriteLine(res);
        }
    }
}
