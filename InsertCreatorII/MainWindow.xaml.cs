using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InsertCreatorII
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            // hardcode or load from database, config, xml file, etc..
            var connections = new Dictionary<string, string>();
            connections.Add("Development", @"Server=YourDevServerName;Database=<<database>>;User Id=DevUser;Password=$$Password123;");

            ConnectionsComboBox.ItemsSource = connections;
            ConnectionsComboBox.DisplayMemberPath = "Key";
            ConnectionsComboBox.SelectedValuePath = "Value";
            
            // same, hardcode or load
            var databases = new List<string>();
            databases.Add("NorthWind");

            DatabasesComboBox.ItemsSource = databases;
        }

        private void CreateInsertButton_Click(object sender, RoutedEventArgs e)
        {
            if (ConnectionsComboBox.SelectedIndex > -1 && DatabasesComboBox.SelectedIndex > -1)
            {
                string connectionString = ConnectionsComboBox.SelectedValue.ToString();
                string database = DatabasesComboBox.SelectedItem.ToString();
                connectionString = connectionString.Replace("<<database>>", database);
                using SqlConnection conn = new(connectionString);
                conn.Open();

                using SqlCommand command = new(SqlStatementTextBox.Text, conn);
                using SqlDataReader reader = command.ExecuteReader(CommandBehavior.KeyInfo);

                DataTable schema = reader.GetSchemaTable();

                string tableName = "";
                var columns = new List<string>();
                foreach (DataRow row in schema.Rows)
                {
                    //foreach (DataColumn column in schema.Columns)
                    //{
                        columns.Add(row["ColumnName"].ToString());

                        if(tableName == "")
                        {
                            tableName = row["BaseTableName"].ToString();
                        }
                    //}
                }
                string insert = $"INSERT INTO {tableName} ({string.Join(',', columns)}) VALUES (";

                var insertStatements = new List<string>();
                while (reader.Read())
                {
                    var fieldValues = new List<string>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string type = reader.GetValue(i).GetType().Name;
                        if(type.ToLower() == "dbnull")
                        {
                            fieldValues.Add("NULL");
                        }
                        else if ("string~datetime".Contains(type.ToLower()))
                        {
                            fieldValues.Add($"'{reader.GetValue(i)}'");
                        }
                        else
                        {
                            fieldValues.Add(reader.GetValue(i).ToString());
                        }
                    }

                    insertStatements.Add($"{insert}{string.Join(',', fieldValues)})");
                }
                InsertStatementsTextBox.Text = string.Join(Environment.NewLine, insertStatements);
            }
            else
            {
                MessageBox.Show("Select a connection and database");
            }
            
        }
    }
}
