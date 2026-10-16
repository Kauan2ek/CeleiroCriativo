using System.Data;
using Microsoft.Data.SqlClient;

// Repository auxiliar de Cargo.
// Os cargos continuam sendo alimentados por INSERT; aqui apenas fazemos leitura
// para preencher o cadastro/edição de Funcionário.
public class CargoSqlRepository : DataConnection
{
    public List<Cargo> ReadAll()
    {
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = "SELECT codigo, descricao FROM Cargos ORDER BY descricao";

        List<Cargo> cargos = new List<Cargo>();
        SqlDataReader reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            cargos.Add(new Cargo
            {
                Codigo = reader.GetInt32("codigo"),
                Descricao = reader.GetString("descricao")
            });
        }

        reader.Close();
        return cargos;
    }
}
