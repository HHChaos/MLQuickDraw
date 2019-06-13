using Microsoft.ML;
using MLQuickDraw.Model.DataModels;
using System;
using System.IO;
using System.Linq;

namespace MLQuickDraw.ConsoleApp
{
    class Program
    {
        //Machine Learning model to load and use for predictions
        private const string MODEL_FILEPATH = @"MLModel.zip";

        //Dataset to use for predictions 
        private const string DATA_FILEPATH = @"Data\data5000.csv";

        static void Main(string[] args)
        {
            //MLContext mlContext = new MLContext();

            // Training code used by ML.NET CLI and AutoML to generate the model
            var useV1 = true;
            Console.WriteLine("键入创建V1还是V2 Model：（输入1或者2，推荐使用V2，训练时间短，效果也好的多）");
            if (int.TryParse(Console.ReadLine(), out var inputChoice))
            {
                if (inputChoice != 1)
                    useV1 = false;
            }
            Console.WriteLine("键入训练数据文件地址：");
            var trainDataPath = Console.ReadLine();
            if (useV1)
            {
                ModelBuilderV1.CreateModel(trainDataPath);
            }
            else
            {
                ModelBuilderV2.CreateModel(trainDataPath);
            }


            //ITransformer mlModel = mlContext.Model.Load(GetAbsolutePath(MODEL_FILEPATH), out DataViewSchema inputSchema);
            //var predEngine = mlContext.Model.CreatePredictionEngine<ModelInputV1, ModelOutputV1>(mlModel);

            //// Create sample data to do a single prediction with it 
            //ModelInputV1 sampleData = CreateSingleDataSample(mlContext, DATA_FILEPATH);

            //// Try a single prediction
            //ModelOutputV1 predictionResult = predEngine.Predict(sampleData);

            //Console.WriteLine($"Single Prediction --> Actual value: {sampleData.Label} | Predicted value: {predictionResult.Prediction} | Predicted scores: [{String.Join(",", predictionResult.Score)}]");

            //Console.WriteLine("=============== End of process, hit any key to finish ===============");
            //Console.ReadKey();
        }

        // Method to load single row of data to try a single prediction
        // You can change this code and create your own sample data here (Hardcoded or from any source)
        private static ModelInputV1 CreateSingleDataSample(MLContext mlContext, string dataFilePath)
        {
            // Read dataset to get a single row for trying a prediction          
            IDataView dataView = mlContext.Data.LoadFromTextFile<ModelInputV1>(
                                            path: dataFilePath,
                                            hasHeader: false,
                                            separatorChar: ',',
                                            allowQuoting: true,
                                            allowSparse: false);

            // Here (ModelInput object) you could provide new test data, hardcoded or from the end-user application, instead of the row from the file.
            ModelInputV1 sampleForPrediction = mlContext.Data.CreateEnumerable<ModelInputV1>(dataView, false)
                                                                        .First();
            return sampleForPrediction;
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }
}
