using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace io.Data
{
    public interface IView
    {
        string Source { get; }
        int Count { get; }
        string ID { get; }
        io.AppDomain.App AppSettings { get; }
        int ConnectionIndex { get; }

        IViewRow this[int index] { get; }
        bool CanUpdate();
        bool CanDelete();

        void SyncForeignKeys(int value);

        io.Data.Return<bool> Delete(params int[] ids);
    }

    [System.ComponentModel.DesignerCategory("")]
    public class View : System.Data.DataTable, System.Collections.IEnumerable, IView
    {
        protected Array _selectFields;
        protected string _view = string.Empty;
        protected string _source = string.Empty;
        protected string _id = string.Empty;

        protected bool _parameterized = false;
        protected string _parameters = string.Empty;

        protected int _top = -1;
        protected string _where = string.Empty;
        protected string _orderBy = string.Empty;

        private Return<System.Data.DataTable> _result;
        protected int _connectionIndex;

        protected AppDomain.App _app;
        protected Systems.IOSystem _ioSystem;

        protected event Event_AfterQueryEventHandler Event_AfterQuery;
        protected delegate void Event_AfterQueryEventHandler();

        public View() : base()
        {
            
        }

        protected List<ForeignKeySyncConnection> _foreignKeySyncConnections = new List<ForeignKeySyncConnection>(); 

        protected class ForeignKeySyncConnection
        {
            public int ConnectionIndex { get; set; }
            public string TableName  { get; set;}
            public string PrimaryKeyName { get; set; }
            public bool Deleted { get; set; }

            public ForeignKeySyncConnection(int connectionIndex, string tableName, string primaryKeyName)
            {
                ConnectionIndex = connectionIndex;
                TableName = tableName;
                PrimaryKeyName = primaryKeyName;
                Deleted = false;
            }
        }

        public void SyncForeignKeys(int value)
        {
            foreach (ForeignKeySyncConnection spec in _foreignKeySyncConnections)
            {
                SyncForeignKeys(spec, value);
            }
        }

        private void SyncForeignKeys(ForeignKeySyncConnection spec, int value)
        {
            var sql = new StringBuilder("INSERT INTO [TABLENAME] ([PRIMARYKEYNAME]) VALUES ([VALUE])");

            sql.Replace("[TABLENAME]", spec.TableName);
            sql.Replace("[PRIMARYKEYNAME]", spec.PrimaryKeyName);
            sql.Replace("[VALUE]", value.ToString());

            var result = new Database.Command(spec.ConnectionIndex, sql.ToString(), _ioSystem, true).ExecuteScalar();
        }

        private bool DeleteForeignKeys(params int[] ids)
        { 
            foreach (int id in ids)
            {
                if (!DeleteForeignKeys(id))
                    return false;
            }

            return true;
        }

        private bool DeleteForeignKeys(int value)
        {
            bool rollBack = false;

            foreach (ForeignKeySyncConnection spec in _foreignKeySyncConnections)
            {
                var sql = new StringBuilder("DELETE FROM [TABLENAME] WHERE ([PRIMARYKEYNAME] = [VALUE])");

                sql.Replace("[TABLENAME]", spec.TableName);
                sql.Replace("[PRIMARYKEYNAME]", spec.PrimaryKeyName);
                sql.Replace("[VALUE]", value.ToString());
                spec.Deleted = false; // Reset to default value

                var result = new Database.Command(spec.ConnectionIndex, sql.ToString(), _ioSystem, true).ExecuteScalar();

                if (result.Success)
                {
                    spec.Deleted = true;
                }
                else
                {
                    rollBack = true;
                    break;
                }
            }

            if (rollBack)
            {
                foreach (ForeignKeySyncConnection spec in _foreignKeySyncConnections)
                {
                    if (spec.Deleted)
                        SyncForeignKeys(spec, value);
                }
                return false; 
            }

            return true;
        }

        public IViewRow this[int index]
        {
            get { return (IViewRow)this.Rows[index]; }
        }

        public AppDomain.App AppSettings
        {
            get { return _app; }
        }

        internal Systems.IOSystem IOSystem
        {
            get { return _ioSystem; }
        }
         
        public List<KeyValuePair<int, string>> GetList(KeyValuePair<int, string> startingWith)
        {
            var result = GetList();
            result.Insert(0, startingWith);
            return result;
        }

        public List<KeyValuePair<int, string>> GetList()
        {
            var result = new List<KeyValuePair<int, string>>();

            foreach (io.Data.ViewRow row in this)
            {
                result.Add(new KeyValuePair<int, string>(row.PrimaryKey, row[1].ToString()));
            }

            return result;
        }

        public int ConnectionIndex
        {
            get { return _connectionIndex; }
        }

        public bool CanUpdate()
        {
            return (_view.Length != 0 & _id.Length != 0);
        }

        public bool CanDelete()
        {
            return CanUpdate();
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return this.Rows.GetEnumerator();
        }

        public string Source
        {
            get
            {
                if (_source.Length == 0)
                    return _view;
                else
                    return _source;
            }
        }

        public string ID
        {
            get { return _id; }
        }

        public int Count
        {
            get { return this.Rows.Count; }
        }

        public Return<System.Data.DataTable> Requery()
        {
            Query();
            return QueryResult;
        }

        public Return<System.Data.DataTable> QueryResult
        {
            get { return _result; }
        }

        public Array SelectFields
        {
            get { return _selectFields; }
        }

        private List<System.Data.SqlClient.SqlParameter> _sqlParameterCollection = new List<System.Data.SqlClient.SqlParameter>();

        public void AddParameterValue(string parameterName, object value)
        {
            _sqlParameterCollection.Add(new System.Data.SqlClient.SqlParameter(parameterName, value));
        }
        
        #region "Query Functions"
        protected void Query()
        {
            if (_ioSystem != null)
                QueryWithIOSystem();
            else
                QueryWithApp();
        }

        private void QueryWithIOSystem()
        {
            if (_view.Length == 0 && _source.Length != 0)
                _view = _source;

            string selectSQL = SQL();

            try
            {
                using (System.Data.SqlClient.SqlConnection cn = new System.Data.SqlClient.SqlConnection(_ioSystem.Connection(_connectionIndex).ConnectionString))
                {
                    var cmd = new System.Data.SqlClient.SqlCommand(selectSQL, cn);

                    foreach (System.Data.SqlClient.SqlParameter item in _sqlParameterCollection)
                        cmd.Parameters.Add(item);

                    using (System.Data.SqlClient.SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(cmd))
                    {
                        da.SelectCommand.CommandTimeout = 0;
                        da.Fill(this);
                    }

                    cn.Close();
                }
                if (Event_AfterQuery != null)
                    Event_AfterQuery();

                _result = new Return<System.Data.DataTable>(Return<DataTable>.ResultEnum.Success, "", _ioSystem, "", this);
            }
            catch (Exception ex)
            {
                _result = new Return<System.Data.DataTable>(Return<DataTable>.ResultEnum.Failure, ex.Message, _ioSystem, selectSQL, null);
            }
        }

        private void QueryWithApp()
        {
            if (_view.Length == 0 && _source.Length != 0)
                _view = _source;

            string selectSQL = SQL();

            try
            {
                using (System.Data.SqlClient.SqlConnection cn = new System.Data.SqlClient.SqlConnection(_app.Connections[_connectionIndex].ToString()))
                {
                    var cmd = new System.Data.SqlClient.SqlCommand(selectSQL, cn);

                    foreach (System.Data.SqlClient.SqlParameter item in _sqlParameterCollection)
                        cmd.Parameters.Add(item);

                    using (System.Data.SqlClient.SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(cmd))
                    {
                        da.SelectCommand.CommandTimeout = 0;
                        da.Fill(this);
                    }

                    cn.Close();
                }
                if (Event_AfterQuery != null)
                    Event_AfterQuery();

                _result = new Return<System.Data.DataTable>(Return<DataTable>.ResultEnum.Success, "", _app, "", this);
            }
            catch (Exception ex)
            {
                _result = new Return<System.Data.DataTable>(Return<DataTable>.ResultEnum.Failure, ex.Message, _app, selectSQL, null);
            }
        }
        #endregion

        #region "Update Functions"
        public Return<bool> Update(bool rollBackTransaction = false)
        {
            Return<bool> lastResult = new Return<bool>(Return<bool>.ResultEnum.Success, "", "", true);
            foreach (ViewRow item in this)
            {
                lastResult = item.UpdateRow();
                if (!lastResult.Success) { break; }
            }
            return lastResult;
        }
        #endregion

        #region "Delete Functions"
        public Return<bool> Delete(params int[] ids)
        {
            if (_ioSystem == null)
                return DeleteUsingApp(ids);
            else
                return DeleteUsingIOSystem(ids);
        }

        private Return<bool> DeleteUsingIOSystem(params int[] ids)
        {
            if (ids.Length != 0)
            {
                if (DeleteForeignKeys(ids))
                {
                    System.Text.StringBuilder sql = new System.Text.StringBuilder("DELETE FROM [TABLE] WHERE ([ID] IN ([IDS]))");
                    string keys = string.Join(",", ids);

                    sql.Replace("[TABLE]", _source);
                    sql.Replace("[ID]", _id);
                    sql.Replace("[IDS]", keys);

                    using (Database.Command cmd = new Database.Command(_connectionIndex, sql.ToString(), _ioSystem, false))
                    {
                        return cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    return new Return<bool>(Return<bool>.ResultEnum.Success, "Unable to delete, item in use.", "", false);
                }
            }
            else
            {
                return new Return<bool>(Return<bool>.ResultEnum.Success, "No Ids specified", "", false);
            }        
        }

        private Return<bool> DeleteUsingApp(params int[] ids)
        {
            if (ids.Length != 0)
            {
                System.Text.StringBuilder sql = new System.Text.StringBuilder("DELETE FROM [TABLE] WHERE ([ID] IN ([IDS]))");
                string keys = string.Join(",", ids);

                sql.Replace("[TABLE]", _source);
                sql.Replace("[ID]", _id);
                sql.Replace("[IDS]", keys);

                using (Database.Command cmd = new Database.Command(_app.Connections[_connectionIndex].ToString(), sql.ToString(), _app, false))
                {
                    return cmd.ExecuteNonQuery();
                }
            }
            else
            {
                return new Return<bool>(Return<bool>.ResultEnum.Success, "No Ids specified", "", false);
            }
        }
        #endregion

        public string FixedSQL
        {
            get { return _view; }
            set { _view = value; }
        }

        public string SQL()
        {
            System.Text.StringBuilder selectSQL;
            _view = _view.Replace(System.Environment.NewLine, "").Trim();

            bool viewIsSQL = false;

            if (_view.StartsWith("SELECT") || _view.StartsWith("WITH"))
                viewIsSQL = true;

            if (viewIsSQL)
            {
                if (_view.ToUpper().Contains("WHERE "))
                    selectSQL = new System.Text.StringBuilder(_view + " [ORDERBY]");
                else
                    selectSQL = new System.Text.StringBuilder(_view + " [WHERE] [ORDERBY]");

                while (selectSQL.ToString().Contains("  "))
                {
                    selectSQL.Replace("  ", " ");
                }
            }
            else
            {
                selectSQL = new System.Text.StringBuilder("SELECT [TOP] [FIELDS] FROM [TABLE] [WHERE] [ORDERBY]");
                var fields = new System.Text.StringBuilder();

                if (_selectFields == null || _selectFields.Length == 0)
                {
                    fields.Append("*");
                }
                else
                {
                    foreach (object field in _selectFields)
                    {
                        if (fields.Length > 0) { fields.Append(','); }
                        fields.Append(field);
                    }
                }
                selectSQL.Replace("[FIELDS]", fields.ToString()).Replace("[TABLE]", (_parameterized ? _view + "(" + _parameters + ")" : _view));
            }

            selectSQL.Replace("[TOP]", _top < 0 ? "" : "TOP " + _top.ToString());
            selectSQL.Replace(" [WHERE]", _where.Length == 0 ? "" : " WHERE " + _where);
            selectSQL.Replace(" [ORDERBY]", _orderBy.Length == 0 ? "" : " ORDER BY " + _orderBy);
            
            return selectSQL.ToString();
        }
    }
}
