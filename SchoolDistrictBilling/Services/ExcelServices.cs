using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using SchoolDistrictBilling.Data;
using SchoolDistrictBilling.Models;

namespace SchoolDistrictBilling.Services
{
    public class ExcelServices
    {
        public ExcelServices()
        {
        }

        public static List<SchoolDistrictRateView> ParseSchoolDistrictRates(AppDbContext context, List<string> fileNames)
        {
            List<SchoolDistrictRateView> rates = new List<SchoolDistrictRateView>();

            foreach (var fileName in fileNames)
            {
                byte[] bin = File.ReadAllBytes(fileName);

                List<string> columns = new List<string>();

                //create a new Excel package in a memorystream
                using (MemoryStream stream = new MemoryStream(bin))
                {
                    using (ExcelPackage excelPackage = new ExcelPackage(stream))
                    {
                        //loop all worksheets
                        //TODO: What's the deal with the second worksheet? Is it safe to only import the first worksheet?
                        //foreach (ExcelWorksheet worksheet in excelPackage.Workbook.Worksheets)
                        //{
                        ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.First();

                            //loop all rows. Start with the second row
                            for (int i = worksheet.Dimension.Start.Row; i <= worksheet.Dimension.End.Row; i++)
                            {
                                SchoolDistrictRateView rate = new SchoolDistrictRateView();
                                SchoolDistrict sd = null;

                                //loop all columns in a row
                                for (int j = worksheet.Dimension.Start.Column; j <= worksheet.Dimension.End.Column; j++)
                                {
                                    if (worksheet.Cells[i, j].Value != null)
                                    {
                                        if (i == 1)
                                        {
                                            columns.Add(worksheet.Cells[i, j].Value.ToString());
                                        }
                                        else
                                        {
                                            // TODO: This feels confusing. Is there a better way to do this with the objects and updating/inserting? What if the columns are in a different order?
                                            switch (columns[j-1].ToString())
                                            {
                                                case "AUN":
                                                    rate.SchoolDistrict.Aun = worksheet.Cells[i, j].Value.ToString();
                                                    sd = context.SchoolDistricts.FirstOrDefault(s => s.Aun == rate.SchoolDistrict.Aun);

                                                    if (sd == null)
                                                    {
                                                        sd = new SchoolDistrict
                                                        {
                                                            Aun = rate.SchoolDistrict.Aun
                                                        };
                                                    }
                                                    else
                                                    {
                                                        rate.SchoolDistrict.SchoolDistrictUid = sd.SchoolDistrictUid;
                                                    }
                                                    break;

                                                case "School District":
                                                    rate.SchoolDistrict.Name = worksheet.Cells[i, j].Value.ToString();

                                                    if (sd != null)
                                                    {
                                                        sd.Name = rate.SchoolDistrict.Name;
                                                    }
                                                    break;

                                                case "County":
                                                    rate.SchoolDistrict.County = worksheet.Cells[i, j].Value.ToString();

                                                    if (sd != null)
                                                    {
                                                        sd.County = rate.SchoolDistrict.County;
                                                    }

                                                    if (rate.SchoolDistrict.SchoolDistrictUid == 0)
                                                    {
                                                        context.SchoolDistricts.Add(sd);
                                                    }
                                                    context.SaveChanges();

                                                    break;

                                                default:
                                                    if (columns[j-1].ToString().Contains("Nonspecial"))
                                                    {
                                                        //TODO: Why are all the rates coming through as whole dollar amounts?
                                                        rate.SchoolDistrictRate.NonSpedRate = Convert.ToDecimal(worksheet.Cells[i, j].Value);
                                                    }
                                                    else if(columns[j-1].ToString().Contains("Special"))
                                                    {
                                                        rate.SchoolDistrictRate.SpedRate = Convert.ToDecimal(worksheet.Cells[i, j].Value);
                                                    }
                                                    else if (columns[j-1].ToString().Contains("Month"))
                                                    {
                                                        //TODO: Display this without the timestamp
                                                        rate.SchoolDistrictRate.EffectiveDate = DateTime.Parse(worksheet.Cells[i, j].Value.ToString());
                                                    }

                                                    break;
                                            }
                                        }
                                    }
                                }

                                if (i > 1)
                                {
                                    SchoolDistrictRate sdr = context.SchoolDistrictRates.FirstOrDefault(r => r.SchoolDistrictUid == sd.SchoolDistrictUid && r.EffectiveDate == rate.SchoolDistrictRate.EffectiveDate);
                                    if (sdr == null)
                                    {
                                        sdr = new SchoolDistrictRate(rate);
                                        sdr.SchoolDistrictUid = sd.SchoolDistrictUid;
                                        context.SchoolDistrictRates.Add(sdr);
                                    }
                                    else
                                    {
                                        sdr.NonSpedRate = rate.SchoolDistrictRate.NonSpedRate;
                                        sdr.SpedRate = rate.SchoolDistrictRate.SpedRate;
                                    }
                                }
                            }
                        //}
                    }
                }
            }

            context.SaveChanges();

            return rates;
        }
    }
}
