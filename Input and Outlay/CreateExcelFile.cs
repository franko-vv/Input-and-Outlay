﻿using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace BelarusianDoor
{
    /// <summary>
    /// Клас для створення і заповнення даними шаблонних файлів
    /// </summary>
    class CreateExcelFile
    {
        private Excel.Application oApp;
        private Excel.Worksheet oSheet;
        private Excel.Workbook oBook;
        private Excel.Range oCells;

        private DateTime dateTime;

        string dateForFileName;
        string monthInput;
        string yearInput;

        private DataGridView dgviewIncome;
        private DataGridView dgviewOutlay;

        string excelFilePath = "D:\\ExcelFilesDveriBelorusii\\excel\\";
        string pdfFilePath = "D:\\ExcelFilesDveriBelorusii\\PDF\\";

        public CreateExcelFile(DateTime _dateTime, DataGridView _dgvIncome, DataGridView _dgvOutlay)
        {
            dateTime = _dateTime;
            dgviewIncome = _dgvIncome;
            dgviewOutlay = _dgvOutlay;

            dateForFileName = dateTime.ToString("dd MMMM", CultureInfo.CreateSpecificCulture("uk-UA"));
            monthInput = dateTime.ToString("MMMM", CultureInfo.CreateSpecificCulture("uk-UA"));
            yearInput = dateTime.ToString("yyyy");
        }

        /// <summary>
        /// Сохраняем файлы в формате пдф и ексель в папки
        /// </summary>
        public void MakeExcelFile(decimal moneyAtStart, decimal moneyBalance)
        {
            string dateFile;    //Повна назва файлу для зберігання
            try
            {
                CreateFolderForSavingFile(excelFilePath, out dateFile);

                oApp = new Excel.Application();
                oBook = oApp.Workbooks.Add();
                oSheet = (Excel.Worksheet)oBook.Worksheets.get_Item(1);

                // Заповнюємо excel файл данними
                CreateFullFile();
                InputInformationFields(moneyAtStart, moneyBalance);

                // Зберігаємо файл в форматі екселя
                oBook.SaveAs(dateFile + ".xlsx");

                CreateFolderForSavingFile(pdfFilePath, out dateFile);

                // Додаткова перевірка при зберіганні pdf файла на встановлене розширення в Office
                try
                {
                    oBook.ExportAsFixedFormat(Excel.XlFixedFormatType.xlTypePDF, dateFile + ".pdf");
                }
                catch (ArgumentException)
                {
                    MessageBox.Show("Помилка при збереженні PDF файла. Перевірте чи у Вас встановлене розширення в "
                        + " Microsoft Office для збереження файлів в форматі PDF/XPS.", "Помилка при збереженні PDF",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }               
                catch (Exception)
                {
                    MessageBox.Show("Помилка при збереженні pdf файла.");
                }


                MessageBox.Show("Дані збережено!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
            finally
            {
                oBook.Close();
                oApp.Quit();
            }
        }
        
        private void CreateFolderForSavingFile(string filePath, out string dateFile)
        {
            // Перевіряємо чи є папка року (2015/2016/2017)
            string directoryFolder = filePath + yearInput;
            if (!Directory.Exists(directoryFolder))
            {
                Directory.CreateDirectory(directoryFolder);
            }

            // Перевіряємо чи є папка місяця (грудень/січень/лютий)
            string monthFolder = directoryFolder + "\\" + monthInput;
            if (!Directory.Exists(monthFolder))
            {
                Directory.CreateDirectory(monthFolder);
            }

            dateFile = monthFolder + "\\" + dateForFileName;

            // Якщо файл існує - видаляємо і створюємо новий
            if (File.Exists(dateFile))
            {
                // якщо файл з історії, робимо перевірку
                if (DateTime.Today != dateTime.Date)
                {
                    DialogResult result = MessageBox.Show("Ви бажаєте змінити файл в історії за " + dateForFileName, "УВАГА",
                                                            MessageBoxButtons.YesNoCancel,
                                                            MessageBoxIcon.Warning,
                                                            MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Yes)
                        File.Delete(dateFile);
                    else
                        return;
                }
                // якщо сьогоднішній, то змінюємо
                else
                    File.Delete(dateFile);
            }
        }

        private void CreateFullFile()
        {
            // Створюємо базовий шаблон
            InputDataToTemplateExcelFile();

            // Заповнюємо ячейки в таблицю НАДХОДЖЕННЯ з gridviewInput
            InputDataToIncome(dgviewIncome);

            // Заповнюємо ячейки в таблицю ВИТРАТИ з gridviewOutlay
            InputDataToOutlay(dgviewOutlay);
        }
        
        /// <summary>
        /// Створюємо шаблон Excel файла
        /// </summary>
        /// TODO: Винести однотипні styles
        private void InputDataToTemplateExcelFile()
        {
            Excel.Style style = oBook.Styles.Add("newStyle");
            style.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            style.Borders.Weight = Excel.XlBorderWeight.xlThick;
            style.Font.Size = 12;
            style.Font.Bold = true;

            string selectedDate = dateForFileName + " " + yearInput;

            // ---------------------------------ШАПКА-------------------------------------------------
            // Звіт про використання коштів
            oCells = oSheet.Range["A2", "H2"];
            oCells.Merge(Type.Missing);
            oCells.Value = "Звіт про використання коштів " + selectedDate + " р.";
            oCells.HorizontalAlignment = Excel.Constants.xlCenter;
            oCells.VerticalAlignment = Excel.Constants.xlCenter;
            oCells.Font.Size = 16;
            oCells.Font.Bold = true;

            // салон Двері Білорусії м.Вінниця
            oCells = oSheet.Range["A3", "H3"];
            oCells.Merge(Type.Missing);
            oCells.Value = "салон Двері Білорусії м.Вінниця";
            oCells.HorizontalAlignment = Excel.Constants.xlCenter;
            oCells.VerticalAlignment = Excel.Constants.xlCenter;
            oCells.Font.Size = 12;
            oCells.Font.Bold = true;

            // ---------------------------------ГРОШІ В КАЗНІ-------------------------------------------------
            // Залишок на початок дня:
            oCells = oSheet.Range["A5", "D5"];
            oCells.Merge(Type.Missing);
            oCells.Value = "Залишок на початок дня: ";
            oCells.HorizontalAlignment = Excel.Constants.xlCenter;
            oCells.VerticalAlignment = Excel.Constants.xlCenter;
            oCells.Font.Size = 12;
            oCells.Font.Bold = true;

            oCells = oSheet.Range["G5"];
            oCells.Merge(Type.Missing);
            oCells.Value = "грн.";
            oCells.HorizontalAlignment = Excel.Constants.xlLeft;
            oCells.VerticalAlignment = Excel.Constants.xlCenter;
            oCells.Font.Size = 12;
            oCells.Font.Bold = true;

            // ---------------------------------ТАБЛИЦЯ-------------------------------------------------
            // Приход:
            oCells = oSheet.Range["A7", "D7"];
            oCells.Merge(Type.Missing);
            oCells.Value = "Приход";
            oCells.HorizontalAlignment = Excel.Constants.xlCenter;
            oCells.VerticalAlignment = Excel.Constants.xlCenter;
            style.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            style.Borders.Weight = Excel.XlBorderWeight.xlThick;
            style.Font.Size = 12;
            style.Font.Bold = true;

            // Видатки:
            oCells = oSheet.Range["E7", "H7"];
            oCells.Merge(Type.Missing);
            oCells.Value = "Видатки";
            oCells.HorizontalAlignment = Excel.Constants.xlCenter;
            oCells.VerticalAlignment = Excel.Constants.xlCenter;
            style.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            style.Borders.Weight = Excel.XlBorderWeight.xlThick;
            style.Font.Size = 12;
            style.Font.Bold = true;

            // ---------------------------------ШАПКА ТАБЛИЦІ-------------------------------------------------
            // Сума:
            oCells = oSheet.Range["A8"];
            oCells.Value = "Сума";
            oCells.HorizontalAlignment = Excel.Constants.xlCenter;
            oCells.VerticalAlignment = Excel.Constants.xlCenter;
            oCells.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            oCells.Borders.Weight = Excel.XlBorderWeight.xlThin;
            oCells.EntireColumn.AutoFit();                              // Автоподбор ширины
            oCells.Font.Size = 12;
            oCells.Font.Bold = true;

            // Від кого поступили:
            oCells = oSheet.Range["B8"];
            oCells.Merge(Type.Missing);
            oCells.Value = "Від кого поступили";
            oCells.HorizontalAlignment = Excel.Constants.xlCenter;
            oCells.VerticalAlignment = Excel.Constants.xlCenter;
            oCells.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            oCells.Borders.Weight = Excel.XlBorderWeight.xlThin;
            oCells.EntireColumn.AutoFit();                              // Автоподбор ширины
            oCells.EntireColumn.WrapText = true;                        // Перенос по словам
            oCells.Font.Size = 12;
            oCells.Font.Bold = true;

            // Хто отримав
            oCells = oSheet.Range["C8"];
            oCells.Value = "Хто отримав (ПІБ)";
            oCells.HorizontalAlignment = Excel.Constants.xlCenter;
            oCells.VerticalAlignment = Excel.Constants.xlCenter;
            oCells.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            oCells.Borders.Weight = Excel.XlBorderWeight.xlThin;
            oCells.EntireColumn.AutoFit();                              // Автоподбор ширины
            oCells.EntireColumn.WrapText = true;                        // Перенос по словам
            oCells.Font.Size = 12;
            oCells.Font.Bold = true;

            // Підпис
            oCells = oSheet.Range["D8"];
            oCells.Value = "Підпис";
            oCells.HorizontalAlignment = Excel.Constants.xlCenter;
            oCells.VerticalAlignment = Excel.Constants.xlCenter;
            oCells.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            oCells.Borders.Weight = Excel.XlBorderWeight.xlThin;
            oCells.EntireColumn.AutoFit();                              // Автоподбор ширины
            oCells.Font.Size = 12;
            oCells.Font.Bold = true;

            // Сума:
            oCells = oSheet.Range["E8"];
            oCells.Value = "Сума";
            oCells.HorizontalAlignment = Excel.Constants.xlCenter;
            oCells.VerticalAlignment = Excel.Constants.xlCenter;
            oCells.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            oCells.Borders.Weight = Excel.XlBorderWeight.xlThin;
            oCells.Font.Size = 12;
            oCells.Font.Bold = true;

            // Куди витрачено
            oCells = oSheet.Range["F8"];
            oCells.Value = "Куди витрачено";
            oCells.HorizontalAlignment = Excel.Constants.xlCenter;
            oCells.VerticalAlignment = Excel.Constants.xlCenter;
            oCells.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            oCells.Borders.Weight = Excel.XlBorderWeight.xlThin;
            oCells.EntireColumn.AutoFit();                              // Автоподбор ширины 
            oCells.EntireColumn.WrapText = true;                        // Перенос по словам
            oCells.Font.Size = 12;
            oCells.Font.Bold = true;

            // Кому видано
            oCells = oSheet.Range["G8"];
            oCells.Value = "Кому видано (ПІБ)";
            oCells.HorizontalAlignment = Excel.Constants.xlCenter;
            oCells.VerticalAlignment = Excel.Constants.xlCenter;
            oCells.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            oCells.Borders.Weight = Excel.XlBorderWeight.xlThin;
            oCells.EntireColumn.AutoFit();                              // Автоподбор ширины 
            oCells.EntireColumn.WrapText = true;                        // Перенос по словам
            oCells.Font.Size = 12;
            oCells.Font.Bold = true;

            // Підпис
            oCells = oSheet.Range["H8"];
            oCells.Value = "Підпис";
            oCells.HorizontalAlignment = Excel.Constants.xlCenter;
            oCells.VerticalAlignment = Excel.Constants.xlCenter;
            oCells.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            oCells.Borders.Weight = Excel.XlBorderWeight.xlThin;
            oCells.EntireColumn.AutoFit();                              // Автоподбор ширины
            oCells.Font.Size = 12;
            oCells.Font.Bold = true;
            
            // ---------------------------------КАЗНА НА КІНЕЦЬ ДНЯ-------------------------------------------------
            // Залишок на кінець дня:
            oCells = oSheet.Range["A20", "D20"];
            oCells.Merge(Type.Missing);
            oCells.Value = "Залишок на кінець дня: ";
            oCells.HorizontalAlignment = Excel.Constants.xlCenter;
            oCells.VerticalAlignment = Excel.Constants.xlCenter;
            oCells.Font.Size = 12;
            oCells.Font.Bold = true;

            oCells = oSheet.Range["G20"];
            oCells.Merge(Type.Missing);
            oCells.Value = "грн.";
            oCells.HorizontalAlignment = Excel.Constants.xlLeft;
            oCells.VerticalAlignment = Excel.Constants.xlCenter;
            oCells.Font.Size = 12;
            oCells.Font.Bold = true;
        }

        /// <summary>
        /// Заповнюємо таблицю надходжень
        /// </summary>
        private void InputDataToIncome(DataGridView dt)
        {
            Excel.Style style = oBook.Styles.Add("newStyle1");

            for (int i = 0, j = 9; i < dt.Rows.Count; i++, j++)
            {
                // Заповнюємо даними СУМУ
                string cellNameSumma = "A" + j;
                oCells = oSheet.Range[cellNameSumma];

                oCells.Value = string.Format(
                    Math.Round(Convert.ToDecimal(dt["Summa", i].Value),2)       //50.0000 --> 50.00
                    .ToString());

                oCells.HorizontalAlignment = Excel.Constants.xlCenter;
                oCells.VerticalAlignment = Excel.Constants.xlCenter;
                oCells.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                oCells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                oCells.Font.Size = 12;
                oCells.EntireColumn.AutoFit();

                // Заповнюємо даними ХТО ДАВ ГРОШІ
                string cellCustomer = "B" + j;
                oCells = oSheet.Range[cellCustomer];
                oCells.Value = dt["Customer", i].Value.ToString();
                oCells.HorizontalAlignment = Excel.Constants.xlCenter;
                oCells.VerticalAlignment = Excel.Constants.xlCenter;
                oCells.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                oCells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                oCells.Font.Size = 12;

                // Заповнюємо даними ХТО ОТРИМАВ ГРОШІ
                string cellEmployee = "C" + j;
                oCells = oSheet.Range[cellEmployee];
                oCells.Value = dt["Employee", i].Value.ToString();
                oCells.HorizontalAlignment = Excel.Constants.xlCenter;
                oCells.VerticalAlignment = Excel.Constants.xlCenter;
                oCells.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                oCells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                oCells.Font.Size = 12;

                // Підпис - "Х"
                string cellSign = "D" + j;
                oCells = oSheet.Range[cellSign];
                oCells.Value = "X";
                oCells.HorizontalAlignment = Excel.Constants.xlCenter;
                oCells.VerticalAlignment = Excel.Constants.xlCenter;
                oCells.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                oCells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                oCells.Font.Size = 12;
            }
        }

        /// <summary>
        /// Заповнюємо таблицю витрат
        /// </summary>
        private void InputDataToOutlay(DataGridView dv)
        {
            for (int i = 0, j = 9; i < dv.Rows.Count; i++, j++)
            {
                // E9 - начало суммы
                string cellNameSumma = "E" + j;
                oCells = oSheet.Range[cellNameSumma];

                oCells.Value = string.Format(
                                    Math.Round(Convert.ToDecimal(dv["Summa", i].Value), 2).
                                    ToString());

                oCells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                oCells.Font.Size = 12;
                oCells.HorizontalAlignment = Excel.Constants.xlCenter;
                oCells.VerticalAlignment = Excel.Constants.xlCenter;


                string cellWhereSpend = "F" + j;
                oCells = oSheet.Range[cellWhereSpend];
                oCells.Value = dv["WhereSpend", i].Value.ToString();
                oCells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                oCells.Font.Size = 12;
                oCells.HorizontalAlignment = Excel.Constants.xlCenter;
                oCells.VerticalAlignment = Excel.Constants.xlCenter;


                string cellWhoReceive = "G" + j;
                oCells = oSheet.Range[cellWhoReceive];
                oCells.Value = dv["WhoReceive", i].Value.ToString();
                oCells.HorizontalAlignment = Excel.Constants.xlCenter;
                oCells.VerticalAlignment = Excel.Constants.xlCenter;
                oCells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                oCells.Font.Size = 12;

                string cellSign = "H" + j;
                oCells = oSheet.Range[cellSign];
                oCells.Value = "X";
                oCells.HorizontalAlignment = Excel.Constants.xlCenter;
                oCells.VerticalAlignment = Excel.Constants.xlCenter;
                oCells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                oCells.Font.Size = 12;
            }
        }

        /// <summary>
        /// Заповнюємо інформаційні поля
        /// </summary>
        private void InputInformationFields(decimal moneyAtStart, decimal moneyBalance)
        {
            // Кількість грошей, що залишилася:
            oCells = oSheet.Range["E5", "F5"];
            oCells.Merge(Type.Missing);
            oCells.Value = moneyAtStart;
            oCells.HorizontalAlignment = Excel.Constants.xlCenter;
            oCells.VerticalAlignment = Excel.Constants.xlCenter;
            oCells.Font.Size = 12;
            oCells.Font.Bold = true;

            // Кількість грошей, що залишилася:
            oCells = oSheet.Range["E20", "F20"];
            oCells.Merge(Type.Missing);
            oCells.Value = moneyBalance;
            oCells.HorizontalAlignment = Excel.Constants.xlCenter;
            oCells.VerticalAlignment = Excel.Constants.xlCenter;
            oCells.Font.Size = 12;
            oCells.Font.Bold = true;
        }
    }
}
