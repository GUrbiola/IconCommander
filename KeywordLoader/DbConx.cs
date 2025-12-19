using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZidUtilities.CommonCode.DataAccess;

namespace KeywordLoader
{
    public partial class DbConx :SqlConnector
    {
        public DbConx() { }
        public DbConx(string connectionString) : base(connectionString) { }

        #region Method to execute the SP: dbo.Keywords_Insert
        public SqlResponse<int> Keywords_Insert(string Text, int? Reference)
        {
            SqlResponse<int> back = new SqlResponse<int>();


            SqlCommand sqlCommand = new SqlCommand("dbo.Keywords_Insert", this.Connection);
            sqlCommand.CommandType = CommandType.StoredProcedure;

            #region Parameter assignation
            sqlCommand.Parameters.Add(new SqlParameter("@Text", SqlDbType.NVarChar, 100));
            sqlCommand.Parameters["@Text"].Value = Text ?? String.Empty;

            sqlCommand.Parameters.Add(new SqlParameter("@Reference", SqlDbType.Int, 10));
            if (Reference.HasValue)
                sqlCommand.Parameters["@Reference"].Value = Reference.Value;
            else
                sqlCommand.Parameters["@Reference"].Value = DBNull.Value;
            #endregion

            try
            {
                sqlCommand.Connection.Open();
                using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader(CommandBehavior.SingleResult))
                {
                    if (sqlDataReader.Read())
                    {
                        int obj;
                        if (!String.IsNullOrEmpty(sqlDataReader["Result"].ToString()))
                            obj = Convert.ToInt32(sqlDataReader["Result"].ToString());
                        else
                            obj = -1;
                        back = SqlResponse<int>.Successful(obj, sqlDataReader["Message"].ToString());
                    }
                }
                LastMessage = "OK";
            }
            catch (SqlException sqlex)
            {
                LastMessage = sqlex.Errors[0].Message;
                LastSqlException = sqlex;
                back.Fail("Sql Exception thrown!", sqlex);
            }
            catch (Exception ex)
            {
                LastMessage = ex.Message;
                LastException = ex;
                back.Fail("Generic Exception thrown!", ex);
            }
            finally
            {
                if (sqlCommand.Connection.State != ConnectionState.Closed)
                    sqlCommand.Connection.Close();
                sqlCommand.Dispose();
            }
            return back;
        }
        #endregion

        #region Method to execute the SP: dbo.Icons_Insert
        public SqlResponse<int> Icons_Insert(string Text, int Reference)
        {
            SqlResponse<int> back = new SqlResponse<int>();


            SqlCommand sqlCommand = new SqlCommand("dbo.Icons_Insert", this.Connection);
            sqlCommand.CommandType = CommandType.StoredProcedure;

            #region Parameter assignation
            sqlCommand.Parameters.Add(new SqlParameter("@Text", SqlDbType.NVarChar, 100));
            sqlCommand.Parameters["@Text"].Value = Text ?? String.Empty;

            sqlCommand.Parameters.Add(new SqlParameter("@Reference", SqlDbType.Int, 10));
            sqlCommand.Parameters["@Reference"].Value = Reference;
            #endregion

            try
            {
                sqlCommand.Connection.Open();
                using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader(CommandBehavior.SingleResult))
                {
                    if (sqlDataReader.Read())
                    {
                        int obj = -1;
                        if (!String.IsNullOrEmpty(sqlDataReader[0].ToString()))
                            obj = Convert.ToInt32(sqlDataReader[0].ToString());
                        else
                            obj = -1;
                        back = SqlResponse<int>.Successful(obj, sqlDataReader["Message"].ToString());
                    }
                }
                LastMessage = "OK";
            }
            catch (SqlException sqlex)
            {
                LastMessage = sqlex.Errors[0].Message;
                LastSqlException = sqlex;
                back.Fail("Sql Exception thrown!", sqlex);
            }
            catch (Exception ex)
            {
                LastMessage = ex.Message;
                LastException = ex;
                back.Fail("Generic Exception thrown!", ex);
            }
            finally
            {
                if (sqlCommand.Connection.State != ConnectionState.Closed)
                    sqlCommand.Connection.Close();
                sqlCommand.Dispose();
            }
            return back;
        }
        #endregion

        #region Method to execute the SP: dbo.RegisterIconRelationShip
        public SqlResponse<bool> RegisterIconRelationShip(int IconId, int KeywordId)
        {

            SqlResponse<bool> back = new SqlResponse<bool>();


            SqlCommand sqlCommand = new SqlCommand("dbo.RegisterIconRelationShip", this.Connection);
            sqlCommand.CommandType = CommandType.StoredProcedure;

            #region Parameter assignation
            sqlCommand.Parameters.Add(new SqlParameter("@IconId", SqlDbType.Int, 10));
            sqlCommand.Parameters["@IconId"].Value = IconId;

            sqlCommand.Parameters.Add(new SqlParameter("@KeywordId", SqlDbType.Int, 10));
            sqlCommand.Parameters["@KeywordId"].Value = KeywordId;
            #endregion

            try
            {
                sqlCommand.Connection.Open();
                sqlCommand.ExecuteNonQuery();
                back = SqlResponse<bool>.Successful(true);

                LastMessage = "OK";
            }
            catch (SqlException sqlex)
            {
                LastMessage = sqlex.Errors[0].Message;
                LastSqlException = sqlex;
                back.Fail("Sql Exception thrown!", sqlex);
            }
            catch (Exception ex)
            {
                LastMessage = ex.Message;
                LastException = ex;
                back.Fail("Generic Exception thrown!", ex);
            }
            finally
            {
                if (sqlCommand.Connection.State != ConnectionState.Closed)
                    sqlCommand.Connection.Close();
                sqlCommand.Dispose();
            }
            return back;
        }
        #endregion

        #region Method to execute the SP: dbo.GetIconId
        public SqlResponse<int> GetIconId(string IconName)
        {
            SqlResponse<int> back = new SqlResponse<int>();


            SqlCommand sqlCommand = new SqlCommand("dbo.GetIconId", this.Connection);
            sqlCommand.CommandType = CommandType.StoredProcedure;

            #region Parameter assignation
            sqlCommand.Parameters.Add(new SqlParameter("@IconName", SqlDbType.NVarChar, 100));
            sqlCommand.Parameters["@IconName"].Value = IconName ?? String.Empty;
            #endregion

            try
            {
                sqlCommand.Connection.Open();
                using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader(CommandBehavior.SingleResult))
                {
                    if (sqlDataReader.Read())
                    {
                        int obj = -1;
                        if (!String.IsNullOrEmpty(sqlDataReader[0].ToString()))
                            obj = Convert.ToInt32(sqlDataReader[0].ToString());
                        else
                            obj = -1;
                        back = SqlResponse<int>.Successful(obj);
                    }
                }
                LastMessage = "OK";
            }
            catch (SqlException sqlex)
            {
                LastMessage = sqlex.Errors[0].Message;
                LastSqlException = sqlex;
                back.Fail("Sql Exception thrown!", sqlex);
            }
            catch (Exception ex)
            {
                LastMessage = ex.Message;
                LastException = ex;
                back.Fail("Generic Exception thrown!", ex);
            }
            finally
            {
                if (sqlCommand.Connection.State != ConnectionState.Closed)
                    sqlCommand.Connection.Close();
                sqlCommand.Dispose();
            }
            return back;
        }
        #endregion

        #region Method to execute the SP: dbo.GetIconCount
        public SqlResponse<int> GetIconCount()
        {
            SqlResponse<int> back = new SqlResponse<int>();


            SqlCommand sqlCommand = new SqlCommand("dbo.GetIconCount", this.Connection);
            sqlCommand.CommandType = CommandType.StoredProcedure;

            #region Parameter assignation
            #endregion

            try
            {
                sqlCommand.Connection.Open();
                using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader(CommandBehavior.SingleResult))
                {
                    if (sqlDataReader.Read())
                    {
                        int obj = -1;
                        if (!String.IsNullOrEmpty(sqlDataReader[0].ToString()))
                            obj = Convert.ToInt32(sqlDataReader[0].ToString());
                        else
                            obj = -1;

                        back = SqlResponse<int>.Successful(obj);
                    }
                }
                LastMessage = "OK";
            }
            catch (SqlException sqlex)
            {
                LastMessage = sqlex.Errors[0].Message;
                LastSqlException = sqlex;
                back.Fail("Sql Exception thrown!", sqlex);
            }
            catch (Exception ex)
            {
                LastMessage = ex.Message;
                LastException = ex;
                back.Fail("Generic Exception thrown!", ex);
            }
            finally
            {
                if (sqlCommand.Connection.State != ConnectionState.Closed)
                    sqlCommand.Connection.Close();
                sqlCommand.Dispose();
            }
            return back;
        }
        #endregion


    }
}
