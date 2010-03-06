using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO ;
using Community.CsharpSqlite;
using Community.CsharpSqlite.SQLiteClient;

namespace OneSync.Synchronization
{
    public class UIProcess
    {      
        public static void CreateDataStore(string userDataPath , SyncSource source, IntermediaryStorage mSource)
        {
            string pathToMdSource = @mSource.Path + @"\data.md";
            string pathTouserSource = @userDataPath + @"\profiles\data.md";

            if (!Directory.Exists(userDataPath + "\\profiles\\")) Directory.CreateDirectory(@userDataPath + "\\profiles\\");
            if (!File.Exists (pathTouserSource))File.Create(@pathTouserSource);

            if (!Directory.Exists(mSource.Path)) Directory.CreateDirectory(@mSource.Path);            
            if (!File.Exists(pathToMdSource)) File.Create(@pathToMdSource);


            string conString1 = string.Format("Version=3,uri=file:{0}", @pathTouserSource );
            string conString2 = string.Format("Version=3,uri=file:{0}", @pathToMdSource);

            SqliteConnection con1 = null;
            SqliteConnection con2 = null;
            SqliteTransaction transaction1 = null;
            SqliteTransaction transaction2 = null;
            try
            {
                (con1 = new SqliteConnection(conString1)).Open();
                transaction1 = (SqliteTransaction)con1.BeginTransaction();

                (con2 = new SqliteConnection(conString2)).Open ();                               
                transaction2 = (SqliteTransaction) con2.BeginTransaction();

                new SQLiteMetaDataProvider(@pathToMdSource).CreateSchema(con2);
                new SQLiteActionProvider(@pathToMdSource).CreateSchema(con2);
                
                new SQLiteSyncSourceProvider(@pathTouserSource).CreateSchema(con1);
                new SQLiteProfileManager(@pathTouserSource).CreateSchema(con1);                              

                transaction2.Commit();
                transaction1.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);                
                if (transaction2 != null) transaction2.Rollback();
                if (transaction1 != null) transaction1.Rollback();
                throw new DatabaseException("Database exception");
            }
            finally
            {
                if (con1 != null) con1.Dispose();                
                if (con2 != null) con2.Dispose();
            }                          
        }

        public static void CreateProfile(string pathToUserData, Profile profile)
        {
            string pathToMdSource = profile.IntermediaryStorage.Path + @"\data.md";
            string pathTouserSource = pathToUserData + @"\profiles\data.md";            

            string conString1 = string.Format("Version=3,uri=file:{0}", @pathTouserSource);            

            SqliteConnection con1 = null;            
            SqliteTransaction transaction1 = null;
            
            try
            {
                (con1 = new SqliteConnection(conString1)).Open();
                transaction1 = (SqliteTransaction)con1.BeginTransaction();
                new SQLiteProfileManager(con1).Insert(profile);
                new SQLiteSyncSourceProvider(con1).Insert(profile.SyncSource);                
                transaction1.Commit();              
            }
            catch (Exception ex)
            {
                if (transaction1 != null) transaction1.Rollback();
                throw new DatabaseException("Database Exception");
            }
            finally
            {
                if (con1 != null) con1.Dispose();
            }            
        }

        public static void InsertMetaData(Profile profile, FileMetaData md)
        {
           string pathToMdSource = profile.IntermediaryStorage.Path + @"\data.md";
           new SQLiteMetaDataProvider(profile).Insert(md);
        }

        public static FileMetaData LoadMetadata(Profile profile)
        {
            return (FileMetaData)(new SQLiteMetaDataProvider(profile).Load(profile.SyncSource) );
        }
    }
}
