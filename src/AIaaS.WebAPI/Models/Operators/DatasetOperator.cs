using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.ExtensionMethods;
using AIaaS.WebAPI.Models.Dtos;
using AIaaS.WebAPI.Models.enums;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Globalization;
using System.Text.Json;

namespace AIaaS.WebAPI.Models.Operators
{
    [Operator("Dataset", OperatorType.Dataset, 1)]
    [OperatorParameter("Dataset", "A dataset is a data source or input file for training a machine learning model", "list")]
    [OperatorParameter("SelectedColumns", "Selected columns to be included on model training", "list")]
    public class DatasetOperator : WorkflowOperatorAbstract
    {
        private readonly EfContext _dbContext;

        public DatasetOperator(EfContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task Execute(WorkflowContext context, WorkflowNodeDto root)
        {
            var datasetId = root.GetParameterValue<int>("Dataset");

            if (datasetId is null)
            {
                root.Error("Please select a dataset");
                return;
            }

            var selectedColumns = root.GetParameterValue<string, IList<string>>("SelectedColumns", x => JsonSerializer.Deserialize<IList<string>>(x));

            if (selectedColumns is null || !selectedColumns.Any())
            {
                root.Error("Please select columns from dataset");
                return;
            }

            var dataset = await _dbContext.Datasets.FindAsync(datasetId);
            if (dataset is null)
            {
                root.Error("Dataset not found");
                return;
            }

            await _dbContext.Entry(dataset).Collection(d => d.ColumnSettings).LoadAsync();
            await _dbContext.Entry(dataset).Reference(x => x.DataViewFile).LoadAsync();

            context.Dataset = dataset;
            context.ColumnSettings = dataset.ColumnSettings.Where(x => selectedColumns.Contains(x.ColumnName, StringComparer.InvariantCultureIgnoreCase));

            if (!context.ColumnSettings.Any())
            {
                root.Error("No columns were found");
                return;
            }

            if (dataset.DataViewFile is null)
            {
                root.Error("DataViewFile not found");
                return;
            }

            var file = dataset.DataViewFile;

            //TODO: check how to manage usings cos if I dispose IDataview cannot be processed
            var memStream = new MemoryStream(file.Data);
            var mss = new MultiStreamSourceFile(memStream);
            var mlContext = new MLContext();
            context.DataView = mlContext.Data.LoadFromBinary(mss);
            context.InputOutputColumns = context.ColumnSettings.Select(x => new InputOutputColumnPair(x.ColumnName, x.ColumnName)).ToArray();
            //var preview = context.DataView.Preview();
            root.Success();
        }

        private IEnumerable<SalesRow> GetSalesAsEnumerable()
        {
            var list = new List<SalesRow>() {
            new SalesRow { TV = 10.0f, Newspaper = 20.0f, Radio = 30.0f, Sales = 20.0f },
             new SalesRow { TV = 15.0f, Newspaper = 25.0f, Radio = 35.0f, Sales = 25.0f },
                   new SalesRow { TV = 40.0f, Newspaper = 60.0f, Radio = 70.0f, Sales = 80.0f },
                   new SalesRow { TV = 50.0f, Newspaper = 60.0f, Radio = 70.0f, Sales = 80.0f },
            };


            foreach (var item in list)
            {
                yield return item;
            }
        }

        public IDataView NewMethod<T>(MLContext mlContext, StreamReader tr, SchemaDefinition schemaDefinition) where T : class
        {
            var csvConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture);
            var csv = new CsvReader(tr, csvConfiguration);
            T record = (T)Activator.CreateInstance(typeof(T));
            var records = csv.EnumerateRecords(record);
            //var records = csv.GetRecords<T>().Take(50).ToList();

            var data = mlContext.Data.LoadFromEnumerable(records, schemaDefinition);
            //var preview = data.Preview();
            return data;
        }

        public IEnumerable<T> GetRecordsMethod<T>(CsvReader csvReader)
        {
            return csvReader.GetRecords<T>();
        }
    }

    public class SalesRow
    {
        [LoadColumn(0)]
        public float TV { get; set; }
        [LoadColumn(1)]
        public float Radio { get; set; }
        [LoadColumn(2)]
        public float Newspaper { get; set; }
        [LoadColumn(3)]
        public float Sales { get; set; }
    }


}
//TextLoader.Column[] columns = new TextLoader.Column[] {
//                new TextLoader.Column("Tv",DataKind.Single,0),
//            new TextLoader.Column("Radio",DataKind.Single,1),
//            new TextLoader.Column("Newspaper",DataKind.Single,2),
//             new TextLoader.Column("Sales",DataKind.Single,3),
//            };



//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.ML;
//using Microsoft.ML.Data;

//// Define a sample class
//public class MyClass
//{
//    public int Property1 { get; set; }
//    public string Property2 { get; set; }
//}

//class Program
//{
//    static void Main()
//    {
//        // Create an instance of MLContext
//        MLContext mlContext = new MLContext();

//        // Define the data as an IEnumerable<T> created at runtime
//        Type elementType = typeof(MyClass);
//        Type genericListType = typeof(List<>).MakeGenericType(elementType);
//        IList<object> dataList = Activator.CreateInstance(genericListType) as IList<object>;

//        // Add dynamic objects to the list
//        object dynamicObject1 = Activator.CreateInstance(elementType);
//        elementType.GetProperty("Property1").SetValue(dynamicObject1, 1);
//        elementType.GetProperty("Property2").SetValue(dynamicObject1, "Value1");
//        dataList.Add(dynamicObject1);

//        object dynamicObject2 = Activator.CreateInstance(elementType);
//        elementType.GetProperty("Property1").SetValue(dynamicObject2, 2);
//        elementType.GetProperty("Property2").SetValue(dynamicObject2, "Value2");
//        dataList.Add(dynamicObject2);

//        // Load the data using mlContext.Data.LoadFromEnumerable
//        var dataView = mlContext.Data.LoadFromEnumerable(dataList.Cast<dynamic>());

//        // Use the dataView for further processing (e.g., training a model)
//        // ...

//        Console.WriteLine("Data loaded successfully.");
//    }