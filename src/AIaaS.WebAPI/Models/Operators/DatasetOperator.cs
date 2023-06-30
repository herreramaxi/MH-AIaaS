using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Models.CustomAttributes;
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
        private IList<string>? _selectedColumns;
        private int? _datasetId;

        public DatasetOperator(EfContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override Task Hydrate(WorkflowContext mlContext, WorkflowNodeDto root)
        {
            _datasetId = root.GetParameterValue<int>("Dataset");
            _selectedColumns = root.GetParameterValue<string, IList<string>>("SelectedColumns", x => JsonSerializer.Deserialize<IList<string>>(x));
            return Task.CompletedTask;
        }

        public override bool Validate(WorkflowContext mlContext, WorkflowNodeDto root)
        {
            if (_datasetId is null)
            {
                root.SetAsFailed("Please select a dataset");
                return false;
            }

            if (_selectedColumns is null || !_selectedColumns.Any())
            {
                root.SetAsFailed("Please select columns from dataset");
                return false;
            }

            return true;
        }

        public override async Task Run(WorkflowContext context, WorkflowNodeDto root)
        {
            if (_datasetId is null || _selectedColumns is null || !_selectedColumns.Any())
                return;

            var dataset = await _dbContext.Datasets.FindAsync(_datasetId);

            if (dataset is null)
            {
                root.SetAsFailed("Dataset not found");
                return;
            }

            context.Dataset = dataset;
            await _dbContext.Entry(context.Dataset).Collection(d => d.ColumnSettings).LoadAsync();
            await _dbContext.Entry(context.Dataset).Reference(x => x.DataViewFile).LoadAsync();

            if (dataset.DataViewFile is null)
            {
                root.SetAsFailed("DataViewFile not found");
                return;
            }

            var datasetColumnNames = context.Dataset.ColumnSettings.Select(x => x.ColumnName);
            var nonExistingColumnNames = _selectedColumns.Where(x => !datasetColumnNames.Contains(x, StringComparer.InvariantCultureIgnoreCase));

            if (nonExistingColumnNames.Any())
            {
                root.SetAsFailed($"The following selected columns do not exists in Dataset: {string.Join(", ", nonExistingColumnNames)}");
                return;
            }

            //TODO: propagate this columns, so if I add editDataset operator, then it will modify and propagate those columns
            context.ColumnSettings = context.Dataset.ColumnSettings.Where(x => _selectedColumns.Contains(x.ColumnName, StringComparer.InvariantCultureIgnoreCase));
            var columnsToBeDropped = context.Dataset.ColumnSettings.Where(x => !_selectedColumns.Contains(x.ColumnName, StringComparer.InvariantCultureIgnoreCase))
                .Select(x => x.ColumnName)
                .ToArray();

            //TODO: check how to manage usings cos if I dispose IDataview cannot be processed
            var memStream = new MemoryStream(dataset.DataViewFile.Data);
            var mss = new MultiStreamSourceFile(memStream);
            var mlContext = new MLContext();
            context.DataView = mlContext.Data.LoadFromBinary(mss);
            context.InputOutputColumns = context.ColumnSettings.Select(x => new InputOutputColumnPair(x.ColumnName, x.ColumnName)).ToArray();
            //var preview = context.DataView.Preview();

            if (columnsToBeDropped.Any())
            {
                var estimator = context.MLContext.Transforms.DropColumns(columnsToBeDropped);

                context.EstimatorChain = context.EstimatorChain is not null ?
                context.EstimatorChain.Append(estimator) :
                estimator;

                //var transformedData = estimator.Fit(context.DataView).Transform(context.DataView);
            }

            //var transformer = context.EstimatorChain.Fit(context.DataView);
            //var dataview = transformer.Transform(context.DataView);
            //var preview = dataview.Preview(50);
        }

        public override void PropagateDatasetColumns(WorkflowContext context, WorkflowNodeDto root)
        {
            root.SetDatasetColumns(_selectedColumns);
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