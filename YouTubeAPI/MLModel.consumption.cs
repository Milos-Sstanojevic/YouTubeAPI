﻿// This file was auto-generated by ML.NET Model Builder. 
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
namespace YouTubeAPI
{
    public partial class MLModel
    {
        /// <summary>
        /// model input class for MLModel.
        /// </summary>
        #region model input class
        public class ModelInput
        {
            [ColumnName(@"CommentId")]
            public string CommentId { get; set; }

            [ColumnName(@"VideoId")]
            public string VideoId { get; set; }

            [ColumnName(@"Text")]
            public string Text { get; set; }

            [ColumnName(@"IsToxic")]
            public string IsToxic { get; set; }

            [ColumnName(@"IsAbusive")]
            public string IsAbusive { get; set; }

            [ColumnName(@"IsThreat")]
            public string IsThreat { get; set; }

            [ColumnName(@"IsProvocative")]
            public string IsProvocative { get; set; }

            [ColumnName(@"IsObscene")]
            public string IsObscene { get; set; }

            [ColumnName(@"IsHatespeech")]
            public string IsHatespeech { get; set; }

            [ColumnName(@"IsRacist")]
            public string IsRacist { get; set; }

            [ColumnName(@"IsNationalist")]
            public string IsNationalist { get; set; }

            [ColumnName(@"IsSexist")]
            public string IsSexist { get; set; }

            [ColumnName(@"IsHomophobic")]
            public string IsHomophobic { get; set; }

            [ColumnName(@"IsReligiousHate")]
            public string IsReligiousHate { get; set; }

            [ColumnName(@"IsRadicalism")]
            public string IsRadicalism { get; set; }

        }

        #endregion

        /// <summary>
        /// model output class for MLModel.
        /// </summary>
        #region model output class
        public class ModelOutput
        {
            [ColumnName(@"CommentId")]
            public string CommentId { get; set; }

            [ColumnName(@"VideoId")]
            public string VideoId { get; set; }

            [ColumnName(@"Text")]
            public float[] Text { get; set; }

            [ColumnName(@"IsToxic")]
            public uint IsToxic { get; set; }

            [ColumnName(@"IsAbusive")]
            public string IsAbusive { get; set; }

            [ColumnName(@"IsThreat")]
            public string IsThreat { get; set; }

            [ColumnName(@"IsProvocative")]
            public string IsProvocative { get; set; }

            [ColumnName(@"IsObscene")]
            public string IsObscene { get; set; }

            [ColumnName(@"IsHatespeech")]
            public string IsHatespeech { get; set; }

            [ColumnName(@"IsRacist")]
            public string IsRacist { get; set; }

            [ColumnName(@"IsNationalist")]
            public string IsNationalist { get; set; }

            [ColumnName(@"IsSexist")]
            public string IsSexist { get; set; }

            [ColumnName(@"IsHomophobic")]
            public string IsHomophobic { get; set; }

            [ColumnName(@"IsReligiousHate")]
            public string IsReligiousHate { get; set; }

            [ColumnName(@"IsRadicalism")]
            public string IsRadicalism { get; set; }

            [ColumnName(@"Features")]
            public float[] Features { get; set; }

            [ColumnName(@"PredictedLabel")]
            public string PredictedLabel { get; set; }

            [ColumnName(@"Score")]
            public float[] Score { get; set; }

        }

        #endregion

        private static string MLNetModelPath = Path.GetFullPath("MLModel.zip");

        public static readonly Lazy<PredictionEngine<ModelInput, ModelOutput>> PredictEngine = new Lazy<PredictionEngine<ModelInput, ModelOutput>>(() => CreatePredictEngine(), true);

        /// <summary>
        /// Use this method to predict on <see cref="ModelInput"/>.
        /// </summary>
        /// <param name="input">model input.</param>
        /// <returns><seealso cref=" ModelOutput"/></returns>
        public static ModelOutput Predict(ModelInput input)
        {
            var predEngine = PredictEngine.Value;
            return predEngine.Predict(input);
        }

        private static PredictionEngine<ModelInput, ModelOutput> CreatePredictEngine()
        {
            var mlContext = new MLContext();
            ITransformer mlModel = mlContext.Model.Load(MLNetModelPath, out var _);
            return mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
        }
    }
}
