using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.IO;
using System.Data.OleDb;
//using Expedicao.Models;

namespace Expedicao.Extensions
{
    public class Utility
    {
        internal static DataTable ConvertXLSXtoDataTable(string connString, string nomeABa)
        {
            DataTable dt = new DataTable();
            OleDbConnection connection = new OleDbConnection(connString);

            try
            {
                DataSet ds = new DataSet();

                connection.Open();
                OleDbCommand command = new OleDbCommand("SELECT * FROM [" + nomeABa + "]", connection);
                OleDbDataAdapter oleda = new OleDbDataAdapter(command);
                oleda.Fill(ds);
                dt = ds.Tables[0];

            }
            catch (Exception e)
            {
                DebugLog.Logar(e.Message.ToString());
                DebugLog.Logar(e.StackTrace);
            }
            finally
            {
                connection.Close();
            }
            return dt;
        }

        internal static DateTime GetFormatDate(String date, String hour)
        {

            DateTime formatedDate = new DateTime();
            string formatedOne = date.Split()[0];
            string formatedTwo = "";

            if (!string.IsNullOrEmpty(hour))
            {
                formatedTwo = hour.Split()[1];
            }
            else
            {
                formatedTwo = "00:00:00";
            }
            DateTime.TryParse(formatedOne + " " + formatedTwo, out formatedDate);

            return formatedDate;
        }

        internal static String Details(Exception e)
        {
            string detail = "";

            while (e != null)
            {
                e = e.InnerException;
                detail = (e != null) ? e.Message : detail;
            }

            return detail;
        }

        internal static DataTable ExportListToDataTable<T>(List<T> objects)
        {
            DataTable table = new DataTable();

            try
            {
                if (objects.Count > 0)
                {
                    var myCastedObject = Convert.ChangeType(objects[0], typeof(T));
                    //getting collumns
                    foreach (var prop in myCastedObject.GetType().GetProperties())
                    {
                        if (myCastedObject.GetType().GetProperty(prop.Name).PropertyType.FullName.StartsWith("System."))
                        {
                            DataColumn col = new DataColumn();
                            col.ColumnName = GetPropertyLabel(prop.Name.Clone().ToString());
                            table.Columns.Add(col);
                        }
                    }

                    foreach (T obj in objects)
                    {
                        myCastedObject = Convert.ChangeType(obj, typeof(T));
                        DataRow row = table.NewRow();

                        foreach (var prop in myCastedObject.GetType().GetProperties())
                        {
                            if (myCastedObject.GetType().GetProperty(prop.Name).PropertyType.FullName.StartsWith("System."))
                            {
                                var propertyValue = myCastedObject.GetType().GetProperty(prop.Name).GetValue(myCastedObject, null);
                                row[GetPropertyLabel(prop.Name.Clone().ToString())] = propertyValue != null ? propertyValue.ToString() : "";
                            }
                        }
                        table.Rows.Add(row);
                    }
                }
            }
            catch (Exception e)
            {
                DebugLog.Logar(e.Message);
                DebugLog.Logar(e.StackTrace);
            }

            return table;
        }

        private static string GetPropertyLabel(string propertyName)
        {
            if (propertyName.Count() > 2)
            {
                propertyName = propertyName.Replace("Id", "");
                var propertySize = propertyName.Count();

                for (int i = 1; i < propertySize; i++)
                {
                    if (Char.IsUpper(propertyName.ElementAt(i)))
                    {
                        propertyName = propertyName.Insert(i, " ");
                        i++;
                        propertySize++;
                    }
                }

            }
            return propertyName;
        }
    }
}