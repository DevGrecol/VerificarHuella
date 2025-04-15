using PruebaDigitalPerson.Conexion;
using PruebaDigitalPerson;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Npgsql;

namespace PruebaDigitalPerson
{
    public partial class frmRegistrar : Form
    {
        private DPFP.Template Template;
        private ConexionBD contexto;
        public frmRegistrar()
        {
            InitializeComponent();
        }

        private void btnRegistrarHuella_Click(object sender, EventArgs e)
        {
            if (txtDocumento.Text == "" || txtIdentificadorCliente.Text == "")
            {

                MessageBox.Show("Error al consultar el cliente: debe de ingresar una cedula de cliente registrado", "Error");

            }
            else
            {
                CapturarHuella capturar = new CapturarHuella();
                capturar.OnTemplate += this.OnTemplate;
                capturar.ShowDialog();
            }
            
        }

        private void OnTemplate(DPFP.Template template)
        {
            this.Invoke(new Function(delegate ()
            {
                Template = template;
                btnAgregar.Enabled = (Template != null);
                if (Template != null)
                {
                    MessageBox.Show("La plantilla de huella dactilar está lista para la verificación de huella dactilar.", "Registro de huella dactilar");
                    txtHuella.Text = "Huella capturada correctamente";
                }
                else
                {
                    MessageBox.Show("La plantilla de huella dactilar no es válida. Repita el registro de huella dactilar");
                }
            }));
        }

        private void frmRegistrar_Load(object sender, EventArgs e)
        {
            
            contexto = new ConexionBD();
            //Listar();
        }

        private void Limpiar()
        {
            txtIdentificadorCliente.Text = "";
        }

        /*private void Listar()
        {
            string query = "SELECT nombre FROM empleados"; // Reemplaza 'empleados' con el nombre de tu tabla
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine(reader["nombre"].ToString());
                        }
                    }
                }
            }
        } */

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] streamHuella = Template.Bytes;

                Cliente cliente = new Cliente()
                {
                    id_cliente =  Convert.ToInt32(txtIdentificadorCliente.Text),
                    huella = streamHuella,

                };

                contexto.GuardarHuellaCliente(cliente);
                contexto.SaveChanges();
                MessageBox.Show("Huella agregada al cliente correctamente");
                Limpiar();
                Template = null;
                btnAgregar.Enabled = false;



                txtDocumento.Text = "";
                txtNombreCliente.Text = "";
                txtApellidoCliente.Text = "";
                txtIdentificadorCliente.Text = "";
                txtHuella.Text = "";

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void buttonBuscar_Click(object sender, EventArgs e)
        {


            if (txtDocumento.Text == "")
            {

                MessageBox.Show("Error al consultar el cliente: debe de ingresar la cedula del cliente", "Error");

            }
            else{

                contexto = new ConexionBD();

                try
                {

                    Cliente cliente = new Cliente()
                    {
                      numero_identificacion = txtDocumento.Text,
                    };

                    ///metodo para consultar un usuario
                    contexto.ConsultarCliente(cliente); 


                    txtNombreCliente.Text = cliente.nombres;
                    txtApellidoCliente.Text = cliente.apellidos;

                    if (cliente.huella != null)
                    {
                        btnRegistrar.Enabled = false;
                        btnAgregar.Enabled = false;

                    }
                    else{

                        btnRegistrar.Enabled = true;
                       
                    }
                    
                   
                    txtIdentificadorCliente.Text = Convert.ToString(cliente.id_cliente);

                    //btnAgregar.Enabled = false;

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al consultar el cliente: " + ex.Message, "Error");
                }

            }
            
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}
