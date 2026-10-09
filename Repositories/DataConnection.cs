using Microsoft.Data.SqlClient;

public abstract class DataConnection
{
    /*
    Para funcionar essa classe, precisa instalar o pacote Microsoft.Data.SqlClient, que é o driver do SQL Server para .NET Core.
    Rodar no terminal: dotnet add package Microsoft.Data.SqlClient
    */
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