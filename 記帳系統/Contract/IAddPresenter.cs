using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using 記帳系統.Models;

namespace 記帳系統.Contract
{
    internal interface IAddPresenter
    {
        //儲存由原始view提供的資料，和repository的SaveData把資料儲存到後端是不同一件事情
        void SaveData(AddModel model);
    }
}
