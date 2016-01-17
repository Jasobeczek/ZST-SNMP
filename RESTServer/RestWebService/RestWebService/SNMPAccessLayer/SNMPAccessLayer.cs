using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNMP_NMS_STATION;
using Lextm.SharpSnmpLib;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Converters;

namespace RestWebService.SNMPAccessLayer
{
    public class AccessLayer
    {
        private SNMPCommandHandler SNMPHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessLayer"/> class.
        /// </summary>
        public AccessLayer()
        {
            this.SNMPHandler = new SNMPCommandHandler();
        }
        /// <summary>
        /// Gets the specified uid value.
        /// </summary>
        /// <param name="uidID">The uid value.</param>
        /// <returns></returns>
        public String Get(String uidID)
        {
            ResultData resultData = new ResultData(SNMPHandler.SNMP_GET(uidID));
            return JsonConvert.SerializeObject(resultData, new ResultDataConverter());
        }
        /// <summary>
        /// Gets the table.
        /// </summary>
        /// <param name="uidID">The uid value.</param>
        /// <returns></returns>
        public String GetTable(String uidID)
        {
            return JsonConvert.SerializeObject(this.ConvertVariables(SNMPHandler.SNMP_GET_TABLE(uidID)), new ResultDataConverter());
        }
        /// <summary>
        /// Convert Variable table to list of ResultData
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns></returns>
        private ResultData[,] ConvertVariables(Variable[,] table)
        {
            ResultData[,] returnTable = new ResultData[table.GetLength(0), table.GetLength(1)];
            for (int y = 0; y < table.GetLength(0); y++)
            {
                for (int x = 0; x < table.GetLength(1); x++)
                {
                    returnTable.SetValue(new ResultData(table[y, x]), y, x);
                }
            }
            return returnTable;
        }

        /// <summary>
        /// Sets the specified uid identifier.
        /// </summary>
        /// <param name="uidID">The uid identifier.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        internal String Set(byte[] value)
        {
            ResultData resultData = JsonConvert.DeserializeObject<ResultData>(Encoding.ASCII.GetString(value), new ResultDataConverter());
            return this.SNMPHandler.SNMP_SET(resultData.ID, resultData.Data);
        }
    }
    /// <summary>
    /// Represent Variable.ToString() data in more managable way
    /// </summary>
    public class ResultData
    {
        public String ID { get; set; }
        public String Data { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultData"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ResultData(Variable value)
        {
            if (value != null)
            {
                this.ID = value.Id.ToString();
                this.Data = value.Data.ToString();
            }
            else
            {
                this.ID = String.Empty;
                this.Data = String.Empty;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultData" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="data">The data.</param>
        
        public ResultData(String id, String data)
        {
            this.ID = id;
            this.Data = data;
        }

        public ResultData()
        {
            this.ID = String.Empty;
            this.Data = String.Empty;
        }

        /// <summary>
        /// ResultData table to String
        /// </summary>
        /// <param name="resultDataTable">The result data table.</param>
        /// <returns></returns>
        public static String[,] ToTableString(ResultData[,] resultDataTable)
        {
            String[,] resultStringTable = new String[resultDataTable.GetLength(0), resultDataTable.GetLength(1)];
            for (int y = 0; y < resultDataTable.GetLength(0); y++)
            {
                for (int x = 0; x < resultDataTable.GetLength(1); x++)
                {
                    resultStringTable.SetValue(resultDataTable[y, x].Data, y, x);
                }
            }
            return resultStringTable;
        }
    }


    public class ResultDataConverter : CustomCreationConverter<ResultData>
    {
        public override ResultData Create(Type objectType)
        {
            return new ResultData();
        }
    }
}
