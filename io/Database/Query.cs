using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace io.Database
{
    public class Query
    {
        private string _viewName = "";
        private string _sql = "";
        private string _connectionString = "";
        private io.Data.Return<System.Data.DataTable> _result = null;
        private System.Data.DataTable _dataTable = null;
        private io.AppDomain.App _app;

        private bool _disposedValue = false; // To detect redundant calls

        public Query(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Query(int connectionIndex, io.AppDomain.App app)
        {
            _connectionString = app.Connections[connectionIndex];
            _app = app;
        }

        public Query(string viewName, string connectionString)
        {
            _viewName = viewName;
            _connectionString = connectionString;
        }

        public Query(string viewName, string server, string database, string userId, string password)
        {
            _viewName = viewName;
            _connectionString = "Server=" + server + ";Database=" + database + ";UID=" + userId + ";Password=" + password + ";";
        }

        public Query(string viewName, int connectionIndex, io.AppDomain.App app)
        {
            _viewName = viewName;
            _connectionString = app.Connections[connectionIndex];
            _app = app;
        }

        public Query(string server, string database, string userId, string password)
        {
            _connectionString = "Server=" + server + ";Database=" + database + ";UID=" + userId + ";Password=" + password + ";";
        }

        public Query self()
        {
            return this;    
        }

        public io.Data.Return<System.Data.DataTable> Result()
        {
            return _result;
        }

        public string SQL
        { 
            get { return _sql; }
            set { _sql = value; }
        }
        
        private string sqlString(string where, string orderBy, string join, int top)
        {
            string result = "";

            if (_sql.Length != 0)
            {
                result = SQL;
            }
            else
            { 
                result = "SELECT * FROM " + _viewName;

                if (top > -1)
                    result = result.Replace("SELECT ", "SELECT TOP " + top.ToString() + " ");

                if (join.Length > 0)
                    result = result + " " + join;

                if (where.Length > 0)
                    result = result + " Where " + where;
            
                if (orderBy.Length > 0)
                    result = result + " Order By " + orderBy;
            }

            return result;
        }

        public io.Data.Return<System.Data.DataTable> Run()
        {
            return Run("", "", "", Constants.ALL);
        }

        public io.Data.Return<System.Data.DataTable> Run(int top)
        {
            return Run("", "", "", top);
        }

        public io.Data.Return<System.Data.DataTable> Run(string where, int top)
        {
            return Run(where, "", "", top);
        }

        public io.Data.Return<System.Data.DataTable> Run(string where, string orderBy, int top)
        {
            return Run(where, orderBy, "", top);
        }
        
        public io.Data.Return<System.Data.DataTable> Run(string where, string orderBy, string join, int top)
        {
            if (_dataTable != null)
                _dataTable.Dispose();

            _dataTable = new System.Data.DataTable();

            try
            {
                using (System.Data.SqlClient.SqlConnection cn = new System.Data.SqlClient.SqlConnection(_connectionString))
                {
                    using (System.Data.SqlClient.SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(sqlString(where, orderBy, join, top), cn))
                    {
                        da.SelectCommand.CommandTimeout = 0;
                        da.Fill(_dataTable);
                    }
                    cn.Close();
                }
                _result = new io.Data.Return<System.Data.DataTable>(io.Constants.SUCCESS,"","", _dataTable);
            }
            catch (System.Exception e)
            {
                _result = new io.Data.Return<System.Data.DataTable>(io.Constants.FAILURE, e.Message, "", null);
            }

            return _result;
        }

        protected void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_dataTable != null) _dataTable.Dispose();
                    _dataTable = null;
                    _result = null;
                }

            _viewName = "";
            _sql = "";
            }

            _disposedValue = true;
        }

        void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
