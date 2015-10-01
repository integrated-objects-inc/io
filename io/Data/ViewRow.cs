using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using io.Data;

namespace io.Data
{
    public interface IViewRow
    {
        int PrimaryKey { get; }
        Return<bool> UpdateRow();
        Return<bool> DeleteRow();
    }

    public class ViewRow : System.Data.DataRow, IViewRow
    {
        private List<string> _fieldsChanged = new List<string>();
        private List<string> _fieldsChangedQuoted = new List<string>();
        private List<string> _fieldsChangedBoolean = new List<string>();

        private IView _view;
        private Systems.IOSystem _ioSystem;

        public ViewRow(System.Data.DataRowBuilder rb, IView view) : base(rb)
        {
            _view = view;
            _ioSystem = ((io.Data.View)_view).IOSystem;
        }

        public int PrimaryKey
        {
            get { return DBInteger(_view.ID); }
        }

        public bool IsDBNull(object value)
        {
            return (value == System.DBNull.Value);
        }

        public Return<bool> UpdateRow()
        {
            if (_view.CanUpdate())
            {
                if (IsDBNull(this[_view.ID]))
                    return RunInsertSQL();
                else
                    return RunUpdateSQL();
            }
            else
                return new Return<bool>(Return<bool>.ResultEnum.Failure, "Update not supported for data source.", "", false);
        }

        public Return<bool> DeleteRow()
        {
            if (_view.CanDelete())
                return _view.Delete(Convert.ToInt32(this[_view.ID]));
            else
                return new Return<bool>(Return<bool>.ResultEnum.Failure, "Delete not supported for data source.", "", false);
        }

        private Return<bool> RunInsertSQL()
        {
            if (_ioSystem != null)
                return RunInsertSQLWithIOSystem();
            else
                return RunInsertSQLWithApp();
        }

        private Return<bool> RunInsertSQLWithIOSystem()
        {
            string sql = InsertSQL();

            if (sql.Length != 0)
            {
                using (var cmd = new io.Database.Command(_view.ConnectionIndex, sql + Constants.SEPARATOR + "SELECT @@IDENTITY", _ioSystem, true))
                {
                    var runSQLResult = cmd.ExecuteScalar();

                    if (runSQLResult.Success)
                    {
                        this[_view.ID] = Convert.ToInt32(runSQLResult.Value);

                        _view.SyncForeignKeys((int)this[_view.ID]);
                        
                        return new Return<bool>(Return<bool>.ResultEnum.Success, runSQLResult.Message, "", true);
                    }
                    else
                    {
                        return TrapErrors(new Return<bool>(Return<bool>.ResultEnum.Failure, runSQLResult.Message, "", false));
                    }
                }
            }
            else
            {
                return new Return<bool>(Return<bool>.ResultEnum.Success, "Nothing to update.", "", true);
            }
        }

        private Return<bool> RunInsertSQLWithApp()
        {
            string sql = InsertSQL();

            if (sql.Length != 0)
            {
                using (var cmd = new io.Database.Command(_view.ConnectionIndex, sql + Constants.SEPARATOR + "SELECT @@IDENTITY", _view.AppSettings, true))
                {
                    var runSQLResult = cmd.ExecuteScalar();

                    if (runSQLResult.Success)
                    {
                        this[_view.ID] = Convert.ToInt32(runSQLResult.Value);
                        return new Return<bool>(Return<bool>.ResultEnum.Success, runSQLResult.Message, "", true);
                    }
                    else
                    {
                        return TrapErrors(new Return<bool>(Return<bool>.ResultEnum.Failure, runSQLResult.Message, "", false));
                    }
                }
            }
            else
            {
                return new Return<bool>(Return<bool>.ResultEnum.Success, "Nothing to update.", "", true);
            }
        }

        private string InsertSQL()
        {
            var sql = new StringBuilder("INSERT INTO [TABLENAME] ([FIELDS]) VALUES ([VALUES])");
            string fields = "";
            string values = "";

            foreach (string f in _fieldsChanged)
            {
                if (fields.Length > 0)
                    fields += ",";

                fields += f;

                if (values.Length > 0)
                    values += ",";

                if (IsDBNull(this[f]))
                    values += "NULL";
                else
                    values += this[f];
            }

            foreach (string f in _fieldsChangedBoolean)
            {
                if (fields.Length > 0)
                    fields += ",";

                fields += f;

                if (values.Length > 0)
                    values += ",";

                if (IsDBNull(this[f]))
                    values += "NULL";
                else
                    values += (Convert.ToBoolean(this[f]) ? "1" : "0");
            }

            foreach (string f in _fieldsChangedQuoted)
            {
                if (fields.Length > 0)
                    fields += ",";

                fields += f;

                if (values.Length > 0)
                    values += ",";

                if (IsDBNull(this[f]))
                    values += "NULL";
                else
                    values += "'" + this[f].ToString().Replace("'", "''") + "'";
            }

            if (values.Length > 0)
            {
                sql.Replace("[TABLENAME]", _view.Source);
                sql.Replace("[FIELDS]", fields);
                sql.Replace("[VALUES]", values);

                return sql.ToString();
            }
            else
            {
                return "";
            }
        }

        private Return<bool> RunUpdateSQL()
        {
            if (_ioSystem != null)
                return RunUpdateSQLWithIOSystem();
            else
                return RunUpdateSQLWithApp();
        }

        private Return<bool> RunUpdateSQLWithApp()
        {
            string sql = UpdateSQL();

            try
            {
                if (sql.Length != 0)
                {
                    using (Database.Command cmd = new Database.Command(_view.AppSettings.Connections[_view.ConnectionIndex].ToString(), sql.ToString(), _view.AppSettings, false))
                    {
                        return TrapErrors(cmd.ExecuteNonQuery());
                    }
                }
                else
                {
                    return new Return<bool>(Return<bool>.ResultEnum.Success, "Nothing to update.", "", true);
                }
            }
            catch (Exception ex)
            {
                return new Return<bool>(Return<bool>.ResultEnum.Fatal, ex.Message, "", false);
            }
        }

        private Return<bool> RunUpdateSQLWithIOSystem()
        {
            string sql = UpdateSQL();

            try
            {
                if (sql.Length != 0)
                {
                    using (Database.Command cmd = new Database.Command(_view.ConnectionIndex, sql.ToString(), _ioSystem, false))
                    {
                        return TrapErrors(cmd.ExecuteNonQuery());
                    }
                }
                else
                {
                    return new Return<bool>(Return<bool>.ResultEnum.Success, "Nothing to update.", "", true);
                }
            }
            catch (Exception ex)
            {
                return new Return<bool>(Return<bool>.ResultEnum.Fatal, ex.Message, "", false);
            }
        }

        private Return<bool> TrapErrors(Return<bool> result)
        {
            try
            {
                if (result.Message.StartsWith("Cannot insert duplicate key"))
                {
                    var parser = new StringBuilder(result.Message);
                    var newMessage = parser.Replace(System.Environment.NewLine, " ").Replace(System.Environment.NewLine, "").Replace(".", "").Replace("'", "").ToString();
                    newMessage = newMessage.Split('_')[newMessage.Split('_').Length - 1] + " already exist.";

                    return new Return<bool>(Return<bool>.ResultEnum.Failure, newMessage, result.Description, false);
                }

                if (result.Message.StartsWith("Cannot insert the value") & !result.Success)
                {
                    string newMessage = result.Message.Split(' ')[7].Replace(",", "") + " cannot be " + result.Message.Split(' ')[4] + ".";
                    return new Return<bool>(Return<bool>.ResultEnum.Failure, newMessage, result.Description, false);
                }

                return result;
            }
            catch
            {
                return result;
            }
        }

        private string UpdateSQL()
        {
            var sql = new StringBuilder("UPDATE [TABLENAME] SET [SET] WHERE [WHERE]");

            string set = "";
            string @where = _view.ID + " = " + this[_view.ID];

            foreach (string f in _fieldsChanged)
            {
                if (set.Length > 0)
                    set += ", ";

                if (IsDBNull(this[f]))
                    set += f + " = NULL";
                else
                    set += f + " = " + this[f] + "";
            }

            foreach (string f in _fieldsChangedBoolean)
            {
                if (set.Length > 0)
                    set += ", ";

                if (IsDBNull(this[f]))
                    set += f + " = NULL";
                else
                    set += f + " = " + (Convert.ToBoolean(this[f]) ? "1" : "0");
            }

            foreach (string f in _fieldsChangedQuoted)
            {
                if (set.Length > 0)
                    set += ", ";

                if (IsDBNull(this[f]))
                    set += f + " = NULL";
                else
                    set += f + " = '" + this[f].ToString().Replace("'", "''") + "'";
            }

            if (set.Length != 0)
            {
                sql.Replace("[TABLENAME]", _view.Source);
                sql.Replace("[SET]", set);
                sql.Replace("[WHERE]", where);

                return sql.ToString();
            }
            else
            {
                return "";
            }
        }

        protected bool? DBBooleanNullable(string columnName)
        {
            try
            {
                if (IsDBNull(this[columnName]))
                    return null;
                else
                    return (bool?)this[columnName];
            }
            catch
            {
                return null;
            }
        }

        protected bool DBBoolean(string columnName)
        {
            try
            {
                if (IsDBNull(this[columnName]))
                    return false;
                else
                    return Convert.ToBoolean(this[columnName]);
            }
            catch
            {
                return false;
            }
        }

        protected string DBDate(string columnName, bool withTime = false)
        {
            try
            {
                if (IsDBNull(this[columnName]))
                    return "";
                else
                {
                    if (Validation.IsDate(this[columnName].ToString()))
                    {
                        if (withTime)
                            return Convert.ToDateTime(this[columnName].ToString()).ToString();
                        else
                            return Convert.ToDateTime(this[columnName].ToString()).ToString("MM/dd/yyyy");
                    }
                    else
                        return "";
                }
            }
            catch
            {
                return "";
            }
        }

        protected System.DateTime? DBDateNullable(string columnName)
        {
            try
            {
                if (IsDBNull(this[columnName]))
                    return null;
                else
                    return Convert.ToDateTime(this[columnName]);
            }
            catch
            {
                return null;
            }
        }

        protected string DBString(string columnName)
        {
            try
            {
                if (IsDBNull(this[columnName]))
                    return "";
                else
                    return this[columnName].ToString();
            }
            catch
            {
                return "";
            }
        }

        protected char DBChar(string columnName)
        {
            try
            {
                if (IsDBNull(this[columnName]) && this[columnName].ToString().Trim().Length == 0)
                    return ' ';
                else
                    return Convert.ToChar(this[columnName].ToString());
            }
            catch
            {
                return ' ';
            }
        }

        protected int DBInteger(string columnName)
        {
            try
            {
                if (IsDBNull(this[columnName]))
                    return 0;
                else
                    return Convert.ToInt32(this[columnName]);
            }
            catch
            {
                return 0;
            }
        }

        protected decimal DBDecimal(string columnName)
        {
            try
            {
                if (IsDBNull(this[columnName]))
                    return 0;
                else
                    return Convert.ToDecimal(this[columnName]);
            }
            catch
            {
                return 0;
            }
        }

        protected double DBDouble(string columnName)
        {
            try
            {
                if (IsDBNull(this[columnName]))
                    return 0;
                else
                    return Convert.ToDouble(this[columnName]);
            }
            catch
            {
                return 0;
            }
        }

        protected int? DBIntegerNullable(string columnName)
        {
            try
            {
                if (IsDBNull(this[columnName]))
                    return null;
                else
                    return Convert.ToInt32(this[columnName]);
            }
            catch
            {
                return null;
            }
        }

        protected void SetDBChar(string field, char value, bool emptyToNull = true)
        {
            string valueAsString = value.ToString().Trim();

            if (emptyToNull && valueAsString.Length == 0)
                this[field.ToString()] = System.Convert.DBNull;
            else
                this[field.ToString()] = valueAsString;

            if (!_fieldsChangedQuoted.Contains(field.ToString())) { _fieldsChangedQuoted.Add(field.ToString()); }
        }

        protected void SetDBString(string field, string value, Int16 maxLength, bool emptyToNull = true)
        {
            if (value == null)
                value = "";

            if (emptyToNull && value.Trim().Length == 0)
            {
                this[field.ToString()] = System.Convert.DBNull;
            }
            else
            {
                if (value.Trim().Length > maxLength)
                    this[field.ToString()] = value.Trim().Substring(0, maxLength);
                else
                    this[field.ToString()] = value.Trim();
            }
            if (!_fieldsChangedQuoted.Contains(field.ToString())) { _fieldsChangedQuoted.Add(field.ToString()); }
        }

        protected void SetDBDate(string field, System.DateTime value)
        {
            if (value == null)
                this[field.ToString()] = System.Convert.DBNull;
            else
                this[field.ToString()] = value;

            if (!_fieldsChangedQuoted.Contains(field.ToString()))
                _fieldsChangedQuoted.Add(field.ToString());
        }

        protected void SetDBDate(string field, string value)
        {
            if (value == null)
                this[field.ToString()] = System.Convert.DBNull;
            else
            {
                if (io.Data.Validation.IsDate(value))
                    this[field.ToString()] = value;
                else
                    this[field.ToString()] = System.Convert.DBNull;
            }
            
            if (!_fieldsChangedQuoted.Contains(field.ToString()))
                _fieldsChangedQuoted.Add(field.ToString());
        }

        protected void SetDBDateNullable(string field, System.DateTime? value)
        {
            if (value.HasValue)
                this[field.ToString()] = value;
            else
                this[field.ToString()] = System.Convert.DBNull;

            if (!_fieldsChangedQuoted.Contains(field.ToString()))
                _fieldsChangedQuoted.Add(field.ToString());
        }

        protected void SetDBInteger(string field, int value, bool zeroToNull = true)
        {
            if (value == 0 & zeroToNull)
                this[field.ToString()] = System.Convert.DBNull;
            else
                this[field.ToString()] = value;

            if (!_fieldsChanged.Contains(field.ToString()))
                _fieldsChanged.Add(field.ToString());
        }

        protected void SetDBDecimal(string field, decimal value, bool zeroToNull = true)
        {
            if (value == 0 & zeroToNull)
                this[field.ToString()] = System.Convert.DBNull;
            else
                this[field.ToString()] = value;

            if (!_fieldsChanged.Contains(field.ToString()))
                _fieldsChanged.Add(field.ToString());
        }

        protected void SetDBDouble(string field, double value, bool zeroToNull = true)
        {
            if (value == 0 & zeroToNull)
                this[field.ToString()] = System.Convert.DBNull;
            else
                this[field.ToString()] = value;

            if (!_fieldsChanged.Contains(field.ToString()))
                _fieldsChanged.Add(field.ToString());
        }

        protected void SetDBBoolean(string field, bool value)
        {
            this[field.ToString()] = value;

            if (!_fieldsChangedBoolean.Contains(field.ToString()))
                _fieldsChangedBoolean.Add(field.ToString());
        }

        protected void SetDBBooleanNullable(string field, bool? value)
        {
            if (value.HasValue)
                this[field.ToString()] = value.Value;
            else
                this[field.ToString()] = System.Convert.DBNull;

            if (!_fieldsChangedBoolean.Contains(field.ToString()))
                _fieldsChangedBoolean.Add(field.ToString());
        }
    }
}