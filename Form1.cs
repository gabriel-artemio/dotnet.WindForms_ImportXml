using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindForms_ImportXml
{
    public partial class FrmPrincipal : Form
    {
        MySqlConnection conn;
        public FrmPrincipal()
        {
            InitializeComponent();
        }
        private void btnImportar_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = @"C:\";
                openFileDialog.Filter = "Arquivos xml (*.xml)|*.xml|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.Multiselect = true;
                openFileDialog.RestoreDirectory = true;

                try
                {
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        prgBar1.Maximum = openFileDialog.FileNames.Length;
                        prgBar1.Value = 0;

                        foreach (String file in openFileDialog.FileNames)
                        {
                            try
                            {
                                XmlDataSet.ReadXml(file);
                                dataGridView1.DataSource = XmlDataSet;
                                dataGridView1.DataMember = "PESSOA";
                            }
                            catch (SecurityException ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                            prgBar1.Value++;
                            Application.DoEvents();
                        }
                        MessageBox.Show("Dados carregados.");
                        prgBar1.Value = 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnImportBD_Click(object sender, EventArgs e)
        {
            try
            {
                string data_source = "datasource=localhost;username={seuusuario};password={suasenha};database=import_pessoa";
                conn = new MySqlConnection(data_source);

                DataTable tab_pessoa = XmlDataSet.Tables["PESSOA"];

                prgBar2.Maximum = tab_pessoa.Rows.Count;
                prgBar2.Value = 0;

                for (int linha = 0; linha < tab_pessoa.Rows.Count; linha++)
                {
                    string comando_sql = "INSERT INTO pessoas(";

                    string cpf = tab_pessoa.Rows[linha]["CPF"].ToString();

                    for (int i = 0; i < tab_pessoa.Columns.Count; i++)
                    {
                        comando_sql += tab_pessoa.Columns[i].ColumnName + ",";
                    }
                    comando_sql = comando_sql.Remove(comando_sql.Length - 1);
                    comando_sql += ") SELECT ";

                    for (int coluna = 0; coluna < tab_pessoa.Columns.Count; coluna++)
                    {
                        comando_sql += "'" + tab_pessoa.Rows[linha][coluna].ToString() + "',";
                    }
                    comando_sql = comando_sql.Remove(comando_sql.Length - 1);
                    comando_sql += " WHERE NOT EXISTS(SELECT CPF FROM pessoas " +
                        "WHERE CPF = '" + cpf + "');";

                    MySqlCommand cmd = new MySqlCommand(comando_sql, conn);
                    conn.Open();
                    cmd.ExecuteReader();
                    conn.Close();

                    prgBar2.Value++;
                    Application.DoEvents();
                }
                MessageBox.Show("Dados importados com sucesso!");
                prgBar2.Value = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}