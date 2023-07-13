using AIaaS.Application.Common.Models;
using AIaaS.WebAPI.ExtensionMethods;
using AIaaS.WebAPI.Interfaces;
using Ardalis.Result;
using CleanArchitecture.Application.Common.Interfaces;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Dynamic;

namespace AIaaS.WebAPI.Services.PredictionService
{
    public class PredictionService : IPredictionService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IPredictionBuilderDirector _predictionBuilderDirector;

        public PredictionService(IApplicationDbContext dbContext, IPredictionBuilderDirector predictionBuilderDirector)
        {
            _dbContext = dbContext;
            _predictionBuilderDirector = predictionBuilderDirector;
        }

        public async Task<Result<SimplePrediction>> Predict(int endpointId, PredictionInputDto? predictionInputDto)
        {
            throw new NotImplementedException();
            //try
            //{
            //    #region Authentication
            //    //TODO: Validate authentication type
            //    //var accessToken = Request.Headers[HeaderNames.Authorization];

            //    //if (!accessToken.ToString().Contains("ZDk0YjUyZDYtZWQxNi00NWQwLTg2ZGUtOGM3NjBhNWM2Njcx"))
            //    //    return new StatusCodeResult(401);
            //    #endregion


            //    #region Validators and parameter builders
            //    if (predictionInputDto?.Data is null)
            //    {
            //        return Result.Error("Data parameter is required");
            //    }

            //    if (predictionInputDto.Columns != null && predictionInputDto.Columns.Length != predictionInputDto.Data.Length)
            //    {
            //        return Result.Error("Columns and Data paramters must have same quantity of elements");
            //    }

            //    var endpoint = await _dbContext.Endpoints.FindAsync(endpointId);

            //    if (endpoint is null)
            //    {
            //        return Result.NotFound("Endpoint not found");
            //    }

            //    if (!endpoint.IsEnabled)
            //    {
            //        return Result.Error("Endpoint is disabled");
            //    }

            //    await _dbContext.Entry(endpoint).Reference(x => x.MLModel).LoadAsync();

            //    if (endpoint.MLModel is null)
            //    {
            //        return Result.NotFound("Model not found");
            //    }

            //    await _dbContext.Entry(endpoint.MLModel).Reference(x => x.Workflow).LoadAsync();

            //    if (endpoint.MLModel.Workflow is null)
            //    {
            //        return Result.NotFound("Workflow not found");
            //    }

            //    if (string.IsNullOrEmpty(endpoint.MLModel.Workflow.Data))
            //    {
            //        return Result.NotFound("Workflow Data not found");
            //    }

            //    if (!endpoint.MLModel.Data.Any())
            //    {
            //        return Result.Error("Model has no data");
            //    }

            //    var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            //    var workflowGraphDto = JsonSerializer.Deserialize<WorkflowGraphDto>(endpoint.MLModel.Workflow.Data, jsonOptions);

            //    if (workflowGraphDto?.Root is null)
            //    {
            //        return Result.Error("Workflow empty after deserialization");
            //    }

            //    var nodes = workflowGraphDto.Root.ToList();
            //    var trainingNode = nodes.FirstOrDefault(x => x.Type.Equals(OperatorType.Train.ToString(), StringComparison.InvariantCultureIgnoreCase));
            //    var datasetNode = nodes.FirstOrDefault(x => x.Type.Equals(OperatorType.Dataset.ToString(), StringComparison.InvariantCultureIgnoreCase));

            //    if (datasetNode is null)
            //    {
            //        return Result.Error("Dataset operator not found");
            //    }

            //    var selectedColumns = datasetNode.GetParameterValue<string, IList<string>>("SelectedColumns", x => JsonSerializer.Deserialize<IList<string>>(x));

            //    //TODO: maybe last columns from last node?
            //    if (selectedColumns is null || !selectedColumns.Any())
            //    {
            //        return Result.Error("No selected columns found on Dataset operator");
            //    }

            //    if (trainingNode is null)
            //    {
            //        return Result.Error("Train Model operator not found");
            //    }

            //    var task = trainingNode.GetParameterValue("Task");
            //    if (string.IsNullOrEmpty(task))
            //    {
            //        return Result.Error("Task not found");
            //    };

            //    if (!task.Equals("Regression", StringComparison.InvariantCultureIgnoreCase) && !task.Equals("BinaryClassification", StringComparison.InvariantCultureIgnoreCase))
            //    {
            //        return Result.Error($"Wrong task: {task}");
            //    };

            //    #endregion

            //    //TODO: at this point I could instantiate a specific predictor and delegate below logic
            //    #region Predictor logic

            //    var label = trainingNode.GetParameterValue("Label");

            //    if (string.IsNullOrEmpty(label))
            //    {
            //        return Result.Error("Label not found");
            //    };

            //    var mlContext = new MLContext();
            //    using var memStream = new MemoryStream(endpoint.MLModel.Data);
            //    var trainedModel = mlContext.Model.Load(memStream, out var inputSchema);
            //    var runtimeType = ClassFactory.CreateType(inputSchema);
            //    var column = inputSchema.GetColumnOrNull(label);

            //    if (column is null)
            //    {
            //        return Result.Error("Label column not found");
            //    }

            //    //TODO: compare with dataset.selected columns?
            //    if (predictionInputDto.Data.Length != selectedColumns.Count - 1)
            //    {
            //        return Result.Error($"Data values and schema definition must have same quanity of elements. Data values: {predictionInputDto.Data.Length}, schema definition columns: {inputSchema.Count}");
            //    }

            //    //TODO: compare with dataset.selected columns?
            //    if (predictionInputDto.Columns != null)
            //    {
            //        var missingColumns = selectedColumns.Where(x => !x.Equals(label, StringComparison.InvariantCultureIgnoreCase) &&
            //            !predictionInputDto.Columns.Contains(x, StringComparer.InvariantCultureIgnoreCase));

            //        if (missingColumns.Any())
            //        {
            //            return Result.Error($"There are missing columns from parameter: {string.Join(',', missingColumns)}");
            //        }
            //    }

            //    //TODO: the problem is that inputSchema has original schema rather than the transformed
            //    var predictionObjectType = task.Equals("Regression", StringComparison.InvariantCultureIgnoreCase) ?
            //    ClassFactory.CreateType(new[] { (label, ((DataViewSchema.Column)column).Type.ToRawType(), "Score") }) :
            //    ClassFactory.CreateType(new[] { (label, ((DataViewSchema.Column)column).Type.ToRawType(), "PredictedLabel"), ("Score", typeof(float), "Score") });
            //    //new str   ClassFactory.CreateType(ing[] { label }, new Type[] { ((DataViewSchema.Column)column).Type.RawType, typeof(float) }, new[] { "Score" }) :
            //    //ClassFactory.CreateObject(new string[] { label, "Score" }, new Type[] { ((DataViewSchema.Column)column).Type.RawType, typeof(float) }, new[] { "PredictedLabel", "Score" });

            //    //TODO: compare with dataset.selected columns?
            //    var featureColumns = inputSchema.Where(x => !x.Name.Equals(label, StringComparison.InvariantCultureIgnoreCase) &&
            //    selectedColumns.Contains(x.Name, StringComparer.InvariantCultureIgnoreCase)).ToArray();

            //    if (predictionInputDto.Columns?.Length > 0)
            //    {
            //        featureColumns = predictionInputDto.Columns
            //            .Where(x => !x.Equals(label, StringComparison.InvariantCultureIgnoreCase) &&
            //             selectedColumns.Contains(x, StringComparer.InvariantCultureIgnoreCase))
            //            .Select(x => inputSchema.FirstOrDefault(y => y.Name.Equals(x, StringComparison.InvariantCultureIgnoreCase))).ToArray();
            //    }

            //    if (runtimeType is null)
            //    {
            //        return Result.Error("Error when trying to create runtime type");
            //    }

            //    dynamic runtimeInstance = Activator.CreateInstance(runtimeType);

            //    for (int i = 0; i < featureColumns.Length; i++)
            //    {
            //        var columnName = featureColumns[i].Name;
            //        var value = predictionInputDto.Data[i]?.ToString() ?? "";
            //        var property = runtimeType.GetProperty(columnName);

            //        if (property is null) continue;//TODO: think this case
            //        try
            //        {
            //            var convertedValue = featureColumns[i].Type is TextDataViewType ?
            //                value :
            //                Convert.ChangeType(value, featureColumns[i].Type.ToRawType());

            //            property.SetValue(runtimeInstance, convertedValue);
            //        }
            //        catch (Exception ex)
            //        {
            //            //TODO: think this case
            //        }
            //    }

            //    dynamic dynamicPredictionEngine;
            //    var genericPredictionMethod = mlContext.Model.GetType().GetMethod("CreatePredictionEngine", new[] { typeof(ITransformer), typeof(bool), typeof(SchemaDefinition), typeof(SchemaDefinition) });
            //    var predictionMethod = genericPredictionMethod.MakeGenericMethod(runtimeType, predictionObjectType);
            //    dynamicPredictionEngine = predictionMethod.Invoke(mlContext.Model, new object[] { trainedModel, true, null, null });

            //    var predictMethod = dynamicPredictionEngine.GetType().GetMethod("Predict", new[] { runtimeType });
            //    var predictedObject = predictMethod.Invoke(dynamicPredictionEngine, new[] { runtimeInstance });

            //    var predictedPropertyLabel = predictionObjectType.GetProperty(label);
            //    if (predictedPropertyLabel is null)
            //    {
            //        return Result.Error("Error when trying to get the Label predicted property");
            //    }

            //    var predictedValue = predictedPropertyLabel.GetValue(predictedObject);

            //    var predictedPropertyScore = predictionObjectType.GetProperty("Score");
            //    if (task.Equals("BinaryClassification", StringComparison.InvariantCultureIgnoreCase) && predictedPropertyScore is null)
            //    {
            //        return Result.Error("Error when trying to get the Score predicted property");
            //    }

            //    var predictedScore = predictedPropertyScore?.GetValue(predictedObject);

            //    var simplePrediction = new SimplePrediction()
            //    {
            //        Value = predictedValue,
            //        Score = predictedScore
            //    };

            //    #endregion

            //    var result = Result.Success(simplePrediction);
            //    return result;
            //}
            //catch (Exception ex)
            //{
            //    var message = ex.Message;
            //    return Result.Error($"Error when trying to generate prediction: {ex.Message}");
            //}
        }

        public async Task<Result<object>> Predict(int endpointId, StreamReader streamReader, bool onlyPredictionProperties = false)
        {
            try
            {
                #region Authentication
                //TODO: Validate authentication type
                //var accessToken = Request.Headers[HeaderNames.Authorization];

                //if (!accessToken.ToString().Contains("ZDk0YjUyZDYtZWQxNi00NWQwLTg2ZGUtOGM3NjBhNWM2Njcx"))
                //    return new StatusCodeResult(401);
                #endregion

                var predictParameter = new PredictionParameter()
                    .SetEnpointId(endpointId)
                    .SetStreamReader(streamReader)
                    .SetOnlyPredictedProperties(onlyPredictionProperties);

                var result = await _predictionBuilderDirector.BuildPredictionParameter(predictParameter);

                if (!result.IsSuccess)
                {
                    return Result<object>.Error(result.Errors.ToArray());
                }

                var prediction = this.GeneratePrediction(predictParameter);

                if (prediction is null)
                {
                    return Result.Error("Error when trying to predict: prediction is null");
                }

                dynamic dynamicObject = predictParameter.OnlyPredictedProperties ?
                    prediction :
                    this.MergeInputParameterWithPrediction(predictParameter, prediction);

                return Result.Success((object)dynamicObject);
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return Result.Error($"Error when trying to generate prediction: {ex.Message}");
            }
        }

        private dynamic MergeInputParameterWithPrediction(PredictionParameter predictParameter, object? aPrediction)
        {
            dynamic dynamicObject = new ExpandoObject();

            foreach (var prop in predictParameter.RuntimeTypeInput.GetProperties())
            {
                //skip if predictedObject already has the property
                if (predictParameter.RuntimeTypeOutput.GetProperty(prop.Name) is not null)
                {
                    continue;
                }

                var value = prop.GetValue(predictParameter.RuntimeInstancesInput);
                ((IDictionary<string, object>)dynamicObject)[prop.Name] = value;
            }

            foreach (var prop in predictParameter.RuntimeTypeOutput.GetProperties())
            {
                var value = prop.GetValue(aPrediction);
                ((IDictionary<string, object>)dynamicObject)[prop.Name] = value;
            }

            return dynamicObject;
        }

        private object? GeneratePrediction(PredictionParameter predictParameter)
        {
            var dynamicPredictionEngine = predictParameter.MLContext.Model.InvokeGenericMethod(
                new Type[] { predictParameter.RuntimeTypeInput, predictParameter.RuntimeTypeOutput },
                "CreatePredictionEngine",
                new[] { typeof(ITransformer),
                    typeof(bool), typeof(SchemaDefinition), typeof(SchemaDefinition) },
                new object[] { predictParameter.TrainedModel, true, null, null });

            var predictedObject = dynamicPredictionEngine?.InvokeMethod("Predict", new[] { predictParameter.RuntimeTypeInput }, new[] { predictParameter.RuntimeInstancesInput });
            return predictedObject;
        }

        public class SimplePrediction
        {
            public object Value { get; set; }
            public float? Score { get; set; }
        }

    }

}
