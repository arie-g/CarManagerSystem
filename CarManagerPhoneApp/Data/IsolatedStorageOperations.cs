using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CarManagerPhoneApp.Data
{
    public static class IsolatedStorageOperations
    {
        public static async Task Save<T>(this T obj, string file)
        {
            await Task.Run(() =>
            {
                IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
                IsolatedStorageFileStream stream = null;

                try
                {
                    stream = storage.CreateFile(file);
                    XmlSerializer serializer = new XmlSerializer(typeof (T));
                    serializer.Serialize(stream, obj);
                }
                catch (Exception)
                {
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                        stream.Dispose();
                    }
                }
            });
        }

        public static async Task<bool> IsExist<T>(string file)
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
            T obj = Activator.CreateInstance<T>();

            return storage.FileExists(file);
        }

        public static async Task<T> Load<T>(string file)
        {

            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
            T obj = Activator.CreateInstance<T>();

            if (storage.FileExists(file))
            {
                IsolatedStorageFileStream stream = null;
                try
                {
                    stream = storage.OpenFile(file, FileMode.Open);
                    XmlSerializer serializer = new XmlSerializer(typeof (T));

                    obj = (T) serializer.Deserialize(stream);
                }
                catch (Exception)
                {
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                        stream.Dispose();
                    }
                }
                return obj;
            }
            await obj.Save(file);
            return obj;
        }
    }
}




