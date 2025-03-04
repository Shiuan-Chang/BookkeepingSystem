﻿using AutoMapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using 記帳系統.Contract;
using 記帳系統.DataGridViewExtension;
using 記帳系統.Models;
using 記帳系統.Mappings;
using 記帳系統.Presenters;
using 記帳系統.Repository;


namespace 記帳系統.Forms
{
    // 了解一下記憶體外洩以及下載3G，5G高清圖片

    [DisplayName("筆記本")]
    public partial class NoteForm : Form, INoteView
    {
        private NotePresenter notePresenter;
        public NoteForm()
        {
            // timer要隔一段時間後，才去做button要做的事
            // button 永遠去呼叫debounce做的事情，因此，會有一個debounce方法，debouce會更改及清空timer的時間(也就是說timer一定會在debounce中)
            InitializeComponent();
            //dataGridView1.CellClick += CellClick;
            startPicker.Value = DateTime.Today.AddDays(-30);
            var config = new MapperConfiguration(cfg => {
                cfg.AddProfile<AutoMapperProfile>();
            });
            IMapper mapper = config.CreateMapper();
            notePresenter = new NotePresenter(this, mapper);
            dataGridView1.CellBeginEdit += DataGridViewExtension.DataGridViewExtension.dataGridView1_CellBeginEdit;
            dataGridView1.CellValueChanged += DataGridViewExtension.DataGridViewExtension.dataGridView1_CellValueChanged;
            dataGridView1.CellClick += dataGridView1_CellClick;
            //dataGridView1.CellEndEdit += dataGridView1_CellEndEdit;
        }

        private void NoteForm_Load(object sender, EventArgs e)
        {

        }
        //設定全局只有一個定時器存在
        private static System.Threading.Timer debounceTimer;
        private static object timerLock = new object();
        private bool isLoading = false;

        //一開始把計時器歸零，然後再做延遲計算，防止使用者不斷重複按查詢造成記憶體爆炸
        private void button1_Click(object sender, EventArgs e)
        {
            this.Debounce(() => {
                notePresenter.LoadData(startPicker.Value, endPicker.Value);
            }, 1000);
        }

        public void Reload()
        {
            SearchData();
        }

        private void SearchData()
        {
            notePresenter.LoadData(startPicker.Value, endPicker.Value);
        }

        public void ClearDataGridView()
        {
            dataGridView1.DataSource = null;
            dataGridView1.Columns.Clear();
            GC.Collect();
        }

        public void UpdateDataView(List<NoteModel> lists)
        {
            ClearDataGridView();
            dataGridView1.DataSource = lists;
            dataGridView1.SetupDataColumns(lists);
        }

        // 僅是抓取這一格的值，不需要丟到DataGridViewExtension去額外操作
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 10) 
            {
                var dateValue = dataGridView1.Rows[e.RowIndex].Cells[2].Value;

                if (dateValue != null && !string.IsNullOrEmpty(dateValue.ToString()))
                {
                    var deleteModel = new DeleteNoteModel(dateValue.ToString());

                    notePresenter.DeleteData(deleteModel);

                }
            }
        }
    }
}

//lock (timerLock)
//{
//    // 如果計時器已經存在，則更改其計時延遲
//    if (debounceTimer != null)
//    {
//        debounceTimer.Change(1000, Timeout.Infinite); 
//        return; // 结束方法
//    }

//    // 如果計時器不存在，則創建一個新的計時器
//    debounceTimer = new System.Threading.Timer(LoadData, null, 1000, Timeout.Infinite);
//}

//不要這樣用，這樣會有很多個timer
//Timer debounceTimer = new Timer();

//debounceTimer.Tick += (s, args) =>
//{
//    debounceTimer.Stop();//停止計時器防止多次觸發
//    debounceTimer.Dispose(); // 釋放計時器資源
//    LoadData();
//};

//debounceTimer.Interval = 1000; // 延遲時間
//debounceTimer.Start();

//private void LoadData(object state)
//{
//    var form = (NoteForm)Application.OpenForms["NoteForm"];
//    // 把datasource指向null
//    //HW825:可以拿到起訖之間的資料，注意檢查之間日期是否有資料夾，沒有就跳過，否則會報錯
//    // 拿到piker 日期相減的值(span)，檢查該日期期間是否存在資料夾逐一做檢查
//    if (form != null)
//    {
//        form.Invoke(new Action(() =>
//        {
//            List<AccountingModel> Lists = new List<AccountingModel>();
//            DateTime startDate = startPicker.Value;
//            TimeSpan dateSpan = endPicker.Value - startPicker.Value;
//            int timePeriod = dateSpan.Days;
//            //找到路徑中日期存在相符的資料夾
//            string csvSearchPath = @"C:\Users\icewi\OneDrive\桌面\testCSV";
//            for (int i = 0; i <= timePeriod; i++)
//            {
//                string folderPath = Path.Combine(csvSearchPath, startDate.AddDays(i).ToString("yyyy-MM-dd"), $"data.csv");
//                if (File.Exists(folderPath))
//                {
//                    List<AccountingModel> periodList = CSVHelper.CSV.ReadCSV<AccountingModel>(folderPath, true);
//                    Lists.AddRange(periodList);
//                }
//            }

//            //為了做到記憶體管理
//            dataGridView1.DataSource = null;

//            // 清空所有columns的欄位
//            dataGridView1.Columns.Clear();

//            // 記憶體回收
//            GC.Collect();

//            //下方程式碼再載入時會造成過多記憶體的損耗以及增加讀取的時間，即使有記憶體回收仍會造成該狀況，因此因從源頭處理，把撈資料的期間限縮
//            //string csvReadPath = Path.Combine(@"C:\Users\icewi\OneDrive\桌面\testCSV", "csvDataPath", $"data.csv");
//            //List<AccountingModel> readList = CSVHelper.CSV.ReadCSV<AccountingModel>(csvReadPath, true);
//            dataGridView1.DataSource = Lists;

//            dataGridView1.Columns["compressImagePath1"].Visible = false;
//            dataGridView1.Columns["compressImagePath2"].Visible = false;
//            dataGridView1.Columns["csvImagePath1"].Visible = false;
//            dataGridView1.Columns["csvImagePath2"].Visible = false;
//            //DataGridViewImageColumn

//            dataGridView1.Columns[0].HeaderText = "日期";
//            dataGridView1.Columns[1].HeaderText = "帳目名稱";
//            dataGridView1.Columns[2].HeaderText = "帳目類型";
//            dataGridView1.Columns[3].HeaderText = "細項";
//            dataGridView1.Columns[4].HeaderText = "支付方式";
//            dataGridView1.Columns[5].HeaderText = "金額";

//            DataGridViewImageColumn iconColumn1 = new DataGridViewImageColumn();
//            DataGridViewImageColumn iconColumn2 = new DataGridViewImageColumn();
//            iconColumn1.HeaderText = "圖片一";
//            iconColumn1.ImageLayout = DataGridViewImageCellLayout.Zoom;

//            iconColumn2.HeaderText = "圖片二";
//            iconColumn2.ImageLayout = DataGridViewImageCellLayout.Zoom;

//            dataGridView1.Columns.Insert(8, iconColumn1);
//            dataGridView1.Columns.Insert(9, iconColumn2);

//            string csvPath1 = "";

//            if (dataGridView1.Rows.Count > 0)
//            {
//                // Print or store the first row's csvImagePath1 for debugging or other uses
//                csvPath1 = Lists[0].compressImagePath1;
//            }

//            for (int row = 0; row < dataGridView1.Rows.Count; row++)
//            {
//                // 先去讀csvImagePath1,2的資料
//                Bitmap bmp1 = new Bitmap(Lists[row].csvImagePath1);
//                Bitmap bmp2 = new Bitmap(Lists[row].csvImagePath2);
//                ((DataGridViewImageCell)dataGridView1.Rows[row].Cells[8]).Value = bmp1;
//                ((DataGridViewImageCell)dataGridView1.Rows[row].Cells[9]).Value = bmp2;
//                // 存四張圖，2張原圖縮小(50*50封面)並略調畫質，另外兩張點進去看到的是壓縮檔圖，約300-500kb
//            }
//        }));
//    }
//}
//private void HideColumns(string[] columnNames)
//{
//    foreach (string columnName in columnNames)
//    {
//        if (dataGridView1.Columns.Contains(columnName))
//        {
//            dataGridView1.Columns[columnName].Visible = false;
//        }
//    }
//}

//private void SetColumnHeaderText(int columnIndex, string headerText)
//{
//    if (columnIndex < dataGridView1.Columns.Count)
//    {
//        dataGridView1.Columns[columnIndex].HeaderText = headerText;
//    }
//}

//private void SetColumnCellTemplate(int columnIndex, DataGridViewCell cellTemplate)
//{
//    if (columnIndex < dataGridView1.Columns.Count)
//    {
//        dataGridView1.Columns[columnIndex].CellTemplate = cellTemplate;
//    }
//}

//private void AddImageColumn(string headerText, DataGridViewImageCellLayout layout)
//{
//    DataGridViewImageColumn imageColumn = new DataGridViewImageColumn
//    {
//        HeaderText = headerText,
//        ImageLayout = layout
//    };
//    dataGridView1.Columns.Add(imageColumn);
//}

//private void SetRowImageValues(int rowIndex, Bitmap[] images)
//{
//    if (rowIndex < dataGridView1.Rows.Count)
//    {
//        for (int i = 0; i < images.Length; i++)
//        {
//            ((DataGridViewImageCell)dataGridView1.Rows[rowIndex].Cells[dataGridView1.Columns.Count - images.Length + i]).Value = images[i];
//        }
//    }
//}