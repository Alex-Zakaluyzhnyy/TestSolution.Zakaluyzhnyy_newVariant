using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace TestSolution.Zakaluyzhnyy
{
    public partial class Test : Form
    {
        private int[] CellsAndRows = new int[2]; //массив, который хранит индекс текущей ячейки
        private string text; //вспомогательная строка, которая используется для создания запроса к БД
        private string connectionString = @"Data Source=DESKTOP-H8MAQ6J\SQLEXPRESS;Initial Catalog=test;Integrated Security=True"; //строка подключения к БД
        private string sql; //строка для формирования запроса
        bool flag; //переменна, определяющая был ли выбран критерий анализа


        public Test()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            this.dISTRIBUTORSTableAdapter.Fill(this.testDataSet.DISTRIBUTORS);
            this.pRICESTableAdapter.Fill(this.testDataSet.PRICES);
            this.dISTRIBUTORSTableAdapter.Fill(this.testDataSet.DISTRIBUTORS);
        }

        private void fillByToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.dISTRIBUTORSTableAdapter.FillBy(this.testDataSet.DISTRIBUTORS);
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

        }

        private void dISTRIBUTORSBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.dISTRIBUTORSBindingSource.EndEdit();

        }

        //метод, позволяющий загрузить в выбранный DataGridVied 1 столбец из БД
        private void request(DataGridView dataGridView, string reqSql)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(reqSql, connection);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                dataGridView.DataSource = ds.Tables[0];
            }
        }

        //метод, позволяющий получить текущий индекс ячейки
        private int[] getCellRow(DataGridView dataGridView)
        {
            int nc = dataGridView.CurrentCell.ColumnIndex;
            int nr = dataGridView.CurrentCell.RowIndex;
            int[] RowAndCell = new int[2] { nc, nr };
            return RowAndCell;
        }

        //метод, позволяющий загрузить в объект DataGridView сразу несколько столбцов из БД
        private void loadData(DataGridView dataGridView,string productName, string sqlRequest)
        {
            int counter = 0;
            SqlConnection myConnection = new SqlConnection(connectionString);
            myConnection.Open();
            SqlCommand command = new SqlCommand(sqlRequest, myConnection);
            command.Parameters.Add("@text",SqlDbType.VarChar, 250).Value = productName;
            command.Prepare();
            SqlDataReader reader = command.ExecuteReader();
            List<string[]> data = new List<string[]>();
            while (reader.Read())
            {
                data.Add(new string[dataGridView.ColumnCount]);
                while (counter != (dataGridView.ColumnCount))
                {
                    data[data.Count - 1][counter] = reader[counter].ToString();
                    counter++;
                }
                counter = 0;
            }
            reader.Close();
            myConnection.Close();
            foreach (var elem in data)
                dataGridView.Rows.Add(elem);
            sql = " ";
        }

        //comboBox1 - форма, содержащая поставщиков. При выборе поставщика предоставляет его ID
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            sql = "SELECT NAME FROM dbo.PRICES WHERE dbo.PRICES.DISID = '" + comboBox1.SelectedValue.ToString() + "'";
            request(dataGridView1, sql);
            sql = " ";
        }

        //dataGridView1 - объект, содержащий список прайс-листов выбранного поставщика
        //dataGridView2 - объект, содержащий базовый прайс-лист
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            text = dataGridView1.CurrentCell.Value.ToString();
            dataGridView1.Rows.RemoveAt(CellsAndRows[0]);
            dataGridView2.Columns.Add("Column1","Прайс-лист");
            dataGridView2.Rows.Add();
            dataGridView2.Rows[0].Cells[0].Value = text;
        }

        //flag - переменная, позволяющая уловить переключение значения в combobox2
        //combobox2 - объект, содержащий критерий выполнения анализа
        private void comboBox2_SelectedValueChanged(object sender, EventArgs e)
        {
            flag = true;
        }

        //непосредственно проведение анализа
        //dataGridView3 - вспомогательная таблица, содержащая все информацию из БД для выбранного прайс-листа
        //dataGridView4 - Таблица 1 из ТЗ
        //dataGridVie5 - Таблица 2 из ТЗ
        private void button1_Click(object sender, EventArgs e)
        {
            text = " ";
            if ((dataGridView2.ColumnCount == 1) && (dataGridView2.Rows.Count >= 1) && (flag))
            {
                CellsAndRows = getCellRow(dataGridView2);
                text = dataGridView2.Rows[CellsAndRows[0]].Cells[CellsAndRows[1]].Value.ToString();
                sql = "SELECT * FROM dbo.PRODUCTS WHERE dbo.PRODUCTS.ID IN (SELECT CATALOGPRODUCTID FROM dbo.LINKS WHERE LINKS. PRICERECORDINDEX IN (SELECT RECORDINDEX FROM dbo.PRICESRECORDS WHERE dbo.PRICESRECORDS.PRICEID IN (SELECT dbo.PRICES.ID FROM dbo.PRICES WHERE (USED = 1) AND (DELETED = 0) AND (ISACTIVE = 1) AND (dbo.PRICES.NAME = '" + text + "'))))";
                request(dataGridView3, sql);
                int rows = dataGridView3.Rows.Count;
                sql = " ";

                text = " ";
                string[] addNumberColumns = { "Column1", "Column2", "Column3", "Column4", "Column5", "Column6", "Column7", "Column8", "Column9" };
                string[] addNameColumns = { "Наименование товара", "Цена", "Минимальная цена", "Поставщик", "Прайс-лист", "Разница", "Максимальная цена", "Разница", "Примечание" };
                for (int i = 0; i <= addNameColumns.Length - 1; i++)
                {
                    dataGridView4.Columns.Add(addNameColumns[i], addNameColumns[i]);
                }
                for (int i = 0; i < rows - 2 ; i++)
                    dataGridView4.Rows.Add();
                if(comboBox2.Text.Contains("Все"))
                {
                    double doubleOne;
                    double doubleTwo;
                    string[] addNumberColumns5 = { "Column1", "Column2", "Column3" };
                    string[] addNameColumns5 = { "Минимальная цена", "Поставщик", "Прайс-лист" };
                    for (int k = 0; k < addNameColumns5.Length; k++)
                    {
                        dataGridView5.Columns.Add(addNumberColumns5[k], addNameColumns5[k]);
                    }
                    for (int i = 0; i < rows - 1; i++)
                    {
                        dataGridView4.Rows[i].Cells[0].Value = dataGridView3.Rows[i].Cells[1].Value.ToString();
                        dataGridView4.Rows[i].Cells[1].Value = dataGridView3.Rows[i].Cells[3].Value.ToString();
                        text = dataGridView3.Rows[i].Cells[1].Value.ToString();
                        sql = " ";
                        sql = "SELECT P2.Цена, DISTRIBUTORS.NAME, P2.[Имя_прайс-листа] FROM(SELECT P1.NAME 'Наименование_товара', PRICES.NAME 'Имя_прайс-листа', PRICES.DISID 'ИД_связи', P1.PRICE 'Цена', P1.COMMENT 'Комментарий' FROM(SELECT * FROM dbo.PRICESRECORDS  WHERE RECORDINDEX IN(SELECT PRICERECORDINDEX  FROM dbo.LINKS WHERE CATALOGPRODUCTID IN(SELECT ID FROM PRODUCTS WHERE NAME = (@text))) AND(USED = 1)) AS P1 JOIN PRICES ON P1.PRICEID = PRICES.ID  WHERE(DELETED = 0) AND(ISACTIVE = 1)) AS P2 JOIN DISTRIBUTORS ON P2.[ИД_связи] = DISTRIBUTORS.ID WHERE DISTRIBUTORS.ACTIVE = 1";
                        dataGridView5.Rows.Clear();
                        loadData(dataGridView5, text, sql);
                        dataGridView5.Sort(dataGridView5.Columns["Column1"], ListSortDirection.Ascending);
                        if ((dataGridView5.Rows[0].Cells[2].Value != null)&&(dataGridView2.Rows[CellsAndRows[0]].Cells[CellsAndRows[1]].Value.ToString() != dataGridView5.Rows[0].Cells[2].Value.ToString()))
                        {
                            dataGridView4.Rows[i].Cells[2].Value = dataGridView5.Rows[0].Cells[0].Value.ToString();
                            dataGridView4.Rows[i].Cells[3].Value = dataGridView5.Rows[0].Cells[1].Value.ToString();
                            dataGridView4.Rows[i].Cells[4].Value = dataGridView5.Rows[0].Cells[2].Value.ToString();
                        }
                        else
                            dataGridView4.Rows[i].Cells[2].Value = dataGridView4.Rows[i].Cells[1].Value.ToString();

                        dataGridView4.Rows[i].Cells[5].Value = Convert.ToDouble(dataGridView4.Rows[i].Cells[1].Value) - Convert.ToDouble(dataGridView4.Rows[i].Cells[2].Value);
                        doubleOne = Convert.ToDouble(dataGridView4.Rows[i].Cells[1].Value);
                        doubleTwo = Convert.ToDouble(dataGridView4.Rows[i].Cells[2].Value);
                        if (doubleOne > doubleTwo)
                        {
                            dataGridView4.Rows[i].Cells[1].Style.BackColor = Color.Red;
                            dataGridView4.Rows[i].Cells[2].Style.BackColor = Color.Green;
                        }
                        text = dataGridView3.Rows[i].Cells[1].Value.ToString();
                        sql = " ";
                        sql = "SELECT P2.Цена, DISTRIBUTORS.NAME, P2.[Имя_прайс-листа] FROM(SELECT P1.NAME 'Наименование_товара', PRICES.NAME 'Имя_прайс-листа', PRICES.DISID 'ИД_связи', P1.PRICE 'Цена', P1.COMMENT 'Комментарий' FROM(SELECT * FROM dbo.PRICESRECORDS  WHERE RECORDINDEX IN(SELECT PRICERECORDINDEX  FROM dbo.LINKS WHERE CATALOGPRODUCTID IN(SELECT ID FROM PRODUCTS WHERE NAME = (@text))) AND(USED = 1)) AS P1 JOIN PRICES ON P1.PRICEID = PRICES.ID  WHERE(DELETED = 0) AND(ISACTIVE = 1)) AS P2 JOIN DISTRIBUTORS ON P2.[ИД_связи] = DISTRIBUTORS.ID WHERE DISTRIBUTORS.ACTIVE = 1";
                        dataGridView5.Rows.Clear();
                        loadData(dataGridView5, text, sql);
                        dataGridView5.Sort(dataGridView5.Columns["Column1"], ListSortDirection.Descending);
                        if ((dataGridView5.Rows[0].Cells[2].Value != null) &&(dataGridView2.Rows[CellsAndRows[0]].Cells[CellsAndRows[1]].Value.ToString() != dataGridView5.Rows[0].Cells[2].Value.ToString()))
                        {
                            if (Convert.ToDouble(dataGridView5.Rows[0].Cells[0].Value) > Convert.ToDouble(dataGridView4.Rows[i].Cells[1].Value))
                            {
                                dataGridView4.Rows[i].Cells[6].Value = dataGridView5.Rows[0].Cells[0].Value.ToString();
                                dataGridView4.Rows[i].Cells[8].Value = dataGridView5.Rows[0].Cells[2].Value.ToString();
                            }

                            else
                            {
                                dataGridView4.Rows[i].Cells[6].Value = Convert.ToDouble(dataGridView4.Rows[i].Cells[1].Value);
                                dataGridView4.Rows[i].Cells[8].Value = dataGridView2.Rows[CellsAndRows[0]].Cells[CellsAndRows[1]].Value.ToString();
                            }
                        }
                        else
                            dataGridView4.Rows[i].Cells[6].Value = dataGridView4.Rows[i].Cells[1];
                        if (!int.TryParse(dataGridView4.Rows[i].Cells[6].Value.ToString(), out int result))
                            dataGridView4.Rows[i].Cells[7].Value = 0 - Convert.ToDouble(dataGridView4.Rows[i].Cells[1].Value);
                        else
                            dataGridView4.Rows[i].Cells[7].Value = Convert.ToDouble(dataGridView4.Rows[i].Cells[6].Value) - Convert.ToDouble(dataGridView4.Rows[i].Cells[1].Value);
                    }
                    dataGridView5.Rows.Clear();
                    string[] wideAddNumberColumns5 = { "Column1", "Column2", "Column3", "Column4", "Column5" };
                    string[] wideAddNameColumns5 = { "Поставщик", " Прайс-лист", "Наименования товара ", " Цена", "Разница" };
                    for (int k = 0; k < wideAddNameColumns5.Length - 1; k++)
                    {
                        if ((k >= 0) && (k < 3))
                            dataGridView5.Columns[k].HeaderText = wideAddNameColumns5[k];
                        else
                            dataGridView5.Columns.Add(wideAddNumberColumns5[k], wideAddNameColumns5[k]);
                    }
                }

                if (comboBox2.Text.Contains("Минимальная цена у других"))
                {

                    string[] addNumberColumns5 = { "Column1", "Column2", "Column3" };
                    string[] addNameColumns5 = { "Минимальная цена", "Поставщик", "Прайс-лист" };
                    for (int k = 0; k < addNameColumns5.Length; k++)
                    {
                        dataGridView5.Columns.Add(addNumberColumns5[k], addNameColumns5[k]);
                    }

                    for (int i = 0; i < rows - 1; i++)
                    {
                        dataGridView4.Rows[i].Cells[0].Value = dataGridView3.Rows[i].Cells[1].Value.ToString();
                        dataGridView4.Rows[i].Cells[1].Value = dataGridView3.Rows[i].Cells[3].Value.ToString();
                        text = dataGridView3.Rows[i].Cells[1].Value.ToString();
                        sql = " ";
                        sql = "SELECT P2.Цена, DISTRIBUTORS.NAME, P2.[Имя_прайс-листа] FROM(SELECT P1.NAME 'Наименование_товара', PRICES.NAME 'Имя_прайс-листа', PRICES.DISID 'ИД_связи', P1.PRICE 'Цена', P1.COMMENT 'Комментарий' FROM(SELECT * FROM dbo.PRICESRECORDS  WHERE RECORDINDEX IN(SELECT PRICERECORDINDEX  FROM dbo.LINKS WHERE CATALOGPRODUCTID IN(SELECT ID FROM PRODUCTS WHERE NAME = (@text))) AND(USED = 1)) AS P1 JOIN PRICES ON P1.PRICEID = PRICES.ID  WHERE(DELETED = 0) AND(ISACTIVE = 1)) AS P2 JOIN DISTRIBUTORS ON P2.[ИД_связи] = DISTRIBUTORS.ID WHERE DISTRIBUTORS.ACTIVE = 1";
                        dataGridView5.Rows.Clear();
                        loadData(dataGridView5, text, sql);
                        dataGridView5.Sort(dataGridView5.Columns["Column1"], ListSortDirection.Ascending);
                        if ((dataGridView5.Rows[0].Cells[2].Value != null) &&(dataGridView2.Rows[CellsAndRows[0]].Cells[CellsAndRows[1]].Value.ToString() != dataGridView5.Rows[0].Cells[2].Value.ToString()) && (Convert.ToDouble(dataGridView5.Rows[0].Cells[0].Value) <= Convert.ToDouble(dataGridView4.Rows[i].Cells[1].Value)))
                        {
                            dataGridView4.Rows[i].Cells[2].Value = dataGridView5.Rows[0].Cells[0].Value.ToString();
                            dataGridView4.Rows[i].Cells[3].Value = dataGridView5.Rows[0].Cells[1].Value.ToString();
                            dataGridView4.Rows[i].Cells[4].Value = dataGridView5.Rows[0].Cells[2].Value.ToString();
                            dataGridView4.Rows[i].Cells[1].Style.BackColor = Color.Red;
                            dataGridView4.Rows[i].Cells[2].Style.BackColor = Color.Green;
                            dataGridView4.Rows[i].Cells[5].Value = Convert.ToDouble(dataGridView4.Rows[i].Cells[1].Value) - Convert.ToDouble(dataGridView4.Rows[i].Cells[2].Value);

                            text = dataGridView3.Rows[i].Cells[1].Value.ToString();
                            sql = " ";
                            sql = "SELECT P2.Цена, DISTRIBUTORS.NAME, P2.[Имя_прайс-листа] FROM(SELECT P1.NAME 'Наименование_товара', PRICES.NAME 'Имя_прайс-листа', PRICES.DISID 'ИД_связи', P1.PRICE 'Цена', P1.COMMENT 'Комментарий' FROM(SELECT * FROM dbo.PRICESRECORDS  WHERE RECORDINDEX IN(SELECT PRICERECORDINDEX  FROM dbo.LINKS WHERE CATALOGPRODUCTID IN(SELECT ID FROM PRODUCTS WHERE NAME = (@text))) AND(USED = 1)) AS P1 JOIN PRICES ON P1.PRICEID = PRICES.ID  WHERE(DELETED = 0) AND(ISACTIVE = 1)) AS P2 JOIN DISTRIBUTORS ON P2.[ИД_связи] = DISTRIBUTORS.ID WHERE DISTRIBUTORS.ACTIVE = 1";
                            dataGridView5.Rows.Clear();
                            loadData(dataGridView5, text, sql);
                            dataGridView5.Sort(dataGridView5.Columns["Column1"], ListSortDirection.Descending);
                            if ((dataGridView5.Rows[0].Cells[2].Value != null) && (dataGridView2.Rows[CellsAndRows[0]].Cells[CellsAndRows[1]].Value.ToString() != dataGridView5.Rows[0].Cells[2].Value.ToString()))
                            {
                                if (Convert.ToDouble(dataGridView5.Rows[0].Cells[0].Value) > Convert.ToDouble(dataGridView4.Rows[i].Cells[1].Value))
                                {
                                    dataGridView4.Rows[i].Cells[6].Value = dataGridView5.Rows[0].Cells[0].Value.ToString();
                                    dataGridView4.Rows[i].Cells[8].Value = dataGridView5.Rows[0].Cells[2].Value.ToString();
                                }

                                else
                                {
                                    dataGridView4.Rows[i].Cells[6].Value = Convert.ToDouble(dataGridView4.Rows[i].Cells[1].Value);
                                    dataGridView4.Rows[i].Cells[8].Value = dataGridView2.Rows[CellsAndRows[0]].Cells[CellsAndRows[1]].Value.ToString();
                                }
                            }
                            else
                                dataGridView4.Rows[i].Cells[6].Value = dataGridView4.Rows[i].Cells[1];
                            if (!int.TryParse(dataGridView4.Rows[i].Cells[6].Value.ToString(), out int result))
                                dataGridView4.Rows[i].Cells[7].Value = 0 - Convert.ToDouble(dataGridView4.Rows[i].Cells[1].Value);
                            else
                                dataGridView4.Rows[i].Cells[7].Value = Convert.ToDouble(dataGridView4.Rows[i].Cells[6].Value) - Convert.ToDouble(dataGridView4.Rows[i].Cells[1].Value);

                        }
                        else
                            dataGridView4.Rows[i].Cells[2].Value = null;
                    }
                    dataGridView5.Rows.Clear();
                    string[] wideAddNumberColumns5 = { "Column1", "Column2", "Column3", "Column4", "Column5" };
                    string[] wideAddNameColumns5 = { "Поставщик", " Прайс-лист", "Наименования товара ", " Цена", "Разница" };
                    for (int k = 0; k < wideAddNameColumns5.Length - 1; k++)
                    {
                        if ((k >= 0) && (k < 3))
                            dataGridView5.Columns[k].HeaderText = wideAddNameColumns5[k];
                        else
                            dataGridView5.Columns.Add(wideAddNumberColumns5[k], wideAddNameColumns5[k]);
                    }
                }

                if (comboBox2.Text.Contains("Минимальная цена здесь"))
                {
                    string[] addNumberColumns5 = { "Column1", "Column2", "Column3" };
                    string[] addNameColumns5 = { "Минимальная цена", "Поставщик", "Прайс-лист" };
                    for (int k = 0; k < addNameColumns5.Length; k++)
                    {
                        dataGridView5.Columns.Add(addNumberColumns5[k], addNameColumns5[k]);
                    }

                    for (int i = 0; i < rows - 1; i++)
                    {
                        dataGridView4.Rows[i].Cells[0].Value = dataGridView3.Rows[i].Cells[1].Value.ToString();
                        dataGridView4.Rows[i].Cells[1].Value = dataGridView3.Rows[i].Cells[3].Value.ToString();
                        text = dataGridView3.Rows[i].Cells[1].Value.ToString();
                        sql = " ";
                        sql = "SELECT P2.Цена, DISTRIBUTORS.NAME, P2.[Имя_прайс-листа] FROM(SELECT P1.NAME 'Наименование_товара', PRICES.NAME 'Имя_прайс-листа', PRICES.DISID 'ИД_связи', P1.PRICE 'Цена', P1.COMMENT 'Комментарий' FROM(SELECT * FROM dbo.PRICESRECORDS  WHERE RECORDINDEX IN(SELECT PRICERECORDINDEX  FROM dbo.LINKS WHERE CATALOGPRODUCTID IN(SELECT ID FROM PRODUCTS WHERE NAME = (@text))) AND(USED = 1)) AS P1 JOIN PRICES ON P1.PRICEID = PRICES.ID  WHERE(DELETED = 0) AND(ISACTIVE = 1)) AS P2 JOIN DISTRIBUTORS ON P2.[ИД_связи] = DISTRIBUTORS.ID WHERE DISTRIBUTORS.ACTIVE = 1";
                        dataGridView5.Rows.Clear();
                        loadData(dataGridView5, text, sql);
                        dataGridView5.Sort(dataGridView5.Columns["Column1"], ListSortDirection.Ascending);
                        if ((dataGridView5.Rows[0].Cells[2].Value != null) && (dataGridView2.Rows[CellsAndRows[0]].Cells[CellsAndRows[1]].Value.ToString() != dataGridView5.Rows[0].Cells[2].Value.ToString()))
                        {
                            if (Convert.ToDouble(dataGridView5.Rows[0].Cells[0].Value) > Convert.ToDouble(dataGridView4.Rows[i].Cells[1].Value))
                            {
                                dataGridView4.Rows[i].Cells[2].Value = dataGridView5.Rows[0].Cells[0].Value.ToString();
                                dataGridView4.Rows[i].Cells[3].Value = dataGridView5.Rows[0].Cells[1].Value.ToString();
                                dataGridView4.Rows[i].Cells[4].Value = dataGridView5.Rows[0].Cells[2].Value.ToString();
                                dataGridView4.Rows[i].Cells[1].Style.BackColor = Color.Green;
                            }
                            else
                            {
                                dataGridView4.Rows[i].Cells[2].Value = dataGridView5.Rows[0].Cells[0].Value.ToString();
                                dataGridView4.Rows[i].Cells[3].Value = dataGridView5.Rows[0].Cells[1].Value.ToString();
                                dataGridView4.Rows[i].Cells[4].Value = dataGridView5.Rows[0].Cells[2].Value.ToString();
                                dataGridView4.Rows[i].Cells[1].Style.BackColor = Color.Red;
                            }
                        }
                        dataGridView4.Rows[i].Cells[5].Value = Convert.ToDouble(dataGridView4.Rows[i].Cells[1].Value) - Convert.ToDouble(dataGridView4.Rows[i].Cells[2].Value);
                        text = dataGridView3.Rows[i].Cells[1].Value.ToString();
                        sql = " ";
                        sql = "SELECT P2.Цена, DISTRIBUTORS.NAME, P2.[Имя_прайс-листа] FROM(SELECT P1.NAME 'Наименование_товара', PRICES.NAME 'Имя_прайс-листа', PRICES.DISID 'ИД_связи', P1.PRICE 'Цена', P1.COMMENT 'Комментарий' FROM(SELECT * FROM dbo.PRICESRECORDS  WHERE RECORDINDEX IN(SELECT PRICERECORDINDEX  FROM dbo.LINKS WHERE CATALOGPRODUCTID IN(SELECT ID FROM PRODUCTS WHERE NAME = (@text))) AND(USED = 1)) AS P1 JOIN PRICES ON P1.PRICEID = PRICES.ID  WHERE(DELETED = 0) AND(ISACTIVE = 1)) AS P2 JOIN DISTRIBUTORS ON P2.[ИД_связи] = DISTRIBUTORS.ID WHERE DISTRIBUTORS.ACTIVE = 1";
                        dataGridView5.Rows.Clear();
                        loadData(dataGridView5, text, sql);
                        dataGridView5.Sort(dataGridView5.Columns["Column1"], ListSortDirection.Descending);
                        if ((dataGridView5.Rows[0].Cells[2].Value != null) && (dataGridView2.Rows[CellsAndRows[0]].Cells[CellsAndRows[1]].Value.ToString() != dataGridView5.Rows[0].Cells[2].Value.ToString()))
                        {
                            if (Convert.ToDouble(dataGridView5.Rows[0].Cells[0].Value) > Convert.ToDouble(dataGridView4.Rows[i].Cells[1].Value))
                            {
                                dataGridView4.Rows[i].Cells[6].Value = dataGridView5.Rows[0].Cells[0].Value.ToString();
                                dataGridView4.Rows[i].Cells[8].Value = dataGridView5.Rows[0].Cells[2].Value.ToString();
                            }

                            else
                            {
                                dataGridView4.Rows[i].Cells[6].Value = Convert.ToDouble(dataGridView4.Rows[i].Cells[1].Value);
                                dataGridView4.Rows[i].Cells[8].Value = dataGridView2.Rows[CellsAndRows[0]].Cells[CellsAndRows[1]].Value.ToString();
                            }
                        }
                        else
                            dataGridView4.Rows[i].Cells[6].Value = Convert.ToDouble(dataGridView4.Rows[i].Cells[1].Value);
                        if (!int.TryParse(dataGridView4.Rows[i].Cells[6].Value.ToString(), out int result))
                            dataGridView4.Rows[i].Cells[7].Value = 0 - Convert.ToDouble(dataGridView4.Rows[i].Cells[1].Value);
                        else
                            dataGridView4.Rows[i].Cells[7].Value = Convert.ToDouble(dataGridView4.Rows[i].Cells[6].Value) - Convert.ToDouble(dataGridView4.Rows[i].Cells[1].Value);

                    }
                    dataGridView5.Rows.Clear();
                    string[] wideAddNumberColumns5 = { "Column1", "Column2", "Column3", "Column4", "Column5" };
                    string[] wideAddNameColumns5 = { "Поставщик", " Прайс-лист", "Наименования товара ", " Цена", "Разница" };
                    for (int k = 0; k < wideAddNameColumns5.Length - 1; k++)
                    {
                        if ((k >= 0) && (k < 3))
                            dataGridView5.Columns[k].HeaderText = wideAddNameColumns5[k];
                        else
                            dataGridView5.Columns.Add(wideAddNumberColumns5[k], wideAddNameColumns5[k]);
                    }
                }
            }

            else MessageBox.Show("Выберете прайс-лист и  критерий для анализа!");
        }

        private void dataGridView4_CellClick(object sender, DataGridViewCellEventArgs e)
        {
           string[] wideAddNumberColumns5 = { "Column1", "Column2", "Column3", "Column4", "Column5" };
           string[] wideAddNameColumns5 = { "Поставщик", " Прайс-лист", "Наименования товара ", " Цена", "Разница" };
           text = dataGridView4.CurrentCell.Value.ToString();
           int currentRow = Convert.ToInt32(dataGridView4.CurrentCell.RowIndex);
           sql = " ";
           sql = "SELECT DISTRIBUTORS.NAME 'Производитель', P2.[Имя_прайс-листа], P2.[Наименование_товара],  P2.Цена FROM(SELECT P1.NAME 'Наименование_товара', PRICES.NAME 'Имя_прайс-листа', PRICES.DISID 'ИД_связи', P1.PRICE 'Цена', P1.COMMENT 'Комментарий' FROM(SELECT * FROM dbo.PRICESRECORDS  WHERE RECORDINDEX IN(SELECT PRICERECORDINDEX  FROM dbo.LINKS WHERE CATALOGPRODUCTID IN(SELECT ID FROM PRODUCTS WHERE NAME = (@text))) AND(USED = 1)) AS P1 JOIN PRICES ON P1.PRICEID = PRICES.ID  WHERE(DELETED = 0) AND(ISACTIVE = 1)) AS P2 JOIN DISTRIBUTORS ON P2.[ИД_связи] = DISTRIBUTORS.ID WHERE DISTRIBUTORS.ACTIVE = 1";
           loadData(dataGridView5, text, sql);
           dataGridView5.Rows[1].Cells[0].Value = text;
           dataGridView5.Columns.Add(wideAddNumberColumns5[4], wideAddNameColumns5[4]);
           dataGridView5.Sort(dataGridView5.Columns["Column4"], ListSortDirection.Ascending);
           for (int i = 0; i < dataGridView5.Rows.Count - 1; i++)
            {
                dataGridView5.Rows[i].Cells[4].Value = Convert.ToDouble(dataGridView5.Rows[i].Cells[3].Value) - Convert.ToDouble(dataGridView4.Rows[currentRow].Cells[1].Value);
            }
        }
    }
}
