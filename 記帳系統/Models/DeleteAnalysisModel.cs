﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 記帳系統.Models
{
    public class DeleteAnalysisModel
    {
        public string DeleteDataDate;

        public DeleteAnalysisModel(string DeleteDataDate)
        {
            this.DeleteDataDate = DeleteDataDate;
        }
    }
}
