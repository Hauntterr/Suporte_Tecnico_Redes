using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Collections.Generic;
using System;
using System.Text;

namespace APICelia.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ArquivoController : ControllerBase
    {
        private readonly ILogger<ArquivoController> _logger;
        private readonly string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Celia;Integrated Security=True;Connect Timeout=30;Encrypt=False";

        public ArquivoController(ILogger<ArquivoController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetArquivo")]
        public IEnumerable<Arquivo> Get()
        {
            List<Arquivo> arquivos = new List<Arquivo>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Arquivo";
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Arquivo arquivo = new Arquivo
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Nome = reader["Nome"].ToString(),
                        Arquivos = reader["Arquivos"].ToString() // Base64 string
                    };

                    arquivos.Add(arquivo);
                }

                reader.Close();
            }

            return arquivos;
        }

        [HttpPost(Name = "AddArquivo")]
        public IActionResult Post([FromBody] Arquivo arquivo)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                string query = "INSERT INTO Arquivo (Nome, Arquivos) VALUES (@Nome, @Arquivos)";
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@Nome", arquivo.Nome);
                command.Parameters.AddWithValue("@Arquivos", arquivo.Arquivos); // Base64 string

                connection.Open();
                command.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpDelete("{id}", Name = "DeleteArquivo")]
        public IActionResult Delete(int id)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                string query = "DELETE FROM Arquivo WHERE Id = @Id";
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@Id", id);

                connection.Open();
                command.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpGet("{id}/view", Name = "ViewArquivo")]
        public IActionResult View(int id)
        {
            Arquivo arquivo = null;

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM Arquivo WHERE Id = @Id";
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@Id", id);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    arquivo = new Arquivo
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Nome = reader["Nome"].ToString(),
                        Arquivos = reader["Arquivos"].ToString() // Base64 string
                    };
                }

                reader.Close();
            }

            if (arquivo == null)
            {
                return NotFound();
            }

            byte[] fileBytes = Convert.FromBase64String(arquivo.Arquivos);
            string mimeType = GetMimeType(arquivo.Nome);
            return File(fileBytes, mimeType);
        }

        private string GetMimeType(string fileName)
        {
            string mimeType = "application/octet-stream";
            string extension = Path.GetExtension(fileName).ToLower();

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    mimeType = "image/jpeg";
                    break;
                case ".png":
                    mimeType = "image/png";
                    break;
                case ".gif":
                    mimeType = "image/gif";
                    break;
                case ".pdf":
                    mimeType = "application/pdf";
                    break;
                case ".txt":
                    mimeType = "text/plain";
                    break;
                case ".html":
                case ".htm":
                    mimeType = "text/html";
                    break;
                // Adicione mais tipos MIME conforme necessário
                default:
                    mimeType = "application/octet-stream";
                    break;
            }

            return mimeType;
        }
    }

    public class Arquivo
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Arquivos { get; set; } // Base64 string
    }
}
