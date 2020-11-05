using Dapper;
using ExcelDataReader;
using ExportFile.Repository;
using ExportFile.Repository.Entities;
using ExportFile.Service.DTO;
using ExportFile.Service.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExportFile.Service.Services
{
    public class ItemService : IItemService
    {
        string filePath = $"{Directory.GetCurrentDirectory()}{@"\UploadExcel"}" + "\\";
        string connString = "Server=CS-LPTP-51A\\SQLEXPRESS; Database=ExportFileDB; Trusted_Connection=True; MultipleActiveResultSets=true";
        private readonly ExportFileContext _dbContext;

        public ItemService(ExportFileContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<ItemModel> UploadFile(IFormFile file)
        {
            try
            {
                string fullPath = filePath + file.FileName;
                using (FileStream fileStream = System.IO.File.Create(fullPath))
                {
                    file.CopyTo(fileStream);
                    fileStream.Flush();
                }

                List<ItemModel> items = GetItems(file.FileName);
                if (items.Any())
                {
                    var fileInfo = new DTO.FileInfo
                    {
                        Name = file.FileName,
                        Type = ".xml",
                        Size = Convert.ToInt32(file.Length),
                    };

                    AddFile(fileInfo, items);
                }

                return items;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private List<ItemModel> GetItems(string fileName)
        {
            try
            {
                List<ItemModel> items = new List<ItemModel>();
                string fullPath = filePath + fileName;
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (FileStream stram = System.IO.File.Open(fullPath, FileMode.Open, FileAccess.Read))
                {
                    using (IExcelDataReader excelReader = ExcelReaderFactory.CreateReader(stram))
                    {
                        while (excelReader.Read())
                        {
                            for (int i = 0; i < 500; i++)
                            {
                                items.Add(new ItemModel()
                                {
                                    Key = excelReader.GetValue(0).ToString(),
                                    ItemCode = excelReader.GetValue(1).ToString(),
                                    ColorCode = excelReader.GetValue(2).ToString(),
                                    Description = excelReader.GetValue(3).ToString(),
                                    Price = excelReader.GetValue(4).ToString(),
                                    DiscountPrice = excelReader.GetValue(5).ToString(),
                                    DeliveredIn = excelReader.GetValue(6).ToString(),
                                    Q1 = excelReader.GetValue(7).ToString(),
                                    Size = excelReader.GetValue(8).ToString(),
                                    Color = excelReader.GetValue(9).ToString(),
                                });
                            }
                        }
                    }
                    return items;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                DeleteFile(fileName);
            }
        }

        private void DeleteFile(string fileName)
        {
            var filePath = $"{Directory.GetCurrentDirectory()}{@"\UploadExcel"}" + "\\" + fileName;
            System.IO.File.Delete(filePath);
        }

        private bool AddFile(DTO.FileInfo fileInfo, List<ItemModel> items)
        {
            try
            {
                Repository.Entities.FileInfo file = new Repository.Entities.FileInfo
                {
                    Name = fileInfo.Name,
                    Type = fileInfo.Type,
                    Size = fileInfo.Size,
                    CreatedBy = 1,
                    CreationDate = DateTime.Now,
                };

                _dbContext.FileInfo.Add(file);
                _dbContext.SaveChanges();

                AddItems(file.Id, items);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private IList<string> GetSqlsInBatches(int fileId, List<ItemModel> items)
        {
            var insertSql = "INSERT INTO [Items] ([FileId],[Key],[ItemCode],[ColorCode],[Description],[Price],[DiscountPrice],[DeliveredIn],[Q1],[Size],[Color],[CreationDate],[CreatedBy]) VALUES ";
            var batchSize = 100;
            var sqlsToExecute = new List<string>();
            var numberOfBatches = (int)Math.Ceiling((double)items.Count / batchSize);

            for (int i = 0; i < numberOfBatches; i++)
            {
                var userToInsert = items.Skip(i * batchSize).Take(batchSize);
                List<string> valuesToInsert = new List<string>();
                foreach (var item in userToInsert)
                {
                    valuesToInsert.Add("(" + fileId + "" +
                        ",'" + item.Key + "'" +
                        ",'" + item.ItemCode + "'" +
                        ",'" + item.ColorCode + "'" +
                        ",'" + item.Description + "'" +
                        ",'" + item.Price + "'" +
                        ",'" + item.DiscountPrice + "'" +
                        ",'" + item.DeliveredIn + "'" +
                        ",'" + item.Q1 + "'" +
                        ",'" + item.Size + "'" +
                        ",'" + item.Color + "'" +
                        "," + item.CreatedBy + "" +
                        "," + item.CreationDate.ToShortDateString() + ")");
                }
                sqlsToExecute.Add(insertSql + string.Join(',', valuesToInsert));
            }

            return sqlsToExecute;
        }

        private bool AddItems(int fileId, List<ItemModel> items)
        {
            try
            {

                //Useing Dapper
                return AddItemsUsingDapper(fileId, items);

                //Using EF
                //return AddItemsUsingEF(fileId, items);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        /// <summary>
        /// In this method we try to add the items using dapper tools, it is take around 5 sec for 85000
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        private bool AddItemsUsingDapper(int fileId, List<ItemModel> items)
        {
            try
            {
                var sqls = GetSqlsInBatches(fileId, items);
                using (var connection = new SqlConnection(connString))
                {
                    foreach (var sql in sqls)
                    {
                        connection.Execute(sql);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// In this method we try to add the items using EF tools with batch, it is take around 1 min for 85000
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        private bool AddItemsUsingEF(int fileId, List<ItemModel> items)
        {
            try
            {
                _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
                _dbContext.Items.AsNoTracking();

                for (int i = 0; i < items.Count; i++)
                {
                    _dbContext.Items.AddAsync(new Item
                    {
                        FileId = fileId,
                        Key = items[i].Key,
                        ItemCode = items[i].ItemCode,
                        ColorCode = items[i].ColorCode,
                        Description = items[i].Description,
                        Price = items[i].Price,
                        DiscountPrice = items[i].DiscountPrice,
                        DeliveredIn = items[i].DeliveredIn,
                        Q1 = items[i].Q1,
                        Size = items[i].Size,
                        Color = items[i].Color,

                        CreatedBy = 1,
                        CreationDate = DateTime.Now,
                    });
                    if (i % 500 == 0)
                        _dbContext.SaveChangesAsync();
                }

                _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
