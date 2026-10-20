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
        // Faculdade
        /*string strConn = @"Server=F121-LB5-PROF;
        User Id=aluno;
        Password=dba;
        Database=sistematarefas;
        TrustServerCertificate=true";*/

        // Kauan
        string strConn = @"Server=(localdb)\MSSQLLocalDB;
        Database=CeleiroCriativo;
        Trusted_Connection=True;
        TrustServerCertificate=true";

        /*string strConn = @"Server=DESKTOP-0PQAFOH\SQLEXPRESS;
        Database=CeleiroCriativo;
        Trusted_Connection=True;
        TrustServerCertificate=true";*/
        
        conn = new SqlConnection(strConn);
        conn.Open();
    }

    public void Dispose()
    {
        conn.Close();
    }
}