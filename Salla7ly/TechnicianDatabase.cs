using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Salla7ly
{
    public class TechnicianDatabase
    {
        //Mobile Service sync table used to access data
        private IMobileServiceSyncTable<Technician> _technicaianTable;

        private readonly MobileServiceSQLiteStore _store;

        //Mobile Service Client reference
        public MobileServiceClient Client { get; }

        private const string ApplicationUrl = @"https://salla7ly.azurewebsites.net";

        public TechnicianDatabase()
        {
            Client = new MobileServiceClient(ApplicationUrl);
            _store = new MobileServiceSQLiteStore(LocalDbFilename);
            _store.DefineTable<Technician>();
        }

        /// <summary></summary>
        /// <exception cref="Java.Net.MalformedURLException"></exception>
        /// <exception cref="System.Exception"></exception>
        /// <param name="pullData"></param>
        /// <returns></returns>
        public async Task CommitChanges(bool pullData = false)
        {
            await Client.SyncContext.PushAsync(); // send changes to the mobile service

            if (pullData)
            {
                await _technicaianTable.PullAsync("technicianQuery", _technicaianTable.CreateQuery()); // query ID is used for incremental sync
            }
        }

        //#if DEBUG
        //        private Technician tech;
        //        /// <summary>Adds dummy data to the database</summary>
        //        public async Task SeedData(int number, ArrayAdapter adapter)
        //        {
        //            for (int j = 0; j < adapter.Count - 1; j++)
        //            {
        //                for (int i = 0; i < number; i++)
        //                {
        //                    tech = new Technician()
        //                    {
        //                        Name = $"dummy {i}",
        //                        City = $"Dummy city {i}",
        //                        Field = $"{adapter.GetItem(j)}",
        //                        Governorate = $"{i} Alex",
        //                        PhoneNumber = $"01111111111",
        //                    };

        //                    await AddTechnician(tech);
        //                    await CommitChanges();
        //                }
        //            }
        //        }
        //#endif

        public async Task<IEnumerable<Technician>> GetAllTechnicians(int take = 20)
        {
            return await _technicaianTable.CreateQuery().Take(20).ToListAsync();
        }

        public async Task AddTechnician(Technician technician)
        {
            if (technician.Id == null)
            {
                await _technicaianTable.InsertAsync(technician); // insert the new item into the local database
            }
            else
            {
                await _technicaianTable.UpdateAsync(technician); // insert the new item into the local database
            }
        }

        public async Task StartDb()
        {
            await Client.SyncContext.InitializeAsync(_store);
            _technicaianTable = Client.GetSyncTable<Technician>();
        }

        public async Task<IList<Technician>> Find(string field)
        {
            Expression<Func<Technician, bool>> queryExpression = t => t.Field.StartsWith(field);

            var results = await _technicaianTable.Where(queryExpression).ToListAsync();
            if (results.Count != 0) return results;

            try
            {
                await _technicaianTable.PullAsync("pullTechByField", _technicaianTable.Where(t => t.Field.StartsWith(field)));
            }
            catch (Exception exc)
            {
                string err = exc.Message;
            }

            return await _technicaianTable.Where(queryExpression).ToListAsync();

        }

        private const string LocalDbFilename = "localstore.db";
    }
}