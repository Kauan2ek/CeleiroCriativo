using Microsoft.Data.SqlClient;

public abstract class DataConnection
{
    protected SqlConnection conn;

    public DataConnection()
    {
        string strConn = @"Server=F121-LB5-PROF;
        User Id=aluno;
        Password=dba;
        Database=sistematarefas;
        TrustServerCertificate=true";
        conn = new SqlConnection(strConn);
        conn.Open();
    }

    public void Dispose()
    {
        conn.Close();
    }
}