using PruebaDigitalPerson;
using PruebaDigitalPerson.Conexion;
using System;
using System.IO;
using DPFP;
using DPFP.Verification;
using Npgsql;
using System.Windows.Forms;
using static System.Data.Entity.Infrastructure.Design.Executor;
using Antlr4.StringTemplate;
using Org.BouncyCastle.Bcpg.Sig;
using static DPFP.Verification.Verification;
using static NPOI.HSSF.Util.HSSFColor;

namespace PruebaDigitalPerson
{
    public partial class frmVerificar : CaptureForm
    {
        private DPFP.Template Template;
        private DPFP.Verification.Verification Verificator;
        private ConexionBD contexto;

        public void Verify(DPFP.Template template)
        {
            Template = template;
            ShowDialog();
        }

        protected override void Init()
        {
            base.Init();
            base.Text = "Verificación de Huella Digital";
            Verificator = new DPFP.Verification.Verification();
            UpdateStatus(0);
        }

        private void UpdateStatus(int FAR)
        {
            SetStatus(String.Format("Tasa de Aceptación Falsa (FAR) = {0}", FAR));
        }

        protected override void Process(DPFP.Sample Sample)
        {
            base.Process(Sample);

            DPFP.FeatureSet features = ExtractFeatures(Sample, DPFP.Processing.DataPurpose.Verification);

            if (features != null)
            {
                DPFP.Verification.Verification.Result result = new DPFP.Verification.Verification.Result();
                DPFP.Template template;
                Stream stream;
                bool huellaVerificada = false;

                try
                {

                    //string query = "SELECT nombre, huella, FROM clientes";
                    string query = "SELECT id_cliente, nombres, apellidos, codigo_ver, huella , numero_identificacion FROM public.clientes order by id_cliente desc;";

                    using (NpgsqlConnection conn = new NpgsqlConnection(contexto.connectionString)) 
                    {
                        conn.Open();
                        using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                        {
                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {

                                    Boolean validador = reader.IsDBNull(4) ? false : true;

                                    if (validador == false)
                                    {
                                        //MessageBox.Show("La huella no esta asignada a ningun cliente");

                                        txtEncontradoNombre.Text = " ";
                                        txtEncontradoApellido.Text = " ";
                                        txtEncontradoCedula.Text = " ";
                                        labelNumeroPago.Text = "XXX";
                                        //break;
                                    }
                                    else
                                    {
                                        byte[] huellaBytes = (byte[])reader["huella"];
                                        stream = new MemoryStream(huellaBytes);
                                        template = new DPFP.Template(stream);

                                        Verificator.Verify(features, template, ref result);
                                        UpdateStatus(result.FARAchieved);

                                        if (result.Verified)
                                        {

                                            if (validador == false)
                                            {
                                                MessageBox.Show("La huella no esta asignada a ningun cliente");
                                                break;
                                            }
                                            else
                                            {


                                                //se toma el numero de indentificacion de la consulta
                                                string numeroIdentificacion = reader.GetValue(5).ToString();

                                                //se envia el numero de indentificacion para actualizar las reyes y obtener el codigo
                                                string code = this.actualizarReyesPagar(numeroIdentificacion);

                                                //id del cliente para consultas
                                                int id_cliente = (int)reader.GetValue(0);

                                                
                                                //int codigoVerifica = Convert.ToInt32(this.codigoVerifica(id_cliente));

                                                int update = this.actualizarCodigo(Convert.ToInt32(code), id_cliente);

                                                txtEncontradoNombre.Text = reader.GetValue(1).ToString();

                                                txtEncontradoApellido.Text = reader.GetValue(2).ToString();

                                                txtEncontradoCedula.Text = reader.GetValue(5).ToString();

                                                //mostramos el codigo para pagar las reyes
                                                labelNumeroPago.Text = code;

                                                MakeReport("La huella dactilar pertenece al cliente. " + reader.GetValue(1).ToString() + " " + reader.GetValue(2).ToString());
                                                huellaVerificada = true;
                                                break;

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (!huellaVerificada)
                    {
                        MakeReport("La huella dactilar NO fue encontrada en la base de datos.");
                    }
                }
                catch (Exception ex)
                {
                    MakeReport("Error durante la verificación: " + ex.Message);
                }
            }
        }

        public frmVerificar()
        {
            contexto = new ConexionBD();
            InitializeComponent();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }


        public int actualizarCodigo(int codigo , int id_cliente) 
        {

            
            using (NpgsqlConnection connection = new NpgsqlConnection(contexto.connectionString))
            {
                try
                {
                    connection.Open();

                    //string sql = "INSERT INTO empleados (nombre, huella) VALUES (@nombre, @huella)";

                    string sql = "UPDATE clientes SET codigo_ver = "+ codigo + " WHERE id_cliente= "+ id_cliente + "";


                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@codigo_ver", codigo);
                        command.ExecuteNonQuery();
                    }
                }
                catch (NpgsqlException ex)
                {

                    Console.WriteLine("Error de PostgreSQL: " + ex.Message);

                    throw;
                }
                catch (Exception ex)
                {

                    Console.WriteLine("Error al guardar cliente: " + ex.Message);

                    throw;
                }
            }

            return 0;

        }

        public string codigoVerifica(int id_cliente)
        {

           
            using (NpgsqlConnection connection = new NpgsqlConnection(contexto.connectionString))
            {

                using (var cmd = connection.CreateCommand())
                {
                    connection.Open();

                    cmd.CommandText = "select sr.codigo_very from sorteos_reyes as sr join reyes as r on r.id_rey = sr.id_rey where r.id_cliente = "+id_cliente+" and sr.estado_pago_rey = 'N' limit 1";
                    //cmd.Parameters.AddWithValue("@id_cliente", id_cliente);
           
                    //cmd.Parameters.AddWithValue("@apellidos", cliente.apellidos);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {

                            return reader.GetString(0);
                        }
                        else
                        {
                            
                            return "0";

                        }
                    }
                }

            }
        }

        public string actualizarReyesPagar(string numdocumento) 
        {

            string codigo = new Random().Next(1, 10000).ToString();

            string fechaActual = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            using (NpgsqlConnection connection = new NpgsqlConnection(contexto.connectionString))
            {
                try
                {
                    connection.Open();

                    string sql = @"UPDATE SORTEOS_REYES AS SR SET codigo_very= '"+codigo+ "' FROM REYES R INNER JOIN CLIENTES C ON C.ID_CLIENTE = R.ID_CLIENTE WHERE C.NUMERO_IDENTIFICACION = '"+numdocumento+"' AND SR.ESTADO_PAGO_REY = 'N' AND SR.ID_REY = R.ID_REY";

                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@codigo_ver", codigo);
                        command.ExecuteNonQuery();
                    }
                }
                catch (NpgsqlException ex)
                {

                    Console.WriteLine("Error de PostgreSQL: " + ex.Message);

                    throw;
                }
                catch (Exception ex)
                {

                    Console.WriteLine("Error al guardar cliente: " + ex.Message);

                    throw;
                }
            }

            return codigo;

        }

    }
}