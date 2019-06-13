﻿using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace MLQuickDraw.Model.DataModels
{
    public class ModelInputV2
    {
        [ColumnName("Label"), LoadColumn(0)]
        public string Label { get; set; }
        [ColumnName("Data"), VectorType(300), LoadColumn(1, 300)]
        public float[] Data { get; set; }
    }
}
