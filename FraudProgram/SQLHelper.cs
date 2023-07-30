using System.Data;
using System.Data.SqlClient;

public class SQLHelper
{
    public static int COMMAND_TIMEOUT = 30; //default

    public static SqlDataReader ExecuteReaderWithSequentialAccess(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
    {
        SqlCommand cmd = new SqlCommand();
        SqlConnection conn = new SqlConnection(connectionString);

        try
        {
            PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
            SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection | CommandBehavior.SequentialAccess);
            cmd.Parameters.Clear();
            return rdr;
        }
        catch (Exception ex)
        {
            //logger.Error("Exception in ExecuteReader :", ex);
            conn.Close();
            throw;
        }
    }

    public static SqlDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
    {
        SqlCommand cmd = new SqlCommand();
        SqlConnection conn = new SqlConnection(connectionString);

        try
        {
            PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
            SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            cmd.Parameters.Clear();
            return rdr;
        }
        catch (Exception ex)
        {
            //logger.Error("Exception in ExecuteReader :", ex);
            conn.Close();
            throw;
        }
    }

    public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
    {

        SqlCommand cmd = new SqlCommand();

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }
    }

    // public static int ExecuteConcurrentNonQuery(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
    // {

    //     int effectedRowsCount = SQLHelper.ExecuteNonQuery(connectionString, cmdType, cmdText, commandParameters);
    //     if (effectedRowsCount == 0)
    //     {
    //         throw new DbKayitDegistirilmisException();
    //     }
    //     return effectedRowsCount;
    // }

    private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] cmdParms)
    {

        if (conn.State != ConnectionState.Open)
            conn.Open();

        cmd.Connection = conn;
        cmd.CommandText = cmdText;
        cmd.CommandTimeout = COMMAND_TIMEOUT;

        if (trans != null)
            cmd.Transaction = trans;

        cmd.CommandType = cmdType;

        if (cmdParms != null)
        {
            foreach (SqlParameter parm in cmdParms)
            {
                parm.Value = GetNullableValue(parm.Value);
                cmd.Parameters.Add(parm);
            }
        }
    }

    private static object GetNullableValue(object obj)
    {
        if (obj == null)
        {
            return System.DBNull.Value;
        }
        else
        {
            return obj;
        }
    }
}