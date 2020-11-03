using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Excel = Microsoft.Office.Interop.Excel;
using EService.Data.Entity;
using System.Drawing;
using Microsoft.Office.Interop.Excel;

namespace EService.VVM.ViewModels
{
    public class ReportVM : BaseVM
    {
        #region Поля
        private int rowIndex = 1, colIndex = 1, startCol = 1, startRow = 1, endCol = 1, endRow = 1;

        private IList<SView> _serviceLogs;
        private Excel.Application _application;
        private Excel.Workbook _workbook;
        private Excel.Worksheet _worksheet;
        private IList<TypeModel> _typeModels;
        private IList<Data.Entity.Model> _models;
        private IList<Dept> _depts;
        private IDelegateCommand _createReport;
        #endregion
        #region Свойства
        public IDelegateCommand CreateReportCommand 
        {
            get
            {
                if (_createReport == null)
                {
                    _createReport = new OpenWindowCommand(ExecuteCreateReport, this);
                }
                return _createReport;
            }
        }
        public bool FullReport { get; set; }
        public bool CountReport { get; set; }
        public bool IN { get; set; }
        public bool SN { get; set; }
        public bool Type { get; set; }
        public bool Model { get; set; }
        public bool Dept { get; set; }
        public bool Date { get; set; }
        public bool Category { get; set; }
        public bool Parameter { get; set; }
        public bool Spare { get; set; }
        public bool Service { get; set; }

        #endregion
        #region Кострукторы
        public ReportVM(IList<SView> serviceLogs)
        {
            _serviceLogs = serviceLogs;
            _typeModels = _serviceLogs.Select(s => s.ServiceLog.Device.Model.TypeModel).ToHashSet().ToList();
            _models = _serviceLogs.Select(s => s.ServiceLog.Device.Model).ToHashSet().ToList();
            _depts = _serviceLogs.Select(s => s.ServiceLog.Device.Dept).ToHashSet().ToList();
            FullReport = false;
            CountReport = false;
            IN = true;
            SN = true;
            Type = true;
            Model = true;
            Dept = true;
            Date = true;
            Category = true;
            Parameter = true;
            Spare = true;
            Service = true;
        }
        #endregion
        #region Методы
        private void CreateFullReport()
        {
            _worksheet.Cells[rowIndex, colIndex] = "№"; colIndex++;
            if (IN) { _worksheet.Cells[rowIndex, colIndex] = "I/N"; colIndex++; }
            if (SN) { _worksheet.Cells[rowIndex, colIndex] = "S/N"; colIndex++; }
            if (Type) { _worksheet.Cells[rowIndex, colIndex] = "Тип"; colIndex++; }
            if (Model) { _worksheet.Cells[rowIndex, colIndex] = "Модель"; colIndex++; }
            if (Dept) { _worksheet.Cells[rowIndex, colIndex] = "Отдел"; colIndex++; }
            if (Date) { _worksheet.Cells[rowIndex, colIndex] = "Дата"; colIndex++; }
            if (Category) { _worksheet.Cells[rowIndex, colIndex] = "Категория"; colIndex++; }
            if (Parameter) { _worksheet.Cells[rowIndex, colIndex] = "Параметры"; colIndex++; }
            if (Spare) { _worksheet.Cells[rowIndex, colIndex] = "Запчасти"; colIndex++; }
            if (Service) { _worksheet.Cells[rowIndex, colIndex] = "Работы"; colIndex++; }
            endCol = colIndex - 1;
            foreach (var item in _serviceLogs)
            {
                colIndex = 1;
                rowIndex++;
                StringBuilder sb = new StringBuilder();
                _worksheet.Cells[rowIndex, colIndex] = item.Index; colIndex++;
                if (IN) { _worksheet.Cells[rowIndex, colIndex] = item.ServiceLog.Device.InventoryNumber; colIndex++; }
                if (SN) { _worksheet.Cells[rowIndex, colIndex] = item.ServiceLog.Device.SerialNumber; colIndex++; }
                if (Type) { _worksheet.Cells[rowIndex, colIndex] = item.ServiceLog.Device.Model.TypeModel.ShortName; colIndex++; }
                if (Model) { _worksheet.Cells[rowIndex, colIndex] = item.ServiceLog.Device.Model.ShortName; colIndex++; }
                if (Dept) { _worksheet.Cells[rowIndex, colIndex] = String.Format("{0}({1})", item.ServiceLog.Device.Dept.Name, item.ServiceLog.Device.Dept.Code); colIndex++; }
                if (Date) { _worksheet.Cells[rowIndex, colIndex] = item.ServiceLog.Date; colIndex++; }
                if (Category) { _worksheet.Cells[rowIndex, colIndex] = item.Categories; colIndex++; }
                if (Parameter) 
                {
                    sb.Clear();
                    item.ServiceLog.ParametersValues.ToList().ForEach(i => sb.AppendFormat("{0}: {1} {2}\n", i.ParameterForModel.Parameter.Name, i.Value, i.ParameterForModel.Parameter.Unit));
                    _worksheet.Cells[rowIndex, colIndex] = sb.ToString().TrimEnd('\n');
                    colIndex++;
                }                
                if (Spare)
                {
                    sb.Clear();
                    item.ServiceLog.SparesUsed.ToList().ForEach(i => sb.AppendFormat(" {0},", i.SpareForModel.Spare.Name));
                    _worksheet.Cells[rowIndex, colIndex] = sb.ToString().TrimEnd(',').TrimStart(' ') + ".";
                    colIndex++;
                }
                if (Service)
                {
                    sb.Clear();
                    item.ServiceLog.ServicesDone.ToList().ForEach(i => sb.AppendFormat(" {0},", i.ServiceForModel.Service.ShortName));
                    _worksheet.Cells[rowIndex, colIndex] = sb.ToString().TrimEnd(',').TrimStart(' ') + ".";
                    colIndex++;
                }
            }
            endRow = rowIndex;
            _worksheet.Range[_worksheet.Cells[startRow, startCol], _worksheet.Cells[startRow, endCol]].Font.Bold = true;
            _worksheet.Range[_worksheet.Cells[startRow, startCol], _worksheet.Cells[startRow, endCol]].Interior.Color = ColorTranslator.ToOle(System.Drawing.Color.FromArgb(198, 224, 180));
            _worksheet.Range[_worksheet.Cells[startRow, startCol], _worksheet.Cells[endRow, endCol]].Borders[XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
            _worksheet.Range[_worksheet.Cells[startRow, startCol], _worksheet.Cells[endRow, endCol]].Borders[XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
            _worksheet.Range[_worksheet.Cells[startRow, startCol], _worksheet.Cells[endRow, endCol]].Borders[XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlContinuous;
            _worksheet.Range[_worksheet.Cells[startRow, startCol], _worksheet.Cells[endRow, endCol]].Borders[XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
            _worksheet.Range[_worksheet.Cells[startRow, startCol], _worksheet.Cells[endRow, endCol]].Borders[XlBordersIndex.xlInsideHorizontal].LineStyle = Excel.XlLineStyle.xlContinuous;
            _worksheet.Range[_worksheet.Cells[startRow, startCol], _worksheet.Cells[endRow, endCol]].Borders[XlBordersIndex.xlInsideVertical].LineStyle = Excel.XlLineStyle.xlContinuous;
            _worksheet.Range[_worksheet.Cells[startRow, startCol], _worksheet.Cells[startRow, endCol]].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            _worksheet.Range[_worksheet.Cells[startRow, startCol], _worksheet.Cells[endRow, endCol]].HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
            _worksheet.Range[_worksheet.Cells[startRow, startCol], _worksheet.Cells[endRow, endCol]].VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
            _worksheet.Columns.EntireColumn.AutoFit();
            _worksheet.Rows.EntireColumn.AutoFit();
            rowIndex += 5;
            endRow = rowIndex;
        }

        private void CreateCountReport()
        {
            rowIndex = endRow; colIndex = 1; startRow = endRow; startCol = 1;
            foreach (var item in _typeModels)
            {
                int slIndex = 1; colIndex = 1;
                _worksheet.Cells[rowIndex, colIndex] = item.ShortName;
                rowIndex++;
                startRow = rowIndex;
                _worksheet.Cells[rowIndex, colIndex] = "№"; colIndex++;
                _worksheet.Cells[rowIndex, colIndex] = "Модель"; colIndex++;
                foreach (var i in _depts)
                {
                    _worksheet.Cells[rowIndex, colIndex] = String.Format("{0} [{1}]", i.Name, i.Code); colIndex++;
                }
                rowIndex++;
                endCol = colIndex - 1;
                colIndex = 1;
                foreach (var i in _models)
                {
                    if (i.TypeModel.Rowid == item.Rowid)
                    {
                        _worksheet.Cells[rowIndex, colIndex] = slIndex; slIndex++; colIndex++;
                        _worksheet.Cells[rowIndex, colIndex] = i.FullName; colIndex++;
                        foreach (var j in _depts)
                        {
                            _worksheet.Cells[rowIndex, colIndex] = _serviceLogs.Where(s => s.ServiceLog.Device.Model.Rowid == i.Rowid && s.ServiceLog.Device.Dept.Rowid == j.Rowid).ToList().Count(); colIndex++;
                        }
                        rowIndex++;
                        colIndex = 1;
                    }
                }
                _worksheet.Cells[rowIndex, colIndex] = "Всего"; colIndex++;
                _worksheet.Cells[rowIndex, colIndex] = _serviceLogs.Where(s => s.ServiceLog.Device.Model.TypeModel.Rowid == item.Rowid).ToList().Count(); colIndex++;
                foreach (var i in _depts)
                {
                    _worksheet.Cells[rowIndex, colIndex] = _serviceLogs.Where(s => s.ServiceLog.Device.Dept.Rowid == i.Rowid && s.ServiceLog.Device.Model.TypeModel.Rowid == item.Rowid).ToList().Count(); colIndex++;
                }

                _worksheet.Range[_worksheet.Cells[startRow, startCol], _worksheet.Cells[startRow, endCol]].Font.Bold = true;
                _worksheet.Range[_worksheet.Cells[startRow, startCol], _worksheet.Cells[startRow, endCol]].Interior.Color = ColorTranslator.ToOle(System.Drawing.Color.FromArgb(198, 224, 180));

                _worksheet.Range[_worksheet.Cells[startRow - 1, startCol], _worksheet.Cells[startRow - 1, startCol]].Borders[XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
                _worksheet.Range[_worksheet.Cells[startRow - 1, startCol], _worksheet.Cells[startRow - 1, startCol]].Borders[XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                _worksheet.Range[_worksheet.Cells[startRow - 1, startCol], _worksheet.Cells[startRow - 1, startCol]].Borders[XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlContinuous;
                _worksheet.Range[_worksheet.Cells[startRow - 1, startCol], _worksheet.Cells[startRow - 1, startCol]].Borders[XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
                _worksheet.Range[_worksheet.Cells[startRow - 1, startCol], _worksheet.Cells[startRow - 1, startCol]].Borders[XlBordersIndex.xlInsideHorizontal].LineStyle = Excel.XlLineStyle.xlContinuous;
                _worksheet.Range[_worksheet.Cells[startRow - 1, startCol], _worksheet.Cells[startRow - 1, startCol]].Borders[XlBordersIndex.xlInsideVertical].LineStyle = Excel.XlLineStyle.xlContinuous;

                _worksheet.Range[_worksheet.Cells[startRow, startCol], _worksheet.Cells[rowIndex, endCol]].Borders[XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
                _worksheet.Range[_worksheet.Cells[startRow, startCol], _worksheet.Cells[rowIndex, endCol]].Borders[XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                _worksheet.Range[_worksheet.Cells[startRow, startCol], _worksheet.Cells[rowIndex, endCol]].Borders[XlBordersIndex.xlEdgeLeft].LineStyle = Excel.XlLineStyle.xlContinuous;
                _worksheet.Range[_worksheet.Cells[startRow, startCol], _worksheet.Cells[rowIndex, endCol]].Borders[XlBordersIndex.xlEdgeRight].LineStyle = Excel.XlLineStyle.xlContinuous;
                _worksheet.Range[_worksheet.Cells[startRow, startCol], _worksheet.Cells[rowIndex, endCol]].Borders[XlBordersIndex.xlInsideHorizontal].LineStyle = Excel.XlLineStyle.xlContinuous;
                _worksheet.Range[_worksheet.Cells[startRow, startCol], _worksheet.Cells[rowIndex, endCol]].Borders[XlBordersIndex.xlInsideVertical].LineStyle = Excel.XlLineStyle.xlContinuous;

                _worksheet.Range[_worksheet.Cells[startRow, startCol], _worksheet.Cells[startRow, endCol]].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                _worksheet.Range[_worksheet.Cells[startRow, startCol], _worksheet.Cells[rowIndex, endCol]].HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                _worksheet.Range[_worksheet.Cells[startRow, startCol], _worksheet.Cells[rowIndex, endCol]].VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                _worksheet.Columns.EntireColumn.AutoFit();
                _worksheet.Rows.EntireColumn.AutoFit();

                colIndex = 1;
                rowIndex += 5;
            }
        }
        private void CreateReport()
        {
            _application = new Excel.Application();
            _workbook = _application.Workbooks.Add(Missing.Value);
            _worksheet = _workbook.ActiveSheet;
            var month = _serviceLogs.Min(s => s.ServiceLog.DateTime).Month.ToString();
            var year = _serviceLogs.Max(s => s.ServiceLog.DateTime).Year.ToString();
            _worksheet.Name = String.Format("Отчет за {0} месяц {1} года",month,year);
            rowIndex = 1; colIndex = 1; startCol = 1; startRow = 1; endCol = 1; endRow = 1;
            if (FullReport) CreateFullReport();
            if (CountReport) CreateCountReport();
            _application.Visible = true;
        }
        private void ExecuteCreateReport(object parameter)
        {
            CreateReport();
        }
        #endregion
    }
}
